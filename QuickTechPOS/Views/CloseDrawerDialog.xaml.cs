using QuickTechPOS.Models;
using QuickTechPOS.ViewModels;
using System.Windows;

namespace QuickTechPOS.Views
{
    public partial class CloseDrawerDialog : Window
    {
        private readonly CloseDrawerViewModel _viewModel;

        public CloseDrawerDialog(Drawer drawer)
        {
            InitializeComponent();
            _viewModel = new CloseDrawerViewModel(drawer);
            DataContext = _viewModel;

            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CloseDrawerViewModel.DialogResult) &&
                    _viewModel.DialogResult.HasValue)
                {
                    DialogResult = _viewModel.DialogResult;
                }
            };
        }

        public Drawer ClosedDrawer => _viewModel.Drawer;
    }
}