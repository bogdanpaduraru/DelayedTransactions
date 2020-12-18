using DelayedTransactions.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelayedTransactions.Services
{
    public interface IStoreService
    {
        public Task<IActionResult> ConsumeOffer(Guid offerId);
    }
}
