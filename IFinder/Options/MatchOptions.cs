using System.Collections.Generic;

namespace ImsMetabolitesFinder.Options
{
    using System;

    using CommandLine;
    using CommandLine.Text;

    using IMSMetabolitesFinder;

    /// <summary>
    /// The match options.
    /// </summary>
    public class MatchOptions
    {
        /// <summary>
        /// The isotopic score threshold.
        /// </summary>
        private double isotopicScoreThreshold;

        /// <summary>
        /// The peak shape score threshold.
        /// </summary>
        private double peakShapeScoreThreshold;

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Option('i', "input", Required = true, HelpText = "Input UIMF files to be read.")]
        public string InputPath { get; set; }

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Option('l', "library", Required = true, HelpText = "AMT library to match the dataset against")]
        public string LibraryPath { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        [Option('o', "output", DefaultValue = "", HelpText = "Folder where the match results get stored.")]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating pause when done.
        /// </summary>
        [Option('p', "pause", DefaultValue = false, HelpText = "Pause the program when result is generated.")]
        public bool PauseWhenDone { get; set; }

        /// <summary>
        /// Gets or sets the ppm error.
        /// </summary>
        [Option('m', "ppm", DefaultValue = 25, HelpText = "Specify the ± mass error in ppm when verifying a detected feature for a match.")]
        public int MassError { get; set; }

        /// <summary>
        /// Gets or sets the ppm error.
        /// </summary>
        [Option('d', "ms", DefaultValue = 1, HelpText = "Specify the ± drift tiem in milliseconds error when verifying a detected feature for a match.")]
        public int DriftTimeError { get; set; }

        /// <summary>
        /// Gets or sets the isotopic score threshold.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
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

        /// <summary>
        /// Gets or sets the peak shape score threshold.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
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

        /// <summary>
        /// The get usage.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText {
                Heading = new HeadingInfo("ImsMetabolitesFinder", typeof(Program).Assembly.GetName().Version.ToString()),
                Copyright = new CopyrightInfo("PNNL", 2015),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine("    searches for the presence of a targets in the AMT");
            help.AddPreOptionsLine("    drift time library");
            help.AddPreOptionsLine(string.Empty);
            
            help.AddPreOptionsLine("    Usage:");
            help.AddPreOptionsLine("        Example 1: Match targets specified by Library.txt to UIMF file");
            help.AddPreOptionsLine("                   Example.uimf with mz tolerance of 250 ppm and drift");
            help.AddPreOptionsLine("                   time tolerance of 1 millisecond");
            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine("               ImsMetabolitesFinder match -i Example.uimf -l Library.txt -m 250 -d 1.0");
            help.AddPreOptionsLine(string.Empty);
                        help.AddPreOptionsLine("    ");

            help.AddPreOptionsLine(string.Empty);

            help.AddDashesToOption = true;
            help.AddOptions(this);
            
            return help;
        }
    }
}
