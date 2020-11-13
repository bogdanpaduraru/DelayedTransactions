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
            return await _transactionsService.GetAvailableTransactions();
        }

        [HttpGet("{itemOfferId}/{tokenOfferId}")]
        public async Task<ActionResult<AvailableTransaction>> GetAvailableTransaction(Guid itemOfferId, Guid tokenOfferId)
        {
            //TODO: body checks
            return await _transactionsService.GetAvailableTransaction(itemOfferId, tokenOfferId);
        }

        [HttpPost("start")]
        public async Task<ActionResult<StartedTransaction>> StartTransaction([FromBody] TransactionBodyRequest body)
        {
            if(body == null)
            {
                return BadRequest();
            }

            var result = await _transactionsService.GetAvailableTransaction(body.ItemOfferId, body.TokenOfferId);
            AvailableTransaction availableTransaction = result.Value;
            if (availableTransaction == null)
            {
                return NotFound();
            }

            var startedTransaction = await _transactionsService.GetStartedTransaction(
                availableTransaction.ItemOfferId, availableTransaction.TokenOfferId);
            if(startedTransaction.Value != null)
            {
                return new ContentResult()
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Content = "Transaction already started"
                };
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

            return newStartedTransaction;
        }

        [HttpPost("finish")]
        public async Task<IActionResult> FinishTransaction([FromBody] TransactionBodyRequest body)
        {
            if(body == null)
            {
                return BadRequest();
            }

            var result = await _transactionsService.GetStartedTransaction(body.ItemOfferId, body.TokenOfferId);
            StartedTransaction startedTransaction = result.Value;

            if(startedTransaction == null)
            {
                return NotFound();
            }

            if(startedTransaction.EndTime > DateTime.UtcNow)
            {
                return new ContentResult()
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Content = "Transaction not finished"
                };
            }

            await _storeService.ConsumeOffer(startedTransaction.ItemOfferId);

            await _transactionsService.RemoveStartedTransaction(startedTransaction);

            return NoContent();
        }
    }
}
