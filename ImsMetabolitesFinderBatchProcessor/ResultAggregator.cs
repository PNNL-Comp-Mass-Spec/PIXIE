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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Instrumentation;

    using ImsInformed.Domain;
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
                                         { AnalysisStatus.Positive, 0 },
                                         { AnalysisStatus.UknownError, 0 },
                                         { AnalysisStatus.NotSufficientPoints, 0 },
                                         { AnalysisStatus.TargetError, 0 },
                                         { AnalysisStatus.Negative, 0 },
                                         { AnalysisStatus.Rejected, 0 },
                                         { AnalysisStatus.MassError, 0 }
                                     };
            this.ChemicalDatasetsMap = new Dictionary<string, ICollection<string>>();
            this.DatasetBasedResultCollection = new Dictionary<string, IDictionary<IonizationAdduct, CrossSectionWorkflowResult>>();
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
        public IDictionary<string, IDictionary<IonizationAdduct, CrossSectionWorkflowResult>> DatasetBasedResultCollection { get; private set; }

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
                    IList<CrossSectionWorkflowResult> results = task.DeserializeResultBinFile();
                    foreach (var result in results)
                    {
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
                            IDictionary<IonizationAdduct, CrossSectionWorkflowResult> ionizationResult = new Dictionary<IonizationAdduct, CrossSectionWorkflowResult>();
                            ionizationResult.Add(result.Target.Adduct, result);
                            this.DatasetBasedResultCollection.Add(datasetName, ionizationResult);
                        } 
                        else 
                        {
                            this.DatasetBasedResultCollection[datasetName].Add(result.Target.Adduct, result);
                        }

                        string chemName = result.Target.ChemicalIdentifier;
                        if (!this.ChemicalDatasetsMap.Keys.Contains(chemName))
                        {
                            this.ChemicalDatasetsMap.Add(chemName, new List<string>());
                            this.ChemicalDatasetsMap[chemName].Add(datasetName);
                        }
                        else
                        {
                            this.ChemicalDatasetsMap[chemName].Add(datasetName);
                        }
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

            foreach (string chemIdentifier in this.ChemicalDatasetsMap.Keys)
            {
                Dictionary<IonizationAdduct, ChemicalBasedAnalysisResult> ionizatonDictionary = new Dictionary<IonizationAdduct, ChemicalBasedAnalysisResult>();
                ICollection<string> datasets = this.ChemicalDatasetsMap[chemIdentifier];

                HashSet<IonizationAdduct> adductSet = new HashSet<IonizationAdduct>();

                foreach (string dataset in datasets)
                {
                    foreach (IonizationAdduct adduct in this.DatasetBasedResultCollection[dataset].Keys)
                    {
                        if (!adductSet.Contains(adduct))
                        {
                            ionizatonDictionary.Add(adduct, this.SummarizeResult(chemIdentifier, adduct));
                        }
                    }
                }

                this.ChemicalBasedResultCollection.Add(chemIdentifier, ionizatonDictionary);
            }

            this.empty = false;
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
                    foreach (var item in this.ChemicalBasedResultCollection)
                    {
                        IList<string> results = new List<string>();
                        
                        foreach (var ionization in ionizationsOfInterest)
                        {
                            if (item.Value.ContainsKey(ionization))
                            {
                                string result = summaryFunction(item.Value[ionization]);
                                
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
                            writer.WriteLine(item.Key + ":");
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
                                writer.Write(item.Key + "\t");
                                writer.WriteLine(result);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Summarize datasets with the same chemical identifier and adduct into chemical based result. Require the dataset-based 
        /// result to be already created.
        /// </summary>
        /// <param name="chemicalIdentifier">
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
        public ChemicalBasedAnalysisResult SummarizeResult(string chemicalIdentifier, IonizationAdduct ionization)
        {
            ChemicalBasedAnalysisResult result = new ChemicalBasedAnalysisResult(chemicalIdentifier, ionization);

            if (!this.ChemicalDatasetsMap.Keys.Contains(chemicalIdentifier))
            {
                throw new InstanceNotFoundException(chemicalIdentifier + " not found in ChemicalDatasetsMap");
            }

            IEnumerable<string> datasets = this.ChemicalDatasetsMap[chemicalIdentifier];
            foreach (string dataset in datasets)
            {
                if (this.DatasetBasedResultCollection[dataset].ContainsKey(ionization))
                {
                    CrossSectionWorkflowResult workflowResult = this.DatasetBasedResultCollection[dataset][ionization];
                    result = new ChemicalBasedAnalysisResult(result, workflowResult);
                }
            }

            return result;
        }
    }
}
