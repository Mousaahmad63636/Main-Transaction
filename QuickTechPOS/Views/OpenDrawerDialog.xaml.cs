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
                    if (_viewModel.DialogResult == true)
                    {
                        this.Close();
                    }
                }
            };
        }
    }
}