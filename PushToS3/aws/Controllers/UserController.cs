using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB;
using MongoDB.Driver;
using System.Net;
using aws.Models;
namespace aws.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class UserController : Controller
    {

        private IMongoDatabase mongoDatabase;

        public IMongoDatabase GetMongoDatabase()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            return mongoClient.GetDatabase("aws_db");
        }


        [HttpGet]
        [Route("getUsers")]
        public async Task<usersResponse> getUsers()
        {            
            mongoDatabase = GetMongoDatabase();
            var _Users = mongoDatabase.GetCollection<users>("users_collection").Find(FilterDefinition<users>.Empty).ToList();
            return new usersResponse
            {
                message = "Users",
                StatusCode = HttpStatusCode.OK,
                Users = _Users
            };
        }


        [HttpPost]
        [Route("PostUser")]
        public async Task<IActionResult> PostUser([FromBody] users _user)
        {
            try
            {
                //Get the database connection  
                mongoDatabase = GetMongoDatabase();
                mongoDatabase.GetCollection<users>("users_collection").InsertOne(_user);
                return Ok();
            }
            catch(Exception ex)
            {
                return OK();
                throw;
            }
        }
    }
}