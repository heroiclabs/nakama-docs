# Realtime Chat

Realtime chat makes it easy to power a live community.

Users can chat with each other 1-on-1, as part of a group, and in chat rooms. Messages can contain images, links, and other content. These messages are delivered immediately to clients if the recipients are online and stored in message history so offline users can catch up when they connect.

Every message which flows through the realtime chat engine belongs to a channel which is used internally to identify which users should receive the messages. Users explicitly join and leave channels when they connect. This makes it easy to selectively listen for messages which they care about or decide to "mute" certain channels when they're busy. Users can also join multiple channels at once to chat simultaneously in multiple groups or chat rooms.

There are 3 types of channel:

1. A chat room is great for public chat. Any user can join and participate without need for permission. These rooms can scale to millions of users all in simultaneous communication. This is perfect for live participation apps or games with live events or tournaments.

2. A group chat is private to only users part of a [group](social-groups-clans.md). Each user must be a member of the group and no other users can participate. You can use group chat with team-based gameplay or collaboration.

3. Direct chat is private between two users. Each user will receive a [notification](social-in-app-notifications.md) when they've been invited to chat. Both users must join for messages to be exchanged which prevents spam from bad users.

### Persistence and message history

By default all channels are persistent, so messages sent through them are saved to the database and available in message history later. This history can be used by offline users to catch up with messages they've missed when they next connect.

If messages should only be sent to online users and never kept in messsage history, clients can join channels with persistence disabled.

### Hidden channel members

By default all users joining a channel are visible to other users. Existing channel participants will receive an event when the user connects and disconnects, and new channel joiners will receive a list of users already in the channel.

Users can opt to hide their channel presence when connecting, so they will not generate join/leave notifications and will not appear in listings of channel members. They will still be able to send and receive realtime messages as normal.

## Receive messages

A user joins a chat channel to start receiving messages in realtime. Each new message is received by an event handler and can be added to your UI. Messages are delivered in the order they are handled by the server.

```js fct_label="Javascript"
socket.onchannelmessage = function(message) {
  console.log("Received a message on channel: %o", message.channel_id);
  console.log("Message content: %@.", message.data);
}
```

```csharp fct_label="Unity"
client.OnTopicMessage = (INTopicMessage message) => {
  // TopicType will be one of DirectMessage, Room, or Group.
  Debug.LogFormat("Received a '{0}' message.", message.Topic.TopicType);
  Debug.LogFormat("Message has id '{0}' and content '{1}'.", message.Topic.Id, message.Topic.Data);
};
```

```swift fct_label="Swift"
client.onTopicMessage = { message in
  // Topic will be one of DirectMessage, Room, or Group.
  NSLog("Received a %@ message", message.topic.description)
  NSLog("Message content: %@.", message.data)
}
```

In group chat a user will receive other messages from the server. These messages contain events on users who join or leave the group, when someone is promoted as an admin, etc. You may want users to see these messages in the chat stream or ignore them in the UI.

You can identify event messages from chat messages by the message "Type".

```js fct_label="Javascript"
if (message.code != 0) {
  console.log("Received message with code %o", message.code)
}
```

```csharp fct_label="Unity"
TopicMessageType messageType = message.Type; // enum
if (messageType != TopicMessageType.Chat) {
  Debug.LogFormat("Received message with event type '{0}'.", messageType);
}
```

```swift fct_label="Swift"
let messageType = message.type; // enum
switch messageType {
  case .chat:
    NSLog("Recieved a chat message")
  default:
    NSLog("Received message with event type %d", messageType.rawValue)
}
```

| Type | Purpose | Source | Description |
| ---- | ------- | ------ | ----------- |
| 0 | chat message | user | All messages sent by users. |
| 1 | chat update | user | A user updating a message they previously sent. |
| 2 | chat remove | user | A user removing a message they previously sent. |
| 3 | joined group | server | An event message for when a user joined the group. |
| 4 | added to group | server | An event message for when a user was added to the group. |
| 5 | left group | server | An event message for when a user left a group. |
| 6 | kicked from group | server | An event message for when an admin kicked a user from the group. |
| 7 | promoted in group | server | An event message for when a user is promoted as a group admin. |

## Join chat

To send messages to other users a user must join the chat channel they want to communicate on. This will also enable messages to be [received in realtime](#receive-messages).

!!! Tip
    Each user can join many rooms, groups, and direct chat with their session. The same user can also be connected to the same chats from other devices because each device is identified as a separate session.

### rooms

A room is created dynamically for users to chat. A room has a name and will be setup on the server when any user joins. The list of room names available to join can be stored within client code or via remote configuration with a [storage record](storage-collections.md).

```js fct_label="Javascript"
var roomName = "Room-Name"

var channel = await socket.send({ channel_join: {
  type: 1, // 1 = Room, 2 = Direct Message, 3 = Group
  target: roomName,
  persistence: true,
  hidden: false
} });
console.log("You can now send messages to channel ID: %o", channel.id);
```

```csharp fct_label="Unity"
INTopicId roomId = null;

string roomName = "Room-Name";
var message = new NTopicJoinMessage.Builder()
    .TopicRoom(roomName)
    .Build();
client.Send(message, (INResultSet<INTopic> topics) => {
  roomId = topics.Results[0].Topic;
  Debug.Log("Successfully joined the room.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
var roomId : TopicId?
let roomName = "Room-Name"

var message = TopicJoinMessage()
message.rooms.append(roomName)
client.send(message: message).then { topics in
  roomId = topics[0].topic
  NSLog("Successfully joined the room.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

The `roomId` variable contains an ID used to [send messages](#send-messages).

### groups

A group chat can only be joined by a user who is a member of the [group](social-groups-clans.md). Messages are pushed in realtime to group members and they can read [historic messages](#message-history).

!!! Note
    If a user is kicked or leaves a group they can no longer receive messages or read history.

A group ID is needed when a user joins group chat and can be [listed by the user](social-groups-clans.md#list-groups).

```js fct_label="Javascript"
var groupId = group.id

var channel = await socket.send({ channel_join: {
  type: 3, // 1 = Room, 2 = Direct Message, 3 = Group
  target: groupId,
  persistence: true,
  hidden: false
} });
console.log("You can now send messages to channel ID: %o", channel.id);
```

```csharp fct_label="Unity"
INTopicId groupTopicId = null;

string groupId = group.Id; // an INGroup ID.
var message = new NTopicJoinMessage.Builder()
    .TopicGroup(groupId)
    .Build();
client.Send(message, (INResultSet<INTopic> topics) => {
  groupTopicId = topics.Results[0].Topic;
  Debug.Log("Successfully joined the group chat.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
var groupTopicId : TopicId?
let groupId = group.id

var message = TopicJoinMessage()
message.groups.append(groupId)
client.send(message: message).then { topics in
  groupTopicId = topics[0].topic
  NSLog("Successfully joined the group chat.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

The `groupTopicId` variable contains an ID used to [send messages](#send-messages).

### direct

A user can direct message another user by ID. Each user will not receive messages in realtime until both users have joined the chat. This is important because it prevents spam messages from bad users.

!!! Tip
    Friends, groups, leaderboards, matchmaker, room chat, and searches in storage are all ways to find users for chat.

A user will receive an [in-app notification](social-in-app-notifications.md) when a request to chat has been received.

```js fct_label="Javascript"
var userId = user.id

var channel = await socket.send({ channel_join: {
  type: 2, // 1 = Room, 2 = Direct Message, 3 = Group
  target: groupId,
  persistence: true,
  hidden: false
} });
console.log("You can now send messages to channel ID: %o", channel.id);
```

```csharp fct_label="Unity"
INTopicId directTopicId = null;

string userId = user.Id; // an INUser ID.
var message = new NTopicJoinMessage.Builder()
    .TopicDirectMessage(userId)
    .Build();
client.Send(message, (INResultSet<INTopic> topics) => {
  directTopicId = topics.Results[0].Topic;
  Debug.Log("Successfully joined the direct chat.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
var directTopicId : TopicId?
let userId = user.id

var message = TopicJoinMessage()
message.userIds.append(userIds)
client.send(message: message).then { topics in
  directTopicId = topics[0].topic
  NSLog("Successfully joined the direct chat.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

The `directTopicId` variable contains an ID used to [send messages](#send-messages).

!!! Note
    A user can [block other users](social-friends.md#block-a-friend) to stop unwanted direct messages.

## List online users

Each user who joins a chat becomes a "presence" in the chat channel - unless they've joined as a "hidden" channel participant. These presences keep information about which users are online.

A presence is made up of a unique session combined with a user ID. This makes it easy to distinguish between the same user connected from multiple devices in the chat channel.

The user who [joins a chat channel](#join-chat) receives an initial presence list of all other connected users in the chat channel. A callback can be used to receive presence changes from the server about users who joined and left. This makes it easy to maintain a list of online users and update it when changes occur.

!!! Summary
    A list of all online users is received when a user joins a chat channel you can combine it with an event handler which notifies when users join or leave. Together it becomes easy to maintain a list of online users.

```js fct_label="Javascript"
var onlineUsers = [];

socket.onchannelpresence = function(presences) {
  // Remove all users who left.
  onlineUsers = onlineUsers.filter(function(user) {
    return !presences.leave.includes(user);
  })
  // Add all users who joined.
  onlineUsers.concat(presences.join);
}

var roomName = "Room-Name";
var channel = await socket.send({ channel_join: {
  type: 1, // 1 = Room, 2 = Direct Message, 3 = Group
  target: roomName,
  persistence: true,
  hidden: false
} });

// Setup initial online user list.
onlineUsers.concat(channel.presences);
// Remove your own user from list.
onlineUsers = onlineUsers.filter(function(user) {
  return user != channel.self;
})
```

```csharp fct_label="Unity"
IList<INUserPresence> onlineUsers = new List<INUserPresence>();

client.OnTopicPresence = (INTopicPresence presences) => {
  // Remove all users who left.
  foreach (var user in presences.Leave) {
    onlineUsers.Remove(user);
  }
  // Add all users who joined.
  onlineUsers.AddRange(presences.Join);
};

string roomName = "Room-Name";
var message = new NTopicJoinMessage.Builder()
    .TopicRoom(roomName)
    .Build();
client.Send(message, (INResultSet<INTopic> topics) => {
  // Setup initial online user list.
  onlineUsers.AddRange(topics.Results[0].Presences);
  // Remove your own user from list.
  onlineUsers.Remove(topics.Results[0].Self);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
var onlineUsers : [UserPresence] = []

client.onTopicPresence = { presence in
  // Remove all users who left.
  for user in presence.leave {
    onlineUsers.removeAtIndex(onlineUsers.indexOf(user))
  }
  // Add all users who joined.
  onlineUsers.append(contentOf: presence.join)
}

let roomName = "Room-Name"

var message = TopicJoinMessage()
message.rooms.append(roomName)
client.send(message: message).then { topics in
  // Setup initial online user list.
  onlineUsers.append(contentOf: topic[0].presences)
  // Remove your own user from list.
  onlineUsers.removeAtIndex(onlineUsers.indexOf(topic[0].self))
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

!!! Tip
    The server is optimized to only push presence updates when other users join or leave the chat.

## Send messages

When a user has [joined a chat channel](#join-chat) its ID can be used to send messages with JSON encoded strings.

Every message sent returns an acknowledgement when it's received by the server. The acknowledgement returned contains a message ID, timestamp, and details back about the user who sent it.

```js fct_label="Javascript"
var channelId = "..."; // A channel ID obtained previously from channel.id
var data = {"some":"data"};

var message_ack = await socket.send({ channel_message_send: {
  channel_id: channelId,
  content: data
} })
```

```csharp fct_label="Unity"
INTopicId chatTopicId = topic.Topic; // A chat topic ID.

var json = "{\"some\":\"data\"}";
var message = NTopicMessageSendMessage.Default(chatTopicId, json);
client.Send(message, (INTopicMessageAck ack) => {
  Debug.LogFormat("New message sent has id '{0}'.", ack.MessageId);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
let chatTopicID = ... // A chat topic ID
let json = "{\"some\":\"data\"}".data(using: .utf8)!

var message = TopicMessageSendMessage()
message.topicId = chatTopicID
message.data = json
client.send(message: message).then { ack in
  NSLog("New message sent has id %@.", ack.messageID)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

## Leave chat

A user can leave a chat channel to no longer be sent messages in realtime. This can be useful to "mute" a chat while in some other part of the UI.

```js fct_label="Javascript"
var channelId = "..."; // A channel ID obtained previously from channel.id

await socket.send({ channel_leave: {
  channel_id: channelId
} });
```

```csharp fct_label="Unity"
INTopicId chatTopicId = topic.Topic; // A chat topic ID.

var message = NTopicLeaveMessage.Default(chatTopicId);
client.Send(message, (bool done) => {
  Debug.Log("Successfully left chat.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
let chatTopicID = ... // A chat topic ID

var message = TopicLeaveMessage()
message.topics.append(chatTopicID)
client.send(message: message).then {
  NSLog("Successfully left chat.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

## Message history

Every chat conversation stores a history of messages - unless the user sending the message has disabled persistence. The history also contains [event messages](#receive-messages) sent by the server to group chat channels. Each user can retrieve old messages for channels when they next connect online.

Messages can be listed in order of most recent to oldest and also in reverse (oldest to newest). Messages are returned in batches of up to 100 each with a cursor for when there are more messages.

!!! Tip
    A user does not have to join a chat channel to see chat history. This is useful to "peek" at old messages without the user appearing online in the chat.

```sh fct_label="cURL"
curl -X GET \
  --url http://127.0.0.1:7350/v2/channel?channel_id=<channelId> \
  --header 'authorization: Bearer <session token>' \
  --header 'content-type: application/json'
```

```js fct_label="Javascript"
var channelId = "..."; // A channel ID obtained previously from channel.id

var result = await client.listChannelMessages(session, channelId, 10)
result.messages.forEach(function(message) {
  console.log("Message has id %o and content %o", message.message_id, message.data);
});
console.log("Get the next page of messages with the cursor: %o", result.next_cursor);
```

```fct_label="REST"
GET /v2/channel?channel_id=<channelId>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

```csharp fct_label="Unity"
string roomName = "Room-Name";
// Fetch 10 messages on the chat room with oldest first.
var message = new NTopicMessagesListMessage.Builder()
    .TopicRoom(roomName)
    .Forward(false)
    .Limit(10)
    .Build();
client.Send(message, (INResultSet<INTopicMessage> list) => {
  foreach (var msg in list.Results) {
    var id = msg.Topic.Id;
    var data = msg.Topic.Data;
    Debug.LogFormat("Message has id '{0}' and content '{1}'.", id, data);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
let roomName = "Room-Name";

// Fetch 10 messages on the chat room with oldest first.
var message = TopicMessagesListMessage(room: roomName)
message.forward(false)
message.limit(10)
client.send(message: message).then { messages in
  for message in messages {
    NSLog("Message has id %@ and content %@", message.topicId.description, message.data)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

A cursor can be used to page after a batch of messages for the next set of results.

We recommend you only list the most recent 100 messages in your UI. A good user experience could be to fetch the next 100 older messages when the user scrolls to the bottom of your UI panel.

```sh fct_label="cURL"
curl -X GET \
  --url http://127.0.0.1:7350/v2/channel?channel_id=<channelId>&forward=true&limit=10&cursor=<cursor> \
  --header 'authorization: Bearer <session token>' \
  --header 'content-type: application/json'
```

```js fct_label="Javascript"
var channelId = "..."; // A channel ID obtained previously from channel.id
var forward = true; // List from oldest message to newest.

var result = await client.listChannelMessages(session, channelId, 10, forward)
result.messages.forEach(function(message) {
  console.log("Message has id %o and content %o", message.message_id, message.data);
});

if result.next_cursor {
  // Get the next 10 messages.
  var result = await client.listChannelMessages(session, channelId, 10, forward, result.next_cursor)
  result.messages.forEach(function(message) {
    console.log("Message has id %o and content %o", message.message_id, message.data);
  });
}
```

```fct_label="REST"
GET /v2/channel?channel_id=<channelId>&forward=true&limit=10&cursor=<cursor>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

```csharp fct_label="Unity"
var errorHandler = delegate(INError err) {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

string roomName = "Room-Name";
var messageBuilder = new NTopicMessagesListMessage.Builder()
    .TopicRoom(roomName)
    .Limit(100);
client.Send(messageBuilder.Build(), (INResultSet<INTopicMessage> list) => {
  // Lets get the next page of results.
  INCursor cursor = list.Cursor;
  if (cursor != null && list.Results.Count > 0) {
    var message = messageBuilder.Cursor(cursor).Build();

    client.Send(message, (INResultSet<INTopicMessage> nextList) => {
      foreach (var msg in nextList.Results) {
        var id = msg.Topic.Id;
        var data = msg.Topic.Data;
        Debug.LogFormat("Message has id '{0}' and content '{1}'.", id, data);
      }
    }, errorHandler);
  }
}, errorHandler);
```

```swift fct_label="Swift"
let roomName = "Room-Name";

var message = TopicMessagesListMessage(room: roomName)
message.limit(100)
client.send(message: message).then { messages in
  if let _cursor = messages.cursor && messages.count > 0 {
    message.cursor = _cursor
    client.send(message: message).then { messages in
      for message in messages {
        NSLog("Message has id %@ and content %@", message.topicId.description, message.data)
      }
    }.catch { err in
      NSLog("Error %@ : %@", err, (err as! NakamaError).message)
    }
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```
