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
    public class UserController : ControllerBase
    {
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IUserLoginService _userLoginService;

        public UserController(IUserRegistrationService userRegistrationService, IUserLoginService userLoginService)
        {
            _userRegistrationService = userRegistrationService;
            _userLoginService = userLoginService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.Password))
            {
                return BadRequest("Invalid user data.");
            }

            var jwtToken = await _userRegistrationService.RegisterUser(userDto);

            if (jwtToken == null)
            {
                return BadRequest("Registration failed");
            }

            return Ok(new { Token = jwtToken });
        }


        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {

            if (userLoginDto == null || string.IsNullOrEmpty(userLoginDto.Password))
            {
                return BadRequest("Invalid login data.");
            }

            var jwtToken = await _userLoginService.LoginUser(userLoginDto);

            if (string.IsNullOrEmpty(jwtToken))
            {
                return Unauthorized();
            }

            return Ok(new { Token = jwtToken });
        }
    }

}
