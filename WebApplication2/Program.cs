using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Routing.Conventions;
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
using WebApplication2.DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using WebApplication2.Controllers;
using WebApplication2.Entity;
using WebApplication2.Infrastructure;
using Microsoft.AspNet.OData.Extensions.ODataQueryMapper;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;


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

   // var optionsBuilder = new DbContextOptionsBuilder<GraphLabsContext>();
    //var options = optionsBuilder.UseNpgsql(connectionString!).Options;

    //using (GraphLabsContext db = new GraphLabsContext(options))
    //{
    //    var users = db.Users.ToList();
    //}
    
    // Add services to the container.
    
    //var postgresHost = Environment.GetEnvironmentVariable("DB_HOST");
    //var postgresDb = Environment.GetEnvironmentVariable("DB_NAME");
    //var postgresUser = Environment.GetEnvironmentVariable("DB_USER");
    //var postgresPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var migrationsAssembly = Type.GetType("builder.Services")!.Assembly.FullName;
    
    builder.Services.AddDbContext<GraphLabsContext>(
        o => o.UseNpgsql(connectionString!, b => b.MigrationsAssembly(migrationsAssembly)));
    
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddSingleton<InitialData>();
        builder.Services.AddCors();
    }
    else
    {
        throw new NotImplementedException("Для продакшна сделаем чуть позже");
    }
    
    builder.Services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    builder.Services.AddControllers().AddOData();
    builder.Services.AddMvc(config =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        config.Filters.Add(new AuthorizeFilter(policy));
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
    
    
    

    builder.Services.AddControllers();
    
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
   
    builder.Host.UseSerilog();    //how its work?????
     
    
    var app = builder.Build();
    
    
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseCors(corsBuilder => corsBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
        
        app.UseSwagger();    //для
        app.UseSwaggerUI();  //тестов, потом удалю
    }
    else
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }
    
    
    app.UseSerilogRequestLogging();
            
    app.UseAuthentication();

    
    
    builder.Services.AddMvc(option => option.EnableEndpointRouting = false);
    
    app.UseRouting();
    
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
    
    // app.UseEndpoints(endpoints =>
    // {
    //     //endpoints.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
    //     endpoint
    // });
    
    // app.UseMvc(b =>
    // {
    //     b.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
    //     const string routeName = "odata";
    //     
    // });
    
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

    app.UseAuthorization();

    //app.MapControllers();

    //app.UseHttpsRedirection();

    //app.UseAuthorization();

    //app.MapControllers();


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
        Namespace = "WebApplication2"
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






