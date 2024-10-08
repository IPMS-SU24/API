#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IPMS.API/IPMS.API.csproj", "IPMS.API/"]
COPY ["IPMS.Business/IPMS.Business.csproj", "IPMS.Business/"]
COPY ["IPMS.DataAccess/IPMS.DataAccess.csproj", "IPMS.DataAccess/"]
COPY ["IPMS.NotificationStorage/IPMS.NotificationStorage.csproj", "IPMS.NotificationStorage/"]
RUN dotnet restore "./IPMS.API/IPMS.API.csproj"
COPY . .
WORKDIR "/src/IPMS.API"
RUN dotnet build "./IPMS.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IPMS.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .


# Set environment variable to run in development mode
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "IPMS.API.dll"]
