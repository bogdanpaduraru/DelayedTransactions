using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelayedTransactions.Models
{
    public class StoreItem
    {
        public Guid Id { get; set; }

        public string Item_Name { get; set; }

        public int Quantity { get; set; }

        public int Max_Quantity { get; set; }
    }
}
