using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {   
            var users = await _userService.GetAllUsersAsync();
            if(users == null || users.Count == 0)
            {
                return NotFound("No users found.");
            }
            return Ok(users);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
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
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            var subject = "Welcome!";
            var message = $"Hello {user.Name},\n\nWelcome to our service. We're glad to have you with us!";
            await _emailService.SendEmailAsync(user.Email, subject, message);
            await _userService.CreateUser(user);
            return CreatedAtAction(nameof(GetUserById), new {id = user.UserId}, user);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            var isValidUser = await _userService.VerifyUser(loginModel.Email, loginModel.Password);
            if(!isValidUser)
            {
                return Unauthorized(new {message = "Email or password invalid."});
            }

            var user = await _userService.GetUserByEmail(loginModel.Email);
            var token = _userService.GenerateJWT(user);

            return Ok(new {token});
        }
    }
