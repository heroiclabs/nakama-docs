# Swift/iOS client guide

The official Swift client handles all communication in realtime with the server and is specifically optimized for iOS projects. It implements all features in the server and is compatible with Swift 3.1+. To work with the Swift client you'll need the <a href="https://swift.org/download/" target="\_blank">Swift distribution</a> and an editor/IDE like Atom or <a href="https://itunes.apple.com/app/xcode/id497799835" target="\_blank">XCode 8.3+</a>.

## Download

Releases for the client are managed on <a href="https://github.com/heroiclabs/nakama-swift" target="\_blank">GitHub</a>. You can use the Swift package manager to add the code as a dependency for your project.

For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-swift/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Bug "Help and contribute"
    The Swift client is <a href="https://github.com/heroiclabs/nakama-swift" target="\_blank">open source on GitHub</a>. Please report issues and contribute code to help us improve it.

## Install and setup

Add the client as a dependency to your "Package.swift" file.

```swift
let package = Package(
  // ...
  dependencies: [
    .Package(url: "https://github.com/heroiclabs/nakama-swift.git", Version(0,1,0)),
  ]
)
```

The client object is used to execute all logic against the server.

```swift
import Nakama

public class NakamaSessionManager {
  private let client: Client;

  init() {
    client = Builder("defaultkey")
        .host("127.0.0.1")
        .port(7350)
        .ssl(false)
        .build()
  }
}
```

You can also use the shorthand builder for the client.

!!! Note
    By default the client uses connection settings "127.0.0.1" and 7350 to connect to a local Nakama server.

```swift
let client : Client = Builder.defaults(serverKey: "defaultkey")
```

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user](user-accounts.md) with one of the [authenticate options](authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```swift
let client : Client = Builder.defaults(serverKey: "defaultkey")
```

&nbsp;&nbsp; 2\. Login or register a user.

!!! Tip
    It's good practice to cache a device identifier on iOS when it's used to authenticate because they can change with device OS updates.

```swift

```

In the code above we use `AuthenticateMessage.device(id: deviceID)` but for other authentication options have a look at the [code examples](authentication.md#register-or-login).

The client uses [promise chains](#promise-chains) for an easy way to execute asynchronous callbacks.

&nbsp;&nbsp; 3\. Store session for quick reconnects.

We can replace the callback marked in step 2 with a callback which stores the session object on iOS.

```swift

```

A __full example__ class with all code above is [here](#full-example).

## Send messages

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](social-friends.md), join [groups](social-groups-clans.md) and [chat](social-realtime-chat.md), or submit scores in [leaderboards](gameplay-leaderboards.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

The server also provides a [storage engine](storage-collections.md) to keep preferences and other records owned by users. We'll use storage to introduce how messages are sent.

```swift

```

Have a look at other sections of documentation for more code examples.

<!--
## Handle events

The client has listeners which are called on various events received from the server.

```java
```

Some events only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| a | Handles an event for when the client is disconnected from the server. |
| b | Receives events about server errors. |
| c | Handles [realtime match](gameplay-multiplayer-realtime.md) messages. |
| d | Receives events when the [matchmaker](gameplay-matchmaker.md) has found match participants. |
| e | When in a [realtime match](gameplay-multiplayer-realtime.md) receives events for when users join or leave. |
| f | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| g | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| h | Similar to "OnMatchPresence" it handles join and leave events but within [chat](social-realtime-chat.md). |
-->

## Promise chains

The client uses promises to represent asynchronous actions with the <a href="https://github.com/mxcl/PromiseKit" target="\_blank">PromiseKit</a> library. This makes it easy to chain together actions which must occur in sequence and handle errors.

A deferred object is created when messages are sent and can attach callbacks and error handlers for working on the deferred result.

```swift
```

You can chain callbacks because each callback returns a `Promise<T>`. Each method on the `Client` returns a `Promise<T>` so you can chain calls with `.then { }`.

```swift

```

## Logs and errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code. To log all messages sent by the client you can enable "trace" when you build a `"Client"`.

```swift
let client : Client = new Builder("defaultkey")
    .trace(true)
    .build();
```

Every error in the Swift client implements the `"NakamaError"` class. It contains details on the source and content of an error:

```swift

```

## Full example

```swift
internal class NakamaSessionManager {
  public let client : Client
  private var session : Session?

  private static let defaults = UserDefaults.standard
  private static let deviceKey = "device_id"
  private static let sessionKey = "session"

  internal init() {
    client = Builder.defaults(serverKey: "defaultkey")
  }

  func loginOrRegister() {
    var deviceId : String? = NakamaSessionManager.defaults.string(forKey: NakamaSessionManager.deviceKey)
    if deviceId == nil {
      deviceId = UIDevice.current.identifierForVendor!.uuidString
      NakamaSessionManager.defaults.set(deviceId!, forKey: NakamaSessionManager.deviceKey)
    }

    let message = AuthenticateMessage(device: deviceId!)
    client.login(with: message).then { session in
      NakamaSessionManager.defaults.set(session.token, forKey: NakamaSessionManager.sessionKey)
      self.restoreSessionAndConnect()
    }.catch{ err in
      if (err is NakamaError) {
        switch err as! NakamaError {
        case .userNotFound(_):
          let _ = self.client.register(with: message)
          return
        default:
          break
        }
      }
      print("Could not login: %@", err)
    }
  }

  func restoreSessionAndConnect() {
    // Lets check if we can restore a cached session

    let sessionString : String? = NakamaSessionManager.defaults.string(forKey: NakamaSessionManager.sessionKey)
    if sessionString == nil {
      return
    }

    let _session = DefaultSession.restore(token: sessionString!)
    if _session.isExpired(currentTimeSince1970: Date().timeIntervalSince1970) {
      return
    }

    client.connect(to: _session).then { validSession in
      self.session = _session

      // Store session for quick reconnects.
      NakamaSessionManager.defaults.set(_session.token, forKey: NakamaSessionManager.sessionKey)
    }.catch{ err in
      print("Failed to connect to server: %@", err)
    }
  }
}
```
