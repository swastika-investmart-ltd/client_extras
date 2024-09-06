using Client.WebApi.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using static Client.WebApi.Models.WealthbagPortfolio;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Components;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace Client.WebApi.Services
{
    public interface IWealthBagPortfolioService
    {
        Task<bool> SavePortfolioDataInMemory();
        Task<ResponseBaseModelWb<PortfolioData>> GetWealthBagDataByClientId(string clientId);
    }
    public class WealthBagPortfolioService : IWealthBagPortfolioService
    {
        private IConfiguration _config;
        private readonly ILog _logger;

        private readonly SqlConnection _dbConnection;

        public WealthBagPortfolioService(SqlConnection dbConnection, IConfiguration config, ILog log)
        {
            _dbConnection = dbConnection;
            _config = config;
            _logger = log;
        }

        public async Task<ResponseBaseModelWb<PortfolioData>> GetWealthBagDataByClientId(string clientId)
        {
            ResponseBaseModelWb<PortfolioData> result = new ResponseBaseModelWb<PortfolioData>();
            if (WBPortfolioDataStore.Reference.PortfolioDictionary != null)
            {
                try
                {
                    if (WBPortfolioDataStore.Reference.PortfolioDictionary.TryGetValue(clientId, out var portfolioDetailsList))
                    { 
                        result.Datas = new PortfolioData();
                        result.Datas.wealthBagStocks = portfolioDetailsList; 
                        result.Datas.totalQuantity = portfolioDetailsList.Sum(a => a.Quantity);
                        return result;
                    } 
                }
                catch (Exception ex)
                {
                    _logger.Log(NLog.LogLevel.Error, " GetWealthBagDataByClientId: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());
                }
            }
            return result;
        }

        public async Task<bool> SavePortfolioDataInMemory()
        {
            try
            {
                using (var connection = new SqlConnection(this._config.GetConnectionString("sqlconnection")))
                {
                    var wealthBagStocks = (await SqlMapper.QueryAsync<WealthBagStocks>(connection, "GetAllClientsWealthBagData", null, commandType: CommandType.StoredProcedure)).ToList();

                    var portfoliodictionary = new Dictionary<string, List<PortfolioDetails>>();

                    if (wealthBagStocks != null && wealthBagStocks.Count() > 0)
                    { 
                        foreach (var portfolio in wealthBagStocks)
                        {
                            if (!portfoliodictionary.ContainsKey(portfolio.ClientCode))
                            {
                                portfoliodictionary[portfolio.ClientCode] = new List<PortfolioDetails>();
                            }

                            portfoliodictionary[portfolio.ClientCode].Add(new PortfolioDetails
                            {
                                CompanyName = portfolio.CompanyName,
                                Quantity = portfolio.Quantity,
                                Symbol = portfolio.Symbol,
                                PortfolioName = portfolio.PortfolioName
                            });
                        }

                        WBPortfolioDataStore.Reference.PortfolioDictionary = portfoliodictionary;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, " SavePortfolioDataInMemory: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());
                return false;
            }
        }
    }
}
