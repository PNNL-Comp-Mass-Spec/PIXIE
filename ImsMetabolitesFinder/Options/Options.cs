// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Options.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the Options type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinder.Options
{
    using CommandLine;
    using CommandLine.Text;

    using IMSMetabolitesFinder;

    /// <summary>
    /// The options.
    /// </summary>
    public class Options
    {
        [VerbOption("find", HelpText = "Extract cross section from direct injection IMS data over a range of drift tube voltages")]
        public FinderOptions FindVerb { get; set; }

        [VerbOption("convert", HelpText = "Convert one file format to another, e.g. multi-voltage UIMF to single-frame UIMFs, multi-voltage UIMF to MzML, .d to UIMF, etc.")]
        public ConverterOptions ConvertVerb { get; set; }

        [VerbOption("index", HelpText = "Add bin centric table to UIMF files to speed up processing")]
        public IndexerOptions IndexerVerb { get; set; }

        [VerbOption("match", HelpText = "Search and quantify target species in a dataset using an existing AMT drift time library")]
        public MatchOptions MatchVerb { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
          return HelpText.AutoBuild(this, verb);
        }
    }
}
