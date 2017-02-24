namespace PIXIEGui.ViewModels
{
    using System;
    using System.IO;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows;

    using ReactiveUI;
    using ReactiveUI.Fody.Helpers;

    /// <summary>
    /// A view model for representing an input dataset in the dataset list.
    /// </summary>
    public class DatasetViewModel : ReactiveObject
    {
        /// <summary>
        /// The extension extracted from the dataset path.
        /// </summary>
        private readonly ObservableAsPropertyHelper<string> fileType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetViewModel" /> class.
        /// </summary>
        public DatasetViewModel()
        {
            this.RemoveCommand = ReactiveCommand.Create(this.CheckForClosure);
            this.RemoveCommand.Where(removeConfirmed => removeConfirmed)
                              .Subscribe(_ => this.IsSelected = false);

            // Set the file type when the dataset path is set
            this.WhenAnyValue(x => x.DatasetPath)
                .Select(Path.GetExtension)
                .Where(ext => !string.IsNullOrEmpty(ext))
                .Select(ext => ext.ToUpper())
                .Where(ext => !string.IsNullOrEmpty(ext))
                .Select(ext => ext.Substring(1, ext.Length - 1))
                .ToProperty(this, x => x.FileType, out this.fileType);
        }

        /// <summary>
        /// Gets a command that signals that this item should be removed.
        /// </summary>
        public ReactiveCommand<Unit, bool> RemoveCommand { get; private set; }

        /// <summary>
        /// Gets or sets the full path to the dataset.
        /// </summary>
        [Reactive]
        public string DatasetPath { get; set; }

        /// <summary>
        /// Gets the extension extracted from the dataset path.
        /// </summary>
        public string FileType => this.fileType?.Value ?? string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this file has been indexed yet.
        /// </summary>
        [Reactive]
        public bool IsIndexed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item has been selected in its containing list.
        /// </summary>
        [Reactive]
        public bool IsSelected { get; set; }

        /// <summary>
        /// Asks the user to confirm whether they actually want to remove the item.
        /// </summary>
        /// <returns>A value indicating whether the user actually wants to remove the item.</returns>
        private bool CheckForClosure()
        {
            var message = $"Are you sure you would like to remove {Path.GetFileName(this.DatasetPath)}?";
            var result = MessageBox.Show(message, "Remove?", MessageBoxButton.YesNo);
            return result == MessageBoxResult.Yes;
        }
    }
}
