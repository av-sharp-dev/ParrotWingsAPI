using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParrotWingsAPI.Data;
using ParrotWingsAPI.Models;
using System.Security.Claims;

namespace ParrotWingsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ApiContext _context;
        public TransactionsController(ApiContext context)
        {
            _context = context;
        }

        [HttpPost, Authorize]
        public JsonResult Pay(PWTransactions transactionInput)
        {
            var senderInDb = getCurrentUserFromDB();

            if (senderInDb.IsLoggedIn == false)
                return new JsonResult(Unauthorized("Error: login required"));

            var recipientEmail = _context.UserAccs.Where(e => e.Name == transactionInput.RecipientName).FirstOrDefault();

            if (recipientEmail == null)
                return new JsonResult(BadRequest("Error: recipient " + transactionInput.RecipientName + " not found"));

            var recipientInDb = _context.UserAccs.Find(recipientEmail.Email);

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

            _context.TransactionsTable.Add(transaction);
            senderInDb.Balance -= transactionInput.Amount;
            recipientInDb.Balance += transactionInput.Amount;
            _context.SaveChanges();

            return new JsonResult(Ok("Success: PW sended"));
        }
        
        //provided the recipient list based on user input (first letters)
        [HttpGet, Authorize]
        public JsonResult GetRecipientsByQuerying(string firstLetters)
        {
            var userInDb = getCurrentUserFromDB();

            if (userInDb.IsLoggedIn == false)
                return new JsonResult(Unauthorized("Error: login required"));

            if (firstLetters == null)
                return new JsonResult(BadRequest("Error: 1 letter at least"));

            var recipients = _context.UserAccs.Where(r => r.Name.StartsWith(firstLetters))
                .Select(s => new { Name = s.Name})
                .ToList();

            if (recipients.Count == 0)
                return new JsonResult(NotFound("Users not found. Try upper/lower case"));

            return new JsonResult(Ok(recipients));
        }

        [HttpGet, Authorize]
        public JsonResult GetUserTransactionHistory()
        {
            var userInDb = getCurrentUserFromDB();

            if (userInDb.IsLoggedIn == false)
                return new JsonResult(Unauthorized("Error: login required"));

            var transactionHistory = _context.TransactionsTable.Where(w => w.SenderEmail == userInDb.Email)
                .Select(x => new { Date = x.TransactionDate, Recipient = x.RecipientName, Amount = x.Amount, Balance = x.ResultingBalance })
                .OrderByDescending(x => x.Date)
                .ToList();

            if (transactionHistory.Count == 0)
                return new JsonResult(NotFound("Transactions not found"));

            return new JsonResult(Ok(transactionHistory));
        }

        private PWUsers getCurrentUserFromDB()
        {
            var userIdentity = User.FindFirstValue(ClaimTypes.Email);
            var userInDb = _context.UserAccs.Find(userIdentity);
            return userInDb;
        }
    }
}
