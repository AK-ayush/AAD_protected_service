using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AadAuth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]         // unauthorised users can't acces this API
    public class BooksController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllBooks()
        {
            // Debug.WriteLine($"The user name is {User.Claims.First(c => c.Type == ClaimTypes.Name).Value}");  // LINQ  || standarize by every one
            // Debug.WriteLine($"The user's object Id is {User.Claims.First(c => c.Type == ClaimConstants.ObjectId).Value}");    // LINQ  || AzureAD specific
            return Ok(new[] { "1: Harry Potter", "2: Lord of the Ring", "3: C# Fundamentals" }); 
        }
    }
}