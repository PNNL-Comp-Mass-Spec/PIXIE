// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessState.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the ProcessState type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PIXIEBatchProcessor
{
    /// <summary>
    /// The process state.
    /// </summary>
    public enum ProcessState
    {
        /// <summary>
        /// The done conclusive.
        /// </summary>
        DoneConclusive,

        /// <summary>
        /// The done inconclusive.
        /// </summary>
        DoneInconclusive,

        /// <summary>
        /// The not done.
        /// </summary>
        NotDone
    }
}
