using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PasswordManagerBlazor.Client.Pages;
using PasswordManagerBlazor.Server.Services;
using PasswordManagerBlazor.Shared.DTOs;
using System.Reflection.Metadata;
using System.Security.Claims;

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
                var userId = User.FindFirst(ClaimTypes.Name)?.Value;
                await _passwordManagerService.AddPassword(passwordDto, long.Parse(userId));
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error" + ex);
            }
        }

        // GET: api/password
        [HttpGet]
        public async Task<IActionResult> GetPasswords()
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            var passwords = await _passwordManagerService.GetPasswordsForUser(long.Parse(userId));

            if (passwords == null)
            {
                return NotFound();
            }

            return Ok(passwords);
        }

        // DELETE: api/password/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePassword(int id)
        {
            try
            {
                await _passwordManagerService.RemovePassword(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/password/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] PasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != passwordDto.Id)
            {
                return BadRequest();
            }

            try
            {

                await _passwordManagerService.UpdatePassword(passwordDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/password/duplicates
        [HttpGet("duplicates")]
        public async Task<IActionResult> GetDuplicatePasswords()
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            var passwords = await _passwordManagerService.GetDuplicatePasswordsForUser(long.Parse(userId));

            if (passwords == null || !passwords.Any())
            {
                return NotFound();
            }

            return Ok(passwords);
        }

        // GET: api/password/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPassword(int id)
        {
            try
            {
                var password = await _passwordManagerService.GetPassword(id);
                if (password == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.Name);
                var passwords = await _passwordManagerService.GetPasswordsForUser(long.Parse(userId));
                if (!passwords.Any(p => p.Id == id))
                {
                    return Forbid();
                }

                return Ok(password);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        // GET: api/password/expired
        [HttpGet("expired")]
        public async Task<IActionResult> GetExpiredPasswords()
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            var passwords = await _passwordManagerService.GetExpiredPasswordsForUser(long.Parse(userId));

            if (passwords == null || !passwords.Any())
            {
                return NotFound();
            }

            return Ok(passwords);
        }

    }
}
