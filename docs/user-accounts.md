# User accounts

A user represents an identity within the server. Every user is registered and has a profile for other users to find and become [friends](social-friends.md) with or join [groups](social-groups-clans.md) and [chat](social-realtime-chat.md).

A user can own [records](storage-record-ownership.md), share public information with other users, and login via a bunch of different social providers.

## Fetch self

When a user has a session you can retrieve their profile. The profile is returned for yourself and so we refer to it as "self" in client code. The profile contains lots of information which includes various "linked" social providers.

```csharp fct_label="Unity"
var message = NSelfFetchMessage.Default();
client.Send(message, (INSelf self) => {
  var id = Encoding.UTF8.GetString(self.Id);
  Debug.LogFormat("User has id '{0}' and handle '{1}'.", id, self.Handle);
  var metadata = Encoding.UTF8.GetString(self.Metadata);
  Debug.LogFormat("User has JSON metadata '{0}'.", metadata);
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
  NSLog("User id '@%' and handle '@%'", selfuser.id, selfuser.handle)
  NSLog("User has JSON metadata '@%'", selfuser.metadata)
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

Some information like social IDs are private but part of the profile is visible to other users.

| Public field | Description |
| ----- | ----------- |
| AvatarUrl | A URL with a profile picture for the user (empty by default). |
| CreatedAt | A timestamp in milliseconds for when the user was created. |
| Fullname | The full name for the user (empty by default). |
| Handle | A unique nickname for the user. |
| Id | The unique identifier for the user. |
| Lang | The preferred language settings for the user (default is "en"). |
| LastOnlineAt | A timestamp in milliseconds when the user was last online. |
| Location | The location of the user (empty by default). |
| Metadata | A slot for custom information to be added for the user (must be JSON encoded). |
| Timezone | The timezone of the user (empty by default). |
| UpdatedAt | A timestamp in milliseconds for when the user was last updated. |

You can store additional fields for a user in `"Metadata"` which is useful to share data you want to be public to other users. Metadata is limited to 16KB per user.

!!! Tip
    We recommend you choose user metadata to store very common fields which other users will need to see. For all other information you can store records with [public read permissions](storage-record-permissions.md) which other users can find.

## Fetch users

You can fetch one or more users by their IDs or handles. This is useful for displaying public profiles with other users.

```csharp fct_label="Unity"
byte[] id = user.Id; // an INUser ID.

var message = NUsersFetchMessage.Default(id);
client.Send(message, (INResultSet<INUser> list) => {
  Debug.LogFormat("Fetched '{0}' users.", list.Results.Count);
  foreach (var user in list.Results) {
    var id = Encoding.UTF8.GetString(user.Id);
    Debug.LogFormat("User has id '{0}' and handle '{1}'.", id, user.Handle);
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
let userID = UUID() // a User ID

var message = UsersFetchMessage()
message.userIDs.append(userID)

client.send(message: message).then { users in
  for user in users {
    NSLog("User id '@%' and handle '@%'", user.id, user.handle)
  }
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

You can also fetch one or more users in server-side code.

```lua
local nk = require("nakama")

local user_ids = {
  "3ea5608a-43c3-11e7-90f9-7b9397165f34",
  "447524be-43c3-11e7-af09-3f7172f05936"
}
local users = nk.user_fetch_id(user_ids)
for _, u in ipairs(users)
do
  local message = ("handle: %q, fullname: %q"):format(u.Handle, u.Fullname)
  nk.logger_info(message)
end
```

## Update self

When a user is registered most of their profile is setup with default values. A user can update their own profile to change fields but cannot change any other user's profile.

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
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

With server-side code it's possible to update one or more user's profiles.

```lua
local nk = require("nakama")

local user_update = {
  UserId = "4ec4f126-3f9d-11e7-84ef-b7c182b36521", -- some user's id.
  AvatarUrl = "http://graph.facebook.com/avatar_url",
  Fullname = "My new Name",
  Location = "San Francisco",
  Metadata = {}
}
local status, err = pcall(nk.users_update, { user_update })
if (not status) then
  nk.logger_info(("User update error: %q"):format(err))
end
```
