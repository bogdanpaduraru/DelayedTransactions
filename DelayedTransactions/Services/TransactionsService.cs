using DelayedTransactions.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelayedTransactions.Services
{
    public class TransactionsService : ITransactionsService
    {
        private readonly IMongoCollection<StartedTransaction> _startedTransactions;
        private readonly IMongoCollection<AvailableTransaction> _availableTransactions;

        public TransactionsService(IDatabaseSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);

            _startedTransactions = database.GetCollection<StartedTransaction>(dbSettings.StartedTransactionsCollectionName);
            _availableTransactions = database.GetCollection<AvailableTransaction>(dbSettings.AvailableTransactionsCollectionName);
        }

        public async Task<ActionResult<List<StartedTransaction>>> GetStartedTransactions() =>
            await _startedTransactions.Find(x => true).ToListAsync();

        public async Task<ActionResult<StartedTransaction>> GetStartedTransaction(Guid itemOfferId, Guid tokenOfferId) =>
            await _startedTransactions.Find(x => x.ItemOfferId == itemOfferId && x.TokenOfferId == tokenOfferId).FirstOrDefaultAsync();
        
        public async Task InsertStartedTransaction(StartedTransaction transaction) =>
            await _startedTransactions.InsertOneAsync(transaction);
        
        public async Task RemoveStartedTransaction(StartedTransaction transaction) =>
            await _startedTransactions.DeleteOneAsync(x => x.ItemOfferId == transaction.ItemOfferId && x.TokenOfferId == transaction.TokenOfferId);

        public async Task<ActionResult<List<AvailableTransaction>>> GetAvailableTransactions() =>
            await _availableTransactions.Find(x => true).ToListAsync();

        public async Task<ActionResult<AvailableTransaction>> GetAvailableTransaction(Guid itemOfferId, Guid tokenOfferId) =>
            await _availableTransactions.Find(x => x.ItemOfferId == itemOfferId && x.TokenOfferId == tokenOfferId).FirstOrDefaultAsync();

        public async Task InsertAvailableTransaction(AvailableTransaction availableTransaction) =>
            await _availableTransactions.InsertOneAsync(availableTransaction);
        public async Task RemoveAvailableTransaction(AvailableTransaction availableTransaction) =>
            await _availableTransactions.DeleteOneAsync(x => x.Id == availableTransaction.Id);
    }
}
