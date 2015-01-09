namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    using ImsInformed.Domain;

    public class ResultAggregator
    {
        public IEnumerable<ImsInformedProcess>  Tasks { get; private set; }
        
        public IDictionary<string, IDictionary<IonizationMethod, MoleculeInformedWorkflowResult>> ResultCollection { get; private set; }

        public ResultAggregator(IEnumerable<ImsInformedProcess> processes)
        {
            this.Tasks = processes;
            ResultCollection = new Dictionary<string, IDictionary<IonizationMethod, MoleculeInformedWorkflowResult>>();
        }

        // process result files collected and generate a final report.
        public void ProcessResultFiles(string analysisDirectory)
        {
            foreach (var task in this.Tasks)
            {
                // Dispose the task as it is no longer used.
                MoleculeInformedWorkflowResult result = task.DeserializeResultBinFile();

                // Remove the pos, neg signatures from dataset name.
                // string chemicalName = result.DatasetName.Replace("pos", "");
                // chemicalName = chemicalName.Replace("neg", "");

                string chemicalName = result.DatasetName;

                if (!this.ResultCollection.ContainsKey(chemicalName))
                {
                    IDictionary<IonizationMethod, MoleculeInformedWorkflowResult> ionizationResult = new Dictionary<IonizationMethod, MoleculeInformedWorkflowResult>();
                    ionizationResult.Add(result.IonizationMethod, result);
                    ResultCollection.Add(chemicalName, ionizationResult);
                } 
                else 
                {
                    ResultCollection[chemicalName].Add(result.IonizationMethod, result);
                }
                task.Dispose();
            }
        }
    }
}
