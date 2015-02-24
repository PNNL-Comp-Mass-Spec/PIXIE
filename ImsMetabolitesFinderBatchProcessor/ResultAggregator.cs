namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;

    using ImsInformed.Domain;

    public class ResultAggregator
    {
        public IDictionary<AnalysisStatus, int> ResultCounter { get; private set; }  

        public IEnumerable<ImsInformedProcess>  Tasks { get; private set; }

        public IDictionary<string, ICollection<string>> ChemicalDatasetsMap { get; private set; }
        
        public IDictionary<string, IDictionary<IonizationMethod, MoleculeInformedWorkflowResult>> ResultCollection { get; private set; }

        public ResultAggregator(IEnumerable<ImsInformedProcess> processes)
        {
            this.Tasks = processes;
            this.ResultCounter = new Dictionary<AnalysisStatus, int>();
            this.ResultCounter.Add(AnalysisStatus.POS, 0);
            this.ResultCounter.Add(AnalysisStatus.ERR, 0);
            this.ResultCounter.Add(AnalysisStatus.NSP, 0);
            this.ResultCounter.Add(AnalysisStatus.TAR, 0);
            this.ResultCounter.Add(AnalysisStatus.NEG, 0);
            this.ResultCounter.Add(AnalysisStatus.REJ, 0);
            this.ResultCounter.Add(AnalysisStatus.MassError, 0);
            this.ResultCollection = new Dictionary<string, IDictionary<IonizationMethod, MoleculeInformedWorkflowResult>>();
            this.ChemicalDatasetsMap = new Dictionary<string, ICollection<string>>();
        }

        public static string SummarizeResult(IDictionary<IonizationMethod, MoleculeInformedWorkflowResult> chemicalResult, IonizationMethod ionization, ref bool found)
        {
            string result = "Nah";
            if (chemicalResult.ContainsKey((ionization)))
            {
                MoleculeInformedWorkflowResult workflowResult = chemicalResult[ionization];
                result = workflowResult.AnalysisStatus.ToString();

                // Print out mobility instead of POS. You can change this at will.
                if (workflowResult.AnalysisStatus == AnalysisStatus.POS)
                {
                    found = true;
                }
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
                    var analysisResult = result.AnalysisStatus;
                    if (!this.ResultCounter.Keys.Contains(analysisResult))
                    {
                        throw new Exception("analysis result \"" + analysisResult + "\" not recognized.");
                    }

                    this.ResultCounter[result.AnalysisStatus]++;

                    // Remove the pos, neg signatures from dataset name.
                    // string chemicalName = result.DatasetName.Replace("pos", "");
                    // chemicalName = chemicalName.Replace("neg", "");

                    string datasetName = result.DatasetName;

                    if (!this.ResultCollection.ContainsKey(datasetName))
                    {
                        IDictionary<IonizationMethod, MoleculeInformedWorkflowResult> ionizationResult = new Dictionary<IonizationMethod, MoleculeInformedWorkflowResult>();
                        ionizationResult.Add(result.IonizationMethod, result);
                        ResultCollection.Add(datasetName, ionizationResult);
                    } 
                    else 
                    {
                        ResultCollection[datasetName].Add(result.IonizationMethod, result);
                    }

                    var meta = new DatasetMetadata(datasetName);
                    string chemName = meta.SampleIdentifier;
                    if (!this.ChemicalDatasetsMap.Keys.Contains(chemName))
                    {
                        this.ChemicalDatasetsMap.Add(chemName, new List<string>());
                        this.ChemicalDatasetsMap[chemName].Add(datasetName);
                    }
                    else
                    {
                        this.ChemicalDatasetsMap[chemName].Add(datasetName);
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
