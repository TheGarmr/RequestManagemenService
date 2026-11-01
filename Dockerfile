FROM mcr.microsoft.com/dotnet/sdk:10.0.403-noble AS build
WORKDIR /src
COPY . .
RUN dotnet publish RequestManagementService.API/RequestManagementService.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-noble
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "RequestManagementService.API.dll"]
