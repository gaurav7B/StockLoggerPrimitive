using HtmlAgilityPack;
using Newtonsoft.Json;
using OtpNet;
using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;


namespace StockLogger.BackgroundServices
{
    public class StockPriceFetcherService2 : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;


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

        private async Task<string> FetchAuthTokenAsync(CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:44364/api/Token", stoppingToken);
                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                dynamic data = JsonConvert.DeserializeObject(responseData);
                return data?.authToken;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string authorizationToken = null;

            // Fetch public IP using ipify API
            string publicIp = await GetPublicIPAsync();

            // Fetch token before the service starts
            authorizationToken = await FetchAuthTokenAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
                {
                    var stopwatch = Stopwatch.StartNew();

                if(authorizationToken != null)
                {
                    var tasks = _stocks.Select(stock => Task.Run(async () =>
                    {

                            string Ticker = stock.ticker;
                            long TickeId = stock.id;
                            string Exchange = stock.exchange;


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
                                response.EnsureSuccessStatusCode();

                                // Read and display the response
                                string responseContent = await response.Content.ReadAsStringAsync();
                                dynamic responseJson = JsonConvert.DeserializeObject(responseContent);

                                List<Candel> RC = new List<Candel>();
                                var RawCandels = responseJson.data;

                                if (RawCandels != null)
                                {
                                    foreach (var c in RawCandels)
                                    {
                                        Candel rawCandel = new Candel
                                        {
                                            OpenTime = DateTime.Parse(c[0].ToString()),
                                            CloseTime = DateTime.Parse(c[0].ToString()).AddMinutes(1),

                                            StartPrice = Convert.ToDecimal(c[1]),
                                            HighestPrice = Convert.ToDecimal(c[2]),
                                            LowestPrice = Convert.ToDecimal(c[3]),
                                            EndPrice = Convert.ToDecimal(c[4]),

                                            Ticker = Ticker,
                                            TickerId = TickeId,
                                            Exchange = Exchange,

                                            Volume = Convert.ToDecimal(c[5]),
                                        };

                                        rawCandel.SetBullBearStatus();
                                        rawCandel.SetPriceChange();

                                        RC.Add(rawCandel);

                                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Candel",
                                              new StringContent(JsonConvert.SerializeObject(rawCandel), Encoding.UTF8, "application/json"),
                                              stoppingToken);

                                    }

                                }

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
                }

                    stopwatch.Stop();

                    // Trigger garbage collection periodically
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

        }

    }
}
