namespace PIXIE.Options
{
    using System.Collections.Generic;

    using CommandLine;
    using CommandLine.Text;

    using PIXIE;

    /// <summary>
    /// The indexer options.
    /// </summary>
    public class IndexerOptions
    {
        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        [ValueList(typeof(List<string>))]
        public IList<string> UimfFileLocation { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText {
                Heading = new HeadingInfo("PIXIE", typeof(Program).Assembly.GetName().Version.ToString()),
                Copyright = new CopyrightInfo("PNNL", 2015),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddPostOptionsLine("  Example: PIXIE [YourData.UIMF]");
            help.AddPostOptionsLine("  Example: PIXIE [YourData1.UIMF] [YourData2.UIMF] [YourData3.UIMF]...");
            help.AddPostOptionsLine(string.Empty);
            help.AddOptions(this);
            return help;
        }
    }
}
