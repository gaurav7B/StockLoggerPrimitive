namespace StockLogger.Models.Candel
{
    public class StockList
    {
        public static List<(string ticker, string exchange, string name, long id, string symboltoken)> GetStocks()
        {
            return new List<(string, string, string, long, string)>
    {
        ("INFY", "NSE", "Infosys", 1, "26000"),
        //("RELIANCE", "NSE", "Reliance Industries", 2, "99926001"),
        //("TCS", "NSE", "Tata Consultancy Services", 3, "99926002"),
        //("HDFCBANK", "NSE", "HDFC Bank", 4, "99926003"),
        //("ICICIBANK", "NSE", "ICICI Bank", 5, "99926004"),
        //("HINDUNILVR", "NSE", "Hindustan Unilever", 6, "99926005"),
        //("ITC", "NSE", "ITC Limited", 7, "99926006"),
        //("KOTAKBANK", "NSE", "Kotak Mahindra Bank", 8, "99926007"),
        //("LT", "NSE", "Larsen & Toubro", 9, "99926008"),
        //("SBIN", "NSE", "State Bank of India", 10, "99926009")
                //("AXISBANK", "NSE", "Axis Bank", 11),
                //("BAJFINANCE", "NSE", "Bajaj Finance", 12),
                //("BHARTIARTL", "NSE", "Bharti Airtel", 13),
                //("HCLTECH", "NSE", "HCL Technologies", 14),
                //("ASIANPAINT", "NSE", "Asian Paints", 15),
                //("DMART", "NSE", "Avenue Supermarts", 16),
                //("MARUTI", "NSE", "Maruti Suzuki India", 17),
                //("SUNPHARMA", "NSE", "Sun Pharmaceutical Industries", 18),
                //("NTPC", "NSE", "NTPC Limited", 19),
                //("TITAN", "NSE", "Titan Company", 20),
            };
        }
    }
}
