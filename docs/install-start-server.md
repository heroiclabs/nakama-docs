# Start server

You can install Nakama as a binary or with Docker. If you plan to use Docker with development or deployment have a look at the [Docker Quickstart](install-docker-quickstart.md#running-nakama).

With the server downloaded or accessible you can start it. You must first start the database server.

## Start database

You can start the database server with this command:

```sh
cockroach start --background --insecure path="./cdb-store1/"
```

This will start CockroachDB in the background, on port `26257` and sets the data directory for the database to the folder "cdb-store1" within the current working directory of the shell environment. For more examples have a look at the [database documentation](https://www.cockroachlabs.com/docs/stable/start-a-local-cluster.html).

## Start Nakama

Before you can start the server you must setup the database schema. The schema definition is bundled within the server and can be executed with this command:

```sh
nakama migrate up
```

If you've configured your database to run with specific connection settings have a look at how to configure the [migrate command](install-server-cli.md#migrate).

When the command runs you'll see logs output to the shell console.

```
{"level":"info","ts":"<sometimestamp>","msg":"Database connection","db":"root@localhost:26257"}
{"level":"info","ts":"<sometimestamp>","msg":"Database information","version":"CockroachDB CCL v1.1.5"}
{"level":"info","ts":"<sometimestamp>","msg":"Using existing database","name":"nakama"}
{"level":"info","ts":"<sometimestamp>","msg":"Successfully applied migration","count":1}
```

The logs indicate how many migrations were run to update the schema definition in the database to the latest version with the release of the server. You will only need to migrate the server once with each new server release. This command does __not__ need to be run before each server start.

You can now start the [server](install-binary.md).

```sh
# start the server and output logs to the terminal
nakama --log.stdout
```

The logs output from the server will tell you:

* What version of the server is started.
* What IP address and port number the server can be reached on by clients.
* What the address looks like to reach the developer console available with the server. The default address is [https://localhost:7352](https://localhost:7352).

!!! info "Startup messages"
    A few of the first log messages output by the server might start with "WARNING". These indicate the server is started with API keys which are default values and must be changed before you deploy in production.

The [configuration](install-configuration.md) section covers all the various server options which can be tweaked to specialize the server at startup. A few of the most common configuration flags are covered below.

### Database connection

By default the server will connect to the database on a local address with the default port number. You can configure a custom address and database name:

```sh
# migrate command
nakama migrate up --database.address "username@database-host:port/database-name"
# startup command
nakama --database.address "username@database-host:port/database-name"
```

If you've setup CockroachDB in secure mode you can pass in certificate information:

```sh
nakama --database.address "username@database-host:port/database-name?sslcert=path/to/cert.der&sslkey=path/to/somekey.key.pk8&sslmode=require"
```

### Server name

The server uses a unique random name each time it is started. This is essential for identifying the server when running multiple instances as part of a cluster or on a single node. You can set the server name for logs and other output to be same each time it's run.

```sh
nakama --name "nakama1"
```

## Data directory

The `data` directory of Nakama is where Nakama stores various working files. This includes a log folder, Lua modules folder, and more. By default, the `data` directory is created in your current working directory of the shell console.

### Logs

Nakama writes all logs to a file in the data directory. The log file name is the name of the server instance.

By default Nakama logs messages with level "INFO", "WARN" and "ERROR". However for development purposes it can be useful to turn up the log level:

```sh
nakama --log.verbose
```

Logs can be redirected to standard output in the shell console if you prefer. They will not be written to a file with this flag set.

```sh
nakama --log.stdout
```

### Lua modules

At startup the server walks the modules directory for all files which end in ".lua". These modules are then parsed and evaluated before they are cached as VM instructions for the server to run. You'll see a list of modules found by the server in the log output.

The default location of modules is the "modules" folder within the data directory ("./data/modules"). You can override this with your own location for Lua modules.

```sh
nakama --runtime.path "path/to/modules"
```

## Start/join cluster

!!! tip "Nakama Enterprise Only"
    The following commands are used with Nakama Enterprise. The enterprise version offers multi-server scale out for realtime chat, multiplayer, presence events, notifications, streams, and session management. You can start a cluster locally on your development machine with an [enterprise license](https://heroiclabs.com/nakama-enterprise). The [Managed Cloud](https://heroiclabs.com/managed-cloud) service we offer already runs our enterprise version.

The clustered version of Nakama server builds in gossip, node awareness, state replication, and multi-node message routing. It uses state of the art distributed systems features to offer a simple scale out model.

An enterprise server will join a cluster if one is already known to the instance it joins or create a new cluster.

```sh
nakama --log.stdout --name "nakama1"
```

You can start a second node and join the cluster.

```sh
nakama --log.stdout --name "nakama2" --cluster.join "localhost:7352"
```

If you'd like to run both instances on the same machine you must also give each server unique ports to listen on.

```sh
# node 1
nakama --name "nakama1"
# node 2
nakama --name "nakama2" --cluster.join "localhost:7352" --socket.port 7360 --dashboard.port 7361 --cluster.gossip_bindport 7362 --cluster.rpc_port 7363
```
