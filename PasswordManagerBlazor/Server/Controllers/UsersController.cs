using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Server.Data;
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

        [HttpPost]
        public async Task<IActionResult> AddUser()
        {
            var user = new User { FirstName = "Test User", LastName = "Test Name", Email="test@email.com", Password="Mleko", Active=true, Hash="1234567qwert"};
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
