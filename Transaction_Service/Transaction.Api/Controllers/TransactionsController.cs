using Microsoft.AspNetCore.Mvc;
using Transaction.Api.DTOs.Requests;
using Transaction.Api.DTOs.Responses;
using Transaction.Api.Events;
using Transaction.Api.Messaging;
using Transaction.Core.Services;

namespace Transaction.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ITransactionProducer _producer;

        public TransactionsController(ITransactionService transactionService, ITransactionProducer producer, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _producer = producer;
        }

        [HttpGet]
        public IActionResult Get() => Ok(new { status = 200, message = "ok" });

        [HttpPost]
        public async Task<ActionResult<CreateTransactionResponse>> CreateAsync([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            if (request.Value <= 0)
            {
                return BadRequest(new { status = 500, message = "Value must be greater than zero." });
            }

            var transaction = await _transactionService.CreateTransactionAsync(request.SourceAccountId, request.TargetAccountId, request.TranferTypeId, request.Value, cancellationToken);

            var evt = new TransactionCreatedEvent
            {
                TransactionExternalId = transaction.ExternalId,
                SourceAccountId = transaction.SourceAccountId,
                TargetAccountId = transaction.TargetAccountId,
                TranferTypeId = transaction.TransferTypeId,
                Value = transaction.Value,
                CreatedAt = transaction.CreatedAt
            };

            try
            {
                await _producer.PublishAsync(evt, cancellationToken);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }

            var response = new CreateTransactionResponse
            {
                TransactionExternalId = transaction.ExternalId,
                CreatedAt = transaction.CreatedAt,
                Status = transaction.Status.ToString(),
                Value = transaction.Value
            };

            return Ok(response);
        }

        [HttpPost("status")]
        public async Task<ActionResult<GetTransactionStatusResponse>> GetStatusAsync([FromBody] GetTransactionStatusRequest request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionService.GetTransactionStatusAsync(request.TransactionExternalId, request.CreatedAt, cancellationToken);

            if (transaction is null)
            {
                return NotFound("Transaction not found.");
            }

            var response = new GetTransactionStatusResponse
            {
                TransactionExternalId = transaction.ExternalId,
                CreatedAt = transaction.CreatedAt,
                Status = transaction.Status.ToString(),
                Value = transaction.Value
            };

            return Ok(response);
        }
    }
}
