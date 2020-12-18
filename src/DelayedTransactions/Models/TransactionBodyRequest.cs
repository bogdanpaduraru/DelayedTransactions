using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelayedTransactions.Models
{
    public class TransactionBodyRequest
    {
        public string PlayerProfileId { get; set; }
        public Guid ItemOfferId { get; set; }
        public Guid TokenOfferId { get; set; }
    }
}
