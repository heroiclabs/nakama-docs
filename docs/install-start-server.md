# Start server

Once you've installed Nakama either via Binary install or Docker, it's simple to start a Nakama server instance.

If you are using Docker, have a look at [this section](install-docker-quickstart.md#running-nakama) to run Nakama.

## Start database

To start Nakama, you'll need to start the CockroachDB database. This can be done by simply running the following command:

```sh
cockroach start --background --insecure path="/tmp/cockroach"
```

This will start CockroachDB in the background, on port `26257` and sets the data directory for the database to the temporary directory. For more info, follow [this documentation](https://www.cockroachlabs.com/docs/stable/start-a-local-cluster.html).

!!! warning "Temporary directory"
    In the above example, we are setting the database's directory to the operating system temporary directory.

    The operating system temporary directory automatically gets wiped when the machine is restarted. This may be useful for development but definitely not recommended for production settings.

## Start Nakama

Make sure that you've migrated the database schema before starting Nakama. Have a look at the [`migrate` command](install-server-cli.md#migrate) to learn more.

```sh
nakama migrate up
```

You should see an output like this:

```
{"level":"info","ts":"","msg":"Database connection","db":"root@localhost:26257"}
{"level":"info","ts":"","msg":"Database information","version":"CockroachDB CCL v1.1.2"}
{"level":"info","ts":"","msg":"Using existing database","name":"nakama"}
{"level":"info","ts":"","msg":"Successfully applied migration","count":5}
```

To start Nakama, simply run the [binary](install-binary.md) like the below:

```sh
# start the server and output logs to the terminal
nakama --log.stdout
```

This should produce an output like below:
```
{"level":"warn","ts":"","msg":"WARNING: insecure default parameter value, change this for production!","param":"socket.server_key"}
{"level":"warn","ts":"","msg":"WARNING: insecure default parameter value, change this for production!","param":"session.encryption_key"}
{"level":"warn","ts":"","msg":"WARNING: insecure default parameter value, change this for production!","param":"session.udp_key"}
{"level":"warn","ts":"","msg":"WARNING: insecure default parameter value, change this for production!","param":"runtime.http_key"}
{"level":"info","ts":"","msg":"Nakama starting"}
{"level":"info","ts":"","msg":"Node","name":"nakama","version":"1.4.0","runtime":"go1.9"}
{"level":"info","ts":"","msg":"Data directory","path":"/nakama/data"}
{"level":"info","ts":"","msg":"Database connections","dsns":["root@localhost:26257"]}
{"level":"info","ts":"","msg":"Database information","version":"CockroachDB CCL v1.1.2"}
{"level":"info","ts":"","msg":"Evaluating modules","count":1,"modules":["/nakama/data/modules/pokeapi.lua"]}
{"level":"info","ts":"","msg":"Modules loaded"}
{"level":"warn","ts":"","msg":"Skip initialising Apple in-app purchase provider","reason":"Missing password"}
{"level":"warn","ts":"","msg":"Skip initialising Google in-app purchase provider","reason":"Missing package name"}
{"level":"info","ts":"","msg":"Dashboard","address":"http://localhost:7351"}
{"level":"info","ts":"","msg":"Client","port":7350}
{"level":"info","ts":"","msg":"Startup done"}
```

The server has started and is waiting to serve requests on port `7350`. You should be able to access the embedded dashboard via on port `7351` like this:
[https://localhost:7351](https://localhost:7351).

Follow the [configuration](install-configuration.md) documentations to resolve the warning messages by setting various options available either via command-line flags or via a config file.

### Connecting to database

By default, Nakama tries to connect to the database running on the local machine. If this is not the case, you'll need to tell Nakama the address of the database:

```sh
# Don't forget to migrate the database schema first!
nakama migrate up --database.address "username@database-host:port"
nakama --database.address "username@database-host:port"
```

If you are running the database in the secure mode (unlike above), you'll need to pass the certificate information to Nakama like this:

```sh
nakama --database.address "username@database-host:port/?sslcert=path/to/cert.der&sslkey=path/to/somekey.key.pk8&sslmode=require"
```

## Server name

The server produces a unique random name each time it is ran. This is essential for identifying the server when running multiple instances as a cluster on a single node.

If you'd like to set the instance name manually, simply pass that in using command line flags when starting Nakama:

```sh
nakama --name "nakama1"
```

## Data directory

The `data` directory of Nakama is where Nakama stores various working files. This includes a log directory, Lua modules directory and more. By default, the `data` directory is created in your current working directory where you started Nakama from.

### Logs

Nakama writes all logs to a file in the data directory. The log file name is the name of the server instance as explained above.

By default Nakama logs messages with level `INFO`, `WARN` and `ERROR`. However for debugging purposes you can turn up the log level:

```sh
nakama --log.verbose
```

If you want to avoid writing the logs to disk (e.g. you use Docker or an orchestration system), simply run Nakama with the following flag:

```sh
nakama --log.stdout
```

### Lua modules

On server start, Nakama scans the Lua modules directory for any files with a `.lua` extension. It will then attempt to parse and evaluate those modules. You should see a list of loaded modules in the server output.

By default the Lua modules directory is in the data directory (`./data/modules`). You can override this via a config flag like below:

```sh
nakama --runtime.path "path/to/modules"
```

You'll see the list of found and loaded modules when the server starts. In the above example, we have a Lua module called `pokeapi.lua` that is found and evaluated during server startup.

## Clustering

!!! tip "Nakama Enterprise Only"
    The following sections is only applicable to Nakama Enterprise version of the Nakama server.

    Nakama is designed to run in production as a highly available cluster. You can start a cluster locally on your development machine if you’re running [Nakama Enterprise](https://heroiclabs.com/nakama-enterprise). In production you can use either Nakama Enterprise or our [Managed Cloud](https://heroiclabs.com/managed-cloud) service.

We've worked very hard to make sure the clustering technology built-in to Nakama Enterprise is the best in class available while making sure that interactions with the server remain as simple as possible.

Nakama Enterprise is able to find nodes in a cluster automatically and join the cluster if one exists. If not, it will create a new cluster and can accept new nodes at any point. 

You can simply start Nakama Enteprise in the same manner as you'd start Nakama Open-source:

```sh
nakama --log.stdout --name "nakama1"
```

You can then start a second Nakama server and join the first server like this:

```sh
nakama --log.stdout --name "nakama2" --cluster.join "localhost:7352"
```

If you are running both instances on the same machine:

```sh
nakama --log.stdout --name "nakama2" --cluster.join "localhost:7352" --socket.port 7360 --dashboard.port 7361 --cluster.gossip_bindport 7362 --cluster.rpc_port 7363
```
