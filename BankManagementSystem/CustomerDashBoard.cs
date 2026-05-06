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

        // Blocker 14 Fix: Remove static state from singleton pattern
        // Use instance variable instead of static/singleton with state
        public List<string> NavigationHistory = new List<string>();

        private readonly IAmazonSimpleSystemsManagement ssmClient;

        public CustomerDashBoard(PersonModel customer)
        {
            personModel = customer;
            InitializeComponent();

            // Blocker 16 Fix: Replace Windows Registry with AWS Systems Manager Parameter Store
            ssmClient = new AmazonSimpleSystemsManagementClient();
            
            // Load configuration from Parameter Store instead of Registry
            LoadConfigurationFromParameterStore();
        }

        private async void LoadConfigurationFromParameterStore()
        {
            try
            {
                // Replace Registry.CurrentUser.OpenSubKey(@"Software\BankApp") 
                // with Parameter Store hierarchical parameters
                var request = new GetParametersByPathRequest
                {
                    Path = "/BankApp/",
                    Recursive = true,
                    WithDecryption = true
                };

                var response = await ssmClient.GetParametersByPathAsync(request);

                // Process configuration parameters
                foreach (var parameter in response.Parameters)
                {
                    // Use configuration values as needed
                    Console.WriteLine($"Config: {parameter.Name} = {parameter.Value}");
                }
            }
            catch (Exception ex)
            {
                // Log error - in production use proper logging framework
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
