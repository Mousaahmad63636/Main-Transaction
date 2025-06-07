using QuickTechPOS.Helpers;
using QuickTechPOS.Services;
using System.Windows.Input;

namespace QuickTechPOS.ViewModels
{
    /// <summary>
    /// ViewModel for the main application view
    /// SIMPLIFIED: Removed print queue functionality for direct printing only
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;
        private readonly NavigationService _navigationService;
        private string _welcomeMessage;

        public MainViewModel(AuthenticationService authService, NavigationService navigationService)
        {
            _authService = authService ?? throw new System.ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new System.ArgumentNullException(nameof(navigationService));

            // Initialize commands
            LogoutCommand = new RelayCommand(param => Logout());

            // REMOVED: ViewPrintQueueCommand - no longer needed with direct printing

            // Set welcome message
            UpdateWelcomeMessage();

            Console.WriteLine("[MainViewModel] Initialized with simplified interface - print queue removed");
        }

        #region Properties

        /// <summary>
        /// Gets the welcome message displayed in the main view
        /// </summary>
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            private set => SetProperty(ref _welcomeMessage, value);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to logout the current user
        /// </summary>
        public ICommand LogoutCommand { get; }

        // REMOVED: ViewPrintQueueCommand - simplified for direct printing only

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the welcome message based on the current user
        /// </summary>
        private void UpdateWelcomeMessage()
        {
            if (_authService?.CurrentEmployee != null)
            {
                WelcomeMessage = $"Welcome, {_authService.CurrentEmployee.FullName}!";
                Console.WriteLine($"[MainViewModel] Welcome message set for: {_authService.CurrentEmployee.FullName}");
            }
            else
            {
                WelcomeMessage = "Welcome to QuickTech POS!";
                Console.WriteLine("[MainViewModel] Default welcome message set");
            }
        }

        /// <summary>
        /// Handles user logout
        /// </summary>
        private void Logout()
        {
            try
            {
                Console.WriteLine("[MainViewModel] Logout initiated");

                // Clear current employee
                if (_authService?.CurrentEmployee != null)
                {
                    Console.WriteLine($"[MainViewModel] Logging out user: {_authService.CurrentEmployee.FullName}");
                }

                // Navigate back to login
                _navigationService?.NavigateTo("LoginView");

                Console.WriteLine("[MainViewModel] Logout completed successfully");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[MainViewModel] Error during logout: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
            }
        }

        #endregion
    }
}