using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParrotWingsAPI.Data;
using ParrotWingsAPI.Models;
using System.Security.Claims;

namespace ParrotWingsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ApiContext _context;
        public TransactionsController(ApiContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<JsonResult> Pay(PWTransactions transactionInput)
        {
            var senderInDb = await getCurrentUserFromDB();

            if (senderInDb.IsLoggedIn == false)
                return new JsonResult(Unauthorized("Error: login required"));

            var recipientEmail = await _context.UserAccs.Where(e => e.Name == transactionInput.RecipientName).FirstOrDefaultAsync();

            if (recipientEmail == null)
                return new JsonResult(BadRequest("Error: recipient " + transactionInput.RecipientName + " not found"));

            var recipientInDb = await _context.UserAccs.FindAsync(recipientEmail.Email);

            if (senderInDb.Email == recipientInDb.Email)
                return new JsonResult(BadRequest("Error: making yourself happy by transfering money to yourself is not allowed"));

            if (transactionInput.Amount <= 0)
                return new JsonResult(BadRequest("Error: PW amount should be greater than 0"));

            if (senderInDb.Balance < transactionInput.Amount)
                return new JsonResult(BadRequest("Error:  not enough PW to remit the transaction"));

            PWTransactionsHistory transaction = new PWTransactionsHistory()
            {
                SenderEmail = senderInDb.Email,
                RecipientEmail = recipientInDb.Email,
                RecipientName = recipientInDb.Name,
                ResultingBalance = senderInDb.Balance - transactionInput.Amount,
                Amount = transactionInput.Amount,
                TransactionDate = DateTime.UtcNow
            };

            await _context.TransactionsTable.AddAsync(transaction);
            senderInDb.Balance -= transactionInput.Amount;
            recipientInDb.Balance += transactionInput.Amount;
            await _context.SaveChangesAsync();

            return new JsonResult(Ok("Success: PW sended"));
        }
        
        //provided the recipient list based on user input (first letters)
        [HttpGet]
        public async Task <JsonResult> GetRecipientsByQueryingAsync(string firstLetters)
        {
            var userInDb = await getCurrentUserFromDB();

            if (userInDb.IsLoggedIn == false)
                return new JsonResult(Unauthorized("Error: login required"));

            if (firstLetters == null)
                return new JsonResult(BadRequest("Error: 1 letter at least"));

            var recipients = await _context.UserAccs.Where(r => r.Name.StartsWith(firstLetters))
                .Select(s => new { Name = s.Name})
                .ToListAsync();

            if (recipients.Count == 0)
                return new JsonResult(NotFound("Users not found. Try upper/lower case"));

            return new JsonResult(Ok(recipients));
        }

        [HttpGet]
        public async Task<JsonResult> GetUserTransactionHistoryAsync()
        {
            var userInDb = await getCurrentUserFromDB();

            if (userInDb.IsLoggedIn == false)
                return new JsonResult(Unauthorized("Error: login required"));

            var transactionHistory = await _context.TransactionsTable.Where(w => w.SenderEmail == userInDb.Email)
                .Select(x => new { Date = x.TransactionDate, Recipient = x.RecipientName, Amount = x.Amount, Balance = x.ResultingBalance })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            if (transactionHistory.Count == 0)
                return new JsonResult(NotFound("Transactions not found"));

            return new JsonResult(Ok(transactionHistory));
        }

        private async Task<PWUsers> getCurrentUserFromDB()
        {
            var userIdentity = User.FindFirstValue(ClaimTypes.Email);
            var userInDb = await _context.UserAccs.FindAsync(userIdentity);
            return userInDb;
        }
    }
}
