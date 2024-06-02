using Microsoft.EntityFrameworkCore;
using VelorettiAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables and replace placeholders
string GetEnvironmentVariable(string key, string defaultValue = "") => 
    Environment.GetEnvironmentVariable(key) ?? defaultValue
;

var host = GetEnvironmentVariable("DB_HOST");
var port = GetEnvironmentVariable("DB_PORT");
var dbName = GetEnvironmentVariable("DB_NAME");
var user = GetEnvironmentVariable("DB_USER");
var password = GetEnvironmentVariable("DB_PASSWORD");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();

// Add DbContext with PostgreSQL configuration
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found")
;

var connectionString = defaultConnectionString.
    Replace("${DB_HOST}", host)
    .Replace("${DB_PORT}", port)
    .Replace("${DB_NAME}", dbName)
    .Replace("${DB_USER}", user)
    .Replace("${DB_PASSWORD}", password);

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString));

// Add authorization services
builder.Services.AddAuthorization();

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