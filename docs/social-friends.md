# Friends

Friends are a great way to build a social community. Users can add other users to their list of friends, see who is online or when they were last online, chat together in realtime, and interact together in gameplay or collaboration.

!!! Summary "Fun fact"
    Nakama is a common Japanese word that directly translates to friend or comrade. Some believe the word means "people who are considered closer than family", though that is not a part of the official definition. We feel it expresses the kind of social communities we want developers to build into their games and apps!

Each user builds up a list of friends by who they know already from their social networks, friend requests they send, requests they receive, and who the server recommends they should know. This information is stored in a social graph within the system as a powerful way to interact with other users. Much like how Twitter or Facebook work.

Any social community must be maintained carefully to prevent spam or abuse. To help with this problem it's also possible for a user to block users they no longer want to communicate with and for the server to ban a user via server-side code to completely disable an account.

## Add friends

A user can add one or more friends by that user's ID or username. The user added will not be marked as a friend in the list until they've confirmed the friend request. The user who receives the request can confirm it by adding the user back.

!!! Hint
    A user who registers or links their account with Facebook or another social network will have friends from that network be added automatically into their friend list.

When a friend request is sent or the user is added an in-app notification will be sent. See the [in-app notification](social-in-app-notifications.md#receive-notifications) section for more info.

```sh fct_label="cURL"
curl -X POST \
  'http://127.0.0.1:7350/v2/friend?ids=user-id1&ids=user-id2&usernames=username1' \
  -H 'Authorization: <session token>'
```

```csharp fct_label=".NET"
var ids = new[] {"user-id1", "user-id2"};
var usernames = new[] {"username1"};
await client.AddFriendsAsync(session, ids, usernames);
```

```csharp fct_label="Unity"
// Requires Nakama 1.x
string userId = ...; // some user ID
var message = NFriendAddMessage.Default(userId);
client.Send(message, (bool done) => {
  Debug.Log("Friend added or request sent.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x
let userID = ... // some user ID
var message = FriendAddMessage()
message.userIds.append(userID)
client.send(message: message).catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
POST /v2/friend?ids=user-id1&ids=user-id2&usernames=username1
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

When both users have added eachother as friends it's easy to initiate realtime chat in a 1-on-1 channel. See the [realtime chat](social-realtime-chat.md) section for more info.

## List friends

You can list all of a user's friends, blocked users, friend requests received (invited), and invites they've sent. These statuses are returned together as part of the friend list which makes it easy to display in a UI.

```sh fct_label="cURL"
curl -X GET \
  http://127.0.0.1:7350/v2/friend \
  -H 'Accept: application/json' \
  -H 'Content-Type: application/json' \
  -H 'Authorization: <session token>'
```

```js fct_label="JavaScript"
const friends = await client.listFriends(session);
console.info("Successfully retrieved friend list:", friends);
```

```csharp fct_label=".NET"
var result = await client.ListFriendsAsync(session);
foreach (var f in result.Friends)
{
  System.Console.WriteLine("Friend '{0}' state '{1}'", f.User.Username, f.State);
}
```

```csharp fct_label="Unity"
// Requires Nakama 1.x
var message = NFriendsListMessage.Default();
client.Send(message, (INResultSet<INFriend> list) => {
  foreach (var f in list.Results) {
    // f.State is one of: Friend, Invite, Invited, Blocked.
    Debug.LogFormat("User {0} has state {1}.", f.Id, f.State);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x
var message = FriendListMessage()
client.send(message: message).then { friends in
  for friend in friends {
    // friend.State is one of: Friend, Invite, Invited, Blocked.
    NSLog("User %@ has state %@.", friend.id, friend.state.rawValue)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
GET /v2/friend
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

## Remove friends

A user can remove a friend, reject a received invite, cancel a friend request sent, or unblock a user. Similar to how Friend Add works we reuse Friend Remove to cancel or undo whatever friend state is current with another user.

!!! Note
    If a user is unblocked they are removed from the friend list entirely. To re-add them each user must add the other again.

```sh fct_label="cURL"
curl -X DELETE \
  'http://127.0.0.1:7350/v2/friend?ids=user-id1&ids=user-id2&usernames=username1' \
  -H 'Authorization: <session token>'
```

```csharp fct_label=".NET"
var ids = new[] {"user-id1", "user-id2"};
var usernames = new[] {"username1"};
await client.RemoveFriendsAsync(session, ids, usernames);
```

```csharp fct_label="Unity"
// Requires Nakama 1.x
string userId = ...; // some user ID
var message = NFriendRemoveMessage.Default(userId);
client.Send(message, (bool done) => {
  Debug.Log("User {0} has been removed.", userId);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x
let userID = ... // some user ID
var message = FriendRemoveMessage()
message.userIds.append(userID)
client.send(message: message).then { _ in
  NSLog("User %@ has been removed", userID)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
DELETE /v2/friend?ids=user-id1&ids=user-id2&usernames=username1
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

## Block a friend

You can stop a user from using 1-on-1 chat or other social features with a user if you block them. The user who wants to block should send the message. They can be unblocked later with a [Friend Remove](#remove-friends) message.

A user who has been blocked will not know which users have blocked them. That user can continue to add friends and interact with other users.

```sh fct_label="cURL"
curl -X POST \
  'http://127.0.0.1:7350/v2/friend/block?ids=user-id1&ids=user-id2&usernames=username1' \
  -H 'Authorization: <session token>'
```

```csharp fct_label=".NET"
var ids = new[] {"user-id1", "user-id2"};
var usernames = new[] {"username1"};
await client.BlockFriendsAsync(session, ids, usernames);
```

```csharp fct_label="Unity"
// Requires Nakama 1.x
string userId = ...; // some user ID
var message = NFriendBlockMessage.Default(userId);
client.Send(message, (bool done) => {
  Debug.Log("User {0} has been blocked.", userId);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x
let userID = ... // some user ID
var message = FriendBlockMessage()
message.userIds.append(userID)
client.send(message: message).then { _ in
  NSLog("User %@ has been blocked", userID)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
POST /v2/friend/block?ids=user-id1&ids=user-id2&usernames=username1
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

<!--

### Ban a user

A user can be banned with server-side code. This will prevent the user from being able to connect to the server and interact at all.

This is best used by a moderator system within your community. You could assign particular users the capabilities to send an RPC to permanently ban a user or you may decide to ban users via your liveops support team.

See the [runtime code basics](runtime-code-basics.md) on how to write server-side code.

```lua
local nk = require("nakama")

-- you can use both IDs and username to ban.
local bad_users = {"someuserid", "anotheruserid", "username"}
local success, err = pcall(nk.users_ban, bad_users)
if (not success) then
  nk.logger_error(("Ban failed: %q"):format(err))
end
```

-->
