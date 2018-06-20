# Unity client guide

The official Unity client handles all communication in realtime with the server. It implements all features in the server and is compatible with Unity 2017 or newer. To work with our Unity client you'll need to install and setup [Unity engine](https://unity3d.com/get-unity/download).

With the Nakama 2.x release we've split out the Unity client into a lower level client for .NET and a new wrapper client which will require Unity 2017 or greater.

1. Nakama .NET client library: A lower level .NET library that you can use directly inside your Unity Project. This will give you complete control over sending and receiving responses from the server, updating your scene and more. You can look at the source code on the [GitHub repo](https://github.com/heroiclabs/nakama-dotnet).
2. Nakama Unity client library: A wrapper around the .NET client library that integrates more tightly into the Unity Engine, and will take of client reconnection, message queuing and interacting with Unity lifecycle events. You can download it from [this repo](https://github.com/heroiclabs/nakama-unity).

!!! info "Nakama 2.x release"
    Make sure that you download the Unity Client v2.0.0 or greater as it is compatible with Nakama 2.

## Download

The client is available on the <a href="https://www.assetstore.unity3d.com/en/#!/content/81338" target="\_blank">Unity Asset store</a> and also on <a href="https://github.com/heroiclabs/nakama-unity/releases/latest" target="\_blank">GitHub releases</a>. You can download "Nakama.unitypackage" which contains all source code and DLL dependencies required in the client code.

<!--
For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-unity/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Bug "Help and contribute"
    The Unity client is <a href="https://github.com/heroiclabs/nakama-unity" target="\_blank">open source on GitHub</a>. Please report issues and contribute code to help us improve it.
-->

## Install and setup

When you've [downloaded](#download) the "Nakama.unitypackage" file you should drag or import it into your Unity editor project to install it. In the editor create a new C# script via the Assets menu with "Assets > Create > C# Script" and create an `INClient`.

The client object is used to execute all logic against the server.

```csharp
using Nakama;
using System.Collections;
using UnityEngine;

public class NakamaSessionManager : MonoBehaviour {
  void Start() {
    var client = new Client("defaultkey", "127.0.0.1", 7350, false);
  }

  void Update() {
  }
}
```

Unity uses an entity component system (ECS) which makes it simple to share the client across game objects. Have a read of <a href="https://docs.unity3d.com/Manual/ControllingGameObjectsComponents.html" target="\_blank">Controlling GameObjects Using Components</a> for examples on how to share a C# object across your game objects.

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user](user-accounts.md) with one of the [authenticate options](authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```csharp
var client = new Client("defaultkey", "127.0.0.1", 7350, false);
```

&nbsp;&nbsp; 2\. Authenticate a user. By default, Nakama will try and create a user if it doesn't exist.

Use the following code to store the session:

```csharp
const string email = "hello@example.com";
const string password = "somesupersecretpassword";
var session = await client.AuthenticateEmailAsync(email, password);
PlayerPrefs.SetString("nk.session", session.AuthToken);
Debug.LogFormat("Authenticated successfully. User id {0}:", session.user_id);
```

In the code above we use `AuthenticateEmailAsync` but for other authentication options have a look at the [code examples](authentication.md#register-or-login).

A __full example__ class with all code above is [here](#full-example).

## Send messages

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](social-friends.md), join [groups](social-groups-clans.md), or submit scores in [leaderboards](gameplay-leaderboards.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

The server also provides a [storage engine](storage-collections.md) to keep save games and other records owned by users. We'll use storage to introduce how messages are sent.

```js
var object = new WriteStorageObject = {
  "collection" = "collection",
  "key" ="key1",
  "value" = "{\"jsonKey\": \"jsonValue\"}"
};
const storageWriteAck = await client.WriteStorageObjectsAsync(session, objects);
Debug.LogFormat("Storage write was successful: {0}", storageWriteAck);
```

Have a look at other sections of documentation for more code examples.

## Realtime data exchange

You can connect to the server over a realtime WebSocket connection to send and receive [chat messages](social-realtime-chat.md), get [notifications](social-in-app-notifications.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

You first need to create a realtime socket to the server:

```csharp
var socket = client.CreateWebSocket();
await socket.ConnectAsync(session);
```

Then proceed to join a chat channel and send a message:

```csharp
socket.OnChannelMessage += (sender, chatMessage) => Debug.Log("Received message.");

var channel = await socket.JoinChatAsync("myroom", ChannelType.Room);

var content = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
var sendAck = await socket.WriteChatMessageAsync(channel, content);
```

You can find more information about the various chat features available [here](social-in-app-notifications.md).

## Handle events

A client socket has event listeners which are called on various events received from the server.

```csharp
socket.OnStatusPresence += (_, presence) =>
{
  foreach (var join in presence.Joins)
  {
    Debug.LogFormat("User id '{0}' name '{1}' and status '{2}'.", join.UserId, join.Username, join.Status);
  }

  foreach (var leave in presence.Leaves)
  {
    Debug.LogFormat("User id '{0}' name '{1}' and status '{2}'.", leave.UserId, leave.Username, leave.Status);
  }
};
```

Some events only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| OnConnect | Receive an event when the socket connects. |
| OnDisconnect | Handles an event for when the client is disconnected from the server. |
| OnError | Receives events about server errors. |
| OnNotification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| OnChannelMessage | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| OnChannelPresence | It handles join and leave events within [chat](social-realtime-chat.md). |
| OnMatchState | Receives [realtime multiplayer](gameplay-multiplayer-realtime.md) match data. |
| OnMatchPresence | It handles join and leave events within [realtime multiplayer](gameplay-multiplayer-realtime.md). |
| OnMatchmakerMatched | Received when the [matchmaker](gameplay-matchmaker.md) has found a suitable match. |
| OnStatusPresence | It handles status updates when subscribed to a user [status feed](social-status.md). |
| OnStreamPresence | Receives [stream](social-stream.md) join and leave event. |
| OnStreamState | Receives [stream](social-stream.md) data sent by the server. |

## Logs and errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code. To log all messages sent by the client you can enable "Trace" when you build an `"IClient"`.

```csharp
var client = new Client("defaultkey");
#if UNITY_EDITOR
  client.Trace(true);
#endif
```

The `#if` preprocessor directives is used so trace is only enabled in Unity editor builds. For more complex directives with debug vs release builds have a look at <a href="https://docs.unity3d.com/Manual/PlatformDependentCompilation.html" target="\_blank">Platform dependent compilation</a>.

## Full example

An example class used to manage a session with the Unity client.

```csharp
using Nakama;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NakamaSessionManager : MonoBehaviour {
  private IClient _client;
  private ISession _session;

  public NakamaSessionManager() {
    _client = new Client("defaultkey");
  }

  private async void Awake() {
    _session = RestoreSession();
    if (_session == null) {
      Authenticate();
    }
  }

  private Session RestoreSession() {
    // Lets check if we can restore a cached session.
    var sessionString = PlayerPrefs.GetString("nk.session");
    if (string.IsNullOrEmpty(sessionString)) {
      return null; // We have no session to restore.
    }

    var session = new Session(sessionString);
    if (session.IsExpired) {
      return null; // We can't restore an expired session.
    }

    return session;
  }

  private async void Authenticate() {
    // See if we have a cached id in PlayerPrefs.
    var id = PlayerPrefs.GetString("nk.id");
    if (string.IsNullOrEmpty(id)) {
      // We'll use device ID for the user. See other authentication options.
      id = SystemInfo.deviceUniqueIdentifier;
      // Store the identifier for next game start.
      PlayerPrefs.SetString("nk.id", id);
    }

    // Use whichever one of the authentication options you want.
    _session = await client.AuthenticateDeviceAsync($"{id}");
    Debug.LogFormat("Session: '{0}'.", session.AuthToken);
  }

  private void Update() {}

  private void OnApplicationQuit() {}
}
```

<!--
## Client reference

You can find the Unity Client Reference [here](https://heroiclabs.github.io/nakama-unity/generated/namespace_nakama.html).
-->
