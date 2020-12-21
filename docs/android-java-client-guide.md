# Android/Java client guide

The official Java client handles all communication in realtime with the server and is specifically optimized for Android projects. It implements all features in the server and is compatible with Java 1.7+ and Android 2.3+. To work with the Java client you'll need a build tool like <a href="https://gradle.org/" target="\_blank">Gradle</a> and an editor/IDE like <a href="https://www.jetbrains.com/idea/" target="\_blank">IntelliJ</a> or <a href="https://eclipse.org/ide/" target="\_blank">Eclipse</a>.

## Download

The client can be downloaded from <a href="https://github.com/heroiclabs/nakama-java/releases/latest" target="\_blank">GitHub releases</a>. You can download "nakama-java-$version.jar" or "nakama-java-$version-all.jar" which includes a shadowed copy of all dependencies. <!-- If you use a build tool like Gradle you can skip the download and fetch it from the [central repository](#install-and-setup). -->

For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-java/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Bug "Help and contribute"
    The Java client is <a href="https://github.com/heroiclabs/nakama-java" target="\_blank">open source on GitHub</a>. Please report issues and contribute code to help us improve it.

## Setup

When you've downloaded the jar package you should include it in your project or if you use Gradle add the client as a dependency to your "build.gradle".

<!-- 	```groovy
	repositories {
	    maven {
	        url 'https://dl.bintray.com/heroiclabs/default/'
	    }
	}``` -->

```groovy
dependencies {
    compile(group: 'com.heroiclabs.nakama', name: 'client', version: '+')
    compile('org.slf4j:slf4j-api:1.7.25') {
    force = true // don't upgrade to "1.8.0-alpha2"
    }
}
```

The client object is used to execute all logic against the server.

```java
import com.heroiclabs.nakama.Client;

public class NakamaSessionManager {
    private final Client client;

    public NakamaSessionManager() {
    client = new DefaultClient("defaultkey", "127.0.0.1", 7349);
    }
}
```

We use the builder pattern with many classes in the Java client. Most classes have a shorthand ".defaults()" method to construct an object with default values.

!!! Note
    By default the client uses connection settings "127.0.0.1" and 7349 to connect to a local Nakama server.

```java
// Quickly setup a client for a local server.
Client client = new DefaultClient("defaultkey");
```

### For Android

Android uses a permissions system which determines which platform services the application will request to use and ask permission for from the user. The client uses the network to communicate with the server so you must add the "INTERNET" permission.

```xml
<uses-permission android:name="android.permission.INTERNET"/>
```

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user](user-accounts.md) with one of the [authenticate options](authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```java
Client client = new DefaultClient("defaultkey");
```

&nbsp;&nbsp; 2\. Authenticate a user. By default Nakama will try and create a user if it doesn't exist.

!!! Tip
    It's good practice to cache a device identifier on Android when it's used to authenticate because they can change with device OS updates.

```java
String id = UUID.randomUUID().toString();
Session session = client.authenticateDevice(id).get();
```

In the code above we use `authenticateDevice()` but for other authentication options have a look at the [code examples](authentication.md#register-or-login).

The client uses [ListenableFuture] from the popular Google Guava library for an easy way to execute asynchronous callbacks.

## Sessions

When authenticated the server responds with an auth token (JWT) which contains useful properties and gets deserialized into a `Session` object.

```java
System.out.println(session.getAuthToken()); // raw JWT token
System.out.format("User id: %s", session.getUserId());
System.out.format("User username: %s'", session.getUsername());
System.out.format("Session has expired: %s", session.isExpired(new Date()));
System.out.format("Session expires at: %s", session.getExpireTime()); // in seconds.
```

It is recommended to store the auth token from the session and check at startup if it has expired. If the token has expired you must reauthenticate. The expiry time of the token can be changed as a [setting](install-configuration.md#common-properties) in the server.

```java
System.out.format("Session connected: '%s'", session.getAuthToken());
// Android code.
SharedPreferences pref = getActivity().getPreferences(Context.MODE_PRIVATE);
SharedPreferences.Editor editor = pref.edit();
editor.putString("nk.session", session.getAuthToken());
editor.commit();
```

A __full example__ class with all code above is [here](#full-android-example).

## Send requests

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](social-friends.md), join [groups](social-groups-clans.md), or submit scores in [leaderboards](gameplay-leaderboards.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

All requests are sent with a session object which authorizes the client.

```java
Account account = client.getAccount(session).get();
System.out.format("User id %s'", account.getUser().getId());
System.out.format("User username %s'", account.getUser().getUsername());
System.out.format("Account virtual wallet %s'", account.getWallet());
```

Have a look at other sections of documentation for more code examples.

## Socket messages

The client can create one or more sockets with the server. Each socket can have it's own event listeners registered for responses received from the server.

!!! Note
    The socket is exposed on a different port on the server to the client. You'll need to specify a different port here to ensure that connection is established successfully.

```java
SocketClient socket = client.createWebSocket();

SocketListener listener = new AbstractSocketListener() {
    @Override
    public void onDisconnect(final Throwable t) {
    System.out.println("Socket disconnected.");
    }
};

socket.connect(session, listener).get();
System.out.println("Socket connected.");
```

You can connect to the server over a realtime socket connection to send and receive [chat messages](social-realtime-chat.md), get [notifications](social-in-app-notifications.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

To join a chat channel and receive messages:

```java
SocketListener listener = new AbstractSocketListener() {
    @Override
    public void onChannelMessage(final ChannelMessage message) {
    System.out.format("Received a message on channel %s", message.getChannelId());
    System.out.format("Message content: %s", message.getContent());
    }
};

socket.connect(session, listener).get();

final string roomName = "Heroes";
final Channel channel = socket.joinChat(roomName, ChannelType.ROOM).get();

final String content = "{\"message\":\"Hello world\"}";
ChannelMessageAck sendAck = socket.writeChatMessage(channel.getId(), content);
```

There are more examples for chat channels [here](social-realtime-chat.md).

## Handle events

A socket object has event handlers which are called on various messages received from the server.

```java
SocketListener listener = new AbstractSocketListener() {
    @Override
    public void onStatusPresence(final StatusPresenceEvent presence) {
    for (UserPresence userPresence : presence.getJoins()) {
        System.out.println("User ID: " + userPresence.getUserId() + " Username: " + userPresence.getUsername() + " Status: " + userPresence.getStatus());
    }

    for (UserPresence userPresence : presence.getLeaves()) {
        System.out.println("User ID: " + userPresence.getUserId() + " Username: " + userPresence.getUsername() + " Status: " + userPresence.getStatus());
    }
    }
};
```

Event handlers only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| onDisconnect | Received when the client is disconnected from the server. |
| onNotification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| onChannelMessage | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| onChannelPresence | Receives join and leave events within [chat](social-realtime-chat.md). |
| onMatchState | Receives [realtime multiplayer](gameplay-multiplayer-realtime.md) match data. |
| onMatchPresence | Receives join and leave events within [realtime multiplayer](gameplay-multiplayer-realtime.md). |
| onMatchmakerMatched | Received when the [matchmaker](gameplay-matchmaker.md) has found a suitable match. |
| onStatusPresence | Receives status updates when subscribed to a user [status feed](social-status.md). |
| onStreamPresence | Receives [stream](advanced-streams.md) join and leave event. |
| onStreamState | Receives [stream](advanced-streams.md) data sent by the server. |

## Logs and errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code.

The client uses <a href="https://www.slf4j.org/manual.html" target="\_blank">SLF4J</a> as the logging facade framework. This lets you choose whatever underlying logging framework you want to use. You should add one to your dependencies.

```groovy
dependencies {
    compile(group: 'org.slf4j', name: 'slf4j-simple', version: '1.7.+')
}
```

With Android you may want to use "slf4j-android" instead.

```groovy
dependencies {
    compile(group: 'org.slf4j', name: 'slf4j-android', version: '1.7.+')
}
```

Every error caused from within the `"SocketClient"` implements the `"Error"` class. It contains details on the source and content of an error:

```java
try {
    Match match = socket.createMatch().get();
} catch (ExecutionException e) {
    Error error = (Error) e.getCause();
    System.out.println("Error code: " +  error.getCode());
    System.out.println("Error message: " +  error.getMessage());
}
```

## Full Android example

An example class used to manage a session with the Java client.

```java
public class NakamaSessionManager {
    private final Client client = new DefaultClient("defaultkey");
    private Session session;

    public void start(final String deviceId) {
    SharedPreferences pref = activity.getPreferences(Context.MODE_PRIVATE);
    // Lets check if we can restore a cached session.
    String sessionString = pref.getString("nk.session", null);
    if (sessionString != null && !sessionString.isEmpty()) {
        Session restoredSession = DefaultSession.restore(sessionString);
        if (!restoredSession.isExpired(new Date())) {
        // Session was valid and is restored now.
        this.session = restoredSession;
        return;
        }
    }

    this.session = client.authenticateDevice(deviceId).get();
    // Login was successful.
    // Store the session for later use.
    SharedPreferences pref = activity.getPreferences(Context.MODE_PRIVATE);
    pref.edit().putString("nk.session", session.getAuthToken()).apply();
    System.out.println(session.getAuthToken());
    }
}
```

## Client reference

You can find the Java Client Reference [here](https://heroiclabs.github.io/nakama-java/current/).
