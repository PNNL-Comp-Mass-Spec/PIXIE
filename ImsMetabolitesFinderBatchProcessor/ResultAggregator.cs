namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public class ResultAggregator
    {
        public IEnumerable<ImsInformedProcess>  Tasks { get; private set; }

        public ResultAggregator(IEnumerable<ImsInformedProcess> processes)
        {
            this.Tasks = processes;
        }

        // process result files collected and generate a final report.
        public void ProcessResultFiles(string analysisDirectory)
        {
            using (StreamWriter aggregatedResultFile = File.CreateText("analysis_summary_" + DateTime.Now + ".txt"))
            {
                foreach (var task in this.Tasks)
                {
                    
                }
            }

            
        }
    }
}
