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
        private readonly IAmazonSQS sqsClient;
        private readonly IAmazonS3 s3Client;
        private readonly string queueUrl;
        private readonly string s3BucketName;

        public CustomerInfo()
        {
            InitializeComponent();

            // Blocker 5 & 6 Fix: Replace MSMQ with Amazon SQS
            sqsClient = new AmazonSQSClient();
            queueUrl = Environment.GetEnvironmentVariable("SQS_CUSTOMER_INFO_QUEUE_URL") 
                ?? "https://sqs.us-east-1.amazonaws.com/123456789012/customer-info";

            // Blocker 10 & 12 Fix: Replace local file writes with Amazon S3
            s3Client = new AmazonS3Client();
            s3BucketName = Environment.GetEnvironmentVariable("S3_CUSTOMER_LOGS_BUCKET") 
                ?? "bank-customer-logs";

            // Send audit message to SQS
            SendAuditMessage("Customer viewed");

            // Blocker 8 & 17 Fix: Replace hard-coded path with S3 and use UTC time
            // Store access log in S3 instead of local file system
            string logKey = $"CustomerLogs/access-{DateTimeOffset.UtcNow:yyyy-MM-dd}.log";
            AppendLogToS3(logKey, DateTimeOffset.UtcNow.ToString("o"));
        }

        private async void SendAuditMessage(string message)
        {
            try
            {
                var sendMessageRequest = new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = message,
                    MessageAttributes = 
                    {
                        ["Timestamp"] = new MessageAttributeValue 
                        { 
                            DataType = "String", 
                            StringValue = DateTimeOffset.UtcNow.ToString("o") 
                        }
                    }
                };

                await sqsClient.SendMessageAsync(sendMessageRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send SQS message: {ex.Message}");
            }
        }

        private async void AppendLogToS3(string key, string content)
        {
            try
            {
                // Read existing content if file exists
                string existingContent = "";
                try
                {
                    var getRequest = new GetObjectRequest
                    {
                        BucketName = s3BucketName,
                        Key = key
                    };

                    using (var response = await s3Client.GetObjectAsync(getRequest))
                    using (var reader = new StreamReader(response.ResponseStream))
                    {
                        existingContent = await reader.ReadToEndAsync();
                    }
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // File doesn't exist yet, that's okay
                }

                // Append new content
                string newContent = existingContent + content + Environment.NewLine;

                // Write back to S3
                var putRequest = new PutObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = key,
                    ContentBody = newContent,
                    ContentType = "text/plain"
                };

                await s3Client.PutObjectAsync(putRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to S3: {ex.Message}");
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
