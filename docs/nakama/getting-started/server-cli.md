# Nakama Commands

Day to day operation of Nakama is straightforward, requiring minimal intervention. There are just a few Nakama commands available, and only three to keep in mind:

* [nakama](#nakama)
* [migrate](#migrate)
* [config override](#config)

## `nakama`

Running the `nakama` command by itself will start the server with the default configuration. You can [override the configuration](#config) default (and configuration file parameters) using command line flags.

## `migrate`

The Nakama binary contains the schema and a way to upgrade an existing database schema. When you first run Nakama, you need to setup the database schema that Nakama interacts with. Similarly, when a new Nakama version is released, you need to migrate the data schema to that of the new version.

| Command          | Description
| -------          | -----------
| `migrate up`     | Creates and updates the database schema to the latest version required by Nakama. By default, the schema is updated sequentially to the latest available.
| `migrate down`   | Downgrades the database schema to the version requested. By default, it downgrades one schema change at a time.
| `migrate redo`   | Downgrades one schema change, and re-applies the change.
| `migrate status` | Provides information on the schemas currently applied to the database, and if there any are unapplied schemas.

| Flags              | description
| -----              | -----------
| `database.address` | Database node to connect to. It should follow the form of `username:password@address:port/dbname` (`postgres://` protocol is appended to the path automatically). Defaults to `root@localhost:26257`.
| `--limit`          | Number of migrations to use when running either `up`, `down`, or `redo`.

<!--
## `doctor`

Nakama ships with a built-in diagnostic tool which is particularly useful when you need support or otherwise are looking to diagnose an issue.

Running `nakama doctor` generates a report that details the server's configuration and environment. By default, the diagnostic tool looks for a Nakama node to connect to on the local machine, but this can be changed:

| Flags     | description
| -----     | -----------
| `host`    | The host running the Nakama instance you want to diagnose. Default value is `127.0.0.1`.
| `limit`   | Dashboard port used by nakama. Default value is 7351.
-->
## `config`

Nakama comes with a default configuration which can be overriden by using a [YML Configuration](configuration.md) file or by passing command line flags like below:

```shell
nakama --config path/to/config.yml --database.address root@localhost:26257 --database.address root@machine-2:26257
```

!!! tip "Configuration Priority"
    Command line flags override options set in a config file. Configuration file overrides default config options.

Have a look at [Configuration documentation](configuration.md#server-configuration) for the complete list of configuration flags.

## `version`

Use the `version` command to see the semantic version of your Nakama server instance. For example:

```shell
nakama --version
3.3.0+83fc6fbc
```

## `check`

The `check` command will parse any command line arguments to look up the runtime path, where the server will scan for Lua and Go library files.

## `help`

Use the `help` command to display all available configuration flags. These are also available on the [Configurations](configuration.md) page.

```shell
nakama --help
```
