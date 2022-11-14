using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParrotWingsAPI.Data;
using ParrotWingsAPI.Models;
using System.Runtime.CompilerServices;

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

            //DB users preset
            try
            {
                _context.UsersTable.Add(new PWUsers("Bill Gates", "Bgates@gmail.com", "preset", 700.00m));
                _context.UsersTable.Add(new PWUsers("Jeff Bezos", "Bezos@gmail.com", "preset", 900.00m));
                _context.UsersTable.Add(new PWUsers("Vasily Lucky", "Vasya@gmail.com", "preset", 500.00m));
                _context.SaveChanges();
            }
            catch { }
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
                var userInDb = _context.UsersTable.Find(user.Name);
                if (userInDb != null)
                {
                    return new JsonResult(NotFound("Error: username " + user.Name + " alredy exists"));
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
