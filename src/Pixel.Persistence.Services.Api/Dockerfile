#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["src/Pixel.Persistence.Services.Api/Pixel.Persistence.Services.Api.csproj", "src/Pixel.Persistence.Services.Api/"]
COPY ["src/Pixel.Automation.Web.Portal/Pixel.Automation.Web.Portal.csproj", "src/Pixel.Automation.Web.Portal/"]
COPY ["src/Pixel.Persistence.Core/Pixel.Persistence.Core.csproj", "src/Pixel.Persistence.Core/"]
COPY ["src/Pixel.Persistence.Respository/Pixel.Persistence.Respository.csproj", "src/Pixel.Persistence.Respository/"]
RUN dotnet restore "src/Pixel.Persistence.Services.Api/Pixel.Persistence.Services.Api.csproj"
COPY . .
WORKDIR "/src/src/Pixel.Persistence.Services.Api"
RUN dotnet build "Pixel.Persistence.Services.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pixel.Persistence.Services.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pixel-persistence.dll"]