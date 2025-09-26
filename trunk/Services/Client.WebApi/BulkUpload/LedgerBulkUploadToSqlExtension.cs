using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Client.WebApi
{
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
}
