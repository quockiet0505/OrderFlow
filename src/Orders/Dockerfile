FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/Orders/Orders.Api/Orders.Api.csproj", "src/Orders/Orders.Api/"]
COPY ["src/Orders/Orders.Application/Orders.Application.csproj", "src/Orders/Orders.Application/"]
COPY ["src/Orders/Orders.Domain/Orders.Domain.csproj", "src/Orders/Orders.Domain/"]
COPY ["src/Orders/Orders.Infrastructure/Orders.Infrastructure.csproj", "src/Orders/Orders.Infrastructure/"]
COPY ["src/Contracts/OrderFlow.Contracts/OrderFlow.Contracts.csproj", "src/Contracts/OrderFlow.Contracts/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/Shared/Shared.Infrastructure/"]

RUN dotnet restore "src/Orders/Orders.Api/Orders.Api.csproj"

COPY . .
WORKDIR "/src/src/Orders/Orders.Api"
RUN dotnet publish "Orders.Api.csproj" -c Release -o /app/out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001

ENTRYPOINT ["dotnet", "Orders.Api.dll"]
