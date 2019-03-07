# Realtime Multiplayer

The realtime multiplayer engine makes it easy for users to set up and join matches where they can rapidly exchange data with opponents.

Any user can participate in matches with other users. Users can create, join, and leave matches with messages sent from clients. A match exists on the server until its last participant has left.

Any data sent through a match is immediately routed to all other match opponents. Matches are kept in-memory but can be persisted as needed.

## Create a match

A match can be created by a user. The server will assign a unique ID which can be shared with other users for them to [join the match](#join-a-match). All users within a match are equal and it is up to the clients to decide on a host.

```js fct_label="JavaScript"
var match = await socket.send({ match_create: {} });
console.log("Created match with ID:", match.id);
```

```csharp fct_label=".NET"
var match = await socket.CreateMatchAsync();
Console.WriteLine("Created match with ID '{0}'.", match.Id);
```

```csharp fct_label="Unity"
var match = await socket.CreateMatchAsync();
Debug.LogFormat("Created match with ID '{0}'.", match.Id);
```

```cpp fct_label="Cocos2d-x C++"
rtClient->createMatch([](const NMatch& match)
  {
    CCLOG("Created Match with ID: %s", match.matchId.c_str());
  });
```

```js fct_label="Cocos2d-x JS"
socket.send({ match_create: {} })
  .then(function(response) {
      cc.log("created match with ID:", response.match.match_id);
    },
    function(error) {
      cc.error("create match failed:", JSON.stringify(error));
    });
```

```cpp fct_label="C++"
rtClient->createMatch([](const NMatch& match)
  {
    std::cout << "Created Match with ID: " << match.matchId << std::endl;
  });
```

```java fct_label="Java"
Match match = socket.createMatch().get();
System.out.format("Created match with ID %s.", match.getId());
```

A user can [leave a match](#leave-a-match) at any point which will notify all other users.

## Join a match

A user can join a specific match by ID. Matches can be joined at any point until the last participant leaves.

!!! Hint
    To find a match instead of specify one by ID use the [matchmaker](gameplay-matchmaker.md).

```js fct_label="JavaScript"
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

```csharp fct_label=".NET"
var matchId = "<matchid>";
var match = await socket.JoinMatchAsync(matchId);
foreach (var presence in match.presences)
{
  Console.WriteLine("User id '{0}' name '{1}'.", presence.UserId, presence.Username);
}
```

```csharp fct_label="Unity"
var matchId = "<matchid>";
var match = await socket.JoinMatchAsync(matchId);
foreach (var presence in match.presences)
{
  Debug.LogFormat("User id '{0}' name '{1}'.", presence.UserId, presence.Username);
}
```

```cpp fct_label="Cocos2d-x C++"
string matchId = "<matchid>";
rtClient->joinMatch(matchId, [](const NMatch& match)
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

```js fct_label="Cocos2d-x JS"
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

```cpp fct_label="C++"
string matchId = "<matchid>";
rtClient->joinMatch(matchId, [](const NMatch& match)
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

```java fct_label="Java"
String matchId = "<matchid>";
Match match = socket.joinMatch(matchId).get();
for (UserPresence presence : match.getPresences()) {
  System.out.format("User id %s name %s.", presence.getUserId(), presence.getUsername());
}
```

The list of match opponents returned in the success callback might not include all users. It contains users who are connected to the match so far.

## List opponents

When a user joins a match they receive an initial list of connected opponents. As other users join or leave the server will push events to clients which can be used to update the list of connected opponents.

```js fct_label="JavaScript"
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

```csharp fct_label=".NET"
var connectedOpponents = new List<IUserPresence>(0);
socket.OnMatchPresence += (_, presence) =>
{
  connectedOpponents.AddRange(presence.Joins);
  foreach (var leave in presence.Leaves)
  {
    connectedOpponents.RemoveAll(item => item.SessionId.Equals(leave.SessionId));
  };
};
```

```csharp fct_label="Unity"
var connectedOpponents = new List<IUserPresence>(0);
socket.OnMatchPresence += (_, presence) =>
{
  connectedOpponents.AddRange(presence.Joins);
  foreach (var leave in presence.Leaves)
  {
    connectedOpponents.RemoveAll(item => item.SessionId.Equals(leave.SessionId));
  };
};
```

```cpp fct_label="Cocos2d-x C++"
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

```js fct_label="Cocos2d-x JS"
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

```cpp fct_label="C++"
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

```java fct_label="Java"
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

```js fct_label="JavaScript"
var id = "<matchid>";
var opCode = 1;
var data = { "move": {"dir": "left", "steps": 4} };
socket.send({ match_data_send: { match_id: id, op_code: opCode, data: data } });
```

```csharp fct_label=".NET"
// using Nakama.TinyJson;
var id = "<matchid>";
var opCode = 1;
var newState = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
socket.SendMatchState(id, opCode, newState);
```

```csharp fct_label="Unity"
// using Nakama.TinyJson;
var id = "<matchid>";
var opCode = 1;
var newState = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
socket.SendMatchState(id, opCode, newState);
```

```cpp fct_label="Cocos2d-x C++"
string id = "<matchid>";
int64_t opCode = 1;
NBytes data = "{ \"move\": {\"dir\": \"left\", \"steps\" : 4} }";
rtClient->sendMatchData(id, opCode, data);
```

```js fct_label="Cocos2d-x JS"
var id = "<matchid>";
var opCode = 1;
var data = { "move": {"dir": "left", "steps": 4} };
socket.send({ match_data_send: { match_id: id, op_code: opCode, data: data } });
```

```cpp fct_label="C++"
string id = "<matchid>";
int64_t opCode = 1;
NBytes data = "{ \"move\": {\"dir\": \"left\", \"steps\" : 4} }";
rtClient->sendMatchData(id, opCode, data);
```

```java fct_label="Java"
String id = "<matchid>";
int opCode = 1;
String data = "{\"message\":\"Hello world\"}";
socket.sendMatchData(id, opCode, data);
```

## Receive data messages

A client can add a callback for incoming match data messages. This should be done before they create (or join) and leave a match.

!!! Note "Message sequences"
    The server delivers data in the order it processes data messages from clients.

```js fct_label="JavaScript"
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

```csharp fct_label=".NET"
socket.OnMatchState = (_, state) => {
  var content = System.Text.Encoding.UTF8.GetString(state.State);
  switch (state.OpCode) {
    case 101:
      Console.WriteLine("A custom opcode.");
      break;
    default:
      Console.WriteLine("User {0} sent {1}", state.UserPresence.Username, content);
  }
};
```

```csharp fct_label="Unity"
socket.OnMatchState = (_, state) => {
  var content = System.Text.Encoding.UTF8.GetString(state.State);
  switch (state.OpCode) {
    case 101:
      Debug.Log("A custom opcode.");
      break;
    default:
      Debug.LogFormat("User {0} sent {1}", state.UserPresence.Username, content);
  }
};
```

```cpp fct_label="Cocos2d-x C++"
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

```js fct_label="Cocos2d-x JS"
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

```cpp fct_label="C++"
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

```java fct_label="Java"
SocketListener listener = new AbstractSocketListener() {
  @Override
  public void onMatchData(final MatchData matchData) {
      System.out.format("Received match data %s with opcode %d", matchData.getData(), matchData.getOpCode());
  }
};
```

## Leave a match

Users can leave a match at any point. A match ends when all users have left.

```js fct_label="JavaScript"
var id = "<matchid>";
socket.send({ match_leave: {match_id: id}});
```

```csharp fct_label=".NET"
var matchId = "<matchid>";
await socket.LeaveMatchAsync(matchId);
```

```csharp fct_label="Unity"
var matchId = "<matchid>";
await socket.LeaveMatchAsync(matchId);
```

```cpp fct_label="Cocos2d-x C++"
string matchId = "<matchid>";
rtClient->leaveMatch(matchId);
```

```js fct_label="Cocos2d-x JS"
var id = "<matchid>";
socket.send({ match_leave: {match_id: id}});
```

```cpp fct_label="C++"
string matchId = "<matchid>";
rtClient->leaveMatch(matchId);
```

```java fct_label="Java"
String matchId = "<matchid>";
socket.leaveMatch(matchId).get();
```

!!! Note
    When all opponents have left a match it's ID becomes invalid and cannot be re-used to join again.
