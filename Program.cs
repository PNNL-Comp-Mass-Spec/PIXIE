#define DEBUG
#define RELEASE

using System;
using System.Collections.Generic;
using FileProcessor;

/**
 * IMSMzFIner is a utility used to extract
 * 
 */
namespace IMSMzFinder
{
	class Program
	{
		private const string PROGRAM_DATE = "September 26, 2014";

		private static string mDatasetAndMzFilePath;
		private static string mInputFilePathSpec;
		private static string mOutputFolderPath;
		private static string mResultsFileName;

		private static float mMZTolerancePPM;

		private static bool mCreateLogFile;
		private static string mLogFilePath;

		private static bool mRecurse;

		static int Main(string[] args)
		{
			var objParseCommandLine = new FileProcessor.clsParseCommandLine();

			mDatasetAndMzFilePath = string.Empty;
			mInputFilePathSpec = string.Empty;
			mOutputFolderPath = string.Empty;
			mResultsFileName = string.Empty;

			mMZTolerancePPM = IMSMzFinder.DEFAULT_MZ_TOLERANCE_PPM;

			mCreateLogFile = false;
			mLogFilePath = string.Empty;

			mRecurse = false;

			try
			{
				bool success = false;

				if (objParseCommandLine.ParseCommandLine())
				{
					if (SetOptionsUsingCommandLineParameters(objParseCommandLine))
						success = true;
				}

				if (!success ||
					objParseCommandLine.NeedToShowHelp ||
					objParseCommandLine.ParameterCount + objParseCommandLine.NonSwitchParameterCount == 0 ||
					string.IsNullOrWhiteSpace(mDatasetAndMzFilePath) ||
					string.IsNullOrWhiteSpace(mInputFilePathSpec))
				{
					ShowProgramHelp();
					return -1;

				}

				var mzFinder  = new IMSMzFinder()
				{
					ShowMessages = true,
					LogMessagesToFile = mCreateLogFile,
					LogFilePath = mLogFilePath,					
					DatasetAndMzFilePath = mDatasetAndMzFilePath,
					ResultsFileName = mResultsFileName,
					MZTolerancePPM = mMZTolerancePPM
				};

				if (mRecurse)
					success = mzFinder.ProcessFilesAndRecurseFolders(mInputFilePathSpec, mOutputFolderPath);
				else
					success = mzFinder.ProcessFilesWildcard(mInputFilePathSpec, mOutputFolderPath);

				if (!success)
					return -2;

			}
			catch (Exception ex)
			{
				Console.WriteLine("Error occurred in Program->Main: " + Environment.NewLine + ex.Message);
				Console.WriteLine(ex.StackTrace);
				return -1;
			}

			return 0;
		}

		private static string GetAppVersion()
		{
			return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (" + PROGRAM_DATE + ")";
		}

		private static bool SetOptionsUsingCommandLineParameters(FileProcessor.clsParseCommandLine objParseCommandLine)
		{
			// Returns True if no problems; otherwise, returns false
			var lstValidParameters = new List<string> { "I", "O", "PPMTol", "R", "S", "L" };

			try
			{
				// Make sure no invalid parameters are present
				if (objParseCommandLine.InvalidParametersPresent(lstValidParameters))
				{
					var badArguments = new List<string>();
					foreach (string item in objParseCommandLine.InvalidParameters(lstValidParameters))
					{
						badArguments.Add("/" + item);
					}

					ShowErrorMessage("Invalid commmand line parameters", badArguments);

					return false;
				}

				// Query objParseCommandLine to see if various parameters are present						

				if (objParseCommandLine.NonSwitchParameterCount > 0)
					mDatasetAndMzFilePath = objParseCommandLine.RetrieveNonSwitchParameter(0);

				if (objParseCommandLine.NonSwitchParameterCount > 1)
					mInputFilePathSpec = objParseCommandLine.RetrieveNonSwitchParameter(1);

				if (!ParseParameter(objParseCommandLine, "I", "a file path", ref mInputFilePathSpec)) return false;
				if (!ParseParameter(objParseCommandLine, "O", "a folder path", ref mOutputFolderPath)) return false;
				if (!ParseParameter(objParseCommandLine, "R", "a file path", ref mResultsFileName)) return false;

				if (objParseCommandLine.IsParameterPresent("PPMTol"))
				{
					string mzTolPPM = string.Empty;

					if (!ParseParameter(objParseCommandLine, "PPMTol", "a ppm-based tolerance", ref mzTolPPM)) return false;
					if (!float.TryParse(mzTolPPM, out mMZTolerancePPM))
					{
						ShowErrorMessage("Value for PPMTol is not numeric: " + mzTolPPM);
						return false;
					}
				}

				if (objParseCommandLine.IsParameterPresent("S"))
					mRecurse = true;

				string filePath;
				if (objParseCommandLine.RetrieveValueForParameter("L", out filePath))
				{
					mCreateLogFile = true;
					if (!string.IsNullOrWhiteSpace(mLogFilePath))
					{
						mLogFilePath = filePath;
					}					
				}

				return true;
			}
			catch (Exception ex)
			{
				ShowErrorMessage("Error parsing the command line parameters: " + Environment.NewLine + ex.Message);
			}

			return false;
		}

		private static bool ParseParameter(clsParseCommandLine objParseCommandLine, string parameterName, string description, ref string targetVariable)
		{
			string value;
			if (objParseCommandLine.RetrieveValueForParameter(parameterName, out value))
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					ShowErrorMessage("/" + parameterName + " does not have " + description);
					return false;
				}
				targetVariable = string.Copy(value);
			}
			return true;
		}

		private static void ShowErrorMessage(string strMessage)
		{
			const string strSeparator = "------------------------------------------------------------------------------";

			Console.WriteLine();
			Console.WriteLine(strSeparator);
			Console.WriteLine(strMessage);
			Console.WriteLine(strSeparator);
			Console.WriteLine();

			WriteToErrorStream(strMessage);
		}

		private static void ShowErrorMessage(string strTitle, IEnumerable<string> items)
		{
			const string strSeparator = "------------------------------------------------------------------------------";

			Console.WriteLine();
			Console.WriteLine(strSeparator);
			Console.WriteLine(strTitle);
			string strMessage = strTitle + ":";

			foreach (string item in items)
			{
				Console.WriteLine("   " + item);
				strMessage += " " + item;
			}
			Console.WriteLine(strSeparator);
			Console.WriteLine();

			WriteToErrorStream(strMessage);
		}


		private static void ShowProgramHelp()
		{
			string exeName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);

			try
			{
				Console.WriteLine();
				Console.WriteLine("This program will search for m/z values in a series of DeconTools _isos.csv files, " +
				                  "reporting the intensity and drift time of the best match for each m/z (within a given target frame range). ");								  
				Console.WriteLine();

				Console.WriteLine("Program syntax:" + Environment.NewLine + exeName);
				Console.WriteLine(" DatasetAndMzFile.txt DatasetFileSpec [/PPMTol:MZTolerancePPM] ");
				Console.WriteLine(" [/O:OutputFolderPath] [/R:ResultsFileName] [/S] [/L]");
			
				Console.WriteLine();
				Console.WriteLine("DatasetAndMzFile specifies a tab-delimited text file with columns:");
				Console.WriteLine("DatasetName, Target_MZ, FrameNum, FrameTolerance, Description");
				Console.WriteLine();
				Console.WriteLine("DatasetFileSpec specifies the file or files to analyze, for example");
				Console.WriteLine("Dataset_isos.csv for one dataset or *_isos.csv for all datasets in a folder");
				Console.WriteLine();
				Console.WriteLine("Use /O to define the results file path.  If not defined, the results");
				Console.WriteLine("file will be created in the folder of the first dataset processed");
				Console.WriteLine();
				Console.WriteLine("Use /S recursively search for files to parse, based on the DatasetFileSpec");
				Console.WriteLine();
				Console.WriteLine("Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2014");
				Console.WriteLine("Version: " + GetAppVersion());
				Console.WriteLine();

				Console.WriteLine("E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com");
				Console.WriteLine("Website: http://panomics.pnnl.gov/ or http://omics.pnl.gov or http://www.sysbio.org/resources/staff/");
				Console.WriteLine();

				// hault the process from exiting to give the user more time in case of user double clicking this 
                // file from within Windows Explorer (or starting the program via a shortcut).
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();

			}
			catch (Exception ex)
			{
				Console.WriteLine("Error displaying the program syntax: " + ex.Message);
			}

		}

		private static void WriteToErrorStream(string strErrorMessage)
		{
			try
			{
				using (var swErrorStream = new System.IO.StreamWriter(Console.OpenStandardError()))
				{
					swErrorStream.WriteLine(strErrorMessage);
				}
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch
			{
				// Ignore errors here
			}
		}

		#region "Event Handlers"
	

		#endregion
	}
}
