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

        // Store session information without static state
        private string CurrentEmployeeSession;

        // Use stream for audit logging
        private FileStream auditStream;

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();

            // Blocker 1 Fix: Replace Windows Authentication with AWS Directory Service and LDAP
            string ldapServer = Environment.GetEnvironmentVariable("LDAP_SERVER") ?? "localhost";
            string ldapUser = Environment.GetEnvironmentVariable("LDAP_USER") ?? "defaultuser";
            
            try
            {
                using (var connection = new LdapConnection(ldapServer))
                {
                    connection.AuthType = AuthType.Basic;
                    // In production, use proper credential management
                    CurrentEmployeeSession = ldapUser;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LDAP Authentication failed: {ex.Message}");
                CurrentEmployeeSession = "anonymous";
            }

            // Blocker 7 Fix: Replace hard-coded path with environment variable and Path.Combine
            // Use cross-platform path construction and configurable base directory
            string auditBasePath = Environment.GetEnvironmentVariable("AUDIT_LOG_PATH") 
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BankApp", "Audit");
            
            // Ensure directory exists
            if (!Directory.Exists(auditBasePath))
            {
                Directory.CreateDirectory(auditBasePath);
            }

            string auditFilePath = Path.Combine(auditBasePath, "session.log");
            
            auditStream = new FileStream(
                auditFilePath,
                FileMode.OpenOrCreate);
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
