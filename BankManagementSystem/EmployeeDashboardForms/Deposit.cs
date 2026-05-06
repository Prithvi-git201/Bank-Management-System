using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        // Blocker 9, 11, 13 Fix: Remove static state - use instance variable
        // Static state causes issues in multi-instance cloud deployments
        private decimal lastDepositAmount;

        // Blocker 11, 13 Fix: Replace local file writes with Amazon S3
        private readonly IAmazonS3 s3Client;
        private readonly string bucketName;

        public Deposit()
        {
            InitializeComponent();

            // Blocker 11, 13 Fix: Initialize Amazon S3 client
            s3Client = new AmazonS3Client();
            bucketName = Environment.GetEnvironmentVariable("S3_DEPOSIT_CACHE_BUCKET") 
                ?? "bank-deposit-cache";
        }

        private void DepositBtn_Click(object sender, System.EventArgs e)
        {
            decimal.TryParse(AmountTextBox.Text, out lastDepositAmount);

            // Blocker 9, 11, 13 Fix: Replace File.WriteAllText with S3 upload
            // Use environment variable for path configuration
            WriteDepositToS3(lastDepositAmount).Wait();
        }

        // Blocker 9, 11, 13 Fix: Method to write deposit amount to S3
        private async Task WriteDepositToS3(decimal amount)
        {
            try
            {
                // Generate key with timestamp for uniqueness
                string key = $"deposits/{DateTimeOffset.UtcNow:yyyy/MM/dd}/last-{Guid.NewGuid()}.txt";

                // Upload to S3 instead of local file system
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(amount.ToString())))
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        InputStream = stream,
                        ContentType = "text/plain",
                        Metadata =
                        {
                            ["amount"] = amount.ToString(),
                            ["timestamp"] = DateTimeOffset.UtcNow.ToString("o")
                        }
                    };

                    await s3Client.PutObjectAsync(putRequest);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write deposit to S3: {ex.Message}");
                MessageBox.Show($"Failed to save deposit information: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
