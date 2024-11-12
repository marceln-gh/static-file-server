#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# BUILD WITH:
# docker build -t repo.marceln.nl/marceln/static-file-server -t localhost:5001/marceln/static-file-server .

# PUSH WITH:
# docker login repo.marceln.nl
# docker push repo.marceln.nl/marceln/time-tracker -a

# RUN WITH:
# docker run --rm -it -p 5000:8080 -e SFS_ALLOW_INDEX=true -e SFS_SHOW_LISTING=true -e SFS_FALLBACK_TO_INDEX=false repo.marceln.nl/marceln/static-file-server

# Copy csproj and restore as distinct layers
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.19 AS build

# Install NativeAOT build prerequisites
RUN apk add --no-cache \
    clang \
    build-base \
    zlib-dev

WORKDIR /
COPY . .

WORKDIR /src/Web
RUN dotnet publish -c Release -o /app/publish /p:DebugType=None /p:DebugSymbols=false

# Create final (runtime) image
FROM alpine:3.19

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

EXPOSE 5000
VOLUME /wwwroot
COPY --from=build /app/publish/sfs /sfs
COPY --from=build /app/publish/wwwroot/index.html /wwwroot/index.html

RUN ls | grep "sfs"

ENTRYPOINT ["/sfs"]
CMD []