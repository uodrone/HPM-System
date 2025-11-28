# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать, как настроить контейнер отладки
# и как Visual Studio использует этот Dockerfile для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
# Порты не требуются — сервис фоновый, без HTTP-эндпоинтов
# EXPOSE 8080
# EXPOSE 8081

# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HPM-System.TelegramBotService/HPM-System.TelegramBotService.csproj", "HPM-System.TelegramBotService/"]
RUN dotnet restore "./HPM-System.TelegramBotService/HPM-System.TelegramBotService.csproj"
COPY . .
WORKDIR "/src/HPM-System.TelegramBotService"
RUN dotnet build "./HPM-System.TelegramBotService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HPM-System.TelegramBotService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HPM-System.TelegramBotService.dll"]