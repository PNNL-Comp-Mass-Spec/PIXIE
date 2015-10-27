namespace PIXIETest
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using ImsInformed;
    using ImsInformed.Domain.DataAssociation;
    using ImsInformed.Scoring;
    using ImsInformed.Targets;
    using ImsInformed.Workflows.CrossSectionExtraction;

    using NUnit.Framework;

    using PIXIEBatchProcessor.Export;

    /// <summary>
    /// The run test.
    /// </summary>
    public class RunTest
    {
        //default values
        private const string OutputFileDir = @"output\";
        #if DEBUG
            private const string Finder = @"..\..\..\..\PIXIE\bin\x64\Debug\PIXIE.exe";
            private const string BatchProcessor = @"..\..\..\..\PIXIE\bin\x64\Debug\PIXIE.exe";
        #else
            private const string Finder = @"..\..\..\..\PIXIE\bin\x64\Release\PIXIE.exe";
            private const string BatchProcessor = @"..\..\..\..\PIXIE\bin\x64\Release\PIXIEBatchProcessor.exe";
        #endif
        
        private const string UimfFilePath = @"\\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule\EXP-BPS_pos2_13Sep14_Columbia_DI.uimf";

        [Test]
        public async static void AnalysisDBTest()
        {
            var snapshot1 = new ArrivalTimeSnapShot();
            snapshot1.DriftTubeVoltageInVolt = 1000;
            snapshot1.MeasuredArrivalTimeInMs = 22;
            snapshot1.PressureInTorr = 4;
            snapshot1.TemperatureInKelvin = 10;
            var iso1 = new IdentifiedIsomerInfo(2 ,222, 0.9, 10.5, 115, 123, new List<ArrivalTimeSnapShot>(){snapshot1}, 100, AnalysisStatus.Positive, new PeakScores(1.0, 0.8, 0.7), new MolecularTarget("C2H4O16P2", IonizationMethod.Protonated, "testamin"), 0.1, 0);
            var idens = new List<IdentifiedIsomerInfo>() {iso1};
            
            PeakScores scores = new PeakScores(0, 0.5, 1);
            CrossSectionWorkflowResult result1 = new CrossSectionWorkflowResult("ABC_Dataset_01", new MolecularTarget("C2H4O16P2", IonizationMethod.Protonated, "testamin"), AnalysisStatus.Positive, new AssociationHypothesisInfo(0.1, 0.2), scores, 0.5, OutputFileDir, "123");
            CrossSectionWorkflowResult result2 = new CrossSectionWorkflowResult("ABC_Dataset_02", new MolecularTarget("C2H4O16P2", IonizationMethod.Protonated, "testamin"), AnalysisStatus.Positive, new AssociationHypothesisInfo(0.1, 0.4), scores, 0.8, OutputFileDir, "123");
            CrossSectionWorkflowResult result3 = new CrossSectionWorkflowResult("ABC_Dataset_03", new MolecularTarget("C2H4O16P2", IonizationMethod.Deprotonated, "testamin"), AnalysisStatus.Positive, new AssociationHypothesisInfo(0.1, 0.4), idens, scores, 0.8, OutputFileDir, "123");
            CrossSectionWorkflowResult result4 = new CrossSectionWorkflowResult("ABC_Dataset_03", new MolecularTarget("C2H4O16P2", IonizationMethod.Sodiumated, "testamin"), AnalysisStatus.Positive, new AssociationHypothesisInfo(0.1, 0.4), idens, scores, 0.8, OutputFileDir, "123");
            CrossSectionWorkflowResult result5 = new CrossSectionWorkflowResult("ABC_Dataset_03", new MolecularTarget("C10H40", IonizationMethod.Protonated, "googlin"), AnalysisStatus.Positive, new AssociationHypothesisInfo(0.1, 0.4), idens, scores, 0.8, OutputFileDir, "123");
            CrossSectionWorkflowResult result6 = new CrossSectionWorkflowResult("ABC_Dataset_03", new MolecularTarget("C10H40", IonizationMethod.Protonated, "googlin"), AnalysisStatus.Negative, null, idens, scores, 0.8, OutputFileDir, "123");

            string fileName = @"output\test.sqlite";
            AnalysisLibrary lib = new AnalysisLibrary(fileName);
            await lib.CreateTables();
            await lib.InsertResult(result1);
            await lib.InsertResult(result2);
            await lib.InsertResult(result3);
            await lib.InsertResult(result4);
            await lib.InsertResult(result5);
            await lib.InsertResult(result6);
        }

        [Test]  
        public static void SerializerDeserializerTest()
        {
            IList<CrossSectionWorkflowResult> results = new List<CrossSectionWorkflowResult>();

            // create fake result 1
            IdentifiedIsomerInfo holyGrail1 = new IdentifiedIsomerInfo(10, 250, 6, 10, 22, 4, null, 5, AnalysisStatus.Positive, new PeakScores(1,2,3), new MolecularTarget("C2H34", IonizationMethod.APCI, "Carbon"), 1, 2);
            PeakScores averageFeatureScores1 = new PeakScores(3, 4, 5);

            IImsTarget target1 = new MolecularTarget("C2H5OH", IonizationMethod.Deprotonated, "Ginger ale");
            IImsTarget target2 = new MolecularTarget("C2H5OH", IonizationMethod.Sodiumated, "Volka");

            CrossSectionWorkflowResult result1 = new CrossSectionWorkflowResult(
                "France", 
                target1,
                AnalysisStatus.Positive,
                null,averageFeatureScores1,
                4,
                "",
                "");

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
