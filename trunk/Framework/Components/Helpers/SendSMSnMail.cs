using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Components.Helpers
{
    public class SendSMSnMail
    {
        private readonly ILog _logger;
        private IConfiguration _config;

        public SendSMSnMail(IConfiguration config, ILog logger)
        {
            _config = config;
            _logger = logger;
        }

        public SendSMSnMail()
        {            
        }
        public void SendEmailByFalconide(List<string> to, string subject, string message, string from)
        {
            //string from = "hello@swastika.co.in";
            string ReplyTo = "kyc@swastika.co.in";
            string Display_Name = "Tradingo Swastika";

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.falconide.com");

            try
            {
                string toMail = string.Join(",", to);
                mail.From = new MailAddress(from, Display_Name);
                mail.To.Add(toMail);
                mail.ReplyToList.Add(ReplyTo);
                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential("swastikafal", "Sil@class12");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                SmtpServer.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $@"SendEmailByFalconide-Exception: " + ex.ToString());
                SmtpServer.Dispose();
            }
        }

        public void SendEmailByAmazonAWS(List<string> to, string subject, string message, string from)
        {
            //string from = "hello@swastika.co.in";
            string ReplyTo = "kyc@swastika.co.in";
            string Display_Name = "Tradingo Swastika";

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("email-smtp.ap-south-1.amazonaws.com");
            try
            {
                string toMail = string.Join(",", to);
                mail.From = new MailAddress(from, Display_Name);
                mail.To.Add(toMail);
                mail.ReplyToList.Add(ReplyTo);

                //mail.CC.Add(cc);
                mail.Bcc.Add("sunil.tiwari@swastika.co.in");

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential("AKIAXYTBBQW7NESSSNW7", "BIM4ixk6XPWoQV8MstZFIK7iY18HRW0zrW8wqfGS9NGF");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                SmtpServer.Dispose();

                _logger.Log(LogLevel.Trace, $@"SendEmailByAmazonAWS- " + mail.Body);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $@"SendEmailByAmazonAWS Exception-" + ex.ToString());
                SmtpServer.Dispose();
            }
        }

        public void SendEmailByNetgain(List<string> to, string subject, string message, string from)
        {
            //string from = "hello@swastika.co.in";
            string ReplyTo = "kyc@swastika.co.in";
            string Display_Name = "Tradingo Swastika";

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("180.179.151.1");
            try
            {
                string toMail = string.Join(",", to);
                mail.From = new MailAddress(from, Display_Name);
                mail.To.Add(toMail);
                mail.ReplyToList.Add(ReplyTo);

                //mail.CC.Add(cc);
                mail.Bcc.Add("sunil.tiwari@swastika.co.in");

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential("swastikatx@m3c.io", "gS1y$er#y32I");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                SmtpServer.Dispose();

                _logger.Log(LogLevel.Trace, $@"SendEmailByNetgain- " + mail.Body);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $@"SendEmailByNetgain Exception-" + ex.ToString());
                SmtpServer.Dispose();
            }
        }

        public void SendNetCoreSMS(string Mobile, string Text)
        {
            StringBuilder sbPostData = new StringBuilder();
            try
            {
                sbPostData.AppendFormat("username={0}", _config["NetCoreSMS:SmsUserName"].ToString());
                sbPostData.AppendFormat("&password={0}", _config["NetCoreSMS:SmsPassword"].ToString());
                sbPostData.AppendFormat("&feedid={0}", _config["NetCoreSMS:SmsFeedId"].ToString());
                sbPostData.AppendFormat("&To={0}", Mobile);
                sbPostData.AppendFormat("&Text={0}", Text);
                sbPostData.AppendFormat("&SmsSenderId={0}", _config["NetCoreSMS:SmsSenderId"].ToString());

                //http://bulkpush.mytoday.com/BulkSms/SingleMsgApi
                string sendSMSUri = _config["NetCoreSMS:SMSBaseAddress"] + "BulkSms/SingleMsgApi";

                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(sendSMSUri);
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] data = encoding.GetBytes(sbPostData.ToString());
                httpWReq.Method = "POST";
                httpWReq.ContentType = "application/x-www-form-urlencoded";
                httpWReq.ContentLength = data.Length;

                using (Stream stream = httpWReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                string responseString = string.Empty;
                using (HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse())
                using (Stream streamResponse = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(streamResponse))
                {
                    responseString = reader.ReadToEnd();
                }

                //_logger.Log(LogLevel.Trace, $@"SMSResponse- " + responseString);
            }
            catch (SystemException ex)
            {
                _logger.Log(LogLevel.Error, $@"SendSMS-Exception: " + ex.ToString());
            }
        }

        public void SendGo2MarketSMS(string Mobile, string Text)
        {
            StringBuilder sbPostData = new StringBuilder();
            try
            {
                sbPostData.AppendFormat(_config["G2MSMS:SMSBaseUrl"]);
                sbPostData.AppendFormat("?ukey={0}", _config["G2MSMS:ukey"]);
                sbPostData.AppendFormat("&msisdn={0}", Mobile);
                sbPostData.AppendFormat("&language={0}", _config["G2MSMS:language"]);
                sbPostData.AppendFormat("&credittype={0}", _config["G2MSMS:credittype"]);
                sbPostData.AppendFormat("&senderid={0}", _config["G2MSMS:senderid"]);
                sbPostData.AppendFormat("&templateid={0}", _config["G2MSMS:templateid"]);
                sbPostData.AppendFormat("&message={0}", Text);
                sbPostData.AppendFormat("&filetype={0}", _config["G2MSMS:filetype"]);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sbPostData.ToString());
                request.Method = "GET";
                request.KeepAlive = true;
                request.ContentType = "appication/json";
                request.Headers.Add("Content-Type", "appication/json");

                string myResponse = string.Empty;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream streamResponse = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(streamResponse))
                {
                    myResponse = reader.ReadToEnd();
                }

                _logger.Log(LogLevel.Trace, $@"G2M-SMSResponse- " + myResponse);

            }
            catch (SystemException ex)
            {
                _logger.Log(LogLevel.Error, $@"G2M SendSMS-Exception: " + ex.ToString());
            }
        }
    }
}
