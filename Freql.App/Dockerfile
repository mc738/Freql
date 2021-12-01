FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Freql.App/Freql.App.fsproj", "Freql.App/"]
RUN dotnet restore "Freql.App/Freql.App.fsproj"
COPY . .
WORKDIR "/src/Freql.App"
RUN dotnet build "Freql.App.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Freql.App.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Freql.App.dll"]
