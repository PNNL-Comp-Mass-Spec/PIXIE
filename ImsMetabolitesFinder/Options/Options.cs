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
        [VerbOption("find", HelpText = "Drift time and cross section analyses")]
        public FinderOptions FindVerb { get; set; }

        [VerbOption("convert", HelpText = "Convert IMS result to different formats")]
        public ConverterOptions ConvertVerb { get; set; }

        [VerbOption("index", HelpText = "Add bin centric table to UIMF file")]
        public IndexerOptions IndexerVerb { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
          return HelpText.AutoBuild(this, verb);
        }
    }
}
