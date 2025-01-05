using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StockLogger.Data;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;

        public TestController(StockLoggerDbContext context)
        {
            _context = context;

            // Fetch stocks from StockList
            _stocks = StockList.GetStocks();
        }

        //// POST https://localhost:44364/api/Test/DragonFly
        //[HttpPost("DragonFly")]
        //public async Task<IActionResult> DragonFly(dynamic CandelData)
        //{
        //    return Ok(CandelData);  // Correct usage of Ok()
        //}

        // Define the PriceData class
        public class PriceData
        {
            public int Count { get; set; }
            public Candel DetectedCandel { get; set; }
            public Candel FutureCandel { get; set; }
        }

        // POST https://localhost:44364/api/Test/DragonFly
        [HttpPost("DragonFly")]
        public async Task<IActionResult> DragonFly(List<Candel> CandelData)
        {
            // Define the MegaList
            List<List<Candel>> MegaList = new List<List<Candel>>();

            // Ensure there is at least enough data to form a group
            //if (CandelData.Count >= 4)
            //{
                for (int i = 0; i <= CandelData.Count - 4; i++)
                {
                    // Create a sublist with indices {i, i+1, i+2, i+3}
                    List<Candel> sublist = CandelData.Skip(i).Take(4).ToList();

                    // Add the sublist to the MegaList
                    MegaList.Add(sublist);
                }
            //}

            List<Candel> DrafonFlyDojiCandels = new List<Candel>();
            List<Candel> CorrectPredictedCandels = new List<Candel>();
            List<Candel> WrongPredictedCandels = new List<Candel>();

            List<Candel> LossList = new List<Candel>();

            List<Candel> MisleniousList = new List<Candel>();

            //foreach (List<Candel> candelList in MegaList)
            //{
            //    Candel CandelSample;


            //    // Get the last 3 candles from the list (the most recent 3)
            //    List<Candel> recentThreeCandles = candelList.OrderByDescending(c => c.CloseTime).Take(3).ToList();

            //    Candel latestCandel = recentThreeCandles.FirstOrDefault();

            //    // Check if all three candles are bullish
            //    bool allThreeBullish = recentThreeCandles.All(c => c.IsBullish == true);

            //    // Check if the three candles close higher than the previous one
            //    bool progressiveCloses = recentThreeCandles[0].EndPrice > recentThreeCandles[1].EndPrice
            //                                                            &&
            //                             recentThreeCandles[1].EndPrice > recentThreeCandles[2].EndPrice;

            //    // Check if the bodies of the candles are progressively larger
            //    bool increasingBodySize = (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) > (recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice)
            //                                                                                                  &&
            //                              (recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice) > (recentThreeCandles[2].EndPrice - recentThreeCandles[2].StartPrice);

            //    // Check for small upper and lower shadows
            //    bool smallUpperShadow = (recentThreeCandles[0].HighestPrice - recentThreeCandles[0].EndPrice) < (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice);
            //    bool smallLowerShadow = (recentThreeCandles[0].StartPrice - recentThreeCandles[0].LowestPrice) < (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice);

            //    // Check if the body is at least 60% of the total range (strong body)
            //    decimal range = recentThreeCandles[0].HighestPrice - recentThreeCandles[0].LowestPrice;
            //    bool strongBodyRatio = range != 0 && (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) / range > 0.6m;


            //    // Check if volumes are progressively increasing
            //    bool increasingVolume = recentThreeCandles[0].Volume > recentThreeCandles[1].Volume &&
            //                            recentThreeCandles[1].Volume > recentThreeCandles[2].Volume;

            //    // Check the 4th previous candle for a potential downtrend or neutral pattern
            //    bool priorConsolidationOrBearish = candelList.Count > 3 &&
            //                                       (candelList[3].IsBearish == true || candelList[3].IsBullish == false);

            //    CandelSample = recentThreeCandles[0];

            //    // Combine all conditions to detect the Three White Soldiers pattern
            //    //if (CandelSample != null)
            //    if (allThreeBullish 
            //        && progressiveCloses 
            //        && increasingBodySize
            //        && smallUpperShadow && smallLowerShadow
            //        && strongBodyRatio
            //        && increasingVolume
            //        && priorConsolidationOrBearish
            //        )
            //    //if (allThreeBullish && progressiveCloses && increasingBodySize && priorConsolidationOrBearish)
            //    {
            //        DrafonFlyDojiCandels.Add(candelList[3]);
            //    }
            //}

            //foreach (var c in CandelData)
            //{
            //    Candel recentCandel = c;

            //    // Check if it is a Doji
            //    bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

            //    // Check for a long lower shadow (the shadow should be at least twice the size of the body)
            //    bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

            //    // The body of the candle should be at the top of the range
            //    bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

            //    // If the condition matches (Doji with a long lower shadow and a small body at the top), perform any actions
            //    if (isDoji && longLowerShadow && smallBodyAtTop)
            //    {
            //        DrafonFlyDojiCandels.Add(recentCandel);
            //    }
            //}

            foreach (var c in CandelData)
            {
                Candel recentCandel = c;

                // Check if it is a Doji with a small body
                bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

                // Check for a long lower shadow (shadow size relative to the body)
                bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

                // The body of the candle should be small and at the top of the range
                bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

                // Preceding candle's trend should be bullish (for confirming upward momentum)
                bool precedingBullishTrend = CandelData.Where(x => x.CloseTime < recentCandel.OpenTime)
                                                       .OrderByDescending(x => x.CloseTime)
                                                       .Take(3)
                                                       .All(x => x.EndPrice > x.StartPrice); // At least the last 3 candles should be bullish

                // Check for higher volume
                bool higherVolume = recentCandel.Volume > CandelData.Average(x => x.Volume);

                // The next candle should also be bullish for confirmation
                bool nextCandleBullish = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
                                                   .OrderBy(x => x.OpenTime)
                                                   .FirstOrDefault()?.EndPrice > recentCandel.EndPrice;

                // If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
                if (isDoji && longLowerShadow && smallBodyAtTop && precedingBullishTrend && higherVolume && nextCandleBullish)
                {
                    DrafonFlyDojiCandels.Add(recentCandel);
                }
            }

            decimal loss = 0;
            decimal profit = 0;
            List<PriceData> PriceIncreasedAfterCount = new List<PriceData>();


            foreach (Candel DetectedCandels in DrafonFlyDojiCandels)
            {
                //// Match the detected candel with the next candel based on time
                //Candel nextCandel = CandelData.FirstOrDefault(c => c.CloseTime > DetectedCandels.CloseTime);
                //Candel nextCandel2 = CandelData.FirstOrDefault(c => c.CloseTime > nextCandel?.CloseTime);
                //Candel nextCandel3 = CandelData.FirstOrDefault(c => c.CloseTime > nextCandel2?.CloseTime);

                Candel Parent = CandelData.FirstOrDefault(c => c.CloseTime > DetectedCandels.CloseTime);

                Candel nextCandel = CandelData.FirstOrDefault(c => c.CloseTime > Parent.CloseTime);
                Candel nextCandel2 = CandelData.FirstOrDefault(c => c.CloseTime > nextCandel?.CloseTime);
                Candel nextCandel3 = CandelData.FirstOrDefault(c => c.CloseTime > nextCandel2?.CloseTime);

                IEnumerable<Candel> nextCandels = CandelData.Where(c => c.CloseTime > Parent.CloseTime);


                List<Candel> FurtherCandel = new List<Candel>();

                if (nextCandel == null || nextCandel2 == null || nextCandel3 == null)
                {
                    MisleniousList.Add(DetectedCandels);
                }



                if (nextCandel != null && nextCandel2 != null && nextCandel3 != null)
                {
                    if (nextCandel.EndPrice > DetectedCandels.EndPrice)
                    {
                        CorrectPredictedCandels.Add(DetectedCandels);
                        decimal calculatedProfit = (nextCandel.EndPrice - DetectedCandels.EndPrice);
                        profit = profit + calculatedProfit;
                    }
                    else if (DetectedCandels.EndPrice > nextCandel.EndPrice)
                    {
                        if (nextCandel2.EndPrice > DetectedCandels.EndPrice)
                        {
                            CorrectPredictedCandels.Add(DetectedCandels);
                            decimal calculatedProfit = (nextCandel2.EndPrice - DetectedCandels.EndPrice);
                            profit = profit + calculatedProfit;

                        }
                        else if (nextCandel2.EndPrice < DetectedCandels.EndPrice)
                        {
                            if (nextCandel3.EndPrice > DetectedCandels.EndPrice)
                            {
                                CorrectPredictedCandels.Add(DetectedCandels);

                                decimal calculatedProfit = (nextCandel3.EndPrice - DetectedCandels.EndPrice);
                                profit = profit + calculatedProfit;

                            }
                            else if (nextCandel3.EndPrice < DetectedCandels.EndPrice)
                            {
                                WrongPredictedCandels.Add(DetectedCandels);
                                LossList.Add(nextCandel3);

                                decimal calculatedloss = (DetectedCandels.EndPrice - nextCandel3.EndPrice);
                                loss = loss + calculatedloss;

                                FurtherCandel = CandelData.Where(candel => candel.CloseTime > DetectedCandels.CloseTime)
                                               .OrderBy(candel => candel.CloseTime) // Sorting by CloseTime if needed
                                               .ToList();
                                // Find the first candle in FurtherCandel where EndPrice > DetectedCandels.EndPrice
                                var matchingCandel = FurtherCandel.FirstOrDefault(candel => candel.EndPrice > DetectedCandels.EndPrice);

                                if (matchingCandel != null)
                                {
                                    // Get the index of the matching candle in the original CandelData list
                                    int matchingCandelIndex = CandelData.IndexOf(matchingCandel);

                                    // Get the index of the DetectedCandels in the original list
                                    int detectedCandelIndex = CandelData.IndexOf(DetectedCandels);

                                    // Calculate how many candles later it was detected
                                    int candlesAfterDetected = matchingCandelIndex - detectedCandelIndex;

                                    PriceIncreasedAfterCount.Add(new PriceData
                                    {
                                        Count = candlesAfterDetected,
                                        DetectedCandel = DetectedCandels,
                                        FutureCandel = matchingCandel
                                    });
                                }
                            }
                            else
                            {
                                MisleniousList.Add(DetectedCandels);
                            }
                        }

                    }else if (DetectedCandels.EndPrice == nextCandel.EndPrice)
                    {
                        if (nextCandel2.EndPrice > DetectedCandels.EndPrice)
                        {
                            CorrectPredictedCandels.Add(DetectedCandels);

                            decimal calculatedProfit = (nextCandel2.EndPrice - DetectedCandels.EndPrice);
                            profit = profit + calculatedProfit;
                        }
                        else if (nextCandel2.EndPrice < DetectedCandels.EndPrice)
                        {
                            if (nextCandel3.EndPrice > DetectedCandels.EndPrice)
                            {
                                CorrectPredictedCandels.Add(DetectedCandels);

                                decimal calculatedProfit = (nextCandel3.EndPrice - DetectedCandels.EndPrice);
                                profit = profit + calculatedProfit;
                            }
                            else if (nextCandel3.EndPrice < DetectedCandels.EndPrice)
                            {
                                WrongPredictedCandels.Add(DetectedCandels);
                                LossList.Add(nextCandel3);
                                decimal calculatedloss = (DetectedCandels.EndPrice - nextCandel3.EndPrice);
                                loss = loss + calculatedloss;
                            }
                        }
                    }
                }

            }


            // Get the object(s) in DrafonFlyDojiCandels that are not present in the other two lists
            var missingObjects = DrafonFlyDojiCandels
                .Where(candle => !CorrectPredictedCandels.Contains(candle) && !WrongPredictedCandels.Contains(candle))
                .ToList();

            foreach(var c in missingObjects)
            {
                MisleniousList.Add(c);
            }

            decimal sumEndPrice = DrafonFlyDojiCandels.Sum(candel => candel.EndPrice);
            decimal correctPredictionsSum = CorrectPredictedCandels.Sum(candel => candel.EndPrice);
            decimal wrongPredictionsSum = WrongPredictedCandels.Sum(candel => candel.EndPrice);
            decimal MissleniousSum = WrongPredictedCandels.Sum(candel => candel.EndPrice);

            // Calculate accuracy using the helper method
            double accuracy = CalculateAccuracy(DrafonFlyDojiCandels.Count, CorrectPredictedCandels.Count);


            // Return an anonymous object containing the lists
            return Ok(new
            {
                profit,
                loss, 
                netprofit = profit - loss,
                Accuracy = accuracy,
                Investment = correctPredictionsSum + wrongPredictionsSum + MissleniousSum,
                CorrectPredict = correctPredictionsSum,
                WrongPredict = wrongPredictionsSum,
                DrafonFlyDojiCandels,
                CorrectPredictedCandels,
                WrongPredictedCandels,
                MisleniousList
            });
        }


        // Helper method to calculate accuracy
        private double CalculateAccuracy(int totalPredictions, int correctPredictions)
        {
            if (totalPredictions == 0)
                return 0;

            double accuracy = ((double)correctPredictions / totalPredictions) * 100;
            return Math.Round(accuracy, 2); // Round the result to 2 decimal places
        }












    }
}
