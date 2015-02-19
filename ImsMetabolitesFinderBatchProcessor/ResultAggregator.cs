namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

        public static string SummarizeResult(IDictionary<IonizationMethod, MoleculeInformedWorkflowResult> chemicalResult, IonizationMethod ionization)
        {
            string result = "Nah";
            if (chemicalResult.ContainsKey((ionization)))
            {
                MoleculeInformedWorkflowResult workflowResult = chemicalResult[ionization];
                result = workflowResult.AnalysisStatus.ToString();

                // // Print out mobility instead of POS. You can change this at will.
                // if (result == "POS")
                // {
                //     result = String.Format("{0:0.00}", workflowResult.Mobility);
                // }
            }
            return result;
        }

        // process result files collected and generate a final report.
        public void ProcessResultFiles(string analysisDirectory)
        {
            foreach (var task in this.Tasks)
            {
                try
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
                catch (Exception e)
                {
                    Console.WriteLine("Result processing for {0} failed", task.DataSetName);
                    Console.WriteLine("Exception: {0}", e.Message);
                    Console.WriteLine("");
                }
            }
        }
    }
}
