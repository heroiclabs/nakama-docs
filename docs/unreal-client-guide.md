# Unreal client guide

The official Unreal module is C++ client which handles all communication in realtime with the server. It implements all features in the server and is written with C++11.

## Download

The client SDK can be downloaded from <a href="https://github.com/heroiclabs/nakama-unreal/releases/latest" target="\_blank">GitHub releases</a>. You can download "nakama-unreal-$version.zip".

For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-unreal/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Bug "Help and contribute"
    The Unreal client is <a href="https://github.com/heroiclabs/nakama-unreal" target="\_blank">open source on GitHub</a>. Please report issues and contribute code to help us improve it.

## Setup

To use nakama-unreal in your Unreal project, you'll need to copy the nakama-unreal files you downloaded into the appropriate place. To do this:

1. Open your Unreal project folder (for example, `D:\\MyUnrealProject\\`) in Explorer or Finder.
1. If one does not already exist, create a `Plugins` folder here.
1. Copy the `Nakama` folder from the nakama-unreal release you downloaded, into this `Plugins` folder.
1. Now, edit your project's `.Build.cs` file, located in the project folder under `Source\\[ProjectFolder]` (for example, `D:\\MyUnrealProject\\Source\\MyUnrealProject\\MyUnrealProject.Build.cs`). Add this line to the constructor:

`PrivateDependencyModuleNames.AddRange(new string[] { "Nakama" });`

So, you might end up with the file that looks something like this:

```c#
using UnrealBuildTool;

public class MyUnrealProject : ModuleRules
{
	public MyUnrealProject(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore" });

		PrivateDependencyModuleNames.AddRange(new string[] { "Nakama" });
	}
}
```

At this point, you are done.  Restart Unreal.  After it compiles things, open Edit->Plugins and scroll to the bottom. If all went well, you should see HeroicLabs.Nakama listed as a plugin.

### Setup for Android projects

Android uses a permissions system which determines which platform services the application will request to use and ask permission for from the user. The client uses the network to communicate with the server so you must add the "INTERNET" permission to `AndroidManifest.xml`.

```xml
<uses-permission android:name="android.permission.INTERNET"/>
```

## Usage

Include nakama header.

```cpp
#include "nakama-cpp/Nakama.h"
```

Use nakama namespace.

```cpp
using namespace Nakama;
```

The client object is used to execute all logic against the server.

```cpp
DefaultClientParameters parameters;
parameters.serverKey = "defaultkey";
parameters.host = "127.0.0.1";
parameters.port = 7350;
NClientPtr client = createDefaultClient(parameters);
```

!!! Note
    By default the client uses connection settings "127.0.0.1" and 7350 port to connect to a local Nakama server.

```cpp
// Quickly setup a client for a local server.
NClientPtr client = createDefaultClient(DefaultClientParameters());
```

## Tick

The `tick` method pumps requests queue and executes callbacks in your thread. You must call it periodically, the `Tick` method of actor is good place for this.

```cpp
// Called every frame
void AMyActor::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    client->tick();
    if (rtClient) rtClient->tick();
}
```

Without this the default client and realtime client will not work, and you will not receive responses from the server.

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user](user-accounts.md) with one of the [authenticate options](authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```cpp
NClientPtr client = createDefaultClient(DefaultClientParameters());
```

&nbsp;&nbsp; 2\. Authenticate a user. By default Nakama will try and create a user if it doesn't exist.

!!! Tip
    It's good practice to cache a device identifier on Android when it's used to authenticate because they can change with device OS updates.

```cpp
auto loginFailedCallback = [](const NError& error)
{
};

auto loginSucceededCallback = [](NSessionPtr session)
{
};

std::string deviceId = "unique device id";

client->authenticateDevice(
        deviceId,
        opt::nullopt,
        opt::nullopt,
        loginSucceededCallback,
        loginFailedCallback);
```

In the code above we use `authenticateDevice()` but for other authentication options have a look at the [code examples](authentication.md#register-or-login).

## Sessions

When authenticated the server responds with an auth token (JWT) which contains useful properties and gets deserialized into a `NSession` object.

```cpp
UE_LOG(LogActor, Warning, TEXT("%s"), session->getAuthToken().c_str()); // raw JWT token
UE_LOG(LogActor, Warning, TEXT("%s"), session->getUserId().c_str());
UE_LOG(LogActor, Warning, TEXT("%s"), session->getUsername().c_str());
UE_LOG(LogActor, Warning, TEXT("Session has expired: %s"), session->isExpired() ? "yes" : "no");
UE_LOG(LogActor, Warning, TEXT("Session expires at: %llu"), session->getExpireTime());
UE_LOG(LogActor, Warning, TEXT("Session created at: %llu"), session->getCreateTime());
```

It is recommended to store the auth token from the session and check at startup if it has expired. If the token has expired you must reauthenticate. The expiry time of the token can be changed as a [setting](install-configuration.md#common-properties) in the server.

## Send requests

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](social-friends.md), join [groups](social-groups-clans.md), or submit scores in [leaderboards](gameplay-leaderboards.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

All requests are sent with a session object which authorizes the client.

```cpp
auto successCallback = [](const NAccount& account)
{
    UE_LOG(LogActor, Warning, TEXT("user id : %s"), account.user.id.c_str());
    UE_LOG(LogActor, Warning, TEXT("username: %s"), account.user.username.c_str());
    UE_LOG(LogActor, Warning, TEXT("wallet  : %s"), account.wallet.c_str());
};

client->getAccount(session, successCallback, errorCallback);
```

Have a look at other sections of documentation for more code examples.

## Realtime client

The client can create one or more realtime clients. Each realtime client can have it's own event listener registered for responses received from the server.

!!! Note
    The socket is exposed on a different port on the server to the client. You'll need to specify a different port here to ensure that connection is established successfully.

```cpp
int port = 7350; // different port to the main API port
bool createStatus = true; // if the server should show the user as online to others.
// define realtime client in your class as NRtClientPtr rtClient;
rtClient = client->createRtClient(port);
// define listener in your class as NRtDefaultClientListener listener;
listener.setConnectCallback([]()
{
    UE_LOG(LogActor, Warning, TEXT("Socket connected"));
});
rtClient->setListener(&listener);
rtClient->connect(session, createStatus);
```

Don't forget to call `tick` method. See [Tick](#tick) section for details.

You can use realtime client to send and receive [chat messages](social-realtime-chat.md), get [notifications](social-in-app-notifications.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

To join a chat channel and receive messages:

```cpp
listener.setChannelMessageCallback([](const NChannelMessage& message)
{
    UE_LOG(LogActor, Warning, TEXT("Received a message on channel %s"), message.channel_id.c_str());
    UE_LOG(LogActor, Warning, TEXT("Message content: %s"), message.content.c_str());
});

std::string roomName = "Heroes";

auto successJoinCallback = [this](NChannelPtr channel)
{
    UE_LOG(LogActor, Warning, TEXT("joined chat: %s"), channel->id.c_str());

    // content must be JSON
    std::string content = "{\"message\":\"Hello world\"}";

    rtClient->writeChatMessage(channel->id, content);
};

rtClient->joinChat(
            roomName,
            NChannelType::ROOM,
            {},
            {},
            successJoinCallback,
            errorCallback);
```

There are more examples for chat channels [here](social-realtime-chat.md).

## Handle events

A realtime client has event handlers which are called on various messages received from the server.

```cpp
listener.setStatusPresenceCallback([](const NStatusPresenceEvent& event)
{
    for (auto& presence : event.joins)
    {
        UE_LOG(LogActor, Warning, TEXT("Joined User ID: %s Username: %s Status: %s"), presence.user_id.c_str(), presence.username.c_str(), presence.status.c_str());
    }

    for (auto& presence : event.leaves)
    {
        UE_LOG(LogActor, Warning, TEXT("Left User ID: %s Username: %s Status: %s"), presence.user_id.c_str(), presence.username.c_str(), presence.status.c_str());
    }
});
```

Event handlers only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| onDisconnect | Handles an event for when the client is disconnected from the server. |
| onNotification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| onChannelMessage | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| onChannelPresence | It handles join and leave events within [chat](social-realtime-chat.md). |
| onMatchState | Receives [realtime multiplayer](gameplay-multiplayer-realtime.md) match data. |
| onMatchPresence | It handles join and leave events within [realtime multiplayer](gameplay-multiplayer-realtime.md). |
| onMatchmakerMatched | Received when the [matchmaker](gameplay-matchmaker.md) has found a suitable match. |
| onStatusPresence | It handles status updates when subscribed to a user [status feed](social-status.md). |
| onStreamPresence | Receives [stream](advanced-streams.md) join and leave event. |
| onStreamState | Receives [stream](advanced-streams.md) data sent by the server. |

## Logging

Client logging is off by default.

To enable logs output to console with debug logging level:

```cpp
#include "NUnrealLogSink.h"

NLogger::init(std::make_shared<NUnrealLogSink>(), NLogLevel::Debug);
```

## Errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code.

To enable client logs see [Logging](#logging) section.

In every request in the client you can set error callback. It will be called when request fails. The callback has `NError` structure which contains details of the error:

```cpp
auto errorCallback = [](const NError& error)
{
    // convert error to readable string
    UE_LOG(LogActor, Warning, TEXT("%s"), toString(error).c_str());

    // check error code
    if (error.code == ErrorCode::ConnectionError)
    {
        UE_LOG(LogActor, Warning, TEXT("The server is currently unavailable. Check internet connection."));
    }
};

client->getAccount(session, successCallback, errorCallback);
```

The client writes all errors to logger so you don't need to do this.

## Client reference

You can find the C++ Client Reference [here](https://heroiclabs.github.io/nakama-cpp/).
