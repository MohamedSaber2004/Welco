# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for caching layer
COPY ["Auth.Services.API/Auth.Services.API.csproj", "Auth.Services.API/"]
COPY ["Welco.Shared/Welco.Shared.csproj", "Welco.Shared/"]

# Restore dependencies
RUN dotnet restore "Auth.Services.API/Auth.Services.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/Auth.Services.API"
RUN dotnet publish "Auth.Services.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published artifacts from build stage
COPY --from=build /app/publish .

# Set default port for Render/Cloud environments
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Auth.Services.API.dll"]
