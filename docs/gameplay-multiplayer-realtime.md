# Realtime Multiplayer

The realtime multiplayer engine makes it easy for users to set up and join matches where they can rapidly exchange data with opponents.

Any user can participate in matches with other users. Users can create, join, and leave matches with messages sent from clients. A match exists on the server until its last participant has left.

Any data sent through a match is immediately routed to all other match opponents. Matches are kept in-memory but can be persisted as needed.

## Create a match

A match can be created by a user. The server will assign a unique ID which can be shared with other users for them to [join the match](#join-a-match). All users within a match are equal and it is up to the clients to decide on a host.

```js fct_label="Javascript"
var match = await socket.send({ match_create: {} });
console.log("Created match with ID %o", match.id);
```

```csharp fct_label="Unity"
// Requires Nakama 1.x
var message = NMatchCreateMessage.Default();
client.Send(message, (INMatch match) => {
  string id = match.Id;
  Debug.Log("Successfully created match.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

A user can [leave a match](#leave-a-match) at any point which will notify all other users.

## Join a match

A user can join a specific match by ID. Matches can be joined at any point until the last participant leaves.

!!! Hint
    To find a match instead of specify one by ID use the [matchmaker](gameplay-matchmaker.md).

```js fct_label="Javascript"
var id = "match ID to join";

var match = await socket.send({ match_join: { match_id: id } });
var connectedOpponents = match.presences.filter((presence) => {
  // Remove your own user from list.
  return presence.user_id != match.self.user_id;
});

connectedOpponents.forEach((opponent) => {
  console.log("User id %o, username %o.", opponent.user_id, opponent.username);
});
```

```csharp fct_label="Unity"
// Requires Nakama 1.x
string id = match.Id; // an INMatch Id.

var message = NMatchJoinMessage.Default(id);
client.Send(message, (INResultSet<INMatch> matches) => {
  Debug.Log("Successfully joined match.");

  IList<INUserPresence> connectedOpponents = new List<INUserPresence>();
  // Add list of connected opponents.
  connectedOpponents.AddRange(matches.Results[0].Presence);
  // Remove your own user from list.
  connectedOpponents.Remove(matches.Results[0].Self);

  foreach (var presence in connectedOpponents) {
    var userId = presence.UserId;
    var handle = presence.Handle;
    Debug.LogFormat("User id '{0}' handle '{1}'.", userId, handle);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

The list of match opponents returned in the success callback might not include all users. It contains users who are connected to the match so far.

## List opponents

When a user joins a match they receive an initial list of connected opponents. As other users join or leave the server will push events to clients which can be used to update the list of connected opponents.

```js fct_label="Javascript"
var connectedOpponents = [];

client.onmatchpresence = (presences) => {
  // Remove all users who left.
  connectedOpponents = connectedOpponents.filter(function(co) {
    var stillConnectedOpponent = true;
    presences.leaves.forEach((leftOpponent) => {
      if (leftOpponent.user_id == co.user_id) {
        stillConnectedOpponent = false;
      }
    });
    return stillConnectedOpponent;
  });

  // Add all users who joined.
  connectedOpponents = connectedOpponents.concat(presences.joins);
};
```

```csharp fct_label="Unity"
// Requires Nakama 1.x
IList<INUserPresence> connectedOpponents = new List<INUserPresence>();

client.OnMatchPresence = (INMatchPresence presences) => {
  // Remove all users who left.
  foreach (var user in presences.Leave) {
    connectedOpponents.Remove(user);
  }

  // Add all users who joined.
  connectedOpponents.AddRange(presences.Join);
};
```

No server updates are sent if there are no changes to the presence list.

## Send data messages

A user in a match can send data messages which will be received by all other opponents. These messages are streamed in realtime to other users and can contain any binary content. To identify each message as a specific "command" it contains an Op code as well as the payload.

An Op code is a numeric identifier for the type of message sent. These can be used to define commands within the gameplay which belong to certain user actions.

The binary content in each data message should be as __small as possible__. It is common to use JSON or preferable to use a compact binary format like <a href="https://developers.google.com/protocol-buffers/" target="\_blank">Protocol Buffers</a> or <a href="https://google.github.io/flatbuffers/" target="\_blank">FlatBuffers</a>.

```js fct_label="Javascript"
var id = "match ID to send to";
var opCode = 1;
var data = {"move": {"dir": "left", "steps": 4}};
socket.send({ match_data_send: {match_id: id, op_code: opCode, data: payload} });
```

```csharp fct_label="Unity"
// Requires Nakama 1.x
string id = match.Id; // an INMatch Id.

long opCode = 001L;
byte[] data = Encoding.UTF8.GetBytes("{\"move\": {\"dir\": \"left\", \"steps\": 4}}");

var message = NMatchDataSendMessage.Default(id, opCode, data);
client.Send(message, (bool done) => {
  Debug.Log("Successfully sent data message.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

## Receive data messages

A client can add a callback for incoming match data messages. This should be done before they create (or join) and leave a match.

!!! Note "Message sequences"
    The server delivers data in the order it processes data messages from clients.

```js fct_label="Javascript"
client.onmatchdata = (data) => {
  var content = data.data;
  switch (data.op_code) {
    case 101:
      console.log("A custom opcode.");
      break;
    default:
      console.log("User %o sent %o", data.presence.user_id, content);
  }
};
```

```csharp fct_label="Unity"
// Requires Nakama 1.x
client.OnMatchData = (INMatchData m) => {
  var content = Encoding.UTF8.GetString(m.Data);
  switch (m.OpCode) {
  case 101L:
    Debug.Log("A custom opcode.");
    break;
  default:
    Debug.LogFormat("User handle '{0}' sent '{1}'", m.Presence.Handle, content);
  };
};
```

## Leave a match

Users can leave a match at any point. A match ends when all users have left.

```js fct_label="Javascript"
var id = "match ID to leave";
socket.send({ match_leave: {match_id: id}});
```

```csharp fct_label="Unity"
// Requires Nakama 1.x
string id = match.Id; // an INMatch Id.

var message = NMatchLeaveMessage.Default(id);
client.Send(message, (bool complete) => {
  Debug.Log("Successfully left match.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

!!! Note
    When all opponents leave a match it's ID becomes invalid and cannot be re-used to join again.
