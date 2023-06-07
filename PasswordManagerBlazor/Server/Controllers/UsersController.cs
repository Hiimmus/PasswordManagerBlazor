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
    [Route("api/user")]
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
        [Route("registration")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.Password))
            {
                return BadRequest("Invalid user data.");
            }

            var result = await _userRegistrationService.RegisterUser(userDto);

            if (result == null || !result.Successful)
            {
                return BadRequest("Registration failed");
            }
            //return Ok(new { Succes });
            return Ok(new { result });
        }



        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {

            if (userLoginDto == null || string.IsNullOrEmpty(userLoginDto.Password))
            {
                return BadRequest("Invalid login data.");
            }

            var result = await _userLoginService.LoginUser(userLoginDto);

            if (result == null || !result.Successful)
            {
                return Unauthorized();
            }

            return Ok(result);
        }
    }

}

