namespace PIXIEGui.ViewModels
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows;

    using Microsoft.WindowsAPICodePack.Dialogs;

    using ReactiveUI;
    using ReactiveUI.Fody.Helpers;

    /// <summary>
    /// View model for tracking the progress of a task.
    /// </summary>
    public class TaskViewModelBase : ReactiveObject
    {
        /// <summary>
        /// A value indicating whether spectra are currently being processed.
        /// </summary>
        private readonly ObservableAsPropertyHelper<bool> isRunning;

        /// <summary>
        /// A value that indicates whether or not the progress bar should be visible.
        /// </summary>
        private readonly ObservableAsPropertyHelper<Visibility> showProgressBar;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskViewModelBase" /> class.
        /// </summary>
        public TaskViewModelBase()
        {
            this.BrowseOutputDirectoriesCommand = ReactiveCommand.Create(this.BrowseOutputDirectoriesImpl);

            // When progress is zero, turn off the IsRunning flag.
            this.WhenAnyValue(x => x.CompletionPercent)
                .Select(x => x > 0)
                .ToProperty(this, x => x.IsRunning, out this.isRunning);

            // When processing isn't running, hide progress bar
            this.WhenAnyValue(x => x.IsRunning)
                .Select(isRunning => isRunning ? Visibility.Visible : Visibility.Collapsed)
                .ToProperty(this, x => x.ShowProgressBar, out this.showProgressBar);
        }

        /// <summary>
        /// Gets a command that requests the user select a valid converter output directory.
        /// </summary>
        public ReactiveCommand<Unit, Unit> BrowseOutputDirectoriesCommand { get; }

        /// <summary>
        /// Gets a value indicating whether spectra are currently being processed.
        /// </summary>
        public bool IsRunning => this.isRunning?.Value ?? false;

        /// <summary>
        /// Gets a value that indicates whether or not the progress bar should be visible.
        /// </summary>
        public Visibility ShowProgressBar => this.showProgressBar?.Value ?? Visibility.Collapsed;

        /// <summary>
        /// Gets the percentage of completion of the task.
        /// </summary>
        [Reactive]
        public double CompletionPercent { get; protected set; }

        /// <summary>
        /// Gets or sets the message describing what the status of the current running task is.
        /// </summary>
        [Reactive]
        public string StatusMessage { get; protected set; }

        /// <summary>
        /// Gets the path to the output directory to write to.
        /// </summary>
        [Reactive]
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Requests the user select a valid converter output directory.
        /// Implementation of <see cref="BrowseOutputDirectoriesCommand" />.
        /// </summary>
        public void BrowseOutputDirectoriesImpl()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.OutputDirectory = dialog.FileName;
            }
        }
    }
}
