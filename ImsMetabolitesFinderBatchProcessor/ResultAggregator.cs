namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Management.Instrumentation;

    using ImsInformed.Domain;

    public class ResultAggregator
    {
        public IDictionary<AnalysisStatus, int> ResultCounter { get; private set; }  

        public IEnumerable<ImsInformedProcess> Tasks { get; private set; }

        public IDictionary<string, ICollection<string>> ChemicalDatasetsMap { get; private set; }

        public IDictionary<string, IDictionary<IonizationMethod, ChemicalBasedAnalysisResult>> ChemicalBasedResultCollection { get; private set; }
        
        public IDictionary<string, IDictionary<IonizationMethod, MoleculeInformedWorkflowResult>> DatasetBasedResultCollection { get; private set; }

        private const double CollisionCrossSectionTolerance = 5;
        private const double NormalizedDriftTimeTolerance = 0.75;

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
            this.ChemicalDatasetsMap = new Dictionary<string, ICollection<string>>();
            this.DatasetBasedResultCollection = new Dictionary<string, IDictionary<IonizationMethod, MoleculeInformedWorkflowResult>>();
            this.ChemicalBasedResultCollection = new Dictionary<string, IDictionary<IonizationMethod, ChemicalBasedAnalysisResult>>();
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

                    if (!this.DatasetBasedResultCollection.ContainsKey(datasetName))
                    {
                        IDictionary<IonizationMethod, MoleculeInformedWorkflowResult> ionizationResult = new Dictionary<IonizationMethod, MoleculeInformedWorkflowResult>();
                        ionizationResult.Add(result.IonizationMethod, result);
                        this.DatasetBasedResultCollection.Add(datasetName, ionizationResult);
                    } 
                    else 
                    {
                        this.DatasetBasedResultCollection[datasetName].Add(result.IonizationMethod, result);
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
            foreach (var chem in this.ChemicalDatasetsMap)
            {
                string chemName = chem.Key;
                Dictionary<IonizationMethod, ChemicalBasedAnalysisResult> dict = new Dictionary<IonizationMethod, ChemicalBasedAnalysisResult>();
                dict.Add(IonizationMethod.ProtonPlus, this.SummarizeResult(chemName, IonizationMethod.ProtonPlus));
                dict.Add(IonizationMethod.ProtonMinus, this.SummarizeResult(chemName, IonizationMethod.ProtonMinus));
                dict.Add(IonizationMethod.SodiumPlus, this.SummarizeResult(chemName, IonizationMethod.SodiumPlus));
                this.ChemicalBasedResultCollection.Add(chemName, dict);
            }
        }

        private ChemicalBasedAnalysisResult SummarizeResult(string chemicalName, IonizationMethod ionization)
        {
            ChemicalBasedAnalysisResult result;
            result.AnalysisStatus = AnalysisStatus.NAH;
            result.ChemicalName = chemicalName;
            result.CrossSectionalArea = 0;
            result.FusionNumber = 0;
            result.IonizationMethod = ionization;
            result.LastVoltageGroupDriftTimeInMs = 0;
            result.MonoisotopicMass = 0;

            if (!this.ChemicalDatasetsMap.Keys.Contains(chemicalName))
            {
                throw new InstanceNotFoundException(chemicalName + " not found in ChemicalDatasetsMap");
            }

            IEnumerable<string> datasets = this.ChemicalDatasetsMap[chemicalName];
            foreach (string dataset in datasets)
            {
                if (this.DatasetBasedResultCollection[dataset].ContainsKey(ionization))
                {
                    MoleculeInformedWorkflowResult workflowResult = this.DatasetBasedResultCollection[dataset][ionization];
                    result = FuseResults(result, workflowResult);
                }
            }
            return result;
        }

        private static ChemicalBasedAnalysisResult InitiateChemicalBasedAnalysisResult(MoleculeInformedWorkflowResult result, string chemName)
        {
            ChemicalBasedAnalysisResult chemicalBasedAnalysisResult;
            chemicalBasedAnalysisResult.AnalysisStatus = result.AnalysisStatus;
            chemicalBasedAnalysisResult.ChemicalName = chemName;
            chemicalBasedAnalysisResult.FusionNumber = 1;
            chemicalBasedAnalysisResult.IonizationMethod = result.IonizationMethod;
            chemicalBasedAnalysisResult.LastVoltageGroupDriftTimeInMs = result.LastVoltageGroupDriftTimeInMs;
            chemicalBasedAnalysisResult.MonoisotopicMass = result.MonoisotopicMass;
            chemicalBasedAnalysisResult.CrossSectionalArea = result.CrossSectionalArea;
            return chemicalBasedAnalysisResult;
        }

        private static bool IsConclusive(AnalysisStatus status)
        {
            if (status == AnalysisStatus.POS || status == AnalysisStatus.NEG)
            {
                return true;
            }

            return false;
        }

        private static ChemicalBasedAnalysisResult FuseResults(ChemicalBasedAnalysisResult result, MoleculeInformedWorkflowResult newWorkflowResult)
        {
            // previous results inconclusive
            if (!IsConclusive(result.AnalysisStatus))
            {
                result = InitiateChemicalBasedAnalysisResult(newWorkflowResult, result.ChemicalName);
                return result;
            }
            // previous results conclusive, new result not conclusive
            else if (!IsConclusive(newWorkflowResult.AnalysisStatus))
            {
                return result;
            }
            // both result conclusive
            else 
            {
                if (CheckConflict(result, newWorkflowResult))
                {
                    result.AnalysisStatus = AnalysisStatus.CON;
                }

                result.CrossSectionalArea = result.CrossSectionalArea * result.FusionNumber + newWorkflowResult.CrossSectionalArea;
                result.CrossSectionalArea /= (result.FusionNumber + 1);

                if (newWorkflowResult.LastVoltageGroupDriftTimeInMs > 0)
                {
                    result.LastVoltageGroupDriftTimeInMs = result.LastVoltageGroupDriftTimeInMs * result.FusionNumber + newWorkflowResult.LastVoltageGroupDriftTimeInMs;
                    result.LastVoltageGroupDriftTimeInMs /= (result.FusionNumber + 1);
                }

                result.FusionNumber++;
                return result;
            }
        }

        // Check if there are conflicts in 
        private static bool CheckConflict(ChemicalBasedAnalysisResult result, MoleculeInformedWorkflowResult newWorkflowResult)
        {
            if (newWorkflowResult.IonizationMethod != result.IonizationMethod)
            {
                throw new InvalidOperationException("Cannot check conflict for results from different chemicals or with different ionization methods");
            }

            if (result.AnalysisStatus != newWorkflowResult.AnalysisStatus)
            {
                return true;
            }

            if (Math.Abs(result.CrossSectionalArea - newWorkflowResult.CrossSectionalArea) > CollisionCrossSectionTolerance)
            {
                return true;
            }

            if (Math.Abs(result.LastVoltageGroupDriftTimeInMs - newWorkflowResult.LastVoltageGroupDriftTimeInMs) > NormalizedDriftTimeTolerance)
            {
                result.AnalysisStatus = AnalysisStatus.CON;
            }

            return false;
        }
    }
}
