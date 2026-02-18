FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DevicesApi.slnx ./
COPY src/DevicesApi.Domain/DevicesApi.Domain.csproj src/DevicesApi.Domain/
COPY src/DevicesApi.Application/DevicesApi.Application.csproj src/DevicesApi.Application/
COPY src/DevicesApi.Infrastructure/DevicesApi.Infrastructure.csproj src/DevicesApi.Infrastructure/
COPY src/DevicesApi.Api/DevicesApi.Api.csproj src/DevicesApi.Api/
COPY tests/DevicesApi.Tests/DevicesApi.Tests.csproj tests/DevicesApi.Tests/
RUN dotnet restore

COPY . .
RUN dotnet publish src/DevicesApi.Api/DevicesApi.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd -r appuser && useradd -r -g appuser appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

USER appuser

ENTRYPOINT ["dotnet", "DevicesApi.Api.dll"]
