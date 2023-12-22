FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source
COPY ["src/Presentation/WebAPI/WebAPI.csproj", "src/Presentation/WebAPI/"]
COPY ["src/Core/Application/SSW.Rewards.Application.csproj", "src/Core/Application/"]
COPY ["src/Core/Domain/SSW.Rewards.Domain.csproj", "src/Core/Domain/"]
COPY ["src/Infrastructure/SSW.Rewards.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/FluentEmail.Graph/FluentEmail.Graph.csproj", "src/FluentEmail.Graph/"]
RUN dotnet restore "src/Presentation/WebAPI/WebAPI.csproj"
COPY . .
WORKDIR "/source/src/Presentation/WebAPI"
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -a ${TARGETARCH/amd64/x64} --use-current-runtime --self-contained false -o /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
COPY . /source
WORKDIR /source/src/Presentation/WebAPI
RUN dotnet dev-certs https
RUN dotnet dev-certs https --trust
CMD dotnet run --no-launch-profile

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SSW.Rewards.WebAPI.dll"]
