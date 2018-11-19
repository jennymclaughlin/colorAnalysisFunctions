using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using awsColorAnalysisFunctions.Services;

namespace awsColorAnalysisFunctions.Controllers
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
    }
}
