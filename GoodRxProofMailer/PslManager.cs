using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GoodRxProofMailer
{
    internal class PslManager
    {
        public string ProofPath;
        public string FinalPath;
        public bool PslCompleted;
        Process PslProcess;
        private int PslCheckedCount;
        public PslManager(string proofPath)
        {
            ProofPath = proofPath;
            PslCheckedCount = 0;
            PslCompleted = false;
        }
        public bool ProcessingComplete()
        {
            if(PslCheckedCount == 5)
            {
                PslProcess.Kill();
                return true;
            }
            else
            {
                if (Directory.GetFiles(Configurator.PslHotfolder, "*.csv").Length > 0) 
                {
                    PslCheckedCount++;
                    return false; 
                }
                else
                {
                    PslCompleted = true;
                    PslProcess.Kill();
                    return true;
                }
            }
        }
        public void Start()
        {
            try
            {
                PslProcess = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Configurator.PslPath;
                info.RedirectStandardInput = true;
                info.RedirectStandardOutput = true;
                info.Arguments = Configurator.PslScript;
                info.WorkingDirectory = Configurator.PslWorkingPath;
                PslProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == "Unable to open status file: 32")
                        {
                            PslProcess.Kill();
                            Start(1);
                        }
                    };
                PslProcess.StartInfo = info;
                PslProcess.Start();

            }
            catch
            {
                Logger.WriteLog("Failed to start PSL. Attempt 1 of 5.", false);
            }
        }
        public void Start(int failedAttempts)
        {
            try
            {
                if (failedAttempts == 5) throw new Exception();
                PslProcess = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Configurator.PslPath;
                info.RedirectStandardInput = true;
                info.Arguments = Configurator.PslScript;
                info.WorkingDirectory = Configurator.PslWorkingPath;
                PslProcess.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == "Unable to open status file: 32")
                    {
                        PslProcess.Kill();
                        Start(failedAttempts + 1);
                    }
                };
                PslProcess.StartInfo = info;
                PslProcess.Start();
            }
            catch
            {
                Logger.WriteLog("Failed to start PSL. Attempt {0} of 5.", false, (++failedAttempts).ToString());
            }
        }
        public bool MoveOutput(string workingDirectory)
        {
            try 
            {
                string[] outputFiles = Directory.GetFiles(Configurator.PslOutput, "*.pdf");
                if (outputFiles.Length == 0)
                {
                    Logger.Display("No PSL output files found, check data integrity.", false);
                    return false;
                }
                for(int i = 0; i < outputFiles.Length; i++)
                {
                    File.Copy(outputFiles[i], Path.Combine(workingDirectory, Path.GetFileName(outputFiles[i])), true);
                    FinalPath = Path.Combine(Configurator.PslArchive, Path.GetFileName(outputFiles[i]));
                    File.Move(outputFiles[i], FinalPath, true);
                }
                return true; 
            }
            catch (Exception e)
            {
                Logger.WriteLog(e.Message, false);
                return false; 
            }
        }
    }
}
