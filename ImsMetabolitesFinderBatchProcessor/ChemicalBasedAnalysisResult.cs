namespace ImsMetabolitesFinderBatchProcessor
{
    using System;

    using ImsInformed.Domain;
    using ImsInformed.Util;

    [Serializable]
    public struct ChemicalBasedAnalysisResult 
    {
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
