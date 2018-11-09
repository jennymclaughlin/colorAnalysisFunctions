using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace aws.Models
{
    public class S3Response
    {
        public HttpStatusCode Status { get; set; }

        public string Message { get; set; }
    }
}
