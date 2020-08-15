# Binary install

To start developing with Nakama, you’ll first need to install it on your development machine. It’s straightforward and takes just a few minutes. In this guide we’re going to focus on installing your Nakama development instance using the binary executable.

!!! summary "Recommended Approach"
    [Docker](install-docker-quickstart.md) is the quickest way to download and get started with Nakama for development purposes. For production settings, we recommend that you install Nakama as a binary to ensure all system resources are available to Nakama.

## Requirement
There are a few things you’ll need to know or have to hand before you install Nakama:

- Operating system: Windows 7 64-bit or above, MacOS 10.9 or above, or a modern Linux.
- Architecture: X86_64 (64bit) processor architecture
- Dependent software: Nakama relies on [CockroachDB](https://cockroachlabs.com).

## Install CockroachDB

Nakama relies on CockroachDB as the main and only database. Nakama uses CockroachDB to store server configuration, user data, chat messages and more persistent data.

You'll first need to install CockroachDB. Follow [this guide](https://www.cockroachlabs.com/docs/stable/install-cockroachdb.html) to install CockroachDB on your machine before installing Nakama.

!!! note "CockroachDB Alternative"
    Nakama unofficially supports [PostgreSQL](https://www.postgresql.org/download/) for environments where CockroachDB is not available.

    For production settings, Nakama should be used with CockroachDB as queries are optimised for the way data is stored on the disk.

## Install Nakama on MacOS

You’ll need to be running MacOS 10.9 (Mavericks) or greater to run Nakama.

<!--
### Homebrew

Homebrew is a community-developed package manager for MacOS. If you’ve used `apt` or `yum` on Linux systems then you’ll find Homebrew to be familiar.

If this is your first time using Homebrew, take a look at [their website](http://brew.sh/) for installation instructions. Once you’ve installed Homebrew, follow the instructions below.

=== "Shell"
	```shell
	# run our brew recipe to download the Nakama code and build the binary
	brew install https://raw.githubusercontent.com/heroiclabs/nakama/master/install/local/nakama.rb
	```

Now you have Nakama running and you’re ready to start developing.
-->

### Without Homebrew

Installing the binaries directly rather than via Homebrew allows you the flexibility of placing the server in your prefered workspace. However, installing through [Homebrew](#homebrew) is the simplest and easiest – including for updates – as everything is handled using the Homebrew workflow.

1\. Download the latest [Nakama tarball for MacOS](https://github.com/heroiclabs/nakama/releases/latest).

2\. Then extract the binary:

=== "Shell"
	```shell
	# replace the X.X.X with the version number you have downloaded
	tar xfz nakama-X.X.X-darwin-amd64.tar.gz
	```

3\. Add the directory containing the binary to your `PATH`. This makes it easy to execute Nakama commands from your terminal.

=== "Shell"
	```shell
	cp -i nakama /usr/local/bin
	```

You may come across a permissions error. If you’re happy to perform the action with root permissions then prefix the command with `sudo`.

4\. Migrate the database schema and then start Nakama

=== "Shell"
	```shell
	# migrate schema
	nakama migrate up
	# start the server
	nakama
	```

## Install Nakama on Windows

Nakama is also available as a [Windows binary](https://github.com/heroiclabs/nakama/releases/latest). However, [Docker](install-docker-quickstart.md) is the recommended way to install CockroachDB and Nakama on Windows.

1. [Download the binary](https://github.com/heroiclabs/nakama/releases/latest) and decompress the downloaded file.
2. Open a Powershell terminal and navigate to the folder you downloaded Nakama into.
3. Migrate the database schema and then start Nakama

=== "Powershell"
	```shell
	# migrate schema
	nakama.exe migrate up
	# start the server
	nakama.exe
	```

## Install Nakama on Linux

Using [Docker](install-docker-quickstart.md) - This is the recommended approach. We will soon provide packages for various Linux package managers to ease the installation procedure. [Let us know](mailto:support@heroiclabs.com) if you have requests for specific package managers.

First you’ll need to create a suitable directory to install Nakama. To some extent its location will depend on your particular flavor of Linux and your own preferences.

1\. [Download the binary](https://github.com/heroiclabs/nakama/releases/latest) and decompress the downloaded file.

2\. Then extract the binary:

=== "Shell"
	```shell
	# replace the X.X.X with the version number you have downloaded
	tar xfz nakama-X.X.X-linux-amd64.tar.gz
	```

3\. Add the directory containing the binary to your `PATH`. This makes it easy to execute Nakama commands from your terminal.

=== "Shell"
	```shell
	cp -i nakama /usr/local/bin
	```

4\. Migrate the database schema and then start Nakama

=== "Shell"
	```shell
	# migrate schema
	nakama migrate up
	# start the server
	nakama
	```

You may come across a permissions error. If you’re happy to perform the action with root permissions then prefix the command with `sudo`.

### systemd

If you prefer to run Nakama as a service, and you’re running a distro that uses systemd, you can optionally use the following script.

!!! Note
    You’ll need to update the paths within the systemd configuration.

1\. Create the service file: `/usr/lib/systemd/system/nakama.service`

=== "nakama.service"
	```ini
	[Unit]
	Description=Nakama server
	
	[Service]
	ExecStart=/path/to/nakama --config /path/to/nakama/config.yml
	KillMode=process
	
	[Install]
	WantedBy=multi-user.target
	```

2\. Update file permission so it's readable by the `systemd` daemon process

=== "Shell"
	```shell
	sudo chmod 644 /usr/lib/systemd/system/nakama.service
	```

3\. Enable and run the service

=== "Shell"
	```shell
	sudo systemctl enable nakama
	sudo systemctl start nakama
	```


