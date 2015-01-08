#define DEBUG
#define RELEASE

using System;

namespace IMSMetabolitesFinder
{
    using System.IO;

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
                    if (!Double.TryParse(options.Target, out Mz))
                    {
                        Mz = 0;
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
                        IsotopicFitScoreMax = 0.15,
                        MassToleranceInPpm = options.PpmError,
                        NumPointForSmoothing = 9,
                        ConfidenceThreshold = 0.5,
                        FeatureFilterLevel = 0.25,
                        FeatureScoreThreshold = 2
                    };

                    ImsTarget target = null;
                    if (Mz == 0)
                    {
                        ImsTarget sample = new ImsTarget(ID, method, formula);
                        target= new ImsTarget(ID, method, formula);
                    } 
                    else 
                    {
                        target= new ImsTarget(ID, method, Mz);
                    }

                    // Preprocessing
                    Console.WriteLine("Start Preprocessing:");
                    BincCentricIndexing.IndexUimfFile(uimfFile);

                    // Run algorithms in IMSInformed
                    MoleculeInformedWorkflow workflow = new MoleculeInformedWorkflow(uimfFile, outputDirectory , resultName, searchParameters);
                    workflow.RunMoleculeInformedWorkFlow(target);
                    
                    if (pause)
                    {
                        PauseProgram();
                    }
                    return 0;
                }
                else
                {
                    return 2;
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
