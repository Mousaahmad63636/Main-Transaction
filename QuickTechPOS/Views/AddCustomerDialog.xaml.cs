// File: QuickTechPOS/Views/AddCustomerDialog.xaml.cs

using QuickTechPOS.Models;
using QuickTechPOS.Services;
using System;
using System.Windows;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Interaction logic for AddCustomerDialog.xaml
    /// </summary>
    public partial class AddCustomerDialog : Window
    {
        private readonly CustomerService _customerService;

        /// <summary>
        /// Gets the newly added customer
        /// </summary>
        public Customer Customer { get; private set; }

        /// <summary>
        /// Initializes a new instance of the add customer dialog
        /// </summary>
        public AddCustomerDialog()
        {
            InitializeComponent();
            _customerService = new CustomerService();
        }

        /// <summary>
        /// Handles the save button click event
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Customer name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    NameTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
                {
                    MessageBox.Show("Phone number is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PhoneTextBox.Focus();
                    return;
                }

                var customer = new Customer
                {
                    Name = NameTextBox.Text.Trim(),
                    Phone = PhoneTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Address = AddressTextBox.Text.Trim(),
                    Balance = 0,
                    IsActive = true
                };

                // Save customer to database
                Customer = await _customerService.AddCustomerAsync(customer);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the cancel button click event
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}