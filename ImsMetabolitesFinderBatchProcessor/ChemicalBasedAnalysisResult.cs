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

    using ImsInformed.Domain;

    /// <summary>
    /// The chemical based analysis result.
    /// </summary>
    [Serializable]
    public struct ChemicalBasedAnalysisResult 
    {
        /// <summary>
        /// The fusion number.
        /// </summary>
        public int FusionNumber;

        /// <summary>
        /// The dataset name.
        /// </summary>
        public string ChemicalName;

        /// <summary>
        /// The ionization method.
        /// </summary>
        public IonizationMethod IonizationMethod;

        /// <summary>
        /// The analysis status.
        /// </summary>
        public AnalysisStatus AnalysisStatus;

        /// <summary>
        /// The cross sectional area.
        /// </summary>
        public double CrossSectionalArea;

        #region data needed by viper
        /// <summary>
        /// The cross sectional area.
        /// </summary>
        public double LastVoltageGroupDriftTimeInMs;

        /// <summary>
        /// The monoisotopic mass.
        /// </summary>
        public double MonoisotopicMass;
        #endregion
    }
}
