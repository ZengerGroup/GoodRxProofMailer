using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace GoodRxProofMailer
{
    internal class Mailer
    {
        SmtpClient Client;
        MailMessage Message;

        public Mailer()
        {
            Client = ConfigureSMTP();
        }
        public void SendMail(string zgJobNumber, string mailingSegment, string[] leadSourceName, string proofPath)
        {
            try
            {
                Message = ConfigureMessage(Configurator.MailRecipients);
                Message.Subject = BuildSubjectLine(zgJobNumber, mailingSegment, leadSourceName);
                Message.Body = BuildMessage();
                Attachment proofFile = GetProofAttachment(proofPath);
                if (proofFile != null) Message.Attachments.Add(proofFile);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                Client.Send(Message);
            }
            catch (Exception e)
            {
                Logger.WriteLog("Failed to send proof email. Attempt 1 of 5.", false);
                RetrySendMail(1);
            }
        }
        public void RetrySendMail(int failedAttempts)
        {
            try { Client.Send(Message); }
            catch (Exception e)
            {
                Logger.WriteLog("Failed to send proof email. Attempt {1} of 5.", false, (++failedAttempts).ToString());
                if (failedAttempts < 5) RetrySendMail(failedAttempts);
            }
        }
        private SmtpClient ConfigureSMTP()
        {
            SmtpClient smtp = new SmtpClient("smtp.office365.com");
            smtp.TargetName = "STARTTLS/smtp.office365.com";
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(Configurator.MailAccount, Configurator.MailSecret);
            return smtp;
        }
        private MailMessage ConfigureMessage(string[] recipients)
        {
            MailAddress from = new MailAddress(Configurator.MailAccount);
            MailAddress to = new MailAddress(recipients[0]);
            MailMessage message = new MailMessage(from, to);
            for (int i = 1; i < recipients.Length; i++) message.To.Add(recipients[i]);

            message.IsBodyHtml = true;
            return message;
        }
        private string BuildSubjectLine(string zgJobNumber, string mailingSegment, string[] leadSourceName)
        {
            string line = String.Format("{0} GoodRx Simulated Proofs - {1} - LeadSource: ", zgJobNumber, mailingSegment);
            for(int i = 0; i < leadSourceName.Length; i++)
            {
                if (i != 0) line += " & ";
                line += leadSourceName[i];
            }
            return line;
        }

        private string BuildMessage()
        {
            string message = @"Hello,<br/><br/>Attached you will find simulated proofs for your review and approval. All reports have been uploaded to the FTP in their respective folders. "
                + "Please let us know if these are approved to proceed with production and mailing.<br/><br/>Thanks.<br/>Rebecca Hacker<br/>Data & Mailing Services Manager<br/>"
                + "Zenger Group<br/>Smart. Print. Now.<br/>777 East Park Drive<br/>Tonawanda, NY 14150-6708<br/>Direct: 716-566-6052,<br/>rhacker@zenger.com";
            return message;
        }
        private Attachment GetProofAttachment(string proofPath)
        {
            try
            {
                FileStream fStream = new FileStream(proofPath, FileMode.Open, FileAccess.Read);
                return new Attachment(fStream, Path.GetFileName(proofPath), "application/pdf");
            }
            catch 
            { 
                Logger.WriteLog("Failed to attach report file.", false);
                return null;
            }
        }
    }
}
