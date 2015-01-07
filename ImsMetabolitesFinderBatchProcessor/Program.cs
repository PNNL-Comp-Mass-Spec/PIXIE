using System;

namespace ImsMetabolitesFinderBatchProcessor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

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

                // Process the search spec file
                try 
                {
                    SearchSpecProcessor processor = new SearchSpecProcessor(exe, searchSpecPath, options.InputPath, options.ShowWindow);
                    // Run the program in a single process.
                    int numberOfCommands = processor.TaskList.Count;
                    int count = 1;
                    int index = 0;
                    HashSet<ImsInformedProcess> runningTasks = new HashSet<ImsInformedProcess>();

                    List<ImsInformedProcess> failedAnalyses = new List<ImsInformedProcess>();

                    while (count < numberOfCommands)
                    {
                        if (runningTasks.Count < numberOfProcesses)
                        {
                            //Find next non-conflicting task that is not done or running.
                            while (processor.TaskList[index].Done || runningTasks.Contains(processor.TaskList[index]) || !processor.TaskList[index].AreResourcesFree(runningTasks) )
                            {
                                index = (index + 1 == numberOfCommands) ? 0 : index + 1;
                            }

                            runningTasks.Add(processor.TaskList[index]);
                            processor.TaskList[index].Start();
                            Console.WriteLine("Initiating Analysis Job [ID = {0}] out of {1} jobs.", processor.TaskList[index].JobID, numberOfCommands);
                            Console.WriteLine("Dataset Name: " + processor.TaskList[index].DataSetName);
                            Console.WriteLine("Running " + processor.TaskList[index].StartInfo.FileName + " " + processor.TaskList[index].StartInfo.Arguments);
                            Console.WriteLine(" ");
                            count++;
                        }

                        else if (runningTasks.Count == numberOfProcesses)
                        {
                            // Wait until the at least one task is finsihed.
                            bool found = false;
                            while (!found)
                            {
                                // Wait for half a second
                                Thread.Sleep(500);
                                foreach (var runningTask in runningTasks)
                                {
                                    if (runningTask.HasExited)
                                    {
                                        found = true;
                                        runningTask.Done = true;
                                        if (runningTask.ExitCode == 0)
                                        {
                                            Console.WriteLine("Analysis completed successfully for Dataset(ID =" + runningTask.JobID + ") " + runningTask.DataSetName);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Analysis failed for (ID =" + runningTask.JobID + ") " + runningTask.DataSetName + ". Check the error file for details.");
                                            failedAnalyses.Add(runningTask);
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
                        
                        Console.WriteLine(" ");
                    }

                    // Print analysis result to console
                    Console.WriteLine();
                    Console.WriteLine("Analysis report:");
                    Console.WriteLine();
                    Console.WriteLine(" {0} out of {1} analysis jobs succeeded. Results and QA data were written to where input UIMF files are,", count - failedAnalyses.Count, count);
                    Console.WriteLine();
                    if (failedAnalyses.Count > 0)
                    {
                        Console.WriteLine(" The following {0} analyses failed: ", failedAnalyses.Count);
                        foreach (ImsInformedProcess dataset in failedAnalyses)
                        {
                            Console.WriteLine("{0} [ID = {1}]", dataset.DataSetName, dataset.JobID);
                            
                        }
                    }
                }
                catch (AggregateException e)
                {
                    Console.WriteLine(e.Message);
                    foreach (Exception exception in e.InnerExceptions)
                    {
                        Console.WriteLine(exception.Message);
                        Console.WriteLine("");
                    }
                }
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();            
        }
    }
}
