using System;

namespace ImsMetabolitesFinderBatchProcessor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using FalkorSDK.SignalPlotter;
    using FalkorSDK.SignalPlotter.Data;
    using FalkorSDK.SignalPlotter.PlottableDataModel;

    using FalkorSignalPlotter;
    using FalkorSignalPlotter.Util;

    using ImsInformed.Domain;

    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
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
                    throw new FileNotFoundException();
                }
                
                int numberOfProcesses = options.NumberOfProcesses;
                bool reanalyze = options.Reanalyze;
                int numberOfAnalysesPerPlot = options.NumberOfAnalysesPerPlot;

                // Process the search spec file
                try 
                {
                    SearchSpecProcessor processor = new SearchSpecProcessor(exe, searchSpecPath, options.InputPath, options.ShowWindow);
                    // Run the program in a single process.
                    int numberOfCommands = processor.TaskList.Count;
                    int count = 0;
                    int index = 0;
                    HashSet<ImsInformedProcess> runningTasks = new HashSet<ImsInformedProcess>();

                    List<ImsInformedProcess> failedAnalyses = new List<ImsInformedProcess>();

                    while (count < numberOfCommands)
                    {
                        if (runningTasks.Count < numberOfProcesses)
                        {
                            // Find the next non-conflicting task that is not done yet and resource-free and is not running.
                            bool done = processor.TaskList[index].Done;
                            bool running = runningTasks.Contains(processor.TaskList[index]);
                            bool resourceFree = processor.TaskList[index].AreResourcesFree(runningTasks);

                            while (done || running || !resourceFree)
                            {
                                index = (index + 1 == numberOfCommands) ? 0 : index + 1;
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
                            }
                            count++;
                        }

                        else if (runningTasks.Count == numberOfProcesses)
                        {
                            // Wait until the at least one task is finsihed.
                            bool found = false;
                            while (!found)
                            {
                                // Wait for half a second
                                Thread.Sleep(100);
                                foreach (var runningTask in runningTasks)
                                {
                                    if (runningTask.Done || runningTask.HasExited)
                                    {
                                        found = true;
                                        
                                        if (runningTask.Done)
                                        {
                                            Console.WriteLine("Analysis was completed before and Reanalyze setting is on, skipping analysis for (ID =" + runningTask.JobID + ") " + runningTask.DataSetName);
                                        }
                                        else if (runningTask.ExitCode == 0)
                                        {
                                            Console.WriteLine("Analysis completed successfully for Dataset(ID =" + runningTask.JobID + ") " + runningTask.DataSetName);
                                            runningTask.Done = true;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Analysis failed for (ID =" + runningTask.JobID + ") " + runningTask.DataSetName + ". Check the error file for details.");
                                            failedAnalyses.Add(runningTask);
                                            runningTask.Done = true;
                                        }
                                        
                                        Console.WriteLine(" ");
                                        runningTasks.Remove(runningTask);
                                        break;
                                    }
                                }
                            }
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
                            Console.WriteLine("Analysis was completed before and Reanalyze setting is on, skipping analysis for (ID =" + item.JobID + ") " + item.DataSetName);
                        }
                        
                        Console.WriteLine(" ");
                    }

                    // Collect result from result files
                    Console.WriteLine("Aggregating Analyses Results...");
                    
                    IEnumerable<ImsInformedProcess> sortedTasks = processor.TaskList;
                    
                    ResultAggregator resultAggregator = new ResultAggregator(sortedTasks);
                    resultAggregator.ProcessResultFiles(options.InputPath);
                    Console.WriteLine("Aggregating Analyses Done");
                    Console.WriteLine();

                    // Print analysis result to console and summary file.
                    string summaryFilePath = Path.Combine(options.InputPath, "analysis_summary.txt");
                    
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
                        Trace.WriteLine("");
                        Trace.WriteLine("<Dataset Name> <M+H> <M-H> <M+Na>");
                        

                        // Write the summary file.
                        foreach (var item in resultAggregator.ResultCollection)
                        {
                            // Result for M+H
                            string protonPlusSummary = ResultAggregator.SummarizeResult(item.Value, IonizationMethod.ProtonPlus);

                            // Result for M-H
                            string protonMinusSummary = ResultAggregator.SummarizeResult(item.Value, IonizationMethod.ProtonMinus);

                            // Result for M+Na
                            string sodiumPlusSummary = ResultAggregator.SummarizeResult(item.Value, IonizationMethod.SodiumPlus);

                            Trace.WriteLine(String.Format("{0}, {1}, {2}, {3}", item.Key, protonPlusSummary, protonMinusSummary, sodiumPlusSummary));
                        }

                        Trace.WriteLine("");

                        Console.WriteLine();
                        Trace.WriteLine("Analysis summary:");
                        Trace.WriteLine("");
                        Trace.WriteLine(String.Format("{0} out of {1} analysis jobs succeeded. Results and QA data were written to where input UIMF files are.", count - failedAnalyses.Count,     count));
                        Trace.WriteLine("");
                        if (failedAnalyses.Count > 0)
                        {
                            Trace.WriteLine(String.Format("The following {0} analyses failed, please check result file for details: ", failedAnalyses.Count));
                            foreach (ImsInformedProcess dataset in failedAnalyses)
                            {
                                Trace.WriteLine(String.Format("Line {0} : {1} [ID = {2}]", dataset.LineNumber, dataset.DataSetName, dataset.JobID));
                            }
                            Console.WriteLine();
                        }
                    }

                    Console.WriteLine("Exporting results to viper input...");
                    ViperExporter.ExportViperInputFile(resultAggregator, options.InputPath);
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

                    // TODO implement sorting analysis conclusion on a chemical by chemical basis.
                    var orderMap = new Dictionary<AnalysisStatus, int>() {
                        { AnalysisStatus.POS, 0 },
                        { AnalysisStatus.REJ, 1 },
                        { AnalysisStatus.NSP, 2 },
                        { AnalysisStatus.NEG, 3 },
                        { AnalysisStatus.TAR, 4 },
                        { AnalysisStatus.ERR, 5 },
                        { AnalysisStatus.PeakToLeft, 6 },
                        { AnalysisStatus.MassError, 7 },
                        { AnalysisStatus.XicNotFound, 8 },
                    };

                    foreach (var item in resultAggregator.ResultCollection)
                    {
                        plotItemCount += AddResultToScoresTable(item.Key, item.Value, IonizationMethod.ProtonPlus, table, colDef);
                        plotItemCount += AddResultToScoresTable(item.Key, item.Value, IonizationMethod.ProtonMinus, table, colDef);
                        plotItemCount += AddResultToScoresTable(item.Key, item.Value, IonizationMethod.SodiumPlus, table, colDef);

                        if (plotItemCount >= numberOfAnalysesPerPlot)
                        {
                            plotItemCount = 0; // reset the plot item count
                            plotPageCount++;

                            var model = table.ToPlotModel(new PlotOptions() 
                            {
                                Title = "Analyses Scores Summary Plot",
                                Subtitle = "Page " + plotPageCount
                            });
                            
                            string plotFilePath = Path.Combine(options.InputPath, "ScoresPlotPage" + plotPageCount + ".png");
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

                        string plotFilePath = Path.Combine(options.InputPath, "ScoresPlotPage" + (plotPageCount + 1) + ".png");
                        double lastHeightDouble = (plotItemCount > (numberOfAnalysesPerPlot / 2)) ? (height / numberOfAnalysesPerPlot * plotItemCount) : height / 2;
                        int lastHeight = (int)Math.Round(lastHeightDouble);
                        PlotterUtil.ExportPlotModel(plotFilePath, remainder, DPI, width, lastHeight);
                    }
                    
                    Console.WriteLine("Done.");
                    Console.WriteLine();
                }
                catch (AggregateException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                    foreach (Exception exception in e.InnerExceptions)
                    {
                        if (exception.Data.Contains("lineNumber"))
                        {
                            Console.Write("Line {0}: ", exception.Data["lineNumber"]);
                        }
                        Console.WriteLine(exception.Message);
                        Console.WriteLine("");
                    }
                }
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();            
        }

        public static int AddResultToScoresTable(string dataset, IDictionary<IonizationMethod, MoleculeInformedWorkflowResult> chemicalResult, IonizationMethod ionization, NumericTable table, IList<string> colDef)
        {
            if (chemicalResult.ContainsKey(ionization))
            {
                TableRow dict = new TableRow(dataset + ionization);
                MoleculeInformedWorkflowResult workflowResult = chemicalResult[ionization];
                dict.Name += "(" + workflowResult.AnalysisStatus + ")";
                dict.Add(colDef[1], workflowResult.AnalysisScoresHolder.AverageCandidateTargetScores.IntensityScore);
                dict.Add(colDef[2], workflowResult.AnalysisScoresHolder.AverageCandidateTargetScores.IsotopicScore);
                dict.Add(colDef[3], workflowResult.AnalysisScoresHolder.AverageCandidateTargetScores.PeakShapeScore);
                dict.Add(colDef[0], workflowResult.AnalysisScoresHolder.AverageVoltageGroupStabilityScore);
                dict.Add(colDef[4], workflowResult.AnalysisScoresHolder.RSquared);
                table.Add(dict);
                return 1;
            }
            return 0;
        }
    }
}
