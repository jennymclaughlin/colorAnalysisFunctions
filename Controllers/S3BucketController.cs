using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using awsColorAnalysisFunctions.Services;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon;

namespace awsColorAnalysisFunctions.Controllers
{   
    [Produces("application/json")]
    [Route("api/S3Bucket")]
    public class S3BucketController : Controller
    {
        private readonly IS3Service _service;
        DynamoDBContext _context;
        public S3BucketController(IS3Service service, IAmazonDynamoDB context)
        {
            this._context = new DynamoDBContext(context);
            _service = service;
        }

        [HttpPost("login")]
        public async Task<userResponse> Get([FromBody]loginRequest request)
        {
            userResponse userResponse = new userResponse();
            var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJYYC5JKJ6B5ANFUQ", "sA6Y5pzFn+5XXmkzmCs43n30ujWCejqhNXNqvJob");
            var tableName = "users";
            var S3Client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);
            var tableResponse = await S3Client.ListTablesAsync();
            if (tableResponse.TableNames.Contains(tableName))
            {
                var conditions = new List<ScanCondition>();
                conditions.Add(new ScanCondition("userName", ScanOperator.Equal, request.username));
                conditions.Add(new ScanCondition("userPassword", ScanOperator.Equal, request.userpassword));
                var allDocs = await _context.ScanAsync<users>(conditions).GetRemainingAsync();
                user _user = new user();
                _user.userEmail = allDocs[0].userEmail;
                _user.userImageUrl = allDocs[0].userImageUrl;
                _user.userFirstName = allDocs[0].userFirstName;
                _user.userId = allDocs[0].userId;
                _user.userLastName = allDocs[0].userLastName;
                _user.userName = allDocs[0].userName;
                _user.userPassword = allDocs[0].userPassword;
                userResponse.user = _user;
                userResponse.code = (int)System.Net.HttpStatusCode.OK;
                userResponse.message = "Success";
                // Create our table if it doesn't exist

                //var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJYYC5JKJ6B5ANFUQ", "sA6Y5pzFn+5XXmkzmCs43n30ujWCejqhNXNqvJob");
                //var S3Client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);
                //var tableName = "users";
                DynamoDBContext context = new DynamoDBContext(S3Client);
                var tableResponse1 = await S3Client.ListTablesAsync();
                if (tableResponse1.TableNames.Contains(tableName))
                {
                    var conditions1 = new List<ScanCondition>();
                    conditions.Add(new ScanCondition("userId", ScanOperator.Equal, allDocs[0].userId));
                    var allDocs1 = await context.ScanAsync<users>(conditions1).GetRemainingAsync();
                    users _user1 = new users();
                    _user1.userEmail = allDocs1[0].userEmail;
                    _user1.userImageUrl = request.imageUrl;
                    _user1.userFirstName = allDocs1[0].userFirstName;
                    _user1.userId = allDocs1[0].userId;
                    _user1.userLastName = allDocs1[0].userLastName;
                    _user1.userName = allDocs1[0].userName;
                    _user1.userPassword = allDocs1[0].userPassword;
                    await context.SaveAsync<users>(_user1);
                }
            }

            return userResponse;
        }

        [HttpPost("Register")]
        public async Task<users> AddUser([FromBody] userRequest request)
        {
            try
            {
                var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJYYC5JKJ6B5ANFUQ", "sA6Y5pzFn+5XXmkzmCs43n30ujWCejqhNXNqvJob");
                var S3Client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);
                var tableName = "users";
                Table userTable = Table.LoadTable(S3Client, tableName);
                var _user = new Document();
                string userId = Guid.NewGuid().ToString("N").Substring(0, 12);
                _user["userId"] = userId;
                _user["userFirstName"] = request.userFirstName;
                _user["userLastName"] = request.userLastName;
                _user["userName"] = request.userName;
                _user["userEmail"] = request.userEmail;
                _user["userImageUrl"] = request.userImageUrl;
                _user["userPassword"] = request.userPassword;

                await   userTable.PutItemAsync(_user);
                return new users
                {
                    message = "User added successfully",
                    code =(int) System.Net.HttpStatusCode.OK
                };
            }
            catch(Exception ex)
            {
                return new users
                {
                    message = "Encountered error while adding a user "+ex.Message,
                    code = (int)System.Net.HttpStatusCode.BadRequest 
                };
            }
        }

        [HttpGet("user/{id}")]
        public async Task<userResponse> getUser([FromRoute] string id)
        {;
            userResponse resUser = new userResponse();
            try
            {
                var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJYYC5JKJ6B5ANFUQ", "sA6Y5pzFn+5XXmkzmCs43n30ujWCejqhNXNqvJob");
                var S3Client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);
                var tableName = "users";
                DynamoDBContext context = new DynamoDBContext(S3Client);
                var tableResponse = await S3Client.ListTablesAsync();
                if (tableResponse.TableNames.Contains(tableName))
                {
                    var conditions = new List<ScanCondition>();
                    conditions.Add(new ScanCondition("userId", ScanOperator.Equal, id));
                    var allDocs = await context.ScanAsync<users>(conditions).GetRemainingAsync();
                    user _user = new user();
                    _user.userEmail = allDocs[0].userEmail;
                    _user.userImageUrl = allDocs[0].userImageUrl;
                    _user.userFirstName = allDocs[0].userFirstName;
                    _user.userId = allDocs[0].userId;
                    _user.userLastName = allDocs[0].userLastName;
                    _user.userName = allDocs[0].userName;
                    _user.userPassword = allDocs[0].userPassword;
                    resUser.user = _user;
                    resUser.code = (int)System.Net.HttpStatusCode.OK;
                    resUser.message = "Success";
                }

            }
            catch(Exception ex)
            {
                resUser.code = (int)System.Net.HttpStatusCode.BadGateway;
                resUser.message = ex.Message;
            }

            return resUser;
        }
             
        [HttpPost]        
        public async Task<IActionResult> Post([FromBody] S3UploadRequest request)
        {
            string bucketName = "coloranalysisusers";
            var response = await _service.UploadFileAsync(bucketName, request.filebase64);
            return Ok(response);
        }
    }

    public class S3UploadRequest
    {
        public string filebase64 { get; set; }
        //public string userId { get; set; }
    }

    public class users
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string userFirstName { get; set; }
        public string userLastName { get; set; }
        public string userEmail { get; set; }
        public string userPassword { get; set; }
        public string userImageUrl { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }

    public class user
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string userFirstName { get; set; }
        public string userLastName { get; set; }
        public string userEmail { get; set; }
        public string userPassword { get; set; }
        public string userImageUrl { get; set; }
    }

    public class userResponse {

        public string message { get; set; }
        public int code { get; set; }
        public user user { get; set; }
    }

    public class loginRequest
    {
        public string username { get; set; }
        public string userpassword { get; set; }
        public string imageUrl { get; set; }
    }

    public class userRequest
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string userFirstName { get; set; }
        public string userLastName { get; set; }
        public string userEmail { get; set; }
        public string userPassword { get; set; }
        public string userImageUrl { get; set; }
    }
}
