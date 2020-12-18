using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelayedTransactions.Models
{
    public class OfferTransaction
    {
        public Guid ItemId { get; set; }
        public int ItemDelta { get; set; }
    }

    public class StoreOffer
    {
        public Guid OfferId { get; set; }

        public string OfferName { get; set; }

        public List<OfferTransaction> Transactions { get; set; }
    }
}
