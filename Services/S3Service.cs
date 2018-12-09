using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;
using System.Net;
using Amazon.S3.Transfer;
using System.IO;
using Amazon;

namespace awsColorAnalysisFunctions.Services
{
    public class S3Service : IS3Service
    {
        public async Task<S3UploadResponse> UploadFileAsync(string bucketName, string filebase64)
        {
            try
            {
                var credentials = new Amazon.Runtime.BasicAWSCredentials(Constants.access_key, Constants.secret_key);

                var S3Client = new AmazonS3Client(credentials, Constants.regionEndpoint);

                var fileTransferUtility = new TransferUtility(S3Client);
                var guid = Guid.NewGuid().ToString("N").Substring(0, 6);
                try
                {
                    byte[] bytes = Convert.FromBase64String(filebase64);

                    using (S3Client)
                    {
                        var request = new PutObjectRequest
                        {
                            BucketName = bucketName,
                            CannedACL = S3CannedACL.PublicRead,
                            Key = string.Format("userImages/{0}", guid + ".jpg")
                        };
                        using (var ms = new MemoryStream(bytes))
                        {
                            request.InputStream = ms;
                            await S3Client.PutObjectAsync(request);
                        }
                        GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest();
                        request1.BucketName = bucketName;
                        request1.Key = string.Format("userImages/{0}", guid + ".jpg");
                        request1.Expires = DateTime.Now.AddHours(2);
                        request1.Protocol = Protocol.HTTP;
                        string url = S3Client.GetPreSignedURL(request1);

                        Console.WriteLine(url);

                        return new S3UploadResponse
                        {
                            Message = "Success",
                            Status = HttpStatusCode.OK,
                            url = url.Split('?')[0].ToString()
                        };

                    }
                }
                catch (Exception ex)
                {
                    return new S3UploadResponse
                    {
                        Message = ex.Message,
                        Status = HttpStatusCode.BadRequest,
                        url = null
                    };
                }

            }
            catch (AmazonS3Exception ex)
            {
                return new S3UploadResponse
                {
                    Message = "Error encountered on server. Message : '{0}' when writting an object" + ex.Message,
                    Status = HttpStatusCode.BadRequest,
                    url = null
                };
            }
            catch (Exception ex)
            {
                return new S3UploadResponse
                {
                    Message = "Error encountered on server. Message : '{0}' when writting an object" + ex.Message,
                    Status = HttpStatusCode.BadRequest,
                    url = null
                };
            }
        }
    }
    public class S3UploadResponse
    {
        public string url { get; set; }

        public HttpStatusCode Status { get; set; }

        public string Message { get; set; }
    }
}
