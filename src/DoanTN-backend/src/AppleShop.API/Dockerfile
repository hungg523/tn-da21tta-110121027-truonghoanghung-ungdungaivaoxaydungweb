# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AppleShop.API/AppleShop.API.csproj", "src/AppleShop.API/"]
COPY ["src/AppleShop.Application/AppleShop.Application.csproj", "src/AppleShop.Application/"]
COPY ["src/AppleShop.Domain/AppleShop.Domain.csproj", "src/AppleShop.Domain/"]
COPY ["src/AppleShop.Share/AppleShop.Share.csproj", "src/AppleShop.Share/"]
COPY ["src/AppleShop.Infrastructure/AppleShop.Infrastructure.csproj", "src/AppleShop.Infrastructure/"]
RUN dotnet restore "./src/AppleShop.API/AppleShop.API.csproj"
COPY . .
WORKDIR "/src/src/AppleShop.API"
RUN dotnet build "./AppleShop.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AppleShop.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENTRYPOINT ["dotnet", "AppleShop.API.dll"]