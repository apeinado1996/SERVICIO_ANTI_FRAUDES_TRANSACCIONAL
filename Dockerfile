FROM mcr.microsoft.com/dotnet/sdk:8.0 AS testrunner

WORKDIR /src
COPY . .
WORKDIR /src/Transaction_Service/Transaction.Tests

CMD ["dotnet", "test", "--logger:trx;LogFileName=test-results.trx"]