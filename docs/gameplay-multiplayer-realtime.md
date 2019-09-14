# Realtime Multiplayer

The realtime multiplayer engine makes it easy for users to set up and join matches where they can rapidly exchange data with opponents.

Any user can participate in matches with other users. Users can create, join, and leave matches with messages sent from clients. A match exists on the server until its last participant has left.

Any data sent through a match is immediately routed to all other match opponents. Matches are kept in-memory but can be persisted as needed.

## Create a match

A match can be created by a user. The server will assign a unique ID which can be shared with other users for them to [join the match](#join-a-match). All users within a match are equal and it is up to the clients to decide on a host.

```js tab="JavaScript"
var response = await socket.send({ match_create: {} });
console.log("Created match with ID:", response.match.match_id);
```

```csharp tab=".NET"
var match = await socket.CreateMatchAsync();
Console.WriteLine("New match with id '{0}'.", match.Id);
```

```csharp tab="Unity"
var match = await socket.CreateMatchAsync();
Debug.LogFormat("New match with id '{0}'.", match.Id);
```

```cpp tab="Cocos2d-x C++"
rtClient->createMatch([](const NMatch& match)
  {
    CCLOG("Created Match with ID: %s", match.matchId.c_str());
  });
```

```js tab="Cocos2d-x JS"
socket.send({ match_create: {} })
  .then(function(response) {
      cc.log("created match with ID:", response.match.match_id);
    },
    function(error) {
      cc.error("create match failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
rtClient->createMatch([](const NMatch& match)
  {
    std::cout << "Created Match with ID: " << match.matchId << std::endl;
  });
```

```java tab="Java"
Match match = socket.createMatch().get();
System.out.format("Created match with ID %s.", match.getId());
```

A user can [leave a match](#leave-a-match) at any point which will notify all other users.

## Join a match

A user can join a specific match by ID. Matches can be joined at any point until the last participant leaves.

!!! Hint
    To find a match instead of specify one by ID use the [matchmaker](gameplay-matchmaker.md).

```js tab="JavaScript"
var id = "<matchid>";
var match = await socket.send({ match_join: { match_id: id } });
var connectedOpponents = match.presences.filter((presence) => {
  // Remove your own user from list.
  return presence.user_id != match.self.user_id;
});
connectedOpponents.forEach((opponent) => {
  console.log("User id %o, username %o.", opponent.user_id, opponent.username);
});
```

```csharp tab=".NET"
var matchId = "<matchid>";
var match = await socket.JoinMatchAsync(matchId);
foreach (var presence in match.Presences)
{
    Console.WriteLine("User id '{0}' name '{1}'.", presence.UserId, presence.Username);
}
```

```csharp tab="Unity"
var matchId = "<matchid>";
var match = await socket.JoinMatchAsync(matchId);
foreach (var presence in match.Presences)
{
    Debug.LogFormat("User id '{0}' name '{1}'.", presence.UserId, presence.Username);
}
```

```cpp tab="Cocos2d-x C++"
string matchId = "<matchid>";
rtClient->joinMatch(matchId, {}, [](const NMatch& match)
  {
    CCLOG("Joined Match!");

    for (auto& presence : match.presences)
    {
      if (presence.userId != match.self.userId)
      {
        CCLOG("User id %s username %s", presence.userId.c_str(), presence.username.c_str());
      }
    }
  });
```

```js tab="Cocos2d-x JS"
var id = "<matchid>";
socket.send({ match_join: { match_id: id } })
  .then(
    function (response) {
      cc.error("joined match:", JSON.stringify(response));
      var match = response.match;
      var connectedOpponents = match.presences.filter((presence) => {
          // Remove your own user from list.
          return presence.user_id != match.self.user_id;
      });
      connectedOpponents.forEach((opponent) => {
          cc.log("User id", opponent.user_id, "username", opponent.username);
      });
    },
    function (error) {
      cc.error("join match failed:", JSON.stringify(error));
    }
  );
```

```cpp tab="C++"
string matchId = "<matchid>";
rtClient->joinMatch(matchId, {}, [](const NMatch& match)
  {
    std::cout << "Joined Match!" << std::endl;

    for (auto& presence : match.presences)
    {
      if (presence.userId != match.self.userId)
      {
        std::cout << "User id " << presence.userId << " username " << presence.username << std::endl;
      }
    }
  });
```

```java tab="Java"
String matchId = "<matchid>";
Match match = socket.joinMatch(matchId).get();
for (UserPresence presence : match.getPresences()) {
  System.out.format("User id %s name %s.", presence.getUserId(), presence.getUsername());
}
```

The list of match opponents returned in the success callback might not include all users. It contains users who are connected to the match so far.

## List opponents

When a user joins a match they receive an initial list of connected opponents. As other users join or leave the server will push events to clients which can be used to update the list of connected opponents.

```js tab="JavaScript"
var connectedOpponents = [];
socket.onmatchpresence = (presences) => {
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

```csharp tab=".NET"
var connectedOpponents = new List<IUserPresence>(2);
socket.ReceivedMatchPresence += presenceEvent =>
{
    foreach (var presence in presenceEvent.Leaves)
    {
        connectedOpponents.Remove(presence);
    }
    connectedOpponents.AddRange(presenceEvent.Joins);
    // Remove yourself from connected opponents.
    connectedOpponents.Remove(self);
    Console.WriteLine("Connected opponents: [{0}]", string.Join(",\n  ", connectedOpponents));
};
```

```csharp tab="Unity"
var connectedOpponents = new List<IUserPresence>(2);
socket.ReceivedMatchPresence += presenceEvent =>
{
    foreach (var presence in presenceEvent.Leaves)
    {
        connectedOpponents.Remove(presence);
    }
    connectedOpponents.AddRange(presenceEvent.Joins);
    // Remove yourself from connected opponents.
    connectedOpponents.Remove(self);
    Debug.LogFormat("Connected opponents: [{0}]", string.Join(",\n  ", connectedOpponents));
};
```

```cpp tab="Cocos2d-x C++"
rtListener->setMatchPresenceCallback([](const NMatchPresenceEvent& event)
  {
    for (auto& presence : event.joins)
    {
      CCLOG("Joined user: %s", presence.username.c_str());
    }

    for (auto& presence : event.leaves)
    {
      CCLOG("Left user: %s", presence.username.c_str());
    }
  });
```

```js tab="Cocos2d-x JS"
var connectedOpponents = [];
socket.onmatchpresence = (presences) => {
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

```cpp tab="C++"
rtListener->setMatchPresenceCallback([](const NMatchPresenceEvent& event)
  {
    for (auto& presence : event.joins)
    {
      std::cout << "Joined user: " << presence.username << std::endl;
    }

    for (auto& presence : event.leaves)
    {
      std::cout << "Left user: " << presence.username << std::endl;
    }
  });
```

```java tab="Java"
List<UserPresence> connectedOpponents = new ArrayList<UserPresence>();
public void onMatchPresence(final MatchPresenceEvent matchPresence) {
  connectedOpponents.addAll(matchPresence.getJoins());
  for (UserPresence leave : matchPresence.getLeaves()) {
    for (int i = 0; i < connectedOpponents.size(); i++) {
      if (connectedOpponents.get(i).getUserId().equals(leave.getUserId())) {
        connectedOpponents.remove(i);
      }
    }
  };
});
```

No server updates are sent if there are no changes to the presence list.

## Send data messages

A user in a match can send data messages which will be received by all other opponents. These messages are streamed in realtime to other users and can contain any binary content. To identify each message as a specific "command" it contains an Op code as well as the payload.

An Op code is a numeric identifier for the type of message sent. These can be used to define commands within the gameplay which belong to certain user actions.

The binary content in each data message should be as __small as possible__. It is common to use JSON or preferable to use a compact binary format like <a href="https://developers.google.com/protocol-buffers/" target="\_blank">Protocol Buffers</a> or <a href="https://google.github.io/flatbuffers/" target="\_blank">FlatBuffers</a>.

```js tab="JavaScript"
var id = "<matchid>";
var opCode = 1;
var data = { "move": {"dir": "left", "steps": 4} };
socket.send({ match_data_send: { match_id: id, op_code: opCode, data: data } });
```

```csharp tab=".NET"
// using Nakama.TinyJson;
var matchId = "<matchid>";
var opCode = 1;
var newState = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
socket.SendMatchStateAsync(matchId, opCode, newState);
```

```csharp tab="Unity"
// using Nakama.TinyJson;
var id = "<matchid>";
var opCode = 1;
var newState = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
socket.SendMatchStateAsync(matchId, opCode, newState);
```

```cpp tab="Cocos2d-x C++"
string id = "<matchid>";
int64_t opCode = 1;
NBytes data = "{ \"move\": {\"dir\": \"left\", \"steps\" : 4} }";
rtClient->sendMatchData(id, opCode, data);
```

```js tab="Cocos2d-x JS"
var id = "<matchid>";
var opCode = 1;
var data = { "move": {"dir": "left", "steps": 4} };
socket.send({ match_data_send: { match_id: id, op_code: opCode, data: data } });
```

```cpp tab="C++"
string id = "<matchid>";
int64_t opCode = 1;
NBytes data = "{ \"move\": {\"dir\": \"left\", \"steps\" : 4} }";
rtClient->sendMatchData(id, opCode, data);
```

```java tab="Java"
String id = "<matchid>";
int opCode = 1;
String data = "{\"message\":\"Hello world\"}";
socket.sendMatchData(id, opCode, data);
```

## Receive data messages

A client can add a callback for incoming match data messages. This should be done before they create (or join) and leave a match.

!!! Note "Message sequences"
    The server delivers data in the order it processes data messages from clients.

```js tab="JavaScript"
socket.onmatchdata = (result) => {
  var content = result.data;
  switch (result.op_code) {
    case 101:
      console.log("A custom opcode.");
      break;
    default:
      console.log("User %o sent %o", result.presence.user_id, content);
  }
};
```

```csharp tab=".NET"
// Use whatever decoder for your message contents.
var enc = System.Text.Encoding.UTF8;
socket.ReceivedMatchState += newState =>
{
    var content = enc.GetString(newState.State);
    switch (newState.OpCode)
    {
        case 101:
            Console.WriteLine("A custom opcode.");
            break;
        default:
            Console.WriteLine("User '{0}'' sent '{1}'", newState.UserPresence.Username, content);
    }
};
```

```csharp tab="Unity"
// Use whatever decoder for your message contents.
var enc = System.Text.Encoding.UTF8;
socket.ReceivedMatchState += newState =>
{
    var content = enc.GetString(newState.State);
    switch (newState.OpCode)
    {
        case 101:
            Debug.Log("A custom opcode.");
            break;
        default:
            Debug.LogFormat("User '{0}'' sent '{1}'", newState.UserPresence.Username, content);
    }
};
```

```cpp tab="Cocos2d-x C++"
rtListener->setMatchDataCallback([](const NMatchData& data)
  {
    switch (data.opCode)
    {
    case 101:
      CCLOG("A custom opcode.");
      break;

    default:
      CCLOG("User %s sent %s", data.presence.userId.c_str(), data.data.c_str());
      break;
    }
  });
```

```js tab="Cocos2d-x JS"
socket.onmatchdata = (result) => {
  var content = result.data;
  switch (result.op_code) {
    case 101:
      cc.log("A custom opcode.");
      break;
    default:
      cc.log("User", result.presence.user_id, "sent", content);
  }
};
```

```cpp tab="C++"
rtListener->setMatchDataCallback([](const NMatchData& data)
  {
    switch (data.opCode)
    {
    case 101:
      std::cout << "A custom opcode." << std::endl;
      break;

    default:
      std::cout << "User " << data.presence.userId << " sent " << data.data << std::endl;
      break;
    }
  });
```

```java tab="Java"
SocketListener listener = new AbstractSocketListener() {
  @Override
  public void onMatchData(final MatchData matchData) {
      System.out.format("Received match data %s with opcode %d", matchData.getData(), matchData.getOpCode());
  }
};
```

## Leave a match

Users can leave a match at any point. A match ends when all users have left.

```js tab="JavaScript"
var id = "<matchid>";
socket.send({ match_leave: {match_id: id}});
```

```csharp tab=".NET"
var matchId = "<matchid>";
await socket.LeaveMatchAsync(matchId);
```

```csharp tab="Unity"
var matchId = "<matchid>";
await socket.LeaveMatchAsync(matchId);
```

```cpp tab="Cocos2d-x C++"
string matchId = "<matchid>";
rtClient->leaveMatch(matchId);
```

```js tab="Cocos2d-x JS"
var id = "<matchid>";
socket.send({ match_leave: {match_id: id}});
```

```cpp tab="C++"
string matchId = "<matchid>";
rtClient->leaveMatch(matchId);
```

```java tab="Java"
String matchId = "<matchid>";
socket.leaveMatch(matchId).get();
```

!!! Note
    When all opponents have left a match it's ID becomes invalid and cannot be re-used to join again.
