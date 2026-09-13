FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine
WORKDIR /app

COPY . .
RUN dotnet publish "src/Inventory/Inventory.Api/Inventory.Api.csproj" -c Release -o /app/out /p:UseAppHost=false

EXPOSE 5002
ENV ASPNETCORE_URLS=http://+:5002

WORKDIR /app/out
ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
