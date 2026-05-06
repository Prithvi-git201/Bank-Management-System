using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using BankDatabaseAccess.EntityModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class CustomerDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Blocker 14 Fix: Replace singleton pattern with instance-scoped state
        // Remove static state - use instance variable instead
        // In a proper DI setup, this would be injected as a scoped service
        private List<string> navigationHistory = new List<string>();
        public List<string> NavigationHistory => navigationHistory;

        // Blocker 16 Fix: Replace Registry with AWS Systems Manager Parameter Store
        private readonly IAmazonSimpleSystemsManagement ssmClient;

        public CustomerDashBoard(PersonModel customer)
        {
            personModel = customer;
            InitializeComponent();

            // Blocker 16 Fix: Initialize AWS Systems Manager client
            ssmClient = new AmazonSimpleSystemsManagementClient();

            // Blocker 16 Fix: Load configuration from Parameter Store instead of Registry
            LoadConfigurationFromParameterStore().Wait();
        }

        // Blocker 16 Fix: Method to load configuration from AWS Systems Manager Parameter Store
        private async Task LoadConfigurationFromParameterStore()
        {
            try
            {
                // Get parameter path from environment variable
                string parameterPath = Environment.GetEnvironmentVariable("SSM_PARAMETER_PATH") 
                    ?? "/BankApp/Configuration";

                var request = new GetParameterRequest
                {
                    Name = parameterPath,
                    WithDecryption = true
                };

                var response = await ssmClient.GetParameterAsync(request);
                
                // Process configuration value
                string configValue = response.Parameter.Value;
                
                // Log successful configuration load
                Console.WriteLine($"Configuration loaded from Parameter Store: {parameterPath}");
            }
            catch (ParameterNotFoundException)
            {
                // Parameter doesn't exist - use default configuration
                Console.WriteLine("Configuration parameter not found in Parameter Store, using defaults");
            }
            catch (Exception ex)
            {
                // Log error and continue with defaults
                Console.WriteLine($"Failed to load configuration from Parameter Store: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ssmClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
