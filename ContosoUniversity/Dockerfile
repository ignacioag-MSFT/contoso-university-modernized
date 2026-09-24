FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ContosoUniversity.csproj .
RUN dotnet restore ContosoUniversity.csproj

COPY . .
RUN dotnet publish ContosoUniversity.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .
USER $APP_UID

ENTRYPOINT ["dotnet", "ContosoUniversity.dll"]