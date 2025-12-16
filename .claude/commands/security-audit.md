---
description: Perform security audit on the codebase
allowed-tools: ["Read", "Glob", "Grep", "Bash"]
argument-hint: "[scope: full|dependencies|code]"
---

Perform a security audit on the codebase.

## Scope
$ARGUMENTS (default: full)

## Security Checklist

### 1. Dependency Vulnerabilities
```bash
dotnet list package --vulnerable
dotnet list package --vulnerable --include-transitive
dotnet list package --deprecated
```

### 2. Code Security Review

#### Injection Vulnerabilities
- **SQL Injection**: Search for raw SQL, string concatenation in queries
- **Command Injection**: Search for Process.Start, shell commands
- **Path Traversal**: Search for file operations with user input

#### Cryptography
- [ ] No deprecated algorithms (MD5, SHA1, DES)
- [ ] Proper random number generation (RandomNumberGenerator, not Random)
- [ ] Secure key storage (not hardcoded)

#### Authentication/Authorization
- [ ] Proper validation on all endpoints
- [ ] Secure session handling
- [ ] Role-based access control implemented correctly

#### Data Protection
- [ ] Sensitive data not logged
- [ ] PII handled appropriately
- [ ] Encryption at rest for sensitive data

#### Error Handling
- [ ] No stack traces exposed in production
- [ ] No sensitive data in error messages
- [ ] Generic error messages for users

### 3. Configuration Security
- [ ] Secrets in environment variables or secret manager
- [ ] Debug disabled in production configuration
- [ ] HTTPS enforced

### 4. Output Report
Provide findings with:
- **Severity**: Critical / High / Medium / Low
- **Location**: File and line number
- **Description**: What the issue is
- **Remediation**: How to fix it
