using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using System;
using Components;

namespace Client.WebApi
{
    public interface IWealthBagService
    {
        Task<bool> SavePortfolioDataInMemory();
        Task<ResponseBaseModelWb<PortfolioData>> GetWealthBagDataByClientId(WBDataByUidReq obj);
    }
    public class WealthBagService : BaseService, IWealthBagService
    {
        private readonly ILog _logger;
        public WealthBagService(ILog log)
        {
            _logger = log;
        }
        public async Task<bool> SavePortfolioDataInMemory()
        {
            try
            {
                var wealthBagStocks = new List<WealthBagStocks>();
                using (IDbConnection con = CreateJarvisConnection())
                {
                    if (con.State != ConnectionState.Open)
                        con.Open();

                    wealthBagStocks = (await SqlMapper.QueryAsync<WealthBagStocks>(con, "GetAllClientsWealthBagData", null, commandType: CommandType.StoredProcedure)).ToList();
                }

                // Check for null and initialize as empty if necessary
                if (wealthBagStocks == null || !wealthBagStocks.Any())
                {
                    return false;
                }

                var portfolioDictionary = new Dictionary<string, List<PortfolioDetails>>();

                foreach (var portfolio in wealthBagStocks)
                {
                    if (!portfolioDictionary.ContainsKey(portfolio.ClientCode))
                    {
                        portfolioDictionary[portfolio.ClientCode] = new List<PortfolioDetails>();
                    }

                    portfolioDictionary[portfolio.ClientCode].Add(new PortfolioDetails
                    {
                        CompanyName = portfolio.CompanyName,
                        Quantity = portfolio.Quantity,
                        Symbol = portfolio.Symbol,
                        PortfolioName = portfolio.PortfolioName
                    });
                }
                WBPortfolioDataStore.Reference.PortfolioDictionary = portfolioDictionary;
                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, " SavePortfolioDataInMemory: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());
                return false;
            }

        }
        public async Task<ResponseBaseModelWb<PortfolioData>> GetWealthBagDataByClientId(WBDataByUidReq obj)
        {
            var result = new ResponseBaseModelWb<PortfolioData>
            {
                Datas = new PortfolioData() // Initialize Datas here to avoid null issues
            };

            // Check if the portfolio dictionary is null or doesn't contain the UID
            if (WBPortfolioDataStore.Reference.PortfolioDictionary == null || !WBPortfolioDataStore.Reference.PortfolioDictionary.TryGetValue(obj.Uid, out var portfolioDetailsList))
            {
                return result; // Return with empty Datas if no data found
            }

            // Populate the result with found data
            result.Datas.wealthBagStocks = portfolioDetailsList;
            result.Datas.totalQuantity = portfolioDetailsList.Sum(a => a.Quantity);

            return result;
        }
    }
}
