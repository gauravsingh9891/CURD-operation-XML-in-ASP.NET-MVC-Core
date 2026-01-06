using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace MVCDHProject.Models
{
    public class MVCCoreDbContext : DbContext
    {
        //passing connectionstring dynamically
        public MVCCoreDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Customer> Customers { get; set; }
    }
}
