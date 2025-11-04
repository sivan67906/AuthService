# AuthService - Enterprise Authentication & Authorization Microservice

A production-ready .NET 9 authentication and authorization microservice built with **Vertical Slice Architecture**, **CQRS pattern**, and **dual database** support (SQL Server for commands, PostgreSQL for queries).

## 🏗️ Architecture

### Vertical Slice Architecture
The project is organized by features rather than layers, making it easier to understand, maintain, and scale. Each feature contains its own:
- Commands/Queries
- Handlers
- Validators
- DTOs

### CQRS Pattern with Dual Database
- **Commands** (Write Operations): SQL Server - Optimized for transactional consistency
- **Queries** (Read Operations): PostgreSQL - Optimized for read performance and scalability

### Technology Stack
- **.NET 9** - Latest framework with modern C# features
- **ASP.NET Core Identity** - User management and authentication
- **Entity Framework Core 9** - Dual ORM support (SQL Server + PostgreSQL)
- **MediatR** - CQRS implementation
- **FluentValidation** - Request validation
- **Mapster** - Object mapping
- **Serilog** - Structured logging
- **xUnit** - Unit testing framework
- **Swashbuckle** - API documentation

## 📁 Project Structure

```
AuthService/
├── AuthService.API/                    # Presentation Layer
│   ├── Controllers/                    # API Controllers
│   ├── Middleware/                     # Custom middleware
│   └── Program.cs                      # Application entry point
│
├── AuthService.Application/            # Application Layer
│   ├── Features/                       # Vertical slices by feature
│   │   ├── Authentication/
│   │   │   ├── Commands/              # Write operations
│   │   │   └── Queries/               # Read operations
│   │   ├── Profile/
│   │   ├── Address/
│   │   ├── TwoFactor/
│   │   └── ExternalAuth/
│   └── Common/
│       ├── Behaviors/                  # MediatR pipeline behaviors
│       ├── Mappings/                   # Mapster configurations
│       └── Validators/                 # FluentValidation rules
│
├── AuthService.Domain/                 # Domain Layer
│   ├── Entities/                       # Domain entities
│   ├── Enums/                         # Domain enumerations
│   ├── Common/                        # Base classes & Result pattern
│   └── Interfaces/                    # Repository & service interfaces
│
├── AuthService.Infrastructure/         # Infrastructure Layer
│   ├── Persistence/
│   │   ├── Contexts/                  # DbContexts (Command & Query)
│   │   ├── Configurations/            # EF Core configurations
│   │   ├── Repositories/              # Repository implementations
│   │   └── Migrations/                # Database migrations
│   ├── Services/                      # Service implementations
│   │   ├── JwtService.cs
│   │   ├── EmailService.cs
│   │   ├── SmsService.cs
│   │   ├── TwoFactorService.cs
│   │   └── ExternalAuthService.cs
│   └── Identity/                      # Identity configuration
│
└── AuthService.Tests/                  # Test Project
    ├── Features/                       # Feature tests
    ├── Integration/                    # Integration tests
    └── Unit/                          # Unit tests
```

## 🚀 Features

### Core Authentication
- ✅ User Registration with Email Confirmation
- ✅ Login with JWT Access & Refresh Tokens
- ✅ Token Refresh & Revocation
- ✅ Password Reset Flow
- ✅ Change Password
- ✅ Email Verification

### Two-Factor Authentication
- ✅ Email-based 2FA
- ✅ SMS-based 2FA
- ✅ Authenticator App (TOTP)
- ✅ QR Code Generation for Authenticator Setup

### External Authentication
- ✅ Google OAuth Integration
- ✅ Microsoft Account Integration
- ✅ Extensible for other providers

### Authorization Types
1. **Simple Authorization**
   - `[Authorize]` attribute for authenticated users

2. **Role-Based Authorization**
   - Admin, Customer, Vendor roles
   - `[Authorize(Roles = "Admin")]`

3. **Claims-Based Authorization**
   - Email verified claims
   - Two-factor enabled claims
   - `[Authorize(Policy = "EmailVerified")]`

4. **Policy-Based Authorization**
   - Minimum age requirements
   - Active user validation
   - Custom policy handlers

### User Management
- ✅ User Profile Management
- ✅ User Address CRUD Operations
- ✅ Account Status Management
- ✅ Session Management

## 🔧 Setup Instructions

### Prerequisites
- .NET 9 SDK
- SQL Server (for command database)
- PostgreSQL (for query database)
- Visual Studio 2022 or JetBrains Rider

### Database Setup

#### SQL Server (Command Database)
```sql
CREATE DATABASE AuthService_Command;
```

#### PostgreSQL (Query Database)
```sql
CREATE DATABASE "AuthService_Query";
```

### Configuration

Update `appsettings.json` with your connection strings:

```json
{
  "ConnectionStrings": {
    "CommandDatabase": "Server=localhost;Database=AuthService_Command;User Id=sa;Password=YourPassword;TrustServerCertificate=True",
    "QueryDatabase": "Host=localhost;Port=5432;Database=AuthService_Query;Username=postgres;Password=YourPassword"
  },
  
  "JwtSettings": {
    "SecretKey": "YOUR-SECRET-KEY-AT-LEAST-256-BITS",
    "Issuer": "AuthService",
    "Audience": "AuthServiceUsers",
    "AccessTokenExpirationMinutes": 60
  },
  
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Microsoft": {
      "ClientId": "YOUR_MICROSOFT_CLIENT_ID",
      "ClientSecret": "YOUR_MICROSOFT_CLIENT_SECRET"
    }
  }
}
```

### Run Migrations

```bash
# Navigate to API project
cd AuthService.API

# Apply migrations to Command Database (SQL Server)
dotnet ef migrations add InitialCreate --context CommandDbContext --project ../AuthService.Infrastructure
dotnet ef database update --context CommandDbContext --project ../AuthService.Infrastructure

# Apply migrations to Query Database (PostgreSQL)
dotnet ef migrations add InitialCreate --context QueryDbContext --project ../AuthService.Infrastructure
dotnet ef database update --context QueryDbContext --project ../AuthService.Infrastructure
```

### Run the Application

```bash
cd AuthService.API
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:7001/swagger`

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh-token` - Refresh access token
- `POST /api/auth/revoke-token` - Revoke refresh token (logout)
- `POST /api/auth/verify-email` - Verify email address
- `POST /api/auth/resend-confirmation` - Resend confirmation email

### Password Management
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token
- `POST /api/auth/change-password` - Change password (authenticated)

### Two-Factor Authentication
- `POST /api/auth/enable-2fa` - Enable 2FA
- `POST /api/auth/verify-2fa` - Verify 2FA code during login

### External Authentication
- `POST /api/auth/external/google` - Google OAuth login
- `POST /api/auth/external/microsoft` - Microsoft Account login

### Health Checks
- `GET /health` - Overall health status
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe

## 🧪 Testing

### Run Unit Tests
```bash
cd AuthService.Tests
dotnet test
```

### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 🔐 Security Features

- **Password Hashing**: Using ASP.NET Core Identity's secure password hasher
- **JWT Tokens**: HS256 algorithm with configurable expiration
- **Refresh Tokens**: Secure token rotation with IP tracking
- **Account Lockout**: Configurable failed attempt limits
- **Email Confirmation**: Required before account activation
- **Two-Factor Authentication**: Multiple methods supported
- **HTTPS**: Enforced in production
- **CORS**: Configurable origin restrictions

## 📊 Database Schema

### Command Database (SQL Server)
- Users
- Roles
- UserRoles
- UserClaims
- RoleClaims
- Addresses
- RefreshTokens
- ExternalLogins
- UserSessions

### Query Database (PostgreSQL)
Mirrored schema optimized for read operations with indexes

## 🛠️ Development Guidelines

### Adding a New Feature

1. **Create Feature Folder**
   ```
   AuthService.Application/Features/MyFeature/
   ├── Commands/
   │   ├── MyCommand.cs
   │   └── MyCommandHandler.cs
   └── Queries/
       ├── MyQuery.cs
       └── MyQueryHandler.cs
   ```

2. **Implement Command/Query**
   ```csharp
   public sealed record MyCommand : IRequest<Result<MyResponse>>;
   
   public sealed class MyCommandHandler : IRequestHandler<MyCommand, Result<MyResponse>>
   {
       // Implementation
   }
   ```

3. **Add Validation**
   ```csharp
   public sealed class MyCommandValidator : AbstractValidator<MyCommand>
   {
       public MyCommandValidator()
       {
           // Validation rules
       }
   }
   ```

4. **Add Controller Endpoint**
   ```csharp
   [HttpPost("my-endpoint")]
   public async Task<IActionResult> MyEndpoint([FromBody] MyCommand command)
   {
       var result = await _mediator.Send(command);
       return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
   }
   ```

## 📈 Performance Considerations

- **Query Database**: Read-only PostgreSQL for fast queries
- **Command Database**: SQL Server with transaction support
- **Caching**: Can be added using Redis or In-Memory cache
- **Connection Pooling**: Enabled by default in EF Core
- **Async/Await**: Used throughout for non-blocking operations

## 🐳 Docker Support

Create `docker-compose.yml` for complete environment:

```yaml
version: '3.8'
services:
  authservice-api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on:
      - sqlserver
      - postgres

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"

  postgres:
    image: postgres:16
    environment:
      POSTGRES_PASSWORD: postgres123
      POSTGRES_DB: AuthService_Query
    ports:
      - "5432:5432"
```

## 📝 License

This project is licensed under the MIT License.

## 👥 Contributing

Contributions are welcome! Please follow the existing code style and architecture patterns.

## 📞 Support

For issues or questions, please create an issue on the GitHub repository.

---

**Built with ❤️ using .NET 9 and Vertical Slice Architecture**
