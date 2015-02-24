using System.Collections.Generic;
using System.Linq;

namespace ImsMetabolitesFinderBatchProcessor
{
    public class DatasetMetadata
    {
        public string SampleIdentifier { get; private set; }
        public string DatePrepared { get; private set; }
        public string IonizationMode { get; private set; }

        public DatasetMetadata(string datasetName)
        {
            List<string> components = datasetName.Split('_').ToList();
            this.SampleIdentifier = components[0];
            this.IonizationMode = components[1];
            this.DatePrepared = components[2];
        }
    }
}
