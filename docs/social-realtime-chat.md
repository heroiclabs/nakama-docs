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

```js fct_label="JavaScript"
socket.onchannelmessage = (message) => {
  console.log("Received a message on channel: %o", message.channel_id);
  console.log("Message content: %o", message.content);
};
```

```csharp fct_label=".NET"
socket.ReceivedChannelMessage += message =>
{
    Console.WriteLine("Received: {0}", message);
    Console.WriteLine("Message has channel id: {0}", message.ChannelId);
    Console.WriteLine("Message content: {0}", message.Content);
};
```

```csharp fct_label="Unity"
socket.ReceivedChannelMessage += message =>
{
    Debug.LogFormat("Received: {0}", message);
    Debug.LogFormat("Message has channel id: {0}", message.ChannelId);
    Debug.LogFormat("Message content: {0}", message.Content);
};
```

```cpp fct_label="Cocos2d-x C++"
rtListener->setChannelMessageCallback([](const NChannelMessage& msg)
{
  // msg.content is JSON string
  CCLOG("OnChannelMessage %s", msg.content.c_str());
});
```

```js fct_label="Cocos2d-x JS"
socket.onchannelmessage = (message) => {
  cc.log("Received a message on channel:", message.channel_id);
  cc.log("Message content:", JSON.stringify(message.content));
};
```

```cpp fct_label="C++"
rtListener->setChannelMessageCallback([](const NChannelMessage& msg)
{
  // msg.content is JSON string
  std::cout << "OnChannelMessage " << msg.content << std::cout;
});
```

```java fct_label="Java"
SocketListener listener = new AbstractSocketListener() {
  @Override
  public void onChannelMessage(final ChannelMessage message) {
    System.out.format("Received a message on channel %s", message.getChannelId());
    System.out.format("Message content: %s", message.getContent());
  }
};
```

```swift fct_label="Swift"
// Requires Nakama 1.x
client.onTopicMessage = { message in
  // Topic will be one of DirectMessage, Room, or Group.
  NSLog("Received a %@ message", message.topic.description)
  NSLog("Message content: %@", message.data)
}
```

In group chat a user will receive other messages from the server. These messages contain events on users who join or leave the group, when someone is promoted as an admin, etc. You may want users to see these messages in the chat stream or ignore them in the UI.

You can identify event messages from chat messages by the message "Type".

```js fct_label="JavaScript"
if (message.code != 0) {
  console.log("Received message with code:", message.code);
}
```

```csharp fct_label=".NET"
if (message.Code != 0)
{
  Console.WriteLine("Received message with code '{0}'", message.Code);
}
```

```csharp fct_label="Unity"
if (message.Code != 0)
{
  Debug.LogFormat("Received message with code '{0}'", message.Code);
}
```

```cpp fct_label="Cocos2d-x C++"
if (msg.code != 0)
{
  CCLOG("Received message with code: %d", msg.code);
}
```

```js fct_label="Cocos2d-x JS"
if (message.code != 0) {
  cc.log("Received message with code:", message.code);
}
```

```cpp fct_label="C++"
if (msg.code != 0)
{
  std::cout << "Received message with code: " << msg.code << std::endl;
}
```

```java fct_label="Java"
if (message.getCode() != 0) {
  System.out.println("Received message with code %s", message.getCode());
}
```

```swift fct_label="Swift"
// Requires Nakama 1.x
let messageType = message.type; // enum
switch messageType {
  case .chat:
    NSLog("Recieved a chat message")
  default:
    NSLog("Received message with event type %d", messageType.rawValue)
}
```

| Code | Purpose | Source | Description |
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

```js fct_label="JavaScript"
const roomname = "MarvelMovieFans";
const response = await socket.send({ channel_join: {
    type: 1, // 1 = Room, 2 = Direct Message, 3 = Group
    target: roomname,
    persistence: true,
    hidden: false
} });
console.log("Now connected to channel id: '%o'", response.channel.id);
```

```csharp fct_label=".NET"
var roomname = "MarvelMovieFans";
var persistence = true;
var hidden = false;
var channel = await socket.JoinChatAsync(roomname, ChannelType.Room, persistence, hidden);
Console.WriteLine("Now connected to channel id: '{0}'", channel.Id);
```

```csharp fct_label="Unity"
var roomname = "MarvelMovieFans";
var persistence = true;
var hidden = false;
var channel = await socket.JoinChatAsync(roomname, ChannelType.Room, persistence, hidden);
Debug.LogFormat("Now connected to channel id: '{0}'", channel.Id);
```

```cpp fct_label="Cocos2d-x C++"
auto successCallback = [](NChannelPtr channel)
{
  CCLOG("Now connected to channel id: '%s'", channel->id.c_str());
};

string roomname = "MarvelMovieFans";
rtClient->joinChat(
    roomname,
    NChannelType::ROOM,
    true,  // persistence
    false, // hidden
    successCallback
);
```

```js fct_label="Cocos2d-x JS"
const roomname = "MarvelMovieFans";
socket.send({ channel_join: {
    type: 1, // 1 = Room, 2 = Direct Message, 3 = Group
    target: roomname,
    persistence: true,
    hidden: false
} }).then(function(response) {
      cc.log("Now connected to channel id: ", response.channel.id);
    },
    function(error) {
      cc.error("join channel failed:", JSON.stringify(error));
    });
```

```cpp fct_label="C++"
auto successCallback = [](NChannelPtr channel)
{
  std::cout << "Now connected to channel id: " << channel->id << std::endl;
};

string roomname = "MarvelMovieFans";
rtClient->joinChat(
    roomname,
    NChannelType::ROOM,
    true,  // persistence
    false, // hidden
    successCallback
);
```

```java fct_label="Java"
String roomname = "MarvelMovieFans";
boolean persistence = true;
boolean hidden = false;
Channel channel = socket.joinChat(roomname, ChannelType.ROOM, persistence, hidden).get();
System.out.format("Now connected to channel id: %s", channel.getId());
```

```swift fct_label="Swift"
// Requires Nakama 1.x
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

```js fct_label="JavaScript"
const groupId = "<group id>";
const response = await socket.send({ channel_join: {
    type: 3, // 1 = Room, 2 = Direct Message, 3 = Group
    target: groupId,
    persistence: true,
    hidden: false
} });
console.log("You can now send messages to channel id: ", response.channel.id);
```

```csharp fct_label=".NET"
var groupId = "<group id>";
var persistence = true;
var hidden = false;
var channel = await socket.JoinChatAsync(groupId, ChannelType.Group, persistence, hidden);
Console.WriteLine("You can now send messages to channel id: '{0}'", channel.Id);
```

```csharp fct_label="Unity"
var groupId = "<group id>";
var persistence = true;
var hidden = false;
var channel = await socket.JoinChatAsync(groupId, ChannelType.Group, persistence, hidden);
Debug.LogFormat("You can now send messages to channel id: '{0}'", channel.Id);
```

```cpp fct_label="Cocos2d-x C++"
auto successCallback = [](NChannelPtr channel)
{
  CCLOG("You can now send messages to channel id: %s", channel->id.c_str());
};

string groupId = "<group id>";
rtClient->joinChat(
    groupId,
    NChannelType::GROUP,
    true,  // persistence
    false, // hidden
    successCallback
);
```

```js fct_label="Cocos2d-x JS"
const groupId = "<group id>";
socket.send({ channel_join: {
    type: 3, // 1 = Room, 2 = Direct Message, 3 = Group
    target: groupId,
    persistence: true,
    hidden: false
} }).then(function(response) {
      cc.log("You can now send messages to channel id:", response.channel.id);
    },
    function(error) {
      cc.error("join channel failed:", JSON.stringify(error));
    });
```

```cpp fct_label="C++"
auto successCallback = [](NChannelPtr channel)
{
  std::cout << "You can now send messages to channel id: " << channel->id << std::endl;
};

string groupId = "<group id>";
rtClient->joinChat(
    groupId,
    NChannelType::GROUP,
    true,  // persistence
    false, // hidden
    successCallback
);
```

```java fct_label="Java"
String groupId = "<group id>";
boolean persistence = true;
boolean hidden = false;
Channel channel = socket.joinChat(groupId, ChannelType.GROUP, persistence, hidden).get();
System.out.format("You can now send messages to channel id %s", channel.getId());
```

```swift fct_label="Swift"
// Requires Nakama 1.x
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

The `"<group id>"` variable must be an ID used to [send messages](#send-messages).

### direct

A user can direct message another user by ID. Each user will not receive messages in realtime until both users have joined the chat. This is important because it prevents spam messages from bad users.

!!! Tip
    Friends, groups, leaderboards, matchmaker, room chat, and searches in storage are all ways to find users for chat.

A user will receive an [in-app notification](social-in-app-notifications.md) when a request to chat has been received.

```js fct_label="JavaScript"
const userId = "<user id>";
const response = await socket.send({ channel_join: {
    type: 2, // 1 = Room, 2 = Direct Message, 3 = Group
    target: userId,
    persistence: true,
    hidden: false
} });
console.log("You can now send messages to channel id:", response.channel.id);
```

```csharp fct_label=".NET"
var userId = "<user id>";
var persistence = true;
var hidden = false;
var channel = await socket.JoinChatAsync(userId, ChannelType.DirectMessage, persistence, hidden);
Console.WriteLine("You can now send messages to channel id: '{0}'", channel.Id);
```

```csharp fct_label="Unity"
var userId = "<user id>";
var persistence = true;
var hidden = false;
var channel = await socket.JoinChatAsync(userId, ChannelType.DirectMessage, persistence, hidden);
Debug.LogFormat("You can now send messages to channel id: '{0}'", channel.Id);
```

```cpp fct_label="Cocos2d-x C++"
auto successCallback = [](NChannelPtr channel)
{
  CCLOG("You can now send messages to channel id: %s", channel->id.c_str());
};

string userId = "<user id>";
rtClient->joinChat(
    userId,
    NChannelType::DIRECT_MESSAGE,
    true,  // persistence
    false, // hidden
    successCallback
);
```

```js fct_label="Cocos2d-x JS"
const userId = "<user id>";
socket.send({ channel_join: {
    type: 2, // 1 = Room, 2 = Direct Message, 3 = Group
    target: userId,
    persistence: true,
    hidden: false
} }).then(function(response) {
      cc.log("You can now send messages to channel id:", response.channel.id);
    },
    function(error) {
      cc.error("join channel failed:", JSON.stringify(error));
    });
```

```cpp fct_label="C++"
auto successCallback = [](NChannelPtr channel)
{
  std::cout << "You can now send messages to channel id: " << channel->id << std::endl;
};

string userId = "<user id>";
rtClient->joinChat(
    userId,
    NChannelType::DIRECT_MESSAGE,
    true,  // persistence
    false, // hidden
    successCallback
);
```

```java fct_label="Java"
String userId = "<user id>";
boolean persistence = true;
boolean hidden = false;
Channel channel = socket.joinChat(userId, ChannelType.DIRECT_MESSAGE, persistence, hidden).get();
System.out.format("You can now send messages to channel id %s", channel.getId());
```

```swift fct_label="Swift"
// Requires Nakama 1.x
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

The `"<user id>"` variable must be an ID used to [send messages](#send-messages).

!!! Note
    A user can [block other users](social-friends.md#block-a-friend) to stop unwanted direct messages.

## List online users

Each user who joins a chat becomes a "presence" in the chat channel (unless they've joined as a "hidden" channel user). These presences keep information about which users are online.

A presence is made up of a unique session combined with a user ID. This makes it easy to distinguish between the same user connected from multiple devices in the chat channel.

The user who [joins a chat channel](#join-chat) receives an initial presence list of all other connected users in the chat channel. A callback can be used to receive presence changes from the server about users who joins and leaves. This makes it easy to maintain a list of online users and update it when changes occur.

!!! Summary
    A list of all online users is received when a user joins a chat channel you can combine it with an event handler which notifies when users join or leave. Together it becomes easy to maintain a list of online users.

```js fct_label="JavaScript"
var onlineUsers = [];
socket.onchannelpresence = (presences) => {
  // Remove all users who left.
  onlineUsers = onlineUsers.filter((user) => {
    return !presences.leave.includes(user);
  });
  // Add all users who joined.
  onlineUsers.concat(presences.join);
};

const roomname = "PizzaFans";
const response = await socket.send({ channel_join: {
    type: 1, // 1 = Room, 2 = Direct Message, 3 = Group
    target: roomname,
    persistence: true,
    hidden: false
} });

// Setup initial online user list.
onlineUsers.concat(response.channel.presences);
// Remove your own user from list.
onlineUsers = onlineUsers.filter((user) => {
  return user != channel.self;
});
```

```csharp fct_label=".NET"
var roomUsers = new List<IUserPresence>(10);
socket.ReceivedChannelPresence += presenceEvent =>
{
    foreach (var presence in presenceEvent.Leaves)
    {
        roomUsers.Remove(presence);
    }

    roomUsers.AddRange(presenceEvent.Joins);
    Console.WriteLine("Room users: [{0}]", string.Join(",\n  ", roomUsers));
};

var roomName = "PizzaFans";
var persistence = true;
var hidden = false;
var channel = await socket.JoinChatAsync(roomName, ChannelType.Room, persistence, hidden);
roomUsers.AddRange(channel.Presences);
```

```csharp fct_label="Unity"
var roomUsers = new List<IUserPresence>(10);
socket.ReceivedChannelPresence += presenceEvent =>
{
    foreach (var presence in presenceEvent.Leaves)
    {
        roomUsers.Remove(presence);
    }

    roomUsers.AddRange(presenceEvent.Joins);
    Debug.LogFormat("Room users: [{0}]", string.Join(",\n  ", roomUsers));
};

var roomname = "PizzaFans";
var persistence = true;
var hidden = false;
var channel = await socket.JoinChatAsync(roomname, ChannelType.Room, persistence, hidden);
connectedUsers.AddRange(channel.Presences);
```

```cpp fct_label="Cocos2d-x C++"
// add this to your class: std::vector<NUserPresence> onlineUsers;

rtListener->setChannelPresenceCallback([this](const NChannelPresenceEvent& event)
{
  // Remove all users who left.
  for (auto& left : event.leaves)
  {
    for (auto it = onlineUsers.begin(); it != onlineUsers.end(); ++it)
    {
      if (it->userId == left.userId)
      {
        onlineUsers.erase(it);
        break;
      }
    }
  }

  // Add all users who joined.
  onlineUsers.insert(onlineUsers.end(), event.joins.begin(), event.joins.end());
});

auto successCallback = [this](NChannelPtr channel)
{
  onlineUsers.reserve(channel->presences.size());

  // Setup initial online user list without self.
  for (auto& joined : channel->presences)
  {
    if (joined.userId != channel->self.userId)
    {
      onlineUsers.push_back(joined);
    }
  }
};

string roomname = "PizzaFans";
rtClient->joinChat(
    roomname,
    NChannelType::ROOM,
    true,  // persistence
    false, // hidden
    successCallback
);
```

```js fct_label="Cocos2d-x JS"
var onlineUsers = [];
socket.onchannelpresence = (presences) => {
  // Remove all users who left.
  onlineUsers = onlineUsers.filter((user) => {
    return !presences.leave.includes(user);
  });
  // Add all users who joined.
  onlineUsers.concat(presences.join);
};

const roomname = "PizzaFans";
socket.send({ channel_join: {
    type: 1, // 1 = Room, 2 = Direct Message, 3 = Group
    target: roomname,
    persistence: true,
    hidden: false
} }).then(function(response) {
      // Setup initial online user list.
      onlineUsers.concat(response.channel.presences);
      // Remove your own user from list.
      onlineUsers = onlineUsers.filter((user) => {
        return user != channel.self;
      });
    },
    function(error) {
      cc.error("join chat failed:", JSON.stringify(error));
    });
```

```cpp fct_label="C++"
// add this to your class: std::vector<NUserPresence> onlineUsers;

rtListener->setChannelPresenceCallback([this](const NChannelPresenceEvent& event)
{
  // Remove all users who left.
  for (auto& left : event.leaves)
  {
    for (auto it = onlineUsers.begin(); it != onlineUsers.end(); ++it)
    {
      if (it->userId == left.userId)
      {
        onlineUsers.erase(it);
        break;
      }
    }
  }

  // Add all users who joined.
  onlineUsers.insert(onlineUsers.end(), event.joins.begin(), event.joins.end());
});

auto successCallback = [this](NChannelPtr channel)
{
  onlineUsers.reserve(channel->presences.size());

  // Setup initial online user list without self.
  for (auto& joined : channel->presences)
  {
    if (joined.userId != channel->self.userId)
    {
      onlineUsers.push_back(joined);
    }
  }
};

string roomname = "PizzaFans";
rtClient->joinChat(
    roomname,
    NChannelType::ROOM,
    true,  // persistence
    false, // hidden
    successCallback
);
```

```java fct_label="Java"
final List<UserPresence> connectedUsers = new ArrayList<UserPresence>();
SocketListener listener = new AbstractSocketListener() {
  @Override
  public void onChannelPresence(final ChannelPresenceEvent presence) {
    connectedUsers.addAll(presence.getJoins());
    for (UserPresence presence : presence.getLeaves()) {
      for (int i = 0; i < connectedUsers.size(); i++) {
        if (connectedUsers.get(i).getUserId().equals(presence.getUserId())) {
          connectedUsers.remove(i);
        }
      }
    }
  }
};

String roomname = "PizzaFans";
boolean persistence = true;
boolean hidden = false;
Channel channel = socket.joinChat(roomname, ChannelType.ROOM, persistence, hidden);
connectedUsers.addAll(channel.getPresences());
```

```swift fct_label="Swift"
// Requires Nakama 1.x
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

```js fct_label="JavaScript"
var channelId = "<channel id>";
var data = { "some": "data" };
const messageAck = await socket.send({ channel_message_send: {
    channel_id: channelId,
    content: data
} });
```

```csharp fct_label=".NET"
var channelId = "<channel id>";
var content = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
var sendAck = await socket.WriteChatMessageAsync(channelId, content);
```

```csharp fct_label="Unity"
var channelId = "<channel id>";
var content = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
var sendAck = await socket.WriteChatMessageAsync(channelId, content);
```

```cpp fct_label="Cocos2d-x C++"
auto successCallback = [](const NChannelMessageAck& ack)
{
  CCLOG("message id: %s", ack.messageId.c_str());
};

string channelId = "<channel id>";
string data = "{ \"some\": \"data\" }";
rtClient->writeChatMessage(channelId, data, successCallback);
```

```js fct_label="Cocos2d-x JS"
var channelId = "<channel id>";
var data = { "some": "data" };
socket.send({ channel_message_send: {
    channel_id: channelId,
    content: data
} }).then(function(messageAck) {
      cc.log("message acknowledgement:", JSON.stringify(messageAck));
    },
    function(error) {
      cc.error("send channel message failed:", JSON.stringify(error));
    });
```

```cpp fct_label="C++"
auto successCallback = [](const NChannelMessageAck& ack)
{
  std::cout << "message id: " << ack.messageId << std::endl;
};

string channelId = "<channel id>";
string data = "{ \"some\": \"data\" }";
rtClient->writeChatMessage(channelId, data, successCallback);
```

```java fct_label="Java"
String channelId = "<channel id>";
final String content = "{\"message\":\"Hello world\"}";
ChannelMessageAck sendAck = socket.writeChatMessage(channelId, content).get();
```

```swift fct_label="Swift"
// Requires Nakama 1.x
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

```js fct_label="JavaScript"
var channelId = "<channel id>";
await socket.send({ channel_leave: {
  channel_id: channelId
} });
```

```csharp fct_label=".NET"
var channelId = "<channel id>";
await socket.LeaveChatAsync(channelId);
```

```csharp fct_label="Unity"
var channelId = "<channel id>";
await socket.LeaveChatAsync(channelId);
```

```cpp fct_label="Cocos2d-x C++"
string channelId = "<channel id>";
rtClient->leaveChat(channelId);
```

```js fct_label="Cocos2d-x JS"
var channelId = "<channel id>";
socket.send({ channel_leave: {
  channel_id: channelId
} });
```

```cpp fct_label="C++"
string channelId = "<channel id>";
rtClient->leaveChat(channelId);
```

```java fct_label="Java"
String channelId = "<channel id>";
socket.leaveChat(channelId).get();
```

```swift fct_label="Swift"
// Requires Nakama 1.x
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

Every chat conversation stores a history of messages (unless persistence is set false). The history also contains [event messages](#receive-messages) sent by the server to group chat channels. Each user can retrieve old messages for channels when they next connect online.

Messages can be listed in order of most recent to oldest and also in reverse (oldest to newest). Messages are returned in batches of up to 100 each with a cursor for when there are more messages.

!!! Tip
    A user does not have to join a chat channel to see chat history. This is useful to "peek" at old messages without the user appearing online in the chat.

```sh fct_label="cURL"
curl -X GET "http://127.0.0.1:7350/v2/channel?channel_id=<channelId>" \
  -H 'authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
const channelId = "<channel id>";
const result = await client.listChannelMessages(session, channelId, 10);
result.messages.forEach((message) => {
  console.log("Message has id %o and content %o", message.message_id, message.data);
});
console.log("Get the next page of messages with the cursor:", result.next_cursor);
```

```csharp fct_label=".NET"
var channelId = "<channel id>";
var result = await client.ListChannelMessagesAsync(session, channelId, 10, true);
foreach (var m in result.Messages)
{
    Console.WriteLine("Message id '{0}' content '{1}'", m.MessageId, m.Content);
}
```

```csharp fct_label="Unity"
var channelId = "<channel id>";
var result = await client.ListChannelMessagesAsync(session, channelId, 10, true);
foreach (var m in result.Messages)
{
    Debug.LogFormat("Message id '{0}' content '{1}'", m.MessageId, m.Content);
}
```

```cpp fct_label="Cocos2d-x C++"
auto successCallback = [](NChannelMessageListPtr list)
{
  for (auto& message : list->messages)
  {
    CCLOG("message content: %s", message.content.c_str());
  }
  CCLOG("Get the next page of messages with the cursor: %s", list->nextCursor.c_str());
};

string channelId = "<channel id>";
client->listChannelMessages(session,
    channelId,
    10,
    opt::nullopt,
    opt::nullopt,
    successCallback);
```

```js fct_label="Cocos2d-x JS"
const channelId = "<channel id>";
client.listChannelMessages(session, channelId, 10)
  .then(function(result) {
      result.messages.forEach((message) => {
        cc.log("Message content", message.data);
      });
      cc.log("Get the next page of messages with the cursor:", result.next_cursor);
    },
    function(error) {
      cc.error("matchmaker add failed:", JSON.stringify(error));
    });
```

```cpp fct_label="C++"
auto successCallback = [](NChannelMessageListPtr list)
{
  for (auto& message : list->messages)
  {
    std::cout << "message content: " << message.content << std::endl;
  }
  std::cout << "Get the next page of messages with the cursor: " << list->nextCursor << std::endl;
};

string channelId = "<channel id>";
client->listChannelMessages(session,
    channelId,
    10,
    opt::nullopt,
    opt::nullopt,
    successCallback);
```

```java fct_label="Java"
String channelId = "<channel id>";
ChannelMessageList messages = client.listChannelMessages(session, channelId, 10).get();
for (ChannelMessage message : messages.getMessagesList()) {
  System.out.format("Message content: %s", message.getContent());
}
```

```swift fct_label="Swift"
// Requires Nakama 1.x
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

```fct_label="REST"
GET /v2/channel?channel_id=<channelId>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

A cursor can be used to page after a batch of messages for the next set of results.

We recommend you only list the most recent 100 messages in your UI. A good user experience could be to fetch the next 100 older messages when the user scrolls to the bottom of your UI panel.

```sh fct_label="cURL"
curl -X GET "http://127.0.0.1:7350/v2/channel?channel_id=<channelId>&forward=true&limit=10&cursor=<cursor>" \
  -H 'Authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
var channelId = "<channel id>";
var forward = true;
var result = await client.listChannelMessages(session, channelId, 10, forward);
result.messages.forEach((message) => {
  console.log("Message has id %o and content %o", message.message_id, message.data);
});

if (result.next_cursor) {
  // Get the next 10 messages.
  var result = await client.listChannelMessages(session, channelId, 10, forward, result.next_cursor);
  result.messages.forEach((message) => {
    console.log("Message has id %o and content %o", message.message_id, message.data);
  });
}
```

```csharp fct_label=".NET"
var channelId = "<channel id>";
var result = await client.ListChannelMessagesAsync(session, channelId, 10, true);
foreach (var m in result.Messages)
{
    Console.WriteLine("Message id '{0}' content '{1}'", m.MessageId, m.Content);
}

if (!string.IsNullOrEmpty(result.NextCursor)) {
    // Get the next 10 messages.
    var result = await client.ListChannelMessagesAsync(session, channelId, 10, true, result.NextCursor);
    foreach (var m in messages)
    {
        Console.WriteLine("Message id '{0}' content '{1}'", m.MessageId, m.Content);
    }
};
```

```csharp fct_label="Unity"
var channelId = "roomname";
var result = await client.ListChannelMessagesAsync(session, channelId, 10, true);
foreach (var m in result.Messages)
{
    Debug.LogFormat("Message id '{0}' content '{1}'", m.MessageId, m.Content);
}

if (!string.IsNullOrEmpty(result.NextCursor)) {
    // Get the next 10 messages.
    var result = await client.ListChannelMessagesAsync(session, channelId, 10, true, result.NextCursor);
    foreach (var m in messages)
    {
        Debug.LogFormat("Message id '{0}' content '{1}'", m.MessageId, m.Content);
    }
};
```

```cpp fct_label="Cocos2d-x C++"
void YourClass::listChannelMessages(const std::string& cursor)
{
  auto successCallback = [this](NChannelMessageListPtr list)
  {
    for (auto& message : list->messages)
    {
      CCLOG("message content: %s", message.content.c_str());
    }

    if (!list->nextCursor.empty())
    {
      listChannelMessages(list->nextCursor);
    }
  };

  string channelId = "<channel id>";
  client->listChannelMessages(session,
      channelId,
      10,
      cursor,
      true, // forward
      successCallback);
}

listChannelMessages("");
```

```js fct_label="Cocos2d-x JS"
ver listChannelMessages(cursor) {
  var channelId = "<channel id>";
  var forward = true;
  client.listChannelMessages(session, channelId, 10, forward, cursor)
    .then(function(result) {
        result.messages.forEach((message) => {
          cc.log("Message content", message.data);
        });

        if (result.next_cursor) {
          // Get the next 10 messages.
          listChannelMessages(result.next_cursor);
        }
      },
      function(error) {
        cc.error("list channel messages failed:", JSON.stringify(error));
      });
}

listChannelMessages();
```

```cpp fct_label="C++"
void YourClass::listChannelMessages(const std::string& cursor)
{
  auto successCallback = [this](NChannelMessageListPtr list)
  {
    for (auto& message : list->messages)
    {
      std::cout << "message content: " << message.content << std::endl;
    }

    if (!list->nextCursor.empty())
    {
      listChannelMessages(list->nextCursor);
    }
  };

  string channelId = "<channel id>";
  client->listChannelMessages(session,
      channelId,
      10,
      cursor,
      true, // forward
      successCallback);
}

listChannelMessages("");
```

```java fct_label="Java"
String channelId = "<channel id>";
ChannelMessageList messages = client.listChannelMessages(session, channelId, 10).get();
if (messages.getNextCursor() != null) {
  messages = client.listChannelMessages(session, channelId, 10, messages.getNextCursor()).get();
  for (ChannelMessage message : messages.getMessagesList()) {
    System.out.format("Message content: %s", message.getContent());
  }
}
```

```swift fct_label="Swift"
// Requires Nakama 1.x
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

```fct_label="REST"
GET /v2/channel?channel_id=<channel id>&forward=true&limit=10&cursor=<cursor>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```
