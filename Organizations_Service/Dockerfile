# See https://aka.ms/customizecontainer to learn how to customize the debug container and how Visual Studio uses this Dockerfile to build images for faster debugging.

# This stage is used when running from VS in fast mode (default for the Debug configuration).
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Organizations_Service/Organizations_Service.csproj", "Organizations_Service/"]
RUN dotnet restore "./Organizations_Service/Organizations_Service.csproj"
COPY . .
WORKDIR "/src/Organizations_Service"
RUN dotnet build "./Organizations_Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Organizations_Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (default when not using the Debug configuration).
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Organizations_Service.dll"]