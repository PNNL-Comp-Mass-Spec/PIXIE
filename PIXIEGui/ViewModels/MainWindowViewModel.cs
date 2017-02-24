namespace PIXIEGui.ViewModels
{
    using System;

    using ReactiveUI;

    /// <summary>
    /// View model for main window.
    /// </summary>
    public class MainWindowViewModel : ReactiveObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
        /// </summary>
        public MainWindowViewModel()
        {
            this.IndexViewModel = new IndexViewModel();
            this.FindViewModel = new FindViewModel();
            this.MatchViewModel = new MatchViewModel();

            // Wire up selected dataset
            this.IndexViewModel.WhenAnyValue(x => x.SelectedDataset)
                .Subscribe(
                ds =>
                    {
                        this.FindViewModel.SelectedDataset = ds;
                        this.MatchViewModel.SelectedDataset = ds;
                    });
        }

        /// <summary>
        /// Gets the view model for indexing and filtering.
        /// </summary>
        public IndexViewModel IndexViewModel { get; }

        /// <summary>
        /// Gets the view model for matching a target to a selected dataset.
        /// </summary>
        public FindViewModel FindViewModel { get; }

        /// <summary>
        /// Gets the view model for matching a target database to a selected dataset.
        /// </summary>
        public MatchViewModel MatchViewModel { get; }
    }
}
