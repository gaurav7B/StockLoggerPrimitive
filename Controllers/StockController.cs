using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using StockLogger.Models.DTO;

namespace StockLogger.Controllers
{
    public class StockController : Controller
    {
        private readonly HttpClient _httpClient;

        public StockController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestStockPrice()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTicker(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_1Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_2Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_3Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_4Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_5Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }


        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_6Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_7Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_8Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_9Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }


        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_10Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_11Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }


        [HttpGet]
        public async Task<IActionResult> GetStockPriceByTickerFor_12Service(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
            };

            var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
            if (priceNode == null)
            {
                return NotFound("Price not found.");
            }

            var priceText = priceNode.InnerText.Trim();
            if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
            {
                return BadRequest("Failed to parse price.");
            }

            var stockData = new StockDataDto
            {
                time = DateTime.Now,
                price = price
            };

            return Json(stockData);
        }
    }
}
