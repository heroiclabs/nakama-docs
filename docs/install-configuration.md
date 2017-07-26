# Configuration

A YAML configuration file configures many aspects of how your Nakama server runs. You can run Nakama without specifying a configuration file and rely on the the default settings instead.

## Specifying a config file
You can specify a configuration file at run-time using --config flag.

```shell fct_label="Shell"
nakama --config my-special-config.yml
```

If you are running Nakama via Docker-Compose, you'll need to bind a folder in your machine so that it's available in Docker. Follow this guide to [setup folder binding](install-docker-quickstart.md#data).

## Common properties

There are a few configuration properties that need to be changed in most environments. The full list of configurations is at the [bottom of the page](#server-configuration).

| Parameter                | Description
| ---------                | -----------
| `name`                   | Nakama node name (must be unique) - It will default to `nakama-xxxx` where `xxxx` is 4 random characters.
| `data_dir`               | An absolute path to a writeable folder where Nakama will store its data, including logs. Default value is the working directory that Nakama was started on.
| `runtime.path`           | Path of modules to scan and load. Defaults to `data_dir/modules`.
| `database.address`       | List of database nodes to connect to. It should follow the form of `username:password@address:port/dbname` (`postgres://` protocol is appended to the path automatically). Defaults to `root@localhost:26257`.
| `socket.server_key`      | Server key to use to establish a connection to the server. Default value is `defaultkey`.
| `session.encryption_key` | The encryption key used to produce the client token. Default value is `defaultencryptionkey`.
| `runtime.http_key`       | Key is used to protect the server's runtime HTTP invocations. Default value is `defaultkey`.

!!! warning "Production settings"
    You must change the values of **`socket.server_key`**, **`session.encryption_key`** and **`runtime.http_key`** before you deploy Nakama to a live production environment. 

    Follow the [production settings deployment guide](deployment-production-settings.md) for more information.

## Server Configuration

Nakama has various configuration options to make it as flexible as possible for various use cases and deployment environments.

Nakama ships with sane default values for all config options, therefore you'll only need to override a subset of the options. You can also setup your own config file, and furthermore override the values in the config file via command-line flags. For instance, to override Runtime Path:

```shell fct_label="Shell"
nakama --runtime.path /tmp/my-modules
```

If fields are not specific, default values will be used. For more information on how to override flags, have a look at the [server command-line](fundemental-server-cli.md) page.

!!! tip "Override configuration"
    Every configuration option can set from a config file, as a command line flag or both where the command-line argument takes precedence and will override the configuration values.

| Parameter   | Flag       | Description
| ---------   | ----       | -----------
| `name`      | `name`     | Nakama node name (must be unique) - It will default to `nakama-xxxx` where `xxxx` is 4 random characters.  This name is also used in the log files.
| `data_dir`  | `data_dir` | An absolute path to a writeable folder where Nakama will store its data, including logs. Default value is the working directory that Nakama was started on.

### Log

Nakama produces logs in JSON format so various systems can interact with the logs. By default they are written to log files inside `data_dir/logs` folder.

| Parameter  | Flag          | Description
| ---------  | ----          | -----------
| `stdout`   | `log.stdout`  | Redirect logs to console standard output. The log file will no longer be used. Default is `false`.
| `verbose`  | `log.verbose` | Turn on verbose logging. You'll see a lot more logs including debug-level information. This is useful for debugging purposes. Default is `false`.

### Database

Nakama requires a CockroachDB server instance to be available. Nakama creates and manages its own database called `nakama` within the CockroachDB database.

| Parameter              | Flag                             | Description
| ---------              | ----                             | -----------
| `address`              | `database.address`               | List of database nodes to connect to. It should follow the form of `username:password@address:port/dbname` (`postgres://` protocol is appended to the path automatically). Defaults to `root@localhost:26257`.
| `conn_max_lifetime_ms` | `database.conn_max_lifetime_ms`  | Time in milliseconds to reuse a database connection before the connection is killed and a new one is created.. Default value is 60000.
| `max_open_conns`       | `database.max_open_conns`        | Maximum number of allowed open connections to the database. Default value is 0 (no limit).
| `max_idle_conns`       | `database.max_idle_conns`        | Maximum number of allowed open but unused connections to the database. Default value is 0 (no limit).

!!! tip "Database addresses"
    You can pass in multiple database addresses to Nakama via command like:

    ```
    nakama --database.address "root@db1:26257" --database.address "root@db2:26257"
    ```

### Runtime

Options related to Lua-based runtime engine.

| Parameter   | Flag               | Description
| ---------   | ----               | -----------
| `env`       | _N/A_              | List of Key-Value properties that are exposed to the Runtime scripts as environment variables.
| `path`      | `runtime.path`     | Path of modules for the server to scan and load at startup. Default value is `data_dir/modules`.
| `http_key`  | `runtime.http_key` | A key used to authenticate HTTP Runtime invocations. Default value is `defaultkey`.

!!! warning "Important"
    You must change `http_key` before going live with your app!

### Socket

Options related to connection socket and transport protocol between the server and clients.

| Parameter                 | Flag                            | Description
| ---------                 | ----                            | -----------
| `server_key`              | `socket.server_key`             | Server key to use to establish a connection to the server. Default value is `defaultkey`.
| `port`                    | `socket.port`                   | The port for accepting connections from the client, listening on all interfaces. Default value is 7350.
| `max_message_size_bytes`  | `socket.max_message_size_bytes` | Maximum amount of data in bytes allowed to be read from the client socket per message. Default value is 1024.
| `write_wait_ms`           | `socket.write_wait_ms`          | Time in milliseconds to wait for an ack from the client when writing data. Default value is 5000.
| `pong_wait_ms`            | `socket.pong_wait_ms`           | Time in milliseconds to wait for a pong message from the client after sending a ping. Default value is 10000.
| `ping_period_ms`          | `socket.ping_period_ms`         | Time in milliseconds to wait between client ping messages. This value must be less than the `pong_wait_ms`. Default value is 8000.

!!! warning "Important"
    You must change `server_key` before going live with your app!

### Session

You can change configuration options related to each user session, such as the encryption key used to create the token.

| Parameter         | Flag                      | Description
| ---------         | ----                      | -----------
| `encryption_key`  | `session.encryption_key`  | The encryption key used to produce the client token. Default value is `defaultencryptionkey`.
| `token_expiry_ms` | `session.token_expiry_ms` | Token expiry in milliseconds. Default value is 60000.

!!! warning "Important"
    You must change `encryption_key` before going live with your app!

### Purchase

Nakama can verify in-app purchases by connecting to various stores and keep a ledger of valid purchases.

#### Apple

Apple In-App Purchase configuration

| Parameter    | Flag                  | Description
| ---------    | ----                  | -----------
| `password`   | `apple.password`      | Your application's shared secret.
| `production` | `apple.production`    | The order in which the reciept verification server will be contacted. Set `false` for test apps and `true` for live apps. Default is `false`.
| `timeout_ms` | `apple.timeout_ms`    | Connection timeout to connect to Apple services. Default value is 1500.

#### Google

Google In-App Purchase configuration

| Parameter          | Flag                       | Description
| ---------          | ----                       | -----------
| `package`          | `google.package`           | The package name the app is published under, such as `com.myapp.testapp`.
| `service_key_file` | `google.service_key_file`  | Absolute file path to the service key JSON file.
| `timeout_ms`       | `google..timeout_ms`       | Connection timeout to connect to Google services. Default value is 1500.

### Social

Nakama can connect to various social networks to fetch user information. It can also act as a notification center for delivering and persisting notifications.

#### Notification

| Parameter   | Flag                     | Description
| ---------   | ----                     | -----------
| `expiry_ms` | `notification.expiry_ms` | Set notification expiry in milliseconds. Default value is 86400000 (1 day).

#### Steam
Configure Steam network settings. Facebook, Google and GameCenter don't require any server settings.

| Parameter       | Flag                  | Description
| ---------       | ----                  | -----------
| `publisher_key` | `steam.publisher_key` | Steam Publisher Key.
| `app_id`        | `steam.app_id`        | Steam App ID.

### Dashboard

This section defined the configuration related for the embedded Dashboard.

| Parameter | Flag             | Description
| --------- | ----             | -----------
| `port`    | `dashboard.port` | The port for accepting connections to the dashboard, listening on all interfaces. Default value is 7351.






