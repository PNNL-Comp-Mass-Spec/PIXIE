// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChemicalBasedAnalysisResult.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the ChemicalBasedAnalysisResult type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;

    using ImsInformed.Domain;
    using ImsInformed.Scoring;

    /// <summary>
    /// The chemical based analysis result.
    /// </summary>
    [Serializable]
    public class ChemicalBasedAnalysisResult 
    {
        /// <summary>
        /// The fusion number.
        /// </summary>
        public readonly int FusionNumber;

        /// <summary>
        /// The dataset name.
        /// </summary>
        public readonly string ChemicalName;

        /// <summary>
        /// The dataset name.
        /// </summary>
        public readonly string TargetDescriptor;

        /// <summary>
        /// The ionization method.
        /// </summary>
        public readonly IonizationMethod IonizationMethod;

        private readonly string targetDescriptor;

        /// <summary>
        /// The analysis status.
        /// </summary>
        public readonly AnalysisStatus AnalysisStatus;

        /// <summary>
        /// The isomer results.
        /// </summary>
        private readonly IEnumerable<TargetIsomerReport> isomerResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChemicalBasedAnalysisResult"/> class. 
        /// The initiate chemical based analysis result.
        /// </summary>
        /// <param name="result">
        /// The result ionization result to initiate from.
        /// </param>
        /// <param name="chemName">
        /// The chemical name.
        /// </param>
        /// <returns>
        /// The <see cref="ChemicalBasedAnalysisResult"/>.
        /// </returns>
        public ChemicalBasedAnalysisResult(MoleculeInformedWorkflowResult result, string chemName)
        {
            this.AnalysisStatus = result.AnalysisStatus;
            this.ChemicalName = chemName;
            this.FusionNumber = 1;
            this.IonizationMethod = result.IonizationMethod;
            this.TargetDescriptor = result.TargetDescriptor;
            this.isomerResults = result.MatchingIsomers;
            this.TargetDescriptor = result.TargetDescriptor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChemicalBasedAnalysisResult"/> class.
        /// </summary>
        /// <param name="chemName">
        /// The chem name.
        /// </param>
        /// <param name="ionization">
        /// The ionization.
        /// </param>
        /// <param name="targetDescriptor">
        /// The target descriptor.
        /// </param>
        public ChemicalBasedAnalysisResult(string chemName, IonizationMethod ionization, string targetDescriptor)
        {
            this.AnalysisStatus = AnalysisStatus.NAH;
            this.ChemicalName = chemName;
            this.FusionNumber = 0;
            this.isomerResults = new List<TargetIsomerReport>();
            this.IonizationMethod = ionization;
            this.targetDescriptor = targetDescriptor;
        }

        /// <summary>
        /// The isomers observed for that given analyses
        /// </summary>
        public IEnumerable<TargetIsomerReport> IsomerResults
        {
            get
            {
                return this.isomerResults;
            }
        }
    }
}
