using Microsoft.EntityFrameworkCore;
using ParrotWingsAPI.Models;

namespace ParrotWingsAPI.Data
{
    public class ApiContext:DbContext
    {
        public DbSet<PWUsers> UsersTable { get; set; } //the Users table instance
        public DbSet<PWTransactions> TransactionsTable { get; set; } //the Transactions table instance
        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options)
        {
        
        }
    }
}
