
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using DAL;
using Domain.Entity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using WebApplication2;
using WebApplication2.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using WebApplication2.Controllers;
using WebApplication2.Infrastructure;


var log = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(Path.Combine("logs", "log.txt"), rollOnFileSizeLimit: true, fileSizeLimitBytes: 1024 * 1024)
    .CreateLogger();

try
{
    log.Information("Starting web host");
    
    var builder = WebApplication.CreateBuilder(args);

    //connecting to the database
    var builderConfig = new ConfigurationBuilder();
    builderConfig.SetBasePath(Directory.GetCurrentDirectory());
    builderConfig.AddJsonFile("appsettings.json");
    var config = builderConfig.Build();
    string? connectionString = config.GetConnectionString("DefaultConnection");
    
    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    var migrationsAssembly = Type.GetType("builder.Services")?.Assembly.FullName;
    
    // var postgresHost = Environment.GetEnvironmentVariable("DB_HOST");
    // var postgresDb = Environment.GetEnvironmentVariable("DB_NAME");
    // var postgresUser = Environment.GetEnvironmentVariable("DB_USER");
    // var postgresPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    
    builder.Services.AddDbContext<GraphLabsContext>(
        o => o.UseNpgsql(connectionString!,
            b => b.MigrationsAssembly(migrationsAssembly)));
    
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddSingleton<InitialData>();
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder => 
                builder.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });
    }
    else
    {
        throw new NotImplementedException("Для продакшна сделаем чуть позже");
    }
    
    builder.Services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    // builder.Services.AddControllers()
    //     .AddOData(opt =>
    //         opt.AddRouteComponents("odata", EdmModelBuilder.Build()));
    
    builder.Services.AddControllers().AddJsonOptions(x =>
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    
    builder.Services.AddControllers().AddOData(opt => opt
        .AddRouteComponents("odata", GetEdmModel())
        .Select()
        .Expand()
        .Filter()
        .OrderBy()
        .Count()
    );
    
    builder.Services.AddMvc(config =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        config.Filters.Add(new AuthorizeFilter(policy));
        //config.EnableEndpointRouting = false;
        
    });
    
    // configure strongly typed settings objects
    var authSettings = builder.Configuration.GetSection("AuthSettings");
    builder.Services.Configure<AuthSettings>(authSettings);
    
    var appSettings = authSettings.Get<AuthSettings>();
    var key = Encoding.ASCII.GetBytes(appSettings.Secret);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(x =>
        {
            if (builder.Environment.IsDevelopment())
                x.RequireHttpsMetadata = false;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                RoleClaimType = ClaimTypes.Role
            };
        });

    builder.Services.AddScoped<UserService>();
    builder.Services.AddSingleton<PasswordHashCalculator>();
    builder.Services.AddScoped<IUserInfoService, UserInfoService>();
    builder.Services.AddScoped<TaskVariantConverter>();
    
    JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
    {
        Converters =
        {
            new StringEnumConverter
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        },
        Formatting = Formatting.Indented,
        Culture = CultureInfo.InvariantCulture,
        TypeNameHandling = TypeNameHandling.None,
        ContractResolver = new LowerCamelCaseContractResolver()
    };
    
    
    //добавил авторизацию пользователя для swagger
    //для доступа к методу без авторизации использовать атрибут [AllowAnonymous]
    
    builder.Services.AddSwaggerGen(c =>
    {
        c.ResolveConflictingActions (apiDescriptions => apiDescriptions.First ());
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });
   
    builder.Host.UseSerilog();   
    
    var app = builder.Build();
    
    
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseCors("CorsPolicy");
        
        app.UseSwagger();    
        app.UseSwaggerUI();  // потом уберу
    }
    else
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }
    
    app.UseSerilogRequestLogging();
            
    app.UseAuthentication();
    
    
    //app.UseRouting();   //?????
    
    app.MapControllers();
    
    
   
    //app.UseAuthorization();
    
    
    // app.UseMvc(builderMvc =>
    // {
    //     builderMvc.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
    //
    //     const string routeName = "odata";
    //     builderMvc.MapODataServiceRoute(routeName, routeName, routeBuilder => routeBuilder
    //         .AddService(ServiceLifetime.Singleton, sp => GetEdmModel())
    //         .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton,
    //             sp => ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, builder))
    //     );
    // });
    
    
    using (var scope = app.Services.CreateScope())
    await using (var context = scope.ServiceProvider.GetRequiredService<GraphLabsContext>())
    {
        await InitializeDb(context, new InitialData(new PasswordHashCalculator()));
    }
            
    await app.RunAsync();
    
}
catch(Exception ex)
{
    log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}



static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder
    {
        Namespace = "GraphLabs.Backend.Api"
    };
    
    // TaskModules =============================================================================================
    var taskModule = builder.EntitySet<TaskModule>("TaskModules").EntityType;
    taskModule.HasKey(m => m.Id);
    taskModule.HasMany(m => m.Variants);
            
    taskModule.Function(nameof(TaskModulesController.RandomVariant)).Returns<IActionResult>();
            
    var downloadFunc = taskModule.Function(nameof(TaskModulesController.Download));
    downloadFunc.Parameter<string>("path").Required();
    downloadFunc.Returns<IActionResult>();
            
    var uploadFunc = taskModule.Action(nameof(TaskModulesController.Upload));
    uploadFunc.Returns<IActionResult>();
    
    // TaskVariants ============================================================================================
    var taskVariant = builder.EntitySet<TaskVariant>("TaskVariants").EntityType;
    taskVariant.HasKey(v => v.Id);
    taskVariant.HasRequired(v => v.TaskModule);

    taskVariant.Function(nameof(TaskVariantsController.Json)).Returns<IActionResult>();
    
    // Users ===================================================================================================
    const string usersEntitySet = "Users";
    var user = builder.EntitySet<User>(usersEntitySet).EntityType;
    user.HasKey(u => u.Id);
    user.Abstract();
    user.Ignore(u => u.PasswordHash);
    user.Ignore(u => u.PasswordSalt);

    var teacher = builder.EntitySet<Teacher>("Teachers").EntityType;
    teacher.DerivesFrom<User>();

    var student = builder.EntitySet<Student>("Students").EntityType;
    student.DerivesFrom<User>();
    student.HasMany(s => s.Logs);
    
    // TaskVariantLogs =========================================================================================
    var taskVariantLog = builder.EntitySet<TaskVariantLog>("TaskVariantLogs").EntityType;
    taskVariantLog.HasKey(l => l.Id);
    taskVariantLog.HasRequired(l => l.Student);
    taskVariantLog.HasRequired(l => l.Variant);
    
    
    // Tests ===================================================================================================

    var test = builder.EntitySet<Test>("Tests").EntityType;
    test.HasKey(t => t.Id);
    test.HasRequired(t => t.Teacher);
    test.HasRequired(t => t.Subject);
    test.HasRequired(t=>t.TestQuestions);
    
    // Subjects ================================================================================================

    var subject = builder.EntitySet<Subject>("Subject").EntityType;
    subject.HasKey(s => s.Id);
    subject.Ignore(s=>s.Tests);
    subject.HasMany(s => s.Tests);
    
    // Questions ===============================================================================================

    builder.EntitySet<Question>("Question");
    
    // TestParticipation =======================================================================================
    
    builder.EntitySet<TestParticipation>("TestParticipation");
    
    // Unbound operations ======================================================================================
    var downloadImageFunc = builder.Function(nameof(ImagesLibraryController.DownloadImage));
    downloadImageFunc.Parameter<string>("name");
    downloadImageFunc.Returns(typeof(IActionResult));
    
    var currentUser = builder.Function(nameof(UsersController.CurrentUser));
    currentUser.ReturnsFromEntitySet<User>(usersEntitySet);
            
    builder.EnableLowerCamelCase();
            
    return builder.GetEdmModel();
    
}


static async Task InitializeDb(GraphLabsContext context, InitialData initialData)
{
    await context.Database.MigrateAsync();
    
    if (!context.TaskModules.Any())
    {
        foreach (var module in initialData.GetTaskModules())
        {
            context.TaskModules.Add(module);
        }
        await context.SaveChangesAsync();
    }
    if (!context.TaskVariants.Any())
    {
        foreach (var variant in initialData.GetTaskVariants(context.TaskModules))
        {
            context.TaskVariants.Add(variant);
        }
        await context.SaveChangesAsync();
    }
    if (!context.Users.Any())
    {
        foreach (var user in initialData.GetUsers())
        {
            context.Users.Add(user);
        }
        await context.SaveChangesAsync();
    }
}



public class ApiExplorerIgnores : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        if (action.Controller.ControllerName.Equals("Pwa"))
            action.ApiExplorer.IsVisible = false;
    }
}

