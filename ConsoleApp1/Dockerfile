
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

##for the corrent Telegram.Bot version, there was no the version on Nuget
RUN dotnet nuget add source "https://pkgs.dev.azure.com/tgbots/Telegram.Bot/_packaging/release/nuget/v3/index.json" -n Telegram.Bot

COPY ConsoleApp1/*.csproj ./
RUN dotnet restore

COPY ConsoleApp1/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=build app/out .
ENTRYPOINT ["dotnet", "HrukniBot.dll"]

