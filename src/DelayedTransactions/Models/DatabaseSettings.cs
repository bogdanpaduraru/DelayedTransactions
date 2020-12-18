using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelayedTransactions.Models
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string StartedTransactionsCollectionName { get; set; }
        public string AvailableTransactionsCollectionName { get; set; }
    }

    public interface IDatabaseSettings
    {
        string CollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string StartedTransactionsCollectionName { get; set; }
        string AvailableTransactionsCollectionName { get; set; }
    }
}
