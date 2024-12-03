using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using StockLogger.Models.DTO;

namespace StockLogger.Helpers
{
    public class StockDataLogger
    {
        private readonly string _baseDirectory;

        public StockDataLogger(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public void LogStockDataTXT(string ticker, StockDataDto stockData)
        {
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            var dataDirectoryPath = Path.Combine(_baseDirectory, "Data");

            if (!Directory.Exists(dataDirectoryPath))
            {
                Directory.CreateDirectory(dataDirectoryPath);
            }

            var filePath = Path.Combine(dataDirectoryPath, $"{ticker}_{currentDate}.txt");

            var stockDataList = new List<StockDataDto>();

            if (File.Exists(filePath))
            {
                var existingData = File.ReadAllText(filePath);
                stockDataList = JsonSerializer.Deserialize<List<StockDataDto>>(existingData) ?? new List<StockDataDto>();
            }

            stockDataList.Add(stockData);

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(stockDataList, jsonOptions);
            File.WriteAllText(filePath, json);
        }

        public void LogStockDataInJSON(string ticker, StockDataDto stockData)
        {
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            var jsonDataDirectoryPath = Path.Combine(_baseDirectory, "JSON DATA");

            if (!Directory.Exists(jsonDataDirectoryPath))
            {
                Directory.CreateDirectory(jsonDataDirectoryPath);
            }

            var filePath = Path.Combine(jsonDataDirectoryPath, $"{ticker}_{currentDate}.json");

            var stockDataList = new List<StockDataDto>();

            if (File.Exists(filePath))
            {
                var existingData = File.ReadAllText(filePath);
                stockDataList = JsonSerializer.Deserialize<List<StockDataDto>>(existingData) ?? new List<StockDataDto>();
            }

            stockDataList.Add(stockData);

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(stockDataList, jsonOptions);
            File.WriteAllText(filePath, json);
        }
    }
}
