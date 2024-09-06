using Components;
using System.Net.Mail;
using System.Net;
using System;
using NLog;

namespace Client.WebApi
{
    public class EmailHelper
    {
        private readonly ILog _logger;
        public EmailHelper(ILog logger)
        {
            _logger = logger;
        }

        public void SendEmailByFalconide(string to, string subject, string message)
        {
            string from = "hello@swastika.co.in";
            string ReplyTo = "kyc@swastika.co.in";
            string Display_Name = "Justrade Swastika";

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.falconide.com");

            try
            {
                mail.From = new MailAddress(from, Display_Name);
                mail.To.Add(to);
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
                _logger.Log(LogLevel.Error, $@"Email-Exception: " + ex.ToString());
                SmtpServer.Dispose();
            }
        }

        public void SendEmailByAmazonAWS(string to, string subject, string message)
        {
            string from = "hello@swastika.co.in";
            string ReplyTo = "kyc@swastika.co.in";
            string Display_Name = "Tradingo Swastika";

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("email-smtp.ap-south-1.amazonaws.com");
            try
            {
                mail.From = new MailAddress(from, Display_Name);
                mail.To.Add(to);

                mail.ReplyToList.Add(ReplyTo);

                //mail.CC.Add(cc);
                //mail.Bcc.Add(bcc);

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential("AKIAXYTBBQW7NESSSNW7", "BIM4ixk6XPWoQV8MstZFIK7iY18HRW0zrW8wqfGS9NGF");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                SmtpServer.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $@"Exception-" + ex.ToString());
                SmtpServer.Dispose();
            }
        }

        public void SendEmailByAttachmentAmazonAWS(string to, string subject, string message, string AttachmentFilePath)
        {
            string from = "hello@swastika.co.in";
            string ReplyTo = "kyc@swastika.co.in";
            string Display_Name = "Swastika";

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("email-smtp.ap-south-1.amazonaws.com");
            try
            {
                mail.From = new MailAddress(from, Display_Name);
                mail.To.Add(to);

                mail.ReplyToList.Add(ReplyTo);

                //mail.CC.Add(cc);
                //mail.Bcc.Add(bcc);

                if (AttachmentFilePath != null)
                {
                    mail.Attachments.Add(new Attachment(AttachmentFilePath));
                }

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential("AKIAXYTBBQW7NESSSNW7", "BIM4ixk6XPWoQV8MstZFIK7iY18HRW0zrW8wqfGS9NGF");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                mail.Attachments.Dispose();
                SmtpServer.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $@"Exception-" + ex.ToString());
                SmtpServer.Dispose();
            }
            //finally
            //{
            //    if (File.Exists(AttachmentFilePath))
            //    {
            //        File.Delete(AttachmentFilePath);
            //    }
            //}
        }
    }
}