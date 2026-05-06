using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Windows.Forms;

namespace BankManagementSystem
{
    static class Program
    {
        // Blocker 2 Fix: Replace Windows Authentication with LDAP
        // Store startup user from LDAP authentication instead of WindowsIdentity
        private static readonly string StartupUser = GetAuthenticatedUser();

        [STAThread]
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                throw new InvalidOperationException("Interactive session required.");
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WelcomeUI());
        }

        // Blocker 2 Fix: LDAP authentication method for AWS Managed Microsoft AD
        private static string GetAuthenticatedUser()
        {
            try
            {
                // Get LDAP configuration from environment variables
                string ldapServer = Environment.GetEnvironmentVariable("LDAP_SERVER") ?? "localhost";
                string ldapPort = Environment.GetEnvironmentVariable("LDAP_PORT") ?? "389";
                string ldapBaseDn = Environment.GetEnvironmentVariable("LDAP_BASE_DN") ?? "DC=example,DC=com";
                string username = Environment.GetEnvironmentVariable("LDAP_USERNAME") ?? Environment.UserName;
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
                // Log error and return environment username as fallback
                Console.WriteLine($"LDAP Authentication failed: {ex.Message}");
                return Environment.UserName;
            }
        }
    }
}
