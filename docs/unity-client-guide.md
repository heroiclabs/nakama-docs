# Unity client guide

!!! info "Nakama 2.x release"
    With the Nakama 2.x release we've split out the Unity client into a lower level client for .NET and a new wrapper which will require Unity 2017 or greater. The new Unity client is not released yet but will be announced on our [blog](https://blog.heroiclabs.com/) and [Twitter](https://twitter.com/heroicdev). In the meantime you can use the .NET client directly and is available [here](https://github.com/heroiclabs/nakama-dotnet).

The official Unity client handles all communication in realtime with the server. It implements all features in the server and is compatible with Unity 5.4+. To work with our Unity client you'll need to install and setup [Unity engine](https://unity3d.com/get-unity/download).

## Download

The client is available on the <a href="https://www.assetstore.unity3d.com/en/#!/content/81338" target="\_blank">Unity Asset store</a> and also on <a href="https://github.com/heroiclabs/nakama-unity/releases/latest" target="\_blank">GitHub releases</a>. You can download "Nakama.unitypackage" which contains all source code and DLL dependencies required in the client code.

For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-unity/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Bug "Help and contribute"
    The Unity client is <a href="https://github.com/heroiclabs/nakama-unity" target="\_blank">open source on GitHub</a>. Please report issues and contribute code to help us improve it.

## Install and setup

When you've [downloaded](#download) the "Nakama.unitypackage" file you should drag or import it into your Unity editor project to install it. In the editor create a new C# script via the Assets menu with "Assets > Create > C# Script" and create an `INClient`.

The client object is used to execute all logic against the server.

```csharp
using Nakama;
using System.Collections;
using UnityEngine;

public class NakamaSessionManager : MonoBehaviour {
  void Start() {
    INClient client = new NClient.Builder("defaultkey")
        .Host("127.0.0.1")
        .Port(7350)
        .SSL(false)
        .Build();
  }

  void Update() {
  }
}
```

We use the builder pattern with many classes in the Unity client. Most classes have a `".Default()"` method to construct an object with default values.

!!! Note
    By default the client uses connection settings "127.0.0.1" and 7350 to connect to a local Nakama server.

```csharp
// Quickly setup a client for a local server.
INClient client = NClient.Default("defaultkey");
```

Unity uses an entity component system (ECS) which makes it simple to share the client across game objects. Have a read of <a href="https://docs.unity3d.com/Manual/ControllingGameObjectsComponents.html" target="\_blank">Controlling GameObjects Using Components</a> for examples on how to share a C# object across your game objects.

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user](user-accounts.md) with one of the [authenticate options](authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```csharp
var client = NClient.Default("defaultkey");
```

&nbsp;&nbsp; 2\. Write a callback which will be used to connect to the server.

```csharp
Action<INSession> sessionHandler = delegate(INSession session) {
  Debug.LogFormat("Session: '{0}'.", session.Token);
  client.Connect(session, (bool done) => {
    Debug.Log("Session connected.");
    // Store session for quick reconnects.
    PlayerPrefs.SetString("nk.session", session.Token);
  });
};
```

&nbsp;&nbsp; 3\. Restore a session for quick reconnects.

```csharp
var sessionString = PlayerPrefs.GetString("nk.session");
if (!string.IsNullOrEmpty(sessionString)) {
  INSession session = NSession.Restore(sessionString);
  if (!session.HasExpired(DateTime.UtcNow)) {
    sessionHandler(session);
  }
}
```

&nbsp;&nbsp; 4\. Login or register a user.

!!! Tip
    It's good practice to cache a device identifier when it's used to authenticate because they can change with device OS updates.

```csharp
Action<INError> errorHandler = delegate(INError err) {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

// See if we have a cached id in PlayerPrefs.
var id = PlayerPrefs.GetString("nk.id");
if (string.IsNullOrEmpty(id)) {
  // We'll use device ID for the user. See other authentication options.
  id = SystemInfo.deviceUniqueIdentifier;
  // Store the identifier for next game start.
  PlayerPrefs.SetString("nk.id", id);
}

var message = NAuthenticateMessage.Device(id);
_client.Login(message, sessionHandler, (INError err) => {
  if (err.Code == ErrorCode.UserNotFound) {
    _client.Register(message, sessionHandler, errorHandler);
  } else {
    ErrorHandler(err);
  }
});
```

In the code above we use `NAuthenticateMessage.Device(id)` but for other authentication options have a look at the [code examples](authentication.md#register-or-login).

A __full example__ class with all code above is [here](#full-example).

## Send messages

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](social-friends.md), join [groups](social-groups-clans.md) and [chat](social-realtime-chat.md), or submit scores in [leaderboards](gameplay-leaderboards.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

The server also provides a [storage engine](storage-collections.md) to keep save games and other records owned by users. We'll use storage to introduce how messages are sent.

```csharp
string json = "{\"jsonkey\":\"jsonvalue\"}";

var message = new NStorageWriteMessage.Builder()
    .Write("someBucket", "someCollection", "myRecord", storageValue)
    .Build();
client.Send(message, (INResultSet<INStorageKey> list) => {
  Debug.Log("Successfully wrote record.");
}, (INError error) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

Have a look at other sections of documentation for more code examples.

## Handle events

The client has callbacks which are called on various events received from the server.

```csharp
client.OnError = (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

client.OnDisconnect = (INDisconnectEvent evt) => {
  Debug.Log("Disconnected from server.");
  Debug.LogFormat("Reason '{0}'", evt.Reason);
}
```

Some events only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| OnDisconnect | Handles an event for when the client is disconnected from the server. |
| OnError | Receives events about server errors. |
| OnMatchData | Handles [realtime match](gameplay-multiplayer-realtime.md) messages. |
| OnMatchmakeMatched | Receives events when the [matchmaker](gameplay-matchmaker.md) has found match participants. |
| OnMatchPresence | When in a [realtime match](gameplay-multiplayer-realtime.md) receives events for when users join or leave. |
| OnNotification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| OnTopicMessage | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| OnTopicPresence | Similar to "OnMatchPresence" it handles join and leave events but within [chat](social-realtime-chat.md). |

## Main thread dispatch

The client runs all callbacks on a socket thread separate to the Unity main thread.

Unity engine does not let code which executes on another thread call one of it's APIs because of how it manages code execution within the game loop. This can causes errors which look something like "<SomeMethod\> can only be called from the main thread".

We recommend a simple pattern which can be used to run any code which calls `UnityEngine` APIs.

&nbsp;&nbsp; 1\. Add a queue to your script which manages a client.

```csharp
Queue<Action> executionQueue = new Queue<Action>(1024);
```

&nbsp;&nbsp; 2\. Add code in your `Update` method so the queued actions are run.

```csharp
for (int i = 0, l = executionQueue.Count; i < l; i++) {
  executionQueue.Dequeue()();
}
```

&nbsp;&nbsp; 3\. Enqueue any code which uses a `UnityEngine` API.

```csharp
client.Connect(_session, (bool done) => {
  executionQueue.Enqueue(() => {
    Debug.Log("Session connected.");
    // Store session for quick reconnects.
    PlayerPrefs.SetString("nk.session", session.Token); // a UnityEngine API
  });
});
```

You can see a more advanced version of this pattern in the [full example](#full-example).

!!! Tip
    This code pattern is not specific to our client. It's useful for any code which executes on a separate thread with Unity engine.

### Managed client

If you don't care about explicit control over which callbacks are dispatched on the Unity main thread you can wrap your code in a helper class which will handle it for you. The `"NManagedClient"` acts as a proxy for all callbacks.

You must call `.ExecuteActions()` in `Update` with the managed client or no callbacks will ever be run.

```csharp hl_lines="14"
using Nakama;
using System.Collections;
using UnityEngine;

public class NakamaSessionManager : MonoBehaviour {
  private INClient _client;

  public NakamaSessionManager() {
    var client = NClient.Default("defaultkey");
    _client = new NManagedClient(client);
  }

  private void Update() {
    (_client as NManagedClient).ExecuteActions(); // important!
  }
}
```

This makes code simpler to reason about but is slightly less performant than if you control exactly which callbacks use UnityEngine APIs and "move them" onto the main thread with an action queue.

## Logs and errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code. To log all messages sent by the client you can enable "Trace" when you build an `"INClient"`.

```csharp
#if UNITY_EDITOR
INClient client = new NClient.Builder("defaultkey")
    .Trace(true)
    .Build();
#else
INClient client = NClient.Default("defaultkey");
#endif
```

The `#if` preprocessor directives is used so trace is only enabled in Unity editor builds. For more complex directives with debug vs release builds have a look at <a href="https://docs.unity3d.com/Manual/PlatformDependentCompilation.html" target="\_blank">Platform dependent compilation</a>.

Every error in the Unity client implements the `"INError"` interface. It contains details on the source and content of an error:

```csharp
Action<INError> errorHandler = delegate(INError error) {
  Debug.LogFormat("Error code {0}", error.Code);
  Debug.LogFormat("Error message {0}", error.Message);
};
```

---

## Full example

Nakama Showreel is a Unity 2017.1 project which you can find on the Asset Store and on [GitHub](https://github.com/heroiclabs/nakama-examples/tree/master/unity/nakama-showreel).

The project is divided into two sections:
- Showreel: C# specific code responsible for scenes and interactions in the Showreel project. Study this code to learn how to interact with Nakama domain objects and display them in a Unity scene.
- Framework: Unity-specific helper code that interacts directly with the Nakama client (available on the [Asset Store](https://assetstore.unity.com/packages/tools/network/nakama-81338)).

The `Framework` package has the following classes:

- `Logger.cs`: A helper class that enables logging output based on the target output in Unity. This is a simple wrapper around the Unity Debug class.
- `Singleton.cs`: An abstract class responsible for managing the lifecycle of a Singleton class. [This guide](http://wiki.unity3d.com/index.php/Singleton) can tell you about Singleton patterns.
- `StateManager.cs`: This class is responsible for storing state retrieved from the Nakama server. Use it to store read-only state retrieved from the remote server, and then rendered in a view. Best to use `C# Properties` as much as possible and avoid `Null` state to simplify code and remove runtime bugs.
- `NakamaManager.cs`: This is a wrapper around the Nakama `INClient` object.
    -- This class enables you to authenticate a user and store their session token to the local storage on device for later retrieval.
    - Perform smart (exponential) reconnect behaviour to the server if the connection drops.
    - Updates state stored in the `StateManager` when data is returned from the server.
    - Disconnect and reconnect to the server automatically when the application focus is lost/gained.
    - Run network activity on a non-UI thread to avoid game UI freezing.

We recommend that you directly copy the `Framework` package into your project and append extra logic into the `StateManager` and `NakamaManager` classes as you wish.

The Showreel project uses the MVC (Model-View-Controller) pattern to store-update-render state data. The MVC pattern allows you to de-couple rendering logic from gameplay logic and network logic that is asynchronous. This is done to separate internal representations of information from the ways information is presented to, and accepted from, the user. The MVC design pattern decouples these major components allowing for efficient code reuse and parallel development. Follow [this guide](http://engineering.socialpoint.es/MVC-pattern-unity3d-ui.html) to learn more about the MVC pattern and how to set it up in your project.

## Client reference

You can find the C# Client Reference [here](https://heroiclabs.github.io/nakama-unity/generated/namespace_nakama.html).
