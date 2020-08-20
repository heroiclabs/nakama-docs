# Realtime Multiplayer

The realtime multiplayer engine makes it easy for users to set up and join matches where they can rapidly exchange data with opponents.

Any user can participate in matches with other users. Users can create, join, and leave matches with messages sent from clients. A match exists on the server until its last participant has left.

Any data sent through a match is immediately routed to all other match opponents. Matches are kept in-memory but can be persisted as needed.

## Create a match

A match can be created by a user. The server will assign a unique ID which can be shared with other users for them to [join the match](#join-a-match). All users within a match are equal and it is up to the clients to decide on a host.

=== "JavaScript"
	```js
	var response = await socket.send({ match_create: {} });
	console.log("Created match with ID:", response.match.match_id);
	```

=== ".NET"
	```csharp
	var match = await socket.CreateMatchAsync();
	Console.WriteLine("New match with id '{0}'.", match.Id);
	```

=== "Unity"
	```csharp
	var match = await socket.CreateMatchAsync();
	Debug.LogFormat("New match with id '{0}'.", match.Id);
	```

=== "Cocos2d-x C++"
	```cpp
	rtClient->createMatch([](const NMatch& match)
	  {
	    CCLOG("Created Match with ID: %s", match.matchId.c_str());
	  });
	```

=== "Cocos2d-x JS"
	```js
	socket.send({ match_create: {} })
	  .then(function(response) {
	      cc.log("created match with ID:", response.match.match_id);
	    },
	    function(error) {
	      cc.error("create match failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	rtClient->createMatch([](const NMatch& match)
	  {
	    std::cout << "Created Match with ID: " << match.matchId << std::endl;
	  });
	```

=== "Java"
	```java
	Match match = socket.createMatch().get();
	System.out.format("Created match with ID %s.", match.getId());
	```

=== "Godot"
	```gdscript
	var created_match : NakamaRTAPI.Match = yield(socket.create_match_async(), "completed")
	if created_match.is_exception():
		print("An error occured: %s" % created_match)
		return
	print("New match with id %s.", created_match.match_id)
	```

A user can [leave a match](#leave-a-match) at any point which will notify all other users.

## Join a match

A user can join a specific match by ID. Matches can be joined at any point until the last participant leaves.

!!! Hint
    To find a match instead of specify one by ID use the [matchmaker](gameplay-matchmaker.md).

=== "JavaScript"
	```js
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

=== ".NET"
	```csharp
	var matchId = "<matchid>";
	var match = await socket.JoinMatchAsync(matchId);
	foreach (var presence in match.Presences)
	{
	    Console.WriteLine("User id '{0}' name '{1}'.", presence.UserId, presence.Username);
	}
	```

=== "Unity"
	```csharp
	var matchId = "<matchid>";
	var match = await socket.JoinMatchAsync(matchId);
	foreach (var presence in match.Presences)
	{
	    Debug.LogFormat("User id '{0}' name '{1}'.", presence.UserId, presence.Username);
	}
	```

=== "Cocos2d-x C++"
	```cpp
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

=== "Cocos2d-x JS"
	```js
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

=== "C++"
	```cpp
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

=== "Java"
	```java
	String matchId = "<matchid>";
	Match match = socket.joinMatch(matchId).get();
	for (UserPresence presence : match.getPresences()) {
	  System.out.format("User id %s name %s.", presence.getUserId(), presence.getUsername());
	}
	```

=== "Godot"
	```gdscript
	var match_id = "<matchid>"
	var joined_match = yield(socket.join_match_async(match_id), "completed")
	if joined_match.is_exception():
		print("An error occured: %s" % joined_match)
		return
	for presence in joined_match.presences:
		print("User id %s name %s'." % [presence.user_id, presence.username])
	```

The list of match opponents returned in the success callback might not include all users. It contains users who are connected to the match so far.

## List opponents

When a user joins a match they receive an initial list of connected opponents. As other users join or leave the server will push events to clients which can be used to update the list of connected opponents.

=== "JavaScript"
	```js
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

=== ".NET"
	```csharp
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

=== "Unity"
	```csharp
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

=== "Cocos2d-x C++"
	```cpp
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

=== "Cocos2d-x JS"
	```js
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

=== "C++"
	```cpp
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

=== "Java"
	```java
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

=== "Godot"
	```gdscript
	var connected_opponents = {}
	
	func _ready():
		# First, setup the socket as explained in the authentication section.
		socket.connect("received_match_presence", self, "_on_match_presence")
	
	func _on_match_presence(p_presence : NakamaRTAPI.MatchPresenceEvent):
		for p in p_presence.joins:
			connected_opponents[p.user_id] = p
		for p in p_presence.leaves:
			connected_opponents.erase(p.user_id)
		print("Connected opponents: %s" % [connected_opponents])
	```

No server updates are sent if there are no changes to the presence list.

## Send data messages

A user in a match can send data messages which will be received by all other opponents. These messages are streamed in realtime to other users and can contain any binary content. To identify each message as a specific "command" it contains an Op code as well as the payload.

An Op code is a numeric identifier for the type of message sent. These can be used to define commands within the gameplay which belong to certain user actions.

The binary content in each data message should be as __small as possible__. It is common to use JSON or preferable to use a compact binary format like <a href="https://developers.google.com/protocol-buffers/" target="\_blank">Protocol Buffers</a> or <a href="https://google.github.io/flatbuffers/" target="\_blank">FlatBuffers</a>.

=== "JavaScript"
	```js
	var id = "<matchid>";
	var opCode = 1;
	var data = { "move": {"dir": "left", "steps": 4} };
	socket.send({ match_data_send: { match_id: id, op_code: opCode, data: data } });
	```

=== ".NET"
	```csharp
	// using Nakama.TinyJson;
	var matchId = "<matchid>";
	var opCode = 1;
	var newState = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
	socket.SendMatchStateAsync(matchId, opCode, newState);
	```

=== "Unity"
	```csharp
	// using Nakama.TinyJson;
	var id = "<matchid>";
	var opCode = 1;
	var newState = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
	socket.SendMatchStateAsync(matchId, opCode, newState);
	```

=== "Cocos2d-x C++"
	```cpp
	string id = "<matchid>";
	int64_t opCode = 1;
	NBytes data = "{ \"move\": {\"dir\": \"left\", \"steps\" : 4} }";
	rtClient->sendMatchData(id, opCode, data);
	```

=== "Cocos2d-x JS"
	```js
	var id = "<matchid>";
	var opCode = 1;
	var data = { "move": {"dir": "left", "steps": 4} };
	socket.send({ match_data_send: { match_id: id, op_code: opCode, data: data } });
	```

=== "C++"
	```cpp
	string id = "<matchid>";
	int64_t opCode = 1;
	NBytes data = "{ \"move\": {\"dir\": \"left\", \"steps\" : 4} }";
	rtClient->sendMatchData(id, opCode, data);
	```

=== "Java"
	```java
	String id = "<matchid>";
	int opCode = 1;
	String data = "{\"message\":\"Hello world\"}";
	socket.sendMatchData(id, opCode, data);
	```

=== "Godot"
	```gdscript
	var match_id = "<matchid>"
	var op_code = 1
	var new_state = {"hello": "world"}
	socket.send_match_state_async(match_id, op_code, JSON.print(new_state))
	```

## Receive data messages

A client can add a callback for incoming match data messages. This should be done before they create (or join) and leave a match.

!!! Note "Message sequences"
    The server delivers data in the order it processes data messages from clients.

=== "JavaScript"
	```js
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

=== ".NET"
	```csharp
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

=== "Unity"
	```csharp
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

=== "Cocos2d-x C++"
	```cpp
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

=== "Cocos2d-x JS"
	```js
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

=== "C++"
	```cpp
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

=== "Java"
	```java
	SocketListener listener = new AbstractSocketListener() {
	  @Override
	  public void onMatchData(final MatchData matchData) {
	      System.out.format("Received match data %s with opcode %d", matchData.getData(), matchData.getOpCode());
	  }
	};
	```

=== "Godot"
	```gdscript
	func _ready():
		# First, setup the socket as explained in the authentication section.
		socket.connect("received_match_state", self, "_on_match_state")
	
	func _on_match_state(p_state : NakamaRTAPI.MatchData):
		print("Received match state with opcode %s, data %s" % [p_state.op_code, parse_json(p_state.data)])
	```

## Leave a match

Users can leave a match at any point. A match ends when all users have left.

=== "JavaScript"
	```js
	var id = "<matchid>";
	socket.send({ match_leave: {match_id: id}});
	```

=== ".NET"
	```csharp
	var matchId = "<matchid>";
	await socket.LeaveMatchAsync(matchId);
	```

=== "Unity"
	```csharp
	var matchId = "<matchid>";
	await socket.LeaveMatchAsync(matchId);
	```

=== "Cocos2d-x C++"
	```cpp
	string matchId = "<matchid>";
	rtClient->leaveMatch(matchId);
	```

=== "Cocos2d-x JS"
	```js
	var id = "<matchid>";
	socket.send({ match_leave: {match_id: id}});
	```

=== "C++"
	```cpp
	string matchId = "<matchid>";
	rtClient->leaveMatch(matchId);
	```

=== "Java"
	```java
	String matchId = "<matchid>";
	socket.leaveMatch(matchId).get();
	```

=== "Godot"
	```gdscript
	var match_id = "<matchid>"
	var leave : NakamaAsyncResult = yield(socket.leave_match_async(match_id), "completed")
	if leave.is_exception():
		print("An error occured: %s" % leave)
		return
	print("Match left")
	```

!!! Note
    When all opponents have left a match it's ID becomes invalid and cannot be re-used to join again.
