# Stage 1: Build and publish
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore dependencies
RUN dotnet restore TaskManagement.csproj --configfile NuGet.Config

# Publish the application
RUN dotnet publish TaskManagement.csproj -c Release -o /app

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Optional: expose the port your app uses
EXPOSE 5069

COPY --from=build /app .

ENTRYPOINT ["dotnet", "TaskManagement.dll"]