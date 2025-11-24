using Antifraud.Core.Entities;
using Antifraud.Core.Interface;
using Antifraud.Core.Rules;
using FluentAssertions;
using Moq;

namespace Antifraud.Tests.Rules;

public class DailyLimitRuleTests
{
    private static IncomingTransaction CreateTx(Guid? sourceId = null, decimal value = 1000m)
    {
        return new IncomingTransaction
        {
            TransactionExternalId = Guid.NewGuid(),
            SourceAccountId = sourceId ?? Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = value,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task ShouldDailyTotal()
    {
        var accountId = Guid.NewGuid();
        var tx = CreateTx(accountId, 5000);

        var repoMock = new Mock<ITransactionReadRepository>();
        repoMock.Setup(r => r.GetDailyTotalForSourceAsync(accountId, tx.CreatedAt.Date, It.IsAny<CancellationToken>())).ReturnsAsync(20000m);

        var rule = new DailyLimitRule(repoMock.Object);

        var result = await rule.IsFraudulentAsync(tx, default);

        result.Should().BeFalse();
        rule.Reason.Should().Be("Daily transaction limit exceeded (20000).");
    }

    [Fact]
    public async Task ShouldDailyTotalIsGreater()
    {
        var accountId = Guid.NewGuid();
        var tx = CreateTx(accountId, 8000);

        var repoMock = new Mock<ITransactionReadRepository>();
        repoMock.Setup(r => r.GetDailyTotalForSourceAsync(accountId, tx.CreatedAt.Date, It.IsAny<CancellationToken>())).ReturnsAsync(25000m);

        var rule = new DailyLimitRule(repoMock.Object);
        var result = await rule.IsFraudulentAsync(tx, default);

        result.Should().BeTrue();
        rule.Reason.Should().Be("Daily transaction limit exceeded (20000).");
    }

    [Fact]
    public async Task ShouldCorrectParameters()
    {
        var accountId = Guid.NewGuid();
        var tx = CreateTx(accountId, 1200m);

        var repoMock = new Mock<ITransactionReadRepository>();
        repoMock.Setup(r => r.GetDailyTotalForSourceAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(0m);
        var rule = new DailyLimitRule(repoMock.Object);

        await rule.IsFraudulentAsync(tx, default);

        repoMock.Verify(r => r.GetDailyTotalForSourceAsync(tx.SourceAccountId, tx.CreatedAt.Date, It.IsAny<CancellationToken>()), Times.Once);
    }
}
