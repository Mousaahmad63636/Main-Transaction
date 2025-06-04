// File: QuickTechPOS/ViewModels/CashInViewModel.cs

using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickTechPOS.ViewModels
{
    /// <summary>
    /// View model for the cash in dialog
    /// </summary>
    public class CashInViewModel : BaseViewModel
    {
        private readonly DrawerService _drawerService;
        private readonly Drawer _drawer;

        private decimal _cashInAmount;
        private string _notes;
        private string _errorMessage;
        private bool _isProcessing;

        /// <summary>
        /// Gets or sets the cash in amount
        /// </summary>
        public decimal CashInAmount
        {
            get => _cashInAmount;
            set => SetProperty(ref _cashInAmount, value);
        }

        /// <summary>
        /// Gets or sets the notes for this cash in operation
        /// </summary>
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
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
        /// Gets the current drawer
        /// </summary>
        public Drawer Drawer => _drawer;

        /// <summary>
        /// Command to execute the cash in operation
        /// </summary>
        public ICommand ExecuteCashInCommand { get; }

        /// <summary>
        /// Command to cancel the operation
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Initializes a new instance of the cash in view model
        /// </summary>
        /// <param name="drawer">The drawer to add cash to</param>
        public CashInViewModel(Drawer drawer)
        {
            _drawerService = new DrawerService();
            _drawer = drawer ?? throw new ArgumentNullException(nameof(drawer));

            CashInAmount = 0;
            Notes = string.Empty;
            ErrorMessage = string.Empty;
            IsProcessing = false;

            ExecuteCashInCommand = new RelayCommand(async param => await ExecuteCashInAsync(), CanExecuteCashIn);
            CancelCommand = new RelayCommand(param => Cancel());
        }

        /// <summary>
        /// Determines if the cash in operation can be executed
        /// </summary>
        private bool CanExecuteCashIn(object parameter)
        {
            return !IsProcessing
                && CashInAmount > 0
                && !string.IsNullOrWhiteSpace(Notes);
        }

        /// <summary>
        /// Executes the cash in operation
        /// </summary>
        private async Task ExecuteCashInAsync()
        {
            try
            {
                IsProcessing = true;
                ErrorMessage = string.Empty;

                if (CashInAmount <= 0)
                {
                    ErrorMessage = "Cash in amount must be greater than zero.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Notes))
                {
                    ErrorMessage = "Please provide a reason for this cash in operation.";
                    return;
                }

                // Log before cash in attempt
                Console.WriteLine($"[CashInViewModel] Executing cash in: Amount=${CashInAmount:F2}, DrawerID={_drawer.DrawerId}");
                Console.WriteLine($"Drawer before cash in: Balance=${_drawer.CurrentBalance:F2}, CashIn=${_drawer.CashIn:F2}");

                // Use the drawer service to perform the cash in operation
                var updatedDrawer = await _drawerService.PerformCashInAsync(_drawer.DrawerId, CashInAmount, Notes);

                if (updatedDrawer != null)
                {
                    // Copy updated values to our drawer object
                    _drawer.CashIn = updatedDrawer.CashIn;
                    _drawer.CurrentBalance = updatedDrawer.CurrentBalance;
                    _drawer.NetCashFlow = updatedDrawer.NetCashFlow;
                    _drawer.Notes = updatedDrawer.Notes;
                    _drawer.LastUpdated = updatedDrawer.LastUpdated;

                    Console.WriteLine($"[CashInViewModel] Cash in operation completed successfully.");
                    Console.WriteLine($"Drawer after: Balance=${_drawer.CurrentBalance:F2}, CashIn=${_drawer.CashIn:F2}");

                    DialogResult = true;
                    OnPropertyChanged(nameof(DialogResult));
                }
                else
                {
                    Console.WriteLine("[CashInViewModel] Cash in operation failed: drawer service returned null");
                    ErrorMessage = "Cash in operation failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CashInViewModel] Cash in operation failed with exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                ErrorMessage = $"Error performing cash in: {ex.Message}";
                DialogResult = false;
                OnPropertyChanged(nameof(DialogResult));
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Cancels the operation
        /// </summary>
        private void Cancel()
        {
            DialogResult = false;
            OnPropertyChanged(nameof(DialogResult));
        }

        /// <summary>
        /// Gets or sets the dialog result
        /// </summary>
        public bool? DialogResult { get; set; }
    }
}