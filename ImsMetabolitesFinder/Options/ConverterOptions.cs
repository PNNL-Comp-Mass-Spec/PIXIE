// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConverterOptions.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the ConverterOptions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinder.Options
{
    using CommandLine;
    using CommandLine.Text;

    using IMSMetabolitesFinder;

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
        [Option('o', "output", DefaultValue = "", HelpText = "Output sirectory.")]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets format to convert to
        /// </summary>
        [Option('t', "type", DefaultValue = "uimf", HelpText = "The format to convert to, such as uimf, mzML")]
        public string ConversionType { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText {
                Heading = new HeadingInfo("ImsMetabolitesFinder", typeof(Program).Assembly.GetName().Version.ToString()),
                Copyright = new CopyrightInfo("PNNL", 2014),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine("    This application searches for the presence of a");
            help.AddPreOptionsLine("    target molecule inside a UIMF file.  The target");
            help.AddPreOptionsLine("    mobility(K0) and cross sectional area(A) can be");
            help.AddPreOptionsLine("    computed with varying drift tube voltages");
            help.AddPreOptionsLine(string.Empty);
            return help;
        }
    }
}
