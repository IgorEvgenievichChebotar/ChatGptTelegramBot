FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ChatGptTelegramBot/ChatGptTelegramBot.csproj", "ChatGptTelegramBot/"]
RUN dotnet restore "ChatGptTelegramBot/ChatGptTelegramBot.csproj"
COPY . .
WORKDIR "/src/ChatGptTelegramBot"
RUN dotnet build "ChatGptTelegramBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatGptTelegramBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatGptTelegramBot.dll"]
