# Friends

Friends are a great way to build a social community. Users can add other users to their list of friends, see who is online or when they were last online, chat together in realtime, and interact together in gameplay or collaboration.

!!! Summary "Fun fact"
    Nakama is a common Japanese word that directly translates to friend or comrade. Some believe the word means "people who are considered closer than family", though that is not a part of the official definition. We feel it expresses the kind of social communities we want developers to build into their games and apps!

Each user builds up a list of friends by who they know already from their social networks, friend requests they send, requests they receive, and who the server recommends they should know. This information is stored in a social graph within the system as a powerful way to interact with other users. Much like how Twitter or Facebook work.

Any social community must be maintained carefully to prevent spam or abuse. To help with this problem it's also possible for a user to block users they no longer want to communicate with and for the server to ban a user via server-side code to completely disable an account.

## Add friends

A user can add one or more friends by that user's ID or handle. The user added will not be marked as a friend in the list until they've confirmed the friend request. The user who receives the request can confirm it by adding the user back.

!!! Hint
    A user who registers or links their account with Facebook or another social network will have friends from that network be added automatically into their friend list.

When a friend request is sent or the user is added an in-app notification will be sent. See the [in-app notification](in-app-notifications.md#receive-notifications) section for more info.

```csharp fct_label="Unity"
byte[] userId = ...; // some user ID
var message = NFriendAddMessage.Default(userId);
client.Send(message, (bool done) => {
  Debug.Log("Friend added or request sent.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

When both users have added eachother as friends it's easy to initiate realtime chat in a 1-on-1 channel. See the [realtime chat](realtime-chat.md) section for more info.

## List friends

You can list all of a user's friends, blocked users, friend requests received (invited), and invites they've sent. These statuses are returned together as part of the friend list which makes it easy to display in a UI.

```csharp fct_label="Unity"
var message = NFriendsListMessage.Default();
client.Send(message, (INResultSet<INFriend> list) => {
  foreach (var f in list.Results) {
    var id = Encoding.UTF8.GetString(f.Id); // convert byte[].
    // f.State is one of: Friend, Invite, Invited, Blocked.
    Debug.LogFormat("User {0} has state {1}.", id, f.State);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

## Remove friends

A user can remove a friend, reject a received invite, cancel a friend request sent, or unblock a user. Similar to how Friend Add works we reuse Friend Remove to cancel or undo whatever friend state is current with another user.

!!! Note
    If a user is unblocked they are removed from the friend list entirely. To re-add them each user must add the other again.

```csharp fct_label="Unity"
byte[] userId = ...; // some user ID
var message = NFriendRemoveMessage.Default(userId);
client.Send(message, (bool done) => {
  var idString = Encoding.UTF8.GetString(userId); // convert byte[].
  Debug.Log("User {0} has been removed.", idString);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

## Block a friend

You can stop a user from using 1-on-1 chat or other social features with a user if you block them. The user who wants to block should send the message. They can be unblocked later with a [Friend Remove](#remove-friends) message.

A user who has been blocked will not know which users have blocked them. That user can continue to add friends and interact with other users.

```csharp fct_label="Unity"
byte[] userId = ...; // some user ID
var message = NFriendBlockMessage.Default(userId);
client.Send(message, (bool done) => {
  var idString = Encoding.UTF8.GetString(id); // convert byte[].
  Debug.Log("User {0} has been blocked.", idString);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

### Ban a user

A user can be banned with server-side code. This will prevent the user from being able to connect to the server and interact at all.

This is best used by a moderator system within your community. You could assign particular users the capabilities to send an RPC to permanently ban a user or you may decide to ban users via your liveops support team.

See the [runtime code basics](runtime-code-basics.md) on how to write server-side code.

```lua
local nk = require("nakama")

-- you can use both IDs and handles to ban.
local bad_users = {"someuserid", "anotheruserid", "userhandle"}
local success, err = pcall(nk.users_ban, bad_users)
if (not success) then
  nk.logger_error(("Ban failed: %q"):format(err))
end
```
