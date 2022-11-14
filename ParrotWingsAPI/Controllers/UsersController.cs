using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParrotWingsAPI.Data;
using ParrotWingsAPI.Models;

namespace ParrotWingsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApiContext _context;

        public UsersController(ApiContext context)
        {
            _context= context;
            ApiContext.createDBPreset(_context); //creating debugging DB data preset
        }

        //Create User
        [HttpPost]
        public JsonResult CreateUser(PWUsers user)
        {   
            if (user.Name== "" || user.Email=="" || user.Password=="")
            {
                return new JsonResult(NotFound("Error: All fields are required"));
            }
            else
            {
                var userInDb = _context.UsersTable.Find(user.Email);
                if (userInDb != null)
                {
                    return new JsonResult(NotFound("Error: user alredy registered"));
                }
                else
                {
                    _context.UsersTable.Add(user);
                    _context.SaveChanges();
                    return new JsonResult(Ok("Success: user " + user.Name + " created"));
                }
            }
        }

        //Get User
        [HttpGet]
        public JsonResult GetUser(string userName)
        {
            var userInDb = _context.UsersTable.Find(userName);
            if (userInDb != null)
            {
                return new JsonResult(Ok("Success: user " + userName + " found"));
            }
            else
            {
                return new JsonResult(NotFound("Error: user " + userName + " not found"));
            }
        }

        //Delete User
        [HttpDelete]
        public JsonResult DeleteUser(string userName)
        {
            var userInDb = _context.UsersTable.Find(userName);
            if (userInDb != null)
            {
                _context.UsersTable.Remove(userInDb);
                _context.SaveChanges();
                return new JsonResult(Ok("Success: user " + userName + " have been deleted"));
            }
            else
            {
                return new JsonResult(NotFound("Error: user " + userName + " not found"));
            }
        }

        //Get All Users
        [HttpGet]
        public JsonResult GetAllUsers()
        {
            var usersInDb = _context.UsersTable.ToList();
            return new JsonResult(Ok(usersInDb));
        }
    }
}
