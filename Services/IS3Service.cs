using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using awsColorAnalysisFunctions.Models;

namespace awsColorAnalysisFunctions.Services
{
    public interface IS3Service
    {
      //  Task<S3Response> CreateBucketAsync(string bucketName);

        Task<S3UploadResponse> UploadFileAsync(string bucketName, string base64file);

       // Task GetObjectFromS3Async(string bucketName, string KeyName);
    }
}
