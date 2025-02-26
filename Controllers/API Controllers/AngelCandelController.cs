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
using System.Runtime.Intrinsics.X86;
using System.Net.WebSockets;
using Azure.Core;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AngelCandelController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;
        private readonly IMemoryCache _cache;


        public AngelCandelController(StockLoggerDbContext context , IMemoryCache cache)
        {
            _context = context;

            // Fetch stocks from StockList
            //_stocks = StockList.GetStocks();

            _stocks = StockList2.GetStocks()
                     .Select(s => (s.Ticker, s.Exchange, s.Name, s.Id, s.SymbolToken))
                     .ToList();

            _cache = cache;

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
                        existingToken.FeedToken = loginResponseJson.data.feedToken;
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
                            FeedToken = loginResponseJson.data.feedToken,
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


        private static async Task SubscribeToStock(ClientWebSocket ws, string token)
        {
            var request = new
            {
                correlationID = "abcde12345",
                action = 1,  // 1 = Subscribe
                @params = new
                {
                    mode = 1,  // 1 = LTP (Last Traded Price)
                    tokenList = new[]
                    {
                    new { exchangeType = 1, tokens = new[] { token } }  // NSE = 1
                }
                }
            };

            string jsonRequest = JsonConvert.SerializeObject(request);
            byte[] requestBytes = Encoding.UTF8.GetBytes(jsonRequest);

            await ws.SendAsync(new ArraySegment<byte>(requestBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Subscribed to stock {token}");
        }

        private static async Task ListenForMessages(ClientWebSocket ws)
        {
            byte[] buffer = new byte[1024];

            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("WebSocket connection closed.");
                    break;
                }

                // Decode and print the received message
                var message = result;
                Console.WriteLine("Received: " + message);

            }
        }

        private static async Task SendHeartbeat(ClientWebSocket ws)
        {
            byte[] heartbeatBytes = Encoding.UTF8.GetBytes("ping");
            await ws.SendAsync(new ArraySegment<byte>(heartbeatBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("Sent Heartbeat: ping");
        }


        public int count;
        private static string webSocketUrl = "wss://smartapisocket.angelone.in/smart-stream";
        private static string clientId = "AAAF282130";
        private static string apiKey = "DcsJlRJp";


        // POST https://localhost:44364/api/AngelCandel/getCandleDataForTest
        [HttpPost("getCandleDataForTest")]
        public async Task<IActionResult> GetCandleDataForTest([FromBody] StockRequest stockRequest)
        {

            //string authtoken = await GetRefreshedAuthorizationTokenAsync();

            string authtoken = "";

            if (count % 2 != 0)
            {
                authtoken = await GetRefreshedAuthorizationTokenAsync();
                count++;
            }
            else
            {
                var token = await _context.Token.FirstOrDefaultAsync();
                authtoken = token.AuthToken;
                count++;
            }

            var tokenData = await _context.Token.FirstOrDefaultAsync();
            string feedToken = tokenData.FeedToken;


            //using (ClientWebSocket ws = new ClientWebSocket())
            //{
            //    // Set request headers for authentication
            //    ws.Options.SetRequestHeader("Authorization", "Bearer " + authtoken);
            //    ws.Options.SetRequestHeader("x-api-key", apiKey);
            //    ws.Options.SetRequestHeader("x-client-code", clientId);
            //    ws.Options.SetRequestHeader("x-feed-token", feedToken);

            //    // Connect to the WebSocket server
            //    await ws.ConnectAsync(new Uri(webSocketUrl), CancellationToken.None);
            //    Console.WriteLine("Connected to WebSocket!");

            //    // Subscribe to ADANIENT-EQ (Token: 25, NSE: 1)
            //    await SubscribeToStock(ws, "25");

            //    // Start listening for messages
            //    _ = Task.Run(() => ListenForMessages(ws));

            //    // Maintain heartbeat every 30 seconds
            //    while (ws.State == WebSocketState.Open)
            //    {
            //        await SendHeartbeat(ws);
            //        await Task.Delay(30000);
            //    }
            //}



            // Extract only the date part from StartDate
            var startDateOnly = stockRequest.StartDate.Date;
            var EndDateOnly = stockRequest.EndDate.Date;

            var matchingStock = _stocks.FirstOrDefault(s => s.symboltoken == stockRequest.SymbolToken);


            // FOR_SPECIFIC_DAY_TESTING
            var startDateWithTime900 = startDateOnly.Date.AddHours(9).AddMinutes(15);
            var startDateWithTime330 = startDateOnly.Date.AddHours(15).AddMinutes(20);



            ////// CODE TO POPULATE THE DB WITH END PRICES
            //var startDateWithTime900 = DateTime.Now.AddDays(-1).Date.AddHours(9).AddMinutes(15); // Previous days Candel
            //var startDateWithTime330 = DateTime.Now.AddDays(-1).Date.AddHours(15).AddMinutes(30);   // CurrentDays candel LTP

            //////// CODE TO TEST THE POPULATED DATA
            //var startDateWithTime900 = DateTime.Now.Date.AddHours(9).AddMinutes(15); // Previous days Candel
            //var startDateWithTime330 = DateTime.Now;   // CurrentDays candel LTP


            ////// FOR_DAY_TO_DAY_TESTING
            //var startDateWithTime900 = startDateOnly.AddHours(9).AddMinutes(15); // Previous days Candel
            //var startDateWithTime330 = DateTime.Now;   // CurrentDays candel LTP

            ////// FOR_DAY_TO_DAY_TESTING
            //var startDateWithTime900 = DateTime.Now.AddDays(-1); // Previous days Candel
            //var startDateWithTime330 = DateTime.Now;   // CurrentDays candel LTP

            //var startDateWithTime900 = startDateOnly.AddDays(-1).Date.AddHours(9).AddMinutes(15);
            //var startDateWithTime330 = startDateOnly.Date.AddHours(15).AddMinutes(20);

            //// 3_YEARS_TESTING
            //var startDateWithTime900 = startDateOnly.AddDays(-1000).AddHours(9).AddMinutes(15);
            //var startDateWithTime330 = DateTime.Now;


            var data = new
            {
                exchange = "NSE",
                symboltoken = stockRequest.SymbolToken,
                interval = "ONE_MINUTE",// COMPLEX HAMMER without stoploss working fine here
                //interval = "ONE_DAY",
                //interval = "THREE_MINUTE", // DRAGON FLY DOJI
                //interval = "FIVE_MINUTE",//--/// 3 white soilders worked at 100% accuracy profit margin 0.0025 // COMPLEX HAMMER working at 0.0025% profit
                //interval = "TEN_MINUTE",
                //interval = "FIFTEEN_MINUTE",
                //interval = "ONE_HOUR",
                //interval = "THIRTY_MINUTE",
                fromdate = startDateWithTime900.ToString("yyyy-MM-dd HH:mm"),
                todate = startDateWithTime330.ToString("yyyy-MM-dd HH:mm")
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
                    //await GetRefreshedAuthorizationTokenAsync();
                }

                //response.EnsureSuccessStatusCode();

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





        public async Task<string> TOTP5PaisaLoginAsync(string _TOTP = "", string _EmailId = "bhoitegaurav7@gmail.com", string _Pin = "636663")
        {
            _TOTP = GenerateTOTP("GUZDAOBVGAZDKXZVKBDUWRKZ");

            string RequestToken = "";

            try
            {
                string URL = "https://Openapi.5paisa.com/VendorsAPI/Service1.svc/" + "TOTPLogin";
                var dataStringSession = JsonConvert.SerializeObject(new
                {
                    head = new { Key = "GyMVwFkIedy5iFNZsSsQ6zVY56jJ31Zy" },
                    body = new { Email_ID = _EmailId, TOTP = _TOTP, PIN = _Pin }

                });

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, URL)
                {
                    Content = new StringContent(dataStringSession, Encoding.UTF8, "application/json")
                };

                var client = new HttpClient();

                HttpResponseMessage response = await client.SendAsync(requestMessage);

                string responseContent = await response.Content.ReadAsStringAsync();
                dynamic responsData = JsonConvert.DeserializeObject(responseContent);
                RequestToken = responsData.body.RequestToken;

                return RequestToken;

            }
            catch (Exception ex)
            {
            }
            return RequestToken;
        }

        public async Task<string> GetOuth5PaisaLoginAsync(string RequestToken)
        {
            string AccessToken = "";
            try
            {
                string URL = "https://Openapi.5paisa.com/VendorsAPI/Service1.svc/" + "GetAccessToken";
                var dataStringSession = JsonConvert.SerializeObject(new
                {
                    head = new { Key = "GyMVwFkIedy5iFNZsSsQ6zVY56jJ31Zy" },
                    //body = new { ClientCode= ClientCode, JWTToken = Token, Key= VendorKey, AllowMap = Allowmap }
                    body = new { RequestToken = RequestToken, EncryKey = "HmNw0CSQIHnV5b7HspQYLUbhlFE5WA4J", UserId = "VjxWPi3kv5f" }

                });

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, URL)
                {
                    Content = new StringContent(dataStringSession, Encoding.UTF8, "application/json")
                };

                var client = new HttpClient();

                HttpResponseMessage response = await client.SendAsync(requestMessage);

                string responseContent = await response.Content.ReadAsStringAsync();
                dynamic responsData = JsonConvert.DeserializeObject(responseContent);
                AccessToken = responsData.body.AccessToken;

                return AccessToken;

            }
            catch (Exception ex)
            {
            }
            return AccessToken;
        }


        public string RequestToken;
        public string AccessToken;
        private static readonly HttpClient _httpClient = new HttpClient();

        // POST https://localhost:44364/api/AngelCandel/getCandleDataForTest5Paisa
        [HttpPost("getCandleDataForTest5Paisa")]
        public async Task<IActionResult> GetCandleDataForTest5Paisa([FromBody] StockRequest stockRequest)
        {
            //if (RequestToken == null)
            //{
            //    RequestToken = await TOTP5PaisaLoginAsync();
            //    AccessToken = await GetOuth5PaisaLoginAsync(RequestToken);
            //}

            //_cache.Remove("AccessToken");
            //_cache.Remove("CreationTime");

            bool existsAccessToken = _cache.Get("AccessToken") != null;
            bool existsCreationTime = _cache.Get("CreationTime") != null;

            if (existsAccessToken == false)
            {
                RequestToken = await TOTP5PaisaLoginAsync();
                AccessToken = await GetOuth5PaisaLoginAsync(RequestToken);
                // Store in cache with expiration

                _cache.Set("AccessToken", AccessToken);
                _cache.Set("CreationTime", DateTime.Now);
            }

            AccessToken = _cache.Get<string>("AccessToken");
            DateTime CreationTime = _cache.Get<DateTime>("CreationTime");

            if ((DateTime.Now - CreationTime).TotalMinutes > 20)
            {
                RequestToken = await TOTP5PaisaLoginAsync();
                AccessToken = await GetOuth5PaisaLoginAsync(RequestToken);
                // Store in cache with expiration

                _cache.Set("AccessToken", AccessToken);
                _cache.Set("CreationTime", DateTime.Now);
            }

            //string RequestToken = await TOTP5PaisaLoginAsync();
            //string AccessToken = await GetOuth5PaisaLoginAsync(RequestToken);

            // Extract only the date part from StartDate
            var startDateOnly = stockRequest.StartDate.Date;
            var EndDateOnly = stockRequest.EndDate.Date;

            var matchingStock = _stocks.FirstOrDefault(s => s.symboltoken == stockRequest.SymbolToken);


            // FOR_SPECIFIC_DAY_TESTING
            var startDateWithTime900 = startDateOnly.Date.AddHours(9).AddMinutes(15);
            var startDateWithTime330 = startDateOnly.Date.AddHours(15).AddMinutes(20);

            var fromdate = startDateWithTime900.ToString("yyyy-MM-dd");
            var todate = startDateWithTime330.ToString("yyyy-MM-dd");


            var client = _httpClient;

            var requestMessage2 = new HttpRequestMessage(HttpMethod.Get,
                $"https://openapi.5paisa.com/V2/historical/N/C/{stockRequest.SymbolToken}/1m?from={fromdate}&end={fromdate}");

            requestMessage2.Headers.Add("Authorization", "Bearer " + AccessToken);
            requestMessage2.Headers.Add("5Paisa-API-Uid", "nosniff");
            requestMessage2.Headers.Add("Accept", "application/json");


            try
            {

                HttpResponseMessage response2 = await client.SendAsync(requestMessage2);

                string responseContent2 = await response2.Content.ReadAsStringAsync();
                dynamic candleData2 = JsonConvert.DeserializeObject(responseContent2);
                var rawCandelData2 = candleData2.data.candles;


                List<Candel> ModifiedCandelDataList = new List<Candel>();

                if (rawCandelData2 == null)
                {
                    return null;
                }

                foreach (var rawCandel2 in rawCandelData2)
                {
                    Candel newCandel = new Candel
                    {
                        OpenTime = DateTime.Parse(rawCandel2[0].ToString()),

                        CloseTime = DateTime.Parse(rawCandel2[0].ToString()).AddMinutes(1),

                        StartPrice = Convert.ToDecimal(rawCandel2[1]),
                        HighestPrice = Convert.ToDecimal(rawCandel2[2]),
                        LowestPrice = Convert.ToDecimal(rawCandel2[3]),
                        EndPrice = Convert.ToDecimal(rawCandel2[4]),

                        Ticker = matchingStock.ticker,
                        TickerId = matchingStock.id,
                        Exchange = matchingStock.exchange,

                        Volume = Convert.ToDecimal(rawCandel2[5]),
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

