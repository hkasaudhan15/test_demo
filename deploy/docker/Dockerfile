# ═══════════════════════════════════════════════════════
# Multi-stage Dockerfile — optimized for layer caching
# ═══════════════════════════════════════════════════════

# Stage 1: Restore dependencies (cached if .csproj files unchanged)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY *.sln .
COPY Directory.Build.props .
COPY global.json .
COPY src/Core/Domain/*.csproj src/Core/Domain/
COPY src/Core/Application/*.csproj src/Core/Application/
COPY src/Core/SharedKernel/*.csproj src/Core/SharedKernel/
COPY src/External/Infrastructure/*.csproj src/External/Infrastructure/
COPY src/External/Presentation/*.csproj src/External/Presentation/
COPY src/External/Migrator/*.csproj src/External/Migrator/
COPY src/CrossCutting/*.csproj src/CrossCutting/

RUN dotnet restore

# Stage 2: Build
FROM restore AS build
COPY . .
RUN dotnet publish src/External/Presentation/CleanArch.Presentation.csproj \
    -c Release -o /app/publish --no-restore

# Stage 3: Runtime (minimal image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Security: non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CleanArch.Presentation.dll"]
