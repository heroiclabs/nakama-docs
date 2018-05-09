# User accounts

A user represents an identity within the server. Every user is registered and has a profile for other users to find and become [friends](social-friends.md) with or join [groups](social-groups-clans.md) and [chat](social-realtime-chat.md).

A user can own [records](storage-access-controls.md), share public information with other users, and authenticate via a bunch of different social providers.

## Fetch account

When a user has a session you can retrieve their account. The profile contains lots of information which includes various "linked" social providers.

```sh fct_label="cURL"
curl -X GET \
  http://127.0.0.1:7350/v2/account \
  -H 'Accept: application/json' \
  -H 'authorization: Bearer <session token>' \
  -H 'Content-Type: application/json' \
```

```js fct_label="Javascript"
var session = "..."; // You can get this when you authenticate

try {
  var account = await client.getAccount(session);
  console.log("User id '%o' and username '%o'.", account.user.id, account.user.username);
  console.log("User's wallet: %o", account.wallet);
  console.log("User's custom ID: %o", account.custom_id);
} catch(e) {
  console.log("An error occured: %o", error);
}
```

```fct_label="REST"
GET /v2/account
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

```csharp fct_label="Unity"
var message = NSelfFetchMessage.Default();
client.Send(message, (INSelf self) => {
  Debug.LogFormat("User has id '{0}' and handle '{1}'.", self.Id, self.Handle);
  Debug.LogFormat("User has JSON metadata '{0}'.", self.Metadata);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
CollatedMessage<Self> message = SelfFetchMessage.Builder.build();
Deferred<Self> deferred = client.send(message);
deferred.addCallback(new Callback<Self, Self>() {
  @Override
  public Self call(Self self) throws Exception {
    String metadata = new String(self.getMetadata());
    System.out.format("User has JSON metadata '%s'.", metadata);
    return self;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
```

```swift fct_label="Swift"
let message = SelfFetchMessage()
client.send(message: message).then { selfuser in
  NSLog("User id '%@' and handle '%@'", selfuser.id, selfuser.handle)
  NSLog("User has JSON metadata '%@'", selfuser.metadata)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

Some information like wallet, device IDs and custom ID are private but part of the profile is visible to other users.

| Public field | Description |
| ----- | ----------- |
| user.id | The unique identifier for the user. |
| user.username | A unique nickname for the user. |
| user.display_name | The display name for the user (empty by default). |
| user.avatar_url | A URL with a profile picture for the user (empty by default). |
| user.lang | The preferred language settings for the user (default is "en"). |
| user.location | The location of the user (empty by default). |
| user.timezone | The timezone of the user (empty by default). |
| user.metadata | A slot for custom information for the user - only readable from the client. |
| user.edge_count | Number of friends this user has. |
| user.facebook_id | Facebook identifier associated with this user. |
| user.google_id | Google identifier associated with this user. |
| user.gamecenter_id | GameCenter identifier associated with this user. |
| user.steam_id | Steam identifier associated with this user. |
| user.create_time | A timestamp for when the user was created. |
| user.update_time | A timestamp for when the user was last updated. |
| user.online | A boolean that indicates whether the user is currently online or not. |

| Private fields | Description |
| ----- | ----------- |
| email | Email address associated with this user. |
| devices | List of device IDs associated with this user. |
| custom_id | Custom identifier associated with this user. |
| wallet | User's wallet - only readable from the client. |
| verifiy_time | A timestamp for when the user was verified (currently only used by Facebook). |

### User metadata

You can store additional fields for a user in `user.metadata` which is useful to share data you want to be public to other users. Metadata is limited to 16KB per user. This can be set only via the [script runtime](runtime-code-basics.md), similar to the `wallet`.

!!! Tip
    We recommend you choose user metadata to store very common fields which other users will need to see. For all other information you can store records with [public read permissions](storage-collections.md) which other users can find.

### Virtual wallet

Nakama has the concept of a virtual wallet and transaction ledger. Nakama allows developers to create, update and list changes to the user's wallet. This operation has transactional guarantuees and is only achievable with the [script runtime](runtime-code-basics.md).

With server-side code it's possible to update the user's wallet.

```lua
local nk = require("nakama")

local user_id = "95f05d94-cc66-445a-b4d1-9e262662cf79" -- who to send
local content = {
  reward_coins = 1000
}

local status, err = pcall(nk.wallet_update, user_id, content)
if (not status) then
  nk.logger_info(("User wallet update error: %q"):format(err))
end
```

The wallet is private to a user and cannot be seen by other users. You can fetch wallet information for a user via [Fetch Account](user-accounts.md#fetch-account) operation.

### Online indicator

Nakama can report back user online indicators in two ways:

1. [Fetch user](user-accounts.md#fetch-users) information. This will give you a quick snapshot view of the user's online indicator and is not a reliable way to detect user presence.
2. Publish and subscribe to user [status presence](social-status.md) updates. This will give you updates when the online status of the user changes (along side a custom message).

## Fetch users

You can fetch one or more users by their IDs or handles. This is useful for displaying public profiles with other users.

```sh fct_label="cURL"
curl -X GET \
  'http://127.0.0.1:7350/v2/user?ids=userid1&ids=userid2&usernames=username1&usernames=username2&facebook_ids=facebookid1' \
  -H 'Accept: application/json' \
  -H 'authorization: Bearer <session token>' \
  -H 'Content-Type: application/json' \
```

```js fct_label="Javascript"
var session = "..."; // You can get this when you authenticate

try {
  var users = await client.getUsers(session, [user_id1], [username1], [facebookid1]);
  users.foreach(function(user){
    console.log("User id '%o' and username '%o'.", user.id, user.username);
  });
} catch(e) {
  console.log("An error occured: %o", error);
}
```

```fct_label="REST"
GET /v2/user?ids=userid1&ids=userid2&usernames=username1&usernames=username2&facebook_ids=facebookid1
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

```csharp fct_label="Unity"
string id = user.Id; // an INUser ID.

var message = NUsersFetchMessage.Default(id);
client.Send(message, (INResultSet<INUser> list) => {
  Debug.LogFormat("Fetched '{0}' users.", list.Results.Count);
  foreach (var user in list.Results) {
    Debug.LogFormat("User has id '{0}' and handle '{1}'.", user.Id, user.Handle);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
byte[] id = user.getId(); // a User object Id.

CollatedMessage<ResultSet<User>> message = UsersFetchMessage.Builder.newBuilder()
  .id(id)
  .build();
Deferred<ResultSet<User>> deferred = client.send(message);
deferred.addCallback(new Callback<ResultSet<User>, ResultSet<User>>() {
  @Override
  public ResultSet<User> call(ResultSet<User> list) throws Exception {
    for (User user : list) {
      String userId = new String(user.getId());
      System.out.format("User(id=%s, handle=%s)", userId, user.getHandle());
    }
    return self;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
```

```swift fct_label="Swift"
let userID // a User ID

var message = UsersFetchMessage()
message.userIDs.append(userID)

client.send(message: message).then { users in
  for user in users {
    NSLog("User id '%@' and handle '%@'", user.id, user.handle)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

You can also fetch one or more users in server-side code.

```lua
local nk = require("nakama")

local user_ids = {
  "3ea5608a-43c3-11e7-90f9-7b9397165f34",
  "447524be-43c3-11e7-af09-3f7172f05936"
}
local users = nk.users_get_id(user_ids)
for _, u in ipairs(users)
do
  local message = ("username: %q, displayname: %q"):format(u.username, u.display_name)
  nk.logger_info(message)
end
```

## Update self

When a user is registered most of their profile is setup with default values. A user can update their own profile to change fields but cannot change any other user's profile.

```sh fct_label="cURL"
curl -X PUT \
  http://127.0.0.1:7350/v2/account \
  -H 'Accept: application/json' \
  -H 'authorization: Bearer <session token>' \
  -H 'Content-Type: application/json' \
  -d '{
    "display_name": "My new name",
    "avatar_url": "http://graph.facebook.com/avatar_url",
    "location": "San Francisco"
  }'
```

```js fct_label="Javascript"
var session = "..."; // You can get this when you authenticate

try {
  await client.updateAccount(session, {
    display_name: "My new name",
    avatar_url: "http://graph.facebook.com/avatar_url",
    location: "San Franciso"
  });

  console.log("Successfully updated yourself.");
} catch(e) {
  console.log("An error occured: %o", error);
}
```

```fct_label="REST"
PUT /v2/account HTTP/1.1
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{
  "display_name": "My new name",
  "avatar_url": "http://graph.facebook.com/avatar_url",
  "location": "San Francisco"
}
```

```csharp fct_label="Unity"
var message = new NSelfUpdateMessage.Builder()
    .AvatarUrl("http://graph.facebook.com/avatar_url")
    .Fullname("My New Name")
    .Location("San Francisco")
    .Build();
client.Send(message, (bool done) => {
  Debug.Log("Successfully updated yourself.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
CollatedMessage<Boolean> message = SelfUpdateMessage.Builder.newBuilder()
    .avatarUrl("http://graph.facebook.com/avatar_url")
    .fullname("My New Name")
    .location("San Francisco")
    .build();
Deferred<Boolean> deferred = client.send(message);
deferred.addCallback(new Callback<Boolean, Boolean>() {
  @Override
  public Boolean call(Boolean done) throws Exception {
    System.out.println("Successfully updated yourself.");
    return done;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
```

```swift fct_label="Swift"
var message = SelfUpdateMessage()
message.avatarUrl = "http://graph.facebook.com/avatar_url"
message.fullname = "My New Name"
message.location = "San Francisco"
client.send(message: message).then {
  NSLog("Successfully updated yourself.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

With server-side code it's possible to update one or more user's profiles.

```lua
local nk = require("nakama")

local user_update = {
  user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521", -- some user's id.
  avatar_url = "http://graph.facebook.com/avatar_url",
  display_name = "My new Name",
  location = "San Francisco",
  metadata = {}
}
local status, err = pcall(nk.users_update, { user_update })
if (not status) then
  nk.logger_info(("User update error: %q"):format(err))
end
```
