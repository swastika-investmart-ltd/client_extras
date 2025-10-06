using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Components;
using Dapper;
using Microsoft.Extensions.Configuration;
using SqlBulkTools;

namespace Client.WebApi.Services
{
    public interface ILedgerService
    {
        public Task<ResponseBaseModel<PassbookData>> GetFundsAddedAndWithdrawnList(LedgerInternalRequest request);
        public Task<ResponseBaseModel<PassbookData>> GetFundsUtilisedList(LedgerInternalRequest request);
        public string GetToDateFromConfig(int FinStartYear);
    }

    public class LedgerService : BaseService, ILedgerService
    {
        private readonly ILog _logger;
        IConfiguration _config;

        public LedgerService(ILog logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Retrieves a list of funds added and withdrawn for the given request.
        /// Merges DP charges and other charges into a single result.
        /// </summary>
        public async Task<ResponseBaseModel<PassbookData>> GetFundsAddedAndWithdrawnList(LedgerInternalRequest request)
        {
            var result = new ResponseBaseModel<PassbookData>()
            {
                Datas = new List<PassbookData>(),
            };

            // 1 = All, 5 = DP Charges
            if (request.CategoryId == 1 || request.CategoryId == 5)
            {
                // Pull the data from DP_05 table
                result.Datas = await GetLedgerGetDPCharges(request);
            }

            // Pull data from existing logic (TechExcel)
            var otherChargesList = await GetLedgerFADataResponseList(request);

            if (otherChargesList != null && otherChargesList.Any())
            {
                // Merge two lists by VoucherDate
                foreach (var item in otherChargesList)
                {
                    var existingItem = result.Datas.FirstOrDefault(x => x.VoucherDate == item.VoucherDate);
                    if (existingItem != null)
                    {
                        // Merge Section1 List
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

        /// <summary>
        /// Retrieves a list of funds utilized for the given request.
        /// Merges DP charges and other charges into a single result.
        /// </summary>
        public async Task<ResponseBaseModel<PassbookData>> GetFundsUtilisedList(LedgerInternalRequest request)
        {
            var result = new ResponseBaseModel<PassbookData>()
            {
                Datas = new List<PassbookData>(),
            };

            int[] fundsUtilisedInList = request.FundsUtilisedIn.Split(',', StringSplitOptions.RemoveEmptyEntries) // split by comma and remove blanks
                                                    .Select(int.Parse) // convert to int
                                                    .ToArray();
            int[] fundsUtilisedForList = request.FundsUtilisedFor.Split(',', StringSplitOptions.RemoveEmptyEntries) // split by comma and remove blanks
                                                    .Select(int.Parse) // convert to int
                                                    .ToArray();

            if(fundsUtilisedInList.Length > 0 && fundsUtilisedForList.Length > 0)
            {
                // "FundsUtilisedIn": ",8" = Equity , "FundsUtilisedFor": ",15" = Misc
                if ((fundsUtilisedInList.All(x => x == 0) && fundsUtilisedForList.All(x => x == 0))
                    || (fundsUtilisedInList.All(x => x == 0) && fundsUtilisedForList.Contains(15)) // All + Misc
                    || (fundsUtilisedInList.All(x => x == 8) && fundsUtilisedForList.Contains(0)) // Equity + All
                    || (fundsUtilisedInList.All(x => x == 8) && fundsUtilisedForList.Contains(15)) // Equity + Misc
                    )
                {
                    // Pull the data from DP_05 table
                    result.Datas = await GetLedgerGetDPCharges(request);
                }
            }

            // Pull data from existing logic (TechExcel)
            var otherChargesList = await GetLedgerFADataResponseList(request, true);

            if (otherChargesList != null && otherChargesList.Any())
            {
                // Merge two lists by VoucherDate
                foreach (var item in otherChargesList)
                {
                    var existingItem = result.Datas.FirstOrDefault(x => x.VoucherDate == item.VoucherDate);
                    if (existingItem != null)
                    {
                        // Merge Section1 List
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

        /// <summary>
        /// Retrieves DP charges from the database for the given request.
        /// </summary>
        private async Task<List<PassbookData>> GetLedgerGetDPCharges(LedgerInternalRequest request)
        {
            var Datas = new List<PassbookData>();

            using (IDbConnection con = CreateTrvwConnection())
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                var param = new DynamicParameters();                
                param.Add("@FromDate", request.FromDate , DbType.Date); //To configure splitter new vs old
                param.Add("@ClientCode", request.ClientCode, DbType.String);

                try
                {
                    var mainLedgerList = (await SqlMapper.QueryAsync<LedgerResponse>(con, "Ledger_GetDPCharges", param, commandType: CommandType.StoredProcedure)).ToList();

                    if (mainLedgerList != null && mainLedgerList.Any())
                    {
#if DEBUG
                        //mainLedgerList = mainLedgerList.Where(x => x.VOUCHERDATE == DateTime.Parse("2025-08-12")).ToList();
#endif
                        Datas = ProcessVoucherDates(mainLedgerList);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Ledger_GetDPCharges: Exception: " + ex.ToString());
                    throw;
                }

                return Datas;
            }
        }

        /// <summary>
        /// Processes the main ledger list and groups data by voucher date.
        /// </summary>
        private List<PassbookData> ProcessVoucherDates(List<LedgerResponse> mainLedgerList)
        {
            var Datas = new List<PassbookData>();
            var voucherDateList = mainLedgerList.Select(a => a.VOUCHERDATE).Distinct();

            foreach (DateTime voucherDateItem in voucherDateList)
            {
                PassbookData record = new PassbookData();
                Datas.Add(record);
                record.VoucherDate = voucherDateItem.ToString("dd MMM yy");

                List<LedgerResponse> listFilteredByDate = mainLedgerList.Where(item => item.VOUCHERDATE == voucherDateItem).ToList();
                if (listFilteredByDate != null && listFilteredByDate.Any())
                {
                    ProcessCategories(listFilteredByDate, record, voucherDateItem);
                }
            }
            return Datas;
        }

        /// <summary>
        /// Processes categories for a given voucher date and populates Section1 data.
        /// </summary>
        private void ProcessCategories(List<LedgerResponse> listFilteredByDate, PassbookData record, DateTime voucherDateItem)
        {
            var categoryList = listFilteredByDate.Select(a => a.CATEGORY).Distinct();
            foreach (var categoryItem in categoryList)
            {
                List<LedgerResponse> listFilteredByCategory = listFilteredByDate.Where(item => item.CATEGORY == categoryItem).ToList();
                if (listFilteredByCategory != null && listFilteredByCategory.Any())
                {
                    Section1 objSection1 = new Section1()
                    {
                        Description = string.Empty,
                        IsTransTypeCR = false,
                    };

                    if (record.Section1List is null) record.Section1List = new List<Section1>();

                    record.Section1List.Add(objSection1);
                    objSection1.TotalAmount = listFilteredByCategory.Sum(x => x.DP_CHARGE);

                    if (categoryItem == DPChargeCategoryType.DP.ToString())
                    {
                        PopulateDPSection1(objSection1, listFilteredByCategory, voucherDateItem);
                    }
                    else
                    {
                        PopulateOtherSection1(objSection1, categoryItem);
                    }
                }
            }
        }

        /// <summary>
        /// Populates Section1 for DP category, including Section2 and Section3 details.
        /// </summary>
        private void PopulateDPSection1(Section1 objSection1, List<LedgerResponse> listFilteredByCategory, DateTime voucherDateItem)
        {
            objSection1.Id = DPChargeCategoryType.DP;
            objSection1.TypeId = Section1Category.VIEW_DETAIL;
            objSection1.ActionText = "View Details";
            objSection1.LabelText = "DP Charges";

            Section2 objSection2 = new Section2();
            objSection1.Section2Item = objSection2;

            objSection2.HeaderText = $"{objSection1.LabelText} for {voucherDateItem.ToString("dd MMM yy")}";
            objSection2.BodyText = string.Empty;

            var subcategoryList = listFilteredByCategory.Select(a => a.SUB_CATEGORY).Distinct();
            foreach (var subcategoryItem in subcategoryList)
            {
                List<LedgerResponse> listFilteredBySubCategory = listFilteredByCategory.Where(item => item.SUB_CATEGORY == subcategoryItem).ToList();
                PopulateSection3(objSection2, subcategoryItem, listFilteredBySubCategory);
            }
        }

        /// <summary>
        /// Populates Section1 for non-DP categories.
        /// </summary>
        private void PopulateOtherSection1(Section1 objSection1, string categoryItem)
        {
            objSection1.TypeId = Section1Category.NONE;
            objSection1.ActionText = string.Empty;

            switch (categoryItem)
            {
                case "DEMAT_SETUP":
                    objSection1.Id = DPChargeCategoryType.DEMAT_SETUP;
                    objSection1.LabelText = "Demat Setup Charges";
                    break;
                default:
                    objSection1.Id = DPChargeCategoryType.NONE;
                    objSection1.LabelText = "Other Charges";
                    break;
            }
        }

        /// <summary>
        /// Populates Section3 for a given subcategory and its related ledger responses.
        /// </summary>
        private void PopulateSection3(Section2 objSection2, string subcategoryItem, List<LedgerResponse> listFilteredBySubCategory)
        {
            if (objSection2.Section3List is null) objSection2.Section3List = new List<Section3>();

            // For PLEDGE/UNPLEDGE, reuse the same Section3 if exists
            Section3 objSection3 = objSection2.Section3List.FirstOrDefault(x => x.LabelText == "Pledge/Unpledge Charges");
            if (subcategoryItem == "PLEDGE" || subcategoryItem == "UNPLEDGE")
            {
                if (objSection3 == null)
                {
                    objSection3 = new Section3();
                    objSection2.Section3List.Add(objSection3);
                }
            }
            else
            {
                objSection3 = new Section3();
                objSection2.Section3List.Add(objSection3);
            }

            //To handle Pledge/Unpledge sum 
            objSection3.TotalAmount += listFilteredBySubCategory.Sum(x => x.DP_CHARGE);

            // Group by stock name for Section4 details
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
                case "STOCK_SELLING":
                    objSection3.LabelText = "Stock Selling Charges";
                    objSection3.InfoText = "Fee for Selling shares, including taxes.";
                    foreach (var stockItem in groupbystocks)
                    {
                        AddSection4(objSection3, stockItem.Amount, $"{stockItem.StockName} (Qty {stockItem.Quantity})");
                    }
                    break;
                case "PLEDGE":
                case "UNPLEDGE":
                    objSection3.LabelText = "Pledge/Unpledge Charges";
                    objSection3.InfoText = "Fee for pledging or unpledging shares.";
                    foreach (var stockItem in groupbystocks)
                    {
                        AddSection4(objSection3, stockItem.Amount, $"{stockItem.StockName} (Qty {stockItem.Quantity})", subcategoryItem.Equals(DPChargeSubCategoryType.PLEDGE.ToString()) ? "Pledged" : "Unpledged");
                    }
                    break;
                case "OFFMARKET":
                    objSection3.LabelText = "Offmarket Transactions";
                    objSection3.InfoText = "Fee for moving shared between demat accounts.";
                    foreach (var stockItem in groupbystocks)
                    {
                        AddSection4(objSection3, stockItem.Amount, $"{stockItem.StockName} (Qty {stockItem.Quantity})");
                    }
                    break;
                default:
                    objSection3.LabelText = "Other DP charges";
                    //objSection3.InfoText = "";
                    foreach (var stockItem in groupbystocks)
                    {
                        AddSection4(objSection3, stockItem.Amount, $"{stockItem.StockName} (Qty {stockItem.Quantity})");
                    }
                    break;
            }
        }

        /// <summary>
        /// Adds a Section4 entry to the given Section3.
        /// </summary>
        private void AddSection4(Section3 objSection3, decimal amount, string labelText, string tag = null)
        {
            if (objSection3.Section4List is null) objSection3.Section4List = new List<Section4>();

            Section4 objSection4 = new Section4
            {
                Amount = amount,
                LabelText = labelText,
                Tag = tag
            };
            objSection3.Section4List.Add(objSection4);
        }

        /// <summary>
        /// Retrieves funds added/withdrawn data from the database or external API and processes it into PassbookData.
        /// </summary>
        private async Task<List<PassbookData>> GetLedgerFADataResponseList(LedgerInternalRequest obj, bool isFundUtilizeReqType = false)
        {
            var objPassbookDataList = new List<PassbookData>();
            var param = new DynamicParameters();
            param.Add("@ClientCode", obj.ClientCode);
            param.Add("@FromDate", obj.FromDate);
            param.Add("@ToDate", obj.ToDate);
            param.Add("@StartYear", obj.StartYear);

            using (IDbConnection conn = CreateTrvwConnection())
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                // Check if data is already downloaded
                int rowId = (await SqlMapper.QueryAsync<int>(conn, "Ledger_APIDataCountAndDelete", param, commandType: CommandType.StoredProcedure)).FirstOrDefault();
                bool isDownload = rowId > 0;

                if (!isDownload)
                {
                    // Call TechExcel (Back Office API) and store data in DB
                    isDownload = await PopulateLedgerAPIData(obj);
                }

                if (isDownload)
                {
                    try
                    {
                        string sp_name = isFundUtilizeReqType ? "Ledger_GetFundsUtilised" : "Ledger_GetFundsAddedAndWithdrawn";

                        if (isFundUtilizeReqType)
                        {
                            //GET Details FROM Table IF Already Exists
                            param.Add(name: "@FundsUtilisedIn", value: obj.FundsUtilisedIn);
                            param.Add(name: "@FundsUtilisedFor", value: obj.FundsUtilisedFor);
                        }
                        else
                        {
                            param.Add("@CategoryId", obj.CategoryId);
                            param.Add("@SubCategoryId", obj.SubCategoryId);
                        }

                        var reportDcs = (await SqlMapper.QueryAsync<LedgerFADataResponse>(conn, sp_name, param, commandType: CommandType.StoredProcedure)).ToList();

                        if (reportDcs != null && reportDcs.Any())
                        {
                            // Group by voucher date and create PassbookData entries
                            foreach (var dateGroup in reportDcs.GroupBy(a => a.VOUCHERDATE))
                            {
                                //VOUCHERDATE is null - TODO: check why null for 'OPENING BALANCE'
                                if (dateGroup.Key is null)
                                {
                                    continue;
                                }

                                var passbookData = new PassbookData
                                {
                                    VoucherDate = DateTime.ParseExact(dateGroup.Key, "MM/dd/yyyy", null).ToString("dd MMM yy")
                                };

                                passbookData.Section1List = new List<Section1>();
                                foreach (var ledgerItem in dateGroup)
                                {
                                    // Special handling for DP balance transfer
                                    if (!string.IsNullOrEmpty(ledgerItem.NARRATION) && ledgerItem.NARRATION.Contains("DP Balance Transfer", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (Convert.ToDecimal(ledgerItem.CR_AMT) > 0)
                                        {
                                            passbookData.Section1List.Add(new Section1
                                            {
                                                TypeId = Section1Category.NONE,
                                                TotalAmount = Convert.ToDecimal(ledgerItem.CR_AMT) + Convert.ToDecimal(ledgerItem.DR_AMT),
                                                IsTransTypeCR = true,
                                                LabelText = "DP Charges Refund"
                                            });

                                            objPassbookDataList.Add(passbookData);

                                        }
                                        // else: skip duplicate DP Charges
                                    }
                                    else
                                    {
                                        passbookData.Section1List.Add(new Section1
                                        {
                                            TypeId = Section1Category.NONE,
                                            TotalAmount = Convert.ToDecimal(ledgerItem.CR_AMT) + Convert.ToDecimal(ledgerItem.DR_AMT),
                                            IsTransTypeCR = Convert.ToDecimal(ledgerItem.CR_AMT) > 0,
                                            LabelText = ledgerItem.NARRATION
                                        });

                                        objPassbookDataList.Add(passbookData);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("GetLedgerFADataResponseList: Exception: " + ex);
                        throw;
                    }
                }
            }
            return objPassbookDataList;
        }

        /// <summary>
        /// Calls the external API to populate ledger data into the database.
        /// </summary>
        private async Task<bool> PopulateLedgerAPIData(LedgerInternalRequest request)
        {
            using (IDbConnection con = CreateTrvwConnection())
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                // Determine financial start year
                int FinStartYear = (DateTime.Now.Month >= 4 ? DateTime.Now.Year : DateTime.Now.Year - 1);

                //FromDate '01/04/2025'
	            //ToDate '25-09-2025'
                var param = new DynamicParameters();
                param.Add("@COMPANY_CODE", "BSE_CASH','BSE_FNO','CD_BSE','CD_NSE','MCX','MF_BSE','MTF','NCDEX','NCL','NSE_CASH','NSE_COM','NSE_FNO", DbType.String);
                param.Add("@START_YEAR", FinStartYear, DbType.Int32);
                param.Add("@FROM_DATE", request.FromDate, DbType.String);
                param.Add("@TO_DATE", DateTime.Now.ToString(@"dd/MM/yyyy"), DbType.String);
                param.Add("@LEDGER_LIST", request.ClientCode, DbType.String);
                param.Add("@MERGECOMPANY", "Y", DbType.String);

                try
                {
                    var LedgerList = (await SqlMapper.QueryAsync<LedgerAPIResponse>(con, "FA_SMARTREPORT_LEDGER_DETAIL", param, commandType: CommandType.StoredProcedure)).ToList();
                    if (LedgerList != null && LedgerList.Any())
                    {
                        // Populate extra fields for each record
                        LedgerList.ForEach(obj =>
                        {
                            obj.FromDate = request.FromDate.Trim();
                            obj.ToDate = request.ToDate.Trim();
                            obj.STARTYEAR = request.StartYear.Trim();
                            obj.CreatedDate = DateTime.Now;
                        });

                        return await LedgerBulkUploadToDatabase(LedgerList, "LedgerAPIData");
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error("PopulateLedgerAPIData: Exception: " + ex.ToString());
                    throw;
                }

                return false;
            }
        }

        private async Task<bool> LedgerBulkUploadToDatabase<T>(List<T> dataList, string tableName)
        {
            try
            {
                if (dataList == null || !dataList.Any())
                    return false;

                // New up an instance
                var bulk = new BulkOperations();
                using (var con = CreateTrvwConnection())
                {
                    if (con.State != ConnectionState.Open)
                        con.Open();

                    bulk.Setup<T>(x => x.ForCollection(dataList))
                        .WithTable(tableName)
                        .AddAllColumns()
                        .BulkInsert();

                    using (var conBulk = new SqlConnection(con.ConnectionString))
                    {
                       await bulk.CommitTransactionAsync(conBulk);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("LedgerBulkUploadToDatabase: Exception: " + ex.ToString());
                return false;
            }
        }

        public string GetToDateFromConfig(int FinStartYear)
        {
            string fromDate = "01/04/" + FinStartYear.ToString();
            try
            {
                //To rollback, remove config value so it will take current date as earlier
                if (DateTime.TryParseExact(_config["DPCharges:ToDate"], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dtToDate))
                {
                    fromDate = DateTime.Parse(fromDate).ToString("dd/MM/yyyy");
                }
            }
            catch(Exception ex)
            {
                // Ignore and use default fromDate
                _logger.Error("GetToDateFromConfig: Exception: " + ex.ToString());
            }

            return fromDate;
        }
    }
}
