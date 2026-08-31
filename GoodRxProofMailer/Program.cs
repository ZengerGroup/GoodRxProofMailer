using System.Windows.Forms;

namespace GoodRxProofMailer
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Logger.Display("Beginning processing.", true);
            using OpenFileDialog filePicker = new OpenFileDialog();
            filePicker.Title = "Select data file.";
            filePicker.Filter = "Comma Seperated Values (*.csv)|*.csv|All Files (*.*)|*.*";
            filePicker.InitialDirectory = Configurator.DefaultDirectory;

            DialogResult result = filePicker.ShowDialog();
            if (result == DialogResult.OK)
            {
                DataProcess Processor = new DataProcess(filePicker.FileName);
                if (Processor.GenerateProofFile())
                {
                    PslManager PSL = new PslManager(Processor.PdfOutputPath);
                    Logger.Display("Starting PSL", false);
                    PSL.Start();
                    while (!PSL.ProcessingComplete())
                    {
                        Thread.Sleep(30000);
                        Logger.WriteLog("Psl is still running...", false);
                    }
                    if (PSL.PslCompleted)
                    {
                        if (PSL.MoveOutput(Processor.PdfOutputPath))
                        {
                            Mailer ProofMailer = new Mailer();
                            try { ProofMailer.SendMail(Processor.ZengerJob, Processor.MailingSegment, Processor.LeadSourceNames.ToArray(), PSL.FinalPath); }
                            catch
                            {
                                Logger.Display("Failed to send proof email. Press any key to exit the application.", false);
                                Console.ReadKey();
                            }
                        }
                        else
                        {
                            Logger.Display("Failed to move PSL output, check hotfolder output. Press any key to exit the application.", false);
                            Console.ReadKey();
                        }
                    }
                    else
                    {
                        Logger.Display("PSL processing failed. Please check status and retry. Press any key to exit the application.", false);
                        Console.ReadKey();
                    }
                }
                else
                {
                    Logger.Display("Failed to generate proof files. Press any key to exit the application.", false);
                    Console.ReadKey();
                }
            }
            else
            {
                Logger.Display("No file selected. Press any key to exit the application.", false);
                Console.ReadKey();
            }
        }
    }
}
