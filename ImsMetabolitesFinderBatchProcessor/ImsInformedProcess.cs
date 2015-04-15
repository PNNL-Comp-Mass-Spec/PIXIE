// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImsInformedProcess.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   The ims informed process.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinderBatchProcessor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using ImsInformed.Domain;
    using ImsInformed.Workflows.CrossSectionExtraction;

    /// <summary>
    /// The ims informed process.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Code")]
    public class ImsInformedProcess : Process
    {
        public int JobID { get; private set; }
        public string DataSetName { get; set; }
        public bool Done { get; set; }
        public HashSet<string> FileResources { get; set; }
        public int LineNumber { get; private set; }
        public string ResultBinFile { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImsInformedProcess"/> class.
        /// </summary>
        /// <param name="ID">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="exe">
        /// The exe.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="shell">
        /// The shell.
        /// </param>
        /// <param name="resultBinFile">
        /// The result bin file.
        /// </param>
        /// <param name="lineNumber">
        /// The line number.
        /// </param>
        public ImsInformedProcess(int ID, string name, string exe, string arguments, bool shell, string resultBinFile, int lineNumber)
        {
            this.JobID = ID;
            this.DataSetName = name;
            this.StartInfo.FileName = exe;
            this.StartInfo.Arguments = arguments;
            this.StartInfo.UseShellExecute = shell;
            this.StartInfo.CreateNoWindow = !shell;
            this.FileResources = new HashSet<string>();
            this.Done = false;
            this.ResultBinFile = resultBinFile;
            this.LineNumber = lineNumber;
        }

        /// <summary>
        /// The are resources free.
        /// </summary>
        /// <param name="tasks">
        /// The tasks.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool AreResourcesFree(IEnumerable<ImsInformedProcess> tasks)
        {
            bool free = true;
            foreach (var task in tasks)
            {
                if (!task.Done)
                {
                    HashSet<string> sharedResources = new HashSet<string>(this.FileResources);
                    sharedResources.IntersectWith(task.FileResources);
                    if (sharedResources.Count != 0)
                    {
                        free = false;
                        break;
                    }
                }
            }
            return free;
        }

        /// <summary>
        /// The deserialize result from the bin file.
        /// </summary>
        /// <returns>
        /// The <see cref="CrossSectionWorkflowResult"/>.
        /// </returns>
        public IList<CrossSectionWorkflowResult> DeserializeResultBinFile()
        {
            IList<CrossSectionWorkflowResult> result;
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(this.ResultBinFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = (IList<CrossSectionWorkflowResult>)formatter.Deserialize(stream);
            }

            return result;
        }
    }
}
