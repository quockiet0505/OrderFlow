# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/Inventory/Inventory.Api/Inventory.Api.csproj", "src/Inventory/Inventory.Api/"]
COPY ["src/Inventory/Inventory.Application/Inventory.Application.csproj", "src/Inventory/Inventory.Application/"]
COPY ["src/Inventory/Inventory.Domain/Inventory.Domain.csproj", "src/Inventory/Inventory.Domain/"]
COPY ["src/Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj", "src/Inventory/Inventory.Infrastructure/"]
COPY ["src/Contracts/OrderFlow.Contracts/OrderFlow.Contracts.csproj", "src/Contracts/OrderFlow.Contracts/"]

RUN dotnet restore "src/Inventory/Inventory.Api/Inventory.Api.csproj"

COPY . .
RUN dotnet publish "src/Inventory/Inventory.Api/Inventory.Api.csproj" -c Release -o /app/out /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5002
ENV ASPNETCORE_URLS=http://+:5002

ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
