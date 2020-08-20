# Streams

!!! Warning "Here be dragons"
    Streams are a powerful feature that gives you full control over Nakama's internal realtime routing and delivery. Use with caution!

Nakama's realtime message routing and delivery subsystem is organised into streams. Streams tie together clients interested in certain message types and allow Nakama's internal components to deliver messages to relevant users.

Clients may receive messages and data from streams, but are not allowed to directly join, leave, or send data themselves. These functions are only available in the server [code runtime](runtime-code-basics.md).

All of the higher-level realtime features in the server ([chat channels](social-realtime-chat.md), [multiplayer](gameplay-multiplayer-realtime.md), [notifications](social-in-app-notifications.md), etc.) are built as features on top of streams. Understanding and using streams are not necessary to use these features.

## Structure of a stream

Streams are defined by two components: a **stream identifier** and **presence list**.

### Stream identifier

All streams have their own unique id. This is used to place users onto streams and locate streams for message delivery. A stream id has 4 fields:

* __Mode__ marks the type of stream. For example chat channels have different names but the same mode.
* __Subject__ contains a primary stream subject usually a user id.
* __Descriptor__ is a secondary id. Used when a stream is scoped to a pair of users or groups like with [direct chat](social-realtime-chat.md) between two users.
* __Label__ stores a string which could be meta-information. A chat room created by name uses the label field.

!!! Tip
    Mode is the only required field.

### Presence list

Streams are a way to address a set of online users and deliver messages to them. Each stream maintains a list of presences that uniquely identify a user with the socket they're connected on. When the server sends a message to a stream it will be delivered to all clients identified by the presences.

### Persistence and message history

Presences may be marked with an optional persistence flag. The server can observe this flag when handling messages delivered to a stream to decide wether the message data should be stored in the database. The [realtime chat](social-realtime-chat.md) feature uses this flag to decide if messages should be stored so clients can request message history.

### Hidden stream members

Streams generate presence events that notify all users currently in the stream when a new user joins or an existing user leaves the stream. Presences may be marked with an optional hidden flag. When this is set to true the server will not generate presence events when this user joins or leaves.

!!! Tip
    Hidden presences are full stream members so they do receive data and presence events as normal.

## Receiving stream data

Clients can register an event handler to consume stream data objects when received over the socket. The handler function will be called once for each stream data message.

=== "JavaScript"
	```js
	socket.onstreamdata = (streamdata) => {
	  console.log("Received data from stream: %o", streamdata.stream.subject);
	  console.log("Data content: %@.", streamdata.data);
	};
	```

=== ".NET"
	```csharp
	socket.ReceivedStreamState += stream =>
	{
	  Console.WriteLine("Received data from stream: '{0}'", stream.Stream.Subject);
	  Console.WriteLine("Data content: {0}", stream.State);
	};
	```

=== "Unity"
	```csharp
	socket.ReceivedStreamState += stream =>
	{
	    Debug.LogFormat("Received data from stream: '{0}'", stream.Stream.Subject);
	    Debug.LogFormat("Data content: {0}", stream.State);
	};
	```

=== "Cocos2d-x C++"
	```cpp
	// add listener to header of your class: NRtDefaultClientListener listener;
	rtClient->setListener(&listener);
	listener.onStreamData([](const NStreamData& data)
	{
	    CCLOG("Received data from stream: %s", data.stream.subject.c_str());
	    CCLOG("Data content: %s", data.data.c_str());
	});
	```

=== "Cocos2d-x JS"
	```js
	socket.onstreamdata = function (streamdata) {
	  cc.log("Received data from stream:", streamdata.stream.subject);
	  cc.log("Data content:", streamdata.data);
	};
	```

=== "C++"
	```cpp
	// add listener to header of your class: NRtDefaultClientListener listener;
	rtClient->setListener(&listener);
	listener.onStreamData([](const NStreamData& data)
	{
	    cout << "Received data from stream: " << data.stream.subject << endl;
	    cout << "Data content: " << data.data << endl;
	});
	```

=== "Java"
	```java
	SocketListener listener = new AbstractSocketListener() {
	  @Override
	  public void onStreamData(final StreamData data) {
	    System.out.println("Received data from stream: " + data.getStream().getSubject());
	    System.out.println("Data content: " + data.getData());
	  }
	};
	```

=== "Godot"
	```gdscript
	func _ready():
		# First, setup the socket as explained in the authentication section.
		socket.connect("received_stream_state", self, "_on_stream_state")
	
	func _on_stream_state(p_state : NakamaRTAPI.StreamData):
		print("Received data from stream: %s" % [p_state.stream])
		print("Data: %s" % [parse_json(p_state.state)])
	```

## Receiving stream presence events

When a new presence joins a stream or an existing presence leaves the server will broadcast presence events to all users currently on the stream.

=== "JavaScript"
	```js
	socket.onstreampresence = (streampresence) => {
	  console.log("Received presence event for stream: %o", streampresence.id);
	  streampresence.joins.forEach((join) => {
	    console.log("New user joined: %o", join.user_id);
	  });
	  streampresence.leaves.forEach((leave) => {
	    console.log("User left: %o", leave.user_id);
	  });
	};
	```

=== ".NET"
	```csharp
	socket.ReceivedStreamPresence += presenceEvent =>
	{
	    Console.WriteLine("Received data from stream: '{0}'", presenceEvent.Stream.Subject);
	    foreach (var joined in presenceEvent.Joins)
	    {
	        Console.WriteLine("Joined: {0}", joined);
	    }
	    foreach (var left in presenceEvent.Leaves)
	    {
	        Console.WriteLine("Left: {0}", left);
	    }
	};
	```

=== "Unity"
	```csharp
	socket.ReceivedStreamPresence += presenceEvent =>
	{
	    Debug.LogFormat("Received data from stream: '{0}'", presenceEvent.Stream.Subject);
	    foreach (var joined in presenceEvent.Joins)
	    {
	        Debug.LogFormat("Joined: {0}", joined);
	    }
	    foreach (var left in presenceEvent.Leaves)
	    {
	        Debug.LogFormat("Left: {0}", left);
	    }
	};
	```

=== "Cocos2d-x C++"
	```cpp
	// add listener to header of your class: NRtDefaultClientListener listener;
	rtClient->setListener(&listener);
	listener.onStreamPresence([](const NStreamPresenceEvent& presence)
	{
	    CCLOG("Received presence event for stream: %s", presence.stream.subject.c_str());
	    for (const NUserPresence& userPresence : presence.joins)
	    {
	      CCLOG("New user joined: %s", userPresence.user_id.c_str());
	    }
	    for (const NUserPresence& userPresence : presence.leaves)
	    {
	      CCLOG("User left: %s", userPresence.user_id.c_str());
	    }
	});
	```

=== "Cocos2d-x JS"
	```js
	socket.onstreampresence = function(streampresence) {
	  cc.log("Received presence event for stream:", streampresence.id);
	  streampresence.joins.forEach(function(join) {
	    cc.log("New user joined:", join.user_id);
	  });
	  streampresence.leaves.forEach(function(leave) {
	    cc.log("User left:", leave.user_id);
	  });
	};
	```

=== "C++"
	```cpp
	// add listener to header of your class: NRtDefaultClientListener listener;
	rtClient->setListener(&listener);
	listener.onStreamPresence([](const NStreamPresenceEvent& presence)
	{
	    cout << "Received presence event for stream: " << presence.stream.subject << endl;
	    for (const NUserPresence& userPresence : presence.joins)
	    {
	      cout << "New user joined: " << userPresence.user_id << endl;
	    }
	    for (const NUserPresence& userPresence : presence.leaves)
	    {
	      cout << "User left: " << userPresence.user_id << endl;
	    }
	});
	```

=== "Java"
	```java
	SocketListener listener = new AbstractSocketListener() {
	  @Override
	  public void onStreamPresence(final StreamPresenceEvent presence) {
	    System.out.println("Received presence event for stream: " + presence.getStream().getSubject());
	
	    for (UserPresence userPresence : presence.getJoins()) {
	      System.out.println("User ID: " + userPresence.getUserId() + " Username: " + userPresence.getUsername() + " Status: " + userPresence.getStatus());
	    }
	
	    for (UserPresence userPresence : presence.getLeaves()) {
	      System.out.println("User ID: " + userPresence.getUserId() + " Username: " + userPresence.getUsername() + " Status: " + userPresence.getStatus());
	    }
	  }
	};
	```

=== "Godot"
	```gdscript
	func _ready():
		# First, setup the socket as explained in the authentication section.
		socket.connect("received_stream_presence", self, "_on_stream_presence")
	
	func _on_stream_presence(p_presence : NakamaRTAPI.StreamPresenceEvent):
		print("Received presences on stream: %s" % [p_presence.stream])
		for p in p_presence.joins:
			print("User ID: %s, Username: %s, Status: %s" % [p.user_id, p.username, p.status])
		for p in p_presence.leaves:
			print("User ID: %s, Username: %s, Status: %s" % [p.user_id, p.username, p.status])
	```

!!! Tip
    Hidden presences do not generate presence events and won't appear in results received by this event handler.

## Join a stream

The server can place users on any number of streams. To add a user to a stream the server needs the user's ID, the unique session ID of the user's current session, and information about the stream they should be placed on.

As an example we can register an RPC function that will place the user that calls it on a custom stream.

=== "Lua"
	```lua
	local function join(context, _)
	  local stream_id = { mode = 123, label = "my custom stream" }
	  local hidden = false
	  local persistence = false
	  nk.stream_user_join(context.user_id, context.session_id, stream_id, hidden, persistence)
	end
	nk.register_rpc(join, "join")
	```

=== "Go"
	```go
	func JoinStream(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
		userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
		if !ok {
			// If user ID is not found, RPC was called without a session token.
			return "", errors.New("Invalid context")
		}
		sessionID, ok := ctx.Value(runtime.RUNTIME_CTX_SESSION_ID).(string)
		if !ok {
			// If session ID is not found, RPC was not called over a connected socket.
			return "", errors.New("Invalid context")
		}
	
	  mode := 123
		hidden := false
		persistence := false
		if _, err := nk.StreamUserJoin(mode, "", "", "label", userID, sessionID, hidden, persistence, ""); err != nil {
			return "", err
		}
	
		return "Success", nil
	}
	
	// Register as RPC function, this call should be in InitModule.
	if err := initializer.RegisterRpc("join", JoinStream); err != nil {
	  logger.Error("Unable to register: %v", err)
	  return err
	}
	```

If this user+session is already a member of the stream the operation will be a no-op.

## Leave a stream

Leaving streams is also controlled by the server. To remove a user from a stream the server needs the user's ID, the unique session ID of the user's current session, and information about the stream they should be removed from.

As an example we can register an RPC function that will remove the user that calls it from the custom stream.

=== "Lua"
	```lua
	local function leave(context, _)
	  local stream_id = { mode = 123, label = "my custom stream" }
	  nk.stream_user_leave(context.user_id, context.session_id, stream_id)
	end
	nk.register_rpc(leave, "leave")
	```

=== "Go"
	```go
	// RPC Code
	func LeaveStream(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
		userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	  if !ok {
	    // If user ID is not found, RPC was called without a session token.
	    return "", errors.New("Invalid context")
	  }
	  sessionID, ok := ctx.Value(runtime.RUNTIME_CTX_SESSION_ID).(string)
	  if !ok {
	    // If session ID is not found, RPC was not called over a connected socket.
	    return "", errors.New("Invalid context")
	  }
	
		if err := nk.StreamUserLeave(123, "", "", "label", userID, sessionID); err != nil {
			return "", err
		}
	
		return "Success", nil
	}
	
	// Register as RPC function, this call should be in InitModule.
	if err := initializer.RegisterRpc("leave", LeaveStream); err != nil {
	  logger.Error("Unable to register: %v", err)
	  return err
	}
	```

If this user+session is not a member of the stream the operation will be a no-op.

!!! Note
    Just like [chat channels](social-realtime-chat.md) and [realtime multiplayer matches](gameplay-multiplayer-realtime.md) when a client disconnects it is automatically removed from any streams it was part of.

## Send data to a stream

The server can send data to a stream through a function call. The message will be delivered to all users present on the stream.

=== "Lua"
	```lua
	local stream_id = { mode = 123, label = "my custom stream" }
	local payload = nk.json_encode({ some = "data" })
	nk.stream_send(stream_id, payload)
	```

=== "Go"
	```go
	mode := uint8(123)
	label := "label"
	// Data does not have to be JSON, but it's a convenient format.
	data := "{\"some\":\"data\"}"
	nk.StreamSend(mode, "", "", label, data, nil)
	```

If the stream is empty the operation will be a no-op.

!!! Tip
    The message payload sent to a stream can be any string, but a structured format such as JSON is recommended.

## Close a stream

Closing a stream removes all presences currently on it. It can be useful to explicitly close a stream and enable the server to reclaim resources more quickly.

=== "Lua"
	```lua
	local stream_id = { mode = 123, label = "my custom stream" }
	nk.stream_close(stream_id)
	```

=== "Go"
	```go
	mode := uint8(123)
	label := "label"
	nk.StreamClose(mode, "", "", label)
	```

## Counting stream presences

The server can peek at the presences on a stream to obtain a quick count without processing the full list of stream presences.

=== "Lua"
	```lua
	local stream_id = { mode = 123, label = "my custom stream" }
	local count = nk.stream_count(stream_id)
	```

=== "Go"
	```go
	mode := uint8(123)
	label := "label"
	count, err := nk.StreamCount(mode, "", "", label)
	if err != nil {
	  // Handle error here.
	}
	```

## Listing stream presences

A list of stream presence contains every user currently online and connected to that stream, along with information about the session ID they are connected through and additional metadata.

=== "Lua"
	```lua
	local stream_id = { mode = 123, label = "my custom stream" }
	local presences = nk.stream_user_list(stream_id)
	
	for _, presence in ipairs(presences) do
	  nk.logger_info("Found user ID " .. presence.user_id)
	end
	```

=== "Go"
	```go
	mode := uint8(123)
	label := "label"
	includeHidden := true
	includeNotHidden := true
	
	members, err := nk.StreamUserList(mode, "", "", label, includeHidden, includeNotHidden)
	if err != nil {
	  // Handle error here
	}
	
	for _, m := range members {
	  logger.Info("Found user: %s\n", m.GetUserId())
	}
	```

## Check a stream presence

If only a single user is needed the server can check if that user is present on a stream and retrieve their presence and metadata.

As an example we can register an RPC function that will check if the user that calls it is active on a custom stream.

=== "Lua"
	```lua
	local function check(context, _)
	  local stream_id = { mode = 123, label = "my custom stream" }
	  local meta = nk.stream_user_get(context.user_id, context.session_id, stream_id)
	
	  -- Meta is nil if the user is not present on the stream.
	  if (meta) then
	    nk.logger_info("User found on stream!")
	  end
	end
	nk.register_rpc(check, "check")
	```

=== "Go"
	```go
	func CheckStream(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
		userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	  if !ok {
	    // If user ID is not found, RPC was called without a session token.
	    return "", errors.New("Invalid context")
	  }
	  sessionID, ok := ctx.Value(runtime.RUNTIME_CTX_SESSION_ID).(string)
	  if !ok {
	    // If session ID is not found, RPC was not called over a connected socket.
	    return "", errors.New("Invalid context")
	  }
	
		mode := uint8(123)
		label := "label"
	
		if metaPresence, err := nk.StreamUserGet(mode, "", "", label, userID, sessionID); err != nil {
			// Handle error.
		} else if metaPresence != nil {
			logger.Info("User found on stream")
		} else {
			logger.Info("User not found on stream")
		}
	
		return "Success", nil
	}
	
	// Register as RPC function, this call should be in InitModule.
	if err := initializer.RegisterRpc("check", CheckStream); err != nil {
	  logger.Error("Unable to register: %v", err)
	  return err
	}
	```

## Built-in streams

The server's realtime features such as [chat channels](social-realtime-chat.md), [multiplayer](gameplay-multiplayer-realtime.md), and [notifications](social-in-app-notifications.md) are built on top of streams.

By understanding the structure of these streams the code runtime can authoritatively change any of these features.

| Stream | Mode | Subject | Descriptor | Label | Information |
| ------ | ---- | ------- | ---------- | ----- | ----------- |
| Notifications | 0 | User ID | - | - | Controls the delivery of in-app notifications to connected users. |
| Status | 1 | User ID | - | - | Controls the [status](social-status.md) feature and broadcasting updates to friends. |
| Chat Channel | 2 | - | - | "channel name" | Membership to a chat channel. |
| Group Chat | 3 | Group ID | - | - | A group's private chat channel. |
| Direct Message | 4 | User ID | User ID | - | A private direct message conversation between two users. |
| Relayed Match | 5 | Match ID | - | - | Membership and message routing for a relayed realtime multiplayer match. |
| Authoritative Match | 6 | Match ID | - | "nakama node name" | Membership and message routing for an authoritative realtime multiplayer match. |

Using these stream identifiers with the functions described above allows full control over the internal behaviour of these features.

### Example: Kick a user from a chat channel

This code removes a user from a chat channel. If the user has more than one session connected to the channel only the specified one will be removed.

=== "Lua"
	```lua
	local stream_id = { mode = 2, label = "some chat channel name" }
	local user_id = "user ID to kick"
	local session_id = "session ID to kick"
	nk.stream_user_leave(user_id, session_id, stream_id)
	```

=== "Go"
	```go
	mode := uint8(123)
	label := "some chat room channel name"
	userID := "user ID to kick"
	sessionID := "session ID to kick"
	
	if err := nk.StreamUserLeave(mode, "", "", label, userID, sessionID); err != nil {
	  // Handle error.
	}
	```

### Example: Stop receiving notifications

By calling this RPC function a user can "silence" their notifications. Even if they remain online they will no longer receive realtime delivery of any in-app notifications.

=== "Lua"
	```lua
	local function enable_silent_mode(context, _)
	  local stream_id = { mode = 0, subject = context.user_id }
	  nk.stream_user_leave(context.user_id, context.session_id, stream_id)
	end
	nk.register_rpc(enable_silent_mode, "enable_silent_mode")
	```

=== "Go"
	```go
	func EnableSilentMode(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
		userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	  if !ok {
	    // If user ID is not found, RPC was called without a session token.
	    return "", errors.New("Invalid context")
	  }
	  sessionID, ok := ctx.Value(runtime.RUNTIME_CTX_SESSION_ID).(string)
	  if !ok {
	    // If session ID is not found, RPC was not called over a connected socket.
	    return "", errors.New("Invalid context")
	  }
	
	  if err := nk.StreamUserLeave(0, userId, "", "", userID, sessionID); err != nil {
			// Handle error.
		}
	
		return "Success", nil
	}
	
	// Register as RPC function, this call should be in InitModule.
	if err := initializer.RegisterRpc("enable_silent_mode", EnableSilentMode); err != nil {
	  logger.Error("Unable to register: %v", err)
	  return err
	}
	```
