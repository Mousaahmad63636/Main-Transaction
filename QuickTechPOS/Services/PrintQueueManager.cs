// File: QuickTechPOS/Services/PrintQueueManager.cs

using QuickTechPOS.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Printing;

namespace QuickTechPOS.Services
{
    /// <summary>
    /// Manages a queue of print jobs with automatic retry capabilities for failed prints
    /// </summary>
    public class PrintQueueManager
    {
        private readonly ReceiptPrinterService _printerService;
        private readonly ConcurrentQueue<PrintJob> _printQueue;
        private readonly ConcurrentDictionary<string, PrintJob> _activeJobs;
        private readonly SemaphoreSlim _processingSemaphore;
        private readonly Timer _retryTimer;
        private bool _isProcessing;
        private readonly BusinessSettingsService _settingsService;
        private readonly string _backupDirectory;

        private const int MAX_RETRY_ATTEMPTS = 5;
        private const int INITIAL_RETRY_DELAY_MS = 1000;
        private readonly int[] RETRY_DELAYS = { 1000, 2000, 5000, 10000, 30000 }; // Increasing backoff

        public event EventHandler<PrintJobStatusChangedEventArgs> PrintJobStatusChanged;

        /// <summary>
        /// Initializes a new instance of the print queue manager
        /// </summary>
        public PrintQueueManager()
        {
            _printerService = new ReceiptPrinterService();
            _settingsService = new BusinessSettingsService();
            _printQueue = new ConcurrentQueue<PrintJob>();
            _activeJobs = new ConcurrentDictionary<string, PrintJob>();
            _processingSemaphore = new SemaphoreSlim(1, 1);
            _retryTimer = new Timer(RetryFailedJobs, null, 10000, 10000); // Check every 10 seconds

            // Create backup directory
            _backupDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "QuickTechPOS", "FailedReceipts");

            Directory.CreateDirectory(_backupDirectory);

            // Attempt to recover any previously failed print jobs at startup
            Task.Run(RecoverFailedPrintJobsAsync);
        }

        /// <summary>
        /// Enqueues a transaction receipt to be printed with automatic retries on failure
        /// </summary>
        /// <param name="transaction">The transaction to print</param>
        /// <param name="cartItems">The cart items for the transaction</param>
        /// <param name="customerId">The customer ID</param>
        /// <param name="previousCustomerBalance">Previous customer balance</param>
        /// <param name="exchangeRate">Exchange rate for alternative currency</param>
        /// <returns>The print job ID that can be used to check status</returns>
        public string EnqueueTransactionReceipt(
            Transaction transaction,
            List<CartItem> cartItems,
            int customerId = 0,
            decimal previousCustomerBalance = 0,
            decimal exchangeRate = 90000)
        {
            // Generate a unique job ID
            string jobId = $"TR-{transaction.TransactionId}-{DateTime.Now.Ticks}";

            var printJob = new PrintJob
            {
                Id = jobId,
                Type = PrintJobType.TransactionReceipt,
                CreatedAt = DateTime.Now,
                Status = PrintJobStatus.Queued,
                RetryCount = 0,
                LastAttempt = null,
                NextRetry = DateTime.Now,
                TransactionId = transaction.TransactionId,
                Transaction = transaction,
                CartItems = cartItems,
                CustomerId = customerId,
                PreviousCustomerBalance = previousCustomerBalance,
                ExchangeRate = exchangeRate
            };

            // Save job data for backup/recovery purposes
            SaveJobData(printJob);

            // Add to queue
            _printQueue.Enqueue(printJob);
            _activeJobs[jobId] = printJob;

            // Notify listeners
            OnPrintJobStatusChanged(printJob);

            // Start processing if not already running
            StartProcessingQueue();

            return jobId;
        }

        /// <summary>
        /// Enqueues a drawer report to be printed with automatic retries on failure
        /// </summary>
        /// <param name="drawer">The drawer to print a report for</param>
        /// <returns>The print job ID that can be used to check status</returns>
        public string EnqueueDrawerReport(Drawer drawer)
        {
            // Generate a unique job ID
            string jobId = $"DR-{drawer.DrawerId}-{DateTime.Now.Ticks}";

            var printJob = new PrintJob
            {
                Id = jobId,
                Type = PrintJobType.DrawerReport,
                CreatedAt = DateTime.Now,
                Status = PrintJobStatus.Queued,
                RetryCount = 0,
                LastAttempt = null,
                NextRetry = DateTime.Now,
                DrawerId = drawer.DrawerId,
                Drawer = drawer
            };

            // Save job data for backup/recovery purposes
            SaveJobData(printJob);

            // Add to queue
            _printQueue.Enqueue(printJob);
            _activeJobs[jobId] = printJob;

            // Notify listeners
            OnPrintJobStatusChanged(printJob);

            // Start processing if not already running
            StartProcessingQueue();

            return jobId;
        }

        /// <summary>
        /// Gets the status of a print job
        /// </summary>
        /// <param name="jobId">The job ID to check</param>
        /// <returns>The current status of the job, or null if not found</returns>
        public PrintJobInfo GetJobStatus(string jobId)
        {
            if (_activeJobs.TryGetValue(jobId, out var job))
            {
                return new PrintJobInfo
                {
                    Id = job.Id,
                    Status = job.Status,
                    CreatedAt = job.CreatedAt,
                    LastAttempt = job.LastAttempt,
                    RetryCount = job.RetryCount,
                    NextRetry = job.NextRetry,
                    ErrorMessage = job.ErrorMessage
                };
            }

            return null;
        }

        /// <summary>
        /// Gets all active print jobs in the system
        /// </summary>
        /// <returns>List of print job information</returns>
        public List<PrintJobInfo> GetAllJobs()
        {
            return _activeJobs.Values.Select(j => new PrintJobInfo
            {
                Id = j.Id,
                Status = j.Status,
                CreatedAt = j.CreatedAt,
                LastAttempt = j.LastAttempt,
                RetryCount = j.RetryCount,
                NextRetry = j.NextRetry,
                ErrorMessage = j.ErrorMessage
            }).ToList();
        }

        /// <summary>
        /// Manually triggers a retry for a failed print job
        /// </summary>
        /// <param name="jobId">The job ID to retry</param>
        /// <returns>True if the job was found and queued for retry, false otherwise</returns>
        public bool RetryJob(string jobId)
        {
            if (_activeJobs.TryGetValue(jobId, out var job) &&
                (job.Status == PrintJobStatus.Failed || job.Status == PrintJobStatus.Canceled))
            {
                job.Status = PrintJobStatus.Queued;
                job.NextRetry = DateTime.Now;

                // Re-enqueue the job
                _printQueue.Enqueue(job);

                // Notify listeners
                OnPrintJobStatusChanged(job);

                // Start processing if not already running
                StartProcessingQueue();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Cancels a pending or failed print job
        /// </summary>
        /// <param name="jobId">The job ID to cancel</param>
        /// <returns>True if the job was found and canceled, false otherwise</returns>
        public bool CancelJob(string jobId)
        {
            if (_activeJobs.TryGetValue(jobId, out var job) &&
                (job.Status == PrintJobStatus.Queued || job.Status == PrintJobStatus.Failed))
            {
                job.Status = PrintJobStatus.Canceled;

                // Notify listeners
                OnPrintJobStatusChanged(job);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a completed or canceled job from the active jobs list
        /// </summary>
        /// <param name="jobId">The job ID to remove</param>
        /// <returns>True if the job was found and removed, false otherwise</returns>
        public bool RemoveJob(string jobId)
        {
            if (_activeJobs.TryGetValue(jobId, out var job) &&
                (job.Status == PrintJobStatus.Completed || job.Status == PrintJobStatus.Canceled))
            {
                return _activeJobs.TryRemove(jobId, out _);
            }

            return false;
        }

        /// <summary>
        /// Starts processing the print queue if not already running
        /// </summary>
        private async void StartProcessingQueue()
        {
            if (_isProcessing)
                return;

            await _processingSemaphore.WaitAsync();

            try
            {
                _isProcessing = true;

                // Process all queued jobs
                while (_printQueue.TryDequeue(out var job))
                {
                    // Skip canceled jobs
                    if (job.Status == PrintJobStatus.Canceled)
                        continue;

                    job.Status = PrintJobStatus.Processing;
                    job.LastAttempt = DateTime.Now;
                    OnPrintJobStatusChanged(job);

                    try
                    {
                        bool success = false;

                        switch (job.Type)
                        {
                            case PrintJobType.TransactionReceipt:
                                string transactionResult = await _printerService.PrintTransactionReceiptWpfAsync(
                                    job.Transaction,
                                    job.CartItems,
                                    job.CustomerId,
                                    job.PreviousCustomerBalance,
                                    job.ExchangeRate);

                                success = !transactionResult.Contains("cancelled") &&
                                         !transactionResult.Contains("error") &&
                                         !transactionResult.Contains("failed");

                                if (!success)
                                {
                                    job.ErrorMessage = transactionResult;
                                }
                                break;

                            case PrintJobType.DrawerReport:
                                string drawerResult = await _printerService.PrintDrawerReportAsync(job.Drawer);

                                success = !drawerResult.Contains("cancelled") &&
                                         !drawerResult.Contains("error") &&
                                         !drawerResult.Contains("failed");

                                if (!success)
                                {
                                    job.ErrorMessage = drawerResult;
                                }
                                break;
                        }

                        if (success)
                        {
                            // Print was successful
                            job.Status = PrintJobStatus.Completed;
                            job.ErrorMessage = null;

                            // Delete backup file if it exists
                            DeleteJobBackup(job.Id);
                        }
                        else
                        {
                            // Print failed, schedule a retry if attempts remaining
                            if (job.RetryCount < MAX_RETRY_ATTEMPTS)
                            {
                                job.Status = PrintJobStatus.Failed;
                                job.RetryCount++;

                                // Use exponential backoff for retry delays
                                int delayIndex = Math.Min(job.RetryCount - 1, RETRY_DELAYS.Length - 1);
                                int delayMs = RETRY_DELAYS[delayIndex];

                                job.NextRetry = DateTime.Now.AddMilliseconds(delayMs);
                                Console.WriteLine($"Print job {job.Id} failed. Retry #{job.RetryCount} scheduled at {job.NextRetry}");

                                // Update backup with retry information
                                SaveJobData(job);
                            }
                            else
                            {
                                // Max retries exceeded
                                job.Status = PrintJobStatus.Failed;

                                // Still save the backup for manual recovery
                                SaveJobData(job);
                                Console.WriteLine($"Print job {job.Id} failed after {MAX_RETRY_ATTEMPTS} retries.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Unexpected error during printing
                        job.Status = PrintJobStatus.Failed;
                        job.ErrorMessage = $"Exception: {ex.Message}";
                        job.RetryCount++;

                        // Use exponential backoff for retry delays
                        int delayIndex = Math.Min(job.RetryCount - 1, RETRY_DELAYS.Length - 1);
                        int delayMs = RETRY_DELAYS[delayIndex];

                        job.NextRetry = DateTime.Now.AddMilliseconds(delayMs);

                        Console.WriteLine($"Error processing print job {job.Id}: {ex.Message}");

                        // Save backup data for recovery
                        SaveJobData(job);
                    }

                    // Notify about the job status change
                    OnPrintJobStatusChanged(job);
                }
            }
            finally
            {
                _isProcessing = false;
                _processingSemaphore.Release();
            }
        }

        /// <summary>
        /// Timer callback that checks for failed jobs that are ready for retry
        /// </summary>
        private void RetryFailedJobs(object state)
        {
            // Find failed jobs that are ready for retry
            var jobsToRetry = _activeJobs.Values
                .Where(j => j.Status == PrintJobStatus.Failed &&
                           j.RetryCount < MAX_RETRY_ATTEMPTS &&
                           j.NextRetry <= DateTime.Now)
                .ToList();

            if (jobsToRetry.Count > 0)
            {
                Console.WriteLine($"Found {jobsToRetry.Count} failed print jobs ready for retry");

                foreach (var job in jobsToRetry)
                {
                    // Put the job back in the queue
                    job.Status = PrintJobStatus.Queued;
                    _printQueue.Enqueue(job);

                    // Notify about the status change
                    OnPrintJobStatusChanged(job);
                }

                // Start processing the queue
                StartProcessingQueue();
            }
        }

        /// <summary>
        /// Raises the PrintJobStatusChanged event
        /// </summary>
        /// <param name="job">The job that changed status</param>
        private void OnPrintJobStatusChanged(PrintJob job)
        {
            PrintJobStatusChanged?.Invoke(this, new PrintJobStatusChangedEventArgs
            {
                JobId = job.Id,
                NewStatus = job.Status,
                ErrorMessage = job.ErrorMessage
            });
        }

        /// <summary>
        /// Saves print job data to disk for recovery purposes
        /// </summary>
        /// <param name="job">The job to save</param>
        private void SaveJobData(PrintJob job)
        {
            try
            {
                string fileName = Path.Combine(_backupDirectory, $"{job.Id}.json");
                string json = System.Text.Json.JsonSerializer.Serialize(job);
                File.WriteAllText(fileName, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving print job data: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a job backup file
        /// </summary>
        /// <param name="jobId">The job ID to delete</param>
        private void DeleteJobBackup(string jobId)
        {
            try
            {
                string fileName = Path.Combine(_backupDirectory, $"{jobId}.json");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting print job backup: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts to recover any previously failed print jobs from disk
        /// </summary>
        private async Task RecoverFailedPrintJobsAsync()
        {
            try
            {
                if (!Directory.Exists(_backupDirectory))
                    return;

                var files = Directory.GetFiles(_backupDirectory, "*.json");
                Console.WriteLine($"Found {files.Length} print job backup files to recover");

                foreach (var file in files)
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(file);
                        var job = System.Text.Json.JsonSerializer.Deserialize<PrintJob>(json);

                        if (job != null)
                        {
                            // If the job was completed or canceled, just delete the backup file
                            if (job.Status == PrintJobStatus.Completed || job.Status == PrintJobStatus.Canceled)
                            {
                                File.Delete(file);
                                continue;
                            }

                            // Otherwise, queue it up for retry
                            job.Status = PrintJobStatus.Queued;
                            job.NextRetry = DateTime.Now;

                            // Add to the queue
                            _printQueue.Enqueue(job);
                            _activeJobs[job.Id] = job;

                            Console.WriteLine($"Recovered print job {job.Id}, type: {job.Type}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error recovering print job from {file}: {ex.Message}");
                    }
                }

                // If we recovered any jobs, start processing them
                if (_printQueue.Count > 0)
                {
                    StartProcessingQueue();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during print job recovery: {ex.Message}");
            }
        }

        /// <summary>
        /// Disposes resources used by the print queue manager
        /// </summary>
        public void Dispose()
        {
            _retryTimer?.Dispose();
            _processingSemaphore?.Dispose();
        }
    }

    /// <summary>
    /// Print job information exposed to clients
    /// </summary>
    public class PrintJobInfo
    {
        public string Id { get; set; }
        public PrintJobStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastAttempt { get; set; }
        public int RetryCount { get; set; }
        public DateTime? NextRetry { get; set; }
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets a user-friendly status message
        /// </summary>
        public string StatusMessage
        {
            get
            {
                switch (Status)
                {
                    case PrintJobStatus.Queued:
                        return "Waiting to print...";
                    case PrintJobStatus.Processing:
                        return "Printing...";
                    case PrintJobStatus.Completed:
                        return "Printed successfully";
                    case PrintJobStatus.Failed:
                        if (NextRetry.HasValue && NextRetry.Value > DateTime.Now)
                        {
                            return $"Print failed. Retrying in {(NextRetry.Value - DateTime.Now).TotalSeconds:F0} seconds...";
                        }
                        return $"Print failed after {RetryCount} attempts. {ErrorMessage}";
                    case PrintJobStatus.Canceled:
                        return "Print job canceled";
                    default:
                        return "Unknown status";
                }
            }
        }
    }

    /// <summary>
    /// Internal representation of a print job
    /// </summary>
    internal class PrintJob
    {
        public string Id { get; set; }
        public PrintJobType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public PrintJobStatus Status { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastAttempt { get; set; }
        public DateTime? NextRetry { get; set; }
        public string ErrorMessage { get; set; }

        // Transaction receipt data
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; }
        public List<CartItem> CartItems { get; set; }
        public int CustomerId { get; set; }
        public decimal PreviousCustomerBalance { get; set; }
        public decimal ExchangeRate { get; set; }

        // Drawer report data
        public int DrawerId { get; set; }
        public Drawer Drawer { get; set; }
    }

    /// <summary>
    /// Types of print jobs
    /// </summary>
    public enum PrintJobType
    {
        TransactionReceipt,
        DrawerReport
    }

    /// <summary>
    /// Status of a print job
    /// </summary>
    public enum PrintJobStatus
    {
        Queued,
        Processing,
        Completed,
        Failed,
        Canceled
    }

    /// <summary>
    /// Event arguments for print job status changes
    /// </summary>
    public class PrintJobStatusChangedEventArgs : EventArgs
    {
        public string JobId { get; set; }
        public PrintJobStatus NewStatus { get; set; }
        public string ErrorMessage { get; set; }
    }
}