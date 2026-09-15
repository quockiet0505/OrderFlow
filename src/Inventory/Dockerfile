FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app

COPY . .
RUN dotnet publish "src/Inventory/Inventory.Api/Inventory.Api.csproj" -c Release -o /app/out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5002
ENV ASPNETCORE_URLS=http://+:5002

ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
