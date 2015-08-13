// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#define DEBUG

namespace IMSMetabolitesFinder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using CommandLine;

    using IFinder.Options;

    using ImsInformed;
    using ImsInformed.IO;
    using ImsInformed.Targets;
    using ImsInformed.Util;
    using ImsInformed.Workflows.CrossSectionExtraction;
    using ImsInformed.Workflows.DriftTimeLibraryMatch;
    using ImsInformed.Workflows.VoltageAccumulation;

    using ImsMetabolitesFinder.Options;
    using ImsMetabolitesFinder.Preprocess;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        [STAThread]
        public static int Main(string[] args)
        {
            string invokedVerb = "Nothing";
            object invokedVerbInstance = null;
            var options = new Options();
            if (!Parser.Default.ParseArguments(
                args,
                options,
                (verb, subOptions) =>
                    {
                    invokedVerb = verb;
                    invokedVerbInstance = subOptions;
                }))
            {
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            if (invokedVerb == "find") 
            {
                return ExecuteFinder((FinderOptions)invokedVerbInstance);
            }
            else if (invokedVerb == "convert")
            {
                return ExecuteConverter((ConverterOptions)invokedVerbInstance);
            }
            else if (invokedVerb == "index")
            {
                return ExecuteIndexer((IndexerOptions)invokedVerbInstance);
            }
            else if (invokedVerb == "match")
            {
                return ExecuteMatch((MatchOptions)invokedVerbInstance);
            }
            else 
            {
                return 0;
            }
        }

        /// <summary>
        /// The execute indexer.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private static int ExecuteIndexer(IndexerOptions options)
        {
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
                foreach (var file in inputFiles)
                {
                    BincCentricIndexing.IndexUimfFile(file);
                }

                return 0;
            }
            else 
            {
                Console.WriteLine("Not all files found, abort ImsMetabolitesFinder");
                return 1;
            }
        }

        /// <summary>
        /// The execute converter.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        private static int ExecuteConverter(ConverterOptions options)
        {
            string inputPath = options.InputPath;
            string outputPath = options.OutputPath;
            string inputExtension = Path.GetExtension(inputPath).ToLower();
            string conversionType = options.ConversionType.ToLower();
            string outputExtension = Path.GetExtension(outputPath).ToLower();
            if (inputExtension == ".uimf")
            {
                if (conversionType == "uimf")
                {
                    VoltageAccumulationWorkflow workflow = new VoltageAccumulationWorkflow(false, inputPath, outputPath);
                    return Convert.ToInt32(workflow.RunVoltageAccumulationWorkflow(FileFormatEnum.UIMF));
                }
                else if (conversionType == "mzml")
                {
                    VoltageAccumulationWorkflow workflow = new VoltageAccumulationWorkflow(false, inputPath, outputPath);
                    return Convert.ToInt32(workflow.RunVoltageAccumulationWorkflow(FileFormatEnum.MzML));
                }
                else
                {
                    throw new Exception("Output type " + inputExtension.ToLower() + " not supported");
                }
            }
            else if (inputExtension == ".d")
            {
                if (outputExtension == "uimf" || conversionType == "uimf")
                {
                    if (outputExtension != "uimf")
                    {
                        DirectoryInfo info = new DirectoryInfo(inputPath);
                        string fileName = info.Name.Replace(".d", "");
                        outputPath = Path.Combine(outputPath, fileName + ".uimf");
                    }
                    
                    Task conversion = AgilentToUimfConversion.ConvertToUimf(inputPath, outputPath);
                    conversion.Wait();
                }
                else
                {
                    throw new Exception("Output type " + inputExtension.ToLower() + " not supported");
                }
            }
            else
            {
                throw new Exception("Input type " + inputExtension.ToLower() + " not supported");
            }

            return 0;
        }

        /// <summary>
        /// The execute finder.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private static int ExecuteFinder(FinderOptions options)
        {
            try
            {
                string uimfFile = options.InputPath; // get the UIMF file
                string datasetName = Path.GetFileNameWithoutExtension(uimfFile);
                string resultName = datasetName + "_"  + "Result.bin";
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
                    options.MinFitPoints,
                    CrossSectionSearchParameters.DefaultPeakDetectorSelection,
                    options.MinR2,
                    options.PostPpm,
                    options.RelativeIntensityPercentageThreshold,
                    options.GraphicsFormat);

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
                            string chemicalIdentifier = target.Item1;

                            bool isDouble = Double.TryParse(formula, out Mz);
                            if (!isDouble)
                            {
                                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                                formula = rgx.Replace(formula, "");
                            }

                            if (!isDouble)
                            {
                                currentTarget = new MolecularTarget(formula, method, chemicalIdentifier);
                                targets.Add(currentTarget);
                            }

                            else 
                            {
                                currentTarget = new MolecularTarget(Mz, method, chemicalIdentifier);
                                targets.Add(currentTarget);
                            }
                        }
                        catch (Exception e)
                        {
                            // In case of error creating targets, create the target error result
                            CrossSectionWorkflowResult informedResult = CrossSectionWorkflowResult.CreateErrorResult(currentTarget, datasetName);
                            
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
                
                if (options.PauseWhenDone)
                {
                    PauseProgram();
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

                if (options.PauseWhenDone)
                {
                    PauseProgram();
                }

                return 1;
            }
        }

        /// <summary>
        /// The execute match.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private static int ExecuteMatch(MatchOptions options)
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
            var parameters = new LibraryMatchParameters(options.DriftTimeError, 250, 9, options.PeakShapeScoreThreshold, options.IsotopicScoreThreshold, 0.25, options.MassError);
            LibraryMatchWorkflow workflow = new LibraryMatchWorkflow(inputPath, outputDirectory, resultName, parameters);
            IDictionary<DriftTimeTarget, LibraryMatchResult> results = workflow.RunLibraryMatchWorkflow(importDriftTimeLibrary);

            // Write out the target / result pairs as serialized objects

            if (options.PauseWhenDone)
            {
                PauseProgram();
            }

            return 1;
        }

        private static void PauseProgram() 
        {
            // hault the process from exiting to give the user more time in case of user double clicking this 
            // file from within Windows Explorer (or starting the program via a shortcut).
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();            
        }

        private static bool IsPeptideSequence(string input)
        {
            throw new NotImplementedException();
        }

        private static Tuple<string, string> ParseTargetToken(string targetToken, string datasetName)
        {
            string[] items = targetToken.Split("-|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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
