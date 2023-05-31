using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Server.Services;
using PasswordManagerBlazor.Shared.DTOs;
using PasswordManagerBlazor.Shared.Models;
using System;

namespace PasswordManagerBlazor.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public UsersController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        //public class UserController : ControllerBase
        //{
        //    private readonly IUserRegistrationService _userRegistrationService;
        //    private readonly IUserLoginService _userLoginService;

        //    public UserController(IUserRegistrationService userRegistrationService, IUserLoginService userLoginService)
        //    {
        //        _userRegistrationService = userRegistrationService;
        //        _userLoginService = userLoginService;
        //    }

        //[HttpPost]
        //[Route("register")]
        //public async Task<IActionResult> Register([FromBody] UserRegistrationDto userDto)
        //{
        //    if (userDto == null || string.IsNullOrEmpty(userDto.Password))
        //    {
        //        return BadRequest("Invalid user data.");
        //    }

        //    var jwtToken = await _userRegistrationService.RegisterUser(userDto);

        //    if (jwtToken == null)
        //    {
        //        return BadRequest("Registration failed");
        //    }

        //    return Ok(new { Token = jwtToken });
        //}
        [HttpPost]
        public async Task<IActionResult> AddUser()
        {
            var user = new User { FirstName = "Test User", LastName = "Test Name", Active = true, Hash = "1234567qwert", Email = "test@email.com" };

            var password = new PasswordModel { Email = "test@email.com", PasswordHash = "Mleko", Url = "example.com", LastChange = DateTime.Now, User = user };



            _dbContext.Users.Add(user);
            _dbContext.UserPasswords.Add(password);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }


        //[HttpPost]
        //public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        //{

        //    if (userLoginDto == null || string.IsNullOrEmpty(userLoginDto.Password))
        //    {
        //        return BadRequest("Invalid login data.");
        //    }

        //    var jwtToken = await _userLoginService.LoginUser(userLoginDto);

        //    if (string.IsNullOrEmpty(jwtToken))
        //    {
        //        return Unauthorized();
        //    }

        //    return Ok(new { Token = jwtToken });
        //}
    }

}
