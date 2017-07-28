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

```shell fct_label="Shell"
docker-compose down # top and remove both the Nakama and CockroachDB containers
docker pull heroiclabs/nakama # download the latest Nakama image
docker-compose up # start the containers (both Nakama and CockroachDB) as fresh installss
```

And, similarly, here’s how to update the CockroachDB container:

```shell fct_label="Shell"
docker pull cockroachdb/cockroach # download the latest CockroachDB image
```

If you'd like to explicity run a database schema migration, issue the following command:

```shell fct_label="Shell"
docker run heroiclabs/nakama migrate up
```

## Manual upgrade

You can upgrade one node or your whole cluster to latest Nakama by following the instructions below.

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
