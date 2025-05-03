using QuickTechPOS.Models;
using QuickTechPOS.ViewModels;
using System;
using System.Windows;
using System.Windows.Threading;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Interaction logic for CloseDrawerDialog.xaml
    /// </summary>
    public partial class CloseDrawerDialog : Window
    {
        private readonly CloseDrawerViewModel _viewModel;

        public CloseDrawerDialog(Drawer drawer)
        {
            InitializeComponent();
            _viewModel = new CloseDrawerViewModel(drawer);
            DataContext = _viewModel;

            // Watch for property changes
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CloseDrawerViewModel.DialogResult) &&
                    _viewModel.DialogResult.HasValue)
                {
                    // Make sure we save the result before closing
                    this.DialogResult = _viewModel.DialogResult;

                    // Only close if the operation was successful
                    if (_viewModel.DialogResult == true)
                    {
                        // Ensure all UI updates are processed
                        System.Windows.Application.Current.Dispatcher.Invoke(() => { });

                        // Delay closing briefly to allow UI to update
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                            this.Close();
                        }), DispatcherPriority.Normal);
                    }
                }
            };
        }

        public Drawer ClosedDrawer => _viewModel.Drawer;
    }
}