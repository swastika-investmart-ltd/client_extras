using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Wordprocessing;
using Irony.Ast;

namespace Client.WebApi
{
    public enum DPChargeCategoryType
    {
        NONE = 0,
        DP = 1,
        DEMAT_SETUP = 2,
        OTHER = 3,
        FundAdded = 4,
        FundWithdrawn = 5,
        DDPI = 6,
        AMC = 7
    }

    public enum DPChargeSubCategoryType
    {
        PLEDGE = 1,
        UNPLEDGE = 2,
        STOCK_SELLING = 3,
        OFFMARKET = 4
    }
    public enum Section1Category
    {
        NONE = 1,
        KNOW_MORE = 2, //AMC, DDPI
        VIEW_DETAIL = 3  //DP Charges
    }

    public class PassbookDataResponse
    {
        public List<PassbookData> PassbookDataList { get; set; } = new List<PassbookData>();
    }

    public class PassbookData
    {
        public string VoucherDate { get; set; }
        public List<Section1> Section1List { get; set; } = new List<Section1>();

        public override string ToString()
        {
            return $"VoucherDate:{VoucherDate}, Section1List Count:{Section1List.Count}";
        }
    }

    public class Section1
    {
        public DPChargeCategoryType Id { get; set; } //DPChargeCategoryType
        public Section1Category TypeId { get; set; } //SectionCategory
        public string LabelText { get; set; }
        public decimal TotalAmount { get; set; }
        //public DateTime TransactionDate { get; set; }
        public bool IsTransTypeCR { get; set; }
        public string ActionText { get; set; }
        public string Description { get; set; }
        public Section2 Section2Item { get; set; } =new Section2();

        public override string ToString()
        {
            return $"Id:{Id}, TypeId:{TypeId}, LabelText:{LabelText}, ActionText:{ActionText}";
        }

    }
    public class Section2
    {
        public string HeaderText { get; set; } //DP Charges for 15 Sep 2025
        public string BodyText { get; set; }
        public List<Section3> Section3List { get; set; } = new List<Section3>();

        public override string ToString()
        {
            return $"HeaderText:{HeaderText}, BodyText:{BodyText}";
        }
    }

    public class Section3
    {
        public string LabelText { get; set; }   //Stock Selling Charges
        public decimal TotalAmount { get; set; }    //150.00
        public string InfoText { get; set; }    //Fee for Selling shares, including taxes.
        public List<Section4> Section4List { get; set; } = new List<Section4>();
        public override string ToString()
        {
            return $"LabelText:{LabelText}, TotalAmount:{TotalAmount}, Section4List Count{Section4List.Count}";
        }
    }
    
    public class Section4
    {
        public string LabelText { get; set; } //RELIANCE (qty 25)
        public string Tag { get; set; } //Pledged
        public decimal Amount { get; set; } //50.00
        public override string ToString()
        {
            return $"LabelText:{LabelText}, Tag:{Tag}, Amount:{Amount}";
        }
    }
}
