using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VelorettiAPI.Services;


var builder = WebApplication.CreateBuilder(args);

// Load environment variables and replace placeholders
string GetEnvironmentVariable(string key, string defaultValue = "") => 
    Environment.GetEnvironmentVariable(key) ?? defaultValue
;

var jwtKey = GetEnvironmentVariable("JWT_KEY");
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT_KEY environment variable is missing.");
}

var host = GetEnvironmentVariable("DB_HOST");
var port = GetEnvironmentVariable("DB_PORT");
var dbName = GetEnvironmentVariable("DB_NAME");
var user = GetEnvironmentVariable("DB_USER");
var password = GetEnvironmentVariable("DB_PASSWORD");

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();
// Service for authentication using JWT Token
var key = Encoding.ASCII.GetBytes(jwtKey);
if (key.Length < 32)
{
    throw new ArgumentOutOfRangeException("JWT_KEY", "The encryption algorithm 'HS256' requires a key size of at least 256 bits (32 bytes).");
}
builder.Configuration["Jwt:key"] = jwtKey; // Add the JWT key to the configuration

builder.Services.AddAuthentication(x => 
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };

});
builder.Services.AddAuthorization(options => 
{
    options.AddPolicy("Admin", policy => policy.RequireClaim("IsAdmin", "True"));
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

// Add DbContext with PostgreSQL configuration
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found")
;
var connectionString = defaultConnectionString.
    Replace("${DB_HOST}", host)
    .Replace("${DB_PORT}", port)
    .Replace("${DB_NAME}", dbName)
    .Replace("${DB_USER}", user)
    .Replace("${DB_PASSWORD}", password)
;
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString))
;

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

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();