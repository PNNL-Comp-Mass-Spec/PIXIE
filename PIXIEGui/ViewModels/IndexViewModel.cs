namespace PIXIEGui.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using ImsInformed.IO;

    using Microsoft.Win32;

    using PIXIE.Options;
    using PIXIE.Runners;

    using ReactiveUI;
    using ReactiveUI.Fody.Helpers;

    public class IndexViewModel : TaskViewModelBase
    {
        /// <summary>
        /// The first dataset from the dataset list that has been marked as selected.
        /// </summary>
        private readonly ObservableAsPropertyHelper<DatasetViewModel> selectedDataset;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskViewModelBase" /> class.
        /// </summary>
        public IndexViewModel()
        {
            this.Datasets = new ReactiveList<DatasetViewModel> { ChangeTrackingEnabled = true };

            this.RestoreDefaultsImpl();

            // Sets the first item selected from the dataset list and the selected dataset.
            this.Datasets.ItemChanged
                         .Where(x => x.PropertyName == "IsSelected")
                         .Select(_ => this.Datasets.FirstOrDefault(ds => ds.IsSelected))
                         .ToProperty(this, x => x.SelectedDataset, out this.selectedDataset);

            this.RestoreDefaultsCommand = ReactiveCommand.Create(this.RestoreDefaultsImpl);

            // Index files command should only be available when there are datasets selected to run on.
            var canIndex = this.Datasets.ItemChanged.Where(s => s.PropertyName == "IsSelected")
                                        .Select(_ => this.Datasets.Any(ds => ds.IsSelected && !ds.IsIndexed));
            this.IndexFilesCommand = ReactiveCommand.CreateFromTask(async () => await this.IndexFilesImpl(), canIndex);

            // Convert files command should only be available when there are datasets selected to run on.
            var canConvert = this.WhenAnyValue(x => x.OutputDirectory, x => x.Datasets.ItemChanged)
                                 .Select(x => Directory.Exists(x.Item1) && this.Datasets.Any(ds => ds.IsSelected));
            this.ConvertFilesCommand = ReactiveCommand.CreateFromTask(this.ConvertFilesImpl, canConvert);

            this.AddDatasetCommand = ReactiveCommand.Create(this.AddDatasetImpl);

            this.AvailableFileFormats = new ReactiveList<FileFormatEnum>(Enum.GetValues(typeof(FileFormatEnum)).Cast<FileFormatEnum>());
            this.SelectedFileFormat = FileFormatEnum.UIMF;  // Select UIMF as conversion target format by default
        }

        /// <summary>
        /// The list of datasets for indexing.
        /// </summary>
        public ReactiveList<DatasetViewModel> Datasets { get; }

        /// <summary>
        /// Gets or sets the first dataset from the dataset list that has been marked as selected.
        /// </summary>
        public DatasetViewModel SelectedDataset => this.selectedDataset?.Value;

        /// <summary>
        /// Gets a command that restores all properties to their default values.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RestoreDefaultsCommand { get; }

        /// <summary>
        /// Gets a command that indexes all of the currently selected files.
        /// </summary>
        public ReactiveCommand<Unit, Unit> IndexFilesCommand { get; }

        /// <summary>
        /// Gets a command that converts the selected files.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ConvertFilesCommand { get; }

        /// <summary>
        /// Gets a commmand that requests that the user selects a file path and then adds the file to the <see cref="Datasets" /> collection.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddDatasetCommand { get; }

        /// <summary>
        /// Gets a list of the possible file formats that can be selected for conversion.
        /// </summary>
        public ReactiveList<FileFormatEnum> AvailableFileFormats { get; }

        /// <summary>
        /// Gets or sets the selected file format for conversion.
        /// </summary>
        [Reactive]
        public FileFormatEnum SelectedFileFormat { get; set; }

        /// <summary>
        /// Gets or sets the length of the IMS drift tube.
        /// </summary>
        [Reactive]
        public double DriftTubeLength { get; set; }

        /// <summary>
        /// Restores all properties to their default values.
        /// Implementation of <see cref="RestoreDefaultsCommand" />.
        /// </summary>
        public void RestoreDefaultsImpl()
        {
            this.OutputDirectory = string.Empty;
            this.SelectedFileFormat = FileFormatEnum.MzML;
            this.DriftTubeLength = 78.45;
        }

        /// <summary>
        /// Indexes all of the currently selected files.
        /// Implementation of the see <see cref="IndexFilesCommand" />.
        /// </summary>
        public async Task IndexFilesImpl()
        {
            var indexerOptions = new IndexerOptions
            {
                UimfFileLocation = new List<string>(this.Datasets.Where(ds => ds.IsSelected).Select(ds => ds.DatasetPath))
            };

            IProgress<double> progressReporter = indexerOptions.UimfFileLocation.Count > 1
                                                     ? new Progress<double>(p => this.CompletionPercent = p)
                                                     : new Progress<double>();

            this.StatusMessage = "Indexing...";
            progressReporter.Report(0.01);  // Force progress bar to show.

            await Task.Run(() => Indexer.ExecuteIndexer(indexerOptions, progressReporter));

            foreach (var dataset in this.Datasets)
            {
                dataset.IsIndexed = true;
            }

            this.StatusMessage = "Completed";

            progressReporter.Report(0.0); // Reset progress bar.
        }

        /// <summary>
        /// Converts all of the currently selected files.
        /// Implementation of the see <see cref="ConvertFilesCommand" />.
        /// </summary>
        /// <returns></returns>
        public async Task ConvertFilesImpl()
        {
            var datasetsToRun = this.Datasets.Where(ds => ds.IsSelected).ToList();
            IProgress<double> progressReporter = datasetsToRun.Count > 1 ? new Progress<double>(p => this.CompletionPercent = p) : new Progress<double>();

            this.StatusMessage = "Converting...";
            progressReporter.Report(0.01);  // Force progress bar to show.
            int count = 0;
            foreach (var dataset in datasetsToRun)
            {
                var converterOptions = new ConverterOptions
                {
                    OutputPath = this.OutputDirectory,
                    ConversionType = this.SelectedFileFormat.ToString(),
                    TubeLength = this.DriftTubeLength.ToString(CultureInfo.InvariantCulture),
                    InputPath = dataset.DatasetPath,
                };

                await Task.Run(() => Converter.ExecuteConverter(converterOptions));

                //this.Datasets.Remove(dataset);
                //this.AddDataset(string.Empty); // TODO: put path of newly created dataset.

                progressReporter.Report(100.0 * count++ / datasetsToRun.Count);
            }

            this.StatusMessage = "Completed";

            progressReporter.Report(0.0); // Reset progress bar.
        }

        /// <summary>
        /// Adds the path to the dataset path to dataset list.
        /// </summary>
        /// <param name="filePath">The path to the file to add</param>
        private void AddDataset(string filePath)
        {
            var isIndexed = Indexer.CheckIndex(filePath);
            var datasetViewModel = new DatasetViewModel { DatasetPath = filePath, IsIndexed = isIndexed };
            datasetViewModel.RemoveCommand.Where(removeConfirmed => removeConfirmed)
                                          .Subscribe(_ => this.Datasets.Remove(datasetViewModel));
            if (this.Datasets.Any(ds => ds.DatasetPath == filePath))
            {   // Don't add the dataset if it is already in the list.
                return;
            }

            this.Datasets.Add(datasetViewModel);

            if (this.Datasets.Count == 1)
            {   // Automatically select the first dataset.
                datasetViewModel.IsSelected = true;
            }
        }

        /// <summary>
        /// Requests that the user selects a file path and then adds the file to the <see cref="Datasets" /> collection.
        /// Implementation of the <see cref="AddDatasetCommand" />.
        /// </summary>
        public void AddDatasetImpl()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".uimf",
                Filter = @"Supported Files|*.uimf;*.mzML;*.d",
                Multiselect = true
            };

            var result = dialog.ShowDialog();
            IEnumerable<string> filePaths;
            if (result == true)
            {
                filePaths = dialog.FileNames;
            }
            else
            {
                return;
            }

            foreach (var filePath in filePaths)
            {
                this.AddDataset(filePath);
            }
        }
    }
}
