using System;
using System.Collections.Generic;
using System.Linq;

namespace ImsMetabolitesFinderBatchProcessor
{
    using System.Data;
    using System.IO;

    public class SearchSpecProcessor
    {
        public List<string> CommandList { get; private set; }
        public string Arguments { get; private set; }
        
        private string inputPath;

        public SearchSpecProcessor(string utilityPath, string searchSpecFilePath, string inputPath)
        {
            this.inputPath = inputPath;

            Console.WriteLine("Processing search spec file at" + searchSpecFilePath);
            Console.WriteLine("");

            // Read the search spec
            if (!File.Exists(searchSpecFilePath))
            {
                throw new FileNotFoundException();
            }

            CommandList = new List<string>();

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
                    string command = ProcessJob(line, lineNumber, exceptions, ref firstLine, ref ID);
                    if (command != null)
                    {
                        this.CommandList.Add(command);
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

            string[] files = Directory.GetFiles(uimfLocation, datasetName + ".uimf", SearchOption.AllDirectories);
            if (files.Count() > 1)
            {
                throw new DuplicateNameException("Multiple matches of the dataset " + datasetName + " exisit inside the directory " + uimfLocation + ". Please resolve the conclits.");
            }

            if (!files.Any())
            {
                throw new DuplicateNameException("Dataset " + datasetName + ".uimf was not found in the directory " + uimfLocation + ". Please refine resolve the file and try again.");
            }
            return files[0];
        }

        // process job and return the commandline, if not whitespace, set firstLine to false
        private string ProcessJob(string line, int lineNumber, List<Exception> exceptions , ref bool firstLine, ref int ID)
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
                    throw new FormatException("Line \"" + line + "\" is not an valid ImsMetabolitesFinder batch processing Job");
                }
                // Find the UIMF files.
                string datasetName = parts[0];
                string uimfPath = this.FindUimfFile(this.inputPath, datasetName);
                string UIMFFileDir = Path.GetDirectoryName(uimfPath);

                string target = parts[1];

                string ionization = "";

                ionization = parts.Count() > 2 ? parts[2] : InferIonization(datasetName);

                commandline += "-i " + uimfPath + " "; 
                commandline += "-t " + target + " ";
                commandline += "-m " + ionization + " ";
                commandline += "-o " + UIMFFileDir + " ";
                commandline += "--ID " + ID + " ";
                commandline += this.Arguments + " ";
                ID++;
                return commandline;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return null;
            }
        }

        // Infer ionization method from data set name
        public static string InferIonization(string datasetName)
        {
            if (datasetName.Contains("pos2"))
            {
                return "M+Na";
            }
            if (datasetName.Contains("pos"))
            {
                return "M+H";
            }
            if (datasetName.Contains("neg"))
            {
                return "M-H";
            }
            throw new Exception(datasetName + ": No ionization method is given cannot infer the ionization method from the dataset name.");
        }
    }
}
