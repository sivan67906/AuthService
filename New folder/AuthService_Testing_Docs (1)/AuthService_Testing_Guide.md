
AuthService – Complete A→Z Testing and Usage Guide
Last Updated: 2025-11-05

This document provides a full testing workflow for your AuthService API including:
- Registration → Email Confirmation → Login
- 2FA Authentication Flow
- Refresh & Revoke Token
- Forgot/Reset/Change Password Flows
- Role-Based Authorization
- Address CRUD demonstrating CQRS Dual Database
- External OAuth Login Setup (Google/Microsoft)
- Health Check & Logging Verification

----------------------------------------------
1. Running The Application
----------------------------------------------

Option A - Docker Compose
----------------------------------------------
docker compose up --build
Navigate to: http://localhost:8080/swagger

Option B - Local Execution
----------------------------------------------
dotnet restore
dotnet build
dotnet run --project src/AuthService.Api


----------------------------------------------
2. Initial Database Migration (If Required)
----------------------------------------------

dotnet ef database update -c WriteDbContext -p src/AuthService.Infrastructure -s src/AuthService.Api
dotnet ef database update -c ReadDbContext -p src/AuthService.Infrastructure -s src/AuthService.Api


----------------------------------------------
3. Testing the Authentication Flow (A→Z)
----------------------------------------------

Step 1: Register
POST /api/auth/register
Body Example:
{
  "email": "alice@example.com",
  "password": "Passw0rd!",
  "firstName": "Alice",
  "lastName": "Doe"
}

Step 2: Confirm Email
Retrieve confirmation link from logs and open in browser.

Step 3: Login
POST /api/auth/login
Returns:
- accessToken (JWT)
- refreshToken

Step 4: Access a Protected Endpoint
GET /api/users/me
Header:
Authorization: Bearer <accessToken>


----------------------------------------------
4. Refresh Token Flow
----------------------------------------------

POST /api/auth/refresh
Body:
"<refreshToken>"


----------------------------------------------
5. Two-Factor Authentication
----------------------------------------------

Enable 2FA:
POST /api/auth/2fa/enable

Disable 2FA:
POST /api/auth/2fa/disable


----------------------------------------------
6. Password Flows
----------------------------------------------

Forgot Password:
POST /api/auth/password/forgot

Reset Password:
POST /api/auth/password/reset

Change Password (Authenticated):
POST /api/auth/password/change


----------------------------------------------
7. CQRS Dual Database Testing (Address CRUD)
----------------------------------------------

Create Address (SQL Server — WriteDbContext):
POST /api/users/addresses

List Addresses (PostgreSQL — ReadDbContext):
GET /api/users/addresses


----------------------------------------------
8. External OAuth Login Testing (Google / Microsoft)
----------------------------------------------

Update appsettings:
ExternalAuth.Google.ClientId & ClientSecret
ExternalAuth.Microsoft.ClientId & ClientSecret

Redirect URIs must match provider config:
http://localhost:8080/signin-google
http://localhost:8080/signin-microsoft


----------------------------------------------
9. Role-Based Authorization
----------------------------------------------

Admin User (Seeded):
Email: admin@demo.local
Password: Admin@12345


----------------------------------------------
10. Health Check and Logging
----------------------------------------------
GET /health
docker compose logs -f api


----------------------------------------------
End of Guide.
