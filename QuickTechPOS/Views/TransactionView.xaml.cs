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
    public partial class TransactionView : UserControl
    {
        #region Private Fields

        private readonly TransactionViewModel _viewModel;
        private readonly TableStatusSynchronizationService _tableStatusService;
        private DateTime _lastTouchTime = DateTime.MinValue;
        private const int TOUCH_DEBOUNCE_MS = 150;
        private const decimal MIN_QUANTITY = 0.1m;
        private const decimal QUANTITY_INCREMENT = 0.5m;

        #endregion

        #region Constructor

        public TransactionView(TransactionViewModel viewModel)
        {
            Console.WriteLine("[TransactionView] Initializing enhanced TransactionView with table status management...");

            InitializeComponent();

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _tableStatusService = TableStatusSynchronizationServiceSingleton.Instance;

            this.FlowDirection = LanguageManager.CurrentFlowDirection;
            DataContext = _viewModel;

            InitializeEventSubscriptions();
            InitializeTableStatusIntegration();

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            ConfigureEnhancedTouchHandling();
            OptimizeForPOSDisplay();

            Console.WriteLine("[TransactionView] Enhanced TransactionView initialization completed with table status management");
        }

        #endregion

        #region Table Status Integration

        private void InitializeTableStatusIntegration()
        {
            try
            {
                Console.WriteLine("[TransactionView] Initializing enhanced table status integration...");

                _tableStatusService.TableStatusChanged += OnTableStatusChanged;

                if (_viewModel.SelectedTable != null)
                {
                    var itemCount = _viewModel.CartItems?.Count ?? 0;
                    _tableStatusService.InitializeTableStatusAsync(_viewModel.SelectedTable.Id, itemCount);
                }

                Console.WriteLine("[TransactionView] Enhanced table status integration initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error initializing enhanced table status integration: {ex.Message}");
            }
        }

        private async void OnTableStatusChanged(object sender, TableStatusSynchronizationService.TableStatusChangedEventArgs e)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Enhanced table status changed: Table {e.TableId} from '{e.OldStatus}' to '{e.NewStatus}' ({e.ItemCount} items)");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (_viewModel.SelectedTable?.Id == e.TableId)
                    {
                        _viewModel.SelectedTable.Status = e.NewStatus;
                    }

                    if (_viewModel.ActiveTables != null)
                    {
                        var activeTable = _viewModel.ActiveTables.FirstOrDefault(t => t.Id == e.TableId);
                        if (activeTable != null)
                        {
                            activeTable.Status = e.NewStatus;
                        }
                    }

                    _viewModel.NotifyTableStatusChanged();
                    CommandManager.InvalidateRequerySuggested();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling enhanced table status change: {ex.Message}");
            }
        }

        private async Task UpdateTableStatusForCurrentCart()
        {
            try
            {
                if (_viewModel.SelectedTable != null)
                {
                    var itemCount = _viewModel.CartItems?.Count ?? 0;

                    Console.WriteLine($"[TransactionView] Updating enhanced table status for {_viewModel.SelectedTable.DisplayName}: {itemCount} items");

                    await _tableStatusService.UpdateTableItemCountAsync(_viewModel.SelectedTable.Id, itemCount);

                    string expectedStatus = itemCount > 0 ? "Occupied" : "Available";
                    string statusColor = itemCount > 0 ? "Red" : "Green";

                    Console.WriteLine($"[TransactionView] Table {_viewModel.SelectedTable.DisplayName} should be {expectedStatus} ({statusColor})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error updating enhanced table status for current cart: {ex.Message}");
            }
        }

        #endregion

        #region Enhanced POS Touch Screen Optimization Methods

        private void ConfigureEnhancedTouchHandling()
        {
            try
            {
                Console.WriteLine("[TransactionView] Configuring enhanced touch handling for improved POS interaction...");

                this.IsManipulationEnabled = true;
                this.ManipulationDelta += OnManipulationDelta;
                this.ManipulationCompleted += OnManipulationCompleted;

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

                var selectTableBinding = new KeyBinding(
                    new RelayCommand(param => {
                        if (_viewModel.SelectTableCommand?.CanExecute(null) == true)
                            _viewModel.SelectTableCommand.Execute(null);
                    }),
                    Key.F5, ModifierKeys.None);
                this.InputBindings.Add(selectTableBinding);

                for (int i = 0; i <= 9; i++)
                {
                    var key = (Key)Enum.Parse(typeof(Key), $"NumPad{i}");
                    var numberBinding = new KeyBinding(
                        new RelayCommand(param => HandleNumberInput(param.ToString())),
                        key, ModifierKeys.None);
                    this.InputBindings.Add(numberBinding);
                }

                this.TouchDown += OnEnhancedTouchDown;
                this.TouchUp += OnEnhancedTouchUp;
                this.TouchMove += OnEnhancedTouchMove;
                this.StylusDown += OnStylusInteraction;
                this.StylusUp += OnStylusInteraction;

                Console.WriteLine("[TransactionView] Enhanced touch handling configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error configuring enhanced touch handling: {ex.Message}");
            }
        }

        private void Quantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                try
                {
                    // Allow navigation and editing keys
                    if (e.Key == Key.Back || e.Key == Key.Delete ||
                        e.Key == Key.Left || e.Key == Key.Right ||
                        e.Key == Key.Home || e.Key == Key.End ||
                        e.Key == Key.Tab)
                    {
                        textBox.Tag = null; // Clear validation state
                        return;
                    }

                    // Enter key saves and moves to next
                    if (e.Key == Key.Enter)
                    {
                        ProcessQuantityUpdate(textBox);
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                        return;
                    }

                    // Escape key cancels edit
                    if (e.Key == Key.Escape)
                    {
                        if (textBox.DataContext is CartItem cartItem)
                        {
                            textBox.Text = FormatQuantity(cartItem.Quantity);
                            textBox.Tag = null;
                        }
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                        return;
                    }

                    // Block function keys and other special keys
                    if (e.Key >= Key.F1 && e.Key <= Key.F24)
                    {
                        e.Handled = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Error in KeyDown: {ex.Message}");
                }
            }
        }

        private async Task ProcessQuantityUpdate(TextBox textBox)
        {
            if (!(textBox.DataContext is CartItem cartItem))
                return;

            try
            {
                string inputText = textBox.Text?.Trim() ?? "";
                Console.WriteLine($"[TransactionView] Processing quantity update for {cartItem.Product.Name}: '{inputText}'");

                // Handle empty input
                if (string.IsNullOrEmpty(inputText))
                {
                    textBox.Text = FormatQuantity(cartItem.Quantity);
                    textBox.Tag = null;
                    return;
                }

                if (TryParseQuantity(inputText, out decimal parsedQuantity))
                {
                    // Valid input
                    decimal oldQuantity = cartItem.Quantity;
                    cartItem.Quantity = parsedQuantity;

                    // Format the display value
                    textBox.Text = FormatQuantity(parsedQuantity);
                    textBox.Tag = "Valid";

                    Console.WriteLine($"[TransactionView] Quantity updated for {cartItem.Product.Name}: {oldQuantity} → {parsedQuantity}");

                    // Update the cart and table status
                    _viewModel.UpdateCartItemQuantityWithStatus(cartItem);
                    await UpdateTableStatusForCurrentCart();
                    ProvideHapticFeedback();

                    // Clear validation state after a short delay
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(1000)
                    };
                    timer.Tick += (s, e) =>
                    {
                        textBox.Tag = null;
                        timer.Stop();
                    };
                    timer.Start();
                }
                else
                {
                    // Invalid input - show error and reset
                    Console.WriteLine($"[TransactionView] Invalid quantity input: '{inputText}'. Resetting to previous value.");

                    textBox.Tag = "Invalid";

                    // Show error message
                    string errorMessage = $"Invalid quantity '{inputText}'.\n\nPlease enter a number between {MIN_QUANTITY} and 999,999.\nExamples: 1, 1.5, 2,3";
                    MessageBox.Show(errorMessage, "Invalid Quantity", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Reset to previous valid value
                    textBox.Text = FormatQuantity(cartItem.Quantity);

                    // Keep focus and select all for easy correction
                    textBox.Focus();
                    textBox.SelectAll();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error processing quantity update: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Quantity update error: {ex.Message}");

                // Reset to safe state
                textBox.Text = FormatQuantity(cartItem.Quantity);
                textBox.Tag = null;
            }
        }


        private void OptimizeForPOSDisplay()
        {
            try
            {
                Console.WriteLine("[TransactionView] Applying enhanced POS display optimizations...");

                RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
                RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

                this.UseLayoutRounding = true;
                this.SnapsToDevicePixels = true;

                TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
                TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
                TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);

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

        private void OnEnhancedTouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                var currentTime = DateTime.Now;
                var timeSinceLastTouch = (currentTime - _lastTouchTime).TotalMilliseconds;

                if (timeSinceLastTouch < TOUCH_DEBOUNCE_MS)
                {
                    e.Handled = true;
                    return;
                }

                _lastTouchTime = currentTime;

                var touchPoint = e.GetTouchPoint(this);
                Console.WriteLine($"[TransactionView] Enhanced touch down at: {touchPoint.Position} (Pressure: {touchPoint.Size})");

                ProvideTouchFeedback(touchPoint.Position);
                this.CaptureTouch(e.TouchDevice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling enhanced touch down: {ex.Message}");
            }
        }

        private void OnEnhancedTouchUp(object sender, TouchEventArgs e)
        {
            try
            {
                var touchPoint = e.GetTouchPoint(this);
                Console.WriteLine($"[TransactionView] Enhanced touch up at: {touchPoint.Position}");

                this.ReleaseTouchCapture(e.TouchDevice);
                ProcessTouchGesture(touchPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling enhanced touch up: {ex.Message}");
            }
        }

        private void OnEnhancedTouchMove(object sender, TouchEventArgs e)
        {
            try
            {
                var touchPoint = e.GetTouchPoint(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling enhanced touch move: {ex.Message}");
            }
        }

        private void OnStylusInteraction(object sender, StylusEventArgs e)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Stylus interaction detected for precise product selection");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error handling stylus interaction: {ex.Message}");
            }
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            try
            {
                if (Math.Abs(e.DeltaManipulation.Translation.Y) > 1)
                {
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

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Manipulation completed with velocity: {e.FinalVelocities.LinearVelocity}");

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

        private void HandleNumberInput(string number)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Number input received: {number}");

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

        private void ProvideTouchFeedback(Point touchPosition)
        {
            try
            {
                var feedbackElement = new Ellipse
                {
                    Width = 24,
                    Height = 24,
                    Fill = new SolidColorBrush(Color.FromArgb(120, 37, 99, 235)),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(touchPosition.X - 12, touchPosition.Y - 12, 0, 0)
                };

                if (this.Content is Grid mainGrid)
                {
                    mainGrid.Children.Add(feedbackElement);

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

        private void ProcessTouchGesture(TouchPoint touchPoint)
        {
            try
            {
                Console.WriteLine($"[TransactionView] Processing enhanced touch gesture at: {touchPoint.Position}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error processing enhanced touch gesture: {ex.Message}");
            }
        }

        #endregion

        #region Enhanced Decimal Quantity Support Methods with Table Status Updates

        private bool TryParseQuantity(string input, out decimal quantity)
        {
            quantity = 0;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                // First try with current culture (handles regional decimal separators)
                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out quantity))
                {
                    if (quantity >= MIN_QUANTITY && quantity <= 999999)
                    {
                        quantity = Math.Round(quantity, 2);
                        return true;
                    }
                }

                // Fallback: normalize comma to dot and try with invariant culture
                var normalizedInput = input.Replace(',', '.');
                if (decimal.TryParse(normalizedInput, NumberStyles.Number, CultureInfo.InvariantCulture, out quantity))
                {
                    if (quantity >= MIN_QUANTITY && quantity <= 999999)
                    {
                        quantity = Math.Round(quantity, 2);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error parsing quantity '{input}': {ex.Message}");
            }

            return false;
        }

        private string FormatQuantity(decimal quantity)
        {
            try
            {
                // Show clean format: "1" for whole numbers, "1.5" for decimals
                if (quantity == Math.Floor(quantity))
                {
                    return quantity.ToString("0", CultureInfo.CurrentCulture);
                }
                else
                {
                    return quantity.ToString("0.##", CultureInfo.CurrentCulture);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error formatting quantity {quantity}: {ex.Message}");
                return quantity.ToString();
            }
        }

        private async void IncrementQuantityWithStatusUpdate(CartItem cartItem)
        {
            if (cartItem == null) return;

            try
            {
                decimal newQuantity = cartItem.Quantity + QUANTITY_INCREMENT;
                cartItem.Quantity = Math.Round(newQuantity, 2);

                Console.WriteLine($"[TransactionView] Incremented quantity for {cartItem.Product.Name} to {cartItem.Quantity}");

                _viewModel.UpdateCartItemQuantityWithStatus(cartItem);
                await UpdateTableStatusForCurrentCart();
                ProvideHapticFeedback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error incrementing quantity: {ex.Message}");
            }
        }

        private async void DecrementQuantityWithStatusUpdate(CartItem cartItem)
        {
            if (cartItem == null) return;

            try
            {
                decimal newQuantity = cartItem.Quantity - QUANTITY_INCREMENT;

                if (newQuantity < MIN_QUANTITY)
                    newQuantity = MIN_QUANTITY;

                cartItem.Quantity = Math.Round(newQuantity, 2);

                Console.WriteLine($"[TransactionView] Decremented quantity for {cartItem.Product.Name} to {cartItem.Quantity}");

                _viewModel.UpdateCartItemQuantityWithStatus(cartItem);
                await UpdateTableStatusForCurrentCart();
                ProvideHapticFeedback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error decrementing quantity: {ex.Message}");
            }
        }

        #endregion

        #region Enhanced Event Handlers with Table Status Updates

        private async void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
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

                if (e.PropertyName == nameof(TransactionViewModel.CartItems) ||
                    e.PropertyName == nameof(TransactionViewModel.TotalAmount))
                {
                    try
                    {
                        await UpdateTableStatusForCurrentCart();
                        _viewModel.RefreshAllTableStatuses();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TransactionView] Error refreshing table statuses: {ex.Message}");
                    }
                }

                Application.Current.Dispatcher.Invoke(() => {
                    CommandManager.InvalidateRequerySuggested();
                });
            }
        }

        private async void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[TransactionView] Enhanced view loaded, performing initialization...");

                await _viewModel.RefreshDrawerStatusAsync();
                ApplyEnhancedScreenSizeOptimizations();
                await UpdateTableStatusForCurrentCart();
                _viewModel.RefreshAllTableStatuses();

                Console.WriteLine("[TransactionView] Enhanced view initialization completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error during enhanced view initialization: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error during view initialization: {ex.Message}");
            }
        }


        private void Quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                try
                {
                    // Get the full text that would result after this input
                    string currentText = textBox.Text ?? "";
                    int caretIndex = textBox.CaretIndex;

                    // Handle selection - if text is selected, it will be replaced
                    string selectedText = textBox.SelectedText ?? "";
                    string textBeforeCaret = currentText.Substring(0, textBox.SelectionStart);
                    string textAfterSelection = currentText.Substring(textBox.SelectionStart + textBox.SelectionLength);
                    string resultText = textBeforeCaret + e.Text + textAfterSelection;

                    Console.WriteLine($"[TransactionView] Input '{e.Text}', Current: '{currentText}', Result: '{resultText}'");

                    // Allow decimal separators (both comma and dot)
                    if (e.Text == "," || e.Text == ".")
                    {
                        // Check if there's already a decimal separator in the text that will remain
                        string textWithoutSelection = textBeforeCaret + textAfterSelection;
                        bool alreadyHasDecimal = textWithoutSelection.Contains(",") || textWithoutSelection.Contains(".");

                        if (alreadyHasDecimal)
                        {
                            Console.WriteLine($"[TransactionView] Blocking decimal separator - already exists");
                            e.Handled = true;
                            return;
                        }

                        Console.WriteLine($"[TransactionView] Allowing decimal separator");
                        return; // Allow the decimal separator
                    }

                    // Allow digits
                    if (char.IsDigit(e.Text[0]))
                    {
                        // Check if the resulting text would be valid
                        if (TryParseQuantity(resultText, out decimal testQuantity))
                        {
                            // Provide immediate visual feedback for valid input
                            textBox.Tag = "Valid";
                            Console.WriteLine($"[TransactionView] Allowing digit - valid result: {testQuantity}");
                        }
                        else
                        {
                            // Check if it's just too large or has too many decimals
                            if (decimal.TryParse(resultText.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsed))
                            {
                                if (parsed > 999999)
                                {
                                    Console.WriteLine($"[TransactionView] Blocking digit - result too large: {parsed}");
                                    e.Handled = true; // Prevent input if too large
                                    return;
                                }
                            }
                            Console.WriteLine($"[TransactionView] Allowing digit despite invalid parse");
                        }
                        return; // Allow the digit
                    }

                    // Block all other characters
                    Console.WriteLine($"[TransactionView] Blocking character: '{e.Text}'");
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Error in PreviewTextInput: {ex.Message}");
                    e.Handled = true;
                }
            }
        }
        private void ApplyEnhancedScreenSizeOptimizations()
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                Console.WriteLine($"[TransactionView] Detected screen size: {screenWidth}x{screenHeight}");

                if (screenWidth == 1024 && screenHeight == 768)
                {
                    UpdateLayoutForScreenSize(screenWidth, screenHeight, "1024x768");
                }
                else if (screenWidth >= 1920)
                {
                    UpdateLayoutForScreenSize(screenWidth, screenHeight, "large");
                }
                else if (screenWidth >= 1440)
                {
                    UpdateLayoutForScreenSize(screenWidth, screenHeight, "standard");
                }
                else
                {
                    UpdateLayoutForScreenSize(screenWidth, screenHeight, "compact");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error applying screen size optimizations: {ex.Message}");
            }
        }

        private void Quantity_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                try
                {
                    // Select all text for easy replacement
                    textBox.SelectAll();
                    textBox.Tag = null; // Clear any validation state

                    Console.WriteLine($"[TransactionView] Quantity field focused for editing");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Error in GotFocus: {ex.Message}");
                }
            }
        }

        private async void Quantity_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItem cartItem)
            {
                await ProcessQuantityUpdate(textBox);
            }
        }


        private async void Discount_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItem cartItem)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Discount updated for {cartItem.Product.Name}");

                    _viewModel.UpdateCartItemDiscount(cartItem);
                    await UpdateTableStatusForCurrentCart();
                    _viewModel.RefreshAllTableStatuses();

                    ProvideHapticFeedback();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Discount update error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Discount update error: {ex.Message}");

                    textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                }
            }
        }

        private async void DiscountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is CartItem cartItem)
            {
                try
                {
                    Console.WriteLine($"[TransactionView] Discount type changed for {cartItem.Product.Name}");

                    _viewModel.UpdateCartItemDiscount(cartItem);
                    await UpdateTableStatusForCurrentCart();
                    _viewModel.RefreshAllTableStatuses();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TransactionView] Discount type change error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Discount type change error: {ex.Message}");
                }
            }
        }

        private async void QuantityIncrement_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItem cartItem)
            {
                await Task.Run(() => IncrementQuantityWithStatusUpdate(cartItem));
            }
        }

        private async void QuantityDecrement_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItem cartItem)
            {
                await Task.Run(() => DecrementQuantityWithStatusUpdate(cartItem));
            }
        }

        #endregion

        #region Enhanced POS Utility Methods

        private void ProvideHapticFeedback()
        {
            try
            {
                Console.WriteLine("[TransactionView] Enhanced haptic feedback triggered");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Error providing enhanced haptic feedback: {ex.Message}");
            }
        }

        public void UpdateLayoutForScreenSize(double screenWidth, double screenHeight, string sizeCategory = "1024x768")
        {
            try
            {
                Console.WriteLine($"[TransactionView] Updating layout for {screenWidth}x{screenHeight} ({sizeCategory})");

                if (this.Content is Grid mainGrid && mainGrid.ColumnDefinitions.Count >= 2)
                {
                    var rightColumn = mainGrid.ColumnDefinitions[1];

                    switch (sizeCategory)
                    {
                        case "1024x768":
                            rightColumn.Width = new GridLength(300);
                            break;
                        case "large":
                            rightColumn.Width = new GridLength(420);
                            break;
                        case "standard":
                            rightColumn.Width = new GridLength(380);
                            break;
                        case "compact":
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

        public void FocusCategoryFilter()
        {
            try
            {
                Console.WriteLine("[TransactionView] Focusing category filter for enhanced touch interaction...");

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

        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("[TransactionView] Starting enhanced view unload cleanup...");

                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                    Console.WriteLine("[TransactionView] Unsubscribed from view model events");
                }

                if (_tableStatusService != null)
                {
                    _tableStatusService.TableStatusChanged -= OnTableStatusChanged;
                    Console.WriteLine("[TransactionView] Unsubscribed from table status events");
                }

                this.TouchDown -= OnEnhancedTouchDown;
                this.TouchUp -= OnEnhancedTouchUp;
                this.TouchMove -= OnEnhancedTouchMove;
                this.StylusDown -= OnStylusInteraction;
                this.StylusUp -= OnStylusInteraction;
                this.ManipulationDelta -= OnManipulationDelta;
                this.ManipulationCompleted -= OnManipulationCompleted;

                this.Loaded -= OnViewLoaded;
                this.Unloaded -= OnViewUnloaded;

                Console.WriteLine("[TransactionView] Enhanced resource cleanup completed successfully");
                System.Diagnostics.Debug.WriteLine("TransactionView: Enhanced cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionView] Enhanced cleanup error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TransactionView cleanup error: {ex.Message}");
            }
        }

        private void InitializeEventSubscriptions()
        {
            try
            {
                Console.WriteLine("[TransactionView] Initializing enhanced event subscriptions...");

                this.Unloaded += OnViewUnloaded;
                this.Loaded += OnViewLoaded;

                Console.WriteLine("[TransactionView] Enhanced event subscriptions initialized");
                System.Diagnostics.Debug.WriteLine("TransactionView: Enhanced subscriptions initialized");
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