
namespace ImsMetabolitesFinderTest
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using ImsInformed.Domain;

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
        public static void SerializerDeserializerTest()
        {
            // create fake result struct
            MoleculeInformedWorkflowResult result;
            result.AnalysisStatus = AnalysisStatus.ChargeStateCorrelation;
            result.CrossSectionalArea = 22;
            result.DatasetName = "Nothing";
            result.IonizationMethod = IonizationMethod.ProtonPlus;
            result.Mobility = 1;
            result.AnalysisScoresHolder.RSquared = 2;
            result.AnalysisScoresHolder.AverageCandidateTargetScores.IntensityScore = 3;
            result.AnalysisScoresHolder.AverageCandidateTargetScores.IsotopicScore = 4;
            result.AnalysisScoresHolder.AverageCandidateTargetScores.PeakShapeScore = 5;
            result.AnalysisScoresHolder.AverageVoltageGroupStabilityScore = 6;
            result.TargetDescriptor = "H2O";
            result.LastVoltageGroupDriftTimeInMs = -1;
            result.MonoisotopicMass = 1.2;

            // Serialize fake result struct
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream("serialized_result.bin", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(stream, result);
            }

            // deserialize fake result struct
            MoleculeInformedWorkflowResult result2;
            using (Stream stream = new FileStream("serialized_result.bin", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result2 = (MoleculeInformedWorkflowResult)formatter.Deserialize(stream);
            }

            // Compare it
            Assert.AreEqual(result.AnalysisStatus, result2.AnalysisStatus);
            Assert.AreEqual(result.CrossSectionalArea, result2.CrossSectionalArea);
            Assert.AreEqual(result.DatasetName, result2.DatasetName);
            Assert.AreEqual(result.IonizationMethod, result2.IonizationMethod);
            Assert.AreEqual(result.Mobility, result2.Mobility);
            Assert.AreEqual(result.AnalysisScoresHolder.AverageCandidateTargetScores.IntensityScore, result2.AnalysisScoresHolder.AverageCandidateTargetScores.IntensityScore);
            Assert.AreEqual(result.AnalysisScoresHolder.AverageCandidateTargetScores.IsotopicScore, result2.AnalysisScoresHolder.AverageCandidateTargetScores.IsotopicScore);
            Assert.AreEqual(result.AnalysisScoresHolder.AverageCandidateTargetScores.PeakShapeScore, result2.AnalysisScoresHolder.AverageCandidateTargetScores.PeakShapeScore);
            Assert.AreEqual(result.TargetDescriptor, result2.TargetDescriptor);
        }
    }
}
