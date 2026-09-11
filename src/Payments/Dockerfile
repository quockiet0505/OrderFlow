# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/Payments/Payments.Api/Payments.Api.csproj", "src/Payments/Payments.Api/"]
COPY ["src/Payments/Payments.Application/Payments.Application.csproj", "src/Payments/Payments.Application/"]
COPY ["src/Payments/Payments.Domain/Payments.Domain.csproj", "src/Payments/Payments.Domain/"]
COPY ["src/Payments/Payments.Infrastructure/Payments.Infrastructure.csproj", "src/Payments/Payments.Infrastructure/"]
COPY ["src/Contracts/OrderFlow.Contracts/OrderFlow.Contracts.csproj", "src/Contracts/OrderFlow.Contracts/"]

RUN dotnet restore "src/Payments/Payments.Api/Payments.Api.csproj"

COPY . .
RUN dotnet publish "src/Payments/Payments.Api/Payments.Api.csproj" -c Release -o /app/out /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5003
ENV ASPNETCORE_URLS=http://+:5003

ENTRYPOINT ["dotnet", "Payments.Api.dll"]
