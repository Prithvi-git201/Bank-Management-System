using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        // Blocker 5 & 6 Fix: Replace MSMQ with Amazon SQS
        private readonly IAmazonSQS sqsClient;
        private readonly string queueUrl;

        // Blocker 10, 12 Fix: Replace local file writes with Amazon S3
        private readonly IAmazonS3 s3Client;
        private readonly string bucketName;

        public CustomerInfo()
        {
            InitializeComponent();

            // Blocker 5 & 6 Fix: Initialize Amazon SQS client
            sqsClient = new AmazonSQSClient();
            queueUrl = Environment.GetEnvironmentVariable("SQS_CUSTOMER_INFO_QUEUE_URL") 
                ?? "https://sqs.us-east-1.amazonaws.com/123456789012/customer-info";

            // Blocker 10, 12 Fix: Initialize Amazon S3 client
            s3Client = new AmazonS3Client();
            bucketName = Environment.GetEnvironmentVariable("S3_CUSTOMER_LOGS_BUCKET") 
                ?? "bank-customer-logs";

            // Send message to SQS
            SendMessageToSQS("Customer viewed").Wait();

            // Blocker 8, 10, 12, 17 Fix: Write to S3 with UTC timestamp
            WriteAccessLogToS3().Wait();
        }

        // Blocker 5 & 6 Fix: Method to send messages to Amazon SQS
        private async Task SendMessageToSQS(string messageBody)
        {
            try
            {
                var sendMessageRequest = new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = messageBody,
                    MessageAttributes = new System.Collections.Generic.Dictionary<string, MessageAttributeValue>
                    {
                        {
                            "Timestamp",
                            new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = DateTimeOffset.UtcNow.ToString("o")
                            }
                        }
                    }
                };

                await sqsClient.SendMessageAsync(sendMessageRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message to SQS: {ex.Message}");
            }
        }

        // Blocker 8, 10, 12, 17 Fix: Replace File.AppendAllText with S3 upload
        // Use DateTimeOffset.UtcNow instead of DateTime.Now for timezone consistency
        private async Task WriteAccessLogToS3()
        {
            try
            {
                // Blocker 17 Fix: Use DateTimeOffset.UtcNow for UTC timestamp
                string timestamp = DateTimeOffset.UtcNow.ToString("o");
                string logContent = $"Customer accessed at: {timestamp}\n";

                // Generate unique key for the log entry
                string key = $"access-logs/{DateTimeOffset.UtcNow:yyyy/MM/dd}/access-{Guid.NewGuid()}.log";

                // Blocker 10, 12 Fix: Upload to S3 instead of local file system
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(logContent)))
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        InputStream = stream,
                        ContentType = "text/plain",
                        Metadata =
                        {
                            ["timestamp"] = timestamp,
                            ["event"] = "customer-access"
                        }
                    };

                    await s3Client.PutObjectAsync(putRequest);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write access log to S3: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sqsClient?.Dispose();
                s3Client?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
