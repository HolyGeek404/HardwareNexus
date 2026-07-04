FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GoodStuff.CartApi.Presentation/GoodStuff.CartApi.Presentation.csproj", "GoodStuff.CartApi.Presentation/"]
COPY ["GoodStuff.CartApi.Domain/GoodStuff.CartApi.Domain.csproj", "GoodStuff.CartApi.Domain/"]
COPY ["GoodStuff.CartApi.Application/GoodStuff.CartApi.Application.csproj", "GoodStuff.CartApi.Application/"]
COPY ["GoodStuff.CartApi.Infrastructure/GoodStuff.CartApi.Infrastructure.csproj", "GoodStuff.CartApi.Infrastructure/"]
RUN dotnet restore "GoodStuff.CartApi.Presentation/GoodStuff.CartApi.Presentation.csproj"
COPY . .
WORKDIR "/src/GoodStuff.CartApi.Presentation"
RUN dotnet build "./GoodStuff.CartApi.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GoodStuff.CartApi.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GoodStuff.CartApi.Presentation.dll"]
