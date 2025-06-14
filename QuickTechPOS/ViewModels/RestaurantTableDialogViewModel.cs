using QuickTechPOS.Helpers;
using QuickTechPOS.Models;
using QuickTechPOS.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickTechPOS.ViewModels
{
    public class RestaurantTableDialogViewModel : BaseViewModel
    {
        #region Private Fields

        private readonly RestaurantTableService _tableService;
        private ObservableCollection<RestaurantTable> _tables;
        private ObservableCollection<RestaurantTable> _filteredTables;
        private RestaurantTable _selectedTable;
        private string _searchQuery;
        private string _selectedStatusFilter;
        private bool _isLoading;
        private string _statusMessage;
        private Dictionary<string, int> _tableStatistics;
        private bool _showOnlyAvailable;
        private bool _hasLoadedInitially;

        #endregion

        #region Public Properties

        public ObservableCollection<RestaurantTable> Tables
        {
            get => _tables;
            set => SetProperty(ref _tables, value);
        }

        public ObservableCollection<RestaurantTable> FilteredTables
        {
            get => _filteredTables;
            set => SetProperty(ref _filteredTables, value);
        }

        public RestaurantTable SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (SetProperty(ref _selectedTable, value))
                {
                    OnPropertyChanged(nameof(IsTableSelected));
                    OnPropertyChanged(nameof(SelectedTableInfo));
                }
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    ApplyFiltersAsync();
                }
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value))
                {
                    ApplyFiltersAsync();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public Dictionary<string, int> TableStatistics
        {
            get => _tableStatistics;
            set => SetProperty(ref _tableStatistics, value);
        }

        public bool ShowOnlyAvailable
        {
            get => _showOnlyAvailable;
            set
            {
                if (SetProperty(ref _showOnlyAvailable, value))
                {
                    ApplyFiltersAsync();
                }
            }
        }

        public bool IsTableSelected => SelectedTable != null;

        public string SelectedTableInfo => SelectedTable?.TableInfo ?? "No table selected";

        public List<string> StatusFilterOptions { get; }

        public int AvailableTableCount => Tables?.Count(t => t.IsAvailable) ?? 0;

        public int OccupiedTableCount => Tables?.Count(t => t.IsOccupied) ?? 0;

        public int FilteredTableCount => FilteredTables?.Count ?? 0;

        #endregion

        #region Commands

        public ICommand SelectTableCommand { get; }
        public ICommand RefreshTablesCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ShowStatisticsCommand { get; }

        #endregion

        #region Events

        public event EventHandler<RestaurantTable> TableSelected;
        public event EventHandler<bool> DialogClose;

        #endregion

        #region Constructor

        public RestaurantTableDialogViewModel()
        {
            _tableService = new RestaurantTableService();

            Tables = new ObservableCollection<RestaurantTable>();
            FilteredTables = new ObservableCollection<RestaurantTable>();
            TableStatistics = new Dictionary<string, int>();

            StatusFilterOptions = new List<string>
            {
                "All Statuses",
                "Available",
                "Occupied",
                "Reserved",
                "Out of Service"
            };

            _selectedStatusFilter = "All Statuses";
            _searchQuery = string.Empty;
            _showOnlyAvailable = false;
            _hasLoadedInitially = false;

            SelectTableCommand = new RelayCommand(
                param => SelectTable(param as RestaurantTable),
                param => param is RestaurantTable table && table.IsActive);

            RefreshTablesCommand = new RelayCommand(
                async param => await RefreshTablesAsync(),
                param => !IsLoading);

            ClearFiltersCommand = new RelayCommand(
                param => ClearAllFilters(),
                param => !IsLoading);

            CancelCommand = new RelayCommand(
                param => CancelSelection(),
                param => true);

            ShowStatisticsCommand = new RelayCommand(
                async param => await LoadTableStatisticsAsync(),
                param => !IsLoading);

            LoadTablesAsync();
        }

        #endregion

        #region Public Methods

        public async Task LoadTablesAsync()
        {
            try
            {
                if (_hasLoadedInitially && IsLoading)
                {
                    return;
                }

                IsLoading = true;
                StatusMessage = "Loading restaurant tables...";

                var loadedTables = await _tableService.GetAllTablesAsync();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Tables.Clear();
                    foreach (var table in loadedTables)
                    {
                        Tables.Add(table);
                    }
                });

                await ApplyFiltersAsync();
                await LoadTableStatisticsAsync();

                OnPropertyChanged(nameof(AvailableTableCount));
                OnPropertyChanged(nameof(OccupiedTableCount));

                StatusMessage = $"Loaded {Tables.Count} tables successfully";
                _hasLoadedInitially = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading tables: {ex.Message}";

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Tables.Clear();
                    FilteredTables.Clear();
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ApplyFiltersAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var filteredResults = Tables.AsEnumerable();

                    if (!string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        var searchLower = SearchQuery.ToLower().Trim();
                        filteredResults = filteredResults.Where(t =>
                            t.TableNumber.ToString().Contains(searchLower) ||
                            (!string.IsNullOrEmpty(t.Description) && t.Description.ToLower().Contains(searchLower)));
                    }

                    if (!string.IsNullOrEmpty(SelectedStatusFilter) && SelectedStatusFilter != "All Statuses")
                    {
                        filteredResults = filteredResults.Where(t =>
                            string.Equals(t.Status, SelectedStatusFilter, StringComparison.OrdinalIgnoreCase));
                    }

                    if (ShowOnlyAvailable)
                    {
                        filteredResults = filteredResults.Where(t => t.IsAvailable);
                    }

                    var sortedResults = filteredResults.OrderBy(t => t.TableNumber).ToList();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        FilteredTables.Clear();
                        foreach (var table in sortedResults)
                        {
                            FilteredTables.Add(table);
                        }

                        OnPropertyChanged(nameof(FilteredTableCount));
                    });
                });
            }
            catch (Exception ex)
            {
                StatusMessage = "Error filtering tables";
            }
        }

        #endregion

        #region Private Methods

        private void SelectTable(RestaurantTable table)
        {
            try
            {
                if (table == null)
                {
                    StatusMessage = "Invalid table selection";
                    return;
                }

                if (!table.IsActive)
                {
                    StatusMessage = "Cannot select inactive table";
                    return;
                }

                SelectedTable = table;
                StatusMessage = $"Selected {table.DisplayName}";

                TableSelected?.Invoke(this, table);
                DialogClose?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                StatusMessage = "Error selecting table";
            }
        }

        private async Task RefreshTablesAsync()
        {
            try
            {
                StatusMessage = "Refreshing table data...";
                await LoadTablesAsync();
                StatusMessage = "Table data refreshed successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error refreshing table data";
            }
        }

        private void ClearAllFilters()
        {
            try
            {
                SearchQuery = string.Empty;
                SelectedStatusFilter = "All Statuses";
                ShowOnlyAvailable = false;
                StatusMessage = "Filters cleared";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error clearing filters";
            }
        }

        private void CancelSelection()
        {
            try
            {
                SelectedTable = null;
                StatusMessage = "Selection cancelled";
                DialogClose?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                StatusMessage = "Error during cancellation";
            }
        }

        private async Task LoadTableStatisticsAsync()
        {
            try
            {
                var stats = await _tableService.GetTableStatisticsAsync();
                TableStatistics = stats;
                OnPropertyChanged(nameof(TableStatistics));
            }
            catch (Exception ex)
            {
                StatusMessage = "Error loading statistics";
            }
        }

        #endregion

        #region Dispose Pattern

        public void Dispose()
        {
            try
            {
                _tableService?.Dispose();
            }
            catch (Exception ex)
            {
                // Silent disposal
            }
        }

        #endregion
    }
}