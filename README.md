# Static File Server

## Introduction

Static file server that uses environment variables for configuration.
The file server is available as a container image here: https://hub.docker.com/r/marcelndev/static-file-server.

###### 💡 Important

The file server _in its current form_ is **not meant** to be used as an edge (internet-facing) web server.
A reverse proxy should be used to support HTTPS for example. Some reverse proxies that can be used: _Apache_, _Caddy_, _IIS_, _Nginx_, _YARP_

## Configuration

The following environment variables are supported to configure the file server:

| Environment variable    | Default value | Description                                                                                                                                                   |
| :---------------------- | :------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `SFS_PORT`              | `80`          | Must be a valid port number.                                                                                                                                  |
| `SFS_DEBUG`             | `false`       | Enable debugging for troubleshooting.<br>If set to `true` this prints detailed information about each request (and its response) to stdout.                   |
| `SFS_ALLOW_INDEX`       | `true`        | When set to `true` the index.html file in the folder will be served.                                                                                          |
| `SFS_SHOW_LISTING`      | `true`        | Serves the directory listing when set to `true`.<br>If `ALLOW_INDEX` is also `true` no listing will be shown when an index.html file exists in the directory. |
| `SFS_FALLBACK_TO_INDEX` | `false`       | When set to `true` the root index.html file will be served in case a non-existent path is requested.                                                          |
