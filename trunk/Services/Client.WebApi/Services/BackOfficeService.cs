using Dapper;
using System.Data;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Client.WebApi.Services
{
    public interface IBackOfficeService
    {
        Task<ResponseBaseTModel<CalBrokerageResponse>> GetCalculatedBrokerage(CalBrokerageRequest param);
    }
    public class BackOfficeService : BaseService, IBackOfficeService
    {
        public async Task<ResponseBaseTModel<CalBrokerageResponse>> GetCalculatedBrokerage(CalBrokerageRequest param)
        {
            var resdata = new CalBrokerageResp();

            string Exchange = param.Exch?.ToUpperInvariant() switch
            {
                "NSE" => "NSE_CASH",
                "BSE" => "BSE_CASH",
                "NFO" => "NSE_FNO",
                "BFO" => "BSE_BFO",
                "CDS" => "CD_NSE",
                "BCD" => "CD_BSE",
                "MCX" => "MCX",
                "NCX" => "NCDEX",
                "NCOM" => "NSE_COM",
                _ => string.Empty
            };          

            var dbArgs = new DynamicParameters();
            dbArgs.Add("@UserId", param.Uid);
            dbArgs.Add("@OptType", param.OptType);
            dbArgs.Add("@ScripSymbol", param.ScripSymbol);
            dbArgs.Add("@Exch", Exchange);

            using IDbConnection conn = CreateTrvwConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using var dbResult = await SqlMapper.QueryMultipleAsync(
                conn,
                "GetBrokerageDetailsWithScripWise",
                dbArgs,
                commandType: CommandType.StoredProcedure
            ).ConfigureAwait(false);

            var scripWiseDetails = dbResult.ReadFirstOrDefault<ScripWiseBrokerageInternalResp>();
            var brokeragelst = dbResult.Read<BrokerageInternalResponse>().ToList();

            if (scripWiseDetails != null)
            {
                if (Exchange is "NSE_CASH" or "BSE_CASH")
                {
                    /*
                     * In The Case of NSE_CASH/BSE_CASH : BuyRate and SellRate Both are same - Use DELIVERYPER from Table.
                     * Need to apply percent base logic 
                     * Formula is : ((Price * Qty) * percent) / 100
                    */
                    if (decimal.TryParse(scripWiseDetails.BuyRate, out var percent))
                    {
                        decimal baseAmount = param.Price * param.Qty;
                        resdata.Brokerage = Math.Round(baseAmount * percent / 100, 2);
                    }
                }
                else
                {
                    /*
                     * In The Case : BuyRate and SellRate Both are difrent - Use OneSideConMin for BuyRate and OtherSideConMin for SellRate from Table.
                     * Need to apply per lot base logic 
                     * Formula is : param.Qty * perlot
                     * There are 3 SCRIP_TYPE -> "All"/"OPT"/"FUT" - But First Proity is "OPT/FUT" (Exact match), if not matched then check for All. 
                     * So "All" work for Both (Incase of OPT or FUT).   
                     * Also we matched @ScripSymbol with coloumn SCRIP_SYMBOL
                     * Always check with to_date IS NULL => when to_date is not null menas row expired.
                    */

                    bool isBuy = param.TransType?.Equals("B", StringComparison.OrdinalIgnoreCase) ?? false;
                    if (isBuy)
                    {
                        if (decimal.TryParse(scripWiseDetails.BuyRate, out var perlot))
                            resdata.Brokerage = Math.Round(param.Qty * perlot, 2);
                    }
                    else
                    {
                        if (decimal.TryParse(scripWiseDetails.SellRate, out var perlot))
                            resdata.Brokerage = Math.Round(param.Qty * perlot, 2);
                    }                   
                }
            }
            else
            {
                //Add case "F": Same As case "C" - For MTF (By Manan)
                if (brokeragelst?.Any() == true && string.IsNullOrWhiteSpace(brokeragelst[0].IBT_Module))
                {
                    string typeFilter = string.Empty;
                    string prd = param.Prd.ToUpper();
                    string opt = param.OptType.ToUpper();
                    string[] exchangeTags = Array.Empty<string>();

                    switch (param.Exch.ToUpper())
                    {
                        case "NSE":
                        case "BSE":
                            exchangeTags = new[] { "NSE_CASH", "BSE_CASH" };

                            if (prd == "I" || prd == "B" || prd == "H") // For Equity Intraday
                                typeFilter = "Trading-One";
                            else if (prd == "C" || prd == "M" || prd == "F") // For Equity Delivery
                                typeFilter = "Delivery";

                            resdata.Brokerage = GetBrokeragePercent(brokeragelst, exchangeTags, typeFilter, param.Price, param.Qty);
                            break;
                        case "NFO":
                        case "BFO":
                            exchangeTags = new[] { "NSE_FNO", "BSE_FNO" };
                            if (opt == "FUT") // For Equity Derivatives Futures => OptType => FUT
                            {
                                typeFilter = "Future";
                                resdata.Brokerage = GetBrokeragePercent(brokeragelst, exchangeTags, typeFilter, param.Price, param.Qty);
                            }
                            else if (opt == "OPT") // For Equity Derivatives Options => OptType => OPT
                            {
                                typeFilter = "Option-Conmin";
                                resdata.Brokerage = GetBrokeragePerLot(brokeragelst, exchangeTags, typeFilter, param.Qty);
                            }
                            break;
                        case "CDS":
                        case "BCD":
                            exchangeTags = new[] { "CD_NSE", "CD_BSE" };
                            if (opt == "FUT") // For Currency Derivatives Futures => OptType => FUT  
                            {
                                typeFilter = "Future";
                                resdata.Brokerage = GetBrokeragePercent(brokeragelst, exchangeTags, typeFilter, param.Price, param.Qty);
                            }
                            else if (opt == "OPT") // For Currency Derivatives Options => OptType => OPT
                            {
                                typeFilter = "Option-Conmin";
                                resdata.Brokerage = GetBrokeragePerLot(brokeragelst, exchangeTags, typeFilter, param.Qty);
                            }
                            break;
                        case "MCX":
                        case "NCX":
                            exchangeTags = new[] { "MCX", "NCDEX" };
                            if (opt == "FUT")
                            {
                                typeFilter = "Future";
                                resdata.Brokerage = GetBrokerageWithMultiplier(brokeragelst, exchangeTags, typeFilter, param.Price, param.Qty, param.PrcFactor, param.Multiplier);
                            }
                            else if (opt == "OPT")
                            {
                                typeFilter = "Option-Conmin";
                                resdata.Brokerage = GetBrokeragePerLot(brokeragelst, exchangeTags, typeFilter, param.Qty);
                            }
                            break;
                        case "NCOM":
                            exchangeTags = new[] { "NSE_COM", "BSE_COM" };
                            if (opt == "FUT") // For Commodity Derivatives Futures => OptType => FUT
                            {
                                typeFilter = "Future";
                                resdata.Brokerage = GetBrokeragePercent(brokeragelst, exchangeTags, typeFilter, param.Price, param.Qty);
                            }
                            else if (opt == "OPT") // For Commodity Derivatives Options => OptType => OPT
                            {
                                typeFilter = "Option-Conmin";
                                resdata.Brokerage = GetBrokeragePerLot(brokeragelst, exchangeTags, typeFilter, param.Qty);
                            }
                            break;
                        default:
                            resdata.Brokerage = 0;
                            break;
                    }
                }
                else
                {
                    /* Calculate brokerage as the lower of (percentage of trade value or Rs. 20), using 0.05% by default and 2.5% for equity delivery.                   
                       EquityIntraday = "0.05% or Rs. 20"; 
                       EquityDelivery = "2.5% or Rs. 20";
                       EquityDerivativesFutures = "0.05% or Rs. 20";
                       CurrencyDerivativesFutures = "0.05% or Rs. 20";
                       CommodityDerivativesFutures = "0.05% or Rs. 20";
                       EquityDerivativesOptions = "Rs. 20 Per Order";
                       CurrencyDerivativesOptions = "Rs. 20 Per Order";
                       CommodityDerivativesOptions = "Rs. 20 Per Order";   
                       param.Prd = "I", "B", "H" (Intraday) or param.OptType = "FUT" => Default: 0.05% => 0.0005M
                       param.Prd = "C", "M", "F" (Delivery) => Default: 2.5% => 0.025M
                     */

                    decimal brokerageRate = 0.0005M; // Default: 0.05%
                    decimal brokerageAmount = 0M;
                    switch (param.Exch.ToUpper())
                    {
                        case "NSE":
                        case "BSE":
                            switch (param.Prd.ToUpper())
                            {
                                case "C":
                                case "M":
                                case "F": // Delivery
                                    brokerageRate = 0.025M; // Override to 2.5%
                                    break;
                            }
                            brokerageAmount = Math.Round((param.Price * param.Qty) * brokerageRate, 2);
                            resdata.Brokerage = Math.Min(brokerageAmount, 20M);
                            break;
                        case "NFO":
                        case "BFO":
                        case "CDS":
                        case "BCD":
                        case "MCX":
                        case "NCX":
                        case "NCOM":
                            if (param.OptType.ToUpper() == "FUT") // OptType => FUT
                            {
                                brokerageAmount = Math.Round((param.Price * param.Qty) * brokerageRate, 2);
                                resdata.Brokerage = Math.Min(brokerageAmount, 20M);
                            }
                            else // OptType => OPT
                            {
                                resdata.Brokerage = 20M;
                            }
                            break;
                        default:
                            resdata.Brokerage = 20M;
                            break;
                    }
                }
            }

            //For Other Charges            
            switch (param.Exch.ToUpper())
            {
                case "NSE":
                    switch (param.Prd.ToUpper())
                    {
                        case "I":
                        case "B":
                        case "H":
                            switch (param.TransType.ToUpper())
                            {
                                case "S"://Selling Side
                                    resdata.STT = Math.Round((param.Price * param.Qty) * ((decimal)0.025 / 100), 5);
                                    resdata.StampDuty = 0;
                                    break;
                                case "B"://Buy Side
                                    resdata.STT = 0;
                                    resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.003 / 100), 5);
                                    break;
                            }
                            break;
                        case "C":
                        case "F":
                            switch (param.TransType.ToUpper())
                            {
                                case "S"://Selling Side
                                    resdata.STT = Math.Round((param.Price * param.Qty) * ((decimal)0.1 / 100), 5);
                                    resdata.StampDuty = 0;
                                    break;
                                case "B"://Buy Side
                                    resdata.STT = Math.Round((param.Price * param.Qty) * ((decimal)0.1 / 100), 5);
                                    resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.015 / 100), 5);
                                    break;
                            }
                            break;
                        default:
                            resdata.STT = 0;
                            resdata.StampDuty = 0;
                            break;
                    }
                    ////----- Changed TurnoverChgs to 0.00325 from 0.00345 on 22 Nov 2023 as discussed with Manan
                    ////----- Changed TurnoverChgs from 0.00325 to 0.00297 on 01 Oct 2024 as discussed with Vaibhav and Hari
                    resdata.TurnoverChgs = Math.Round((param.Price * param.Qty) * ((decimal)0.00297 / 100), 5);

                    ////----- Changed SEBIFees to 0.0001 from 0.00005 on 22 Nov 2023 as discussed with Manan
                    resdata.SEBIFees = Math.Round((param.Price * param.Qty) * ((decimal)0.0001 / 100), 5);
                    resdata.CMCharges = 0;
                    ////----- Changed GST to added SEBI Fees on 22 Nov 2023 as discussed with Manan
                    resdata.GST = Math.Round(((resdata.Brokerage + resdata.TurnoverChgs + resdata.SEBIFees) * 18) / 100, 5);
                    resdata.CTT = 0;
                    resdata.RMFFees = 0;
                    break;
                case "BSE":
                    switch (param.Prd.ToUpper())
                    {
                        case "I":
                        case "B":
                        case "H":
                            switch (param.TransType.ToUpper())
                            {
                                case "S"://Selling Side
                                    resdata.STT = Math.Round((param.Price * param.Qty) * ((decimal)0.025 / 100), 5);
                                    resdata.StampDuty = 0;
                                    break;
                                case "B"://Buy Side
                                    resdata.STT = 0;
                                    resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.003 / 100), 5);
                                    break;
                            }
                            break;
                        case "C":
                        case "F":
                            switch (param.TransType.ToUpper())
                            {
                                case "S"://Selling Side
                                    resdata.STT = Math.Round((param.Price * param.Qty) * ((decimal)0.1 / 100), 5);
                                    resdata.StampDuty = 0;
                                    break;
                                case "B"://Buy Side
                                    resdata.STT = Math.Round((param.Price * param.Qty) * ((decimal)0.1 / 100), 5);
                                    resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.015 / 100), 5);
                                    break;
                            }
                            break;
                        default:
                            resdata.STT = 0;
                            resdata.StampDuty = 0;
                            break;
                    }
                    ////----- Changed TurnoverChgs to 0.00375 from 0.00300 on 22 Nov 2023 as discussed with Manan
                    resdata.TurnoverChgs = Math.Round((param.Price * param.Qty) * ((decimal)0.00375 / 100), 5);
                    ////----- Changed SEBIFees to 0.0001 from 0.00005 on 22 Nov 2023 as discussed with Manan
                    resdata.SEBIFees = Math.Round((param.Price * param.Qty) * ((decimal)0.0001 / 100), 5);
                    resdata.CMCharges = 0;
                    ////----- Changed GST to added SEBI Fees on 22 Nov 2023 as discussed with Manan
                    resdata.GST = Math.Round(((resdata.Brokerage + resdata.TurnoverChgs + resdata.SEBIFees) * 18) / 100, 5);
                    resdata.CTT = 0;
                    resdata.RMFFees = 0;
                    break;
                case "NFO":
                    switch (param.OptType.ToUpper())
                    {
                        case "FUT":
                            switch (param.Prd.ToUpper())
                            {
                                case "I":
                                case "B":
                                case "H":
                                    switch (param.TransType.ToUpper())
                                    {
                                        ////----- Changed STT to 0.0125 from 0.01 on 22 Nov 2023 as discussed with Manan
                                        ////----- Changed STT from 0.0125 to 0.02 on 01 Oct 2024 as discussed with Vaibhav and Hari
                                        case "S"://Selling Side
                                            resdata.STT = Math.Round((param.Price * param.Qty) * ((decimal)0.02 / 100), 5);
                                            resdata.StampDuty = 0;
                                            break;
                                        case "B"://Buy Side
                                            resdata.STT = 0;
                                            resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.002 / 100), 5);
                                            break;
                                    }
                                    break;
                                case "M":
                                    switch (param.TransType.ToUpper())
                                    {
                                        ////----- Changed STT to Math.Round((param.Price * param.Qty) * ((decimal)0.0125 / 100), 5) from 0
                                        /// on 22 Nov 2023 as discussed with Manan
                                        ////----- Changed STT from 0.0125 to 0.02 on 01 Oct 2024 as discussed with Vaibhav and Hari
                                        case "S"://Selling Side
                                            resdata.STT = Math.Round((param.Price * param.Qty) * ((decimal)0.02 / 100), 5);
                                            resdata.StampDuty = 0;
                                            break;
                                        case "B"://Buy Side
                                            resdata.STT = 0;
                                            resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.002 / 100), 5);
                                            break;
                                    }
                                    break;
                                default:
                                    resdata.STT = 0;
                                    resdata.StampDuty = 0;
                                    break;
                            }
                            ////----- Changed TurnoverChgs to 0.0019 from 0.00200 on 22 Nov 2023 as discussed with Manan
                            ////----- Changed TurnoverChgs from 0.0019 to 0.00173 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.Qty) * ((decimal)0.00173 / 100), 5);
                            ////----- Changed CMCharges from 0.00130 to 0.00050 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.CMCharges = Math.Round((param.Price * param.Qty) * ((decimal)0.00050 / 100), 5);
                            resdata.SEBIFees = Math.Round((param.Price * param.Qty) * ((decimal)0.0001 / 100), 5);
                            break;
                        case "OPT":
                            switch (param.Prd.ToUpper())
                            {
                                case "I":
                                case "B":
                                case "H":
                                    switch (param.TransType.ToUpper())
                                    {
                                        ////----- Changed STT to 0.0625 from 0.05 on 22 Nov 2023 as discussed with Manan
                                        ////----- Changed STT from 0.0625 to 0.010 on 01 Oct 2024 as discussed with Vaibhav and Hari
                                        case "S"://Selling Side                                            
                                            resdata.STT = Math.Round((param.Price * param.OptQty) * ((decimal)0.010 / 100), 5);
                                            resdata.StampDuty = 0;
                                            break;
                                        case "B"://Buy Side
                                            resdata.STT = 0;
                                            resdata.StampDuty = Math.Round((param.Price * param.OptQty) * ((decimal)0.003 / 100), 5);
                                            break;
                                    }
                                    break;
                                case "M":
                                    switch (param.TransType.ToUpper())
                                    {
                                        ////----- Changed STT to  Math.Round((param.Price * param.OptQty) * ((decimal)0.0625 / 100), 5) from 0
                                        /// on 22 Nov 2023 as discussed with Manan
                                        ////----- Changed STT from 0.0625 to 0.010 on 01 Oct 2024 as discussed with Vaibhav and Hari
                                        case "S"://Selling Side
                                            resdata.STT = Math.Round((param.Price * param.OptQty) * ((decimal)0.010 / 100), 5);
                                            resdata.StampDuty = 0;
                                            break;
                                        case "B"://Buy Side
                                            resdata.STT = 0;
                                            resdata.StampDuty = Math.Round((param.Price * param.OptQty) * ((decimal)0.003 / 100), 5);
                                            break;
                                    }
                                    break;
                                default:
                                    resdata.STT = 0;
                                    resdata.StampDuty = 0;
                                    break;
                            }
                            ////----- Changed TurnoverChgs to 0.05 from 0.05300 on 22 Nov 2023 as discussed with Manan
                            ////----- Changed TurnoverChgs from 0.05 to 0.03503 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.OptQty) * ((decimal)0.03503 / 100), 5);
                            ////----- Changed TurnoverChgs from 0.02500 to 0.01250 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.CMCharges = Math.Round((param.Price * param.OptQty) * ((decimal)0.01250 / 100), 5);
                            resdata.SEBIFees = Math.Round((param.Price * param.OptQty) * ((decimal)0.0001 / 100), 5);
                            break;
                        default:
                            resdata.STT = 0;
                            resdata.StampDuty = 0;
                            resdata.TurnoverChgs = 0;
                            resdata.CMCharges = 0;
                            break;
                    }
                    ////----- Changed in GST, added SEBI Fees instead of CMCharges on 22 Nov 2023 as discussed with Manan
                    ////----- Changed in GST, added CMCharges on 01 Oct 2024 as discussed with Deepesh ji and Hari
                    resdata.GST = Math.Round(((resdata.Brokerage + resdata.TurnoverChgs + resdata.SEBIFees + resdata.CMCharges) * 18) / 100, 5);
                    resdata.CTT = 0;
                    resdata.RMFFees = 0;
                    break;
                case "BFO":
                    switch (param.OptType.ToUpper())
                    {
                        case "FUT":
                            switch (param.Prd.ToUpper())
                            {
                                case "I":
                                case "B":
                                case "H":
                                    switch (param.TransType.ToUpper())
                                    {
                                        case "S"://Selling Side
                                            ////----- Changed STT from 0.01 to 0.02 on 01 Oct 2024 as discussed with Vaibhav and Hari
                                            resdata.STT = Math.Round((param.Price * param.Qty) * ((decimal)0.02 / 100), 5);
                                            resdata.StampDuty = 0;
                                            break;
                                        case "B"://Buy Side
                                            resdata.STT = 0;
                                            resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.002 / 100), 5);
                                            break;
                                    }
                                    break;
                                case "M":
                                    switch (param.TransType.ToUpper())
                                    {
                                        ////----- Changed STT to  Math.Round((param.Price * param.Qty) * ((decimal)0.01 / 100), 5); from 0
                                        /// on 22 Nov 2023 as discussed with Manan
                                        ////----- Changed STT from 0.01 to 0.02 on 01 Oct 2024 as discussed with Vaibhav and Hari
                                        case "S"://Selling Side
                                            resdata.STT = Math.Round((param.Price * param.Qty) * ((decimal)0.02 / 100), 5);
                                            resdata.StampDuty = 0;
                                            break;
                                        ////----- Changed StampDuty to Math.Round((param.Price * param.Qty) * ((decimal)0.002 / 100), 5) from 0
                                        /// on 22 Nov 2023 as discussed with Manan
                                        case "B"://Buy Side
                                            resdata.STT = 0;
                                            resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.002 / 100), 5);
                                            break;
                                    }
                                    break;
                                default:
                                    resdata.STT = 0;
                                    resdata.StampDuty = 0;
                                    break;
                            }
                            ////----- Changed TurnoverChgs to 0 from Math.Round((param.Price * param.Qty) * ((decimal)0.00050 / 100), 5) on 22 Nov 2023 as discussed with Manan
                            resdata.TurnoverChgs = 0;
                            resdata.CMCharges = Math.Round((param.Price * param.Qty) * ((decimal)0.00125 / 100), 5);
                            resdata.SEBIFees = Math.Round((param.Price * param.Qty) * ((decimal)0.0001 / 100), 5);
                            break;
                        case "OPT":
                            switch (param.Prd.ToUpper())
                            {
                                case "I":
                                case "B":
                                case "H":
                                    switch (param.TransType.ToUpper())
                                    {
                                        case "S"://Selling Side
                                            ////----- Changed STT from 0.05 to 0.010 on 01 Oct 2024 as discussed with Vaibhav and Hari
                                            resdata.STT = Math.Round((param.Price * param.OptQty) * ((decimal)0.010 / 100), 5);
                                            resdata.StampDuty = 0;
                                            break;
                                        case "B"://Buy Side
                                            resdata.STT = 0;
                                            resdata.StampDuty = Math.Round((param.Price * param.OptQty) * ((decimal)0.003 / 100), 5);
                                            break;
                                    }
                                    break;
                                case "M":
                                    ////----- Changed STT to Math.Round((param.Price * param.OptQty) * ((decimal)0.05 / 100), 5) from 0
                                    /// on 22 Nov 2023 as discussed with Manan
                                    switch (param.TransType.ToUpper())
                                    {
                                        case "S"://Selling Side
                                            ////----- Changed STT from 0.05 to 0.010 on 01 Oct 2024 as discussed with Vaibhav and Hari
                                            resdata.STT = Math.Round((param.Price * param.OptQty) * ((decimal)0.010 / 100), 5);
                                            resdata.StampDuty = 0;
                                            break;
                                        ////----- Changed StampDuty to Math.Round((param.Price * param.OptQty) * ((decimal)0.003 / 100), 5) from 0
                                        /// on 22 Nov 2023 as discussed with Manan
                                        case "B"://Buy Side
                                            resdata.STT = 0;
                                            resdata.StampDuty = Math.Round((param.Price * param.OptQty) * ((decimal)0.003 / 100), 5);
                                            break;
                                    }
                                    break;
                                default:
                                    resdata.STT = 0;
                                    resdata.StampDuty = 0;
                                    break;
                            }
                            ////----- Changed TurnoverChgs to 0 from 0.005 on 22 Nov 2023 as discussed with Manan
                            ////----- Changed TurnoverChgs from 0.005 to 0.03250 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.OptQty) * ((decimal)0.03250 / 100), 5);
                            resdata.CMCharges = Math.Round((param.Price * param.OptQty) * ((decimal)0.02500 / 100), 5);
                            resdata.SEBIFees = Math.Round((param.Price * param.OptQty) * ((decimal)0.0001 / 100), 5);
                            break;
                        default:
                            resdata.STT = 0;
                            resdata.StampDuty = 0;
                            resdata.TurnoverChgs = 0;
                            resdata.CMCharges = 0;
                            break;
                    }
                    ////----- Change in GST, added SEBI Fees instead of CMCharges on 22 Nov 2023 as discussed with Manan
                    ////----- Changed in GST, added CMCharges on 01 Oct 2024 as discussed with Deepesh ji and Hari
                    resdata.GST = Math.Round(((resdata.Brokerage + resdata.TurnoverChgs + resdata.SEBIFees + resdata.CMCharges) * 18) / 100, 5);
                    resdata.CTT = 0;
                    resdata.RMFFees = 0;
                    break;
                case "CDS":
                    resdata.STT = 0;
                    ////----- Change in SEBIFees, from 0.00005 to 0.0001 on 22 Nov 2023 as discussed with Manan
                    //// ----- Moved StampDuty below and changed Qty to OptQty in case of "OPT"
                    switch (param.OptType.ToUpper())
                    {
                        case "FUT":
                            ////----- Change in TurnoverChgs, from 0.00115 to 0.0009 on 22 Nov 2023 as discussed with Manan
                            ////----- Changed TurnoverChgs from 0.0009 to 0.00035 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.Qty) * ((decimal)0.00035 / 100), 5);
                            resdata.CMCharges = Math.Round((param.Price * param.Qty) * ((decimal)0.00050 / 100), 5);
                            resdata.SEBIFees = Math.Round((param.Price * param.Qty) * ((decimal)0.0001 / 100), 5);
                            resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.0001 / 100), 5);
                            break;
                        case "OPT":
                            ////----- Changed TurnoverChgs from 0.03500 to 0.03110 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.OptQty) * ((decimal)0.03110 / 100), 5);
                            resdata.CMCharges = Math.Round((param.Price * param.OptQty) * ((decimal)0.02000 / 100), 5);
                            resdata.SEBIFees = Math.Round((param.Price * param.OptQty) * ((decimal)0.0001 / 100), 5);
                            resdata.StampDuty = Math.Round((param.Price * param.OptQty) * ((decimal)0.0001 / 100), 5);
                            break;
                        default:
                            resdata.TurnoverChgs = 0;
                            resdata.CMCharges = 0;
                            break;
                    }
                    ////----- Change in GST, added SEBI Fees instead of CMCharges on 22 Nov 2023 as discussed with Manan
                    ////----- Changed in GST, added CMCharges on 01 Oct 2024 as discussed with Deepesh ji and Hari
                    resdata.GST = Math.Round(((resdata.Brokerage + resdata.TurnoverChgs + resdata.SEBIFees + resdata.CMCharges) * 18) / 100, 5);
                    resdata.CTT = 0;
                    resdata.RMFFees = 0;
                    break;
                case "BCD":
                    resdata.STT = 0;
                    resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.0001 / 100), 5);
                    ////----- Change in TurnoverChgs, from 0.00010 to 0.0009 on 22 Nov 2023 as discussed with Manan

                    ////----- Change in SEBIFees, from 0.00005 to 0.0001 on 22 Nov 2023 as discussed with Manan
                    resdata.SEBIFees = Math.Round((param.Price * param.Qty) * ((decimal)0.0001 / 100), 5);
                    switch (param.OptType.ToUpper())
                    {
                        case "FUT":
                            ////----- Changed TurnoverChgs from 0.0009 to 0.00045 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.Qty) * ((decimal)0.00045 / 100), 5);
                            resdata.CMCharges = Math.Round((param.Price * param.Qty) * ((decimal)0.00050 / 100), 5);
                            break;
                        case "OPT":
                            ////----- Changed TurnoverChgs from 0.0009 to 0.00100 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.Qty) * ((decimal)0.00100 / 100), 5);
                            resdata.CMCharges = Math.Round((param.Price * param.OptQty) * ((decimal)0.02000 / 100), 5);
                            break;
                        default:
                            resdata.CMCharges = 0;
                            break;
                    }
                    ////----- Change in GST, added SEBI Fees instead of CMCharges on 22 Nov 2023 as discussed with Manan
                    ////----- Changed in GST, added CMCharges on 01 Oct 2024 as discussed with Deepesh ji and Hari
                    resdata.GST = Math.Round(((resdata.Brokerage + resdata.TurnoverChgs + resdata.SEBIFees + resdata.CMCharges) * 18) / 100, 5);
                    resdata.CTT = 0;
                    resdata.RMFFees = 0;
                    break;
                case "MCX":
                    resdata.STT = 0;
                    //// Moved Stamp Duty to Buy Side only
                    switch (param.OptType.ToUpper())
                    {
                        case "FUT":
                            ////----- Changed TurnoverChgs from 0.0026 to 0.0021 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.Qty * param.PrcFactor * param.Multiplier) * ((decimal)0.0021 / 100), 5);
                            ////----- Change in SEBIFees, from 0.00005 to 0.0001 on 22 Nov 2023 as discussed with Manan
                            resdata.SEBIFees = Math.Round((param.Price * param.Qty * param.PrcFactor * param.Multiplier) * ((decimal)0.0001 / 100), 5);
                            switch (param.TransType.ToUpper())
                            {
                                case "S"://Selling Side
                                    resdata.CTT = Math.Round((param.Price * param.Qty * param.PrcFactor * param.Multiplier) * ((decimal)0.01 / 100), 5);
                                    resdata.StampDuty = 0;
                                    break;
                                case "B"://Buy Side
                                    resdata.CTT = 0;
                                    resdata.StampDuty = Math.Round((param.Price * param.Qty * param.PrcFactor * param.Multiplier) * ((decimal)0.002 / 100), 5);
                                    break;
                            }
                            break;
                        case "OPT":
                            //// Changed CTT = 0 to case wise on 22 Nov 2023 as discussed with Manan
                            // resdata.CTT = 0;
                            switch (param.TransType.ToUpper())
                            {
                                case "S"://Selling Side
                                    resdata.CTT = Math.Round((param.Price * param.OptQty * param.PrcFactor * param.Multiplier) * ((decimal)0.05 / 100), 5);
                                    resdata.StampDuty = 0;
                                    break;
                                case "B"://Buy Side
                                    resdata.CTT = 0;
                                    resdata.StampDuty = Math.Round((param.Price * param.OptQty * param.PrcFactor * param.Multiplier) * ((decimal)0.003 / 100), 5);
                                    break;
                            }

                            //// Changed TurnoverChgs = 0 to following on 22 Nov 2023 as discussed with Manan
                            // resdata.TurnoverChgs = 0;
                            ////----- Changed TurnoverChgs from 0.05 to 0.0418 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.OptQty * param.PrcFactor * param.Multiplier) * ((decimal)0.0418 / 100), 5);
                            //// Changed SEBIFees = 0 to following on 22 Nov 2023 as discussed with Manan
                            //resdata.SEBIFees = 0;
                            resdata.SEBIFees = Math.Round((param.Price * param.OptQty * param.PrcFactor * param.Multiplier) * ((decimal)0.0001 / 100), 5);
                            break;
                        default:
                            resdata.CTT = 0;
                            resdata.StampDuty = 0;
                            resdata.TurnoverChgs = 0;
                            resdata.SEBIFees = 0;
                            break;
                    }
                    ////----- Change in GST, added SEBI Fees on 22 Nov 2023 as discussed with Manan
                    resdata.GST = Math.Round(((resdata.Brokerage + resdata.TurnoverChgs + resdata.SEBIFees) * 18) / 100, 5);
                    resdata.RMFFees = 0;
                    break;
                case "NCX":
                    resdata.STT = 0;
                    resdata.StampDuty = Math.Round((param.Price * param.Qty * param.PrcFactor * param.Multiplier) * ((decimal)0.002 / 100), 5);

                    resdata.SEBIFees = Math.Round((param.Price * param.Qty * param.PrcFactor * param.Multiplier) * ((decimal)0.00001 / 100), 5);
                    resdata.GST = Math.Round(((resdata.Brokerage + resdata.TurnoverChgs) * 18) / 100, 5);

                    decimal tAmt = (param.Price * param.Qty * param.PrcFactor * param.Multiplier);
                    if (tAmt >= 100000)
                        resdata.RMFFees = Math.Round((tAmt * (decimal)0.00005), 5);
                    else
                        resdata.RMFFees = 0;

                    switch (param.OptType.ToUpper())
                    {
                        case "FUT":
                            ////----- Changed TurnoverChgs from 0.0065 to 0.0058 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.Qty * param.PrcFactor * param.Multiplier) * ((decimal)0.0058 / 100), 5);
                            switch (param.TransType.ToUpper())
                            {
                                case "S"://Selling Side
                                    resdata.CTT = Math.Round((param.Price * param.Qty * param.PrcFactor * param.Multiplier) * ((decimal)0.01 / 100), 5);
                                    break;
                                case "B"://Buy Side
                                    resdata.CTT = 0;
                                    break;
                            }
                            break;
                        case "OPT":
                            ////----- Changed TurnoverChgs from 0.0065 to 0.03 on 01 Oct 2024 as discussed with Vaibhav and Hari
                            resdata.TurnoverChgs = Math.Round((param.Price * param.Qty * param.PrcFactor * param.Multiplier) * ((decimal)0.03 / 100), 5);
                            switch (param.TransType.ToUpper())
                            {
                                case "S"://Selling Side
                                    resdata.CTT = Math.Round((param.Price * param.OptQty * param.PrcFactor * param.Multiplier) * ((decimal)0.01 / 100), 5);
                                    break;
                                case "B"://Buy Side
                                    resdata.CTT = 0;
                                    break;
                            }
                            break;
                        default:
                            resdata.CTT = 0;
                            break;
                    }
                    break;
                case "NCOM":
                    resdata.STT = 0;
                    switch (param.OptType.ToUpper())
                    {
                        case "FUT":
                            //// Moved Stamp Duty to Buy Side only
                            resdata.TurnoverChgs = Math.Round((param.Price * param.Qty) * ((decimal)0.0026 / 100), 5);
                            ////----- Change in SEBIFees, from 0.00005 to 0.0001 on 22 Nov 2023 as discussed with Manan
                            resdata.SEBIFees = Math.Round((param.Price * param.Qty) * ((decimal)0.0001 / 100), 5);
                            switch (param.TransType.ToUpper())
                            {
                                case "S"://Selling Side
                                    resdata.CTT = Math.Round((param.Price * param.Qty) * ((decimal)0.01 / 100), 5);
                                    resdata.StampDuty = 0;
                                    break;
                                case "B"://Buy Side
                                    resdata.CTT = 0;
                                    resdata.StampDuty = Math.Round((param.Price * param.Qty) * ((decimal)0.002 / 100), 5);
                                    break;
                            }
                            break;
                        case "OPT":
                            //// Changed CTT = 0 to case wise on 22 Nov 2023 as discussed with Manan
                            // resdata.CTT = 0;
                            switch (param.TransType.ToUpper())
                            {
                                case "S"://Selling Side
                                    resdata.CTT = Math.Round((param.Price * param.OptQty) * ((decimal)0.05 / 100), 5);
                                    resdata.StampDuty = 0;
                                    break;
                                case "B"://Buy Side
                                    resdata.CTT = 0;
                                    resdata.StampDuty = Math.Round((param.Price * param.OptQty) * ((decimal)0.003 / 100), 5);
                                    break;
                            }
                            //// Changed TurnoverChgs = 0 to following on 22 Nov 2023 as discussed with Manan
                            // resdata.TurnoverChgs = 0;
                            resdata.TurnoverChgs = Math.Round((param.Price * param.OptQty) * ((decimal)0.05 / 100), 5);
                            //// Changed SEBIFees = 0 to following on 22 Nov 2023 as discussed with Manan
                            //resdata.SEBIFees = 0;
                            resdata.SEBIFees = Math.Round((param.Price * param.OptQty) * ((decimal)0.0001 / 100), 5);

                            break;
                        default:
                            resdata.CTT = 0;
                            resdata.StampDuty = 0;
                            resdata.TurnoverChgs = 0;
                            resdata.SEBIFees = 0;
                            break;
                    }
                    ////----- Change in GST, added SEBI Fees on 22 Nov 2023 as discussed with Manan
                    resdata.GST = Math.Round(((resdata.Brokerage + resdata.TurnoverChgs + resdata.SEBIFees) * 18) / 100, 5);
                    resdata.RMFFees = 0;
                    break;
                default:
                    resdata.STT = 0;
                    resdata.StampDuty = 0;
                    resdata.TurnoverChgs = 0;
                    resdata.SEBIFees = 0;
                    resdata.CMCharges = 0;
                    resdata.GST = 0;
                    resdata.CTT = 0;
                    resdata.RMFFees = 0;
                    break;
            }

            //Remove this becoz Only Charge displaying now So don't need(By Manan)
            //resdata.MarginReq = param.MarginReq > 0 ? (param.MarginReq - resdata.Brokerage) : 0;

            resdata.Total = resdata.Brokerage + resdata.MarginReq + (resdata.STT + resdata.StampDuty + resdata.TurnoverChgs + resdata.SEBIFees + resdata.CMCharges + resdata.GST + resdata.CTT + resdata.RMFFees);

            System.Globalization.CultureInfo culinfo = new System.Globalization.CultureInfo(0x0439);
            return new ResponseBaseTModel<CalBrokerageResponse>()
            {
                Data = new CalBrokerageResponse()
                {
                    Brokerage = resdata.Brokerage > 0 ? resdata.Brokerage.ToString("#,0.##", culinfo) : resdata.Brokerage.ToString(),
                    //MarginReq = resdata.MarginReq == 0 ? "0.0" : resdata.MarginReq.ToString("#,0.##", culinfo),
                    Total = resdata.Total > 0 ? resdata.Total.ToString("#,0.##", culinfo) : resdata.Total.ToString(),
                    STT = resdata.STT == 0 ? "0.0" : resdata.STT >= 1 ? resdata.STT.ToString("#,0.##", culinfo) : resdata.STT.ToString("#,0.#####", culinfo),
                    StampDuty = resdata.StampDuty == 0 ? "0.0" : resdata.StampDuty >= 1 ? resdata.StampDuty.ToString("#,0.##", culinfo) : resdata.StampDuty.ToString("#,0.#####", culinfo),
                    TurnoverChgs = resdata.TurnoverChgs == 0 ? "0.0" : resdata.TurnoverChgs >= 1 ? resdata.TurnoverChgs.ToString("#,0.##", culinfo) : resdata.TurnoverChgs.ToString("#,0.#####", culinfo),
                    SEBIFees = resdata.SEBIFees == 0 ? "0.0" : resdata.SEBIFees >= 1 ? resdata.SEBIFees.ToString("#,0.##", culinfo) : resdata.SEBIFees.ToString("#,0.#####", culinfo),
                    CMCharges = resdata.CMCharges == 0 ? "0.0" : resdata.CMCharges >= 1 ? resdata.CMCharges.ToString("#,0.##", culinfo) : resdata.CMCharges.ToString("#,0.#####", culinfo),
                    CTT = resdata.CTT == 0 ? "0.0" : resdata.CTT >= 1 ? resdata.CTT.ToString("#,0.##", culinfo) : resdata.CTT.ToString("#,0.#####", culinfo),
                    RMFFees = resdata.RMFFees == 0 ? "0.0" : resdata.RMFFees >= 1 ? resdata.RMFFees.ToString("#,0.##", culinfo) : resdata.RMFFees.ToString("#,0.#####", culinfo),
                    GST = resdata.GST == 0 ? "0.0" : resdata.GST >= 1 ? resdata.GST.ToString("#,0.##", culinfo) : resdata.GST.ToString("#,0.#####", culinfo),
                }
            };
        }

        // Method For: Percentage (basic)
        private decimal GetBrokeragePercent(List<BrokerageInternalResponse> list, string[] exchangeTags, string typeFilter, decimal price, decimal qty)
        {
            var item = list.FirstOrDefault(x =>
                exchangeTags.Any(tag => x.EXCHANGE.Contains(tag))
                && x.Type.Contains(typeFilter)
                && !string.IsNullOrEmpty(x.DELIVERYPER));

            if (item != null && decimal.TryParse(item.DELIVERYPER, out var percent))
            {
                decimal baseAmount = price * qty;
                return Math.Round(baseAmount * percent / 100, 2);
            }
            return 0;
        }

        // Method For: Per Lot
        private decimal GetBrokeragePerLot(List<BrokerageInternalResponse> list, string[] exchangeTags, string typeFilter, decimal qty)
        {
            var item = list.FirstOrDefault(x =>
                exchangeTags.Any(tag => x.EXCHANGE.Contains(tag))
                && x.Type.Contains(typeFilter)
                && !string.IsNullOrEmpty(x.DELIVERYPER));

            if (item != null && decimal.TryParse(item.DELIVERYPER, out var perlot))
            {
                return Math.Round(qty * perlot, 2);
            }
            return 0;
        }

        // Method For: With Multiplier
        private decimal GetBrokerageWithMultiplier(List<BrokerageInternalResponse> list, string[] exchangeTags, string typeFilter, decimal price, decimal qty, decimal prcFactor, decimal multiplier)
        {
            var item = list.FirstOrDefault(x =>
                exchangeTags.Any(tag => x.EXCHANGE.Contains(tag))
                && x.Type.Contains(typeFilter)
                && !string.IsNullOrEmpty(x.DELIVERYPER));

            if (item != null && decimal.TryParse(item.DELIVERYPER, out var percent))
            {
                decimal baseAmount = price * qty * prcFactor * multiplier;
                return Math.Round(baseAmount * percent / 100, 2);
            }
            return 0;
        }

    }
}
