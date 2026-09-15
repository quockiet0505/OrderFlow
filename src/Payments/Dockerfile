FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app

COPY . .
RUN dotnet publish "src/Payments/Payments.Api/Payments.Api.csproj" -c Release -o /app/out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5003
ENV ASPNETCORE_URLS=http://+:5003

ENTRYPOINT ["dotnet", "Payments.Api.dll"]
