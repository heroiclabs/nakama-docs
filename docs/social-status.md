# Status

Nakama users can set a status message when they connect and update it while they're online.

Users can follow each other to be notified of status changes. This is ideal to know when their friends are online and what they're up to.

The status is set for each connection, and is erased when the user disconnects. If the user is connected from multiple devices each one is allowed to have a different status.

## Set a status

By default users have no status when they first connect, and will not appear online to their followers. To appear online the user must set a status.

```js fct_label="JavaScript"
socket.send({ status_update: { status: "Hello everyone!" } });
```

```csharp fct_label=".NET"
await socket.UpdateStatusAsync("Hello everyone!");
```

```csharp fct_label="Unity"
await socket.UpdateStatusAsync("Hello everyone!");
```

The status can be set and updated as often as needed with this operation.

!!! Tip
    A status can be as simple as a text message from the user to their followers or it can be a structured JSON string with complex information such as the [realtime multiplayer match ID](gameplay-multiplayer-realtime.md) the user is currently in - so their friends can jump in and join them!

## Appear offline

If the user needs to appear offline or "invisible" they can do so by erasing their status. Their followers will receive the same status update as they would if the user disconnects.

```js fct_label="JavaScript"
socket.send({ status_update: {} });
```

```csharp fct_label=".NET"
await socket.UpdateStatusAsync(null);
```

```csharp fct_label="Unity"
await socket.UpdateStatusAsync(null);
```

## Receive status updates

When a user updates their status all of their followers receive an event that contains both the old status and the new one. Clients register an event handler to be called when receiving a status update.

```js fct_label="JavaScript"
socket.onstatuspresence = (statuspresence) => {
  statuspresence.leaves.forEach((leave) => {
    console.log("User %o no longer has status %o", leave.user_id, leave.status);
  });
  statuspresence.joins.forEach((join) => {
    console.log("User %o now has status %o", join.user_id, join.status);
  });
};
```

```csharp fct_label=".NET"
socket.OnStatusPresence += (_, presence) =>
{
  Console.WriteLine("Presence {0}", presence);
  foreach (var join in presence.Joins)
  {
    Console.WriteLine("User id '{0}' status update '{1}'", join.UserId, join.Status);
  }

  foreach (var leave in presence.Leaves)
  {
    Console.WriteLine("User id '{0}' status gone '{1}'", leave.UserId, leave.Status);
  }
};
```

```csharp fct_label="Unity"
socket.OnStatusPresence += (_, presence) =>
{
  Debug.LogFormat("Presence {0}", presence);
  foreach (var join in presence.Joins)
  {
    Debug.LogFormat("User id '{0}' status update '{1}'", join.UserId, join.Status);
  }

  foreach (var leave in presence.Leaves)
  {
    Debug.LogFormat("User id '{0}' status gone '{1}'", leave.UserId, leave.Status);
  }
};
```

If a user is disconnecs or appears offline they will leave their previous status but there will be no corresponding new status.

## Follow users

Users only receive status updates from those they follow. Users can follow anyone they're interested in, but it's common to follow a list of friends to see when they're online and what they're up to.

When following a set of users the operation will immediately return the status of those that are online and have set a visible status.

```js fct_label="JavaScript"
var status = await socket.send({ status_follow: { user_ids: ["<user id>"] } });
status.presences.forEach((presence) => {
  console.log("User %o has status %o", presence.user_id, presence.status);
});
```

```csharp fct_label=".NET"
await socket.FollowUsersAsync(new[] { "<user id>" });
```

```csharp fct_label="Unity"
await socket.FollowUsersAsync(new[] { "<user id>" });
```

!!! Note
    Following a user is only active with the current session. When the user disconnects they automatically unfollow anyone they may have followed while connected.

## Unfollow users

Unfollowing a set of users immediately stops the user from receiving any further status updates from them.

```js fct_label="JavaScript"
socket.send({ status_unfollow: { user_ids: ["<user id>"] } });
```

```csharp fct_label=".NET"
await socket.UnfollowUsersAsync(new[] { "<user id>" });
```

```csharp fct_label="Unity"
await socket.UnfollowUsersAsync(new[] { "<user id>" });
```
