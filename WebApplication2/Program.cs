
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using WebApplication2.DAL;

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

    var optionsBuilder = new DbContextOptionsBuilder<GraphLabsContext>();
    var options = optionsBuilder.UseNpgsql(connectionString!).Options;

    using (GraphLabsContext db = new GraphLabsContext(options))
    {
        var users = db.Users.ToList();
    }
    
    // Add services to the container.

    builder.Services.AddControllers();
    
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    
    var app = builder.Build();
    
    
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();


    app.Run();
    
}
catch(Exception ex)
{
    log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}






