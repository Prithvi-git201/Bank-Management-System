using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        // Blocker 14 Fix: Remove static state - use instance variable instead
        private decimal LastDepositAmount;
        
        private readonly IAmazonS3 s3Client;
        private readonly string s3BucketName;

        public Deposit()
        {
            InitializeComponent();

            // Blocker 11 & 13 Fix: Initialize S3 client for cloud storage
            s3Client = new AmazonS3Client();
            s3BucketName = Environment.GetEnvironmentVariable("S3_DEPOSIT_CACHE_BUCKET") 
                ?? "bank-deposit-cache";
        }

        private void DepositBtn_Click(object sender, System.EventArgs e)
        {
            decimal.TryParse(AmountTextBox.Text, out LastDepositAmount);

            // Blocker 9, 11, 13 Fix: Replace hard-coded path and File.WriteAllText with S3
            // Store deposit cache in S3 instead of local file system (D:\DepositCache\last.txt)
            string cacheKey = "DepositCache/last.txt";
            WriteToS3(cacheKey, LastDepositAmount.ToString());
        }

        private async void WriteToS3(string key, string content)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = key,
                    ContentBody = content,
                    ContentType = "text/plain"
                };

                await s3Client.PutObjectAsync(putRequest);
            }
            catch (Exception ex)
            {
                // Log error - in production use proper logging framework
                Console.WriteLine($"Failed to write to S3: {ex.Message}");
                MessageBox.Show($"Failed to save deposit cache: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                s3Client?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
