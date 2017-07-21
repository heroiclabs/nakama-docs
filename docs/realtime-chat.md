# Realtime Chat

Realtime chat makes it easy to power a live community.

Users can chat with each other 1-on-1, as part of a group, and in chat rooms. Messages can contain images, links, and other content. These messages are delivered immediately to clients if the recipients are online and stored in message history so offline users can catch up when they connect.

Every message which flows through the realtime chat engine belongs to a topic which is used internally to identify which users should receive the messages. Users explicitly join and leave topics when they connect. This makes it easy to selectively listen for messages which they care about or decide to "mute" certain topics when they're busy. Users can also join multiple topics at once to chat simultaneously in multiple groups or chat rooms.

There are 3 types of topic:

1. A chat room is great for public chat. Any user can join and participate without need for permission. These rooms can scale to millions of users all in simultaneous communication. This is perfect for live participation apps or games with live events or tournaments.

2. A group chat is private to only users part of a [group](groups-clans.md). Each user must be a member of the group and no other users can participate. You can use group chat with team-based gameplay or collaboration.

3. Direct chat is private between two users. Each user will receive a [notification](in-app-notifications.md) when they've been invited to chat. Both users must join for messages to be exchanged which prevents spam from bad users.

## Receive messages

A user joins a chat topic to start receiving messages in realtime. Each new message is received by an event handler and can be added to your UI. Messages are delivered in the order they are handled by the server.

```csharp fct_label="Unity"
client.OnTopicMessage += (object source, NTopicMessageEventArgs args) => {
  INTopicMessage message = args.TopicMessage;
  // TopicType will be one of DirectMessage, Room, or Group.
  Debug.LogFormat("Received a '{0}' message.", message.Topic.TopicType);
  var id = Encoding.UTF8.GetString(message.Topic.Id);     // convert byte[].
  var data = Encoding.UTF8.GetString(message.Topic.Data); // convert byte[].
  Debug.LogFormat("Message has id '{0}' and content '{1}'.", id, data);
};
```

In group chat a user will receive other messages from the server. These messages contain events on users who join or leave the group, when someone is promoted as an admin, etc. You may want users to see these messages in the chat stream or ignore them in the UI.

You can identify event messages from chat messages by the message "Type".

```csharp fct_label="Unity"
TopicMessageType messageType = message.Type; // enum
if (messageType != TopicMessageType.Chat) {
  Debug.LogFormat("Received message with event type '{0}'.", messageType);
}
```

| Type | Purpose | Source | Description |
| ---- | ------- | ------ | ----------- |
| 0 | chat message | user | All messages sent by users. |
| 1 | joined group | server | An event message for when a user joined the group. |
| 2 | added to group | server | An event message for when a user was added to the group. |
| 3 | left group | server | An event message for when a user left a group. |
| 4 | kicked from group | server | An event message for when an admin kicked a user from the group. |
| 5 | promoted in group | server | An event message for when a user is promoted as a group admin. |

## Join chat

To send messages to other users a user must join the chat topic they want to communicate on. This will also enable messages to be [received in realtime](#receive-messages).

!!! tip
    Each user can join many rooms, groups, and direct chat with their session. The same user can also be connected to the same chats from other devices because each device is identified as a separate session.

### rooms

A room is created dynamically for users to chat. A room has a name and will be setup on the server when any user joins. The list of room names available to join can be stored within client code or via remote configuration with a [storage record](storage-collections.md).

```csharp fct_label="Unity"
INTopicId roomId = null;

byte[] roomName = Encoding.UTF8.GetBytes("Room-Name"); // convert string.
var message = new NTopicJoinMessage.Builder()
    .TopicRoom(roomName)
    .Build();
client.Send(message, (INTopic topic) => {
  roomId = topic.Topic;
  Debug.Log("Successfully joined the room.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

The `roomId` variable contains an ID used to [send messages](#send-messages).

### groups

A group chat can only be joined by a user who is a member of the [group](groups-clans.md). Messages are pushed in realtime to group members and they can read [historic messages](#message-history).

!!! note
    If a user is kicked or leaves a group they can no longer receive messages or read history.

A group ID is needed when a user joins group chat and can be [listed by the user](groups-clans.md#list-groups).

```csharp fct_label="Unity"
INTopicId groupTopicId = null;

byte[] groupId = group.Id; // an INGroup ID.
var message = new NTopicJoinMessage.Builder()
    .TopicGroup(groupId)
    .Build();
client.Send(message, (INTopic topic) => {
  groupTopicId = topic.Topic;
  Debug.Log("Successfully joined the group chat.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

The `groupTopicId` variable contains an ID used to [send messages](#send-messages).

### direct

A user can direct message another user by ID. Each user will not receive messages in realtime until both users have joined the chat. This is important because it prevents spam messages from bad users.

!!! tip
    Friends, groups, leaderboards, matchmaker, room chat, and searches in storage are all ways to find users for chat.

A user will receive an [in-app notification](in-app-notifications.md) when a request to chat has been received.

```csharp fct_label="Unity"
INTopicId directTopicId = null;

byte[] userId = user.Id; // an INUser ID.
var message = new NTopicJoinMessage.Builder()
    .TopicDirectMessage(userId)
    .Build();
client.Send(message, (INTopic topic) => {
  directTopicId = topic.Topic;
  Debug.Log("Successfully joined the direct chat.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

The `directTopicId` variable contains an ID used to [send messages](#send-messages).

!!! note
    A user can [block other users](friends.md#block-a-friend) to stop unwanted direct messages.

## List online users

Each user who joins a chat becomes a "presence" in the chat topic. These presences keep information about which users are connected.

A presence is made up of a unique session combined with a user ID. This makes it easy to distinguish between the same user connected from multiple devices in the chat topic.

The user who [joins a chat topic](#join-chat) receives an initial presence list of all other connected users in the chat topic. An event handler can be used to receive "presence" changes from the server about users who joined and left. This makes it easy to maintain a list of online users and update it when changes occur.

!!! summary
    A list of all online users is received when a user joins a chat topic you can combine it with an event handler which notifies when users join or leave. Together it becomes easy to maintain a list of online users.

```csharp fct_label="Unity"
IList<INUserPresence> onlineUsers = new List<INUserPresence>();

client.OnTopicPresence += (object source, NTopicPresenceEventArgs args) => {
  INTopicPresence presenceUpdate = args.TopicPresence;
  // Remove all users who left.
  foreach (var user in presenceUpdate.Leave) {
    onlineUsers.Remove(user);
  }
  // Add all users who joined.
  onlineUsers.AddRange(presenceUpdate.Join);
};

byte[] roomName = Encoding.UTF8.GetBytes("Room-Name"); // convert string.
var message = new NTopicJoinMessage.Builder()
    .TopicRoom(roomName)
    .Build();
client.Send(message, (INTopic topic) => {
  // Setup initial online user list.
  onlineUsers.AddRange(topic.Presences);
  // Remove your own user from list.
  onlineUsers.Remove(topic.Self);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

!!! tip
    The server is optimized to only push presence updates when other users join or leave the chat.

## Send messages

When a user has [joined a chat topic](#join-chat) it's ID can be used to send messages with JSON encoded strings.

Every message sent returns an acknowledgement when it's received by the server. The acknowledgement returned contains a message ID, timestamp, and details back about the user who sent it.

```csharp fct_label="Unity"
INTopicId chatTopicId = topic.Topic; // A chat topic ID.

var json = "{'some':'data'}";
byte[] data = Encoding.UTF8.GetBytes(json);
var message = NTopicMessageSendMessage.Default(chatTopicId, data);
client.Send(message, (INTopicMessageAck ack) => {
  var messageId = Encoding.UTF8.GetString(ack.MessageId); // convert byte[].
  Debug.LogFormat("New message sent has id '{0}'.", messageId);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

## Leave chat

A user can leave a chat topic to no longer be sent messages in realtime. This can be useful to "mute" a chat while in some other part of the UI.

```csharp fct_label="Unity"
INTopicId chatTopicId = topic.Topic; // A chat topic ID.

var message = NTopicLeaveMessage.Default(chatTopicId);
client.Send(message, (bool done) => {
  Debug.Log("Successfully left chat.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

## Message history

Every chat conversation stores a history of messages. The history also contains [event messages](#receive-messages) sent by the server with group chat. Each user can retrieve old messages for chat when they next connect online.

Messages can be listed in order of most recent to oldest and also in reverse (oldest to newest). Messages are returned in batches of up to 100 each with a cursor for when there are more messages.

!!! tip
    A user does not have to join a chat topic to see chat history. This is useful to "peek" at old messages without the user appearing online in the chat.

```csharp fct_label="Unity"
byte[] roomName = Encoding.UTF8.GetBytes("Room-Name"); // convert string.
// Fetch 10 messages on the chat room with oldest first.
var message = new NTopicMessagesListMessage.Builder()
    .TopicRoom(roomName)
    .Forward(false)
    .Limit(10)
    .Build();
client.Send(message, (INResultSet<INTopicMessage> list) => {
  foreach (var msg in list.Results) {
    var id = Encoding.UTF8.GetString(msg.Topic.Id);     // convert byte[].
    var data = Encoding.UTF8.GetString(msg.Topic.Data); // convert byte[].
    Debug.LogFormat("Message has id '{0}' and content '{1}'.", id, data);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

A cursor can be used to page after a batch of messages for the next set of results.

We recommend you only list the most recent 100 messages in your UI. A good user experience could be to fetch the next 100 older messages when the user scrolls to the bottom of your UI panel.

```csharp fct_label="Unity"
var errorHandler = delegate(INError err) {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

byte[] roomName = Encoding.UTF8.GetBytes("Room-Name"); // convert string.
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
        var id = Encoding.UTF8.GetString(msg.Topic.Id);     // convert byte[].
        var data = Encoding.UTF8.GetString(msg.Topic.Data); // convert byte[].
        Debug.LogFormat("Message has id '{0}' and content '{1}'.", id, data);
      }
    }, errorHandler);
  }
}, errorHandler);
```
