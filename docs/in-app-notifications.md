# In-app Notifications

In-app notifications make it easy to broadcast a message to one or more users. They are great for sending announcements, alerts, or notices of in-game rewards and gifts.

A notification can be stored until read when the app is next opened or it can be pushed so only an active connected user will see it. You can also use notifications to trigger custom actions within your game and change client behavior.

These notifications are viewed within the app which makes them a great companion to push notifications viewed outside the app.

## Send notifications

You can send a notification to one or more users with server-side Lua code. It can be sent to any user in the game, no need to be a friend to be able to exchange messages. A number of notifications are also sent by the server implicitly on certain events. Each notification has a code which is used to categorize it.

!!! note
    The code you choose for your notifications must start at "101" and upwards. See [below](#notification-codes) for reserved message codes.

A notification has content which will be encoded as JSON and must be given an "expires_at" value in milliseconds which indicates how long the notification will be available before it's removed. A message cannot be sent which expires in less than 60 seconds.

Notifications can be sent as persistent or not. A non-persistent message will only be received by a client which is currently connected to the server (i.e. a user who is online). If you want to make sure a notification is never lost before it's read it should be marked as persistent when sent.

```lua
local nk = require("nakama")

local user_ids = {"someuserid", "anotheruserid"}
local sender_id = nil   -- "nil" for server sent.
local content = {
  item_id = "192308394345345",
  item_icon = "storm_bringer_sword.png"
}

local notifications = {}
for _, user_id in ipairs(user_ids)
do
  local notification = {
    UserId = user_id,
    SenderId = sender_id,
    Subject = "You earned a secret item!",
    Content = content,
    Code = 101,
    ExpiresAt = 1000 * 60 * 60 * 24 * 7,  -- expires in 7 days.
    Persistent = true
  }
  table.insert(notifications, notification)
end

nk.notification_send_id(notifications)
```

## Receive notifications

An event handler can be registered for notifications received when a client is connected. The handler will be called whenever a message is received. When multiple messages are returned (batched for performance) the handler will be called for each notification.

```csharp fct_label="Unity"
client.OnNotification += (object sender, NNotificationEventArgs args) => {
  INNotification n = args.Notification;
  Debug.LogFormat("Received code '{0}' and subject '{1}'.", n.Code, n.Subject);
  var content = Encoding.UTF8.GetString(n.Content); // convert byte[].
  Debug.LogFormat("Received id '{0}' and content '{1}'.", n.Id, content);
};
```

## List notifications

You can list notifications which were received when the user was offline. These notifications are ones which were marked "persistent" when sent. It depends on your game or app but we suggest you retrieve notifications after a client reconnects. You can then display a UI within your game or app with the list.

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

A list of notifications can be retrieved in batches of up to 100 at a time. To retrieve all messages you should accumulate them with the resume cursor.

!!! hint
    You usually only want to list 100 notifications at a time otherwise you might cause user fatigue. A better option could be to have the UI fetch the next 100 notifications when the user scrolls to the bottom of your UI panel.

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

It can be useful to retrieve only notifications which have been added since the list was last retrieved by a client. This can be done with the resume cursor returned with each list message.

The resume cursor marks the position of the most recent notification retrieved. We recommend you store the resume cursor in device storage and use it when the client makes it's next request for recent notifications.

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

## Delete notifications

You can delete one or more notifications from the client. This is useful to purge notifications which have been read or consumed by the user and prevent a build up of old messages. When a notification is deleted (or it expires), all record of the message is removed from the system and it cannot be restored.

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

## Notification codes

The server reserves codes 0 up to 100 for messages sent implicitly on certain events. The code is useful to decide how to display the notification in your UI.

| Code | Purpose |
| ---- | ------- |
|  001 | User X wants to chat. |
|  002 | User X wants to add you as a friend. |
|  003 | User X accepted your friend invite. |
|  004 | You've been accepted to X group. |
|  005 | User X wants to join your group. |
|  006 | Your friend X has just joined the game. |
