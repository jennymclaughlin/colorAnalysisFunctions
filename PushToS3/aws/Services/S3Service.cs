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

namespace aws.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _client;

        public S3Service (IAmazonS3 client)
        {
            _client = client; 
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


        public async Task UploadFileAsync(string bucketName)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_client);

                //Option1
                await fileTransferUtility.UploadAsync(FilePath, bucketName);

                //option2
                await fileTransferUtility.UploadAsync(FilePath, bucketName, UploadWithKeyName);

                //option 3
                using (var fileToupload = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    await fileTransferUtility.UploadAsync(fileToupload, bucketName, FileStreamUpload);
                }

                //Option 4
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    FilePath = FilePath,
                    StorageClass = S3StorageClass.Standard,
                    PartSize = 6291456, //6MB
                    Key= AdvancedUpload,
                    CannedACL = S3CannedACL.NoACL
                };

                fileTransferUtilityRequest.Metadata.Add("param1", "value1");
                fileTransferUtilityRequest.Metadata.Add("param2", "value2");

                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
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


        public async Task GetObjectFromS3Async(string bucketName)
        {
            const string KeyName = "ss";
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = KeyName
                };
                string responseBody;

                using (var response = await _client.GetObjectAsync(request))
                using (var responseStream = response.ResponseStream)
                using (var reader = new StreamReader(responseStream))
                {
                    var title = response.Metadata["x-amz-meta-title"];
                    var contentType = response.Metadata["Content-Type"];

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
