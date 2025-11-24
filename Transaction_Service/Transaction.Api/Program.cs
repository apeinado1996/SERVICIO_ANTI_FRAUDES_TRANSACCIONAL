using Transaction.Api.Messaging;
using Transaction.Core.Services;
using Transaction.Infrastructure.Injection;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.


builder.Services.AddTransactionInfrastructure(configuration);
builder.Services.AddHostedService<TransactionValidatedConsumer>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddSingleton<ITransactionProducer, TransactionProducer>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
