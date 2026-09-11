FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine
WORKDIR /app
COPY . .
RUN dotnet publish src/Client/OrderFlow.Client/OrderFlow.Client.csproj -c Release -o /app/out

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "/app/out/OrderFlow.Client.dll"]
