using System;

namespace ImsMetabolitesFinderBatchProcessor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

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
                SearchSpecProcessor processor = new SearchSpecProcessor(exe, searchSpecPath, options.InputPath);

                // Run the program in a single process.
                List<Process> processes = new List<Process>();
                if (numberOfProcesses == 1)
                {
                    foreach (string command in processor.CommandList)
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = exe;
                        p.StartInfo.Arguments = command;
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.RedirectStandardOutput = true;
                        //p.StartInfo.FileName = "YOURBATCHFILE.bat";
                        p.Start();
                    }
                } 
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();            
        }
    }
}
