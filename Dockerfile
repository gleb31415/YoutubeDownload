# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file
COPY YoutubeDownloader.Web/YoutubeDownloader.Web.csproj YoutubeDownloader.Web/

# Restore dependencies
RUN dotnet restore YoutubeDownloader.Web/YoutubeDownloader.Web.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/YoutubeDownloader.Web
RUN dotnet build YoutubeDownloader.Web.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish YoutubeDownloader.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install ffmpeg
RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "YoutubeDownloader.Web.dll"]