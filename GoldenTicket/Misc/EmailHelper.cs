using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace GoldenTicket.Misc
{
    /**
     * <summary>
     * Sends emails! Relies on the Web.config settings: 
     * -SmtpyAddress
     * -SmtpPort
     * -SmtpUsername
     * -SmtpPassword
     * -MailFrom
     * </summary>
     */
    public class EmailHelper
    {
        /**
         * <summary>Sends an email</summary>
         * <param name="toAddress">Address to send the email to</param>
         * <param name="fromAddress">Reply to address for the email</param>
         * <param name="subject">Subject line for the email</param>
         * <param name="messageBody">Body of the email</param>
         */
        public static void SendEmail(string toAddress, string fromAddress, string subject, string messageBody)
        {
            // Get the configuration settings
            var mailServerAddress = ConfigurationManager.AppSettings["SmtpAddress"];
            var mailServerPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            var mailServerUsername = ConfigurationManager.AppSettings["SmtpUsername"];
            var mailServerPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            
            // Setup the SMTP connection 
            var mailClient = new SmtpClient(mailServerAddress, mailServerPort);
            mailClient.Credentials = new NetworkCredential(mailServerUsername, mailServerPassword);

            // Send the message
            var mailMessage = new MailMessage(to: toAddress, @from: fromAddress, subject: subject, body: messageBody);
            mailMessage.IsBodyHtml = true;
            mailClient.Send(mailMessage);
        }

        /**
         * <summary>Sends an email. Uses the settings "MailFrom" address as the reply to address.</summary>
         * <param name="toAddress">Address to send the email to</param>
         * <param name="subject">Subject line for the email</param>
         * <param name="messageBody">Body of the email</param>
         */
        public static void SendEmail(string toAddress, string subject, string messageBody)
        {
            SendEmail(toAddress, ConfigurationManager.AppSettings["MailFrom"], subject, messageBody);
        }

    }
}