using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VelorettiAPI.Models;

namespace VelorettiAPI.Services;

public class UserService
{
    private readonly DatabaseContext _context;
    private readonly IConfiguration _configuration;
    public UserService(DatabaseContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    //get all users
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    //get user by Id
    public async Task<User> GetById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"The user with the ID {id} was not found.");
        }
        return user;
    }
    //get user by email
    public async Task<User> GetUserByEmail(string email)
    {
        return await _context.Users.SingleOrDefaultAsync(user => user.Email == email) ?? throw new KeyNotFoundException($"User with email {email} not found.");
    }
    //create user
    public async Task CreateUser(User user)
    {
        var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == user.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email not valid.");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
    //verify user
    public async Task<bool> VerifyUser( string email, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(user => user.Email == email);
        if (user == null)
        {
            return false;
        }
        return BCrypt.Net.BCrypt.Verify(password, user.Password);
    }
    //generate jwt token
    public string GenerateJWT(User user)
    {
        var JwtKey = _configuration["Jwt:key"];
        if(string.IsNullOrEmpty(JwtKey))
        {
            throw new ArgumentNullException("Jwt:key", "JWT key configuration is missing or null.");
        }
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}