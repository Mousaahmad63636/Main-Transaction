using QuickTechPOS.Models;
using QuickTechPOS.ViewModels;
using System.Windows;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Interaction logic for CashOutDialog.xaml
    /// </summary>
    public partial class CashOutDialog : Window
    {
        private readonly CashOutViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the cash out dialog
        /// </summary>
        /// <param name="drawer">The drawer to take cash from</param>
        public CashOutDialog(Drawer drawer)
        {
            InitializeComponent();
            _viewModel = new CashOutViewModel(drawer);
            DataContext = _viewModel;

            // Close the dialog when the view model sets the DialogResult
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CashOutViewModel.DialogResult) &&
                    _viewModel.DialogResult.HasValue)
                {
                    DialogResult = _viewModel.DialogResult;
                }
            };
        }

        /// <summary>
        /// Gets the updated drawer after cash out
        /// </summary>
        public Drawer UpdatedDrawer => _viewModel.Drawer;
    }
}