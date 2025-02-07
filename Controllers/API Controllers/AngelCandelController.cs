using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models;
using StockLogger.Models.Candel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OtpNet;
using System;
using static StockLogger.Controllers.API_Controllers.BuySellController;
using StockLogger.Models.Stratergic_Models.Hammer;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AngelCandelController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;

        public AngelCandelController(StockLoggerDbContext context)
        {
            _context = context;

            // Fetch stocks from StockList
            _stocks = StockList.GetStocks();
        }

        // POST api/angelcandel/login
        [HttpPost("login")]
        public async Task<IActionResult> Login()
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
                    return Ok(new { Token = authorizationToken });  // Return the JWT token
                }
                else
                {
                    return Unauthorized(new { Message = loginResponseJson.message }); // Return failed login message
                }
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = "Error: " + e.Message });  // Return error message if exception occurs
            }
        }




        // Helper method to fetch the JWT token
        private async Task<string> GetRefreshedAuthorizationTokenAsync()
        {
            var Token = await _context.Token.FirstOrDefaultAsync();

            var RefreshToken = Token.RefreshToken;

            string authorizationToken = string.Empty;

            // Fetch public IP using ipify API
            string publicIp = await GetPublicIPAsync();

            // Setup login credentials and generate TOTP
            var loginData = new
            {
                refreshToken = RefreshToken,
            };

            var loginJsonData = JsonConvert.SerializeObject(loginData);
            var loginClient = new HttpClient();
            var loginRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/auth/angelbroking/jwt/v1/generateTokens")
            {
                Content = new StringContent(loginJsonData, Encoding.UTF8, "application/json")
            };

            // Set headers for login request
            loginRequestMessage.Headers.Add("Authorization", "Bearer " + Token.AuthToken);
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


                    // Look for an existing token
                    var existingToken = await _context.Token.FirstOrDefaultAsync();

                    if (existingToken != null)
                    {
                        // If a token exists, update it
                        existingToken.AuthToken = loginResponseJson.data.jwtToken;  // Assuming `AuthToken` is the property to update
                        existingToken.RefreshToken = loginResponseJson.data.refreshToken;
                        existingToken.AuthTokenCreationTime = DateTime.UtcNow;  // Update the creation time

                        // Mark the entry as modified
                        _context.Entry(existingToken).State = EntityState.Modified;
                    }
                    else
                    {
                        // Otherwise, create a new token
                        var token = new Token
                        {
                            AuthToken = loginResponseJson.data.jwtToken,
                            RefreshToken = loginResponseJson.data.refreshToken,
                            AuthTokenCreationTime = DateTime.UtcNow, // Set creation time
                        };

                        // Add the new token to the context
                        _context.Token.Add(token);
                    }

                    // Save changes to the context (whether adding or updating)
                    await _context.SaveChangesAsync();

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

        // POST https://localhost:44364/api/AngelCandel/getCandleDataForTest
        [HttpPost("getCandleDataForTest")]
        public async Task<IActionResult> GetCandleDataForTest([FromBody] StockRequest stockRequest)
        {
            
            string authtoken = await GetRefreshedAuthorizationTokenAsync();

            // Extract only the date part from StartDate
            var startDateOnly = stockRequest.StartDate.Date;
            var EndDateOnly = stockRequest.EndDate.Date;

            var matchingStock = _stocks.FirstOrDefault(s => s.symboltoken == stockRequest.SymbolToken);

            // Create start date with time 9:15 AM
            //var startDateWithTime900 = startDateOnly.AddHours(9).AddMinutes(15);
            var startDateWithTime900 = startDateOnly.AddDays(-1000).AddHours(9).AddMinutes(15);
            //var startDateWithTime900 = DateTime.Now.AddDays(-20).AddHours(9).AddMinutes(15);

            // Create start date with time 3:30 PM
            //var startDateWithTime330 = EndDateOnly.AddHours(15).AddMinutes(20);
            var startDateWithTime330 = DateTime.Now.AddHours(15).AddMinutes(20);

            var data = new
            {
                exchange = "NSE",
                symboltoken = stockRequest.SymbolToken,
                //interval = "ONE_MINUTE",// COMPLEX HAMMER without stoploss working fine here
                interval = "ONE_DAY",
                //interval = "THREE_MINUTE", // DRAGON FLY DOJI
                //interval = "FIVE_MINUTE",//--/// 3 white soilders worked at 100% accuracy prfit margin 0.0025 // COMPLEX HAMMER working at 0.0025% profit
                //interval = "TEN_MINUTE",
                //interval = "FIFTEEN_MINUTE",
                //interval = "ONE_HOUR",
                //interval = "THIRTY_MINUTE",
                fromdate = startDateWithTime900.ToString("yyyy-MM-dd HH:mm"),
                todate = startDateWithTime330.ToString("yyyy-MM-dd HH:mm")
                ////fromdate = DateTime.Today.AddHours(9).AddMinutes(15).ToString("yyyy-MM-dd HH:mm"),
                ////todate = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };

            var jsonData = JsonConvert.SerializeObject(data);
            var client = new HttpClient();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/secure/angelbroking/historical/v1/getCandleData")
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
            //requestMessage.Headers.Add("Authorization", "Bearer " + stockRequest.AuthorizationToken);
            requestMessage.Headers.Add("Authorization", "Bearer " + authtoken);
            requestMessage.Headers.Add("X-PrivateKey", "DcsJlRJp"); // Your actual API Key

            try
            {
                // Send request to get historical data
                HttpResponseMessage response = await client.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    //HttpResponseMessage tokenResponse = await client.PostAsync("https://localhost:44364/api/Token", null); //Creats new JWT token in database
                }

                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                dynamic candleData = JsonConvert.DeserializeObject(responseContent);
                var rawCandelData = candleData.data;

                List<Candel> ModifiedCandelDataList = new List<Candel>();

                if (rawCandelData == null)
                {
                    return null;
                }

                var miutesToAdd = 0;

                if (data.interval == "ONE_MINUTE")
                {
                    miutesToAdd = 1;
                }
                else if(data.interval == "THREE_MINUTE")
                {
                    miutesToAdd = 3;
                }
                else if (data.interval == "FIVE_MINUTE")
                {
                    miutesToAdd = 5;
                }
                else if (data.interval == "TEN_MINUTE")
                {
                    miutesToAdd = 10;
                }
                else if (data.interval == "FIFTEEN_MINUTE")
                {
                    miutesToAdd = 15;
                }

                foreach (var rawCandel in rawCandelData)
                {
                    Candel newCandel = new Candel
                    {
                        OpenTime = DateTime.Parse(rawCandel[0].ToString()),

                        CloseTime = DateTime.Parse(rawCandel[0].ToString()).AddMinutes(miutesToAdd),

                        StartPrice = Convert.ToDecimal(rawCandel[1]),
                        HighestPrice = Convert.ToDecimal(rawCandel[2]),
                        LowestPrice = Convert.ToDecimal(rawCandel[3]),
                        EndPrice = Convert.ToDecimal(rawCandel[4]),

                        Ticker = matchingStock.ticker,
                        TickerId = matchingStock.id,
                        Exchange = matchingStock.exchange,

                        Volume = Convert.ToDecimal(rawCandel[5]),
                    };
                    newCandel.SetBullBearStatus();
                    newCandel.SetPriceChange();

                    if (newCandel.CloseTime < DateTime.Now)
                    {
                        ModifiedCandelDataList.Add(newCandel);
                    }

                }

                return Ok(ModifiedCandelDataList);  // Return the fetched historical candle data
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error fetching candle data: " + ex.Message });
            }
        }


        // POST https://localhost:44364/api/AngelCandel/getCandleData
        [HttpPost("getCandleData")]
        public async Task<IActionResult> GetCandleData([FromBody] StockRequest stockRequest)
        {
            // Extract only the date part from StartDate
            var startDateOnly = stockRequest.StartDate.Date;
            var EndDateOnly = stockRequest.EndDate.Date;

            var matchingStock = _stocks.FirstOrDefault(s => s.symboltoken == stockRequest.SymbolToken);

            // Create start date with time 9:15 AM
            var startDateWithTime900 = startDateOnly.AddHours(9).AddMinutes(15);

            // Create start date with time 3:30 PM
            var startDateWithTime330 = EndDateOnly.AddHours(15).AddMinutes(30);

            var data = new
            {
                exchange = "NSE",
                symboltoken = stockRequest.SymbolToken,
                interval = "ONE_MINUTE",
                fromdate = stockRequest.StartDate.ToString("yyyy-MM-dd HH:mm"),
                todate = stockRequest.EndDate.ToString("yyyy-MM-dd HH:mm")
            };

            var jsonData = JsonConvert.SerializeObject(data);
            var client = new HttpClient();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/secure/angelbroking/historical/v1/getCandleData")
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
            requestMessage.Headers.Add("Authorization", "Bearer " + stockRequest.AuthorizationToken);
            requestMessage.Headers.Add("X-PrivateKey", "DcsJlRJp"); // Your actual API Key

            try
            {
                // Send request to get historical data
                HttpResponseMessage response = await client.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    HttpResponseMessage tokenResponse = await client.PostAsync("https://localhost:44364/api/Token", null); //Creats new JWT token in database
                }

                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                dynamic candleData = JsonConvert.DeserializeObject(responseContent);
                var rawCandelData = candleData.data;

                List<Candel> ModifiedCandelDataList = new List<Candel>();

                if(rawCandelData == null)
                {
                    return null;
                }

                foreach (var rawCandel in rawCandelData)
                {
                    Candel newCandel = new Candel
                    {
                        OpenTime = DateTime.Parse(rawCandel[0].ToString()),
                        CloseTime = DateTime.Parse(rawCandel[0].ToString()).AddMinutes(1),

                        StartPrice = Convert.ToDecimal(rawCandel[1]),
                        HighestPrice = Convert.ToDecimal(rawCandel[2]),
                        LowestPrice = Convert.ToDecimal(rawCandel[3]),
                        EndPrice = Convert.ToDecimal(rawCandel[4]),

                        Ticker = matchingStock.ticker,
                        TickerId = matchingStock.id,
                        Exchange = matchingStock.exchange,

                        Volume = Convert.ToDecimal(rawCandel[5]),
                    };
                    newCandel.SetBullBearStatus();
                    newCandel.SetPriceChange();

                    if(newCandel.CloseTime < DateTime.Now)
                    {
                        ModifiedCandelDataList.Add(newCandel);
                    }
                    
                }

                return Ok(ModifiedCandelDataList);  // Return the fetched historical candle data
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error fetching candle data: " + ex.Message });
            }
        }

        public class TradingDataResponse
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public string ErrorCode { get; set; }
            public TradingData Data { get; set; }
        }

        public class TradingData
        {
            public string Net { get; set; }
            public string AvailableCash { get; set; }
            public string AvailableIntradayPayin { get; set; }
            public string AvailableLimitMargin { get; set; }
            public string Collateral { get; set; }
            public string M2MUnrealized { get; set; }
            public string M2MRealized { get; set; }
            public string UtilisedDebits { get; set; }
            public string UtilisedSpan { get; set; }
            public string UtilisedOptionPremium { get; set; }
            public string UtilisedHoldingSales { get; set; }
            public string UtilisedExposure { get; set; }
            public string UtilisedTurnover { get; set; }
            public string UtilisedPayout { get; set; }
        }

        // GET https://localhost:44364/api/AngelCandel/getFunds
        [HttpGet("getFunds")]
        public async Task<IActionResult> GetFundDetails()
        {
            var client = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://apiconnect.angelone.in/rest/secure/angelbroking/user/v1/getRMS");

            var token = await _context.Token.FirstOrDefaultAsync();

            // Set the headers
            requestMessage.Headers.Add("Accept", "application/json");
            requestMessage.Headers.Add("X-SourceID", "WEB");
            requestMessage.Headers.Add("X-ClientLocalIP", "192.168.56.177");  // Your local IP from ipconfig
            requestMessage.Headers.Add("X-ClientPublicIP", await GetPublicIPAsync());  // Fetching the public IP dynamically
            requestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX"); // Replace with your actual MAC address
            requestMessage.Headers.Add("X-UserType", "USER");
            requestMessage.Headers.Add("Authorization", "Bearer " + token.AuthToken);
            requestMessage.Headers.Add("X-PrivateKey", "DcsJlRJp"); // Your actual API Key


            try
            {
                // Send request to get historical data
                HttpResponseMessage response = await client.SendAsync(requestMessage);

                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                // Deserialize into the custom ApiResponse class
                var parsedContent = JsonConvert.DeserializeObject<TradingDataResponse>(responseContent);

                // Return the parsed content as JSON result
                return new JsonResult(parsedContent);

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error fetching data: " + ex.Message });
            }

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
    }

    // Model for stock request data
    public class StockRequest
    {
        public string SymbolToken { get; set; }
        public string AuthorizationToken { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

