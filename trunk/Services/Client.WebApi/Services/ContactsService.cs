using Client.WebApi.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using SqlBulkTools;

namespace Client.WebApi.Services
{
    public interface IContactsService
    {
        Task<ResponseBaseModel> InsertContacts(InsertContactReq objleadsource);
    }
    public class ContactsService : BaseService, IContactsService
    {
        public async Task<ResponseBaseModel> InsertContacts(InsertContactReq objleadsource)
        {
            var objRBM = new ResponseBaseModel();
            List<Contacts> objContacts = new();
            objContacts = objleadsource.ContactJSON;
            objContacts = objContacts.GroupBy(x => x.MobileNo).Select(y => y.FirstOrDefault()).ToList();
            objContacts.RemoveAll(s => string.IsNullOrWhiteSpace(s.MobileNo));
            if (objContacts != null && objContacts.Count > 0)
            {
                using IDbConnection con = CreateJarvisConnection();
                var bulk = new BulkOperations();
                bulk.Setup<Contacts>(x => x.ForCollection(objContacts))
                .WithTable("tbl_Contacts")
                .AddColumn(x => x.ClientID)
                .AddColumn(x => x.Name)
                .AddColumn(x => x.MobileNo)
                .BulkInsertOrUpdate()
                .MatchTargetOn(x => x.MobileNo);
                await bulk.CommitTransactionAsync((SqlConnection)con);
            }
            return objRBM;
        }
    }
}
