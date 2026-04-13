using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OtpNet;
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
        private HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;
        private  ClientWebSocket _webSocket;
        private readonly IMemoryCache _cache;
        private string _clientCode = "AAAF282130";
        private string _authToken = "";
        private string _feedToken = "";
        private string _apiKey = "DcsJlRJp";
        private string _wsUrl = "wss://smartapisocket.angelone.in/smart-stream";
        private readonly StockLoggerDbContext _context;

        public LTPController(StockLoggerDbContext context, IMemoryCache cache)
        {
            _context = context;
            _stocks = StockList2.GetStocks()
               .Select(s => (s.Ticker, s.Exchange, s.Name, s.Id, s.SymbolToken))
               .ToList();
        }

    //// GET: api/LTP/WebSocket
    //[HttpGet("WebSocket")]
    //public async void RunWebSocket(CancellationToken stoppingToken)
    //{
    //    await ConnectWebSocket(stoppingToken);
    //}


    //private async Task ConnectWebSocket(CancellationToken stoppingToken)
    //{
    //    try
    //    {
    //        _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_authToken}");
    //        _webSocket.Options.SetRequestHeader("x-api-key", _apiKey);
    //        _webSocket.Options.SetRequestHeader("x-client-code", _clientCode);
    //        _webSocket.Options.SetRequestHeader("x-feed-token", _feedToken);

    //        await _webSocket.ConnectAsync(new Uri(_wsUrl), stoppingToken);
    //        Console.WriteLine("✅ Connected to WebSocket");

    //        // Subscribe to all stocks at once
    //        var subscribeMessage = new
    //        {
    //            correlationID = "abcde12345",
    //            action = 1,
    //            @params = new
    //            {
    //                mode = 1, // LTP Mode
    //                tokenList = _stocks.GroupBy(s => s.exchange)
    //                    .Select(g => new
    //                    {
    //                        exchangeType = 1, // Assuming NSE=1
    //                        tokens = g.Select(s => s.symboltoken).ToArray()
    //                    })
    //                    .ToArray()
    //            }
    //        };

    //        string jsonMessage = JsonConvert.SerializeObject(subscribeMessage);
    //        await _webSocket.SendAsync(Encoding.UTF8.GetBytes(jsonMessage), WebSocketMessageType.Text, true, stoppingToken);

    //        // Start Listening
    //        _ = SendHeartbeat(stoppingToken);
    //        await ReceiveData(stoppingToken);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"❌ WebSocket error: {ex.Message}");
    //    }
    //}

    //private async Task ReceiveData(CancellationToken stoppingToken)
    //{
    //    var buffer = new byte[1024];

    //    while (!stoppingToken.IsCancellationRequested)
    //    {
    //        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

    //        if (result.MessageType == WebSocketMessageType.Close)
    //        {
    //            Console.WriteLine("❌ WebSocket connection closed.");
    //            break;
    //        }

    //        // Parse Binary Response
    //        ParseBinaryDataAsync(buffer);
    //    }
    //}

    //private async Task ParseBinaryDataAsync(byte[] buffer)
    //{
    //    if (buffer.Length < 51) return; // Minimum size for LTP data

    //    byte subscriptionMode = buffer[0];  // Subscription Mode
    //    byte exchangeType = buffer[1];      // Exchange Type
    //    string token = Encoding.UTF8.GetString(buffer, 2, 25).TrimEnd('\0'); // Token

    //    long sequenceNumber = BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(27, 8));
    //    long exchangeTimestamp = BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(35, 8));

    //    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(exchangeTimestamp);
    //    DateTime dateTimeUtc = dateTimeOffset.UtcDateTime;
    //    DateTime dateTimeLocal = dateTimeUtc.ToLocalTime();

    //    int lastTradedPricePaise = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(43, 4));

    //    double lastTradedPrice = lastTradedPricePaise / 100.0; // Convert paise to INR

    //    Console.WriteLine($"📌 LTP Update: Token={token}, Price={lastTradedPrice}, Exchange={exchangeType}, Time={dateTimeLocal}");

    //    var stock = _stocks.FirstOrDefault(s => s.symboltoken == token);

    //    LTP LTP = new LTP
    //    {
    //        SymbolToken = token,
    //        Ticker = stock.ticker,
    //        Exchange = stock.exchange,
    //        Name = stock.name,
    //        Price = (decimal)lastTradedPrice,
    //        Time = dateTimeLocal
    //    };

    //    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/LTP",
    //              new StringContent(JsonConvert.SerializeObject(LTP), Encoding.UTF8, "application/json"));
    //}

    //private async Task SendHeartbeat(CancellationToken stoppingToken)
    //{
    //    while (!stoppingToken.IsCancellationRequested)
    //    {
    //        try
    //        {
    //            await _webSocket.SendAsync(Encoding.UTF8.GetBytes("ping"), WebSocketMessageType.Text, true, stoppingToken);
    //            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"❌ Heartbeat Error: {ex.Message}");
    //        }
    //    }
    //}

    private async Task<(string AuthorizationToken, string RefreshToken, string FeedToken)> GetAuthorizationTokenAsync()
    {
      string authorizationToken = string.Empty;
      string refreshToken = string.Empty;
      string feedToken = string.Empty;

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
      //var loginRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/auth/angelbroking/user/v1/loginByPassword")
      var loginRequestMessage = new HttpRequestMessage(HttpMethod.Post, "")
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
          refreshToken = loginResponseJson.data.refreshToken;
          feedToken = loginResponseJson.data.feedToken;
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

      return (authorizationToken, refreshToken, feedToken);
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


    // GET: api/LTP/WebSocket
    [HttpGet("WebSocket")]
    public async Task<IActionResult> RunWebSocket(CancellationToken stoppingToken)
    {
      //var existingToken = await _context.Token.FirstOrDefaultAsync();
      var (authToken, refreshToken, feedToken) = await GetAuthorizationTokenAsync();

      await ConnectWebSocket(stoppingToken , authToken, feedToken);
      return Ok("WebSocket connection closed after processing stock data.");
    }

    private async Task ConnectWebSocket(CancellationToken stoppingToken , string authToken , string feedToken)
    {
      if (_webSocket == null)
      {
        _webSocket = new ClientWebSocket();
      }

      try
      {
        _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {authToken}");
        _webSocket.Options.SetRequestHeader("x-api-key", _apiKey);
        _webSocket.Options.SetRequestHeader("x-client-code", _clientCode);
        _webSocket.Options.SetRequestHeader("x-feed-token", feedToken);

        await _webSocket.ConnectAsync(new Uri(_wsUrl), stoppingToken);
        Console.WriteLine("✅ Connected to WebSocket");

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

        // Start Listening and Heartbeat
        _ = SendHeartbeat(stoppingToken);
        await ReceiveData(stoppingToken);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ WebSocket error: {ex.Message}");
      }
      finally
      {
        // Close WebSocket after processing
        if (_webSocket.State == WebSocketState.Open)
        {
          await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Processing completed", stoppingToken);
          Console.WriteLine("✅ WebSocket connection closed.");
        }
      }
    }

    private async Task ReceiveData(CancellationToken stoppingToken)
    {
      if (_webSocket == null)
      {
        _webSocket = new ClientWebSocket();
      }

      var buffer = new byte[1024];
      int receivedStocksCount = 0;

      while (!stoppingToken.IsCancellationRequested)
      {
        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

        if (result.MessageType == WebSocketMessageType.Close)
        {
          Console.WriteLine("❌ WebSocket connection closed.");
          break;
        }

        // Parse and Store Data
        await ParseBinaryDataAsync(buffer);
        receivedStocksCount++;

        // If all stocks are received, break the loop
        if (receivedStocksCount >= _stocks.Count)
        {
          Console.WriteLine("✅ All stock data received, closing WebSocket.");
          break;
        }
      }
    }

    private async Task ParseBinaryDataAsync(byte[] buffer)
    {
      if (_webSocket == null)
      {
        _webSocket = new ClientWebSocket();
      }
      if (buffer.Length < 51) return; // Minimum size for LTP data

      byte subscriptionMode = buffer[0];
      byte exchangeType = buffer[1];
      string token = Encoding.UTF8.GetString(buffer, 2, 25).TrimEnd('\0');

      long sequenceNumber = BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(27, 8));
      long exchangeTimestamp = BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(35, 8));

      DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(exchangeTimestamp);
      DateTime dateTimeUtc = dateTimeOffset.UtcDateTime;
      DateTime dateTimeLocal = dateTimeUtc.ToLocalTime();

      int lastTradedPricePaise = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(43, 4));
      double lastTradedPrice = lastTradedPricePaise / 100.0;

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

      _httpClient = new HttpClient();

        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/LTP",
            new StringContent(JsonConvert.SerializeObject(LTP), Encoding.UTF8, "application/json"));
    }

    private async Task SendHeartbeat(CancellationToken stoppingToken)
    {
      if (_webSocket == null)
      {
        _webSocket = new ClientWebSocket();
      }
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
