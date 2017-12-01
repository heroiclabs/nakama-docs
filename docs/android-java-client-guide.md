# Android/Java client guide

The official Java client handles all communication in realtime with the server and is specifically optimized for Android projects. It implements all features in the server and is compatible with Java 1.7+ and Android 2.3+. To work with the Java client you'll need a build tool like <a href="https://gradle.org/" target="\_blank">Gradle</a> and an editor/IDE like <a href="https://www.jetbrains.com/idea/" target="\_blank">IntelliJ</a> or <a href="https://eclipse.org/ide/" target="\_blank">Eclipse</a>.

## Download

The client can be downloaded from <a href="https://github.com/heroiclabs/nakama-java/releases/latest" target="\_blank">GitHub releases</a>. You can download "nakama-java-$version.jar" or "nakama-java-$version-all.jar" which includes a shadowed copy of all dependencies. If you use a build tool like Gradle you can skip the download and fetch it from the [central repository](#install-and-setup).

For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-java/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Bug "Help and contribute"
    The Java client is <a href="https://github.com/heroiclabs/nakama-java" target="\_blank">open source on GitHub</a>. Please report issues and contribute code to help us improve it.

## Install and setup

When you've downloaded the jar package you should include it in your project or if you use Gradle add the client as a dependency to your "build.gradle".

```groovy
repositories {
    maven {
        url 'https://dl.bintray.com/heroiclabs/default/'
    }
}

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
    client = DefaultClient.builder("defaultkey")
        .host("127.0.0.1")
        .port(7350)
        .ssl(false)
        .build();
  }
}
```

We use the builder pattern with many classes in the Java client. Most classes have a shorthand ".defaults()" method to construct an object with default values.

!!! Note
    By default the client uses connection settings "127.0.0.1" and 7350 to connect to a local Nakama server.

```java
// Quickly setup a client for a local server.
Client client = DefaultClient.defaults("defaultkey");
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
Client client = DefaultClient.defaults("defaultkey");
```

&nbsp;&nbsp; 2\. Login or register a user.

!!! Tip
    It's good practice to cache a device identifier on Android when it's used to authenticate because they can change with device OS updates.

```java
String id = UUID.randomUUID().toString();
AuthenticateMessage message = AuthenticateMessage.Builder.device(id);
Deferred<Session> deferred = client.login(message)
deferred.addCallbackDeferring(new Callback<Deferred<Session>, Session>() {
  @Override
  public Deferred<Session> call(Session session) throws Exception {
    return client.connect(session);
  }
}).addErrback(new Callback<Deferred<Session>, Error>() {
  @Override
  public Deferred<Session> call(Error err) throws Exception {
    if (err.getCode() == Error.ErrorCode.USER_NOT_FOUND) {
      System.out.println("User not found, we'll register the user.");
      return client.register(message);
    }
    throw err;
  }
}).addCallbackDeferring(new Callback<Deferred<Session>, Session>() {
  @Override
  public Deferred<Session> call(Session session) throws Exception {
    return client.connect(session);
  }
}).addCallback(new Callback<Session, Session>() { // See step (3).
  @Override
  public Session call(Session session) throws Exception {
    System.out.format("Session connected: '%s'", session.getToken());
    return session;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
```

In the code above we use `AuthenticateMessage.Builder.device(id)` but for other authentication options have a look at the [code examples](authentication.md#register-or-login).

The client uses [promise chains](#promise-chains) for an easy way to execute asynchronous callbacks.

&nbsp;&nbsp; 3\. Store session for quick reconnects.

We can replace the callback marked in step 2 with a callback which stores the session object on Android.

```java
Callback<Session, Session> callback = new Callback<Session, Session>() {
  @Override
  public Session call(Session session) throws Exception {
    System.out.format("Session connected: '%s'", session.getToken());
    // Android code.
    SharedPreferences pref = getActivity().getPreferences(Context.MODE_PRIVATE);
    SharedPreferences.Editor editor = pref.edit();
    editor.putString("nk.session", session.getToken());
    editor.commit();
    return session;
  }
};
```

A __full example__ class with all code above is [here](#full-android-example).

## Send messages

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](social-friends.md), join [groups](social-groups-clans.md) and [chat](social-realtime-chat.md), or submit scores in [leaderboards](gameplay-leaderboards.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

The server also provides a [storage engine](storage-collections.md) to keep preferences and other records owned by users. We'll use storage to introduce how messages are sent.

```java
String json = "{\"jsonkey\":\"jsonvalue\"}";

CollatedMessage<ResultSet<RecordId>> message = StorageWriteMessage.Builder.newBuilder()
    .record("someBucket", "someCollection", "myRecord", json)
    .build();
Deferred<ResultSet<RecordId>> deferred = client.send(message)
deferred.addCallback(new Callback<ResultSet<RecordId>, ResultSet<RecordId>>() {
  @Override
  public ResultSet<RecordId> call(ResultSet<RecordId> list) throws Exception {
    for (RecordId recordId : list) {
      String version = new String(recordId.getVersion());
      System.out.format("Record stored '%s'", version);
    }
    return list;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
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

The client uses promises to represent asynchronous actions with the <a href="http://tsunanet.net/~tsuna/async/1.0/com/stumbleupon/async/Deferred.html" target="\_blank">Deferred</a> library. This makes it easy to chain together actions which must occur in sequence and handle errors.

A deferred object is created when messages are sent and can attach callbacks and error handlers for working on the deferred result.

```java
Callback<Session, Session> callback = new Callback<Session, Session>() {
  @Override
  public Session call(Session session) throws Exception {
    System.out.format("Session connected: '%s'", session.getToken());
    return session;
  }
};

Callback<Error, Error> errback = new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
};
```

You can chain callbacks because each callback returns a `Deferred<T>`. Each method on the `Client` returns a `Deferred<T>` so you can chain calls with `deferred.addCallbackDeferring(...)`.

```java
String id = UUID.randomUUID().toString();
AuthenticateMessage message = AuthenticateMessage.Builder.device(id);
Deferred<Session> deferred = client.register(message)
deferred.addCallbackDeferring(new Callback<Deferred<Session>, Session>() {
  @Override
  public Deferred<Session> call(Session session) throws Exception {
    return client.connect(session);
  }
}).addCallback(new Callback<Session, Session>() {
  @Override
  public Session call(Session session) throws Exception {
    System.out.format("Session connected: '%s'", session.getToken());
    return session;
  }
});
```

## Logs and errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code. To log all messages sent by the client you can enable "trace" when you build a `"Client"`.

```java
Client client = DefaultClient.builder("defaultkey")
    .trace(true)
    .build();
```

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

Every error in the Java client implements the `"Error"` class. It contains details on the source and content of an error:

```java
Callback<Error, Error> errback = new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error code '%s'", err.getCode());
    System.err.format("Error message '%s'", err.getMessage());
    return err;
  }
};
```

## Full Android example

An example class used to manage a session with the Java client.

```java
package com.heroiclabs.nakama.example;

import android.app.Activity;
import android.content.Context;
import android.content.SharedPreferences;

import com.heroiclabs.nakama.*;
import com.heroiclabs.nakama.Error;
import com.heroiclabs.nakama.Error.ErrorCode;
import com.stumbleupon.async.*;

public class NakamaSessionManager {
  private final Client client;
  private final Callback<Error, Error> errback;
  private Session session;

  public NakamaSessionManager() {
    client = DefaultClient.builder("defaultkey").build();
    errback = new Callback<Error, Error>() {
      @Override
      public Error call(Error err) throws Exception {
        System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
        return err;
      }
    };
  }

  /** Get the Client reference to send/receive messages from the server. */
  public Client getClient() {
    return client;
  }

  /**
   * Login (or register) and connect a user using a device ID.
   * @param activity The Activity calling this method.
   * @param deviceId The Device ID to login with.
   */
  public void connect(final Activity activity, String deviceId) {
    final AuthenticateMessage message = AuthenticateMessage.Builder.device(deviceId);
    client.login(message)
      .addCallbackDeferring(new Callback<Deferred<Session>, Session>() {
        @Override
        public Deferred<Session> call(Session session) throws Exception {
          // Login was successful.
          // Store the session for later use.
          SharedPreferences pref = activity.getPreferences(Context.MODE_PRIVATE);
          pref.edit()
            .putString("nk.session", session.getToken())
            .apply();

          return client.connect(session);
        }
      })
      .addErrback(new Callback<Deferred<Session>, Error>() {
        @Override
        public Deferred<Session> call(Error err) throws Exception {
          if (err.getCode() == ErrorCode.USER_NOT_FOUND) {
            // Login failed because this is a new user.
            // Let's register instead.
            System.out.println("User not found, registering.");
            return client.register(message);
          }
          throw err;
        }
      })
      .addCallbackDeferring(new Callback<Deferred<Session>, Session>() {
        @Override
        public Deferred<Session> call(Session session) throws Exception {
          // Registration has succeeded, try connecting again.
          // Store the session for later use.
          SharedPreferences pref = activity.getPreferences(Context.MODE_PRIVATE);
          pref.edit().putString("nk.session", session.getToken()).apply();

          return client.connect(session);
        }
      })
      .addCallback(new Callback<Session, Session>() {
        @Override
        public Session call(Session session) throws Exception {
          // We're connected to the server!
          System.out.println("Connected!");
          return session;
        }
      })
      .addErrback(errback);
  }

  /**
   * Attempt to restore a Session from SharedPreferences and connect.
   * @param activity The Activity calling this method.
   */
  public void restoreSessionAndConnect(Activity activity) {
    SharedPreferences pref = activity.getPreferences(Context.MODE_PRIVATE);
    // Lets check if we can restore a cached session.
    String sessionString = pref.getString("nk.session", null);
    if (sessionString == null || sessionString.isEmpty()) {
      return; // We have no session to restore.
    }

    Session session = DefaultSession.restore(sessionString);
    if (session.isExpired(System.currentTimeMillis())) {
      return; // We can't restore an expired session.
    }

    final NakamaSessionManager self = this;
    client.connect(session)
      .addCallback(new Callback<Session, Session>() {
        @Override
        public Session call(Session session) throws Exception {
          System.out.format("Session connected: '%s'.", session.getToken());
          self.session = session;
          return session;
        }
      });
  }
}
```

## Client reference

You can find the Java Client Reference [here](https://heroiclabs.github.io/nakama-java/current/).
