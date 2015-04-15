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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;

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
            resultAggregator.SummarizeResultChemicalBased(outputPath, SummarizeResultCrossSection, "#[Chemical Name] [Ionization Mode] [CrossSection(Å^2)] ", IonizationMethodUtilities.GetAll().Select(ionizationAdduct => ionizationAdduct.ToAdduct()));            
        }    

        /// <summary>
        /// The summarize result viper.
        /// </summary>
        /// <param name="workflowResult">
        /// The workflow Result.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string SummarizeResultCrossSection(ChemicalBasedAnalysisResult workflowResult)
        {
            string result = string.Empty;
            if (workflowResult.AnalysisStatus == AnalysisStatus.Positive)
            {
                string name = workflowResult.Target.ChemicalIdentifier;
                string[] crossSections = workflowResult.DetectedIsomers.Select((x) => string.Format("{0:F4}", x.CrossSectionalArea)).ToArray();
                

                result = string.Format("{0}: {1:F4} ", name, string.Join(", ", crossSections));
            }

            return result;
        }
    }
}
