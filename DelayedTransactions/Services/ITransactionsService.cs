using DelayedTransactions.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelayedTransactions.Services
{
    public interface ITransactionsService
    {
        public Task<ActionResult<List<StartedTransaction>>> GetStartedTransactions();

        public Task<ActionResult<StartedTransaction>> GetStartedTransaction(Guid itemOfferId, Guid tokenOfferId);

        public Task InsertStartedTransaction(StartedTransaction transaction);

        public Task RemoveStartedTransaction(StartedTransaction transaction);

        public Task<ActionResult<List<AvailableTransaction>>> GetAvailableTransactions();

        public Task<ActionResult<AvailableTransaction>> GetAvailableTransaction(Guid itemOfferId, Guid tokenOfferId);

        public Task InsertAvailableTransaction(AvailableTransaction availableTransaction);

        public Task RemoveAvailableTransaction(AvailableTransaction availableTransaction);
    }
}
