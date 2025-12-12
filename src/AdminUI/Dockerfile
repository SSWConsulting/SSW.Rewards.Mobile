FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /src
COPY ["src/WebAPI/WebAPI.csproj", "src/WebAPI/"]
COPY ["src/Application/SSW.Rewards.Application.csproj", "src/Application/"]
COPY ["src/Domain/SSW.Rewards.Domain.csproj", "src/Domain/"]
COPY ["src/SSW.Rewards.Enums/SSW.Rewards.Enums.csproj", "src/SSW.Rewards.Enums/"]
COPY ["src/Common/Shared.csproj", "src/Common/"]
COPY ["src/Infrastructure/SSW.Rewards.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/AdminUI/AdminUI.csproj"

FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS development
COPY . /src
WORKDIR /src/src/AdminUI
RUN dotnet workload update
RUN dotnet workload restore
RUN dotnet dev-certs https
RUN dotnet dev-certs https --trust
CMD dotnet run --no-launch-profile

# FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
# ARG BUILD_CONFIGURATION=Release

# WORKDIR /app
# COPY [".", "./"]

# RUN dotnet publish "src/AdminUI/AdminUI.csproj" -c $BUILD_CONFIGURATION -o output

# # Prepare an Alpine-based image with OpenSSL.
# FROM alpine:latest as alpine
# RUN apk add --no-cache openssl
# RUN openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout localhost.key -out localhost.crt -subj "/C=GB/CN=localhost"

# FROM nginx
# # Copy the certificate files from the Alpine image.
# COPY --from=alpine localhost.key /etc/nginx/ssl/localhost.key
# COPY --from=alpine localhost.crt /etc/nginx/ssl/localhost.crt
# # COPY --from=build /app/src/Presentation/UI/AdminUI/nginx/nginx-ssl.conf /etc/nginx/conf.d/default.conf

# # Copy Blazor App
# WORKDIR /usr/share/nginx/html
# COPY --from=build /app/output/wwwroot .