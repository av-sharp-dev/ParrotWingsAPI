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

            //DB transactions preset
            if (_context.TransactionsTable.Find(1) == null)
            {
                _context.TransactionsTable.Add(new PWTransactions("preset", "Jeff Bezos", 100.00m, DateTime.Now));
                _context.TransactionsTable.Add(new PWTransactions("Bill Gates", "Vasily Lucky", 200.00m, DateTime.Now));
                _context.TransactionsTable.Add(new PWTransactions("Jeff Bezos", "Vasily Lucky", 200.00m, DateTime.Now));
                _context.SaveChanges();
            }
        }

        //Make a transaction
        [HttpPost]
        public JsonResult Pay(PWTransactions transaction)
        {
            if (transaction.SenderName == "" || transaction.RecipientName == "" || transaction.Id != 0 || transaction.Amount <= 0)
            {
                return new JsonResult(NotFound("Error: sender and recipient name are required. Payment amount should be larger than 0"));
            }else
            {
                var senderInDb = _context.UsersTable.Find(transaction.SenderName);
                var recipientInDb = _context.UsersTable.Find(transaction.RecipientName);
                
                if (senderInDb == null)
                    return new JsonResult(NotFound("Error: user (sender) " + transaction.SenderName + " not found"));
                if (recipientInDb == null)
                    return new JsonResult(NotFound("Error: user (recipient) " + transaction.RecipientName + " not found"));
                if (senderInDb.Balance < transaction.Amount)
                    return new JsonResult(NotFound("Error: sender balance too low"));

                senderInDb.Balance -= transaction.Amount;
                recipientInDb.Balance += transaction.Amount;
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
