using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickTechPOS.ViewModels
{
    /// <summary>
    /// View model for the open drawer dialog
    /// </summary>
    public class OpenDrawerViewModel : BaseViewModel
    {
        private readonly DrawerService _drawerService;
        private readonly AuthenticationService _authService;

        private decimal _openingBalance;
        private string _errorMessage;
        private bool _isProcessing;
        private string _notes;

        /// <summary>
        /// Gets or sets the opening balance
        /// </summary>
        public decimal OpeningBalance
        {
            get => _openingBalance;
            set => SetProperty(ref _openingBalance, value);
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
        /// Gets or sets the notes for this drawer session
        /// </summary>
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        /// <summary>
        /// Command to open the drawer
        /// </summary>
        public ICommand OpenDrawerCommand { get; }

        /// <summary>
        /// Command to cancel the operation
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Initializes a new instance of the open drawer view model
        /// </summary>
        /// <param name="authService">Authentication service</param>
        public OpenDrawerViewModel(AuthenticationService authService)
        {
            _drawerService = new DrawerService();
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            OpeningBalance = 0;
            ErrorMessage = string.Empty;
            IsProcessing = false;
            Notes = string.Empty;

            OpenDrawerCommand = new RelayCommand(async param => await OpenDrawerAsync(), CanOpenDrawer);
            CancelCommand = new RelayCommand(param => Cancel());
        }

        /// <summary>
        /// Determines if the drawer can be opened
        /// </summary>
        private bool CanOpenDrawer(object parameter)
        {
            return !IsProcessing && OpeningBalance >= 0;
        }

        /// <summary>
        /// Opens the drawer with the specified opening balance
        /// </summary>
        private async Task OpenDrawerAsync()
        {
            try
            {
                IsProcessing = true;
                ErrorMessage = string.Empty;

                if (OpeningBalance < 0)
                {
                    ErrorMessage = "Opening balance cannot be negative.";
                    return;
                }

                var employee = _authService.CurrentEmployee;
                if (employee == null)
                {
                    ErrorMessage = "No cashier is logged in.";
                    return;
                }

                string cashierId = employee.EmployeeId.ToString();
                string cashierName = employee.FullName;

                // Open the drawer
                var drawer = await _drawerService.OpenDrawerAsync(cashierId, cashierName, OpeningBalance, Notes);

                // Signal success to the view
                DialogResult = true;
                OnPropertyChanged(nameof(DialogResult));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error opening drawer: {ex.Message}";
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
        }

        /// <summary>
        /// Gets or sets the dialog result
        /// </summary>
        public bool? DialogResult { get; set; }
    }
}