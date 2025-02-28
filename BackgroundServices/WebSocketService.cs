using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StockLogger.Data;
using StockLogger.Models.Candel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Net.WebSockets;
using System.Text;
using static StockLogger.BackgroundServices.CandelMakerService;
using static StockLogger.Controllers.API_Controllers.BuySellController;

namespace StockLogger.BackgroundServices
{
    public class WebSocketService : BackgroundService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;


        public WebSocketService()
        {
            // Fetch stocks from StockList
            //_stocks = StockList.GetStocks();

            _stocks = StockList2.GetStocks()
                     .Select(s => (s.Ticker, s.Exchange, s.Name, s.Id, s.SymbolToken))
                     .ToList();
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

                // Extract and display the latest traded price
                if (result.Count >= 51) // LTP data should be at least 51 bytes
                {
                    int ltpRaw = BitConverter.ToInt32(buffer, 43);
                    double ltp = ltpRaw / 100.0; // Convert paise to rupees
                    Console.WriteLine($"Latest Price: {ltp}");
                }
                else
                {
                    Console.WriteLine("Received incomplete data.");
                }
            }
        }

        private static async Task SendHeartbeat(ClientWebSocket ws)
        {
            byte[] heartbeatBytes = Encoding.UTF8.GetBytes("ping");
            await ws.SendAsync(new ArraySegment<byte>(heartbeatBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("Sent Heartbeat: ping");
        }


        private static string webSocketUrl = "wss://smartapisocket.angelone.in/smart-stream";
        private static string clientId = "AAAF282130";
        private static string apiKey = "DcsJlRJp";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var client = _httpClient;

                dynamic AuthFeedTokenData = await _httpClient.GetAsync("https://localhost:44364/api/AngelCandel/GetFeedTokenForAngelWebSocket");
                string responseCurrentData = await AuthFeedTokenData.Content.ReadAsStringAsync(stoppingToken);
                dynamic candleData = JsonConvert.DeserializeObject(responseCurrentData);


                var authtoken = candleData.authToken;
                var feedToken = candleData.feedToken;
                using (ClientWebSocket ws = new ClientWebSocket())
                {
                    // Set request headers for authentication
                    ws.Options.SetRequestHeader("Authorization", "Bearer " + authtoken);
                    ws.Options.SetRequestHeader("x-api-key", apiKey);
                    ws.Options.SetRequestHeader("x-client-code", clientId);
                    ws.Options.SetRequestHeader("x-feed-token", feedToken);

                    // Connect to the WebSocket server
                    await ws.ConnectAsync(new Uri(webSocketUrl), CancellationToken.None);
                    Console.WriteLine("Connected to WebSocket!");

                    // Subscribe to ADANIENT-EQ (Token: 25, NSE: 1)
                    await SubscribeToStock(ws, "25");

                    // Start listening for messages
                    _ = Task.Run(() => ListenForMessages(ws));

                    // Maintain heartbeat every 30 seconds
                    while (ws.State == WebSocketState.Open)
                    {
                        await SendHeartbeat(ws);
                        await Task.Delay(30000);
                    }
                }
            }
            catch (Exception ex)
            {

            }
 
        }

    }

}
