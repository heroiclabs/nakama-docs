[deployment-digital-ocean]: images/deployment-digital-ocean.png "Digital Ocean Dashboard"

# Deploy on Digital Ocean

Running Nakama in a Digital Ocean droplet is a great way to develop using Nakama without needing to install it locally.

You can install Nakama on your Digital Ocean droplet using the [Docker installation guide](install-docker-quickstart.md) but first there are some things that need to be configured on the droplet.

## Droplet configuration

Your droplet should have the following configuration:

- Linux distribution: **Ubuntu 16.04 x64**
- RAM: **4GB RAM** / 60GB SSD Disk
- Region: To minimise latency, choose a region close to you.
- Add a new SSH Key: **This is important**. Follow [Digital Ocean’s guide](https://www.digitalocean.com/community/tutorials/how-to-use-ssh-keys-with-digitalocean-droplets) if you need help setting up a public key.

!!! warning "SSH Key"
    You must create a new SSH key and store the key safely. This is the way you'll login to your server.

## Accessing the droplet

Once the droplet is running, make a note of its IP address from within your Digital Ocean console.

![Digital Ocean Dashboard][deployment-digital-ocean]

For the next steps, you need to SSH into the droplet. There isn’t a need for username/password as you’ve pre-authorized yourself using a public key in the previous step.

=== "Shell"
	```shell
	ssh root@<IP ADDRESS>
	# you are now connected to the droplet through an SSH session.
	# you can type `exit` to close the session.
	```

#### Creating a user to run Nakama
You should create a separate user, with sudo privileges, to run Nakama. You’ll see how in this guide.

#### Installing Docker Compose
To prepare for installing Nakama, you’ll need to install both Docker and Docker Compose on your droplet.

The most straightforward way to do that is to follow Digital Ocean’s own guides:

1. [Install Docker on your droplet](https://www.digitalocean.com/community/tutorials/how-to-install-and-use-docker-on-ubuntu-16-04)
2. [Install Docker Compose on your droplet](https://www.digitalocean.com/community/tutorials/how-to-install-docker-compose-on-ubuntu-16-04)

## Running Nakama

Now that you’ve installed Docker and Docker Compose you can follow our guide to [installing Nakama using Docker](install-docker-quickstart.md).
