// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultAggregator.cs" company="PNNL">
//   The Software was produced by Battelle under Contract No. DE-AC05-76RL01830 with the Department of Energy.  
// The U.S. Government is granted for itself and others acting on its behalf a nonexclusive, paid-up, irrevocable 
// worldwide license in this data to reproduce, prepare derivative works, distribute copies to the public, perform 
// publicly and display publicly, and to permit others to do so.  The specific term of the license can be identified 
// by inquiry made to Battelle or DOE.  NEITHER THE UNITED STATES NOR THE UNITED STATES DEPARTMENT OF ENERGY, NOR 
// ANY OF THEIR EMPLOYEES, MAKES ANY WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LEGAL LIABILITY OR RESPONSIBILITY
// FOR THE ACCURACY, COMPLETENESS OR USEFULNESS OF ANY DATA, APPARATUS, PRODUCT OR PROCESS DISCLOSED, OR REPRESENTS
// THAT ITS USE WOULD NOT INFRINGE PRIVATELY OWNED RIGHTS.
// </copyright>
// <summary>
//   Defines the ResultAggregator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PIXIEBatchProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ImsInformed;
    using ImsInformed.Targets;
    using ImsInformed.Workflows.CrossSectionExtraction;

    using PIXIEBatchProcessor.Export;
    using PIXIEBatchProcessor.Util.DeviceModule.Util;

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
