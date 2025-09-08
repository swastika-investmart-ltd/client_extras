using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System;
using Components;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Dapper;
using System.Data;
using System.IO;
using ClosedXML.Excel;

namespace Client.WebApi.Services
{
    public interface IReportsService
    {
        Task<ResponseNotificationTLBaseModel<AnnualPnlSummaryExpResMdl, AnnualPnlSummaryResMdl>> GetAnnualPnlSummary(AnnualPnlSummaryReqMdl obj);
        Task<ResponseNotificationBaseModelList<GlobalSummaryResMdl, GlobalSummaryResMdl>> GetGlobalSummary(GlobalSummaryReqMdl obj);
        Task<ResponseBaseModel<TradeSummaryResMdl>> GetTradeSummaryReport(TradeSummaryReqMdl obj);
        Task<ResponseBaseModel<TradeSummaryDataResMdl>> GetTradeSummaryWebReport(TradeSummaryReqMdl obj);
        Task<ResponseBaseModel<HoldingTradeSummaryResMdl>> GetHoldingTradeSummaryReport(HoldingTradeSummaryReqMdl obj);
        Task<ResponseBaseModel> GetDownLoadAnnualReport(DownLoadReportReqMdl obj, string filePath);
        Task<ResponseBaseMTFRepModel<MTFInterestReportResMdl>> GetMTFInterestReport(MTFInterestReportReqMdl obj);
    }
    public class ReportsService : BaseService, IReportsService
    {
        // HashSet to store invalid response values
        private readonly HashSet<string> invalidResponses = new HashSet<string> { "No Such Client ID found..", "No Data Found" };
        private readonly IConfiguration _config;
        private readonly IHttpClientPostService _httpClientPostService;
        private readonly ILog _logger;
        public ReportsService(IConfiguration config, IHttpClientPostService httpClientPostService, ILog logger)
        {
            _config = config;
            _httpClientPostService = httpClientPostService;
            _logger = logger;
        }
        // Private method to check if a response is valid
        private bool IsValidResponse(string response)
        {
            return !string.IsNullOrEmpty(response) && !invalidResponses.Contains(response);
        }

        public async Task<ResponseNotificationTLBaseModel<AnnualPnlSummaryExpResMdl, AnnualPnlSummaryResMdl>> GetAnnualPnlSummary(AnnualPnlSummaryReqMdl obj)
        {
            var result = new ResponseNotificationTLBaseModel<AnnualPnlSummaryExpResMdl, AnnualPnlSummaryResMdl>()
            {
                Data = new AnnualPnlSummaryExpResMdl(),
                Datas = new List<AnnualPnlSummaryResMdl>()
            };

            List<AnnualPnlSummaryResponseMdl> AnPnlSummaryList = new List<AnnualPnlSummaryResponseMdl>();
            //For EXPENSES
            List<AnnualPnlSummaryResponseMdl> AnPnExpensesList = new List<AnnualPnlSummaryResponseMdl>();

            string baseURL = _config["TechexcelApi:BaseUrl"].ToString();
            string addressSuffix = "ANNUAL_PNL_SUMMARY/ANNUAL_PNL_SUMMARY1?";

            string ToDate = "31/03/" + obj.FinYear;
            var UrlDataYear = obj.FinYear - 1;

            string UrlDatabase = string.Empty;
            string ToCompDate = "03/31/" + obj.FinYear;
            DateTime dt = Convert.ToDateTime(ToCompDate);
            //if (obj.FinYear == DateTime.Now.Year) 
            if (dt >= DateTime.Now.Date)

                UrlDatabase = _config["TechexcelApi:TechexcelDB"].ToString(); //"CAPSFO"
            else
                UrlDatabase = _config["TechexcelApi:TechexcelOldDB"].ToString(); //"CAPSFO2122"

            //switch (obj.FinYear)
            //{
            //    case long Y when Y == DateTime.Now.Year:
            //        UrlDatabase = "CAPSFO";
            //        break;
            //    case long Y when Y == DateTime.Now.Year - 1:
            //        UrlDatabase = "CAPSFO" + (Y-1).ToString().Substring(2, 2) + Y.ToString().Substring(2, 2);
            //        break;
            //    default:
            //        UrlDatabase = "CAPSFO";
            //        break;
            //}

            //Prepare string to API Call          
            var ReqString = new StringBuilder()
                    .Append("&UrlUserName=").Append(_config["TechexcelApi:APIUserName"].ToString())
                    .Append("&UrlPassword=").Append(_config["TechexcelApi:APIPassword"].ToString())
                    .Append("&UrlDatabase=").Append(UrlDatabase)
                    .Append("&UrlDataYear=").Append(UrlDataYear.ToString())
                    .Append("&Client_code=").Append(obj.Uid)
                    .Append("&ToDate=").Append(ToDate)
                    .Append("&WithExp=").Append("Y")
                    .ToString();


            addressSuffix = addressSuffix + ReqString;
            //using (GenericRestHttpClient<string, string> memberClient = new GenericRestHttpClient<string, string>(baseURL, addressSuffix))
            //{
            //    string strResponse = AsyncContext.Run(() => memberClient.GetStringAsync());

            string strResponse = await _httpClientPostService.WebRequestPostAsync(baseURL, addressSuffix, ReqString);
            strResponse = strResponse.Trim();

            if (IsValidResponse(strResponse))
            {
                ////Bad JSON escape sequence: \T. Path                   
                strResponse = strResponse.Replace(@"\", @"/").Replace(@"\\", @"/");
                var RetDataList = JsonConvert.DeserializeObject<List<InternalTEResponse>>(strResponse);
                if (RetDataList != null && RetDataList.Any())
                {

                    //When Data NULL OR NOT FOUND
                    //[{"COLUMNS":["CLIENT_ID","PL_AMT","BUY_QTY","BUY_RATE","BUY_AMT","SALE_QTY","SALE_AMT","SALE_RATE","LONG_TERM","SHORT_TERM","SPECULATION","TR_TYPE","CURR_AMOUNT","CLIENT_NAME","SCRIP_SYMBOL1","SCRIP_NAME","NET_QTY","NET_RATE","NET_AMOUNT","CLOSING_PRICE"],"DATA":[]},""]

                    //WHEN DATA
                    //[{"COLUMNS":["CLIENT_ID","PL_AMT","BUY_QTY","BUY_RATE","BUY_AMT","SALE_QTY","SALE_AMT","SALE_RATE","LONG_TERM","SHORT_TERM","SPECULATION","TR_TYPE","CURR_AMOUNT","CLIENT_NAME","SCRIP_SYMBOL1","SCRIP_NAME","NET_QTY","NET_RATE","NET_AMOUNT","CLOSING_PRICE"],"DATA":[["DTS1317",911,20,610.25,12205,0,0,0,0,0,0,"OP_ASSETS",-12205,"RASHMI TIWARI","511196","CAN FIN HOMES LTD.",20,610.25,-12205,655.80]]},""]
                    var dataList = RetDataList.FirstOrDefault().DATA;
                    if (dataList != null && dataList.Any())
                    {
                        foreach (List<string> list in RetDataList.FirstOrDefault().DATA)
                        {
                            // when SCRIP_SYMBOL1 and SCRIP_NAME is null or blank
                            list[14] = string.IsNullOrEmpty(list[14].Trim()) ? "-" : list[14].Trim();
                            list[15] = string.IsNullOrEmpty(list[15].Trim()) ? "-" : list[14].Trim();

                            var objList = new AnnualPnlSummaryResponseMdl
                            {
                                //Uid = list[0].Trim(), //CLIENT_ID
                                PlAmt = string.IsNullOrEmpty(list[1].Trim()) ? 0 : Convert.ToDecimal(list[1].Trim()), //PL_AMT
                                BuyQty = string.IsNullOrEmpty(list[2].Trim()) ? 0 : Convert.ToInt32(list[2].Trim()), //BUY_QTY
                                BuyRate = string.IsNullOrEmpty(list[3].Trim()) ? 0 : Convert.ToDecimal(list[3].Trim()), //BUY_RATE
                                BuyAmt = string.IsNullOrEmpty(list[4].Trim()) ? 0 : Convert.ToDecimal(list[4].Trim()), //BUY_AMT
                                SellQty = string.IsNullOrEmpty(list[5].Trim()) ? 0 : Convert.ToInt32(list[5].Trim()), //SALE_QTY
                                SellAmt = string.IsNullOrEmpty(list[6].Trim()) ? 0 : Convert.ToDecimal(list[6].Trim()), //SALE_AMT
                                SellRate = string.IsNullOrEmpty(list[7].Trim()) ? 0 : Convert.ToDecimal(list[7].Trim()), //SALE_RATE
                                                                                                                         //LongTerm = list[8].Trim(), //LONG_TERM
                                                                                                                         //ShortTerm = list[9].Trim(), //SHORT_TERM
                                                                                                                         //Speculation = list[10].Trim(), //SPECULATION
                                TrType = (list[11].Trim() == "OP_SHORTTERM") ? "SHORTTERM" : (list[11].Trim() == "OP_ASSETS") ? "ASSETS" : list[11].Trim(), //TR_TYPE
                                                                                                                                                            //CurrAmount = list[12].Trim(), //CURR_AMOUNT
                                                                                                                                                            //ClientName = list[13].Trim(), //CLIENT_NAME
                                ScripSymbol = (list[14].Trim().All(char.IsDigit) == false) ? list[14].Trim() : await GetScripSymbolFromBSESecurityInfo(list[14].Trim()), //SCRIP_SYMBOL1
                                ScripName = (list[11].Trim() == "EXPENSES") ? list[14].Trim() : list[15].Trim(), //SCRIP_NAME
                                NetQty = string.IsNullOrEmpty(list[16].Trim()) ? 0 : Convert.ToInt32(list[16].Trim()), //NET_QTY
                                                                                                                       //NetRate = list[17].Trim(), //NET_RATE
                                                                                                                       //NetAmount = list[18].Trim(), //NET_AMOUNT
                                ClosingPrice = list[19].Trim() //CLOSING_PRICE
                            };

                            if (objList.TrType == "EXPENSES")
                                AnPnExpensesList.Add(objList);
                            else
                                AnPnlSummaryList.Add(objList);
                        }
                    }
                }
            }
            // }


            //Create New Summary List 
            List<AnnualPnlSummaryResMdl> AnnualPnlSummaryList = new List<AnnualPnlSummaryResMdl>();
            if (AnPnlSummaryList != null && AnPnlSummaryList.Any())
            {
                //var groupedAnPnlSummaryList = AnPnlSummaryList
                //                            .GroupBy(u => u.ScripName)
                //                            .Select(grp => grp.ToList())
                //                            .ToList();

                //List<AnnualPnlSummaryResMdl> APSummaryList = AnPnlSummaryList.GroupBy(u => u.ScripName)
                //                                  .Select(grp => new AnnualPnlSummaryResMdl
                //                                  {
                //                                      ScripName = grp.Key,
                //                                      TotalRows = grp.ToList().Count,
                //                                      SummaryList = grp.ToList()
                //                                  }).ToList();

                List<AnnualPnlSummaryResMdl> APSummaryList = AnPnlSummaryList.GroupBy(x => new { x.ScripName, x.ScripSymbol })
                                                  .Select(grp => new AnnualPnlSummaryResMdl
                                                  {
                                                      ScripName = grp.Key.ScripName,
                                                      ScripSymbol = grp.Key.ScripSymbol,
                                                      TotalRows = grp.ToList().Count,
                                                      SummaryList = grp.ToList().ConvertAll(x => new SummaryList
                                                      {
                                                          PlAmt = x.PlAmt,
                                                          BuyQty = x.BuyQty,
                                                          BuyRate = x.BuyRate,
                                                          BuyAmt = x.BuyAmt,
                                                          SellQty = x.SellQty,
                                                          SellAmt = x.SellAmt,
                                                          SellRate = x.SellRate,
                                                          TrType = x.TrType,
                                                          NetQty = x.NetQty,
                                                          ClosingPrice = x.ClosingPrice
                                                      })
                                                  }).ToList();

                foreach (var apsRow in APSummaryList)
                {
                    if (apsRow.TotalRows > 1)
                    {
                        //Check SHORTTERM Count is > 1
                        int shcount = apsRow.SummaryList.Where(x => x.TrType == "SHORTTERM").ToList().Count;
                        //Check ASSETS Count is > 1
                        int ascount = apsRow.SummaryList.Where(y => y.TrType == "ASSETS").ToList().Count;
                        if (shcount > 1 || ascount > 1)
                        {
                            //List<AnnualPnlSummaryResMdl> AggrSummaryList = new List<AnnualPnlSummaryResMdl>();
                            List<SummaryList> AggrSummaryList = new List<SummaryList>();
                            if (shcount > 1 && ascount > 1)
                            {
                                //For SHORTTERM
                                foreach (var item in apsRow.SummaryList)
                                {
                                    if ((item.TrType == "SHORTTERM") && (AggrSummaryList.Exists(x => x.TrType == "SHORTTERM")))
                                    {
                                        AggrSummaryList.Where(u => u.TrType == "SHORTTERM").ToList().ForEach(x =>
                                        {
                                            x.PlAmt = (x.PlAmt + item.PlAmt);
                                            x.BuyQty = (x.BuyQty + item.BuyQty);
                                            x.BuyAmt = (x.BuyAmt + item.BuyAmt);
                                            x.BuyRate = (x.BuyQty > 0) ? Math.Round((decimal)(x.BuyAmt / x.BuyQty), 2) : 0;
                                            x.SellQty = (x.SellQty + item.SellQty);
                                            x.SellAmt = (x.SellAmt + item.SellAmt);
                                            x.SellRate = (x.SellQty > 0) ? Math.Round((decimal)(x.SellAmt / x.SellQty), 2) : 0;
                                            x.TrType = item.TrType;
                                            //x.ScripSymbol = item.ScripSymbol;
                                            //x.ScripName = item.ScripName;
                                            x.NetQty = (x.NetQty + item.NetQty);
                                            x.ClosingPrice = item.ClosingPrice;
                                        });
                                    }
                                    else
                                    {
                                        //First Time - Add item
                                        AggrSummaryList.Add(item);
                                    }
                                }

                                //For ASSETS                               
                                foreach (var item in apsRow.SummaryList)
                                {
                                    if ((item.TrType == "ASSETS") && (AggrSummaryList.Exists(x => x.TrType == "ASSETS")))
                                    {
                                        //Already Added - Update Record
                                        AggrSummaryList.Where(u => u.TrType == "ASSETS").ToList().ForEach(x =>
                                        {
                                            x.PlAmt = (x.PlAmt + item.PlAmt);
                                            x.BuyQty = (x.BuyQty + item.BuyQty);
                                            x.BuyAmt = (x.BuyAmt + item.BuyAmt);
                                            x.BuyRate = (x.BuyQty > 0) ? Math.Round((decimal)(x.BuyAmt / x.BuyQty), 2) : 0;
                                            x.SellQty = (x.SellQty + item.SellQty);
                                            x.SellAmt = (x.SellAmt + item.SellAmt);
                                            x.SellRate = (x.SellQty > 0) ? Math.Round((decimal)(x.SellAmt / x.SellQty), 2) : 0;
                                            x.TrType = item.TrType;
                                            //x.ScripSymbol = item.ScripSymbol;
                                            //x.ScripName = item.ScripName;
                                            x.NetQty = (x.NetQty + item.NetQty);
                                            x.ClosingPrice = item.ClosingPrice;
                                        });
                                    }
                                    else
                                    {
                                        AggrSummaryList.Add(item);
                                    }
                                }

                            }
                            else if (shcount > 1)
                            {
                                //For SHORTTERM
                                foreach (var item in apsRow.SummaryList)
                                {
                                    if ((item.TrType == "SHORTTERM") && (AggrSummaryList.Exists(x => x.TrType == "SHORTTERM")))
                                    {
                                        AggrSummaryList.Where(u => u.TrType == "SHORTTERM").ToList().ForEach(x =>
                                        {
                                            x.PlAmt = (x.PlAmt + item.PlAmt);
                                            x.BuyQty = (x.BuyQty + item.BuyQty);
                                            x.BuyAmt = (x.BuyAmt + item.BuyAmt);
                                            x.BuyRate = (x.BuyQty > 0) ? Math.Round((decimal)(x.BuyAmt / x.BuyQty), 2) : 0;
                                            x.SellQty = (x.SellQty + item.SellQty);
                                            x.SellAmt = (x.SellAmt + item.SellAmt);
                                            x.SellRate = (x.SellQty > 0) ? Math.Round((decimal)(x.SellAmt / x.SellQty), 2) : 0;
                                            x.TrType = item.TrType;
                                            //x.ScripSymbol = item.ScripSymbol;
                                            //x.ScripName = item.ScripName;
                                            x.NetQty = (x.NetQty + item.NetQty);
                                            x.ClosingPrice = item.ClosingPrice;
                                        });
                                    }
                                    else
                                    {
                                        //First Time - Add item
                                        AggrSummaryList.Add(item);
                                    }
                                }
                            }
                            else if (ascount > 1)
                            {
                                //For ASSETS                               
                                foreach (var item in apsRow.SummaryList)
                                {
                                    if ((item.TrType == "ASSETS") && (AggrSummaryList.Exists(x => x.TrType == "ASSETS")))
                                    {
                                        //Already Added - Update Record
                                        AggrSummaryList.Where(u => u.TrType == "ASSETS").ToList().ForEach(x =>
                                        {
                                            x.PlAmt = (x.PlAmt + item.PlAmt);
                                            x.BuyQty = (x.BuyQty + item.BuyQty);
                                            x.BuyAmt = (x.BuyAmt + item.BuyAmt);
                                            x.BuyRate = (x.BuyQty > 0) ? Math.Round((decimal)(x.BuyAmt / x.BuyQty), 2) : 0;
                                            x.SellQty = (x.SellQty + item.SellQty);
                                            x.SellAmt = (x.SellAmt + item.SellAmt);
                                            x.SellRate = (x.SellQty > 0) ? Math.Round((decimal)(x.SellAmt / x.SellQty), 2) : 0;
                                            x.TrType = item.TrType;
                                            //x.ScripSymbol = item.ScripSymbol;
                                            //x.ScripName = item.ScripName;
                                            x.NetQty = (x.NetQty + item.NetQty);
                                            x.ClosingPrice = item.ClosingPrice;
                                        });
                                    }
                                    else
                                    {
                                        AggrSummaryList.Add(item);
                                    }
                                }
                            }

                            //Add in to List
                            AnnualPnlSummaryList.Add(new AnnualPnlSummaryResMdl
                            {
                                ScripName = apsRow.ScripName,
                                ScripSymbol = apsRow.ScripSymbol,
                                TotalRows = AggrSummaryList.Count,
                                SummaryList = AggrSummaryList.ConvertAll(x => new SummaryList
                                {
                                    PlAmt = x.PlAmt,
                                    BuyQty = x.BuyQty,
                                    BuyRate = x.BuyRate,
                                    BuyAmt = x.BuyAmt,
                                    SellQty = x.SellQty,
                                    SellAmt = x.SellAmt,
                                    SellRate = x.SellRate,
                                    TrType = x.TrType,
                                    NetQty = x.NetQty,
                                    ClosingPrice = x.ClosingPrice
                                })
                            });
                        }
                        else
                        {
                            //when Only One row of SHORTTERM or ASSETS
                            AnnualPnlSummaryList.Add(apsRow);
                        }
                    }
                    else
                    {
                        AnnualPnlSummaryList.Add(apsRow);
                    }
                }
            }


            // For DATA
            if (AnnualPnlSummaryList != null && AnnualPnlSummaryList.Any())
                result.Datas = AnnualPnlSummaryList;

            //For EXPENSES 
            if (AnPnExpensesList != null && AnPnExpensesList.Any())
            {
                result.Data = new AnnualPnlSummaryExpResMdl
                {
                    ScripName = "EXPENSES",
                    ScripSymbol = "EXPENSES",
                    TotalRows = AnPnExpensesList.Count,
                    SummaryList = AnPnExpensesList.ConvertAll(x => new SummaryExpensesList
                    {
                        PlAmt = x.PlAmt,
                        BuyQty = x.BuyQty,
                        BuyRate = x.BuyRate,
                        BuyAmt = x.BuyAmt,
                        SellQty = x.SellQty,
                        SellAmt = x.SellAmt,
                        SellRate = x.SellRate,
                        NetQty = x.NetQty,
                        ClosingPrice = x.ClosingPrice,
                        ScripName = x.ScripName
                    })
                };
            }

            return result;
        }

        public async Task<ResponseNotificationBaseModelList<GlobalSummaryResMdl, GlobalSummaryResMdl>> GetGlobalSummary(GlobalSummaryReqMdl obj)
        {
            List<GlobalSummaryResMdl> GlobalSummaryList = new List<GlobalSummaryResMdl>();
            //For EXPENSES
            List<GlobalSummaryResMdl> GlobalExpensesList = new List<GlobalSummaryResMdl>();

            string baseURL = _config["TechexcelApi:BaseUrl"].ToString();
            string addressSuffix = "Global/Global?";

            string ToDate = "31/03/" + obj.FinYear;
            var UrlDataYear = obj.FinYear - 1;

            string UrlDatabase = string.Empty;
            string ToCompDate = "03/31/" + obj.FinYear;
            DateTime dt = Convert.ToDateTime(ToCompDate);
            //if (obj.FinYear == DateTime.Now.Year) 
            if (dt >= DateTime.Now.Date)

                UrlDatabase = _config["TechexcelApi:TechexcelDB"].ToString(); // "CAPSFO";
            else
                UrlDatabase = _config["TechexcelApi:TechexcelOldDB"].ToString(); // "CAPSFO2122";

            //switch (obj.FinYear)
            //{
            //    case long Y when Y == DateTime.Now.Year:
            //        UrlDatabase = "CAPSFO";
            //        break;
            //    case long Y when Y == DateTime.Now.Year - 1:
            //        UrlDatabase = "CAPSFO" + (Y - 1).ToString().Substring(2, 2) + Y.ToString().Substring(2, 2);
            //        break;
            //    default:
            //        UrlDatabase = "CAPSFO";
            //        break;
            //}

            //Prepare string to API Call          
            var ReqString = new StringBuilder()
                    .Append("&UrlUserName=").Append(_config["TechexcelApi:APIUserName"].ToString())
                    .Append("&UrlPassword=").Append(_config["TechexcelApi:APIPassword"].ToString())
                    .Append("&UrlDatabase=").Append(UrlDatabase)
                    .Append("&UrlDataYear=").Append(UrlDataYear.ToString())
                    .Append("&COCD=").Append(obj.Segments)
                    .Append("&Client_Code=").Append(obj.Uid)
                    .Append("&Finstyr=").Append(UrlDataYear.ToString())
                    .Append("&To_date=").Append(ToDate)
                    .Append("&WITHOpening=").Append("Y")
                    .ToString();

            addressSuffix = addressSuffix + ReqString;


            string strResponse = await _httpClientPostService.WebRequestPostAsync(baseURL, addressSuffix, ReqString);
            strResponse = strResponse.Trim();

            //using (GenericRestHttpClient<string, string> memberClient = new GenericRestHttpClient<string, string>(baseURL, addressSuffix))
            //{
            //string strResponse = AsyncContext.Run(() => memberClient.GetStringAsync());

            //if (!string.IsNullOrEmpty(strResponse) && strResponse.Trim() != "No Data Found")
            if (IsValidResponse(strResponse))
            {
                ////Bad JSON escape sequence: \T. Path                   
                strResponse = strResponse.Replace(@"\", @"/").Replace(@"\\", @"/");

                var RetDataList = JsonConvert.DeserializeObject<List<InternalTEResponse>>(strResponse);
                if (RetDataList != null && RetDataList.Any())
                {

                    //When Data NULL OR NOT FOUND
                    //[{"COLUMNS":["START_YEAR","TRADING_AMOUNT","BUY_QUANTITY","BUY_RATE","BUY_AMOUNT","SALE_QUANTITY","SALE_RATE","SALE_AMOUNT","NET_QUANTITY","NET_RATE","NET_AMOUNT","TODATE","SR","CLOSING_PRICE","NOT_PROFIT","COMPANY_LEGER_BALANCE","CLIENT_ID_FAX","CLIENT_ID_MAIL","BRANCH_CODE_FAX","BRANCH_CODE_MAIL","BRANCH_CODE","BRANCH_NAME","MARKET","FAMILY_GROUP","FAMILY_GROUP_NAME","FAMILY_GROUP_MAIL","FAMILY_GROUP_FAX","COMPCODE","SCRIP_NAME","EXPIRY_DATE1","EXCHANGE","COMPANY_CODE","CLIENT_ID","CLIENT_NAME","SCRIP_SYMBOL","TRADING_QUANTITY"],"DATA":[]},""]

                    //WHEN DATA
                    //[{"COLUMNS":["START_YEAR","TRADING_AMOUNT","BUY_QUANTITY","BUY_RATE","BUY_AMOUNT","SALE_QUANTITY","SALE_RATE","SALE_AMOUNT","NET_QUANTITY","NET_RATE","NET_AMOUNT","TODATE","SR","CLOSING_PRICE","NOT_PROFIT","COMPANY_LEGER_BALANCE","CLIENT_ID_FAX","CLIENT_ID_MAIL","BRANCH_CODE_FAX","BRANCH_CODE_MAIL","BRANCH_CODE","BRANCH_NAME","MARKET","FAMILY_GROUP","FAMILY_GROUP_NAME","FAMILY_GROUP_MAIL","FAMILY_GROUP_FAX","COMPCODE","SCRIP_NAME","EXPIRY_DATE1","EXCHANGE","COMPANY_CODE","CLIENT_ID","CLIENT_NAME","SCRIP_SYMBOL","TRADING_QUANTITY"],"DATA":[[2022,0,5,3153.20,15766,0,0,0,5,3153.15,-15766,"25/11/2022",1,3090.8500,-312,0,"","rashmidwivedi25@gmail.com","","DWIVEDISAURABH11@GMAIL.COM","DTSDI","SAURABH DWIVEDI","CAPS","","","","","NSE_CASH","ABB INDIA LIMITED","November, 28 2022 16:28:25 +0530","","CAPITAL","DTS1317","RASHMI TIWARI","500002 ABB INDIA LIMITED",0]]},""]
                    var dataList = RetDataList.FirstOrDefault().DATA;
                    if (dataList != null && dataList.Any())
                    {
                        foreach (List<string> list in RetDataList.FirstOrDefault().DATA)
                        {
                            var objList = new GlobalSummaryResMdl
                            {
                                //StartYear = list[0].Trim(), //START_YEAR
                                //TradingAmt = list[1].Trim(), //TRADING_AMOUNT
                                BuyQty = list[2].Trim(), //BUY_QUANTITY
                                BuyRate = list[3].Trim(), //BUY_RATE
                                BuyAmt = list[4].Trim(), //BUY_AMOUNT
                                SellQty = list[5].Trim(), //SALE_QUANTITY
                                SellRate = list[6].Trim(), //SALE_RATE
                                SellAmt = list[7].Trim(), //SALE_AMOUNT
                                NetQty = list[8].Trim(), //NET_QUANTITY
                                                         //NetRate = list[9].Trim(), //NET_RATE
                                                         //NetAmt = list[10].Trim(), //NET_AMOUNT
                                                         //ToDate = list[11].Trim(), //TODATE
                                Sr = list[12].Trim(), //SR
                                ClosingPrice = list[13].Trim(), //CLOSING_PRICE
                                                                //NotProfit = list[14].Trim(), //NOT_PROFIT                                  
                                                                //CompanyLegerBalance = list[15].Trim(), //COMPANY_LEGER_BALANCE
                                                                //ClientIdFax = list[16].Trim(), //CLIENT_ID_FAX
                                                                //ClientIdMail = list[17].Trim(), //CLIENT_ID_MAIL
                                                                //BranchCodeFax = list[18].Trim(), //BRANCH_CODE_FAX                                   
                                                                //BranchCodeMail = list[19].Trim(),//BRANCH_CODE_MAIL,
                                                                //BranchCode = list[20].Trim(), //BRANCH_CODE,
                                                                //BranchName = list[21].Trim(), //BRANCH_NAME,
                                                                //Market = list[22].Trim(), //MARKET,
                                                                //FamilyGroup = list[23].Trim(), //FAMILY_GROUP,
                                                                //FamilyGroupName = list[24].Trim(), //FAMILY_GROUP_NAME,
                                                                //FamilyGroupMail = list[25].Trim(), //FAMILY_GROUP_MAIL,
                                                                //FamilyGroupFax = list[26].Trim(), //FAMILY_GROUP_FAX,
                                CompCode = list[27].Trim(), //COMPCODE,
                                ScripName = list[28].Trim(), //SCRIP_NAME,
                                                             //ExpireDate = list[29].Trim(), //EXPIRY_DATE1,
                                                             //Exchange = list[30].Trim(), //EXCHANGE,
                                CompanyCode = list[31].Trim(), //COMPANY_CODE,
                                                               //ClientId = list[32].Trim(), //CLIENT_ID,
                                                               //ClientName = list[33].Trim(), //CLIENT_NAME,
                                ScripSymbol = list[34].Trim(), //SCRIP_SYMBOL,
                                                               //TradingQty = list[35].Trim(), //TRADING_QUANTITY
                                TradingQty = list[35].Trim(), //TRADING_QUANTITY,
                                TradingAmt = list[1].Trim(), //OPENING TRADING_RATE,                              
                            };

                            if (objList.CompanyCode == "EXPENSES")
                                GlobalExpensesList.Add(objList);
                            else
                            {
                                if ((objList.CompCode == "CD_NSE") || (objList.CompCode == "CD_BSE"))
                                {
                                    //For BuyRate
                                    if (!string.IsNullOrEmpty(objList.BuyAmt) && !string.IsNullOrEmpty(objList.BuyQty))
                                    {
                                        long BuyQty = Convert.ToInt64(objList.BuyQty);
                                        decimal BuyAmt = Convert.ToDecimal(objList.BuyAmt);
                                        if (BuyQty > 0 && BuyAmt > 0)
                                        {
                                            decimal BuyRate = BuyAmt / (BuyQty * 1000);
                                            objList.BuyRate = Convert.ToString(Math.Round(BuyRate, 4));
                                        }
                                    }

                                    //For SellRate
                                    if (!string.IsNullOrEmpty(objList.SellAmt) && !string.IsNullOrEmpty(objList.SellQty))
                                    {
                                        long SellQty = Convert.ToInt64(objList.SellQty);
                                        decimal SellAmt = Convert.ToDecimal(objList.SellAmt);
                                        if (SellQty > 0 && SellAmt > 0)
                                        {
                                            decimal SellRate = SellAmt / (SellQty * 1000);
                                            objList.SellRate = Convert.ToString(Math.Round(SellRate, 4));
                                        }
                                    }
                                }
                                GlobalSummaryList.Add(objList);
                            }

                        }
                    }
                }
            }
            //}

            //Return Response
            if (GlobalSummaryList != null && GlobalSummaryList.Any())
            {
                ////List<GlobalSummaryResMdl> GSummaryList = GlobalSummaryList.GroupBy(u => u.ScripName)
                ////                                  .Select(grp => new GlobalSummaryResMdl
                ////                                  {
                ////                                      ScripName = string.IsNullOrEmpty(grp.Key) ? "EXPENSES" : grp.Key,
                ////                                      TotalRows = grp.ToList().Count,
                ////                                      SummaryList = grp.ToList()
                ////                                  }).ToList();

                //return new ResponseBaseModel<GlobalSummaryResMdl>
                //{
                //    TotalRows = GSummaryList.Count,
                //    Datas = GSummaryList
                //};

                return new ResponseNotificationBaseModelList<GlobalSummaryResMdl, GlobalSummaryResMdl>()
                {
                    Datas1 = GlobalSummaryList,
                    Datas2 = GlobalExpensesList
                };
            }
            else
            {
                return new ResponseNotificationBaseModelList<GlobalSummaryResMdl, GlobalSummaryResMdl>()
                {
                    Datas1 = new List<GlobalSummaryResMdl>(),
                    Datas2 = new List<GlobalSummaryResMdl>()
                };
            }

        }

        public async Task<ResponseBaseModel<TradeSummaryResMdl>> GetTradeSummaryReport(TradeSummaryReqMdl obj)
        {
            var response = new ResponseBaseModel<TradeSummaryResMdl>()
            {
                Datas = new List<TradeSummaryResMdl>()
            };
            using (IDbConnection TRwcon = CreateTrvwConnection())
            {
                if (TRwcon.State != ConnectionState.Open)
                    TRwcon.Open();

                var param = new DynamicParameters();
                param.Add("@ClientId", obj.Uid);
                param.Add("@FromDate", obj.FromDate);
                param.Add("@ToDate", obj.ToDate);
                param.Add("@Segment", obj.Segment);
                var result = (await SqlMapper.QueryAsync<TradeSummaryDataResMdl>(TRwcon, "RP_Get_Trade_Summary_Report", param, commandType: CommandType.StoredProcedure)).ToList();

                if (result != null && result.Any())
                {
                    //// Changed on 08 Sep 2025, the order by descending after the feedback from Parth sir that latest order 
                    ///should be on top. Show date in descending order and trades (time) in ascending order for a particular date.
                    response.Datas = result.GroupBy(u => u.TradeDate)
                                                      .Select(grp => new TradeSummaryResMdl
                                                      {
                                                          TradeDate = grp.Key,
                                                          TotalRows = grp.ToList().Count,
                                                          SummaryList = grp.ToList().OrderBy(p => p.TradeDateTime).ToList()
                                                      }).ToList().OrderByDescending(x => DateTime.Parse(x.TradeDate)).ToList();

                    //response.Datas = result.GroupBy(u => u.TRADE_DATE)
                    //                                  .Select(grp => new TradeSummaryResMdl
                    //                                  {
                    //                                      TradeDate = grp.Key,
                    //                                      TotalRows = grp.ToList().Count,
                    //                                      SummaryList = grp.ToList()//.OrderBy(p => p.TRADE_DATETIME).ToList()
                    //                                      .ConvertAll(x => new TradeSummaryDataResMdl
                    //                                      {
                    //                                          CompanyCode = x.COMPANY_CODE,
                    //                                          ScripSymbol = x.SCRIP_SYMBOL,
                    //                                          ScripName = x.SCRIP_NAME,
                    //                                          BuySale = x.BUY_SALE,
                    //                                          Quantity = x.QUANTITY,
                    //                                          Price = x.PRICE_PREMIUM,
                    //                                          TradeDate = x.TRADE_DATE,
                    //                                          TradeTime = x.TRADE_TIME
                    //                                      })
                    //                                  }).ToList();
                    //                  
                    response.TotalRows = response.Datas.Count;
                }
            }
            return response;
        }

        public async Task<ResponseBaseModel<TradeSummaryDataResMdl>> GetTradeSummaryWebReport(TradeSummaryReqMdl obj)
        {
            var response = new ResponseBaseModel<TradeSummaryDataResMdl>()
            {
                Datas = new List<TradeSummaryDataResMdl>()
            };
            using (IDbConnection TRwcon = CreateTrvwConnection())
            {
                if (TRwcon.State != ConnectionState.Open)
                    TRwcon.Open();

                var param = new DynamicParameters();
                param.Add("@ClientId", obj.Uid);
                param.Add("@FromDate", obj.FromDate);
                param.Add("@ToDate", obj.ToDate);
                param.Add("@Segment", obj.Segment);
                var responseDb = (await SqlMapper.QueryAsync<TradeSummaryDataResMdl>(TRwcon, "RP_Get_Trade_Summary_Report", param, commandType: CommandType.StoredProcedure)).ToList();
                if (responseDb != null && responseDb.Any())
                    response.Datas = responseDb.OrderBy(p => p.TradeDateTime).ToList();
                response.TotalRows = response.Datas.Count;
            }
            return response;
        }

        public async Task<ResponseBaseModel<HoldingTradeSummaryResMdl>> GetHoldingTradeSummaryReport(HoldingTradeSummaryReqMdl obj)
        {
            var response = new ResponseBaseModel<HoldingTradeSummaryResMdl>()
            {
                Datas = new List<HoldingTradeSummaryResMdl>()
            };
            using (IDbConnection TRwcon = CreateTrvwConnection())
            {
                if (TRwcon.State != ConnectionState.Open)
                    TRwcon.Open();

                var param = new DynamicParameters();
                param.Add("@ClientId", obj.Uid);
                param.Add("@ScripSymbol", obj.ScripSymbol);
                param.Add("@Type", obj.Type);
                var result = (await SqlMapper.QueryAsync<HoldingTradeSummaryIntrlResMdl>(TRwcon, "RP_Get_HoldingTrade_Summary_Report", param, commandType: CommandType.StoredProcedure)).ToList();

                if (result != null && result.Any())
                {
                    response.Datas = result.GroupBy(u => u.TRADE_DATE)
                                                      .Select(grp => new HoldingTradeSummaryResMdl
                                                      {
                                                          TradeDate = grp.Key,
                                                          TotalRows = grp.ToList().Count,
                                                          SummaryList = grp.ToList().ConvertAll(x => new HoldingTradeSummaryDataResMdl
                                                          {
                                                              BuySell = x.BuySell,
                                                              RowText = x.RowText
                                                          })
                                                      }).ToList().OrderByDescending(x => DateTime.Parse(x.TradeDate)).ToList();

                    response.TotalRows = response.Datas.Count;
                }
            }
            return response;
        }

        public async Task<ResponseBaseModel> GetDownLoadAnnualReport(DownLoadReportReqMdl obj, string filePath)
        {
            AES256 aes = new AES256();

            ClientInfoPost obclapps = new ClientInfoPost() { UserId = obj.Uid, SecurityKey = obj.SecurityKey };
            //Below method called to get the Client Details
            string CLIENT_ID = obj.Uid;
            string CLIENT_NAME = obj.CName;
            string CLIENT_EMAILID = obj.CEmail;

            string PAN_NO = string.Empty;
            try
            {
                if (IsBase64String(obj.Pan))
                    PAN_NO = aes.Decrypt(obj.Pan, obj.Uid);
                else
                    PAN_NO = obj.Pan;
            }
            catch
            {
                PAN_NO = "";
            }

            //Below are the separete methods to call Pnl Summary and Global Pnl Summary
            DataSet ds1 = await PnlSummary(obj);
            DataSet ds2 = await GlobalPnlSummary(obj, "NSE_FNO");
            DataSet ds3 = await GlobalPnlSummary(obj, "MCX");
            DataSet ds4 = await GlobalPnlSummary(obj, "CD_NSE");

            List<DataSet> allList = new List<DataSet>();
            allList.Add(ds1);
            allList.Add(ds2);
            allList.Add(ds3);
            allList.Add(ds4);

            string folderNamePath = "Documents/Reports/YearlyReportDownloadTemplate";
            string webRootPath = filePath;
            string newPath = Path.Combine(webRootPath, "Documents/Reports/DownLoadedFiles/");
            string getTemplatePath = Path.Combine(webRootPath, folderNamePath) + "/WorksBook_Template.xlsx";

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            //Added Template Details and download file paths
            if (!string.IsNullOrEmpty(CLIENT_ID))
            {
                string fileName = CLIENT_ID + "_PNLReport" + ".xlsx";
                string downloadsFilePath = Path.Combine(newPath, fileName);

                if (File.Exists(downloadsFilePath))
                    File.Delete(downloadsFilePath);

                using (XLWorkbook workBook = new XLWorkbook(getTemplatePath))
                {
                    //Read the first Sheet from Excel file.
                    IXLWorksheet Page1 = workBook.Worksheet("EQUITY");
                    IXLWorksheet Page2 = workBook.Worksheet("DERIVATIVE");
                    IXLWorksheet Page3 = workBook.Worksheet("COMMODITY");
                    IXLWorksheet Page4 = workBook.Worksheet("CURRENCY");

                    IXLWorksheet sheetName = null;
                    //Open the Excel file using ClosedXML.

                    //Added code to identify the separate sheets of excel template to fill data on specified sheets
                    int z = 0;
                    int it = 0;

                    foreach (var item in allList.ToList())
                    {
                        decimal avgIntradayEQ = 0;
                        decimal avgShortTermEQ = 0;
                        decimal avgLongTermEQ = 0;
                        decimal avgUnrealizedEQ = 0;
                        decimal avgRealizedPrOptions = 0;
                        decimal avgRealizedPrFutures = 0;
                        decimal avgUnrealizedPrOptions = 0;
                        decimal avgUnrealizedPrFutures = 0;

                        if (it != 0) { z = 20; } else { z = 17; }
                        if (it == 0) sheetName = Page1;
                        else if (it == 1) sheetName = Page2;
                        else if (it == 2) sheetName = Page3;
                        else if (it == 3) sheetName = Page4;

                        workBook.Worksheet(sheetName.ToString()).Cell(3, 1).Value = "Client ID";
                        workBook.Worksheet(sheetName.ToString()).Cell(3, 2).Value = CLIENT_ID;
                        workBook.Worksheet(sheetName.ToString()).Cell(4, 1).Value = "Client Name";
                        workBook.Worksheet(sheetName.ToString()).Cell(4, 2).Value = CLIENT_NAME;
                        workBook.Worksheet(sheetName.ToString()).Cell(5, 1).Value = "PAN";
                        workBook.Worksheet(sheetName.ToString()).Cell(5, 2).Value = PAN_NO;

                        for (int a = 0; a < item.Tables.Count; a++)
                        {
                            //Added this condition for the total SUM/ProfitLoss with the specific tables
                            if (it == 0 && a == 3)
                            {
                                if (item.Tables[3] != null && item.Tables[3].Rows.Count > 0)
                                {
                                    avgIntradayEQ = item.Tables[3].AsEnumerable().Where(x => x["Realized P&L"] != DBNull.Value).Sum(x => x.Field<decimal>("Realized P&L"));
                                }
                            }
                            else if (it == 0 && a == 5)
                            {
                                if (item.Tables[5] != null && item.Tables[5].Rows.Count > 0)
                                {
                                    avgShortTermEQ = item.Tables[5].AsEnumerable().Where(x => x["Realized P&L"] != DBNull.Value).Sum(x => x.Field<decimal>("Realized P&L"));
                                }

                            }
                            else if (it == 0 && a == 7)
                            {
                                if (item.Tables[7] != null && item.Tables[7].Rows.Count > 0)
                                {
                                    avgLongTermEQ = item.Tables[7].AsEnumerable().Where(x => x["Realized P&L"] != DBNull.Value).Sum(x => x.Field<decimal>("Realized P&L"));
                                }
                            }
                            else if (it == 0 && a == 9)
                            {
                                if (item.Tables[9] != null && item.Tables[9].Rows.Count > 0)
                                {
                                    avgUnrealizedEQ = item.Tables[9].AsEnumerable().Where(x => x["unrealized P&L"] != DBNull.Value).Sum(x => x.Field<decimal>("unrealized P&L"));
                                }
                            }
                            else if (it != 0 && a == 5)
                            {
                                avgRealizedPrFutures = item.Tables[5].AsEnumerable().Where(x => x["Realized P&L"] != DBNull.Value).Sum(x => x.Field<decimal>("Realized P&L"));
                                avgUnrealizedPrFutures = item.Tables[5].AsEnumerable().Where(x => x["Unrealized P&L"] != DBNull.Value).Sum(x => x.Field<decimal>("Unrealized P&L"));
                            }
                            else if (it != 0 && a == 3)
                            {
                                if (item.Tables[3] != null && item.Tables[3].Rows.Count > 0)
                                {
                                    avgRealizedPrOptions = item.Tables[3].AsEnumerable().Where(x => x["Realized P&L"] != DBNull.Value).Sum(x => x.Field<decimal>("Realized P&L"));
                                    avgUnrealizedPrOptions = item.Tables[3].AsEnumerable().Where(x => x["Unrealized P&L"] != DBNull.Value).Sum(x => x.Field<decimal>("Unrealized P&L"));
                                }
                            }
                            int r = z;
                            int t = r + 1;
                            //this loop creates the dynamic header creation for the specific datatables
                            for (int b = 1; b < item.Tables[a].Columns.Count + 1; b++)
                            {
                                //sheetName.Cells[r, b] = item.Tables[a].Columns[b - 1].Caption;
                                workBook.Worksheet(sheetName.ToString()).Cell(r, b).Value = item.Tables[a].Columns[b - 1].Caption;
                                int ColumnsCount = item.Tables[a].Columns.Count;
                                object[] Header = new object[ColumnsCount];
                                workBook.Worksheet(sheetName.ToString()).Cell(r, b).Style.Font.Bold = true;
                                workBook.Worksheet(sheetName.ToString()).Cell(r, b).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                            }
                            //z = z + 1;
                            //This loop adds the data on that specific
                            for (int c = 0; c < item.Tables[a].Rows.Count; c++)
                            {
                                bool boldColumn = false; //Added for specific column (Total column)bold 
                                if (a == 1 && c == (item.Tables[a].Rows.Count - 1))
                                {
                                    boldColumn = true;
                                }
                                for (int j = 0; j < item.Tables[a].Columns.Count; j++)
                                {

                                    workBook.Worksheet(sheetName.ToString()).Cell(c + t, j + 1).Value = item.Tables[a].Rows[c]
                                       [j].ToString();
                                    if (boldColumn)
                                    {
                                        workBook.Worksheet(sheetName.ToString()).Cell(c + t, j + 1).Style.Font.Bold = true;
                                        workBook.Worksheet(sheetName.ToString()).Cell(c + t, j + 1).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                                    }
                                }
                            }
                            z = (item.Tables[a].Rows.Count) + (z + 2);
                        }
                        //Added below code for the total's or header calculations like Total realised/unrealised profits calculations for Equity 
                        if (it == 0)
                        {
                            workBook.Worksheet(sheetName.ToString()).Cell(7, 1).Value = "Taxpnl Statement for Equity from " + (obj.FinYear - 1) + "-04-01 to " + obj.FinYear + "-03-31";
                            workBook.Worksheet(sheetName.ToString()).Cell(9, 1).Value = "Realized Profit Breakdown";
                            workBook.Worksheet(sheetName.ToString()).Cell(11, 1).Value = "Intraday/Speculative profit";
                            workBook.Worksheet(sheetName.ToString()).Cell(11, 2).Value = Decimal.Round(Convert.ToDecimal(avgIntradayEQ), 2);
                            workBook.Worksheet(sheetName.ToString()).Cell(12, 1).Value = "Short Term profit";
                            workBook.Worksheet(sheetName.ToString()).Cell(12, 2).Value = Decimal.Round(Convert.ToDecimal(avgShortTermEQ), 2);
                            workBook.Worksheet(sheetName.ToString()).Cell(13, 1).Value = "Long Term profit";
                            workBook.Worksheet(sheetName.ToString()).Cell(13, 2).Value = Decimal.Round(Convert.ToDecimal(avgLongTermEQ), 2);
                            workBook.Worksheet(sheetName.ToString()).Cell(15, 1).Value = "Unrealized Profit";
                            workBook.Worksheet(sheetName.ToString()).Cell(15, 2).Value = Decimal.Round(Convert.ToDecimal(avgUnrealizedEQ), 2);
                        }
                        //Added below code for the total's or header calculations like Total realised/unrealised profits calculations for Derivative/Commodity/Currency 
                        if (it != 0)
                        {
                            var sName = "";
                            if (it == 1) { sName = "Derivative"; } else if (it == 2) { sName = "Commodity"; } else if (it == 3) { sName = "Currency"; }
                            workBook.Worksheet(sheetName.ToString()).Cell(7, 1).Value = "Taxpnl Statement for " + sName + " from " + (obj.FinYear - 1) + "-04-01 to " + obj.FinYear + "-03-31";
                            workBook.Worksheet(sheetName.ToString()).Cell(10, 1).Value = "Realized Profit Breakdown";
                            workBook.Worksheet(sheetName.ToString()).Cell(12, 1).Value = "Options Realized Profit";
                            workBook.Worksheet(sheetName.ToString()).Cell(12, 2).Value = Decimal.Round(Convert.ToDecimal(avgRealizedPrOptions), 2);
                            workBook.Worksheet(sheetName.ToString()).Cell(13, 1).Value = "Futures Realized Profit";
                            //sheetName.Cells[12, 2] = "-42.5";
                            workBook.Worksheet(sheetName.ToString()).Cell(13, 2).Value = Decimal.Round(Convert.ToDecimal(avgRealizedPrFutures), 2);

                            workBook.Worksheet(sheetName.ToString()).Cell(15, 1).Value = "Unrealized Profit Breakdown";
                            workBook.Worksheet(sheetName.ToString()).Cell(17, 1).Value = "Options unrealized Profit";
                            workBook.Worksheet(sheetName.ToString()).Cell(18, 1).Value = "Futures unrealized Profit";
                            workBook.Worksheet(sheetName.ToString()).Cell(17, 2).Value = Decimal.Round(Convert.ToDecimal(avgUnrealizedPrOptions), 2);
                            workBook.Worksheet(sheetName.ToString()).Cell(18, 2).Value = Decimal.Round(Convert.ToDecimal(avgUnrealizedPrFutures), 2);
                        }
                        it++;
                    }

                    workBook.SaveAs(downloadsFilePath);
                }
                //xlWorkBook.SaveAs(downloadsFilePath, Type.Missing, Type.Missing, Type.Missing,
                //Type.Missing,
                //Type.Missing,
                //Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
                //Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                //xlWorkBook.Close();
                //excelApp.Quit();
                //below code are sending email as an attched excel to the clients               
                if (!string.IsNullOrEmpty(CLIENT_EMAILID) && obj.IsEmail)
                {
                    long finyear = obj.FinYear % 100;
                    var ObjEmailHelper = new EmailHelper(_logger);
                    string EmailMessage = "Hello " + CLIENT_NAME + ",<br/><br/>We hope this mail finds you in good health and spirit. <br/><br/>You are receiving this email in response to a request initiated on " + DateTime.Now.ToString() + " For Tax & PnL Report.<br/><br/><b> Report requested by you has been attached in this mail.</b><br/><br/> Assuring services <b>True To You.</b> Always. <br/><br/> Warm regards,<br/><b>Team Swastika</b>";
                    string EmailSubject = "Tax & PnL Report FY " + (finyear - 1) + "-" + finyear;
                    ObjEmailHelper.SendEmailByAttachmentAmazonAWS(CLIENT_EMAILID, EmailSubject, EmailMessage, downloadsFilePath);
                }
                return new ResponseBaseModel()
                {
                    ResponseId = 1,
                    ResponseMessage = obj.IsEmail ? "Success" : downloadsFilePath,
                };
            }
            else
            {
                return new ResponseBaseModel()
                {
                    ResponseId = 0,
                    ResponseMessage = "No Client Found"
                };
            }
        }

        private static bool IsBase64String(string input)
        {
            try
            {
                // Attempt to convert the string to a byte array
                byte[] result = Convert.FromBase64String(input);

                // If successful, the input is a valid Base64 string
                return true;
            }
            catch (FormatException)
            {
                // If an exception is thrown, the input is not a valid Base64 string
                return false;
            }
        }

        public async Task<DataSet> PnlSummary(DownLoadReportReqMdl obj)
        {
            AnnualPnlSummaryReqMdl objaps = new AnnualPnlSummaryReqMdl()
            {
                Uid = obj.Uid,
                FinYear = obj.FinYear
            };
            var resPnlSmry = await GetAnnualPnlSummary(objaps);
            DataSet objDs = new DataSet();
            // As per the Excel Template Added Datatables
            DataTable DownLoadReportChargesDT = new DataTable();
            DownLoadReportChargesDT.Columns.Add("Account Head");
            DownLoadReportChargesDT.Columns.Add("Amount", typeof(decimal));

            DataTable DownloadIntradayDT = new DataTable();
            DownloadIntradayDT.Columns.Add("Symbol");
            DownloadIntradayDT.Columns.Add("Quantity", typeof(decimal));
            DownloadIntradayDT.Columns.Add("Buy Value", typeof(decimal));
            DownloadIntradayDT.Columns.Add("Sell Value", typeof(decimal));
            DownloadIntradayDT.Columns.Add("Realized P&L", typeof(decimal));

            DataTable DownloadShortTermDT = new DataTable();
            DownloadShortTermDT.Columns.Add("Symbol");
            DownloadShortTermDT.Columns.Add("Quantity", typeof(decimal));
            DownloadShortTermDT.Columns.Add("Buy Value", typeof(decimal));
            DownloadShortTermDT.Columns.Add("Sell Value", typeof(decimal));
            DownloadShortTermDT.Columns.Add("Realized P&L", typeof(decimal));

            DataTable DownloadLongTermDT = new DataTable();
            DownloadLongTermDT.Columns.Add("Symbol");
            DownloadLongTermDT.Columns.Add("Quantity", typeof(decimal));
            DownloadLongTermDT.Columns.Add("Buy Value", typeof(decimal));
            DownloadLongTermDT.Columns.Add("Sell Value", typeof(decimal));
            DownloadLongTermDT.Columns.Add("Realized P&L", typeof(decimal));

            DataTable DownloadAssetsDT = new DataTable();
            DownloadAssetsDT.Columns.Add("Symbol");
            DownloadAssetsDT.Columns.Add("Quantity", typeof(decimal));
            DownloadAssetsDT.Columns.Add("Buy Value", typeof(decimal));
            DownloadAssetsDT.Columns.Add("Sell Value", typeof(decimal));
            DownloadAssetsDT.Columns.Add("Unrealized P&L", typeof(decimal));
            Decimal totalHeadAmount = 0;
            if (resPnlSmry != null)
            {
                if (resPnlSmry.Data != null && resPnlSmry.Data.SummaryList != null)
                {
                    foreach (var itempnlsmry in resPnlSmry.Data.SummaryList)
                    {
                        totalHeadAmount = totalHeadAmount + (itempnlsmry.PlAmt * (-1));
                        DownLoadReportChargesDT.Rows.Add(itempnlsmry.ScripName, (itempnlsmry.PlAmt * (-1)));
                    }
                }
                if (resPnlSmry.Datas != null && resPnlSmry.Datas.Count() > 0)
                {
                    foreach (var apsRow in resPnlSmry.Datas)
                    {
                        var symbolName = apsRow.ScripName;
                        foreach (var item in apsRow.SummaryList)
                        {
                            //Added Switch cases as per the TrType that data we are adding in that specific datatable obj
                            switch (item.TrType.ToString())
                            {
                                case "TRADING":
                                    DownloadIntradayDT.Rows.Add(symbolName, decimal.Round(item.BuyQty, 2), decimal.Round(item.BuyAmt, 2), decimal.Round(item.SellAmt, 2), decimal.Round(item.PlAmt, 2));
                                    break;
                                case "SHORTTERM":
                                    DownloadShortTermDT.Rows.Add(symbolName, decimal.Round(item.BuyQty, 2), decimal.Round(item.BuyAmt, 2), decimal.Round(item.SellAmt, 2), decimal.Round(item.PlAmt, 2));
                                    break;
                                case "LONGTERM":
                                    DownloadLongTermDT.Rows.Add(symbolName, decimal.Round(item.BuyQty, 2), decimal.Round(item.BuyAmt, 2), decimal.Round(item.SellAmt, 2), decimal.Round(item.PlAmt, 2));
                                    break;
                                case "ASSETS":
                                    DownloadAssetsDT.Rows.Add(symbolName, decimal.Round(item.BuyQty, 2), decimal.Round(item.BuyAmt, 2), decimal.Round(item.SellAmt, 2), decimal.Round(item.PlAmt, 2));
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            DataTable ChargesDT = new DataTable();
            ChargesDT.Columns.Add("Charges");
            objDs.Tables.Add(ChargesDT);
            if (DownLoadReportChargesDT.Rows.Count > 0) { DownLoadReportChargesDT.Rows.Add("Total Charges", totalHeadAmount); }
            objDs.Tables.Add(DownLoadReportChargesDT);
            DataTable IntradayBLDT = new DataTable();
            IntradayBLDT.Columns.Add("Intraday");
            objDs.Tables.Add(IntradayBLDT); objDs.Tables.Add(DownloadIntradayDT);
            DataTable ShortTermBLDT = new DataTable();
            ShortTermBLDT.Columns.Add("Short Term Trades");
            objDs.Tables.Add(ShortTermBLDT); objDs.Tables.Add(DownloadShortTermDT);
            DataTable LongTermBLDT = new DataTable();
            LongTermBLDT.Columns.Add("Long Term Trades");
            objDs.Tables.Add(LongTermBLDT); objDs.Tables.Add(DownloadLongTermDT);
            DataTable AssetBLDT = new DataTable();
            AssetBLDT.Columns.Add("Unrealized Profit");
            objDs.Tables.Add(AssetBLDT); objDs.Tables.Add(DownloadAssetsDT);
            return objDs;
        }

        public async Task<DataSet> GlobalPnlSummary(DownLoadReportReqMdl obj, string segments)
        {
            GlobalSummaryReqMdl objgsmry = new GlobalSummaryReqMdl() { Uid = obj.Uid, Segments = segments, FinYear = obj.FinYear };
            DataTable DerChargesDT = new DataTable();
            DerChargesDT.Columns.Add("Account Head");
            DerChargesDT.Columns.Add("Amount", typeof(decimal));
            int totalHeadAmount = 0;

            DataTable DownloadOptionsDT = new DataTable();
            DownloadOptionsDT.Columns.Add("Symbol");
            DownloadOptionsDT.Columns.Add("Quantity", typeof(decimal));
            DownloadOptionsDT.Columns.Add("Buy Value", typeof(decimal));
            DownloadOptionsDT.Columns.Add("Sell Value", typeof(decimal));
            DownloadOptionsDT.Columns.Add("Realized P&L", typeof(decimal));
            DownloadOptionsDT.Columns.Add("Unrealized P&L", typeof(decimal));
            DataTable DownloadFuturesDT = new DataTable();
            DownloadFuturesDT.Columns.Add("Symbol");
            DownloadFuturesDT.Columns.Add("Quantity", typeof(decimal));
            DownloadFuturesDT.Columns.Add("Buy Value", typeof(decimal));
            DownloadFuturesDT.Columns.Add("Sell Value", typeof(decimal));
            DownloadFuturesDT.Columns.Add("Realized P&L", typeof(decimal));
            DownloadFuturesDT.Columns.Add("Unrealized P&L", typeof(decimal));
            ResponseBaseMXCModel objMCX = new ResponseBaseMXCModel();
            objMCX.Result = new ResponseBaseMCXResult();
            objMCX.Result.Data = new List<MCXUnderlyingInfoResponse>();
            //Added MCX Call API In MCX/Commodity Case we are calling GetMCXUnderlyingInfo() service for the calculations
            if (segments == "MCX")
            {
                if (MCXDataStore.Reference.MCXDataStoreMdl != null && MCXDataStore.Reference.MCXDataStoreMdl.Any())
                {
                    objMCX.Result.Data = MCXDataStore.Reference.MCXDataStoreMdl;
                }
                else
                {
                    objMCX = await GetMCXUnderlyingInfo();
                }
            }

            var resGlobalSmry = await GetGlobalSummary(objgsmry);
            if (resGlobalSmry != null)
            {
                if (resGlobalSmry.Datas2 != null && resGlobalSmry.Datas2.Count() > 0)
                {
                    foreach (var itempnlsmry in resGlobalSmry.Datas2.ToList())
                    {
                        //totalHeadAmount = totalHeadAmount + int.Parse(itempnlsmry.BuyAmt);
                        totalHeadAmount = totalHeadAmount + Convert.ToInt32(Convert.ToDouble(itempnlsmry.BuyAmt));
                        DerChargesDT.Rows.Add(itempnlsmry.ScripName, (itempnlsmry.BuyAmt));
                    }
                }
                if (resGlobalSmry.Datas1 != null && resGlobalSmry.Datas1.Count() > 0)
                {
                    foreach (var item in resGlobalSmry.Datas1.ToList())
                    {
                        double mcxCalculate = 1;
                        if (segments == "MCX" && objMCX.Result.Data != null)
                        {
                            var mcxobj = objMCX.Result.Data.Where(p => p.Ic == item.ScripName).FirstOrDefault();
                            double pn = Double.Parse(mcxobj.Pn);
                            double pd = Double.Parse(mcxobj.Pd);
                            double gn = Double.Parse(mcxobj.Gn);
                            double gd = Double.Parse(mcxobj.Gd);
                            double ls = Double.Parse(mcxobj.Ls);
                            mcxCalculate = ((pn / pd) * (gn / gd)) * ls;
                        }
                        double realiseQTY = 0;
                        double unrealiseQTY = 0;
                        double forCurrency = 1;
                        if (segments == "CD_NSE") { forCurrency = 1000; }
                        double realisePL = 0;
                        double sellrate = Double.Parse(item.SellRate);
                        double buyrate = Double.Parse(item.BuyRate);
                        double sellQty = Double.Parse(item.SellQty);
                        double buyQty = Double.Parse(item.BuyQty);
                        double netQty = Double.Parse(item.NetQty);
                        double closingPrice = Double.Parse(item.ClosingPrice);

                        //Realised PL Case when BuyQty and Sell Qty is not equal to zero 
                        if (buyQty != 0 && sellQty != 0)
                        {
                            if (netQty == 0 || netQty > 0)
                            {
                                realisePL = (sellrate - buyrate) * ((sellQty * mcxCalculate) * forCurrency);
                                realiseQTY = ((sellQty * mcxCalculate) * forCurrency); ;
                            }
                            else
                            {
                                realisePL = (sellrate - buyrate) * ((buyQty * mcxCalculate) * forCurrency);
                                realiseQTY = ((buyQty * mcxCalculate) * forCurrency);
                            }
                        }
                        //un-Realised PL Case when BuyQty and Sell Qty is not equal to zero 
                        double unRealisePL = 0;
                        if (netQty > 0 || netQty < 0)
                        {
                            if (netQty > 0)
                            {
                                unRealisePL = (closingPrice - buyrate) * ((netQty * mcxCalculate) * forCurrency);
                                unrealiseQTY = ((netQty * mcxCalculate) * forCurrency);
                            }
                            else
                            {
                                unRealisePL = (closingPrice - sellrate) * ((netQty * mcxCalculate) * forCurrency);
                                unrealiseQTY = ((netQty * mcxCalculate) * forCurrency);
                            }
                        }

                        //below codee are the spilting with 'P' and 'C' for the options and Futures and also added in that specific datatable 
                        string lastCharacter = item.ScripSymbol.Substring(item.ScripSymbol.Length - 1);
                        switch (lastCharacter)
                        {
                            case "P":
                                DownloadOptionsDT.Rows.Add(item.ScripSymbol, (realiseQTY + unrealiseQTY), Decimal.Round(Convert.ToDecimal(item.BuyAmt), 2), Decimal.Round(Convert.ToDecimal(item.SellAmt), 2), Decimal.Round(Convert.ToDecimal(realisePL), 2), Decimal.Round(Convert.ToDecimal(unRealisePL), 2));
                                break;
                            case "C":
                                DownloadOptionsDT.Rows.Add(item.ScripSymbol, (realiseQTY + unrealiseQTY), Decimal.Round(Convert.ToDecimal(item.BuyAmt), 2), Decimal.Round(Convert.ToDecimal(item.SellAmt), 2), Decimal.Round(Convert.ToDecimal(realisePL), 2), Decimal.Round(Convert.ToDecimal(unRealisePL), 2));
                                break;
                            default:
                                DownloadFuturesDT.Rows.Add(item.ScripSymbol, (realiseQTY + unrealiseQTY), Decimal.Round(Convert.ToDecimal(item.BuyAmt), 2), Decimal.Round(Convert.ToDecimal(item.SellAmt), 2), Decimal.Round(Convert.ToDecimal(realisePL), 2), Decimal.Round(Convert.ToDecimal(unRealisePL), 2));
                                break;
                        }
                    }
                }
            }
            //Added all the datatables into the datasets
            DataSet objGSmary = new DataSet();
            DataTable ChargesDT = new DataTable();
            ChargesDT.Columns.Add("Charges");
            objGSmary.Tables.Add(ChargesDT);
            if (DerChargesDT.Rows.Count > 0) { DerChargesDT.Rows.Add("Total Charges", totalHeadAmount); }
            objGSmary.Tables.Add(DerChargesDT);
            DataTable OptionsDT = new DataTable();
            OptionsDT.Columns.Add("Options");
            objGSmary.Tables.Add(OptionsDT);
            objGSmary.Tables.Add(DownloadOptionsDT);
            DataTable FuturesDT = new DataTable();
            FuturesDT.Columns.Add("Futures");
            objGSmary.Tables.Add(FuturesDT);
            objGSmary.Tables.Add(DownloadFuturesDT);
            return objGSmary;
        }

        /// <summary>
        /// For Getting MTF Interest Report Summary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Obj MTFInterestReportResMdl </returns>
        public async Task<ResponseBaseMTFRepModel<MTFInterestReportResMdl>> GetMTFInterestReport(MTFInterestReportReqMdl obj)
        {
            string FromDate = string.Empty;
            string ToDate = string.Empty;
            int curYear = DateTime.Now.Year;

            switch (obj.Type)
            {
                case "CFY":
                    FromDate = "01/04/" + curYear;
                    ToDate = "31/03/" + (curYear + 1);
                    break;
                case "PFY":
                    FromDate = "01/04/" + (curYear - 1);
                    ToDate = "31/03/" + curYear;
                    break;
                default:
                    break;
            }

            DateTime FromFYDate = DateTime.ParseExact(FromDate, "dd/MM/yyyy", null);
            DateTime ToFYDate = DateTime.ParseExact(ToDate, "dd/MM/yyyy", null);

            var response = new ResponseBaseMTFRepModel<MTFInterestReportResMdl>()
            {
                FromDate = FromDate,
                ToDate = ToDate,
                TotalAmount = 0,
                Datas = new List<MTFInterestReportResMdl>(),
                TotalRows = 0
            };
            using (IDbConnection TRwcon = CreateTrvwConnection())
            {
                if (TRwcon.State != ConnectionState.Open)
                    TRwcon.Open();

                var param = new DynamicParameters();
                param.Add("@ClientId", obj.Uid);
                param.Add("@FromDate", Convert.ToDateTime(FromFYDate));
                param.Add("@ToDate", Convert.ToDateTime(ToFYDate));
                var result = (await SqlMapper.QueryAsync<MTFInterestReportResMdl>(TRwcon, "RP_Get_MTF_Interest_Report", param, commandType: CommandType.StoredProcedure)).ToList();

                if (result != null && result.Any())
                {
                    response.FromDate = FromDate;
                    response.ToDate = ToDate;
                    response.TotalAmount = result.Sum(x => x.Amount);
                    response.Datas = result.OrderByDescending(x => DateTime.Parse(x.VoucherDate)).ToList();
                    response.TotalRows = response.Datas.Count;
                }
            }
            return response;
        }

        public async Task<ResponseBaseMXCModel> GetMCXUnderlyingInfo()
        {
            ResponseBaseMXCModel obj = new ResponseBaseMXCModel();
            List<MCXUnderlyingInfoResponse> ListData = new List<MCXUnderlyingInfoResponse>();
            string baseURL = _config["TradingoRestAPI:BaseUrl"].ToString();
            string addressSuffix = "itrl/Search/GetMCXUnderlyingInfo";
            string strResponse = await _httpClientPostService.WebRequestPostAsync(baseURL, addressSuffix, "");

            if (!string.IsNullOrEmpty(strResponse))
            {
                strResponse = strResponse.Replace(@"\", @"/").Replace(@"\\", @"/");
                obj = JsonConvert.DeserializeObject<ResponseBaseMXCModel>(strResponse);
                if (obj.Result != null && obj.Result.Data != null && obj.Result.Data.Any())
                {
                    MCXDataStore.Reference.MCXDataStoreMdl = new List<MCXUnderlyingInfoResponse>();
                    MCXDataStore.Reference.MCXDataStoreMdl = obj.Result.Data;
                }
            }
            return obj;
        }

        public async Task<string> GetScripSymbolFromBSESecurityInfo(string Token)
        {
            string ScripID = string.Empty;
            if (BSESecurityReportDataStore.Reference.BSESecurityReqMdl == null)
                await LoadBSESecurityInfoReport();

            if (BSESecurityReportDataStore.Reference.BSESecurityReqMdl != null)
            {
                var ObjBSESecInfo = BSESecurityReportDataStore.Reference.BSESecurityReqMdl.Where(x => x.Token == Convert.ToInt32(Token)).FirstOrDefault();
                if (ObjBSESecInfo != null)
                    ScripID = ObjBSESecInfo.ScripID;
            }

            return string.IsNullOrEmpty(ScripID) ? Token : ScripID;
        }

        public async Task<ResponseBaseModel> LoadBSESecurityInfoReport()
        {
            using (IDbConnection con = CreateRPConnection())
            {
                if (con.State != ConnectionState.Open)
                    con.Open();
                //BSESecurity_LoadDataForStore - Change name
                var result = (await SqlMapper.QueryAsync<BSESecurityReqMdl>(con, "BSESecurityInfoReport_LoadDataForStore", null, commandType: CommandType.StoredProcedure)).ToList();

                if (result != null && result.Any())
                {
                    BSESecurityReportDataStore.Reference.BSESecurityReqMdl = new List<BSESecurityReqMdl>();
                    BSESecurityReportDataStore.Reference.BSESecurityReqMdl = result;
                }
            }
            return new ResponseBaseModel
            {
                ResponseId = 1,
                ResponseMessage = "Information Loaded successfully"
            };
        }
    }
}
