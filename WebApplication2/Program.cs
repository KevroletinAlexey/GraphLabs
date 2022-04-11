
using Microsoft.EntityFrameworkCore;
using WebApplication2.DAL;


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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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