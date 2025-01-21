using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OtpNet;
using StockLogger.Data;
using StockLogger.Models.Candel;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuySellController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;

        public BuySellController(StockLoggerDbContext context)
        {
            _context = context;

            // Fetch stocks from StockList and transform them into the required tuple format
            _stocks = StockList2.GetStocks()
                .Select(stock => (stock.Ticker, stock.Exchange, stock.Name, stock.Id, stock.SymbolToken))
                .ToList();
        }

        //POST https://localhost:44364/api/BuySell/buy
        [HttpPost("buy")]
        public async Task<IActionResult> BuyIntradayStock()
        {
            // Fetch the authorization token (assuming this is a string)
            string authToken = await GetAuthorizationTokenAsync();

            var data = new
            {
                variety = "NORMAL",
                tradingsymbol = "IDEA-EQ",
                symboltoken = "14366",
                transactiontype = "BUY",
                exchange = "NSE",
                ordertype = "MARKET",
                producttype = "INTRADAY",
                duration = "DAY",
                price = "0",
                squareoff = "0",
                stoploss = "0",
                quantity = "1"
            };

            var jsonData = JsonConvert.SerializeObject(data);
            var client = new HttpClient();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/secure/angelbroking/order/v1/placeOrder")
            {
                Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
            };


            // Set the headers
            requestMessage.Headers.Add("Accept", "application/json");
            requestMessage.Headers.Add("X-SourceID", "WEB");
            requestMessage.Headers.Add("X-ClientLocalIP", "192.168.56.177");  // Your local IP from ipconfig
            requestMessage.Headers.Add("X-ClientPublicIP", await GetPublicIPAsync());  // Fetching the public IP dynamically
            requestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX"); // Replace with your actual MAC address
            requestMessage.Headers.Add("X-UserType", "USER");
            requestMessage.Headers.Add("Authorization", "Bearer " + authToken);
            requestMessage.Headers.Add("X-PrivateKey", "GmTkiYil"); // Your actual API Key


            try
            {
                HttpResponseMessage response = await client.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {

                }

                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();

                return Ok(responseContent);

            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }


            return Ok();
        }



        //POST https://localhost:44364/api/BuySell/sell
        [HttpPost("sell")]
        public async Task<IActionResult> SellIntradayStock()
        {
            // Fetch the authorization token (assuming this is a string)
            string authToken = await GetAuthorizationTokenAsync();

            var data = new
            {
                variety = "NORMAL",
                tradingsymbol = "IDEA-EQ",
                symboltoken = "14366",
                transactiontype = "SELL",              
                exchange = "NSE",             
                ordertype = "MARKET",        
                producttype = "INTRADAY",         
                duration = "DAY",             
                price = "0",             
                squareoff = "0",                             
                stoploss = "0",                             
                quantity = "1"                  
            };

            var jsonData = JsonConvert.SerializeObject(data);
            var client = new HttpClient();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/secure/angelbroking/order/v1/placeOrder")
            {
                Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
            };


            // Set the headers
            requestMessage.Headers.Add("Accept", "application/json");
            requestMessage.Headers.Add("X-SourceID", "WEB");
            requestMessage.Headers.Add("X-ClientLocalIP", "192.168.56.177");  // Your local IP from ipconfig
            requestMessage.Headers.Add("X-ClientPublicIP", await GetPublicIPAsync());  // Fetching the public IP dynamically
            requestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX"); // Replace with your actual MAC address
            requestMessage.Headers.Add("X-UserType", "USER");
            requestMessage.Headers.Add("Authorization", "Bearer " + authToken);
            requestMessage.Headers.Add("X-PrivateKey", "GmTkiYil"); // Your actual API Key


            try
            {
                HttpResponseMessage response = await client.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {

                }

                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();

                return Ok(responseContent);

            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }


            return Ok();
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
            loginRequestMessage.Headers.Add("X-PrivateKey", "GmTkiYil");          // Your actual API Key

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


        // Generate TOTP based on the secret key
        private static string GenerateTOTP(string secretKey)
        {
            var otp = new Totp(Base32Encoding.ToBytes(secretKey));
            return otp.ComputeTotp(); // Generates the TOTP value
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















    }
}
