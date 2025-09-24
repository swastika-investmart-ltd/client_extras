using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Components;
using Dapper;

namespace Client.WebApi.Services
{
    public interface ILedgerService
    {
        public Task GetLedgerData(LedgerInternalRequest request);
        public Task<ResponseBaseModel<PassbookData>> Ledger_GetDPCharges(LedgerInternalRequest request);
    }
    public class LedgerService : BaseService, ILedgerService
    {
        private readonly ILog _logger;
        public LedgerService(ILog logger)
        {
            _logger = logger;
        }

        public async Task<ResponseBaseModel<PassbookData>> Ledger_GetDPCharges(LedgerInternalRequest request)
        {
            var result = new ResponseBaseModel<PassbookData>()
            {
                Datas = new List<PassbookData>(),
            };

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
                        List<PassbookData> objPassbookData = new List<PassbookData>();
                        result.Datas = objPassbookData;

                        //Process DB records group by Voucher Date and Categories
                        var voucherDateList = mainLedgerList.Select(a => a.VOUCHERDATE).Distinct();

                        #region Date wise processing
                        foreach (DateTime voucherDateItem in voucherDateList)
                        {
                            PassbookData record = new PassbookData();
                            result.Datas.Add(record);

                            record.VoucherDate = voucherDateItem.ToString("D");                            

                            List<LedgerResponse> listFilteredByDate = mainLedgerList.Where(item => item.VOUCHERDATE == voucherDateItem).ToList();
                            if (listFilteredByDate != null && listFilteredByDate.Any())
                            {                                
                                //Process DB records group by Voucher Date and Categories
                                var categoryList = listFilteredByDate.Select(a => a.Category).Distinct();
                                foreach (var categoryItem in categoryList)
                                {
                                    //Category wise records
                                    List<LedgerResponse> listFilteredByCategory = listFilteredByDate.Where(item => item.Category == categoryItem).ToList();
                                    if (listFilteredByCategory != null && listFilteredByCategory.Any())
                                    {
                                        //Section 1
                                        Section1 objSection1 = new Section1()
                                        {
                                            Description = string.Empty,
                                            IsTransTypeCR = false,
                                        };

                                        record.Section1List.Add(objSection1);
                                        objSection1.TotalAmount = listFilteredByCategory.Sum(x => x.DpCharge); //DP Charegs Total

                                        switch (categoryItem)
                                        {
                                            case "DP": // DPChargeCategoryType.DP.ToString():
                                                objSection1.Id = (int)DPChargeCategoryType.DP;
                                                objSection1.TypeId = (int)SectionCategory.ViewMore;
                                                objSection1.ActionText = "View Details";
                                                objSection1.LabelText = "DP Charges";

                                                var subcategoryList = listFilteredByCategory.Select(a => a.Sub_Category).Distinct();
                                                foreach (var subcategoryItem in subcategoryList)
                                                {
                                                    //Sub Category wise records
                                                    List<LedgerResponse> listFilteredBySubCategory = listFilteredByCategory.Where(item => item.Sub_Category == subcategoryItem).ToList();

                                                    //Section 2
                                                    Section2 objSection2 = new Section2();
                                                    objSection1.Section2Item = objSection2;

                                                    objSection2.HeaderText = $"{categoryItem} for {voucherDateItem.ToString("D")}"; //DP Charges for 15 Sep 2025
                                                    objSection2.BodyText = string.Empty;

                                                    Section3 objSection3 = new Section3();
                                                    objSection2.Section3List.Add(objSection3);
                                                    objSection3.TotalAmount = listFilteredBySubCategory.Sum(x => x.DpCharge);


                                                    var groupbystocksList = listFilteredBySubCategory.Select(a => a.ScripName).Distinct();

                                                    var groupbystocks = listFilteredBySubCategory.GroupBy(r => r.ScripName)
                                                                    .Select(g => new
                                                                    {
                                                                        StockName = g.Key,
                                                                        Quantity = g.Sum(x => x.Qty),
                                                                        Amount = g.Sum(x => x.DpCharge)
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
                                                                objSection4.LabelText = $"{stockItem.StockName} (qty {stockItem.Quantity})";
                                                            }

                                                            break;

                                                        case "PLEDGE": // DPChargeSubCategoryType.PLEDGE.ToString():
                                                        case "UNPLEDGE": // DPChargeSubCategoryType.UNPLEDGE.ToString():
                                                            objSection3.LabelText = "Pledge/Unpledge Charges";
                                                            objSection3.InfoText = "Fee for pledging or unpledging shares.";

                                                            //var pledgesListStock = listFilteredBySubCategory
                                                            //                            .GroupBy(r => new { r.ScripName, r.Sub_Category })
                                                            //                            .Select(g => new
                                                            //                            {
                                                            //                                StockName = g.Key.ScripName,
                                                            //                                Quantity = g.Sum(x => x.Qty),
                                                            //                                Amount = g.Sum(x => x.DpCharge),
                                                            //                                SubCategory = g.Key.Sub_Category
                                                            //                            })
                                                            //                            .ToList();


                                                            foreach (var stockItem in groupbystocks)
                                                            {
                                                                //Section 4
                                                                Section4 objSection4 = new Section4();
                                                                objSection3.Section4List.Add(objSection4);
                                                                objSection4.Amount = stockItem.Amount;
                                                                objSection4.LabelText = $"{stockItem.StockName} (qty {stockItem.Quantity})";
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
                                                                objSection4.LabelText = $"{stockItem.StockName} (qty {stockItem.Quantity})";
                                                            }

                                                            break;

                                                        default:
                                                            objSection3.LabelText = "Unknown";                                                            
                                                            break;
                                                    }

                                                }
                                                break;
                                            case "DEMAT_SETUP":// DPChargeCategoryType.DEMAT_SETUP.ToString():
                                                objSection1.Id = (int)DPChargeCategoryType.DEMAT_SETUP;
                                                objSection1.TypeId = (int)SectionCategory.None;
                                                objSection1.LabelText = "Demat setup charges";
                                                break;
                                            case "OTHER":// DPChargeCategoryType.OTHER.ToString():
                                                objSection1.Id = (int)DPChargeCategoryType.OTHER;
                                                objSection1.TypeId = (int)SectionCategory.ViewMore;
                                                objSection1.ActionText = "View Details";
                                                objSection1.LabelText = "Other";
                                                break;
                                            default:
                                                break;
                                        }

                                        //result.Datas.Add(new LedgerResponse { VOUCHERDATE = voucherDateItem, LedgerResponse = listFiltered });
                                    }
                                }
                            }



                            

                            

                        }
                        #endregion
                        result.TotalRows = result.Datas.Count;

                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Ledger_GetDPCharges: Exception: " + ex.ToString());
                }
            }

            return result;
        }
        public async Task GetLedgerData(LedgerInternalRequest request)
        {
            using (IDbConnection con = CreateCapsfoConnection())
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                //EXEC FA_SMARTREPORT_LEDGER_DETAIL
                //    N'BSE_CASH'',''BSE_FNO'',''CD_BSE'',''CD_NSE'',''MCX'',''MF_BSE'',''MTF'',''NCDEX'',''NCL'',''NSE_CASH'',''NSE_COM'',''NSE_FNO',
                // 2025,
                // N'01/04/2025',
                // N'28-08-2025',
                // N'YNIE7530',
                // N'',
                // N'Y',
                // N'S',
                // N'Y'

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
                        var objBulk = new LedgerBulkUploadToSql<LedgerAPIResponse>();
                        objBulk.InternalStore = LedgerList;
                        objBulk.CommitBatchSize = 1000;
                        objBulk.ConnectionString = CreateTrvwConnection().ConnectionString;
                        objBulk.TableName = "dbo.LedgerAPIData";
                        await objBulk.LedgerCommit();

                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }


    public class LedgerBulkUploadToSql<T>
    {
        public IList<T> InternalStore { get; set; }
        public string TableName { get; set; }
        public int CommitBatchSize { get; set; } = 1000;
        public string ConnectionString { get; set; }

        public async Task LedgerCommit()
        {
            if (InternalStore.Count > 0)
            {
                DataTable dt;
                int numberOfPages = (InternalStore.Count / CommitBatchSize) + (InternalStore.Count % CommitBatchSize == 0 ? 0 : 1);
                for (int pageIndex = 0; pageIndex < numberOfPages; pageIndex++)
                {
                    dt = InternalStore.Skip(pageIndex * CommitBatchSize).Take(CommitBatchSize).LedgerToDataTable();
                    await LedgerBulkInsert(dt);
                }
            }
        }

        public async Task LedgerBulkInsert(DataTable dt)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                // make sure to enable triggers
                // more on triggers in next post
                SqlBulkCopy bulkCopy =
                    new SqlBulkCopy
                    (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );

                ////ADD COLUMN MAPPING
                //foreach (DataColumn col in dt.Columns)
                //{
                //    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                //}

                bulkCopy.ColumnMappings.Add("COCD", "COCD");
                bulkCopy.ColumnMappings.Add("CONAME", "CONAME");
                bulkCopy.ColumnMappings.Add("KINDOFACCOUNT", "KINDOFACCOUNT");
                bulkCopy.ColumnMappings.Add("ACCOUNTCODE", "ACCOUNTCODE");
                bulkCopy.ColumnMappings.Add("ACCOUNTNAME", "ACCOUNTNAME");
                bulkCopy.ColumnMappings.Add("TELNO", "TELNO");
                bulkCopy.ColumnMappings.Add("FAX", "FAX");
                bulkCopy.ColumnMappings.Add("ADDR", "ADDR");
                bulkCopy.ColumnMappings.Add("OPENINGBALANCE", "OPENINGBALANCE");
                bulkCopy.ColumnMappings.Add("DR_AMT", "DR_AMT");
                bulkCopy.ColumnMappings.Add("CR_AMT", "CR_AMT");
                bulkCopy.ColumnMappings.Add("VOUCHERDATE", "VOUCHERDATE");
                bulkCopy.ColumnMappings.Add("SETTLEMENT_NO", "SETTLEMENT_NO");
                bulkCopy.ColumnMappings.Add("CTRCODE", "CTRCODE");
                bulkCopy.ColumnMappings.Add("CTRNAME", "CTRNAME");
                bulkCopy.ColumnMappings.Add("TRANS_TYPE", "TRANS_TYPE");
                bulkCopy.ColumnMappings.Add("VOUCHERNO", "VOUCHERNO");
                bulkCopy.ColumnMappings.Add("NARRATION", "NARRATION");
                bulkCopy.ColumnMappings.Add("BILLNO", "BILLNO");
                bulkCopy.ColumnMappings.Add("CHQNO", "CHQNO");
                bulkCopy.ColumnMappings.Add("EXPECTED_DATE", "EXPECTED_DATE");
                bulkCopy.ColumnMappings.Add("TRADING_COCD", "TRADING_COCD");
                bulkCopy.ColumnMappings.Add("PANNO", "PANNO");
                bulkCopy.ColumnMappings.Add("EMAIL", "EMAIL");
                bulkCopy.ColumnMappings.Add("MANUALVNO", "MANUALVNO");
                bulkCopy.ColumnMappings.Add("BOOKTYPECODE", "BOOKTYPECODE");
                bulkCopy.ColumnMappings.Add("BILL_DATE", "BILL_DATE");
                bulkCopy.ColumnMappings.Add("MKT_TYPE", "MKT_TYPE");
                bulkCopy.ColumnMappings.Add("GROUPCODE", "GROUPCODE");
                bulkCopy.ColumnMappings.Add("BRSFLAG", "BRSFLAG");
                bulkCopy.ColumnMappings.Add("SETL_PAYINDATE", "SETL_PAYINDATE");
                bulkCopy.ColumnMappings.Add("LAST2SETL", "LAST2SETL");
                bulkCopy.ColumnMappings.Add("ACCOUNTCODE1", "ACCOUNTCODE1");
                bulkCopy.ColumnMappings.Add("GATEWAYID", "GATEWAYID");
                bulkCopy.ColumnMappings.Add("PUNCH_TIME", "PUNCH_TIME");
                bulkCopy.ColumnMappings.Add("voctype", "voctype");
                bulkCopy.ColumnMappings.Add("CHQIMAGEPATH", "CHQIMAGEPATH");
                bulkCopy.ColumnMappings.Add("TRANS_TYPE1", "TRANS_TYPE1");
                bulkCopy.ColumnMappings.Add("FromDate", "FromDate");
                bulkCopy.ColumnMappings.Add("ToDate", "ToDate");
                bulkCopy.ColumnMappings.Add("STARTYEAR", "STARTYEAR");
                bulkCopy.ColumnMappings.Add("CreatedDate", "CreatedDate");
                // set the destination table name
                bulkCopy.DestinationTableName = TableName;
                //Set the timeout.
                bulkCopy.BulkCopyTimeout = 900;
                connection.Open();

                // write the data in the "dataTable"
                //bulkCopy.WriteToServer(dt);
                await bulkCopy.WriteToServerAsync(dt);
                connection.Close();
            }
            // reset
            //this.dataTable.Clear();
        }
    }

    public static class LedgerBulkUploadToSqlHelper
    {
        public static DataTable LedgerToDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
