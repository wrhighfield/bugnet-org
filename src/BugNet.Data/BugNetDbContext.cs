using BugNet.Data.Entities;

namespace BugNet.Data
{
    public class BugNetDbContext : IdentityDbContext<BugNetUser, BugNetRole, Guid>
    {
        public BugNetDbContext(DbContextOptions<BugNetDbContext> options)
            : base(options)
        {
        }

        public DbSet<Log> Logs { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Ignore<Log>();
        }

        // design support for everything in a different assembly
        public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BugNetDbContext>
        {
            public BugNetDbContext CreateDbContext(string[] args)
            {
                var configuration =
                    new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(@Directory.GetCurrentDirectory() + "/../BugNet.Web/appsettings.json")
                        .Build();

                var builder = new DbContextOptionsBuilder<BugNetDbContext>();
                var connectionString = configuration.GetConnectionString(DataConstants.BugNetConnectionStringName);
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException($"Connection string '{DataConstants.BugNetConnectionStringName}' not found.");
                }
                builder.UseSqlServer(connectionString);
                return new BugNetDbContext(builder.Options);
            }
        }
    }
}