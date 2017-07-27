# Unity client guide

The official Unity client handles all realtime communication with the server. It implements all features in the server and is compatible with Unity 5.4+. To work with our Unity client you'll need to install and setup [Unity engine](https://unity3d.com/get-unity/download).

## Download

The client is available on the <a href="https://www.assetstore.unity3d.com/en/#!/content/81338" target="\_blank">Unity Asset store</a> and also on <a href="https://github.com/heroiclabs/nakama-unity/releases/latest" target="\_blank">GitHub releases</a>. You can download "Nakama.unitypackage" which contains all source code and DLL dependencies required in the client code.

For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-unity/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Bug "Help and contribute"
    The Unity client is <a href="" target="\_blank">open source on GitHub</a>. Please report issues and contribute code to help us make it better.

## Install and setup

When you've [downloaded](#download) the "Nakama.unitypackage" file you should drag or import it into your Unity editor project to install it. In the editor create a new C# script via the Assets menu with "Assets > Create > C# Script" and create an `INClient`.

The client object is used to execute all logic against the server.

```csharp
using Nakama;
using System.Collections;
using UnityEngine;

public class NakamaManager : MonoBehaviour {
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
// quick setup a client for a local server.
INClient client = NClient.Default("defaultkey");
```

Unity uses an entity component system (ECS) which makes it simple to share the client across game objects. Have a read of <a href="https://docs.unity3d.com/Manual/ControllingGameObjectsComponents.html" target="\_blank">Controlling GameObjects Using Components</a> for examples on how to share a C# object across your game objects.

## Authenticate

With a client object you can authenticate against the server. You can register and/or login a [user](user-accounts.md) with one of the [authenticate options](authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```csharp
var client = NClient.Default("defaultkey");
```

&nbsp;&nbsp; 2\. Write a callback which will be used to connect to the server.

```csharp
var sessionHandler = delegate(INSession session) {
  Debug.LogFormat("Session: '{0}'.", session.Token);
  client.Connect(_session, (bool done) => {
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

&nbsp;&nbsp; 4\. Login or register user.

!!! Tip
    It's good practice to cache a device identifier when it's used to authenticate because they can change with device OS updates.

```csharp
var errorHandler = delegate(INError err) {
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
_client.Login(message, SessionHandler, (INError err) => {
  if (err.Code == ErrorCode.UserNotFound) {
    _client.Register(message, SessionHandler, ErrorHandler);
  } else {
    ErrorHandler(err);
  }
});
```

In the code above we use `NAuthenticateMessage.Device(id)` but for other authenticate options have a look at the [code examples](authentication.md#register-or-login).

A __full example__ class with all code above is [here](#full-example).

## Send messages

When a user has been authenticated and a session is used to connect with the server. You can send messages for all the different features in the server.

This could be to [add friends](social-friends.md), join [groups](social-groups-clans.md) and [chat](social-realtime-chat.md), or submit scores in [leaderboards](gameplay-leaderboards.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-realtime-multiplayer.md). You can also execute remote code on the server via [RPC](remote-code-basics.md).

The server also provides a [storage engine](storage-collections.md) to keep save games and other records owned by users. We'll use storage to introduce how messages are sent.

```csharp
byte[] json = Encoding.UTF8.GetBytes("{\"jsonkey\":\"jsonvalue\"}");

var message = new NStorageWriteMessage.Builder()
    .Write("someBucket", "someCollection", "myRecord", storageValue)
    .Build();
client.Send(message, (bool done) => {
  Debug.Log("Successfully wrote record.");
}, (INError error) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

Have a look at other sections of documentation for more code examples.

## Handle events

The client uses event handlers which are called on various events received from the server.

```csharp
client.OnError += (NErrorEventArgs args) => {
  INError err = args.Error;
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

client.OnDisconnect += () => {
  Debug.Log("Disconnected from server.");
}
```

Some events only need to be implemented for the features you want to use.

| Event handler | Description |
| ------------- | ----------- |
| OnDisconnect | Handles an event for when the client is disconnected from the server. |
| OnError | Receives events about server errors. |
| OnMatchData | Handles [realtime match](gameplay-realtime-multiplayer.md) messages. |
| OnMatchmakeMatched | Receives events when the [matchmaker](gameplay-matchmaker.md) has found match participants. |
| OnMatchPresence | When in a [realtime match](gameplay-realtime-multiplayer.md) receives events for when users join or leave. |
| OnNotification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| OnTopicMessage | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| OnTopicPresence | Similar to "OnMatchPresence" it handles join and leave events but within [chat](social-realtime-chat.md). |

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
var errorHandler = delegate(INError error) {
  Debug.LogFormat("Error code {0}", error.Code);
  Debug.LogFormat("Error message {0}", error.Message);
};
```

<a id="full-example"></a>

---

An example class used to manage a session with the Unity client.

```csharp
using Nakama;
using System.Collections;
using UnityEngine;

public class NakamaManager : MonoBehaviour {
  private INClient _client;
  private INSession _session;

  public NakamaManager() {
    _client = NClient.Default("defaultkey");
  }

  private void Awake() {
    RestoreSessionAndConnect();
    if (_session == null) {
      LoginOrRegister();
    }
  }

  private void RestoreSessionAndConnect() {
    // Lets check if we can restore a cached session.
    var sessionString = PlayerPrefs.GetString("nk.session");
    if (string.IsNullOrEmpty(sessionString)) {
      return; // We have no session to restore.
    }

    var session = NSession.Restore(sessionString);
    if (session.HasExpired(DateTime.UtcNow)) {
      return; // We can't restore an expired session.
    }

    SessionHandler(session);
  }

  private void LoginOrRegister() {
    // See if we have a cached id in PlayerPrefs.
    var id = PlayerPrefs.GetString("nk.id");
    if (string.IsNullOrEmpty(id)) {
      // We'll use device ID for the user. See other authentication options.
      id = SystemInfo.deviceUniqueIdentifier;
      // Store the identifier for next game start.
      PlayerPrefs.SetString("nk.id", id);
    }

    var message = NAuthenticateMessage.Device(id);
    _client.Login(message, SessionHandler, (INError err) => {
      if (err.Code == ErrorCode.UserNotFound) {
        _client.Register(message, SessionHandler, ErrorHandler);
      } else {
        ErrorHandler(err);
      }
    });
  }

  private void SessionHandler(INSession session) {
    _session = session;
    Debug.LogFormat("Session: '{0}'.", session.Token);
    _client.Connect(_session, (bool done) => {
      Debug.Log("Session connected.");
      // Store session for quick reconnects.
      PlayerPrefs.SetString("nk.session", session.Token);
    });
  }

  private static void ErrorHandler(INError error) {
    Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
  }
}
```
