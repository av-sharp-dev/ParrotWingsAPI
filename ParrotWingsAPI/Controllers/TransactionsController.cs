using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParrotWingsAPI.Data;
using ParrotWingsAPI.Models;

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
            ApiContext.createDBPreset(_context); //creating debugging DB data preset
        }

        //Make a transaction
        [HttpPost]
        public JsonResult Pay(PWTransactions transaction)
        {
            if (transaction.SenderEmail == "" || transaction.RecipientEmail == "" || transaction.Id != 0 || transaction.Amount <= 0)
            {
                return new JsonResult(NotFound("Error: sender and recipient name are required. Payment amount should be larger than 0"));
            }else
            {
                var senderInDb = _context.UsersTable.Find(transaction.SenderEmail);
                var recipientInDb = _context.UsersTable.Find(transaction.RecipientEmail);

                if (senderInDb == null)
                    return new JsonResult(NotFound("Error: sender not found"));
                if (recipientInDb == null)
                    return new JsonResult(NotFound("Error: recipient not found"));
                if (senderInDb.Balance < transaction.Amount)
                    return new JsonResult(NotFound("Error: not enough PW to remit the transaction"));

                senderInDb.Balance -= transaction.Amount;
                recipientInDb.Balance += transaction.Amount;

                var lastId = _context.TransactionsTable.Last().Id;
                transaction.Id = ++lastId;

                _context.TransactionsTable.Add(transaction);
                _context.SaveChanges();

                return new JsonResult(Ok("Success: money sended"));
            }
        }

        //Get All Transactions
        [HttpGet]
        public JsonResult GetAllTransactions()
        {
            var transactionsInDb = _context.TransactionsTable.ToList();
            return new JsonResult(Ok(transactionsInDb));
        }
    }
}
