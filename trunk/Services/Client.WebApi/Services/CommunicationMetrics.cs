using Prometheus;

namespace Client.WebApi.Services
{
    public class CommPickyAssistMetrics
    {
        public static readonly Counter SendMessageSuccess = Metrics.CreateCounter("PicAssist_send_message_success", "Success in whatsapp send message");
        public static readonly Counter SendMessageError = Metrics.CreateCounter("PicAssist_send_message_error", "Error in whatsapp send message");
        public static readonly Histogram SendMessageDuration = Metrics.CreateHistogram("PicAssist_send_message_duration", "Histogram of whatsapp send message.");
    }
    public class CommTATATeleMetrics
    {
        public static readonly Counter SendTATAMessageSuccess = Metrics.CreateCounter("TATATele_send_message_success", "Success in TATATele send message");
        public static readonly Counter SendTATAMessageError = Metrics.CreateCounter("TATATele_send_message_error", "Error in TATATele send message");
        public static readonly Histogram SendTATAMessageDuration = Metrics.CreateHistogram("TATATele_send_message_duration", "Histogram of TATATele send message.");
    }
}