using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.Services;
using QuickTechPOS.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Enhanced TransactionView with category-based product filtering for improved POS workflow efficiency
    /// Implements modern category dropdown selection replacing traditional name-based search interface
    /// </summary>
    public partial class TransactionView : UserControl
    {
        #region Private Fields

        private readonly TransactionViewModel _viewModel;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the TransactionView with enhanced category-based product filtering functionality
        /// </summary>
        /// <param name="viewModel">The transaction view model containing business logic and data binding</param>
        /// <exception cref="ArgumentNullException">Thrown when viewModel is null</exception>
        public TransactionView(TransactionViewModel viewModel)
        {
            Console.WriteLine("[TransactionView] Initializing TransactionView with category filtering...");

            InitializeComponent();

            // Validate required dependencies
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            // Apply current localization flow direction
            this.FlowDirection = LanguageManager.CurrentFlowDirection;

            // Establish data context for MVVM binding
            DataContext = _viewModel;

            // Initialize event subscriptions for proper lifecycle management
            InitializeEventSubscriptions();

            // Subscribe to critical view model property changes for UI state management
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            Console.WriteLine("[TransactionView] TransactionView initialization completed");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles view model property changes to maintain UI consistency
        /// Specifically monitors drawer state, command availability, and category changes
        /// </summary>
        /// <param name="sender">The source view model</param>
        /// <param name="e">Property change event arguments</param>
        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Monitor critical properties that affect UI command state
            if (e.PropertyName == nameof(TransactionViewModel.IsDrawerOpen) ||
                e.PropertyName == nameof(TransactionViewModel.CurrentDrawer) ||
                e.PropertyName == nameof(TransactionViewModel.SelectedCategory) ||
                e.PropertyName == nameof(TransactionViewModel.SelectedCategoryId))
            {
                Console.WriteLine($"[TransactionView] Property changed: {e.PropertyName}");

                // Ensure UI command states are refreshed on the main thread
                Application.Current.Dispatcher.Invoke(() => {
                    CommandManager.InvalidateRequerySuggested();
                });
            }
        }

        /// <summary>
        /// Performs initialization tasks when the view is fully loaded
        /// </summary>
        /// <param name="sender">The source control</param>
        /// <param name="e">Event arguments</param>
        private async void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[TransactionView] View loaded, refreshing drawer status...");

                // Refresh drawer status to ensure accurate initial state
                await _viewModel.RefreshDrawerStatusAsync();

                Console.WriteLine("[TransactionView] View loaded initialization completed");
            }
            catch (Exception ex)
            {
                // Log initialization errors without breaking the UI
                Console.WriteLine($"[TransactionView] Error during view initialization: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error during view initialization: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles barcode input with Enter key submission for rapid product scanning
        /// Provides immediate feedback and prevents event bubbling
        /// </summary>
        /// <param name="sender">The barcode TextBox control</param>
        /// <param name="e">Key event arguments</param>
        private void BarcodeSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Barcode search triggered: {_viewModel.BarcodeQuery}");

                    // Execute barcode search command if available
                    if (_viewModel.SearchBarcodeCommand?.CanExecute(null) == true)
                    {
                        _viewModel.SearchBarcodeCommand.Execute(null);
                    }
                    else
                    {
                        Console.WriteLine("[TransactionView] Barcode search command not available or cannot execute");
                    }
                }
                catch (Exception ex)
                {
                    // Handle search errors gracefully
                    Console.WriteLine($"[TransactionView] Barcode search error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Barcode search error: {ex.Message}");
                }
                finally
                {
                    // Prevent further event processing
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles customer search ComboBox text changes for real-time filtering
        /// Implements debounced search with dropdown visibility management
        /// </summary>
        /// <param name="sender">The customer search ComboBox</param>
        /// <param name="e">Text change event arguments</param>
        private void CustomerSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Customer search text changed: {comboBox.Text}");

                    // Update the search query in the view model
                    _viewModel.UpdateCustomerQuery(comboBox.Text);

                    // Manage dropdown visibility based on input content
                    comboBox.IsDropDownOpen = !string.IsNullOrWhiteSpace(comboBox.Text);
                }
                catch (Exception ex)
                {
                    // Handle customer search errors without breaking UI flow
                    Console.WriteLine($"[TransactionView] Customer search error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Customer search error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles customer selection from the search ComboBox
        /// Automatically clears search input and resets selection state
        /// </summary>
        /// <param name="sender">The customer search ComboBox</param>
        /// <param name="e">Selection change event arguments</param>
        private void CustomerSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is Customer customer)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Customer selected: {customer.Name} (ID: {customer.CustomerId})");

                    // Set the selected customer in the view model
                    _viewModel.SetSelectedCustomer(customer);

                    // Update the display text and reset selection state
                    _viewModel.CustomerQuery = customer.Name;
                }
                catch (Exception ex)
                {
                    // Handle customer selection errors
                    Console.WriteLine($"[TransactionView] Customer selection error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Customer selection error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles customer search text input for cursor positioning optimization
        /// Ensures cursor remains at end of text during rapid typing
        /// </summary>
        /// <param name="sender">The customer search ComboBox</param>
        /// <param name="e">Text composition event arguments</param>
        private void CustomerSearch_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is ComboBox comboBox &&
                comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
            {
                // Defer cursor positioning to maintain text input flow
                comboBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.CaretIndex = textBox.Text.Length;
                    textBox.SelectionLength = 0;
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        /// <summary>
        /// Handles customer item double-click for rapid selection
        /// Provides alternative selection method for mouse-centric workflows
        /// </summary>
        /// <param name="sender">The customer list item</param>
        /// <param name="e">Mouse button event arguments</param>
        private void CustomerItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 &&
                sender is FrameworkElement element &&
                element.DataContext is Customer customer)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Customer double-clicked: {customer.Name}");

                    // Set customer and populate search field
                    _viewModel.SetSelectedCustomerAndFillSearch(customer);
                }
                catch (Exception ex)
                {
                    // Handle double-click selection errors
                    Console.WriteLine($"[TransactionView] Customer double-click selection error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Customer double-click selection error: {ex.Message}");
                }
                finally
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles cart item quantity updates when focus leaves the quantity TextBox
        /// Validates input and triggers recalculation of totals
        /// </summary>
        /// <param name="sender">The quantity TextBox control</param>
        /// <param name="e">Focus event arguments</param>
        private void Quantity_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItem cartItem)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Quantity updated for {cartItem.Product.Name}: {cartItem.Quantity}");

                    // Delegate quantity update to view model for business logic processing
                    _viewModel.UpdateCartItemQuantity(cartItem);
                }
                catch (Exception ex)
                {
                    // Handle quantity update errors with user feedback
                    Console.WriteLine($"[TransactionView] Quantity update error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Quantity update error: {ex.Message}");

                    // Reset to previous valid value if update fails
                    textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                }
            }
        }

        /// <summary>
        /// Handles cart item discount updates when focus leaves the discount TextBox
        /// Supports both percentage and fixed amount discount types
        /// </summary>
        /// <param name="sender">The discount TextBox control</param>
        /// <param name="e">Focus event arguments</param>
        private void Discount_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItem cartItem)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Discount updated for {cartItem.Product.Name}");

                    // Process discount update through view model business logic
                    _viewModel.UpdateCartItemDiscount(cartItem);
                }
                catch (Exception ex)
                {
                    // Handle discount update errors gracefully
                    Console.WriteLine($"[TransactionView] Discount update error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Discount update error: {ex.Message}");

                    // Restore previous valid value if update fails
                    textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                }
            }
        }

        /// <summary>
        /// Handles discount type selection changes (percentage vs fixed amount)
        /// Immediately recalculates discount values when type changes
        /// </summary>
        /// <param name="sender">The discount type ComboBox</param>
        /// <param name="e">Selection change event arguments</param>
        private void DiscountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is CartItem cartItem)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Discount type changed for {cartItem.Product.Name}");

                    // Update discount calculation based on new type selection
                    _viewModel.UpdateCartItemDiscount(cartItem);
                }
                catch (Exception ex)
                {
                    // Handle discount type change errors
                    Console.WriteLine($"[TransactionView] Discount type change error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Discount type change error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Generic button click handler for extensibility
        /// Placeholder for future button functionality that doesn't require specific handling
        /// </summary>
        /// <param name="sender">The clicked button</param>
        /// <param name="e">Event arguments</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Reserved for future button implementations
            // Current implementation intentionally empty for forward compatibility
            Console.WriteLine("[TransactionView] Generic button click handled");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Focuses the barcode input for rapid scanning workflows
        /// Optimizes for high-frequency barcode scanning operations
        /// </summary>
        public void FocusBarcodeInput()
        {
            try
            {
                Console.WriteLine("[TransactionView] Focusing barcode input...");

                // Find the barcode TextBox by name
                if (FindName("BarcodeInputTextBox") is TextBox barcodeBox)
                {
                    barcodeBox.Focus();
                    barcodeBox.SelectAll();
                    Console.WriteLine("[TransactionView] Barcode input focused successfully");
                }
                else
                {
                    Console.WriteLine("[TransactionView] Barcode input TextBox not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Focus barcode input error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Focus barcode input error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets focus to the category filter for keyboard-driven workflows
        /// Provides programmatic access for category selection via keyboard
        /// </summary>
        public void FocusCategoryFilter()
        {
            try
            {
                Console.WriteLine("[TransactionView] Focusing category filter...");

                // The category ComboBox doesn't have a specific name, but we can find it by type
                if (this.Content is Grid mainGrid)
                {
                    var categoryComboBox = FindVisualChild<ComboBox>(mainGrid);
                    if (categoryComboBox != null && categoryComboBox.ItemsSource == _viewModel.Categories)
                    {
                        categoryComboBox.Focus();
                        categoryComboBox.IsDropDownOpen = true;
                        Console.WriteLine("[TransactionView] Category filter focused successfully");
                    }
                    else
                    {
                        Console.WriteLine("[TransactionView] Category filter ComboBox not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Focus category filter error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Focus category filter error: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to find visual children of a specific type
        /// </summary>
        /// <typeparam name="T">The type of visual child to find</typeparam>
        /// <param name="parent">The parent visual element</param>
        /// <returns>The first child of the specified type, or null if not found</returns>
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }

        #endregion

        #region Resource Cleanup

        /// <summary>
        /// Handles the Unloaded event for proper resource cleanup and memory management
        /// Implements enterprise-grade disposal pattern to prevent memory leaks in long-running POS applications
        /// </summary>
        /// <param name="sender">The source UserControl instance</param>
        /// <param name="e">Routed event arguments containing event lifecycle information</param>
        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[TransactionView] Starting view unload cleanup...");

                // Unsubscribe from view model events to prevent memory leaks
                // Critical for long-running POS applications with frequent view transitions
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                    Console.WriteLine("[TransactionView] Unsubscribed from view model property changes");
                }

                // Additional cleanup for any subscribed events or resources
                this.Loaded -= OnViewLoaded;
                this.Unloaded -= OnViewUnloaded;

                // Log successful cleanup for diagnostic purposes
                Console.WriteLine("[TransactionView] Resource cleanup completed successfully");
                System.Diagnostics.Debug.WriteLine("TransactionView: Resource cleanup completed successfully");
            }
            catch (Exception ex)
            {
                // Log cleanup errors but don't throw during disposal to maintain application stability
                // Essential for production POS systems where view lifecycle errors shouldn't crash the application
                Console.WriteLine($"[TransactionView] Cleanup error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TransactionView cleanup error: {ex.Message}");

                // In production, consider logging to a centralized logging system
                // Logger.Error($"View cleanup failed: {ex}");
            }
        }

        /// <summary>
        /// Initializes event subscriptions and prepares the view for resource tracking
        /// Called during constructor execution to establish proper lifecycle management
        /// </summary>
        private void InitializeEventSubscriptions()
        {
            try
            {
                Console.WriteLine("[TransactionView] Initializing event subscriptions...");

                // Subscribe to the Unloaded event for resource cleanup
                this.Unloaded += OnViewUnloaded;

                // Ensure Loaded event subscription is properly established
                this.Loaded += OnViewLoaded;

                Console.WriteLine("[TransactionView] Event subscriptions initialized successfully");
                System.Diagnostics.Debug.WriteLine("TransactionView: Event subscriptions initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Event subscription error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TransactionView event subscription error: {ex.Message}");
                throw; // Re-throw during initialization as this indicates a critical setup failure
            }
        }

        #endregion
    }
}