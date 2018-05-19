# In-app Notifications

In-app notifications make it easy to broadcast a message to one or more users. They are great for sending announcements, alerts, or notices of in-game rewards and gifts.

A notification can be stored until read when the app is next opened or it can be pushed so only an active connected user will see it. You can also use notifications to trigger custom actions within your game and change client behavior.

These notifications are viewed within the app which makes them a great companion to push notifications viewed outside the app.

## Send notifications

You can send a notification to one or more users with server-side Lua code. It can be sent to any user in the game, no need to be a friend to be able to exchange messages. A number of notifications are also sent by the server implicitly on certain events. Each notification has a code which is used to categorize it.

!!! Note
    The code you choose for your notifications must start at `0` and upwards. See [below](#notification-codes) for reserved message codes.

A notification has a content object which will be encoded as JSON.

Notifications can be sent as persistent or not. A non-persistent message will only be received by a client which is currently connected to the server (i.e. a user who is online). If you want to make sure a notification is never lost before it's read it should be marked as persistent when sent.

```lua
local nk = require("nakama")

local user_id = "user id to send to"
local sender_id = nil   -- "nil" for server sent.
local content = {
  item_id = "192308394345345",
  item_icon = "storm_bringer_sword.png"
}
local subject = "You earned a secret item!"
local code = 1
local persistent = true

nk.notification_send(user_id, subject, content, code, sender_id, persistent)
```

## Receive notifications

A callback can be registered for notifications received when a client is connected. The handler will be called whenever a notification is received as long as the client socket remains connected. When multiple messages are returned (batched for performance) the handler will be called once for each notification.

```js fct_label="JavaScript"
socket.onnotification = (notification) => {
  console.log("Received code %o and subject %o", notification.code, notification.subject);
  console.log("Received id %o and content %o", notification.id, notification.content);
}
```

```csharp fct_label="Unity"
// Updated example TBD
```

```swift fct_label="Swift"
// Requires Nakama 1.x
client.onNotification = { notification in
  NSLog("Received code %d and subject %@", notification.code, notification.subject)
  NSLog("Received id %d and content %@", notification.id, notification.content)
}
```

## List notifications

You can list notifications which were received when the user was offline. These notifications are ones which were marked "persistent" when sent. It depends on your game or app but we suggest you retrieve notifications after a client reconnects. You can then display a UI within your game or app with the list.

```sh fct_label="cURL"
curl http://127.0.0.1:7350/v2/notification?limit=10 \
  -H 'Authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
const result = await client.listNotifications(session, 10);
result.notifications.forEach(notification => {
  console.info("Notification code %o and subject %o.", notification.code, notification.subject);
});
console.info("Fetch more results with cursor:", result.cacheable_cursor);
```

```csharp fct_label=".NET"
var result = await client.ListNotificationsAsync(session, 10);
foreach (var n in result.Notifications)
{
  System.Console.WriteLine("Subject '{0}' content '{1}'", n.Subject, n.Content);
}
```

```csharp fct_label="Unity"
var result = await client.ListNotificationsAsync(session, 10);
foreach (var n in result.Notifications)
{
  Debug.LogFormat("Subject '{0}' content '{1}'", n.Subject, n.Content);
}
```

```swift fct_label="Swift"
// Requires Nakama 1.x
var message = NotificationListMessage(limit: 100)
client.send(message: message).then { notifications in
  for notification in notifications {
    NSLog("Notice code %d and subject %@.", notification.code, notification.subject)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
GET /v2/notification?limit=10
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

A list of notifications can be retrieved in batches of up to 100 at a time. To retrieve all messages you should accumulate them with the cacheable cursor. You can keep this cursor on the client and use it when the user reconnects to catch up on any notifications they may have missed while offline.

!!! Hint
    You usually only want to list 100 notifications at a time otherwise you might cause user fatigue. A better option could be to have the UI fetch the next 100 notifications when the user scrolls to the bottom of your UI panel.

```sh fct_label="cURL"
curl http://127.0.0.1:7350/v2/notification?limit=10&cursor=<cursor> \
  -H 'Authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
var allNotifications = [];

var accumulateNotifications = (cursor) => {
  var result = await client.listNotifications(session, 10, cursor);
  if (result.notifications.length == 0) {
    return;
  }
  allNotifications.concat(result.notifications.notifications);
  accumulateNotifications(result.cacheable_cursor);
}
accumulateNotifications("");
```

```csharp fct_label=".NET"
var result = await client.ListNotificationsAsync(session, 10);
if (result.CacheableCursor != null)
{
  result = await client.ListNotificationsAsync(session, 10, result.CacheableCursor);
  foreach (var n in result.Notifications)
  {
    System.Console.WriteLine("Subject '{0}' content '{1}'", n.Subject, n.Content);
  }
}
```

```csharp fct_label="Unity"
var result = await client.ListNotificationsAsync(session, 10);
if (result.CacheableCursor != null)
{
  result = await client.ListNotificationsAsync(session, 10, result.CacheableCursor);
  foreach (var n in result.Notifications)
  {
    Debug.LogFormat("Subject '{0}' content '{1}'", n.Subject, n.Content);
  }
}
```

```swift fct_label="Swift"
// Requires Nakama 1.x
var allNotifications : [Notification] = []

let accumulateNotifications = { cursor in
  var message = NotificationListMessage(limit: 100)
  message.cursor = cursor
  client.send(message).then { notifications in
    if notifications.count == 0 {
      return
    }

    allNotifications.append(notifications)
    accumulateNotifications(notifications.cursor) // recursive async call
  }.catch { err in
    NSLog("Error %@ : %@", err, (err as! NakamaError).message)
  }
}

var message = NotificationListMessage(limit: 100)
client.send(message: message).then { notifications in
  allNotifications.append(notifications)
  accumulateNotifications(notifications.cursor)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
GET /v2/notification?limit=10&cursor=<cursor>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

It can be useful to retrieve only notifications which have been added since the list was last retrieved by a client. This can be done with the cacheable cursor returned with each list message. Sending the cursor through a new list operation will retrieve only notifications newer than those seen.

The cacheable cursor marks the position of the most recent notification retrieved. We recommend you store the cacheable cursor in device storage and use it when the client makes its next request for recent notifications.

```sh fct_label="cURL"
curl http://127.0.0.1:7350/v2/notification?limit=10&cursor=<cacheableCursor> \
  -H 'Authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
const cacheableCursor = "<cached cursor>";
const result = await client.listNotifications(session, 10, cacheableCursor);
result.notifications.forEach(notification => {
  console.info("Notification code %o and subject %o.", notification.code, notification.subject);
});
```

```csharp fct_label=".NET"
const string cacheableCursor = "<cached cursor>";
var result = await client.ListNotificationsAsync(session, 10, cacheableCursor);
foreach (var n in result.Notifications)
{
  System.Console.WriteLine("Subject '{0}' content '{1}'", n.Subject, n.Content);
}
```

```csharp fct_label="Unity"
const string cacheableCursor = "<cached cursor>";
var result = await client.ListNotificationsAsync(session, 10, cacheableCursor);
foreach (var n in result.Notifications)
{
  Debug.LogFormat("Subject '{0}' content '{1}'", n.Subject, n.Content);
}
```

```swift fct_label="Swift"
// Requires Nakama 1.x
var resumableCursor = "<cached cursor>";
var message = NotificationListMessage(limit: 100)
message.cursor = resumableCursor
client.send(message: message).then { notifications in
  resumableCursor = notifications.cursor
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
GET /v2/notification?limit=10&cursor=<cacheableCursor>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

## Delete notifications

You can delete one or more notifications from the client. This is useful to purge notifications which have been read or consumed by the user and prevent a build up of old messages. When a notification is deleted, all record of it is removed from the system and it cannot be restored.

```sh fct_label="cURL"
curl -X DELETE \
  --url "http://127.0.0.1:7350/v2/notification?ids=<notification id>&ids=<notification id>" \
  -H 'Authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
const notificationIds = ["<notification id>"];
await client.deleteNotifications(session, notificationIds);
```

```csharp fct_label=".NET"
var notificationIds = new[] {"<notification id>"};
await client.DeleteNotificationsAsync(session, notificationIds);
```

```csharp fct_label="Unity"
var notificationIds = new[] {"<notification id>"};
await client.DeleteNotificationsAsync(session, notificationIds);
```

```swift fct_label="Swift"
// Requires Nakama 1.x
var message = NotificationRemoveMessage()
message.notificationIds.append(...) // Add notification from your internal list
client.send(message: message).then {
  NSLog("Notifications were removed.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
DELETE /v2/notification?ids=<notification id>&ids=<notification id>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

## Notification codes

The server reserves all negative integer codes for messages sent implicitly on certain events. You can define your own notification codes by simply using values greater than or equal to 0.

The code is useful to decide how to display the notification in your UI.

| Code | Purpose |
| ---- | ------- |
|   -1 | User X wants to chat. |
|   -2 | User X wants to add you as a friend. |
|   -3 | User X accepted your friend invite. |
|   -4 | You've been accepted to X group. |
|   -5 | User X wants to join your group. |
|   -6 | Your friend X has just joined the game. |
