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
        public static void createDBPreset(ApiContext context)
        {
            //DB users preset
            try
            {
                context.UsersTable.Add(new PWUsers("Bill Gates", "Bgates@gmail.com", "preset", 700.00m));
                context.UsersTable.Add(new PWUsers("Jeff Bezos", "Bezos@gmail.com", "preset", 900.00m));
                context.UsersTable.Add(new PWUsers("Vasily Lucky", "Vasya@gmail.com", "preset", 500.00m));
                context.TransactionsTable.Add(new PWTransactions("preset", "Jeff Bezos", 100.00m, DateTime.Now));
                context.TransactionsTable.Add(new PWTransactions("Bill Gates", "Vasily Lucky", 200.00m, DateTime.Now));
                context.TransactionsTable.Add(new PWTransactions("Jeff Bezos", "Vasily Lucky", 200.00m, DateTime.Now));
                context.SaveChanges();
            }
            catch
            {
                context.ChangeTracker.DetectChanges();
                context.ChangeTracker.Clear();
            }
        }
    }
}
