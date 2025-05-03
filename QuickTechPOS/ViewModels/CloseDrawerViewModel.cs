using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickTechPOS.ViewModels
{
    /// <summary>
    /// View model for the close drawer dialog
    /// </summary>
    public class CloseDrawerViewModel : BaseViewModel
    {
        private readonly DrawerService _drawerService;

        private Drawer _drawer;
        private decimal _closingBalance;
        private decimal _calculatedDifference;
        private string _errorMessage;
        private bool _isProcessing;
        private string _closingNotes;
        private bool _printReportAfterClosing = true;

        /// <summary>
        /// Gets the current drawer
        /// </summary>
        public Drawer Drawer
        {
            get => _drawer;
            private set => SetProperty(ref _drawer, value);
        }

        /// <summary>
        /// Gets or sets the closing balance
        /// </summary>
        public decimal ClosingBalance
        {
            get => _closingBalance;
            set
            {
                if (SetProperty(ref _closingBalance, value))
                {
                    CalculatedDifference = ClosingBalance - (Drawer?.CurrentBalance ?? 0);
                }
            }
        }
        /// <summary>
        /// Gets or sets whether to print the drawer report after closing
        /// </summary>
        public bool PrintReportAfterClosing
        {
            get => _printReportAfterClosing;
            set => SetProperty(ref _printReportAfterClosing, value);
        }
        /// <summary>
        /// Gets the calculated difference between the system balance and the entered closing balance
        /// </summary>
        public decimal CalculatedDifference
        {
            get => _calculatedDifference;
            private set => SetProperty(ref _calculatedDifference, value);
        }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Gets or sets whether the view is in a processing state
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        /// <summary>
        /// Gets or sets the closing notes
        /// </summary>
        public string ClosingNotes
        {
            get => _closingNotes;
            set => SetProperty(ref _closingNotes, value);
        }

        /// <summary>
        /// Command to close the drawer
        /// </summary>
        public ICommand CloseDrawerCommand { get; }

        /// <summary>
        /// Command to cancel the operation
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Initializes a new instance of the close drawer view model
        /// </summary>
        /// <param name="drawer">The drawer to close</param>
        public CloseDrawerViewModel(Drawer drawer)
        {
            _drawerService = new DrawerService();
            Drawer = drawer ?? throw new ArgumentNullException(nameof(drawer));

            // Initialize with the system's current balance
            ClosingBalance = drawer.CurrentBalance;
            CalculatedDifference = 0;
            ErrorMessage = string.Empty;
            IsProcessing = false;
            ClosingNotes = string.Empty;

            CloseDrawerCommand = new RelayCommand(async param => await CloseDrawerAsync(), CanCloseDrawer);
            CancelCommand = new RelayCommand(param => Cancel());
        }

        /// <summary>
        /// Determines if the drawer can be closed
        /// </summary>
        private bool CanCloseDrawer(object parameter)
        {
            return !IsProcessing && ClosingBalance >= 0 && Drawer != null;
        }

        /// <summary>
        /// Closes the drawer with the specified closing balance
        /// </summary>
        private async Task CloseDrawerAsync()
        {
            try
            {
                IsProcessing = true;
                ErrorMessage = string.Empty;

                if (ClosingBalance < 0)
                {
                    ErrorMessage = "Closing balance cannot be negative.";
                    IsProcessing = false;
                    return;
                }

                Console.WriteLine($"Attempting to close drawer #{_drawer.DrawerId} with balance ${ClosingBalance:F2}");

                // Close the drawer
                var updatedDrawer = await _drawerService.CloseDrawerAsync(_drawer.DrawerId, ClosingBalance, ClosingNotes);

                if (updatedDrawer != null)
                {
                    // Update our local drawer object with the updated values from the database
                    Drawer = updatedDrawer;
                    Console.WriteLine($"Drawer #{_drawer.DrawerId} closed successfully. Status: {Drawer.Status}");

                    // Print the drawer report if requested
                    if (PrintReportAfterClosing)
                    {
                        try
                        {
                            Console.WriteLine("Printing drawer report...");
                            var receiptPrinterService = new ReceiptPrinterService();
                            string printResult = await receiptPrinterService.PrintDrawerReportAsync(updatedDrawer);
                            Console.WriteLine($"Print result: {printResult}");
                        }
                        catch (Exception printEx)
                        {
                            Console.WriteLine($"Error printing drawer report: {printEx.Message}");
                            // Don't fail the closing operation if printing fails
                        }
                    }

                    // Set DialogResult to true to indicate success and close the dialog
                    DialogResult = true;
                    OnPropertyChanged(nameof(DialogResult));

                    // Log success
                    Console.WriteLine("Close drawer operation completed successfully");
                }
                else
                {
                    ErrorMessage = "Failed to close drawer. Please try again.";
                    Console.WriteLine("Close drawer operation failed: drawer service returned null");
                    DialogResult = false;
                    OnPropertyChanged(nameof(DialogResult));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error closing drawer: {ex.Message}";
                Console.WriteLine($"Exception in CloseDrawerAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                DialogResult = false;
                OnPropertyChanged(nameof(DialogResult));
            }
            finally
            {
                IsProcessing = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
        /// <summary>
        /// Cancels the operation
        /// </summary>
        private void Cancel()
        {
            DialogResult = false;
        }

        /// <summary>
        /// Gets or sets the dialog result
        /// </summary>
        public bool? DialogResult { get; set; }
    }
}