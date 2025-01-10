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
                ("TITAN-EQ", "NSE", "Titan Company", 12 , "3506"),

                ("BHARTIARTL-EQ", "NSE", "Bharti Airtel", 13 , "10604"),
                ("HCLTECH-EQ", "NSE", "HCL Technologies", 14, "7229"),
                ("ASIANPAINT-EQ", "NSE", "Asian Paints", 15, "236"),
                ("DMART-EQ", "NSE", "Avenue Supermarts", 16, "19913"),
                ("MARUTI-EQ", "NSE", "Maruti Suzuki India", 17 , "10999"),
                ("SUNPHARMA-EQ", "NSE", "Sun Pharmaceutical Industries", 18 , "3351"),
                ("NTPC-EQ", "NSE", "NTPC Limited", 19 , "11630"),
                ("BAJFINANCE-EQ", "NSE", "Bajaj Finance", 20 , "317"),
                ("POWERGRID-EQ", "NSE", "Power Grid Corporation", 21 , "14977"),
                ("ULTRACEMCO-EQ", "NSE", "UltraTech Cement", 22 , "11532"),
                ("WIPRO-EQ", "NSE", "Wipro", 23 , "3787"),
                ("TECHM-EQ", "NSE", "Tech Mahindra", 24 , "13538"),
                ("BAJAJFINSV-EQ", "NSE", "Bajaj Finserv", 25 , "16675"),
                ("ONGC-EQ", "NSE", "Oil and Natural Gas Corporation", 26 , "2475"),
                ("HDFCLIFE-EQ", "NSE", "HDFC Life Insurance", 27 , "467"),
                ("SBILIFE-EQ", "NSE", "SBI Life Insurance", 28 , "21808"),

                ("M%26M-EQ", "NSE", "Mahindra & Mahindra", 29 , "2031"),
                ("DIVISLAB-EQ", "NSE", "Divi's Laboratories", 30 , "10940"),
                ("JSWSTEEL-EQ", "NSE", "JSW Steel", 31 , "11723"),
                ("ADANIENT-EQ", "NSE", "Adani Enterprises", 32 , "25"),
                ("BPCL-EQ", "NSE", "Bharat Petroleum Corporation", 33 , "526"),
                ("INDUSINDBK-EQ", "NSE", "IndusInd Bank", 34 , "5258"),
                ("CIPLA-EQ", "NSE", "Cipla", 35 , "694"),
                ("DRREDDY-EQ", "NSE", "Dr. Reddy's Laboratories", 36 , "881"),
                ("ADANIPORTS-EQ", "NSE", "Adani Ports and SEZ", 37 , "15083"),
                ("GRASIM-EQ", "NSE", "Grasim Industries", 38 , "1232"),
                ("HEROMOTOCO-EQ", "NSE", "Hero MotoCorp", 39 , "1348"),

                ("EICHERMOT-EQ", "NSE", "Eicher Motors", 40 , "910"),
                ("AXISBANK-EQ", "NSE", "Axis Bank", 41 ,"5900"),
                ("TATAMOTORS-EQ", "NSE", "Tata Motors", 42 , "3456"),
                ("SHREECEM-EQ", "NSE", "Shree Cement", 43 , "3103"),
                ("APOLLOHOSP-EQ", "NSE", "Apollo Hospitals", 44 , "157"),
                ("BRITANNIA-EQ", "NSE", "Britannia Industries", 45 , "547"),
                ("UPL-EQ", "NSE", "UPL Limited", 46 , "11287"),
                ("PIDILITIND-EQ", "NSE", "Pidilite Industries", 47 , "2664"),
                ("VEDL-EQ", "NSE", "Vedanta", 48 , "3063"),
                ("BAJAJ-AUTO-EQ", "NSE", "Bajaj Auto", 49 , "16669"),
                ("NESTLEIND-EQ", "NSE", "Nestlé India", 50, "17963")

            };
        }
    }
}
