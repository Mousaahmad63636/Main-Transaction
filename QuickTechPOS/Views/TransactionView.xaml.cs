// File: QuickTechPOS/Views/TransactionView.xaml.cs

using QuickTechPOS.Models;
using QuickTechPOS.Services;
using QuickTechPOS.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Interaction logic for TransactionView.xaml
    /// </summary>
    public partial class TransactionView : UserControl
    {
        private readonly TransactionViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the transaction view
        /// </summary>
        /// <param name="viewModel">The view model for this view</param>
        public TransactionView(TransactionViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        /// <summary>
        /// Handles the key down event for the barcode search textbox
        /// </summary>
        private void BarcodeSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.SearchBarcodeCommand.Execute(null);
                e.Handled = true;
            }
        }

        private async Task CheckForOpenDrawerAsync(AuthenticationService authService)
        {
            if (authService.CurrentEmployee == null)
                return;

            try
            {
                string cashierId = authService.CurrentEmployee.EmployeeId.ToString();
                var drawerService = new DrawerService();
                var openDrawer = await drawerService.GetOpenDrawerAsync(cashierId);

                if (openDrawer == null)
                {
                    // Show the open drawer dialog
                    var viewModel = new OpenDrawerViewModel(authService);
                    var dialog = new OpenDrawerDialog(viewModel);
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for open drawer: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the text changed event for the product search combobox
        /// </summary>
        private void ProductSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                _viewModel.UpdateNameQuery(comboBox.Text);

                // Keep dropdown open while typing
                comboBox.IsDropDownOpen = !string.IsNullOrWhiteSpace(comboBox.Text);
            }
        }

        /// <summary>
        /// Handles the selection changed event for the product search combobox
        /// </summary>
        private void ProductSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is Product product)
            {
                _viewModel.AddSelectedProductToCart(product);
                comboBox.Text = string.Empty;
                comboBox.SelectedItem = null;
            }
        }

        /// <summary>
        /// Handles the text changed event for the customer search combobox
        /// </summary>
        private void CustomerSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                _viewModel.UpdateCustomerQuery(comboBox.Text);

                // Keep dropdown open while typing
                comboBox.IsDropDownOpen = !string.IsNullOrWhiteSpace(comboBox.Text);
            }
        }

        /// <summary>
        /// Handles the selection changed event for the customer search combobox
        /// </summary>
        // File: QuickTechPOS/Views/TransactionView.xaml.cs

        // File: QuickTechPOS/Views/TransactionView.xaml.cs

        private void CustomerSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is Customer customer)
            {
                _viewModel.SetSelectedCustomer(customer);

                // Update the CustomerQuery property directly instead of manipulating the ComboBox
                _viewModel.CustomerQuery = customer.Name;
            }
        }

        /// <summary>
        /// Handles the preview text input event for the customer search combobox
        /// </summary>
        private void CustomerSearch_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
            {
                // Schedule the caret position restoration after the text update
                comboBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Restore caret to the end of the text
                    textBox.CaretIndex = textBox.Text.Length;

                    // Clear any selection
                    textBox.SelectionLength = 0;
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        /// <summary>
        /// Handles the mouse double click event for customer suggestion items
        /// </summary>
        private void CustomerItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement element && element.DataContext is Customer customer)
            {
                // Select the customer and fill the search field with the customer name
                _viewModel.SetSelectedCustomerAndFillSearch(customer);

                // Prevent the event from bubbling up
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the lost focus event for quantity textbox
        /// </summary>
        private void Quantity_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItem cartItem)
            {
                _viewModel.UpdateCartItemQuantity(cartItem);
            }
        }

        /// <summary>
        /// Handles the lost focus event for discount textbox
        /// </summary>
        private void Discount_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItem cartItem)
            {
                _viewModel.UpdateCartItemDiscount(cartItem);
            }
        }

        /// <summary>
        /// Handles the selection changed event for discount type combobox
        /// </summary>
        private void DiscountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is CartItem cartItem)
            {
                _viewModel.UpdateCartItemDiscount(cartItem);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}