using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace GoodRxProofMailer
{
    public static class Configurator
    {
        public static string LogPath = ConfigurationManager.AppSettings["LogPath"];
        public static string DefaultDirectory = ConfigurationManager.AppSettings["DefaultDirectory"];
        public static string PslPath = ConfigurationManager.AppSettings["PslPath"];
        public static string PslHotfolder = ConfigurationManager.AppSettings["PslHotfolder"];
        public static string PslOutput = ConfigurationManager.AppSettings["PslOutput"];
        public static string PslArchive = ConfigurationManager.AppSettings["PslArchive"];
        public static string PslScript = ConfigurationManager.AppSettings["PslScript"];
        public static string PslWorkingPath = ConfigurationManager.AppSettings["PslWorkingPath"];
        public static string MailAccount = ConfigurationManager.AppSettings["MailAccount"];
        public static string MailSecret = ConfigurationManager.AppSettings["MailSecret"];
        public static string[] MailRecipients = ConfigurationManager.AppSettings["MailRecipients"].Split("|");
    }
}
