# Groups and Clans

A group brings together a bunch of users into a small community or team.

A group is made up of a superadmin, admins, and members. It can be public or private which determines whether it appears in results when a user lists groups. Private groups are similar to how Whatsapp groups work. A user can only be added when they're invited to join by one of the group's admins.

A group also has a maximum member count. This is set to 100 by default if the group is created by the client, or can be overriden if the group is created by the code runtime.

A group user has four states:

| Code | Purpose | |
| ---- | ------- | - |
|    0 | Superadmin | There must at least be 1 superadmin in any group. The superadmin can only delete the group and promote admin members. |
|    1 | Admin | There can be one of more admins. Admins can update groups as well as accept, kick, promote or add members. |
|    2 | Member | Regular group member. They cannot accept join request from new users. |
|    3 | Join request | A new join request from a new user. This does not count towards the maximum group member count. |

## List and filter groups

A user can find public groups to join by listing groups and filtering for groups with a name. This makes it easy to assemble new users into smaller groups for team-based play or collaboration.

Filtering is achieved using a wildcard query that uses the `%` as a way to look for similarities. For instance, if you are looking for groups that contain the world "persian" in them, make the filter `%persian%`. If you don't supply a filtering criteria, Nakama will simply list groups.

```sh tab="cURL"
curl "http://127.0.0.1:7350/v2/group?limit=20&name=heroes%25" \
  -H 'Authorization: Bearer <session token>'
```

```js tab="JavaScript"
const groups = await client.listGroups(session, "heroes%", 20); // fetch first 20 groups
console.info("List of groups:", groups);
```

```csharp tab=".NET"
// Filter for group names which start with "heroes"
const string nameFilter = "heroes%";
var result = await client.ListGroupsAsync(session, nameFilter, 20);
foreach (var g in result.Groups)
{
    System.Console.WriteLine("Group name '{0}' count '{1}'", g.Name, g.EdgeCount);
}
```

```csharp tab="Unity"
// Filter for group names which start with "heroes"
const string nameFilter = "heroes%";
var result = await client.ListGroupsAsync(session, nameFilter, 20);
foreach (var g in result.Groups)
{
    Debug.LogFormat("Group name '{0}' count '{1}'", g.Name, g.EdgeCount);
}
```

```cpp tab="Cocos2d-x C++"
auto successCallback = [this](NGroupListPtr list)
{
  for (auto& group : list->groups)
  {
    CCLOG("Group name '%s' count %d", group.name.c_str(), group.edgeCount);
  }
};

client->listGroups(session,
    "heroes%", // Filter for group names which start with "heroes"
    20,        // fetch first 20 groups
    "",
    successCallback);
```

```js tab="Cocos2d-x JS"
client.listGroups(session, "heroes%", 20) // fetch first 20 groups
  .then(function(groups) {
      cc.log("List of groups:", JSON.stringify(groups));
    },
    function(error) {
      cc.error("list groups failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = [this](NGroupListPtr list)
{
  for (auto& group : list->groups)
  {
    std::cout << "Group name '" << group.name << "' count " << group.edgeCount << std::endl;
  }
};

client->listGroups(session,
    "heroes%", // Filter for group names which start with "heroes"
    20,        // fetch first 20 groups
    "",
    successCallback);
```

```java tab="Java"
// Filter for group names which start with "heroes"
String nameFilter = "heroes%";
GroupList groups = client.listGroups(session, nameFilter, 20).get();
for (Group group : groups.getGroupsList()) {
  System.out.format("Group name %s count %s", group.getName(), group.getEdgeCount());
}
```

```swift tab="Swift"
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

```tab="REST"
GET /v2/group?limit=20&name=heroes%&cursor=<cursor>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

The message response for a list of groups contains a cursor. The cursor can be used to quickly retrieve the next set of results.

!!! tip
    Cursors are used across different server features to page through batches of results quickly and efficiently. It's used with storage, friends, chat history, etc.

```sh tab="cURL"
curl -X GET "http://127.0.0.1:7350/v2/group?limit=20&name=%25heroes%25&cursor=somecursor" \
  -H 'Authorization: Bearer <session token>'
```

```js tab="JavaScript"
const groups = await client.listGroups(session, "heroes%", 20, cursor);
console.info("List of groups:", groups);
```

```csharp tab=".NET"
// Filter for group names which start with "heroes"
const string nameFilter = "heroes%";
var result = await client.ListGroupsAsync(session, nameFilter, 20);
// If there are more results get next page.
if (result.Cursor != null)
{
    result = await client.ListGroupsAsync(session, nameFilter, 20, result.Cursor);
    foreach (var g in result.Groups)
    {
        System.Console.WriteLine("Group name '{0}' count '{1}'", g.Name, g.EdgeCount);
    }
}
```

```csharp tab="Unity"
// Filter for group names which start with "heroes"
const string nameFilter = "heroes%";
var result = await client.ListGroupsAsync(session, nameFilter, 20);
// If there are more results get next page.
if (result.Cursor != null)
{
    result = await client.ListGroupsAsync(session, nameFilter, 20, result.Cursor);
    foreach (var g in result.Groups)
    {
        Debug.LogFormat("Group name '{0}' count '{1}'", g.Name, g.EdgeCount);
    }
}
```

```cpp tab="Cocos2d-x C++"
void YourClass::processGroupList(NGroupListPtr list)
{
  for (auto& group : list->groups)
  {
    CCLOG("Group name '%s' count %d", group.name.c_str(), group.edgeCount);
  }

  if (!list->cursor.empty())
  {
    // request next page
    requestHeroes(list->cursor);
  }
}

void YourClass::requestHeroes(const string& cursor)
{
  client->listGroups(session,
      "heroes%", // Filter for group names which start with "heroes"
      20,        // fetch first 20 groups
      cursor,
      std::bind(&YourClass::processGroupList, this, std::placeholders::_1));
}
```

```js tab="Cocos2d-x JS"
client.listGroups(session, "heroes%", 20) // fetch first 20 groups
  .then(function(groups) {
      cc.log("List of groups:", JSON.stringify(groups));
    },
    function(error) {
      cc.error("list groups failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
void YourClass::processGroupList(NGroupListPtr list)
{
  for (auto& group : list->groups)
  {
    std::cout << "Group name '" << group.name << "' count " << group.edgeCount << std::endl;
  }

  if (!list->cursor.empty())
  {
    // request next page
    requestHeroes(list->cursor);
  }
}

void YourClass::requestHeroes(const string& cursor)
{
  client->listGroups(session,
      "heroes%", // Filter for group names which start with "heroes"
      20,        // fetch first 20 groups
      cursor,
      std::bind(&YourClass::processGroupList, this, std::placeholders::_1));
}
```

```java tab="Java"
// Filter for group names which start with "heroes"
String nameFilter = "heroes%";
GroupList groups = client.listGroups(session, nameFilter, 20).get();
if (groups.getCursor() != null) {
  groups = client.listGroups(session, nameFilter, 20, groups.getCursor()).get();
  for (Group group : groups.getGroupsList()) {
    System.out.format("Group name %s count %s", group.getName(), group.getEdgeCount());
  }
}
```

```swift tab="Swift"
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

```tab="REST"
GET /v2/group?limit=20&name=heroes%&cursor=somecursor
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

## Join groups

When a user has found a group to join they can request to become a member. A public group can be joined without any need for permission while a private group requires a [superadmin or an admin to accept](#accept-new-members) the user.

A user who's part of a group can join [group chat](social-realtime-chat.md#groups) and access it's [message history](social-realtime-chat.md#message-history).

!!! Tip
    When a user joins or leaves a group event messages are added to chat history. This makes it easy for members to see what's changed in the group.

```sh tab="cURL"
curl -X POST "http://127.0.0.1:7350/v2/group/<group id>/join" \
  -H 'Authorization: Bearer <session token>'
```

```js tab="JavaScript"
const group_id = "<group id>";
await client.joinGroup(session, group_id);
console.info("Sent group join request", group_id);
```

```csharp tab=".NET"
const string groupId = "<group id>";
await client.JoinGroupAsync(session, groupId);
System.Console.WriteLine("Sent group join request '{0}'", groupId);
```

```csharp tab="Unity"
const string groupId = "<group id>";
await client.JoinGroupAsync(session, groupId);
Debug.LogFormat("Sent group join request '{0}'", groupId);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = []()
{
  CCLOG("Sent group join request");
};

string group_id = "<group id>";
client->joinGroup(session, group_id, successCallback);
```

```js tab="Cocos2d-x JS"
const group_id = "<group id>";
client.joinGroup(session, group_id)
  .then(function(ticket) {
      cc.log("Sent group join request", group_id);
    },
    function(error) {
      cc.error("join group failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = []()
{
  std::cout << "Sent group join request" << std::endl;
};

string group_id = "<group id>";
client->joinGroup(session, group_id, successCallback);
```

```java tab="Java"
String groupid = "<group id>";
client.joinGroup(session, groupid).get();
System.out.format("Sent group join request %s", groupid);
```

```swift tab="Swift"
// Requires Nakama 1.x
let groupID
var message = GroupJoinMessage()
message.groupIds.append(groupID)
client.send(message: message).then { _ in
  NSLog("Requested to join group.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```tab="REST"
POST /v2/group/<group id>/join
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

The user will receive an [in-app notification](social-in-app-notifications.md) when they've been added to the group. In a private group an admin or superadmin will receive a notification when a user has requested to join.

## List a user's groups

Each user can list groups they've joined as a member or an admin or a superadmin. The list also contains groups which they've requested to join but not been accepted into yet.

```sh tab="cURL"
curl -X GET "http://127.0.0.1:7350/v2/user/<user id>/group" \
  -H 'Authorization: Bearer <session token>'
```

```js tab="JavaScript"
const userId = "<user id>";
const groups = await client.listUserGroups(session, userid);
groups.user_groups.forEach(function(userGroup){
  console.log("Group: name '%o' id '%o'.", userGroup.group.name, userGroup.group.id);
  // group.State is one of: SuperAdmin, Admin, Member, or Join.
  console.log("Group's state is %o.", userGroup.state);
});
```

```csharp tab=".NET"
const string userId = "<user id>";
var result = await client.ListUserGroupsAsync(session, userId);
foreach (var ug in result.UserGroups)
{
    var g = ug.Group;
    System.Console.WriteLine("Group '{0}' role '{1}'", g.Id, ug.State);
}
```

```csharp tab="Unity"
const string userId = "<user id>";
var result = await client.ListUserGroupsAsync(session, userId);
foreach (var ug in result.UserGroups)
{
    var g = ug.Group;
    Debug.LogFormat("Group '{0}' role '{1}'", g.Id, ug.State);
}
```

```cpp tab="Cocos2d-x C++"
auto successCallback = [](NUserGroupListPtr list)
{
  for (auto& userGroup : list->userGroups)
  {
    CCLOG("Group name %s", userGroup.group.name.c_str());
  }
};

string userId = "<user id>";
client->listUserGroups(session, userId, {}, {}, {}, successCallback);
```

```js tab="Cocos2d-x JS"
const userId = "<user id>";
client.listUserGroups(session, userid)
  .then(function(groups) {
      groups.user_groups.forEach(function(userGroup){
        cc.log("Group name", userGroup.group.name);
        // group.State is one of: SuperAdmin, Admin, Member, or Join.
        cc.log("Group's state is", userGroup.state);
      })
    },
    function(error) {
      cc.error("list user groups failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = [](NUserGroupListPtr list)
{
  for (auto& userGroup : list->userGroups)
  {
    std::cout << "Group name " << userGroup.group.name << std::endl;
  }
};

string userId = "<user id>";
client->listUserGroups(session, userId, {}, {}, {}, successCallback);
```

```java tab="Java"
String userid = "<user id>";
UserGroupList userGroups = client.listUserGroups(session, userid).get();
for (UserGroupList.UserGroup userGroup : userGroups.getUserGroupsList()) {
  System.out.format("Group name %s role %d", userGroup.getGroup().getName(), userGroup.getState());
}
```

```swift tab="Swift"
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

```tab="REST"
GET /v2/user/<user id>/group
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

## List group members

A user can list all members who're part of their group. These include other users who've requested to join the private group but not been accepted into yet.

```sh tab="cURL"
curl -X GET "http://127.0.0.1:7350/v2/group/<group id>/user" \
  -H 'Authorization: Bearer <session token>'
```

```js tab="JavaScript"
const group_id = "<group id>";
const users = await client.listGroupUsers(session, group_id);
console.info("Users in group:", users);
```

```csharp tab=".NET"
const string groupId = "<group id>";
var result = await client.ListGroupUsersAsync(session, groupId);
foreach (var ug in result.UserGroups)
{
  var g = ug.Group;
  System.Console.WriteLine("group '{0}' role '{1}'", g.Id, ug.State);
}
```

```csharp tab="Unity"
const string groupId = "<group id>";
var result = await client.ListGroupUsersAsync(session, groupId);
foreach (var ug in result.UserGroups)
{
    var g = ug.Group;
    Debug.LogFormat("group '{0}' role '{1}'", g.Id, ug.State);
}
```

```cpp tab="Cocos2d-x C++"
auto successCallback = [](NGroupUserListPtr list)
{
  CCLOG("Users in group: %u", list->groupUsers.size());
};

string group_id = "<group id>";
client->listGroupUsers(session, group_id, {}, {}, {}, successCallback);
```

```js tab="Cocos2d-x JS"
const group_id = "<group id>";
client.listGroupUsers(session, group_id)
  .then(function(users) {
      cc.log("Users in group:", JSON.stringify(users));
    },
    function(error) {
      cc.error("list group users failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = [](NGroupUserListPtr list)
{
  std::cout << "Users in group: " << list->groupUsers << std::endl;
};

string group_id = "<group id>";
client->listGroupUsers(session, group_id, {}, {}, {}, successCallback);
```

```java tab="Java"
String groupid = "<group id>";
GroupUserList groupUsers = client.listGroupUsers(session, groupid).get();
for (GroupUserList.GroupUser groupUser : groupUsers.getGroupUsersList()) {
  System.out.format("Username %s role %d", groupUser.getUser().getUsername(), groupUser.getState());
}
```

```swift tab="Swift"
// Requires Nakama 1.x
let groupID = "<group id>"
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

```tab="REST"
GET /v2/group/<group id>/user
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

## Create a group

A group can be created with a name and other optional fields. These optional fields are used when a user [lists and filter groups](#list-and-filter-groups). The user who creates the group becomes the owner and a superadmin for it.

```sh tab="cURL"
curl -X POST "http://127.0.0.1:7350/v2/group" \
  -H 'Authorization: Bearer <session token>' \
  -d '{
    "name": "pizza-lovers",
    "description": "pizza lovers, pineapple haters",
    "lang_tag": "en_US",
    "open": true
  }'
```

```js tab="JavaScript"
const group_name = "pizza-lovers";
const description = "pizza lovers, pineapple haters";
const group = await client.createGroup(session, {
  name: group_name,
  description: description,
  lang_tag: "en_US",
  open: true
});
console.info("New group:", group);
```

```csharp tab=".NET"
const string name = "pizza-lovers";
const string desc = "pizza lovers, pineapple haters";
var group = await client.CreateGroupAsync(session, name, desc);
System.Console.WriteLine("New group: {0}", group);
```

```csharp tab="Unity"
const string name = "pizza-lovers";
const string desc = "pizza lovers, pineapple haters";
var group = await client.CreateGroupAsync(session, name, desc);
Debug.LogFormat("New group: {0}", group);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = [](const NGroup& group)
{
  CCLOG("New group ID: %s", group.id.c_str());
};

string group_name = "pizza-lovers";
string description = "pizza lovers, pineapple haters";
client->createGroup(session,
    group_name,
    description,
    "",  // avatar URL
    "en_US",
    true, // open
    {},
    successCallback);
```

```js tab="Cocos2d-x JS"
const group_name = "pizza-lovers";
const description = "pizza lovers, pineapple haters";
client.createGroup(session, {
  name: group_name,
  description: description,
  lang_tag: "en_US",
  open: true
}).then(function(group) {
    cc.log("New group:", JSON.stringify(group));
  },
  function(error) {
    cc.error("create group failed:", JSON.stringify(error));
  });
```

```cpp tab="C++"
auto successCallback = [](const NGroup& group)
{
  std::cout << "New group ID: " << group.id << std::endl;
};

string group_name = "pizza-lovers";
string description = "pizza lovers, pineapple haters";
client->createGroup(session,
    group_name,
    description,
    "",  // avatar URL
    "en_US",
    true, // open
    {},
    successCallback);
```

```java tab="Java"
String name = "pizza-lovers";
String desc = "pizza lovers, pineapple haters";
Group group = client.createGroup(session, name, desc).get();
System.out.format("New group %s", group.getId());
```

```swift tab="Swift"
// Requires Nakama 1.x
var groupCreate = GroupCreate("pizza-lovers")
groupCreate.description = "pizza lovers, pineapple haters"
groupCreate.lang = "en_US"
groupCreate.privateGroup = true
groupCreate.avatarURL = "url://somelink"
groupCreate.metadata = "{'my_custom_field': 'some value'}".data(using: .utf8)!

var message = GroupCreateMessage()
message.groupsCreate.append(groupCreate)
client.send(message: message).then { groups in
  for group in groups {
    NSLog("New group: id '%@' with name '%@", group.id, group.name)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```tab="REST"
POST /v2/group
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{
  "name": "pizza-lovers",
  "description": "pizza lovers, pineapple haters",
  "lang_tag": "en_US",
  "open": true
}
```

You can also create a group with server-side code. This can be useful when the group must be created together with some other record or feature.

```lua tab="Lua"
local nk = require("nakama")

local metadata = { -- Add whatever custom fields you want.
    my_custom_field = "some value"
}
local user_id = "<user id>"
local name = "pizza-lovers"
local creator_id = user_id
local lang = "en_US"
local description = "pizza lovers, pineapple haters"
local avatar_url = "url://somelink"
local open = true
local maxMemberCount = 100

local success, err = pcall(nk.group_create, user_id, name, creator_id, lang,
    description, avatar_url, open, metadata, maxMemberCount)
if (not success) then
    nk.logger_error(("Could not create group: %q"):format(err))
end
```

```go tab="Go"
metadata := map[string]interface{}{
  "my_custom_field": "some value" // Add whatever custom fields you want.
}

userID := "<user id>"
creatorID := userID
name := "pizza-lovers"
description := "pizza lovers, pineapple haters"
langTag := "en"
open := true
avatarURL := "url://somelink"
maxCount := 100

if _, err := nk.GroupCreate(ctx, userID, name, creatorID, langTag, description, avatarURL, open, metadata, maxCount); err != nil {
	logger.Error("Could not create group: %s", err.Error())
}
```

## Update a group

When a group has been created it's admins can update optional fields.

```sh tab="cURL"
curl -X PUT "http://127.0.0.1:7350/v2/group/<group id>" \
  -H 'Authorization: Bearer <session token>' \
  -d '{
    "description": "Better than Marvel Heroes!",
  }'
```

```js tab="JavaScript"
const group_id = "<group id>";
const description = "Better than Marvel Heroes!";
const group = await client.updateGroup(session, group_id, { description: description });
console.info("Updated group:", group);
```

```csharp tab=".NET"
const string groupId = "<group id>";
const string desc = "Better than Marvel Heroes!";
var group = await client.UpdateGroupAsync(session, groupId, null, desc);
System.Console.WriteLine("Updated group: {0}", group);
```

```csharp tab="Unity"
const string groupId = "<group id>";
const string desc = "Better than Marvel Heroes!";
var group = await client.UpdateGroupAsync(session, groupId, null, desc);
Debug.LogFormat("Updated group: {0}", group);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = []()
{
  CCLOG("Updated group");
};

string group_id = "<group id>";
string description = "Better than Marvel Heroes!";
client->updateGroup(session,
    group_id,
    opt::nullopt,
    description,
    opt::nullopt,
    opt::nullopt,
    opt::nullopt,
    successCallback);
```

```js tab="Cocos2d-x JS"
const group_id = "<group id>";
const description = "Better than Marvel Heroes!";
client.updateGroup(session, group_id, { description: description })
  .then(function(group) {
      cc.log("Updated group:", JSON.stringify(group));
    },
    function(error) {
      cc.error("update group failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = []()
{
  std::cout << "Updated group" << std::endl;
};

string group_id = "<group id>";
string description = "Better than Marvel Heroes!";
client->updateGroup(session,
    group_id,
    opt::nullopt,
    description,
    opt::nullopt,
    opt::nullopt,
    opt::nullopt,
    successCallback);
```

```java tab="Java"
String groupid = "<group id>";
String desc = "Better than Marvel Heroes!";
client.updateGroup(session, groupid, null, desc).get();
System.out.format("Updated group %s", groupid);
```

```swift tab="Swift"
// Requires Nakama 1.x
let groupID = "<group id>"
var groupUpdate = GroupUpdate(groupID)
groupUpdate.description = "I was only kidding. Basil sauce ftw!"

var message = GroupUpdateMessage()
message.groupsUpdate.append(groupUpdate)
client.send(message: message).then { _ in
  NSLog("Successfully updated group.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```tab="REST"
PUT /v2/group/<group id>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{
  "description": "I was only kidding. Basil sauce ftw!",
}
```

## Leave a group

A user can leave a group and will no longer be able to join [group chat](social-realtime-chat.md#groups) or read [message history](social-realtime-chat.md#message-history). If the user is a superadmin they will only be able to leave when at least one other superadmin exists in the group.

!!! Note
    Any user who leaves the group will generate an event message in group chat which other members can read.

```sh tab="cURL"
curl -X POST "http://127.0.0.1:7350/v2/group/<group id>/leave" \
  -H 'Authorization: Bearer <session token>'
```

```js tab="JavaScript"
const group_id = "<group id>";
await client.leaveGroup(session, group_id);
```

```csharp tab=".NET"
const string groupId = "<group id>";
await client.LeaveGroupAsync(session, groupId);
```

```csharp tab="Unity"
const string groupId = "<group id>";
await client.LeaveGroupAsync(session, groupId);
```

```cpp tab="Cocos2d-x C++"
string group_id = "<group id>";
client->leaveGroup(session, group_id);
```

```js tab="Cocos2d-x JS"
const group_id = "<group id>";
client.leaveGroup(session, group_id);
```

```cpp tab="C++"
string groupId = "<group id>";
client->leaveGroup(session, groupId);
```

```java tab="Java"
String groupId = "<group id>";
client.leaveGroup(session, groupId).get();
```

```swift tab="Swift"
// Requires Nakama 1.x
let groupID = "<group id>"
var message = GroupLeaveMessage()
message.groupIds.append(groupID)
client.send(message: message).then { _ in
  NSLog("Left the group.")
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

```tab="REST"
POST /v2/group/<group id>/leave
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

## Manage groups

Each group is managed by one or more superadmins or admins. These users are members with permission to make changes to optional fields, accept or reject new members, remove members or other admins, and promote other members as admins.

!!! Warning
    A group must have at least one superadmin. The last superadmin has to promote another member before they can [leave](#leave-a-group).

### Accept new members

When a user joins a private group it will create a join request until an admin accepts or rejects the user. The superadmin or admin can accept the user into the group.

```sh tab="cURL"
curl -X POST "http://127.0.0.1:7350/v2/group/<group id>/add" \
  -H 'Authorization: Bearer <session token>' \
  -d '{
    "user_ids":["<user id>"]
  }'
```

```js tab="JavaScript"
const group_id = "<group id>";
const user_id = "<user id>";
await client.addGroupUsers(session, group_id, [user_id]);
```

```csharp tab=".NET"
const string groupId = "<group id>";
var userIds = new[] {"<user id>"};
await client.AddGroupUsersAsync(session, groupId, userIds);
```

```csharp tab="Unity"
const string groupId = "<group id>";
var userIds = new[] {"<user id>"};
await client.AddGroupUsersAsync(session, groupId, userIds);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = []()
{
  CCLOG("added user to group");
};

string group_id = "<group id>";
string user_id = "<user id>";
client->addGroupUsers(session, group_id, { user_id }, successCallback);
```

```js tab="Cocos2d-x JS"
const group_id = "<group id>";
const user_id = "<user id>";
client.addGroupUsers(session, group_id, [user_id]);
```

```cpp tab="C++"
auto successCallback = []()
{
  std::cout << "added user to group" << std::endl;
};

string group_id = "<group id>";
string user_id = "<user id>";
client->addGroupUsers(session, group_id, { user_id }, successCallback);
```

```java tab="Java"
String groupid = "<group id>";
String[] userIds = new String[] {"<user id>"};
client.addGroupUsers(session, groupid, userIds).get();
```

```swift tab="Swift"
// Requires Nakama 1.x
let groupID = "<group id>"
let userID = "<user id>"
// A tuple that represents which group the user is added to
let addUser = (groupID: groupID, userID: userID)

var message = GroupAddUserMessage()
message.groupUsers.append(addUser)
client.send(message: message).then { _ in
  NSLog("Added user to group.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```tab="REST"
POST /v2/group/<group id>/add
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{
  "user_ids":["<user id>"]
}
```

The user will receive an [in-app notification](social-in-app-notifications.md) when they've been added to the group. In a private group an admin will receive a notification about the join request.

To reject the user from joining the group you should [kick them](#kick-a-member).

### Promote a member

An admin can promote another member of the group as an admin. This grants the member the same privileges to [manage the group](#manage-groups). A group can have one or more admins.

```sh tab="cURL"
curl -X POST "http://127.0.0.1:7350/v2/group/<group id>/promote" \
  -H 'Authorization: Bearer <session token>' \
  -d '{
    "user_ids":["<user id>"]
  }'
```

```js tab="JavaScript"
const group_id = "<group id>";
const user_id = "<user id>";
await client.promoteGroupUsers(session, group_id, [user_id]);
```

```csharp tab=".NET"
const string groupId = "<group id>";
var userIds = new[] {"<user id>"};
await client.PromoteGroupUsersAsync(session, groupId, userIds);
```

```csharp tab="Unity"
const string groupId = "<group id>";
var userIds = new[] {"<user id>"};
await client.PromoteGroupUsersAsync(session, groupId, userIds);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = []()
{
  CCLOG("user has been promoted");
};

string group_id = "<group id>";
string user_id = "<user id>";
client->promoteGroupUsers(session, group_id, { user_id }, successCallback);
```

```js tab="Cocos2d-x JS"
const group_id = "<group id>";
const user_id = "<user id>";
client.promoteGroupUsers(session, group_id, [user_id]);
```

```cpp tab="C++"
auto successCallback = []()
{
  std::cout << "user has been promoted" << std::endl;
};

string group_id = "<group id>";
string user_id = "<user id>";
client->promoteGroupUsers(session, group_id, { user_id }, successCallback);
```

```java tab="Java"
String groupid = "<group id>";
String[] userIds = new String[] {"<user id>"};
client.promoteGroupUsers(session, groupid, userIds).get();
```

```swift tab="Swift"
// Requires Nakama 1.x
let groupID = "<group id>"
let userID = "<user id>"
let promoteUser = (groupID: groupID, userID: userID)
var message = GroupPromoteUserMessage()
message.groupUsers.append(promoteUser)
client.send(message: message).then { _ in
  NSLog("Promoted user as an admin.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```tab="REST"
POST /v2/group/<group id>/promote
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{
  "user_ids":["<user id>"]
}
```

To demote an admin as a member you can [kick](#kick-a-member) and [re-add](#accept-new-members) them.

### Kick a member

An admin or superadmin can kick a member from the group. The user is removed but can rejoin again later unless the group is private in which case an admin must accept the join request.

If a user is removed from a group it does not prevent them from joining other groups.

```sh tab="cURL"
curl -X POST "http://127.0.0.1:7350/v2/group/<group id>/kick" \
  -H 'Authorization: Bearer <session token>' \
  -d '{
    "user_ids":["<user id>"]
  }'
```

```js tab="JavaScript"
const group_id = "<group id>";
const user_id = "<user id>";
await client.kickGroupUsers(session, group_id, [user_id]);
```

```csharp tab=".NET"
const string groupId = "<group id>";
var userIds = new[] {"<user id>"};
await client.KickGroupUsersAsync(session, groupId, userIds);
```

```csharp tab="Unity"
const string groupId = "<group id>";
var userIds = new[] {"<user id>"};
await client.KickGroupUsersAsync(session, groupId, userIds);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = []()
{
  CCLOG("user has been kicked");
};

string group_id = "<group id>";
string user_id = "<user id>";
client->kickGroupUsers(session, group_id, { user_id }, successCallback);
```

```js tab="Cocos2d-x JS"
const group_id = "<group id>";
const user_id = "<user id>";
client.kickGroupUsers(session, group_id, [user_id]);
```

```cpp tab="C++"
auto successCallback = []()
{
  std::cout << "user has been kicked" << std::endl;
};

string group_id = "<group id>";
string user_id = "<user id>";
client->kickGroupUsers(session, group_id, { user_id }, successCallback);
```

```java tab="Java"
String groupid = "<group id>";
String[] userIds = new String[] {"<user id>"};
client.kickGroupUsers(session, groupid, userIds).get();
```

```swift tab="Swift"
// Requires Nakama 1.x
let groupID = "<group id>"
let userID = "<user id>"
let groupUser = (groupID: groupID, userID: userID)
var message = GroupKickUserMessage()
message.groupUsers.append(groupUser)
client.send(message: message).then { _ in
  NSLog("Kicked user from group.");
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```tab="REST"
POST /v2/group/<group id>/kick
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{
  "user_ids":["<user id>"]
}
```

!!! Hint
    Sometimes a bad user needs to be kicked from the group and [permanently banned](social-friends.md#ban-a-user). This will prevent the user from being able to connect to the server and interact at all.

## Remove a group

A group can only be removed by one of the superadmins which will disband all members. When a group is removed it's name can be re-used to [create a new group](#create-a-group).

```sh tab="cURL"
curl -X DELETE "http://127.0.0.1:7350/v2/group/<group id>" \
  -H 'Authorization: Bearer <session token>'
```

```js tab="JavaScript"
const group_id = "<group id>";
await client.deleteGroup(session, group_id);
```

```csharp tab=".NET"
const string groupId = "<group id>";
await client.DeleteGroupAsync(session, groupId);
```

```csharp tab="Unity"
const string groupId = "<group id>";
await client.DeleteGroupAsync(session, groupId);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = []()
{
  CCLOG("group deleted");
};

string group_id = "<group id>";
client->deleteGroup(session, group_id, successCallback);
```

```js tab="Cocos2d-x JS"
const group_id = "<group id>";
client.deleteGroup(session, group_id);
```

```cpp tab="C++"
auto successCallback = []()
{
  std::cout << "group deleted" << std::endl;
};

string group_id = "<group id>";
client->deleteGroup(session, group_id, successCallback);
```

```java tab="Java"
String groupid = "<group id>";
client.deleteGroup(session, groupid).get();
```

```swift tab="Swift"
// Requires Nakama 1.x
let groupID = "<group id>"
var message = GroupRemoveMessage()
message.groupIds.append(groupID)
client.send(message: message).then { _ in
  NSLog("The group has been removed.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```tab="REST"
DELETE /v2/group/<group id>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```
