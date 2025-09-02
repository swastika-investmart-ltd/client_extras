using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ResearchPanel.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Client.WebApi.Services
{
    public interface IRPTradingoService
    {
        Task<ResponseBaseModel<ScripGeneralResponse>> GetScripGeneral(long CompanyId);
        Task<ResponseBaseModel<ScripOffersResponse>> GetScripOffers(long CompanyId);
        Task<ResponseBaseModel<ScripOrderFollowUpResponse>> GetScripOrderFollowup(long OrderId);
        Task<ResponseBaseModel<AllScripInfoResponse>> GetAllScripInfo(string ClientId, long CompanyId);
        Task<ResponseBaseModel<AllScripInfoResponse>> GetAllScripInfoWithPagination(string ClientId, long PageNo, long CompanyId);
        Task<ResponseBaseRecModel<ScripOrderbySegmentsRes>> GetScripOrderbySegments(ScripOrderbySegmentsReq obj);
        Task<ResponseBaseModel<ViewRecPercentageInfo>> ViewRecommendationPercentage();
        Task<ResponseBaseModel<RecommendationPercentageInfo>> GetRecommendationPercentage();
        Task GetTopRecommendationListFromDatabase(string strSegment);
        Task<List<ScripOrderbySegmentsRes>> GetShortTermRecomFromDb();
        Task<List<ScripOrderbySegmentsRes>> GetLongTermRecomFromDb();
        Task<bool> GetAllSegmentsData();
        Task<ResponseBaseCallRecModel<GraphData, WebCallRecommendation>> GetWebCallRecommendation(OrderbySegmentsReq obj);
        Task<ResponseBaseMobCallRecModel<GraphData, MobCallRecommendation, ClosedData>> GetMobCallRecommendation(OrderbySegmentsReq obj);
    }
    public class RPTradingoService : BaseService, IRPTradingoService
    {
        private readonly IReportsService _reportsService;
        private readonly HashSet<string> _recoKeys;
        private readonly CacheManager<ScripOrderbySegmentsRes> _cacheManager;
        public RPTradingoService(IReportsService reportsService, IOptionsSnapshot<PathOptions> optionsPaths, IMemoryCache memoryCache)
        {
            _reportsService = reportsService;
            _recoKeys = optionsPaths.Get("Recomkeys").Paths;
            _cacheManager = new CacheManager<ScripOrderbySegmentsRes>(memoryCache);
        }

        public async Task<ResponseBaseModel<ScripGeneralResponse>> GetScripGeneral(long CompanyId)
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var param = new DynamicParameters();
                param.Add("@CompanyId", CompanyId);
                var result = new ResponseBaseModel<ScripGeneralResponse>();
                result.Datas = (await SqlMapper.QueryAsync<ScripGeneralResponse>(con, "RP_GetScripGeneral", param, commandType: CommandType.StoredProcedure)).ToList();
                result.TotalRows = result.Datas.Count;
                return result;
            }
        }
        public async Task<ResponseBaseModel<ScripOffersResponse>> GetScripOffers(long CompanyId)
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var param = new DynamicParameters();
                param.Add("@CompanyId", CompanyId);
                var result = new ResponseBaseModel<ScripOffersResponse>();
                result.Datas = (await SqlMapper.QueryAsync<ScripOffersResponse>(con, "RP_GetScripOffers", param, commandType: CommandType.StoredProcedure)).ToList();
                result.TotalRows = result.Datas.Count;
                return result;
            }
        }
        public async Task<ResponseBaseModel<ScripOrderFollowUpResponse>> GetScripOrderFollowup(long OrderId)
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var param = new DynamicParameters();
                param.Add("@OrderId", OrderId);
                //param.Add("@TotalRows", dbType: DbType.Int64, direction: ParameterDirection.Output, size: 1000);
                var result = new ResponseBaseModel<ScripOrderFollowUpResponse>();
                result.Datas = (await SqlMapper.QueryAsync<ScripOrderFollowUpResponse>(con, "RP_GetScripOrderFollowup", param, commandType: CommandType.StoredProcedure)).ToList();
                result.TotalRows = result.Datas.Count;
                //param.Get<long>("TotalRows");
                return result;
            }
        }
        public async Task<ResponseBaseModel<AllScripInfoResponse>> GetAllScripInfo(string ClientId, long CompanyId)
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var param = new DynamicParameters();
                param.Add("@ClientId", ClientId);
                param.Add("@CompanyId", CompanyId);
                var dbResult = (await SqlMapper.QueryMultipleAsync(con, "RP_GetAllActiveScripDetails", param, commandType: CommandType.StoredProcedure));

                var ScripOrder = dbResult.Read<ScripOrderResponse>().ToList();
                var ScripOffers = dbResult.Read<ScripOffersResponse>().ToList();
                var ScripGeneral = dbResult.Read<ScripGeneralResponse>().ToList();
                var ScripOrderFollowUp = dbResult.Read<ScripWithOrderFollowUpResponse>().ToList();

                var listResponseData = new List<AllScripInfoResponse>();
                var listOrderData = new List<AllScripInfoResponse>();
                if (ScripOrder != null && ScripOrder.Any())
                    listOrderData = ScripOrder.ConvertAll(x => new AllScripInfoResponse
                    {
                        OrderId = x.OrderId,
                        ScripSymbol = x.ScripSymbol,
                        ScripToken = x.ScripToken,
                        IntradaybtstDelivery = x.IntradaybtstDelivery,
                        BuySell = x.BuySell,
                        PriceRangeFrom = x.PriceRangeFrom,
                        PriceRangeTo = x.PriceRangeTo,
                        Target = x.Target,
                        StopLoss = x.StopLoss,
                        Duration = x.Duration,
                        DurationType = x.DurationType,
                        CreatedOn = x.CreatedOn,
                        MessageType = x.MessageType,
                        Message = x.Message,
                        ScripOption = x.ScripOption,
                        CashToken = x.CashToken,
                        StrikePrice = x.StrikePrice,
                        CallOrPut = x.CallOrPut,
                        ExpiryDate = x.ExpiryDate,
                        CompanyName = x.CompanyName,
                        InstrumentName = x.InstrumentName,
                        SegmentName = x.SegmentName,
                        ExchangeName = x.ExchangeName,
                        IndustryType = x.IndustryType,
                        Status = x.Status,
                        IsRead = x.IsRead,
                        FilePath = x.FilePath,
                        CompanyId = x.CompanyId,
                        ResearchType = "Order",
                        KBContract = x.KBContract
                    });

                if (listOrderData != null && listOrderData.Any())
                    listResponseData.AddRange(listOrderData);

                //For Offers
                var listOffersData = new List<AllScripInfoResponse>();
                if (ScripOffers != null && ScripOffers.Any())
                    listOffersData = ScripOffers.ConvertAll(x => new AllScripInfoResponse
                    {
                        OfferId = x.OfferId,
                        Heading = x.Heading,
                        OfferDetailes = x.OfferDetailes,
                        ButtonText = x.ButtonText,
                        ButtonHyperlink = x.ButtonHyperlink,
                        DurationFrom = x.DurationFrom,
                        DurationTo = x.DurationTo,
                        CreatedOn = x.CreatedOn,
                        Status = x.Status,
                        IsRead = x.IsRead,
                        ReferStatus = x.ReferStatus,
                        CompanyId = x.CompanyId,
                        ResearchType = "Offers"
                    });

                if (listOffersData != null && listOffersData.Any())
                    listResponseData.AddRange(listOffersData);

                //For General
                var listGeneralData = new List<AllScripInfoResponse>();
                if (ScripGeneral != null && ScripGeneral.Any())
                    listGeneralData = ScripGeneral.ConvertAll(x => new AllScripInfoResponse
                    {
                        GeneralId = x.GeneralId,
                        Equity = x.Equity,
                        FutureOptions = x.FutureOptions,
                        Currencies = x.Currencies,
                        Commodities = x.Commodities,
                        Subject = x.Subject,
                        Message = x.Message,
                        CreatedOn = x.CreatedOn,
                        Status = x.Status,
                        IsRead = x.IsRead,
                        CompanyId = x.CompanyId,
                        ResearchType = "General"
                    });

                if (listGeneralData != null && listGeneralData.Any())
                    listResponseData.AddRange(listGeneralData);

                //For Scrip Order FollowUp 
                var listFollowUpData = new List<AllScripInfoResponse>();
                if (ScripOrderFollowUp != null && ScripOrderFollowUp.Any())
                    listFollowUpData = ScripOrderFollowUp.ConvertAll(x => new AllScripInfoResponse
                    {

                        OrderId = x.OrderId,
                        ScripSymbol = x.ScripSymbol,
                        ScripToken = x.ScripToken,
                        IntradaybtstDelivery = x.IntradaybtstDelivery,
                        BuySell = x.BuySell,
                        PriceRangeFrom = x.PriceRangeFrom,
                        PriceRangeTo = x.PriceRangeTo,
                        Target = x.Target,
                        StopLoss = x.StopLoss,
                        Duration = x.Duration,
                        DurationType = x.DurationType,
                        CreatedOn = x.CreatedOn,
                        MessageType = x.MessageType,
                        Message = x.Message,
                        ScripOption = x.ScripOption,
                        CashToken = x.CashToken,
                        StrikePrice = x.StrikePrice,
                        CallOrPut = x.CallOrPut,
                        ExpiryDate = x.ExpiryDate,
                        CompanyName = x.CompanyName,
                        InstrumentName = x.InstrumentName,
                        SegmentName = x.SegmentName,
                        ExchangeName = x.ExchangeName,
                        IndustryType = x.IndustryType,
                        Status = x.Status,
                        FollowupId = x.FollowupId,
                        FollowupMessage = x.FollowupMessage,
                        FollowupCreatedOn = x.FollowupCreatedOn,
                        IsRead = x.IsRead,
                        IsButtonDisplayed = x.IsButtonDisplayed,
                        ButtonBuySell = x.ButtonBuySell,
                        FilePath = x.FilePath,
                        CompanyId = x.CompanyId,
                        ResearchType = "FollowUp",
                        KBContract = x.KBContract
                    });

                if (listFollowUpData != null && listFollowUpData.Any())
                    listResponseData.AddRange(listFollowUpData);

                ////Return Response
                var result = new ResponseBaseModel<AllScripInfoResponse>();
                if (listResponseData != null && listResponseData.Any())
                    result.Datas = listResponseData.OrderByDescending(x => x.CreatedOn).ToList();
                else
                    result.Datas = new List<AllScripInfoResponse>();
                result.TotalRows = (listFollowUpData != null && listFollowUpData.Any()) ? Convert.ToInt64(result.Datas.Count) : 0;

                return result;
            }
        }
        public async Task<ResponseBaseModel<AllScripInfoResponse>> GetAllScripInfoWithPagination(string ClientId, long PageNo, long CompanyId)
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var param = new DynamicParameters();
                param.Add("@ClientId", ClientId);
                param.Add("@PageNo", PageNo);

                param.Add("@CompanyId", CompanyId);
                //param.Add("@TotalRows", dbType: DbType.Int64, direction: ParameterDirection.Output, size: 1000);
                var dbResult = (await SqlMapper.QueryMultipleAsync(con, "RP_GetAllActiveScripDetailsWithPagination", param, commandType: CommandType.StoredProcedure));
                //var TotalRows = param.Get<long>("TotalRows");

                var ScripOrder = dbResult.Read<ScripOrderResponse>().ToList();
                var ScripOffers = dbResult.Read<ScripOffersResponse>().ToList();
                //var ScripIpo = dbResult.Read<ScripIpoResponse>().ToList();
                var ScripGeneral = dbResult.Read<ScripGeneralResponse>().ToList();
                var ScripOrderFollowUp = dbResult.Read<ScripWithOrderFollowUpResponse>().ToList();
                var TotalRows = dbResult.Read<long>().FirstOrDefault();
                //var listData = new List<AllScripInfoResponse>()
                //    {
                //       new AllScripInfoResponse() { Type = "Order", Data = ScripOrder },
                //       new AllScripInfoResponse() { Type = "Offers", Data = ScripOffers },
                //       new AllScripInfoResponse() { Type = "Ipo", Data = ScripIpo },
                //       new AllScripInfoResponse() { Type = "General", Data = ScripGeneral },
                //    };

                var listResponseData = new List<AllScripInfoResponse>();

                var listOrderData = new List<AllScripInfoResponse>();
                if (ScripOrder != null && ScripOrder.Any())
                    listOrderData = ScripOrder.ConvertAll(x => new AllScripInfoResponse
                    {
                        OrderId = x.OrderId,
                        ScripSymbol = x.ScripSymbol,
                        ScripToken = x.ScripToken,
                        IntradaybtstDelivery = x.IntradaybtstDelivery,
                        BuySell = x.BuySell,
                        PriceRangeFrom = x.PriceRangeFrom,
                        PriceRangeTo = x.PriceRangeTo,
                        Target = x.Target,
                        StopLoss = x.StopLoss,
                        Duration = x.Duration,
                        DurationType = x.DurationType,
                        CreatedOn = x.CreatedOn,
                        MessageType = x.MessageType,
                        Message = x.Message,
                        ScripOption = x.ScripOption,
                        CashToken = x.CashToken,
                        StrikePrice = x.StrikePrice,
                        CallOrPut = x.CallOrPut,
                        ExpiryDate = x.ExpiryDate,
                        CompanyName = x.CompanyName,
                        InstrumentName = x.InstrumentName,
                        SegmentName = x.SegmentName,
                        ExchangeName = x.ExchangeName,
                        IndustryType = x.IndustryType,
                        Status = x.Status,
                        IsRead = x.IsRead,
                        FilePath = x.FilePath,
                        CompanyId = x.CompanyId,
                        ResearchType = "Order",
                        ShortDate = x.CreatedOn,
                        KBContract = x.KBContract
                    });

                if (listOrderData != null && listOrderData.Any())
                    listResponseData.AddRange(listOrderData);

                //For Offers
                var listOffersData = new List<AllScripInfoResponse>();
                if (ScripOffers != null && ScripOffers.Any())
                    listOffersData = ScripOffers.ConvertAll(x => new AllScripInfoResponse
                    {
                        OfferId = x.OfferId,
                        Heading = x.Heading,
                        OfferDetailes = x.OfferDetailes,
                        ButtonText = x.ButtonText,
                        ButtonHyperlink = x.ButtonHyperlink,
                        DurationFrom = x.DurationFrom,
                        DurationTo = x.DurationTo,
                        CreatedOn = x.CreatedOn,
                        Status = x.Status,
                        IsRead = x.IsRead,
                        ReferStatus = x.ReferStatus,
                        CompanyId = x.CompanyId,
                        ResearchType = "Offers",
                        ShortDate = x.CreatedOn
                    });

                if (listOffersData != null && listOffersData.Any())
                    listResponseData.AddRange(listOffersData);

                ////For Ipo
                //var listIpoData = new List<AllScripInfoResponse>();
                //if (ScripIpo != null && ScripIpo.Any())
                //    listIpoData = ScripIpo.ConvertAll(x => new AllScripInfoResponse
                //    {
                //        IpoId = x.IpoId,
                //        Name = x.Name,
                //        FromDate = x.FromDate,
                //        ToDate = x.ToDate,
                //        CreatedOn = x.CreatedOn,
                //        Status = x.Status,
                //        IsRead = x.IsRead,
                //        ResearchType = "Ipo"
                //    });

                //if (listIpoData != null && listIpoData.Any())
                //    listResponseData.AddRange(listIpoData);

                //For General
                var listGeneralData = new List<AllScripInfoResponse>();
                if (ScripGeneral != null && ScripGeneral.Any())
                    listGeneralData = ScripGeneral.ConvertAll(x => new AllScripInfoResponse
                    {
                        GeneralId = x.GeneralId,
                        Equity = x.Equity,
                        FutureOptions = x.FutureOptions,
                        Currencies = x.Currencies,
                        Commodities = x.Commodities,
                        Subject = x.Subject,
                        Message = x.Message,
                        CreatedOn = x.CreatedOn,
                        Status = x.Status,
                        IsRead = x.IsRead,
                        CompanyId = x.CompanyId,
                        ResearchType = "General",
                        ShortDate = x.CreatedOn
                    });

                if (listGeneralData != null && listGeneralData.Any())
                    listResponseData.AddRange(listGeneralData);

                //For Scrip Order FollowUp 
                var listFollowUpData = new List<AllScripInfoResponse>();
                if (ScripOrderFollowUp != null && ScripOrderFollowUp.Any())
                    listFollowUpData = ScripOrderFollowUp.ConvertAll(x => new AllScripInfoResponse
                    {

                        OrderId = x.OrderId,
                        ScripSymbol = x.ScripSymbol,
                        ScripToken = x.ScripToken,
                        IntradaybtstDelivery = x.IntradaybtstDelivery,
                        BuySell = x.BuySell,
                        PriceRangeFrom = x.PriceRangeFrom,
                        PriceRangeTo = x.PriceRangeTo,
                        Target = x.Target,
                        StopLoss = x.StopLoss,
                        Duration = x.Duration,
                        DurationType = x.DurationType,
                        CreatedOn = x.CreatedOn,
                        MessageType = x.MessageType,
                        Message = x.Message,
                        ScripOption = x.ScripOption,
                        CashToken = x.CashToken,
                        StrikePrice = x.StrikePrice,
                        CallOrPut = x.CallOrPut,
                        ExpiryDate = x.ExpiryDate,
                        CompanyName = x.CompanyName,
                        InstrumentName = x.InstrumentName,
                        SegmentName = x.SegmentName,
                        ExchangeName = x.ExchangeName,
                        IndustryType = x.IndustryType,
                        Status = x.Status,
                        FollowupId = x.FollowupId,
                        FollowupMessage = x.FollowupMessage,
                        FollowupCreatedOn = x.FollowupCreatedOn,
                        IsRead = x.IsRead,
                        IsButtonDisplayed = x.IsButtonDisplayed,
                        ButtonBuySell = x.ButtonBuySell,
                        FilePath = x.FilePath,
                        CompanyId = x.CompanyId,
                        ResearchType = "FollowUp",
                        ShortDate = x.FollowupCreatedOn,
                        KBContract = x.KBContract
                    });

                if (listFollowUpData != null && listFollowUpData.Any())
                    listResponseData.AddRange(listFollowUpData);

                ////Return Response
                var result = new ResponseBaseModel<AllScripInfoResponse>();
                if (listResponseData != null && listResponseData.Any())
                    result.Datas = listResponseData.OrderByDescending(x => x.ShortDate).ToList(); //(x => x.CreatedOn)
                else
                    result.Datas = new List<AllScripInfoResponse>();
                result.TotalRows = TotalRows;

                return result;
            }
        }
        public async Task<ResponseBaseRecModel<ScripOrderbySegmentsRes>> GetScripOrderbySegments(ScripOrderbySegmentsReq obj)
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var param = new DynamicParameters();
                param.Add("@Segment", obj.Segment);
                param.Add("@Type", obj.Type);
                param.Add("@PageNo", obj.PageNo);
                param.Add("@PageSize", obj.PageSize);
                param.Add("@PositiveCalls", dbType: DbType.Int32, direction: ParameterDirection.Output, size: 100000);
                param.Add("@NegativeCalls", dbType: DbType.Int32, direction: ParameterDirection.Output, size: 100000);
                param.Add("@TotalRows", dbType: DbType.Int32, direction: ParameterDirection.Output, size: 100000);
                var result = new ResponseBaseRecModel<ScripOrderbySegmentsRes>();
                result.Datas = (await SqlMapper.QueryAsync<ScripOrderbySegmentsRes>(con, "RP_GetScripOrderbySegments", param, commandType: CommandType.StoredProcedure)).ToList();
                result.PositiveCalls = param.Get<Int32>("PositiveCalls");
                result.NegativeCalls = param.Get<Int32>("NegativeCalls");
                result.TotalRows = param.Get<Int32>("TotalRows");
                return result;
            }
        }
        public async Task<ResponseBaseModel<ViewRecPercentageInfo>> ViewRecommendationPercentage()
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var result = new ResponseBaseModel<ViewRecPercentageInfo>();
                result.Datas = (await SqlMapper.QueryAsync<ViewRecPercentageInfo>(con, "Per_Get_RecommendationPercentage", null, commandType: CommandType.StoredProcedure)).ToList();
                result.TotalRows = result.Datas.Count;
                return result;
            }
        }

        public async Task<ResponseBaseModel<RecommendationPercentageInfo>> GetRecommendationPercentage()
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var result = new ResponseBaseModel<RecommendationPercentageInfo>
                {
                    Datas = (await SqlMapper.QueryAsync<RecommendationPercentageInfo>(con, "RecommendationPercentage", commandType: CommandType.StoredProcedure)).ToList()
                };

                result.TotalRows = result.Datas.Count;
                return result;
            }
        }

        //public async Task<List<ScripOrderbySegmentsRes>> GetTopRecommendationListFromDatabase(string strSegment)
        public async Task GetTopRecommendationListFromDatabase(string strSegment)
        {
            //// Client's segment wise recommendation 
            using (IDbConnection con = CreateRPConnection())
            {
                var param = new DynamicParameters();
                param.Add("@ClientSegment", strSegment);

                // Fetch data from DB based on client segment
                var result = await SqlMapper.QueryAsync<ScripOrderbySegmentsRes>(con, "ScripOrder_GetDataByClientSegment", param, commandType: CommandType.StoredProcedure);

                // Store raw result for direct segments in cache
                if (_recoKeys.Contains(strSegment))
                    _cacheManager.Set(strSegment, result.ToList(), TimeSpan.FromHours(24));

                // Pre-fetch all base segments from cache
                var segmentDataMap = new Dictionary<string, List<ScripOrderbySegmentsRes>>
                {
                    ["Commodity"] = _cacheManager.Get<List<ScripOrderbySegmentsRes>>("Commodity") ?? new(),
                    ["Delivery"] = _cacheManager.Get<List<ScripOrderbySegmentsRes>>("Delivery") ?? new(),
                    ["Intraday"] = _cacheManager.Get<List<ScripOrderbySegmentsRes>>("Intraday") ?? new(),
                    ["FNO"] = _cacheManager.Get<List<ScripOrderbySegmentsRes>>("FNO") ?? new()
                };

                // Loop through keys that include the current segment
                foreach (var cacheKey in _recoKeys.Where(key =>
                    key.Contains(strSegment, StringComparison.OrdinalIgnoreCase)))
                {
                    var queue = new TimePriorityQueue<ScripOrderbySegmentsRes>(maxSize: 5);

                    // Split composite keys like "Delivery_FNO"
                    var parts = cacheKey.Split('_', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (segmentDataMap.TryGetValue(part, out var data))
                            AddListToQueue(queue, data);
                    }
                    _cacheManager.Set(cacheKey, queue.GetData(), TimeSpan.FromHours(24));
                }
            }
        }
        public async Task<bool> GetAllSegmentsData()
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var param = new DynamicParameters();
                // Execute stored procedure to get all segment-wise data
                var reco_result = await SqlMapper.QueryMultipleAsync(con, "ScripOrder_GetAllSegmentsData", param, commandType: CommandType.StoredProcedure);

                // Read results for each segment
                var commodityRecom = reco_result.Read<ScripOrderbySegmentsRes>().ToList();
                var deliveryRecom = reco_result.Read<ScripOrderbySegmentsRes>().ToList();
                var intradayRecom = reco_result.Read<ScripOrderbySegmentsRes>().ToList();
                var fnoRecom = reco_result.Read<ScripOrderbySegmentsRes>().ToList();
                var allRecom = reco_result.Read<ScripOrderbySegmentsRes>().ToList();

                // Store base segment data in a dictionary for easy lookup
                var segmentDataMap = new Dictionary<string, List<ScripOrderbySegmentsRes>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Commodity"] = commodityRecom,
                    ["Delivery"] = deliveryRecom,
                    ["Intraday"] = intradayRecom,
                    ["FNO"] = fnoRecom
                };

                // Iterate over all configured cache keys
                foreach (var cacheKey in _recoKeys)
                {
                    // Handle special case for "New/All"
                    if (cacheKey.Equals("Commodity_Delivery_FNO_Intraday", StringComparison.OrdinalIgnoreCase))
                    {
                        _cacheManager.Set("Commodity_Delivery_FNO_Intraday", allRecom, TimeSpan.FromHours(24));
                        continue;
                    }

                    // If it's a single base segment, set it directly
                    if (segmentDataMap.ContainsKey(cacheKey))
                    {
                        _cacheManager.Set(cacheKey, segmentDataMap[cacheKey], TimeSpan.FromHours(24));
                        continue;
                    }

                    // Otherwise, treat it as a composite key and combine segment data
                    var queue = new TimePriorityQueue<ScripOrderbySegmentsRes>(maxSize: 5);
                    var parts = cacheKey.Split('_', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var part in parts)
                    {
                        if (segmentDataMap.TryGetValue(part, out var segmentList))
                        {
                            AddListToQueue(queue, segmentList);
                        }
                    }

                    _cacheManager.Set(cacheKey, queue.GetData(), TimeSpan.FromHours(24));
                }
                return true;
            }
        }
        void AddListToQueue(TimePriorityQueue<ScripOrderbySegmentsRes> queue, List<ScripOrderbySegmentsRes> list)
        {
            foreach (var item in list)
            {
                // Use a unique key (e.g., item ID or GUID or combination)
                queue.AddOrUpdate(Guid.NewGuid().ToString(), item, item.CreatedOn);
            }
        }
        public async Task<List<ScripOrderbySegmentsRes>> GetShortTermRecomFromDb()
        {
            using (IDbConnection con = CreateRPConnection())
            {
                // Fetch recommendation list from the database    
                var param = new DynamicParameters();
                param.Add("@IsShortTerm", 1);
                var result = await SqlMapper.QueryAsync<ScripOrderbySegmentsRes>(con, "RP_GetRecommendations", param, commandType: CommandType.StoredProcedure);

                // Check for null and return an empty list instead
                return result?.ToList() ?? new List<ScripOrderbySegmentsRes>();
            }
        }
        public async Task<List<ScripOrderbySegmentsRes>> GetLongTermRecomFromDb()
        {
            using (IDbConnection con = CreateRPConnection())
            {
                // Fetch recommendation list from the database    
                var param = new DynamicParameters();
                param.Add("@IsShortTerm", 0);
                var result = await SqlMapper.QueryAsync<ScripOrderbySegmentsRes>(con, "RP_GetRecommendations", param, commandType: CommandType.StoredProcedure);

                // Check for null and return an empty list instead
                return result?.ToList() ?? new List<ScripOrderbySegmentsRes>();
            }
        }

        public async Task<ResponseBaseCallRecModel<GraphData, WebCallRecommendation>> GetWebCallRecommendation(OrderbySegmentsReq obj)
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var param = new DynamicParameters();
                param.Add("@ProductType", obj.Segment);
                param.Add("@SegmentType", obj.Type);
                param.Add("@CallStatus", obj.CallStatus);

                var dbResult = await con.QueryMultipleAsync("GetWebCallRecommendation", param, commandType: CommandType.StoredProcedure);

                var webCallRecommendation = dbResult.Read<WebCallRecommendation>().ToList();
                var GraphPerformance = dbResult.Read<DailyWebRecommendation>()
                                         .Select(x => x.NetDayGainPercent)
                                         .ToList();
                var graphCallStatics = dbResult.ReadFirstOrDefault<GraphCallStatics>();
                
                var pagedData = webCallRecommendation
                .Skip((obj.PageNumber - 1) * obj.PageSize)
                .Take(obj.PageSize)
                .ToList();

                return new ResponseBaseCallRecModel<GraphData, WebCallRecommendation>()
                {
                    GraphData = new GraphData
                    {
                        GraphCallStatics = graphCallStatics,
                        GraphPerformance = GraphPerformance
                    },
                    Datas = pagedData,
                    TotalRows = webCallRecommendation.Count,
                };
            }
        }

        public async Task<ResponseBaseMobCallRecModel<GraphData, MobCallRecommendation, ClosedData>> GetMobCallRecommendation(OrderbySegmentsReq obj)
        {
            using (IDbConnection con = CreateRPConnection())
            {
                var param = new DynamicParameters();
                param.Add("@ProductType", obj.Segment);
                param.Add("@SegmentType", obj.Type);
                param.Add("@CallStatus", obj.CallStatus);

                using var dbResult = await con.QueryMultipleAsync("GetMobCallRecommendation", param, commandType: CommandType.StoredProcedure);

                var mobCallRecommendation = dbResult.Read<MobCallRecommendation>().ToList(); // All Data List
                var graphPerformance = dbResult.Read<DailyWebRecommendation>().ToList(); // Date And Precent for Graph
                var graphCallStatics = dbResult.ReadFirstOrDefault<GraphCallStatics>(); //Graph Statics

                var graphData = new GraphData
                {
                    GraphCallStatics = graphCallStatics,
                    GraphPerformance = graphPerformance.Select(x => x.NetDayGainPercent).ToList()
                };

                if (obj.CallStatus.Equals("Live", StringComparison.OrdinalIgnoreCase))
                {
                    var activeDatas = mobCallRecommendation
                                    .Skip((obj.PageNumber - 1) * obj.PageSize)
                                    .Take(obj.PageSize)
                                    .ToList();

                    return new ResponseBaseMobCallRecModel<GraphData, MobCallRecommendation, ClosedData>()
                    {
                        GraphData = graphData,
                        ActiveDatas = activeDatas,
                        ClosedDatas = null,
                        TotalRows = mobCallRecommendation.Count
                    };
                }
                else
                {
                    var closedDataList = mobCallRecommendation
                                       .GroupBy(u => u.ExitDate.Value.Date)
                                       .Select(g => new ClosedData
                                       {
                                           ExitDate = g.Key.ToString("yyyy-MMM-dd"),
                                           ClosedList = g.ToList(),
                                           NetDayGainPercent = graphPerformance
                                                                .Where(d => d.OrderClosedDate == g.Key.ToString("yyyy-MM-dd"))
                                                                .Select(d => d.NetDayGainPercent)
                                                                .FirstOrDefault()
                                       })
                                       .ToList();

                    var closedDatas = closedDataList
                                    .Skip((obj.PageNumber - 1) * obj.PageSize)
                                    .Take(obj.PageSize)
                                    .ToList();

                    return new ResponseBaseMobCallRecModel<GraphData, MobCallRecommendation, ClosedData>()
                    {
                        GraphData = graphData,
                        ActiveDatas = null,
                        ClosedDatas = closedDatas,
                        TotalRows = closedDataList.Count
                    };
                }
            }
        }
    }
}
