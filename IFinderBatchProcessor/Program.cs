// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace IFinderBatchProcessor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using FalkorSDK.SignalPlotter;
    using FalkorSDK.SignalPlotter.Data;
    using FalkorSDK.SignalPlotter.PlottableDataModel;
    using FalkorSDK.SignalPlotter.Util;

    using IFinderBatchProcessor.Export;
    using IFinderBatchProcessor.SearchSpec;

    using ImsInformed.Domain;
    using ImsInformed.Interfaces;
    using ImsInformed.Workflows.CrossSectionExtraction;

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
        /// <exception cref="FileNotFoundException">
        /// </exception>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Check if ImsMetabolitesFinder.exe is present.
                string exe = "ImsMetabolitesFinder.exe";
                if (!File.Exists(exe))
                {
                    throw new FileNotFoundException("ImsMetabolitesFinder.exe not found in directory " + Directory.GetCurrentDirectory() + "Please double check installation");
                }

                var options = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    string searchSpecPath = options.SearchSpecFile;

                    // Check if searchSpec file exisits.
                    if (!File.Exists(searchSpecPath))
                    {
                        throw new FileNotFoundException("Search spec file: " + searchSpecPath + " not found");
                    }

                     if (!Directory.Exists(options.InputPath))
                     {
                         throw new FileNotFoundException("Input path does not exist", options.InputPath);
                     }

                    if (!string.IsNullOrEmpty(options.OutputPath))
                    {
                        if (!Directory.Exists(options.OutputPath))
                        {
                            Directory.CreateDirectory(options.OutputPath);
                        }
                    }
                    
                    int maxNumberOfProcesses = options.NumberOfProcesses;
                    bool reanalyze = options.Reanalyze;
                    int numberOfAnalysesPerPlot = options.NumberOfAnalysesPerPlot;
                    string workspaceDir = string.IsNullOrEmpty(options.OutputPath) ? options.InputPath : options.OutputPath;

                    // Process the search spec file
                    try 
                    {
                        TextSearchSpecProcessor processor = new TextSearchSpecProcessor(exe, searchSpecPath, options.InputPath, options.OutputPath, options.ShowWindow, options.IgnoreMissingFiles);
                        
                        // Run the program in a single process.
                        int numberOfCommands = processor.TaskList.Count;
                        int count = 0;
                        int index = 0;
                        HashSet<ImsInformedProcess> runningTasks = new HashSet<ImsInformedProcess>();

                        List<ImsInformedProcess> failedAnalyses = new List<ImsInformedProcess>();

                        while (count < numberOfCommands)
                        {
                            if (runningTasks.Count < maxNumberOfProcesses)
                            {
                                // Find the next non-conflicting task that is not done yet and resource-free and is not running.
                                bool done = processor.TaskList[index].Done;
                                bool running = runningTasks.Contains(processor.TaskList[index]);
                                bool resourceFree = processor.TaskList[index].AreResourcesFree(runningTasks);

                                while (done || running || !resourceFree)
                                {
                                    if (index + 1 == numberOfCommands)
                                    {
                                        ImsInformedProcess doneProcess = WaitUntilAtLeastOneTaskDone(runningTasks);
                                        ProcessFinishedTask(doneProcess, failedAnalyses);
                                        index = 0;
                                    }
                                    else
                                    {
                                        index++;
                                    }

                                    done = processor.TaskList[index].Done;
                                    running = runningTasks.Contains(processor.TaskList[index]);
                                    resourceFree = processor.TaskList[index].AreResourcesFree(runningTasks);
                                }

                                runningTasks.Add(processor.TaskList[index]);

                                if (!File.Exists(processor.TaskList[index].ResultBinFile) || reanalyze)
                                {
                                    string outputDir = Path.GetDirectoryName(processor.TaskList[index].ResultBinFile);
                                    if (outputDir != null && Directory.Exists(outputDir))
                                    {
                                        DirectoryInfo di = new DirectoryInfo(outputDir);
                                        FileInfo[] files = di.GetFiles("*.png")
                                                             .Where(p => p.Extension == ".png").ToArray();
                                        foreach (FileInfo file in files)
                                        {
                                                file.Attributes = FileAttributes.Normal;
                                                File.Delete(file.FullName);
                                        }
                                    }

                                    processor.TaskList[index].Start();
                                    Console.WriteLine("Initiating Analysis Job [ID = {0}] out of {1} jobs.", processor.TaskList[index].JobID, numberOfCommands);
                                    Console.WriteLine("Dataset Name: " + processor.TaskList[index].DataSetName);
                                    Console.WriteLine("Running " + processor.TaskList[index].StartInfo.FileName + " " + processor.TaskList[index].StartInfo.Arguments);
                                    Console.WriteLine(" ");
                                }
                                else
                                {
                                    // skip the given analysis job
                                    processor.TaskList[index].Done = true;
                                    ProcessFinishedTask(processor.TaskList[index], failedAnalyses);
                                }

                                count++;
                            }
                            else if (runningTasks.Count == maxNumberOfProcesses)
                            {
                                ImsInformedProcess doneProcess = WaitUntilAtLeastOneTaskDone(runningTasks);
                                ProcessFinishedTask(doneProcess, failedAnalyses);
                            }
                        }

                        // Wait until runningTasks is empty
                        foreach (var item in runningTasks)
                        {
                            if (!item.Done)
                            {
                                item.WaitForExit();
                                item.Done = true;
                                if (item.ExitCode == 0)
                                {
                                    Console.WriteLine("Analysis completed succeessfully for (ID =" + item.JobID + ") " + item.DataSetName);
                                }
                                else
                                {
                                    Console.WriteLine("Analysis failed for (ID =" + item.JobID + ") " + item.DataSetName + ". Check the error file for details.");
                                    failedAnalyses.Add(item);
                                }
                            }
                            else
                            {
                                ProcessFinishedTask(item, failedAnalyses);
                                Console.WriteLine("Analysis was completed before and Reanalyze setting is on, skipping analysis for (ID =" + item.JobID + ") " + item.DataSetName);
                            }
                            
                            Console.WriteLine(" ");
                        }

                        // Collect result from result files
                        Console.WriteLine("Aggregating Analyses Results...");
                        
                        IEnumerable<ImsInformedProcess> sortedTasks = processor.TaskList;
                        
                        ResultAggregator resultAggregator = new ResultAggregator(sortedTasks);
                        resultAggregator.ProcessResultFiles(workspaceDir);
                        Console.WriteLine("Aggregating Analyses Done");
                        Console.WriteLine();

                        // Print analysis result to console and summary file.
                        string summaryFilePath = Path.Combine(workspaceDir, "analysis_summary.txt");
                        
                        // Setup result file.
                        Trace.Listeners.Clear();
                        ConsoleTraceListener consoleTraceListener = new ConsoleTraceListener(false);
                        consoleTraceListener.TraceOutputOptions = TraceOptions.DateTime;

                        using (FileStream resultFile = new FileStream(summaryFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            TextWriterTraceListener resultFileTraceListener = new TextWriterTraceListener(resultFile)
                            {
                                Name = "Result",
                                TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
                            };

                            Trace.Listeners.Add(consoleTraceListener);
                            Trace.Listeners.Add(resultFileTraceListener);
                            Trace.AutoFlush = true;

                            Trace.WriteLine("Results summary:");
                            Trace.WriteLine(string.Empty);
                            Trace.Write("[Dataset Name]");
                            foreach (IonizationAdduct item in resultAggregator.DetectedIonizationMethods)
                            {
                                Trace.Write(" " + item);
                            }

                            Trace.WriteLine("");

                            // Write the summary file.
                            int identifiedChemicalCounter = 0;
                            int totalChemicalCounter = 0;

                            foreach (var chem in resultAggregator.ChemicalDatasetsMap)
                            {
                                string chemName = chem.Key;
                                bool found = false;

                                Trace.Write(string.Format("    {0}: ", chemName));

                                foreach (var ionization in resultAggregator.DetectedIonizationMethods)
                                {
                                    string summary;
                                    var ionizationMethod = ionization;
                                    if (resultAggregator.ChemicalBasedResultCollection[chemName].ContainsKey(ionizationMethod))
                                    {
                                        ChemicalBasedAnalysisResult result = resultAggregator.ChemicalBasedResultCollection[chemName][ionizationMethod];
                                        summary = result.AnalysisStatus.ToConclusionCode();
                                        if (result.AnalysisStatus == AnalysisStatus.Positive)
                                        {
                                            found = true;
                                        }
                                    }
                                    else
                                    {
                                        summary = AnalysisStatus.NoAnalysis.ToConclusionCode();
                                    }

                                    Trace.Write(string.Format(" " + summary));
                                }

                                Trace.WriteLine("");

                                if (found)
                                {
                                    identifiedChemicalCounter++;
                                }

                                totalChemicalCounter++;
                            }

                            Trace.WriteLine(string.Empty);

                            Console.WriteLine();
                            Trace.WriteLine("Analysis summary:");
                            Trace.WriteLine(processor.Message);
                            Trace.WriteLine(string.Format("{0} out of {1} datasets were analyzed finished without errors.", count - failedAnalyses.Count,     count));
                            Trace.WriteLine(string.Empty);
                            Trace.WriteLine(string.Format("{0} out of {1} chemicals have at least 1 ionization mode concluding positive.", identifiedChemicalCounter, totalChemicalCounter));
                            Trace.WriteLine(string.Empty);
                            Trace.WriteLine("Results and QA data were written where the input UIMF files are.");
                            Trace.WriteLine(string.Format("   Analyses concluded positive            (POS) : {0}", resultAggregator.ResultCounter[AnalysisStatus.Positive]));
                            Trace.WriteLine(string.Format("   Analyses concluded Negative            (NEG) : {0}", resultAggregator.ResultCounter[AnalysisStatus.Negative]));
                            Trace.WriteLine(string.Format("   Analyses concluded Rejected            (REJ) : {0}", resultAggregator.ResultCounter[AnalysisStatus.Rejected]));
                            Trace.WriteLine(string.Format("   Analyses concluded Insufficent Points  (NSP) : {0}", resultAggregator.ResultCounter[AnalysisStatus.NotSufficientPoints]));
                            Trace.WriteLine(string.Format("   Analyses concluded Analysis Error      (ERR) : {0}", resultAggregator.ResultCounter[AnalysisStatus.UknownError]));
                            Trace.WriteLine(string.Format("   Analyses concluded Target Error        (TAR) : {0}", resultAggregator.ResultCounter[AnalysisStatus.TargetError]));
                            Trace.WriteLine(string.Empty);
                            if (failedAnalyses.Count > 0)
                            {
                                Trace.WriteLine(string.Format("The following {0} analyses failed, please check result file for details: ", failedAnalyses.Count));
                                foreach (ImsInformedProcess dataset in failedAnalyses)
                                {
                                    Trace.WriteLine(string.Format("Line {0} : {1}[ID = {2}]", dataset.LineNumber, dataset.DataSetName, dataset.JobID));
                                }
                                Console.WriteLine();
                            }
                        }

                        Console.WriteLine("Exporting results to viper input...");
                        ViperExporter.ExportViperChemicalBased(resultAggregator, workspaceDir);
                        ViperExporter.ExportViperDatasetBased(resultAggregator, workspaceDir);
                        Console.WriteLine("Done.");
                        Console.WriteLine();

                        CsLibraryExporter.ExportCrossSectionChemicalBased(resultAggregator, workspaceDir);
                        Console.WriteLine("Exporting results to cross section library...");
                        Console.WriteLine("Done.");
                        Console.WriteLine();

                        // Plot the stack bar diagram
                        Console.WriteLine("Plotting the stack bar diagram for scores visualization...");
                        IList<string> colDef = new List<string>()
                        {
                            "Voltage Group Stability Score",
                            "Average Feature Intensity Score",
                            "Average Feature Isotopic Score",
                            "Average Feature Peak Shape Score",
                            "Analysis Score"
                        };

                        NumericTable table = new NumericTable(colDef);
                        
                        int plotItemCount = 0;
                        int plotPageCount = 0;

                        int DPI = 96;
                        int width = 1500;
                        int height = 700;

                        foreach (string dataset in resultAggregator.DatasetBasedResultCollection.Keys)
                        {
                            foreach (KeyValuePair<IImsTarget, CrossSectionWorkflowResult> item in resultAggregator.DatasetBasedResultCollection[dataset])
                            {
                                plotItemCount += AddResultToScoresTable(dataset, item.Value, item.Key, table, colDef);
                            }

                            if (plotItemCount >= numberOfAnalysesPerPlot)
                            {
                                plotItemCount = 0; // reset the plot item count
                                plotPageCount++;

                                var model = table.ToPlotModel(new PlotOptions() 
                                {
                                    Title = "Analyses Scores Summary Plot",
                                    Subtitle = "Page " + plotPageCount
                                });
                                
                                string plotFilePath = Path.Combine(workspaceDir, "ScoresPlotPage" + plotPageCount + ".png");
                                PlotterUtil.ExportPlotModel(plotFilePath, model, DPI, width, height);

                                // Renew the table
                                table = new NumericTable(colDef);
                            }
                        }

                        // Plot the last few items
                        if (plotItemCount > 0)
                        {
                            var remainder = table.ToPlotModel(new PlotOptions() 
                            {
                                Title = "Analyses Scores Summary Plot",
                                        Subtitle = "Page " + plotPageCount + 1
                            });

                            string plotFilePath = Path.Combine(workspaceDir, "ScoresPlotPage" + (plotPageCount + 1) + ".png");
                            double lastHeightDouble = (plotItemCount > (numberOfAnalysesPerPlot / 2)) ? (height / numberOfAnalysesPerPlot * plotItemCount) : height / 2;
                            int lastHeight = (int)Math.Round(lastHeightDouble);
                            PlotterUtil.ExportPlotModel(plotFilePath, remainder, DPI, width, lastHeight);
                        }
                        
                        Console.WriteLine("Done.");
                        Console.WriteLine();
                    }
                    catch (AggregateException e)
                    {
                        foreach (Exception exception in e.InnerExceptions)
                        {
                            if (exception.Data.Contains("lineNumber"))
                            {
                                Console.Write("Line {0}: ", exception.Data["lineNumber"]);
                            }

                            Console.WriteLine(exception.Message);
                            Console.WriteLine(string.Empty);
                        }

                        Console.WriteLine(e.Message);
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(String.Empty);
            }
            

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();            
        }

        /// <summary>
        /// The add result to scores table.
        /// </summary>
        /// <param name="dataset">
        /// The dataset.
        /// </param>
        /// <param name="chemicalResult">
        /// The chemical result.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="table">
        /// The table.
        /// </param>
        /// <param name="colDef">
        /// The col def.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int AddResultToScoresTable(string dataset, CrossSectionWorkflowResult chemicalResult, IImsTarget target, NumericTable table, IList<string> colDef)
        {
            TableRow dict = new TableRow(dataset + target);
            dict.Name += "(" + chemicalResult.AnalysisStatus + ")";
            dict.Add(colDef[1], chemicalResult.AverageObservedPeakStatistics.IntensityScore);
            dict.Add(colDef[2], chemicalResult.AverageObservedPeakStatistics.IsotopicScore);
            dict.Add(colDef[3], chemicalResult.AverageObservedPeakStatistics.PeakShapeScore);
            dict.Add(colDef[0], chemicalResult.AverageVoltageGroupStability);
            dict.Add(colDef[4], chemicalResult.AssociationHypothesisInfo.ProbabilityOfHypothesisGivenData);
            table.Add(dict);
            return 1;
        }

        /// <summary>
        /// The wait until at least one task done.
        /// </summary>
        /// <param name="runningTasks">
        /// The running tasks.
        /// </param>
        /// <returns>
        /// The <see cref="ImsInformedProcess"/>.
        /// </returns>
        private static ImsInformedProcess WaitUntilAtLeastOneTaskDone(HashSet<ImsInformedProcess> runningTasks)
        {
            // Wait until the at least one task is finsihed.
            while (true)
            {
                // Wait for half a second
                Thread.Sleep(25);
                foreach (var runningTask in runningTasks)
                {
                    if (runningTask.Done || runningTask.HasExited)
                    {
                        runningTasks.Remove(runningTask);
                        return runningTask;
                    }
                }
            }
        }

        private static void ProcessFinishedTask(ImsInformedProcess doneProcess, IList<ImsInformedProcess> failedAnalyses)
        {
            if (doneProcess.Done)
            {
                Console.WriteLine("Analysis was completed before and Reanalyze([-r]) was turned off, skipping analysis for (ID =" + doneProcess.JobID + ") " +doneProcess.DataSetName);
            }
            else if (doneProcess.ExitCode == 0)
            {
                Console.WriteLine("Analysis completed successfully for Dataset(ID =" + doneProcess.JobID + ") " + doneProcess.DataSetName);
                doneProcess.Done = true;
            }
            else
            {
                Console.WriteLine("Analysis failed for (ID =" + doneProcess.JobID + ") " + doneProcess.DataSetName + ". Check the error file for details.");
                failedAnalyses.Add(doneProcess);
                doneProcess.Done = true;
            }
            
            Console.WriteLine(" ");
        }
    }
}
