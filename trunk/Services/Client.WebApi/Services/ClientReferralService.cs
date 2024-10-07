using Dapper;
using Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using System.Data.SqlClient;
using Components;
using System.Net.Mail;
using System.Net.Mime;
using NLog;
using Client.Models.WebApi;


namespace Client.WebApi.Services
{
    public interface IClientReferralService
    {
        Task<ResponseBaseModelClient<ClientReferralResponse, TopTwoEarners>> ClientReferralDetailsByClientCode(ClientReferral objClient);
        Task<ResponseBaseModelClientReferral<TopTwoEarners>> ClientReferralDetailsByClientCodeV1(ClientReferral objClient);
        Task<ResponseBaseModelLeadReferral<ClientReferralResponse>> LeadReferralDetailsByClientCode(LeadReferralRequest objClient);
    }

    public class ClientReferralService : IClientReferralService
    {
        private readonly SqlConnection _dbConnection;
        private IConfiguration _config;
        private static readonly HttpClient client = new HttpClient();
        private readonly IUserService _userService;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ILog _logger;
        public ClientReferralService(SqlConnection dbConnection, IUserService userService, IConfiguration config, IWebHostEnvironment hostingEnvironment, ILog logger)
        {
            _dbConnection = dbConnection;
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _config = config;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }


        public async Task<ResponseBaseModelClient<ClientReferralResponse, TopTwoEarners>> ClientReferralDetailsByClientCode(ClientReferral objClient)
        {
            var param = new DynamicParameters();

            DateTime StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 2, 1);
            DateTime EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

            param.Add("@ClientId", objClient.ClientId);
            param.Add("@StartDate", StartDate);
            param.Add("@EndDate", EndDate);
            param.Add("@Search", "Filter");
            param.Add("@TotalReferralAmtEarned", dbType: DbType.Decimal, direction: ParameterDirection.Output, size: 50);
            param.Add("@TotalClientReferred", dbType: DbType.Int64, direction: ParameterDirection.Output, size: 50);

            var dbResult = (await SqlMapper.QueryMultipleAsync(_dbConnection, "Client_Referral_Details_ClientCode", param, commandType: CommandType.StoredProcedure, commandTimeout: 180));

            var result = new ResponseBaseModelClient<ClientReferralResponse, TopTwoEarners>();
            result.ReferralDetails = null;
            result.TopTwoEarners = null;
            result.TotalReferredClient = 0;
            result.ReferralPercentage = 10;
            result.CurrentStage = "Bronze";
            result.YoutubeUrl = _config["ClientReferralDashboard:YoutubeUrl"];
            result.TotalReferralAmtEarned = 0;
            return result;
        }


        public async Task<ResponseBaseModelClientReferral<TopTwoEarners>> ClientReferralDetailsByClientCodeV1(ClientReferral objClient)
        {
            if (BrokerageDataStore.Reference.totalReferredClient.Count == 0)
                await LoadBrokerageDataInfo();

            var result = new ResponseBaseModelClientReferral<TopTwoEarners>();
            result.TopTwoEarners = BrokerageDataStore.Reference.topTwoEarners;
            result.ReferralPercentage = 10;
            result.CurrentStage = "Bronze";
            result.YoutubeUrl = _config["ClientReferralDashboard:YoutubeUrl"];
            result.IsDirect = BrokerageDataStore.Reference.directIndirectClients.Where(x => x.ClientCode == objClient.ClientId).Select(x => x.IsDirect).FirstOrDefault();
            result.TotalReferredClient = BrokerageDataStore.Reference.threeMonthDictionary.ContainsKey(objClient.ClientId) ? BrokerageDataStore.Reference.totalReferredClient[objClient.ClientId] : 0;

            if (objClient.DateFilter == "LastMonth")
                result.TotalReferralAmtEarned = BrokerageDataStore.Reference.lastMonthDictionary.ContainsKey(objClient.ClientId) ? BrokerageDataStore.Reference.lastMonthDictionary[objClient.ClientId] : 0;
            else if (objClient.DateFilter == "ThisMonth")
                result.TotalReferralAmtEarned = BrokerageDataStore.Reference.thisMonthDictionary.ContainsKey(objClient.ClientId) ? BrokerageDataStore.Reference.thisMonthDictionary[objClient.ClientId] : 0;
            else
                result.TotalReferralAmtEarned = BrokerageDataStore.Reference.threeMonthDictionary.ContainsKey(objClient.ClientId) ? BrokerageDataStore.Reference.threeMonthDictionary[objClient.ClientId] : 0;

            return result;
        }

        public async Task<ResponseBaseModelLeadReferral<ClientReferralResponse>> LeadReferralDetailsByClientCode(LeadReferralRequest objClient)
        {
            var param = new DynamicParameters();
            param.Add("@ClientId", objClient.ClientId);
            param.Add("@Search", objClient.SearchValue);
            param.Add("@PageNo", objClient.PageNo);
            param.Add("@PageSize", objClient.PageSize);
            param.Add("@TotalClientReferred", dbType: DbType.Int64, direction: ParameterDirection.Output, size: 50);
            param.Add("@TotalRows", dbType: DbType.Int64, direction: ParameterDirection.Output, size: 50);

            var dbResult = (await SqlMapper.QueryMultipleAsync(_dbConnection, "Client_Referral_Details_ClientCodeV1", param, commandType: CommandType.StoredProcedure, commandTimeout: 180));
            var result = new ResponseBaseModelLeadReferral<ClientReferralResponse>();
            result.ReferralDetails = dbResult.Read<ClientReferralResponse>().ToList();
            result.TotalReferredClient = param.Get<long>("TotalClientReferred");
            result.TotalRows = param.Get<long>("@TotalRows");
            return result;
        }

        public async Task<ResponseBaseModel<BrokerageDetails>> LoadReferralData(DateTime StartDate, DateTime EndDate)
        {
            var param = new DynamicParameters();
            param.Add("@StartDate", StartDate);
            param.Add("@EndDate", EndDate);
            var result = new ResponseBaseModel<BrokerageDetails>
            {
                Datas = (await SqlMapper.QueryAsync<BrokerageDetails>(_dbConnection, "Get_Referral_brokerage_Data", param: param, commandType: CommandType.StoredProcedure, commandTimeout: 180)).ToList(),
            };
            result.TotalRows = result.Datas.Count;
            return result;
        }

        public async Task<bool> LoadBrokerageDataInfo()
        {
            try
            {
                var today = DateTime.Today;
                var TodaysDate = new DateTime(today.Year, today.Month, 1);

                DateTime StartDate, EndDate;
                int yearCurr = DateTime.Now.Year;
                int monthCurr = DateTime.Now.Month;

                StartDate = TodaysDate.AddMonths(-2);
                EndDate = new DateTime(yearCurr, monthCurr, DateTime.DaysInMonth(yearCurr, monthCurr));
                var param = new DynamicParameters();

                param.Add("@StartDate", StartDate);
                param.Add("@EndDate", EndDate);

                var result = (await SqlMapper.QueryAsync<BrokerageDetails>(_dbConnection, "Get_Referral_brokerage_Data", param: param, commandType: CommandType.StoredProcedure, commandTimeout: 180)).ToList();

                if (result != null && result.Any())
                {
                    BrokerageDataStore.Reference.totalReferredClient = result
                        .Select(x => new { x.ClientCode, x.ReferredBy })
                        .Distinct().Where(x => x.ReferredBy != null)
                        .GroupBy(u => u.ReferredBy)
                        .ToDictionary(group => group.Key, group => group.ToList().Count);

                    BrokerageDataStore.Reference.threeMonthDictionary = result
                        .Where(x => x.ReferredBy != null)
                        .GroupBy(u => u.ReferredBy)
                        .ToDictionary(
                            group => group.Key,
                            group => Math.Round(
                                group.Where(c => c.TradeDate >= StartDate.AddMonths(-3) && c.TradeDate <= EndDate)
                                    .Sum(c => c.brokerage / 10),
                                0
                            )
                        );

                    //Last Month
                    StartDate = TodaysDate.AddMonths(-1);
                    EndDate = TodaysDate.AddDays(-1);

                    BrokerageDataStore.Reference.lastMonthDictionary = result
                        .Where(x => x.ReferredBy != null)
                        .GroupBy(u => u.ReferredBy)
                        .ToDictionary(
                            group => group.Key,
                            group => Math.Round(
                                group.Where(c => c.TradeDate >= StartDate && c.TradeDate <= EndDate)
                                    .Sum(c => c.brokerage / 10),
                                0
                            )
                        );


                    StartDate = new DateTime(yearCurr, monthCurr, 1);
                    EndDate = new DateTime(yearCurr, monthCurr, DateTime.DaysInMonth(yearCurr, monthCurr));

                    BrokerageDataStore.Reference.thisMonthDictionary = result
                        .Where(x => x.ReferredBy != null)
                        .GroupBy(u => u.ReferredBy)
                        .ToDictionary(
                            group => group.Key,
                            group => Math.Round(
                                group.Where(c => c.TradeDate >= StartDate && c.TradeDate <= EndDate)
                                    .Sum(c => c.brokerage / 10),
                                0
                            )
                        );
                    BrokerageDataStore.Reference.directIndirectClients = (await SqlMapper.QueryAsync<DirectCustomers>(_dbConnection, "GetDirectIndirectClients", commandType: CommandType.StoredProcedure, commandTimeout: 180)).ToList();
                    
                    BrokerageDataStore.Reference.topTwoEarners = result
                        .Where(x => x.ReferredBy != null)
                        .GroupBy(x => new { x.ReferredBy })
                        .Select(group => new TopTwoEarners
                        {
                            ClientName = BrokerageDataStore.Reference.directIndirectClients.
                                Where(x => x.ClientCode == group.Key.ReferredBy).
                                Select(x => x.ClientName).FirstOrDefault(),
                            AmountEarnered = Math.Round(group.Sum(hit => hit.brokerage / 10), 0)
                        })
                        .OrderByDescending(hit => hit.AmountEarnered)
                        .Take(2)
                        .ToList();

                }
            }
            catch (Exception ex)
            {
                _logger.Debug("LoadBrkDataInfo Error: " + ex.ToString());
                return false;
            }
            return true;
        }


        public Tuple<DateTime, DateTime> GetDates(string DateFilter = "")
        {
            int yearCurr = DateTime.Now.Year;
            int monthCurr = DateTime.Now.Month;
            DateTime StartDate, EndDate;


            if (DateFilter == "LastMonth")
            {
                StartDate = new DateTime(yearCurr, monthCurr - 1, 1);
                EndDate = new DateTime(yearCurr, monthCurr - 1, DateTime.DaysInMonth(yearCurr, monthCurr - 1));
            }
            else if (DateFilter == "ThisMonth")
            {
                StartDate = new DateTime(yearCurr, monthCurr, 1);
                EndDate = new DateTime(yearCurr, monthCurr, DateTime.DaysInMonth(yearCurr, monthCurr));
            }
            else
            {
                StartDate = new DateTime(yearCurr, monthCurr - 2, 1);
                EndDate = new DateTime(yearCurr, monthCurr, DateTime.DaysInMonth(yearCurr, monthCurr));
            }
            return Tuple.Create(StartDate, EndDate);
        }

    }
}
