using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AadAuthClient.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {

        private readonly IProtectedAPICaller _protectedAPICaller;
        private readonly IAuthenticationConfig _authenticationConfig;
        private readonly ILogger _logger;

        public ProductsController(
            IProtectedAPICaller protectedAPICaller, 
            IAuthenticationConfig authenticationConfig,
            ILogger<ProductsController> logger)
        {
            _protectedAPICaller = protectedAPICaller;
            _authenticationConfig = authenticationConfig;
            _logger = logger;
        }

        public async Task<IActionResult> GetProducts()
        {
            IAuthenticationConfig config = _authenticationConfig.ReadFromJsonFile("appsettings.json");

            //Console.WriteLine($"Calling the protected API on {config.AADAuthBaseAddress}/api/books ...");
            _logger.LogInformation($"Calling the protected API on {config.AADAuthBaseAddress}/api/books ...");
            
            HttpResponseMessage response = await _protectedAPICaller.CallWebApiAsync($"{config.AADAuthBaseAddress}/api/books");
            _logger.LogInformation($"Response from Protected API : {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                // var result = JsonConvert.DeserializeObject<IEnumerable<string>>(json);
                return Ok(json);
            }

            // Console.WriteLine($"Failed to call protected webApi : {response.StatusCode}");
            // string content = await response.Content.ReadAsStringAsync();

            //Console.WriteLine($"Content : {content}");
            //return new List<string> { $"Failed to call the Protected webApi! Content : {content}" };
            // response.ToList().ForEach(i => Console.WriteLine($"{i}"));
            return Ok(response.StatusCode); //.Union(response2));
        }
    }
}
