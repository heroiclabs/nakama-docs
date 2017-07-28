# Nakama Commands

Day to day operation of Nakama is straightforward, requiring minimal intervention. There are just three nakama commands that you’ll need.

Running the `nakama` command by itself will start the server with the default configuration. You can [override the configuration](#config-override) used using command line flags.

## `migrate`

The Nakama binary contains the schema and a way to upgrade an existing database schema. When you first run Nakama, you need to setup the database schema that Nakama interacts with. Similarly, when a new Nakama version is released, you need to migrate the data schema to that of the new version.

| Command          | description
| -------          | -----------
| `migrate up`     | Creates and updates the database schema to the latest version required by nakama. By default, the schema is updated sequentially to the latest available.
| `migrate down`   | Downgrades the database schema to the version requested. By default, it downgrades one schema change at a time.
| `migrate redo`   | Downgrades one schema change, and re-applies the change.
| `migrate status` | Provides information on the schema's currently applied to the database, and if there any are unapplied schemas.

| Flags              | description
| -----              | -----------
| `database.address` | Database node to connect to. It should follow the form of `username:password@address:port/dbname` (`postgres://` protocol is appended to the path automatically). Defaults to `root@localhost:26257`.
| `--limit`          | Number of migrations to use when running either up, down, or redo.

## `doctor`

Nakama ships with a built-in diagnostic tool which is particularly useful when you need support or otherwise are looking to diagnose an issue.

Running `nakama doctor` generates a report that details the server's configuration and environment. By default, the diagnostic tool looks for a Nakama node to connect to on the local machine, but this can be changed:

| Flags     | description
| -----     | -----------
| `host`    | The host running the Nakama instance you want to diagnose. Default value is `127.0.0.1`.
| `limit`   | Dashboard port used by nakama. Default value is 7351.

## Config override

Nakama comes with a default configuration which can be overriden by using a [YML Configuration](install-configuration.md) file or by passing command line flags like below:

```shell
nakama --config path/to/config.yml --purchase.apple.password "shared-secret" --database.address root@localhost:26257 --database.address root@machine-2:26257 
```

!!! tip "Configuration Priority"
    Command line flags override options set in a config file. Configuration file overrides default config options.

Have a look at [Configuration documentation](install-configuration.md#server-configuration) for the complete list of configuration flags.
