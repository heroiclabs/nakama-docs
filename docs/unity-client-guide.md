# Unity client guide

This client is built on the [.NET client](https://github.com/heroiclabs/nakama-dotnet) with extensions for Unity Engine. To work with our Unity client you'll need to install and setup [Unity engine](https://unity3d.com/get-unity/download).

The client is available on the <a href="https://assetstore.unity.com/packages/tools/network/nakama-81338" target="\_blank">Unity Asset Store</a> and also on <a href="https://github.com/heroiclabs/nakama-unity/releases/latest" target="\_blank">GitHub releases</a>. You can download "Nakama.unitypackage" which contains all source code and DLL dependencies required in the client code.

It requires the .NET4.6 scripting runtime version to be set in the editor. Navigate to Edit -> Project Settings -> Player -> Configuration (subheading) to apply it.

For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-unity/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Tip "Contribute"
    The Unity client is <a href="https://github.com/heroiclabs/nakama-unity" target="\_blank">open source</a> on GitHub. Report issues and contribute code to help us improve it.

## Setup

When you've <a href="https://github.com/heroiclabs/nakama-unity/releases/latest" target="\_blank">downloaded</a> the "Nakama.unitypackage" file you should drag or import it into your Unity editor project to install it. In the editor create a new C# script via the Assets menu with "Assets > Create > C# Script" and create a client object.

The client object is used to interact with the server.

```csharp
using System.Collections;
using Nakama;
using UnityEngine;

public class YourGameObject : MonoBehaviour
{
  void Start()
  {
    var client = new Client("http", "127.0.0.1", 7350, "defaultkey");
  }
}
```

Unity uses an entity component system (ECS) which makes it possible to share the client across game objects. Have a read of <a href="https://docs.unity3d.com/Manual/ControllingGameObjectsComponents.html" target="\_blank">Controlling GameObjects Using Components</a> for examples on how to share a C# object across your game objects.

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user](user-accounts.md) with one of the [authenticate options](authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```csharp
var client = new Client("http", "127.0.0.1", 7350, "defaultkey");
```

&nbsp;&nbsp; 2\. Authenticate a user. By default the server will create a user if it doesn't exist.

```csharp
const string email = "hello@example.com";
const string password = "somesupersecretpassword";
var session = await client.AuthenticateEmailAsync(email, password);
Debug.Log(session);
```

In the code above we use `AuthenticateEmailAsync` but for other authentication options have a look at the [code examples](authentication.md#authenticate). This [full example](#full-example) covers all our recommended steps.

## Sessions

When authenticated the server responds with an auth token (JWT) which contains useful properties and gets deserialized into a `Session` object.

```csharp
Debug.Log(session.AuthToken); // raw JWT token
Debug.LogFormat("Session user id: '{0}'", session.UserId);
Debug.LogFormat("Session user username: '{0}'", session.Username);
Debug.LogFormat("Session has expired: {0}", session.IsExpired);
Debug.LogFormat("Session expires at: {0}", session.ExpireTime); // in seconds.
```

It is recommended to store the auth token from the session and check at startup if it has expired. If the token has expired you must reauthenticate. The expiry time of the token can be changed as a [setting](install-configuration.md#common-properties) in the server.

```csharp
const string prefKeyName = "nakama.session";
ISession session;
var authToken = PlayerPrefs.GetString(prefKeyName);
if (string.IsNullOrEmpty(authToken) || (session = Session.Restore(authToken)).IsExpired)
{
    Debug.Log("Session has expired. Must reauthenticate!");
};
Debug.Log(session);
```

## Send requests

The client includes lots of builtin APIs for various features of the game server. These are accessed with async methods. It can also call custom logic through RPC functions on the server.

All requests are sent with a session object which authorizes the client.

```csharp
var account = await client.GetAccountAsync(session);
Debug.LogFormat("User id: '{0}'", account.User.Id);
Debug.LogFormat("User username: '{0}'", account.User.Username);
Debug.LogFormat("Account virtual wallet: '{0}'", account.Wallet);
```

Methods which end with "Async" can use C# Tasks to asynchronously wait for the response. This can be done with the `await` keyword. For more advice on async/await features in C# have look at the <a href="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/" target="\_blank">official documentation</a>.

The other sections of the documentation include more code examples on the client.

## Socket messages

The client can create one or more sockets with the server. Each socket can have it's own event listeners registered for responses received from the server.

```csharp
var socket = client.NewSocket();
socket.Connected += () =>
{
    Debug.Log("Socket connected.");
};
socket.Closed += () =>
{
    Debug.Log("Socket closed.");
};
await socket.ConnectAsync(session);
```

You can connect to the server over a realtime socket connection to send and receive [chat messages](social-realtime-chat.md), get [notifications](social-in-app-notifications.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

To join a chat channel and receive messages:

```csharp
const string RoomName = "Heroes";
socket.ReceivedChannelMessage += message =>
{
    Debug.LogFormat("Received: {0}", message);
    Debug.LogFormat("Message has channel id: {0}", message.ChannelId);
    Debug.LogFormat("Message content: {0}", message.Content);
};
var channel = await socket.JoinChatAsync(RoomName, ChannelType.Room);
// using Nakama.TinyJson;
var content = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
var sendAck = await socket.WriteChatMessageAsync(channel, content);
Debug.Log(sendAck);
```

There are more examples for chat channels [here](social-realtime-chat.md).

## Handle events

A socket object has event handlers which are called on various messages received from the server.

```csharp
socket.ReceivedChannelPresence += presenceEvent =>
{
    Debug.Log(presenceEvent);
    foreach (var left in presence.Leaves)
    {
        Debug.LogFormat("User '{0}' left.", left.Username);
    }
    foreach (var joined in presence.Joins)
    {
        Debug.LogFormat("User '{0}' joined.", joined.Username);
    }
};
```

Event handlers only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| Connected | Receive an event when the socket connects. |
| Closed | Handles an event for when the client is disconnected from the server. |
| ReceivedError | Receives events about server errors. |
| ReceivedNotification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| ReceivedChannelMessage | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| ReceivedChannelPresence | It handles join and leave events within [chat](social-realtime-chat.md). |
| ReceivedMatchState | Receives [realtime multiplayer](gameplay-multiplayer-realtime.md) match data. |
| ReceivedMatchPresence | It handles join and leave events within [realtime multiplayer](gameplay-multiplayer-realtime.md). |
| ReceivedMatchmakerMatched | Received when the [matchmaker](gameplay-matchmaker.md) has found a suitable match. |
| ReceivedStatusPresence | It handles status updates when subscribed to a user [status feed](social-status.md). |
| ReceivedStreamPresence | Receives [stream](advanced-streams.md) join and leave event. |
| ReceivedStreamState | Receives [stream](advanced-streams.md) data sent by the server. |

## Logs and errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code. To log all messages sent by the client you can enable "Trace" when you build a client and attach a logger.

```csharp
var client = new Client("http", "127.0.0.1", 7350, "defaultkey")
{
#if UNITY_EDITOR
    Logger = new UnityLogger()
#endif
};
```

The `#if` preprocessor directives is used so trace is only enabled in Unity editor builds. For more complex directives with debug vs release builds have a look at <a href="https://docs.unity3d.com/Manual/PlatformDependentCompilation.html" target="\_blank">Platform dependent compilation</a>.

## WebGL Builds

For WebGL builds you should switch the `IHttpAdapter` to use the `UnityWebRequestAdapter` and use the `NewSocket()` extension method to create the socket OR manually set the right `ISocketAdapter` per platform.

```csharp
var client = new Client("http", "127.0.0.1", 7350, "defaultkey", UnityWebRequestAdapter.Instance);
var socket = client.NewSocket();

// OR
#if UNITY_WEBGL && !UNITY_EDITOR
    ISocketAdapter adapter = new JsWebSocketAdapter();
#else
    ISocketAdapter adapter = new WebSocketAdapter();
#endif
var socket = Socket.From(client, adapter);
```

## Full example

A small example on how to manage a session object with Unity engine and the client.

```csharp
using System.Collections;
using System.Collections.Generic;
using Nakama;
using UnityEngine;

public class SessionWithPlayerPrefs : MonoBehaviour
{
    private const string PrefKeyName = "nakama.session";

    private IClient _client = new Client("http", "127.0.0.1", 7350, "defaultkey");
    private ISession _session;

    private async void Awake()
    {
        var authtoken = PlayerPrefs.GetString(PrefKeyName);
        if (!string.IsNullOrEmpty(authtoken))
        {
            var session = Session.Restore(authtoken);
            if (!session.IsExpired)
            {
                _session = session;
                Debug.Log(_session);
                return;
            }
        }
        var deviceid = SystemInfo.deviceUniqueIdentifier;
        _session = await _client.AuthenticateDeviceAsync(deviceid);
        PlayerPrefs.SetString(PrefKeyName, _session.AuthToken);
        Debug.Log(_session);
    }
}
```

A collection of other code examples is available <a href="https://github.com/heroiclabs/nakama-unity/tree/master/Assets/Nakama/Snippets" target="\_blank">here</a>.
<br>
<br>
