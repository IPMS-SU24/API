using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace IPMS.DataAccess
{
    public class IPMSDbContextFactory : IDesignTimeDbContextFactory<IPMSDbContext>
    {
        public IPMSDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IPMSDbContext>();
            optionsBuilder.UseNpgsql(GetConnectionString());

            return new IPMSDbContext(optionsBuilder.Options);
        }
        private string GetConnectionString()
        {

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            return configuration.GetConnectionString("IPMS");
        }
    }

}
