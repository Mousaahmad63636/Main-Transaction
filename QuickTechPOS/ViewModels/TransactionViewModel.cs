// File: QuickTechPOS/ViewModels/TransactionViewModel.cs
// COMPLETE FILE: Enhanced with table navigation and multi-table support

using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.Models.Enums;
using QuickTechPOS.Services;
using QuickTechPOS.Views;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace QuickTechPOS.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        #region Table-Specific Data Storage

        /// <summary>
        /// Stores table-specific transaction data for each active table
        /// </summary>
        private readonly Dictionary<int, TableTransactionData> _tableTransactionData;

        /// <summary>
        /// Collection of currently active tables with transactions
        /// </summary>
        private ObservableCollection<RestaurantTable> _activeTables;

        /// <summary>
        /// Index of the currently selected table in the active tables collection
        /// </summary>
        private int _currentTableIndex = -1;

        /// <summary>
        /// Data structure to store table-specific transaction information
        /// </summary>
        private class TableTransactionData
        {
            public List<CartItem> CartItems { get; set; } = new List<CartItem>();
            public int CustomerId { get; set; } = 0;
            public string CustomerName { get; set; } = "Walk-in Customer";
            public Customer SelectedCustomer { get; set; }
            public decimal PaidAmount { get; set; } = 0;
            public bool AddToCustomerDebt { get; set; } = false;
            public decimal AmountToDebt { get; set; } = 0;
            public DateTime LastActivity { get; set; } = DateTime.Now;
            public string Notes { get; set; } = string.Empty;
        }

        #endregion

        #region Service Dependencies

        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly TransactionService _transactionService;
        private readonly CustomerService _customerService;
        private readonly AuthenticationService _authService;
        private readonly DrawerService _drawerService;
        private readonly ReceiptPrinterService _receiptPrinterService;
        private readonly BusinessSettingsService _businessSettingsService;
        private readonly CustomerProductPriceService _customerPriceService;
        private readonly Customer _walkInCustomer;
        private readonly PrintQueueManager _printQueueManager;

        #endregion

        #region Private Fields

        private bool _wholesaleMode;
        private string _barcodeQuery;
        private int _selectedCategoryId;
        private Category _selectedCategory;
        private string _customerQuery;
        private decimal _totalAmount;
        private ObservableCollection<HeldCart> _heldCarts;
        private int _nextCartId = 1;
        private string _customerName;
        private int _customerId;
        private string _statusMessage;
        private bool _isProcessing;
        private CartItem _selectedCartItem;
        private Product _selectedProduct;
        private Customer _selectedCustomer;
        private ObservableCollection<Product> _searchedProducts;
        private ObservableCollection<Customer> _searchedCustomers;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<CartItem> _cartItems;
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
        private int _pendingTransactionCount;
        private decimal _exchangeRate;
        private decimal _exchangeAmount;
        private decimal _changeDueAmount;
        private RestaurantTable _selectedTable;
        private string _tableDisplayText;
        private System.Timers.Timer _customerSearchTimer;

        #endregion

        #region Table Navigation Properties

        /// <summary>
        /// Gets the collection of active tables with transaction data
        /// </summary>
        public ObservableCollection<RestaurantTable> ActiveTables
        {
            get => _activeTables;
            private set => SetProperty(ref _activeTables, value);
        }

        /// <summary>
        /// Gets whether there are multiple active tables to navigate between
        /// </summary>
        public bool HasMultipleTables => ActiveTables?.Count > 1;

        /// <summary>
        /// Gets whether navigation to previous table is possible
        /// </summary>
        public bool CanNavigateToPreviousTable => _currentTableIndex > 0;

        /// <summary>
        /// Gets whether navigation to next table is possible
        /// </summary>
        public bool CanNavigateToNextTable => _currentTableIndex >= 0 && _currentTableIndex < (ActiveTables?.Count - 1 ?? 0);

        /// <summary>
        /// Gets the current table position information for display
        /// </summary>
        public string TableNavigationInfo
        {
            get
            {
                if (ActiveTables == null || ActiveTables.Count == 0)
                    return "No active tables";

                if (_currentTableIndex < 0)
                    return $"{ActiveTables.Count} table(s) available";

                return $"Table {_currentTableIndex + 1} of {ActiveTables.Count}";
            }
        }

        /// <summary>
        /// Gets detailed information about the current table's transaction
        /// </summary>
        public string CurrentTableInfo
        {
            get
            {
                if (SelectedTable == null) return "No table selected";

                var tableData = GetCurrentTableData();
                if (tableData == null) return SelectedTable.DisplayName;

                var itemCount = tableData.CartItems?.Count ?? 0;
                var totalAmount = tableData.CartItems?.Sum(i => i.Total) ?? 0;

                return $"{SelectedTable.DisplayName} - {itemCount} items (${totalAmount:F2})";
            }
        }

        #endregion

        #region Public Properties

        public RestaurantTable SelectedTable
        {
            get => _selectedTable;
            set
            {
                var oldTable = _selectedTable;
                if (SetProperty(ref _selectedTable, value))
                {
                    OnTableSelectionChanged(oldTable); // Pass previous table
                }
            }
        }


        public string TableDisplayText
        {
            get => _tableDisplayText;
            set => SetProperty(ref _tableDisplayText, value);
        }

        public decimal ChangeDueAmount
        {
            get => _changeDueAmount;
            private set => SetProperty(ref _changeDueAmount, value);
        }

        public string BarcodeQuery
        {
            get => _barcodeQuery;
            set => SetProperty(ref _barcodeQuery, value);
        }

        public int SelectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                Console.WriteLine($"[TransactionViewModel] SelectedCategoryId changing from {_selectedCategoryId} to {value}");

                if (SetProperty(ref _selectedCategoryId, value))
                {
                    Console.WriteLine($"[TransactionViewModel] SelectedCategoryId changed to {value}, triggering product refresh");
                    LoadProductsByCategoryAsync().ConfigureAwait(false);
                }
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                Console.WriteLine($"[TransactionViewModel] SelectedCategory changing to: {value?.Name ?? "null"}");

                if (SetProperty(ref _selectedCategory, value))
                {
                    if (value != null)
                    {
                        Console.WriteLine($"[TransactionViewModel] Setting SelectedCategoryId to {value.CategoryId} from SelectedCategory");
                        SelectedCategoryId = value.CategoryId;
                    }
                }
            }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public Drawer CurrentDrawer
        {
            get => _currentDrawer;
            private set => SetProperty(ref _currentDrawer, value);
        }

        public string DrawerStatusToolTip
        {
            get => IsDrawerOpen ? "Process payment and complete the transaction" : "Drawer must be open to complete a sale";
        }

        public ObservableCollection<HeldCart> HeldCarts
        {
            get => _heldCarts;
            set => SetProperty(ref _heldCarts, value);
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

        public int PendingTransactionCount
        {
            get => _pendingTransactionCount;
            set => SetProperty(ref _pendingTransactionCount, value);
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

                    // FIXED: Auto-save when customer changes
                    AutoSaveCurrentTableState();
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

                    // FIXED: Auto-save when payment amount changes
                    AutoSaveCurrentTableState();
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

                    // FIXED: Auto-save when debt option changes
                    AutoSaveCurrentTableState();
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

        public bool HasPendingTransactions => PendingTransactionCount > 0;
        public bool CanRemoveItem => SelectedCartItem != null;
        public bool IsDrawerOpen => CurrentDrawer != null && CurrentDrawer.Status == "Open";
        public bool CanCheckout => CartItems.Count > 0 && IsDrawerOpen;

        public bool WholesaleMode
        {
            get => _wholesaleMode;
            set
            {
                if (SetProperty(ref _wholesaleMode, value))
                {
                    UpdateCartWholesaleMode();
                    StatusMessage = value ?
                        "Switched to Wholesale mode. All items will use wholesale pricing." :
                        "Switched to Retail mode. All items will use regular pricing.";

                    Console.WriteLine($"[TransactionViewModel] Wholesale mode changed to: {value}");
                }
            }
        }

        #endregion

        #region Commands

        // Table Navigation Commands
        public ICommand NavigateToPreviousTableCommand { get; }
        public ICommand NavigateToNextTableCommand { get; }
        public ICommand ShowTableNavigationCommand { get; }
        public ICommand CloseCurrentTableCommand { get; }

        // Existing Commands
        public ICommand SearchBarcodeCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand SelectCustomerCommand { get; }
        public ICommand SearchCustomersCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand AddToCartAsBoxCommand { get; }
        public ICommand AddToCartAsWholesaleCommand { get; }
        public ICommand AddToCartAsWholesaleBoxCommand { get; }
        public ICommand HoldCartCommand { get; }
        public ICommand RestoreCartCommand { get; }
        public ICommand AddCustomerCommand { get; }
        public ICommand SelectTableCommand { get; }
        public ICommand LookupTransactionCommand { get; }
        public ICommand EditTransactionCommand { get; }
        public ICommand OpenRecoveryDialogCommand { get; }
        public ICommand PrintDrawerReportCommand { get; }
        public ICommand SaveTransactionCommand { get; }
        public ICommand PrintReceiptCommand { get; }
        public ICommand NextTransactionCommand { get; }
        public ICommand PreviousTransactionCommand { get; }
        public ICommand CashOutCommand { get; }
        public ICommand OpenDrawerCommand { get; }
        public ICommand CloseDrawerCommand { get; }
        public ICommand ViewPrintQueueCommand { get; }
        public ICommand CashInCommand { get; }

        #endregion

        #region Constructor

        public TransactionViewModel(AuthenticationService authService, Customer walkInCustomer = null)
        {
            Console.WriteLine("[TransactionViewModel] Initializing enhanced TransactionViewModel with table navigation...");

            // Initialize table-specific data storage
            _tableTransactionData = new Dictionary<int, TableTransactionData>();
            ActiveTables = new ObservableCollection<RestaurantTable>();

            // Initialize basic services
            _productService = new ProductService();
            _categoryService = new CategoryService();
            _customerService = new CustomerService();
            _customerPriceService = new CustomerProductPriceService();
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _walkInCustomer = walkInCustomer;
            _drawerService = new DrawerService();
            _receiptPrinterService = new ReceiptPrinterService();
            _businessSettingsService = new BusinessSettingsService();

            Console.WriteLine("[TransactionViewModel] Services initialized successfully");

            // Get the print queue manager from the App
            _printQueueManager = (Application.Current as App)?.PrintQueueManager;
            _transactionService = new TransactionService();

            // Initialize default values
            _exchangeRate = 90000;
            _selectedCategoryId = 0;

            // Initialize collections
            HeldCarts = new ObservableCollection<HeldCart>();
            SearchedProducts = new ObservableCollection<Product>();
            SearchedCustomers = new ObservableCollection<Customer>();
            Categories = new ObservableCollection<Category>();
            CartItems = new ObservableCollection<CartItem>();

            Console.WriteLine("[TransactionViewModel] Collections initialized");

            // Initialize customer information
            CustomerName = "Walk-in Customer";
            if (_walkInCustomer != null)
            {
                CustomerId = _walkInCustomer.CustomerId;
                Console.WriteLine($"[TransactionViewModel] Using walk-in customer with ID: {CustomerId}");
            }
            else
            {
                CustomerId = 0;
                Console.WriteLine("[TransactionViewModel] No walk-in customer provided. Customer ID set to 0.");
            }

            // Initialize customer search timer
            _customerSearchTimer = new System.Timers.Timer(300);
            _customerSearchTimer.Elapsed += OnCustomerSearchTimerElapsed;
            _customerSearchTimer.AutoReset = false;

            Console.WriteLine("[TransactionViewModel] Customer search timer initialized");

            // Initialize payment properties
            PaidAmount = 0;
            AddToCustomerDebt = false;
            AmountToDebt = 0;

            // Initialize table navigation commands
            NavigateToPreviousTableCommand = new RelayCommand(
                param => NavigateToPreviousTable(),
                param => CanNavigateToPreviousTable);

            NavigateToNextTableCommand = new RelayCommand(
                param => NavigateToNextTable(),
                param => CanNavigateToNextTable);

            ShowTableNavigationCommand = new RelayCommand(
                param => ShowTableNavigationOverview(),
                param => HasMultipleTables);

            CloseCurrentTableCommand = new RelayCommand(
                param => CloseCurrentTable(),
                param => SelectedTable != null);

            // Initialize existing commands
            LogoutCommand = new RelayCommand(param => Logout());
            SelectTableCommand = new RelayCommand(param => OpenTableSelectionDialog());

            // Transaction navigation commands
            NextTransactionCommand = new RelayCommand(
                async param => await NavigateToNextTransactionAsync(),
                param => IsTransactionLoaded && CanNavigateNext);

            PreviousTransactionCommand = new RelayCommand(
                async param => await NavigateToPreviousTransactionAsync(),
                param => IsTransactionLoaded && CanNavigatePrevious);

            // Cart management commands
            HoldCartCommand = new RelayCommand(param => HoldCurrentCart(), param => CanHoldCart());
            RestoreCartCommand = new RelayCommand(param => RestoreHeldCart(), param => CanRestoreCart());

            // Search commands
            SearchBarcodeCommand = new RelayCommand(async param => await SearchByBarcodeAsync());
            SelectCategoryCommand = new RelayCommand(param => SelectCategory(param as Category));
            SearchCustomersCommand = new RelayCommand(async param => await SearchCustomersAsync());

            Console.WriteLine("[TransactionViewModel] Search commands initialized");

            // Cart item commands
            AddToCartCommand = new RelayCommand(param => AddToCart(param as Product));
            RemoveFromCartCommand = new RelayCommand(param => RemoveFromCart(param as CartItem), param => param != null);
            ClearCartCommand = new RelayCommand(param => ClearCart());
            AddToCartAsBoxCommand = new RelayCommand(param => AddToCart(param as Product, true, false));
            AddToCartAsWholesaleCommand = new RelayCommand(param => AddToCart(param as Product, false, true));
            AddToCartAsWholesaleBoxCommand = new RelayCommand(param => AddToCart(param as Product, true, true));

            // Transaction commands
            CheckoutCommand = new RelayCommand(async param => await CheckoutAsync(), param => CanCheckout && IsDrawerOpen);
            PrintReceiptCommand = new RelayCommand(param => PrintReceipt(), param => IsTransactionLoaded);
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

            // Drawer commands
            PrintDrawerReportCommand = new RelayCommand(async param => await PrintDrawerReportAsync(), param => IsDrawerOpen);
            CashOutCommand = new RelayCommand(async param => await ShowCashOutDialogAsync(), param => IsDrawerOpen);
            OpenDrawerCommand = new RelayCommand(async param => await OpenDrawerDialogAsync());
            CloseDrawerCommand = new RelayCommand(async param => await CloseDrawerDialogAsync(), param => IsDrawerOpen);
            CashInCommand = new RelayCommand(async param => await ShowCashInDialogAsync(), param => IsDrawerOpen);

            // Recovery command
            OpenRecoveryDialogCommand = new RelayCommand(param => OpenRecoveryDialog());

            // Customer commands
            AddCustomerCommand = new RelayCommand(param => OpenAddCustomerDialog());
            SelectCustomerCommand = new RelayCommand(param => OpenCustomerSelectionDialog());

            // Print queue commands
            ViewPrintQueueCommand = new RelayCommand(param => ViewPrintQueue(), param => _printQueueManager != null);

            Console.WriteLine("[TransactionViewModel] All commands initialized");

            // Initial data loading
            LoadInitialDataAsync();
            GetCurrentDrawerAsync().ConfigureAwait(false);
            LoadExchangeRateAsync();
            CheckForFailedTransactionsAsync().ConfigureAwait(false);

            Console.WriteLine("[TransactionViewModel] Enhanced TransactionViewModel initialization completed");
        }

        #endregion

        #region Table Navigation Methods

        private TableTransactionData GetCurrentTableData()
        {
            if (SelectedTable == null) return null;

            // FIXED: Always ensure data exists before returning
            EnsureTableDataIsInitialized(SelectedTable);

            _tableTransactionData.TryGetValue(SelectedTable.Id, out var data);
            return data;
        }

        private TableTransactionData EnsureTableData(RestaurantTable table)
        {
            if (table == null) return null;

            if (!_tableTransactionData.ContainsKey(table.Id))
            {
                _tableTransactionData[table.Id] = new TableTransactionData();
                Console.WriteLine($"[TransactionViewModel] Created new transaction data for {table.DisplayName}");
            }

            return _tableTransactionData[table.Id];
        }
        #region FIXED: Auto-Save Table State Methods
        /// <summary>
        /// Automatically saves current table state when cart or customer data changes
        /// Prevents data loss and ensures table-specific isolation
        /// </summary>
        private void AutoSaveCurrentTableState()
        {
            if (SelectedTable != null)
            {
                SaveTableState(SelectedTable);
            }
        }

        /// <summary>
        /// Saves the current transaction state for the specified table with comprehensive data preservation.
        /// This method performs a deep copy of all cart items and preserves all customer and payment information
        /// to ensure table-specific transaction isolation and prevent data loss during table navigation.
        /// </summary>
        /// <param name="table">The restaurant table to save state for. If null, the method returns immediately.</param>
        /// <remarks>
        /// This method is critical for multi-table POS operations, ensuring that:
        /// - Cart items are deep-copied to prevent reference issues
        /// - Customer selections and payment data are preserved
        /// - Table-specific pricing and discounts are maintained
        /// - Transaction timestamps are updated for audit purposes
        /// </remarks>
        private void SaveTableState(RestaurantTable table)
        {
            if (table == null)
            {
                Console.WriteLine("[TransactionViewModel] SaveTableState: Cannot save state for null table");
                return;
            }

            try
            {
                Console.WriteLine($"[TransactionViewModel] Starting SaveTableState for {table.DisplayName} (ID: {table.Id})...");

                // Ensure table data structure exists in the dictionary
                var tableData = EnsureTableData(table);
                if (tableData == null)
                {
                    Console.WriteLine($"[TransactionViewModel] Failed to ensure table data for {table.DisplayName}");
                    StatusMessage = $"Error: Unable to save state for {table.DisplayName}";
                    return;
                }

                // Clear existing cart items to prevent duplication
                tableData.CartItems.Clear();

                // Deep copy all cart items with complete property preservation
                if (CartItems != null && CartItems.Count > 0)
                {
                    Console.WriteLine($"[TransactionViewModel] Copying {CartItems.Count} cart items for {table.DisplayName}...");

                    foreach (var originalItem in CartItems)
                    {
                        if (originalItem?.Product == null)
                        {
                            Console.WriteLine("[TransactionViewModel] Warning: Skipping null cart item or item with null product");
                            continue;
                        }

                        // Create a complete deep copy of the cart item
                        var copiedItem = new CartItem
                        {
                            // Core product and quantity information
                            Product = originalItem.Product,
                            Quantity = originalItem.Quantity,
                            UnitPrice = originalItem.UnitPrice,

                            // Discount and pricing information
                            Discount = originalItem.Discount,
                            DiscountType = originalItem.DiscountType,
                            DiscountValue = originalItem.DiscountValue,

                            // Product variant flags
                            IsBox = originalItem.IsBox,
                            IsWholesale = originalItem.IsWholesale,

                           

                            // Ensure calculated properties are preserved
                            // Note: Total property will be recalculated automatically via CartItem.Total getter
                        };

                        tableData.CartItems.Add(copiedItem);

                        Console.WriteLine($"[TransactionViewModel] Copied item: {originalItem.Product.Name} " +
                                        $"(Qty: {originalItem.Quantity}, Price: ${originalItem.UnitPrice:F2}, " +
                                        $"IsBox: {originalItem.IsBox}, IsWholesale: {originalItem.IsWholesale})");
                    }
                }
                else
                {
                    Console.WriteLine($"[TransactionViewModel] No cart items to save for {table.DisplayName}");
                }

                // Save comprehensive customer information
                tableData.CustomerId = CustomerId;
                tableData.CustomerName = !string.IsNullOrWhiteSpace(CustomerName) ? CustomerName : "Walk-in Customer";
                tableData.SelectedCustomer = SelectedCustomer;

                // Save complete payment and transaction state
                tableData.PaidAmount = PaidAmount;
                tableData.AddToCustomerDebt = AddToCustomerDebt;
                tableData.AmountToDebt = AmountToDebt;

                // Update activity timestamp for audit and session management
                tableData.LastActivity = DateTime.Now;

                // Calculate and log summary information
                var totalItemCount = tableData.CartItems.Count;
                var totalValue = tableData.CartItems.Sum(item => item.Total);
                var customerInfo = tableData.CustomerId > 0 ? $" (Customer: {tableData.CustomerName})" : " (Walk-in)";

                Console.WriteLine($"[TransactionViewModel] Successfully saved state for {table.DisplayName}: " +
                                 $"{totalItemCount} items, Total value: ${totalValue:F2}, " +
                                 $"Paid: ${tableData.PaidAmount:F2}, " +
                                 $"Debt: {(tableData.AddToCustomerDebt ? $"${tableData.AmountToDebt:F2}" : "None")}" +
                                 customerInfo);

                // Update the table's status if needed (for UI display purposes)
                if (totalItemCount > 0)
                {
                    // Mark table as having active transaction data
                    table.Status = "Occupied";
                }
                else if (totalItemCount == 0 && table.Status == "Occupied")
                {
                    // Clear status if no items remain
                    table.Status = "Available";
                }

                // Force UI refresh for table-related properties
                UpdateTableDisplayInformation();

                Console.WriteLine($"[TransactionViewModel] SaveTableState completed successfully for {table.DisplayName}");
            }
            catch (ArgumentNullException ex)
            {
                var errorMsg = $"Invalid argument while saving table state: {ex.ParamName}";
                Console.WriteLine($"[TransactionViewModel] {errorMsg} - {ex.Message}");
                StatusMessage = errorMsg;
            }
            catch (InvalidOperationException ex)
            {
                var errorMsg = $"Invalid operation during table state save: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] {errorMsg}");
                StatusMessage = errorMsg;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Unexpected error saving state for {table.DisplayName}: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] {errorMsg}");
                Console.WriteLine($"[TransactionViewModel] Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[TransactionViewModel] Inner exception: {ex.InnerException.Message}");
                }

                StatusMessage = $"Error saving table state: {ex.Message}";

                // Attempt to maintain system stability by ensuring table data exists even after error
                try
                {
                    EnsureTableData(table);
                }
                catch (Exception ensureEx)
                {
                    Console.WriteLine($"[TransactionViewModel] Critical error: Cannot ensure table data after save failure: {ensureEx.Message}");
                }
            }
        }
        #endregion
        private void SaveCurrentTableState()
        {
            if (SelectedTable == null) return;

            var tableData = EnsureTableData(SelectedTable);
            if (tableData == null) return;

            try
            {
                Console.WriteLine($"[TransactionViewModel] Saving state for {SelectedTable.DisplayName}...");

                // Save cart items with deep copy to prevent reference issues
                tableData.CartItems.Clear();
                if (CartItems != null)
                {
                    foreach (var item in CartItems)
                    {
                        var copiedItem = new CartItem
                        {
                            Product = item.Product,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            Discount = item.Discount,
                            DiscountType = item.DiscountType,
                            IsBox = item.IsBox,
                            IsWholesale = item.IsWholesale
                        };
                        tableData.CartItems.Add(copiedItem);
                    }
                }

                // Save customer and payment information
                tableData.CustomerId = CustomerId;
                tableData.CustomerName = CustomerName ?? "Walk-in Customer";
                tableData.SelectedCustomer = SelectedCustomer;
                tableData.PaidAmount = PaidAmount;
                tableData.AddToCustomerDebt = AddToCustomerDebt;
                tableData.AmountToDebt = AmountToDebt;
                tableData.LastActivity = DateTime.Now;

                Console.WriteLine($"[TransactionViewModel] Saved state for {SelectedTable.DisplayName}: {tableData.CartItems.Count} items, Total value: ${tableData.CartItems.Sum(i => i.Total):F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error saving table state: {ex.Message}");
                StatusMessage = $"Error saving table state: {ex.Message}";
            }
        }


        private void LoadCurrentTableState()
        {
            if (SelectedTable == null)
            {
                Console.WriteLine("[TransactionViewModel] No selected table - clearing transaction state");
                ClearTransactionState();
                return;
            }

            var tableData = GetCurrentTableData();
            if (tableData == null)
            {
                Console.WriteLine($"[TransactionViewModel] No data found for {SelectedTable.DisplayName} - using default state");
                ClearTransactionState();
                return;
            }

            try
            {
                Console.WriteLine($"[TransactionViewModel] Loading state for {SelectedTable.DisplayName}...");

                // Suspend UI updates during bulk changes
                IsProcessing = true;

                // Clear and reload cart items
                Application.Current.Dispatcher.Invoke(() => {
                    CartItems.Clear();
                    foreach (var item in tableData.CartItems)
                    {
                        CartItems.Add(item);
                    }
                });

                // Restore customer information
                CustomerId = tableData.CustomerId;
                CustomerName = tableData.CustomerName ?? "Walk-in Customer";
                SelectedCustomer = tableData.SelectedCustomer;

                // Restore payment information
                PaidAmount = tableData.PaidAmount;
                AddToCustomerDebt = tableData.AddToCustomerDebt;
                AmountToDebt = tableData.AmountToDebt;

                // Update all calculated values
                UpdateTotals();
                CalculateExchangeAmount();
                CalculateAmountToDebt();

                // Force refresh of all UI-bound properties
                RefreshAllProperties();

                Console.WriteLine($"[TransactionViewModel] Loaded state for {SelectedTable.DisplayName}: {CartItems.Count} items, Customer: {CustomerName}, Total: ${TotalAmount:F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error loading table state: {ex.Message}");
                StatusMessage = $"Error loading table state: {ex.Message}";
                ClearTransactionState();
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ClearTransactionState()
        {
            CartItems.Clear();

            if (_walkInCustomer != null)
            {
                CustomerId = _walkInCustomer.CustomerId;
                CustomerName = _walkInCustomer.Name;
            }
            else
            {
                CustomerId = 0;
                CustomerName = "Walk-in Customer";
            }

            SelectedCustomer = null;
            PaidAmount = 0;
            AddToCustomerDebt = false;
            AmountToDebt = 0;

            UpdateTotals();
        }

        private void OnTableSelectionChanged(RestaurantTable previousTable)
        {
            try
            {
                Console.WriteLine($"[TransactionViewModel] Switching tables: {previousTable?.DisplayName ?? "None"} -> {SelectedTable?.DisplayName ?? "None"}");

                // Save previous table state
                if (previousTable != null)
                {
                    SaveTableState(previousTable);
                    Console.WriteLine($"[TransactionViewModel] Saved state for table: {previousTable.DisplayName}");
                }

                // Load new table state
                if (SelectedTable != null)
                {
                    LoadCurrentTableState();
                    Console.WriteLine($"[TransactionViewModel] Loaded state for table: {SelectedTable.DisplayName}");
                }
                else
                {
                    ClearTransactionState();
                }

                // Refresh UI
                UpdateTableDisplayInformation();
                RefreshTransactionUI();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error switching tables: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Table switch error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures table data is properly initialized with empty state if it doesn't exist
        /// </summary>
        private void EnsureTableDataIsInitialized(RestaurantTable table)
        {
            if (table == null) return;

            if (!_tableTransactionData.ContainsKey(table.Id))
            {
                _tableTransactionData[table.Id] = new TableTransactionData
                {
                    CartItems = new List<CartItem>(),
                    CustomerId = _walkInCustomer?.CustomerId ?? 0,
                    CustomerName = "Walk-in Customer",
                    SelectedCustomer = null,
                    PaidAmount = 0,
                    AddToCustomerDebt = false,
                    AmountToDebt = 0,
                    LastActivity = DateTime.Now,
                    Notes = string.Empty
                };

                Console.WriteLine($"[TransactionViewModel] Initialized empty data for {table.DisplayName}");
            }
        }
        private void NavigateToPreviousTable()
        {
            if (!CanNavigateToPreviousTable) return;

            try
            {
                Console.WriteLine($"[TransactionViewModel] Navigating to previous table from index {_currentTableIndex}");

                // Save current state before navigation
                SaveCurrentTableState();

                // Navigate to previous table
                _currentTableIndex--;
                var previousTable = ActiveTables[_currentTableIndex];

                // Set the selected table (this will trigger OnTableSelectionChanged)
                SelectedTable = previousTable;

                Console.WriteLine($"[TransactionViewModel] Navigated to previous table: {previousTable.DisplayName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error navigating to previous table: {ex.Message}");
                StatusMessage = "Error navigating to previous table";
            }
        }

        private void NavigateToNextTable()
        {
            if (!CanNavigateToNextTable) return;

            try
            {
                Console.WriteLine($"[TransactionViewModel] Navigating to next table from index {_currentTableIndex}");

                // Save current state before navigation
                SaveCurrentTableState();

                // Navigate to next table
                _currentTableIndex++;
                var nextTable = ActiveTables[_currentTableIndex];

                // Set the selected table (this will trigger OnTableSelectionChanged)
                SelectedTable = nextTable;

                Console.WriteLine($"[TransactionViewModel] Navigated to next table: {nextTable.DisplayName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error navigating to next table: {ex.Message}");
                StatusMessage = "Error navigating to next table";
            }
        }

        private void UpdateTableDisplayInformation()
        {
            try
            {
                // Update table display text
                if (SelectedTable != null)
                {
                    var itemCount = CartItems?.Count ?? 0;
                    var totalValue = TotalAmount;
                    var customerInfo = string.IsNullOrEmpty(CustomerName) || CustomerName == "Walk-in Customer"
                        ? ""
                        : $" - {CustomerName}";

                    TableDisplayText = $"{SelectedTable.DisplayName}: {itemCount} items (${totalValue:F2}){customerInfo}";
                }
                else
                {
                    TableDisplayText = "No table selected";
                }

                // Update computed table info properties
                OnPropertyChanged(nameof(CurrentTableInfo));
                OnPropertyChanged(nameof(TableNavigationInfo));
                OnPropertyChanged(nameof(TableDisplayText));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error updating table display information: {ex.Message}");
            }
        }

        private void RefreshTransactionUI()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Refreshing transaction UI...");

                // Ensure we're on the UI thread for property updates
                Application.Current.Dispatcher.Invoke(() => {

                    // Refresh all collection-based properties
                    OnPropertyChanged(nameof(CartItems));
                    OnPropertyChanged(nameof(SearchedProducts));
                    OnPropertyChanged(nameof(SearchedCustomers));

                    // Refresh customer-related properties
                    OnPropertyChanged(nameof(CustomerId));
                    OnPropertyChanged(nameof(CustomerName));
                    OnPropertyChanged(nameof(SelectedCustomer));
                    OnPropertyChanged(nameof(CustomerQuery));

                    // Refresh calculation properties
                    OnPropertyChanged(nameof(TotalAmount));
                    OnPropertyChanged(nameof(PaidAmount));
                    OnPropertyChanged(nameof(AddToCustomerDebt));
                    OnPropertyChanged(nameof(AmountToDebt));
                    OnPropertyChanged(nameof(ChangeDueAmount));
                    OnPropertyChanged(nameof(ExchangeAmount));

                    // Refresh table-specific properties
                    OnPropertyChanged(nameof(SelectedTable));
                    OnPropertyChanged(nameof(TableDisplayText));
                    OnPropertyChanged(nameof(CurrentTableInfo));
                    OnPropertyChanged(nameof(HasMultipleTables));
                    OnPropertyChanged(nameof(TableNavigationInfo));

                    // Refresh command availability
                    OnPropertyChanged(nameof(CanCheckout));
                    OnPropertyChanged(nameof(CanRemoveItem));

                    // Force command re-evaluation
                    CommandManager.InvalidateRequerySuggested();
                });

                Console.WriteLine("[TransactionViewModel] Transaction UI refresh completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error refreshing transaction UI: {ex.Message}");
            }
        }

        private void RefreshAllProperties()
        {
            try
            {
                // Core transaction properties
                OnPropertyChanged(nameof(CartItems));
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(PaidAmount));
                OnPropertyChanged(nameof(ChangeDueAmount));
                OnPropertyChanged(nameof(ExchangeAmount));

                // Customer properties
                OnPropertyChanged(nameof(CustomerId));
                OnPropertyChanged(nameof(CustomerName));
                OnPropertyChanged(nameof(SelectedCustomer));

                // Payment properties
                OnPropertyChanged(nameof(AddToCustomerDebt));
                OnPropertyChanged(nameof(AmountToDebt));

                // Table properties
                OnPropertyChanged(nameof(SelectedTable));
                OnPropertyChanged(nameof(TableDisplayText));
                OnPropertyChanged(nameof(CurrentTableInfo));
                OnPropertyChanged(nameof(HasMultipleTables));
                OnPropertyChanged(nameof(CanNavigateToPreviousTable));
                OnPropertyChanged(nameof(CanNavigateToNextTable));
                OnPropertyChanged(nameof(TableNavigationInfo));

                // Command states
                OnPropertyChanged(nameof(CanCheckout));
                OnPropertyChanged(nameof(CanRemoveItem));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error refreshing properties: {ex.Message}");
            }
        }
        private void ShowTableNavigationOverview()
        {
            try
            {
                if (ActiveTables.Count == 0)
                {
                    StatusMessage = "No active tables to navigate";
                    MessageBox.Show("No active tables are currently available.",
                                  "Table Navigation",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    return;
                }

                // FIXED: Ensure current table state is saved before showing overview
                if (SelectedTable != null)
                {
                    SaveCurrentTableState();
                }

                var message = "Active Tables Overview:\n\n";
                for (int i = 0; i < ActiveTables.Count; i++)
                {
                    var table = ActiveTables[i];

                    // FIXED: Get the actual table-specific data, not current cart
                    var tableData = GetTableDataById(table.Id);

                    int itemCount;
                    decimal totalAmount;
                    string customerName;

                    if (tableData != null)
                    {
                        // Use table-specific data
                        itemCount = tableData.CartItems?.Count ?? 0;
                        totalAmount = tableData.CartItems?.Sum(item => item.Total) ?? 0;
                        customerName = tableData.CustomerName ?? "Walk-in Customer";
                    }
                    else
                    {
                        // Table has no data - show empty state
                        itemCount = 0;
                        totalAmount = 0;
                        customerName = "Walk-in Customer";
                    }

                    var current = i == _currentTableIndex ? " ← CURRENT" : "";

                    message += $"{i + 1}. {table.DisplayName}\n";
                    message += $"   • {itemCount} items (${totalAmount:F2})\n";
                    message += $"   • Customer: {customerName}\n";
                    message += $"   • Status: {table.Status}{current}\n\n";
                }

                message += $"\nNavigation: Use ◀ ▶ buttons or select a table directly.";

                MessageBox.Show(message,
                               "Table Navigation Overview",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error showing table navigation overview: {ex.Message}");
                StatusMessage = "Error showing table overview";
            }
        }
        private void CloseCurrentTable()
        {
            if (SelectedTable == null) return;

            try
            {
                var tableToClose = SelectedTable;
                var hasItems = CartItems.Count > 0;
                var totalValue = TotalAmount;

                // Confirm closure if table has items
                if (hasItems)
                {
                    var result = MessageBox.Show(
                        $"Table {tableToClose.DisplayName} contains {CartItems.Count} items worth ${totalValue:F2}.\n\n" +
                        "Are you sure you want to close this table?\n\n" +
                        "⚠️ All items and customer information will be lost!",
                        "Close Table Confirmation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                        return;
                }

                Console.WriteLine($"[TransactionViewModel] Closing table: {tableToClose.DisplayName}");

                // Remove from active tables and clear data
                Application.Current.Dispatcher.Invoke(() => {
                    ActiveTables.Remove(tableToClose);
                });

                _tableTransactionData.Remove(tableToClose.Id);

                // Navigate to another table or clear state
                if (ActiveTables.Count > 0)
                {
                    // Navigate to the nearest available table
                    _currentTableIndex = Math.Max(0, Math.Min(_currentTableIndex, ActiveTables.Count - 1));
                    SelectedTable = ActiveTables[_currentTableIndex];
                }
                else
                {
                    // No more tables - clear everything
                    SelectedTable = null;
                    _currentTableIndex = -1;
                }

                StatusMessage = $"Closed {tableToClose.DisplayName}" +
                               (hasItems ? $" (${totalValue:F2} in items lost)" : "");

                Console.WriteLine($"[TransactionViewModel] Successfully closed table: {tableToClose.DisplayName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error closing table: {ex.Message}");
                StatusMessage = "Error closing table";
            }
        }

        private TableTransactionData GetTableDataById(int tableId)
        {
            _tableTransactionData.TryGetValue(tableId, out var data);
            return data;
        }

        private void UpdateNavigationProperties()
        {
            // Update navigation availability
            OnPropertyChanged(nameof(HasMultipleTables));
            OnPropertyChanged(nameof(CanNavigateToPreviousTable));
            OnPropertyChanged(nameof(CanNavigateToNextTable));
            OnPropertyChanged(nameof(TableNavigationInfo));
            OnPropertyChanged(nameof(CurrentTableInfo));

            // Force command re-evaluation for navigation commands
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                CommandManager.InvalidateRequerySuggested();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }


        #endregion

        #region Existing Methods (All Original Functionality)

        private void OpenTableSelectionDialog()
        {
            try
            {
                if (SelectedTable != null)
                {
                    SaveCurrentTableState();
                }

                var (success, selectedTable) = RestaurantTableDialog.ShowTableSelectionDialog(
                    Application.Current.MainWindow);

                if (success && selectedTable != null)
                {
                    SelectedTable = selectedTable;
                    StatusMessage = $"Selected {selectedTable.DisplayName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error selecting table: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Error in table selection: {ex.Message}");
            }
        }

        private async void LoadInitialDataAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Starting LoadInitialDataAsync...");

                StatusMessage = "Loading categories and products...";
                await LoadCategoriesAsync();
                await LoadProductsByCategoryAsync();
                LoadInitialCustomersAsync();

                Console.WriteLine("[TransactionViewModel] LoadInitialDataAsync completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error in LoadInitialDataAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[TransactionViewModel] Inner exception: {ex.InnerException.Message}");
                }
                StatusMessage = $"Error loading initial data: {ex.Message}";
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Loading categories for filter dropdown...");

                var categories = await _categoryService.GetCategoriesForFilterAsync();

                Console.WriteLine($"[TransactionViewModel] Retrieved {categories.Count} categories from service");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Categories.Clear();
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                        Console.WriteLine($"[TransactionViewModel] Added category: {category.Name} (ID: {category.CategoryId}, Products: {category.ProductCount})");
                    }

                    var allCategoriesOption = Categories.FirstOrDefault(c => c.CategoryId == 0);
                    if (allCategoriesOption != null)
                    {
                        SelectedCategory = allCategoriesOption;
                        Console.WriteLine("[TransactionViewModel] Set default selection to 'All Categories'");
                    }
                });

                Console.WriteLine($"[TransactionViewModel] Successfully loaded {categories.Count} categories");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error loading categories: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[TransactionViewModel] Inner exception: {ex.InnerException.Message}");
                }
                StatusMessage = $"Error loading categories: {ex.Message}";
            }
        }

        private async Task LoadProductsByCategoryAsync()
        {
            try
            {
                Console.WriteLine($"[TransactionViewModel] Loading products for category ID: {SelectedCategoryId}");

                StatusMessage = "Loading products...";

                var products = await _productService.SearchByCategoryAsync(SelectedCategoryId, 100);

                Console.WriteLine($"[TransactionViewModel] Retrieved {products.Count} products from service");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchedProducts.Clear();

                    foreach (var product in products)
                    {
                        SearchedProducts.Add(product);
                        Console.WriteLine($"[TransactionViewModel] Added product: {product.Name} (ID: {product.ProductId}, Stock: {product.CurrentStock})");
                    }
                });

                if (products.Count > 0)
                {
                    var categoryName = SelectedCategory?.Name ?? "Selected Category";
                    StatusMessage = $"Loaded {products.Count} products from {categoryName}";
                    Console.WriteLine($"[TransactionViewModel] Successfully loaded {products.Count} products for category: {categoryName}");
                }
                else
                {
                    var categoryName = SelectedCategory?.Name ?? "Selected Category";
                    StatusMessage = $"No products found in {categoryName}";
                    Console.WriteLine($"[TransactionViewModel] No products found for category: {categoryName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error loading products by category: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[TransactionViewModel] Inner exception: {ex.InnerException.Message}");
                }
                StatusMessage = $"Error loading products: {ex.Message}";
            }
        }

        private void SelectCategory(Category category)
        {
            if (category == null)
            {
                Console.WriteLine("[TransactionViewModel] SelectCategory called with null category");
                return;
            }

            Console.WriteLine($"[TransactionViewModel] Category selected: {category.Name} (ID: {category.CategoryId})");

            try
            {
                SelectedCategory = category;
                StatusMessage = $"Selected category: {category.Name}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error in SelectCategory: {ex.Message}");
                StatusMessage = $"Error selecting category: {ex.Message}";
            }
        }

        private void ViewPrintQueue()
        {
            if (_printQueueManager == null)
            {
                StatusMessage = "Print queue manager is not available.";
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
                StatusMessage = $"Error opening print queue dialog: {ex.Message}";
            }
        }

        private async Task CheckForFailedTransactionsAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Checking for failed transactions...");

                var failedTransactionService = new FailedTransactionService();
                var transactions = await failedTransactionService.GetFailedTransactionsAsync();

                int previousCount = PendingTransactionCount;
                PendingTransactionCount = transactions.Count;
                OnPropertyChanged(nameof(HasPendingTransactions));

                Console.WriteLine($"[TransactionViewModel] Found {PendingTransactionCount} failed transactions (previous: {previousCount})");

                if (PendingTransactionCount > 0)
                {
                    if (PendingTransactionCount > previousCount)
                    {
                        string message = $"⚠️ ATTENTION: {PendingTransactionCount} failed transaction(s) need attention in the recovery center.";
                        StatusMessage = message;

                        if (PendingTransactionCount == 1 && previousCount == 0)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                MessageBox.Show(
                                    "A transaction has failed and has been moved to the recovery center. " +
                                    "Please review it at your earliest convenience.",
                                    "Transaction Recovery Required",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            });
                        }
                    }
                    else
                    {
                        StatusMessage = $"There are {PendingTransactionCount} failed transactions that need attention.";
                    }
                }
                else if (previousCount > 0 && PendingTransactionCount == 0)
                {
                    StatusMessage = "All failed transactions have been successfully resolved. System ready.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error checking for failed transactions: {ex.Message}");
            }
        }

        private void OpenRecoveryDialog()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Opening recovery dialog...");

                var dialog = new Views.FailedTransactionRecoveryDialog();
                dialog.Owner = Application.Current.MainWindow;
                var result = dialog.ShowDialog();

                if (result == true)
                {
                    CheckForFailedTransactionsAsync().ConfigureAwait(false);
                    RefreshDrawerStatusAsync().ConfigureAwait(false);
                    StatusMessage = "Transaction recovery completed successfully.";
                }
                else
                {
                    CheckForFailedTransactionsAsync().ConfigureAwait(false);
                }

                Console.WriteLine("[TransactionViewModel] Recovery dialog operation completed");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening recovery dialog: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Error opening recovery dialog: {ex.Message}");
            }
        }

        private async Task LoadExchangeRateAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Loading exchange rate...");

                ExchangeRate = await _businessSettingsService.GetExchangeRateAsync();
                Console.WriteLine($"[TransactionViewModel] Loaded exchange rate: {ExchangeRate}");

                UseExchangeRate = true;
                CalculateExchangeAmount();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error loading exchange rate: {ex.Message}");
            }
        }

        private bool CanHoldCart()
        {
            return CartItems.Count > 0 && HeldCarts.Count < 10;
        }

        private bool CanRestoreCart()
        {
            return HeldCarts.Count > 0;
        }

        private void HoldCurrentCart()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Starting HoldCurrentCart...");

                if (CartItems.Count == 0)
                {
                    StatusMessage = "Cannot hold an empty cart.";
                    return;
                }

                if (HeldCarts.Count >= 10)
                {
                    StatusMessage = "Maximum number of held carts (10) reached. Please restore or complete a held cart first.";
                    return;
                }

                var heldCart = new HeldCart
                {
                    Id = _nextCartId++,
                    CreatedAt = DateTime.Now,
                    CustomerId = CustomerId,
                    CustomerName = CustomerName,
                    TotalAmount = TotalAmount,
                    Items = new List<CartItem>()
                };

                foreach (var item in CartItems)
                {
                    var clonedItem = new CartItem
                    {
                        Product = item.Product,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Discount = item.Discount,
                        DiscountType = item.DiscountType,
                        IsBox = item.IsBox,
                        IsWholesale = item.IsWholesale
                    };
                    heldCart.Items.Add(clonedItem);
                }

                HeldCarts.Add(heldCart);

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

                StatusMessage = $"Cart #{heldCart.Id} held successfully.";
                Console.WriteLine($"[TransactionViewModel] Cart #{heldCart.Id} held successfully with {heldCart.Items.Count} items");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error holding cart: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Error holding cart: {ex}");
            }
        }

        private void RestoreHeldCart()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Starting RestoreHeldCart...");

                if (HeldCarts.Count == 0)
                {
                    StatusMessage = "No held carts available to restore.";
                    return;
                }

                if (CartItems.Count > 0)
                {
                    var result = MessageBox.Show(
                        "Current cart contains items. Would you like to hold it before restoring another cart?",
                        "Hold Current Cart?",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                    else if (result == MessageBoxResult.Yes)
                    {
                        HoldCurrentCart();

                        if (CartItems.Count > 0)
                        {
                            return;
                        }
                    }
                    else
                    {
                        CartItems.Clear();
                        UpdateTotals();
                    }
                }

                var dialog = new RestoreCartDialog(HeldCarts.ToList());
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true && dialog.SelectedCart != null)
                {
                    var selectedCart = dialog.SelectedCart;

                    CartItems.Clear();
                    foreach (var item in selectedCart.Items)
                    {
                        CartItems.Add(item);
                    }

                    CustomerId = selectedCart.CustomerId;
                    CustomerName = selectedCart.CustomerName;

                    if (selectedCart.CustomerId > 0)
                    {
                        try
                        {
                            var customer = _customerService.GetByIdAsync(selectedCart.CustomerId).Result;
                            if (customer != null)
                            {
                                SelectedCustomer = customer;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[TransactionViewModel] Error loading customer details: {ex.Message}");
                        }
                    }
                    else
                    {
                        SelectedCustomer = null;
                    }

                    OnPropertyChanged(nameof(CustomerName));
                    OnPropertyChanged(nameof(CustomerId));
                    OnPropertyChanged(nameof(SelectedCustomer));

                    HeldCarts.Remove(selectedCart);

                    UpdateTotals();

                    StatusMessage = $"Cart #{selectedCart.Id} restored successfully.";
                    Console.WriteLine($"[TransactionViewModel] Cart #{selectedCart.Id} restored successfully with {selectedCart.Items.Count} items");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error restoring cart: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Error restoring cart: {ex}");
            }
        }

        private async Task CheckoutAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Starting checkout process...");

                if (!IsDrawerOpen)
                {
                    ShowErrorPopup("Drawer is Closed", "Cannot checkout: Drawer is closed. Please open a drawer first.");
                    StatusMessage = "Cannot checkout: Drawer is closed. Please open a drawer first.";
                    return;
                }

                if (CartItems.Count == 0)
                {
                    ShowErrorPopup("Empty Cart", "Cart is empty. Cannot checkout.");
                    StatusMessage = "Cart is empty. Cannot checkout.";
                    return;
                }

                if (PaidAmount < 0)
                {
                    ShowErrorPopup("Invalid Payment", "Payment amount cannot be negative.");
                    StatusMessage = "Payment amount cannot be negative.";
                    return;
                }

                if (AddToCustomerDebt && AmountToDebt > 0 && (CustomerId <= 0 || CustomerName == "Walk-in Customer"))
                {
                    ShowErrorPopup("Customer Required", "Cannot add debt to a walk-in customer. Please select a registered customer.");
                    StatusMessage = "Cannot add debt to a walk-in customer. Please select a registered customer.";
                    return;
                }

                IsProcessing = true;
                StatusMessage = "Processing transaction...";
                Console.WriteLine($"[TransactionViewModel] Checkout validation passed at: {DateTime.Now}");
                Console.WriteLine($"[TransactionViewModel] Cart contains {CartItems.Count} items, total amount: {TotalAmount:C2}, paid amount: {PaidAmount:C2}");

                if (AddToCustomerDebt && AmountToDebt > 0)
                {
                    Console.WriteLine($"[TransactionViewModel] Adding {AmountToDebt:C2} to customer debt (Customer ID: {CustomerId})");
                }

                var currentEmployee = _authService.CurrentEmployee;
                if (currentEmployee == null)
                {
                    ShowErrorPopup("Authentication Error", "No cashier is logged in. Please log in first.");
                    StatusMessage = "Error: No cashier is logged in.";
                    IsProcessing = false;
                    Console.WriteLine("[TransactionViewModel] Checkout failed: No cashier logged in");
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
                        Console.WriteLine($"[TransactionViewModel] Using walk-in customer: ID={customerIdForTransaction}, Name={customerNameForTransaction}");
                    }
                    else
                    {
                        var dbService = new DatabaseService();
                        var walkInCustomer = await dbService.EnsureWalkInCustomerExistsAsync();

                        if (walkInCustomer != null)
                        {
                            customerIdForTransaction = walkInCustomer.CustomerId;
                            customerNameForTransaction = walkInCustomer.Name;
                            Console.WriteLine($"[TransactionViewModel] Retrieved walk-in customer: ID={customerIdForTransaction}, Name={customerNameForTransaction}");
                        }
                        else
                        {
                            ShowErrorPopup("Customer Error", "Cannot proceed without a valid customer ID. Please select a customer.");
                            StatusMessage = "Error: Cannot proceed without a valid customer ID. Please select a customer.";
                            IsProcessing = false;
                            Console.WriteLine("[TransactionViewModel] Checkout failed: No valid walk-in customer");
                            return;
                        }
                    }
                }
                else
                {
                    customerIdForTransaction = CustomerId;
                    customerNameForTransaction = CustomerName;
                    Console.WriteLine($"[TransactionViewModel] Using selected customer: ID={customerIdForTransaction}, Name={customerNameForTransaction}");
                }

                Console.WriteLine($"[TransactionViewModel] Cashier: {currentEmployee.FullName} (ID: {currentEmployee.EmployeeId})");
                Console.WriteLine($"[TransactionViewModel] Customer: {customerNameForTransaction} (ID: {customerIdForTransaction})");

                string paymentMethod = DeterminePaymentMethod(TotalAmount, PaidAmount, AddToCustomerDebt, AmountToDebt);
                Console.WriteLine($"[TransactionViewModel] Determined payment method: {paymentMethod}");

                Transaction transaction;
                try
                {
                    transaction = await _transactionService.CreateTransactionAsync(
                        CartItems.ToList(),
                        PaidAmount,
                        currentEmployee,
                        paymentMethod,
                        customerNameForTransaction,
                        customerIdForTransaction
                    );
                }
                catch (Exception ex)
                {
                    string errorTitle = "Checkout Error";
                    string errorMsg = ex.Message;

                    if (ex.Message.Contains("Insufficient stock") ||
                        (ex.InnerException != null && ex.InnerException.Message.Contains("Insufficient stock")))
                    {
                        string errorText = ex.InnerException?.Message ?? ex.Message;
                        errorTitle = "Inventory Error";
                        errorMsg = errorText;
                    }

                    ShowErrorPopup(errorTitle, errorMsg);

                    StatusMessage = $"Error during checkout: {ex.Message}";
                    IsProcessing = false;
                    await CheckForFailedTransactionsAsync();
                    return;
                }

                if (transaction == null || transaction.TransactionId <= 0)
                {
                    ShowErrorPopup("Transaction Failed", "Failed to create transaction record. The system could not complete the sale.");
                    StatusMessage = "Error: Failed to create transaction record.";
                    IsProcessing = false;
                    Console.WriteLine("[TransactionViewModel] Checkout failed: Transaction record creation failed");
                    return;
                }

                Console.WriteLine($"[TransactionViewModel] Transaction #{transaction.TransactionId} created successfully");
                Console.WriteLine($"[TransactionViewModel] - Total Amount: {transaction.TotalAmount:C2}, Paid Amount: {transaction.PaidAmount:C2}, Payment Method: {transaction.PaymentMethod}");

                string receiptResult = await _receiptPrinterService.PrintTransactionReceiptWpfAsync(
                    transaction,
                    CartItems.ToList(),
                    customerIdForTransaction,
                    0,
                    ExchangeRate);

                bool printed = !receiptResult.Contains("cancelled") && !receiptResult.Contains("error");
                Console.WriteLine($"[TransactionViewModel] Receipt printing result: {receiptResult}");

                await GetCurrentDrawerAsync();

                TransactionLookupId = transaction.TransactionId.ToString();

                // Reset transaction state for current table
                if (SelectedTable != null)
                {
                    _tableTransactionData.Remove(SelectedTable.Id);
                    LoadCurrentTableState(); // This will clear the state since data was removed
                }
                else
                {
                    ResetTransactionState();
                }

                await LoadProductsByCategoryAsync();

                string successMessage = $"Transaction #{transaction.TransactionId} completed successfully ({paymentMethod}).";
                if (printed)
                {
                    successMessage += " Receipt printed.";
                }
                if (AddToCustomerDebt && AmountToDebt > 0)
                {
                    successMessage += $" Added {AmountToDebt:C2} to customer debt.";
                }

                successMessage += " Ready for new transaction.";
                StatusMessage = successMessage;

                Console.WriteLine("[TransactionViewModel] Checkout process completed successfully");
            }
            catch (Exception ex)
            {
                string errorTitle = "Checkout Error";
                string errorMessage = ex.Message;

                if (ex.InnerException != null)
                {
                    errorMessage = $"{errorMessage}\n\nDetails: {ex.InnerException.Message}";
                }

                ShowErrorPopup(errorTitle, errorMessage);

                StatusMessage = $"Error during checkout: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Checkout error: {ex.Message}");
                Console.WriteLine($"[TransactionViewModel] Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[TransactionViewModel] Inner exception: {ex.InnerException.Message}");
                }

                await CheckForFailedTransactionsAsync();
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private string DeterminePaymentMethod(decimal totalAmount, decimal paidAmount, bool addToCustomerDebt, decimal amountToDebt)
        {
            try
            {
                Console.WriteLine($"[TransactionViewModel] DeterminePaymentMethod - Total: {totalAmount:C2}, Paid: {paidAmount:C2}, AddDebt: {addToCustomerDebt}, DebtAmount: {amountToDebt:C2}");

                if (paidAmount >= totalAmount && (!addToCustomerDebt || amountToDebt <= 0))
                {
                    Console.WriteLine("[TransactionViewModel] Payment method: Cash (full payment)");
                    return "Cash";
                }

                if (paidAmount > 0 && paidAmount < totalAmount && addToCustomerDebt && amountToDebt > 0)
                {
                    Console.WriteLine("[TransactionViewModel] Payment method: Debt (partial payment with debt)");
                    return "Debt";
                }

                if (paidAmount == 0 && addToCustomerDebt && amountToDebt > 0)
                {
                    Console.WriteLine("[TransactionViewModel] Payment method: Debt (full debt, no payment)");
                    return "Debt";
                }

                Console.WriteLine("[TransactionViewModel] Payment method: Cash (fallback)");
                return "Cash";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error determining payment method: {ex.Message}");
                return "Cash";
            }
        }

        private void CalculateExchangeAmount()
        {
            if (ExchangeRate > 0)
            {
                ExchangeAmount = TotalAmount * ExchangeRate;
                Console.WriteLine($"[TransactionViewModel] Calculated exchange amount: {TotalAmount} USD × {ExchangeRate} = {ExchangeAmount} LBP");
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
                ChangeDueAmount = 0;
            }
            else
            {
                AmountToDebt = 0;
                ChangeDueAmount = Math.Max(0, PaidAmount - TotalAmount);
            }
        }

        private void UpdateCartWholesaleMode()
        {
            Console.WriteLine($"[TransactionViewModel] Updating cart wholesale mode to: {WholesaleMode}");

            foreach (var item in CartItems)
            {
                if (item.IsWholesale != WholesaleMode)
                {
                    item.IsWholesale = WholesaleMode;
                    Console.WriteLine($"[TransactionViewModel] Updated {item.Product.Name} wholesale mode to: {WholesaleMode}");
                }
            }

            UpdateTotals();
        }

        private async Task GetCurrentDrawerAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Getting current drawer...");

                if (_authService.CurrentEmployee == null)
                {
                    CurrentDrawer = null;
                    OnPropertyChanged(nameof(IsDrawerOpen));
                    Console.WriteLine("[TransactionViewModel] No current employee, drawer set to null");
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

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    CommandManager.InvalidateRequerySuggested();
                });

                Console.WriteLine($"[TransactionViewModel] Current drawer status: {(drawer != null ? $"ID={drawer.DrawerId}, Status={drawer.Status}" : "None")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error getting current drawer: {ex.Message}");
                StatusMessage = $"Error retrieving drawer information: {ex.Message}";
            }
        }

        private async Task ShowCashInDialogAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Starting ShowCashInDialogAsync...");

                await GetCurrentDrawerAsync();

                if (!IsDrawerOpen)
                {
                    StatusMessage = "No open drawer found.";
                    return;
                }

                var dialog = new CashInDialog(CurrentDrawer);
                dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                dialog.Owner = System.Windows.Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    await Task.Delay(100);
                    await RefreshDrawerStatusAsync();

                    var updatedDrawer = dialog.UpdatedDrawer;
                    StatusMessage = $"Cash in operation completed successfully.";

                    OnPropertyChanged(nameof(CurrentDrawer));
                    OnPropertyChanged(nameof(IsDrawerOpen));
                    CommandManager.InvalidateRequerySuggested();
                }

                Console.WriteLine("[TransactionViewModel] ShowCashInDialogAsync completed");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error during cash in: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Cash in dialog error: {ex}");
            }
        }

        private async Task ShowCashOutDialogAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Starting ShowCashOutDialogAsync...");

                await GetCurrentDrawerAsync();

                if (!IsDrawerOpen)
                {
                    StatusMessage = "No open drawer found.";
                    return;
                }

                var dialog = new CashOutDialog(CurrentDrawer);
                dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                dialog.Owner = System.Windows.Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    await Task.Delay(100);
                    await RefreshDrawerStatusAsync();

                    var updatedDrawer = dialog.UpdatedDrawer;
                    StatusMessage = $"Cash out operation completed successfully.";

                    OnPropertyChanged(nameof(CurrentDrawer));
                    OnPropertyChanged(nameof(IsDrawerOpen));
                    CommandManager.InvalidateRequerySuggested();
                }

                Console.WriteLine("[TransactionViewModel] ShowCashOutDialogAsync completed");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error during cash out: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Cash out dialog error: {ex}");
            }
        }

        public async Task RefreshDrawerStatusAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Starting RefreshDrawerStatusAsync...");

                if (_authService.CurrentEmployee == null)
                {
                    CurrentDrawer = null;
                    Console.WriteLine("[TransactionViewModel] RefreshDrawerStatusAsync: No employee logged in");
                    OnPropertyChanged(nameof(IsDrawerOpen));
                    return;
                }

                string cashierId = _authService.CurrentEmployee.EmployeeId.ToString();
                Console.WriteLine($"[TransactionViewModel] Refreshing drawer status for cashier ID: {cashierId}...");

                var freshDrawerService = new DrawerService();

                CurrentDrawer = null;
                OnPropertyChanged(nameof(CurrentDrawer));
                OnPropertyChanged(nameof(IsDrawerOpen));

                bool success = false;
                Exception lastException = null;

                for (int attempt = 1; attempt <= 3 && !success; attempt++)
                {
                    try
                    {
                        if (attempt > 1)
                        {
                            await Task.Delay(attempt * 200);
                            Console.WriteLine($"[TransactionViewModel] Retry attempt {attempt} for drawer status...");
                        }

                        var drawer = await freshDrawerService.GetOpenDrawerAsync(cashierId);

                        Console.WriteLine($"[TransactionViewModel] Drawer status refreshed. Found drawer: {drawer != null}, " +
                            $"Status: {drawer?.Status ?? "None"}, DrawerId: {drawer?.DrawerId.ToString() ?? "None"}");

                        CurrentDrawer = drawer;
                        success = true;

                        if (CurrentDrawer != null && CurrentDrawer.Notes == null)
                        {
                            CurrentDrawer.Notes = string.Empty;
                        }
                    }
                    catch (Exception queryEx)
                    {
                        lastException = queryEx;
                        Console.WriteLine($"[TransactionViewModel] Error querying drawer (attempt {attempt}): {queryEx.Message}");
                        if (queryEx.InnerException != null)
                        {
                            Console.WriteLine($"[TransactionViewModel] Inner exception: {queryEx.InnerException.Message}");
                        }
                    }
                }

                if (!success && lastException != null)
                {
                    Console.WriteLine($"[TransactionViewModel] All attempts to refresh drawer status failed: {lastException.Message}");
                    throw lastException;
                }

                OnPropertyChanged(nameof(CurrentDrawer));
                OnPropertyChanged(nameof(IsDrawerOpen));
                OnPropertyChanged(nameof(CanCheckout));
                OnPropertyChanged(nameof(DrawerStatusToolTip));

                await Application.Current.Dispatcher.InvokeAsync(async () => {
                    CommandManager.InvalidateRequerySuggested();
                    await Task.Delay(50);
                    CommandManager.InvalidateRequerySuggested();
                });

                Console.WriteLine($"[TransactionViewModel] Drawer refresh complete. IsDrawerOpen = {IsDrawerOpen}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error in RefreshDrawerStatusAsync: {ex.Message}");
                Console.WriteLine($"[TransactionViewModel] Stack trace: {ex.StackTrace}");
                StatusMessage = $"Error refreshing drawer status: {ex.Message}";
            }
        }

        private async Task OpenDrawerDialogAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Starting OpenDrawerDialogAsync...");

                await RefreshDrawerStatusAsync();

                if (IsDrawerOpen)
                {
                    StatusMessage = $"Drawer already open with ID #{CurrentDrawer.DrawerId}";
                    return;
                }

                var viewModel = new OpenDrawerViewModel(_authService);
                var dialog = new OpenDrawerDialog(viewModel);

                dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                dialog.Owner = System.Windows.Application.Current.MainWindow;
                dialog.Topmost = true;

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    await Task.Delay(500);
                    await RefreshDrawerStatusAsync();

                    await Task.Delay(200);
                    await RefreshDrawerStatusAsync();

                    StatusMessage = "Drawer opened successfully.";

                    Application.Current.Dispatcher.Invoke(() => {
                        OnPropertyChanged(nameof(IsDrawerOpen));
                        OnPropertyChanged(nameof(CurrentDrawer));
                        OnPropertyChanged(nameof(CanCheckout));
                        OnPropertyChanged(nameof(DrawerStatusToolTip));
                        CommandManager.InvalidateRequerySuggested();
                    });
                }
                else
                {
                    StatusMessage = "Drawer opening was cancelled or failed.";
                }

                Console.WriteLine("[TransactionViewModel] OpenDrawerDialogAsync completed");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening drawer: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Open drawer dialog error: {ex}");

                try
                {
                    await RefreshDrawerStatusAsync();
                }
                catch { }
            }
        }

        private async Task CloseDrawerDialogAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] CloseDrawerDialogAsync started");

                Console.WriteLine("[TransactionViewModel] Refreshing drawer status from database");
                await RefreshDrawerStatusAsync();

                if (!IsDrawerOpen)
                {
                    StatusMessage = "No open drawer found.";
                    Console.WriteLine("[TransactionViewModel] Cannot close drawer: No open drawer found");
                    MessageBox.Show("No open drawer found. Please open a drawer first.",
                        "No Drawer", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (CurrentDrawer == null)
                {
                    StatusMessage = "Drawer information is not available.";
                    Console.WriteLine("[TransactionViewModel] CurrentDrawer is null despite IsDrawerOpen being true");
                    return;
                }

                if (CurrentDrawer.Status != "Open")
                {
                    StatusMessage = $"Drawer status is '{CurrentDrawer.Status}', not 'Open'.";
                    Console.WriteLine($"[TransactionViewModel] Cannot close drawer with status: {CurrentDrawer.Status}");
                    return;
                }

                Console.WriteLine($"[TransactionViewModel] About to close drawer #{CurrentDrawer.DrawerId}, Current status: {CurrentDrawer.Status}");
                Console.WriteLine($"[TransactionViewModel] Current balance: ${CurrentDrawer.CurrentBalance:F2}");

                try
                {
                    int drawerIdBeforeClose = CurrentDrawer.DrawerId;

                    var dialog = new CloseDrawerDialog(CurrentDrawer);
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialog.Owner = Application.Current.MainWindow;
                    dialog.Topmost = true;

                    Console.WriteLine("[TransactionViewModel] Showing CloseDrawerDialog");
                    bool? result = dialog.ShowDialog();
                    Console.WriteLine($"[TransactionViewModel] Dialog ShowDialog returned result: {result}");

                    CurrentDrawer = null;
                    OnPropertyChanged(nameof(CurrentDrawer));
                    OnPropertyChanged(nameof(IsDrawerOpen));
                    CommandManager.InvalidateRequerySuggested();

                    Console.WriteLine("[TransactionViewModel] Waiting for database operations to complete");
                    await Task.Delay(1000);

                    for (int i = 0; i < 3; i++)
                    {
                        Console.WriteLine($"[TransactionViewModel] Refreshing drawer status after dialog close (attempt {i + 1})");
                        await RefreshDrawerStatusAsync();

                        if (i < 2) await Task.Delay(300);
                    }

                    Application.Current.Dispatcher.Invoke(() => {
                        OnPropertyChanged(nameof(IsDrawerOpen));
                        OnPropertyChanged(nameof(CurrentDrawer));
                        OnPropertyChanged(nameof(CanCheckout));
                        OnPropertyChanged(nameof(DrawerStatusToolTip));
                        CommandManager.InvalidateRequerySuggested();
                    });

                    if (result == true)
                    {
                        StatusMessage = "Drawer closed successfully.";
                        Console.WriteLine("[TransactionViewModel] Drawer closed successfully according to dialog result");

                        var drawerService = new DrawerService();
                        var drawerDb = await drawerService.GetDrawerByIdAsync(drawerIdBeforeClose);

                        if (drawerDb != null)
                        {
                            Console.WriteLine($"[TransactionViewModel] Database check confirms drawer status is: {drawerDb.Status}");

                            if (drawerDb.Status != "Closed")
                            {
                                Console.WriteLine("[TransactionViewModel] WARNING: Database shows drawer is not actually closed");
                                await RefreshDrawerStatusAsync();
                            }
                        }
                    }
                    else
                    {
                        StatusMessage = "Drawer closing was cancelled or unsuccessful.";
                        Console.WriteLine("[TransactionViewModel] Drawer closing was cancelled or unsuccessful");

                        await RefreshDrawerStatusAsync();
                    }
                }
                catch (Exception dialogEx)
                {
                    StatusMessage = $"Error showing close drawer dialog: {dialogEx.Message}";
                    Console.WriteLine($"[TransactionViewModel] Dialog creation/show error: {dialogEx.Message}");
                    MessageBox.Show($"Error showing close drawer dialog: {dialogEx.Message}",
                        "Dialog Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    await RefreshDrawerStatusAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error closing drawer: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Close drawer dialog error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[TransactionViewModel] Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"[TransactionViewModel] Stack trace: {ex.StackTrace}");

                MessageBox.Show($"Error closing drawer: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                try
                {
                    await RefreshDrawerStatusAsync();
                }
                catch { }
            }
            finally
            {
                Console.WriteLine("[TransactionViewModel] CloseDrawerDialogAsync completed");
            }
        }

        private void OpenCustomerSelectionDialog()
        {
            Console.WriteLine("[TransactionViewModel] Opening customer selection dialog...");

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
                Console.WriteLine("[TransactionViewModel] Loading initial customers...");

                var customers = await _customerService.SearchCustomersAsync("");
                SearchedCustomers.Clear();

                foreach (var customer in customers)
                {
                    SearchedCustomers.Add(customer);
                }

                Console.WriteLine($"[TransactionViewModel] Loaded {customers.Count} customers");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading customers: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Error loading initial customers: {ex}");
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
                Console.WriteLine($"[TransactionViewModel] Error in customer search timer: {ex}");
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    StatusMessage = $"Customer search error: {ex.Message}";
                });
            }
        }

        public void UpdateCustomerQuery(string query)
        {
            Console.WriteLine($"[TransactionViewModel] Updating customer query to: '{query}'");

            CustomerQuery = query;

            _customerSearchTimer.Stop();
            _customerSearchTimer.Start();
        }

        private async Task SearchByBarcodeAsync()
        {
            try
            {
                Console.WriteLine($"[TransactionViewModel] Starting barcode search for: '{BarcodeQuery}'");

                if (string.IsNullOrWhiteSpace(BarcodeQuery))
                {
                    await LoadProductsByCategoryAsync();
                    return;
                }

                StatusMessage = "Searching by barcode...";

                var searchResult = await _productService.FindByAnyBarcodeAsync(BarcodeQuery);

                if (searchResult != null)
                {
                    AddToCart(searchResult.Product, searchResult.IsBoxBarcode, false);
                    BarcodeQuery = string.Empty;
                    StatusMessage = searchResult.IsBoxBarcode ?
        $"Added BOX-{searchResult.Product.Name} to cart." :
        $"Added {searchResult.Product.Name} to cart.";

                    Console.WriteLine($"[TransactionViewModel] Barcode search successful: {searchResult.Product.Name} (IsBox: {searchResult.IsBoxBarcode})");
                }
                else
                {
                    StatusMessage = "No product found with this barcode.";
                    Console.WriteLine($"[TransactionViewModel] No product found for barcode: '{BarcodeQuery}'");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Barcode search error: {ex}");
            }
        }

        private async Task SearchCustomersAsync()
        {
            try
            {
                Console.WriteLine($"[TransactionViewModel] Searching customers for: '{CustomerQuery}'");

                var customers = await _customerService.SearchCustomersAsync(CustomerQuery);

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    SearchedCustomers.Clear();

                    foreach (var customer in customers)
                    {
                        SearchedCustomers.Add(customer);
                    }
                });

                Console.WriteLine($"[TransactionViewModel] Found {customers.Count} customers");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching customers: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Customer search error: {ex}");
            }
        }

        public async void SetSelectedCustomer(Customer customer)
        {
            if (customer == null)
                return;

            try
            {
                Console.WriteLine($"[TransactionViewModel] Setting selected customer: {customer.Name} (ID: {customer.CustomerId})");

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
                Console.WriteLine($"[TransactionViewModel] Error setting customer: {ex.Message}");
                StatusMessage = $"Error selecting customer: {ex.Message}";
            }
        }

        private async Task ApplyCustomerPricingToCartAsync()
        {
            if (CustomerId <= 0 || CartItems.Count == 0)
                return;

            Console.WriteLine($"[TransactionViewModel] Applying customer pricing for customer ID: {CustomerId}");

            var customerPrices = await _customerPriceService.GetAllPricesForCustomerAsync(CustomerId);

            if (customerPrices.Count == 0)
            {
                Console.WriteLine("[TransactionViewModel] No customer-specific prices found");
                return;
            }

            foreach (var item in CartItems)
            {
                if (customerPrices.TryGetValue(item.Product.ProductId, out decimal specialPrice))
                {
                    if (item.UnitPrice != specialPrice)
                    {
                        item.UnitPrice = specialPrice;
                        Console.WriteLine($"[TransactionViewModel] Updated price for {item.Product.Name}: ${specialPrice}");
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
                Console.WriteLine($"[TransactionViewModel] Setting selected customer and filling search: {customer.Name} (ID: {customer.CustomerId})");

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
                Console.WriteLine($"[TransactionViewModel] Error setting customer: {ex.Message}");
                StatusMessage = $"Error selecting customer: {ex.Message}";
            }
        }

        private async Task PrintDrawerReportAsync()
        {
            try
            {
                Console.WriteLine("[TransactionViewModel] Starting PrintDrawerReportAsync...");

                if (CurrentDrawer == null)
                {
                    StatusMessage = "No drawer available to print report.";
                    return;
                }

                IsProcessing = true;
                StatusMessage = $"Preparing drawer report for drawer #{CurrentDrawer.DrawerId}...";

                if (_printQueueManager != null)
                {
                    string jobId = _printQueueManager.EnqueueDrawerReport(CurrentDrawer);
                    StatusMessage = $"Drawer report #{CurrentDrawer.DrawerId} queued for printing (Job ID: {jobId}).";
                }
                else
                {
                    string result = await _receiptPrinterService.PrintDrawerReportAsync(CurrentDrawer);
                    StatusMessage = result;
                }

                Console.WriteLine("[TransactionViewModel] PrintDrawerReportAsync completed");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error printing drawer report: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Drawer report print error: {ex}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void OpenAddCustomerDialog()
        {
            Console.WriteLine("[TransactionViewModel] Opening add customer dialog...");

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

            Console.WriteLine($"[TransactionViewModel] Adding selected product to cart: {product.Name}");

            AddToCart(product);
        }

        private async void AddToCart(Product product, bool isBox = false, bool? isWholesale = null)
        {
            if (product == null)
                return;

            try
            {
                Console.WriteLine($"[TransactionViewModel] Adding to cart: {product.Name} (IsBox: {isBox}, IsWholesale: {isWholesale}, WholesaleMode: {WholesaleMode})");

                bool useWholesale = isWholesale ?? WholesaleMode;

                decimal unitPrice;
                if (isBox)
                {
                    unitPrice = useWholesale ? product.BoxWholesalePrice : product.BoxSalePrice;
                }
                else
                {
                    unitPrice = useWholesale ? product.WholesalePrice : product.SalePrice;
                }

                Console.WriteLine($"[TransactionViewModel] Calculated unit price: ${unitPrice:F2}");

                if (!useWholesale && CustomerId > 0)
                {
                    var specialPrice = await _customerPriceService.GetCustomerProductPriceAsync(CustomerId, product.ProductId);
                    if (specialPrice.HasValue)
                    {
                        unitPrice = specialPrice.Value;
                        Console.WriteLine($"[TransactionViewModel] Applied customer-specific price for {product.Name}: ${unitPrice}");
                    }
                }

                var existingItemIndex = CartItems.ToList().FindIndex(i =>
                    i.Product.ProductId == product.ProductId &&
                    i.IsBox == isBox &&
                    i.IsWholesale == useWholesale);

                if (existingItemIndex >= 0)
                {
                    var existingItem = CartItems[existingItemIndex];

                    existingItem.Quantity += 1;

                    if (existingItem.DiscountType == 1)
                    {
                        decimal currentPercentage = existingItem.DiscountValue;
                        decimal subtotal = existingItem.Quantity * existingItem.UnitPrice;
                        existingItem.Discount = (currentPercentage / 100) * subtotal;
                    }

                    var updatedItem = existingItem;
                    CartItems.RemoveAt(existingItemIndex);
                    CartItems.Insert(existingItemIndex, updatedItem);

                    StatusMessage = $"Updated {(isBox ? $"BOX-{product.Name}" : product.Name)} quantity to {updatedItem.Quantity}";
                    Console.WriteLine($"[TransactionViewModel] Updated existing cart item quantity to {updatedItem.Quantity}");
                }
                else
                {
                    var newItem = new CartItem
                    {
                        Product = product,
                        Quantity = 1,
                        UnitPrice = unitPrice,
                        Discount = 0,
                        DiscountType = 0,
                        IsBox = isBox,
                        IsWholesale = useWholesale
                    };

                    CartItems.Add(newItem);
                    StatusMessage = $"Added {(isBox ? $"BOX-{product.Name}" : product.Name)} to cart.";
                    Console.WriteLine($"[TransactionViewModel] Added new cart item: {product.Name}");
                }

                UpdateTotals();

                // FIXED: Auto-save table state after adding to cart
                AutoSaveCurrentTableState();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error adding to cart: {ex.Message}");
                StatusMessage = $"Error adding product to cart: {ex.Message}";
            }
        }

        public void UpdateCartItemQuantity(CartItem cartItem)
        {
            if (cartItem == null)
                return;

            try
            {
                Console.WriteLine($"[TransactionViewModel] Updating quantity for {cartItem.Product.Name} from {cartItem.Quantity}");

                if (cartItem.Quantity <= 0)
                    cartItem.Quantity = 1;

                decimal subtotal = cartItem.Quantity * cartItem.UnitPrice;

                if (cartItem.DiscountType == 1)
                {
                    cartItem.Discount = (cartItem.DiscountValue / 100) * subtotal;
                }
                else if (cartItem.DiscountType == 0 && cartItem.Discount > subtotal)
                {
                    cartItem.Discount = subtotal;
                    cartItem.DiscountValue = subtotal;
                }

                Console.WriteLine($"[TransactionViewModel] Updated quantity for {cartItem.Product.Name}: Qty={cartItem.Quantity}, " +
                                 $"Subtotal={subtotal}, Discount={cartItem.Discount}, Final={cartItem.Total}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error updating quantity: {ex.Message}");
            }

            cartItem.RefreshCalculations();
            UpdateTotals();

            // FIXED: Auto-save table state after quantity update
            AutoSaveCurrentTableState();
        }
        public void UpdateCartItemDiscount(CartItem cartItem)
        {
            if (cartItem == null)
                return;

            try
            {
                Console.WriteLine($"[TransactionViewModel] Updating discount for {cartItem.Product.Name}");

                decimal subtotal = cartItem.Quantity * cartItem.UnitPrice;

                if (cartItem.DiscountType == 0)
                {
                    if (cartItem.DiscountValue > subtotal)
                        cartItem.DiscountValue = subtotal;

                    cartItem.Discount = cartItem.DiscountValue;
                }
                else if (cartItem.DiscountType == 1)
                {
                    if (cartItem.DiscountValue > 100)
                        cartItem.DiscountValue = 100;

                    cartItem.Discount = (cartItem.DiscountValue / 100) * subtotal;
                }

                Console.WriteLine($"[TransactionViewModel] Updated discount for {cartItem.Product.Name}: Type={cartItem.DiscountType}, " +
                                 $"Value={cartItem.DiscountValue}, Amount={cartItem.Discount}, " +
                                 $"Subtotal={subtotal}, Final={cartItem.Total}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error updating discount: {ex.Message}");
            }

            cartItem.RefreshCalculations();
            UpdateTotals();

            // FIXED: Auto-save table state after discount update
            AutoSaveCurrentTableState();
        }
        private void RemoveFromCart(CartItem cartItem = null)
        {
            var itemToRemove = cartItem ?? SelectedCartItem;

            if (itemToRemove == null)
                return;

            Console.WriteLine($"[TransactionViewModel] Removing from cart: {itemToRemove.Product.Name}");

            CartItems.Remove(itemToRemove);
            UpdateTotals();
            StatusMessage = "Item removed from cart.";

            if (SelectedCartItem == itemToRemove)
                SelectedCartItem = null;

            // FIXED: Auto-save table state after removing from cart
            AutoSaveCurrentTableState();
        }

        private void ClearCart()
        {
            Console.WriteLine("[TransactionViewModel] Clearing cart...");

            CartItems.Clear();
            UpdateTotals();
            StatusMessage = "Cart cleared.";

            LoadedTransaction = null;
            IsTransactionLoaded = false;
            IsEditMode = false;

            // FIXED: Auto-save table state after clearing cart
            AutoSaveCurrentTableState();
        }

        private void UpdateTotals()
        {
            TotalAmount = CartItems.Sum(i => i.Total);

            PaidAmount = TotalAmount;

            CalculateExchangeAmount();
            CalculateAmountToDebt();
            OnPropertyChanged(nameof(ExchangeAmount));
        }

        private void ShowErrorPopup(string title, string message)
        {
            try
            {
                Console.WriteLine($"[TransactionViewModel] Showing error popup: {title} - {message}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    string formattedTitle = title.Contains("Error") ? title : $"{title} Error";

                    MessageBox.Show(
                        message,
                        formattedTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });

                Console.WriteLine($"[TransactionViewModel] [ERROR POPUP] {title}: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Failed to show error popup: {ex.Message}");
            }
        }

        private void ResetTransactionState()
        {
            Console.WriteLine("[TransactionViewModel] Resetting transaction state...");

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
            ChangeDueAmount = 0;
            UseExchangeRate = true;

            LoadedTransaction = null;
            IsTransactionLoaded = false;
            IsEditMode = false;
            CanNavigateNext = false;
            CanNavigatePrevious = false;

            OnPropertyChanged(nameof(IsTransactionLoaded));
            OnPropertyChanged(nameof(LoadedTransaction));
            OnPropertyChanged(nameof(CanCheckout));

            CommandManager.InvalidateRequerySuggested();

            Console.WriteLine("[TransactionViewModel] Transaction state reset completed");
        }

        private void Logout()
        {
            Console.WriteLine("[TransactionViewModel] Logout requested");
            Application.Current.MainWindow.Close();
        }

        private async Task LookupTransactionAsync()
        {
            IsEditMode = false;

            try
            {
                Console.WriteLine($"[TransactionViewModel] Looking up transaction: {TransactionLookupId}");

                if (string.IsNullOrWhiteSpace(TransactionLookupId) || !int.TryParse(TransactionLookupId, out int transactionId) || transactionId <= 0)
                {
                    StatusMessage = "Please enter a valid transaction ID.";
                    LoadedTransaction = null;
                    IsTransactionLoaded = false;
                    CanNavigateNext = false;
                    CanNavigatePrevious = false;
                    CommandManager.InvalidateRequerySuggested();
                    return;
                }

                IsProcessing = true;
                StatusMessage = $"Looking up transaction #{transactionId}...";

                CartItems.Clear();

                Transaction transaction = null;
                try
                {
                    transaction = await _transactionService.GetTransactionWithDetailsAsync(transactionId);
                }
                catch (InvalidCastException ex)
                {
                    Console.WriteLine($"[TransactionViewModel] Type conversion error loading transaction: {ex.Message}");
                    transaction = await LoadTransactionWithDirectQueryAsync(transactionId);
                }

                if (transaction == null)
                {
                    StatusMessage = $"Transaction #{transactionId} not found.";
                    LoadedTransaction = null;
                    IsTransactionLoaded = false;
                    CanNavigateNext = false;
                    CanNavigatePrevious = false;
                    CommandManager.InvalidateRequerySuggested();
                    return;
                }

                await LoadTransactionToCartAsync(transaction);

                LoadedTransaction = transaction;
                IsTransactionLoaded = true;

                if (transaction.CustomerId.HasValue && transaction.CustomerId.Value > 0)
                {
                    CustomerId = transaction.CustomerId.Value;
                    CustomerName = transaction.CustomerName;

                    var customer = await _customerService.GetByIdAsync(transaction.CustomerId.Value);
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
                CommandManager.InvalidateRequerySuggested();

                CalculateExchangeAmount();
                OnPropertyChanged(nameof(ExchangeAmount));

                StatusMessage = $"Loaded transaction #{transactionId} ({transaction.TransactionTypeString}/{transaction.StatusString}) completed on {transaction.FormattedDate}.";
                Console.WriteLine($"[TransactionViewModel] Successfully loaded transaction #{transactionId}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error looking up transaction: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Transaction lookup error: {ex}");
                LoadedTransaction = null;
                IsTransactionLoaded = false;
                CanNavigateNext = false;
                CanNavigatePrevious = false;
                CommandManager.InvalidateRequerySuggested();
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task<Transaction> LoadTransactionWithDirectQueryAsync(int transactionId)
        {
            try
            {
                Console.WriteLine($"[TransactionViewModel] Using direct query fallback for transaction #{transactionId}");

                using var dbContext = new DatabaseContext(ConfigurationService.ConnectionString);

                var transaction = await dbContext.Transactions
                    .FromSqlRaw("SELECT * FROM Transactions WHERE TransactionId = {0}", transactionId)
                    .FirstOrDefaultAsync();

                if (transaction == null)
                {
                    Console.WriteLine($"[TransactionViewModel] Transaction #{transactionId} not found using direct query");
                    return null;
                }

                if (transaction.Status == default && !string.IsNullOrEmpty(transaction.StatusString))
                {
                    transaction.Status = Helpers.EnumConverter.StringToTransactionStatus(transaction.StatusString);
                    Console.WriteLine($"[TransactionViewModel] Converted status from string '{transaction.StatusString}' to enum '{transaction.Status}'");
                }

                if (transaction.TransactionType == default && !string.IsNullOrEmpty(transaction.TransactionTypeString))
                {
                    transaction.TransactionType = Helpers.EnumConverter.StringToTransactionType(transaction.TransactionTypeString);
                    Console.WriteLine($"[TransactionViewModel] Converted type from string '{transaction.TransactionTypeString}' to enum '{transaction.TransactionType}'");
                }

                var details = await dbContext.TransactionDetails
                    .Where(d => d.TransactionId == transactionId)
                    .ToListAsync();

                transaction.Details = details;
                Console.WriteLine($"[TransactionViewModel] Loaded {details.Count} details for transaction #{transactionId} using direct query");

                return transaction;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error in direct query transaction loading: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[TransactionViewModel] Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        private async Task LoadTransactionToCartAsync(Transaction transaction)
        {
            if (transaction == null || transaction.Details == null || !transaction.Details.Any())
                return;

            Console.WriteLine($"[TransactionViewModel] Loading transaction #{transaction.TransactionId} to cart with {transaction.Details.Count} details");

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
                Console.WriteLine($"[TransactionViewModel] Added cart item: {product.Name} x {detail.Quantity}");
            }

            UpdateTotals();
        }

        private async Task<Product> GetProductForTransactionDetailAsync(TransactionDetail detail)
        {
            try
            {
                Console.WriteLine($"[TransactionViewModel] Getting product for transaction detail: ProductId={detail.ProductId}");

                var dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
                var product = await dbContext.Products.FindAsync(detail.ProductId);

                if (product != null)
                {
                    Console.WriteLine($"[TransactionViewModel] Found product: {product.Name}");
                    return product;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error retrieving product: {ex.Message}");
            }

            Console.WriteLine($"[TransactionViewModel] Creating placeholder product for ID: {detail.ProductId}");
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

            Console.WriteLine($"[TransactionViewModel] Entering edit mode for transaction #{LoadedTransaction.TransactionId}");

            IsEditMode = true;
            StatusMessage = $"Editing transaction #{LoadedTransaction.TransactionId}. Make changes and click Save Changes when done.";
        }

        private async Task SaveTransactionChangesAsync()
        {
            try
            {
                if (LoadedTransaction == null)
                    return;

                Console.WriteLine($"[TransactionViewModel] Saving changes to transaction #{LoadedTransaction.TransactionId}");

                IsProcessing = true;
                StatusMessage = $"Saving changes to transaction #{LoadedTransaction.TransactionId}...";

                var updatedTransaction = new Transaction
                {
                    TransactionId = LoadedTransaction.TransactionId,
                    CustomerId = CustomerId > 0 ? CustomerId : null,
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
                    Console.WriteLine($"[TransactionViewModel] Successfully saved changes to transaction #{LoadedTransaction.TransactionId}");
                }
                else
                {
                    StatusMessage = $"Failed to update transaction #{LoadedTransaction.TransactionId}.";
                    Console.WriteLine($"[TransactionViewModel] Failed to save changes to transaction #{LoadedTransaction.TransactionId}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating transaction: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Transaction update error: {ex}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task CheckNavigationAvailabilityAsync(int currentTransactionId)
        {
            try
            {
                Console.WriteLine($"[TransactionViewModel] Checking navigation availability for transaction #{currentTransactionId}");

                var nextId = await _transactionService.GetNextTransactionIdAsync(currentTransactionId);
                CanNavigateNext = nextId.HasValue && nextId.Value > 0;

                var prevId = await _transactionService.GetPreviousTransactionIdAsync(currentTransactionId);
                CanNavigatePrevious = prevId.HasValue && prevId.Value > 0;

                Console.WriteLine($"[TransactionViewModel] Navigation availability updated: Previous={CanNavigatePrevious}, Next={CanNavigateNext}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionViewModel] Error checking navigation availability: {ex.Message}");
                CanNavigateNext = false;
                CanNavigatePrevious = false;
            }
        }

        private void PrintReceipt()
        {
            try
            {
                if (LoadedTransaction == null)
                {
                    StatusMessage = "No transaction loaded to print.";
                    return;
                }

                Console.WriteLine($"[TransactionViewModel] Printing receipt for transaction #{LoadedTransaction.TransactionId}");

                StatusMessage = $"Preparing receipt for transaction #{LoadedTransaction.TransactionId}...";
                IsProcessing = true;

                decimal previousBalance = 0;
                if (CustomerId > 0 && SelectedCustomer != null)
                {
                    try
                    {
                        var customer = _customerService.GetByIdAsync(CustomerId).Result;
                        if (customer != null)
                        {
                            previousBalance = customer.Balance;

                            bool transactionHasDebt = LoadedTransaction.PaidAmount < LoadedTransaction.TotalAmount;
                            if (transactionHasDebt)
                            {
                                decimal transactionDebt = LoadedTransaction.TotalAmount - LoadedTransaction.PaidAmount;
                                previousBalance -= transactionDebt;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TransactionViewModel] Error retrieving customer balance: {ex.Message}");
                    }
                }

                if (_printQueueManager != null)
                {
                    string jobId = _printQueueManager.EnqueueTransactionReceipt(
                        LoadedTransaction,
                        CartItems.ToList(),
                        CustomerId,
                        previousBalance,
                        ExchangeRate);

                    StatusMessage = $"Receipt for transaction #{LoadedTransaction.TransactionId} queued for printing (Job ID: {jobId}).";
                    Console.WriteLine($"[TransactionViewModel] Receipt queued for printing with job ID: {jobId}");
                }
                else
                {
                    _receiptPrinterService.PrintTransactionReceiptWpfAsync(
                        LoadedTransaction,
                        CartItems.ToList(),
                        CustomerId,
                        previousBalance,
                        ExchangeRate).ContinueWith(t =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                StatusMessage = t.Result;
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error printing receipt: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Print receipt error: {ex}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task NavigateToNextTransactionAsync()
        {
            try
            {
                if (!IsTransactionLoaded || LoadedTransaction == null)
                    return;

                int currentId = LoadedTransaction.TransactionId;
                Console.WriteLine($"[TransactionViewModel] Navigating to next transaction after ID: {currentId}");

                var nextId = await _transactionService.GetNextTransactionIdAsync(currentId);

                if (nextId.HasValue && nextId.Value > 0)
                {
                    Console.WriteLine($"[TransactionViewModel] Navigating to next transaction: {nextId.Value}");
                    TransactionLookupId = nextId.Value.ToString();
                    await LookupTransactionAsync();
                }
                else
                {
                    StatusMessage = "No more transactions available.";
                    Console.WriteLine("[TransactionViewModel] No next transaction found");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error navigating to next transaction: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Next transaction navigation error: {ex}");
            }
        }

        private async Task NavigateToPreviousTransactionAsync()
        {
            try
            {
                if (!IsTransactionLoaded || LoadedTransaction == null)
                    return;

                int currentId = LoadedTransaction.TransactionId;
                Console.WriteLine($"[TransactionViewModel] Navigating to previous transaction before ID: {currentId}");

                var prevId = await _transactionService.GetPreviousTransactionIdAsync(currentId);

                if (prevId.HasValue && prevId.Value > 0)
                {
                    Console.WriteLine($"[TransactionViewModel] Navigating to previous transaction: {prevId.Value}");
                    TransactionLookupId = prevId.Value.ToString();
                    await LookupTransactionAsync();
                }
                else
                {
                    StatusMessage = "No previous transactions available.";
                    Console.WriteLine("[TransactionViewModel] No previous transaction found");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error navigating to previous transaction: {ex.Message}";
                Console.WriteLine($"[TransactionViewModel] Previous transaction navigation error: {ex}");
            }
        }

        #endregion
    }
}