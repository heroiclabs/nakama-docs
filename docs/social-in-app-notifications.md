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

```js fct_label="Javascript"
socket.onnotification = function(notification) {
  console.log("Received code %o and subject %o", notification.code, notification.subject);
  console.log("Received id %o and content %o", notification.id, notification.content);
}
```

```csharp fct_label="Unity"
client.OnNotification = (INNotification n) => {
  Debug.LogFormat("Received code '{0}' and subject '{1}'.", n.Code, n.Subject);
  Debug.LogFormat("Received id '{0}' and content '{1}'.", n.Id, n.Content);
};
```

```swift fct_label="Swift"
client.onNotification = { notification in
  NSLog("Received code %d and subject %@", notification.code, notification.subject)
  NSLog("Received id %d and content %@", notification.id, notification.content)
}
```

## List notifications

You can list notifications which were received when the user was offline. These notifications are ones which were marked "persistent" when sent. It depends on your game or app but we suggest you retrieve notifications after a client reconnects. You can then display a UI within your game or app with the list.

```sh fct_label="cURL"
curl -X GET \
  --url http://127.0.0.1:7350/v2/notification \
  --header 'authorization: Bearer <session token>' \
  --header 'content-type: application/json'
```

```js fct_label="Javascript"
var result = await client.listNotifications(session, 10);
result.notifications.forEach(function(notification) {
  console.log("Notification code %o and subject %o.", notification.code, notification.subject);
})
console.log("To fetch more, use the cursor %o.", result.cacheable_cursor);
```

```fct_label="REST"
GET /v2/notification
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

```csharp fct_label="Unity"
var message = NNotificationsListMessage.Default(100);
client.Send(message, (INResultSet<INNotification> list) => {
  foreach (var n in list.Results) {
    Debug.LogFormat("Notice code '{0}' and subject '{1}'.", n.Code, n.Subject);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
var message = NotificationListMessage(limit: 100)
client.send(message: message).then { notifications in
  for notification in notifications {
    NSLog("Notice code %d and subject %@.", notification.code, notification.subject)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

A list of notifications can be retrieved in batches of up to 100 at a time. To retrieve all messages you should accumulate them with the cacheable cursor. You can keep this cursor on the client and use it when the user reconnects to catch up on any notifications they may have missed while offline.

!!! Hint
    You usually only want to list 100 notifications at a time otherwise you might cause user fatigue. A better option could be to have the UI fetch the next 100 notifications when the user scrolls to the bottom of your UI panel.

```sh fct_label="cURL"
curl -X GET \
  --url http://127.0.0.1:7350/v2/notification?limit=10&cursor=<cursor> \
  --header 'authorization: Bearer <session token>' \
  --header 'content-type: application/json'
```

```js fct_label="Javascript"
var allNotifications = [];

var accumulateNotifications = function(cursor) {
  var result = await client.listNotifications(session, 10, cursor);
  if (result.notifications.length == 0) {
    return;
  }
  allNotifications.concat(result.notifications.notifications);
  accumulateNotifications(result.cacheable_cursor); // recursive async call
}
accumulateNotifications("");
```

```fct_label="REST"
GET /v2/notification?limit=10&cursor=<cursor>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

```csharp fct_label="Unity"
IList<INNotification> allNotifications = new List<INNotification>();

Action accumulateNotifications = delegate(INCursor resumeCursor) {
  var message = new NNotificationsListMessage.Builder(100)
      .Cursor(resumeCursor)
      .Build();
  client.Send(message, (INResultSet<INNotification> list) => {
    if (list.Results.Length < 1) {
      return;
    } else {
      allNotifications.AddRange(list.Results);
      accumulateNotifications(list.Cursor); // recursive async call.
    }
  }, (INError err) => {
    Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
  });
};

var message = NNotificationsListMessage.Default(100);
client.Send(message, (INResultSet<INNotification> list) => {
  allNotifications.AddRange(list.Results);
  accumulateNotifications(list.Cursor);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
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

It can be useful to retrieve only notifications which have been added since the list was last retrieved by a client. This can be done with the cacheable cursor returned with each list message. Sending the cursor through a new list operation will retrieve only notifications newer than those seen.

The cacheable cursor marks the position of the most recent notification retrieved. We recommend you store the cacheable cursor in device storage and use it when the client makes its next request for recent notifications.

```sh fct_label="cURL"
curl -X GET \
  --url http://127.0.0.1:7350/v2/notification?limit=10&cursor=<cacheableCursor> \
  --header 'authorization: Bearer <session token>' \
  --header 'content-type: application/json'
```

```js fct_label="Javascript"
var cacheableCursor = "..."; // stored from last list retrieval.
var results = await client.listNotifications(session, 10, cacheableCursor);
```

```fct_label="REST"
GET /v2/notification?limit=10&cursor=<cacheableCursor>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

```csharp fct_label="Unity"
INCursor resumeCursor = ...; // stored from last list retrieval.

var message = new NNotificationsListMessage.Builder(100)
    .Cursor(resumeCursor)
    .Build();
client.Send(message, (INResultSet<INNotification> list) => {
  // use notification list.
  resumeCursor = list.Cursor; // cache resume cursor.
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
var resumableCursor = ...; // stored from last list retrieval.

var message = NotificationListMessage(limit: 100)
message.cursor = resumableCursor
client.send(message: message).then { notifications in
  resumableCursor = notifications.cursor
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

## Delete notifications

You can delete one or more notifications from the client. This is useful to purge notifications which have been read or consumed by the user and prevent a build up of old messages. When a notification is deleted, all record of it is removed from the system and it cannot be restored.

```sh fct_label="cURL"
curl -X DELETE \
  --url http://127.0.0.1:7350/v2/notification?ids=<notificationId>&ids=<notificationId> \
  --header 'authorization: Bearer <session token>' \
  --header 'content-type: application/json'
```

```js fct_label="Javascript"
var notificationIDs = ["notification id to delete"];
await client.deleteNotifications(session, notificationIDs);
```

```fct_label="REST"
DELETE /v2/notification?ids=<notificationId>&ids=<notificationId>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

```csharp fct_label="Unity"
IList<INNotification> list = new List<INNotification>();
list.Add(...); // Add notification from your internal list
var message = NNotificationsRemoveMessage.Default(list);
client.Send(message, (bool done) => {
  Debug.Log("Notifications were removed.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
var message = NotificationRemoveMessage()
message.notificationIds.append(...) // Add notification from your internal list
client.send(message: message).then {
  NSLog("Notifications were removed.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
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
