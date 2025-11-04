# 🚀 Quick Start Guide

Get the AuthService API running in 5 minutes!

## Prerequisites

- .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0
- Docker Desktop (recommended): https://www.docker.com/products/docker-desktop

## Option 1: Quick Start with Docker (Recommended)

### Step 1: Start Databases
```bash
cd AuthService
docker-compose up -d sqlserver postgres
```

Wait 30 seconds for databases to initialize.

### Step 2: Apply Migrations
```bash
cd AuthService.API

# SQL Server migrations
dotnet ef database update --context CommandDbContext --project ../AuthService.Infrastructure

# PostgreSQL migrations  
dotnet ef database update --context QueryDbContext --project ../AuthService.Infrastructure
```

### Step 3: Run the API
```bash
dotnet run
```

### Step 4: Test the API
Open your browser to: https://localhost:7001/swagger

## Option 2: Without Docker

### Step 1: Install Databases
- Install SQL Server Express: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
- Install PostgreSQL: https://www.postgresql.org/download/

### Step 2: Create Databases
```sql
-- SQL Server
CREATE DATABASE AuthService_Command;

-- PostgreSQL
CREATE DATABASE "AuthService_Query";
```

### Step 3: Update Connection Strings
Edit `AuthService.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "CommandDatabase": "Server=localhost;Database=AuthService_Command;Integrated Security=True;TrustServerCertificate=True",
    "QueryDatabase": "Host=localhost;Port=5432;Database=AuthService_Query;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### Step 4: Apply Migrations & Run
```bash
cd AuthService.API

# Apply migrations
dotnet ef database update --context CommandDbContext --project ../AuthService.Infrastructure
dotnet ef database update --context QueryDbContext --project ../AuthService.Infrastructure

# Run API
dotnet run
```

## 🧪 Test the API

### 1. Register a User
```bash
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "userName": "testuser",
    "password": "Test@1234",
    "confirmPassword": "Test@1234",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### 2. Login
```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "test@example.com",
    "password": "Test@1234"
  }'
```

### 3. Use Swagger UI
Navigate to https://localhost:7001/swagger for an interactive API explorer.

## 📊 Verify Databases

### SQL Server
```bash
# Using Docker
docker exec -it authservice-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" \
  -Q "SELECT name FROM sys.databases"
```

### PostgreSQL
```bash
# Using Docker
docker exec -it authservice-postgres psql -U postgres -l
```

### pgAdmin
Open http://localhost:8080
- Email: admin@authservice.com
- Password: admin123

## 🔍 Troubleshooting

### Port Already in Use
```bash
# Change ports in appsettings.json or docker-compose.yml
# Or stop conflicting services:
docker-compose down
```

### Database Connection Failed
```bash
# Check if databases are running
docker-compose ps

# View logs
docker-compose logs sqlserver
docker-compose logs postgres
```

### Migration Errors
```bash
# Reset and recreate migrations
cd AuthService.API
rm -rf ../AuthService.Infrastructure/Persistence/Migrations
dotnet ef migrations add InitialCreate --context CommandDbContext --project ../AuthService.Infrastructure
dotnet ef migrations add InitialCreate --context QueryDbContext --project ../AuthService.Infrastructure
dotnet ef database update --context CommandDbContext --project ../AuthService.Infrastructure
dotnet ef database update --context QueryDbContext --project ../AuthService.Infrastructure
```

## 🎯 Next Steps

1. ✅ API is running
2. 📖 Read [README.md](README.md) for full documentation
3. 🔧 Review [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) for known issues
4. 🧪 Run tests: `cd AuthService.Tests && dotnet test`
5. 🔐 Configure OAuth providers in appsettings.json
6. 📧 Configure email/SMS services

## 📝 Default Roles Seeded

The database comes pre-seeded with:
- **Admin** - Full access
- **Customer** - Standard user
- **Vendor** - Product management access

## 🔐 Sample Credentials

Since you're just starting, you'll need to register users via the API.
No default users are created for security reasons.

## 📞 Need Help?

Check these files:
- [README.md](README.md) - Complete documentation
- [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) - Architecture overview
- Logs in `logs/authservice-*.txt`

---

**Ready to build amazing apps! 🎉**
