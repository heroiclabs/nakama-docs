# JavaScript client guide

The official JavaScript client handles all communication in realtime with the server. It implements all features in the server and can be used in a browser and be embedded inside React Native applications. The client also comes with type definition so it can be used via TypeScript.

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

&nbsp;&nbsp; 2\. Authenticate a user. By default, Nakama will try and create a user if it doesn't exist.

If you are in an environment that supports `localStorage` then use the following code to store the session:

```js
const email = "hello@example.com";
const password = "somesupersecretpassword";

try {
  var session = await client.authenticateEmail({ email: email, password: password });

  // Store session for quick reconnects.
  localStorage.nakamaToken = session.token;
  console.log("Authenticated successfully. User ID: %o", session.user_id);
} catch(e) {
  console.log("An error occured while trying to authenticate user: %o", e)
}
```

For React Native, you should use the following:

```js
const customId = "someuniqueidentifier";

try {
  var session = await client.authenticateCustom({ id: customId });

  // Store session for quick reconnects.
  // This returns a promise - if it fails, we are catching the error in the `error` callback below
  return AsyncStorage.setItem('@MyApp:nakamaToken', session.token);
  console.log("Authenticated successfully. User ID: %o", session.user_id);
} catch(e) {
  console.log("An error occured while trying to authenticate user: %o", e)
}
```

In the code above we use `authenticateEmail` but for other authentication options have a look at the [code examples](authentication.md#register-or-login).

A __full example__ with all code above is [here](#full-example).

## Send messages

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](social-friends.md), join [groups](social-groups-clans.md), or submit scores in [leaderboards](gameplay-leaderboards.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

The server also provides a [storage engine](storage-collections.md) to keep save games and other records owned by users. We'll use storage to introduce how messages are sent.

```js
var data = [
  {
    "collection": "collection",
    "key": "key1",
    "value": {"jsonKey": "jsonValue"}
  },
  {
    "collection": "collection",
    "key": "key2",
    "value": {"jsonKey": "jsonValue"}
  }
];

try {
  var storageWriteAck = await client.writeStorageObjects(session, data);
  console.log("Storage write was successful - ack: %o", storageWriteAck);
} catch(e) {
  console.log("An error occured: %o", e)
}
```

Have a look at other sections of documentation for more code examples.

## Realtime data exchange

You can connect to the server over a realtime WebSocket connection to send and receive [chat messages](social-realtime-chat.md), get [notifications][social-in-app-notifications.md], and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

You first need to create a realtime socket to the server:

```js
const useSSL = false;
const verboseLogging = false;
const socket = client.createSocket(useSSL, verboseLogging);
await socket.connect(session);
```

Then proceed to join a chat channel and send a message:

```js

socket.onchannelmessage = (channelMessage) => {
  console.log("Received chat message: %o", channelMessage);
}

const channelId = "pineapple-pizza-lovers-room";
var channel = await socket.send({
  channel_join: {
    type: 1, //1 = room, 2 = Direct Message, 3 = Group
    target: channelId,
    persistence: false,
    hidden: false
  }
});

console.log("Successfully joined channel - channel: %o", channel);

var messageAck = await socket.send({
  channel_message_send: {
    channel_id: channel.id,
    content: {"message": "Pineapple doesn't belong on a pizza!"}
  }
})

console.log("Successfully sent chat message - ack: %o", messageAck);
```

There is a lot more information about various chat feature available [here](social-in-app-notifications.md).

## Handle events

The client has callbacks which are called on various events received from the server.

```js
client.ondisconnect = function(event) {
  console.log("Disconnected from the server. Event: %o", event);
};

client.onnotification = function(notification) {
  console.log("Recieved a live notification: %o", notification);
};

client.onchannelpresence = function(presence) {
  console.log("Recieved a presence update: %o", presence);
};

client.onchannelmessage = function(message) {
  console.log("Recieved a new chat message: %o", message);
}

socket.onmatchdata(matchData) {
  console.log("Recieved a match data: %o", matchData);
}

socket.onmatchpresence(matchPresence) {
  console.log("Recieved a match presence update: %o", matchPresence);
}

socket.onmatchmakermatched(matchmakerMatched) {
  console.log("Recieved a matchmaker update: %o", matchmakerMatched);
}

socket.onstatuspresence(statusPresence) {
  console.log("Recieved a status presence update: %o", statusPresence);
}

socket.onstreampresence(streamPresence) {
  console.log("Recieved a stream presence update: %o", streamPresence);
}

socket.onstreamdata(streamData) {
  console.log("Recieved a stream data: %o", streamData);
}
```

Some events only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| ondisconnect | Handles an event for when the client is disconnected from the server. |
| onerror | Receives events about server errors. |
| onnotification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| onchannelmessage | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| onchannelpresence | It handles join and leave events within [chat](social-realtime-chat.md). |
| onmatchdata | Receives [realtime multiplayer](gameplay-multiplayer-realtime.md) match data. |
| onmatchpresence | It handles join and leave events within [realtime multiplayer](gameplay-multiplayer-realtime.md). |
| onmatchmakermatched | Received when the [matchmaker](gameplay-matchmaker.md) has found a suitable match. |
| onstatuspresence | It handles status updates when subscribed to a user [status feed](social-status.md). |
| onstreampresence | Receives [stream](social-stream.md) join and leave event. |
| onstreamdata | Receives [stream](social-stream.md) data sent by the server. |

## Logs and errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code. To log all messages sent by the client you can enable `verbose` when you build a `client`.

```js
const verboseLogging = true;
const useSSL = false;
var client = new nakamajs.Client("defaultkey");
client.verbose = verboseLogging;
client.createSocket(useSSL, verboseLogging);
```

---

## Full example

An example class used to manage a session with the Unity client.

```js

var client = new nakamajs.Client("defaultkey");
var currentSession = null;

function storeSession(session) {
  if (typeof(Storage) !== "undefined") {
    localStorage.setItem("nakamaToken", session.token);
    console.log("Session stored.");
  } else {
    // We are assuming that you are using React Native...
    AsyncStorage.setItem('@MyApp:nakamaToken', session.token).then(function(session) {
      console.log("Session stored.");
    }).catch(function(error) {
      console.log("An error occured while storing session: %o", error);
    })
  };
}

async function getSessionFromStorage() {
  if (typeof(Storage) !== "undefined") {
    return Promise.resolve(localStorage.getItem("nakamaToken"));
  } else {
    try {
      // We are assuming that you are using React Native...
      return AsyncStorage.getItem('@MyApp:nakamaToken');
    } catch(e) {
      console.log("Could not fetch data, error: %o", error);
    }
  }
}

async function restoreSessionOrAuthenticate() {
  const email = "hello@example.com";
  const password = "somesupersecretpassword";
  var session = null;
  try {
    var sessionString = await getSessionFromStorage();
    if (sessionString && sessionString != "") {
      session = nakamajs.Session.restore(sessionString);
      var currentTimeInSec = new Date() / 1000;
      if (!session.isexpired(currentTimeInSec)) {
        console.log("Restored session. User ID: %o", session.user_id);
        return Promise.resolve(session);
      }
    }

    var session = await client.authenticateEmail({ email: email, password: password });
    storeSession(session);

    console.log("Authenticated successfully. User ID: %o", session.user_id);
    return Promise.resolve(session);
  } catch(e) {
    console.log("An error occured while trying to restore session or authenticate user: %o", e)
  }
}

// Let's begin here...
restoreSessionOrAuthenticate().then(function(session) {
  currentSession = session;
  return client.writeStorageObjects(currentSession, [{
    "collection": "collection",
    "key": "key1",
    "value": {"jsonKey": "jsonValue"}
  }]);
}).then(function(writeAck) {
  console.log("Storage write was successful - ack: %o", writeAck);
}).catch(function(error) {
  console.log("An error occured: %o", e)
});
```

