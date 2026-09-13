FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine
WORKDIR /app

COPY . .
RUN dotnet publish "src/Payments/Payments.Api/Payments.Api.csproj" -c Release -o /app/out /p:UseAppHost=false

EXPOSE 5003
ENV ASPNETCORE_URLS=http://+:5003

WORKDIR /app/out
ENTRYPOINT ["dotnet", "Payments.Api.dll"]
