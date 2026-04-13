using StockLogger.Models.Candel;

namespace StockLogger.BackgroundServices.Helper_methods
{
    public class CandleAnalysisMethods
    {
        public Candel GetTargetCandel(List<Candel> TestDayCandelList, decimal? predictedPrice)
        {
            if (predictedPrice == null)
            {
                return null;
            }

            // 1) Get the candle where Predicted RSI was achieved
            Candel targetCandel = TestDayCandelList
                .Where(c => c.HighestPrice >= predictedPrice)
                .OrderBy(c => c.OpenTime)
                .FirstOrDefault();

            return targetCandel;
        }

        public List<Candel> GetCandlesAfterTarget(List<Candel> TestDayCandelList, Candel targetCandle)
        {
            // 2) Get all candles after the target candle
            List<Candel> ListFurtherOfTargetCandel  = TestDayCandelList
                .Where(c => c.OpenTime > targetCandle.OpenTime)
                .OrderBy(c => c.OpenTime)
                .ToList();

            return ListFurtherOfTargetCandel;
        }

        public Candel GetProfitCandle(List<Candel> ListFurtherOfTargetCandel, decimal? expectedPrice)
        {
            if (expectedPrice == null)
            {
                return null;
            }

            // 3) Get earliest candle where expected price is achieved
            Candel profitCandel = ListFurtherOfTargetCandel
                .Where(c => c.LowestPrice <= expectedPrice)
                .OrderBy(c => c.OpenTime)
                .FirstOrDefault();

            return profitCandel;
        }
    }
}
