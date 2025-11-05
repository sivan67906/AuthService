using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Persistence;

public sealed class WriteDbContextFactory : IDesignTimeDbContextFactory<WriteDbContext>
{
    public WriteDbContext CreateDbContext(string[] args)
    {
        // Try to read from appsettings.json if present; otherwise fall back to sensible default.
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = config.GetConnectionString("SqlServer")
                 ?? "Server=localhost,1433;Database=AuthServiceWrite;User Id=sa;Password=YourStrong(!)Password;TrustServerCertificate=True;";

        var opts = new DbContextOptionsBuilder<WriteDbContext>()
            .UseSqlServer(cs)
            .Options;

        return new WriteDbContext(opts);
    }
}

public sealed class ReadDbContextFactory : IDesignTimeDbContextFactory<ReadDbContext>
{
    public ReadDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = config.GetConnectionString("Postgres")
                 ?? "Host=localhost;Port=5432;Database=authservice_read;Username=postgres;Password=postgres";

        var opts = new DbContextOptionsBuilder<ReadDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new ReadDbContext(opts);
    }
}
