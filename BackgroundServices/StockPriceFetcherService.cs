﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StockLogger.Helpers;
using StockLogger.Models.DTO;

namespace StockLogger.BackgroundServices
{
    public class StockPriceFetcherService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StockPriceFetcherService> _logger;
        private readonly StockDataLogger _stockDataLogger;

        public StockPriceFetcherService(HttpClient httpClient, ILogger<StockPriceFetcherService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _stockDataLogger = new StockDataLogger(@"C:\Users\Admin\Desktop\Project");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var stocks = (await GetStockTickerExchanges()).Select(x => new { Ticker = (string)x.Ticker, Exchange = (string)x.Exchange }).ToArray(); //got the TICKERS here using the API

                var stockTasks = stocks.Select(stock => FetchAndLogStockPriceAsync(stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
                await Task.WhenAll(stockTasks); // Process all tasks in parallel
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait for 1 minute before the next iteration
            }
        }

        // Method to fetch stock TICKER and EXCHANGE data from the API
        private async Task<IEnumerable<dynamic>> GetStockTickerExchanges()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:44364/api/StockTickerExchange/GetStockTickerExchangesWithoutID"); // API to get the TICKER and EXCHANGE

                if (response.IsSuccessStatusCode)
                {
                    var stockTickerExchanges = await response.Content.ReadAsAsync<IEnumerable<StockTickerExchangeDto>>();
                    return stockTickerExchanges.Select(x => new { Ticker = x.Ticker, Exchange = x.Exchange });
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;

            }
        }


        private async Task FetchAndLogStockPriceAsync(string ticker, string exchange)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://localhost:44364/Stock/GetStockPriceByTicker?ticker={ticker}&exchange={exchange}"); // API to get the TICKER Price

                if (response.IsSuccessStatusCode)
                {
                    var stockData = await response.Content.ReadAsAsync<StockDataDto>();

                    _stockDataLogger.LogStockDataTXT(ticker, exchange, stockData);
                    _stockDataLogger.LogStockDataInJSON(ticker, exchange, stockData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching stock price for {ticker}.");
            }
        }
    }
}

