// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultAggregator.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the ResultAggregator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Instrumentation;
    using System.Runtime.InteropServices;

    using ImsInformed.Domain;

    /// <summary>
    /// The result aggregator.
    /// </summary>
    public class ResultAggregator
    {
        /// <summary>
        /// The collision cross section tolerance.
        /// </summary>
        private const double CollisionCrossSectionTolerance = 5;

        /// <summary>
        /// The normalized drift time tolerance.
        /// </summary>
        private const double NormalizedDriftTimeTolerance = 0.75;

        /// <summary>
        /// The normalized drift time tolerance.
        /// </summary>
        private bool empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultAggregator"/> class.
        /// </summary>
        /// <param name="processes">
        /// The processes.
        /// </param>
        public ResultAggregator(IEnumerable<ImsInformedProcess> processes)
        {
            IList<IonizationMethod> method = new List<IonizationMethod>();
            method.Add(IonizationMethod.ProtonPlus);
            method.Add(IonizationMethod.ProtonMinus);
            method.Add(IonizationMethod.SodiumPlus);
            method.Add(IonizationMethod.Proton2MinusSodiumPlus);
            method.Add(IonizationMethod.APCI);
            method.Add(IonizationMethod.HCOOMinus);
            this.SupportedIonizationMethods = method;

            this.Tasks = processes;
            this.empty = true;
            this.ResultCounter = new Dictionary<AnalysisStatus, int>
                                     {
                                         { AnalysisStatus.POS, 0 },
                                         { AnalysisStatus.ERR, 0 },
                                         { AnalysisStatus.NSP, 0 },
                                         { AnalysisStatus.TAR, 0 },
                                         { AnalysisStatus.NEG, 0 },
                                         { AnalysisStatus.REJ, 0 },
                                         { AnalysisStatus.MassError, 0 }
                                     };
            this.ChemicalDatasetsMap = new Dictionary<string, ICollection<string>>();
            this.DatasetBasedResultCollection = new Dictionary<string, IDictionary<IonizationMethod, MoleculeInformedWorkflowResult>>();
            this.ChemicalBasedResultCollection = new Dictionary<string, IDictionary<IonizationMethod, ChemicalBasedAnalysisResult>>();
        }

        /// <summary>
        /// Gets the result counter.
        /// </summary>
        public IDictionary<AnalysisStatus, int> ResultCounter { get; private set; }

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        public IEnumerable<ImsInformedProcess> Tasks { get; private set; }

        /// <summary>
        /// Map chemical name to dataset names, e.g. EXP-ABC_pos_Sept-15 and EXP-ABC_pos_Oct-16 both maps to EXP-ABC
        /// </summary>
        public IDictionary<string, ICollection<string>> ChemicalDatasetsMap { get; private set; }

        /// <summary>
        /// Gets the chemical based result collection.
        /// </summary>
        public IDictionary<string, IDictionary<IonizationMethod, ChemicalBasedAnalysisResult>> ChemicalBasedResultCollection { get; private set; }

        /// <summary>
        /// Gets the dataset based result collection.
        /// </summary>
        public IDictionary<string, IDictionary<IonizationMethod, MoleculeInformedWorkflowResult>> DatasetBasedResultCollection { get; private set; }

        /// <summary>
        /// Gets the dataset based result collection.
        /// </summary>
        public IEnumerable<IonizationMethod> SupportedIonizationMethods { get; private set; }

        /// <summary>
        /// process result files collected and generate a final report.
        /// </summary>
        /// <param name="analysisDirectory">
        /// The analysis directory.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
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
                    Console.WriteLine(string.Empty);
                }
            }

            foreach (var chem in this.ChemicalDatasetsMap)
            {
                string chemName = chem.Key;
                Dictionary<IonizationMethod, ChemicalBasedAnalysisResult> dict = new Dictionary<IonizationMethod, ChemicalBasedAnalysisResult>();
                foreach (var item in this.SupportedIonizationMethods)
                {
                    dict.Add(item, this.SummarizeResult(chemName, item));
                }

                this.ChemicalBasedResultCollection.Add(chemName, dict);
            }

            this.empty = false;
        }

        /// <summary>
        /// The summarize result chemical based. Summarize all ionization modes.
        /// </summary>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <param name="summaryFuntion">
        /// The summary funtion.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public void SummarizeResultChemicalBased(string outputPath, Func<ChemicalBasedAnalysisResult, string> summaryFuntion, string description)
        {
            this.SummarizeResultChemicalBased(outputPath, summaryFuntion, description, this.SupportedIonizationMethods);
        }

        /// <summary>
        /// Export the analyses result on a per chemical basis for all ionization method.
        /// </summary>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <param name="summaryFuntion">
        /// The summary funtion.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="ionizationsOfInterest">
        /// The ionizations Of Interest.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public void SummarizeResultChemicalBased(string outputPath, Func<ChemicalBasedAnalysisResult, string> summaryFuntion, string description, IEnumerable<IonizationMethod> ionizationsOfInterest, bool hiearchicalFormat = true)
        {
            if (this.empty)
            {
                throw new InvalidOperationException("Please call ProcessResultFiles(string analysisDirectory) first to aggregate the result before exporting");
            }

            string resultFilePath = outputPath;
            using (FileStream resultFile = new FileStream(resultFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(resultFile))
                {
                    writer.WriteLine(description);
                    foreach (var item in this.ChemicalBasedResultCollection)
                    {
                        IList<string> results = new List<string>();
                        
                        foreach (var ionization in ionizationsOfInterest)
                        {
                            if (item.Value.ContainsKey(ionization))
                            {
                                string result = summaryFuntion(item.Value[ionization]);
                                
                                if (!String.IsNullOrEmpty(result))
                                {
                                    results.Add("    " + result);
                                }
                            }
                        }

                        // Format like
                        // LIP-DHA:
                        //     C26H53NO3[M-H] 427.4025 0.5000 30.68 1
                        //     C26H53NO3[M+HCOO] 473.4080 0.5000 32.15 1
                        // LIP-EICO20-1:
                        //     C20H38O2[M-H] 310.2872 0.5000 26.44 1
                        if (results.Count > 0 && hiearchicalFormat)
                        {
                            writer.WriteLine(item.Key + ":");
                            foreach (string result in results)
                            {
                                writer.WriteLine(result);
                            }
                        }
                        // Format like
                        //    LIP-DHA C26H53NO3[M-H], 427.4025, 0.5000, 30.68, 1
                        //    LIP-DHA C26H53NO3[M+HCOO], 473.4080, 0.5000, 32.15, 1
                        //    LIP-EICO20-1 C20H38O2[M-H], 310.2872, 0.5000, 26.44, 1
                        else if (results.Count > 0)
                        {
                            foreach (string result in results)
                            {
                                writer.Write(item.Key + "\t");
                                writer.WriteLine(result);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The summarize result.
        /// </summary>
        /// <param name="chemicalName">
        /// The chemical name.
        /// </param>
        /// <param name="ionization">
        /// The ionization.
        /// </param>
        /// <returns>
        /// The <see cref="ChemicalBasedAnalysisResult"/>.
        /// </returns>
        /// <exception cref="InstanceNotFoundException">
        /// </exception>
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
            result.TargetDescriptor = String.Empty;

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

        /// <summary>
        /// The initiate chemical based analysis result.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <param name="chemName">
        /// The chem name.
        /// </param>
        /// <returns>
        /// The <see cref="ChemicalBasedAnalysisResult"/>.
        /// </returns>
        private static ChemicalBasedAnalysisResult InitializeChemicalBasedAnalysisResult(MoleculeInformedWorkflowResult result, string chemName)
        {
            ChemicalBasedAnalysisResult chemicalBasedAnalysisResult;
            chemicalBasedAnalysisResult.AnalysisStatus = result.AnalysisStatus;
            chemicalBasedAnalysisResult.ChemicalName = chemName;
            chemicalBasedAnalysisResult.FusionNumber = 1;
            chemicalBasedAnalysisResult.IonizationMethod = result.IonizationMethod;
            chemicalBasedAnalysisResult.LastVoltageGroupDriftTimeInMs = result.LastVoltageGroupDriftTimeInMs;
            chemicalBasedAnalysisResult.MonoisotopicMass = result.MonoisotopicMass;
            chemicalBasedAnalysisResult.CrossSectionalArea = result.CrossSectionalArea;
            chemicalBasedAnalysisResult.TargetDescriptor = result.TargetDescriptor;
            return chemicalBasedAnalysisResult;
        }

        /// <summary>
        /// The is conclusive.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool IsConclusive(AnalysisStatus status)
        {
            if (status == AnalysisStatus.POS || status == AnalysisStatus.NEG)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The fuse results.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <param name="newWorkflowResult">
        /// The new workflow result.
        /// </param>
        /// <returns>
        /// The <see cref="ChemicalBasedAnalysisResult"/>.
        /// </returns>
        private static ChemicalBasedAnalysisResult FuseResults(ChemicalBasedAnalysisResult result, MoleculeInformedWorkflowResult newWorkflowResult)
        {
            // previous results inconclusive
            if (!IsConclusive(result.AnalysisStatus))
            {
                result = InitializeChemicalBasedAnalysisResult(newWorkflowResult, result.ChemicalName);
                return result;
            }

            if (!IsConclusive(newWorkflowResult.AnalysisStatus)) 
            {
                // previous results conclusive, new result not conclusive
                return result;
            }

            // both result conclusive
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

        /// <summary>
        /// Check if there are conflicts in 
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <param name="newWorkflowResult">
        /// The new workflow result.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// </exception>
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
