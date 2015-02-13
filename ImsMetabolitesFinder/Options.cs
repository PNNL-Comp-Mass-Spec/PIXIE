namespace ImsMetabolitesFinder
{
    using System;
    using System.Text;

    using CommandLine;
    using CommandLine.Text;

    using IMSMetabolitesFinder;

    public class Options
    {
        private double isotopicScoreThreshold;
                
        private double intensityThreshold;
                
        private double peakShapeScoreThreshold;

        [Option('i', "input", Required = true, HelpText = "Input UIMF files to be read.")]
        public string InputPath { get; set; }

        [Option('t', "target", Required = true, HelpText = "IMS targeted to be identified. Could either be an Mz value(e.g. 192.23), or empirical formula (e.g. C6H10ClN5)")]
        public string Target { get; set; }

        [Option('m', "method", Required = true, HelpText = "Select the ionization method used for the given experiment(M+H, M-H, M+Na)")]
        public string IonizationMethod { get; set; }

        [Option('o', "output", DefaultValue = "", HelpText = "Folder where the search result and the QC file get stored.")]
        public string OutputPath { get; set; }

        [Option("pause", DefaultValue = false, HelpText = "Pause the program when result is generated.")]
        public bool PalseWhenDone { get; set; }

        [Option("ID", DefaultValue = 0, HelpText = "An unique ID to keep track of search for batch processing")]
        public int ID{ get; set; }

        [Option('p', "ppm", DefaultValue = 10, HelpText = "Specify the PPM error allowed for MZ search.")]
        public int PpmError { get; set; }

        [Option("Tintensity", DefaultValue = 0.5, HelpText = "Specify intensity score threshold for features")]
        public double IntensityThreshold
        {
            get
            {
                return this.intensityThreshold;
            }
            set
            {
                if (value > 0 || value < 1)
                {
                    this.intensityThreshold = value;
                }
                else
                {
                    throw new ArgumentException("Score threshold needs to be between 0 and 1.");
                }
            }
        }

        [Option("Tisotopic", DefaultValue = 0.4, HelpText = "Specify isotopic profile score threshold for features")]
        public double IsotopicScoreThreshold
        {
            get
            {
                return this.isotopicScoreThreshold;
            }
            set
            {
                if (value > 0 || value < 1)
                {
                    this.isotopicScoreThreshold = value;
                }
                else
                {
                    throw new ArgumentException("Score threshold needs to be between 0 and 1.");
                }
            }
        }

        [Option("Tpeakshape", DefaultValue = 0.4, HelpText = "Specify peak shape score threshold for features.")]
        public double PeakShapeScoreThreshold
        {
            get
            {
                return this.peakShapeScoreThreshold;
            }
            set
            {
                if (value > 0 || value < 1)
                {
                    this.peakShapeScoreThreshold = value;
                }
                else
                {
                    throw new ArgumentException("Score threshold needs to be between 0 and 1.");
                }
            }
        }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText {
                Heading = new HeadingInfo("ImsMetabolitesFinder", typeof(Program).Assembly.GetName().Version.ToString()),
                Copyright = new CopyrightInfo("PNNL", 2014),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine("    This application searches for the presence of a");
            help.AddPreOptionsLine("    target molecule inside a UIMF file.  The target");
            help.AddPreOptionsLine("    mobility(K0) and cross sectional area(A) can be");
            help.AddPreOptionsLine("    computed with varying drift tube voltages");
            help.AddPreOptionsLine("");
            
            help.AddPreOptionsLine("    Usage:");
            help.AddPreOptionsLine("        Example 1: To search for Mz = 255.4 in UIMF file Example.uimf");
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine("               ImsMetabolitesFinder.exe -i Example.uimf -m M+H -t 255.4");
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
