using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace GoldenTicket.Misc
{
    public class EmailHelper
    {
        public static void SendEmail(string toAddress, string fromAddress, string subject, string messageBody)
        {
            // Setup the server
            var mailServerAddress = ConfigurationManager.AppSettings["SmtpAddress"];
            var mailServerPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            var mailServerUsername = ConfigurationManager.AppSettings["SmtpUsername"];
            var mailServerPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            
            var mailClient = new SmtpClient(mailServerAddress, mailServerPort);
            mailClient.Credentials = new NetworkCredential(mailServerUsername, mailServerPassword);

            var mailMessage = new MailMessage(to: toAddress, @from: fromAddress, subject: subject, body: messageBody);
            mailMessage.IsBodyHtml = true;
            mailClient.Send(mailMessage);
        }

        public static void SendEmail(string toAddress, string subject, string messageBody)
        {
            SendEmail(toAddress, ConfigurationManager.AppSettings["MailFrom"], subject, messageBody);
        }

    }
}