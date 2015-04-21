// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChemicalBasedAnalysisResult.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the ChemicalBasedAnalysisResult type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinderBatchProcessor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using ImsInformed.Domain;
    using ImsInformed.Interfaces;
    using ImsInformed.Workflows.CrossSectionExtraction;

    using OxyPlot;

    /// <summary>
    /// The chemical based analysis result.
    /// </summary>
    [Serializable]
    public class ChemicalBasedAnalysisResult 
    {
        /// <summary>
        /// The collision cross section tolerance.
        /// </summary>
        public const double CollisionCrossSectionTolerance = 5;

        /// <summary>
        /// The normalized drift time tolerance.
        /// </summary>
        public const double NormalizedDriftTimeTolerance = 0.75;

        /// <summary>
        /// The fusion number.
        /// </summary>
        public int FusionNumber { get; private set; }

        /// <summary>
        /// The ionization method.
        /// </summary>
        public IImsTarget Target{ get; private set; }

        /// <summary>
        /// The analysis status.
        /// </summary>
        public AnalysisStatus AnalysisStatus { get; private set;}

        /// <summary>
        /// The isomer results.
        /// </summary>
        public IEnumerable<TargetIsomerReport> DetectedIsomers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChemicalBasedAnalysisResult"/> class. 
        /// The initiate chemical based analysis result.
        /// </summary>
        /// <param name="result">
        /// The result ionization result to initiate from.
        /// </param>
        /// <returns>
        /// The <see cref="ChemicalBasedAnalysisResult"/>.
        /// </returns>
        public ChemicalBasedAnalysisResult(CrossSectionWorkflowResult result)
        {
            this.InitiateFromWorkflowResult(result);
        }

        public ChemicalBasedAnalysisResult(ChemicalBasedAnalysisResult result, CrossSectionWorkflowResult newWorkflowResult)
        {
            // previous results inconclusive, new result conclusive
            if (!IsConclusive(result.AnalysisStatus))
            {
                this.InitiateFromWorkflowResult(newWorkflowResult);
            }

            // new result inconclusive, previous result conclusive or not conclusive
            else if (!IsConclusive(newWorkflowResult.AnalysisStatus)) 
            {
                this.AnalysisStatus = result.AnalysisStatus;
                this.FusionNumber = result.FusionNumber;
                this.DetectedIsomers = result.DetectedIsomers;
                this.Target = result.Target;
            }

            // both result conclusive, conflict
            if (CheckConflict(result, newWorkflowResult))
            {
                this.AnalysisStatus = AnalysisStatus.ConflictRuns;
                this.FusionNumber = result.FusionNumber;
                this.DetectedIsomers = result.DetectedIsomers;
                this.Target = result.Target;
            }

            // both result conclusive, no conflict
            IEnumerable<TargetIsomerReport> newIsomerList = result.DetectedIsomers.Zip(newWorkflowResult.MatchingIsomers, (A, B) => FuseIsomerResult(A, B, result.FusionNumber, 1));

            this.AnalysisStatus = result.AnalysisStatus;
            this.FusionNumber = result.FusionNumber + 1;
            this.DetectedIsomers = newIsomerList;
            this.Target = result.Target;
        }

        private static TargetIsomerReport FuseIsomerResult(TargetIsomerReport A, TargetIsomerReport B, int weightA, int weightB)
        {
            TargetIsomerReport newIsomer;
            newIsomer.CrossSectionalArea = A.CrossSectionalArea * weightA + B.CrossSectionalArea * weightB;
            newIsomer.CrossSectionalArea /= weightA + weightB;

            if (A.LastVoltageGroupDriftTimeInMs > 0 && B.LastVoltageGroupDriftTimeInMs > 0)
            {
                newIsomer.LastVoltageGroupDriftTimeInMs = A.LastVoltageGroupDriftTimeInMs * weightA + B.LastVoltageGroupDriftTimeInMs * weightB;
                newIsomer.LastVoltageGroupDriftTimeInMs /= weightA + weightB;
            }
            else
            {
                newIsomer.LastVoltageGroupDriftTimeInMs = A.LastVoltageGroupDriftTimeInMs;
            }

            newIsomer.Mobility = A.Mobility;
            newIsomer.MonoisotopicMass = A.MonoisotopicMass;
            return newIsomer;
        }

        /// <summary>
        /// The is conclusive.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool IsConclusive(AnalysisStatus status)
        {
            if (status == AnalysisStatus.Positive || status == AnalysisStatus.Negative)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The initiate from workflow result.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        private void InitiateFromWorkflowResult(CrossSectionWorkflowResult result)
        {
            this.Target = result.Target;
            this.AnalysisStatus = result.AnalysisStatus;
            this.FusionNumber = 1;
            this.DetectedIsomers = result.MatchingIsomers;
        }

        /// <summary>
        /// Check if there are conflicts in 
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <param name="newWorkflowResult">
        /// The new workflow result.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private static bool CheckConflict(ChemicalBasedAnalysisResult result, CrossSectionWorkflowResult newWorkflowResult)
        {
            if (newWorkflowResult.Target != result.Target)
            {
                throw new InvalidOperationException("Cannot check conflict for results from different chemicals or with different ionization methods");
            }

            if (result.DetectedIsomers.Count() != newWorkflowResult.MatchingIsomers.Count())
            {
                return true;
            }

            if (result.AnalysisStatus != newWorkflowResult.AnalysisStatus)
            {
                return true;
            }
            
            IEnumerable<bool> r = result.DetectedIsomers.Zip(newWorkflowResult.MatchingIsomers, CheckConflict);
            return !r.Select(b => b == false).Any();
        }

        /// <summary>
        /// The check conflict.
        /// </summary>
        /// <param name="A">
        /// The a.
        /// </param>
        /// <param name="B">
        /// The b.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool CheckConflict(TargetIsomerReport A, TargetIsomerReport B)
        {
            if (Math.Abs(A.CrossSectionalArea - B.CrossSectionalArea) > CollisionCrossSectionTolerance)
            {
                return true;
            }

            if (Math.Abs(A.LastVoltageGroupDriftTimeInMs - B.LastVoltageGroupDriftTimeInMs) > NormalizedDriftTimeTolerance)
            {
                return true;
            }

            return false;
        }
    }
}
