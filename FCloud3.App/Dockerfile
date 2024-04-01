#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ENV DOTNET_URLS=http://+:5000
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FCloud3.App/FCloud3.App.csproj", "FCloud3.App/"]
COPY ["FCloud3.HtmlGen/FCloud3.HtmlGen.csproj", "FCloud3.HtmlGen/"]
COPY ["FCloud3.Repos/FCloud3.Repos.csproj", "FCloud3.Repos/"]
COPY ["FCloud3.DbContexts/FCloud3.DbContexts.csproj", "FCloud3.DbContexts/"]
COPY ["FCloud3.Entities/FCloud3.Entities.csproj", "FCloud3.Entities/"]
COPY ["FCloud3.Services/FCloud3.Services.csproj", "FCloud3.Services/"]
RUN dotnet restore "./FCloud3.App/FCloud3.App.csproj"
COPY . .
WORKDIR "/src/FCloud3.App"
RUN dotnet build "./FCloud3.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FCloud3.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCloud3.App.dll"]