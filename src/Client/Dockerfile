FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app

COPY . .
RUN dotnet publish "src/Client/OrderFlow.Client/OrderFlow.Client.csproj" -c Release -o /app/out

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/out/wwwroot .

EXPOSE 80
