// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextSearchSpecProcessor.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the TextSearchSpecProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinderBatchProcessor.SearchSpec
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// The search spec processor.
    /// </summary>
    public class TextSearchSpecProcessor
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
        /// Initializes a new instance of the <see cref="TextSearchSpecProcessor"/> class.
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
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <param name="showWindow">
        /// The show window.
        /// </param>
        /// <param name="force">
        /// The force.
        /// </param>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        /// <exception cref="AggregateException">
        /// </exception>
        public TextSearchSpecProcessor(string utilityPath, string searchSpecFilePath, string inputPath, string outputPath, bool showWindow, bool force)
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

            this.TaskList = new List<ImsInformedProcess>();

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
                    
                    ImsInformedProcess process = this.ProcessLine(utilityPath, line, lineNumber, exceptions, ref firstLine, ref ID);
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
        /// process job and return the command line, if not whitespace, set firstLine to false
        /// </summary>
        /// <param name="utility">
        /// The name for processing software.
        /// </param>
        /// <param name="line">
        /// The line to be processed in the search spec txt file.
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
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Returns null to skip current line. <see cref="ImsInformedProcess"/>.
        /// </returns>
        private ImsInformedProcess ProcessLine(string utility, string line, int lineNumber, List<Exception> exceptions, ref bool firstLine, ref int id)
        {
            bool outputWhereInputsAre = string.IsNullOrEmpty(this.outputPath);
            string datasetName = string.Empty;
            
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
                    throw new FormatException("Line " + lineNumber + " \"" + line + "\" does not have enough arguments");
                }
                else if (parts.Count() > 3)
                {
                    throw new FormatException("Line " + lineNumber + " \"" + line + "\" has too many arguments, please don't put spaces in targets or ionization modes");
                }

                // Group tokens into ionization modes targets according to parentheses
                datasetName = parts[0];

                IList<string> targetList = parts[1].Split(":,&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                targetList = parts[1].Split(":,&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string ionization;
                if (parts.Count() <= 2)
                {
                    ionization = this.InferIonization(datasetName);
                }
                else
                {
                    ionization = parts[2];
                }

                // Figure out various file paths
                string uimfPath = this.FindUimfFile(this.inputPath, datasetName);
                string UIMFFileDir = Path.GetDirectoryName(uimfPath);
                string workspaceDir = outputWhereInputsAre ? UIMFFileDir : this.outputPath;
                
                string outputDirectory = Path.Combine(workspaceDir, datasetName + "_ImsMetabolitesFinderResult" + "_" + ionization);
                string binPath = Path.Combine(outputDirectory, datasetName + "_" + ionization + "_Result.bin");

                string commandline = null;
                commandline += String.Join(" ", targetList);
                commandline += " -i " + uimfPath + " "; 
                
                commandline += "-m " + ionization + " ";
                commandline += "-o " + outputDirectory + " ";
                commandline += "--id " + id + " ";
                commandline += this.Arguments + " ";

                id++;
                return new ImsInformedProcess(id, datasetName, utility, commandline, this.showWindow, binPath, lineNumber);
            }
            catch (Exception e)
            {
                e.Data.Add("lineNumber", lineNumber);
                exceptions.Add(e);
                return null;
            }
        }

        /// <summary>
        /// Infer ionization method from data set name
        /// </summary>
        /// <param name="datasetName">
        /// The dataset name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        private string InferIonization(string datasetName)
        {
            if (datasetName.Contains("pos"))
            {
                return "M+H,M+Na";
            }

            if (datasetName.Contains("neg"))
            {
                return "M-H";
            }

            throw new Exception(datasetName + ": No ionization method is given, and it is not possible to infer the ionization method from the dataset name " + datasetName + ".");
        }
    }
}
