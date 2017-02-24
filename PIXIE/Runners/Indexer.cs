using System;
using System.IO;

using PIXIE.Options;
using PIXIE.Preprocess;

namespace PIXIE.Runners
{
    using UIMFLibrary;

    public class Indexer
    {
        public static bool CheckIndex(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            var lowerExt = ext.ToLower();
            if (lowerExt != ".uimf")
            {
                return false;
            }

            using (var dataReader = new DataReader(filePath))
            {
                return dataReader.DoesContainBinCentricData();
            }
        }

        /// <summary>
        /// The execute indexer.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="progressReporter"></param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int ExecuteIndexer(IndexerOptions options, IProgress<double> progressReporter = null)
        {
            progressReporter = progressReporter ?? new Progress<double>();

            var inputFiles = options.UimfFileLocation;
            if (inputFiles.Count < 1)
            {
                Console.Error.Write(options.GetUsage());
                return 1;
            }

            // verify that all files exist
            bool allFound = true;
            foreach (var file in inputFiles)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine("File {0} is not found", file);
                    allFound = false;
                }
            }

            if (allFound)
            {
                Console.WriteLine("Start Preprocessing:");
                for (int i = 0; i < inputFiles.Count; i++)
                {
                    var file = inputFiles[i];
                    BincCentricIndexing.IndexUimfFile(file);
                    progressReporter.Report(100.0 * i / inputFiles.Count);
                }

                return 0;
            }
            else
            {
                Console.WriteLine("Not all files found, abort PIXIE");
                return 1;
            }
        }
    }
}
