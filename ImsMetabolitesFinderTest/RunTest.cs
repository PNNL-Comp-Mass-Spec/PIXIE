
namespace ImsMetabolitesFinderTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using ImsInformed.Domain;
    using ImsInformed.Parameters;
    using ImsInformed.Util;

    using ImsMetabolitesFinder.Preprocess;

    using NUnit.Framework;

    public class RunTest
    {
        //default values
        private const string OutputFileDir = @"output\";
        private const string ExeDir = @"..\..\..\ImsMetabolitesFinder\Bin\Debug\ImsMetabolitesFinder.exe";
        private const string UimfFilePath = @"\\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule\EXP-BPS_pos2_13Sep14_Columbia_DI.uimf";

        [Test]  
        public static void TestImsMetabolitesFinder_Help() 
        {
            string sample_name = "EXP-BPS_pos2_13Sep14_Columbia_DI";
            string cmd_line_opts = "-h"; 
            Console.WriteLine("Test case: Help page " + sample_name + "\r\n");
            Console.WriteLine("invoking ImsMetabolitesFinder . . .\r\n");
            System.Diagnostics.Process.Start(ExeDir, cmd_line_opts);
        }

        [Test]  
        public static void TestImsMetabolitesFinder_MZ() 
        {
            string sample_name = "EXP-BPS_pos2_13Sep14_Columbia_DI";
            string cmd_line_opts = "-i " + UimfFilePath + " -t 273.0192006876" + " -m M+Na" + " -o output/"; 
            Console.WriteLine("Test case: detecting target particle list using its Mz value, for " + sample_name + "\r\n");
            Console.WriteLine("invoking ImsMetabolitesFinder . . .\r\n");
            System.Diagnostics.Process.Start(ExeDir, cmd_line_opts);
        }

        [Test]  
        public static void TestImsMetabolitesFinder_Formula() 
        {
            string sample_name = "EXP-BPS_pos2_13Sep14_Columbia_DI";
            string cmd_line_opts = "-i " + UimfFilePath + " -t C12H10O4S" + " -m M+Na" + " -o output/"; 
            Console.WriteLine("Test case: detecting target particle using its empirical formula, for " + sample_name + "\r\n");
            Console.WriteLine("invoking ImsMetabolitesFinder . . .\r\n");
            System.Diagnostics.Process.Start(ExeDir, cmd_line_opts);
        }

       [Test]  
        public static void TestImsMetabolitesFinder_Formula2() 
        {
            string cmd_line_opts = @"-i \\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule\EXP-NIC_pos2_13Sep14_Columbia_DI.uimf -t C10H14N2 -m M+H -o \\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule/EXP-NIC_pos2_13Sep14_Columbia_DI_ImsMetabolitesFinderResult_M+H --ID 20 -p 10";
            System.Diagnostics.Process.Start(ExeDir, cmd_line_opts);
        }

        [Test]  
        public static void TestPreprocessing()
        {
            BincCentricIndexing.IndexUimfFile(@"\\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule\EXP-NIC_pos2_13Sep14_Columbia_DI.uimf");
        }

        [Test]
        public static void TestMedac2UIMFConvert()
        {
            const string filePath = @"\\proto-2\UnitTest_Files\Midac\EXP-PRO_pos2_9Oct14_Columbia_DI\EXP-PRO_pos2_9Oct14_Columbia_DI.d";
            Midac2UIMFConvert.Convert(filePath, "");
        }

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

                /// <summary>
        /// The test single molecule with formula.
        /// </summary>
        [Test][STAThread]
        public void TestSingleMoleculeWithFormula()
        {
            // Nicotine
            string formula = "C10H14N2";
            ImsTarget sample = new ImsTarget(1, IonizationMethod.Proton2Plus, formula);
            Console.WriteLine("Nicotine:");
            Console.WriteLine("Monoisotopic Mass: " + sample.Mass);
            string fileLocation = @"\\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule\EXP-NIC_pos2_13Sep14_Columbia_DI.uimf";

            MoleculeWorkflowParameters parameters = new MoleculeWorkflowParameters 
            {
                IsotopicFitScoreMax = 0.15,
                MassToleranceInPpm = 10,
                NumPointForSmoothing = 9
            };

            MoleculeInformedWorkflow informedWorkflow = new MoleculeInformedWorkflow(fileLocation, "output", "result.txt", parameters);
            informedWorkflow.RunMoleculeInformedWorkFlow(sample);
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
            result.RSquared = 2;
            result.TargetDescriptor = "H2O";

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
            Assert.AreEqual(result.RSquared, result2.RSquared);
            Assert.AreEqual(result.TargetDescriptor, result2.TargetDescriptor);
        }
    }
}
