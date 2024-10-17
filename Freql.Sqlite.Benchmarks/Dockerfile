FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Freql.Sqlite.Benchmarks/Freql.Sqlite.Benchmarks.fsproj", "Freql.Sqlite.Benchmarks/"]
RUN dotnet restore "Freql.Sqlite.Benchmarks/Freql.Sqlite.Benchmarks.fsproj"
COPY . .
WORKDIR "/src/Freql.Sqlite.Benchmarks"
RUN dotnet build "Freql.Sqlite.Benchmarks.fsproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Freql.Sqlite.Benchmarks.fsproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Freql.Sqlite.Benchmarks.dll"]
