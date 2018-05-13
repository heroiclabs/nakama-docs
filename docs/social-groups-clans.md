# Groups and Clans

A group brings together a bunch of users into a small community or team.

A group is made up of a superadmin, admins, and members. It can be public or private which determines whether it appears in results when a user lists groups. Private groups are similar to how Whatsapp groups work. A user can only be added when they're invited to join by one of the group's admins.

A group also has a maximum member count. This is set to 100 by default if the group is created by the client, or can be overriden if the group is created by the code runtime.

A group user has four states:

- __Superadmin__: there must at least be 1 superadmin in any group. The superadmin can only delete the group and promote admin members.
- __Admin__: there can be one of more admins. Admins can update groups as well as accept, kick, promote or add members.
- __Member__: Regular group member. They cannot accept join request from new users.
- __Join request__: A new join request from a new user. This does not count towards the maximum group member count.

## List and filter groups

A user can find public groups to join by listing groups and filtering for groups with a name. This makes it easy to assemble new users into smaller groups for team-based play or collaboration.

Filtering is achieved using a wildcard query that uses the `%` as a way to look for similarities. For instance, if you are looking for groups that contain the world "persian" in them, make the filter `%persian%`. If you don't supply a filtering criteria, Nakama will simply list groups.

```sh fct_label="cURL"
curl 'http://127.0.0.1:7350/v2/group?limit=20&name=%25persian%25' \
  -H 'Authorization: <session token>'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const groups = await client.listGroups(session, "%persian%", 20) // fetch first 20 groups
console.info("Successfully retrieved groups:", groups);
```

```fct_label="REST"
GET /v2/group?limit=20&name=%persian%&cursor={{cursor}}
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

var message = new NGroupsListMessage.Builder()
    .OrderByAsc(true)
    .FilterByLang("en")
    .Build();
client.Send(message, (INResultSet<INGroup> list) => {
  foreach (var group in list.Results) {
    Debug.LogFormat("Group: name '{0}' id '{1}'.", group.Name, group.Id);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

var message = GroupsListMessage()
message.lang = "en"
message.orderAscending = true
client.send(message: message).then { groups in
  for group in groups {
    NSLog("Group: name '%@' id '%@'.", group.name, group.id)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

The message response for a list of groups contains a cursor. The cursor can be used to quickly retrieve the next set of results.

!!! tip
    Cursors are used across different server features to page through batches of results quickly and efficiently. It's used with storage, friends, chat history, etc.

```sh fct_label="cURL"
curl 'http://127.0.0.1:7350/v2/group?limit=20&name=%25persian%25&cursor=somecursor' \
  -H 'Authorization: <session token>'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const cursor = "" // cursor obtained from previous group listing
const groups = await client.listGroups(session, "%persian%", 20) // fetch first 20 groups
console.info("Successfully retrieved groups:", groups);
```

```fct_label="REST"
GET /v2/group?limit=20&name=%persian%&cursor=somecursor
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

var errorHandler = delegate(INError err) {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

var messageBuilder = new NGroupsListMessage.Builder()
    .OrderByAsc(true)
    .FilterByLang("en");
    .PageLimit(100);

client.Send(messageBuilder.Build(), (INResultSet<INGroup> list) => {
  // Lets get the next page of results.
  INCursor cursor = list.Cursor;
  if (cursor != null && list.Results.Count > 0) {
    var message = messageBuilder.Cursor(cursor).Build();

    client.Send(message, (INResultSet<INGroup> nextList) => {
      foreach (var group in nextList.Results) {
        Debug.LogFormat("Group: name '{0}' id '{1}'.", group.Name, group.Id);
      }
    }, errorHandler);
  }
}, errorHandler);
```

```swift fct_label="Swift"
// Requires Nakama 1.x

var message = GroupsListMessage()
message.lang = "en"
message.orderAscending = true
client.send(message: message).then { groups in

  // Lets get the next page of results.
  if let _cursor = groups.cursor && groups.count > 0 {
    message.cursor = _cursor
    client.send(message).then { nextGroups in
      // ...
    }.catch { err in
      throw err
    }
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

## Join groups

When a user has found a group to join they can request to become a member. A public group can be joined without any need for permission while a private group requires a [superadmin or an admin to accept](#accept-new-members) the user.

A user who's part of a group can join [group chat](social-realtime-chat.md#groups) and access it's [message history](social-realtime-chat.md#message-history).

!!! Tip
    When a user joins or leaves a group event messages are added to chat history. This makes it easy for members to see what's changed in the group.

```sh fct_label="cURL"
curl -X POST 'http://127.0.0.1:7350/v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/join' \
  -H 'Authorization: <session token>'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const group_id = "57840ae4-acdc-4b72-b1f9-7bcf41766258";
await client.joinGroup(session, group_id);
console.info("Successfully sent group join request.");
```

```fct_label="REST"
POST /v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/join
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

string groupId = group.Id; // an INGroup ID.

var message = NGroupJoinMessage.Default(groupId);
client.Send(message, (bool done) => {
  Debug.Log("Requested to join group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

let groupID // a group ID
var message = GroupJoinMessage()
message.groupIds.append(groupID)
client.send(message: message).then { _ in
  NSLog("Requested to join group.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

The user will receive an [in-app notification](social-in-app-notifications.md) when they've been added to the group. In a private group an admin or superadmin will receive a notification when a user has requested to join.

## List a user's groups

Each user can list groups they've joined as a member or an admin or a superadmin. The list also contains groups which they've requested to join but not been accepted into yet.

```sh fct_label="cURL"
curl 'http://127.0.0.1:7350/v2/user/042643dd-6bea-47d5-a5b2-a67b58c09693/group' \
  -H 'Authorization: <session token>'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const groups = await client.listUserGroups(session, session.user_id)
groups.user_groups.forEach(function(userGroup){
  console.log("Group: name '%o' id '%o'.", userGroup.group.name, userGroup.group.id);
  // group.State is one of: Admin, Member, or Join.
  console.log("Group's state is %o.", userGroup.state);
})
```

```fct_label="REST"
GET /v2/user/042643dd-6bea-47d5-a5b2-a67b58c09693/group
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

var message = NGroupsSelfListMessage.Default();
client.Send(message, (INResultSet<INGroupSelf> list) => {
  foreach (var group in list.Results) {
    Debug.LogFormat("Group: name '{0}' id '{1}'.", group.Name, group.Id);
    // group.State is one of: Admin, Member, or Join.
    GroupState state = group.State;
    Debug.LogFormat("Group's state is '{0}'.", state);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

var message = GroupsSelfListMessage()
client.send(message: message).then { groups in
  for group in groups {
    NSLog("Group: name '%@' id '%@'", group.name, group.id)
    // group.State is one of: Admin, Member, or Join.
    NSLog("Group's state is '%d'", group.state.rawValue)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

## List group members

A user can list all members who're part of their group. These include other users who've requested to join the private group but not been accepted into yet.

```sh fct_label="cURL"
curl 'http://127.0.0.1:7350/v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/user' \
  -H 'Authorization: <session token>'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const group_id = "57840ae4-acdc-4b72-b1f9-7bcf41766258";
const users = await client.listGroupUsers(session, group_id);
console.info("Successfully retrieved group users:", users);
```

```fct_label="REST"
GET /v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/user
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

string groupId = group.Id; // an INGroup ID.

var message = NGroupUsersListMessage.Default(groupId);
client.Send(message, (INResultSet<INGroupUser> list) => {
  foreach (var member in list.Results) {
    Debug.LogFormat("Member id '{0}' with name '{1}'.", member.Id, member.Fullname);
    // member.State is one of: Admin, Member, or Join.
    UserState state = member.State;
    Debug.LogFormat("Has handle '{0}' with state '{1}'.", member.Handle, state);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

let groupID = ... // a GroupId
var message = GroupUsersListMessage(groupID)
client.send(message: message).then { users in
  for user in users {
    NSLog("Member id '%@' with name '%@", member.fullname, member.id)
    // member.state is one of: Admin, Member, or Join.
    NSLog("Member's state is '%d'", member.state.rawValue)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

## Create a group

A group can be created with a name and other optional fields. These optional fields are used when a user [lists and filter groups](#list-and-filter-groups). The user who creates the group becomes the owner and a superadmin for it.

```sh fct_label="cURL"
curl 'http://127.0.0.1:7350/v2/group' \
  -H 'Authorization: <session token>' \
  -d '{
    "name": "pizza-lovers",
    "description": "pizza lovers, pineapple haters",
    "lang_tag": "fa",
    "open": true
  }'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const group_name = "pizza-lovers";
const description = "pizza lovers, pineapple haters";
const group = await client.createGroup(session, { name: group_name, description: desc, lang_tag: "fa", open: true })
console.info("Successfully created group: ", group);
```

```fct_label="REST"
POST /v2/group
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "name": "pizza-lovers",
  "description": "pizza lovers, pineapple haters",
  "lang_tag": "fa",
  "open": true
}
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

var metadata = "{'my_custom_field': 'some value'}";

var message = new NGroupCreateMessage.Builder("Some unique group name")
    .Description("My awesome group.")
    .Lang("en")
    .Private(true)
    .AvatarUrl("url://somelink")
    .Metadata(metadata)
    .Build();

client.Send(message, (INGroup group) => {
  Debug.LogFormat("New group: name '{0}' id '{1}'.", group.Name, group.Id);
  Debug.Log ("Successfully created a private group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

var groupCreate = GroupCreate("Some unique group name")
groupCreate.description = "My awesome group."
groupCreate.lang = "en"
groupCreate.privateGroup = true
groupCreate.avatarURL = "url://somelink"
groupCreate.metadata = "{'my_custom_field': 'some value'}".data(using: .utf8)!

var message = GroupCreateMessage()
message.groupsCreate.append(groupCreate)
client.send(message: message).then { groups in
  for group in groups {
    NSLog("New group: id '%@' with name '%@", group.id, group.name)
    NSLog("Successfully created a group.")
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

You can also create a group with server-side code. This can be useful when the group must be created together with some other record or feature.

```lua
local nk = require("nakama")
local metadata = { -- Add whatever custom fields you want.
  my_custom_field = "some value"
}

local user_id = "dcb891ea-a311-4681-9213-6741351c9994"
local creator_id = "dcb891ea-a311-4681-9213-6741351c9994"
local name = "Some unique group name"
local description = "My awesome group."
local lang = "en"
local open = true
local creator_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
local avatar_url = "url://somelink"
local maxMemberCount = 100

local success, err = pcall(nk.group_create, user_id, name, creator_id, lang, description, avatar_url, open, metadata, maxMemberCount)
if (not success) then
  nk.logger_error(("Error with creating group: %q"):format(err))
end
```

## Update a group

When a group has been created it's admins can update optional fields.

```sh fct_label="cURL"
curl -X PUT 'http://127.0.0.1:7350/v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258' \
  -H 'Authorization: <session token>' \
  -d '{
    "description": "I was only kidding. Basil for all.",
  }'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const group_id = "57840ae4-acdc-4b72-b1f9-7bcf41766258";
const description = "I was only kidding. Basil for all.";
const group = await client.createGroup(session, group_id, { description: desc });
console.info("Successfully created group: ", group);
```

```fct_label="REST"
PUT /v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "description": "I was only kidding. Basil for all.",
}
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

string groupId = group.Id; // an INGroup ID.

var message = new NGroupUpdateMessage.Builder(groupId)
    .Description("A new group description.")
    .Build();
client.Send(message, (bool done) => {
  Debug.Log("Successfully updated group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

let groupID = ... // an INGroup ID.

var groupUpdate = GroupUpdate(groupID)
groupUpdate.description = "A new group description."

var message = GroupUpdateMessage()
message.groupsUpdate.append(groupUpdate)
client.send(message: message).then { _ in
  NSLog("Successfully updated group.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

## Leave a group

A user can leave a group and will no longer be able to join [group chat](social-realtime-chat.md#groups) or read [message history](social-realtime-chat.md#message-history). If the user is a superadmin they will only be able to leave when at least one other superadmin exists in the group.

!!! Note
    Any user who leaves the group will generate an event message in group chat which other members can read.

```sh fct_label="cURL"
curl -X POST 'http://127.0.0.1:7350/v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/leave' \
  -H 'Authorization: <session token>'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const group_id = "57840ae4-acdc-4b72-b1f9-7bcf41766258";

await client.leaveGroup(session, group_id);
console.info("Successfully left group: ", group);
```

```fct_label="REST"
POST /v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/leave
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

string groupId = group.Id; // an INGroup ID.

var message = NGroupLeaveMessage.Default(groupId);
client.Send(message, (bool done) => {
  Debug.Log ("Successfully left the group.");
}, (INError err) => {
  if (err.Code == ErrorCode.GroupLastAdmin) {
    // Must promote another admin before user can leave group.
    Debug.Log("Unable to leave as last admin.");
  } else {
    Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
  }
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

let groupID // a group ID
var message = GroupLeaveMessage()
message.groupIds.append(groupID)
client.send(message: message).then { _ in
  NSLog("Successfully left the group.")
}.catch { err in
  let nkErr = err as! NakamaError
  switch nkErr {
    case .groupLastAdmin:
      NSLog("Unable to leave as last admin.")
    default:
      NSLog("Error %@ : %@", err, nkErr.message)
  }
}
```

## Manage groups

Each group is managed by one or more superadmins or admins. These users are members with permission to make changes to optional fields, accept or reject new members, remove members or other admins, and promote other members as admins.

!!! Warning
    A group must have at least one superadmin. The last superadmin has to promote another member before they can [leave](#leave-a-group).

### Accept new members

When a user joins a private group it will create a join request until an admin accepts or rejects the user. The superadmin or admin can accept the user into the group.

```sh fct_label="cURL"
curl 'http://127.0.0.1:7350/v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/add' \
  -H 'Authorization: <session token>' \
  -d '{
    "user_ids":["75707313-9654-4521-936d-e6ccf0b29cd4"]
  }'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const group_id = "57840ae4-acdc-4b72-b1f9-7bcf41766258";
const second_user_id = "75707313-9654-4521-936d-e6ccf0b29cd4";
await client.addGroupUsers(session, group_id, [second_user_id]);
console.info("Successfully added user to group.");
```

```fct_label="REST"
POST /v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/add
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "user_ids":["75707313-9654-4521-936d-e6ccf0b29cd4"]
}
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

string groupId = group.Id; // an INGroup ID.
string userId = user.Id;   // an INUser ID.

var message = NGroupAddUserMessage.Default(groupId, userId);
client.Send(message, (bool done) => {
  Debug.Log("Successfully added user to group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

let groupID = ... // a group ID
let userID = ... // a user Id

// A tuple that represents which group the user is added to
let addUser = (groupID: groupID, userID: userID)

var message = GroupAddUserMessage()
message.groupUsers.append(addUser)
client.send(message: message).then { _ in
  NSLog("Successfully added user to group.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

The user will receive an [in-app notification](social-in-app-notifications.md) when they've been added to the group. In a private group an admin will receive a notification about the join request.

To reject the user from joining the group you should [kick them](#kick-a-member).

### Promote a member

An admin can promote another member of the group as an admin. This grants the member the same privileges to [manage the group](#manage-groups). A group can have one or more admins.

```sh fct_label="cURL"
curl 'http://127.0.0.1:7350/v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/promote' \
  -H 'Authorization: <session token>' \
  -d '{
    "user_ids":["75707313-9654-4521-936d-e6ccf0b29cd4"]
  }'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const group_id = "57840ae4-acdc-4b72-b1f9-7bcf41766258";
const second_user_id = "75707313-9654-4521-936d-e6ccf0b29cd4";
await client.promoteGroupUsers(session, group_id, [second_user_id]);
console.info("Successfully promoted user.");
```

```fct_label="REST"
POST /v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/promote
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "user_ids":["75707313-9654-4521-936d-e6ccf0b29cd4"]
}
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

string groupId = group.Id; // an INGroup ID.
string userId = user.Id;   // an INUser ID.

var message = NGroupPromoteUserMessage.Default(groupId, userId);
client.Send(message, (bool done) => {
  Debug.Log("Successfully promoted user as an admin.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

let groupID = ... // a group ID
let userID = ... // a user Id

let promoteUser = (groupID: groupID, userID: userID)

var message = GroupPromoteUserMessage()
message.groupUsers.append(promoteUser)
client.send(message: message).then { _ in
  NSLog("Successfully promoted user as an admin.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

To demote an admin as a member you can [kick](#kick-a-member) and [re-add](#accept-new-members) them.

### Kick a member

An admin or superadmin can kick a member from the group. The user is removed but can rejoin again later unless the group is private in which case an admin must accept the join request.

If a user is removed from a group it does not prevent them from joining other groups.

```sh fct_label="cURL"
curl 'http://127.0.0.1:7350/v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/kick' \
  -H 'Authorization: <session token>' \
  -d '{
    "user_ids":["75707313-9654-4521-936d-e6ccf0b29cd4"]
  }'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const group_id = "57840ae4-acdc-4b72-b1f9-7bcf41766258";
const second_user_id = "75707313-9654-4521-936d-e6ccf0b29cd4";
await client.kickGroupUsers(session, group_id, [second_user_id]);
console.info("Successfully kicked user from the group.");
```

```fct_label="REST"
POST /v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258/kick
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "user_ids":["75707313-9654-4521-936d-e6ccf0b29cd4"]
}
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

string groupId = group.Id; // an INGroup ID.
string userId = user.Id;   // an INUser ID.

var message = NGroupKickUserMessage.Default(groupId, userId);
client.Send(message, (bool done) => {
  Debug.Log ("Successfully kicked user from group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

let groupID = ... // a group ID
let userID = ... // a user Id

let groupUser = (groupID: groupID, userID: userID)

var message = GroupKickUserMessage()
message.groupUsers.append(groupUser)
client.send(message: message).then { _ in
  NSLog("Successfully kicked user from group.");
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

<!--
!!! Hint
    Sometimes a bad user needs to be kicked from the group and [permanently banned](social-friends.md#ban-a-user). This will prevent the user from being able to connect to the server and interact at all.
-->

## Remove a group

A group can only be removed by one of the superadmins which will disband all members. When a group is removed it's name can be re-used to [create a new group](#create-a-group).

```sh fct_label="cURL"
curl -X DELETE 'http://127.0.0.1:7350/v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258' \
  -H 'Authorization: <session token>'
```

```js fct_label="Javascript"
const session = ""; // obtained from authentication.
const group_id = "57840ae4-acdc-4b72-b1f9-7bcf41766258";

await client.deleteGroup(session, group_id);
console.info("Successfully deleted group.");
```

```fct_label="REST"
DELETE /v2/group/57840ae4-acdc-4b72-b1f9-7bcf41766258
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)
```

```csharp fct_label="Unity"
// Requires Nakama 1.x

string groupId = group.Id; // an INGroup ID.

var message = NGroupRemoveMessage.Default(groupId);
client.Send(message, (bool done) => {
  Debug.Log("The group has been removed.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
// Requires Nakama 1.x

let groupID = ... // a group ID
var message = GroupRemoveMessage()
message.groupIds.append(groupID)
client.send(message: message).then { _ in
  NSLog("The group has been removed.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```
