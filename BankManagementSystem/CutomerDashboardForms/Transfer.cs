using Amazon.SQS;
using Amazon.SQS.Model;
using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        // Blocker 3 & 4 Fix: Replace MSMQ with Amazon SQS
        private readonly IAmazonSQS sqsClient;
        private readonly string queueUrl;

        // Store recent transfers without static state
        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        public Tansfer(PersonModel customer)
        {
            InitializeComponent();

            // Blocker 3 & 4 Fix: Initialize Amazon SQS client
            // In production, use AWS SDK configuration from environment or IAM roles
            sqsClient = new AmazonSQSClient();
            
            // Get queue URL from environment variable or use default
            queueUrl = Environment.GetEnvironmentVariable("SQS_TRANSFER_AUDIT_QUEUE_URL") 
                ?? "https://sqs.us-east-1.amazonaws.com/123456789012/transfer-audit";

            // Send message asynchronously to SQS
            SendTransferAuditMessage("Transfer initiated");
        }

        private async void SendTransferAuditMessage(string message)
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
                // Log error - in production use proper logging framework
                Console.WriteLine($"Failed to send SQS message: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sqsClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
