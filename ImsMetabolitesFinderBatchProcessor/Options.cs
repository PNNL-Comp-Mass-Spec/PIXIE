namespace ImsMetabolitesFinderBatchProcessor
{
    using CommandLine;
    using CommandLine.Text;

    public class Options
    {
        [Option('s', "searchspec", Required = true, HelpText = "Search spec file")]
        public string SearchSpecFile { get; set; }

        [Option('w', "window", DefaultValue = false, HelpText = "Show the console output for each ImsInformed workflow")]
        public bool ShowWindow { get; set; }

        [Option('i', "input", Required = true, HelpText = "specify the directory containing the uimf files, could be recursive")]
        public string InputPath { get; set; }

        [Option('p', "parallel", DefaultValue = 1, HelpText = "specify the number of processes allocated to run the program.")]
        public int NumberOfProcesses { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText {
                Heading = new HeadingInfo("ImsMetabolitesFinder Batch Processor", typeof(Program).Assembly.GetName().Version.ToString()),
                Copyright = new CopyrightInfo("PNNL", 2014),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine("    This application batche processes IMS target identification");
            help.AddPreOptionsLine("    using ImsMetabolitesFinder. It reads in a search spec file ");
            help.AddPreOptionsLine("    specifying the Dataset names to be searched and their targets");
            help.AddPreOptionsLine("    files. ");
            help.AddPreOptionsLine("");
            
            help.AddPreOptionsLine("    Usage:");
            help.AddPreOptionsLine("      Example: To use <SEARCH_FILE> on UIMF files in directory <UIMF_DIR>");
            help.AddPreOptionsLine("        ImsMetabolitesFinderBatchProcessor.exe -i <UIMF_DIR> -s <SEARCH_FILE>");
            help.AddPreOptionsLine("");

            help.AddDashesToOption = true;
            help.AddOptions(this);
            
            return help;
        }
    }
}
