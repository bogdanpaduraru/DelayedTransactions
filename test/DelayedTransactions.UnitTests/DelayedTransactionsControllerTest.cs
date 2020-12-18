using DelayedTransactions.Controllers;
using DelayedTransactions.Models;
using DelayedTransactions.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Xunit;

namespace DelayedTransactions.UnitTests
{
    public class DelayedTransactionsControllerTest
    {
        public static IEnumerable<object[]> AvailableTransactionsTestData =>
        new List<object[]>
        {
            new object[]
            {
                new List<AvailableTransaction>
                {
                    new AvailableTransaction
                    {
                        ItemOfferId = new Guid(),
                        TokenOfferId = new Guid(),
                        SecondsToComplete = 5
                    }
                }
            },
            new object[]
            {
                new List<AvailableTransaction>()
            },
        };

        [Theory]
        [MemberData(nameof(AvailableTransactionsTestData))]
        public async Task GetAvailableTransactionsTests(List<AvailableTransaction> _availableTransactions)
        {
            var transactionsServiceMock = new Mock<ITransactionsService>();

            transactionsServiceMock.Setup(x => x.GetAvailableTransactions()).ReturnsAsync(_availableTransactions);

            var controller = new DelayedTransactionsController(null, transactionsServiceMock.Object);

            var result = await controller.GetAvailableTransactions();

            Assert.NotNull(result);

            transactionsServiceMock.Verify(x => x.GetAvailableTransactions(), Times.Once());

            Assert.Equal(result.Value, _availableTransactions);
        }

        public static IEnumerable<object[]> StartTransactionTestData =>
        new List<object[]>
        {
            new object[]
            {
                new TransactionBodyRequest
                {
                    ItemOfferId = new Guid(),
                    PlayerProfileId = "",
                    TokenOfferId = new Guid()
                },
                null,
                null,
                (int)HttpStatusCode.NotFound
            },
            new object[]
            {
                new TransactionBodyRequest
                {
                    ItemOfferId = new Guid(),
                    PlayerProfileId = "",
                    TokenOfferId = new Guid()
                },
                new AvailableTransaction
                {
                    ItemOfferId = new Guid(),
                    TokenOfferId = new Guid(),
                    SecondsToComplete = 5
                },
                new StartedTransaction
                { },
                (int)HttpStatusCode.Forbidden
            },
            new object[]
            {
                new TransactionBodyRequest
                {
                    ItemOfferId = new Guid(),
                    PlayerProfileId = "",
                    TokenOfferId = new Guid()
                },
                new AvailableTransaction
                {
                    ItemOfferId = new Guid(),
                    TokenOfferId = new Guid(),
                    SecondsToComplete = 5
                },
                null,
                (int)HttpStatusCode.OK
            }
        };

        [Theory]
        [MemberData(nameof(StartTransactionTestData))]
        public async Task StartTransactionTests(TransactionBodyRequest body, AvailableTransaction availableTransaction, StartedTransaction startedTransaction, int expectedHttpStatusCode)
        {
            var transactionsServiceMock = new Mock<ITransactionsService>();

            transactionsServiceMock.Setup(x => x.GetAvailableTransaction(body.ItemOfferId, body.TokenOfferId)).ReturnsAsync(availableTransaction);

            transactionsServiceMock.Setup(x => x.GetStartedTransaction(body.ItemOfferId, body.TokenOfferId)).ReturnsAsync(startedTransaction);

            var storeServiceMock = new Mock<IStoreService>();

            //TODO: ConsumeOffer and InsertStartedTransaction should also me mocked

            var controller = new DelayedTransactionsController(storeServiceMock.Object, transactionsServiceMock.Object);

            var result = await controller.StartTransaction(body);

            var statusCodeResult = result.Result as ObjectResult;

            Assert.NotNull(statusCodeResult);

            Assert.Equal(statusCodeResult.StatusCode, expectedHttpStatusCode);

            switch (statusCodeResult.StatusCode)
            {
                case (int)HttpStatusCode.NotFound:
                    {
                        transactionsServiceMock.Verify(x => x.GetAvailableTransaction(body.ItemOfferId, body.TokenOfferId), Times.Once());
                        break;
                    }
                case (int)HttpStatusCode.Forbidden:
                    {
                        transactionsServiceMock.Verify(x => x.GetAvailableTransaction(body.ItemOfferId, body.TokenOfferId), Times.Once());
                        transactionsServiceMock.Verify(x => x.GetStartedTransaction(body.ItemOfferId, body.TokenOfferId), Times.Once());
                        break;
                    }
                case (int)HttpStatusCode.OK:
                    {
                        Assert.NotNull(result.Result);
                        break;
                    }
                default:
                    {
                        Assert.True(false);
                        break;
                    }
            }
        }

        public static IEnumerable<object[]> FinishTransactionData =>
        new List<object[]>
        {
            new object[]
            {
                new TransactionBodyRequest
                {
                    ItemOfferId = new Guid(),
                    TokenOfferId = new Guid(),
                    PlayerProfileId = ""
                },
                null,
                (int)HttpStatusCode.NotFound
            },
            new object[]
            {
                new TransactionBodyRequest
                {
                    ItemOfferId = new Guid(),
                    TokenOfferId = new Guid(),
                    PlayerProfileId = ""
                },
                new StartedTransaction
                {
                    ItemOfferId = new Guid(),
                    TokenOfferId = new Guid(),
                    EndTime = DateTime.UtcNow.AddSeconds(100)
                },
                (int)HttpStatusCode.Forbidden
            },
            new object[]
            {
                new TransactionBodyRequest
                {
                    ItemOfferId = new Guid(),
                    TokenOfferId = new Guid(),
                    PlayerProfileId = ""
                },
                new StartedTransaction
                {
                    ItemOfferId = new Guid(),
                    TokenOfferId = new Guid(),
                    EndTime = DateTime.UtcNow.AddSeconds(-1)
                },
                (int)HttpStatusCode.OK
            }
        };

        [Theory]
        [MemberData(nameof(FinishTransactionData))]
        public async Task FinishTransactionTests(TransactionBodyRequest body, StartedTransaction startedTransaction, int expectedHttpStatusCode)
        {
            var storeServiceMock = new Mock<IStoreService>();
            var transactionServiceMock = new Mock<ITransactionsService>();

            //TODO: ConsumeOffer and RemoveStartedTransaction should also be mocked
            transactionServiceMock.Setup(x => x.GetStartedTransaction(body.ItemOfferId, body.TokenOfferId)).ReturnsAsync(startedTransaction);

            var controller = new DelayedTransactionsController(storeServiceMock.Object, transactionServiceMock.Object);

            var result = await controller.FinishTransaction(body);

            var statusCodeResult = result.Result as ObjectResult;

            Assert.NotNull(statusCodeResult);
            Assert.Equal(statusCodeResult.StatusCode, expectedHttpStatusCode);

            transactionServiceMock.Verify(x => x.GetStartedTransaction(body.ItemOfferId, body.TokenOfferId), Times.Once());

            switch (statusCodeResult.StatusCode)
            {
                case (int)HttpStatusCode.NotFound:
                case (int)HttpStatusCode.Forbidden:
                {
                    break;
                }
                case (int)HttpStatusCode.OK:
                {
                    Assert.NotNull(statusCodeResult.Value);
                    break;
                }
                default:
                {
                    Assert.True(false);
                    break;
                }
            }
        }
    }
}
