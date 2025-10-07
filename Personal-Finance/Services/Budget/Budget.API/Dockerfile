FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Budget/Budget.API/Budget.API.csproj", "Services/Budget/Budget.API/"]
COPY ["Services/Budget/Budget.Application/Budget.Application.csproj", "Services/Budget/Budget.Application/"]
COPY ["Services/Budget/Budget.Domain/Budget.Domain.csproj", "Services/Budget/Budget.Domain/"]
COPY ["Services/Budget/Budget.Infrastructure/Budget.Infrastructure.csproj", "Services/Budget/Budget.Infrastructure/"]
RUN dotnet restore "Services/Budget/Budget.API/Budget.API.csproj"
COPY . .
WORKDIR "/src/Services/Budget/Budget.API"
RUN dotnet build "./Budget.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Budget.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Budget.API.dll"]
