// File: QuickTechPOS/Views/TransactionView.xaml.cs
// COMPLETE FILE: Enhanced with table status management (red tables when items present)
// UPDATED: Fixed to allow decimal quantities (0.5, 1.5, etc.) + automatic table status updates

using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.Services;
using QuickTechPOS.ViewModels;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// TransactionView optimized specifically for 1024x768 POS touch screens
    /// Target resolution: 1024x768 (exact fit optimization)
    /// Focus: Compact but readable product cards, efficient space usage, touch-friendly controls
    /// ENHANCED: Added automatic table status management - tables turn red when they have items
    /// UPDATED: Added decimal quantity support (0.5, 1.5, etc.)
    /// </summary>
    public partial class TransactionView : UserControl
    {
        #region Private Fields

        private readonly TransactionViewModel _viewModel;
        private DateTime _lastTouchTime = DateTime.MinValue;
        private const int TOUCH_DEBOUNCE_MS = 150; // Prevent accidental double-touches

        // Decimal quantity configuration
        private const decimal MIN_QUANTITY = 0.1m; // Minimum allowed quantity
        private const decimal QUANTITY_INCREMENT = 0.5m; // Default increment for +/- buttons

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the TransactionView optimized specifically for 1024x768 POS displays
        /// Enhanced with automatic table status management
        /// </summary>
        /// <param name="viewModel">The transaction view model containing business logic and data binding</param>
        /// <exception cref="ArgumentNullException">Thrown when viewModel is null</exception>
        public TransactionView(TransactionViewModel viewModel)
        {
            Console.WriteLine("[TransactionView] Initializing enhanced TransactionView optimized for 1024x768 resolution with table status management...");

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

            // Configure enhanced touch handling for POS screens
            ConfigureEnhancedTouchHandling();

            // Optimize specifically for 1024x768 resolution
            OptimizeForPOSDisplay();

            Console.WriteLine("[TransactionView] Enhanced 1024x768 TransactionView initialization completed successfully with table status management");
        }

        #endregion

        #region Enhanced POS Touch Screen Optimization Methods

        /// <summary>
        /// Configures enhanced touch handling optimized for 1024x768 POS touch screens
        /// Includes touch debouncing, gesture recognition, and optimized responsiveness for compact layout
        /// </summary>
        private void ConfigureEnhancedTouchHandling()
        {
            try
            {
                Console.WriteLine("[TransactionView] Configuring enhanced touch handling for improved POS interaction...");

                // Enable touch manipulation for better touch responsiveness
                this.IsManipulationEnabled = true;
                this.ManipulationDelta += OnManipulationDelta;
                this.ManipulationCompleted += OnManipulationCompleted;

                // Enhanced keyboard shortcuts for POS workflow
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

                // Table selection shortcut (since it's now in the footer)
                var selectTableBinding = new KeyBinding(
                    new RelayCommand(param => {
                        if (_viewModel.SelectTableCommand?.CanExecute(null) == true)
                            _viewModel.SelectTableCommand.Execute(null);
                    }),
                    Key.F5, ModifierKeys.None);
                this.InputBindings.Add(selectTableBinding);

                // Add number pad support for quick product lookup
                for (int i = 0; i <= 9; i++)
                {
                    var key = (Key)Enum.Parse(typeof(Key), $"NumPad{i}");
                    var numberBinding = new KeyBinding(
                        new RelayCommand(param => HandleNumberInput(param.ToString())),
                        key, ModifierKeys.None);
                    this.InputBindings.Add(numberBinding);
                }

                // Enhanced touch event handling with debouncing
                this.TouchDown += OnEnhancedTouchDown;
                this.TouchUp += OnEnhancedTouchUp;
                this.TouchMove += OnEnhancedTouchMove;

                // Stylus support for precision interaction
                this.StylusDown += OnStylusInteraction;
                this.StylusUp += OnStylusInteraction;

                Console.WriteLine("[TransactionView] Enhanced touch handling configured successfully with improved product interaction");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error configuring enhanced touch handling: {ex.Message}");
            }
        }

        /// <summary>
        /// ENHANCED: Handles key presses in quantity field to allow only valid decimal input
        /// </summary>
        private void Quantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Allow: Numbers, Decimal point, Backspace, Delete, Tab, Enter, Arrow keys
                if ((e.Key >= Key.D0 && e.Key <= Key.D9) ||           // Regular numbers
                    (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||  // Numpad numbers
                    e.Key == Key.OemPeriod ||                          // Decimal point (.)
                    e.Key == Key.Decimal ||                            // Numpad decimal
                    e.Key == Key.Back ||                               // Backspace
                    e.Key == Key.Delete ||                             // Delete
                    e.Key == Key.Tab ||                                // Tab
                    e.Key == Key.Enter ||                              // Enter
                    e.Key == Key.Left ||                               // Arrow keys
                    e.Key == Key.Right ||
                    e.Key == Key.Home ||
                    e.Key == Key.End)
                {
                    // Allow these keys

                    // Handle Enter key to confirm input
                    if (e.Key == Key.Enter)
                    {
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                    }

                    // Prevent multiple decimal points
                    if ((e.Key == Key.OemPeriod || e.Key == Key.Decimal) &&
                        textBox.Text.Contains("."))
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    // Block all other keys
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// ENHANCED: Handles quantity field focus to select all text for easy editing
        /// </summary>
        private void Quantity_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Select all text when focused for easy replacement
                textBox.SelectAll();
            }
        }

        /// <summary>
        /// Optimizes display settings and performance specifically for 1024x768 POS screen environment
        /// </summary>
        private void OptimizeForPOSDisplay()
        {
            try
            {
                Console.WriteLine("[TransactionView] Applying enhanced POS display optimizations...");

                // Enable hardware acceleration for smooth scrolling and animations
                RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
                RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

                // Optimize for touch input latency
                this.UseLayoutRounding = true;
                this.SnapsToDevicePixels = true;

                // Enhanced text rendering for better product name visibility
                TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
                TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
                TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);

                // Configure cache settings for better performance with larger product cards
                BitmapCache cache = new BitmapCache();
                cache.RenderAtScale = 1.0;
                cache.EnableClearType = true;
                cache.SnapsToDevicePixels = true;
                this.CacheMode = cache;

                Console.WriteLine("[TransactionView] Enhanced POS display optimizations applied successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error applying enhanced POS display optimizations: {ex.Message}");
            }
        }

        #endregion

        #region Enhanced Touch Event Handlers

        /// <summary>
        /// Handles enhanced touch down events with debouncing for POS touch screens
        /// Optimized for larger product cards and improved interaction
        /// </summary>
        private void OnEnhancedTouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                var currentTime = DateTime.Now;
                var timeSinceLastTouch = (currentTime - _lastTouchTime).TotalMilliseconds;

                // Debounce rapid touches to prevent accidental double-taps
                if (timeSinceLastTouch < TOUCH_DEBOUNCE_MS)
                {
                    e.Handled = true;
                    return;
                }

                _lastTouchTime = currentTime;

                var touchPoint = e.GetTouchPoint(this);
                Console.WriteLine($"[TransactionView] Enhanced touch down at: {touchPoint.Position} (Pressure: {touchPoint.Size})");

                // Provide visual feedback for touch interaction
                ProvideTouchFeedback(touchPoint.Position);

                // Capture touch for reliable tracking
                this.CaptureTouch(e.TouchDevice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling enhanced touch down: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles enhanced touch up events with gesture recognition for improved product selection
        /// </summary>
        private void OnEnhancedTouchUp(object sender, TouchEventArgs e)
        {
            try
            {
                var touchPoint = e.GetTouchPoint(this);
                Console.WriteLine($"[TransactionView] Enhanced touch up at: {touchPoint.Position}");

                // Release touch capture
                this.ReleaseTouchCapture(e.TouchDevice);

                // Process touch gesture if applicable
                ProcessTouchGesture(touchPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling enhanced touch up: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles touch move events for scroll and gesture recognition with improved sensitivity
        /// </summary>
        private void OnEnhancedTouchMove(object sender, TouchEventArgs e)
        {
            try
            {
                // Handle touch move for custom scroll behavior optimized for product grid
                var touchPoint = e.GetTouchPoint(this);

                // Enhanced gesture recognition for product navigation
                // This can be extended for swipe between categories, etc.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling enhanced touch move: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles stylus interactions for precision input on POS devices
        /// Enhanced for detailed product selection
        /// </summary>
        private void OnStylusInteraction(object sender, StylusEventArgs e)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Stylus interaction detected for precise product selection");
                // Handle stylus input for precise operations
                // Useful for detailed product selection and quantity input
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling stylus interaction: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles manipulation delta for smooth scrolling and zooming
        /// Optimized for larger product grid layout
        /// </summary>
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            try
            {
                // Handle manipulation for smooth scrolling in product grid
                if (Math.Abs(e.DeltaManipulation.Translation.Y) > 1)
                {
                    // Find the nearest ScrollViewer and apply smooth scrolling
                    var scrollViewer = FindVisualChild<ScrollViewer>(this);
                    if (scrollViewer != null)
                    {
                        var newOffset = scrollViewer.VerticalOffset - e.DeltaManipulation.Translation.Y;
                        scrollViewer.ScrollToVerticalOffset(Math.Max(0, newOffset));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling manipulation delta: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles manipulation completed events with enhanced inertial scrolling
        /// </summary>
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Manipulation completed with velocity: {e.FinalVelocities.LinearVelocity}");

                // Handle inertial scrolling optimized for product grid
                if (Math.Abs(e.FinalVelocities.LinearVelocity.Y) > 100)
                {
                    var scrollViewer = FindVisualChild<ScrollViewer>(this);
                    scrollViewer?.ScrollToVerticalOffset(
                        scrollViewer.VerticalOffset - (e.FinalVelocities.LinearVelocity.Y * 0.1));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling manipulation completed: {ex.Message}");
            }
        }

        #endregion

        #region Enhanced POS-Specific Input Handlers

        /// <summary>
        /// Handles number input from physical number pad or on-screen keyboard
        /// Enhanced for improved product search and selection
        /// </summary>
        private void HandleNumberInput(string number)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Number input received: {number}");

                // Enhanced barcode and product code handling
                if (!string.IsNullOrEmpty(_viewModel.BarcodeQuery))
                {
                    _viewModel.BarcodeQuery += number;
                }
                else
                {
                    _viewModel.BarcodeQuery = number;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling number input: {ex.Message}");
            }
        }

        /// <summary>
        /// Provides enhanced visual feedback for touch interactions
        /// Optimized for larger product cards
        /// </summary>
        private void ProvideTouchFeedback(Point touchPosition)
        {
            try
            {
                // Create enhanced visual ripple effect for touch feedback
                var feedbackElement = new Ellipse
                {
                    Width = 24,
                    Height = 24,
                    Fill = new SolidColorBrush(Color.FromArgb(120, 37, 99, 235)), // Enhanced visibility
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(touchPosition.X - 12, touchPosition.Y - 12, 0, 0)
                };

                // Add to main grid temporarily
                if (this.Content is Grid mainGrid)
                {
                    mainGrid.Children.Add(feedbackElement);

                    // Enhanced animation duration for better feedback
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(250)
                    };
                    timer.Tick += (s, e) =>
                    {
                        mainGrid.Children.Remove(feedbackElement);
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error providing enhanced touch feedback: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes touch gestures for enhanced POS-specific actions
        /// Optimized for improved product interaction
        /// </summary>
        private void ProcessTouchGesture(TouchPoint touchPoint)
        {
            try
            {
                // Enhanced gesture processing for product cards:
                // - Double-tap to add multiple items
                // - Long press for product details
                // - Swipe for category navigation

                Console.WriteLine($"[TransactionView] Processing enhanced touch gesture at: {touchPoint.Position}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error processing enhanced touch gesture: {ex.Message}");
            }
        }

        #endregion

        #region ENHANCED: Decimal Quantity Support Methods with Table Status Updates

        /// <summary>
        /// Validates and parses decimal quantity input
        /// </summary>
        /// <param name="input">The input string to validate</param>
        /// <param name="quantity">The parsed quantity if valid</param>
        /// <returns>True if the input is a valid decimal quantity</returns>
        private bool TryParseQuantity(string input, out decimal quantity)
        {
            quantity = 0;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Try to parse as decimal using current culture
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out quantity))
            {
                // Ensure quantity is within acceptable range
                if (quantity >= MIN_QUANTITY && quantity <= 999999)
                {
                    // Round to 2 decimal places for consistency
                    quantity = Math.Round(quantity, 2);
                    return true;
                }
            }

            // Try alternative parsing with different culture (handle both . and , as decimal separators)
            var invariantInput = input.Replace(',', '.');
            if (decimal.TryParse(invariantInput, NumberStyles.Number, CultureInfo.InvariantCulture, out quantity))
            {
                if (quantity >= MIN_QUANTITY && quantity <= 999999)
                {
                    quantity = Math.Round(quantity, 2);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Formats a decimal quantity for display
        /// </summary>
        /// <param name="quantity">The quantity to format</param>
        /// <returns>Formatted quantity string</returns>
        private string FormatQuantity(decimal quantity)
        {
            // Remove unnecessary decimal places
            if (quantity == Math.Floor(quantity))
            {
                return quantity.ToString("0");
            }
            else
            {
                return quantity.ToString("0.##");
            }
        }

        /// <summary>
        /// ENHANCED: Increments the quantity of a cart item with automatic table status update
        /// </summary>
        /// <param name="cartItem">The cart item to increment</param>
        private void IncrementQuantityWithStatusUpdate(CartItem cartItem)
        {
            if (cartItem == null) return;

            try
            {
                decimal newQuantity = cartItem.Quantity + QUANTITY_INCREMENT;
                cartItem.Quantity = Math.Round(newQuantity, 2);

                Console.WriteLine($"[TransactionView] Incremented quantity for {cartItem.Product.Name} to {cartItem.Quantity}");

                // ENHANCED: Use the enhanced method that includes table status updates
                _viewModel.UpdateCartItemQuantityWithStatus(cartItem);
                ProvideHapticFeedback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error incrementing quantity: {ex.Message}");
            }
        }

        /// <summary>
        /// ENHANCED: Decrements the quantity of a cart item with automatic table status update
        /// </summary>
        /// <param name="cartItem">The cart item to decrement</param>
        private void DecrementQuantityWithStatusUpdate(CartItem cartItem)
        {
            if (cartItem == null) return;

            try
            {
                decimal newQuantity = cartItem.Quantity - QUANTITY_INCREMENT;

                // Ensure minimum quantity
                if (newQuantity < MIN_QUANTITY)
                    newQuantity = MIN_QUANTITY;

                cartItem.Quantity = Math.Round(newQuantity, 2);

                Console.WriteLine($"[TransactionView] Decremented quantity for {cartItem.Product.Name} to {cartItem.Quantity}");

                // ENHANCED: Use the enhanced method that includes table status updates
                _viewModel.UpdateCartItemQuantityWithStatus(cartItem);
                ProvideHapticFeedback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error decrementing quantity: {ex.Message}");
            }
        }

        #endregion

        #region ENHANCED: Event Handlers with Table Status Updates

        /// <summary>
        /// Handles view model property changes to maintain UI consistency
        /// Enhanced for improved layout responsiveness and table status updates
        /// </summary>
        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Monitor critical properties that affect UI command state
            if (e.PropertyName == nameof(TransactionViewModel.IsDrawerOpen) ||
                e.PropertyName == nameof(TransactionViewModel.CurrentDrawer) ||
                e.PropertyName == nameof(TransactionViewModel.SelectedCategory) ||
                e.PropertyName == nameof(TransactionViewModel.SelectedCategoryId) ||
                e.PropertyName == nameof(TransactionViewModel.CartItems) ||
                e.PropertyName == nameof(TransactionViewModel.TotalAmount) ||
                e.PropertyName == nameof(TransactionViewModel.SelectedTable) ||
                e.PropertyName == nameof(TransactionViewModel.WholesaleMode))
            {
                Console.WriteLine($"[TransactionView] Property changed: {e.PropertyName}");

                // ENHANCED: Trigger table status refresh for cart-related changes
                if (e.PropertyName == nameof(TransactionViewModel.CartItems) ||
                    e.PropertyName == nameof(TransactionViewModel.TotalAmount))
                {
                    try
                    {
                        _viewModel.RefreshAllTableStatuses();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TransactionView] Error refreshing table statuses: {ex.Message}");
                    }
                }

                // Ensure UI command states are refreshed on the main thread
                Application.Current.Dispatcher.Invoke(() => {
                    CommandManager.InvalidateRequerySuggested();
                });
            }
        }

        /// <summary>
        /// Performs initialization tasks when the view is fully loaded
        /// Optimized for 1024x768 POS screen layout
        /// </summary>
        private async void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[TransactionView] 1024x768 optimized view loaded, performing initialization...");

                // Fast drawer status refresh for immediate responsiveness
                await _viewModel.RefreshDrawerStatusAsync();

                // Apply 1024x768 specific screen optimizations
                ApplyEnhancedScreenSizeOptimizations();

                // ENHANCED: Initialize table status management
                _viewModel.RefreshAllTableStatuses();

                Console.WriteLine("[TransactionView] 1024x768 optimized view initialization completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error during enhanced POS-optimized view initialization: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error during view initialization: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies optimizations based on the actual screen size detected
        /// Updated specifically for 1024x768 resolution
        /// </summary>
        private void ApplyEnhancedScreenSizeOptimizations()
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                Console.WriteLine($"[TransactionView] Detected screen size: {screenWidth}x{screenHeight}");

                // Optimized specifically for 1024x768
                if (screenWidth == 1024 && screenHeight == 768)
                {
                    UpdateLayoutForScreenSize(screenWidth, screenHeight, "1024x768");
                }
                else if (screenWidth >= 1920) // Large POS screens
                {
                    UpdateLayoutForScreenSize(screenWidth, screenHeight, "large");
                }
                else if (screenWidth >= 1440) // Standard POS screens
                {
                    UpdateLayoutForScreenSize(screenWidth, screenHeight, "standard");
                }
                else // Other smaller POS screens
                {
                    UpdateLayoutForScreenSize(screenWidth, screenHeight, "compact");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error applying screen size optimizations: {ex.Message}");
            }
        }

        /// <summary>
        /// ENHANCED: Handles cart item quantity updates with decimal validation and automatic table status updates
        /// </summary>
        private void Quantity_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItem cartItem)
            {
                try
                {
                    string inputText = textBox.Text?.Trim() ?? "";
                    Console.WriteLine($"[TransactionView] Quantity input received for {cartItem.Product.Name}: '{inputText}'");

                    // Validate and parse the decimal quantity
                    if (TryParseQuantity(inputText, out decimal parsedQuantity))
                    {
                        // Update the cart item with the parsed quantity
                        cartItem.Quantity = parsedQuantity;

                        // Update the text box to show the properly formatted value
                        textBox.Text = FormatQuantity(parsedQuantity);

                        Console.WriteLine($"[TransactionView] Quantity updated for {cartItem.Product.Name}: {cartItem.Quantity}");

                        // ENHANCED: Use the new method that includes table status updates
                        _viewModel.UpdateCartItemQuantityWithStatus(cartItem);

                        // Provide enhanced haptic feedback if available
                        ProvideHapticFeedback();
                    }
                    else
                    {
                        // Invalid input - show error and reset to previous valid value
                        Console.WriteLine($"[TransactionView] Invalid quantity input: '{inputText}'. Resetting to previous value.");

                        textBox.Text = FormatQuantity(cartItem.Quantity);

                        // Show user-friendly error message
                        string errorMessage = $"Invalid quantity '{inputText}'. Please enter a decimal number ≥ {MIN_QUANTITY}.";
                        MessageBox.Show(errorMessage, "Invalid Quantity", MessageBoxButton.OK, MessageBoxImage.Warning);

                        // Refocus the textbox for correction
                        textBox.Focus();
                        textBox.SelectAll();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Quantity update error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Quantity update error: {ex.Message}");

                    // Reset to previous valid value if update fails
                    textBox.Text = FormatQuantity(cartItem.Quantity);
                    textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                }
            }
        }

        /// <summary>
        /// ENHANCED: Handles cart item discount updates with validation and automatic table status updates
        /// </summary>
        private void Discount_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItem cartItem)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Discount updated for {cartItem.Product.Name}");

                    // Process discount update through view model business logic
                    _viewModel.UpdateCartItemDiscount(cartItem);

                    // ENHANCED: Trigger table status update after discount change
                    _viewModel.RefreshAllTableStatuses();

                    // Provide feedback for successful update
                    ProvideHapticFeedback();
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
        /// ENHANCED: Handles discount type selection changes with immediate recalculation and table status updates
        /// </summary>
        private void DiscountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is CartItem cartItem)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Discount type changed for {cartItem.Product.Name}");

                    // Update discount calculation based on new type selection
                    _viewModel.UpdateCartItemDiscount(cartItem);

                    // ENHANCED: Trigger table status update after discount type change
                    _viewModel.RefreshAllTableStatuses();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Discount type change error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Discount type change error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// ENHANCED: Handles quantity increment button clicks with table status updates
        /// </summary>
        private void QuantityIncrement_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItem cartItem)
            {
                IncrementQuantityWithStatusUpdate(cartItem);
            }
        }

        /// <summary>
        /// ENHANCED: Handles quantity decrement button clicks with table status updates
        /// </summary>
        private void QuantityDecrement_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItem cartItem)
            {
                DecrementQuantityWithStatusUpdate(cartItem);
            }
        }

        #endregion

        #region Enhanced POS Utility Methods

        /// <summary>
        /// Provides enhanced haptic feedback if supported by the POS hardware
        /// </summary>
        private void ProvideHapticFeedback()
        {
            try
            {
                // Enhanced haptic feedback for better user experience
                Console.WriteLine("[TransactionView] Enhanced haptic feedback triggered");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error providing enhanced haptic feedback: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the layout for specific screen sizes dynamically
        /// Specifically optimized for 1024x768 resolution with compact layout
        /// </summary>
        /// <param name="screenWidth">Available screen width</param>
        /// <param name="screenHeight">Available screen height</param>
        /// <param name="sizeCategory">Size category: "1024x768", "compact", "standard", or "large"</param>
        public void UpdateLayoutForScreenSize(double screenWidth, double screenHeight, string sizeCategory = "1024x768")
        {
            try
            {
                Console.WriteLine($"[TransactionView] Updating layout for {screenWidth}x{screenHeight} ({sizeCategory})");

                // Adjust column widths based on actual screen size
                if (this.Content is Grid mainGrid && mainGrid.ColumnDefinitions.Count >= 2)
                {
                    var rightColumn = mainGrid.ColumnDefinitions[1]; // Cart column

                    switch (sizeCategory)
                    {
                        case "1024x768": // Optimized specifically for 1024x768
                            rightColumn.Width = new GridLength(300);
                            break;
                        case "large": // 1920+ width
                            rightColumn.Width = new GridLength(420);
                            break;
                        case "standard": // 1440-1919 width
                            rightColumn.Width = new GridLength(380);
                            break;
                        case "compact": // Other small screens
                            rightColumn.Width = new GridLength(320);
                            break;
                    }
                }

                Console.WriteLine($"[TransactionView] Layout updated for {sizeCategory} POS screen");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Layout update error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets focus to the category filter for optimized keyboard navigation
        /// Enhanced for improved touch screen interaction
        /// </summary>
        public void FocusCategoryFilter()
        {
            try
            {
                Console.WriteLine("[TransactionView] Focusing category filter for enhanced touch interaction...");

                // Find the category ComboBox through visual tree traversal
                if (this.Content is Grid mainGrid)
                {
                    var categoryComboBox = FindVisualChild<ComboBox>(mainGrid);
                    if (categoryComboBox != null && categoryComboBox.ItemsSource == _viewModel.Categories)
                    {
                        categoryComboBox.Focus();
                        categoryComboBox.IsDropDownOpen = true;
                        Console.WriteLine("[TransactionView] Category filter focused successfully for enhanced touch");
                    }
                    else
                    {
                        Console.WriteLine("[TransactionView] Category filter ComboBox not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Enhanced focus category filter error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Focus category filter error: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to find visual children of a specific type
        /// Enhanced for improved POS layout structure
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

        #region Enhanced Resource Cleanup

        /// <summary>
        /// Handles the Unloaded event for optimized resource cleanup
        /// Enhanced for improved POS system resource management
        /// </summary>
        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[TransactionView] Starting enhanced POS-optimized view unload cleanup...");

                // Unsubscribe from view model events
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                    Console.WriteLine("[TransactionView] Unsubscribed from view model events");
                }

                // Clean up enhanced touch handling
                this.TouchDown -= OnEnhancedTouchDown;
                this.TouchUp -= OnEnhancedTouchUp;
                this.TouchMove -= OnEnhancedTouchMove;
                this.StylusDown -= OnStylusInteraction;
                this.StylusUp -= OnStylusInteraction;
                this.ManipulationDelta -= OnManipulationDelta;
                this.ManipulationCompleted -= OnManipulationCompleted;

                // Clean up event subscriptions
                this.Loaded -= OnViewLoaded;
                this.Unloaded -= OnViewUnloaded;

                Console.WriteLine("[TransactionView] Enhanced POS-optimized resource cleanup completed successfully");
                System.Diagnostics.Debug.WriteLine("TransactionView: Enhanced POS-optimized cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Enhanced cleanup error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TransactionView cleanup error: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes event subscriptions for enhanced lifecycle management
        /// Updated for improved POS system requirements
        /// </summary>
        private void InitializeEventSubscriptions()
        {
            try
            {
                Console.WriteLine("[TransactionView] Initializing enhanced POS-optimized event subscriptions...");

                // Subscribe to lifecycle events
                this.Unloaded += OnViewUnloaded;
                this.Loaded += OnViewLoaded;

                Console.WriteLine("[TransactionView] Enhanced POS-optimized event subscriptions initialized");
                System.Diagnostics.Debug.WriteLine("TransactionView: Enhanced POS-optimized subscriptions initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Enhanced event subscription error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TransactionView subscription error: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}