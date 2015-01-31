namespace ImsMetabolitesFinderBatchProcessor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using ImsInformed.Domain;

    [System.ComponentModel.DesignerCategory("Code")]
    public class ImsInformedProcess : Process
    {
        public int JobID { get; private set; }
        public string DataSetName { get; set; }
        public bool Done { get; set; }
        public HashSet<string> FileResources { get; set; }
        public int LineNumber { get; private set; }
        public string ResultBinFile { get; private set; }

        public ImsInformedProcess(int ID, string name, string exe, string arguments, bool shell, string resultBinFile, int lineNumber)
        {
            this.JobID = ID;
            this.DataSetName = name;
            this.StartInfo.FileName = exe;
            this.StartInfo.Arguments = arguments;
            this.StartInfo.UseShellExecute = shell;
            this.StartInfo.CreateNoWindow = !shell;
            this.FileResources = new HashSet<string>();
            this.Done = false;
            this.ResultBinFile = resultBinFile;
            this.LineNumber = lineNumber;
        }

        public bool AreResourcesFree(IEnumerable<ImsInformedProcess> tasks)
        {
            bool free = true;
            foreach (var task in tasks)
            {
                if (!task.Done)
                {
                    HashSet<string> sharedResources = new HashSet<string>(this.FileResources);
                    sharedResources.IntersectWith(task.FileResources);
                    if (sharedResources.Count != 0)
                    {
                        free = false;
                        break;
                    }
                }
            }
            return free;
        }

        public MoleculeInformedWorkflowResult DeserializeResultBinFile()
        {
            MoleculeInformedWorkflowResult result;
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(this.ResultBinFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = (MoleculeInformedWorkflowResult) formatter.Deserialize(stream);
            }
            return result;
        }
    }
}
