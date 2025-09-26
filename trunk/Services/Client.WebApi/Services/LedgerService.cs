using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Components;
using Dapper;

namespace Client.WebApi.Services
{
    public interface ILedgerService
    {
        public Task<ResponseBaseModel<PassbookData>> GetFundsAddedAndWithdrawnList(LedgerInternalRequest request);
        public Task<ResponseBaseModel<PassbookData>> GetFundsUtilisedList(LedgerInternalRequest request);        
    }
    public class LedgerService : BaseService, ILedgerService
    {
        private readonly ILog _logger;

        public LedgerService(ILog logger)
        {
            _logger = logger;
        }

        public async Task<ResponseBaseModel<PassbookData>> GetFundsAddedAndWithdrawnList(LedgerInternalRequest request)
        {
            var result = new ResponseBaseModel<PassbookData>()
            {
                Datas = new List<PassbookData>(),
            };

            //1 = All, 5 = DP Charges
            if (request.CategoryId == 1 || request.CategoryId == 5)
            {
                //Pull the data from DP_05 table
                result.Datas = await GetLedgerGetDPCharges(request);
            }

            //Pull data from existing logic (TechExcel)
            var otherChargesList = await GetLedgerFADataResponseList(request);

            if (otherChargesList != null && otherChargesList.Any())
            {
                //Merge two list
                foreach (var item in otherChargesList)
                {
                    var existingItem = result.Datas.FirstOrDefault(x => x.VoucherDate == item.VoucherDate);
                    if (existingItem != null)
                    {
                        //Merge Section1 List
                        existingItem.Section1List.AddRange(item.Section1List);
                    }
                    else
                    {
                        result.Datas.Add(item);
                    }
                }
            }

            result.TotalRows = result.Datas.Count;
            return result;
        }

        public async Task<ResponseBaseModel<PassbookData>> GetFundsUtilisedList(LedgerInternalRequest request)
        {
            var result = new ResponseBaseModel<PassbookData>()
            {
                Datas = new List<PassbookData>(),
            };

            // //"FundsUtilisedIn": ",8" = Equity ,    "FundsUtilisedFor": ",15" = Misc
            if (request.FundsUtilisedFor == "15" || request.FundsUtilisedIn == "8")
            {
                //Pull the data from DP_05 table
                result.Datas = await GetLedgerGetDPCharges(request);
            }

            //Pull data from existing logic (TechExcel)
            var otherChargesList = await GetLedgerFADataResponseList(request);

            if (otherChargesList != null && otherChargesList.Any())
            {
                //Merge two list
                foreach (var item in otherChargesList)
                {
                    var existingItem = result.Datas.FirstOrDefault(x => x.VoucherDate == item.VoucherDate);
                    if (existingItem != null)
                    {
                        //Merge Section1 List
                        existingItem.Section1List.AddRange(item.Section1List);
                    }
                    else
                    {
                        result.Datas.Add(item);
                    }
                }
            }

            result.TotalRows = result.Datas.Count;
            return result;
        }


        private async Task<List<PassbookData>> GetLedgerGetDPCharges(LedgerInternalRequest request)
        {
            var Datas = new List<PassbookData>();

            using (IDbConnection con = CreateTrvwConnection())
            {
                if (con.State != ConnectionState.Open)
                    con.Open();
                int FinStartYear = (DateTime.Now.Month >= 4 ? DateTime.Now.Year : DateTime.Now.Year - 1);

                var param = new DynamicParameters();
                param.Add("@FromDate", DateTime.Parse("2025-04-01"), DbType.Date);
                param.Add("@ClientCode", request.ClientCode, DbType.String);

                try
                {
                    var mainLedgerList = (await SqlMapper.QueryAsync<LedgerResponse>(con, "Ledger_GetDPCharges", param, commandType: CommandType.StoredProcedure)).ToList();

                    if (mainLedgerList != null && mainLedgerList.Any())
                    {
#if DEBUG
                        //mainLedgerList = mainLedgerList.Where(x => x.VOUCHERDATE == DateTime.Parse("2025-08-12")).ToList();
#endif

                        List<PassbookData> objPassbookData = new List<PassbookData>();
                        Datas = objPassbookData;

                        //Process DB records group by Voucher Date and Categories
                        var voucherDateList = mainLedgerList.Select(a => a.VOUCHERDATE).Distinct();

                        #region Date wise processing
                        foreach (DateTime voucherDateItem in voucherDateList)
                        {
                            PassbookData record = new PassbookData();
                            Datas.Add(record);

                            record.VoucherDate = voucherDateItem.ToString("dd MMM yy");

                            List<LedgerResponse> listFilteredByDate = mainLedgerList.Where(item => item.VOUCHERDATE == voucherDateItem).ToList();
                            if (listFilteredByDate != null && listFilteredByDate.Any())
                            {
                                //Process DB records group by Voucher Date and Categories
                                var categoryList = listFilteredByDate.Select(a => a.CATEGORY).Distinct();
                                foreach (var categoryItem in categoryList)
                                {
                                    //Category wise records
                                    List<LedgerResponse> listFilteredByCategory = listFilteredByDate.Where(item => item.CATEGORY == categoryItem).ToList();
                                    if (listFilteredByCategory != null && listFilteredByCategory.Any())
                                    {
                                        //Section 1
                                        Section1 objSection1 = new Section1()
                                        {
                                            Description = string.Empty,
                                            IsTransTypeCR = false,
                                        };

                                        record.Section1List.Add(objSection1);
                                        objSection1.TotalAmount = listFilteredByCategory.Sum(x => x.DP_CHARGE); //DP Charges Total

                                        //categoryItem is DP Charges
                                        if (categoryItem == DPChargeCategoryType.DP.ToString())
                                        {
                                            objSection1.Id = DPChargeCategoryType.DP;
                                            objSection1.TypeId = Section1Category.VIEW_DETAIL;
                                            objSection1.ActionText = "View Details";
                                            objSection1.LabelText = "DP Charges";

                                            //Section 2
                                            Section2 objSection2 = new Section2();
                                            objSection1.Section2Item = objSection2;

                                            objSection2.HeaderText = $"{objSection1.LabelText} for {voucherDateItem.ToString("dd MMM yy")}"; //DP Charges for 15 Sep 2025
                                            objSection2.BodyText = string.Empty;

                                            var subcategoryList = listFilteredByCategory.Select(a => a.SUB_CATEGORY).Distinct();

                                            foreach (var subcategoryItem in subcategoryList)
                                            {
                                                //Sub Category wise records
                                                List<LedgerResponse> listFilteredBySubCategory = listFilteredByCategory.Where(item => item.SUB_CATEGORY == subcategoryItem).ToList();

                                                Section3 objSection3 = new Section3();

                                                //To merge Pledge and Unpledge Charges into one subcategory
                                                objSection3 = objSection2.Section3List.FirstOrDefault(x => x.LabelText == "Pledge/Unpledge Charges");
                                                if (objSection3 == null)
                                                {
                                                    objSection3 = new Section3();
                                                    objSection2.Section3List.Add(objSection3);
                                                }

                                                objSection3.TotalAmount = listFilteredBySubCategory.Sum(x => x.DP_CHARGE);

                                                var groupbystocks = listFilteredBySubCategory.GroupBy(r => r.SCRIP_NAME)
                                                                                                                    .Select(g => new
                                                                                                                    {
                                                                                                                        StockName = g.Key,
                                                                                                                        Quantity = Math.Round(g.Sum(x => x.QTY), 0),
                                                                                                                        Amount = g.Sum(x => x.DP_CHARGE)
                                                                                                                    })
                                                                                                                    .ToList();

                                                switch (subcategoryItem)
                                                {
                                                    case "STOCK_SELLING":// DPChargeSubCategoryType.STOCK_SELLING.ToString():
                                                        objSection3.LabelText = "Stock Selling Charges";
                                                        objSection3.InfoText = "Fee for Selling shares, including taxes.";

                                                        foreach (var stockItem in groupbystocks)
                                                        {
                                                            //Section 4
                                                            Section4 objSection4 = new Section4();
                                                            objSection3.Section4List.Add(objSection4);
                                                            objSection4.Amount = stockItem.Amount;
                                                            objSection4.LabelText = $"{stockItem.StockName} (Qty {stockItem.Quantity})";
                                                        }

                                                        break;

                                                    case "PLEDGE": // DPChargeSubCategoryType.PLEDGE.ToString():
                                                    case "UNPLEDGE": // DPChargeSubCategoryType.UNPLEDGE.ToString():
                                                        objSection3.LabelText = "Pledge/Unpledge Charges";
                                                        objSection3.InfoText = "Fee for pledging or unpledging shares.";

                                                        foreach (var stockItem in groupbystocks)
                                                        {
                                                            //Section 4
                                                            Section4 objSection4 = new Section4();
                                                            objSection3.Section4List.Add(objSection4);
                                                            objSection4.Amount = stockItem.Amount;
                                                            objSection4.LabelText = $"{stockItem.StockName} (Qty {stockItem.Quantity})";
                                                            objSection4.Tag = subcategoryItem.Equals(DPChargeSubCategoryType.PLEDGE.ToString()) ? "Pledged" : "Unpledged";
                                                        }

                                                        break;

                                                    case "OFFMARKET": // DPChargeSubCategoryType.OFFMARKET.ToString():
                                                        objSection3.LabelText = "Offmarket Transactions";
                                                        objSection3.InfoText = "Fee for moving shared between demat accounts.";

                                                        foreach (var stockItem in groupbystocks)
                                                        {
                                                            //Section 4
                                                            Section4 objSection4 = new Section4();
                                                            objSection3.Section4List.Add(objSection4);
                                                            objSection4.Amount = stockItem.Amount;
                                                            objSection4.LabelText = $"{stockItem.StockName} (Qty {stockItem.Quantity})";
                                                        }

                                                        break;

                                                    default:
                                                        objSection3.LabelText = "Unknown";
                                                        break;
                                                }

                                            }


                                        }
                                        else
                                        {
                                            //Top list - Section 1
                                            objSection1.TypeId = Section1Category.NONE;
                                            objSection1.ActionText = string.Empty;

                                            switch (categoryItem)
                                            {
                                                case "DEMAT_SETUP":// DPChargeCategoryType.DEMAT_SETUP.ToString():
                                                    objSection1.Id = DPChargeCategoryType.DEMAT_SETUP;
                                                    objSection1.LabelText = "Demat setup charges";
                                                    break;
                                                case "OTHER":// DPChargeCategoryType.OTHER.ToString():
                                                    objSection1.Id = DPChargeCategoryType.OTHER;
                                                    objSection1.LabelText = "Other";
                                                    break;
                                                default:
                                                    objSection1.Id = DPChargeCategoryType.NONE;
                                                    objSection1.LabelText = "Unknow";
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Ledger_GetDPCharges: Exception: " + ex.ToString());
                }

                return Datas;
            }
        }

        private async Task<List<PassbookData>> GetLedgerFADataResponseList(LedgerInternalRequest obj)
        {
            bool IsDownload = true;
            List<PassbookData> objPassbookDataList = new List<PassbookData>();

            var param = new DynamicParameters();
            param.Add(name: "@ClientCode", value: obj.ClientCode);
            param.Add(name: "@FromDate", value: obj.FromDate);
            param.Add(name: "@ToDate", value: obj.ToDate);
            param.Add(name: "@StartYear", value: obj.StartYear);

            using (IDbConnection conn = CreateTrvwConnection())
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                int RowId = (await SqlMapper.QueryAsync<int>(conn, "Ledger_APIDataCountAndDelete", param, commandType: CommandType.StoredProcedure)).FirstOrDefault();

                if (RowId <= 0)
                {
                    IsDownload = false;

                    //Call TechExcel(Back Office API) and Stored Data in to DB                   
                    IsDownload = await PopulateLedgerAPIData(obj);
                }

                if (IsDownload)
                {
                    try
                    {
                        // After Store Data From TechExcel Now Get Data from DB
                        param.Add(name: "@CategoryId", value: obj.CategoryId);
                        param.Add(name: "@SubCategoryId", value: obj.SubCategoryId);
                        var ReportDcs = (await SqlMapper.QueryAsync<LedgerFADataResponse>(conn, "Ledger_GetFundsAddedAndWithdrawn", param, commandType: CommandType.StoredProcedure)).ToList();
                        if (ReportDcs != null && ReportDcs.Any())
                        {
                            var myDateList = ReportDcs.Select(a => a.VOUCHERDATE).Distinct(); //.OrderBy(a => a)

                            //VoucherDate
                            foreach (string value in myDateList)
                            {
                                List<LedgerFADataResponse> listFiltered = ReportDcs.Where(item => item.VOUCHERDATE == value).ToList();
                                if (listFiltered != null && listFiltered.Any())
                                {
                                    PassbookData passbookData = new PassbookData();
                                    objPassbookDataList.Add(passbookData);
                                    passbookData.VoucherDate = DateTime.ParseExact(value, "MM/dd/yyyy", null).ToString("dd MMM yy");

                                    //NARRATION
                                    foreach (var ledgerItem in listFiltered)
                                    {
                                        if (ledgerItem.NARRATION != null && ledgerItem.NARRATION.Contains("dp balance transfer"))
                                        {
                                            if (Convert.ToDecimal(ledgerItem.CR_AMT) > 0)
                                            {
                                                var section1 = new Section1();
                                                passbookData.Section1List.Add(section1);
                                                //section1.Id = DPChargeCategoryType.DP;
                                                section1.TypeId = Section1Category.NONE;
                                                section1.TotalAmount = Convert.ToDecimal(ledgerItem.CR_AMT) + Convert.ToDecimal(ledgerItem.DR_AMT); //TODO - get this from DB
                                                section1.IsTransTypeCR = true;
                                                section1.LabelText = "DP charges refund";
                                            }
                                            else
                                            {
                                                //Skip duplicate DP Charges
                                            }
                                        }
                                        else
                                        {
                                            var section1 = new Section1();
                                            passbookData.Section1List.Add(section1);
                                            //section1.Id = DPChargeCategoryType.NONE;
                                            section1.TypeId = Section1Category.NONE;
                                            section1.TotalAmount = Convert.ToDecimal(ledgerItem.CR_AMT) + Convert.ToDecimal(ledgerItem.DR_AMT); //TODO - get this from DB
                                            section1.IsTransTypeCR = Convert.ToDecimal(ledgerItem.CR_AMT) > 0;
                                            section1.LabelText = ledgerItem.NARRATION;
                                        }
                                    }
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("GetLedgerFADataResponseList: Exception: " + ex.ToString());
                    }
                }
            }
            return objPassbookDataList;
        }

        private async Task<bool> PopulateLedgerAPIData(LedgerInternalRequest request)
        {
            using (IDbConnection con = CreateCapsfoConnection())
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                int FinStartYear = (DateTime.Now.Month >= 4 ? DateTime.Now.Year : DateTime.Now.Year - 1);

                var param = new DynamicParameters();
                param.Add("@COMPANY_CODE", "BSE_CASH','BSE_FNO','CD_BSE','CD_NSE','MCX','MF_BSE','MTF','NCDEX','NCL','NSE_CASH','NSE_COM','NSE_FNO", DbType.String);
                param.Add("@START_YEAR", FinStartYear, DbType.Int32);
                param.Add("@FROM_DATE", "01/04/" + FinStartYear.ToString(), DbType.String);
                param.Add("@TO_DATE", DateTime.Now.ToString(@"dd/MM/yyyy"), DbType.String);
                param.Add("@LEDGER_LIST", request.ClientCode, DbType.String);
                param.Add("@MERGECOMPANY", "Y", DbType.String);

                try
                {
                    var LedgerList = (await SqlMapper.QueryAsync<LedgerAPIResponse>(con, "FA_SMARTREPORT_LEDGER_DETAIL", param, commandType: CommandType.StoredProcedure)).ToList();
                    if (LedgerList != null && LedgerList.Any())
                    {
                        //Populate extra fields
                        LedgerList.ForEach(obj =>
                        {
                            obj.FromDate = request.FromDate.Trim();
                            obj.ToDate = request.ToDate.Trim();
                            obj.STARTYEAR = request.StartYear.Trim();
                            obj.CreatedDate = DateTime.Now;
                        });

                        var objBulk = new LedgerBulkUploadToSql<LedgerAPIResponse>();
                        objBulk.InternalStore = LedgerList;
                        objBulk.CommitBatchSize = 1000;
                        objBulk.ConnectionString = CreateTrvwConnection().ConnectionString;
                        objBulk.TableName = "dbo.LedgerAPIData";
                        await objBulk.LedgerCommit();
                        return true;
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error("PopulateLedgerAPIData: Exception: " + ex.ToString());
                }

                return false;
            }
        }
    }

    

    
}
