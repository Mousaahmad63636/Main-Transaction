using Microsoft.Extensions.Configuration;
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

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create the main window and navigation frame
            _mainWindow = new MainWindow();
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
                return;

            try
            {
                string cashierId = authService.CurrentEmployee.EmployeeId.ToString();
                var drawerService = new DrawerService();
                var openDrawer = await drawerService.GetOpenDrawerAsync(cashierId);

                if (openDrawer == null)
                {
                    // Show the open drawer dialog
                    var viewModel = new OpenDrawerViewModel(authService);
                    var dialog = new OpenDrawerDialog(viewModel);
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialog.Topmost = true;
                    dialog.Owner = _mainWindow;

                    bool? result = dialog.ShowDialog();

                    if (result != true)
                    {
                        // If user cancels opening a drawer, show message
                        MessageBox.Show("You must open a drawer to continue using the system.",
                            "Drawer Required", MessageBoxButton.OK, MessageBoxImage.Warning);

                        // Try again or exit
                        if (!await RetryOpenDrawerAsync(authService))
                        {
                            Application.Current.Shutdown();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for open drawer: {ex.Message}");
                MessageBox.Show($"Error checking for open drawer: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> RetryOpenDrawerAsync(AuthenticationService authService)
        {
            var viewModel = new OpenDrawerViewModel(authService);
            var dialog = new OpenDrawerDialog(viewModel);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.Topmost = true;
            dialog.Owner = _mainWindow;
            return dialog.ShowDialog() == true;
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
                var transactionViewModel = new TransactionViewModel(authService, _walkInCustomer);
                var transactionView = new TransactionView(transactionViewModel);

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