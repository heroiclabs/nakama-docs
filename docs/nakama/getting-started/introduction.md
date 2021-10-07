[nakama_logo]: ../../images/nakama-logo.png "Nakama Logo"

# Nakama Server

![Nakama Logo][nakama_logo]

Nakama is a scalable server for social and realtime games and apps.

With Nakama server you can add user authentication, social networking, storage, and realtime data exchange into your apps and games. It is developed by <a href="https://heroiclabs.com" target="\_blank">Heroic Labs</a> to handle the difficult but essential services that go into all social and realtime games and apps.

The server is designed to run at massive scale. Nakama works as a <a href="https://heroiclabs.com/nakama-enterprise/" target="\_blank">distributed cluster</a> so when your game or app gets featured in the App Store then all you need do is launch a few more instances and you'll handle the extra load with ease.

During development you can run the server on your macOS, Linux, or Windows machine. When it's time to move into production either move into the cloud or use the Heroic Labs <a href="https://heroiclabs.com/managed-cloud/" target="\_blank">Managed cloud</a> service.

## Features

You get to focus on building your project while the server handles all [user accounts](../concepts/user-accounts.md), [social profiles](../concepts/authentication.md#social-providers), [realtime chat](../concepts/realtime-chat.md), [data storage](../concepts/collections.md), [multiplayer matches](../concepts/client-relayed-multiplayer.md), and lots more.

<div style="display: flex">
  <div style="flex: 1; margin: 0 1em 0 0">
    <strong>User accounts</strong>
    <p>Every <a href="./user-accounts/">user</a> is registered and has a profile for other users to find and become friends with or join groups and chat.</p>
  </div>
  <div style="flex: 1">
    <strong>Friends</strong>
    <p><a href="./social-friends/">Friends</a> are a great way to build a social community.</p>
  </div>
</div>

<div style="display: flex">
  <div style="flex: 1; margin: 0 1em 0 0">
    <strong>Groups and Clans</strong>
    <p>A <a href="./social-groups-clans/">group</a> brings together a bunch of users into a small community or team.</p>
  </div>
  <div style="flex: 1">
    <strong>Realtime chat</strong>
    <p>Users can <a href="./social-realtime-chat/">chat</a> with each other 1-on-1, as part of a group, and in chat rooms.</p>
  </div>
</div>

<div style="display: flex">
  <div style="flex: 1; margin: 0 1em 0 0">
    <strong>In-app notifications</strong>
    <p><a href="./social-in-app-notifications/">In-app notifications</a> make it easy to broadcast a message to one or more users.</p>
  </div>
  <div style="flex: 1">
    <strong>Leaderboards</strong>
    <p><a href="./gameplay-leaderboards/">Leaderboards</a> are a great way to add a social and competitive element to any game.</p>
  </div>
</div>

<div style="display: flex">
  <div style="flex: 1; margin: 0 1em 0 0">
    <strong>Matchmaker</strong>
    <p>The <a href="./gameplay-matchmaker/">matchmaker</a> makes it easy in realtime and turn-based games to find active opponents to play against.</p>
  </div>
  <div style="flex: 1">
    <strong>Multiplayer</strong>
    <p>The <a href="./gameplay-multiplayer-realtime/">multiplayer engine</a> makes it easy for users to set up and join matches where they can rapidly exchange data with opponents.</p>
  </div>
</div>

__Server-side code__

The server integrates the Lua programming language as a fast embedded [code runtime](../server-framework/basics.md).

This is useful to run custom logic which isn't running on the device or browser. The code you deploy with the server can be used immediately by clients so you can change behavior on the fly and add new features faster.

## Next steps

Build the next big hit for mobile, console, desktop, and web with Nakama. We have first-class game engine integration including [Unity](../client-libraries/unity-client-guide.md) for ease of development.

To get the most out of Nakama you should follow the rest of this guide. And if you need help <a href="mailto:support@heroiclabs.com">get in touch</a> with the Heroic Labs team about our developer training.

The first step to building your project with Nakama is to [install it](docker-quickstart.md).
