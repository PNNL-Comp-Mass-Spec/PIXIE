namespace ImsMetabolitesFinder.Options
{
    using System.Collections.Generic;

    using CommandLine;
    using CommandLine.Text;

    using IMSMetabolitesFinder;

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
                Heading = new HeadingInfo("ImsMetabolitesFinder", typeof(Program).Assembly.GetName().Version.ToString()),
                Copyright = new CopyrightInfo("PNNL", 2014),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddPostOptionsLine("  Example: ImsMetabolitesFinder [YourData.UIMF]");
            help.AddPostOptionsLine("  Example: ImsMetabolitesFinder [YourData1.UIMF] [YourData2.UIMF] [YourData3.UIMF]...");
            help.AddPostOptionsLine(string.Empty);
            help.AddOptions(this);
            return help;
        }
    }
}
