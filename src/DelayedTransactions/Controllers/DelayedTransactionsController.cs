using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DelayedTransactions.Models;
using DelayedTransactions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DelayedTransactions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DelayedTransactionsController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly ITransactionsService _transactionsService;

        public DelayedTransactionsController(IStoreService storeService, ITransactionsService transactionsService)
        {
            _storeService = storeService;
            _transactionsService = transactionsService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AvailableTransaction>>> GetAvailableTransactions()
        {
            var result = await _transactionsService.GetAvailableTransactions();
            return result;
        }

        [HttpPost("start")]
        public async Task<ActionResult<StartedTransaction>> StartTransaction([FromBody] TransactionBodyRequest body)
        {
            var result = await _transactionsService.GetAvailableTransaction(body.ItemOfferId, body.TokenOfferId);
            AvailableTransaction availableTransaction = result.Value;
            if (availableTransaction == null)
            {
                return StatusCode(
                    (int)HttpStatusCode.NotFound,
                    "Cannot find transaction for given data"
                    );
            }

            var startedTransaction = await _transactionsService.GetStartedTransaction(
                availableTransaction.ItemOfferId, availableTransaction.TokenOfferId);
            if(startedTransaction.Value != null)
            {
                return StatusCode(
                    (int)HttpStatusCode.Forbidden,
                    "Transaction already failed");
            }

            //TODO: validate that transaction is OK
            await _storeService.ConsumeOffer(availableTransaction.TokenOfferId);
            
            StartedTransaction newStartedTransaction = new StartedTransaction
            {
                TokenOfferId = availableTransaction.TokenOfferId,
                ItemOfferId = availableTransaction.ItemOfferId,
                EndTime = DateTime.UtcNow.AddSeconds(availableTransaction.SecondsToComplete)
            };

            await _transactionsService.InsertStartedTransaction(newStartedTransaction);

            return StatusCode(
                (int)HttpStatusCode.OK,
                newStartedTransaction);
        }

        [HttpPost("finish")]
        public async Task<ActionResult<Guid>> FinishTransaction([FromBody] TransactionBodyRequest body)
        {
            var result = await _transactionsService.GetStartedTransaction(body.ItemOfferId, body.TokenOfferId);
            StartedTransaction startedTransaction = result.Value;

            if(startedTransaction == null)
            {
                return StatusCode(
                    (int)HttpStatusCode.NotFound,
                    "Transaction not found");
            }

            if(startedTransaction.EndTime > DateTime.UtcNow)
            {
                return StatusCode(
                    (int)HttpStatusCode.Forbidden,
                    "Transaction not finished");
            }

            await _storeService.ConsumeOffer(startedTransaction.ItemOfferId);

            await _transactionsService.RemoveStartedTransaction(startedTransaction);

            return StatusCode(
                (int)HttpStatusCode.OK,
                startedTransaction.Id);
        }
    }
}
