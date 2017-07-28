# Docker quickstart

To start developing with Nakama, you’ll first need to install it on your development machine. It’s straightforward and takes just a few minutes. In this guide we’re going to focus on installing your Nakama development instance using Docker.

!!! summary "Recommended Approach"
    Docker is the quickest way to download and get started with Nakama for development purposes. For production settings, we recommend that you install Nakama as a [binary](install-binary.md) to ensure all system resources are available to Nakama.

There is a single, minimal Nakama image that contains the Nakama binary. The basic format is:

```shell fct_label="Shell"
docker run heroiclabs/nakama <command> [options]
```

Installing Nakama using Docker is ideal for a few reasons, including:

- you install to a pristine environment

- you get everything you need in one go, including CockroachDB

- you can take snapshots, re-install and remove Nakama without affecting your primary operating system.

- It also means that the installation instructions are the same whether your development machine runs Windows, MacOS and Linux.

## What is Docker?

If you’re new to Docker, then here’s what you need to know: Docker is an open soure containerization tool that lets you create multiple distinct Linux environments, each separate from the other.

In a Docker container you run a suite of tools to do a particular job; in this case we’ll have one container running Nakama and another running CockroachDB. You can think of Docker containers as lightweight virtual machines.

- Follow this [guide](https://www.docker.com/community-edition), if you are trying to install Docker on Mac, Linux and Windows 10 Pro edition.

- [Docker Toolbox](https://www.docker.com/products/docker-toolbox) is needed, if you are installing Docker on Windows 7, 8 or 10 Home (non-Pro) editions.

- Use the [Docker Store](https://store.docker.com/search?offering=community&q=&type=edition) to find the right version of Docker Community Edition for your environment.

## Connecting the Nakama client

Once Nakama is running via Docker, use the following connection detail to configure your client to connect to the server:

**Host**: `127.0.0.1` (or `localhost`)    
**Port** : `7350`     
**SSL**: `False`    
**Server Key**: `defaultkey`     

In the Unity client, you can create an `NClient` like this:
```csharp fct_label="Unity"
INClient client = new NClient.Builder("defaultkey")
    .Host("127.0.0.1")
    .Port(7350)
    .SSL(false)
    .Build();
```

## Running Nakama

You can run Nakama and Cockroach without using Docker-Compose. This will mean you have greater control over how they are started, and various data volumes options but in return, you'll have to configure the two containers:

```shell fct_label="Shell"
# Let's pull and start CockroachDB
docker run --name=db cockroachdb/cockroach start --insecure
# Let's pull and migrate the database
docker run --link=db heroiclabs/nakama migrate up --db root@db:26257
# start Nakama server
docker run --link=db -p 7350:7350 heroiclabs/nakama --db root@db:26257
```

## Using Docker-Compose

Docker Compose simplifies running more than one Docker container in conjunction. For Nakama, we’ll need two containers: one for Nakama itself and one for the database it relies on, CockroachDB.

You can choose to configure the Nakama and CockroachDB containers without Docker Compose but we do not recommend it when you’re starting out.

Docker Compose uses YAML configuration files to declare which containers to use and how they should work together.

1\. Let’s start by downloading the [Nakama Docker Compose file](https://raw.githubusercontent.com/heroiclabs/nakama/master/install/docker/docker-compose.yml):

```shell fct_label="Shell"
wget https://raw.githubusercontent.com/heroiclabs/nakama/master/install/docker/docker-compose.yml
```

!!! warning "Windows users"
    If you are trying to run Nakama via Docker-Compose on Windows, you'll need to make a small change to the downloaded `docker-compose.yml` file. Follow this [instruction](#data) to bind the correct path.

This will download `docker-compose.yml` to your current working directory.

2\. Next, we’ll ask Docker Compose to follow the instructions in the file we just downloaded:

```shell fct_label="Shell"
docker-compose -f docker-compose.yml up
```

Docker Compose will download the latest CockroachDB and Nakama images published on Docker Hub.

3\. You now have both CockroachDB and Nakama running on your machine, available at `127.0.0.1:26257` and `127.0.0.1:7350` respectively.

### Data

Docker containers are ephemeral by design: when you remove the container, you lose the data stored inside them.

For development purposes, we suggest that you bind a folder in the local machine's filesystem to the Docker file system. The easiest way to achieve this is by editing the `docker-compose.yml` file:

``` yml hl_lines="9" fct_label="docker-compose.yml"
...
nakama:
  image: heroiclabs/nakama:latest
  entrypoint:
    - /bin/bash
    - -ecx
    - /nakama/nakama migrate up --db "root@cockroachdb:26257" && /nakama/nakama --db "root@cockroachdb:26257"
  volumes:
    - ./nakama/data:/nakama/data # Edit this line
  expose:
    - "7350"
...
```

- On Mac and Linux systems, the path highlighted above will create a folder called `nakama` in the same directory as where you are running `docker-compose` from.
- On Windows, you'll need to update the path above so that Docker can bind the folder properly. A valid value can look like this: `C:/users/<username>/projects/docker`.

!!! warning "Drive Binding on Windows"
    Docker will complain about an unshared Drive if the path above is not changed or is not available. The error looks like this:

    `ERROR: for bin_nakama_1 Cannot create container for service nakama: Drive has not been shared`

    Make sure to change the line highlighted above to the correct path and restart Nakama.

You can put your Lua scripts in the `data/modules` directory and restart Nakama using `docker-compose --restart`.

### Logs
Logs generated within the containers are printed to the console as part of the docker-compose output, and you can access them with `docker-compose logs` from within the same the directory as the `docker-compose.yml` file.

### Stopping containers
If you need to temporarily pause the Docker containers, without losing the state of those containers, you have two options:

- In the terminal where docker-compose is currently running, hit CTRL-C.
- Or run `docker-compose stop` in the same directory as docker-compose.yml and all containers will be shut down gracefully.

You can re-activate them by running `docker-compose up`.

To stop the containers and purge all stored data, run `docker-compose down`.
