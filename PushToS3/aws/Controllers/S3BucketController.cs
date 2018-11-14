using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using aws.Services;
namespace aws.Controllers
{
    [Produces("application/json")]
    [Route("api/S3Bucket")]
    public class S3BucketController : Controller
    {
        private readonly IS3Service _service;

        public S3BucketController(IS3Service service)
        {
            _service = service;
        }

        [HttpPost("CreateBucket/{bucketName}")]
        public async Task<IActionResult> CreateBucket([FromRoute] string bucketName)
        {
            var response = await _service.CreateBucketAsync(bucketName);
            return Ok(response);
        }


        [HttpPost]
        [Route("AddFile")]
        public async Task<IActionResult> AddFile([FromBody] string filebase64)
        {
                string bucketName = "coloranalysisusers";
            await _service.UploadFileAsync(bucketName, filebase64);

            return Ok();
        }

        [HttpGet]
        [Route("GetFile/{bucketName}")]
        public async Task<IActionResult> GetObjectFromS3Async([FromRoute] string bucketName)
        {
          //  await _service.GetObjectFromS3Async(bucketName);
            return Ok();
        }

    }
}