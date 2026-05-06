# Cloud Configuration Guide

This document describes the environment variables and AWS resources required for the cloud-ready Bank Management System.

## Environment Variables

### Database Configuration
- `DATABASE_CONNECTION_STRING`: SQL Server connection string for RDS
  - Example: `Server=mydb.abc123.us-east-1.rds.amazonaws.com;Database=BankDB;User Id=admin;Password=secret;`
  - Used by: `BankDatabaseAccess/DatabaseConnection.cs`

### LDAP Authentication (AWS Managed Microsoft AD)
- `LDAP_SERVER`: LDAP server hostname
  - Example: `corp.example.com`
  - Used by: `EmployeeDashBoard.cs`, `Program.cs`

- `LDAP_PORT`: LDAP server port
  - Default: `389` (LDAP) or `636` (LDAPS)
  - Used by: `EmployeeDashBoard.cs`, `Program.cs`

- `LDAP_BASE_DN`: LDAP base distinguished name
  - Example: `DC=corp,DC=example,DC=com`
  - Used by: `EmployeeDashBoard.cs`, `Program.cs`

- `LDAP_USERNAME`: LDAP bind username
  - Example: `admin@corp.example.com`
  - Used by: `EmployeeDashBoard.cs`, `Program.cs`

- `LDAP_PASSWORD`: LDAP bind password
  - Store in AWS Secrets Manager and inject at runtime
  - Used by: `EmployeeDashBoard.cs`, `Program.cs`

### Amazon SQS Configuration
- `SQS_TRANSFER_AUDIT_QUEUE_URL`: SQS queue URL for transfer audit messages
  - Example: `https://sqs.us-east-1.amazonaws.com/123456789012/transfer-audit`
  - Used by: `CutomerDashboardForms/Transfer.cs`

- `SQS_CUSTOMER_INFO_QUEUE_URL`: SQS queue URL for customer info messages
  - Example: `https://sqs.us-east-1.amazonaws.com/123456789012/customer-info`
  - Used by: `EmployeeDashboardForms/CustomerInfo.cs`

### Amazon S3 Configuration
- `S3_CUSTOMER_LOGS_BUCKET`: S3 bucket name for customer access logs
  - Example: `bank-customer-logs`
  - Used by: `EmployeeDashboardForms/CustomerInfo.cs`

- `S3_DEPOSIT_CACHE_BUCKET`: S3 bucket name for deposit cache
  - Example: `bank-deposit-cache`
  - Used by: `EmployeeDashboardForms/Deposit.cs`

### AWS Systems Manager Parameter Store
- `SSM_PARAMETER_PATH`: Parameter Store path for application configuration
  - Example: `/BankApp/Configuration`
  - Used by: `CustomerDashBoard.cs`

### File System Configuration
- `AUDIT_LOG_PATH`: Path for audit log files (can be EFS mount)
  - Example: `/mnt/efs/logs/session.log`
  - Used by: `EmployeeDashBoard.cs`

## AWS Resources Required

### 1. Amazon RDS (SQL Server)
- Create an RDS SQL Server instance
- Configure security groups to allow access from ECS/EKS
- Optionally use RDS Proxy for connection pooling
- Store connection string in environment variable

### 2. AWS Managed Microsoft AD (Directory Service)
- Create a Managed Microsoft AD directory
- Configure LDAP access
- Migrate existing Active Directory users and groups
- Store LDAP credentials in AWS Secrets Manager

### 3. Amazon SQS Queues
Create the following SQS queues:
- `transfer-audit`: For transfer transaction audit messages
- `customer-info`: For customer information access messages

Configure:
- Message retention period: 4 days (default) or as needed
- Dead-letter queue for failed messages
- Encryption at rest using AWS KMS

### 4. Amazon S3 Buckets
Create the following S3 buckets:
- `bank-customer-logs`: For customer access logs
- `bank-deposit-cache`: For deposit transaction cache

Configure:
- Versioning enabled
- Encryption at rest using AWS KMS
- Lifecycle policies for log retention
- Access logging enabled

### 5. AWS Systems Manager Parameter Store
Create parameters:
- `/BankApp/Configuration`: Application configuration settings

Configure:
- Use SecureString type for sensitive data
- Enable encryption using AWS KMS
- Set appropriate IAM permissions

### 6. Amazon EFS (Optional)
- Create an EFS file system for audit logs
- Mount to ECS tasks or EKS pods
- Configure security groups for access

### 7. IAM Roles and Policies
Create IAM roles with policies for:
- RDS access
- SQS send/receive messages
- S3 read/write objects
- Systems Manager Parameter Store read
- Directory Service LDAP bind

## Deployment Checklist

1. ✅ Create all AWS resources (RDS, SQS, S3, Parameter Store, Managed AD)
2. ✅ Configure security groups and network access
3. ✅ Set up IAM roles and policies
4. ✅ Store sensitive credentials in AWS Secrets Manager
5. ✅ Configure environment variables in ECS task definition or EKS deployment
6. ✅ Test LDAP authentication against Managed AD
7. ✅ Verify SQS message delivery
8. ✅ Verify S3 file uploads
9. ✅ Test database connectivity through RDS
10. ✅ Monitor CloudWatch logs for errors

## Security Best Practices

1. **Never hardcode credentials** - Use AWS Secrets Manager or Parameter Store
2. **Use IAM roles** - Assign roles to ECS tasks or EKS pods instead of access keys
3. **Enable encryption** - Use KMS for S3, SQS, RDS, and Parameter Store
4. **Restrict network access** - Use security groups and NACLs
5. **Enable CloudTrail** - Audit all AWS API calls
6. **Use VPC endpoints** - For private connectivity to AWS services
7. **Rotate credentials** - Regularly rotate database passwords and LDAP credentials

## Monitoring and Logging

1. **CloudWatch Logs** - Application logs from ECS/EKS
2. **CloudWatch Metrics** - Custom metrics for business operations
3. **X-Ray** - Distributed tracing for performance analysis
4. **CloudWatch Alarms** - Alert on errors and performance issues
5. **S3 Access Logs** - Audit S3 bucket access
6. **VPC Flow Logs** - Network traffic analysis

## Cost Optimization

1. Use S3 lifecycle policies to transition old logs to Glacier
2. Configure SQS message retention based on actual needs
3. Use RDS reserved instances for predictable workloads
4. Enable S3 Intelligent-Tiering for automatic cost optimization
5. Monitor and right-size ECS tasks or EKS pods
