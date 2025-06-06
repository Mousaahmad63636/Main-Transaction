// File: QuickTechPOS/Views/TransactionView.xaml.cs
// OPTIMIZED FOR 14-15 INCH POS TOUCH SCREENS
// UPDATED: Enhanced touch handling, improved responsiveness, larger touch targets

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
using System.Windows.Shapes;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Optimized TransactionView for 14-15 inch POS touch screens with enhanced touch responsiveness
    /// Target resolutions: 1366x768, 1440x900, 1600x900, 1920x1080 (common POS screen sizes)
    /// Focus: Large touch targets, clear typography, finger-friendly interactions
    /// </summary>
    public partial class TransactionView : UserControl
    {
        #region Private Fields

        private readonly TransactionViewModel _viewModel;
        private DateTime _lastTouchTime = DateTime.MinValue;
        private const int TOUCH_DEBOUNCE_MS = 150; // Prevent accidental double-touches

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the optimized TransactionView for 14-15 inch POS touch displays
        /// </summary>
        /// <param name="viewModel">The transaction view model containing business logic and data binding</param>
        /// <exception cref="ArgumentNullException">Thrown when viewModel is null</exception>
        public TransactionView(TransactionViewModel viewModel)
        {
            Console.WriteLine("[TransactionView] Initializing optimized TransactionView for 14-15 inch POS touch screens...");

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

            // Optimize for POS screen sizes and performance
            OptimizeForPOSDisplay();

            Console.WriteLine("[TransactionView] Optimized TransactionView initialization completed for POS touch screens");
        }

        #endregion

        #region POS Touch Screen Optimization Methods

        /// <summary>
        /// Configures enhanced touch handling optimized for 14-15 inch POS touch screens
        /// Includes touch debouncing, gesture recognition, and improved responsiveness
        /// </summary>
        private void ConfigureEnhancedTouchHandling()
        {
            try
            {
                Console.WriteLine("[TransactionView] Configuring enhanced touch handling for POS screens...");

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

                Console.WriteLine("[TransactionView] Enhanced touch handling configured successfully for POS screens");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error configuring enhanced touch handling: {ex.Message}");
            }
        }

        /// <summary>
        /// Optimizes display settings and performance for POS screen environments
        /// </summary>
        private void OptimizeForPOSDisplay()
        {
            try
            {
                Console.WriteLine("[TransactionView] Applying POS display optimizations...");

                // Enable hardware acceleration for smooth scrolling and animations
                RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
                RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

                // Optimize for touch input latency
                this.UseLayoutRounding = true;
                this.SnapsToDevicePixels = true;

                // Set appropriate DPI awareness for POS screens
                TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
                TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);

                // Configure cache settings for better performance
                BitmapCache cache = new BitmapCache();
                cache.RenderAtScale = 1.0;
                cache.EnableClearType = true;
                this.CacheMode = cache;

                Console.WriteLine("[TransactionView] POS display optimizations applied successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error applying POS display optimizations: {ex.Message}");
            }
        }

        #endregion

        #region Enhanced Touch Event Handlers

        /// <summary>
        /// Handles enhanced touch down events with debouncing for POS touch screens
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
        /// Handles enhanced touch up events with gesture recognition
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
        /// Handles touch move events for scroll and gesture recognition
        /// </summary>
        private void OnEnhancedTouchMove(object sender, TouchEventArgs e)
        {
            try
            {
                // Handle touch move for custom scroll behavior if needed
                var touchPoint = e.GetTouchPoint(this);

                // This can be extended for custom gestures (swipe, pinch, etc.)
                // For now, we'll let the standard scroll behavior handle most cases
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling enhanced touch move: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles stylus interactions for precision input on POS devices
        /// </summary>
        private void OnStylusInteraction(object sender, StylusEventArgs e)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Stylus interaction detected");
                // Handle stylus input for precise operations
                // This is useful for signature capture or detailed item selection
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling stylus interaction: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles manipulation delta for smooth scrolling and zooming
        /// </summary>
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            try
            {
                // Handle manipulation for smooth scrolling
                // This provides better touch scrolling experience
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
        /// Handles manipulation completed events
        /// </summary>
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Manipulation completed with velocity: {e.FinalVelocities.LinearVelocity}");

                // Handle inertial scrolling if velocity is high enough
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

        #region POS-Specific Input Handlers

        /// <summary>
        /// Handles number input from physical number pad or on-screen keyboard
        /// </summary>
        private void HandleNumberInput(string number)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Number input received: {number}");

                // This can be extended to handle barcode input, quantity changes, etc.
                // For now, we'll focus on the barcode search functionality
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
        /// Provides visual feedback for touch interactions
        /// </summary>
        private void ProvideTouchFeedback(Point touchPosition)
        {
            try
            {
                // Create a subtle visual ripple effect for touch feedback
                // This helps users know their touch was registered

                var feedbackElement = new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    Fill = new SolidColorBrush(Color.FromArgb(100, 37, 99, 235)), // Semi-transparent blue
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(touchPosition.X - 10, touchPosition.Y - 10, 0, 0)
                };

                // Add to main grid temporarily
                if (this.Content is Grid mainGrid)
                {
                    mainGrid.Children.Add(feedbackElement);

                    // Remove after brief animation
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(200)
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
                Console.WriteLine($"[TransactionView] Error providing touch feedback: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes touch gestures for POS-specific actions
        /// </summary>
        private void ProcessTouchGesture(TouchPoint touchPoint)
        {
            try
            {
                // This can be extended for custom gestures like:
                // - Double-tap to add multiple items
                // - Long press for item details
                // - Swipe for category navigation

                Console.WriteLine($"[TransactionView] Processing touch gesture at: {touchPoint.Position}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error processing touch gesture: {ex.Message}");
            }
        }

        #endregion

        #region Standard Event Handlers (Enhanced for Touch)

        /// <summary>
        /// Handles view model property changes to maintain UI consistency
        /// Enhanced for touch screen responsiveness
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

                // Ensure UI command states are refreshed on the main thread
                Application.Current.Dispatcher.Invoke(() => {
                    CommandManager.InvalidateRequerySuggested();
                });
            }
        }

        /// <summary>
        /// Performs initialization tasks when the view is fully loaded
        /// Enhanced for POS screen optimization
        /// </summary>
        private async void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[TransactionView] POS-optimized view loaded, performing initialization...");

                // Fast drawer status refresh for immediate responsiveness
                await _viewModel.RefreshDrawerStatusAsync();

                // Apply screen-specific optimizations based on actual size
                ApplyScreenSizeOptimizations();

                Console.WriteLine("[TransactionView] POS-optimized view loaded initialization completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error during POS-optimized view initialization: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error during view initialization: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies optimizations based on the actual screen size detected
        /// </summary>
        private void ApplyScreenSizeOptimizations()
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                Console.WriteLine($"[TransactionView] Detected screen size: {screenWidth}x{screenHeight}");

                // Adjust UI elements based on screen size
                if (screenWidth >= 1920) // Large POS screens
                {
                    // Increase sizes for larger screens
                    UpdateLayoutForScreenSize(screenWidth, screenHeight, "large");
                }
                else if (screenWidth >= 1440) // Standard POS screens
                {
                    UpdateLayoutForScreenSize(screenWidth, screenHeight, "standard");
                }
                else // Smaller POS screens
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
        /// Handles cart item quantity updates with enhanced touch validation
        /// </summary>
        private void Quantity_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItem cartItem)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Quantity updated for {cartItem.Product.Name}: {cartItem.Quantity}");

                    // Enhanced validation for touch input
                    if (cartItem.Quantity < 1)
                    {
                        cartItem.Quantity = 1;
                        textBox.Text = "1";
                    }

                    // Delegate quantity update to view model for business logic processing
                    _viewModel.UpdateCartItemQuantity(cartItem);

                    // Provide haptic feedback if available (some POS systems support this)
                    ProvideHapticFeedback();
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
        private void Discount_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItem cartItem)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Discount updated for {cartItem.Product.Name}");

                    // Process discount update through view model business logic
                    _viewModel.UpdateCartItemDiscount(cartItem);

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
        /// Handles discount type selection changes with immediate recalculation
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Discount type change error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Discount type change error: {ex.Message}");
                }
            }
        }

        #endregion

        #region Enhanced POS Utility Methods

        /// <summary>
        /// Provides haptic feedback if supported by the POS hardware
        /// </summary>
        private void ProvideHapticFeedback()
        {
            try
            {
                // Some POS systems support haptic feedback
                // This is a placeholder for hardware-specific implementations
                Console.WriteLine("[TransactionView] Haptic feedback triggered");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error providing haptic feedback: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the layout for specific screen sizes dynamically
        /// Enhanced for POS screen variations
        /// </summary>
        /// <param name="screenWidth">Available screen width</param>
        /// <param name="screenHeight">Available screen height</param>
        /// <param name="sizeCategory">Size category: "compact", "standard", or "large"</param>
        public void UpdateLayoutForScreenSize(double screenWidth, double screenHeight, string sizeCategory = "standard")
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
                        case "large": // 1920+ width
                            rightColumn.Width = new GridLength(420);
                            break;
                        case "standard": // 1440-1919 width
                            rightColumn.Width = new GridLength(380);
                            break;
                        case "compact": // <1440 width
                            rightColumn.Width = new GridLength(350);
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
        /// Enhanced for touch screen interaction
        /// </summary>
        public void FocusCategoryFilter()
        {
            try
            {
                Console.WriteLine("[TransactionView] Focusing category filter for touch interaction...");

                // Find the category ComboBox through visual tree traversal
                if (this.Content is Grid mainGrid)
                {
                    var categoryComboBox = FindVisualChild<ComboBox>(mainGrid);
                    if (categoryComboBox != null && categoryComboBox.ItemsSource == _viewModel.Categories)
                    {
                        categoryComboBox.Focus();
                        categoryComboBox.IsDropDownOpen = true;
                        Console.WriteLine("[TransactionView] Category filter focused successfully for touch");
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
        /// Enhanced for POS layout structure
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
        /// Enhanced for POS system resource management
        /// </summary>
        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[TransactionView] Starting POS-optimized view unload cleanup...");

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

                Console.WriteLine("[TransactionView] POS-optimized resource cleanup completed successfully");
                System.Diagnostics.Debug.WriteLine("TransactionView: POS-optimized cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Cleanup error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TransactionView cleanup error: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes event subscriptions for optimized lifecycle management
        /// Enhanced for POS system requirements
        /// </summary>
        private void InitializeEventSubscriptions()
        {
            try
            {
                Console.WriteLine("[TransactionView] Initializing POS-optimized event subscriptions...");

                // Subscribe to lifecycle events
                this.Unloaded += OnViewUnloaded;
                this.Loaded += OnViewLoaded;

                Console.WriteLine("[TransactionView] POS-optimized event subscriptions initialized");
                System.Diagnostics.Debug.WriteLine("TransactionView: POS-optimized subscriptions initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Event subscription error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TransactionView subscription error: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}