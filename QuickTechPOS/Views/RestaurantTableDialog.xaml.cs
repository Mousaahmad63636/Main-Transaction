// File: QuickTechPOS/Views/RestaurantTableDialog.xaml.cs

using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Enterprise-grade restaurant table selection dialog with advanced user interaction patterns
    /// Implements sophisticated dialog lifecycle management and optimal user experience design
    /// </summary>
    public partial class RestaurantTableDialog : Window
    {
        #region Private Fields

        private readonly RestaurantTableDialogViewModel _viewModel;
        private RestaurantTable _selectedTable;
        private bool _dialogResult;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the table selected by the user, null if no selection was made
        /// </summary>
        public RestaurantTable SelectedTable => _selectedTable;

        /// <summary>
        /// Gets whether a valid table selection was made before closing the dialog
        /// </summary>
        public bool HasValidSelection => _selectedTable != null && _dialogResult;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the RestaurantTableDialog with comprehensive UI setup and event management
        /// </summary>
        public RestaurantTableDialog()
        {
            try
            {
                Console.WriteLine("[RestaurantTableDialog] Initializing restaurant table selection dialog...");

                InitializeComponent();

                // Initialize view model with dependency injection pattern
                _viewModel = new RestaurantTableDialogViewModel();
                DataContext = _viewModel;

                // Subscribe to view model events for coordinated dialog behavior
                _viewModel.TableSelected += OnTableSelected;
                _viewModel.DialogClose += OnDialogClose;

                // Apply current localization flow direction for RTL language support
                this.FlowDirection = LanguageManager.CurrentFlowDirection;

                // Configure window behavior for optimal user experience
                ConfigureWindowBehavior();

                // Initialize keyboard and focus management
                InitializeInputHandling();

                Console.WriteLine("[RestaurantTableDialog] Dialog initialization completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error during initialization: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[RestaurantTableDialog] Inner exception: {ex.InnerException.Message}");
                }

                // Ensure dialog can still be displayed even if initialization partially fails
                MessageBox.Show($"Error initializing table dialog: {ex.Message}",
                               "Initialization Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Configures advanced window behavior and visual properties for professional appearance
        /// </summary>
        private void ConfigureWindowBehavior()
        {
            try
            {
                // Set window state and positioning for optimal user interaction
                this.WindowState = WindowState.Normal;
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                // Configure window chrome and behavior
                this.ShowInTaskbar = false;
                this.ResizeMode = ResizeMode.CanResize;
                this.MinWidth = 800;
                this.MinHeight = 600;

                // Set professional title with context information
                this.Title = "Select Restaurant Table - QuickTech POS";

                // Configure closing behavior
                this.Closing += OnWindowClosing;

                Console.WriteLine("[RestaurantTableDialog] Window behavior configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error configuring window behavior: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes advanced keyboard shortcuts and input handling for power user workflows
        /// </summary>
        private void InitializeInputHandling()
        {
            try
            {
                // Configure keyboard shortcuts for enhanced productivity
                var escapeBinding = new KeyBinding(
                    new RelayCommand(param => CancelAndClose()),
                    Key.Escape,
                    ModifierKeys.None);
                this.InputBindings.Add(escapeBinding);

                var enterBinding = new KeyBinding(
                    new RelayCommand(param => SelectCurrentTable()),
                    Key.Enter,
                    ModifierKeys.None);
                this.InputBindings.Add(enterBinding);

                var f5Binding = new KeyBinding(
                    new RelayCommand(param => RefreshTables()),
                    Key.F5,
                    ModifierKeys.None);
                this.InputBindings.Add(f5Binding);

                // Set initial focus to search box for immediate typing
                this.Loaded += (s, e) =>
                {
                    if (SearchTextBox != null)
                    {
                        SearchTextBox.Focus();
                    }
                };

                Console.WriteLine("[RestaurantTableDialog] Input handling initialized with keyboard shortcuts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error initializing input handling: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles table selection from the view model with validation and user feedback
        /// </summary>
        /// <param name="sender">Event source (view model)</param>
        /// <param name="selectedTable">The table selected by the user</param>
        private void OnTableSelected(object sender, RestaurantTable selectedTable)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableDialog] Table selection event received: {selectedTable?.DisplayName ?? "null"}");

                if (selectedTable == null)
                {
                    Console.WriteLine("[RestaurantTableDialog] Invalid table selection received");
                    MessageBox.Show("Invalid table selection. Please try again.",
                                   "Selection Error",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                if (!selectedTable.IsActive)
                {
                    Console.WriteLine($"[RestaurantTableDialog] Inactive table selected: {selectedTable.DisplayName}");
                    MessageBox.Show($"Table {selectedTable.DisplayName} is not active and cannot be selected.",
                                   "Invalid Selection",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                    return;
                }

                // Store selection and prepare for dialog closure
                _selectedTable = selectedTable;
                _dialogResult = true;

                Console.WriteLine($"[RestaurantTableDialog] Table {selectedTable.DisplayName} successfully selected");

                // Close dialog with success result
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error handling table selection: {ex.Message}");
                MessageBox.Show($"Error processing table selection: {ex.Message}",
                               "Selection Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles dialog close requests from the view model with state validation
        /// </summary>
        /// <param name="sender">Event source (view model)</param>
        /// <param name="success">Whether the dialog should close with success</param>
        private void OnDialogClose(object sender, bool success)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableDialog] Dialog close event received with success: {success}");

                _dialogResult = success;

                // Set appropriate dialog result
                this.DialogResult = success;
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error handling dialog close: {ex.Message}");

                // Ensure dialog can still be closed even on error
                try
                {
                    this.DialogResult = false;
                    this.Close();
                }
                catch
                {
                    // Force close if normal close fails
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => this.Hide()));
                }
            }
        }

        /// <summary>
        /// Handles window closing event with cleanup and validation
        /// </summary>
        /// <param name="sender">Window instance</param>
        /// <param name="e">Closing event arguments</param>
        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            try
            {
                Console.WriteLine("[RestaurantTableDialog] Window closing initiated");

                // Perform cleanup operations
                CleanupResources();

                // If no explicit result was set, treat as cancellation
                if (!_dialogResult && _selectedTable == null)
                {
                    Console.WriteLine("[RestaurantTableDialog] Dialog closing without selection - treating as cancellation");
                    this.DialogResult = false;
                }

                Console.WriteLine($"[RestaurantTableDialog] Window closing completed with result: {this.DialogResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error during window closing: {ex.Message}");
                // Don't cancel closing due to cleanup errors
            }
        }

        /// <summary>
        /// Performs comprehensive resource cleanup and event unsubscription
        /// </summary>
        private void CleanupResources()
        {
            try
            {
                Console.WriteLine("[RestaurantTableDialog] Starting resource cleanup...");

                // Unsubscribe from view model events to prevent memory leaks
                if (_viewModel != null)
                {
                    _viewModel.TableSelected -= OnTableSelected;
                    _viewModel.DialogClose -= OnDialogClose;

                    // Dispose view model if it implements IDisposable
                    if (_viewModel is IDisposable disposableViewModel)
                    {
                        disposableViewModel.Dispose();
                    }
                }

                // Clear event handlers
                this.Closing -= OnWindowClosing;

                Console.WriteLine("[RestaurantTableDialog] Resource cleanup completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error during resource cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancels table selection and closes the dialog
        /// </summary>
        private void CancelAndClose()
        {
            try
            {
                Console.WriteLine("[RestaurantTableDialog] Cancel operation initiated");

                _selectedTable = null;
                _dialogResult = false;
                this.DialogResult = false;
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error during cancel operation: {ex.Message}");
            }
        }

        /// <summary>
        /// Selects the currently highlighted table (for keyboard shortcuts)
        /// </summary>
        private void SelectCurrentTable()
        {
            try
            {
                if (_viewModel?.SelectedTable != null)
                {
                    Console.WriteLine($"[RestaurantTableDialog] Keyboard selection of table: {_viewModel.SelectedTable.DisplayName}");

                    // Trigger selection through view model command
                    if (_viewModel.SelectTableCommand?.CanExecute(_viewModel.SelectedTable) == true)
                    {
                        _viewModel.SelectTableCommand.Execute(_viewModel.SelectedTable);
                    }
                }
                else
                {
                    Console.WriteLine("[RestaurantTableDialog] No table selected for keyboard selection");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error during keyboard table selection: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes the table list (for F5 shortcut)
        /// </summary>
        private void RefreshTables()
        {
            try
            {
                Console.WriteLine("[RestaurantTableDialog] F5 refresh triggered");

                if (_viewModel?.RefreshTablesCommand?.CanExecute(null) == true)
                {
                    _viewModel.RefreshTablesCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error during F5 refresh: {ex.Message}");
            }
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Creates and displays a new restaurant table selection dialog with optimal configuration
        /// </summary>
        /// <param name="owner">Parent window for modal display</param>
        /// <returns>Tuple containing success status and selected table</returns>
        public static (bool Success, RestaurantTable SelectedTable) ShowTableSelectionDialog(Window owner = null)
        {
            try
            {
                Console.WriteLine("[RestaurantTableDialog] Creating table selection dialog...");

                var dialog = new RestaurantTableDialog();

                if (owner != null)
                {
                    dialog.Owner = owner;
                }
                else if (Application.Current?.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                var result = dialog.ShowDialog();
                bool success = result == true && dialog.HasValidSelection;

                Console.WriteLine($"[RestaurantTableDialog] Dialog completed with success: {success}, " +
                                 $"Selected table: {dialog.SelectedTable?.DisplayName ?? "none"}");

                return (success, dialog.SelectedTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error showing table selection dialog: {ex.Message}");

                MessageBox.Show($"Error displaying table selection dialog: {ex.Message}",
                               "Dialog Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);

                return (false, null);
            }
        }

        /// <summary>
        /// Creates a table selection dialog with pre-filtered results
        /// </summary>
        /// <param name="statusFilter">Initial status filter to apply</param>
        /// <param name="owner">Parent window for modal display</param>
        /// <returns>Tuple containing success status and selected table</returns>
        public static (bool Success, RestaurantTable SelectedTable) ShowTableSelectionDialog(
            string statusFilter,
            Window owner = null)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableDialog] Creating filtered table selection dialog with status: {statusFilter}");

                var dialog = new RestaurantTableDialog();

                if (owner != null)
                {
                    dialog.Owner = owner;
                }
                else if (Application.Current?.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                // Apply initial filter if provided
                if (!string.IsNullOrWhiteSpace(statusFilter) && dialog._viewModel != null)
                {
                    dialog._viewModel.SelectedStatusFilter = statusFilter;
                }

                var result = dialog.ShowDialog();
                bool success = result == true && dialog.HasValidSelection;

                Console.WriteLine($"[RestaurantTableDialog] Filtered dialog completed with success: {success}");

                return (success, dialog.SelectedTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableDialog] Error showing filtered table selection dialog: {ex.Message}");

                MessageBox.Show($"Error displaying table selection dialog: {ex.Message}",
                               "Dialog Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);

                return (false, null);
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Ensures proper resource disposal when dialog is garbage collected
        /// </summary>
        ~RestaurantTableDialog()
        {
            CleanupResources();
        }

        #endregion
    }
}