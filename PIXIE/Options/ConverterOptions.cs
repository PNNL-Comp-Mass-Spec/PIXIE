// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConverterOptions.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the ConverterOptions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PIXIE.Options
{
    using CommandLine;
    using CommandLine.Text;

    using PIXIE;

    public class ConverterOptions
    {
        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Option('i', "input", Required = true, HelpText = "Input UIMF files to be read. Supports .uimf and .D")]
        public string InputPath { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        [Option('o', "output", DefaultValue = "", HelpText = "Output directory.")]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets format to convert to
        /// </summary>
        [Option('t', "type", DefaultValue = "uimf", HelpText = "The format to convert to, such as uimf, mzML")]
        public string ConversionType { get; set; }

        [Option('d', Required = false, HelpText = "IMS Drift Tube length.")]
        public string TubeLength { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText {
                Heading = new HeadingInfo("PIXIE", typeof(Program).Assembly.GetName().Version.ToString()),
                Copyright = new CopyrightInfo("PNNL", 2015),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddOptions(this);
            return help;
        }
    }
}
