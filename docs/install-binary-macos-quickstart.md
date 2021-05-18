# Install Nakama Binary for macOS

This tutorial will show you how to setup and run Nakama binary and its prerequisites on macOS.

The binary setup allows you to install and manage Nakama without Docker but it requires extra steps to setup and manage:

* Installing CockroachDB or PostgreSQL
* Manually applying database migrations
* Manually upgrading Nakama and its database to newer versions
*	Configuring services or manually starting Nakama and its database

!!! summary "Recommended Approach"
	[Docker](install-docker-quickstart.md) is the quickest way to download and start developing with Nakama.

## Prerequisites

### Operating system

Before proceeding ensure that you are running macOS 10.9 (Mavericks) or higher with a X86_64 (64bit) processor architecture.

Next, install a supported database engine.

### CockroachDB

Nakama officially supports CockroachDB v20.0 or higher, with queries optimised for its storage engine.

Install CockroachDB via one the [official supported methods](https://www.cockroachlabs.com/docs/stable/install-cockroachdb.html):

* Homebrew
* Binary
* Build from source

### PostgreSQL

Nakama unofficially supports PostgreSQL 9.6 or higher for development environments only.

Install PostgreSQL via:

* [Official PostgreSQL macOS packages](https://www.postgresql.org/download/macosx/)
*	[Postgres.app](https://postgresapp.com/)
* [Homebrew](https://wiki.postgresql.org/wiki/Homebrew)

## Download Nakama

Get the latest binary release of Nakama server for macOS:

1. Download a release from the Nakama GitHub repo [releases page](https://github.com/heroiclabs/nakama/releases)
2. Extract the archive, optionally rename and move the folder to a suitable location

## Running Nakama

Before starting Nakama server you will need to run:

1. Your chosen database, CockroachDB or PostgreSQL.
2. Nakama database migrations if it is your first time running Nakama or you have upgraded Nakama versions.

### Nakama migrations

If this is your first time running Nakama or you have upgraded Nakama versions, you may need to run Nakama database migrations. If you are unsure, running migrations again is safe.

If you are running CockroachDB with the default configuration, running migrations is straightforward with no additional options necessary.

If you are using PosgreSQL you will need to supply your database server address, user and password.

To run Nakama migrations, navigate to your Nakama directory and run the following:

=== "CockroachDB"
	```shell
	./nakama migrate up
	```
=== "PostgreSQL"
	```shell
	./nakama migrate up --database.address postgres:password@127.0.0.1:5432
	```

### Nakama server

To start Nakama server with CockroachDB or PostgreSQL, navigate to your Nakama directory and run the following:

=== "CockroachDB"
	```shell
	./nakama
	```
=== "PostgreSQL"
	```shell
	./nakama --database.address postgres:password@127.0.0.1:5432
	```

!!! note "PostgreSQL"
	You will need to provide the same `database.address` value used for running [Nakama PostgreSQL database migrations](#nakama-migrations):

## Nakama Console

You can also access the Nakama Console by navigating your browser to `127.0.0.1:7351`:

![Nakama console](images/install/docker/nakama-console.png)

!!! note "Note"
    When prompted to login, the default credentials are `admin:password`. These can be changed via configuration file or command-line flags.

## Configuration file

There are many [configuration options](install-configuration.md) available that you can customize for your Nakama server.

## Next steps

With your Nakama server now up and running with the desired configuration, you can get started with your preferred client SDK:

* [.NET/Unity](unity-client-guide.md)
* [JavaScript](javascript-client-guide.md)
* [Godot](godot-client-guide.md)
* [Defold](defold-client-guide.md)
* [Java/Android client](android-java-client-guide.md)
* [C++](cpp-client-guide.md)
* [Unreal](unreal-client-guide.md)
* [Cocos2d-x C++](cocos2d-x-client-guide.md)
* [Cocos2d-x JS](cocos2d-x-js-client-guide.md)
