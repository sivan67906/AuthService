
# AuthService – Full End‑to‑End Testing Guide (A→Z)

Last Updated: 2025-11-05

This document explains how to test the complete AuthService project step‑by‑step, from user registration through authentication, 2FA, password reset, refresh tokens, external logins, and role-based authorization. It also explains how to test Aspire hosting, health checks, logging, and DB separation (CQRS with SQL Server & PostgreSQL).

---

## 1. Verify Environment Requirements

| Component | Minimum Version | Used For |
|---------|----------------|----------|
| .NET SDK | 9.0 (RTM) | Running API + AppHost |
| SQL Server | 2019+ or Docker image | WriteDbContext (Commands) |
| PostgreSQL | 13+ or Docker image | ReadDbContext (Queries) |
| Docker Desktop | Latest | Compose orchestration |
| REST Client | Postman / Swagger / VSCode .http | API testing |

---

## 2. Run the System

### Option A — Using Docker (Recommended)

```
docker compose up --build
```

API → http://localhost:8080  
Swagger UI → http://localhost:8080/swagger

### Option B — Run Locally

```
dotnet restore
dotnet build
dotnet run --project src/AuthService.Api
```

---

## 3. Database Migrations (Only required if first setup)

```
dotnet ef database update -c WriteDbContext -p src/AuthService.Infrastructure -s src/AuthService.Api
dotnet ef database update -c ReadDbContext -p src/AuthService.Infrastructure -s src/AuthService.Api
```

---

## 4. Test Flow A→Z

### 4.1 Register User

**POST** `/api/auth/register`

Example:
```json
{
  "email": "alice@example.com",
  "password": "Passw0rd!",
  "firstName": "Alice",
  "lastName": "Doe"
}
```

### 4.2 Confirm Email
Check logs → copy confirmation URL → open in browser.

### 4.3 Login

**POST** `/api/auth/login`

Returns `accessToken`, `refreshToken`.

### 4.4 Access a Protected Endpoint

**GET** `/api/users/me`  
Header → `Authorization: Bearer <accessToken>`

### 4.5 Test Refresh Token

**POST** `/api/auth/refresh`

Body:
```
"<refreshToken>"
```

### 4.6 Test 2FA

- Enable 2FA → `/api/auth/2fa/enable`
- Scan QR in Google Authenticator
- Login will now require `twoFactorCode`

### 4.7 Forgot & Reset Password

- `POST /api/auth/password/forgot`
- Receive token in logs
- `POST /api/auth/password/reset`

### 4.8 Test Address CQRS Split

| Action | DB used | Endpoint |
|--------|--------|----------|
| Add Address | SQL Server | POST `/api/users/addresses` |
| List Addresses | PostgreSQL | GET `/api/users/addresses` |

---

## 5. External Authentication (Google / Microsoft)

Set the provider keys in appsettings or environment variables.

Redirect URIs should be:
```
http://localhost:8080/signin-google
http://localhost:8080/signin-microsoft
```

---

## 6. Role-Based Authorization

Admin user seeded:
```
Email: admin@demo.local
Password: Admin@12345
```

Test secure endpoints:
- Try as normal user → expect 403 Forbidden
- Try as admin → allowed

---

## 7. Health Checks

```
GET /health
```

---

## 8. Logs (Serilog)
If running Docker:

```
docker compose logs -f api
```

---

## 9. Run Tests

```
dotnet test
```

---

End of Testing Guide.
