using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Transaction.Api.Controllers;
using Transaction.Api.DTOs.Requests;
using Transaction.Api.DTOs.Responses;
using Transaction.Api.Events;
using Transaction.Api.Messaging;
using Transaction.Core.Entities;
using Transaction.Core.Services;

namespace Transaction.Tests.Controllers
{
    public class TransactionsControllerTests
    {
        private static TransactionsController CreateController(Mock<ITransactionService>? serviceMock = null, Mock<ITransactionProducer>? producerMock = null)
        {
            serviceMock ??= new Mock<ITransactionService>();
            producerMock ??= new Mock<ITransactionProducer>();
            var loggerMock = new Mock<ILogger<TransactionsController>>();

            return new TransactionsController(serviceMock.Object, producerMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task CreateAsyncZero()
        {
            var controller = CreateController();
            var request = new CreateTransactionRequest
            {
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                TranferTypeId = 1,
                Value = 0
            };

            var response = await controller.CreateAsync(request, CancellationToken.None);
            response.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsyncOk()
        {
            var serviceMock = new Mock<ITransactionService>();
            var producerMock = new Mock<ITransactionProducer>();
            var controller = CreateController(serviceMock, producerMock);

            var entity = new TransactionEntity
            {
                Id = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                TransferTypeId = 1,
                Value = 500m,
                Status = TransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            serviceMock.Setup(s => s.CreateTransactionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);

            var request = new CreateTransactionRequest
            {
                SourceAccountId = entity.SourceAccountId,
                TargetAccountId = entity.TargetAccountId,
                TranferTypeId = entity.TransferTypeId,
                Value = entity.Value
            };

            var response = await controller.CreateAsync(request, CancellationToken.None);

            var okResult = response.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var payload = okResult!.Value as CreateTransactionResponse;
            payload.Should().NotBeNull();
            payload!.TransactionExternalId.Should().Be(entity.ExternalId);
            payload.Value.Should().Be(entity.Value);
            payload.Status.Should().Be(entity.Status.ToString());

            serviceMock.Verify(s => s.CreateTransactionAsync(request.SourceAccountId, request.TargetAccountId, request.TranferTypeId, request.Value, It.IsAny<CancellationToken>()), Times.Once);

            producerMock.Verify(p => p.PublishAsync(It.IsAny<TransactionCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetStatusAsyncNotFound()
        {
            var serviceMock = new Mock<ITransactionService>();
            var controller = CreateController(serviceMock);

            var externalId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            serviceMock.Setup(s => s.GetTransactionStatusAsync(externalId, createdAt, It.IsAny<CancellationToken>())).ReturnsAsync((TransactionEntity?)null);

            var request = new GetTransactionStatusRequest
            {
                TransactionExternalId = externalId,
                CreatedAt = createdAt
            };


            var response = await controller.GetStatusAsync(request, CancellationToken.None);

            response.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetStatusAsyncTransactionExists()
        {
            var serviceMock = new Mock<ITransactionService>();
            var controller = CreateController(serviceMock);

            var tx = new TransactionEntity
            {
                Id = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Value = 1500m,
                Status = TransactionStatus.Approved,
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                TransferTypeId = 1
            };

            serviceMock.Setup(s => s.GetTransactionStatusAsync(tx.ExternalId, tx.CreatedAt, It.IsAny<CancellationToken>())).ReturnsAsync(tx);

            var request = new GetTransactionStatusRequest
            {
                TransactionExternalId = tx.ExternalId,
                CreatedAt = tx.CreatedAt
            };

            var response = await controller.GetStatusAsync(request, CancellationToken.None);

            var okResult = response.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var payload = okResult!.Value as GetTransactionStatusResponse;
            payload.Should().NotBeNull();

            payload!.TransactionExternalId.Should().Be(tx.ExternalId);
            payload.Status.Should().Be(tx.Status.ToString());
            payload.Value.Should().Be(tx.Value);
        }
    }
}
