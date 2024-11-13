#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# BUILD WITH:
# docker build -t repo.marceln.nl/marceln/static-file-server -t localhost:5001/marceln/static-file-server .

# PUSH WITH:
# docker login repo.marceln.nl
# docker push repo.marceln.nl/marceln/time-tracker -a

# RUN WITH:
# docker run --rm -it -p 5000:8080 -e SFS_ALLOW_INDEX=true -e SFS_SHOW_LISTING=true -e SFS_FALLBACK_TO_INDEX=false SFS_DEBUG=false repo.marceln.nl/marceln/static-file-server

# Copy csproj and restore as distinct layers
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG TARGETARG
ARG BUILDPLATFORM

# Install NativeAOT build prerequisites
RUN apk add clang binutils musl-dev build-base zlib-static

WORKDIR /
COPY *.sln .
COPY src/*.Build.props ./src/
COPY src/Web/*.csproj ./src/Web/

RUN dotnet restore -a ${TARGETARCH}

# Copy everything else and publish
COPY . .
WORKDIR /src/Web
RUN dotnet publish -c Release --no-restore -a ${TARGETARG} -o /app/publish /p:DebugType=None /p:DebugSymbols=false

RUN adduser --system --no-create-home --uid 1000 --shell /usr/sbin/nologin static

# Create final (runtime) image
FROM --platform=$BUILDPLATFORM scratch
ARG TARGETARG
ARG BUILDPLATFORM

EXPOSE 5000
VOLUME /wwwroot
COPY --from=build /etc/passwd /etc/passwd
COPY --from=build /app/publish/sfs /sfs
COPY --from=build /app/publish/wwwroot/index.html /wwwroot/index.html

USER static
ENTRYPOINT ["/sfs"]
CMD []