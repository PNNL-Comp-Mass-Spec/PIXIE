// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultAggregator.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the ResultAggregator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace IFinderBatchProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using IFinderBatchProcessor.Export;
    using IFinderBatchProcessor.Util.DeviceModule.Util;

    using ImsInformed;
    using ImsInformed.Targets;
    using ImsInformed.Workflows.CrossSectionExtraction;

    /// <summary>
    /// The result aggregator.
    /// </summary>
    public class ResultAggregator
    {
        /// <summary>
        /// The normalized drift time tolerance.
        /// </summary>
        private bool empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultAggregator"/> class.
        /// </summary>
        /// <param name="processes">
        /// The processes.
        /// </param>
        public ResultAggregator(IEnumerable<ImsInformedProcess> processes)
        {
            this.Tasks = processes;
            this.empty = true;
        }

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        public IEnumerable<ImsInformedProcess> Tasks { get; private set; }

        /// <summary>
        /// process result files collected and generate the CCS/mz DB.
        /// </summary>
        /// <param name="analysisDirectory">
        /// The analysis directory.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public void ProcessResultFiles(string analysisDirectory, AnalysisLibrary lib)
        {
            long count = 0;
            long totalTasks = this.Tasks.Count();
            foreach (ImsInformedProcess task in this.Tasks)
            {
                // Deserialize the results and dispatch them into lookup tables.
                try
                {
                    IList<CrossSectionWorkflowResult> results = task.DeserializeResultBinFile();
                    
                    AsyncHelpers.RunSync(() => lib.InsertResult(results));
                    count++;
                    Console.WriteLine("Number of results processed: {0}/{1}", count, totalTasks);
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    // Dispose the task as it is no longer used.
                    task.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Result processing for {0} failed", task.DataSetName);
                    Console.WriteLine("Exception: {0}", e.Message);
                    Console.WriteLine(string.Empty);
                }
            }

            this.empty = false;
        }
    }
}
