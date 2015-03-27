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
    using System.IO;
    using System.Net.Configuration;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.RegularExpressions;

    using ImsInformed.Domain;
    using ImsInformed.IO;
    using ImsInformed.Parameters;
    using ImsInformed.Workflows;

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
            if (!CommandLine.Parser.Default.ParseArguments(args, options, 
                (verb, subOptions) => 
                {
                    invokedVerb = verb;
                    invokedVerbInstance = subOptions;
                }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            if (invokedVerb == "find") 
            {
                return ExecuteFinder((FinderOptions)invokedVerbInstance);
            }
            else if (invokedVerb == "convert")
            {
                return ExecuteConverter((ConverterOptions)invokedVerbInstance);
            }
            else 
            {
                return 0;
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
            else if (inputExtension == "d")
            {
                if (outputExtension == "uimf" || conversionType == "uimf")
                {
                    //MIDAC
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

        private static int ExecuteFinder(FinderOptions options)
        {
            try
            {
                string uimfFile = options.InputPath; // get the UIMF file
                string ionizationMethod = options.IonizationMethod.ToUpper(); 
                string datasetName = Path.GetFileNameWithoutExtension(uimfFile);
                string resultName = datasetName + "_" + ionizationMethod + "_Result.txt";
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

                // Load parameters
                double Mz = 0;
                string formula = string.Empty;
                
                // get the target
                bool isDouble = Double.TryParse(options.Target, out Mz);
                if (!isDouble)
                {
                    formula = options.Target;
                    Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                    formula = rgx.Replace(formula, "");
                }

                bool pause = options.PauseWhenDone;

                int ID = options.ID;
                
                // get the ionization method.
                IonizationMethod method = IonizationMethodUtilities.ParseIonizationMethod(options.IonizationMethod);

                CrossSectionSearchParameters searchParameters = new CrossSectionSearchParameters(
                    4,
                    options.PpmError,
                    9,
                    0.25,
                    options.IntensityThreshold,
                    options.PeakShapeScoreThreshold,
                    options.IsotopicScoreThreshold,
                    options.MinFitPoints); 

                IFormatter formatter = new BinaryFormatter();

                // If target cannot be constructed. Create a result 
                ImsTarget target = null;
                try
                {
                    if (!isDouble)
                    {
                        ImsTarget sample = new ImsTarget(ID, method, formula);
                        target = new ImsTarget(ID, method, formula);
                    } 
                    else 
                    {
                        target = new ImsTarget(ID, method, Mz);
                    }
                }
                catch (Exception)
                {
                    CrossSectionWorkflowResult targetErrorResult;
                    targetErrorResult.DatasetName = datasetName;
                    targetErrorResult.TargetDescriptor = null;
                    targetErrorResult.IonizationMethod = method;
                    targetErrorResult.AnalysisStatus = AnalysisStatus.TargetError;
                    targetErrorResult.Mobility = -1;
                    targetErrorResult.LastVoltageGroupDriftTimeInMs = -1;
                    targetErrorResult.CrossSectionalArea = -1;
                    targetErrorResult.AnalysisScoresHolder.RSquared = 0;
                    targetErrorResult.AnalysisScoresHolder.AverageCandidateTargetScores.IntensityScore = 0;
                    targetErrorResult.AnalysisScoresHolder.AverageCandidateTargetScores.IsotopicScore = 0;
                    targetErrorResult.AnalysisScoresHolder.AverageCandidateTargetScores.PeakShapeScore = 0;
                    targetErrorResult.AnalysisScoresHolder.AverageVoltageGroupStabilityScore = 0;
                    targetErrorResult.MonoisotopicMass = 0;


                    string errorBinPath = Path.Combine(outputDirectory, datasetName + "_" + ionizationMethod + "_Result.bin");

                    using (Stream stream = new FileStream(errorBinPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        formatter.Serialize(stream, targetErrorResult);
                    }

                    throw;
                }

                // Preprocessing
                Console.WriteLine("Start Preprocessing:");
                BincCentricIndexing.IndexUimfFile(uimfFile);

                // Run algorithms in IMSInformed
                CrossSectionWorkfow workflow = new CrossSectionWorkfow(uimfFile, outputDirectory, resultName, searchParameters);
                CrossSectionWorkflowResult result = workflow.RunCrossSectionInformedWorkFlow(target);

                // Serialize the result
                string binPath = Path.Combine(outputDirectory, datasetName + "_" + ionizationMethod + "_Result.bin");

                using (Stream stream = new FileStream(binPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(stream, result);
                }
                
                if (pause)
                {
                    PauseProgram();
                }

                // Define success
                if (result.AnalysisStatus == AnalysisStatus.Positive ||
                    result.AnalysisStatus == AnalysisStatus.Negative || 
                    result.AnalysisStatus == AnalysisStatus.NotSufficientPoints || 
                    result.AnalysisStatus == AnalysisStatus.Rejected)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception e)
            {
                string uimfFile = options.InputPath; // get the UIMF file
                string ionizationMethod = options.IonizationMethod.ToUpper(); 
                string datasetName = Path.GetFileNameWithoutExtension(uimfFile);
                string resultName = datasetName + "_" + ionizationMethod + "_Result.txt";
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

        private static void PauseProgram() 
        {
            // hault the process from exiting to give the user more time in case of user double clicking this 
            // file from within Windows Explorer (or starting the program via a shortcut).
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();            
        }
    }
}
