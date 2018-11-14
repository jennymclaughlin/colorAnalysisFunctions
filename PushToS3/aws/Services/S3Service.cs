using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;
using aws.Models;
using System.Net;
using Amazon.S3.Transfer;
using System.IO;
using Amazon;
using MongoDB;
using MongoDB.Driver;
using aws.Models;
namespace aws.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _client;

        public S3Service ()
        {            
            
        }

        public async Task<S3Response> CreateBucketAsync(string bucketName)
        {
            try
            {
                if(await AmazonS3Util.DoesS3BucketExistAsync(_client, bucketName) == false)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName  = bucketName,
                        UseClientRegion = true,

                    };

                    var response = await _client.PutBucketAsync(putBucketRequest);

                    return new S3Response
                    {
                        Message = response.ResponseMetadata.RequestId,
                        Status = response.HttpStatusCode
                    };
                }
            }
            catch(AmazonS3Exception ex)
            {
                return new S3Response
                {
                    Message = ex.Message,
                    Status = ex.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new S3Response
                {
                    Message = ex.Message,
                    Status = HttpStatusCode.InternalServerError 
                };
            }
            return new S3Response
            {
                Message = "Something went wrong",
                Status = HttpStatusCode.InternalServerError
            };
        }

        private const string FilePath = "";
        private const string UploadWithKeyName = "UploadWithKeyName";
        private const string FileStreamUpload = "FileStreamUpload";
        private const string AdvancedUpload = "AdvancedUpload";


        public async Task UploadFileAsync(string bucketName, string filebase64)
        {
            try
            {
                var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJYYC5JKJ6B5ANFUQ", "sA6Y5pzFn+5XXmkzmCs43n30ujWCejqhNXNqvJob");

                var S3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast2);

                var fileTransferUtility = new TransferUtility(S3Client);
                var guid = Guid.NewGuid().ToString("N").Substring(0, 4);
                try
                {
                    byte[] bytes = Convert.FromBase64String(filebase64);

                    using (S3Client)
                    {
                        var request = new PutObjectRequest
                        {
                            BucketName = bucketName,
                            CannedACL = S3CannedACL.PublicRead,
                            Key = string.Format("bucketName/{0}", guid + ".jpg")
                        };
                        using (var ms = new MemoryStream(bytes))
                        {
                            request.InputStream = ms;
                          await  S3Client.PutObjectAsync(request);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("AWS Fail");
                }
                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest();
                request1.BucketName = bucketName;
                request1.Key = string.Format("bucketName/{0}", guid + ".jpg");
                request1.Expires = DateTime.Now.AddHours(1);
                request1.Protocol = Protocol.HTTP;
                string url = S3Client.GetPreSignedURL(request1);


                Console.WriteLine(url);

                //Option1
                //  await fileTransferUtility.UploadAsync(FilePath, bucketName);

                //option2
                //  await fileTransferUtility.UploadAsync(FilePath, bucketName, UploadWithKeyName);

                //option 3
                //using (var fileToupload = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                //{
                //    await fileTransferUtility.UploadAsync(fileToupload, bucketName, FileStreamUpload);
                //}

                //Option 4
                // var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                // {
                //   BucketName = bucketName,
                //   FilePath = FilePath,
                //    StorageClass = S3StorageClass.Standard,
                //   PartSize = 6291456, //6MB
                //    Key= AdvancedUpload,
                //    CannedACL = S3CannedACL.NoACL
                //  };

                // fileTransferUtilityRequest.Metadata.Add("param1", "value1");
                // fileTransferUtilityRequest.Metadata.Add("param2", "value2");


                // await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine("Error encountered on server. Message : '{0}' when writting an object", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error encountered on server. Message : '{0}' when writting an object", ex.Message);
            }
        }


        public async Task GetObjectFromS3Async(string bucketName, string KeyName)
        {
            //KeyName = "ss";
            try
            {
                var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJYYC5JKJ6B5ANFUQ", "sA6Y5pzFn+5XXmkzmCs43n30ujWCejqhNXNqvJob");

                var S3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast2);
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = KeyName
                };
                string responseBody;

                using (GetObjectResponse response = await S3Client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string title = response.Metadata["x-amz-meta-title"];
                    string contentType = response.Metadata["Content-Type"];

                    Console.WriteLine($"Object meta, Title : {title}");
                    Console.WriteLine($"Content type: {contentType}");

                    responseBody = reader.ReadToEnd();
                }

                var pathandFileName = $"C:\\S3Temp\\{KeyName}";
                var createText = responseBody;

                File.WriteAllText(pathandFileName, createText);

            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine("Error encountered on server. Message : '{0}' when writting an object", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error encountered on server. Message : '{0}' when writting an object", ex.Message);
            }
        }

    }
}
