using QuickTechPOS.Helpers;
using QuickTechPOS.Services;
using QuickTechPOS.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace QuickTechPOS.ViewModels
{
    /// <summary>
    /// View model for the main application screen
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;
        private readonly NavigationService _navigationService;

        private string _welcomeMessage;
        private readonly PrintQueueManager _printQueueManager;

        /// <summary>
        /// Gets or sets the welcome message
        /// </summary>
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        /// <summary>
        /// Command to log out of the application
        /// </summary>
        public ICommand LogoutCommand { get; }
        public ICommand ViewPrintQueueCommand { get; }
        /// <summary>
        /// Initializes a new instance of the main view model
        /// </summary>
        /// <param name="authService">Service for authentication</param>
        /// <param name="navigationService">Service for navigating between views</param>
        public MainViewModel(AuthenticationService authService, NavigationService navigationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _printQueueManager = (Application.Current as App)?.PrintQueueManager;

            // Set welcome message based on current user
            WelcomeMessage = $"Welcome, {_authService.CurrentEmployee?.FullName ?? _authService.CurrentEmployee?.Username ?? "User"}!";

            // Initialize commands
            LogoutCommand = new RelayCommand(Logout);
            ViewPrintQueueCommand = new RelayCommand(param => ViewPrintQueue(), param => _printQueueManager != null);
        }

        private void ViewPrintQueue()
        {
            if (_printQueueManager == null)
            {
                MessageBox.Show("Print queue manager is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var dialog = new PrintJobStatusDialog(_printQueueManager);
                dialog.Owner = Application.Current.MainWindow;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening print queue dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Logs out the current user and returns to the login view
        /// </summary>
        private void Logout(object parameter)
        {
            _authService.Logout();
            _navigationService.NavigateTo("LoginView");
        }
    }
}