using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using VelorettiAPI.Attributes;
using VelorettiAPI.Models;
using VelorettiAPI.Services;

namespace VelorettiAPI.Controllers;

[Microsoft.AspNetCore.Mvc.Route("/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly EmailService _emailService;
    public UserController(UserService userService, EmailService emailService)
    {
        _userService = userService;
        _emailService = emailService;
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<User>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        if (users == null || users.Count == 0)
        {
            return NotFound("No users found.");
        }
        return Ok(users);
    }
    [HttpGet("{id}")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);
        // Check if the requested client ID matches the authenticated user's ID
        if (userId != id)
        {
            return Forbid();
        }
        var user = await _userService.GetById(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
    [HttpPost("signup")]
    public async Task<IActionResult> CreateUser(User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        try
        {
            await _userService.CreateUser(user);
            var subject = "Welcome!";
            var message = $"Hello {user.Name},\n\nWelcome to our service. We're glad to have you with us!";
            await _emailService.SendEmailAsync(user.Email, subject, message);
            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
        } catch (InvalidOperationException ex) {
            return BadRequest(new {message = ex.Message});
        } catch (Exception ex) {
            return StatusCode(500, new {message = ex.Message});
        }

    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var isValidUser = await _userService.VerifyUser(loginModel.Email, loginModel.Password);
        if (!isValidUser)
        {
            return Unauthorized(new { message = "Email or password invalid." });
        }

        var user = await _userService.GetUserByEmail(loginModel.Email);

        var subject = "Log In";
        var message = $"One login was realized in {DateTime.Now}";
        await _emailService.SendEmailAsync(user.Email, subject, message);
        var token = _userService.GenerateJWT(user);

        return Ok(new { token });
    }
}
