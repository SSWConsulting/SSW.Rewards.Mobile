# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/WebAPI/WebAPI.csproj", "src/WebAPI/"]
COPY ["src/Application/SSW.Rewards.Application.csproj", "src/Application/"]
COPY ["src/Domain/SSW.Rewards.Domain.csproj", "src/Domain/"]
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

# # FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# # USER $APP_UID
# # WORKDIR /app

# # FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# # ARG BUILD_CONFIGURATION=Release
# # WORKDIR /src
# # COPY ["src/WebAPI/WebAPI.csproj", "src/WebAPI/"]
# # COPY ["src/Application/SSW.Rewards.Application.csproj", "src/Application/"]
# # COPY ["src/Domain/SSW.Rewards.Domain.csproj", "src/Domain/"]
# # COPY ["src/Infrastructure/SSW.Rewards.Infrastructure.csproj", "src/Infrastructure/"]
# # RUN dotnet restore "src/WebAPI/WebAPI.csproj"

# # COPY . .
# # WORKDIR "/src/src/WebAPI"
# # RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
# #     dotnet publish -a ${TARGETARCH/amd64/x64} --use-current-runtime --self-contained false -o /app

# # FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
# # COPY . /src
# # WORKDIR /src/src/WebAPI
# # RUN dotnet dev-certs https
# # RUN dotnet dev-certs https --trust
# # CMD dotnet run --no-launch-profile

# FROM build AS publish
# ARG BUILD_CONFIGURATION=Release
# RUN dotnet publish "WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# FROM base AS final
# WORKDIR /app
# COPY --from=publish /app/publish .
# ENTRYPOINT ["dotnet", "SSW.Rewards.WebAPI.dll"]
