using System.Collections.Generic;

namespace Client.WebApi.Models.InfoBipWhatsapp
{
    public class InfoBipWhatsapp
    {
        public List<Message> messages { get; set; }
    }
    public class Body
    {
        public List<string> placeholders { get; set; }
    }

    public class Content
    {
        public string templateName { get; set; }
        public TemplateData templateData { get; set; }
        public string language { get; set; }
    }

    public class Message
    {
        public string from { get; set; }
        public string to { get; set; }
        public Content content { get; set; }
        public string callbackData { get; set; }
        public string notifyUrl { get; set; }
    }


    public class TemplateData
    {
        public Body body { get; set; }
    }


}
