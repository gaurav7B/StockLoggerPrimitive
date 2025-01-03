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

        // POST https://localhost:44364/api/Test/DragonFly
        [HttpPost("DragonFly")]
        public async Task<IActionResult> DragonFly(List<Candel> CandelData)
        {
            List<Candel> DrafonFlyDojiCandels = new List<Candel>();
            List<Candel> CorrectPredictedCandels = new List<Candel>();
            List<Candel> WrongPredictedCandels = new List<Candel>();

            List<Candel> MisleniousList = new List<Candel>();


            foreach (var c in CandelData)
            {
                Candel recentCandel = c;

                // Check if it is a Doji
                bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

                // Check for a long lower shadow (the shadow should be at least twice the size of the body)
                bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 2 * (recentCandel.EndPrice - recentCandel.StartPrice);

                // The body of the candle should be at the top of the range
                bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

                // If the condition matches (Doji with a long lower shadow and a small body at the top), perform any actions
                if (isDoji && longLowerShadow && smallBodyAtTop)
                {
                    DrafonFlyDojiCandels.Add(recentCandel);
                }
            }

            foreach (Candel DetectedCandels in DrafonFlyDojiCandels)
            {
                // Match the detected candel with the next candel based on time
                Candel nextCandel = CandelData.FirstOrDefault(c => c.CloseTime > DetectedCandels.CloseTime);
                Candel nextCandel2 = CandelData.FirstOrDefault(c => c.CloseTime > nextCandel?.CloseTime);
                Candel nextCandel3 = CandelData.FirstOrDefault(c => c.CloseTime > nextCandel2?.CloseTime);

                if(nextCandel == null || nextCandel2 == null || nextCandel3 == null)
                {
                    MisleniousList.Add(DetectedCandels);
                }



                if (nextCandel != null && nextCandel2 != null && nextCandel3 != null)
                {
                    if (nextCandel.EndPrice > DetectedCandels.EndPrice)
                    {
                        CorrectPredictedCandels.Add(DetectedCandels);
                    }
                    else if (DetectedCandels.EndPrice > nextCandel.EndPrice)
                    {
                        if (nextCandel2.EndPrice > DetectedCandels.EndPrice)
                        {
                            CorrectPredictedCandels.Add(DetectedCandels);
                        }
                        else if (nextCandel2.EndPrice < DetectedCandels.EndPrice)
                        {
                            if (nextCandel3.EndPrice > DetectedCandels.EndPrice)
                            {
                                CorrectPredictedCandels.Add(DetectedCandels);
                            }
                            else if (nextCandel3.EndPrice < DetectedCandels.EndPrice)
                            {
                                WrongPredictedCandels.Add(DetectedCandels);
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
                        }
                        else if (nextCandel2.EndPrice < DetectedCandels.EndPrice)
                        {
                            if (nextCandel3.EndPrice > DetectedCandels.EndPrice)
                            {
                                CorrectPredictedCandels.Add(DetectedCandels);
                            }
                            else if (nextCandel3.EndPrice < DetectedCandels.EndPrice)
                            {
                                WrongPredictedCandels.Add(DetectedCandels);
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
                Profit = (correctPredictionsSum + wrongPredictionsSum + MissleniousSum) - sumEndPrice,
                Accuracy = accuracy,
                Investment = sumEndPrice,
                InvestmentNow = correctPredictionsSum + wrongPredictionsSum + MissleniousSum,
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
