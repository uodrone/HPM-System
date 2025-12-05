# Stage 1: сборка
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем .csproj для восстановления зависимостей
COPY ["Gateway/HPM-System.Gateway.csproj", "Gateway/"]

# Восстанавливаем зависимости
RUN dotnet restore "Gateway/HPM-System.Gateway.csproj"

# Копируем всё остальное
COPY . .
WORKDIR "/src/Gateway"

# Собираем в Release
RUN dotnet build "HPM-System.Gateway.csproj" -c Release -o /app/build

# Stage 2: публикация
FROM build AS publish
RUN dotnet publish "HPM-System.Gateway.csproj" -c Release -o /app/publish --no-build

# Stage 3: финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HPM-System.Gateway.dll"]