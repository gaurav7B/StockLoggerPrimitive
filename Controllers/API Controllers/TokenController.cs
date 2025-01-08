using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OtpNet;
using StockLogger.Data;
using StockLogger.Models.Candel;
using System.Text;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public TokenController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/Token
        [HttpGet]
        public async Task<ActionResult<Token>> GetToken()
        {
            var token = await _context.Token.FirstOrDefaultAsync();
            if (token == null)
            {
                return NotFound();  // Return NotFound if no entry exists
            }
            return Ok(token);  // Return the token if found
        }


        // GET: api/Token/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Token>> GetToken(long id)
        {
            var token = await _context.Token.FindAsync(id);

            if (token == null)
            {
                return NotFound();
            }

            return token;
        }
        // POST: https://localhost:44364/api/Token
        [HttpPost]
        public async Task<ActionResult<Token>> CreateToken()
        {
            // Fetch the authorization token (assuming this is a string)
            string authToken = await GetAuthorizationTokenAsync();

            // Look for an existing token
            var existingToken = await _context.Token.FirstOrDefaultAsync();

            if (existingToken != null)
            {
                // If a token exists, update it
                existingToken.AuthToken = authToken;  // Assuming `AuthToken` is the property to update
                existingToken.AuthTokenCreationTime = DateTime.UtcNow;  // Update the creation time

                // Mark the entry as modified
                _context.Entry(existingToken).State = EntityState.Modified;
            }
            else
            {
                // Otherwise, create a new token
                var token = new Token
                {
                    AuthToken = authToken,
                    AuthTokenCreationTime = DateTime.UtcNow, // Set creation time
                };

                // Add the new token to the context
                _context.Token.Add(token);
            }

            // Save changes to the context (whether adding or updating)
            await _context.SaveChangesAsync();

            // Return the token (either newly created or updated)
            return Ok(existingToken ?? new Token { AuthToken = authToken, AuthTokenCreationTime = DateTime.UtcNow });
        }



        // Helper method to fetch the JWT token
        private async Task<string> GetAuthorizationTokenAsync()
        {
            string authorizationToken = string.Empty;

            // Fetch public IP using ipify API
            string publicIp = await GetPublicIPAsync();

            // Setup login credentials and generate TOTP
            var loginData = new
            {
                clientcode = "AAAF282130",  // Your actual client code
                password = "6366",          // Your actual pin
                totp = GenerateTOTP("3IGPCM52A2WTQCH7FW2RYOCYIY") // Generate TOTP from secret key
            };

            var loginJsonData = JsonConvert.SerializeObject(loginData);
            var loginClient = new HttpClient();
            var loginRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/auth/angelbroking/user/v1/loginByPassword")
            {
                Content = new StringContent(loginJsonData, Encoding.UTF8, "application/json")
            };

            // Set headers for login request
            loginRequestMessage.Headers.Add("Accept", "application/json");
            loginRequestMessage.Headers.Add("X-UserType", "USER");
            loginRequestMessage.Headers.Add("X-SourceID", "WEB");
            loginRequestMessage.Headers.Add("X-ClientLocalIP", "192.168.56.177");  // Your local IP from ipconfig
            loginRequestMessage.Headers.Add("X-ClientPublicIP", publicIp);
            loginRequestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX"); // Replace with your MAC address
            loginRequestMessage.Headers.Add("X-PrivateKey", "DcsJlRJp");          // Your actual API Key

            try
            {
                // Send login request and fetch login token
                HttpResponseMessage loginResponse = await loginClient.SendAsync(loginRequestMessage);
                loginResponse.EnsureSuccessStatusCode();  // Throws an exception if not successful
                string loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
                dynamic loginResponseJson = JsonConvert.DeserializeObject(loginResponseContent);

                // Check login status
                if (loginResponseJson.status == true)
                {
                    authorizationToken = loginResponseJson.data.jwtToken;  // Assuming the token is present here
                }
                else
                {
                    throw new Exception("Failed to authenticate: " + loginResponseJson.message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                throw;
            }

            return authorizationToken;
        }

        // Fetches the public IP from ipify API
        private static async Task<string> GetPublicIPAsync()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync("https://api.ipify.org?format=json");
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();
                    dynamic ipData = JsonConvert.DeserializeObject(content);
                    return ipData.ip;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching public IP: " + ex.Message);
                    return string.Empty;
                }
            }
        }

        // Generate TOTP based on the secret key
        private static string GenerateTOTP(string secretKey)
        {
            var otp = new Totp(Base32Encoding.ToBytes(secretKey));
            return otp.ComputeTotp(); // Generates the TOTP value
        }









        // PUT: api/Token/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateToken(long id, Token token)
        {
            if (id != token.Id)
            {
                return BadRequest();
            }

            _context.Entry(token).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TokenExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: https://localhost:44364/api/Token
        [HttpDelete]
        public async Task<IActionResult> TruncateTable()
        {
            try
            {
                // This will truncate the Token table (delete all rows without logging individual deletions)
                await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Token");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private bool TokenExists(long id)
        {
            return _context.Token.Any(e => e.Id == id);
        }
    }
}
