using Microsoft.Extensions.Configuration;
using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.Services;
using QuickTechPOS.ViewModels;
using QuickTechPOS.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuickTechPOS
{
    public partial class App : Application
    {
        private NavigationService _navigationService;
        private Frame _mainFrame;
        private MainWindow _mainWindow;
        private Customer _walkInCustomer;
        private TransactionViewModel _transactionViewModel;

        // REMOVED: Print queue manager - direct printing only

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // REMOVED: Print queue manager initialization - simplified printing

            // Initialize language settings first (loads saved language)
            LanguageManager.Initialize();

            // Create the main window and navigation frame
            _mainWindow = new MainWindow();

            // Apply the current flow direction to the main window
            LanguageManager.ApplyFlowDirectionToWindow(_mainWindow);

            _mainFrame = new Frame();
            _mainWindow.Content = _mainFrame;

            // Setup navigation service
            _navigationService = new NavigationService(_mainFrame);

            // Create appsettings.json if it doesn't exist
            EnsureAppSettingsExists();

            // Ensure walk-in customer exists before registering views
            var dbService = new DatabaseService();
            _walkInCustomer = await dbService.EnsureWalkInCustomerExistsAsync();

            // Register views
            RegisterViews();

            // Navigate to login view
            _navigationService.NavigateTo("LoginView");

            // Show the main window
            _mainWindow.Show();
        }

        private async Task CheckForOpenDrawerAsync(AuthenticationService authService)
        {
            if (authService.CurrentEmployee == null)
            {
                Console.WriteLine("[App] CheckForOpenDrawerAsync - No current employee, skipping drawer check");
                return;
            }

            try
            {
                string cashierId = authService.CurrentEmployee.EmployeeId.ToString();
                Console.WriteLine($"[App] Checking for open drawer for cashier ID: {cashierId}");

                // Use enhanced verification with retry logic
                bool drawerExists = await VerifyOpenDrawerWithRetryAsync(cashierId, 3, 1000);

                if (!drawerExists)
                {
                    Console.WriteLine("[App] No open drawer found, showing dialog to open drawer");

                    // Show the open drawer dialog
                    var viewModel = new OpenDrawerViewModel(authService);
                    var dialog = new OpenDrawerDialog(viewModel);

                    // Apply flow direction to dialog
                    LanguageManager.ApplyFlowDirectionToWindow(dialog);

                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialog.Topmost = true;
                    dialog.Owner = _mainWindow;

                    Console.WriteLine("[App] Opening drawer dialog...");

                    try
                    {
                        bool? result = dialog.ShowDialog();
                        Console.WriteLine($"[App] Dialog result: {result}");

                        if (result == true)
                        {
                            // Use a more robust verification with additional retries
                            Console.WriteLine("[App] Dialog returned success, verifying drawer was created...");
                            bool drawerVerified = await VerifyOpenDrawerWithRetryAsync(cashierId, 5, 1000);

                            if (drawerVerified)
                            {
                                Console.WriteLine("[App] Drawer opened successfully and verified");

                                // Refresh the transaction view if needed
                                if (_transactionViewModel != null)
                                {
                                    await _transactionViewModel.RefreshDrawerStatusAsync();
                                    Console.WriteLine("[App] Refreshed transaction view");
                                }

                                return;
                            }
                            else
                            {
                                Console.WriteLine("[App] ERROR: OpenDrawerDialog returned success but drawer not found");

                                // Try database diagnostics
                                try
                                {
                                    var drawerService = new DrawerService();
                                    bool connectionOk = await drawerService.TestDatabaseConnectionAsync();
                                    Console.WriteLine($"[App] Database connection test: {(connectionOk ? "Success" : "Failed")}");
                                }
                                catch (Exception dbEx)
                                {
                                    Console.WriteLine($"[App] Database diagnostic error: {dbEx.Message}");
                                }

                                MessageBox.Show(
                                    "The drawer creation reported success but verification failed. The system will try again.",
                                    "Drawer Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }
                        }
                        else if (result == false)
                        {
                            Console.WriteLine("[App] User cancelled drawer opening");
                        }
                        else
                        {
                            Console.WriteLine("[App] Dialog closed with null result");
                        }
                    }
                    catch (Exception dialogEx)
                    {
                        Console.WriteLine($"[App] Error showing or handling dialog: {dialogEx.Message}");
                        if (dialogEx.InnerException != null)
                            Console.WriteLine($"[App] Inner exception: {dialogEx.InnerException.Message}");
                    }

                    // Show message and retry or exit
                    Console.WriteLine("[App] Attempting retry process for opening drawer");
                    if (!await RetryOpenDrawerAsync(authService))
                    {
                        Console.WriteLine("[App] Retry process failed, shutting down application");
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    Console.WriteLine("[App] Existing open drawer found, continuing");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[App] Error in CheckForOpenDrawerAsync: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[App] Inner exception: {ex.InnerException.Message}");

                MessageBox.Show(
                    $"Error checking for open drawer: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task<bool> RetryOpenDrawerAsync(AuthenticationService authService)
        {
            Console.WriteLine("[App] RetryOpenDrawerAsync - Starting retry process");

            try
            {
                if (authService.CurrentEmployee == null)
                {
                    Console.WriteLine("[App] RetryOpenDrawerAsync - No current employee, cannot retry");
                    MessageBox.Show(
                        "No cashier is currently logged in. Please log in first.",
                        "Authentication Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return false;
                }

                string cashierId = authService.CurrentEmployee.EmployeeId.ToString();

                // First, check if somehow a drawer was opened but not detected
                Console.WriteLine("[App] RetryOpenDrawerAsync - Checking if drawer exists before showing dialog");
                var drawerService = new DrawerService();
                var existingDrawer = await drawerService.GetOpenDrawerAsync(cashierId);

                if (existingDrawer != null)
                {
                    Console.WriteLine($"[App] RetryOpenDrawerAsync - Found existing drawer: ID={existingDrawer.DrawerId}");

                    // Force database syncs
                    await drawerService.DrainPendingOperationsAsync();

                    if (_transactionViewModel != null)
                    {
                        await _transactionViewModel.RefreshDrawerStatusAsync();
                    }

                    MessageBox.Show(
                        "An open drawer has been found. You can now proceed.",
                        "Drawer Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return true;
                }

                // Show new dialog for retry
                Console.WriteLine("[App] RetryOpenDrawerAsync - Creating retry dialog");
                var viewModel = new OpenDrawerViewModel(authService);
                var dialog = new OpenDrawerDialog(viewModel);

                // Apply flow direction to dialog
                LanguageManager.ApplyFlowDirectionToWindow(dialog);

                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.Topmost = true;
                dialog.Owner = _mainWindow;

                // Show dialog with improved error handling
                bool? result;
                try
                {
                    Console.WriteLine("[App] RetryOpenDrawerAsync - Showing retry dialog");
                    result = dialog.ShowDialog();
                    Console.WriteLine($"[App] RetryOpenDrawerAsync - Dialog result: {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[App] RetryOpenDrawerAsync - Error showing dialog: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"[App] Inner exception: {ex.InnerException.Message}");

                    MessageBox.Show(
                        $"Error showing drawer dialog: {ex.Message}",
                        "Dialog Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return false;
                }

                if (result == true)
                {
                    Console.WriteLine("[App] RetryOpenDrawerAsync - Dialog returned success, verifying drawer");

                    // Enhanced verification with more retries and longer delays
                    bool drawerVerified = await VerifyOpenDrawerWithRetryAsync(cashierId, 5, 1000);

                    if (drawerVerified)
                    {
                        Console.WriteLine("[App] RetryOpenDrawerAsync - Verification successful");

                        if (_transactionViewModel != null)
                        {
                            await _transactionViewModel.RefreshDrawerStatusAsync();
                        }

                        return true;
                    }
                    else
                    {
                        Console.WriteLine("[App] RetryOpenDrawerAsync - Verification failed despite success dialog");

                        // Force database sync
                        await drawerService.DrainPendingOperationsAsync();

                        // One final check
                        var finalCheck = await drawerService.GetOpenDrawerAsync(cashierId);
                        if (finalCheck != null)
                        {
                            Console.WriteLine($"[App] RetryOpenDrawerAsync - Final check found drawer: ID={finalCheck.DrawerId}");

                            if (_transactionViewModel != null)
                            {
                                await _transactionViewModel.RefreshDrawerStatusAsync();
                            }

                            return true;
                        }

                        MessageBox.Show(
                            "The drawer creation process completed but verification failed. Please contact your system administrator.",
                            "Verification Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("[App] RetryOpenDrawerAsync - User cancelled drawer dialog");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[App] RetryOpenDrawerAsync - Unhandled exception: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[App] Inner exception: {ex.InnerException.Message}");

                MessageBox.Show(
                    $"Error during retry process: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }

        private async Task<bool> VerifyOpenDrawerWithRetryAsync(string cashierId, int maxRetries = 3, int delayMs = 800)
        {
            Console.WriteLine($"[App] VerifyOpenDrawerWithRetryAsync - Starting verification for cashier ID {cashierId} with {maxRetries} retries");

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                // Use a fresh database context each time to ensure we're not using cached data
                try
                {
                    Console.WriteLine($"[App] Verification attempt {attempt + 1} of {maxRetries + 1}");

                    // Force-create a new service to avoid any stale context
                    var drawerService = new DrawerService();

                    // Attempt to get the drawer
                    var drawer = await drawerService.GetOpenDrawerAsync(cashierId);

                    if (drawer != null)
                    {
                        Console.WriteLine($"[App] Open drawer found: ID={drawer.DrawerId}, Status={drawer.Status}, CashierId={drawer.CashierId}");

                        // Additional verification to ensure it's really our drawer
                        if (drawer.Status == "Open" && drawer.CashierId == cashierId)
                        {
                            Console.WriteLine("[App] Drawer verification successful");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"[App] Found drawer but status/owner mismatch: Status={drawer.Status}, CashierId={drawer.CashierId}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[App] No open drawer found for this cashier");
                    }

                    // If we have more retries, wait before trying again
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"[App] Waiting {delayMs}ms before next verification attempt");
                        await Task.Delay(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[App] Error during verification attempt {attempt + 1}: {ex.Message}");

                    if (ex.InnerException != null)
                        Console.WriteLine($"[App] Inner exception: {ex.InnerException.Message}");

                    // If we have more retries, wait before trying again
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs);
                    }
                }
            }

            Console.WriteLine($"[App] Drawer verification failed after {maxRetries + 1} attempts");
            return false;
        }

        private void RegisterViews()
        {
            // Create a shared authentication service
            var authService = new AuthenticationService();

            // Register the login view
            _navigationService.RegisterView("LoginView", () =>
                new LoginView(new LoginViewModel(_navigationService, authService)));

            // Register the main view with transaction view
            _navigationService.RegisterView("MainView", () =>
            {
                var mainViewModel = new MainViewModel(authService, _navigationService);
                var mainView = new MainView(mainViewModel);

                // Add transaction view to the main view content
                _transactionViewModel = new TransactionViewModel(authService, _walkInCustomer);
                var transactionView = new TransactionView(_transactionViewModel);

                // Set the transaction view as the main content
                mainView.SetContent(transactionView);

                // Check for open drawer
                CheckForOpenDrawerAsync(authService).ConfigureAwait(false);

                return mainView;
            });
        }

        private void EnsureAppSettingsExists()
        {
            string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            if (!File.Exists(appSettingsPath))
            {
                string json = @"{
                    ""ConnectionStrings"": {
                        ""DefaultConnection"": ""Server=.\\posserver;Database=QuickTechSystem;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True""
                    },
                    ""ImageStorage"": {
                        ""ProductImagesPath"": ""ProductImages""
                    }
                }";

                File.WriteAllText(appSettingsPath, json);
            }
        }
    }
}