# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/CronCaps.API/CronCaps.API.csproj", "src/CronCaps.API/"]
COPY ["src/CronCaps.Application/CronCaps.Application.csproj", "src/CronCaps.Application/"]
COPY ["src/CronCaps.Domain/CronCaps.Domain.csproj", "src/CronCaps.Domain/"]
COPY ["src/CronCaps.Infrastructure/CronCaps.Infrastructure.csproj", "src/CronCaps.Infrastructure/"]
COPY ["src/CronCaps.BackgroundServices/CronCaps.BackgroundServices.csproj", "src/CronCaps.BackgroundServices/"]

# Restore dependencies
RUN dotnet restore "src/CronCaps.API/CronCaps.API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/src/CronCaps.API"
RUN dotnet build "CronCaps.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CronCaps.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r croncaps && useradd --no-log-init -r -g croncaps croncaps

# Copy published application
COPY --from=publish /app/publish .

# Set ownership and permissions
RUN chown -R croncaps:croncaps /app
USER croncaps

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "CronCaps.API.dll"]