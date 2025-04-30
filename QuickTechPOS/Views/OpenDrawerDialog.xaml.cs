// File: QuickTechPOS/Views/OpenDrawerDialog.xaml.cs
using QuickTechPOS.ViewModels;
using System.Windows;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Interaction logic for OpenDrawerDialog.xaml
    /// </summary>
    public partial class OpenDrawerDialog : Window
    {
        private readonly OpenDrawerViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the open drawer dialog
        /// </summary>
        /// <param name="viewModel">The view model for this dialog</param>
        public OpenDrawerDialog(OpenDrawerViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            // Close the dialog when the view model sets the DialogResult
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(OpenDrawerViewModel.DialogResult) &&
                    _viewModel.DialogResult.HasValue)
                {
                    DialogResult = _viewModel.DialogResult;
                }
            };
        }
    }
}