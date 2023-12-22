FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app
COPY [".", "./"]
RUN dotnet publish "src/Presentation/UI/AdminUI/AdminUI.csproj" -c $BUILD_CONFIGURATION -o output

FROM nginx:latest
WORKDIR /usr/share/nginx/html
COPY --from=build /app/output/wwwroot .
