// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultAggregator.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the ResultAggregator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace IFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using IFinderBatchProcessor.Export;
    using IFinderBatchProcessor.Util.DeviceModule.Util;

    using ImsInformed;
    using ImsInformed.Targets;
    using ImsInformed.Workflows.CrossSectionExtraction;

    /// <summary>
    /// The result aggregator.
    /// </summary>
    public class ResultAggregator
    {
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
            this.DetectedIonizationMethods = new HashSet<IonizationAdduct>();

            this.Tasks = processes;
            this.empty = true;
            this.ResultCounter = new Dictionary<AnalysisStatus, int>
                                     {
                                         { AnalysisStatus.Positive, 0 },
                                         { AnalysisStatus.UknownError, 0 },
                                         { AnalysisStatus.NotSufficientPoints, 0 },
                                         { AnalysisStatus.TargetError, 0 },
                                         { AnalysisStatus.Negative, 0 },
                                         { AnalysisStatus.Rejected, 0 },
                                         { AnalysisStatus.MassError, 0 }
                                     };
            this.ChemicalDatasetsMap = new Dictionary<string, ICollection<string>>();
            this.DatasetBasedResultCollection = new Dictionary<string, IDictionary<IImsTarget, CrossSectionWorkflowResult>>();
            this.ChemicalBasedResultCollection = new Dictionary<string, IDictionary<IonizationAdduct, ChemicalBasedAnalysisResult>>();
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
        /// Gets the chemical based result collection. where the string is the chemical identifier.
        /// </summary>
        public IDictionary<string, IDictionary<IonizationAdduct, ChemicalBasedAnalysisResult>> ChemicalBasedResultCollection { get; private set; }

        /// <summary>
        /// Gets the dataset based result collection, where the string is the dataset name.
        /// </summary>
        public IDictionary<string, IDictionary<IImsTarget, CrossSectionWorkflowResult>> DatasetBasedResultCollection { get; private set; }

        /// <summary>
        /// Gets the dataset based result collection.
        /// </summary>
        public HashSet<IonizationAdduct> DetectedIonizationMethods { get; private set; }

        /// <summary>
        /// process result files collected and generate the CCS/mz DB.
        /// </summary>
        /// <param name="analysisDirectory">
        /// The analysis directory.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public void ProcessResultFiles(string analysisDirectory, AnalysisLibrary lib)
        {
            foreach (var task in this.Tasks)
            {
                // Deserialize the results and dispatch them into lookup tables.
                try
                {
                    IList<CrossSectionWorkflowResult> results = task.DeserializeResultBinFile();
                    
                    AsyncHelpers.RunSync(() => lib.InsertResult(results));

                    foreach (var result in results)
                    {
                        if (!this.DetectedIonizationMethods.Contains(result.Target.Adduct))
                        {
                            this.DetectedIonizationMethods.Add(result.Target.Adduct);
                        }

                        var analysisResult = result.AnalysisStatus;
                        if (!this.ResultCounter.Keys.Contains(analysisResult))
                        {
                            throw new Exception("analysis result \"" + analysisResult + "\" not recognized.");
                        }

                        this.ResultCounter[result.AnalysisStatus]++;

                        // Remove the pos, neg signatures from dataset name.
                        // string chemicalIdentifier = result.DatasetName.Replace("pos", "");
                        // chemicalIdentifier = chemicalIdentifier.Replace("neg", "");
                        string datasetName = result.DatasetName;

                        if (!this.DatasetBasedResultCollection.ContainsKey(datasetName))
                        {
                            IDictionary<IImsTarget, CrossSectionWorkflowResult> targetResultLookup = new Dictionary<IImsTarget, CrossSectionWorkflowResult>();
                            targetResultLookup.Add(result.Target, result);
                            this.DatasetBasedResultCollection.Add(datasetName, targetResultLookup);
                        } 
                        else 
                        {
                            this.DatasetBasedResultCollection[datasetName].Add(result.Target, result);
                        }

                        string chemName = result.Target.SampleClass;
                        if (!this.ChemicalDatasetsMap.Keys.Contains(chemName))
                        {
                            this.ChemicalDatasetsMap.Add(chemName, new List<string>());
                            this.ChemicalDatasetsMap[chemName].Add(datasetName);
                        }
                        else
                        {
                            if (!this.ChemicalDatasetsMap[chemName].Contains(datasetName))
                            {
                                this.ChemicalDatasetsMap[chemName].Add(datasetName);
                            }
                        }
                    }

                    // Dispose the task as it is no longer used.
                    task.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Result processing for {0} failed", task.DataSetName);
                    Console.WriteLine("Exception: {0}", e.Message);
                    Console.WriteLine(string.Empty);
                }
            }

            // Create the chemical based result
            //foreach (string chemIdentifier in this.ChemicalDatasetsMap.Keys)
            //{
            //    Dictionary<IonizationAdduct, ChemicalBasedAnalysisResult> ionizatonDictionary = new Dictionary<IonizationAdduct, ChemicalBasedAnalysisResult>();
            //    ICollection<string> datasets = this.ChemicalDatasetsMap[chemIdentifier];

            //    // Loop through every target and create the chemical based result collection.
            //    foreach (string dataset in datasets)
            //    {
            //        foreach (IImsTarget target in this.DatasetBasedResultCollection[dataset].Keys)
            //        {
            //            // Only use the target with matching chemical identifier.
            //            if (target.SampleClass == chemIdentifier)
            //            {
            //                CrossSectionWorkflowResult workflowResult = this.DatasetBasedResultCollection[dataset][target];
            //                if (!ionizatonDictionary.ContainsKey(target.Adduct))
            //                {
                                
            //                    ionizatonDictionary.Add(target.Adduct, new ChemicalBasedAnalysisResult(workflowResult));
            //                }
            //                else
            //                {
            //                    ChemicalBasedAnalysisResult previousResult = ionizatonDictionary[target.Adduct];
            //                    ionizatonDictionary[target.Adduct] = new ChemicalBasedAnalysisResult(previousResult, workflowResult);
            //                }
            //            }
            //        }
            //    }

            //    this.ChemicalBasedResultCollection.Add(chemIdentifier, ionizatonDictionary);
            //}

            this.empty = false;
        }

        /// <summary>
        /// The summarize result dataset based.
        /// </summary>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <param name="summaryFunction">
        /// The summary function.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="ionizationsOfInterest">
        /// The ionizations of interest.
        /// </param>
        /// <param name="filter"></param>
        public void SummarizeResultDatasetBased(string outputPath, Func<CrossSectionWorkflowResult, string> summaryFunction, string description, Predicate<CrossSectionWorkflowResult> filter)
        {
            using (FileStream resultFile = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(resultFile))
                {
                    // writer.WriteLine("#[Chemical Name], [Monoisotopic mass], [NET], [Normalized Drift Time], [Charge State]");
                    writer.WriteLine(description);
                    foreach (var dataset in this.DatasetBasedResultCollection.Keys)
                    {
                        foreach (KeyValuePair<IImsTarget, CrossSectionWorkflowResult> item in this.DatasetBasedResultCollection[dataset])
                        {
                            IImsTarget target = item.Key;
                            CrossSectionWorkflowResult result = item.Value;
                            if (filter(result))
                            {
                                string summaryLine = summaryFunction(result);
                                if (!string.IsNullOrEmpty(summaryLine))
                                {
                                    writer.WriteLine(summaryLine);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Export the analyses result on a per chemical basis for all ionization method.
        /// </summary>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <param name="summaryFunction">
        /// The summary function.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="ionizationsOfInterest">
        /// The ionizations Of Interest.
        /// </param>
        /// <param name="hiearchicalFormat">
        /// The hiearchical Format.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public void SummarizeResultChemicalBased(string outputPath, Func<ChemicalBasedAnalysisResult, string> summaryFunction, string description, IEnumerable<IonizationAdduct> ionizationsOfInterest, bool hierarchical = true)
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
                    foreach (string chemicalIdentifier in this.ChemicalBasedResultCollection.Keys)
                    {
                        IList<string> results = new List<string>();
                        
                        foreach (IonizationAdduct ionization in ionizationsOfInterest)
                        {
                            if (this.ChemicalBasedResultCollection[chemicalIdentifier].ContainsKey(ionization))
                            {
                                string result = summaryFunction(this.ChemicalBasedResultCollection[chemicalIdentifier][ionization]);
                                
                                if (!string.IsNullOrEmpty(result))
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
                        if (results.Count > 0 && hierarchical)
                        {
                            writer.WriteLine(chemicalIdentifier + ":");
                            foreach (string result in results)
                            {
                                writer.WriteLine(result);
                            }
                        }

                        // Format like
                        // LIP-DHA C26H53NO3[M-H], 427.4025, 0.5000, 30.68, 1
                        // LIP-DHA C26H53NO3[M+HCOO], 473.4080, 0.5000, 32.15, 1
                        // LIP-EICO20-1 C20H38O2[M-H], 310.2872, 0.5000, 26.44, 1
                        else if (results.Count > 0)
                        {
                            foreach (string result in results)
                            {
                                writer.Write(chemicalIdentifier + "\t");
                                writer.WriteLine(result);
                            }
                        }
                    }
                }
            }
        }
    }
}
