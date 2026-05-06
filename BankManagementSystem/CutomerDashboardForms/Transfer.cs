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

        // State management - keep as instance variable (not static)
        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        public Tansfer(PersonModel customer)
        {
            InitializeComponent();

            // Blocker 3 & 4 Fix: Initialize Amazon SQS client
            sqsClient = new AmazonSQSClient();
            
            // Get queue URL from environment variable
            queueUrl = Environment.GetEnvironmentVariable("SQS_TRANSFER_AUDIT_QUEUE_URL") 
                ?? "https://sqs.us-east-1.amazonaws.com/123456789012/transfer-audit";

            // Send message to SQS asynchronously
            SendMessageToSQS("Transfer initiated").Wait();
        }

        // Blocker 3 & 4 Fix: Method to send messages to Amazon SQS
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
                // Log error - in production, use proper logging framework
                Console.WriteLine($"Failed to send message to SQS: {ex.Message}");
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
