// File: QuickTechPOS/Views/CustomerSelectionDialog.xaml.cs

using QuickTechPOS.Models;
using QuickTechPOS.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuickTechPOS.Views
{
    /// <summary>
    /// Interaction logic for CustomerSelectionDialog.xaml
    /// </summary>
    public partial class CustomerSelectionDialog : Window
    {
        private readonly CustomerService _customerService;

        /// <summary>
        /// Gets the selected customer
        /// </summary>
        public Customer SelectedCustomer { get; private set; }

        /// <summary>
        /// Initializes a new instance of the customer selection dialog
        /// </summary>
        public CustomerSelectionDialog()
        {
            InitializeComponent();
            _customerService = new CustomerService();

            // Load customers when dialog opens
            Loaded += CustomerSelectionDialog_Loaded;
        }

        /// <summary>
        /// Loads initial customers when the dialog is shown
        /// </summary>
        private async void CustomerSelectionDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var customers = await _customerService.SearchCustomersAsync("");
                CustomerListView.ItemsSource = customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Searches for customers based on text input
        /// </summary>
        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await SearchCustomersAsync();
        }

        /// <summary>
        /// Handles the search button click
        /// </summary>
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await SearchCustomersAsync();
        }

        /// <summary>
        /// Searches for customers based on the search text
        /// </summary>
        private async System.Threading.Tasks.Task SearchCustomersAsync()
        {
            try
            {
                var searchText = SearchTextBox.Text;
                var customers = await _customerService.SearchCustomersAsync(searchText);
                CustomerListView.ItemsSource = customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles double-click on a customer in the list
        /// </summary>
        private void CustomerListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CustomerListView.SelectedItem is Customer customer)
            {
                SelectedCustomer = customer;
                DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// Handles the select button click
        /// </summary>
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerListView.SelectedItem is Customer customer)
            {
                SelectedCustomer = customer;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a customer from the list.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Handles the cancel button click
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}