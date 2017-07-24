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
    var id = Encoding.UTF8.GetString(group.Id);  // convert byte[].
    Debug.LogFormat("Group: name '{0}' id '{1}'.", group.Name, id);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
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
        var id = Encoding.UTF8.GetString(group.Id);  // convert byte[].
        Debug.LogFormat("Group: name '{0}' id '{1}'.", group.Name, id);
      }
    }, errorHandler);
  }
}, errorHandler);
```

## Join groups

When a user has found a group to join they can request to become a member. A public group can be joined without any need for permission while a private group requires an [admin to accept](#accept-new-members) the user.

A user who's part of a group can join [group chat](realtime-chat.md#groups) and access it's [message history](realtime-chat.md#message-history).

!!! Tip
    When a user joins or leaves a group event messages are added to chat history. This makes it easy for members to see what's changed in the group.

```csharp fct_label="Unity"
byte[] groupId = group.Id; // an INGroup ID.

var message = NGroupJoinMessage.Default(groupId);
client.Send(message, (bool done) => {
  Debug.Log("Requested to join group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

The user will receive an [in-app notification](in-app-notifications.md) when they've been added to the group. In a private group an admin will receive a notification when a user has requested to join.

## List a user's groups

Each user can list groups they've joined as a member or an admin. The list also contains groups which they've requested to join but not been accepted into yet.

```csharp fct_label="Unity"
var message = NGroupsSelfListMessage.Default();
client.Send(message, (INResultSet<INGroup> list) => {
  foreach (var group in list.Results) {
    var id = Encoding.UTF8.GetString(group.Id);  // convert byte[].
    Debug.LogFormat("Group: name '{0}' id '{1}'.", group.Name, id);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

## List group members

A user can list all members who're part of their group. These include other users who've requested to join the private group but not been accepted into yet.

```csharp fct_label="Unity"
byte[] groupId = group.Id; // an INGroup ID.

var message = NGroupUsersListMessage.Default(groupId);
client.Send(message, (INResultSet<INGroupUser> list) => {
  foreach (var member in list.Results) {
    var id = Encoding.UTF8.GetString(member.Id);  // convert byte[].
    Debug.LogFormat("Member id '{0}' with name '{1}'.", id, member.Fullname);
    // member.Type is one of: Admin, Member, Join.
    UserType status = member.Type;
    Debug.LogFormat("Has handle '{0}' with status '{1}'.", member.Handle, status);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
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
  var id = Encoding.UTF8.GetString(group.Id);  // convert byte[].
  Debug.LogFormat("New group: name '{0}' id '{1}'.", group.Name, id);
  Debug.Log ("Successfully created a private group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
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
byte[] groupId = group.Id; // an INGroup ID.

var message = new NGroupUpdateMessage.Builder(groupId)
    .Description("A new group description.")
    .Build();
client.Send(message, (bool done) => {
  Debug.Log("Successfully updated group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

## Leave a group

A user can leave a group and will no longer be able to join [group chat](realtime-chat.md#groups) or read [message history](realtime-chat.md#message-history). If the user is an admin they will only be able to leave when at least one other admin exists in the group.

!!! Note
    Any user who leaves the group will generate an event message in group chat which other members can read.

```csharp fct_label="Unity"
byte[] groupId = group.Id; // an INGroup ID.

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

## Manage groups

Each group is managed by one or more admins. These users are members with permission to make changes to optional fields, accept or reject new members, remove members or other admins, and promote other members as admins.

!!! Warning
    A group must have at least one admin so the last admin will have to promote another member before they can [leave](#leave-a-group).

### Accept new members

When a user joins a private group it will create a join request until an admin accepts or rejects the user. The admin can accept the user into the group.

```csharp fct_label="Unity"
byte[] groupId = group.Id; // an INGroup ID.
byte[] userId = user.Id;   // an INUser ID.

var message = NGroupAddUserMessage.Default(groupId, userId);
client.Send(message, (bool done) => {
  Debug.Log("Successfully added user to group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

The user will receive an [in-app notification](in-app-notifications.md) when they've been added to the group. In a private group an admin will receive a notification about the join request.

To reject the user from joining the group you should [kick them](#kick-a-member).

### Promote a member

An admin can promote another member of the group as an admin. This grants the member the same privileges to [manage the group](#manage-groups). A group can have one or more admins.

```csharp fct_label="Unity"
byte[] groupId = group.Id; // an INGroup ID.
byte[] userId = user.Id;   // an INUser ID.

var message = NGroupPromoteUserMessage.Default(groupId, userId);
client.Send(message, (bool done) => {
  Debug.Log("Successfully promoted user as an admin.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

To demote an admin as a member you can kick and re-add them.

```csharp fct_label="Unity"
var errorHandler = delegate(INError err) {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

byte[] groupId = group.Id; // an INGroup ID.
byte[] userId = user.Id;   // an INUser ID.

var kickMessage = NGroupKickUserMessage.Default(groupId, userId);
client.Send(kickMessage, (bool completed) => {
  var addMessage = NGroupAddUserMessage.Default(groupId, userId);
  client.Send(addMessage, (bool done) => {
    Debug.Log("Admin user demoted to member in group.");
  }, errorHandler);
}, errorHandler);
```

### Kick a member

An admin can kick a member from the group. The user is removed but can rejoin again later unless the group is private in which case an admin must accept the join request.

If a user is removed from a group it does not prevent them from joining other groups.

```csharp fct_label="Unity"
byte[] groupId = group.Id; // an INGroup ID.
byte[] userId = user.Id;   // an INUser ID.

var message = NGroupKickUserMessage.Default(groupId, userId);
client.Send(message, (bool done) => {
  Debug.Log ("Successfully kicked user from group.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

!!! Hint
    Sometimes a bad user needs to be kicked from the group and [permanently banned](friends.md#ban-a-user). This will prevent the user from being able to connect to the server and interact at all.

## Remove a group

A group can only be removed by one of the admins which will disband all members. When a group is removed it's name can be re-used to [create a new group](#create-a-group).

```csharp fct_label="Unity"
byte[] groupId = group.Id; // an INGroup ID.

var message = NGroupRemoveMessage.Default(groupId);
client.Send(message, (bool done) => {
  Debug.Log("The group has been removed.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```
