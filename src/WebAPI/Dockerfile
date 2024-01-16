# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/WebAPI/WebAPI.csproj", "src/WebAPI/"]
COPY ["src/Application/SSW.Rewards.Application.csproj", "src/Application/"]
COPY ["src/Domain/SSW.Rewards.Domain.csproj", "src/Domain/"]
COPY ["src/SSW.Rewards.Enums/SSW.Rewards.Enums.csproj", "src/SSW.Rewards.Enums/"]
COPY ["src/Common/Shared.csproj", "src/Common/"]
COPY ["src/Infrastructure/SSW.Rewards.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/WebAPI/WebAPI.csproj"

COPY . .
WORKDIR "/src/src/WebAPI"

# copy and publish app and libraries
RUN dotnet publish "WebAPI.csproj" --no-restore -o /app /p:UseAppHost=false


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 5001
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "SSW.Rewards.WebAPI.dll"]