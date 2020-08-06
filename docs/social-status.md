# Status

Nakama users can set a status message when they connect and update it while they're online.

Users can follow each other to be notified of status changes. This is ideal to know when their friends are online and what they're up to.

The status is set for each connection, and is erased when the user disconnects. If the user is connected from multiple devices each one is allowed to have a different status.

## Set a status

By default users have no status when they first connect, and will not appear online to their followers. To appear online the user must set a status.

```js tab="JavaScript"
socket.updateStatus("Hello everyone!");
```

```csharp tab=".NET"
await socket.UpdateStatusAsync("Hello everyone!");
```

```csharp tab="Unity"
await socket.UpdateStatusAsync("Hello everyone!");
```

```cpp tab="Cocos2d-x C++"
rtClient->updateStatus("Hello everyone!");
```

```js tab="Cocos2d-x JS"
socket.updateStatus("Hello everyone!");
```

```cpp tab="C++"
rtClient->updateStatus("Hello everyone!");
```

```java tab="Java"
socket.updateStatus("Hello everyone!").get();
```

```gdscript tab="Godot"
var update : NakamaAsyncResult = yield(socket.update_status_async(JSON.print({"status": "happy"})), "completed")
if update.is_exception():
	print("An error occured: %s" % update)
	return
print("Status updated")
```

The status can be set and updated as often as needed with this operation.

!!! Tip
    A status can be as simple as a text message from the user to their followers or it can be a structured JSON string with complex information such as the [realtime multiplayer match ID](gameplay-multiplayer-realtime.md) the user is currently in - so their friends can jump in and join them!

## Appear offline

If the user needs to appear offline or "invisible" they can do so by erasing their status. Their followers will receive the same status update as they would if the user disconnects.

```js tab="JavaScript"
socket.updateStatus();
```

```csharp tab=".NET"
await socket.UpdateStatusAsync(null);
```

```csharp tab="Unity"
await socket.UpdateStatusAsync(null);
```

```cpp tab="Cocos2d-x C++"
rtClient->updateStatus("");
```

```js tab="Cocos2d-x JS"
socket.updateStatus();
```

```cpp tab="C++"
rtClient->updateStatus("");
```

```java tab="Java"
socket.updateStatus(null).get();
```

```gdscript tab="Godot"
var leave : NakamaAsyncResult = yield(socket.update_status_async(""), "completed")
if leave.is_exception():
	print("An error occured: %s" % leave)
	return
print("Status updated")
```

## Receive status updates

When a user updates their status all of their followers receive an event that contains both the old status and the new one. Clients register an event handler to be called when receiving a status update.

```js tab="JavaScript"
socket.onstatuspresence = (statuspresence) => {
  statuspresence.leaves.forEach((leave) => {
    console.log("User %o no longer has status %o", leave.user_id, leave.status);
  });
  statuspresence.joins.forEach((join) => {
    console.log("User %o now has status %o", join.user_id, join.status);
  });
};
```

```csharp tab=".NET"
socket.ReceivedStatusPresence += presenceEvent =>
{
    Console.WriteLine(presenceEvent);
    foreach (var joined in presenceEvent.Joins)
    {
        Console.WriteLine("User id '{0}' status joined '{1}'", joined.UserId, joined.Status);
    }
    foreach (var left in presenceEvent.Leaves)
    {
        Console.WriteLine("User id '{0}' status left '{1}'", left.UserId, left.Status);
    }
};
```

```csharp tab="Unity"
socket.ReceivedStatusPresence += presenceEvent =>
{
    Debug.Log(presenceEvent);
    foreach (var joined in presenceEvent.Joins)
    {
        Debug.LogFormat("User id '{0}' status joined '{1}'", joined.UserId, joined.Status);
    }
    foreach (var left in presenceEvent.Leaves)
    {
        Debug.LogFormat("User id '{0}' status left '{1}'", left.UserId, left.Status);
    }
};
```

```cpp tab="Cocos2d-x C++"
rtListener->setStatusPresenceCallback([](const NStatusPresenceEvent& event)
{
  for (auto& presence : event.leaves)
  {
    CCLOG("User %s no longer has status %s", presence.username.c_str(), presence.status.c_str());
  }

  for (auto& presence : event.joins)
  {
    CCLOG("User %s now has status %s", presence.username.c_str(), presence.status.c_str());
  }
});
```

```js tab="Cocos2d-x JS"
socket.onstatuspresence = (statuspresence) => {
  statuspresence.leaves.forEach((leave) => {
    cc.log("User", leave.user_id, "no longer has status", leave.status);
  });
  statuspresence.joins.forEach((join) => {
    cc.log("User", join.user_id, "now has status", join.status);
  });
};
```

```cpp tab="C++"
rtListener->setStatusPresenceCallback([](const NStatusPresenceEvent& event)
{
  for (auto& presence : event.leaves)
  {
    std::cout << "User " << presence.username << " no longer has status " << presence.status << std::endl;
  }

  for (auto& presence : event.joins)
  {
    std::cout << "User " << presence.username << " now has status " << presence.status << std::endl;
  }
});
```

```java tab="Java"
SocketListener listener = new AbstractSocketListener() {
  @Override
  public void onStatusPresence(final StatusPresenceEvent presence) {
    for (UserPresence userPresence : presence.getJoins()) {
      System.out.println("User ID: " + userPresence.getUserId() + " Username: " + userPresence.getUsername() + " Status: " + userPresence.getStatus());
    }

    for (UserPresence userPresence : presence.getLeaves()) {
      System.out.println("User ID: " + userPresence.getUserId() + " Username: " + userPresence.getUsername() + " Status: " + userPresence.getStatus());
    }
  }
};
```

```gdscript tab="Godot"
func _ready():
	# First, setup the socket as explained in the authentication section.
	socket.connect("received_status_presence", self, "_on_status_presence")

func _on_status_presence(p_presence : NakamaRTAPI.StatusPresenceEvent):
	print(p_presence)
	for j in p_presence.joins:
		print("%s joined with status: %s" % [j.user_id, j.status])
	for j in p_presence.leaves:
		print("%s left with status: %s" % [j.user_id, j.status])
```

If a user is disconnecs or appears offline they will leave their previous status but there will be no corresponding new status.

## Follow users

Users only receive status updates from those they follow. Users can follow anyone they're interested in, but it's common to follow a list of friends to see when they're online and what they're up to.

When following a set of users the operation will immediately return the status of those that are online and have set a visible status.

```js tab="JavaScript"
var status = await socket.followUsers(["<user id>"]);
status.presences.forEach((presence) => {
  console.log("User %o has status %o", presence.user_id, presence.status);
});
```

```csharp tab=".NET"
await socket.FollowUsersAsync(new[] { "<user id>" });
```

```csharp tab="Unity"
await socket.FollowUsersAsync(new[] { "<user id>" });
```

```cpp tab="Cocos2d-x C++"
auto successCallback = [](const NStatus& status)
{
  for (auto& presence : status.presences)
  {
    CCLOG("User %s has status %s", presence.username.c_str(), presence.status.c_str());
  }
};

rtClient->followUsers({ "<user id>" }, successCallback);
```

```js tab="Cocos2d-x JS"
socket.followUsers(["<user id>"])
  .then(function(status) {
      status.presences.forEach((presence) => {
        cc.log("User", presence.user_id, "has status", presence.status);
      });
    },
    function(error) {
      cc.error("follow status failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = [](const NStatus& status)
{
  for (auto& presence : status.presences)
  {
    std::cout << "User " << presence.username << " has status " << presence.status << std::endl;
  }
};

rtClient->followUsers({ "<user id>" }, successCallback);
```

```java tab="Java"
socket.followUsers("<user id>").get();
```

```gdscript tab="Godot"
var user_ids = ["<user-id1>", "<user-id2>"]
var status : NakamaRTAPI.Status = yield(socket.follow_users_async(user_ids), "completed")
if status.is_exception():
	print("An error occured: %s" % status)
	return
print(status)
```

!!! Note
    Following a user is only active with the current session. When the user disconnects they automatically unfollow anyone they may have followed while connected.

## Unfollow users

Unfollowing a set of users immediately stops the user from receiving any further status updates from them.

```js tab="JavaScript"
socket.unfollowUsers(["<user id>"]);
```

```csharp tab=".NET"
await socket.UnfollowUsersAsync(new[] { "<user id>" });
```

```csharp tab="Unity"
await socket.UnfollowUsersAsync(new[] { "<user id>" });
```

```cpp tab="Cocos2d-x C++"
rtClient->unfollowUsers({ "<user id>" });
```

```js tab="Cocos2d-x JS"
socket.unfollowUsers(["<user id>"]);
```

```cpp tab="C++"
rtClient->unfollowUsers({ "<user id>" });
```

```java tab="Java"
socket.unfollowUsers("<user id>").get();
```

```gdscript tab="Godot"
var user_ids = ["<user-id1>", "<user-id2>"]
var status : NakamaAsyncResult = yield(socket.unfollow_users_async(user_ids), "completed")
if status.is_exception():
	print("An error occured: %s" % status)
	return
print(status)
```
