# JavaScript client guide

The official JavaScript client handles all communication in realtime with the server. It implements all features in the server and can be used in a browser and be embdedded inside React Native applications.

## Download

The client is available on <a href="https://www.npmjs.com/package/nakama-js" target="\_blank">NPM</a>, Bower and on the <a href="https://github.com/heroiclabs/nakama-js/releases/latest" target="\_blank">GitHub releases page</a>. You can download `nakama-js.umd.js` which contains the Nakama-js module and UMD module loader.

If you are using NPM to manage your dependencies, simply add the following to your `package.json`:
```json
//...
"dependencies": {
  "@heroiclabs/nakama-js": "^0.1.0"
}
//...
```

If you are using Bower, add the following to your `bower.json`:
```json
//...
"dependencies": {
  "@heroiclabs/nakama-js": "^0.1.0"
}
//...
```

For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-js/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Bug "Help and contribute"
    The JavaScript client is <a href="https://github.com/heroiclabs/nakama-js" target="\_blank">open source on GitHub</a>. Please report issues and contribute code to help us improve it.

## Install and setup

When you've [downloaded](#download) the "Nakama-js.umd.js" file you should import it into your project to use it.

```html
<script src="path/to/dist/nakama-js.umd.js"></script>
```

The client object is used to execute all logic against the server. In your main JavaScript function, you'll need to instatiate a Client object:

```js
var client = new nakamajs.Client("defaultkey", "127.0.0.1", 7350);
client.ssl = false;
```

!!! Note
    By default the client uses connection settings "127.0.0.1" and 7350 to connect to a local Nakama server.

```js
// Quickly setup a client for a local server.
var client = new nakamajs.Client("defaultkey");
```

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user](user-accounts.md) with one of the [authenticate options](authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```js
// Quickly setup a client for a local server.
var client = new nakamajs.Client("defaultkey");
```

&nbsp;&nbsp; 2\. Write a callback which will be used to connect to the server.

If you are in an environment that supports `localStorage` then use the following code to store the session:

```js
var sessionHandler = function(session) {
  console.log("Session: %o", session);
  client.connect(session).then(function(session) {
    console.log("Session connected.");

    // Store session for quick reconnects.
    localStorage.nakamaToken = session.token;
  }).catch(function(error) {
    console.log("An error occured during session connection: %o", error);
  });
}
```

For React Native, you should use the following:

```js
var sessionHandler = function(session) {
  console.log("Session: %o", session);
  client.connect(session).then(function(session) {
    console.log("Session connected.");

    // Store session for quick reconnects.
    // This returns a promise - if it fails, we are catching the error in the `error` callback below
    return AsyncStorage.setItem('@MyApp:nakamaToken', session.token);
  }).catch(function(error) {
    console.log("An error occured during session connection: %o", error);
  });
}
```

&nbsp;&nbsp; 4\. Login or register a user.

```js
var errorHandler = function(error) {
  console.log("An error occured: %o", error);
};

var message = nakamajs.AuthenticateRequest.email("hello@world.com", "password");
client.login(message).then(sessionHandler).catch(function(error) {
   // USER_NOT_FOUND
  if (error.code == 5) {
      client.register(message).then(sessionHandler).catch(errorHandler);
  } else {
      errorHandler(error);
  }
});
```

In the code above we use `AuthenticateRequest.email(emailAddress, password)` but for other authentication options have a look at the [code examples](authentication.md#register-or-login).

A __full example__ with all code above is [here](#full-example).

## Send messages

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](social-friends.md), join [groups](social-groups-clans.md) and [chat](social-realtime-chat.md), or submit scores in [leaderboards](gameplay-leaderboards.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

The server also provides a [storage engine](storage-collections.md) to keep save games and other records owned by users. We'll use storage to introduce how messages are sent.

```js
var data = {
  "jsonKey": "jsonValue",
};
var message = new nakamajs.StorageWriteRequest();
message.write("someBucket", "someCollection", "myRecord", data);

client.send(message).then(function(result){
  console.log("Successfully wrote record.");
}).catch(errorHandler);
```

Have a look at other sections of documentation for more code examples.

## Handle events

The client has callbacks which are called on various events received from the server.

```js
client.ondisconnect = function(event) {
  console.log("Disconnected from the server. Event: %o", event);
};

client.onnotification = function(notification) {
  console.log("Recieved a live notification: %o", notification);
};

client.ontopicpresence = function(presence) {
  console.log("Recieved a presence update: %o", presence);
};

client.ontopicmessage = function(message) {
  console.log("Recieved a new chat message: %o", message);
}
```

Some events only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| ondisconnect | Handles an event for when the client is disconnected from the server. |
| onerror | Receives events about server errors. |
| onnotification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| ontopicmessage | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| ontopicpresence | It handles join and leave events within [chat](social-realtime-chat.md). |

## Logs and errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code. To log all messages sent by the client you can enable `verbose` when you build a `client`.

```js
var client = new nakamajs.Client("defaultkey");
client.verbose = true;
```

---

## Full example

An example class used to manage a session with the Unity client.

```js

var client = new nakamajs.Client("defaultkey");
var currentSession = null;

var errorHandler = function(error) {
  console.log("An error occured: %o", error);
};

var sessionHandler = function(session) {
  currentSession = session;

  console.log("Session: %o", session);
  client.connect(session).then(function(session) {
    console.log("Session connected.");

    // Store session for quick reconnects.
    // This returns a promise - if it fails, we are catching the error in the `error` callback below
    return AsyncStorage.setItem('@MyApp:nakamaToken', session.token);
  }).catch(function(error) {
    console.log("An error occured during session connection: %o", error);
  });
}

function restoreSessionAndConnect() {
  var sessionString = null;
  if (typeof(Storage) !== "undefined") {
    sessionString = localStorage.getItem("nakamaToken");
  } else {
    // We are assuming that you are using React Native...
    AsyncStorage.getItem('@MyApp:nakamaToken').then(function(token) {
        sessionString = token;
    }).catch(function(error){
        console.log("Could not fetch data, error: %o", error);
    });
  }

  // Lets check if we can restore a cached session.
  if (!sessionString || sessionString == "") {
    return
  }

  var session = nakamajs.Session.Restore(sessionString);
  if (session.isexpired(new Date())) {
    return; // We can't restore an expired session.
  }

  sessionHandler(session);
}

function loginOrRegister() {
  var message = nakamajs.AuthenticateRequest.email("hello@world.com", "password");
  client.login(message).then(sessionHandler).catch(function(error) {
     // USER_NOT_FOUND
    if (error.code == 5) {
        client.register(message).then(sessionHandler).catch(errorHandler);
    } else {
        errorHandler(error);
    }
  });
}

// Let's begin here...
restoreSessionAndConnect();
if (currentSession == null) {
  loginOrRegister();
}
```

