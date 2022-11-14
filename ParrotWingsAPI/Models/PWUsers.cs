using System.ComponentModel.DataAnnotations;

namespace ParrotWingsAPI.Models
{
    public class PWUsers
    {
        [Key]
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public decimal Balance { get; set; }

        //parameterized constructor
        public PWUsers(string name, string email, string password, decimal balance = 0)
        {
            this.Name = name;
            this.Email = email;
            this.Password = password;
            this.Balance = balance;
        }
    }
}
