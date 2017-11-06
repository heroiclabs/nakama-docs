# Groups and Clans

A group brings together a bunch of users into a small community or team.

A group is made up of an owner, admins, and members. It can be public or private which determines whether it appears in results when a user searches. Private groups are similar to how Whatsapp groups work. A user can only be added when they're invited to join by one of the group's admins.

## Search for groups

A user can find public groups to join by a filter on language, recently created, how many members in a group, and more. These filters make it easy to assemble new users into smaller groups for team-based play or collaboration.

```csharp fct_label="Unity"
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
var message = GroupsListMessage()
message.lang = "en"
message.orderAscending = true
client.send(message: message).then { groups in
  for group in groups {
    NSLog("Group: name '%@' id '%@'.", group.name, group.id)
  }
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var message = new nakamajs.GroupsListRequest();
message.lang = "en"
message.orderByAsc = true;
client.send(message).then(function(groups) {
  groups.forEach(function(group) {
    console.log("Group: name '%o' id '%o'.", group.name, group.id);
  })
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

The message response for a list of groups contains a cursor. The cursor can be used to quickly retrieve the next set of results.

!!! tip
    Cursors are used across different server features to page through batches of results quickly and efficiently. It's used with storage, friends, chat history, etc.

```csharp fct_label="Unity"
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
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var message = new nakamajs.GroupsListRequest();
message.lang = "en"
message.orderByAsc = true;
client.send(message).then(function(groups) {
  if (groups.length > 0 && groups.cursor) {
    message.cursor = groups.cursor
    client.send(message).then(function(nextGroups) {
      // ...
    }).catch(function (error) {
      throw error
    });
  }
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

## Join groups

When a user has found a group to join they can request to become a member. A public group can be joined without any need for permission while a private group requires an [admin to accept](#accept-new-members) the user.

A user who's part of a group can join [group chat](social-realtime-chat.md#groups) and access it's [message history](social-realtime-chat.md#message-history).

!!! Tip
    When a user joins or leaves a group event messages are added to chat history. This makes it easy for members to see what's changed in the group.

```csharp fct_label="Unity"
string groupId = group.Id; // an INGroup ID.

var message = NGroupJoinMessage.Default(groupId);
client.Send(message, (bool done) => {
  Debug.Log("Requested to join group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
let groupID // a group ID
var message = GroupJoinMessage()
message.groupIds.append(groupID)
client.send(message: message).then { _ in
  NSLog("Requested to join group.")
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var groupId; // a group ID
var message = new nakamajs.GroupsJoinRequest();
message.groups.push(groupId);
client.send(message).then(function() {
  console.log("Requested to join group.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

The user will receive an [in-app notification](social-in-app-notifications.md) when they've been added to the group. In a private group an admin will receive a notification when a user has requested to join.

## List a user's groups

Each user can list groups they've joined as a member or an admin. The list also contains groups which they've requested to join but not been accepted into yet.

```csharp fct_label="Unity"
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
var message = GroupsSelfListMessage()
client.send(message: message).then { groups in
  for group in groups {
    NSLog("Group: name '%@' id '%@'", group.name, group.id)
    // group.State is one of: Admin, Member, or Join.
    NSLog("Group's state is '%d'", group.state.rawValue)
  }
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var message = new nakamajs.GroupsSelfListRequest();
client.send(message).then(function(groups) {
  groups.forEach(function(group){
    console.log("Group: name '%o' id '%o'.", group.name, group.id);
    // group.State is one of: Admin, Member, or Join.
    console.log("Group's state is %o.", group.state);
  })
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

## List group members

A user can list all members who're part of their group. These include other users who've requested to join the private group but not been accepted into yet.

```csharp fct_label="Unity"
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
let groupID = ... // a GroupId
var message = GroupUsersListMessage(groupID)
client.send(message: message).then { users in
  for user in users {
    NSLog("Member id '%@' with name '%@", member.fullname, member.id)
    // member.state is one of: Admin, Member, or Join.
    NSLog("Member's state is '%d'", member.state.rawValue)
  }
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var groupId = ... // a GroupId
var message = new nakamajs.GroupUsersListRequest(groupId);
client.send(message).then(function(users) {
  users.forEach(function(user){
    console.log("Member: name '%o' id '%o'.", member.fullname, member.id);
    // member.state is one of: Admin, Member, or Join.
    console.log("Member's state is %o.", member.state);
  })
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

## Create a group

A group can be created with a name and other optional fields. These optional fields are used when a user [searches for groups](#search-for-groups). The user who creates the group becomes the owner and an admin for it.

```csharp fct_label="Unity"
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
var groupCreate = GroupCreate("Some unique group name")
groupCreate.description = "My awesome group."
groupCreate.lang = "en"
groupCreate.privateGroup = true
groupCreate.avatarURL = "url://somelink"

var message = GroupCreateMessage()
message.groupsCreate.append(groupCreate)
client.send(message: message).then { group in
  NSLog("New group: id '%@' with name '%@", group.id, group.name)
  NSLog("Successfully created a private group.")
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var metadata = {'my_custom_field': 'some value'};
var message = new nakamajs.GroupsCreateRequest();
message.create("Some unique group name", "My awesome group.", "url://somelink", "en", metadata, true);
client.send(message).then(function(group) {
  console.log("New group: id '%@' with name '%@", group.id, group.name);
  console.log("Successfully created a private group.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

You can also create a group with server-side code. This can be useful when the group must be created together with some other record or feature.

```lua
local nk = require("nakama")

local metadata = { -- Add whatever custom fields you want.
  my_custom_field = "some value"
}
local group = {
  Name = "Some unique group name",
  Description = "My awesome group.",
  Lang = "en",
  Private = true,
  CreatorId = "4c2ae592-b2a7-445e-98ec-697694478b1c",
  AvatarUrl = "url://somelink",
  Metadata = metadata
}
local new_groups = { group }

local success, err = pcall(nk.groups_create, new_groups)
if (not success) then
  nk.logger_error(("Error with groups create: %q"):format(err))
end
```

## Update a group

When a group has been created it's admins can update optional fields.

```csharp fct_label="Unity"
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
let groupID = ... // an INGroup ID.

var groupUpdate = GroupUpdate(groupID)
groupUpdate.description = "A new group description."

var message = GroupUpdateMessage()
message.groupsUpdate.append(groupUpdate)
client.send(message: message).then { _ in
  NSLog("Successfully updated group.")
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var groupId = ... // an INGroup ID.
var message = new nakamajs.GroupsUpdateRequest();
message.create(groupId, "Some unique group name", "A new group description.");
client.send(message).then(function() {
  console.log("Successfully updated group.")
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

## Leave a group

A user can leave a group and will no longer be able to join [group chat](social-realtime-chat.md#groups) or read [message history](social-realtime-chat.md#message-history). If the user is an admin they will only be able to leave when at least one other admin exists in the group.

!!! Note
    Any user who leaves the group will generate an event message in group chat which other members can read.

```csharp fct_label="Unity"
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
      NSLog("Error @% : @%", err, nkErr.message)
  }
}
```

```js fct_label="Javascript"
var groupId; // a group ID
var message = new nakamajs.GroupsLeaveRequest();
message.groups.push(groupId);
client.send(message).then(function() {
  console.log("Successfully left the group.");
}).catch(function(error) {
  // GROUP_LAST_ADMIN
  if (error.code == 12) {
    console.log("Unable to leave as last admin.");
  } else {
    console.log("An error occured: %o", error);
  }
})
```

## Manage groups

Each group is managed by one or more admins. These users are members with permission to make changes to optional fields, accept or reject new members, remove members or other admins, and promote other members as admins.

!!! Warning
    A group must have at least one admin so the last admin will have to promote another member before they can [leave](#leave-a-group).

### Accept new members

When a user joins a private group it will create a join request until an admin accepts or rejects the user. The admin can accept the user into the group.

```csharp fct_label="Unity"
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
let groupID = ... // a group ID
let userID = ... // a user Id

// A tuple that represents which group the user is added to
let addUser = (groupID: groupID, userID: userID)

var message = GroupAddUserMessage()
message.groupUsers.append(addUser)
client.send(message: message).then { _ in
  NSLog("Successfully added user to group.")
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var groupId; // a group ID
var userId; // a group ID

var message = new nakamajs.GroupUsersAddRequest();
message.add(groupId, userId)
client.send(message).then(function() {
  console.log("Successfully added user to group.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

The user will receive an [in-app notification](social-in-app-notifications.md) when they've been added to the group. In a private group an admin will receive a notification about the join request.

To reject the user from joining the group you should [kick them](#kick-a-member).

### Promote a member

An admin can promote another member of the group as an admin. This grants the member the same privileges to [manage the group](#manage-groups). A group can have one or more admins.

```csharp fct_label="Unity"
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
let groupID = ... // a group ID
let userID = ... // a user Id

let promoteUser = (groupID: groupID, userID: userID)

var message = GroupPromoteUserMessage()
message.groupUsers.append(promoteUser)
client.send(message: message).then { _ in
  NSLog("Successfully promoted user as an admin.")
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var groupId; // a group ID
var userId; // a group ID

var message = new nakamajs.GroupUsersPromoteRequest();
message.promote(groupId, userId)
client.send(message).then(function() {
  console.log("Successfully promoted user as an admin.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

To demote an admin as a member you can kick and re-add them.

```csharp fct_label="Unity"
var errorHandler = delegate(INError err) {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

string groupId = group.Id; // an INGroup ID.
string userId = user.Id;   // an INUser ID.

var kickMessage = NGroupKickUserMessage.Default(groupId, userId);
client.Send(kickMessage, (bool completed) => {
  var addMessage = NGroupAddUserMessage.Default(groupId, userId);
  client.Send(addMessage, (bool done) => {
    Debug.Log("Admin user demoted to member in group.");
  }, errorHandler);
}, errorHandler);
```

```swift fct_label="Swift"
let groupID = ... // a group ID
let userID = ... // a user Id

let groupUser = (groupID: groupID, userID: userID)

var message = GroupKickUserMessage()
message.groupUsers.append(groupUser)
client.send(message: message).then { _ -> Promise<Void> in
  var message2 = GroupAddUserMessage()
  message.groupUsers.append(groupUser)
  return client.send(message: message2)
}.then { in _
  NSLog("Admin user demoted to member in group.");
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var groupId; // a group ID
var userId; // a group ID

var message = new nakamajs.GroupUsersKickRequest();
message.kick(groupId, userId)
client.send(message).then(function() {
  message = new nakamajs.GroupUsersAddRequest();
  message.add(groupId, userId)
  return client.send(message);
}).then(function() {
  console.log("Admin user demoted to member in group.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

### Kick a member

An admin can kick a member from the group. The user is removed but can rejoin again later unless the group is private in which case an admin must accept the join request.

If a user is removed from a group it does not prevent them from joining other groups.

```csharp fct_label="Unity"
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
let groupID = ... // a group ID
let userID = ... // a user Id

let groupUser = (groupID: groupID, userID: userID)

var message = GroupKickUserMessage()
message.groupUsers.append(groupUser)
client.send(message: message).then { _ in
  NSLog("Successfully kicked user from group.");
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var groupId; // a group ID
var userId; // a group ID

var message = new nakamajs.GroupUsersKickRequest();
message.kick(groupId, userId)
client.send(message).then(function() {
  console.log("Successfully kicked user from group.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

!!! Hint
    Sometimes a bad user needs to be kicked from the group and [permanently banned](social-friends.md#ban-a-user). This will prevent the user from being able to connect to the server and interact at all.

## Remove a group

A group can only be removed by one of the admins which will disband all members. When a group is removed it's name can be re-used to [create a new group](#create-a-group).

```csharp fct_label="Unity"
string groupId = group.Id; // an INGroup ID.

var message = NGroupRemoveMessage.Default(groupId);
client.Send(message, (bool done) => {
  Debug.Log("The group has been removed.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
let groupID = ... // a group ID
var message = GroupRemoveMessage()
message.groupIds.append(groupID)
client.send(message: message).then { _ in
  NSLog("The group has been removed.")
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var groupId; // a group ID
var message = new nakamajs.GroupsRemoveRequest();
message.groups.push(groupId);
client.send(message).then(function() {
  console.log("The group has been removed.");
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```
