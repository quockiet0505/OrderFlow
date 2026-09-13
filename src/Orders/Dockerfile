FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine
WORKDIR /app

COPY . .
RUN dotnet publish "src/Orders/Orders.Api/Orders.Api.csproj" -c Release -o /app/out /p:UseAppHost=false

EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001

WORKDIR /app/out
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
