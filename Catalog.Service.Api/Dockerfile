# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first (for layer caching)
COPY Catalog.Service.Api/Catalog.Service.Api.csproj Catalog.Service.Api/
COPY Catalog.BLL/Catalog.BLL.csproj Catalog.BLL/
COPY Catalog.Contracts/Catalog.Contracts.csproj Catalog.Contracts/
COPY Catalog.Domain/Catalog.Domain.csproj Catalog.Domain/
COPY Catalog.DAL/Catalog.DAL.csproj Catalog.DAL/
COPY Catalog.Identity/Catalog.Identity.csproj Catalog.Identity/

# Restore dependencies
RUN dotnet restore Catalog.Service.Api/Catalog.Service.Api.csproj

# Copy the rest of the source
COPY . .

# Build & publish
WORKDIR /src/Catalog.Service.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Catalog.Service.Api.dll"]
