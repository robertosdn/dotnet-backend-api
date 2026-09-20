# Test build stage is independent from production.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY Directory.Build.props ./
COPY Backend.sln ./
COPY src/Backend.Api/Backend.Api.csproj src/Backend.Api/
COPY src/Backend.Application/Backend.Application.csproj src/Backend.Application/
COPY src/Backend.Contracts/Backend.Contracts.csproj src/Backend.Contracts/
COPY src/Backend.Domain/Backend.Domain.csproj src/Backend.Domain/
COPY src/Backend.Infrastructure/Backend.Infrastructure.csproj src/Backend.Infrastructure/
COPY tests/Backend.Application.Tests/Backend.Application.Tests.csproj tests/Backend.Application.Tests/
COPY tests/Backend.Api.IntegrationTests/Backend.Api.IntegrationTests.csproj tests/Backend.Api.IntegrationTests/
RUN dotnet restore Backend.sln

FROM restore AS build
COPY src ./src
COPY tests ./tests
RUN dotnet build src/Backend.Api/Backend.Api.csproj --configuration Release --no-restore --warnaserror

FROM build AS test
RUN dotnet test Backend.sln --configuration Release --no-restore --verbosity normal

FROM build AS publish
RUN dotnet publish src/Backend.Api/Backend.Api.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS production
WORKDIR /app

RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Backend.Api.dll"]