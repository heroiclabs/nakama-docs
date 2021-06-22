# Go Setup

Nakama server can run trusted game server code written in Go, TypeScript and Lua. This allows you to separate sensitive code from running on clients e.g. purchases, daily rewards etc.

Choosing to write your game server custom logic using Go brings with it the advantage that Go runtime code has full low-level access to the server and its environment. For more information on how the Go runtime differs from using Lua or TypeScript you can read the [Server Runtime Code Sandboxing](runtime-code-basics/#sandboxing) documentation.

<iframe width="560" height="315" src="https://www.youtube.com/embed/Ru3RZ6LkJEk" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Prerequisites

You will need to have these tools installed to work use the Nakama Go server runtimes:

* The [Go Binaries](https://golang.org/dl/)
* Basic UNIX tools or knowledge on the Windows equivalents
* [Docker Desktop](https://www.docker.com/products/docker-desktop) if you're planning to run Nakama using Docker

## Initialize the project

These steps will set up a workspace to write all your project code to be run by the game server.

1. Define the folder name that will be the workspace for the project.

```shell
mkdir go-project
cd go-project
```

2. Use Go to initialize the project, providing a [valid Go module path](https://golang.org/ref/mod#module-path), and install the Nakama runtime package.

```shell
go mod init example.com/go-project
go get github.com/heroiclabs/nakama-common/runtime
```

## Develop code

All code must start execution from a function that the game server looks for in the global scope at startup. This function must be called `InitModule` and is how you register RPCs, before/after hooks, and other event functions managed by the server.

The code below is a simple Hello World example which uses the `Logger` to write a message. Name the source file `main.go`. You can write it in your favourite editor or IDE.

```go
package main

import (
    "context"
    "database/sql"
    "github.com/heroiclabs/nakama-common/runtime"
)

func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
    logger.Info("Hello World!")
    return nil
}
```

With this code added, head back to your Terminal/Command Prompt and run the following command to vendor your Go package dependencies.

```shell
go mod vendor
```

Vendoring your Go package dependencies will place a copy of them inside a `vendor/` folder at the root of your project, as well as a `go.sum` file. Both of these should be checked in to your source control repository.

Next add a `local.yml` Nakama server configuration file. You can read more about what [configuration options](/install-configuration/) are available.

```yml
logger:
    level: DEBUG
```

## Build the Go shared object

In order to use your custom logic inside the Nakama server, you need to compile it into a shared object.

```shell
go build --trimpath --mod=vendor --buildmode=plugin -o ./backend.so
```

!!! Note
    If you are using Windows you will not be able to run this command as there is currently no support for building Go Plugins on Windows. You can use the Dockerfile example below instead to run the server using Docker.

If you're using the Docker method of running the Nakama server below, you do not need to build the Go Shared Object separately as the `Dockerfile` will take of this.

## Running the project

### With Docker

The easiest way to run your server locally is with Docker.

To do this, create a file called `Dockerfile`.

```dockerfile
FROM heroiclabs/nakama-pluginbuilder:3.3.0 AS go-builder

ENV GO111MODULE on
ENV CGO_ENABLED 1

WORKDIR /backend

COPY go.mod .
COPY main.go .
COPY vendor/ vendor/

RUN go build --trimpath --mod=vendor --buildmode=plugin -o ./backend.so

FROM heroiclabs/nakama:3.3.0

COPY --from=go-builder /backend/backend.so /nakama/data/modules/
COPY local.yml .
```

!!! Note
    It's important to note that in order for your Go module to work with Nakama it needs to be compiled using the same version of Go as was used to compile the Nakama binary itself. You can guarantee this by using the same version tags of the `nakama-pluginbuilder` and `nakama` images as you can see above.

Next create a `docker-compose.yml` file. For more information see the [Install Nakama with Docker Compose](/install-docker-quickstart/) documentation.

```yml
version: '3'
services:
  postgres:
    command: postgres -c shared_preload_libraries=pg_stat_statements -c pg_stat_statements.track=all
    environment:
      - POSTGRES_DB=nakama
      - POSTGRES_PASSWORD=localdb
    expose:
      - "8080"
      - "5432"
    image: postgres:12.2-alpine
    ports:
      - "5432:5432"
      - "8080:8080"
    volumes:
      - data:/var/lib/postgresql/data

  nakama:
    build: .
    depends_on:
      - postgres
    entrypoint:
      - "/bin/sh"
      - "-ecx"
      - >
        /nakama/nakama migrate up --database.address postgres:localdb@postgres:5432/nakama &&
        exec /nakama/nakama --config /nakama/data/local.yml --database.address postgres:localdb@postgres:5432/nakama
    expose:
      - "7349"
      - "7350"
      - "7351"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:7350/"]
      interval: 10s
      timeout: 5s
      retries: 5
    links:
      - "postgres:db"
    ports:
      - "7349:7349"
      - "7350:7350"
      - "7351:7351"
    restart: unless-stopped

volumes:
  data:
```

Now run the server with the command:

```
docker compose up
```

### Without Docker

Install a Nakama binary stack for [Linux](install-binary-linux-quickstart.md), [Windows](install-binary-windows-quickstart.md), or [macOS](install-binary-macos-quickstart.md). When this is complete you can run the game server and have it load your code:

``` shell
nakama --config local.yml --database.address <DATABASE ADDRESS>
```

### Confirming the server is running

The server logs will show this output or similar which shows that the code we wrote above was loaded and executed at startup.

``` json
{
  "level": "info",
  "ts": "....",
  "caller": "go-project/main.go:10",
  "msg": "Hello World!",
  "runtime": "go"
}
```

## Next steps

Have a look at the [Nakama project template](https://github.com/heroiclabs/nakama-project-template) which shows a larger Go example which includes how to write an [authoritative multiplayer match handler](gameplay-multiplayer-server-multiplayer.md). It shows off other concepts as well which includes [In-App Notifications](social-in-app-notifications.md), [Storage](storage-collections.md), [RPCs](runtime-code-basics.md#rpc-hook), and [User Wallets](user-accounts.md#virtual-wallet).

[https://github.com/heroiclabs/nakama-project-template](https://github.com/heroiclabs/nakama-project-template)
