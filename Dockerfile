# Copy csproj and restore as distinct layers
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG TARGETARG
ARG BUILDPLATFORM

# Install NativeAOT build prerequisites
RUN apk add clang binutils musl-dev build-base zlib-static

WORKDIR /
COPY . .
WORKDIR /src/Web
RUN dotnet publish -c Release -a "$TARGETARG" -o /app/publish /p:DebugType=None /p:DebugSymbols=false

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