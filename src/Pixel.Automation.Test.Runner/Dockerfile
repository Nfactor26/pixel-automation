#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#FROM mcr.microsoft.com/windows/servercore:ltsc2022-amd64 
WORKDIR /pixel-automation
COPY . .
Run ls -la
RUN dotnet restore "src/Pixel.Automation.Test.Runner/Pixel.Automation.Test.Runner.csproj" --runtime linux-x64 -p:TargetFramework=net7.0
RUN dotnet restore "src/Pixel.Automation.Native.Linux/Pixel.Automation.Native.Linux.csproj" --runtime linux-x64 -p:TargetFramework=net7.0
RUN dotnet restore "src/Pixel.Automation.Web.Selenium.Components/Pixel.Automation.Web.Selenium.Components.csproj" --runtime linux-x64 -p:TargetFramework=net7.0
RUN dotnet restore "src/Pixel.Automation.Web.Playwright.Components/Pixel.Automation.Web.Playwright.Components.csproj" --runtime linux-x64 -p:TargetFramework=net7.0
RUN dotnet restore "src/Pixel.Automation.Scripting.Components/Pixel.Automation.Scripting.Components.csproj" --runtime linux-x64 -p:TargetFramework=net7.0
RUN dotnet restore "src/Pixel.Automation.RestApi.Components/Pixel.Automation.RestApi.Components.csproj" --runtime linux-x64 -p:TargetFramework=net7.0
RUN dotnet restore "src/Pixel.Automation.Assertions.Components/Pixel.Automation.Assertions.Components.csproj" --runtime linux-x64 -p:TargetFramework=net7.0
RUN dotnet restore "src/Pixel.Automation.Appium.Components/Pixel.Automation.Appium.Components.csproj" --runtime linux-x64 -p:TargetFramework=net7.0

WORKDIR "/pixel-automation/src/Pixel.Automation.Test.Runner"
RUN dotnet build "Pixel.Automation.Test.Runner.csproj" -c release --runtime linux-x64 --self-contained false -p:TargetFramework=net7.0

FROM base AS final
RUN apk add --no-cache bash
WORKDIR /pixel-run
COPY --from=build /pixel-automation/.builds/release/Runner/net7.0/linux-x64 .
ENTRYPOINT ["dotnet", "pixel-run.dll"]