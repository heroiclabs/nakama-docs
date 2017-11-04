# Realtime Chat

Realtime chat makes it easy to power a live community.

Users can chat with each other 1-on-1, as part of a group, and in chat rooms. Messages can contain images, links, and other content. These messages are delivered immediately to clients if the recipients are online and stored in message history so offline users can catch up when they connect.

Every message which flows through the realtime chat engine belongs to a topic which is used internally to identify which users should receive the messages. Users explicitly join and leave topics when they connect. This makes it easy to selectively listen for messages which they care about or decide to "mute" certain topics when they're busy. Users can also join multiple topics at once to chat simultaneously in multiple groups or chat rooms.

There are 3 types of topic:

1. A chat room is great for public chat. Any user can join and participate without need for permission. These rooms can scale to millions of users all in simultaneous communication. This is perfect for live participation apps or games with live events or tournaments.

2. A group chat is private to only users part of a [group](social-groups-clans.md). Each user must be a member of the group and no other users can participate. You can use group chat with team-based gameplay or collaboration.

3. Direct chat is private between two users. Each user will receive a [notification](social-in-app-notifications.md) when they've been invited to chat. Both users must join for messages to be exchanged which prevents spam from bad users.

## Receive messages

A user joins a chat topic to start receiving messages in realtime. Each new message is received by an event handler and can be added to your UI. Messages are delivered in the order they are handled by the server.

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

```js fct_label="Javascript"
client.ontopicmessage = function(message) {
  console.log("Received a %o message", message.topic);
  console.log("Message content: %@.", message.data);
}
```

In group chat a user will receive other messages from the server. These messages contain events on users who join or leave the group, when someone is promoted as an admin, etc. You may want users to see these messages in the chat stream or ignore them in the UI.

You can identify event messages from chat messages by the message "Type".

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

```js fct_label="Javascript"
if (message.type != 0) {
  console.log("Received message with event type %o", message.type)
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

!!! Tip
    Each user can join many rooms, groups, and direct chat with their session. The same user can also be connected to the same chats from other devices because each device is identified as a separate session.

### rooms

A room is created dynamically for users to chat. A room has a name and will be setup on the server when any user joins. The list of room names available to join can be stored within client code or via remote configuration with a [storage record](storage-collections.md).

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

```js fct_label="Javascript"
var roomId;
var roomName = "Room-Name"

var message = new nakamajs.TopicsJoinRequest();
message.room(roomName);
client.send(message).then(function(result) {
  roomId = result.topics[0].topic;
  console.log("Successfully joined the room.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

The `roomId` variable contains an ID used to [send messages](#send-messages).

### groups

A group chat can only be joined by a user who is a member of the [group](social-groups-clans.md). Messages are pushed in realtime to group members and they can read [historic messages](#message-history).

!!! Note
    If a user is kicked or leaves a group they can no longer receive messages or read history.

A group ID is needed when a user joins group chat and can be [listed by the user](social-groups-clans.md#list-groups).

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

```js fct_label="Javascript"
var groupTopicId;
var groupId = group.id

var message = new nakamajs.TopicsJoinRequest();
message.group(groupId);
client.send(message).then(function(result) {
  groupTopicId = result.topics[0].topic;
  console.log("Successfully joined the group chat.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

The `groupTopicId` variable contains an ID used to [send messages](#send-messages).

### direct

A user can direct message another user by ID. Each user will not receive messages in realtime until both users have joined the chat. This is important because it prevents spam messages from bad users.

!!! Tip
    Friends, groups, leaderboards, matchmaker, room chat, and searches in storage are all ways to find users for chat.

A user will receive an [in-app notification](social-in-app-notifications.md) when a request to chat has been received.

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

```js fct_label="Javascript"
var directTopicId;
var userId = user.id

var message = new nakamajs.TopicsJoinRequest();
message.dm(userId);
client.send(message).then(function(result) {
  directTopicId = result.topics[0].topic;
  console.log("Successfully joined the direct chat.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

The `directTopicId` variable contains an ID used to [send messages](#send-messages).

!!! Note
    A user can [block other users](social-friends.md#block-a-friend) to stop unwanted direct messages.

## List online users

Each user who joins a chat becomes a "presence" in the chat topic. These presences keep information about which users are online.

A presence is made up of a unique session combined with a user ID. This makes it easy to distinguish between the same user connected from multiple devices in the chat topic.

The user who [joins a chat topic](#join-chat) receives an initial presence list of all other connected users in the chat topic. A callback can be used to receive presence changes from the server about users who joined and left. This makes it easy to maintain a list of online users and update it when changes occur.

!!! Summary
    A list of all online users is received when a user joins a chat topic you can combine it with an event handler which notifies when users join or leave. Together it becomes easy to maintain a list of online users.

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

```js fct_label="Javascript"
var onlineUsers = []

client.ontopicpresence = function(presence) {
  // Remove all users who left.
  onlineUsers = onlineUsers.filter(function(user) {
    return !presence.leave.includes(user);
  })

  // Add all users who joined.
  onlineUsers.concat(presence.join);
}

var roomName = "Room-Name"
var message = new nakamajs.TopicsJoinRequest();
message.room(roomName);
client.send(message).then(function(result) {
  // Setup initial online user list.
  onlineUsers.concat(result.topics[0].presences);
  // Remove your own user from list.
  onlineUsers = onlineUsers.filter(function(user) {
    return user != result.topics[0].self;
  })
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

!!! Tip
    The server is optimized to only push presence updates when other users join or leave the chat.

## Send messages

When a user has [joined a chat topic](#join-chat) it's ID can be used to send messages with JSON encoded strings.

Every message sent returns an acknowledgement when it's received by the server. The acknowledgement returned contains a message ID, timestamp, and details back about the user who sent it.

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

```js fct_label="Javascript"
var chatTopicId = ... // A chat topic ID
var data = {"some":"data"};

var message = new nakamajs.TopicMessageSendRequest();
message.topic = chatTopicId;
message.data = data;
client.send(message).then(function(ack) {
  console.log("New message sent has id %@.", ack.messageId);
}).catch(function(error){
  console.log("An error occured: %o", error);
})
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

```js fct_label="Javascript"
var chatTopicId = ... // A chat topic ID

var message = new nakamajs.TopicsLeaveRequest();
message.topics.push(chatTopicId);
client.send(message).then(function() {
  console.log("Successfully left chat.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

## Message history

Every chat conversation stores a history of messages. The history also contains [event messages](#receive-messages) sent by the server with group chat. Each user can retrieve old messages for chat when they next connect online.

Messages can be listed in order of most recent to oldest and also in reverse (oldest to newest). Messages are returned in batches of up to 100 each with a cursor for when there are more messages.

!!! Tip
    A user does not have to join a chat topic to see chat history. This is useful to "peek" at old messages without the user appearing online in the chat.

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

```js fct_label="Javascript"
var roomName = "Room-Name";

// Fetch 10 messages on the chat room with oldest first.
var message = new nakamajs.TopicMessagesListRequest();
message.room = roomName;
message.forward = false;
message.limit = 10;

client.send(message).then(function(result) {
  result.messages.forEach(function(message) {
    console.log("Message has id %o and content %o", message.topicId, message.data);
  });
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

A cursor can be used to page after a batch of messages for the next set of results.

We recommend you only list the most recent 100 messages in your UI. A good user experience could be to fetch the next 100 older messages when the user scrolls to the bottom of your UI panel.

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

```js fct_label="Javascript"
var roomName = "Room-Name";

var message = new nakamajs.TopicMessagesListRequest();
message.room = roomName;
message.limit = 100;

client.send(message).then(function(result) {
  if result.cursor && result.messages.length > 0 {
    message.cursor = result.cursor;
    client.send(message).then(function(messages) {
      messages.messages.forEach(function(message) {
        console.log("Message has id %o and content %o", message.topicId, message.data);
      });
    }).catch(function(error){
      console.log("An error occured: %o", error);
    })
  }
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```
