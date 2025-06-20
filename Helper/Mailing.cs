using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace Facelift_App.Helper
{
    public class Mailing
    {
        public string smtp_from = ConfigurationManager.AppSettings["smtp_from"].ToString();
        public string smtp_from_alias = ConfigurationManager.AppSettings["smtp_from_alias"].ToString();
        public string smtp_username = ConfigurationManager.AppSettings["smtp_username"].ToString();
        public string smtp_password = ConfigurationManager.AppSettings["smtp_password"].ToString();
        public string smtp_host = ConfigurationManager.AppSettings["smtp_host"].ToString();
        public int smtp_port = Convert.ToInt32(ConfigurationManager.AppSettings["smtp_port"].ToString());


        private static Logger logger = LogManager.GetCurrentClassLogger();


        public void SendEmail(List<string> recipients, String subject, String body, List<Attachment> attachments = null)
        {
            using (var mail = new SmtpClient())
            {
                mail.Host = smtp_host;
                mail.Port = smtp_port;
                mail.DeliveryMethod = SmtpDeliveryMethod.Network;
                mail.Credentials =
                   new NetworkCredential(smtp_username, smtp_password);
                mail.EnableSsl = false;

                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                message.From = new MailAddress(smtp_username, smtp_from_alias);
                
                foreach(string recipient in recipients)
                {
                    message.To.Add(recipient);
                }
                
                message.Subject = subject;
                message.Body = body;

                if(attachments != null)
                {
                    foreach (Attachment att in attachments)
                    {
                        message.Attachments.Add(att);
                    }

                }

                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.Priority = MailPriority.High;

                try
                {
                    //await mail.SendMailAsync(message);
                    mail.Send(message);
                    logger.Info(string.Format("SendEmail - {0} sent to {1}", subject, string.Join(";", recipients)));
                }
                catch (SmtpException ex)
                {
                    String errMsg = string.Format("SendEmail - SmtpException : {0}", ex.Message);
                    logger.Error(errMsg);
                }
                catch (Exception ex)
                {
                    String errMsg = string.Format("SendEmail - SmtpException : {0}", ex.Message);
                    logger.Error(errMsg);
                }
            }
        }

    }
}