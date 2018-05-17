# Streams

!!! Warning "Here be dragons"
    Streams are a powerful feature that gives you full control over Nakama's internal realtime routing and delivery. Use with caution!

Nakama's realtime message routing and delivery layer is organised into streams. Streams tie together clients interested in certain message types and allow Nakama's internal components to deliver messages to relevant users.

Clients may receive messages and data from streams, but are not allowed to directly join, leave, or send data themselves. These functions are only available in the server [code runtime](runtime-code-basics.md).

All of the higher-level realtime features in the server ([chat channels](social-realtime-chat.md), [multiplayer](gameplay-multiplayer-realtime.md), [notifications](social-in-app-notifications.md), etc.) are built as abstractions on top of the streams system. Understanding and using the low-level streams system is not necessary to use these features.

## Structure of a stream

Streams are defined by two components: a **stream identifier** and **presence list**.

### Stream identifier

All streams have their own unique identifier. This allows operations to uniquely add users to streams, then locate streams for message delivery. A stream identifier has 4 fields:

* __Mode__ marks the type of stream. For example chat channels have different names but the same mode.
* __Subject__ contains a primary stream subject, such as a user ID. This usually applies to streams that are scoped to users.
* __Descriptor__ is a secondary identifier. Used when a stream is relevant to a pair of users or groups, such as [direct chat](social-realtime-chat.md) between two users.
* __Label__ stores a string which isn't contextual to a specific user or other Nakama data types. A chat room created by name uses the label field.

!!! Tip
    Mode is the only required field.

### Presence list

Streams are ultimately a way to address a set of online users and deliver messages to them. Each stream maintains a list of presences that uniquely identify a user and the session they are connected through.

When the server sends a message to a stream it will be delivered to all clients identified by the presence list.

### Persistence and message history

Presences may be marked with an optional persistence flag. The server may observe this flag when handling messages delivered to a stream to decide wether the message data should be stored in the database.

The [realtime chat feature](social-realtime-chat.md) uses this flag to decide if messages should be persisted and made available later through to clients requesting message history.

### Hidden stream members

Streams generate presence events that notify all users currently in the stream when a new user joins or an existing user leaves the stream. Presences may be marked with an optional hidden flag. When this is set to true the server will not generate presence events when this presence joins or leaves.

!!! Tip
    Hidden presences are still full stream members, so they'll receive data and presence events as normal.

## Receiving stream data

Clients must register an event handler to receive stream data when it's received over the socket. The handler function will be called once for each stream data message.

```js fct_label="Javascript"
socket.onstreamdata = (streamdata) => {
  console.log("Received data from stream: %o", streamdata.id)
  console.log("Data content: %@.", streamdata.data);
};
```

## Receiving stream presence events

When a new presence joins a stream or an existing presence leaves the server will broadcast presence events to all users currently in the stream.

```js fct_label="Javascript"
socket.onstreampresence = (streampresence) => {
  console.log("Received presence event for stream: %o", streampresence.id)
  streampresence.joins.forEach((join) => {
    console.log("New user joined: %o", join.user_id);
  });
  streampresence.leaves.forEach((leave) => {
    console.log("User left: %o", leave.user_id);
  });
};
```

!!! Tip
    Hidden presences do not generate presence events, and don't result in a call to this event handler.

## Join a stream

The server can place users on any number of streams at its discretion. To add a user to a stream the server needs the user's ID, the unique session ID of the user's current session, and information about the stream they should be placed on.

As an example we can register an RPC function that will place the user that calls it on a pre-determined stream.

```lua
local function join(context, _)
  local stream_id = { mode = 123, label = "my custom stream" }
  local hidden = false
  local persistence = false
  nk.stream_user_join(context.user_id, context.session_id, stream_id, hidden, persistence)
end
nk.register_rpc(join, "join")
```

If this user-session is already a member of the stream, the operation will be a no-op.

## Leave a stream

Leaving streams is also controlled exclusively by the server. To remove a user to a stream the server needs the user's ID, the unique session ID of the user's current session, and information about the stream they should be removed from.

As an example we can register an RPC function that will remove the user that calls it from a pre-determined stream.

```lua
local function leave(context, _)
  local stream_id = { mode = 123, label = "my custom stream" }
  nk.stream_user_leave(context.user_id, context.session_id, stream_id)
end
nk.register_rpc(leave, "leave")
```

If this user-session is not a member of the stream, the operation will be a no-op.

!!! Note
    Just like [chat channels](social-realtime-chat.md) and [realtime multiplayer matches](gameplay-multiplayer-realtime.md) when a client disconnects it is automatically removed from any streams it was part of.

## Send data to a stream

The server can send data to a stream through a function call. The message will be delivered to all users present on the stream.

```lua
local stream_id = { mode = 123, label = "my custom stream" }
local payload = nk.json_encode({ some = "data" })
nk.stream_data_send(stream_id, payload)
```

If the stream is empty the operation will be a no-op.

!!! Tip
    The message payload sent to a stream can be any string, but a structured format such as JSON is usually best.

## Close a stream

Closing a stream removes all presences currently on it. It's a convenient way to explicitly close a channel and indicate to the server that all associated resources are no longer needed.

```lua
local stream_id = { mode = 123, label = "my custom stream" }
nk.stream_close(stream_id)
```

## Counting stream presences

The server can peek at the presences on a stream to obtain a quick count without processing the full list of stream presences.

```lua
local stream_id = { mode = 123, label = "my custom stream" }
local count = nk.stream_count(stream_id)
```

## Listing stream presences

A list of stream presence contains every user currently online and connected to that stream, along with information about the session ID they are connected through and additional metadata.

```lua
local stream_id = { mode = 123, label = "my custom stream" }
local presences = nk.streamUserList(stream_id)

for i, presence in ipairs(presences) do
  print("Found user ID " .. presence.user_id)
end
```

## Check a stream presence

If only a single user is needed, the server can check if that user is present on a stream and retrieve their presence information and metadata.

As an example we can register an RPC function that will check if the user that calls it is present on a pre-determined stream.

```lua
local function check(context, _)
  local stream_id = { mode = 123, label = "my custom stream" }
  local meta = nk.stream_user_get(context.user_id, context.session_id, stream_id)

  -- Meta is nil if the user was not present on the stream.
  if (meta) then
    print("User found on stream!")
  end
end
nk.register_rpc(check, "check")
```

## Built-in streams

Nakama's high level realtime features such as [chat channels](social-realtime-chat.md), [multiplayer](gameplay-multiplayer-realtime.md), and [notifications](social-in-app-notifications.md) are built on top of pre-defined streams.

By understanding the structure of these streams the script runtime can authoritatively control any of these features.

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

This code removes a user from a chat channel. If the user has more than one session connected to the channel, only the specified one will be removed.

```lua
local stream_id = { mode = 2, label = "some chat channel name" }
local user_id = "user ID to kick"
local session_id = "session ID to kick"
nk.stream_user_leave(user_id, session_id, stream_id)
```

### Example: Stop receiving notifications

By calling this RPC function a user can "silence" their notifications. Even if they remain online they will no longer receive realtime delivery of any in-app notifications.

```lua
local function enable_silent_mode(context, _)
  local stream_id = { mode = 0, subject = context.user_id }
  nk.stream_user_leave(context.user_id, context.session_id, stream_id)
end
nk.register_rpc(enable_silent_mode, "enable_silent_mode")
```
