using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.VisualBasic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Web.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Web.Razor.Parser.SyntaxTree;
using System;

namespace StockLogger.Models.Candel
{
    public class StockList2
    {
        public static List<Stock> GetStocks()
        {
            return new List<Stock>
            {
                new Stock { Ticker = "COALINDIA-EQ", Exchange = "NSE", Name = "Coal India", Id = 11, SymbolToken = "20374" },
                new Stock { Ticker = "BHARTIARTL-EQ", Exchange = "NSE", Name = "Bharti Airtel", Id = 13, SymbolToken = "10604" },
                new Stock { Ticker = "NTPC-EQ", Exchange = "NSE", Name = "NTPC Limited", Id = 19, SymbolToken = "11630" },
                new Stock { Ticker = "POWERGRID-EQ", Exchange = "NSE", Name = "Power Grid Corporation", Id = 21, SymbolToken = "14977" },
                new Stock { Ticker = "WIPRO-EQ", Exchange = "NSE", Name = "Wipro", Id = 23, SymbolToken = "3787" },
                new Stock { Ticker = "TECHM-EQ", Exchange = "NSE", Name = "Tech Mahindra", Id = 24, SymbolToken = "13538" },
                new Stock { Ticker = "ONGC-EQ", Exchange = "NSE", Name = "Oil and Natural Gas Corporation", Id = 26, SymbolToken = "2475" },
                new Stock { Ticker = "TATAMOTORS-EQ", Exchange = "NSE", Name = "Tata Motors", Id = 42, SymbolToken = "3456" },
                new Stock { Ticker = "ITC-EQ", Exchange = "NSE", Name = "ITC Limited", Id = 7, SymbolToken = "1660" },



                new Stock { Ticker = "INFY-EQ", Exchange = "NSE", Name = "Infosys", Id = 1, SymbolToken = "1594" },
                new Stock { Ticker = "RELIANCE-EQ", Exchange = "NSE", Name = "Reliance Industries", Id = 2, SymbolToken = "2885" },
                new Stock { Ticker = "TCS-EQ", Exchange = "NSE", Name = "Tata Consultancy Services", Id = 3, SymbolToken = "11536" },
                new Stock { Ticker = "HDFCBANK-EQ", Exchange = "NSE", Name = "HDFC Bank", Id = 4, SymbolToken = "1333" },
                new Stock { Ticker = "ICICIBANK-EQ", Exchange = "NSE", Name = "ICICI Bank", Id = 5, SymbolToken = "4963" },
                new Stock { Ticker = "HINDUNILVR-EQ", Exchange = "NSE", Name = "Hindustan Unilever", Id = 6, SymbolToken = "1394" },
                new Stock { Ticker = "KOTAKBANK-EQ", Exchange = "NSE", Name = "Kotak Mahindra Bank", Id = 8, SymbolToken = "1922" },
                new Stock { Ticker = "LT-EQ", Exchange = "NSE", Name = "Larsen & Toubro", Id = 9, SymbolToken = "11483" },
                new Stock { Ticker = "SBIN-EQ", Exchange = "NSE", Name = "State Bank of India", Id = 10, SymbolToken = "3045" },
                new Stock { Ticker = "TITAN-EQ", Exchange = "NSE", Name = "Titan Company", Id = 12, SymbolToken = "3506" },



                new Stock { Ticker = "HCLTECH-EQ", Exchange = "NSE", Name = "HCL Technologies", Id = 14, SymbolToken = "7229" },
                new Stock { Ticker = "ASIANPAINT-EQ", Exchange = "NSE", Name = "Asian Paints", Id = 15, SymbolToken = "236" },
                new Stock { Ticker = "DMART-EQ", Exchange = "NSE", Name = "Avenue Supermarts", Id = 16, SymbolToken = "19913" },
                new Stock { Ticker = "SUNPHARMA-EQ", Exchange = "NSE", Name = "Sun Pharmaceutical Industries", Id = 18, SymbolToken = "3351" },
                new Stock { Ticker = "BAJFINANCE-EQ", Exchange = "NSE", Name = "Bajaj Finance", Id = 20, SymbolToken = "317" },
                new Stock { Ticker = "ULTRACEMCO-EQ", Exchange = "NSE", Name = "UltraTech Cement", Id = 22, SymbolToken = "11532" },
                new Stock { Ticker = "BAJAJFINSV-EQ", Exchange = "NSE", Name = "Bajaj Finserv", Id = 25, SymbolToken = "16675" },
                new Stock { Ticker = "HDFCLIFE-EQ", Exchange = "NSE", Name = "HDFC Life Insurance", Id = 27, SymbolToken = "467" },
                new Stock { Ticker = "SBILIFE-EQ", Exchange = "NSE", Name = "SBI Life Insurance", Id = 28, SymbolToken = "21808" },
                new Stock { Ticker = "M%26M-EQ", Exchange = "NSE", Name = "Mahindra & Mahindra", Id = 29, SymbolToken = "2031" },
                new Stock { Ticker = "DIVISLAB-EQ", Exchange = "NSE", Name = "Divi's Laboratories", Id = 30, SymbolToken = "10940" },
                new Stock { Ticker = "JSWSTEEL-EQ", Exchange = "NSE", Name = "JSW Steel", Id = 31, SymbolToken = "11723" },
                new Stock { Ticker = "ADANIENT-EQ", Exchange = "NSE", Name = "Adani Enterprises", Id = 32, SymbolToken = "25" },
                new Stock { Ticker = "INDUSINDBK-EQ", Exchange = "NSE", Name = "IndusInd Bank", Id = 34, SymbolToken = "5258" },
                new Stock { Ticker = "CIPLA-EQ", Exchange = "NSE", Name = "Cipla", Id = 35, SymbolToken = "694" },
                new Stock { Ticker = "DRREDDY-EQ", Exchange = "NSE", Name = "Dr. Reddy's Laboratories", Id = 36, SymbolToken = "881" },
                new Stock { Ticker = "ADANIPORTS-EQ", Exchange = "NSE", Name = "Adani Ports and SEZ", Id = 37, SymbolToken = "15083" },
                new Stock { Ticker = "GRASIM-EQ", Exchange = "NSE", Name = "Grasim Industries", Id = 38, SymbolToken = "1232" },
                new Stock { Ticker = "HEROMOTOCO-EQ", Exchange = "NSE", Name = "Hero MotoCorp", Id = 39, SymbolToken = "1348" },
                new Stock { Ticker = "EICHERMOT-EQ", Exchange = "NSE", Name = "Eicher Motors", Id = 40, SymbolToken = "910" },
                new Stock { Ticker = "AXISBANK-EQ", Exchange = "NSE", Name = "Axis Bank", Id = 41, SymbolToken = "5900" },
                new Stock { Ticker = "APOLLOHOSP-EQ", Exchange = "NSE", Name = "Apollo Hospitals", Id = 44, SymbolToken = "157" },
                new Stock { Ticker = "BRITANNIA-EQ", Exchange = "NSE", Name = "Britannia Industries", Id = 45, SymbolToken = "547" },
                new Stock { Ticker = "PIDILITIND-EQ", Exchange = "NSE", Name = "Pidilite Industries", Id = 47, SymbolToken = "2664" },
                new Stock { Ticker = "VEDL-EQ", Exchange = "NSE", Name = "Vedanta", Id = 48, SymbolToken = "3063" },
                new Stock { Ticker = "BAJAJ-AUTO-EQ", Exchange = "NSE", Name = "Bajaj Auto", Id = 49, SymbolToken = "16669" },
                new Stock { Ticker = "NESTLEIND-EQ", Exchange = "NSE", Name = "Nestlé India", Id = 50, SymbolToken = "17963" },


                new Stock { Ticker = "ADANIGAS-EQ", Exchange = "NSE", Name = "ADANI GAS", Id = 51, SymbolToken = "6066" },
                new Stock { Ticker = "ADANIGREEN-EQ", Exchange = "NSE", Name = "ADANI GREEN", Id = 52, SymbolToken = "3563" },
                new Stock { Ticker = "ADANIPOWER-EQ", Exchange = "NSE", Name = "ADANI POWER", Id = 53, SymbolToken = "17388" },
                new Stock { Ticker = "ADANITRANS-EQ", Exchange = "NSE", Name = "ADANI TRANS", Id = 54, SymbolToken = "10217" },



                //new Stock { Ticker = "DLF-EQ", Exchange = "NSE", Name = "DLF", Id = 55, SymbolToken = "14732" },
                //new Stock { Ticker = "PNB-EQ", Exchange = "NSE", Name = "PUNJAB NATIONAL BANK", Id = 56, SymbolToken = "10666" },
                //new Stock { Ticker = "IOC-EQ", Exchange = "NSE", Name = "INDIAN OIL CORPORATION LTD", Id = 57, SymbolToken = "1624" },
                //new Stock { Ticker = "HAL-EQ", Exchange = "NSE", Name = "HINDUSTAN AERONAUTICS LTD", Id = 58, SymbolToken = "2303" },



                //new Stock { Ticker = "MARUTI-EQ", Exchange = "NSE", Name = "Maruti Suzuki India", Id = 17, SymbolToken = "10999" },
                //new Stock { Ticker = "BPCL-EQ", Exchange = "NSE", Name = "Bharat Petroleum Corporation", Id = 33, SymbolToken = "526" },
                //new Stock { Ticker = "SHREECEM-EQ", Exchange = "NSE", Name = "Shree Cement", Id = 43, SymbolToken = "3103" },
                //new Stock { Ticker = "UPL-EQ", Exchange = "NSE", Name = "UPL Limited", Id = 46, SymbolToken = "11287" },



                //new Stock { Ticker = "ABB-EQ", Exchange = "NSE", Name = "ABB India Ltd.", Id = 59, SymbolToken = "13" },
                //new Stock { Ticker = "AMBUJACEM-EQ", Exchange = "NSE", Name = "Ambuja Cements Ltd.", Id = 60, SymbolToken = "1270" },
                //new Stock { Ticker = "ZYDUSWELL-EQ", Exchange = "NSE", Name = "Zydus Lifesciences Ltd.", Id = 61, SymbolToken = "17635" },
                //new Stock { Ticker = "BEL-EQ", Exchange = "NSE", Name = "Bharat Electronics Ltd.", Id = 62, SymbolToken = "383" },
                //new Stock { Ticker = "BIOCON-EQ", Exchange = "NSE", Name = "Biocon Ltd.", Id = 63, SymbolToken = "11373" },
                //new Stock { Ticker = "BOSCHLTD-EQ", Exchange = "NSE", Name = "Bosch Ltd.", Id = 64, SymbolToken = "2181" },
                //new Stock { Ticker = "COLPAL-EQ", Exchange = "NSE", Name = "Colgate-Palmolive (India) Ltd.", Id = 65, SymbolToken = "15141" },
                //new Stock { Ticker = "CONCOR-EQ", Exchange = "NSE", Name = "Container Corporation of India Ltd.", Id = 66, SymbolToken = "4749" },
                //new Stock { Ticker = "GAIL-EQ", Exchange = "NSE", Name = "GAIL (India) Ltd. ", Id = 67, SymbolToken = "4717" },
                //new Stock { Ticker = "HDFC-EQ", Exchange = "NSE", Name = "HDFC Ltd.", Id = 68, SymbolToken = "1330" },
                //new Stock { Ticker = "HINDALCO-EQ", Exchange = "NSE", Name = "Hindalco Industries Ltd.", Id = 69, SymbolToken = "1363" },
                //new Stock { Ticker = "HINDPETRO-EQ", Exchange = "NSE", Name = "Hindustan Petroleum Corporation Ltd.", Id = 70, SymbolToken = "1406" },
                //new Stock { Ticker = "ICICIGI-EQ", Exchange = "NSE", Name = "ICICI Lombard General Insurance Co.", Id = 71, SymbolToken = "21770" },
                //new Stock { Ticker = "ICICIPRULI-EQ", Exchange = "NSE", Name = "ICICI Prudential Life Insurance Co.", Id = 72, SymbolToken = "18652" },
                //new Stock { Ticker = "INDHOTEL-EQ", Exchange = "NSE", Name = "Indian Hotels Company Ltd.", Id = 73, SymbolToken = "1512" },
                //new Stock { Ticker = "NAUKRI-EQ", Exchange = "NSE", Name = "NAUKRI", Id = 74, SymbolToken = "13751" },
                //new Stock { Ticker = "INDIGO-EQ", Exchange = "NSE", Name = "InterGlobe Aviation Ltd.", Id = 75, SymbolToken = "11195" },
                //new Stock { Ticker = "JINDALSTEL-EQ", Exchange = "NSE", Name = "Jindal Steel & Power Ltd.", Id = 76, SymbolToken = "6733" },
                //new Stock { Ticker = "LICHSGFIN-EQ", Exchange = "NSE", Name = "LIC Housing Finance Ltd.", Id = 77, SymbolToken = "1997" },
                //new Stock { Ticker = "LUPIN-EQ", Exchange = "NSE", Name = "Lupin Ltd.", Id = 78, SymbolToken = "10440" },
                //new Stock { Ticker = "MARICO-EQ", Exchange = "NSE", Name = "Marico Ltd.", Id = 79, SymbolToken = "4067" },
                //new Stock { Ticker = "MOTHERSUMI-EQ", Exchange = "NSE", Name = "Motherson Sumi Wiring India Ltd.", Id = 80, SymbolToken = "4204" },



                //new Stock { Ticker = "MRF-EQ", Exchange = "NSE", Name = "MRF Ltd.", Id = 81, SymbolToken = "2277" },
                //new Stock { Ticker = "PAGEIND-EQ", Exchange = "NSE", Name = "Page Industries Ltd.", Id = 82, SymbolToken = "14413" }, // 45000 R stock
                //new Stock { Ticker = "SBICARD-EQ", Exchange = "NSE", Name = "SBI Cards and Payment Services Ltd.", Id = 83, SymbolToken = "17971" },
                //new Stock { Ticker = "SIEMENS-EQ", Exchange = "NSE", Name = "Siemens Ltd.", Id = 84, SymbolToken = "3150" },
                //new Stock { Ticker = "SRF-EQ", Exchange = "NSE", Name = "SRF Ltd.", Id = 85, SymbolToken = "3273" },
                //new Stock { Ticker = "SAIL-EQ", Exchange = "NSE", Name = "Steel Authority of India Ltd.", Id = 86, SymbolToken = "2963" },
                //new Stock { Ticker = "TATACONSUM-EQ", Exchange = "NSE", Name = "Tata Consumer Products Ltd.", Id = 87, SymbolToken = "3432" },
                //new Stock { Ticker = "TATAPOWER-EQ", Exchange = "NSE", Name = "Tata Power Company Ltd.", Id = 88, SymbolToken = "3426" },
                //new Stock { Ticker = "TATASTEEL-EQ", Exchange = "NSE", Name = "Tata Steel Ltd.", Id = 89, SymbolToken = "3499" },
                //new Stock { Ticker = "TORNTPHARM-EQ", Exchange = "NSE", Name = "Torrent Pharmaceuticals Ltd.", Id = 90, SymbolToken = "3518" },
                //new Stock { Ticker = "UBL-EQ", Exchange = "NSE", Name = "United Breweries Ltd.", Id = 91, SymbolToken = "16713" },
                //new Stock { Ticker = "MCDOWELL-N-EQ", Exchange = "NSE", Name = "United Spirits Ltd.", Id = 92, SymbolToken = "10447" },



                //new Stock { Ticker = "APLAPOLLO-EQ", Exchange = "NSE", Name = "APLAPOLLO", Id = 93, SymbolToken = "25780" },
                //new Stock { Ticker = "AUBANK-EQ", Exchange = "NSE", Name = "AUBANK-EQ", Id = 94, SymbolToken = "21238" },
                //new Stock { Ticker = "ABCAPITAL-EQ", Exchange = "NSE", Name = "ABCAPITAL-EQ", Id = 95, SymbolToken = "21614" },
                //new Stock { Ticker = "ABFRL-EQ", Exchange = "NSE", Name = "ABFRL-EQ", Id = 96, SymbolToken = "30108" },
                //new Stock { Ticker = "ALKEM-EQ", Exchange = "NSE", Name = "ALKEM-EQ", Id = 97, SymbolToken = "11703" },
                //new Stock { Ticker = "APOLLOTYRE-EQ", Exchange = "NSE", Name = "APOLLOTYRE-EQ", Id = 98, SymbolToken = "163" },
                //new Stock { Ticker = "ASHOKLEY-EQ", Exchange = "NSE", Name = "ASHOKLEY-EQ", Id = 99, SymbolToken = "212" },
                //new Stock { Ticker = "ASTRAL-EQ", Exchange = "NSE", Name = "ASTRAL-EQ", Id = 100, SymbolToken = "14418" },


                //new Stock { Ticker = "AUROPHARMA-EQ", Exchange = "NSE", Name = "AUROPHARMA-EQ", Id = 101, SymbolToken = "275" },
                //new Stock { Ticker = "BSE-EQ", Exchange = "BSE", Name = "19585 BSE-EQ", Id = 102, SymbolToken = "19585" },
                //new Stock { Ticker = "BAJAJHLDNG-EQ", Exchange = "NSE", Name = "BAJAJHLDNG-EQ", Id = 103, SymbolToken = "305" },
                //new Stock { Ticker = "BALKRISIND-EQ", Exchange = "NSE", Name = "BALKRISIND-EQ", Id = 104, SymbolToken = "335" },
                //new Stock { Ticker = "BANDHANBNK-EQ", Exchange = "NSE", Name = "BANDHANBNK-EQ", Id = 105, SymbolToken = "2263" },
                //new Stock { Ticker = "BANKBARODA-EQ", Exchange = "NSE", Name = "BANKBARODA-EQ", Id = 106, SymbolToken = "4668" },
                //new Stock { Ticker = "BANKINDIA-EQ", Exchange = "NSE", Name = "BANKINDIA-EQ", Id = 107, SymbolToken = "4745" },
                //new Stock { Ticker = "MAHABANK-EQ", Exchange = "NSE", Name = "MAHABANK-EQ", Id = 108, SymbolToken = "11377" },
                //new Stock { Ticker = "BDL-EQ", Exchange = "NSE", Name = "BDL-EQ", Id = 109, SymbolToken = "2144" },
                //new Stock { Ticker = "BHARATFORG-EQ", Exchange = "NSE", Name = "BHARATFORG-EQ", Id = 110, SymbolToken = "422" },
                //new Stock { Ticker = "BHEL-EQ", Exchange = "NSE", Name = "BHEL-EQ", Id = 111, SymbolToken = "438" },
                //new Stock { Ticker = "CGPOWER-EQ", Exchange = "NSE", Name = "CGPOWER-EQ", Id = 112, SymbolToken = "760" },
                //new Stock { Ticker = "CANBK-EQ", Exchange = "NSE", Name = "CANBK-EQ", Id = 113, SymbolToken = "10794" },
                //new Stock { Ticker = "CHOLAFIN-EQ", Exchange = "NSE", Name = "CHOLAFIN-EQ", Id = 114, SymbolToken = "685" },
                //new Stock { Ticker = "COCHINSHIP-EQ", Exchange = "NSE", Name = "COCHINSHIP-EQ", Id = 115, SymbolToken = "21508" },
                //new Stock { Ticker = "COFORGE-EQ", Exchange = "NSE", Name = "COFORGE-EQ", Id = 116, SymbolToken = "11543" },
                //new Stock { Ticker = "CUMMINSIND-EQ", Exchange = "NSE", Name = "CUMMINSIND-EQ", Id = 117, SymbolToken = "1901" },
                //new Stock { Ticker = "DABUR-EQ", Exchange = "NSE", Name = "DABUR-EQ", Id = 118, SymbolToken = "772" },
                //new Stock { Ticker = "DIXON-EQ", Exchange = "NSE", Name = "DIXON-EQ", Id = 119, SymbolToken = "21690" },
                //new Stock { Ticker = "ESCORTS-EQ", Exchange = "NSE", Name = "ESCORTS-EQ", Id = 120, SymbolToken = "958" },
                //new Stock { Ticker = "EXIDEIND-EQ", Exchange = "NSE", Name = "EXIDEIND-EQ", Id = 121, SymbolToken = "676" },
                //new Stock { Ticker = "FEDERALBNK-EQ", Exchange = "NSE", Name = "FEDERALBNK-EQ", Id = 122, SymbolToken = "1023" },
                //new Stock { Ticker = "FACT-EQ", Exchange = "NSE", Name = "FACT-EQ", Id = 123, SymbolToken = "1008" },
                //new Stock { Ticker = "GODREJCP-EQ", Exchange = "NSE", Name = "GODREJCP-EQ", Id = 124, SymbolToken = "10099" },
                //new Stock { Ticker = "GODREJPROP-EQ", Exchange = "NSE", Name = "GODREJPROP-EQ", Id = 125, SymbolToken = "17875" },
                //new Stock { Ticker = "HDFCAMC-EQ", Exchange = "NSE", Name = "HDFCAMC-EQ", Id = 126, SymbolToken = "4244" },
                //new Stock { Ticker = "HAVELLS-EQ", Exchange = "NSE", Name = "HAVELLS-EQ", Id = 127, SymbolToken = "9819" },
                //new Stock { Ticker = "HUDCO-EQ", Exchange = "NSE", Name = "HUDCO-EQ", Id = 128, SymbolToken = "20825" },
                //new Stock { Ticker = "HINDZINC-EQ", Exchange = "NSE", Name = "HINDZINC-EQ", Id = 129, SymbolToken = "1424" },
                //new Stock { Ticker = "IDBI-EQ", Exchange = "NSE", Name = "IDBI-EQ", Id = 130, SymbolToken = "1476" },
                //new Stock { Ticker = "IDFCFIRSTB-EQ", Exchange = "NSE", Name = "IDFCFIRSTB-EQ", Id = 131, SymbolToken = "11184" },
                //new Stock { Ticker = "IRB-EQ", Exchange = "NSE", Name = " IRB-EQ", Id = 132, SymbolToken = "15313" },
                //new Stock { Ticker = "INDIANB-EQ", Exchange = "NSE", Name = "INDIANB-EQ", Id = 133, SymbolToken = "14309" },
                //new Stock { Ticker = "IOB-EQ", Exchange = "NSE", Name = "IOB-EQ", Id = 134, SymbolToken = "9348" },
                //new Stock { Ticker = "IRCTC-EQ", Exchange = "NSE", Name = "IRCTC-EQ", Id = 135, SymbolToken = "13611" },
                //new Stock { Ticker = "IGL-EQ", Exchange = "NSE", Name = "IGL-EQ", Id = 136, SymbolToken = "11262" },
                //new Stock { Ticker = "JSWENERGY-EQ", Exchange = "NSE", Name = "JSWENERGY-EQ", Id = 137, SymbolToken = "17869" },
                //new Stock { Ticker = "JUBLFOOD-EQ", Exchange = "NSE", Name = "JUBLFOOD-EQ", Id = 138, SymbolToken = "18096" },
                //new Stock { Ticker = "KPITTECH-EQ", Exchange = "NSE", Name = "KPITTECH-EQ", Id = 139, SymbolToken = "9683" },
                //new Stock { Ticker = "LTI-EQ", Exchange = "NSE", Name = "LTI-EQ", Id = 140, SymbolToken = "17818" },
                //new Stock { Ticker = "M%26MFIN-EQ", Exchange = "NSE", Name = "M%26MFIN-EQ", Id = 141, SymbolToken = "13285" },
                //new Stock { Ticker = "MRPL-EQ", Exchange = "NSE", Name = "MRPL-EQ", Id = 142, SymbolToken = "2283" },
                //new Stock { Ticker = "MFSL-EQ", Exchange = "NSE", Name = "MFSL-EQ", Id = 143, SymbolToken = "2142" },
                //new Stock { Ticker = "MAXHEALTH-EQ", Exchange = "NSE", Name = "MAXHEALTH-EQ", Id = 144, SymbolToken = "22377" },
                //new Stock { Ticker = "MAZDOCK-EQ", Exchange = "NSE", Name = "MAZDOCK-EQ", Id = 145, SymbolToken = "509" },
                //new Stock { Ticker = "MPHASIS-EQ", Exchange = "NSE", Name = "MPHASIS-EQ", Id = 146, SymbolToken = "4503" },
                //new Stock { Ticker = "MUTHOOTFIN-EQ", Exchange = "NSE", Name = "MUTHOOTFIN-EQ", Id = 147, SymbolToken = "23650" },
                //new Stock { Ticker = "NHPC-EQ", Exchange = "NSE", Name = "NHPC-EQ", Id = 148, SymbolToken = "17400" },
                //new Stock { Ticker = "NLCINDIA-EQ", Exchange = "NSE", Name = "NLCINDIA-EQ", Id = 149, SymbolToken = "8585" },
                //new Stock { Ticker = "NMDC-EQ", Exchange = "NSE", Name = "NMDC-EQ", Id = 150, SymbolToken = "15332" },
                //new Stock { Ticker = "OBEROIRLTY-EQ", Exchange = "NSE", Name = "OBEROIRLTY-EQ", Id = 151, SymbolToken = "20242" },




                //new Stock { Ticker = "OIL-EQ", Exchange = "NSE", Name = "OIL-EQ", Id = 152, SymbolToken = "17438" },
                //new Stock { Ticker = "OFSS-EQ", Exchange = "NSE", Name = "OFSS-EQ", Id = 153, SymbolToken = "10738" },
                //new Stock { Ticker = "PIIND-EQ", Exchange = "NSE", Name = "PIIND-EQ", Id = 154, SymbolToken = "24184" },
                //new Stock { Ticker = "PERSISTENT-EQ", Exchange = "NSE", Name = "PERSISTENT-EQ", Id = 155, SymbolToken = "18365" },
                //new Stock { Ticker = "PETRONET-EQ", Exchange = "NSE", Name = "PETRONET-EQ", Id = 156, SymbolToken = "11351" },
                //new Stock { Ticker = "PHOENIXLTD-EQ", Exchange = "NSE", Name = "PHOENIXLTD-EQ", Id = 157, SymbolToken = "14552" },
                //new Stock { Ticker = "POLYCAB-EQ", Exchange = "NSE", Name = "POLYCAB-EQ", Id = 158, SymbolToken = "9590" },
                //new Stock { Ticker = "PFC-EQ", Exchange = "NSE", Name = "PFC-EQ", Id = 159, SymbolToken = "14299" },
                //new Stock { Ticker = "PRESTIGE-EQ", Exchange = "NSE", Name = "PRESTIGE-EQ", Id = 160, SymbolToken = "20302" },
                //new Stock { Ticker = "RECLTD-EQ", Exchange = "NSE", Name = "RECLTD-EQ", Id = 161, SymbolToken = "15355" },
                //new Stock { Ticker = "RVNL-EQ", Exchange = "NSE", Name = "RVNL-EQ", Id = 162, SymbolToken = "9552" },


                ////new Stock { Ticker = "", Exchange = "NSE", Name = "", Id = 163, SymbolToken = "" },
                ////new Stock { Ticker = "", Exchange = "NSE", Name = "", Id = 164, SymbolToken = "" },





//SJVN Ltd.   Power   SJVN    EQ  INE002L01015
//SRF Ltd.    Chemicals   SRF EQ  INE647A01010
//Samvardhana Motherson International Ltd.    Automobile and Auto Components  MOTHERSON   EQ  INE775A01035
//Shree Cement Ltd.   Construction Materials  SHREECEM    EQ  INE070A01015
//Shriram Finance Ltd.    Financial Services  SHRIRAMFIN  EQ  INE721A01047
//Siemens Ltd.    Capital Goods   SIEMENS EQ  INE003A01024
//Solar Industries India Ltd. Chemicals   SOLARINDS   EQ  INE343H01029
//Sona BLW Precision Forgings Ltd.    Automobile and Auto Components  SONACOMS    EQ  INE073K01018
//State Bank of India Financial Services  SBIN    EQ  INE062A01020
//Steel Authority of India Ltd.   Metals & Mining SAIL    EQ  INE114A01011
//Sun Pharmaceutical Industries Ltd.  Healthcare  SUNPHARMA   EQ  INE044A01036
//Sundaram Finance Ltd.   Financial Services  SUNDARMFIN  EQ  INE660A01013
//Supreme Industries Ltd. Capital Goods   SUPREMEIND  EQ  INE195A01028
//Suzlon Energy Ltd.  Capital Goods   SUZLON  EQ  INE040H01021
//TVS Motor Company Ltd.  Automobile and Auto Components  TVSMOTOR    EQ  INE494B01023
//Tata Chemicals Ltd. Chemicals   TATACHEM    EQ  INE092A01019
//Tata Communications Ltd.    Telecommunication   TATACOMM    EQ  INE151A01013
//Tata Consultancy Services Ltd.  Information Technology  TCS EQ  INE467B01029
//Tata Consumer Products Ltd. Fast Moving Consumer Goods  TATACONSUM  EQ  INE192A01025
//Tata Elxsi Ltd. Information Technology  TATAELXSI   EQ  INE670A01012
//Tata Motors Ltd.    Automobile and Auto Components  TATAMOTORS  EQ  INE155A01022
//Tata Power Co. Ltd. Power   TATAPOWER   EQ  INE245A01021
//Tata Steel Ltd. Metals & Mining TATASTEEL   EQ  INE081A01020
//Tata Technologies Ltd.  Information Technology  TATATECH    EQ  INE142M01025
//Tech Mahindra Ltd.  Information Technology  TECHM   EQ  INE669C01036
//Titan Company Ltd.  Consumer Durables   TITAN   EQ  INE280A01028
//Torrent Pharmaceuticals Ltd.    Healthcare  TORNTPHARM  EQ  INE685A01028
//Torrent Power Ltd.  Power   TORNTPOWER  EQ  INE813H01021
//Trent Ltd.  Consumer Services   TRENT   EQ  INE849A01020
//Tube Investments of India Ltd.  Automobile and Auto Components  TIINDIA EQ  INE974X01010
//UPL Ltd.    Chemicals   UPL EQ  INE628A01036
//UltraTech Cement Ltd.   Construction Materials  ULTRACEMCO  EQ  INE481G01011
//Union Bank of India Financial Services  UNIONBANK   EQ  INE692A01016
//United Spirits Ltd. Fast Moving Consumer Goods  UNITDSPR    EQ  INE854D01024
//Varun Beverages Ltd.    Fast Moving Consumer Goods  VBL EQ  INE200M01039
//Vedanta Ltd.    Metals & Mining VEDL    EQ  INE205A01025
//Vodafone Idea Ltd.  Telecommunication   IDEA    EQ  INE669E01016
//Voltas Ltd. Consumer Durables   VOLTAS  EQ  INE226A01021
//Yes Bank Ltd.   Financial Services  YESBANK EQ  INE528G01035
//Zomato Ltd. Consumer Services   ZOMATO  EQ  INE758T01015
//Zydus Lifesciences Ltd. Healthcare  ZYDUSLIFE   EQ  INE010B01027













            };
        }
    }
}
