// File: QuickTechPOS/Views/PrintJobStatusDialog.xaml.cs

using QuickTechPOS.Services;
using QuickTechPOS.ViewModels;
using System;
using System.Windows;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Interaction logic for PrintJobStatusDialog.xaml
    /// </summary>
    public partial class PrintJobStatusDialog : Window
    {
        private readonly PrintJobStatusViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the print job status dialog
        /// </summary>
        /// <param name="printQueueManager">The print queue manager</param>
        public PrintJobStatusDialog(PrintQueueManager printQueueManager)
        {
            InitializeComponent();

            _viewModel = new PrintJobStatusViewModel(printQueueManager);
            DataContext = _viewModel;

            _viewModel.RequestClose += ViewModel_RequestClose;
        }

        /// <summary>
        /// Handles the RequestClose event from the view model
        /// </summary>
        private void ViewModel_RequestClose(object sender, bool e)
        {
            DialogResult = e;
            Close();
        }
    }
}