using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.Models
{

    public class UserPreferences
    {
        [BsonElement("newsletter")]
        public bool Newsletter { get; set; }

        [BsonElement("notifications")]
        public string Notifications { get; set; }
    }
}
