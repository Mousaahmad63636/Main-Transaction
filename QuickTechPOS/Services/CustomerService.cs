using Microsoft.EntityFrameworkCore;
using QuickTechPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickTechPOS.Services
{
    public class CustomerService
    {
        private readonly DatabaseContext _dbContext;

        public CustomerService()
        {
            _dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
        }

        public async Task<List<Customer>> SearchCustomersAsync(string query, int maxResults = 10)
        {
            try
            {
                Console.WriteLine($"Searching for customers with query: '{query}'");

                if (string.IsNullOrWhiteSpace(query))
                {
                    var allCustomers = await _dbContext.Customers
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.Name)
                        .Take(maxResults)
                        .ToListAsync();

                    Console.WriteLine($"Found {allCustomers.Count} active customers for empty query");
                    return allCustomers;
                }

                var customers = await _dbContext.Customers
                    .Where(c => c.IsActive &&
                               (c.Name.Contains(query) ||
                                c.Phone.Contains(query)))
                    .OrderBy(c => c.Name)
                    .Take(maxResults)
                    .ToListAsync();

                Console.WriteLine($"Found {customers.Count} customers matching '{query}'");
                return customers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchCustomersAsync: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return new List<Customer>();
            }
        }

        public async Task<Customer> GetByIdAsync(int customerId)
        {
            try
            {
                return await _dbContext.Customers.FindAsync(customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            try
            {
                customer.CreatedAt = DateTime.Now;
                customer.IsActive = true;

                _dbContext.Customers.Add(customer);
                await _dbContext.SaveChangesAsync();

                return customer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddCustomerAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateCustomerBalanceAsync(int customerId, decimal balanceChange)
        {
            try
            {
                Console.WriteLine($"[CustomerService] UpdateCustomerBalanceAsync called - Customer ID: {customerId}, Balance Change: {balanceChange:C2}");

                var customer = await _dbContext.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    Console.WriteLine($"[CustomerService] ERROR: Customer with ID {customerId} not found.");
                    return false;
                }

                decimal oldBalance = customer.Balance;
                customer.Balance += balanceChange;
                customer.UpdatedAt = DateTime.Now;

                Console.WriteLine($"[CustomerService] Updating customer {customer.Name} (ID: {customerId}) " +
                                 $"balance from {oldBalance:C2} by {balanceChange:C2} to {customer.Balance:C2}");

                await _dbContext.SaveChangesAsync();

                Console.WriteLine($"[CustomerService] Customer balance update saved successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CustomerService] ERROR in UpdateCustomerBalanceAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[CustomerService] Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }
}