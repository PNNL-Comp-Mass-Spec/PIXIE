using System;
using System.Collections.Generic;
using System.Linq;

namespace ImsMetabolitesFinderBatchProcessor
{
    using System.Data;
    using System.IO;

    public class SearchSpecProcessor
    {
        public List<ImsInformedProcess> TaskList { get; private set; }
        public string Arguments { get; private set; }
        private string inputPath;
        private bool showWindow;

        public SearchSpecProcessor(string utilityPath, string searchSpecFilePath, string inputPath, bool showWindow)
        {
            this.showWindow = showWindow;
            this.inputPath = inputPath;

            Console.WriteLine("Processing search spec file at" + searchSpecFilePath);
            Console.WriteLine("");

            // Read the search spec
            if (!File.Exists(searchSpecFilePath))
            {
                throw new FileNotFoundException();
            }

            TaskList = new List<ImsInformedProcess>();

            var exceptions = new List<Exception>();
            using (var reader = new StreamReader(searchSpecFilePath))
            {
                int lineNumber = 0;
                int ID = 0;
                string line;
                bool firstLine = true;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;

                    Console.WriteLine("Number of lines processed: {0}", lineNumber);
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    
                    ImsInformedProcess process = ProcessJob(utilityPath, line, lineNumber, exceptions, ref firstLine, ref ID);
                    if (process != null)
                    {
                        
                        this.TaskList.Add(process);
                    }
                }
            }
            
            if (exceptions.Count > 0)
            {
                throw new AggregateException("Failed to process search spec file, abort batch processor", exceptions);
            }
        }

        // search for the uimf file in the given directory.
        private string FindUimfFile(string uimfLocation, string datasetName)
        {
            if (File.Exists(uimfLocation))
            {
                if (uimfLocation.Contains(datasetName))
                {
                    return uimfLocation;
                }
            }
     
            if (!Directory.Exists(inputPath))
            {
                throw new FileNotFoundException();
            }

            //string[] files = Directory.GetFiles(uimfLocation, datasetName + ".uimf", SearchOption.AllDirectories);

            IEnumerable<string> files = Directory.EnumerateFiles(uimfLocation, datasetName + ".uimf", SearchOption.AllDirectories);

            try
            {
                return files.First();
            }
            catch (Exception)
            {
                throw new FileNotFoundException("Dataset " + datasetName + ".uimf was not found in the directory " + uimfLocation + ". Please refine the search spec and try again.");
            }
        }

        // process job and return the commandline, if not whitespace, set firstLine to false
        private ImsInformedProcess ProcessJob(string utility, string line, int lineNumber, List<Exception> exceptions , ref bool firstLine, ref int ID)
        {
            string commandline = null;
            try
            {
                line = line.Trim();

                if (line.Trim().StartsWith("#") || String.IsNullOrWhiteSpace(line))
                {
                    return null;
                }

                if (firstLine)
                {
                    this.Arguments = line;
                    firstLine = false;
                    return null;
                }
                string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    
                if (parts.Count() < 2)
                {
                    exceptions.Add(new FormatException("Line " + lineNumber + " \"" + line + "\" is not an valid ImsMetabolitesFinder batch processing Job"));
                }
                // Find the UIMF files.
                string datasetName = parts[0];
                string uimfPath = this.FindUimfFile(this.inputPath, datasetName);
                string UIMFFileDir = Path.GetDirectoryName(uimfPath);

                string target = parts[1];

                string ionization = "";

                ionization = parts.Count() > 2 ? parts[2] : InferIonization(datasetName);

                string outputDirectory = UIMFFileDir + "\\" + datasetName + "_ImsMetabolitesFinderResult" + "_" + ionization;

                commandline += "-i " + uimfPath + " "; 
                commandline += "-t " + target + " ";
                commandline += "-m " + ionization + " ";
                commandline += "-o " + outputDirectory + " ";
                commandline += "--ID " + ID + " ";
                commandline += this.Arguments + " ";

                string binPath = Path.Combine(outputDirectory, datasetName + "_" + ionization + "_Result.bin");

                ImsInformedProcess process = new ImsInformedProcess(ID, datasetName, utility, commandline, this.showWindow, binPath, lineNumber);
                process.FileResources.Add(uimfPath);
                ID++;
                return process;
            }
            catch (Exception e)
            {
                e.Data.Add("lineNumber", lineNumber);
                exceptions.Add(e);
                return null;
            }
        }

        // Infer ionization method from data set name
        public static string InferIonization(string datasetName)
        {
            // if (datasetName.Contains("pos"))
            // {
            //     return "M+H";
            // }

            if (datasetName.Contains("neg"))
            {
                return "M-H";
            }
            throw new Exception(datasetName + ": No ionization method is given, and it is not possible to infer the ionization method from the dataset name " + datasetName + ".");
        }
    }
}
