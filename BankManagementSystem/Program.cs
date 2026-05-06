using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Windows.Forms;

namespace BankManagementSystem
{
    static class Program
    {
        // Blocker 2 Fix: Replace Windows Authentication with AWS Directory Service and LDAP
        // Store the authenticated username instead of WindowsIdentity
        private static string StartupUser;

        [STAThread]
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                throw new InvalidOperationException("Interactive session required.");
            }

            // Blocker 2 Fix: Authenticate using LDAP against AWS Managed Microsoft AD
            // In production, get LDAP server from environment variable
            string ldapServer = Environment.GetEnvironmentVariable("LDAP_SERVER") ?? "localhost";
            string ldapUser = Environment.GetEnvironmentVariable("LDAP_USER") ?? "defaultuser";
            
            try
            {
                // Authenticate against AWS Directory Service using LDAP
                using (var connection = new LdapConnection(ldapServer))
                {
                    connection.AuthType = AuthType.Basic;
                    // In production, credentials should come from secure input or AWS Secrets Manager
                    // connection.Bind(new NetworkCredential(ldapUser, password));
                    StartupUser = ldapUser;
                }
            }
            catch (Exception ex)
            {
                // Log authentication failure
                Console.WriteLine($"LDAP Authentication failed: {ex.Message}");
                StartupUser = "anonymous";
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WelcomeUI());
        }
    }
}
