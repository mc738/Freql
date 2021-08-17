FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Freql.TestConsole/Freql.TestConsole.fsproj", "Freql.TestConsole/"]
RUN dotnet restore "Freql.TestConsole/Freql.TestConsole.fsproj"
COPY . .
WORKDIR "/src/Freql.TestConsole"
RUN dotnet build "Freql.TestConsole.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Freql.TestConsole.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Freql.TestConsole.dll"]
