using BankDatabaseAccess.EntityModel;
using System;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class EmployeeDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Blocker 1 Fix: Replace Windows Authentication with LDAP
        // Store username instead of WindowsIdentity
        private string CurrentEmployeeSession;

        // Blocker 7 Fix: Replace hard-coded path with environment variable
        private FileStream auditStream;

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();

            // Blocker 1 Fix: Replace Windows Authentication with AWS Directory Service LDAP
            // Authenticate using LDAP against AWS Managed Microsoft AD
            CurrentEmployeeSession = AuthenticateWithLDAP();

            // Blocker 7 Fix: Use environment variable for audit log path
            string auditLogPath = Environment.GetEnvironmentVariable("AUDIT_LOG_PATH") 
                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "session.log");
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(auditLogPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            auditStream = new FileStream(
                auditLogPath,
                FileMode.OpenOrCreate);
        }

        // Blocker 1 Fix: LDAP authentication method for AWS Managed Microsoft AD
        private string AuthenticateWithLDAP()
        {
            try
            {
                // Get LDAP configuration from environment variables
                string ldapServer = Environment.GetEnvironmentVariable("LDAP_SERVER") ?? "localhost";
                string ldapPort = Environment.GetEnvironmentVariable("LDAP_PORT") ?? "389";
                string ldapBaseDn = Environment.GetEnvironmentVariable("LDAP_BASE_DN") ?? "DC=example,DC=com";
                string username = Environment.GetEnvironmentVariable("LDAP_USERNAME") ?? "user";
                string password = Environment.GetEnvironmentVariable("LDAP_PASSWORD") ?? "";

                // Create LDAP connection
                LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(ldapServer, int.Parse(ldapPort));
                NetworkCredential credential = new NetworkCredential(username, password, ldapBaseDn);
                
                using (LdapConnection connection = new LdapConnection(identifier))
                {
                    connection.Credential = credential;
                    connection.AuthType = AuthType.Basic;
                    connection.Bind();
                    
                    return username;
                }
            }
            catch (Exception ex)
            {
                // Log error and return default user
                Console.WriteLine($"LDAP Authentication failed: {ex.Message}");
                return "anonymous";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                auditStream?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
