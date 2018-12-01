using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon;

namespace awsColorAnalysisFunctions.Controllers
{
    [Route("api/DynmoDb")]
    [ApiController]
    public class DynmoDbController : ControllerBase
    {
        DynamoDBContext context;
        private static RegionEndpoint regionEndpoint = RegionEndpoint.USEast2;
        private static string access_key = "AKIAJYYC5JKJ6B5ANFUQ";
        private static string secret_key = "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";
        public DynmoDbController(IAmazonDynamoDB context)
        {
            this.context = new DynamoDBContext(context);
        }

        [HttpGet]
        public async Task<users> Get([FromQuery]string userName, [FromQuery] string userPassword)
        {
            users userResponse = new users();
            var credentials = new Amazon.Runtime.BasicAWSCredentials(access_key, secret_key);
            var tableName = "users";
            var S3Client = new AmazonDynamoDBClient(credentials, regionEndpoint);
            var tableResponse = await S3Client.ListTablesAsync();
            if (tableResponse.TableNames.Contains(tableName))
            {
                var conditions = new List<ScanCondition>();
                conditions.Add(new ScanCondition("userName", ScanOperator.Equal, userName));
                conditions.Add(new ScanCondition("userPassword", ScanOperator.Equal, userPassword));
                var allDocs = await context.ScanAsync<users>(conditions).GetRemainingAsync();
                userResponse = allDocs[0];
                // Create our table if it doesn't exist
            }

            return userResponse;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]users _user)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(access_key, secret_key);
            var S3Client = new AmazonDynamoDBClient(credentials, regionEndpoint);
            var tableName = "users";
            DynamoDBContext context = new DynamoDBContext(S3Client);
            var tableResponse = await S3Client.ListTablesAsync();
            if (tableResponse.TableNames.Contains(tableName))
            {
                var conditions = new List<ScanCondition>();
                conditions.Add(new ScanCondition("userId", ScanOperator.Equal, _user.userId));
                var allDocs = await context.ScanAsync<users>(conditions).GetRemainingAsync();
                allDocs[0].userEmail = _user.userEmail;
                allDocs[0].userImageUrl = _user.userImageUrl;
                allDocs[0].userFirstName = _user.userFirstName;
                allDocs[0].userId = _user.userId;
                allDocs[0].userLastName = _user.userLastName;
                allDocs[0].userName = _user.userName;
                allDocs[0].userPassword = _user.userPassword;

                await context.SaveAsync<users>(allDocs[0]);
            }
            return Ok();
        }

    }  
 
}