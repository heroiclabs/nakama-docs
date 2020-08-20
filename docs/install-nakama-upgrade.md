# Upgrade Nakama

When upgrading to a new version of Nakama, you need to do three things before you run the newer version:

- Check the [Release Notes](https://github.com/heroiclabs/nakama/blob/master/CHANGELOG.md), in case that version has specific upgrade requirements.
- Migrate your data to the format supported by the new version of Nakama.
- Ensure that the client version you’re running is compatible with the server deployed.

## Docker

As new versions of Nakama, or CockroachDB, become available you might want to upgrade. Docker makes upgrading easy, as all you need to do is pull down a new version of the container image.

!!! note "Schema migration"
    When upgrading to a new version of Nakama, the process in the `docker-compose.yml` performs a migration of your data to the new database schema automatically.

You can pull down the most recent Nakama image and allow Docker Compose to perform the data migration.

=== "Shell"
	```shell
	docker-compose down # top and remove both the Nakama and CockroachDB containers
	docker pull heroiclabs/nakama # download the latest Nakama image
	docker-compose up # start the containers (both Nakama and CockroachDB) as fresh installss
	```

And, similarly, here’s how to update the CockroachDB container:

=== "Shell"
	```shell
	docker pull cockroachdb/cockroach # download the latest CockroachDB image
	```

If you'd like to explicity run a database schema migration, issue the following command:

=== "Shell"
	```shell
	docker run heroiclabs/nakama migrate up
	```

## Manual upgrade

You can upgrade one node or your whole cluster to the latest version of Nakama by following the instructions below.

For each node in the cluster:

1\. Stop Nakama. If you are using `systemd` on a Linux setup, the command is:

```shell
sudo systemctl stop nakama
```

2\. Back-up CockroachDB

```shell
cockroach dump nakama > mydatabackup.sql --insecure
```

3\. Download the [latest release](https://github.com/heroiclabs/nakama/releases/latest) and replace the `nakama` binary with the newer download.
4\. Upgrade the database schema with the following command:

```shell
nakama migrate up
```

!!! tip "Downgrade"
    To downgrade Nakama you can follow the same procedure, but replace the migration command with the following:

```
nakama migrate down --limit 1
```

5\. Start Nakama and verify that the upgraded version is running.

```shell
sudo systemctl start nakama
```

## Upgrade from Nakama 1

Nakama 2 is not backwards compatible with Nakama 1 due to database schema changes and wire protocol changes. This means you'll need to manually upgrade the database before you can run Nakama 2.

!!! warning "Beware. Backup your data now!"
    The following commands assumes that you have already backed up your data and understand that you'll need to migrate your data back into the database manually yourself after the upgrade procedure is completed.

### Docker-compose

If you've used Nakama 1 with Docker Compose then you'll need to delete the database volume and start afresh.

This will remove the Docker storage volumes which contain the database files and you can start the servers with Docker and a new database.

```sh
docker-compose -f ./docker-compose.yml down -v
```

Follow this [guide to continue](install-docker-quickstart.md#using-docker-compose).

### Docker

If you've used Nakama 1 using a Docker container then you'll need to delete the database volume and start again:

```sh
docker volume ls
```

Find the relevant volume and delete the database volume. This will delete all database files - tables and records.

```
docker volume rm <volumename>
```

Follow this [guide to continue](install-start-server.md).

### Binaries

You'll need to connect to the database directly and issue the following command:

```sh
cockroach sql --insecure -e "DROP DATABASE nakama CASCADE;"
```

Follow this [guide to continue](install-start-server.md).
