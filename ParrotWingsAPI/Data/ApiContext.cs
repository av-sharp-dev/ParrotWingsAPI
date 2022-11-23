using Microsoft.EntityFrameworkCore;
using ParrotWingsAPI.Models;

namespace ParrotWingsAPI.Data
{
    public class ApiContext:DbContext
    {
        public DbSet<PWUsers> UserAccs { get; set; } //the user accounts table instance
        public DbSet<PWTransactionsHistory> TransactionsTable { get; set; } //the Transactions table instance
        public DbSet<PWRefreshTokens> UserTokens { get; set; } // the refresh tokens table instance
        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options)
        {
        
        }
    }
}
