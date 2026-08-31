using System;
using System.Collections.Generic;
using System.Text;

namespace GoodRxProofMailer
{
    internal class DataProcess
    {
        string FilePath;
        public string ProofPath;
        public string WorkingDirectory;
        public string PdfOutputPath;
        public bool DidProofsGenerate;
        public string ZengerJob;
        public string MailingSegment;
        public List<string> LeadSourceNames;
        public DataProcess(string filePath)
        {
            ParseFilePath(filePath);
            LeadSourceNames = new List<string>();
            WorkingDirectory = GetWorkingPath(filePath);
            PdfOutputPath = Path.Combine(Path.GetDirectoryName(WorkingDirectory), "PDF Proofs");
            ProofPath = GetProofPath(filePath);
        }
        private void ParseFilePath(string filePath)
        {
            FilePath = filePath;
            string[] splitFileName = Path.GetFileNameWithoutExtension(filePath).Split("_");
            ZengerJob = splitFileName[0];
            MailingSegment = splitFileName[1];
        }
        private string GetProofPath(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string[] splitName = name.Split('_');
            return Path.Combine(WorkingDirectory, String.Format("{0}_{1}_Proofs.csv", splitName[0], splitName[1]));
        }
        private string GetWorkingPath(string filePath)
        {
            string parentPath = Path.GetDirectoryName(filePath);
            if (Path.GetFileName(parentPath) == "Data") return parentPath;
            else return Path.GetDirectoryName(Path.GetDirectoryName(parentPath));
        }
        public bool GenerateProofFile()
        {
            try 
            {
                string[] SortedLines = SortLines(GetLines());
                if (SortedLines == null) return false;
                StreamWriter sWriter = new StreamWriter(ProofPath);
                for(int i = 0; i < SortedLines.Length; i++) sWriter.WriteLine(SortedLines[i]);
                sWriter.Close();
                File.Copy(ProofPath, Path.Combine(Configurator.PslHotfolder, Path.GetFileName(ProofPath)), true);
                Logger.WriteLog("Made it here, though", false);
                return true;
            }
            catch (Exception e) 
            {
                Logger.WriteLog(e.Message, false);
                return false; 
            }
        }
        private List<string> GetLines()
        {
            try
            {
                List<string> lines = new List<string>();
                StreamReader sReader = new StreamReader(FilePath);
                while (!sReader.EndOfStream)
                {
                    string line = sReader.ReadLine();
                    if (line == null) continue;
                    if (line.Contains("\"ll\"")) lines.Add(line);
                }
                sReader.Close();
                if (lines.Count == 0) return null;
                else return lines;
            }
            catch { return null; }
        }
        private string[] SortLines(List<string> unsortedLines)
        {
            if (unsortedLines == null) return null;
            List<List<string>> sortingBox = new List<List<string>>();
            List<string> sortedLines = new List<string>();
            for(int i = 0; i < unsortedLines.Count; i++)
            {
                string lineDJC = unsortedLines[i].Split(",")[12];
                bool found = false;
                foreach (List<string> sort in sortingBox)
                {
                    if (sort[0].Contains(lineDJC))
                    {
                        sort.Add(unsortedLines[i]);
                        found = true;
                    }
                }
                if (!found)
                {
                    sortingBox.Add(new List<string> { unsortedLines[i] });
                    LeadSourceNames.Add(unsortedLines[i].Split(",")[19]);
                }
            }
            return OrderLines(sortingBox);
        }
        private string[] OrderLines(List<List<string>> sortedLines)
        {
            try
            {
                int totalCount = 0;
                for (int i = 0; i < sortedLines.Count; i++) totalCount += sortedLines[i].Count;
                List<string> orderedLines = new List<string>();
                while (orderedLines.Count != totalCount)
                {
                    int lowestIndex = -1;
                    for (int i = 0; i < sortedLines.Count; i++)
                    {
                        if (lowestIndex == -1) lowestIndex = i;
                        else if (String.Compare(sortedLines[i][0].Split(",")[12], sortedLines[lowestIndex][0].Split(",")[12], StringComparison.OrdinalIgnoreCase) < 0) lowestIndex = i;
                    }
                    foreach (string line in sortedLines[lowestIndex])
                    {
                        orderedLines.Add(line);
                    }
                    if(lowestIndex >=0) sortedLines.RemoveAt(lowestIndex);
                }
                return orderedLines.ToArray();
            }
            catch (Exception e)
            {
                Logger.WriteLog(e.Message, false);
                return null;
            }
        }
    }
}
