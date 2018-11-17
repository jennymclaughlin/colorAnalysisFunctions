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

namespace awsColorAnalysisFunctions.Models
{
    public class S3Response
    {
        public HttpStatusCode Status { get; set; }

        public string Message { get; set; }
    }
}
