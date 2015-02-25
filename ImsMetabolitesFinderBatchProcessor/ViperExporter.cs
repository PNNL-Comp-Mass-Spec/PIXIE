namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using ImsInformed.Domain;

    public class ViperExporter
    {
        public static void ExportViperInputFile(ResultAggregator resultAggregator, string viperFileDir)
        {
            string viperPosFilePath = Path.Combine(viperFileDir, "viperInputPosIon.txt");
            string viperNegFilePath = Path.Combine(viperFileDir, "viperInputNegIon.txt");

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
                        if (!String.IsNullOrEmpty(negResult))
                        {
                            writer.WriteLine(negResult);
                        }
                    }
                }
            }
        }

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
    }
}
