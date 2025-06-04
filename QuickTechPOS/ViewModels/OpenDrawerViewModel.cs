// File: QuickTechPOS/ViewModels/OpenDrawerViewModel.cs

using QuickTechPOS.Helpers;
using QuickTechPOS.Services;
using QuickTechPOS.ViewModels;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

public class OpenDrawerViewModel : BaseViewModel
{
    private readonly DrawerService _drawerService;
    private readonly AuthenticationService _authService;

    private decimal _openingBalance;
    private string _errorMessage;
    private bool _isProcessing;
    private string _notes;

    public decimal OpeningBalance
    {
        get => _openingBalance;
        set => SetProperty(ref _openingBalance, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set => SetProperty(ref _isProcessing, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public ICommand OpenDrawerCommand { get; }
    public ICommand CancelCommand { get; }

    public OpenDrawerViewModel(AuthenticationService authService)
    {
        Console.WriteLine("[OpenDrawerViewModel] Initializing");

        _drawerService = new DrawerService();
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        if (_authService.CurrentEmployee == null)
        {
            Console.WriteLine("[OpenDrawerViewModel] WARNING: CurrentEmployee is null in constructor");
        }
        else
        {
            Console.WriteLine($"[OpenDrawerViewModel] Current employee: {_authService.CurrentEmployee.EmployeeId} ({_authService.CurrentEmployee.FullName})");
        }

        OpeningBalance = 0;
        ErrorMessage = string.Empty;
        IsProcessing = false;
        Notes = string.Empty;

        OpenDrawerCommand = new RelayCommand(async param => await OpenDrawerAsync(), CanOpenDrawer);
        CancelCommand = new RelayCommand(param => Cancel());

        Console.WriteLine("[OpenDrawerViewModel] Initialized successfully");
    }

    private bool CanOpenDrawer(object parameter)
    {
        bool result = !IsProcessing && OpeningBalance >= 0;
        Console.WriteLine($"[OpenDrawerViewModel] CanOpenDrawer called, result: {result}, IsProcessing: {IsProcessing}, OpeningBalance: {OpeningBalance}");
        return result;
    }

    private async Task OpenDrawerAsync()
    {
        try
        {
            Console.WriteLine("[OpenDrawerViewModel] OpenDrawerAsync started");
            IsProcessing = true;
            ErrorMessage = string.Empty;

            if (OpeningBalance < 0)
            {
                ErrorMessage = "Opening balance cannot be negative.";
                Console.WriteLine("[OpenDrawerViewModel] Validation failed: Opening balance is negative");
                IsProcessing = false;
                return;
            }

            var employee = _authService.CurrentEmployee;
            if (employee == null)
            {
                ErrorMessage = "No cashier is logged in.";
                Console.WriteLine("[OpenDrawerViewModel] Error: No cashier is logged in");
                IsProcessing = false;
                return;
            }

            string cashierId = employee.EmployeeId.ToString();
            string cashierName = employee.FullName;

            Console.WriteLine($"[OpenDrawerViewModel] Attempting to open drawer for cashier {cashierId} ({cashierName})");
            Console.WriteLine($"[OpenDrawerViewModel] Opening balance: ${OpeningBalance:F2}, Notes: {Notes}");

            // Check if there's already an open drawer
            var existingDrawer = await _drawerService.GetOpenDrawerAsync(cashierId);
            if (existingDrawer != null)
            {
                Console.WriteLine($"[OpenDrawerViewModel] Found existing open drawer: ID={existingDrawer.DrawerId}");

                // If there's already an open drawer, consider this a success
                DialogResult = true;
                OnPropertyChanged(nameof(DialogResult));
                return;
            }

            // Try to open a new drawer
            var drawer = await _drawerService.OpenDrawerAsync(cashierId, cashierName, OpeningBalance, Notes);

            if (drawer != null)
            {
                Console.WriteLine($"[OpenDrawerViewModel] Drawer opened successfully: ID={drawer.DrawerId}");

                // Double-check that the drawer is properly saved and accessible
                var verificationDrawer = await _drawerService.GetDrawerByIdAsync(drawer.DrawerId);

                if (verificationDrawer != null)
                {
                    Console.WriteLine($"[OpenDrawerViewModel] Verification successful: Drawer ID={verificationDrawer.DrawerId}, Status={verificationDrawer.Status}");
                    DialogResult = true;
                }
                else
                {
                    Console.WriteLine("[OpenDrawerViewModel] Verification failed: Could not retrieve drawer after creation");
                    ErrorMessage = "Drawer was created but could not be verified. Please try again.";
                    DialogResult = false;
                }
            }
            else
            {
                Console.WriteLine("[OpenDrawerViewModel] Drawer open operation failed: No drawer returned");
                ErrorMessage = "Failed to open drawer. Please try again.";
                DialogResult = false;
            }

            OnPropertyChanged(nameof(DialogResult));
        }
        catch (Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Error opening drawer: {ex.Message}");

            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner exception: {ex.InnerException.Message}");
            }

            Console.WriteLine($"[OpenDrawerViewModel] Exception in OpenDrawerAsync: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[OpenDrawerViewModel] Inner exception: {ex.InnerException.Message}");
            }
            Console.WriteLine($"[OpenDrawerViewModel] Stack trace: {ex.StackTrace}");

            ErrorMessage = sb.ToString();
            DialogResult = false;
            OnPropertyChanged(nameof(DialogResult));
        }
        finally
        {
            IsProcessing = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void Cancel()
    {
        Console.WriteLine("[OpenDrawerViewModel] Cancel method called");
        DialogResult = false;
        OnPropertyChanged(nameof(DialogResult));
    }

    public bool? DialogResult { get; private set; }
}