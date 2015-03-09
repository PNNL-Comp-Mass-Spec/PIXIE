namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
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
            string viperPosFilePath = Path.Combine(viperFileDir, "viper_pos_chemical_based.txt");
            string viperNegFilePath = Path.Combine(viperFileDir, "viper_neg_chemical_based.txt");

            // Export M+H and M+Na
            using (FileStream resultFile = new FileStream(viperPosFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(resultFile))
                {
                    writer.WriteLine("[Chemical Name], [Monoisotopic mass], [NET], [Normalized Drift Time], [Charge State]");
                    foreach (var item in resultAggregator.ChemicalBasedResultCollection)
                    {
                        string posResult = SummarizeResultViper(item.Value, IonizationMethod.ProtonPlus);
                        string sodiumResult = SummarizeResultViper(item.Value, IonizationMethod.SodiumPlus);
                        if (!String.IsNullOrEmpty(posResult))
                        {
                            writer.WriteLine(posResult);
                        }

                        if (!String.IsNullOrEmpty(sodiumResult))
                        {
                            writer.WriteLine(sodiumResult);
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
                    foreach (var item in resultAggregator.ChemicalBasedResultCollection)
                    {
                        string negResult = SummarizeResultViper(item.Value, IonizationMethod.ProtonMinus);
                        if (!String.IsNullOrEmpty(negResult))
                        {
                            writer.WriteLine(negResult);
                        }
                    }
                }
            }
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
                            writer.WriteLine(posResult);
                        }

                        if (!String.IsNullOrEmpty(sodiumResult))
                        {
                            writer.WriteLine(sodiumResult);
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
                            writer.WriteLine(negResult);
                        }

                        if (!String.IsNullOrEmpty(negResult))
                        {
                            writer.WriteLine(hcooResult);
                        }

                        if (!String.IsNullOrEmpty(negResult))
                        {
                            writer.WriteLine(proton2MinusSodiumPlusResult);
                        }

                        if (!String.IsNullOrEmpty(negResult))
                        {
                            writer.WriteLine(apci);
                        }
                    }
                }
            }
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
        private static string SummarizeResultViper(IDictionary<IonizationMethod, MoleculeInformedWorkflowResult> chemicalResult, IonizationMethod ionization)
        {
            string result = String.Empty;
            if (chemicalResult.ContainsKey((ionization)))
            {
                MoleculeInformedWorkflowResult workflowResult = chemicalResult[ionization];
                if (workflowResult.AnalysisStatus == AnalysisStatus.POS && workflowResult.LastVoltageGroupDriftTimeInMs > 0)
                {
                    string name = workflowResult.DatasetName + "_" + workflowResult.IonizationMethod;
                    double mass = workflowResult.MonoisotopicMass;
                    const double Net = 0.5;
                    double driftTime = workflowResult.LastVoltageGroupDriftTimeInMs;
                    const int ChargeState = 1;
                    result = String.Format("{0}, {1:F4}, {2:F4}, {3:F2}, {4}", name, mass, Net, driftTime, ChargeState);
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
        private static string SummarizeResultViper(IDictionary<IonizationMethod, ChemicalBasedAnalysisResult> chemicalResult, IonizationMethod ionization)
        {
            string result = String.Empty;
            if (chemicalResult.ContainsKey((ionization)))
            {
                ChemicalBasedAnalysisResult workflowResult = chemicalResult[ionization];
                if (workflowResult.AnalysisStatus == AnalysisStatus.POS && workflowResult.LastVoltageGroupDriftTimeInMs > 0)
                {
                    string name = workflowResult.ChemicalName + "_" + workflowResult.IonizationMethod;
                    double mass = workflowResult.MonoisotopicMass;
                    const double Net = 0.5;
                    double driftTime = workflowResult.LastVoltageGroupDriftTimeInMs;
                    const int ChargeState = 1;
                    result = String.Format("{0}, {1:F4}, {2:F4}, {3:F2}, {4}", name, mass, Net, driftTime, ChargeState);
                }
            }
            return result;
        }
    }
}
