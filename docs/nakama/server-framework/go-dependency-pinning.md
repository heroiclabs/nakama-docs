# Go Module Dependency Pinning

When developing your server runtime code with Go it is important to be aware of the way the Go plugin module is loaded into the Nakama binary and what constraints this places on your runtime module. Since Go is a compiled and linked language, it is strict about how the final binary is put together. It is for this reason that the server runtime plugin you compile must be compiled in the same way as the Nakama server binary you are using. This imposes a few restrictions which can result in several common errors if they are not met.

Below are some of the common pitfalls you may encounter when developing your server runtime code using Go and how to effectively solve them.

## Platform Mismatch

### Problem

You will encounter an issue when attempting to load a server runtime Go module that has been compiled using a different platform than the Nakama binary itself.
For example, you cannot use a Go module that has been compiled on Linux together with a Nakama binary that has been compiled on macOS.

```json
{
  "level":"error",
  "ts":"...",
  "caller":"...",
  "msg":"Error initialising Go runtime provider",
  "error":"plugin.Open(\"/nakama/data/modules/backend.so\"): /nakama/data/modules/backend.so: invalid ELF header"
}
```

### Solution

Ensure the platform you are compiling the Go server runtime module for matches the platform that the Nakama binary was compiled for.

## Go Runtime Version Mismatch

### Problem

You will encounter an issue when attempting to load a server runtime Go module that has been compiled using a different version of the Go Runtime than the Nakama binary itself.
For example, you cannot use a Go module that has been compiled with Go version 1.14 together with a Nakama binary that has been compiled with Go version 1.15.


```json
{
  "level":"error",
  "ts":"...",
  "caller":"...",
  "msg":"Could not open Go module",
  "path":"/nakama/data/modules/backend.so",
  "error":"plugin.Open(\"/nakama/data/modules/backend\"): plugin was built with a different version of package runtime/internal/sys"
}
```

### Solution

Ensure you compile your Go server runtime module with the same version of Go as the Nakama binary was compiled with. You can make sure this is the case by using the same tag version number of the [nakama](https://hub.docker.com/r/heroiclabs/nakama) and [nakama-pluginbuilder](https://hub.docker.com/r/heroiclabs/nakama-pluginbuilder) Docker images in your `Dockerfile`.

## Go Dependency Version Mismatch

### Problem

Any dependency/package you use, either directly or indirectly (transitive dependencies), that are also used by the Nakama binary, must be the same version. For example, if you are using version `3.3.0` of the `github.com/gofrs/uuid` package in your server runtime module and the Nakama binary is using the `4.0.0` version, then you will encounter a dependency version mismatch error.

```json
{
  "level":"fatal",
  "ts":"...",
  "caller":"...",
  "msg":"Failed initializing runtime modules",
  "error":"plugin.Open(\"/nakama/data/modules/backend\"): plugin was built with a different version of package go.uber.org/zap/buffer"
}
```

### Solution

Ensure you are using the same dependency/package versions as the Nakama binary you intend to run your server runtime module on.

Where the mismatch occurs with a direct dependency, you must ensure you are importing the exact version that is used by Nakama in your `go.mod` file.

In a situation where the dependency mismatch is on an indirect (transitive) dependency, you can ensure you are using the same version by explicitly importing the dependency in your `go.mod` file, then adding a blank import statement using an `_` (underscore) alias to make it a direct dependency as seen in the example below.

```go
import (
  _ "go.uber.org/zap"
)
```

Once you have done this, you can run the `go mod vendor` command to vendor your dependencies again and ensure you are now using the appropriate version.

!!! Note
    It may help you to identify the correct version of a transitive dependency to be pinned in your plugin `go.mod` file by searching Nakama's `vendor/modules.txt`file and look for the package that lead to the error.

This method may take some trial and error as Nakama implements the fail fast methodology whereby the server will throw an error and shutdown at the first error it encounters with your server runtime code. This means that if there are several dependency version mismatches, you may need to fix them one at a time.
