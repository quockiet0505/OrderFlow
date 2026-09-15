FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/Inventory/Inventory.Api/Inventory.Api.csproj", "src/Inventory/Inventory.Api/"]
COPY ["src/Inventory/Inventory.Application/Inventory.Application.csproj", "src/Inventory/Inventory.Application/"]
COPY ["src/Inventory/Inventory.Domain/Inventory.Domain.csproj", "src/Inventory/Inventory.Domain/"]
COPY ["src/Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj", "src/Inventory/Inventory.Infrastructure/"]
COPY ["src/Contracts/OrderFlow.Contracts/OrderFlow.Contracts.csproj", "src/Contracts/OrderFlow.Contracts/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/Shared/Shared.Infrastructure/"]

RUN dotnet restore "src/Inventory/Inventory.Api/Inventory.Api.csproj"

COPY . .
WORKDIR "/src/src/Inventory/Inventory.Api"
RUN dotnet publish "Inventory.Api.csproj" -c Release -o /app/out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5002
ENV ASPNETCORE_URLS=http://+:5002

ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
