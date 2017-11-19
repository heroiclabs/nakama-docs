# Realtime Multiplayer

The realtime multiplayer engine makes it easy for users to set up and join matches where they can rapidly exchange data with opponents.

Any user can participate in matches with other users. Users can create, join, and leave matches with messages sent from clients. A match exists on the server until its last participant has left.

Any data sent through a match is immediately routed to all other match opponents. Matches are kept in-memory but can be persisted as needed.

## Create a match

A match can be created by a user. The server will assign a unique ID which can be shared with other users for them to [join the match](#join-a-match). All users within a match are equal and it is up to the clients to decide on a host.

```csharp fct_label="Unity"
var message = NMatchCreateMessage.Default();
client.Send(message, (INMatch match) => {
  string id = match.Id;
  Debug.Log("Successfully created match.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```js fct_label="Javascript"
var message = new nakamajs.MatchCreateRequest();
client.send(message).then(function() {
  var id = match.id;
  console.log("Successfully created match.");
}).catch(function(error){
  console.log("An error occured: %o", error);
});
```

A user can [leave a match](#leave-a-match) at any point which will notify all other users.

## Join a match

A user can join a specific match by ID. Matches can be joined at any point until the last participant leaves.

!!! Hint
    To find a match instead of specify one by ID use the [matchmaker](gameplay-matchmaker.md).

```csharp fct_label="Unity"
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

```js fct_label="Javascript"
var id = match.Id; // an INMatch Id.

var message = new nakamajs.MatchesJoinRequest();
message.matchIds.push(id);
client.send(message).then(function(matches) {
  console.log("Successfully joined match.");

  var connectedOpponents = matches[0].presences.filter(function(presence) {
    // Remove your own user from list.
    return presence.userId != matches[0].self.userId;
  });

  connectedOpponents.forEach(function(opponent) {
    console.log("User id %o handle %o.", opponent.userId, opponent.handle);
  });
}).catch(function(error){
  console.log("An error occured: %o", error);
});
```

The list of match opponents returned in the success callback might not include all users. It contains users who are connected to the match so far.

## List opponents

When a user joins a match they receive an initial list of connected opponents. As other users join or leave the server will push events to clients which can be used to update the list of connected opponents.

```csharp fct_label="Unity"
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

```js fct_label="Javascript"
var connectedOpponents = [];

client.onmatchpresence = function(presences) {
  // Remove all users who left.
  connectedOpponents = connectedOpponents.filter(function(co) {
    var stillConnectedOpponent = true;
    presences.leaves.forEach(function(leftOpponent) {
      if (leftOpponent.userId == co.userId) {
        stillConnectedOpponent = false;
      }
    });
    return stillConnectedOpponent;
  });

  // Add all users who joined.
  connectedOpponents = connectedOpponents.concat(presences.joins);
};
```

No server updates are sent if there are no changes to the presence list.

## Send data messages

A user in a match can send data messages which will be received by all other opponents. These messages are streamed in realtime to other users and can contain any binary content. To identify each message as a specific "command" it contains an Op code as well as the payload.

An Op code is a numeric identifier for the type of message sent. These can be used to define commands within the gameplay which belong to certain user actions.

The binary content in each data message should be as __small as possible__. It is common to use JSON or preferable to use a compact binary format like <a href="https://developers.google.com/protocol-buffers/" target="\_blank">Protocol Buffers</a> or <a href="https://google.github.io/flatbuffers/" target="\_blank">FlatBuffers</a>.

```csharp fct_label="Unity"
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

```js fct_label="Javascript"
var id = match.Id; // an INMatch Id.

var opCode = 1;
var data = {"move": {"dir": "left", "steps": 4}};

var message = new nakamajs.MatchDataSendRequest();
message.matchId = id;
message.opCode = opCode;
message.data = data;

client.send(message).then(function() {
  console.log("Successfully sent data message.");
}).catch(function(error){
  console.log("An error occured: %o", error);
});
```

## Receive data messages

A client can add a callback for incoming match data messages. This should be done before they create (or join) and leave a match.

!!! Note "Message sequences"
    The server delivers data in the order it processes data messages from clients.

```csharp fct_label="Unity"
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

```js fct_label="Javascript"
client.onmatchdata = function(data) {
  var content = data.data;
  switch (data.opCode) {
    case 101:
      console.log("A custom opcode.");
      break;
    default:
      console.log("User handle %o sent %o", data.presence.handle, content);
  }
};
```

## Leave a match

Users can leave a match at any point. A match ends when all users have left.

```csharp fct_label="Unity"
string id = match.Id; // an INMatch Id.

var message = NMatchLeaveMessage.Default(id);
client.Send(message, (bool complete) => {
  Debug.Log("Successfully left match.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```js fct_label="Javascript"
var id = match.Id; // an INMatch Id.

var message = new nakamajs.MatchesLeaveRequest();
message.matchIds.push(id);
client.send(message).then(function() {
  console.log("Successfully left match.");
}).catch(function(error){
  console.log("An error occured: %o", error);
});
```

!!! Note
    When all opponents leave a match it's ID becomes invalid and cannot be re-used to join again.
