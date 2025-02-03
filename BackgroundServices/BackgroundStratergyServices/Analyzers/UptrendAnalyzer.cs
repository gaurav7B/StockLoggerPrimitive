using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;
using StockLogger.Models.Stratergic_Models.Hammer;
using System.Diagnostics.SymbolStore;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class UptrendAnalyzer
    {


        public bool IsListInUptrendAdvanced(List<Candel> candles)
        {
            const int minCandles = 10; // Minimum required candles for analysis
            //const int smaPeriod = 20;  // Moving average period
            const int smaPeriod = 10;  // Moving average period
            const double bullishThreshold = 0.65; // 65% bullish candles
            const double volumeGrowthThreshold = 0.90; // 25% volume increase

            // Initial validation
            if (candles == null || candles.Count < minCandles)
                return false;

            // Ensure chronological order
            var sorted = candles.OrderBy(c => c.OpenTime).ToList();

            // 1. Price Structure Analysis
            var (higherHighs, higherLows) = AnalyzePriceStructure(sorted);
            if (higherHighs < 0.7 || higherLows < 0.7)
                return false;

            // 2. Moving Average Analysis
            var smaValues = CalculateSMA(sorted, smaPeriod);
            if (!IsPriceAboveSMA(sorted, smaValues, smaPeriod))
                return false;

            // 3. Momentum Analysis
            if (!HasStrongMomentum(sorted))
                return false;

            // 4. Volume Analysis
            if (!HasVolumeConfirmation(sorted, volumeGrowthThreshold))
                return false;

            // 5. Bullish Consistency
            if (CalculateBullishRatio(sorted) < bullishThreshold)
                return false;

            return true;
        }

        // Helper 1: Analyze higher highs and higher lows
        private (double higherHighs, double higherLows) AnalyzePriceStructure(List<Candel> candles)
        {
            int hhCount = 0, hlCount = 0;

            for (int i = 1; i < candles.Count; i++)
            {
                if (candles[i].HighestPrice > candles[i - 1].HighestPrice) hhCount++;
                if (candles[i].LowestPrice > candles[i - 1].LowestPrice) hlCount++;
            }

            return (
                (double)hhCount / (candles.Count - 1),
                (double)hlCount / (candles.Count - 1)
            );
        }

        // Helper 2: Calculate Simple Moving Average
        private List<decimal> CalculateSMA(List<Candel> candles, int period)
        {
            List<decimal> sma = new List<decimal>();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i >= period - 1)
                {
                    decimal sum = candles.Skip(i - period + 1).Take(period).Sum(c => c.EndPrice);
                    sma.Add(sum / period);
                }
                else
                {
                    sma.Add(decimal.MinValue); // Invalid value
                }
            }
            return sma;
        }

        // Helper 3: Check price position relative to SMA
        private bool IsPriceAboveSMA(List<Candel> candles, List<decimal> sma, int validPeriod)
        {
            int validChecks = 0;
            int aboveCount = 0;

            for (int i = validPeriod - 1; i < candles.Count; i++)
            {
                validChecks++;
                if (candles[i].EndPrice > sma[i]) aboveCount++;
            }

            return validChecks > 0 && (double)aboveCount / validChecks >= 0.75;
        }

        // Helper 4: Check momentum using price changes
        private bool HasStrongMomentum(List<Candel> candles)
        {
            int positiveCloses = 0;
            decimal totalChange = 0;

            for (int i = 1; i < candles.Count; i++)
            {
                decimal change = candles[i].EndPrice - candles[i - 1].EndPrice;
                if (change > 0) positiveCloses++;
                totalChange += change;
            }

            double positiveRatio = (double)positiveCloses / (candles.Count - 1);
            decimal avgChange = totalChange / (candles.Count - 1);

            return positiveRatio >= 0.6 && avgChange > 0;
        }

        // Helper 5: Analyze volume growth
        private bool HasVolumeConfirmation(List<Candel> candles, double growthThreshold)
        {
            // Compare last third of data to first third
            int segment = candles.Count / 3;
            if (segment < 1) return true;

            decimal earlyVolume = candles.Take(segment).Average(c => c.Volume);
            decimal lateVolume = candles.TakeLast(segment).Average(c => c.Volume);

            if (earlyVolume == 0) return true; // Avoid division by zero

            double volumeGrowth = (double)((lateVolume - earlyVolume) / earlyVolume);
            return volumeGrowth >= growthThreshold;
        }

        // Helper 6: Calculate bullish candle ratio
        private double CalculateBullishRatio(List<Candel> candles)
        {
            int bullishCount = candles.Count(c => c.IsBullish == true);
            return (double)bullishCount / candles.Count;
        }





        // Helper method to check downtrend
        public bool CheckDowntrend(List<Candel> candles, int currentIndex, int lookback)
        {
            if (currentIndex < lookback) return false;

            for (int i = 1; i <= lookback; i++)
            {
                if (candles[currentIndex - i].EndPrice >= candles[currentIndex - i - 1].EndPrice)
                    return false;
            }
            return true;
        }











        public async void UptrendAnalyzerLogic(List<Candel> CandelData, HttpClient _httpClient, int Range)
        {


            Candel testCandel = CandelData
                            .OrderByDescending(c => c.OpenTime) // Order in descending order
                            .FirstOrDefault();

            if (testCandel != null)
            {

                List<Candel> CandelDataBeforeTestCandel = CandelData
                                   .Where(candel => candel.OpenTime < testCandel.OpenTime)
                                   .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                                   .Take(10)  // Take the last 21 candles
                                   .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                                   .ToList();

                bool isListinUptrend = IsListInUptrendAdvanced(CandelDataBeforeTestCandel);


                if (
                    isListinUptrend
                )
                {







                    if (Range == 1)
                    {
                        HammerDb Payload = new HammerDb
                        {
                            Ticker = testCandel.Ticker,
                            TickerId = testCandel.TickerId,
                            Exchange = testCandel.Exchange,
                            IsHammerDetected = true,
                            DetectionRange = 1,
                            DetectionTime = testCandel.CloseTime,
                            DetectedPrice = testCandel.EndPrice,
                            ExpectedPrice = testCandel.EndPrice * 1.00065m,
                            HammerCandels = null
                        };

                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Hammer",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else
                {
                    Console.WriteLine("No Hammer pattern detected.");
                }
            }

        }            



        public async Task Analyze1MinCandelAsync(string symboltoken, HttpClient _httpClient, CancellationToken stoppingToken, string authToken)
        {

            var symbolTokenValue = symboltoken; // Replace with the correct value from stock
            var token = authToken; // Replace with the actual token
            var startDate = DateTime.Now.AddMinutes(-5); // Example start date
            var endDate = DateTime.Now; // Example end date

            //var requestBody = new
            //{
            //    SymbolToken = symbolTokenValue.ToString(),
            //    AuthorizationToken = token.ToString(),
            //    StartDate = DateTime.Now,
            //    EndDate = DateTime.Now
            //};

            var requestBody = new
            {
                SymbolToken = symbolTokenValue?.ToString() ?? string.Empty,
                AuthorizationToken = token?.ToString() ?? string.Empty,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now
            };

            var client = new HttpClient();
            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", content);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    UptrendAnalyzerLogic(candels, _httpClient, 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {symboltoken}: {ex.Message}");
            }
        }


    }
}

