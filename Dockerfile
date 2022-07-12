FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

ARG VAULT_TOKEN
ARG CONSUL_HTTP_TOKEN

ENV HTDC_VAULT_TOKEN=$VAULT_TOKEN
ENV CONSUL_HTTP_TOKEN=$CONSUL_HTTP_TOKEN
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
ARG Configuration=Release
ARG GITHUB_TOKEN
WORKDIR /src
COPY . .
RUN dotnet restore

FROM build AS juniper.dependencies
ARG Configuration=Release
RUN dotnet build HappyTravel.JuniperConnector.Common -c $Configuration --no-restore --no-dependencies && \
    dotnet build HappyTravel.JuniperConnector.Data -c $Configuration --no-restore --no-dependencies 
    

FROM giata.dependencies AS publish
ARG Configuration=Release
ARG GITHUB_TOKEN
WORKDIR /src
RUN dotnet build HappyTravel.JuniperConnector.Api -c $Configuration --no-restore --no-dependencies
RUN dotnet publish --no-build --no-restore --no-dependencies -c $Configuration -o /app HappyTravel.JuniperConnector.Api

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

HEALTHCHECK --interval=6s --timeout=10s --retries=3 CMD curl -sS 127.0.0.1:80/health || exit 1

ENTRYPOINT ["dotnet", "HappyTravel.JuniperConnector.Api.dll"]
