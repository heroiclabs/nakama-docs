# Cocos2d-x JavaScript Client Guide

The official cocos2d JavaScript client handles all communication in realtime with the server. It implements all features in the server and can be used in a cocos2d-x js projects. The client also comes with type definition so it can be used via TypeScript.

## Download

Download latest release from the [GitHub releases page](https://github.com/heroiclabs/nakama-cocos2d-x-javascript/releases/latest) which contains the Nakama-js module with UMD module loader and polyfill library.

For upgrades you can see changes and enhancements in the [CHANGELOG](https://github.com/heroiclabs/nakama-cocos2d-x-javascript/blob/master/CHANGELOG.md) before you update to newer versions.

!!! Bug "Help and contribute"
    The JavaScript client is [open source](https://github.com/heroiclabs/nakama-js) on GitHub. Please report issues and contribute code to help us improve it.

## Setup

When you've [downloaded](#download) a release, extract it to your `src` folder.

Import into your `project.json`:

```json
"jsList" : [
  ...
  "src/NakamaSDK/ThirdParty/polyfill.js",
  "src/NakamaSDK/nakama-js.umd.js"
]
```

The client object is used to execute all logic against the server:

```js
var serverkey = "defaultkey";
var host = "127.0.0.1";
var port = "7350";
var useSSL = false;
var timeout = 7000; // ms

var client = new nakamajs.Client(serverkey, host, port, useSSL, timeout);
```

!!! Note
    By default the client uses connection settings "127.0.0.1" and "7350" port to connect to a local Nakama server.

```js
var client = new nakamajs.Client();
```

!!! Bug
    `await` and promises JS features are not supported by cocos2d. You have to use promises which are provided by `polyfill` third party library.

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user]((../concepts/user-accounts.md) with one of the [authenticate options](../concepts/authentication.md).

To authenticate you should follow our recommended pattern in your client code.

By default Nakama will try to create a user if it doesn't exist.

Use the following code to store the session:

```js
const email = "hello@example.com";
const password = "somesupersecretpassword";

client.authenticateEmail(email, password).then(function(session) {
        cc.log("Authenticated successfully. User id:", session.user_id);
        // Store session token for quick reconnects.
        cc.sys.localStorage.setItem("nakamaToken", session.token);
    },
    function(error) {
        cc.error("authenticate failed:", JSON.stringify(error));
    });
```

In the code above we use `authenticateEmail` but for other authentication options have a look at the [code examples](../concepts/authentication.md).

A __full example__ with all code above is [here](#full-example).

## Send messages

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](../concepts/friends.md), join [groups](../concepts/groups-clans.md), or submit scores in [leaderboards](../concepts/leaderboards.md). You can also execute remote code on the server via [RPC](../server-framework/basics.md).

The server also provides a [storage engine](../concepts/collections.md) to keep save games and other records owned by users. We'll use storage to introduce how messages are sent.

```js
const objects = [{
  "collection": "collection",
  "key": "key1",
  "value": {"jsonKey": "jsonValue"}
}, {
  "collection": "collection",
  "key": "key2",
  "value": {"jsonKey": "jsonValue"}
}];
client.writeStorageObjects(session, objects)
  .then(function(storageWriteAck) {
        cc.log("Storage write was successful:", JSON.stringify(storageWriteAck));
    },
    function(error) {
        cc.error("Storage write failed:", JSON.stringify(error));
    });
```

Have a look at other sections of documentation for more code examples.

## Realtime data exchange

You can connect to the server over a realtime WebSocket connection to send and receive [chat messages](../concepts/realtime-chat.md), get [notifications](../concepts/in-app-notifications.md), and [matchmake](../concepts/matches.md) into a [multiplayer match](../concepts/client-relayed-multiplayer.md). You can also execute remote code on the server via [RPC](../server-framework/basics.md).

You first need to create a realtime socket to the server:

```js
const useSSL = false;
const verboseLogging = false;
const createStatus = false;    // set `true` to send presence events to subscribed users.
const socket = client.createSocket(useSSL, verboseLogging);
var session = ""; // obtained by authentication.
socket.connect(session, createStatus)
  .then(
        function() {
            cc.log("connected");
        },
        function(error) {
            cc.error("connect failed:", JSON.stringify(error));
        }
    );
```

Then proceed to join a chat channel and send a message:

```js
socket.onchannelmessage = function (channelMessage) {
  cc.log("Received chat message:", JSON.stringify(channelMessage));
};

const channelId = "pineapple-pizza-lovers-room";
const persistence = false;
const hidden = false;

socket.joinChat(channelId, 1, persistence, hidden).then(
      function(response) {
          cc.log("Successfully joined channel:", response.channel.id);
          sendChatMessage(response.channel.id);
      },
      function(error) {
          cc.error("join channel failed:", JSON.stringify(error));
      }
  );

function sendChatMessage(channelId) {
  socket.writeChatMessage(channelId, {"message": "Pineapple doesn't belong on a pizza!"}).then(
      function(messageAck) {
          cc.log("Successfully sent chat message:", JSON.stringify(messageAck));
      },
      function(error) {
          cc.error("send message failed:", JSON.stringify(error));
      }
  );
}
```

You can find more information about the various chat features available [here](../concepts/in-app-notifications.md).

## Handle events

A client socket has event listeners which are called on various events received from the server.

```js
socket.ondisconnect = function (event) {
  cc.log("Disconnected from the server. Event:", JSON.stringify(event));
};
socket.onnotification = function (notification) {
  cc.log("Received notification:", JSON.stringify(notification));
};
socket.onchannelpresence = function (presence) {
  cc.log("Received presence update:", JSON.stringify(presence));
};
socket.onchannelmessage = function (message) {
  cc.log("Received new chat message:", JSON.stringify(message));
};
socket.onmatchdata = function (matchdata) {
  cc.log("Received match data: ", JSON.stringify(matchdata));
};
socket.onmatchpresence = function (matchpresence) {
  cc.log("Received match presence update:", JSON.stringify(matchpresence));
};
socket.onmatchmakermatched = function (matchmakerMatched) {
  cc.log("Received matchmaker update:", JSON.stringify(matchmakerMatched));
};
socket.onstatuspresence = function (statusPresence) {
  cc.log("Received status presence update:", JSON.stringify(statusPresence));
};
socket.onstreampresence = function (streamPresence) {
  cc.log("Received stream presence update:", JSON.stringify(streamPresence));
};
socket.onstreamdata = function (streamdata) {
  cc.log("Received stream data:", JSON.stringify(streamdata));
};
```

Some events only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| onDisconnect | Received when the client is disconnected from the server. |
| onNotification | Receives live [in-app notifications](../concepts/in-app-notifications.md) sent from the server. |
| onChannelMessage | Receives [realtime chat](../concepts/realtime-chat.md) messages sent by other users. |
| onChannelPresence | Receives join and leave events within [chat](../concepts/realtime-chat.md). |
| onMatchState | Receives [realtime multiplayer](../concepts/client-relayed-multiplayer.md) match data. |
| onMatchPresence | Receives join and leave events within [realtime multiplayer](../concepts/client-relayed-multiplayer.md). |
| onMatchmakerMatched | Received when the [matchmaker](../concepts/matches.md) has found a suitable match. |
| onStatusPresence | Receives status updates when subscribed to a user [status feed](../concepts/status.md). |
| onStreamPresence | Receives [stream](../server-framework/streams.md) join and leave event. |
| onStreamState | Receives [stream](../server-framework/streams.md) data sent by the server. |

## Logs and errors

The [server](../getting-started/configuration.md#log) and the client can generate logs which are helpful to debug code. To log all messages sent by the client you can enable `verbose` when you build a `client`.

```js
const verboseLogging = true;
const useSSL = false;
var client = new nakamajs.Client("defaultkey");
client.verbose = verboseLogging;
socket = client.createSocket(useSSL, verboseLogging);
```

---

## Full example

An example class used to manage a session with the cocos2d-x JavaScript client.

```js
var client = new nakamajs.Client("defaultkey");
var currentSession = null;

function storeSession(session) {
  cc.sys.localStorage.setItem("nakamaToken", session.token);
  cc.log("Session stored.");
}

function getSessionFromStorage() {
  return cc.sys.localStorage.getItem("nakamaToken");
}

function restoreSessionOrAuthenticate() {
const email = "hello@example.com";
  const password = "somesupersecretpassword";
  var session = null;
  try {
    var sessionString = getSessionFromStorage();
    if (sessionString && sessionString !== "") {
      session = nakamajs.Session.restore(sessionString);
      var currentTimeInSec = new Date() / 1000;
      if (!session.isexpired(currentTimeInSec)) {
        cc.log("Restored session. User ID: ", session.user_id);
        return Promise.resolve(session);
      }
    }

    return new Promise(function(resolve, reject) {
      client.authenticateEmail(email, password)
        .then(function(session) {
            storeSession(session);
            cc.log("Authenticated successfully. User ID:", session.user_id);
            resolve(session);
          },
          function(error) {
            cc.error("authenticate failed:", JSON.stringify(error));
            reject(error);
          });
    });
  } catch(e) {
    cc.log("An error occurred while trying to restore session or authenticate user:", JSON.stringify(e));
    return Promise.reject(e);
  }
}

restoreSessionOrAuthenticate().then(function(session) {
  currentSession = session;
  return client.writeStorageObjects(currentSession, [{
    "collection": "collection",
    "key": "key1",
    "value": {"jsonKey": "jsonValue"}
  }]);
}).then(function(writeAck) {
  cc.log("Storage write was successful - ack:", JSON.stringify(writeAck));
}).catch(function(e) {
  cc.log("An error occured:", JSON.stringify(e));
});
```
