namespace ImsMetabolitesFinder
{
    using System.Text;

    using CommandLine;
    using CommandLine.Text;

    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input UIMF files to be read.")]
        public string InputPath { get; set; }

        [Option('t', "target", Required = true, HelpText = "IMS targeted to be identified. Could either be an Mz value(e.g. 192.23), or empirical formula (e.g. C6H10ClN5)")]
        public string Target { get; set; }

        [Option('m', "method", Required = true, HelpText = "Select the ionization method used for the given experiment(M+H, M-H, M+Na)")]
        public string IonizationMethod { get; set; }

        [Option('p', "ppm", DefaultValue = 10, HelpText = "Specify the PPM error allowed for MZ search.")]
        public int PpmError { get; set; }

        [Option('o', "output", DefaultValue = "", HelpText = "Folder where the search result and the QC file get stored.")]
        public string OutputPath { get; set; }

        [Option("isotopic", DefaultValue = false, HelpText = "Use isotopic feature instead of Mz only for feature selection.")]
        public bool IsotopicFeatureEnable { get; set; }

        [Option('l', "log", DefaultValue = false, HelpText = "Enable Log File")]
        public bool LogEnable { get; set; }

        [Option('q', "qc", DefaultValue = false, HelpText = "Enable quality control")]
        public bool QcEnable { get; set; }

        [Option("ID", DefaultValue = 0, HelpText = "An unique ID to keep track of search for batch processing")]
        public int ID{ get; set; }

        [Option('v', "Verbose", DefaultValue = false, HelpText = "Detailed console output.")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText {
                Heading = new HeadingInfo("ImsMetabolitesFinder", "1.0.0.0"),
                Copyright = new CopyrightInfo("PNNL", 2014),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine("    This application searches for the precense of a");
            help.AddPreOptionsLine("    target molecule inside a UIMF file.  The target");
            help.AddPreOptionsLine("    mobility(K0) and cross sectional area(A) can be");
            help.AddPreOptionsLine("    computed with varying drift tube voltages");
            help.AddPreOptionsLine("");
            
            help.AddPreOptionsLine("    Usage:");
            help.AddPreOptionsLine("        Example 1: To search for Mz = 255.4 in UIMF file Example.uimf");
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine("               ImsMetabolitesFinder.exe -i Example.uimf, -m M+H, -t 255.4");
            help.AddPreOptionsLine("");
                        help.AddPreOptionsLine("    ");

            help.AddPreOptionsLine("        Example 2: to search for nicotine in UIMF file");
            help.AddPreOptionsLine("                   EXP-NIC_neg2_28Aug14_Columbia_DI.uimf");
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine("               ImsMetabolitesFinder.exe -m M-H, -t C10H14N2");
            help.AddPreOptionsLine("                 -i EXP-NIC_neg2_28Aug14_Columbia_DI.uimf");
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine("");

            help.AddDashesToOption = true;
            help.AddOptions(this);
            
            return help;
        }
    }
}
