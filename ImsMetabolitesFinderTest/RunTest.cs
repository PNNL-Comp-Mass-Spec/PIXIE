
namespace ImsMetabolitesFinderTest
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using ImsInformed.Domain;
    using ImsInformed.Interfaces;
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
        public static void SerializerDeserializerTest()
        {
            IList<CrossSectionWorkflowResult> results = new List<CrossSectionWorkflowResult>();

            // create fake result struct
            TargetIsomerReport holyGrail1;
            holyGrail1.CrossSectionalArea = 22;
            holyGrail1.LastVoltageGroupDriftTimeInMs = 20;
            holyGrail1.Mobility = 10;
            holyGrail1.MonoisotopicMass = 5;

            FeatureScoreHolder averageFeatureScores1;
            averageFeatureScores1.IntensityScore = 3;
            averageFeatureScores1.IsotopicScore = 4;
            averageFeatureScores1.PeakShapeScore = 5;

            AnalysisScoresHolder analysisScoresHolder1;
            analysisScoresHolder1.AverageCandidateTargetScores = averageFeatureScores1;
            analysisScoresHolder1.AverageVoltageGroupStabilityScore = 4;
            analysisScoresHolder1.RSquared = 6;

            IImsTarget target1 = new MolecularTarget("C2H5OH", IonizationMethod.ProtonMinus, "Ginger ale");
            IImsTarget target2 = new MolecularTarget("C2H5OH", IonizationMethod.ProtonMinus, "Volka");

            CrossSectionWorkflowResult result1 = new CrossSectionWorkflowResult(
                "France", 
                target1,
                AnalysisStatus.Positive,
                analysisScoresHolder1,
                holyGrail1);

            // create fake result struct
            TargetIsomerReport holyGrail2;
            holyGrail2.CrossSectionalArea = 22;
            holyGrail2.LastVoltageGroupDriftTimeInMs = 20;
            holyGrail2.Mobility = 10;
            holyGrail2.MonoisotopicMass = 5;

            FeatureScoreHolder averageFeatureScores;
            averageFeatureScores.IntensityScore = 3;
            averageFeatureScores.IsotopicScore = 4;
            averageFeatureScores.PeakShapeScore = 5;

            AnalysisScoresHolder analysisScoresHolder;
            analysisScoresHolder.AverageCandidateTargetScores = averageFeatureScores;
            analysisScoresHolder.AverageVoltageGroupStabilityScore = 4;
            analysisScoresHolder.RSquared = 6;

            CrossSectionWorkflowResult result2 = new CrossSectionWorkflowResult(
                "France", 
                target2,
                AnalysisStatus.Positive,
                analysisScoresHolder,
                holyGrail2);

            results.Add(result1);
            results.Add(result2);

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

            // Compare it
            var result = newResults.First();
            Assert.AreEqual(result.AnalysisStatus, result1.AnalysisStatus);
            Assert.AreEqual(result.DatasetName, result2.DatasetName);
            Assert.AreEqual(result.AnalysisScoresHolder.AverageCandidateTargetScores.IntensityScore, result2.AnalysisScoresHolder.AverageCandidateTargetScores.IntensityScore);
            Assert.AreEqual(result.AnalysisScoresHolder.AverageCandidateTargetScores.IsotopicScore, result2.AnalysisScoresHolder.AverageCandidateTargetScores.IsotopicScore);
            Assert.AreEqual(result.AnalysisScoresHolder.AverageCandidateTargetScores.PeakShapeScore, result2.AnalysisScoresHolder.AverageCandidateTargetScores.PeakShapeScore);
        }
    }
}
