// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchSpecProcessor.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the SearchSpecProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// The search spec processor.
    /// </summary>
    public class SearchSpecProcessor
    {
        /// <summary>
        /// The show window.
        /// </summary>
        private readonly bool showWindow;

        /// <summary>
        /// The input path.
        /// </summary>
        private readonly string inputPath;

        /// <summary>
        /// The output path.
        /// </summary>
        private readonly string outputPath;

        /// <summary>
        /// Gets the task list.
        /// </summary>
        public List<ImsInformedProcess> TaskList { get; private set; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public string Arguments { get; private set; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchSpecProcessor"/> class.
        /// </summary>
        /// <param name="utilityPath">
        /// The utility path.
        /// </param>
        /// <param name="searchSpecFilePath">
        /// The search spec file path.
        /// </param>
        /// <param name="inputPath">
        /// The input path.
        /// </param>
        /// <param name="showWindow">
        /// The show window.
        /// </param>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        /// <exception cref="AggregateException">
        /// </exception>
        public SearchSpecProcessor(string utilityPath, string searchSpecFilePath, string inputPath, string outputPath, bool showWindow, bool force)
        {
            this.showWindow = showWindow;
            this.outputPath = outputPath;
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
            int ID = 0;
            using (var reader = new StreamReader(searchSpecFilePath))
            {
                int lineNumber = 0;
                string line;
                bool firstLine = true;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;

                    Console.WriteLine("Number of lines processed: {0}", lineNumber);
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    
                    ImsInformedProcess process = this.ProcessJob(utilityPath, line, lineNumber, exceptions, ref firstLine, ref ID);
                    if (process != null)
                    {
                        
                        this.TaskList.Add(process);
                    }
                }
            }
            
            if (exceptions.Count > 0 && !force)
            {
                throw new AggregateException("Failed to process search spec file, abort batch processor", exceptions);
            }
            else if (exceptions.Count > 0 && force)
            {
                this.Message = String.Format("{0} datasets were not found out of {1} datasets\r\n", exceptions.Count, exceptions.Count + ID);
            }
        }

        /// <summary>
        /// search for the uimf file in the given directory.
        /// </summary>
        /// <param name="fileOrFolderPath">
        /// The file or folder path.
        /// </param>
        /// <param name="datasetName">
        /// The dataset name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        private string FindUimfFile(string fileOrFolderPath, string datasetName)
        {
            string uimfFileName = datasetName + ".uimf";

            if (File.Exists(fileOrFolderPath))
            {
                if (fileOrFolderPath.EndsWith(uimfFileName))
                {
                    return fileOrFolderPath;
                }
            }
     
            if (!Directory.Exists(fileOrFolderPath))
            {
                throw new FileNotFoundException("Input path does not exist", fileOrFolderPath);
            }

            // Search for the immediate directory.
            string immediateUimfFilePath = Path.Combine(fileOrFolderPath, uimfFileName);

            if (File.Exists(immediateUimfFilePath))
            {
                return immediateUimfFilePath;
            }

            // Search for subdirectories
            IEnumerable<string> files = Directory.EnumerateFiles(fileOrFolderPath, datasetName + ".uimf", SearchOption.AllDirectories);

            try
            {
                return files.First();
            }
            catch (Exception)
            {
                throw new FileNotFoundException("Dataset " + datasetName + ".uimf was not found in the directory " + fileOrFolderPath + ". Please refine the search spec and try again.");
            }
        }

        /// <summary>
        /// process job and return the commandline, if not whitespace, set firstLine to false
        /// </summary>
        /// <param name="utility">
        /// The utility.
        /// </param>
        /// <param name="line">
        /// The line.
        /// </param>
        /// <param name="lineNumber">
        /// The line number.
        /// </param>
        /// <param name="exceptions">
        /// The exceptions.
        /// </param>
        /// <param name="firstLine">
        /// The first line.
        /// </param>
        /// <param name="ID">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ImsInformedProcess"/>.
        /// </returns>
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

                bool outputWhereInputsAre = string.IsNullOrEmpty(this.outputPath);

                string workspaceDir = outputWhereInputsAre ? UIMFFileDir : this.outputPath;
                string outputDirectory = Path.Combine(workspaceDir, datasetName + "_ImsMetabolitesFinderResult" + "_" + ionization);

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
            if (datasetName.Contains("pos"))
            {
                return "M+H";
            }

            if (datasetName.Contains("neg"))
            {
                return "M-H";
            }

            throw new Exception(datasetName + ": No ionization method is given, and it is not possible to infer the ionization method from the dataset name " + datasetName + ".");
        }
    }
}
