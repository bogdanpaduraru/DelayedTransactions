using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelayedTransactions.Models
{
    public class AvailableTransaction
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid TokenOfferId { get; set; }
        public Guid ItemOfferId { get; set; }
        public int SecondsToComplete { get; set; }
    }

    public class StartedTransaction
    {
        [BsonId]
        public Guid Id { get; set; }
        
        [BsonRepresentation(BsonType.String)]
        public Guid TokenOfferId { get; set; }
        
        [BsonRepresentation(BsonType.String)]
        public Guid ItemOfferId { get; set; }
        
        public DateTime EndTime { get; set; }
    }
}
