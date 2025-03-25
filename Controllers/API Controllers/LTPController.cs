using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StockLogger.Data;
using StockLogger.Models.Candel;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LTPController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;
        private readonly ClientWebSocket _webSocket;
        private string _clientCode = "AAAF282130";
        private string _authToken = "";
        private string _feedToken = "";
        private string _apiKey = "DcsJlRJp";
        private string _wsUrl = "wss://smartapisocket.angelone.in/smart-stream";
        private readonly StockLoggerDbContext _context;

        public LTPController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: api/LTP/WebSocket
        [HttpGet("WebSocket")]
        public async void RunWebSocket(CancellationToken stoppingToken)
        {
            await ConnectWebSocket(stoppingToken);
        }


        private async Task ConnectWebSocket(CancellationToken stoppingToken)
        {
            try
            {
                _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_authToken}");
                _webSocket.Options.SetRequestHeader("x-api-key", _apiKey);
                _webSocket.Options.SetRequestHeader("x-client-code", _clientCode);
                _webSocket.Options.SetRequestHeader("x-feed-token", _feedToken);

                await _webSocket.ConnectAsync(new Uri(_wsUrl), stoppingToken);
                Console.WriteLine("✅ Connected to WebSocket");

                // Subscribe to all stocks at once
                var subscribeMessage = new
                {
                    correlationID = "abcde12345",
                    action = 1,
                    @params = new
                    {
                        mode = 1, // LTP Mode
                        tokenList = _stocks.GroupBy(s => s.exchange)
                            .Select(g => new
                            {
                                exchangeType = 1, // Assuming NSE=1
                                tokens = g.Select(s => s.symboltoken).ToArray()
                            })
                            .ToArray()
                    }
                };

                string jsonMessage = JsonConvert.SerializeObject(subscribeMessage);
                await _webSocket.SendAsync(Encoding.UTF8.GetBytes(jsonMessage), WebSocketMessageType.Text, true, stoppingToken);

                // Start Listening
                _ = SendHeartbeat(stoppingToken);
                await ReceiveData(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ WebSocket error: {ex.Message}");
            }
        }

        private async Task ReceiveData(CancellationToken stoppingToken)
        {
            var buffer = new byte[1024];

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("❌ WebSocket connection closed.");
                    break;
                }

                // Parse Binary Response
                ParseBinaryDataAsync(buffer);
            }
        }

        private async Task ParseBinaryDataAsync(byte[] buffer)
        {
            if (buffer.Length < 51) return; // Minimum size for LTP data

            byte subscriptionMode = buffer[0];  // Subscription Mode
            byte exchangeType = buffer[1];      // Exchange Type
            string token = Encoding.UTF8.GetString(buffer, 2, 25).TrimEnd('\0'); // Token

            long sequenceNumber = BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(27, 8));
            long exchangeTimestamp = BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(35, 8));

            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(exchangeTimestamp);
            DateTime dateTimeUtc = dateTimeOffset.UtcDateTime;
            DateTime dateTimeLocal = dateTimeUtc.ToLocalTime();

            int lastTradedPricePaise = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(43, 4));

            double lastTradedPrice = lastTradedPricePaise / 100.0; // Convert paise to INR

            Console.WriteLine($"📌 LTP Update: Token={token}, Price={lastTradedPrice}, Exchange={exchangeType}, Time={dateTimeLocal}");

            var stock = _stocks.FirstOrDefault(s => s.symboltoken == token);

            LTP LTP = new LTP
            {
                SymbolToken = token,
                Ticker = stock.ticker,
                Exchange = stock.exchange,
                Name = stock.name,
                Price = (decimal)lastTradedPrice,
                Time = dateTimeLocal
            };

            HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/LTP",
                      new StringContent(JsonConvert.SerializeObject(LTP), Encoding.UTF8, "application/json"));
        }

        private async Task SendHeartbeat(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _webSocket.SendAsync(Encoding.UTF8.GetBytes("ping"), WebSocketMessageType.Text, true, stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Heartbeat Error: {ex.Message}");
                }
            }
        }


        // GET: api/LTP
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LTP>>> GetLTPs()
        {
            return await _context.LTP.ToListAsync();
        }

        // GET: api/LTP/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LTP>> GetLTP(long id)
        {
            var ltp = await _context.LTP.FindAsync(id);
            return ltp == null ? NotFound() : Ok(ltp);
        }

        // POST: api/LTP
        [HttpPost]
        public async Task<ActionResult<LTP>> PostLTP(LTP ltp)
        {
            if (ltp == null)
                return BadRequest("Invalid data.");

            _context.LTP.Add(ltp);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLTP), new { id = ltp.Id }, ltp);
        }


        // DELETE: api/LTP/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLTP(long id)
        {
            var ltp = await _context.LTP.FindAsync(id);
            if (ltp == null)
                return NotFound();

            _context.LTP.Remove(ltp);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
