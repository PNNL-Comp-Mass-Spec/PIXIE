// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViperExporter.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   The viper exporter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    using ImsInformed.Domain;

    /// <summary>
    /// The viper exporter.
    /// </summary>
    public class ViperExporter
    {
        /// <summary>
        /// The export viper chemical based.
        /// </summary>
        /// <param name="resultAggregator">
        /// The result aggregator.
        /// </param>
        /// <param name="viperFileDir">
        /// The viper file dir.
        /// </param>
        public static void ExportViperChemicalBased(ResultAggregator resultAggregator, string viperFileDir)
        {
            IList<IonizationMethod> posMode = new List<IonizationMethod>();
            posMode.Add(IonizationMethod.ProtonPlus);
            posMode.Add(IonizationMethod.SodiumPlus);

            IList<IonizationMethod> negMode = new List<IonizationMethod>();
            negMode.Add(IonizationMethod.ProtonMinus);
            negMode.Add(IonizationMethod.APCI);
            negMode.Add(IonizationMethod.HCOOMinus);
            negMode.Add(IonizationMethod.Proton2MinusSodiumPlus);

            string viperPosFilePath = Path.Combine(viperFileDir, "viper_pos_chemical_based.txt");
            resultAggregator.SummarizeResultChemicalBased(viperPosFilePath, SummarizeResultViper, "[Chemical Name]\t[target]\t[Monoisotopic mass]\t[NET]\t[Normalized Drift Time]\t[Charge State]", posMode, false);        
    
            string viperNegFilePath = Path.Combine(viperFileDir, "viper_neg_chemical_based.txt");
            resultAggregator.SummarizeResultChemicalBased(viperNegFilePath, SummarizeResultViper, "[Chemical Name]\t[target]\t[Monoisotopic mass]\t[NET]\t[Normalized Drift Time]\t[Charge State]", negMode, false);      

            string viperAllFilePath = Path.Combine(viperFileDir, "viper_chemical_based.txt");
            resultAggregator.SummarizeResultChemicalBased(viperAllFilePath, SummarizeResultViper, "[Chemical Name]\t[target]\t[Monoisotopic mass]\t[NET]\t[Normalized Drift Time]\t[Charge State]");      
        }

        /// <summary>
        /// The export viper dataset based.
        /// </summary>
        /// <param name="resultAggregator">
        /// The result aggregator.
        /// </param>
        /// <param name="viperFileDir">
        /// The viper file dir.
        /// </param>
        public static void ExportViperDatasetBased(ResultAggregator resultAggregator, string viperFileDir)
        {
            string viperPosFilePath = Path.Combine(viperFileDir, "viper_pos_dataset_based.txt");
            string viperNegFilePath = Path.Combine(viperFileDir, "viper_neg_dataset_based.txt");

            // Export M+H and M+Na
            using (FileStream resultFile = new FileStream(viperPosFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(resultFile))
                {
                    writer.WriteLine("[Chemical Name], [Monoisotopic mass], [NET], [Normalized Drift Time], [Charge State]");
                    foreach (var item in resultAggregator.DatasetBasedResultCollection)
                    {
                        string posResult = SummarizeResultViper(item.Value, IonizationMethod.ProtonPlus);
                        string sodiumResult = SummarizeResultViper(item.Value, IonizationMethod.SodiumPlus);
                        if (!String.IsNullOrEmpty(posResult))
                        {
                            writer.Write(posResult);
                        }

                        if (!String.IsNullOrEmpty(sodiumResult))
                        {
                            writer.Write(sodiumResult);
                        }
                    }
                }
            }

            // Export M-H
            using (FileStream resultFile = new FileStream(viperNegFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(resultFile))
                {
                    writer.WriteLine("[Chemical Name], [Monoisotopic mass], [NET], [Normalized Drift Time], [Charge State]");
                    foreach (var item in resultAggregator.DatasetBasedResultCollection)
                    {
                        string negResult = SummarizeResultViper(item.Value, IonizationMethod.ProtonMinus);
                        string hcooResult = SummarizeResultViper(item.Value, IonizationMethod.HCOOMinus);
                        string proton2MinusSodiumPlusResult = SummarizeResultViper(item.Value, IonizationMethod.Proton2MinusSodiumPlus);
                        string apci = SummarizeResultViper(item.Value, IonizationMethod.APCI);
                        
                        if (!String.IsNullOrEmpty(negResult))
                        {
                            writer.Write(negResult);
                        }

                        if (!String.IsNullOrEmpty(negResult))
                        {
                            writer.Write(hcooResult);
                        }

                        if (!String.IsNullOrEmpty(negResult))
                        {
                            writer.Write(proton2MinusSodiumPlusResult);
                        }

                        if (!String.IsNullOrEmpty(negResult))
                        {
                            writer.Write(apci);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The summarize result viper.
        /// </summary>
        /// <param name="analysesResults">
        /// The chemical result.
        /// </param>
        /// <param name="ionization">
        /// The ionization.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string SummarizeResultViper(IDictionary<IonizationMethod, CrossSectionWorkflowResult> analysesResults, IonizationMethod ionization)
        {
            string result = String.Empty;
            if (analysesResults.ContainsKey((ionization)))
            {
                CrossSectionWorkflowResult workflowResult = analysesResults[ionization];
                if (workflowResult.AnalysisStatus == AnalysisStatus.Positive && workflowResult.LastVoltageGroupDriftTimeInMs > 0)
                {
                    string name = workflowResult.DatasetName + "_" + workflowResult.TargetDescriptor + "_" + workflowResult.IonizationMethod.ToFriendlyString();
                    double mass = workflowResult.MonoisotopicMass;
                    const double Net = 0.5;
                    double driftTime = workflowResult.LastVoltageGroupDriftTimeInMs;
                    const int ChargeState = 1;
                    result = String.Format("{0}, {1:F4}, {2:F4}, {3:F2}, {4}\r\n", name, mass, Net, driftTime, ChargeState);
                }
            }

            return result;
        }

        /// <summary>
        /// The summarize result viper.
        /// </summary>
        /// <param name="chemicalResult">
        /// The chemical result.
        /// </param>
        /// <param name="ionization">
        /// The ionization.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string SummarizeResultViper(ChemicalBasedAnalysisResult workflowResult)
        {
            string result = String.Empty;
            if (workflowResult.AnalysisStatus == AnalysisStatus.Positive && workflowResult.LastVoltageGroupDriftTimeInMs > 0)
            {
                string target = workflowResult.TargetDescriptor + workflowResult.IonizationMethod.ToFriendlyString();
                double mass = workflowResult.MonoisotopicMass;
                const double Net = 0.5;
                double driftTime = workflowResult.LastVoltageGroupDriftTimeInMs;
                const int ChargeState = 1;
                result = String.Format("{0}\t{1:F4}\t{2:F4}\t{3:F2}\t{4}", target, mass, Net, driftTime, ChargeState);
            }

            return result;
        }
    }
}
