﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Options.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the Options type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PIXIEBatchProcessor
{
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// The options.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets the search spec file.
        /// </summary>
        [Option('s', "searchspec", Required = true, HelpText = "Search spec file")]
        public string SearchSpecFile { get; set; }

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Option('i', "input", Required = true, HelpText = "specify the directory containing the datasets, the datasets could be in a subdirectory of the input folder")]
        public string InputPath { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        [Option('o', "output", DefaultValue = "",
            HelpText = "specify the output directory. If left empty results will be written into the same directory as the input directory")]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the number of processes.
        /// </summary>
        [Option('p', "parallel", DefaultValue = 1, HelpText = "specify the number of processes allocated to run the program.")]
        public int NumberOfProcesses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether show window.
        /// </summary>
        [Option('w', "window", DefaultValue = false, HelpText = "Show the console output for each ImsInformed workflow")]
        public bool ShowWindow { get; set; }

        /// <summary>
        /// Gets or sets the number of analyses per plot.
        /// </summary>
        [Option('n', "datasetsperplot", DefaultValue = 20, HelpText = "specify the number of items per plot in the final sore summary.")]
        public int NumberOfAnalysesPerPlot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether reanalyze.
        /// </summary>
        [Option('r', "reanalyze", DefaultValue = false, HelpText = "If reanalyze is specified, the batch processor would overwrite results from the previous runs if present, instead of using existing results")]
        public bool Reanalyze { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ignore missing files.
        /// </summary>
        [Option('f', "--force", DefaultValue = false, HelpText = "If force is set, the batch processor would start the batch regardless of missing files. Analyses will only be performed on datasets that were found")]
        public bool IgnoreMissingFiles { get; set; }

        /// <summary>
        /// The get usage.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
                           {
                               Heading =
                                   new HeadingInfo(
                                   "PIXIE Batch Processor",
                                   typeof(Program).Assembly.GetName().Version.ToString()),
                               Copyright = new CopyrightInfo("PNNL", 2014),
                               AdditionalNewLineAfterOption = true,
                               AddDashesToOption = true
                           };
            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine("    This application batche processes IMS target identification");
            help.AddPreOptionsLine("    using PIXIE. It reads in a search spec file ");
            help.AddPreOptionsLine("    specifying the Dataset names to be searched and their targets");
            help.AddPreOptionsLine("    files. ");
            help.AddPreOptionsLine(string.Empty);

            help.AddPreOptionsLine("    Usage:");
            help.AddPreOptionsLine("      Example: To use <SEARCH_FILE> on UIMF files in directory <UIMF_DIR>");
            help.AddPreOptionsLine("        PIXIEBatchProcessor.exe -i <UIMF_DIR> -s <SEARCH_FILE>");
            help.AddPreOptionsLine(string.Empty);

            help.AddDashesToOption = true;
            help.AddOptions(this);

            return help;
        }
    }
}
