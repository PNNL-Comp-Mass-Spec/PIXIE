using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIXIE.Runners
{
    using System.IO;

    using ImsInformed.IO;
    using ImsInformed.Targets;
    using ImsInformed.Workflows.DriftTimeLibraryMatch;

    using PIXIE.Options;

    public class Matcher
    {
        /// <summary>
        /// The execute match.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int ExecuteMatch(MatchOptions options)
        {
            string inputPath = options.InputPath;  // get the UIMF file
            string libraryPath = options.LibraryPath;
            string datasetName = Path.GetFileNameWithoutExtension(inputPath);
            string libraryFlieName = Path.GetFileNameWithoutExtension(libraryPath);
            string resultName = datasetName + "_" + libraryFlieName + "_MatchResult.txt";
            string resultPath = Path.Combine(options.OutputPath, resultName);
            string outputDirectory = options.OutputPath;

            if (outputDirectory == string.Empty)
            {
                outputDirectory = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(outputDirectory))
            {
                try
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                catch (Exception)
                {
                    Console.WriteLine(string.Format("Failed to create directory {0}", outputDirectory));
                    throw;
                }
            }

            // Delete the result file if it already exists
            if (File.Exists(resultPath))
            {
                File.Delete(resultPath);
            }

            IList<DriftTimeTarget> importDriftTimeLibrary = DriftTimeLibraryImporter.ImportDriftTimeLibrary(libraryPath);
            var parameters = new LibraryMatchParameters(options.DriftTimeError, 250, 9, options.PeakShapeScoreThreshold, options.IsotopicScoreThreshold, 0.25, options.MassError, options.DriftTubeLength);
            LibraryMatchWorkflow workflow = new LibraryMatchWorkflow(inputPath, outputDirectory, resultName, parameters);
            IDictionary<DriftTimeTarget, LibraryMatchResult> results = workflow.RunLibraryMatchWorkflow(importDriftTimeLibrary);

            // Write out the target / result pairs as serialized objects

            return 1;
        }
    }
}
