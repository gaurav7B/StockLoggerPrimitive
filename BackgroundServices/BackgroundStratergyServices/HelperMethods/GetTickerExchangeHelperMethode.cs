using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.HelperMethods
{
    public class GetTickerExchangeHelperMethode
    {
        private readonly HttpClient _httpClient;

        public GetTickerExchangeHelperMethode()
        {
            _httpClient = new HttpClient();
        }


        // Method to fetch stock TICKER and EXCHANGE data from the API
        public async Task<IEnumerable<StockTickerExchange>> GetStockTickerExchanges()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:44364/api/StockTickerExchange/GetStockTickerExchanges");

                if (response.IsSuccessStatusCode)
                {
                    var stockTickerExchanges = await response.Content.ReadAsAsync<IEnumerable<StockTickerExchange>>();
                    return stockTickerExchanges;
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
    }
}


