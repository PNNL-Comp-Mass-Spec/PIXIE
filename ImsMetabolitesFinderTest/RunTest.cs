
namespace ImsMetabolitesFinderTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using ImsInformed.Domain;

    using ImsMetabolitesFinder.Preprocess;

    using NUnit.Framework;

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
        public static void TestImsMetabolitesFinder_Help() 
        {
            string sample_name = "EXP-BPS_pos2_13Sep14_Columbia_DI";
            string cmd_line_opts = "-h"; 
            Console.WriteLine("Test case: Help page " + sample_name + "\r\n");
            Console.WriteLine("invoking ImsMetabolitesFinder . . .\r\n");
            System.Diagnostics.Process.Start(Finder, cmd_line_opts);
        }

        [Test]  
        public static void TestImsMetabolitesFinder_MZ() 
        {
            string sample_name = "EXP-BPS_pos2_13Sep14_Columbia_DI";
            string cmd_line_opts = "-i " + UimfFilePath + " -t 273.0192006876" + " -m M+Na" + " -o output/"; 
            Console.WriteLine("Test case: detecting target particle list using its Mz value, for " + sample_name + "\r\n");
            Console.WriteLine("invoking ImsMetabolitesFinder . . .\r\n");
            System.Diagnostics.Process.Start(Finder, cmd_line_opts);
        }

        [Test]  
        public static void TestImsMetabolitesFinder_Formula() 
        {

            string sample_name = "EXP-BPS_pos2_13Sep14_Columbia_DI";
            string cmd_line_opts = "-i " + UimfFilePath + " -t C12H10NNaO3S" + " -m M+Na" + " -o output/ --pause"; 
            Console.WriteLine("Test case: detecting target particle using its empirical formula, for " + sample_name + "\r\n");
            Console.WriteLine("invoking ImsMetabolitesFinder . . .\r\n");
            System.Diagnostics.Process.Start(Finder, cmd_line_opts);
        }

        // [Test]  
        //  public static void TestFailReadingSpecs() 
        //  {
        //      string cmd_line_opts = @"-i \\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule -s  \\proto-2\UnitTest_Files\ImsMetabolitesFinderBatchProcessorTestFiles // \ExampleSearchSpecError.txt -p 5 -w";
        //      System.Diagnostics.Process.Start(BatchProcessor, cmd_line_opts);
        //  }

        [Test]  
        public static void TestPreprocessing()
        {
            BincCentricIndexing.IndexUimfFile(@"\\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule\EXP-AAP_pos_12Sep14_Columbia_DI.uimf");
        }

        // This test is too destructive. Disabled for good.
        //[Test]
        //public static void TestMedac2UIMFConvert()
        //{
        //    const string filePath = @"\\proto-2\UnitTest_Files\Midac\EXP-PRO_pos2_9Oct14_Columbia_DI\EXP-PRO_pos2_9Oct14_Columbia_DI.d";
        //    Midac2UIMFConvert.Convert(filePath, "");
        //}

        [Test]  
        public static void InferIonizationTest()
        {
            List<string> testCases = new List<string>();
            testCases.Add("EXP-PLG_pos_28May14_Columbia_DI");
            testCases.Add("EXP-TCY_neg_26Aug14_Columbia_DI");
            testCases.Add("EXP-VIN_neg_25Aug14_Columbia_DI"); 
            testCases.Add("EXP-DSS_pos_28May14_Columbia_DI"); 
            testCases.Add("EXP-TET_neg2_28Aug14_Columbia_DI");
            testCases.Add("There is nothing I guess");
            testCases.Add("EXP-DSS_POS_28May14_Columbia_DI");
            testCases.Add("EXP-DSS-28May14_Columbia_DI");
        }

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
                result2 = (MoleculeInformedWorkflowResult) formatter.Deserialize(stream);
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
