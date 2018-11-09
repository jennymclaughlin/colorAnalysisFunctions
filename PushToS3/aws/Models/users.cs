using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Net;

namespace aws.Models
{
    public class users
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string userId { get; set; }
        [BsonElement]
        public string userFirstName { get; set; }
        [BsonElement]
        public string userEmail { get; set; }
        [BsonElement]
        public string userName { get; set; }
        [BsonElement]
        public string userPassword { get; set; }
        [BsonElement]
        public string userLastName { get; set; }
        [BsonElement]   
        public string userImageUrl { get; set; }
    }

    public class usersResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string message { get; set; }
        public  List<users> Users { get; set; }
    }
}
