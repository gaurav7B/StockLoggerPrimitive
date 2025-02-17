using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OtpNet;
using StockLogger.Data;
using StockLogger.Models.Candel;
using System;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Web.Helpers;
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

        public class LTPData
        {
            public string Exchange { get; set; }
            public string TradingSymbol { get; set; }
            public string SymbolToken { get; set; }
            public decimal Open { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
            public decimal Close { get; set; }
            public decimal Ltp { get; set; }
        }

        public class LTPApiResponse
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public string ErrorCode { get; set; }
            public LTPData Data { get; set; }
        }

        //POST https://localhost:44364/api/BuySell/LTP
        [HttpPost("LTP")]
        public async Task<IActionResult> LTP()
        {
            // Fetch the authorization token (assuming this is a string)
            string authToken = await GetAuthorizationTokenAsync();

            var client = new HttpClient();


            var LTPData = new
            {
                exchange = "NSE",
                tradingsymbol = "IDEA-EQ",
                symboltoken = "14366"
            };

            var LTPjsonData = JsonConvert.SerializeObject(LTPData);

            var LTPrequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/secure/angelbroking/order/v1/getLtpData")
            {
                Content = new StringContent(LTPjsonData, Encoding.UTF8, "application/json")
            };

            LTPrequestMessage.Headers.Add("Accept", "application/json");
            LTPrequestMessage.Headers.Add("X-SourceID", "WEB");
            LTPrequestMessage.Headers.Add("X-ClientLocalIP", "192.168.56.177");
            LTPrequestMessage.Headers.Add("X-ClientPublicIP", await GetPublicIPAsync());
            LTPrequestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX");
            LTPrequestMessage.Headers.Add("X-UserType", "USER");
            LTPrequestMessage.Headers.Add("Authorization", "Bearer " + authToken);
            LTPrequestMessage.Headers.Add("X-PrivateKey", "GmTkiYil");


            try
            {
                HttpResponseMessage response = await client.SendAsync(LTPrequestMessage);

                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize into the custom ApiResponse class
                var parsedContent = JsonConvert.DeserializeObject<LTPApiResponse>(responseContent);

                // Return the parsed content as JSON result
                return new JsonResult(parsedContent.Data.Ltp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        public class StockTradeResponse
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public string ErrorCode { get; set; }
            public List<TodaysTradeData> Data { get; set; }
        }

        public class TodaysTradeData
        {
            public string Exchange { get; set; }
            public string ProductType { get; set; }
            public string TradingSymbol { get; set; }
            public string InstrumentType { get; set; }
            public string SymbolGroup { get; set; }
            public string StrikePrice { get; set; }
            public string OptionType { get; set; }
            public string ExpiryDate { get; set; }
            public string MarketLot { get; set; }
            public string Precision { get; set; }
            public string Multiplier { get; set; }
            public string TradeValue { get; set; }
            public string TransactionType { get; set; }
            public string FillPrice { get; set; }
            public string FillSize { get; set; }
            public string OrderId { get; set; }
            public string FillId { get; set; }
            public string FillTime { get; set; }
        }


        //GET https://localhost:44364/api/BuySell/GetTodaysOrders
        [HttpGet("GetTodaysOrders")]
        public async Task<IActionResult> GetTodaysOrders()
        {
            // Fetch the authorization token (assuming this is a string)
            string authToken = await GetAuthorizationTokenAsync();

            var client = new HttpClient();

            var LTPrequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://apiconnect.angelone.in/rest/secure/angelbroking/order/v1/getTradeBook");

            LTPrequestMessage.Headers.Add("Accept", "application/json");
            LTPrequestMessage.Headers.Add("X-SourceID", "WEB");
            LTPrequestMessage.Headers.Add("X-ClientLocalIP", "192.168.56.177");
            LTPrequestMessage.Headers.Add("X-ClientPublicIP", await GetPublicIPAsync());
            LTPrequestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX");
            LTPrequestMessage.Headers.Add("X-UserType", "USER");
            LTPrequestMessage.Headers.Add("Authorization", "Bearer " + authToken);
            LTPrequestMessage.Headers.Add("X-PrivateKey", "GmTkiYil");


            try
            {
                HttpResponseMessage response = await client.SendAsync(LTPrequestMessage);

                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize into the custom ApiResponse class
                var parsedContent = JsonConvert.DeserializeObject<StockTradeResponse>(responseContent);

                // Return the parsed content as JSON result
                return new JsonResult(parsedContent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }


        public class BuyData
        {
            public string tradingsymbol { get; set; }
            public string symboltoken { get; set; }
            public decimal CurrentPrice { get; set; }
            public decimal PreviousDaayEndPrice { get; set; }
        }


        //POST https://localhost:44364/api/BuySell/buy
        [HttpPost("buy")]
        public async Task<IActionResult> BuyIntradayStock([FromBody] BuyData buyData)
        {
            // Fetch the authorization token (assuming this is a string)
            string authToken = await GetAuthorizationTokenAsync();

            var client = new HttpClient();

            //string LTP = "";
            //string ExpectedPrice = "";
            //string ProfitPrice = "";

            //var LTPData = new
            //{
            //    exchange = "NSE",
            //    tradingsymbol = "IDEA-EQ",
            //    symboltoken = "14366"
            //};

            //var LTPjsonData = JsonConvert.SerializeObject(LTPData);

            //var LTPrequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/secure/angelbroking/order/v1/getLtpData")
            //{
            //    Content = new StringContent(LTPjsonData, Encoding.UTF8, "application/json")
            //};

            //LTPrequestMessage.Headers.Add("Accept", "application/json");
            //LTPrequestMessage.Headers.Add("X-SourceID", "WEB");
            //LTPrequestMessage.Headers.Add("X-ClientLocalIP", "192.168.56.177");
            //LTPrequestMessage.Headers.Add("X-ClientPublicIP", await GetPublicIPAsync());
            //LTPrequestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX");
            //LTPrequestMessage.Headers.Add("X-UserType", "USER");
            //LTPrequestMessage.Headers.Add("Authorization", "Bearer " + authToken);
            //LTPrequestMessage.Headers.Add("X-PrivateKey", "GmTkiYil");


            //try
            //{
            //    HttpResponseMessage response = await client.SendAsync(LTPrequestMessage);

            //    response.EnsureSuccessStatusCode();

            //    string responseContent = await response.Content.ReadAsStringAsync();

            //    // Deserialize into the custom ApiResponse class
            //    var parsedContent = JsonConvert.DeserializeObject<LTPApiResponse>(responseContent);

            //    LTP = parsedContent.Data.Ltp.ToString();

            //    decimal currentPrice = parsedContent.Data.Ltp;

            //    decimal expectedPrice = currentPrice - (currentPrice * 0.03m);

            //    ExpectedPrice = expectedPrice.ToString();



            //    decimal amount = 10000;

            //    decimal Quntity = amount / currentPrice;




            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(new { ex.Message });
            //}


            decimal amount = 17500;

            decimal Quantity = amount / buyData.CurrentPrice;

            int ModifiedQuantity = (int)Quantity;


            var data = new
            {
                variety = "NORMAL",
                tradingsymbol = buyData.tradingsymbol,
                symboltoken = buyData.symboltoken,
                transactiontype = "BUY",
                exchange = "NSE",
                ordertype = "MARKET",
                producttype = "INTRADAY",
                duration = "DAY",
                price = "0",
                squareoff = "0",
                stoploss = "0",
                quantity = ModifiedQuantity.ToString(),
            };

            var jsonData = JsonConvert.SerializeObject(data);

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

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();



                return Ok(responseContent);

            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }

        }


        public class SellData
        {
            public string tradingsymbol { get; set; }
            public string symboltoken { get; set; }
            public decimal CurrentPrice { get; set; }
        }

        //POST https://localhost:44364/api/BuySell/sell
        [HttpPost("sell")]
        public async Task<IActionResult> SellIntradayStock([FromBody] SellData sellData)
        {
            // Fetch the authorization token (assuming this is a string)
            string authToken = await GetAuthorizationTokenAsync();

            var client = new HttpClient();

            decimal amount = 17500;

            decimal Quantity = amount / sellData.CurrentPrice;

            int ModifiedQuantity = (int)Quantity;

            decimal expectedprice = sellData.CurrentPrice * 1.0025m;

            int ModifiedExpectedPrice = (int)expectedprice;

            var data = new
            {
                variety = "NORMAL",
                tradingsymbol = sellData.tradingsymbol,
                symboltoken = sellData.symboltoken,
                transactiontype = "SELL",
                exchange = "NSE",
                ordertype = "LIMIT",
                producttype = "INTRADAY",
                duration = "DAY",
                price = ModifiedExpectedPrice.ToString(),
                squareoff = "0",
                stoploss = "0",
                quantity = ModifiedQuantity.ToString(),
            };


            var jsonData = JsonConvert.SerializeObject(data);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/secure/angelbroking/order/v1/placeOrder")
            {
                Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
            };

            // Set the headers
            requestMessage.Headers.Add("Accept", "application/json");
            requestMessage.Headers.Add("X-SourceID", "WEB");
            requestMessage.Headers.Add("X-ClientLocalIP", "192.168.56.177");
            requestMessage.Headers.Add("X-ClientPublicIP", await GetPublicIPAsync());
            requestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX");
            requestMessage.Headers.Add("X-UserType", "USER");
            requestMessage.Headers.Add("Authorization", "Bearer " + authToken);
            requestMessage.Headers.Add("X-PrivateKey", "GmTkiYil");


            try
            {
                HttpResponseMessage response = await client.SendAsync(requestMessage);

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();



                return Ok(responseContent);

            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }

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







//If you want to place an intraday market order where the order executes at the current market price, you can set "price": "0".
//Along with this, you can define the squareoff value to set a target profit level.

//Here’s the updated payload for your requirement:

//Updated JSON:
//json
//Copy
//Edit
//{
//  "variety": "NORMAL",
//  "tradingsymbol": "SBIN-EQ",
//  "symboltoken": "3045",
//  "transactiontype": "BUY",
//  "exchange": "NSE",
//  "ordertype": "MARKET",
//  "producttype": "INTRADAY",
//  "duration": "DAY",
//  "price": "0",
//  "squareoff": "1.95",
//  "stoploss": "0",
//  "quantity": "1"
//}
//Key Points:
//price: "0":

//Setting the price to 0 ensures that the order is placed at the current market rate.
//squareoff Parameter:

//The squareoff value determines your profit target.
//In this case, it's set to 1.95, which is approximately 1% of your expected buying price (194.50).
//Example Execution Logic:

//Assuming the current market price is 194.50, the system will:
//Execute the buy order at the market price.
//Place a sell order with a target price at:
//Target Price
//=
//Market Price
//+
//Squareoff
//Target Price=Market Price+Squareoff
//Target Price
//=
//194.50
//+
//1.95
//=
//196.45
//Target Price=194.50+1.95=196.45