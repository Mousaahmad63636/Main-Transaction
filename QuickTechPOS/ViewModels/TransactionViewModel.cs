using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.Services;
using QuickTechPOS.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickTechPOS.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        private readonly ProductService _productService;
        private readonly TransactionService _transactionService;
        private readonly CustomerService _customerService;
        private readonly AuthenticationService _authService;
        private readonly DrawerService _drawerService;
        private readonly ReceiptPrinterService _receiptPrinterService;
        private readonly BusinessSettingsService _businessSettingsService;

        private string _lastSearchQuery;
        private System.Timers.Timer _searchTimer;
        private System.Timers.Timer _customerSearchTimer;
        private string _barcodeQuery;
        private string _nameQuery;
        private string _customerQuery;
        private decimal _totalAmount;
        private string _customerName;
        private int _customerId;
        private string _statusMessage;
        private bool _isProcessing;
        private CartItem _selectedCartItem;
        private Product _selectedProduct;
        private Customer _selectedCustomer;
        private ObservableCollection<Product> _searchedProducts;
        private ObservableCollection<Customer> _searchedCustomers;
        private ObservableCollection<CartItem> _cartItems;
        private readonly CustomerProductPriceService _customerPriceService;
        private readonly Customer _walkInCustomer;
        private string _transactionLookupId;
        private Transaction _loadedTransaction;
        private bool _isTransactionLoaded;
        private bool _isEditMode;
        private bool _canNavigateNext;
        private bool _canNavigatePrevious;
        private decimal _paidAmount;
        private bool _addToCustomerDebt;
        private decimal _amountToDebt;
        private Drawer _currentDrawer;
        private bool _useExchangeRate;
        private decimal _exchangeRate;
        private decimal _exchangeAmount;

        public string BarcodeQuery
        {
            get => _barcodeQuery;
            set => SetProperty(ref _barcodeQuery, value);
        }

        public Drawer CurrentDrawer
        {
            get => _currentDrawer;
            private set => SetProperty(ref _currentDrawer, value);
        }

        public string NameQuery
        {
            get => _nameQuery;
            set => SetProperty(ref _nameQuery, value);
        }

        public string CustomerQuery
        {
            get => _customerQuery;
            set => SetProperty(ref _customerQuery, value);
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public int CustomerId
        {
            get => _customerId;
            set => SetProperty(ref _customerId, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public CartItem SelectedCartItem
        {
            get => _selectedCartItem;
            set
            {
                if (SetProperty(ref _selectedCartItem, value))
                {
                    OnPropertyChanged(nameof(CanRemoveItem));
                }
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value) && value != null)
                {
                    CustomerId = value.CustomerId;
                    CustomerName = value.Name;
                }
            }
        }

        public ObservableCollection<Product> SearchedProducts
        {
            get => _searchedProducts;
            set => SetProperty(ref _searchedProducts, value);
        }

        public ObservableCollection<Customer> SearchedCustomers
        {
            get => _searchedCustomers;
            set => SetProperty(ref _searchedCustomers, value);
        }

        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        public string TransactionLookupId
        {
            get => _transactionLookupId;
            set => SetProperty(ref _transactionLookupId, value);
        }

        public Transaction LoadedTransaction
        {
            get => _loadedTransaction;
            set => SetProperty(ref _loadedTransaction, value);
        }

        public bool IsTransactionLoaded
        {
            get => _isTransactionLoaded;
            set => SetProperty(ref _isTransactionLoaded, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool CanNavigateNext
        {
            get => _canNavigateNext;
            set => SetProperty(ref _canNavigateNext, value);
        }

        public bool CanNavigatePrevious
        {
            get => _canNavigatePrevious;
            set => SetProperty(ref _canNavigatePrevious, value);
        }

        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                if (SetProperty(ref _paidAmount, value))
                {
                    CalculateAmountToDebt();
                }
            }
        }

        public bool AddToCustomerDebt
        {
            get => _addToCustomerDebt;
            set
            {
                if (SetProperty(ref _addToCustomerDebt, value))
                {
                    CalculateAmountToDebt();
                }
            }
        }

        public decimal AmountToDebt
        {
            get => _amountToDebt;
            private set => SetProperty(ref _amountToDebt, value);
        }

        public bool UseExchangeRate
        {
            get => _useExchangeRate;
            set
            {
                if (SetProperty(ref _useExchangeRate, value))
                {
                    CalculateExchangeAmount();
                }
            }
        }

        public decimal ExchangeRate
        {
            get => _exchangeRate;
            set
            {
                if (SetProperty(ref _exchangeRate, value))
                {
                    CalculateExchangeAmount();
                }
            }
        }

        public decimal ExchangeAmount
        {
            get => _exchangeAmount;
            private set => SetProperty(ref _exchangeAmount, value);
        }

        public bool CanCheckout => CartItems.Count > 0;

        public bool CanRemoveItem => SelectedCartItem != null;

        public bool IsDrawerOpen => CurrentDrawer != null && CurrentDrawer.Status == "Open";

        public ICommand SearchBarcodeCommand { get; }
        public ICommand SearchNameCommand { get; }
        public ICommand SelectCustomerCommand { get; }
        public ICommand SearchCustomersCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand AddCustomerCommand { get; }
        public ICommand LookupTransactionCommand { get; }
        public ICommand EditTransactionCommand { get; }
        public ICommand SaveTransactionCommand { get; }
        public ICommand PrintReceiptCommand { get; }
        public ICommand NextTransactionCommand { get; }
        public ICommand PreviousTransactionCommand { get; }
        public ICommand CashOutCommand { get; }
        public ICommand OpenDrawerCommand { get; }
        public ICommand CloseDrawerCommand { get; }

        public TransactionViewModel(AuthenticationService authService, Customer walkInCustomer = null)
        {
            _productService = new ProductService();
            _transactionService = new TransactionService();
            _customerService = new CustomerService();
            _customerPriceService = new CustomerProductPriceService();
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _walkInCustomer = walkInCustomer;
            _drawerService = new DrawerService();
            _receiptPrinterService = new ReceiptPrinterService();
            _businessSettingsService = new BusinessSettingsService();
            _exchangeRate = 90000; // Default value until loaded from DB

            SearchedProducts = new ObservableCollection<Product>();
            SearchedCustomers = new ObservableCollection<Customer>();
            CartItems = new ObservableCollection<CartItem>();

            CustomerName = "Walk-in Customer";
            if (_walkInCustomer != null)
            {
                CustomerId = _walkInCustomer.CustomerId;
                Console.WriteLine($"Using walk-in customer with ID: {CustomerId}");
            }
            else
            {
                CustomerId = 0;
                Console.WriteLine("No walk-in customer provided. Customer ID set to 0.");
            }

            _searchTimer = new System.Timers.Timer(300);
            _searchTimer.Elapsed += OnSearchTimerElapsed;
            _searchTimer.AutoReset = false;

            _customerSearchTimer = new System.Timers.Timer(300);
            _customerSearchTimer.Elapsed += OnCustomerSearchTimerElapsed;
            _customerSearchTimer.AutoReset = false;

            PaidAmount = 0;
            AddToCustomerDebt = false;
            AmountToDebt = 0;

            // Initialize commands
            CashOutCommand = new RelayCommand(async param => await ShowCashOutDialogAsync(), param => IsDrawerOpen);
            SearchBarcodeCommand = new RelayCommand(async param => await SearchByBarcodeAsync());
            SearchNameCommand = new RelayCommand(async param => await SearchByNameAsync());
            SearchCustomersCommand = new RelayCommand(async param => await SearchCustomersAsync());
            AddToCartCommand = new RelayCommand(param => AddToCart(param as Product));
            RemoveFromCartCommand = new RelayCommand(param => RemoveFromCart(param as CartItem), param => param != null);
            ClearCartCommand = new RelayCommand(param => ClearCart());
            CheckoutCommand = new RelayCommand(async param => await CheckoutAsync(), param => CanCheckout);
            AddCustomerCommand = new RelayCommand(param => OpenAddCustomerDialog());
            SelectCustomerCommand = new RelayCommand(param => OpenCustomerSelectionDialog());
            OpenDrawerCommand = new RelayCommand(async param => await OpenDrawerDialogAsync());
            CloseDrawerCommand = new RelayCommand(async param => await CloseDrawerDialogAsync(), param => IsDrawerOpen);

            LookupTransactionCommand = new RelayCommand(async param =>
            {
                if (param != null)
                {
                    TransactionLookupId = param.ToString();
                }
                await LookupTransactionAsync();
            });

            EditTransactionCommand = new RelayCommand(param => EnterEditMode(), param => IsTransactionLoaded && !IsEditMode);
            SaveTransactionCommand = new RelayCommand(async param => await SaveTransactionChangesAsync(), param => IsTransactionLoaded && IsEditMode);
            PrintReceiptCommand = new RelayCommand(param => PrintReceipt(), param => IsTransactionLoaded);

            NextTransactionCommand = new RelayCommand(async param => await NavigateToNextTransactionAsync(), param => CanNavigateNext);
            PreviousTransactionCommand = new RelayCommand(async param => await NavigateToPreviousTransactionAsync(), param => CanNavigatePrevious);

            LoadInitialProductsAsync();
            LoadInitialCustomersAsync();
            GetCurrentDrawerAsync().ConfigureAwait(false);
            LoadExchangeRateAsync();
        }

        private async void LoadExchangeRateAsync()
        {
            try
            {
                ExchangeRate = await _businessSettingsService.GetExchangeRateAsync();
                Console.WriteLine($"Loaded exchange rate: {ExchangeRate}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading exchange rate: {ex.Message}");
            }
        }

        private void CalculateExchangeAmount()
        {
            if (UseExchangeRate && ExchangeRate > 0)
            {
                ExchangeAmount = TotalAmount * ExchangeRate;
            }
            else
            {
                ExchangeAmount = 0;
            }
        }

        private void CalculateAmountToDebt()
        {
            if (AddToCustomerDebt && PaidAmount < TotalAmount)
            {
                AmountToDebt = TotalAmount - PaidAmount;
            }
            else
            {
                AmountToDebt = 0;
            }
        }

        private async Task GetCurrentDrawerAsync()
        {
            try
            {
                if (_authService.CurrentEmployee == null)
                {
                    CurrentDrawer = null;
                    return;
                }

                string cashierId = _authService.CurrentEmployee.EmployeeId.ToString();
                var drawer = await _drawerService.GetOpenDrawerAsync(cashierId);

                if (drawer != null && drawer.Notes == null)
                {
                    drawer.Notes = string.Empty;
                }

                CurrentDrawer = drawer;
                OnPropertyChanged(nameof(IsDrawerOpen));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current drawer: {ex.Message}");
                StatusMessage = $"Error retrieving drawer information: {ex.Message}";
            }
        }

        private async Task ShowCashOutDialogAsync()
        {
            try
            {
                await GetCurrentDrawerAsync();

                if (!IsDrawerOpen)
                {
                    StatusMessage = "No open drawer found.";
                    return;
                }

                var dialog = new CashOutDialog(CurrentDrawer);

                if (dialog.ShowDialog() == true)
                {
                    var updatedDrawer = dialog.UpdatedDrawer;
                    StatusMessage = $"Cash out operation completed successfully.";

                    await GetCurrentDrawerAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error during cash out: {ex.Message}";
                Console.WriteLine($"Cash out dialog error: {ex}");
            }
        }

        private async Task OpenDrawerDialogAsync()
        {
            try
            {
                await GetCurrentDrawerAsync();

                if (IsDrawerOpen)
                {
                    StatusMessage = $"Drawer already open with ID #{CurrentDrawer.DrawerId}";
                    return;
                }

                var viewModel = new OpenDrawerViewModel(_authService);
                var dialog = new OpenDrawerDialog(viewModel);

                if (dialog.ShowDialog() == true)
                {
                    await GetCurrentDrawerAsync();
                    StatusMessage = "Drawer opened successfully.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening drawer: {ex.Message}";
                Console.WriteLine($"Open drawer dialog error: {ex}");
            }
        }

        private async Task CloseDrawerDialogAsync()
        {
            try
            {
                await GetCurrentDrawerAsync();

                if (!IsDrawerOpen)
                {
                    StatusMessage = "No open drawer found.";
                    return;
                }

                var dialog = new CloseDrawerDialog(CurrentDrawer);

                if (dialog.ShowDialog() == true)
                {
                    var closedDrawer = dialog.ClosedDrawer;

                    // Generate and print the receipt
                    string receiptContent = await _receiptPrinterService.GenerateDrawerReportAsync(closedDrawer);
                    bool printed = await _receiptPrinterService.PrintReceiptAsync(receiptContent);

                    if (printed)
                    {
                        StatusMessage = $"Drawer #{closedDrawer.DrawerId} closed and report printed.";
                    }
                    else
                    {
                        StatusMessage = $"Drawer #{closedDrawer.DrawerId} closed but failed to print report.";
                    }

                    await GetCurrentDrawerAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error closing drawer: {ex.Message}";
                Console.WriteLine($"Close drawer dialog error: {ex}");
            }
        }

        private async void LoadInitialProductsAsync()
        {
            try
            {
                StatusMessage = "Loading products...";
                var products = await _productService.SearchByNameAsync("");
                SearchedProducts.Clear();

                foreach (var product in products)
                {
                    SearchedProducts.Add(product);
                }

                if (products.Count > 0)
                {
                    StatusMessage = "Ready.";
                }
                else
                {
                    StatusMessage = "No products available.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Console.WriteLine($"Error loading initial products: {ex}");
            }
        }

        private void OpenCustomerSelectionDialog()
        {
            var customerDialog = new Views.CustomerSelectionDialog();
            if (customerDialog.ShowDialog() == true)
            {
                var selectedCustomer = customerDialog.SelectedCustomer;
                if (selectedCustomer != null)
                {
                    SetSelectedCustomer(selectedCustomer);
                    StatusMessage = $"Selected customer: {selectedCustomer.Name}";
                }
            }
        }

        private async void LoadInitialCustomersAsync()
        {
            try
            {
                var customers = await _customerService.SearchCustomersAsync("");
                SearchedCustomers.Clear();

                foreach (var customer in customers)
                {
                    SearchedCustomers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading customers: {ex.Message}";
                Console.WriteLine($"Error loading initial customers: {ex}");
            }
        }

        private async void OnSearchTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await SearchByNameAsync();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in search timer: {ex}");
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    StatusMessage = $"Search error: {ex.Message}";
                });
            }
        }

        private async void OnCustomerSearchTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await SearchCustomersAsync();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in customer search timer: {ex}");
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    StatusMessage = $"Customer search error: {ex.Message}";
                });
            }
        }

        public void StartSearchAsYouType()
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        public void UpdateNameQuery(string query)
        {
            NameQuery = query;

            if (string.IsNullOrWhiteSpace(query))
            {
                _lastSearchQuery = query;
                LoadInitialProductsAsync();
                return;
            }

            if (_lastSearchQuery == query)
                return;

            _lastSearchQuery = query;
            StartSearchAsYouType();
        }

        public void UpdateCustomerQuery(string query)
        {
            CustomerQuery = query;

            _customerSearchTimer.Stop();
            _customerSearchTimer.Start();
        }

        private async Task SearchByBarcodeAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BarcodeQuery))
                {
                    LoadInitialProductsAsync();
                    return;
                }

                StatusMessage = "Searching by barcode...";

                var product = await _productService.GetByBarcodeAsync(BarcodeQuery);

                if (product != null)
                {
                    AddToCart(product);
                    BarcodeQuery = string.Empty;
                    StatusMessage = $"Added {product.Name} to cart.";
                }
                else
                {
                    StatusMessage = "No product found with this barcode.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Console.WriteLine($"Barcode search error: {ex}");
            }
        }

        private async Task SearchByNameAsync()
        {
            try
            {
                StatusMessage = "Searching...";

                if (string.IsNullOrWhiteSpace(NameQuery))
                {
                    var allProducts = await _productService.SearchByNameAsync("");

                    System.Windows.Application.Current.Dispatcher.Invoke(() => {
                        SearchedProducts.Clear();
                        foreach (var product in allProducts)
                        {
                            SearchedProducts.Add(product);
                        }
                    });

                    StatusMessage = "Ready";
                    return;
                }

                var products = await _productService.SearchByNameAsync(NameQuery);

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    SearchedProducts.Clear();

                    if (products.Count > 0)
                    {
                        foreach (var product in products)
                        {
                            SearchedProducts.Add(product);
                        }
                        StatusMessage = $"Found {products.Count} products matching '{NameQuery}'";
                    }
                    else
                    {
                        StatusMessage = "No products found matching your search.";
                    }
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Console.WriteLine($"Search error: {ex}");
            }
        }

        private async Task SearchCustomersAsync()
        {
            try
            {
                var customers = await _customerService.SearchCustomersAsync(CustomerQuery);

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    SearchedCustomers.Clear();

                    foreach (var customer in customers)
                    {
                        SearchedCustomers.Add(customer);
                    }
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching customers: {ex.Message}";
                Console.WriteLine($"Customer search error: {ex}");
            }
        }

        public async void SetSelectedCustomer(Customer customer)
        {
            if (customer == null)
                return;

            try
            {
                Console.WriteLine($"Setting selected customer: {customer.Name} (ID: {customer.CustomerId})");

                SelectedCustomer = customer;
                CustomerId = customer.CustomerId;
                CustomerName = customer.Name;

                OnPropertyChanged(nameof(SelectedCustomer));
                OnPropertyChanged(nameof(CustomerId));
                OnPropertyChanged(nameof(CustomerName));

                if (CartItems.Count > 0)
                {
                    await ApplyCustomerPricingToCartAsync();
                }

                StatusMessage = $"Selected customer: {customer.Name}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting customer: {ex.Message}");
                StatusMessage = $"Error selecting customer: {ex.Message}";
            }
        }

        private async Task ApplyCustomerPricingToCartAsync()
        {
            if (CustomerId <= 0 || CartItems.Count == 0)
                return;

            var customerPrices = await _customerPriceService.GetAllPricesForCustomerAsync(CustomerId);

            if (customerPrices.Count == 0)
                return;

            foreach (var item in CartItems)
            {
                if (customerPrices.TryGetValue(item.Product.ProductId, out decimal specialPrice))
                {
                    if (item.UnitPrice != specialPrice)
                    {
                        item.UnitPrice = specialPrice;
                        Console.WriteLine($"Updated price for {item.Product.Name}: ${specialPrice}");
                    }
                }
            }

            UpdateTotals();
            StatusMessage = "Updated prices based on customer-specific pricing.";
        }

        public void SetSelectedCustomerAndFillSearch(Customer customer)
        {
            if (customer == null)
                return;

            try
            {
                Console.WriteLine($"Setting selected customer: {customer.Name} (ID: {customer.CustomerId})");

                SelectedCustomer = customer;
                CustomerId = customer.CustomerId;
                CustomerName = customer.Name;
                CustomerQuery = customer.Name;

                OnPropertyChanged(nameof(SelectedCustomer));
                OnPropertyChanged(nameof(CustomerId));
                OnPropertyChanged(nameof(CustomerName));
                OnPropertyChanged(nameof(CustomerQuery));

                StatusMessage = $"Selected customer: {customer.Name}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting customer: {ex.Message}");
                StatusMessage = $"Error selecting customer: {ex.Message}";
            }
        }

        private void OpenAddCustomerDialog()
        {
            var newCustomerDialog = new Views.AddCustomerDialog();
            if (newCustomerDialog.ShowDialog() == true)
            {
                var newCustomer = newCustomerDialog.Customer;
                if (newCustomer != null)
                {
                    SetSelectedCustomer(newCustomer);
                    StatusMessage = $"Added new customer: {newCustomer.Name}";
                }
            }
        }

        public void AddSelectedProductToCart(Product product)
        {
            if (product == null)
                return;

            AddToCart(product);
            NameQuery = string.Empty;
        }

        private async void AddToCart(Product product)
        {
            if (product == null)
                return;

            decimal unitPrice = product.SalePrice;
            if (CustomerId > 0)
            {
                var specialPrice = await _customerPriceService.GetCustomerProductPriceAsync(CustomerId, product.ProductId);
                if (specialPrice.HasValue)
                {
                    unitPrice = specialPrice.Value;
                    Console.WriteLine($"Applied customer-specific price for {product.Name}: ${unitPrice}");
                }
            }

            var existingItem = CartItems.FirstOrDefault(i => i.Product.ProductId == product.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += 1;
                UpdateCartItemQuantity(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    Product = product,
                    Quantity = 1,
                    UnitPrice = unitPrice,
                    Discount = 0,
                    DiscountType = 0
                };

                CartItems.Add(newItem);
            }

            UpdateTotals();
            StatusMessage = $"Added {product.Name} to cart.";
        }

        public void UpdateCartItemQuantity(CartItem cartItem)
        {
            if (cartItem == null)
                return;

            if (cartItem.Quantity <= 0)
                cartItem.Quantity = 1;

            if (cartItem.DiscountType == 1)
            {
                decimal percentage = (cartItem.Discount / (cartItem.Quantity * cartItem.UnitPrice - cartItem.Discount)) * 100;
                cartItem.Discount = (percentage / 100) * (cartItem.Quantity * cartItem.UnitPrice);
            }

            UpdateTotals();
        }

        public void UpdateCartItemDiscount(CartItem cartItem)
        {
            if (cartItem == null)
                return;

            if (cartItem.DiscountType == 0 && cartItem.Discount > cartItem.Subtotal)
                cartItem.Discount = cartItem.Subtotal;

            if (cartItem.DiscountType == 1)
            {
                decimal percentage = cartItem.DiscountValue;
                if (percentage > 100)
                    cartItem.DiscountValue = 100;
            }

            UpdateTotals();
        }

        private void RemoveFromCart(CartItem cartItem = null)
        {
            var itemToRemove = cartItem ?? SelectedCartItem;

            if (itemToRemove == null)
                return;

            CartItems.Remove(itemToRemove);
            UpdateTotals();
            StatusMessage = "Item removed from cart.";

            if (SelectedCartItem == itemToRemove)
                SelectedCartItem = null;
        }

        private void ClearCart()
        {
            CartItems.Clear();
            UpdateTotals();
            StatusMessage = "Cart cleared.";

            LoadedTransaction = null;
            IsTransactionLoaded = false;
            IsEditMode = false;
        }

        private void UpdateTotals()
        {
            TotalAmount = CartItems.Sum(i => i.Total);

            if (PaidAmount <= 0 || PaidAmount > TotalAmount)
            {
                PaidAmount = TotalAmount;
            }

            CalculateExchangeAmount();
            CalculateAmountToDebt();

            OnPropertyChanged(nameof(CanCheckout));
        }

        private async Task CheckoutAsync()
        {
            try
            {
                if (CartItems.Count == 0)
                {
                    StatusMessage = "Cart is empty. Cannot checkout.";
                    return;
                }

                if (PaidAmount < 0)
                {
                    StatusMessage = "Payment amount cannot be negative.";
                    return;
                }

                if (AddToCustomerDebt && AmountToDebt > 0 && (CustomerId <= 0 || CustomerName == "Walk-in Customer"))
                {
                    StatusMessage = "Cannot add debt to a walk-in customer. Please select a registered customer.";
                    return;
                }

                IsProcessing = true;
                StatusMessage = "Processing transaction...";
                Console.WriteLine($"Starting checkout process at: {DateTime.Now}");
                Console.WriteLine($"Cart contains {CartItems.Count} items, total amount: {TotalAmount:C2}, paid amount: {PaidAmount:C2}");

                if (AddToCustomerDebt && AmountToDebt > 0)
                {
                    Console.WriteLine($"Adding {AmountToDebt:C2} to customer debt (Customer ID: {CustomerId})");
                }

                var currentEmployee = _authService.CurrentEmployee;
                if (currentEmployee == null)
                {
                    StatusMessage = "Error: No cashier is logged in.";
                    IsProcessing = false;
                    Console.WriteLine("Checkout failed: No cashier logged in");
                    return;
                }

                int customerIdForTransaction;
                string customerNameForTransaction;

                if (CustomerId <= 0 || CustomerName == "Walk-in Customer")
                {
                    if (_walkInCustomer != null)
                    {
                        customerIdForTransaction = _walkInCustomer.CustomerId;
                        customerNameForTransaction = _walkInCustomer.Name;
                        Console.WriteLine($"Using walk-in customer: ID={customerIdForTransaction}, Name={customerNameForTransaction}");
                    }
                    else
                    {
                        var dbService = new DatabaseService();
                        var walkInCustomer = await dbService.EnsureWalkInCustomerExistsAsync();

                        if (walkInCustomer != null)
                        {
                            customerIdForTransaction = walkInCustomer.CustomerId;
                            customerNameForTransaction = walkInCustomer.Name;
                            Console.WriteLine($"Retrieved walk-in customer: ID={customerIdForTransaction}, Name={customerNameForTransaction}");
                        }
                        else
                        {
                            StatusMessage = "Error: Cannot proceed without a valid customer ID. Please select a customer.";
                            IsProcessing = false;
                            Console.WriteLine("Checkout failed: No valid walk-in customer");
                            return;
                        }
                    }
                }
                else
                {
                    customerIdForTransaction = CustomerId;
                    customerNameForTransaction = CustomerName;
                    Console.WriteLine($"Using selected customer: ID={customerIdForTransaction}, Name={customerNameForTransaction}");
                }

                Console.WriteLine($"Cashier: {currentEmployee.FullName} (ID: {currentEmployee.EmployeeId})");
                Console.WriteLine($"Customer: {customerNameForTransaction} (ID: {customerIdForTransaction})");

                var transaction = await _transactionService.CreateTransactionAsync(
                    CartItems.ToList(),
                    PaidAmount,
                    currentEmployee,
                    "Cash",
                    customerNameForTransaction,
                    customerIdForTransaction
                );

                if (transaction == null || transaction.TransactionId <= 0)
                {
                    StatusMessage = "Error: Failed to create transaction record.";
                    IsProcessing = false;
                    Console.WriteLine("Checkout failed: Transaction record creation failed");
                    return;
                }

                Console.WriteLine($"Transaction #{transaction.TransactionId} created successfully");
                Console.WriteLine($"- Total Amount: {transaction.TotalAmount:C2}, Paid Amount: {transaction.PaidAmount:C2}");

                // Generate and print receipt
                string receiptContent = await _receiptPrinterService.GenerateTransactionReceiptAsync(
                    transaction,
                    CartItems.ToList(),
                    ExchangeRate,
                    UseExchangeRate);

                bool printed = await _receiptPrinterService.PrintReceiptAsync(receiptContent);

                if (printed)
                {
                    Console.WriteLine("Receipt printed successfully");
                }
                else
                {
                    Console.WriteLine("Failed to print receipt");
                }

                // After successful transaction, update the drawer
                try
                {
                    if (CurrentDrawer != null && CurrentDrawer.Status == "Open")
                    {
                        // Update the drawer with the transaction amount
                        await _drawerService.UpdateDrawerTransactionsAsync(
                            CurrentDrawer.DrawerId,
                            transaction.TotalAmount, // Sales amount
                            0, // No expenses
                            0  // No supplier payments
                        );

                        Console.WriteLine($"Updated drawer #{CurrentDrawer.DrawerId} with sales amount: {transaction.TotalAmount:C2}");

                        // Also refresh the drawer data
                        await GetCurrentDrawerAsync();
                    }
                    else
                    {
                        Console.WriteLine("Warning: No open drawer found to update for this transaction");
                    }
                }
                catch (Exception drawerEx)
                {
                    // Log the error but don't fail the transaction
                    Console.WriteLine($"Error updating drawer: {drawerEx.Message}");
                    StatusMessage += " (Warning: Drawer update failed)";
                }

                if (AddToCustomerDebt && AmountToDebt > 0 && customerIdForTransaction > 0 && CustomerName != "Walk-in Customer")
                {
                    var customerService = new CustomerService();
                    bool balanceUpdated = await customerService.UpdateCustomerBalanceAsync(customerIdForTransaction, AmountToDebt);

                    if (balanceUpdated)
                    {
                        Console.WriteLine($"Added {AmountToDebt:C2} to customer balance successfully");
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Failed to update customer balance");
                        StatusMessage = $"Transaction completed but failed to update customer balance. Please check customer account.";
                    }
                }

                TransactionLookupId = transaction.TransactionId.ToString();

                CartItems.Clear();
                UpdateTotals();

                if (CustomerId > 0 && CustomerName != "Walk-in Customer")
                {
                    if (_walkInCustomer != null)
                    {
                        CustomerId = _walkInCustomer.CustomerId;
                    }
                    else
                    {
                        CustomerId = 0;
                    }

                    CustomerName = "Walk-in Customer";
                    SelectedCustomer = null;
                    OnPropertyChanged(nameof(CustomerName));
                    OnPropertyChanged(nameof(CustomerId));
                }

                PaidAmount = 0;
                AddToCustomerDebt = false;
                AmountToDebt = 0;
                UseExchangeRate = false;

                await LookupTransactionAsync();

                LoadInitialProductsAsync();

                StatusMessage = $"Transaction #{transaction.TransactionId} completed successfully.";
                if (printed)
                {
                    StatusMessage += " Receipt printed.";
                }
                if (AddToCustomerDebt && AmountToDebt > 0)
                {
                    StatusMessage += $" Added {AmountToDebt:C2} to customer balance.";
                }

                Console.WriteLine("Checkout process completed successfully");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error during checkout: {ex.Message}";
                if (ex.InnerException != null)
                {
                    StatusMessage += $" Inner exception: {ex.InnerException.Message}";
                }

                Console.WriteLine($"Checkout error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task LookupTransactionAsync()
        {
            IsEditMode = false;

            try
            {
                if (string.IsNullOrWhiteSpace(TransactionLookupId) || !int.TryParse(TransactionLookupId, out int transactionId))
                {
                    StatusMessage = "Please enter a valid transaction ID.";
                    return;
                }

                IsProcessing = true;
                StatusMessage = $"Looking up transaction #{transactionId}...";

                CartItems.Clear();

                var transaction = await _transactionService.GetTransactionWithDetailsAsync(transactionId);

                if (transaction == null)
                {
                    StatusMessage = $"Transaction #{transactionId} not found.";
                    LoadedTransaction = null;
                    IsTransactionLoaded = false;
                    CanNavigateNext = false;
                    CanNavigatePrevious = false;
                    return;
                }

                await LoadTransactionToCartAsync(transaction);

                LoadedTransaction = transaction;
                IsTransactionLoaded = true;

                if (transaction.CustomerId > 0)
                {
                    CustomerId = transaction.CustomerId;
                    CustomerName = transaction.CustomerName;

                    var customer = await _customerService.GetByIdAsync(transaction.CustomerId);
                    if (customer != null)
                    {
                        SelectedCustomer = customer;
                    }
                }
                else
                {
                    CustomerId = 0;
                    CustomerName = transaction.CustomerName ?? "Walk-in Customer";
                    SelectedCustomer = null;
                }

                await CheckNavigationAvailabilityAsync(transactionId);

                StatusMessage = $"Loaded transaction #{transactionId} completed on {transaction.FormattedDate}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error looking up transaction: {ex.Message}";
                Console.WriteLine($"Transaction lookup error: {ex}");
                LoadedTransaction = null;
                IsTransactionLoaded = false;
                CanNavigateNext = false;
                CanNavigatePrevious = false;
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task LoadTransactionToCartAsync(Transaction transaction)
        {
            if (transaction == null || transaction.Details == null || !transaction.Details.Any())
                return;

            CartItems.Clear();

            foreach (var detail in transaction.Details)
            {
                var product = await GetProductForTransactionDetailAsync(detail);

                var cartItem = new CartItem
                {
                    Product = product,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    Discount = detail.Discount
                };

                CartItems.Add(cartItem);
            }

            UpdateTotals();
        }

        private async Task<Product> GetProductForTransactionDetailAsync(TransactionDetail detail)
        {
            try
            {
                var dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
                var product = await dbContext.Products.FindAsync(detail.ProductId);

                if (product != null)
                    return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving product: {ex.Message}");
            }

            return new Product
            {
                ProductId = detail.ProductId,
                Name = "Product #" + detail.ProductId,
                SalePrice = detail.UnitPrice,
                PurchasePrice = detail.PurchasePrice
            };
        }

        private void EnterEditMode()
        {
            if (LoadedTransaction == null)
                return;

            IsEditMode = true;
            StatusMessage = $"Editing transaction #{LoadedTransaction.TransactionId}. Make changes and click Save Changes when done.";
        }

        private async Task SaveTransactionChangesAsync()
        {
            try
            {
                if (LoadedTransaction == null)
                    return;

                IsProcessing = true;
                StatusMessage = $"Saving changes to transaction #{LoadedTransaction.TransactionId}...";

                var updatedTransaction = new Transaction
                {
                    TransactionId = LoadedTransaction.TransactionId,
                    CustomerId = CustomerId,
                    CustomerName = CustomerName,
                    TotalAmount = TotalAmount,
                    PaidAmount = LoadedTransaction.PaidAmount,
                    TransactionDate = LoadedTransaction.TransactionDate,
                    TransactionType = LoadedTransaction.TransactionType,
                    Status = LoadedTransaction.Status,
                    PaymentMethod = LoadedTransaction.PaymentMethod,
                    CashierId = LoadedTransaction.CashierId,
                    CashierName = LoadedTransaction.CashierName,
                    CashierRole = LoadedTransaction.CashierRole
                };

                var updatedDetails = CartItems.Select(item => new TransactionDetail
                {
                    TransactionId = LoadedTransaction.TransactionId,
                    ProductId = item.Product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    PurchasePrice = item.Product.PurchasePrice,
                    Discount = item.Discount,
                    Total = item.Total
                }).ToList();

                bool success = await _transactionService.UpdateTransactionAsync(updatedTransaction, updatedDetails);

                if (success)
                {
                    StatusMessage = $"Transaction #{LoadedTransaction.TransactionId} updated successfully.";

                    LoadedTransaction = updatedTransaction;
                    LoadedTransaction.Details = updatedDetails;

                    IsEditMode = false;
                }
                else
                {
                    StatusMessage = $"Failed to update transaction #{LoadedTransaction.TransactionId}.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating transaction: {ex.Message}";
                Console.WriteLine($"Transaction update error: {ex}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async void PrintReceipt()
        {
            try
            {
                if (LoadedTransaction == null)
                {
                    StatusMessage = "No transaction loaded to print.";
                    return;
                }

                StatusMessage = $"Printing receipt for transaction #{LoadedTransaction.TransactionId}...";

                // Generate and print receipt for loaded transaction
                string receiptContent = await _receiptPrinterService.GenerateTransactionReceiptAsync(
                    LoadedTransaction,
                    CartItems.ToList(),
                    ExchangeRate,
                    UseExchangeRate);

                bool printed = await _receiptPrinterService.PrintReceiptAsync(receiptContent);

                if (printed)
                {
                    StatusMessage = $"Receipt for transaction #{LoadedTransaction.TransactionId} printed successfully.";
                }
                else
                {
                    StatusMessage = $"Failed to print receipt for transaction #{LoadedTransaction.TransactionId}.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error printing receipt: {ex.Message}";
                Console.WriteLine($"Print receipt error: {ex}");
            }
        }

        private async Task CheckNavigationAvailabilityAsync(int currentTransactionId)
        {
            try
            {
                var nextId = await _transactionService.GetNextTransactionIdAsync(currentTransactionId);
                CanNavigateNext = nextId.HasValue;

                var prevId = await _transactionService.GetPreviousTransactionIdAsync(currentTransactionId);
                CanNavigatePrevious = prevId.HasValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking navigation availability: {ex.Message}");
                CanNavigateNext = false;
                CanNavigatePrevious = false;
            }
        }

        private async Task NavigateToNextTransactionAsync()
        {
            try
            {
                if (!IsTransactionLoaded || LoadedTransaction == null)
                    return;

                int currentId = LoadedTransaction.TransactionId;
                var nextId = await _transactionService.GetNextTransactionIdAsync(currentId);

                if (nextId.HasValue)
                {
                    TransactionLookupId = nextId.Value.ToString();
                    await LookupTransactionAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error navigating to next transaction: {ex.Message}";
                Console.WriteLine($"Next transaction navigation error: {ex}");
            }
        }

        private async Task NavigateToPreviousTransactionAsync()
        {
            try
            {
                if (!IsTransactionLoaded || LoadedTransaction == null)
                    return;

                int currentId = LoadedTransaction.TransactionId;
                var prevId = await _transactionService.GetPreviousTransactionIdAsync(currentId);

                if (prevId.HasValue)
                {
                    TransactionLookupId = prevId.Value.ToString();
                    await LookupTransactionAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error navigating to previous transaction: {ex.Message}";
                Console.WriteLine($"Previous transaction navigation error: {ex}");
            }
        }
    }
}