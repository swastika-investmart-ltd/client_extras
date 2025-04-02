using System;
using System.Collections.Generic;

namespace Client.WebApi.Models.InfoBipWhatsapp
{
    public class InfoBipResp
    {
        public List<Result> results { get; set; }
    }

    public class Error
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
        public bool permanent { get; set; }
    }

    public class Price
    {
        public int pricePerMessage { get; set; }
        public string currency { get; set; }
    }
    public class Result
    {
        public string bulkId { get; set; }
        public Price price { get; set; }
        public Status status { get; set; }
        public Error error { get; set; }
        public string messageId { get; set; }
        public DateTime doneAt { get; set; }
        public int messageCount { get; set; }
        public DateTime sentAt { get; set; }
        public string callbackData { get; set; }
        public string to { get; set; }
        public string channel { get; set; }
    }

    public class Status
    {
        public int id { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }


}
