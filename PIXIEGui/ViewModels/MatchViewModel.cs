namespace PIXIEGui.ViewModels
{
    using System;
    using System.IO;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using Microsoft.Win32;

    using PIXIE.Options;
    using PIXIE.Runners;

    using ReactiveUI;
    using ReactiveUI.Fody.Helpers;

    public class MatchViewModel : TaskViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatchViewModel" /> class.
        /// </summary>
        public MatchViewModel()
        {
            this.RestoreDefaultsImpl();

            this.RestoreDefaultsCommand = ReactiveCommand.Create(this.RestoreDefaultsImpl);

            this.BrowseLibraryFilesCommand = ReactiveCommand.Create(this.BrowseLibraryFilesImpl);

            var canMatch = this.WhenAnyValue(x => x.SelectedDataset, x => x.LibraryFilePath, x => x.OutputDirectory)
                               .Select(x => x.Item1 != null && File.Exists(x.Item2) && Directory.Exists(x.Item3));
            this.RunFeatureMatchingCommand = ReactiveCommand.CreateFromTask(async () => await this.RunFeatureMatchingImpl(), canMatch);
        }

        /// <summary>
        /// Gets a command that restores all properties to their default values.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RestoreDefaultsCommand { get; private set; }

        /// <summary>
        /// Gets a command that prompts the user to select a library file.
        /// </summary>
        public ReactiveCommand<Unit, Unit> BrowseLibraryFilesCommand { get; private set; }

        /// <summary>
        /// Gets a command that runs feature matching on the selected library file and dataset.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RunFeatureMatchingCommand { get; private set; }

        /// <summary>
        /// Gets or sets the dataset selected to run matcher on.
        /// </summary>
        [Reactive]
        public DatasetViewModel SelectedDataset { get; set; }

        /// <summary>
        /// Gets or sets the full file path to the database library file to match features against.
        /// </summary>
        [Reactive]
        public string LibraryFilePath { get; set; }

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
        public int MzError { get; set; }

        /// <summary>
        /// Gets or sets the the error tolerance in the drift time dimension.
        /// </summary>
        [Reactive]
        public int DriftError { get; set; }

        /// <summary>
        /// Restores all properties to their default values.
        /// Implementation of <see cref="RestoreDefaultsCommand" />.
        /// </summary>
        public void RestoreDefaultsImpl()
        {
            this.LibraryFilePath = string.Empty;

            this.DriftError = 1;
            this.MzError = 250;

            this.DriftTubeLength = 78.45;
            this.IsotopicScoreThreshold = 0.4;
            this.PeakShapeScoreThreshold = 0.4;
        }

        /// <summary>
        /// Prompts the user to select a library file.
        /// Implementation of <see cref="BrowseLibraryFilesCommand" />.
        /// </summary>
        private void BrowseLibraryFilesImpl()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = @"Supported Files|*.txt",
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                this.LibraryFilePath = dialog.FileName;
            }
        }

        /// <summary>
        /// Runs feature matching on the selected library file and dataset.
        /// Implementation of <see cref="RunFeatureMatchingCommand" />.
        /// </summary>
        private async Task RunFeatureMatchingImpl()
        {
            var matchOptions = new MatchOptions
            {
                DriftTubeLength = this.DriftTubeLength,
                DriftTimeError = this.DriftError,
                MassError = this.MzError,
                InputPath = this.SelectedDataset.DatasetPath,
                OutputPath = this.OutputDirectory,
                LibraryPath = this.LibraryFilePath,
                IsotopicScoreThreshold = this.IsotopicScoreThreshold,
                PeakShapeScoreThreshold = this.PeakShapeScoreThreshold
            };

            this.StatusMessage = string.Format("Matching {0}", Path.GetFileNameWithoutExtension(this.LibraryFilePath));
            await Task.Run(() => Matcher.ExecuteMatch(matchOptions));
            this.StatusMessage = "Completed";
        }
    }
}
