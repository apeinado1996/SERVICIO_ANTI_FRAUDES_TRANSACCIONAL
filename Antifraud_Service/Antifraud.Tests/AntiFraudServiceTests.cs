using Antifraud.Core.Entities;
using Antifraud.Core.Rules;
using Antifraud.Core.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Antifraud.Tests.Services
{
    public class AntiFraudServiceTests
    {
        private static IncomingTransaction CreateTx(decimal value = 1000m)
        {
            return new IncomingTransaction
            {
                TransactionExternalId = Guid.NewGuid(),
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                TransferTypeId = 1,
                Value = value,
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task AnalyzeAsyncNotFraud()
        {
            var rule1 = new Mock<IAntiFraudRule>();
            var rule2 = new Mock<IAntiFraudRule>();

            rule1.Setup(r => r.IsFraudulentAsync(It.IsAny<IncomingTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            rule2.Setup(r => r.IsFraudulentAsync(It.IsAny<IncomingTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var service = new AntiFraudService(new[] { rule1.Object, rule2.Object });
            var tx = CreateTx(1500m);

            var result = await service.AnalyzeAsync(tx, CancellationToken.None);

            result.IsFraud.Should().BeFalse();
            result.Reason.Should().Be("Approved");

            rule1.Verify(r => r.IsFraudulentAsync(tx, It.IsAny<CancellationToken>()), Times.Once);
            rule2.Verify(r => r.IsFraudulentAsync(tx, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AnalyzeAsynFraudFirstRuleFails()
        {
            var rule1 = new Mock<IAntiFraudRule>();
            var rule2 = new Mock<IAntiFraudRule>();

            rule1.Setup(r => r.IsFraudulentAsync(It.IsAny<IncomingTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            rule1.SetupGet(r => r.Reason).Returns("Max amount exceeded.");

            rule2.Setup(r => r.IsFraudulentAsync(It.IsAny<IncomingTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var service = new AntiFraudService(new[] { rule1.Object, rule2.Object });
            var tx = CreateTx(3000m);

            var result = await service.AnalyzeAsync(tx, CancellationToken.None);

            result.IsFraud.Should().BeTrue();
            result.Reason.Should().Be("Max amount exceeded.");

            rule1.Verify(r => r.IsFraudulentAsync(tx, It.IsAny<CancellationToken>()), Times.Once);
            rule2.Verify(r => r.IsFraudulentAsync(It.IsAny<IncomingTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AnalyzeAsyncMultipleRuleFail()
        {
            var rule1 = new Mock<IAntiFraudRule>();
            var rule2 = new Mock<IAntiFraudRule>();

            rule1.Setup(r => r.IsFraudulentAsync(It.IsAny<IncomingTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            rule1.SetupGet(r => r.Reason).Returns("Rule1 failed.");
            rule2.Setup(r => r.IsFraudulentAsync(It.IsAny<IncomingTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            rule2.SetupGet(r => r.Reason).Returns("Rule2 failed.");

            var service = new AntiFraudService(new[] { rule1.Object, rule2.Object });
            var tx = CreateTx(9000m);

            var result = await service.AnalyzeAsync(tx, CancellationToken.None);

            result.IsFraud.Should().BeTrue();
            result.Reason.Should().Be("Rule1 failed.");

            rule1.Verify(r => r.IsFraudulentAsync(tx, It.IsAny<CancellationToken>()), Times.Once);
            rule2.Verify(r => r.IsFraudulentAsync(It.IsAny<IncomingTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AnalyzeAsynNoRulesAConfigured()
        {
            var service = new AntiFraudService(new List<IAntiFraudRule>());
            var tx = CreateTx(50000m);

            var result = await service.AnalyzeAsync(tx, CancellationToken.None);

            result.IsFraud.Should().BeFalse();
            result.Reason.Should().Be("Approved");
        }
    }
}
