// File: QuickTechPOS/Views/TransactionView.xaml.cs
// OPTIMIZED FOR 14-INCH POS SCREENS

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
    /// Optimized TransactionView for 14-inch POS screens with enhanced space efficiency
    /// Maintains all functionality while maximizing use of limited screen real estate
    /// Target resolutions: 1366x768, 1280x800, 1440x900
    /// </summary>
    public partial class TransactionView : UserControl
    {
        #region Private Fields

        private readonly TransactionViewModel _viewModel;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the optimized TransactionView for compact POS displays
        /// </summary>
        /// <param name="viewModel">The transaction view model containing business logic and data binding</param>
        /// <exception cref="ArgumentNullException">Thrown when viewModel is null</exception>
        public TransactionView(TransactionViewModel viewModel)
        {
            Console.WriteLine("[TransactionView] Initializing optimized TransactionView for 14-inch screens...");

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

            // Configure optimized input handling for compact screens
            ConfigureOptimizedInputHandling();

            Console.WriteLine("[TransactionView] Optimized TransactionView initialization completed");
        }

        #endregion

        #region Optimized Configuration Methods

        /// <summary>
        /// Configures input handling optimized for 14-inch touch screens and compact keyboards
        /// </summary>
        private void ConfigureOptimizedInputHandling()
        {
            try
            {
                Console.WriteLine("[TransactionView] Configuring optimized input handling...");

                // Enhanced keyboard shortcuts for compact screens
                var quickScanBinding = new KeyBinding(
                    new RelayCommand(param => FocusBarcodeInput()),
                    Key.F1, ModifierKeys.None);
                this.InputBindings.Add(quickScanBinding);

                var categoryFilterBinding = new KeyBinding(
                    new RelayCommand(param => FocusCategoryFilter()),
                    Key.F2, ModifierKeys.None);
                this.InputBindings.Add(categoryFilterBinding);

                var clearCartBinding = new KeyBinding(
                    new RelayCommand(param => {
                        if (_viewModel.ClearCartCommand?.CanExecute(null) == true)
                            _viewModel.ClearCartCommand.Execute(null);
                    }),
                    Key.F3, ModifierKeys.None);
                this.InputBindings.Add(clearCartBinding);

                var checkoutBinding = new KeyBinding(
                    new RelayCommand(param => {
                        if (_viewModel.CheckoutCommand?.CanExecute(null) == true)
                            _viewModel.CheckoutCommand.Execute(null);
                    }),
                    Key.F4, ModifierKeys.None);
                this.InputBindings.Add(checkoutBinding);

                // Optimized touch handling
                this.TouchDown += OnTouchInput;
                this.TouchUp += OnTouchInput;

                Console.WriteLine("[TransactionView] Optimized input handling configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error configuring optimized input handling: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles touch input for enhanced mobile/tablet POS experience
        /// </summary>
        private void OnTouchInput(object sender, TouchEventArgs e)
        {
            try
            {
                // Ensure touch events are properly handled for POS terminals
                // This can be extended for gesture recognition if needed
                Console.WriteLine($"[TransactionView] Touch input detected at: {e.GetTouchPoint(this).Position}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling touch input: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles view model property changes to maintain UI consistency
        /// Optimized for compact screen updates and performance
        /// </summary>
        /// <param name="sender">The source view model</param>
        /// <param name="e">Property change event arguments</param>
        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Monitor critical properties that affect UI command state
            if (e.PropertyName == nameof(TransactionViewModel.IsDrawerOpen) ||
                e.PropertyName == nameof(TransactionViewModel.CurrentDrawer) ||
                e.PropertyName == nameof(TransactionViewModel.SelectedCategory) ||
                e.PropertyName == nameof(TransactionViewModel.SelectedCategoryId) ||
                e.PropertyName == nameof(TransactionViewModel.CartItems) ||
                e.PropertyName == nameof(TransactionViewModel.TotalAmount))
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
        /// Optimized for fast startup on resource-constrained POS systems
        /// </summary>
        /// <param name="sender">The source control</param>
        /// <param name="e">Event arguments</param>
        private async void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[TransactionView] Optimized view loaded, performing quick initialization...");

                // Fast drawer status refresh for immediate responsiveness
                await _viewModel.RefreshDrawerStatusAsync();

                // Set initial focus to barcode input for immediate scanning capability
                FocusBarcodeInput();

                Console.WriteLine("[TransactionView] Optimized view loaded initialization completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error during optimized view initialization: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error during view initialization: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles barcode input with optimized Enter key submission for rapid scanning workflows
        /// Enhanced for high-frequency scanning operations common in retail POS
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

                        // Clear input for next scan (optimized workflow)
                        if (sender is TextBox textBox)
                        {
                            textBox.SelectAll();
                        }
                    }
                    else
                    {
                        Console.WriteLine("[TransactionView] Barcode search command not available");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Barcode search error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Barcode search error: {ex.Message}");
                }
                finally
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles customer search ComboBox text changes with optimized debouncing
        /// Optimized for responsive search on compact displays
        /// </summary>
        /// <param name="sender">The customer search ComboBox</param>
        /// <param name="e">Text change event arguments</param>
        private void CustomerSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Customer search: {comboBox.Text}");

                    // Update the search query in the view model
                    _viewModel.UpdateCustomerQuery(comboBox.Text);

                    // Optimized dropdown management for compact screens
                    comboBox.IsDropDownOpen = !string.IsNullOrWhiteSpace(comboBox.Text) && comboBox.Text.Length >= 2;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Customer search error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Customer search error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles customer selection from the search ComboBox with optimized UX
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

                    // Auto-focus back to barcode for continued scanning workflow
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                        FocusBarcodeInput();
                    }), System.Windows.Threading.DispatcherPriority.Input);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Customer selection error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Customer selection error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles customer search text input with optimized cursor positioning
        /// </summary>
        /// <param name="sender">The customer search ComboBox</param>
        /// <param name="e">Text composition event arguments</param>
        private void CustomerSearch_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is ComboBox comboBox &&
                comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
            {
                // Optimized cursor positioning for fast typing
                comboBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.CaretIndex = textBox.Text.Length;
                    textBox.SelectionLength = 0;
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        /// <summary>
        /// Handles customer item double-click for rapid selection
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

                    // Set customer and return focus to scanning workflow
                    _viewModel.SetSelectedCustomerAndFillSearch(customer);

                    // Return focus to barcode input for optimized workflow
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                        FocusBarcodeInput();
                    }), System.Windows.Threading.DispatcherPriority.Input);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Customer double-click error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Customer double-click error: {ex.Message}");
                }
                finally
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles cart item quantity updates with optimized validation
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
                    Console.WriteLine($"[TransactionView] Quantity update error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Quantity update error: {ex.Message}");

                    // Reset to previous valid value if update fails
                    textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                }
            }
        }

        /// <summary>
        /// Handles cart item discount updates with enhanced validation
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
                    Console.WriteLine($"[TransactionView] Discount update error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Discount update error: {ex.Message}");

                    // Restore previous valid value if update fails
                    textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                }
            }
        }

        /// <summary>
        /// Handles discount type selection changes with immediate recalculation
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
                    Console.WriteLine($"[TransactionView] Discount type change error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Discount type change error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Generic button click handler for extensibility
        /// </summary>
        /// <param name="sender">The clicked button</param>
        /// <param name="e">Event arguments</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("[TransactionView] Generic button click handled");
        }

        #endregion

        #region Optimized Focus Management

        /// <summary>
        /// Focuses the barcode input for rapid scanning workflows
        /// Optimized for high-frequency scanning operations
        /// </summary>
        public void FocusBarcodeInput()
        {
            try
            {
                Console.WriteLine("[TransactionView] Focusing barcode input for optimized scanning...");

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
        /// Sets focus to the category filter for optimized keyboard navigation
        /// </summary>
        public void FocusCategoryFilter()
        {
            try
            {
                Console.WriteLine("[TransactionView] Focusing category filter...");

                // Find the category ComboBox through visual tree traversal
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
        /// Optimized for the compact layout structure
        /// </summary>
        /// <typeparam name="T">The type of visual child to find</typeparam>
        /// <param name="parent">The parent visual element</param>
        /// <returns>The first child of the specified type, or null if not found</returns>
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            try
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error finding visual child: {ex.Message}");
            }
            return null;
        }

        #endregion

        #region Resource Cleanup

        /// <summary>
        /// Handles the Unloaded event for optimized resource cleanup
        /// Essential for memory management in long-running POS applications
        /// </summary>
        /// <param name="sender">The source UserControl instance</param>
        /// <param name="e">Routed event arguments</param>
        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[TransactionView] Starting optimized view unload cleanup...");

                // Unsubscribe from view model events
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                    Console.WriteLine("[TransactionView] Unsubscribed from view model events");
                }

                // Clean up optimized input handling
                this.TouchDown -= OnTouchInput;
                this.TouchUp -= OnTouchInput;

                // Additional cleanup for any subscribed events or resources
                this.Loaded -= OnViewLoaded;
                this.Unloaded -= OnViewUnloaded;

                Console.WriteLine("[TransactionView] Optimized resource cleanup completed successfully");
                System.Diagnostics.Debug.WriteLine("TransactionView: Optimized cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Cleanup error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TransactionView cleanup error: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes event subscriptions for optimized lifecycle management
        /// </summary>
        private void InitializeEventSubscriptions()
        {
            try
            {
                Console.WriteLine("[TransactionView] Initializing optimized event subscriptions...");

                // Subscribe to lifecycle events
                this.Unloaded += OnViewUnloaded;
                this.Loaded += OnViewLoaded;

                Console.WriteLine("[TransactionView] Optimized event subscriptions initialized");
                System.Diagnostics.Debug.WriteLine("TransactionView: Optimized subscriptions initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Event subscription error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TransactionView subscription error: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Performance Optimization Methods

        /// <summary>
        /// Optimizes the view for better performance on resource-constrained POS systems
        /// </summary>
        public void OptimizeForPerformance()
        {
            try
            {
                Console.WriteLine("[TransactionView] Applying performance optimizations...");

                // Enable bitmap caching for better scrolling performance
                if (this.Content is FrameworkElement content)
                {
                    RenderOptions.SetBitmapScalingMode(content, BitmapScalingMode.LowQuality);
                    RenderOptions.SetCachingHint(content, CachingHint.Cache);
                }

                // Optimize for touch input
                this.IsManipulationEnabled = true;

                Console.WriteLine("[TransactionView] Performance optimizations applied");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Performance optimization error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the layout for specific screen sizes dynamically
        /// </summary>
        /// <param name="screenWidth">Available screen width</param>
        /// <param name="screenHeight">Available screen height</param>
        public void UpdateLayoutForScreenSize(double screenWidth, double screenHeight)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Updating layout for {screenWidth}x{screenHeight}");

                // Adjust column widths based on actual screen size
                if (this.Content is Grid mainGrid && mainGrid.ColumnDefinitions.Count >= 3)
                {
                    var leftColumn = mainGrid.ColumnDefinitions[0];
                    var rightColumn = mainGrid.ColumnDefinitions[2];

                    if (screenWidth < 1280)
                    {
                        // Extra compact for very small screens
                        leftColumn.Width = new GridLength(260);
                        rightColumn.Width = new GridLength(280);
                    }
                    else if (screenWidth < 1366)
                    {
                        // Standard compact
                        leftColumn.Width = new GridLength(280);
                        rightColumn.Width = new GridLength(300);
                    }
                    else
                    {
                        // Slightly more spacious for larger 14" screens
                        leftColumn.Width = new GridLength(300);
                        rightColumn.Width = new GridLength(320);
                    }
                }

                Console.WriteLine("[TransactionView] Layout updated for screen size");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Layout update error: {ex.Message}");
            }
        }

        #endregion
    }
}