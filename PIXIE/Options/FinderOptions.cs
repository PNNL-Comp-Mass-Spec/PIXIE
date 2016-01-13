// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinderOptions.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   The options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PIXIE.Options
{
    using System;
    using System.Collections.Generic;

    using CommandLine;
    using CommandLine.Text;

    using ImsInformed.Workflows.CrossSectionExtraction;

    using PIXIE;

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
        private int maxOutliers;

        /// <summary>
        /// The peak shape score threshold.
        /// </summary>
        private double peakShapeScoreThreshold;

        
        /// <summary>
        /// Gets or sets a value indicating whether detailed verbose.
        /// </summary>
        [Option('d', "drifttubelength", Required = true, HelpText = "Specify length of the drift tube in centimeters")]
        public double DriftTubeLength{ get; set; }

                
        /// <summary>
        /// Gets or sets a value indicating whether detailed verbose.
        /// </summary>
        [Option("framesfraction", DefaultValue = CrossSectionSearchParameters.DefaultInsufficientFramesFraction, HelpText = "Specify fration of max accumulated frames under which voltage groups would be discarded")]
        public double InsufficientFramesFraction{ get; set; }

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Option('i', "input", Required = true, HelpText = "The input UIMF file to be read.")]
        public string InputPath { get; set; }

        /// <summary>
        /// Gets or sets the ionization method.
        /// </summary>
        [OptionList('m', "method", Required = true, Separator = ',', HelpText = "Select the ionization method used for the given experiment(Choose one or many: M+H, M-H, M+Na, APCI, M+HCOO, M-2H+Na), separated by a comma, no space.")]
        public IList<string> IonizationList { get; set; }
        
        /// <summary>
        /// Gets or sets the target list.
        /// </summary>
        [OptionList('t', "targetlist", Required = true, Separator = ',', HelpText = "Select the target to be searched, e.g., C2H5OH, 220.55, separated by a comma, no space.")]
        public IList<string> TargetList { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        [Option('o', "output", DefaultValue = "", HelpText = "Directory where the search result and the QC file get stored.")]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating pause when done.
        /// </summary>
        [Option('p', "pause", DefaultValue = false, HelpText = "Pause the program after processing")]
        public bool PauseWhenDone { get; set; }

        /// <summary>
        /// Gets or sets the ppm error.
        /// </summary>
        [Option("pre_ppm", DefaultValue = CrossSectionSearchParameters.DefaultMzWindowHalfWidthInPpm, HelpText = "Specify the PPM error window for initial MZ search based on the target list.")]
        public double PrePpm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed verbose.
        /// </summary>
        [Option('v', "verbose", DefaultValue = true, HelpText = "Detailed verbose and log of each step of the finder algorithm")]
        public bool DetailedVerbose{ get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed verbose.
        /// </summary>
        [Option("smoothingpoints", DefaultValue = CrossSectionSearchParameters.DefaultNumPointForSmoothing, HelpText = "Specify the number of points to be used to smooth the IMS spectra")]
        public int NumberOfPointsForSmoothing{ get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed verbose.
        /// </summary>
        [Option("filterlevel", DefaultValue = CrossSectionSearchParameters.DefaultFeatureFilterLevel, HelpText = "Specify filter level for multidimensional peak finding")]
        public double FeatureFilterLevel{ get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed verbose.
        /// </summary>
        [Option("trelativeintensity", DefaultValue = CrossSectionSearchParameters.DefaultRelativeIntensityPercentageThreshold, HelpText = "Relative intensity as percentage of the highest peak intensity in a single m/z range")]
        public double RelativeIntensityPercentageThreshold{ get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed verbose.
        /// </summary>
        [Option("tdrifttime", DefaultValue = CrossSectionSearchParameters.DefaultDriftTimeToleranceInMs, HelpText = "Drift time tolerance in milliseconds")]
        public double DriftTimeToleranceInMs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed verbose.
        /// </summary>
        [Option('r', "tr2", DefaultValue = CrossSectionSearchParameters.DefaultMinR2, HelpText = "Specify the minimum acceptable R^2 value identifying peak responses belonging to the same ion")]
        public double MinR2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed verbose.
        /// </summary>
        [Option('g', "graphics", DefaultValue = CrossSectionSearchParameters.DefaultGraphicsExtension, HelpText = "Specify the format you want to qc file to be plotted, e.g., svg, png")]
        public string GraphicsFormat{ get; set; }

        /// <summary>
        /// Gets or sets a value indicating the type of regression to use.
        /// </summary>
        [Option("robustregression", DefaultValue = true, HelpText = "Use iteratively reweighted least squares, weighted using bisquare weights to imporve measurement accuracy. Highly recommanded")]
        public bool RobustRegression{ get; set; }

        /// <summary>
        /// Gets or sets the intensity threshold.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
        [Option("tabsoluteintensity", DefaultValue = CrossSectionSearchParameters.DefaultAbsoluteIntensityThreshold, HelpText = "Specify absolute intensity score threshold for features, 0 - 1")]
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
        [Option("maxoutliers", DefaultValue = CrossSectionSearchParameters.DefaultMaxOutliers, HelpText = "Specify minimum number of fit points required to compute cross section")]
        public int MaxOutliers
        {
            get
            {
                return this.maxOutliers;
            }
            set
            {
                this.maxOutliers = value;
            }
        }

        /// <summary>
        /// Gets or sets the isotopic score threshold.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
        [Option("tisotopicscore", DefaultValue = CrossSectionSearchParameters.DefaultIsotopicThreshold, HelpText = "Specify isotopic profile score threshold for features, 0 - 1")]
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
        [Option("tpeakshapescore", DefaultValue = CrossSectionSearchParameters.DefaultPeakShapeThreshold, HelpText = "Specify peak shape score threshold for features, 0 - 1")]
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
                Heading = new HeadingInfo("PIXIE", typeof(Program).Assembly.GetName().Version.ToString()),
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
            help.AddPreOptionsLine("               PIXIE.exe -i Example.uimf -m M+H -t 255.4");
            help.AddPreOptionsLine(string.Empty);
                        help.AddPreOptionsLine("    ");

            help.AddPreOptionsLine("        Example 2: to search for nicotine in UIMF file");
            help.AddPreOptionsLine("                   EXP-NIC_neg2_28Aug14_Columbia_DI.uimf");
            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine("               PIXIE.exe -m M-H, -t C10H14N2");
            help.AddPreOptionsLine("                 -i EXP-NIC_neg2_28Aug14_Columbia_DI.uimf");
            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine(string.Empty);

            help.AddDashesToOption = true;
            help.AddOptions(this);
            
            return help;
        }
    }
}
