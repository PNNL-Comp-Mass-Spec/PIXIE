namespace PIXIEGui.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;

    using ImsInformed.Targets;

    using PIXIE.Options;
    using PIXIE.Runners;

    using ReactiveUI;
    using ReactiveUI.Fody.Helpers;

    /// <summary>
    /// This is a view model for the view that selects options for the single target search mode of PIXiE.
    /// </summary>
    public class FindViewModel : TaskViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FindViewModel" /> class.
        /// </summary>
        public FindViewModel()
        {
            this.RestoreDefaultsImpl();
            this.IsFormulaTargetSelected = true;
            this.MzTarget = 0;
            this.FormulaTarget = string.Empty;

            this.IonizationMethods = new ReactiveList<string> { "M+H", "M-H", "M+Na", "APCI", "M+HCOO", "M-2H+Na" };
            this.GraphicsFormats = new ReactiveList<string> { "PNG", "SVG" };

            this.RestoreDefaultsCommand = ReactiveCommand.Create(this.RestoreDefaultsImpl);

            var canSearch = this.WhenAnyValue(
                                              x => x.IsMzTargetSelected,
                                              x => x.IsFormulaTargetSelected,
                                              x => x.FormulaTarget,
                                              x => x.MzTarget,
                                              x => x.SelectedDataset,
                                              x => x.OutputDirectory)
                                .Select(_ => 
                                             this.IsSelectedTargetValid() && 
                                             this.SelectedDataset != null &&
                                             this.SelectedDataset.IsIndexed &&
                                             Directory.Exists(this.OutputDirectory));
            this.FindCommand = ReactiveCommand.CreateFromTask(async () => await this.FindImpl(), canSearch);
        }

        /// <summary>
        /// Gets a command that restores all properties to their default values.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RestoreDefaultsCommand { get; }

        /// <summary>
        /// Gets a command that starts a search for the selected target.
        /// </summary>
        public ReactiveCommand<Unit, Unit> FindCommand { get; }

        /// <summary>
        /// Gets or sets the dataset selected to run finder on.
        /// </summary>
        [Reactive]
        public DatasetViewModel SelectedDataset { get; set; }

        /// <summary>
        /// Gets the list of possible adducts for selection.
        /// </summary>
        public ReactiveList<string> IonizationMethods { get; }

        /// <summary>
        /// Gets or sets the adduct selected from the adduct list.
        /// </summary>
        [Reactive]
        public string SelectedIonizationMethod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the target to search for is specified by an M/Z.
        /// </summary>
        [Reactive]
        public bool IsMzTargetSelected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the target to search for is specified by a chemical formula.
        /// </summary>
        [Reactive]
        public bool IsFormulaTargetSelected { get; set; }

        /// <summary>
        /// Gets or sets the selected M/Z to search for.
        /// </summary>
        [Reactive]
        public double MzTarget { get; set; }

        /// <summary>
        /// Gets or sets the select formula to search for.
        /// </summary>
        [Reactive]
        public string FormulaTarget { get; set; }

        /// <summary>
        /// Gets or sets the length of the drift tube in centimeters.
        /// </summary>
        [Reactive]
        public double DriftTubeLength { get; set; }

        /// <summary>
        /// Gets or sets the isotopic profile score threshold for features.
        /// </summary>
        [Reactive]
        public double IsotopicScoreThreshold { get; set; }

        /// <summary>
        /// Gets or sets the peak shape score threshold for features.
        /// </summary>
        [Reactive]
        public double PeakShapeScoreThreshold { get; set; }

        /// <summary>
        /// Gets or sets the error tolerance in the M/Z dimension.
        /// </summary>
        [Reactive]
        public double MzError { get; set; }

        /// <summary>
        /// Gets or sets the the error tolerance in the drift time dimension.
        /// </summary>
        [Reactive]
        public double DriftError { get; set; }

        /// <summary>
        /// Gets or sets the fraction of max accumulated frames under which voltage groups would be discarded.
        /// </summary>
        [Reactive]
        public double InsufficientFramesFraction { get; set; }

        /// <summary>
        /// Gets or sets the number of points to be used to smooth the IMS spectra.
        /// </summary>
        [Reactive]
        public int SmoothingPoints { get; set; }

        /// <summary>
        /// Gets or sets the filter level for multidimensional peak finding.
        /// </summary>
        [Reactive]
        public double FeatureFilterLevel { get; set; }

        /// <summary>
        /// Gets or sets the relative intensity as percentage of the highest peak intensity in a single m/z range.
        /// </summary>
        [Reactive]
        public double RelativeIntensityThreshold { get; set; }

        /// <summary>
        /// Gets or sets the minimum acceptable R^2 value identifying peak responses belonging to the same ion.
        /// </summary>
        [Reactive]
        public double MinR2 { get; set; }

        /// <summary>
        /// Gets the list of possible image formats that can be selected (svg, png).
        /// </summary>
        public ReactiveList<string> GraphicsFormats { get; }

        /// <summary>
        /// Gets or sets the format you want to qc file to be plotted, e.g., svg, png.
        /// </summary>
        [Reactive]
        public string GraphicsFormat { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether iteratively reweighted least squares,
        /// weighted using bisquare weights to imporve measurement accuracy should be used.
        /// </summary>
        [Reactive]
        public bool UseRobustRegression { get; set; }

        /// <summary>
        /// Gets or sets the absolute intensity score threshold for features.
        /// </summary>
        [Reactive]
        public double AbsoluteMaxIntensity { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of fit points required to compute cross section.
        /// </summary>
        [Reactive]
        public int MaxOutliers { get; set; }

        /// <summary>
        /// Restores all properties to their default values.
        /// Implementation of <see cref="RestoreDefaultsCommand" />.
        /// </summary>
        public void RestoreDefaultsImpl()
        {
            this.FormulaTarget = string.Empty;
            this.MzTarget = 0.0;

            this.DriftError = 0.5;
            this.MzError = 250;

            this.SelectedIonizationMethod = "M+H";

            this.DriftTubeLength = 78.45;
            this.SmoothingPoints = 11;
            this.FeatureFilterLevel = 0.25;
            this.AbsoluteMaxIntensity = 0;
            this.IsotopicScoreThreshold = 0.4;
            this.PeakShapeScoreThreshold = 0.4;
            this.RelativeIntensityThreshold = 10;
            this.MaxOutliers = 1;
            this.UseRobustRegression = false;
            this.MinR2 = 0.96;
            this.InsufficientFramesFraction = 0.0;

            this.GraphicsFormat = "SVG";
        }

        /// <summary>
        /// Performs the search on the specified target and dataset.
        /// Implementation of <see cref="FindCommand" />.
        /// </summary>
        /// <returns></returns>
        private async Task FindImpl()
        {
            string target = this.IsMzTargetSelected ? this.MzTarget.ToString(CultureInfo.InvariantCulture) : this.FormulaTarget;
            var finderOptions = new FinderOptions
            {
                InputPath = this.SelectedDataset.DatasetPath,
                DriftTubeLength = this.DriftTubeLength,
                InsufficientFramesFraction = this.InsufficientFramesFraction,
                IsotopicScoreThreshold = this.IsotopicScoreThreshold,
                PeakShapeScoreThreshold = this.PeakShapeScoreThreshold,
                TargetList = new List<string> { target },
                IonizationList = new List<string> { this.SelectedIonizationMethod.ToString() },
                OutputPath = this.OutputDirectory,
                PrePpm = this.MzError,
                DriftTimeToleranceInMs = this.DriftError,
                NumberOfPointsForSmoothing = this.SmoothingPoints,
                FeatureFilterLevel = this.FeatureFilterLevel,
                RelativeIntensityPercentageThreshold = this.RelativeIntensityThreshold,
                MinR2 = this.MinR2,
                RobustRegression = this.UseRobustRegression,
                GraphicsFormat = this.GraphicsFormat,
                IntensityThreshold = this.AbsoluteMaxIntensity,
                MaxOutliers = this.MaxOutliers,
                UseAverageTemperature = true
            };

            try
            {
                this.StatusMessage = string.Format("Running {0}", target);
                await Task.Run(() => Finder.ExecuteFinder(finderOptions));
                this.StatusMessage = string.Format("Completed {0}", target);
            }
            catch (Exception e)
            {
                var message = string.Format("Cannot run finder: {0}\n\n{1}", e.Message, e.StackTrace);
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Checks to see if the selected MZ or formula target is a valid one.
        /// </summary>
        /// <returns>A value indicating whether the selected target is valid.</returns>
        private bool IsSelectedTargetValid()
        {
            if (this.IsMzTargetSelected && this.MzTarget > 0)
            {   // M/Z target is selected.
                return true;
            }
            else
            {   // Formula target is selected.
                var rgx = new Regex("([CHNOSP]([0-9])*)+");
                if (rgx.IsMatch(this.FormulaTarget))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
