# syntax=docker/dockerfile:1
# ---- مرحلة البناء ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FalakAlkhair.sln ./
COPY FalakAlkhair.Domain/FalakAlkhair.Domain.csproj FalakAlkhair.Domain/
COPY FalakAlkhair.Application/FalakAlkhair.Application.csproj FalakAlkhair.Application/
COPY FalakAlkhair.Infrastructure/FalakAlkhair.Infrastructure.csproj FalakAlkhair.Infrastructure/
COPY FalakAlkhair.API/FalakAlkhair.API.csproj FalakAlkhair.API/
COPY FalakAlkhair.UnitTests/FalakAlkhair.UnitTests.csproj FalakAlkhair.UnitTests/
RUN dotnet restore FalakAlkhair.API/FalakAlkhair.API.csproj

COPY . .
RUN dotnet publish FalakAlkhair.API/FalakAlkhair.API.csproj -c Release -o /app/publish --no-restore

# ---- مرحلة التشغيل ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN useradd --uid 1654 --user-group appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FalakAlkhair.API.dll"]
