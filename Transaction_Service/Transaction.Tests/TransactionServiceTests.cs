using FluentAssertions;
using Moq;
using Transaction.Core.Entities;
using Transaction.Core.Interface;
using Transaction.Core.Services;
using Xunit;

namespace Transaction.Tests.Services
{
    public class TransactionServiceTests
    {
        private TransactionService CreateService(Mock<ITransactionRepository> repoMock)
        {
            return new TransactionService(repoMock.Object);
        }

        [Fact]
        public async Task CreateTransactionAsyncPending()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var service = CreateService(repoMock);

            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var transferTypeId = 1;
            var value = 1500m;

            TransactionEntity? addedEntity = null;

            repoMock.Setup(r => r.AddAsync(It.IsAny<TransactionEntity>(), It.IsAny<CancellationToken>())).Callback<TransactionEntity, CancellationToken>((t, _) => addedEntity = t).Returns(Task.CompletedTask);
            repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await service.CreateTransactionAsync(sourceId, targetId, transferTypeId, value, CancellationToken.None);

            result.Should().NotBeNull();
            result.ExternalId.Should().NotBe(Guid.Empty);
            result.Id.Should().NotBe(Guid.Empty);
            result.Status.Should().Be(TransactionStatus.Pending);
            result.SourceAccountId.Should().Be(sourceId);
            result.TargetAccountId.Should().Be(targetId);
            result.TransferTypeId.Should().Be(transferTypeId);
            result.Value.Should().Be(value);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.UpdatedAt.Should().BeNull();

            repoMock.Verify(r => r.AddAsync(It.IsAny<TransactionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
            repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            addedEntity.Should().NotBeNull();
            addedEntity!.ExternalId.Should().Be(result.ExternalId);
            addedEntity.SourceAccountId.Should().Be(sourceId);
            addedEntity.TargetAccountId.Should().Be(targetId);
            addedEntity.Value.Should().Be(value);
        }

        [Fact]
        public async Task CreateTransactionAsyncDifferentExternalIds()
        {
            var repoMock = new Mock<ITransactionRepository>();
            repoMock.Setup(r => r.AddAsync(It.IsAny<TransactionEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var service = CreateService(repoMock);

            var tx1 = await service.CreateTransactionAsync(Guid.NewGuid(), Guid.NewGuid(), 1, 1000m, CancellationToken.None);
            var tx2 = await service.CreateTransactionAsync(Guid.NewGuid(), Guid.NewGuid(), 1, 2000m, CancellationToken.None);

            tx1.ExternalId.Should().NotBe(tx2.ExternalId);
            tx1.Id.Should().NotBe(tx2.Id);
        }

        [Fact]
        public async Task GetTransactionStatusAsync()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var service = CreateService(repoMock);

            var externalId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow.AddMinutes(-5);

            var expected = new TransactionEntity
            {
                Id = Guid.NewGuid(),
                ExternalId = externalId,
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                TransferTypeId = 1,
                Value = 1500m,
                Status = TransactionStatus.Approved,
                CreatedAt = createdAt,
                UpdatedAt = DateTime.UtcNow
            };

            repoMock.Setup(r => r.GetByExternalIdAsync(externalId, createdAt, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
            var result = await service.GetTransactionStatusAsync(externalId, createdAt, CancellationToken.None);

            result.Should().NotBeNull();
            result!.ExternalId.Should().Be(externalId);
            result.Status.Should().Be(TransactionStatus.Approved);

            repoMock.Verify(r => r.GetByExternalIdAsync(externalId, createdAt, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTransactionStatusAsyncNull()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var service = CreateService(repoMock);

            var externalId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            repoMock.Setup(r => r.GetByExternalIdAsync(externalId, createdAt, It.IsAny<CancellationToken>())).ReturnsAsync((TransactionEntity?)null);

            var result = await service.GetTransactionStatusAsync(externalId, createdAt, CancellationToken.None);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ApplyValidationResultAsync()
        {
            var repoMock = new Mock<ITransactionRepository>();
            var service = CreateService(repoMock);

            var externalId = Guid.NewGuid();
            var status = "APPROVED";
            var reason = "OK";

            repoMock.Setup(r => r.ApplyValidationResultAsync(externalId, status, reason, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await service.ApplyValidationResultAsync(externalId, status, reason, CancellationToken.None);
            repoMock.Verify(r => r.ApplyValidationResultAsync(externalId, status, reason, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
