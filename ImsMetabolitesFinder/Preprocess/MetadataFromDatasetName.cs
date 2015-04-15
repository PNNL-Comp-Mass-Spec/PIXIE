// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetadataFromDatasetName.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   The dataset metadata.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinder.Preprocess
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The dataset metadata.
    /// </summary>
    public class MetadataFromDatasetName
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataFromDatasetName"/> class.
        /// </summary>
        /// <param name="datasetName">
        /// The dataset name.
        /// </param>
        public MetadataFromDatasetName(string datasetName)
        {
            List<string> components = datasetName.Split('_').ToList();
            this.SampleIdentifier = components[0];
            this.IonizationMode = components[1];
            this.DatePrepared = components[2];
        }

        /// <summary>
        /// Gets the sample identifier.
        /// </summary>
        public string SampleIdentifier { get; private set; }

        /// <summary>
        /// Gets the date prepared.
        /// </summary>
        public string DatePrepared { get; private set; }

        /// <summary>
        /// Gets the ionization mode.
        /// </summary>
        public string IonizationMode { get; private set; }
    }
}
