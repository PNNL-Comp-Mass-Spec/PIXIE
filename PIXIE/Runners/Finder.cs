using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIXIE.Runners
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.RegularExpressions;

    using ImsInformed;
    using ImsInformed.Statistics;
    using ImsInformed.Targets;
    using ImsInformed.Workflows.CrossSectionExtraction;

    using PIXIE.Options;
    using PIXIE.Preprocess;

    public class Finder
    {
        /// <summary>
        /// The execute finder.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int ExecuteFinder(FinderOptions options)
        {
            try
            {
                string uimfFile = options.InputPath; // get the UIMF file
                string datasetName = Path.GetFileNameWithoutExtension(uimfFile);
                string resultName = datasetName + "_" + "Result.bin";
                string resultPath = Path.Combine(options.OutputPath, resultName);
                string outputDirectory = options.OutputPath;
                IList<string> targetList = options.TargetList;
                bool verbose = options.DetailedVerbose;

                if (outputDirectory == string.Empty)
                {
                    outputDirectory = Directory.GetCurrentDirectory();
                }

                if (!outputDirectory.EndsWith("\\"))
                {
                    outputDirectory += "\\";
                }

                if (!Directory.Exists(outputDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to create directory.");
                        throw;
                    }
                }

                // Delete the result file if it already exists
                if (File.Exists(resultPath))
                {
                    File.Delete(resultPath);
                }

                // Load parameters
                double Mz;

                CrossSectionSearchParameters searchParameters = new CrossSectionSearchParameters(
                    options.DriftTimeToleranceInMs,
                    options.PrePpm,
                    options.NumberOfPointsForSmoothing,
                    options.FeatureFilterLevel,
                    options.IntensityThreshold,
                    options.PeakShapeScoreThreshold,
                    options.IsotopicScoreThreshold,
                    options.MaxOutliers,
                    CrossSectionSearchParameters.DefaultPeakDetectorSelection, // No longer an option
                    options.RobustRegression ? FitlineEnum.IterativelyBiSquareReweightedLeastSquares : FitlineEnum.OrdinaryLeastSquares,
                    options.MinR2,
                    options.RelativeIntensityPercentageThreshold,
                    options.GraphicsFormat,
                    options.InsufficientFramesFraction,
                    options.DriftTubeLength,
                    options.UseAverageTemperature);

                IFormatter formatter = new BinaryFormatter();

                // If target cannot be constructed. Create a result.
                IList<IImsTarget> targets = new List<IImsTarget>();
                IImsTarget currentTarget = null;
                IList<CrossSectionWorkflowResult> errorTargets = new List<CrossSectionWorkflowResult>();
                foreach (string item in targetList)
                {
                    foreach (string ionization in options.IonizationList)
                    {
                        try
                        {
                            // get the ionization method.
                            IonizationMethod method = IonizationMethodUtilities.ParseIonizationMethod(ionization.Trim());

                            Tuple<string, string> target = ParseTargetToken(item, datasetName);
                            string formula = target.Item2;
                            string sampleClass = target.Item1;

                            bool isDouble = Double.TryParse(formula, out Mz);
                            if (!isDouble)
                            {
                                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                                formula = rgx.Replace(formula, "");
                            }

                            if (!isDouble)
                            {
                                currentTarget = new MolecularTarget(formula, method, sampleClass);
                                targets.Add(currentTarget);
                            }

                            else
                            {
                                currentTarget = new MolecularTarget(Mz, method, sampleClass);
                                targets.Add(currentTarget);
                            }
                        }
                        catch (Exception e)
                        {
                            // In case of error creating targets, create the target error result
                            CrossSectionWorkflowResult informedResult = CrossSectionWorkflowResult.CreateErrorResult(currentTarget, datasetName, uimfFile, resultPath, "");

                            using (Stream stream = new FileStream("serialized_result.bin", FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                formatter.Serialize(stream, informedResult);
                            }

                            throw e;
                        }
                    }
                }

                // Preprocessing
                Console.WriteLine("Start Preprocessing:");
                BincCentricIndexing.IndexUimfFile(uimfFile);

                // Run algorithms in IMSInformed
                CrossSectionWorkfow workflow = new CrossSectionWorkfow(uimfFile, outputDirectory, searchParameters);
                IList<CrossSectionWorkflowResult> results = workflow.RunCrossSectionWorkFlow(targets, verbose);
                workflow.Dispose();

                // Merge the target error result dictionary and other results
                foreach (var pair in errorTargets)
                {
                    results.Add(pair);
                }

                // Serialize the result
                string binPath = Path.Combine(outputDirectory, resultName);

                using (Stream stream = new FileStream(binPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(stream, results);
                }

                // Define success
                foreach (CrossSectionWorkflowResult result in results)
                {
                    if (!(result.AnalysisStatus == AnalysisStatus.Positive || result.AnalysisStatus == AnalysisStatus.Negative || result.AnalysisStatus == AnalysisStatus.NotSufficientPoints | result.AnalysisStatus == AnalysisStatus.Rejected))
                    {
                        return 1;
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                string uimfFile = options.InputPath; // get the UIMF file
                string datasetName = Path.GetFileNameWithoutExtension(uimfFile);
                string resultName = datasetName + "_Result.txt";
                string resultPath = Path.Combine(options.OutputPath, resultName);
                string outputDirectory = options.OutputPath;

                if (outputDirectory == string.Empty)
                {
                    outputDirectory = Directory.GetCurrentDirectory();
                }

                if (!outputDirectory.EndsWith("\\"))
                {
                    outputDirectory += "\\";
                }

                if (!Directory.Exists(outputDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to create directory.");
                        throw;
                    }
                }

                // Delete the result file if it already exists
                if (File.Exists(resultPath))
                {
                    File.Delete(resultPath);
                }

                Console.WriteLine(e.Message);
                using (StreamWriter errorFile = File.CreateText(resultPath))
                {
                    errorFile.Write(e.Message);
                    errorFile.Write(e.StackTrace);
                }

                return 1;
            }
        }

        private static Tuple<string, string> ParseTargetToken(string targetToken, string datasetName)
        {
            string[] items = targetToken.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (items.Length == 2)
            {
                return new Tuple<string, string>(items[0], items[1]);
            }
            else if (items.Length == 1)
            {
                return new Tuple<string, string>(InferChemicalIdentifier(datasetName), items[0]);
            }
            else
            {
                throw new ArgumentException(string.Format("Cannot parse target {0}", targetToken));
            }
        }

        /// <summary>
        /// The infer chemical identifier.
        /// </summary>
        /// <param name="datasetName">
        /// The dataset name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private static string InferChemicalIdentifier(string datasetName)
        {
            if (datasetName.ToLower().Contains("mix"))
            {
                throw new ArgumentException("Cannot infer chemical identifier from Mixed dataset, please specify in search specs.");
            }
            else
            {
                MetadataFromDatasetName metadata = new MetadataFromDatasetName(datasetName);
                return metadata.SampleIdentifier;
            }
        }
    }
}
