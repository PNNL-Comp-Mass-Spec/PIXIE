namespace ImsMetabolitesFinderBatchProcessor
{
    using System.Collections.Generic;
    using System.Diagnostics;

    [System.ComponentModel.DesignerCategory("Code")]
    public class ImsInfomredProcess : Process
    {
        public int JobID { get; private set; }
        public string DataSetName { get; set; }
        public bool Done { get; set; }
        public HashSet<string> FileResources { get; set; }


        public ImsInfomredProcess(int ID, string name, string exe, string arguments, bool shell)
        {
            this.JobID = ID;
            this.DataSetName = name;
            this.StartInfo.FileName = exe;
            this.StartInfo.Arguments = arguments;
            this.StartInfo.UseShellExecute = shell;
            this.StartInfo.CreateNoWindow = !shell;
            this.FileResources = new HashSet<string>();
            this.Done = false;
        }

        public bool AreResourcesFree(IEnumerable<ImsInfomredProcess> tasks)
        {
            bool free = true;
            foreach (var task in tasks)
            {
                HashSet<string> sharedResources = new HashSet<string>(this.FileResources);
                sharedResources.IntersectWith(task.FileResources);
                if (sharedResources.Count != 0)
                {
                    free = false;
                    break;
                }
            }
            return free;
        }
    }
}
