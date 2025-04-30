// File: QuickTechPOS/Services/ReceiptPrinterService.cs

using QuickTechPOS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QuickTechPOS.Services
{
    public class ReceiptPrinterService
    {
        private readonly BusinessSettingsService _settingsService;

        public ReceiptPrinterService()
        {
            _settingsService = new BusinessSettingsService();
        }

        public async Task<string> GenerateTransactionReceiptAsync(Transaction transaction, List<CartItem> cartItems, decimal exchangeRate = 90000, bool useAlternativeCurrency = false)
        {
            var receipt = new StringBuilder();

            try
            {
                var companyName = await _settingsService.GetCompanyNameAsync();
                var address = await _settingsService.GetAddressAsync();
                var phone = await _settingsService.GetPhoneAsync();
                var currency = await _settingsService.GetCurrencyAsync();
                var footer1 = await _settingsService.GetReceiptFooter1Async();
                var footer2 = await _settingsService.GetReceiptFooter2Async();

                receipt.AppendLine("===================================");
                receipt.AppendLine("             RECEIPT              ");
                receipt.AppendLine("===================================");
                receipt.AppendLine();

                receipt.AppendLine($"          {companyName}          ");
                if (!string.IsNullOrEmpty(address))
                {
                    receipt.AppendLine($"      {address}       ");
                }
                if (!string.IsNullOrEmpty(phone))
                {
                    receipt.AppendLine($"         {phone}          ");
                }
                receipt.AppendLine();

                receipt.AppendLine($"Transaction #: {transaction.TransactionId}");
                receipt.AppendLine($"Date: {transaction.TransactionDate:yyyy-MM-dd HH:mm:ss}");
                receipt.AppendLine($"Cashier: {transaction.CashierName}");
                receipt.AppendLine($"Customer: {transaction.CustomerName}");
                receipt.AppendLine($"Payment Method: {transaction.PaymentMethod}");
                receipt.AppendLine();

                receipt.AppendLine("------------- ITEMS ---------------");
                receipt.AppendLine("Qty  Description            Price   Total");
                receipt.AppendLine("------------------------------------");

                foreach (var item in cartItems)
                {
                    string description = item.Product.Name;
                    if (description.Length > 20)
                    {
                        description = description.Substring(0, 17) + "...";
                    }

                    decimal itemPrice = item.UnitPrice;
                    decimal itemTotal = item.Total;

                    receipt.AppendLine($"{item.Quantity,-4:F2} {description,-20} {currency}{itemPrice,-6:F2} {currency}{itemTotal:F2}");
                }

                receipt.AppendLine("------------------------------------");

                receipt.AppendLine($"Subtotal:         {currency}{transaction.TotalAmount:F2}");

                if (useAlternativeCurrency)
                {
                    decimal alternativeAmount = transaction.TotalAmount * exchangeRate;
                    receipt.AppendLine($"Exchange Rate:     1 {currency} = {exchangeRate:F0} LBP");
                    receipt.AppendLine($"Total (LBP):       LBP {alternativeAmount:F0}");
                }

                receipt.AppendLine($"Amount Paid:       {currency}{transaction.PaidAmount:F2}");
                receipt.AppendLine($"Change:            {currency}{transaction.ChangeAmount:F2}");
                receipt.AppendLine();

                if (!string.IsNullOrEmpty(footer1))
                {
                    receipt.AppendLine(footer1);
                }
                if (!string.IsNullOrEmpty(footer2))
                {
                    receipt.AppendLine(footer2);
                }
                receipt.AppendLine();
                receipt.AppendLine("===================================");
                receipt.AppendLine("         Thank You!               ");
                receipt.AppendLine("===================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating receipt: {ex.Message}");
                receipt.AppendLine($"Error generating receipt: {ex.Message}");
            }

            return receipt.ToString();
        }

        public string GenerateDrawerReport(Drawer drawer)
        {
            var receipt = new StringBuilder();

            try
            {
                receipt.AppendLine("===================================");
                receipt.AppendLine("           DRAWER REPORT           ");
                receipt.AppendLine("===================================");
                receipt.AppendLine();

                receipt.AppendLine("          QUICKTECH POS            ");
                receipt.AppendLine("      123 Main St, Suite 101       ");
                receipt.AppendLine("       Anytown, ST 12345           ");
                receipt.AppendLine("         (555) 123-4567            ");
                receipt.AppendLine();

                receipt.AppendLine($"Report Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                receipt.AppendLine($"Drawer ID: {drawer.DrawerId}");
                receipt.AppendLine($"Status: {drawer.Status}");
                receipt.AppendLine();

                receipt.AppendLine($"Cashier: {drawer.CashierName}");
                receipt.AppendLine($"Opened At: {drawer.FormattedOpenedAt}");
                if (drawer.ClosedAt.HasValue)
                {
                    receipt.AppendLine($"Closed At: {drawer.FormattedClosedAt}");
                }

                if (!string.IsNullOrEmpty(drawer.Notes))
                {
                    receipt.AppendLine();
                    receipt.AppendLine("------------- NOTES ---------------");
                    receipt.AppendLine(drawer.Notes);
                }

                receipt.AppendLine();

                receipt.AppendLine("------------ BALANCES --------------");
                receipt.AppendLine($"Opening Balance:    ${drawer.OpeningBalance:F2}");
                receipt.AppendLine($"Closing Balance:    ${drawer.CurrentBalance:F2}");
                receipt.AppendLine();

                receipt.AppendLine("------------ SALES -----------------");
                receipt.AppendLine($"Daily Sales:        ${drawer.DailySales:F2}");
                receipt.AppendLine($"Total Sales:        ${drawer.TotalSales:F2}");
                receipt.AppendLine();

                receipt.AppendLine("------------ CASH FLOW ------------");
                receipt.AppendLine($"Cash Out:          ${drawer.CashOut:F2}");
                receipt.AppendLine();

                receipt.AppendLine("------------ SUMMARY --------------");
                receipt.AppendLine($"Net Sales:          ${drawer.NetSales:F2}");
                receipt.AppendLine($"Net Cash Flow:      ${drawer.NetCashFlow:F2}");
                receipt.AppendLine();

                receipt.AppendLine("===================================");
                receipt.AppendLine("         End of Report             ");
                receipt.AppendLine("===================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating drawer report: {ex.Message}");
                receipt.AppendLine($"Error generating report: {ex.Message}");
            }

            return receipt.ToString();
        }

        public async Task<string> GenerateDrawerReportAsync(Drawer drawer)
        {
            try
            {
                var companyName = await _settingsService.GetCompanyNameAsync();
                var address = await _settingsService.GetAddressAsync();
                var phone = await _settingsService.GetPhoneAsync();

                var receipt = new StringBuilder();

                receipt.AppendLine("===================================");
                receipt.AppendLine("           DRAWER REPORT           ");
                receipt.AppendLine("===================================");
                receipt.AppendLine();

                receipt.AppendLine($"          {companyName}          ");
                if (!string.IsNullOrEmpty(address))
                {
                    receipt.AppendLine($"      {address}       ");
                }
                if (!string.IsNullOrEmpty(phone))
                {
                    receipt.AppendLine($"         {phone}          ");
                }
                receipt.AppendLine();

                receipt.AppendLine($"Report Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                receipt.AppendLine($"Drawer ID: {drawer.DrawerId}");
                receipt.AppendLine($"Status: {drawer.Status}");
                receipt.AppendLine();

                receipt.AppendLine($"Cashier: {drawer.CashierName}");
                receipt.AppendLine($"Opened At: {drawer.FormattedOpenedAt}");
                if (drawer.ClosedAt.HasValue)
                {
                    receipt.AppendLine($"Closed At: {drawer.FormattedClosedAt}");
                }

                if (!string.IsNullOrEmpty(drawer.Notes))
                {
                    receipt.AppendLine();
                    receipt.AppendLine("------------- NOTES ---------------");
                    receipt.AppendLine(drawer.Notes);
                }

                receipt.AppendLine();

                receipt.AppendLine("------------ BALANCES --------------");
                receipt.AppendLine($"Opening Balance:    ${drawer.OpeningBalance:F2}");
                receipt.AppendLine($"Closing Balance:    ${drawer.CurrentBalance:F2}");
                receipt.AppendLine();

                receipt.AppendLine("------------ SALES -----------------");
                receipt.AppendLine($"Daily Sales:        ${drawer.DailySales:F2}");
                receipt.AppendLine($"Total Sales:        ${drawer.TotalSales:F2}");
                receipt.AppendLine();

                receipt.AppendLine("------------ CASH FLOW ------------");
                receipt.AppendLine($"Cash Out:          ${drawer.CashOut:F2}");
                receipt.AppendLine();

                receipt.AppendLine("------------ SUMMARY --------------");
                receipt.AppendLine($"Net Sales:          ${drawer.NetSales:F2}");
                receipt.AppendLine($"Net Cash Flow:      ${drawer.NetCashFlow:F2}");
                receipt.AppendLine();

                receipt.AppendLine("===================================");
                receipt.AppendLine("         End of Report             ");
                receipt.AppendLine("===================================");

                return receipt.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating drawer report: {ex.Message}");
                return $"Error generating drawer report: {ex.Message}";
            }
        }

        public async Task<bool> PrintReceiptAsync(string receiptContent)
        {
            try
            {
                Console.WriteLine("Printing receipt:");
                Console.WriteLine(receiptContent);

                await Task.Delay(500);

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LastReceipt.txt");
                await File.WriteAllTextAsync(filePath, receiptContent);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error printing receipt: {ex.Message}");
                return false;
            }
        }
    }
}