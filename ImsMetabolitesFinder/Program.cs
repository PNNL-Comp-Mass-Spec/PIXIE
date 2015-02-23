#define DEBUG

using System;

namespace IMSMetabolitesFinder
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using ImsInformed.Domain;
    using ImsInformed.Parameters;
    using ImsInformed.Util;

    using ImsMetabolitesFinder;
    using ImsMetabolitesFinder.Preprocess;

    public class Program
	{
        [STAThread]
		public static int Main(string[] args)
		{
            try
            {
                var options = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, options))
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
                    string formula = "";

                    // get the target
                    bool isDouble = Double.TryParse(options.Target, out Mz);
                    if (!isDouble)
                    {
                        formula = options.Target;
                    }

                    bool pause = options.PalseWhenDone;

                    int ID = options.ID;
                    
                    // get the ionization method.
                    IonizationMethod method;
                    if (ionizationMethod == "M+H")
                    {
                        method = IonizationMethod.ProtonPlus;
                    }
                    else if (ionizationMethod == "M-H")
                    {
                        method = IonizationMethod.ProtonMinus;
                    }
                    else if (ionizationMethod == "M+NA")
                    {
                        method = IonizationMethod.SodiumPlus;
                    }
                    else 
                    {
                        throw new ArgumentException("Ionization" + ionizationMethod + "method is not supported", "IonizationMethod");
                    }

                    MoleculeWorkflowParameters searchParameters = new MoleculeWorkflowParameters 
                    {
                        MassToleranceInPpm = options.PpmError,
                        NumPointForSmoothing = 9,
                        FeatureFilterLevel = 0.25,
                        ScanWindowWidth = 4,
                        IsotopicFitScoreThreshold = options.IsotopicScoreThreshold,
                        IntensityThreshold = options.IntensityThreshold,
                        PeakShapeThreshold = options.PeakShapeScoreThreshold,
                        MinFitPoints = options.MinFitPoints
                    };

                    IFormatter formatter = new BinaryFormatter();

                    // If target cannot be constructed. Create a result 
                    ImsTarget target = null;
                    try
                    {
                        if (!isDouble)
                        {
                            ImsTarget sample = new ImsTarget(ID, method, formula);
                            target= new ImsTarget(ID, method, formula);
                        } 
                        else 
                        {
                            target= new ImsTarget(ID, method, Mz);
                        }
                    }
                    catch (Exception)
                    {
                        MoleculeInformedWorkflowResult targetErrorResult;
                        targetErrorResult.DatasetName = datasetName;
                        targetErrorResult.TargetDescriptor = null;
                        targetErrorResult.IonizationMethod = method;
                        targetErrorResult.AnalysisStatus = AnalysisStatus.TAR;
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
                    MoleculeInformedWorkflow workflow = new MoleculeInformedWorkflow(uimfFile, outputDirectory , resultName, searchParameters);
                    MoleculeInformedWorkflowResult result = workflow.RunMoleculeInformedWorkFlow(target);

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
                    if (result.AnalysisStatus == AnalysisStatus.POS || result.AnalysisStatus == AnalysisStatus.NEG || result.AnalysisStatus == AnalysisStatus.NSP || result.AnalysisStatus == AnalysisStatus.REJ)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                    
                }
                else
                {
                    return 1;
                }
                
            }
            catch (Exception e)
            {
                var options = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, options))
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

                    if (options.PalseWhenDone)
                    {
                        PauseProgram();
                    }
                    return 1;
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
