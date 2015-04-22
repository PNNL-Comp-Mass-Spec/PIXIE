// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViperExporter.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   The viper exporter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinderBatchProcessor.Export
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;

    using ImsInformed.Domain;
    using ImsInformed.Interfaces;
    using ImsInformed.Workflows.CrossSectionExtraction;

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
            IList<IonizationAdduct> posMode = new List<IonizationAdduct>();
            posMode.Add(IonizationMethod.ProtonPlus.ToAdduct());
            posMode.Add(IonizationMethod.SodiumPlus.ToAdduct());

            IList<IonizationAdduct> negMode = new List<IonizationAdduct>();
            negMode.Add(IonizationMethod.ProtonMinus.ToAdduct());
            negMode.Add(IonizationMethod.APCI.ToAdduct());
            negMode.Add(IonizationMethod.HCOOMinus.ToAdduct());
            negMode.Add(IonizationMethod.Proton2MinusSodiumPlus.ToAdduct());

            string viperPosFilePath = Path.Combine(viperFileDir, "viper_pos_chemical_based.txt");
            resultAggregator.SummarizeResultChemicalBased(viperPosFilePath, SummarizeResultViper, "#[Chemical Name]\t[target]\t[Monoisotopic mass]\t[NET]\t[Normalized Drift Time]\t[Charge State]", posMode, false);        
    
            string viperNegFilePath = Path.Combine(viperFileDir, "viper_neg_chemical_based.txt");
            resultAggregator.SummarizeResultChemicalBased(viperNegFilePath, SummarizeResultViper, "#[Chemical Name]\t[target]\t[Monoisotopic mass]\t[NET]\t[Normalized Drift Time]\t[Charge State]", negMode, false);      

            string viperAllFilePath = Path.Combine(viperFileDir, "viper_chemical_based.txt");
            resultAggregator.SummarizeResultChemicalBased(viperAllFilePath, SummarizeResultViper, "#[Chemical Name]\t[target]\t[Monoisotopic mass]\t[NET]\t[Normalized Drift Time]\t[Charge State]", IonizationMethodUtilities.GetAll().Select(ionization => ionization.ToAdduct()));      
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
            Predicate<CrossSectionWorkflowResult> posChargeState = (result) => { return result.Target.ChargeState > 0; };
            Predicate<CrossSectionWorkflowResult> negChargeState = (result) => { return result.Target.ChargeState < 0; };

            string viperPosFilePath = Path.Combine(viperFileDir, "viper_pos_dataset_based.txt");
            string viperNegFilePath = Path.Combine(viperFileDir, "viper_neg_dataset_based.txt");
            string description = "#[Dataset Name / Target], [Monoisotopic mass], [NET], [Normalized Drift Time], [Charge State]";

            resultAggregator.SummarizeResultDatasetBased(viperPosFilePath, SummarizeResultViper, description, posChargeState);
            resultAggregator.SummarizeResultDatasetBased(viperNegFilePath, SummarizeResultViper, description, negChargeState);
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
        /// <param name="workflowResult"></param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string SummarizeResultViper(CrossSectionWorkflowResult workflowResult)
        {
            string result = string.Empty;
            if (workflowResult.AnalysisStatus == AnalysisStatus.Positive)
            {
                foreach (var isomer in workflowResult.MatchingIsomers)
                {
                    if (isomer.LastVoltageGroupDriftTimeInMs > 0)
                    {
                        string name = workflowResult.DatasetName + "_" + workflowResult.Target.TargetDescriptor;
                        double mass = workflowResult.Target.MonoisotopicMass;
                        const double Net = 0.5;
                        double driftTime = isomer.LastVoltageGroupDriftTimeInMs;
                        const int ChargeState = 1;
                        result += String.Format("{0}, {1:F4}, {2:F4}, {3:F2}, {4}", name, mass, Net, driftTime, ChargeState);
                    }
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
            if (workflowResult.AnalysisStatus == AnalysisStatus.Positive)
            {
                IImsTarget target = workflowResult.Target;
                double mass = workflowResult.Target.MonoisotopicMass;

                foreach (var isomer in workflowResult.DetectedIsomers)
                {
                    const double Net = 0.5;
                    double driftTime = isomer.LastVoltageGroupDriftTimeInMs;
                    const int ChargeState = 1;
                    result += String.Format("{0}\t{1:F4}\t{2:F4}\t{3:F2}\t{4}", target.TargetDescriptor, mass, Net, driftTime, ChargeState);
                }
            }

            return result;
        }
    }
}
