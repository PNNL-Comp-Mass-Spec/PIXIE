
namespace ImsMetabolitesFinderTest
{
    using System;
    using System.Collections.Generic;
    using ImsMetabolitesFinderBatchProcessor;
    using NUnit.Framework;

    public class RunTest
    {
        //default values
        private const string output_file_dir = @"output\";
        private const string exe_dir = @"..\..\..\ImsMetabolitesFinder\Bin\Debug\ImsMetabolitesFinder.exe";
        private const string uimfFilePath = @"\\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule\EXP-BPS_pos2_13Sep14_Columbia_DI.uimf";

        [Test]  
        public static void TestImsMetabolitesFinder_Help() 
        {
            string sample_name = "EXP-BPS_pos2_13Sep14_Columbia_DI";
            string cmd_line_opts = "-h"; 
            Console.WriteLine("Test case: Help page " + sample_name + "\r\n");
            Console.WriteLine("invoking ImsMetabolitesFinder . . .\r\n");
            System.Diagnostics.Process.Start(exe_dir, cmd_line_opts);
        }

        [Test]  
        public static void TestImsMetabolitesFinder_MZ() 
        {
            string sample_name = "EXP-BPS_pos2_13Sep14_Columbia_DI";
            string cmd_line_opts = "-i " + uimfFilePath + " -t 273.0192006876" + " -m M+Na" + " -o output/"; 
            Console.WriteLine("Test case: detecting target particle list using its Mz value, for " + sample_name + "\r\n");
            Console.WriteLine("invoking ImsMetabolitesFinder . . .\r\n");
            System.Diagnostics.Process.Start(exe_dir, cmd_line_opts);
        }

        [Test]  
        public static void TestImsMetabolitesFinder_Formula() 
        {
            string sample_name = "EXP-BPS_pos2_13Sep14_Columbia_DI";
            string cmd_line_opts = "-i " + uimfFilePath + " -t C12H10O4S" + " -m M+Na" + " -o output/"; 
            Console.WriteLine("Test case: detecting target particle using its empirical formula, for " + sample_name + "\r\n");
            Console.WriteLine("invoking ImsMetabolitesFinder . . .\r\n");
            System.Diagnostics.Process.Start(exe_dir, cmd_line_opts);
        }

       [Test]  
        public static void TestImsMetabolitesFinder_Formula2() 
        {
            string cmd_line_opts = @"-i \\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule\EXP-NIC_pos2_13Sep14_Columbia_DI.uimf -t C10H14N2 -m M+H -o \\proto-2\UnitTest_Files\IMSInformedTestFiles\uimf_files\smallMolecule/EXP-NIC_pos2_13Sep14_Columbia_DI_ImsMetabolitesFinderResult_M+H --ID 20 -p 10 -l -q";
            System.Diagnostics.Process.Start(exe_dir, cmd_line_opts);
        }

        [Test]  
        public static void TestBatchProcessor()
        {
        
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
    }
}
