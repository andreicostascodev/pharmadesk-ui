using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PharmaDesk.Data;

public class PharmaDeskDbContextFactory : IDesignTimeDbContextFactory<PharmaDeskDbContext>
{
    public PharmaDeskDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var serverVersion = MySqlServerVersion.Parse(configuration["App:MySqlServerVersion"] ?? "8.0.36");
        var options = new DbContextOptionsBuilder<PharmaDeskDbContext>()
            .UseMySql(configuration.GetConnectionString("DefaultConnection"), serverVersion)
            .Options;

        return new PharmaDeskDbContext(options);
    }
}
