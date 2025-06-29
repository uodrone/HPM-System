# Этап base — runtime для Fast Mode
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Этап build — сборка проекта
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HPM-System/HPM-System.csproj", "HPM-System/"]
RUN dotnet restore "./HPM-System/HPM-System.csproj"
COPY . .
WORKDIR "/src/HPM-System"
RUN dotnet build "./HPM-System.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этап publish — публикация
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HPM-System.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный этап
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HPM-System.dll"]