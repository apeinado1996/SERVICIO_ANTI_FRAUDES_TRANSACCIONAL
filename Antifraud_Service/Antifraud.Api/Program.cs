using Antifraud.Api.Messaging;
using Antifraud.Infrastructure.Injection;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;


builder.Services.AddAntiFraudInfrastructure(configuration);

builder.Services.AddSingleton<TransactionProducer>();
builder.Services.AddHostedService<TransactionConsumer>();

// Add services to the container.

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
