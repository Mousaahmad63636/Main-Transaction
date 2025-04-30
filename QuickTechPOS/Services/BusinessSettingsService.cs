// File: QuickTechPOS/Services/BusinessSettingsService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace QuickTechPOS.Services
{
    public class BusinessSettingsService
    {
        private readonly DatabaseContext _dbContext;
        private Dictionary<string, string> _cachedSettings;

        public BusinessSettingsService()
        {
            _dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
            _cachedSettings = new Dictionary<string, string>();
        }

        public async Task<string> GetSettingAsync(string key, string defaultValue = "")
        {
            if (_cachedSettings.TryGetValue(key, out string value))
            {
                return value;
            }

            try
            {
                var setting = await _dbContext.Database
                    .SqlQueryRaw<BusinessSetting>($"SELECT [Id], [Key], [Value], [Description], [Group], [DataType], [IsSystem], [LastModified], [ModifiedBy] FROM [QuickTechSystem].[dbo].[BusinessSettings] WHERE [Key] = '{key}'")
                    .FirstOrDefaultAsync();

                if (setting != null)
                {
                    _cachedSettings[key] = setting.Value;
                    return setting.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting business setting {key}: {ex.Message}");
            }

            return defaultValue;
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            try
            {
                var settings = await _dbContext.Database
                    .SqlQueryRaw<BusinessSetting>("SELECT [Id], [Key], [Value], [Description], [Group], [DataType], [IsSystem], [LastModified], [ModifiedBy] FROM [QuickTechSystem].[dbo].[BusinessSettings]")
                    .ToListAsync();

                _cachedSettings = settings.ToDictionary(s => s.Key, s => s.Value);
                return _cachedSettings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all business settings: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        public async Task<string> GetCompanyNameAsync() => await GetSettingAsync("CompanyName", "QuickTech Systems");
        public async Task<string> GetAddressAsync() => await GetSettingAsync("Address", "");
        public async Task<string> GetPhoneAsync() => await GetSettingAsync("Phone", "");
        public async Task<string> GetEmailAsync() => await GetSettingAsync("Email", "");
        public async Task<string> GetCurrencyAsync() => await GetSettingAsync("Currency", "USD");

        public async Task<decimal> GetTaxRateAsync()
        {
            var taxRateStr = await GetSettingAsync("TaxRate", "0");
            if (decimal.TryParse(taxRateStr, out decimal taxRate))
            {
                return taxRate;
            }
            return 0;
        }

        public async Task<decimal> GetExchangeRateAsync()
        {
            var rateStr = await GetSettingAsync("ExchangeRate", "90000");
            if (decimal.TryParse(rateStr, out decimal rate))
            {
                return rate;
            }
            return 90000;
        }

        public async Task<string> GetReceiptFooter1Async() => await GetSettingAsync("ReceiptFooter1", "Stay caffeinated!!");
        public async Task<string> GetReceiptFooter2Async() => await GetSettingAsync("ReceiptFooter2", "See you next time");
    }

    public class BusinessSetting
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public string DataType { get; set; }
        public bool IsSystem { get; set; }
        public DateTime LastModified { get; set; }
        public string ModifiedBy { get; set; }
    }
}