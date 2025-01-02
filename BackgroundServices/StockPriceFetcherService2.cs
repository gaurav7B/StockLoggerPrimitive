using HtmlAgilityPack;
using Newtonsoft.Json;
using OtpNet;
using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Diagnostics;
using System.Text;
using System.Text.Json;


namespace StockLogger.BackgroundServices
{
    public class StockPriceFetcherService2 : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;
        string authorizationToken;


        public StockPriceFetcherService2(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _stocks = StockList.GetStocks();
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




        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Fetch public IP using ipify API
            string publicIp = await GetPublicIPAsync();

            // Setup login credentials and generate TOTP
            var loginData = new
            {
                clientcode = "AAAF282130", // Your actual client code
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
                    authorizationToken = loginResponseJson.data.jwtToken;  // Corrected key  // Assuming the token is present here
                    Console.WriteLine("Login Successful: " + JsonConvert.SerializeObject(loginResponseJson, Formatting.Indented));

                    // Now fetch the historical data
                    //await GetHistoricalData(publicIp, authorizationToken);
                }
                else
                {
                    Console.WriteLine("Login Failed: " + loginResponseJson.message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                //    var nextRunTime = DateTime.UtcNow.AddMinutes(1).AddSeconds(-DateTime.UtcNow.Second); // Calculate next full minute
                //    var delay = nextRunTime - DateTime.UtcNow; // Calculate the delay to the next minute

                //    if (delay > TimeSpan.Zero)
                //    {
                //        await Task.Delay(delay, stoppingToken); // Wait until the next full minute
                //    }

                //    var stopwatch = Stopwatch.StartNew();

                //    var tasks = _stocks.Select(stock => Task.Run(async () =>
                //    {
                //        string fromdate = DateTime.Today.AddHours(9).ToString("yyyy-MM-dd HH:mm");
                //        string todate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                //        var data = new
                //        {
                //            exchange = "NSE",
                //            symboltoken = stock.symboltoken,
                //            interval = "ONE_MINUTE",
                //            fromdate = fromdate,
                //            todate = todate
                //        };

                //        var jsonData = JsonConvert.SerializeObject(data);
                //        var client = new HttpClient();

                //        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/secure/angelbroking/historical/v1/getCandleData")
                //        {
                //            Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                //        };

                //        // Set the headers
                //        requestMessage.Headers.Add("Accept", "application/json");
                //        requestMessage.Headers.Add("X-SourceID", "WEB");
                //        requestMessage.Headers.Add("X-ClientLocalIP", "192.168.56.177"); // Replace with your actual IP
                //        requestMessage.Headers.Add("X-ClientPublicIP", publicIp);
                //        requestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX"); // Replace with your actual MAC address
                //        requestMessage.Headers.Add("X-UserType", "USER");
                //        requestMessage.Headers.Add("Authorization", "Bearer " + authorizationToken);
                //        requestMessage.Headers.Add("X-PrivateKey", "DcsJlRJp"); // Your actual API Key

                //        try
                //        {
                //            // Send request to get historical data
                //            HttpResponseMessage response = await client.SendAsync(requestMessage);
                //            response.EnsureSuccessStatusCode(); // Throws an exception if not successful

                //            // Read and display the response
                //            string responseContent = await response.Content.ReadAsStringAsync();
                //            dynamic responseJson = JsonConvert.DeserializeObject(responseContent);

                //            // Check if the request is successful and print the data
                //            if (responseJson.status == true)
                //            {
                //                Console.WriteLine("Historical Data: " + JsonConvert.SerializeObject(responseJson.data, Formatting.Indented));
                //            }
                //            else
                //            {
                //                Console.WriteLine("Failed to fetch data: " + responseJson.message);
                //            }
                //        }
                //        catch (Exception e)
                //        {
                //            Console.WriteLine("Error fetching historical data: " + e.Message);
                //        }
                //    }, stoppingToken));

                //    // Wait for all tasks to complete.
                //    await Task.WhenAll(tasks);
                //    stopwatch.Stop();

                //    // Trigger garbage collection periodically
                //    GC.Collect();
                //    GC.WaitForPendingFinalizers();
                //}


                while (!stoppingToken.IsCancellationRequested)
                {
                    var stopwatch = Stopwatch.StartNew();

                    var tasks = _stocks.Select(stock => Task.Run(async () =>
                    {
                        string fromdate = DateTime.Today.AddHours(9).ToString("yyyy-MM-dd HH:mm");
                        string todate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                        var data = new
                        {
                            exchange = "NSE",
                            symboltoken = stock.symboltoken,
                            interval = "ONE_MINUTE",
                            fromdate = fromdate,
                            todate = todate
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
                        requestMessage.Headers.Add("X-ClientPublicIP", publicIp);
                        requestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX"); // Replace with your actual MAC address
                        requestMessage.Headers.Add("X-UserType", "USER");
                        requestMessage.Headers.Add("Authorization", "Bearer " + authorizationToken);
                        requestMessage.Headers.Add("X-PrivateKey", "DcsJlRJp"); // Your actual API Key

                        try
                        {
                            // Send request to get historical data
                            HttpResponseMessage response = await client.SendAsync(requestMessage);
                            response.EnsureSuccessStatusCode();  // Throws an exception if not successful

                            // Read and display the response
                            string responseContent = await response.Content.ReadAsStringAsync();
                            dynamic responseJson = JsonConvert.DeserializeObject(responseContent);

                            List<RawCandel> RC = new List<RawCandel>();
                            var Can = responseJson.data;

                            // Check if the request is successful and print the data
                            if (responseJson.status == true)
                            {
                                Console.WriteLine("Historical Data: " + JsonConvert.SerializeObject(responseJson.data, Formatting.Indented));
                            }
                            else
                            {
                                Console.WriteLine("Failed to fetch data: " + responseJson.message);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error fetching historical data: " + e.Message);
                        }
                    }, stoppingToken));

                    // Wait for all tasks to complete.
                    await Task.WhenAll(tasks);
                    stopwatch.Stop();

                    // Trigger garbage collection periodically
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Delay for half a second (if needed)
                    //await Task.Delay(20, stoppingToken);
                }

            }
        }

    }
}
