# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 9080

FROM oven/bun:1.2-debian AS deps
WORKDIR /deps
RUN apt update && apt install -y git && bun add -g https://gitlab.com/haondt/dcdn.git
COPY package.json ./
COPY bun.lock ./
COPY dcdn.json ./
RUN dcdn install

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
#COPY ["Hestia/Hestia.csproj", "Hestia/"]
COPY ["Hestia/*/*.csproj", "."]
COPY ["Hestia/nuget.config", "."]
RUN for file in $(ls *.csproj); do mkdir -p ./${file%.*}/ && mv $file ./${file%.*}/; done

RUN dotnet restore "./Hestia/Hestia.csproj"
COPY ./Hestia .
COPY --from=deps /deps/Hestia/Hestia.UI/wwwroot/vendored Hestia.UI/wwwroot/vendored
WORKDIR "/src/Hestia"
RUN dotnet build "./Hestia.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Hestia.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hestia.dll"]