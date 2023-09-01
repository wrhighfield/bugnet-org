namespace BugNet.Data;

public class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    // design support for everything in a different assembly
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
    {
        public IdentityDbContext CreateDbContext(string[] args)
        {
            var configuration =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(@Directory.GetCurrentDirectory() + "/../BugNet.Web/appsettings.json")
                    .Build();

            var builder = new DbContextOptionsBuilder<IdentityDbContext>();
            var connectionString = configuration.GetConnectionString(DataConstants.IdentityConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"Connection string '{DataConstants.IdentityConnectionStringName}' not found.");
            }
            builder.UseSqlServer(connectionString);
            return new IdentityDbContext(builder.Options);
        }
    }
}