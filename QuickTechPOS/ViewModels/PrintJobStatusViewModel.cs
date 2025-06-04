// File: QuickTechPOS/ViewModels/PrintJobStatusViewModel.cs

using QuickTechPOS.Helpers;
using QuickTechPOS.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace QuickTechPOS.ViewModels
{
    /// <summary>
    /// View model for the print job status dialog
    /// </summary>
    public class PrintJobStatusViewModel : BaseViewModel
    {
        private readonly PrintQueueManager _printQueueManager;
        private readonly DispatcherTimer _refreshTimer;
        private ObservableCollection<PrintJobInfo> _printJobs;
        private PrintJobInfo _selectedJob;
        private string _statusMessage;

        /// <summary>
        /// Gets or sets the collection of print jobs
        /// </summary>
        public ObservableCollection<PrintJobInfo> PrintJobs
        {
            get => _printJobs;
            set => SetProperty(ref _printJobs, value);
        }

        /// <summary>
        /// Gets or sets the selected print job
        /// </summary>
        public PrintJobInfo SelectedJob
        {
            get => _selectedJob;
            set
            {
                if (SetProperty(ref _selectedJob, value))
                {
                    OnPropertyChanged(nameof(CanRetryJob));
                    OnPropertyChanged(nameof(CanCancelJob));
                    OnPropertyChanged(nameof(CanRemoveJob));
                }
            }
        }

        /// <summary>
        /// Gets or sets the status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Gets whether the selected job can be retried
        /// </summary>
        public bool CanRetryJob => SelectedJob != null && SelectedJob.Status == PrintJobStatus.Failed;

        /// <summary>
        /// Gets whether the selected job can be cancelled
        /// </summary>
        public bool CanCancelJob => SelectedJob != null &&
                                  (SelectedJob.Status == PrintJobStatus.Queued ||
                                   SelectedJob.Status == PrintJobStatus.Failed);

        /// <summary>
        /// Gets whether the selected job can be removed
        /// </summary>
        public bool CanRemoveJob => SelectedJob != null &&
                                  (SelectedJob.Status == PrintJobStatus.Completed ||
                                   SelectedJob.Status == PrintJobStatus.Canceled);

        /// <summary>
        /// Command to retry the selected job
        /// </summary>
        public ICommand RetryJobCommand { get; }

        /// <summary>
        /// Command to cancel the selected job
        /// </summary>
        public ICommand CancelJobCommand { get; }

        /// <summary>
        /// Command to remove the selected job
        /// </summary>
        public ICommand RemoveJobCommand { get; }

        /// <summary>
        /// Command to refresh the job list
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Command to close the dialog
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// Event that occurs when the dialog should be closed
        /// </summary>
        public event EventHandler<bool> RequestClose;

        /// <summary>
        /// Initializes a new instance of the print job status view model
        /// </summary>
        /// <param name="printQueueManager">The print queue manager</param>
        public PrintJobStatusViewModel(PrintQueueManager printQueueManager)
        {
            _printQueueManager = printQueueManager ?? throw new ArgumentNullException(nameof(printQueueManager));
            PrintJobs = new ObservableCollection<PrintJobInfo>();

            // Subscribe to job status changes
            _printQueueManager.PrintJobStatusChanged += PrintQueueManager_PrintJobStatusChanged;

            // Set up commands
            RetryJobCommand = new RelayCommand(RetryJob, param => CanRetryJob);
            CancelJobCommand = new RelayCommand(CancelJob, param => CanCancelJob);
            RemoveJobCommand = new RelayCommand(RemoveJob, param => CanRemoveJob);
            RefreshCommand = new RelayCommand(param => RefreshJobs());
            CloseCommand = new RelayCommand(param => Close());

            // Set up refresh timer
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _refreshTimer.Tick += (s, e) => RefreshJobs();
            _refreshTimer.Start();

            // Initial refresh
            RefreshJobs();
        }

        /// <summary>
        /// Handles the PrintJobStatusChanged event
        /// </summary>
        private void PrintQueueManager_PrintJobStatusChanged(object sender, PrintJobStatusChangedEventArgs e)
        {
            // Make sure we update on the UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RefreshJobs();

                // Show status message for important events
                if (e.NewStatus == PrintJobStatus.Completed)
                {
                    StatusMessage = $"Print job {e.JobId} completed successfully.";
                }
                else if (e.NewStatus == PrintJobStatus.Failed)
                {
                    StatusMessage = $"Print job {e.JobId} failed: {e.ErrorMessage}";
                }
            });
        }

        /// <summary>
        /// Retries the selected job
        /// </summary>
        private void RetryJob(object parameter)
        {
            if (SelectedJob == null)
                return;

            bool success = _printQueueManager.RetryJob(SelectedJob.Id);

            if (success)
            {
                StatusMessage = $"Retrying print job {SelectedJob.Id}...";
            }
            else
            {
                StatusMessage = $"Unable to retry print job {SelectedJob.Id}.";
            }

            RefreshJobs();
        }

        /// <summary>
        /// Cancels the selected job
        /// </summary>
        private void CancelJob(object parameter)
        {
            if (SelectedJob == null)
                return;

            bool success = _printQueueManager.CancelJob(SelectedJob.Id);

            if (success)
            {
                StatusMessage = $"Canceled print job {SelectedJob.Id}.";
            }
            else
            {
                StatusMessage = $"Unable to cancel print job {SelectedJob.Id}.";
            }

            RefreshJobs();
        }

        /// <summary>
        /// Removes the selected job
        /// </summary>
        private void RemoveJob(object parameter)
        {
            if (SelectedJob == null)
                return;

            bool success = _printQueueManager.RemoveJob(SelectedJob.Id);

            if (success)
            {
                StatusMessage = $"Removed print job {SelectedJob.Id}.";

                // Remove from our collection
                var job = PrintJobs.FirstOrDefault(j => j.Id == SelectedJob.Id);
                if (job != null)
                {
                    PrintJobs.Remove(job);
                }

                SelectedJob = null;
            }
            else
            {
                StatusMessage = $"Unable to remove print job {SelectedJob.Id}.";
            }
        }

        /// <summary>
        /// Refreshes the job list
        /// </summary>
        private void RefreshJobs()
        {
            var jobs = _printQueueManager.GetAllJobs();

            // Update on the UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Remember selected job ID
                string selectedJobId = SelectedJob?.Id;

                PrintJobs.Clear();

                foreach (var job in jobs.OrderByDescending(j => j.CreatedAt))
                {
                    PrintJobs.Add(job);
                }

                // Restore selection if possible
                if (selectedJobId != null)
                {
                    SelectedJob = PrintJobs.FirstOrDefault(j => j.Id == selectedJobId);
                }

                // Update command availability
                CommandManager.InvalidateRequerySuggested();
            });
        }

        /// <summary>
        /// Closes the dialog
        /// </summary>
        private void Close()
        {
            _refreshTimer.Stop();
            _printQueueManager.PrintJobStatusChanged -= PrintQueueManager_PrintJobStatusChanged;
            RequestClose?.Invoke(this, true);
        }
    }
}