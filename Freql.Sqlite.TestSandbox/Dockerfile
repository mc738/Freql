FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Freql.SqliteTestSandbox/Freql.SqliteTestSandbox.fsproj", "Freql.SqliteTestSandbox/"]
RUN dotnet restore "Freql.SqliteTestSandbox/Freql.SqliteTestSandbox.fsproj"
COPY . .
WORKDIR "/src/Freql.SqliteTestSandbox"
RUN dotnet build "Freql.SqliteTestSandbox.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Freql.SqliteTestSandbox.fsproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Freql.SqliteTestSandbox.dll"]
