namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;

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
            string summaryFileDir = Path.Combine(analysisDirectory, "analysis_summary_" + DateTime.Now + ".txt");
            using (StreamWriter aggregatedResultFile = File.CreateText(summaryFileDir))
            {
                foreach (var task in this.Tasks)
                {
                    // Dispose the task as it is no longer used.
                    task.Dispose();
                }
            }
        }
    }
}
