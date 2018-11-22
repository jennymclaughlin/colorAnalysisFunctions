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

        [HttpGet("login")]
        public async Task<userResponse> Get([FromQuery]string userName, [FromQuery] string userPassword)
        {
            userResponse userResponse = new userResponse();
            var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJYYC5JKJ6B5ANFUQ", "sA6Y5pzFn+5XXmkzmCs43n30ujWCejqhNXNqvJob");
            var tableName = "users";
            var S3Client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);
            var tableResponse = await S3Client.ListTablesAsync();
            if (tableResponse.TableNames.Contains(tableName))
            {
                var conditions = new List<ScanCondition>();
                conditions.Add(new ScanCondition("userName", ScanOperator.Equal, userName));
                conditions.Add(new ScanCondition("userPassword", ScanOperator.Equal, userPassword));
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
           
            var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJYYC5JKJ6B5ANFUQ", "sA6Y5pzFn+5XXmkzmCs43n30ujWCejqhNXNqvJob");
            var S3Client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);
            var tableName = "users";
            DynamoDBContext context = new DynamoDBContext(S3Client);
            var tableResponse = await S3Client.ListTablesAsync();
            if (tableResponse.TableNames.Contains(tableName))
            {
                var conditions = new List<ScanCondition>();
                conditions.Add(new ScanCondition("userId", ScanOperator.Equal, request.userId));
                var allDocs = await context.ScanAsync<users>(conditions).GetRemainingAsync();
                users _user = new users();
                _user.userEmail = allDocs[0].userEmail;
                _user.userImageUrl = response.url;
                _user.userFirstName = allDocs[0].userFirstName;
                _user.userId = allDocs[0].userId;
                _user.userLastName = allDocs[0].userLastName;
                _user.userName = allDocs[0].userName;
                _user.userPassword = allDocs[0].userPassword;
                await context.SaveAsync<users>(_user);
            }

            return Ok(response);
        }
    }

    public class S3UploadRequest
    {
        public string filebase64 { get; set; }
        public string userId { get; set; }
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
