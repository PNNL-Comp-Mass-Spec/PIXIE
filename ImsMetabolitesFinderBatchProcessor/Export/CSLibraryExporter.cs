// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CSLibraryExporter.cs" company="PNNL">
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
    using System.IO;

    using ImsInformed.Domain;

    /// <summary>
    /// The viper exporter.
    /// </summary>
    public class CsLibraryExporter
    {
        /// <summary>
        /// The export cross section chemical based.
        /// </summary>
        /// <param name="resultAggregator">
        /// The result aggregator.
        /// </param>
        /// <param name="OutputDir">
        /// The viper file dir.
        /// </param>
        public static void ExportCrossSectionChemicalBased(ResultAggregator resultAggregator, string OutputDir)
        {
            string outputPath = Path.Combine(OutputDir, "cross_section_chemical_based.txt");
            resultAggregator.SummarizeResultChemicalBased(outputPath, SummarizeResultCrossSection, "[Chemical Name] [Ionization Mode] [CrossSection(Å^2)] ");            
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
        private static string SummarizeResultCrossSection(ChemicalBasedAnalysisResult workflowResult)
        {
            string result = String.Empty;
            if (workflowResult.AnalysisStatus == AnalysisStatus.POS && workflowResult.LastVoltageGroupDriftTimeInMs > 0)
            {
                string name = workflowResult.IonizationMethod.ToFriendlyString();
                double cs = workflowResult.CrossSectionalArea;
                result = String.Format("{0}: {1:F4}, ", name, cs);
            }

            return result;
        }
    }
}
