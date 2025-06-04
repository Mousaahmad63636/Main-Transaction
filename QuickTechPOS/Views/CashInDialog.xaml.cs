// File: QuickTechPOS/Views/CashInDialog.xaml.cs

using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.ViewModels;
using System.Windows;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Interaction logic for CashInDialog.xaml
    /// </summary>
    public partial class CashInDialog : Window
    {
        private readonly CashInViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the cash in dialog
        /// </summary>
        /// <param name="drawer">The drawer to add cash to</param>
        public CashInDialog(Drawer drawer)
        {
            InitializeComponent();

            // Apply current flow direction
            this.FlowDirection = LanguageManager.CurrentFlowDirection;

            _viewModel = new CashInViewModel(drawer);
            DataContext = _viewModel;

            // Close the dialog when the view model sets the DialogResult
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CashInViewModel.DialogResult) &&
                    _viewModel.DialogResult.HasValue)
                {
                    DialogResult = _viewModel.DialogResult;

                    // Set the result before closing to ensure the updated drawer is available
                    if (_viewModel.DialogResult == true)
                    {
                        // Log the updated drawer values
                        System.Console.WriteLine($"CashInDialog: Updated drawer values before closing:");
                        System.Console.WriteLine($"  - DrawerID: {_viewModel.Drawer.DrawerId}");
                        System.Console.WriteLine($"  - CashIn: ${_viewModel.Drawer.CashIn:F2}");
                        System.Console.WriteLine($"  - CurrentBalance: ${_viewModel.Drawer.CurrentBalance:F2}");

                        // Ensure all UI updates are processed
                        System.Windows.Application.Current.Dispatcher.Invoke(() => { });

                        this.Close();
                    }
                }
            };
        }

        /// <summary>
        /// Gets the updated drawer after cash in
        /// </summary>
        public Drawer UpdatedDrawer => _viewModel.Drawer;
    }
}