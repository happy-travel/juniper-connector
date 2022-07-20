FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

ARG VAULT_TOKEN
ARG BUILD_VERSION
ARG CONSUL_HTTP_TOKEN

ENV CONSUL_HTTP_TOKEN=$CONSUL_HTTP_TOKEN
ENV HTDC_VAULT_TOKEN=$VAULT_TOKEN
ENV BUILD_VERSION=$BUILD_VERSION

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
ARG GITHUB_TOKEN
WORKDIR /src/app
COPY *.sln ./
COPY . .

RUN dotnet restore HappyTravel.JuniperConnector.Api

RUN dotnet build --no-restore -c Release HappyTravel.JuniperConnector.Api

RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app

COPY --from=build /app .
HEALTHCHECK --interval=6s --timeout=10s --retries=3 CMD curl -sS 127.0.0.1/health || exit 1

ENTRYPOINT ["dotnet", "HappyTravel.JuniperConnector.Api.dll"]
