# User accounts

A user represents an identity within the server. Every user is registered and has a profile for other users to find and become [friends](social-friends.md) with or join [groups](social-groups-clans.md) and [chat](social-realtime-chat.md).

A user can own [records](storage-access-controls.md), share public information with other users, and authenticate via a variety of different social providers.

## Fetch account

When a user has a session you can retrieve their account. The profile contains a variety of information which includes various "linked" social providers.

```sh fct_label="cURL"
curl -X GET "http://127.0.0.1:7350/v2/account" \
  -H 'authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
const account = await client.getAccount(session);
const user = account.user;
console.info("User id '%o' and username '%o'.", user.id, user.username);
console.info("User's wallet:", account.wallet);
```

```csharp fct_label=".NET"
var account = await client.GetAccountAsync(session);
var user = account.User;
System.Console.WriteLine("User id '{0}' username '{1}'", user.Id, user.Username);
System.Console.WriteLine("User wallet: '{0}'", account.Wallet);
```

```csharp fct_label="Unity"
var account = await client.GetAccountAsync(session);
var user = account.User;
Debug.LogFormat("User id '{0}' username '{1}'", user.Id, user.Username);
Debug.LogFormat("User wallet: '{0}'", account.Wallet);
```

```cpp fct_label="Cocos2d-x C++"
auto successCallback = [](const NAccount& account)
{
  CCLOG("User's wallet: %s", account.wallet.c_str());
};
client->getAccount(session, successCallback);
```

```js fct_label="Cocos2d-x JS"
client.getAccount(session)
  .then(function(account) {
      cc.log("User's wallet:", account.wallet);
    },
    function(error) {
      cc.error("get account failed:", JSON.stringify(error));
    });
```

```cpp fct_label="C++"
auto successCallback = [](const NAccount& account)
{
  CCLOG("User's wallet: %s", account.wallet.c_str());
};
client->getAccount(session, successCallback);
```

```java fct_label="Java"
Account account = client.getAccount(session);
User user = account.getUser();
System.out.format("User id %s username %s", user.getId(), user.getUsername());
System.out.format("User wallet %s", account.getWallet());
```

```swift fct_label="Swift"
// Requires Nakama 1.x
let message = SelfFetchMessage()
client.send(message: message).then { selfuser in
  NSLog("User id '%@' and handle '%@'", selfuser.id, selfuser.handle)
  NSLog("User has JSON metadata '%@'", selfuser.metadata)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
GET /v2/account
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
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

Nakama has the concept of a virtual wallet and transaction ledger. Nakama allows developers to create, update and list changes to the user's wallet. This operation has transactional guarantees and is only achievable with the [script runtime](runtime-code-basics.md).

With server-side code it's possible to update the user's wallet.

```lua fct_label="Lua"
local nk = require("nakama")

local user_id = "95f05d94-cc66-445a-b4d1-9e262662cf79"
local content = {
  reward_coins = 1000 -- Add 1000 coins to the user's wallet.
}

local status, err = pcall(nk.wallet_update, user_id, content)
if (not status) then
    nk.logger_error(("User wallet update error: %q"):format(err))
end
```

```golang fct_label="Go"
userID := "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
content := map[string]interface{}{
	"reward_coins": 1000, // Add 1000 coins to the user's wallet.
}
if err := nk.WalletUpdate(ctx, userID, content, nil, true); err != nil {
	logger.Error("User wallet update error: %v", err.Error())
}
```

The wallet is private to a user and cannot be seen by other users. You can fetch wallet information for a user via [Fetch Account](user-accounts.md#fetch-account) operation.

### Online indicator

Nakama can report back user online indicators in two ways:

1. [Fetch user](user-accounts.md#fetch-users) information. This will give you a quick snapshot view of the user's online indicator and is not a reliable way to detect user presence.
2. Publish and subscribe to user [status presence](social-status.md) updates. This will give you updates when the online status of the user changes (along side a custom message).

## Fetch users

You can fetch one or more users by their IDs or handles. This is useful for displaying public profiles with other users.

```sh fct_label="cURL"
curl -X GET "http://127.0.0.1:7350/v2/user?ids=userid1&ids=userid2&usernames=username1&usernames=username2&facebook_ids=facebookid1" \
  -H 'authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
const users = await client.getUsers(session, ["user_id1"], ["username1"], ["facebookid1"]);
users.foreach(user => {
  console.info("User id '%o' and username '%o'.", user.id, user.username);
});
```

```csharp fct_label=".NET"
var ids = new[] {"userid1", "userid2"};
var usernames = new[] {"username1", "username2"};
var facebookIds = new[] {"facebookid1"};
var result = await client.GetUsersAsync(session, ids, usernames, facebookIds);
foreach (var u in result.Users)
{
    System.Console.WriteLine("User id '{0}' username '{1}'", u.Id, u.Username);
}
```

```csharp fct_label="Unity"
var ids = new[] {"userid1", "userid2"};
var usernames = new[] {"username1", "username2"};
var facebookIds = new[] {"facebookid1"};
var result = await client.GetUsersAsync(session, ids, usernames, facebookIds);
foreach (var u in result.Users)
{
    Debug.LogFormat("User id '{0}' username '{1}'", u.Id, u.Username);
}
```

```cpp fct_label="Cocos2d-x C++"
auto successCallback = [](const NUsers& users)
{
  for (auto& user : users.users)
  {
    CCLOG("User id '%s' username %s", user.id.c_str(), user.username.c_str());
  }
};
client->getUsers(session,
    { "user_id1" },
    { "username1" },
    { "facebookid1" },
    successCallback);
```

```js fct_label="Cocos2d-x JS"
client.getUsers(session, ["user_id1"], ["username1"], ["facebookid1"])
  .then(function(users) {
      cc.log("Users:", JSON.stringify(users));
    },
    function(error) {
      cc.error("get users failed:", JSON.stringify(error));
    });
```

```cpp fct_label="C++"
auto successCallback = [](const NUsers& users)
{
  for (auto& user : users.users)
  {
    std::cout << "User id '" << user.id << "' username " << user.username << std::endl;
  }
};
client->getUsers(session,
    { "user_id1" },
    { "username1" },
    { "facebookid1" },
    successCallback);
```

```java fct_label="Java"
List<String> ids = Arrays.asList("userid1", "userid2");
List<String> usernames = Arrays.asList("username1", "username1");
String[] facebookIds = new String[] {"facebookid1"};
Users users = client.getUsers(session, ids, usernames, facebookIds).get();

for (User user : users.getUsersList()) {
  System.out.format("User id %s username %s", user.getId(), user.getUsername());
}
```

```swift fct_label="Swift"
// Requires Nakama 1.x
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

```fct_label="REST"
GET /v2/user?ids=userid1&ids=userid2&usernames=username1&usernames=username2&facebook_ids=facebookid1
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

You can also fetch one or more users in server-side code.

```lua fct_label="Lua"
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

```golang fct_label="Go"
if users, err := nk.UsersGetId(ctx, []string{
	"3ea5608a-43c3-11e7-90f9-7b9397165f34",
	"447524be-43c3-11e7-af09-3f7172f05936",
}); err != nil {
	// Handle error.
} else {
	for _, u := range users {
		logger.Info("username: %s, displayname: %s", u.Username, u.DisplayName)
	}
}
```

## Update account

When a user is registered most of their profile is setup with default values. A user can update their own profile to change fields but cannot change any other user's profile.

```sh fct_label="cURL"
curl -X PUT "http://127.0.0.1:7350/v2/account" \
  -H 'authorization: Bearer <session token>' \
  --data '{
    "display_name": "My new name",
    "avatar_url": "http://graph.facebook.com/avatar_url",
    "location": "San Francisco"
  }'
```

```js fct_label="JavaScript"
await client.updateAccount(session, {
  display_name: "My new name",
  avatar_url: "http://graph.facebook.com/avatar_url",
  location: "San Francisco"
});
```

```csharp fct_label=".NET"
const string displayName = "My new name";
const string avatarUrl = "http://graph.facebook.com/avatar_url";
const string location = "San Francisco";
await client.UpdateAccountAsync(session, null, displayName, avatarUrl, null, location);
```

```csharp fct_label="Unity"
const string displayName = "My new name";
const string avatarUrl = "http://graph.facebook.com/avatar_url";
const string location = "San Francisco";
await client.UpdateAccountAsync(session, null, displayName, avatarUrl, null, location);
```

```cpp fct_label="Cocos2d-x C++"
client->updateAccount(session,
    opt::nullopt,
    "My new name", // display name
    "http://graph.facebook.com/avatar_url", // avatar URL
    opt::nullopt,
    "San Francisco" // location
    );
```

```js fct_label="Cocos2d-x JS"
client.updateAccount(session, {
  display_name: "My new name",
  avatar_url: "http://graph.facebook.com/avatar_url",
  location: "San Francisco"
});
```

```cpp fct_label="C++"
client->updateAccount(session,
    opt::nullopt,
    "My new name", // display name
    "http://graph.facebook.com/avatar_url", // avatar URL
    opt::nullopt,
    "San Francisco" // location
    );
```

```java fct_label="Java"
String displayName = "My new name";
String avatarUrl = "http://graph.facebook.com/avatar_url";
String location = "San Francisco";
client.updateAccount(session, null, displayName, avatarUrl, null, location);
```

```swift fct_label="Swift"
// Requires Nakama 1.x
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

With server-side code it's possible to update any user's profile.

```lua fct_label="Lua"
local nk = require("nakama")

local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user's id.
local metadata = {}
local username = "my-new-username"
local display_name = "My new Name"
local timezone = nil
local location = "San Francisco"
local lang_tag = nil
local avatar_url = "http://graph.facebook.com/avatar_url"

local status, err = pcall(nk.account_update_id, metadata, username, display_name, timezone, location, lang_tag, avatar_url)
if (not status) then
  nk.logger_info(("Account update error: %q"):format(err))
end
```

```golang fct_label="Go"
userID := "4ec4f126-3f9d-11e7-84ef-b7c182b36521" // some user's id.
username := "my-new-username" // must be unique
metadata := make(map[string]interface{})
displayName := "My new name"
timezone := ""
location := "San Francisco"
langTag := ""
avatarUrl := "http://graph.facebook.com/avatar_url"
if err := nk.AccountUpdateId(ctx, userID, username, metadata, displayName, timezone, location, langTag, avatarUrl); err != nil {
	// Handle error.
	logger.Error("Account update error: %s", err.Error())
}
```
