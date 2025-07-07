FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HPM-System.UserService/HPM-System.UserService.csproj", "HPM-System.UserService/"]
RUN dotnet restore "./HPM-System.UserService/HPM-System.UserService.csproj"
COPY . .
WORKDIR "/src/HPM-System.UserService"
RUN dotnet build "./HPM-System.UserService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HPM-System.UserService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HPM-System.UserService.dll"]