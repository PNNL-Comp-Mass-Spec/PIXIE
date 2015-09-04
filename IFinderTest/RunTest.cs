
namespace IFinderTest
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using IFinderBatchProcessor.Export;

    using ImsInformed;
    using ImsInformed.Scoring;
    using ImsInformed.Targets;
    using ImsInformed.Workflows.CrossSectionExtraction;

    using NUnit.Framework;

    /// <summary>
    /// The run test.
    /// </summary>
    public class RunTest
    {
        //default values
        private const string OutputFileDir = @"output\";
        #if DEBUG
            private const string Finder = @"..\..\..\..\ImsMetabolitesFinder\bin\x64\Debug\ImsMetabolitesFinder.exe";
            private const string BatchProcessor = @"..\..\..\..\ImsMetabolitesFinderBatchProcessor\bin\x64\Debug\ImsMetabolitesFinderBatchProcessor.exe";
        #else
            private const string Finder = @"..\..\..\..\ImsMetabolitesFinder\bin\x64\Release\ImsMetabolitesFinder.exe";
            private const string BatchProcessor = @"..\..\..\..\ImsMetabolitesFinderBatchProcessor\bin\x64\Release\ImsMetabolitesFinderBatchProcessor.exe";
        #endif
        
        private const string UimfFilePath = @"\\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule\EXP-BPS_pos2_13Sep14_Columbia_DI.uimf";

        [Test]  
        public static void AnalysisDBTest()
        {
            string fileName = @"output\test.sqlite";
            AnalysisLibrary lib = new AnalysisLibrary(fileName);
        }

        [Test]  
        public static void SerializerDeserializerTest()
        {
            IList<CrossSectionWorkflowResult> results = new List<CrossSectionWorkflowResult>();

            // create fake result 1
            IdentifiedIsomerInfo holyGrail1 = new IdentifiedIsomerInfo(10, 250, 6, 10, 22, 4, null, 5, AnalysisStatus.Positive, null, null);
            PeakScores averageFeatureScores1 = new PeakScores(3, 4, 5);

            IImsTarget target1 = new MolecularTarget("C2H5OH", IonizationMethod.Deprotonated, "Ginger ale");
            IImsTarget target2 = new MolecularTarget("C2H5OH", IonizationMethod.Sodiumated, "Volka");

            CrossSectionWorkflowResult result1 = new CrossSectionWorkflowResult(
                "France", 
                target1,
                AnalysisStatus.Positive,
                null,averageFeatureScores1,
                4);

            results.Add(result1);

            // Serialize fake result struct
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream("serialized_result.bin", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(stream, results);
            }

            IList<CrossSectionWorkflowResult> newResults;

            // deserialize fake result struct
            using (Stream stream = new FileStream("serialized_result.bin", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                newResults = (IList<CrossSectionWorkflowResult>)formatter.Deserialize(stream);
            }

            CrossSectionWorkflowResult result2 = newResults.First();

            // Compare it
            var result = newResults.First();
            Assert.AreEqual(result.AnalysisStatus, result1.AnalysisStatus);
            Assert.AreEqual(result.DatasetName, result2.DatasetName);
            Assert.AreEqual(result.AverageObservedPeakStatistics.IntensityScore, result2.AverageObservedPeakStatistics.IntensityScore);
        }
    }
}
