
namespace ImsMetabolitesFinderTest
{
    using System;

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
        public static void TestBatchProcessor()
        {
        
        }
    }
}
