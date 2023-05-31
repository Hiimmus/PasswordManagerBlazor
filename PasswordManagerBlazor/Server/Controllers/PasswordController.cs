﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManagerBlazor.Server.Services;
using PasswordManagerBlazor.Shared.DTOs;

namespace PasswordManagerBlazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordManagerService _passwordManagerService;

        public PasswordController(IPasswordManagerService passwordManagerService)
        {
            _passwordManagerService = passwordManagerService;
        }

        [HttpPost]
        public async Task<IActionResult> PostPassword([FromBody] PasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _passwordManagerService.AddPassword(passwordDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }

}