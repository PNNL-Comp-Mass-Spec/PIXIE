// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinderOptions.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   The options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinder.Options
{
    using System;
    using System.Collections.Generic;

    using CommandLine;
    using CommandLine.Text;

    using IMSMetabolitesFinder;

    /// <summary>
    /// The options.
    /// </summary>
    public class FinderOptions
    {
        /// <summary>
        /// The isotopic score threshold.
        /// </summary>
        private double isotopicScoreThreshold;

        /// <summary>
        /// The intensity threshold.
        /// </summary>
        private double intensityThreshold;

        /// <summary>
        /// The min fit points.
        /// </summary>
        private int minFitPoints;

        /// <summary>
        /// The peak shape score threshold.
        /// </summary>
        private double peakShapeScoreThreshold;

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Option('i', "input", Required = true, HelpText = "The input UIMF file to be read.")]
        public string InputPath { get; set; }

        /// <summary>
        /// Gets or sets the ionization method.
        /// </summary>
        [OptionList('m', "method", Required = true, Separator = ',', HelpText = "Select the ionization method used for the given experiment(Choose one or many: M+H, M-H, M+Na, APCI, M+HCOO, M-2H+Na), separated by a comma.")]
        public IList<string> IonizationList { get; set; }
        
        /// <summary>
        /// Gets or sets the target list.
        /// </summary>
        [OptionList('t', "targetlist", Required = true, Separator = ',', HelpText = "Select the target to be searched, e.g., C2H5OH, 220.55, separated by a comma.")]
        public IList<string> TargetList { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        [Option('o', "output", DefaultValue = "", HelpText = "Folder where the search result and the QC file get stored.")]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating pause when done.
        /// </summary>
        [Option('p', "pause", DefaultValue = false, HelpText = "Pause the program when result is generated.")]
        public bool PauseWhenDone { get; set; }

        /// <summary>
        /// Gets or sets the ppm error.
        /// </summary>
        [Option("ppm", DefaultValue = 10, HelpText = "Specify the PPM error allowed for MZ search.")]
        public int PpmError { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed verbose.
        /// </summary>
        [Option('v', "verbose", DefaultValue = true, HelpText = "Detailed verbose and log of each step of the finder algorithm")]
        public bool DetailedVerbose{ get; set; }

        /// <summary>
        /// Gets or sets the intensity threshold.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
        [Option("Tintensity", DefaultValue = 0.0, HelpText = "Specify intensity score threshold for features")]
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

        /// <summary>
        /// Gets or sets the min fit points.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
        [Option("minfitpoints", DefaultValue = 4, HelpText = "Specify minimum number of fit points required to compute cross section")]
        public int MinFitPoints
        {
            get
            {
                return this.minFitPoints;
            }
            set
            {
                if (value > 1)
                {
                    this.minFitPoints = value;
                }
                else
                {
                    throw new ArgumentException("Minimium fit points should be greaster than 1.");
                }
            }
        }

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
            help.AddPreOptionsLine("    This application searches for the presence of a");
            help.AddPreOptionsLine("    target molecule inside a UIMF file.  The target");
            help.AddPreOptionsLine("    mobility(K0) and cross sectional area(A) can be");
            help.AddPreOptionsLine("    computed with varying drift tube voltages");
            help.AddPreOptionsLine(string.Empty);
            
            help.AddPreOptionsLine("    Usage:");
            help.AddPreOptionsLine("        Example 1: To search for Mz = 255.4 in UIMF file Example.uimf");
            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine("               ImsMetabolitesFinder.exe -i Example.uimf -m M+H -t 255.4");
            help.AddPreOptionsLine(string.Empty);
                        help.AddPreOptionsLine("    ");

            help.AddPreOptionsLine("        Example 2: to search for nicotine in UIMF file");
            help.AddPreOptionsLine("                   EXP-NIC_neg2_28Aug14_Columbia_DI.uimf");
            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine("               ImsMetabolitesFinder.exe -m M-H, -t C10H14N2");
            help.AddPreOptionsLine("                 -i EXP-NIC_neg2_28Aug14_Columbia_DI.uimf");
            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine(string.Empty);

            help.AddDashesToOption = true;
            help.AddOptions(this);
            
            return help;
        }
    }
}
