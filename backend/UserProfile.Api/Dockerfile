FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY UserProfile.Api/UserProfile.Api.csproj UserProfile.Api/
RUN dotnet restore UserProfile.Api/UserProfile.Api.csproj
COPY UserProfile.Api/ UserProfile.Api/
RUN dotnet publish UserProfile.Api/UserProfile.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN mkdir -p /app/data && chown -R app:app /app
USER app
COPY --from=build --chown=app:app /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "UserProfile.Api.dll"]
