using Antifraud.Core.Entities;
using Antifraud.Core.Rules;
using FluentAssertions;
using Xunit;

namespace Antifraud.Tests.Rules
{
    public class AmountLimitRuleTests
    {
        private static IncomingTransaction CreateTx(decimal value)
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
        public async Task ShouldMax()
        {            
            var rule = new AmountLimitRule();
            var tx = CreateTx(1999m);            
            var result = await rule.IsFraudulentAsync(tx, default);

            
            result.Should().BeFalse();
            rule.Reason.Should().Be("Transaction exceeds allowed maximum amount (2000).");
        }

        [Fact]
        public async Task ShouldEqualMax()
        {
            
            var rule = new AmountLimitRule();
            var tx = CreateTx(2000m);            
            var result = await rule.IsFraudulentAsync(tx, default);
            
            result.Should().BeFalse();
            rule.Reason.Should().Be("Transaction exceeds allowed maximum amount (2000).");
        }

        [Fact]
        public async Task ShouldGreaterMax()
        {
            
            var rule = new AmountLimitRule();
            var tx = CreateTx(2001m);            
            var result = await rule.IsFraudulentAsync(tx, default);   
            
            result.Should().BeTrue();
            rule.Reason.Should().Be("Transaction exceeds allowed maximum amount (2000).");
        }
    }
}
