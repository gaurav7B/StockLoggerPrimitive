namespace StockLogger.Models.Candel
{
    public class StockList
    {
        public static List<(string ticker, string exchange, string name, long id, string symboltoken)> GetStocks()
        {
            return new List<(string, string, string, long, string)>
            {
                ("INFY-EQ", "NSE", "Infosys", 1, "1594"),
                ("RELIANCE-EQ", "NSE", "Reliance Industries", 2, "2885"),
                ("TCS-EQ", "NSE", "Tata Consultancy Services", 3, "11536"),
                ("HDFCBANK-EQ", "NSE", "HDFC Bank", 4, "1333"),
                ("ICICIBANK-EQ", "NSE", "ICICI Bank", 5, "4963"),
                ("HINDUNILVR-EQ", "NSE", "Hindustan Unilever", 6, "1394"),
                ("ITC-EQ", "NSE", "ITC Limited", 7, "1660"),
                ("KOTAKBANK-EQ", "NSE", "Kotak Mahindra Bank", 8, "1922"),
                ("LT-EQ", "NSE", "Larsen & Toubro", 9, "11483"),
                ("SBIN-EQ", "NSE", "State Bank of India", 10, "3045"),
                ("COALINDIA-EQ", "NSE", "Coal India", 11, "20374"),

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
                //("POWERGRID", "NSE", "Power Grid Corporation", 21),
                //("ULTRACEMCO", "NSE", "UltraTech Cement", 22),
                //("WIPRO", "NSE", "Wipro", 23),
                //("TECHM", "NSE", "Tech Mahindra", 24),
                //("BAJAJFINSV", "NSE", "Bajaj Finserv", 25),
                //("ONGC", "NSE", "Oil and Natural Gas Corporation", 26),
                //("HDFCLIFE", "NSE", "HDFC Life Insurance", 27),
                //("SBILIFE", "NSE", "SBI Life Insurance", 28),
                //("M%26M", "NSE", "Mahindra & Mahindra", 29),
                //("DIVISLAB", "NSE", "Divi's Laboratories", 30),
                //("JSWSTEEL", "NSE", "JSW Steel", 31),
                //("ADANIENT", "NSE", "Adani Enterprises", 32),
                //("BPCL", "NSE", "Bharat Petroleum Corporation", 33),
                //("INDUSINDBK", "NSE", "IndusInd Bank", 34),
                //("CIPLA", "NSE", "Cipla", 35),
                //("DRREDDY", "NSE", "Dr. Reddy's Laboratories", 36),
                //("ADANIPORTS", "NSE", "Adani Ports and SEZ", 37),
                //("GRASIM", "NSE", "Grasim Industries", 38),
                //("HEROMOTOCO", "NSE", "Hero MotoCorp", 39),
                //("EICHERMOT", "NSE", "Eicher Motors", 40),
                //("COALINDIA", "NSE", "Coal India", 41),
                //("TATAMOTORS", "NSE", "Tata Motors", 42),
                //("SHREECEM", "NSE", "Shree Cement", 43),
                //("APOLLOHOSP", "NSE", "Apollo Hospitals", 44),
                //("BRITANNIA", "NSE", "Britannia Industries", 45),
                //("UPL", "NSE", "UPL Limited", 46),
                //("PIDILITIND", "NSE", "Pidilite Industries", 47),
                //("VEDL", "NSE", "Vedanta", 48),
                //("BAJAJAUTO", "NSE", "Bajaj Auto", 49)
            };
        }
    }
}
