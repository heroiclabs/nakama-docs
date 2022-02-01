# Nakama JavaScript Client Guide

This client library guide will show you how to use the core Nakama features in **JavaScript** by showing you how to develop the Nakama specific parts (without full game logic or UI) of an [Among Us (external)](https://www.innersloth.com/games/among-us/) inspired game called Sagi-shi (Japanese for "Imposter").

<figure>
  <img src="../images/gameplay.png" alt="Sagi-shi gameplay screen">
  <figcaption>Sagi-shi gameplay</figcaption>
</figure>

## Prerequisites

Before proceeding ensure that you have:

* Installed [Nakama server](../getting-started/docker-quickstart/)
* Installed the [Nakama JavaScript SDK](#installation)

### Installation

The client is available on:

* [NPM](https://www.npmjs.com/package/@heroiclabs/nakama-js)
* [Heroic Labs GitHub Releases](https://github.com/heroiclabs/nakama-js/releases/latest)

If using NPM or Yarn just add the dependency to your `package.json` file:

```sh
yarn add "@herioclabs/nakama-js"
yarn install
```

After installing the client import it into your project:

```sh
import {Client} from "@heroiclabs/nakama-js"
```

In you main JavaScript function create a [client object](#nakama-client).

#### Updates

New versions of the Nakama JavaScript Client and the corresponding improvements are documented in the [Changelog](https://github.com/heroiclabs/nakama-js/blob/master/CHANGELOG.md).

### Asynchronous programming

Many methods of Nakama's APIs available in the JavaScript SDK are asynchronous and non-blocking.

Sagi-shi calls async methods using the `await` operator to not block the calling thread so that the game is responsive and efficient.

```js
await client.authenticateDevice("<deviceId>");
```

!!! note "Note"
    Read about [`async` functions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function) and the [`await` operator](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/await).

### Handling exceptions

Network programming requires additional safeguarding against connection and payload issues.

API calls in Sagi-shi are surrounded with a try block and a catch clause to gracefully handle errors:

```js
try {
    await client.authenticateDevice("<deviceId>");
}
catch (err) {
    console.log("Error authenticating device: %o:%o", err.statusCode, err.message);
}
```

<!--

### Handling retries

Nakama has a global or per-request `RetryConfiguration` object to control how failed failed API calls are retried. The retry pattern can be useful for authentication or background service related tasks.

Sagi-shi uses a global retry configuration to try failed API calls up to five times before outputting an error to the console.

Passing a `RetryConfiguration` object to an individual request will override any globally set configuration.

```csharp
var retryConfiguration = new Nakama.RetryConfiguration(baseDelay: 1, maxRetries: 5, delegate { System.Console.Writeline("about to retry."); });

// Configure the retry configuration globally.
client.GlobalRetryConfiguration = retryConfiguration;
var account = await client.GetAccountAsync(session);

// Alternatively, pass the retry configuration to an individual request.
var account = await client.GetAccountAsync(session, retryConfiguration);
```

Nakama APIs can take an optional `CancellationTokenSource` object which can be used to cancel requests:

```csharp
// Part of System.Threading namespace
var canceller = new CancellationTokenSource();
var account = await client.GetAccountAsync(session, retryConfiguration: null, canceller);

canceller.Cancel();
```

-->

## Getting started

Learn how to get started using the Nakama Client and Socket objects to start building Sagi-shi and your own game.


### Nakama Client

The Nakama Client connects to a Nakama Server and is the entry point to access Nakama features. It is recommended to have one client per server per game.

To create a client for Sagi-shi pass in your server connection details:

```js
var client = new nakamajs.Client("defaultkey", "127.0.0.1", 7350);
```

### Nakama Socket

The Nakama Socket is used for gameplay and real-time latency-sensitive features such as chat, parties, matches and RPCs.

From the client create a socket:

```js
const socket = client.createSocket();

var appearOnline = true;
var connectionTimeout = 30;
await socket.connect(session, appearOnline, connectionTimeout);
```

## Authentication

Nakama has many [authentication methods](../../concepts/authentication/) and supports creating [custom authentication](../../concepts/authentication/#custom) on the server.

Sagi-shi will use device and Facebook authentication, linked to the same user account so that players can play from multiple devices.

<figure>
  <img src="../images/login.png" alt="Sagi-shi login screen">
  <figcaption>Login screen and Authentication options</figcaption>
</figure>


### Device authentication

Nakama [Device Authentication](../../concepts/authentication/#device) uses the physical device's unique identifier to easily authenticate a user and create an account if one does not exist.

When using only device authentication, you don't need a login UI as the player can automatically authenticate when the game launches.

Authentication is an example of a Nakama feature accessed from a Nakama Client instance.

```js
// This import is only required with React Native
var deviceInfo = require('react-native-device-info');

var deviceId = null;
// If the user's device ID is already stored, grab that - alternatively get the System's unique device identifier.
try {
  const value = await AsyncStorage.getItem('@MyApp:deviceKey');
  if (value !== null){
    deviceId = value
  } else {
    deviceId = deviceInfo.getUniqueID();
    // Save the user's device ID so it can be retrieved during a later play session for re-authenticating.
    AsyncStorage.setItem('@MyApp:deviceKey', deviceId).catch(function(error) {
      console.log("An error occurred: %o", error);
    });
  }
} catch (error) {
  console.log("An error occurred: %o", error);
}

// Authenticate with the Nakama server using Device Authentication.
var create = true;
const session = await client.authenticateDevice(deviceId, create, "mycustomusername");
console.info("Successfully authenticated:", session);
```

### Facebook authentication

Nakama [Facebook Authentication](../../concepts/authentication/#facebook) is an easy to use authentication method which lets you optionally import the player's Facebook friends and add them to their Nakama Friends list.

!!! note "Note"
    Install the [Facebook SDK for JavaScript (external)](https://developers.facebook.com/docs/javascript/) to use Nakama Facebook Authentication.

```js
const oauthToken = "<token>";
const importFriends = true;
try {
    const session = await client.authenticateFacebook(oauthToken, true, "mycustomusername", importFriends);
    console.log("Successfully authenticated:", session);
}
catch(err) {
    console.log("Error authenticating with Facebook: %o", err.message);
}
```

### Custom authentication

Nakama supports [Custom Authentication](../../concepts/authentication/#custom) methods to integrate with additional identity services.

See the [Itch.io custom authentication](../server-framework/recipes/itch-authentication) recipe for an example.


### Linking authentication

Nakama allows players to [Link Authentication](../../concepts/authentication/#link-or-unlink) methods to their account once they have authenticated.

<!-- mockup Sagi-shi player account screen with linked authentication methods -->


**Linking Device ID authentication**

```js
// Acquiring the unique device ID has been shortened for brevity, see previous example.
var deviceId = "<uniqueDeviceId>";

// Link Device Authentication to existing player account.
try {
    await client.linkDevice(session, deviceId);
    console.log("Successfully linked Device ID authentication to existing player account");
}
catch(err) {
    console.log("Error linking Device ID: %o", err.message);
}
```

**Linking Facebook authentication**

```js
const oauthToken = "<token>";
const import = true;
try {
    const session = await client.linkFacebook(session, oauthToken, true, import);
    console.log("Successfully linked Facebook authentication to existing player account");
}
catch(err) {
    console.log("Error authenticating with Facebook: %o", err.message);
}
```

### Session variables

Nakama [Session Variables](../../concepts/session/#session-variables) can be stored when authenticating and will be available on the client and server as long as the session is active.

Sagi-shi uses session variables to implement analytics, referral and rewards programs and more.

Store session variables by passing them as an argument when authenticating:

```js
const vars = {
  deviceId = localStorage.getItem("deviceId"),
  deviceOs = localStorage.getItem("deviceOs"),
  inviteUserId = "<someUserId>",
  // ...
}

const session = await client.authenticateDevice(deviceId, null, true, vars);
```

To access session variables on the Client use the `vars` property on the `session` object:

```js
var deviceOs = session.vars["deviceOs"];
```


### Session lifecycle

Nakama [Sessions](../../concepts/session/) expire after a time set in your server [configuration](../getting-started/configuration/#session). Expiring inactive sessions is a good security practice.

Nakama provides ways to restore sessions, for example when Sagi-shi players re-launch the game, or refresh tokens to keep the session active while the game is being played.

Use the auth and refresh tokens on the session object to restore or refresh sessions.

Store the tokens for use later:

```js
var authToken = session.token;
var refreshToken = session.refresh_token;
```

Restore a session without having to re-authenticate:

```js
session = session.restore(authToken, refreshToken);
```

Check if a session has expired or is close to expiring and refresh it to keep it alive:

```js
// Check whether a session has expired or is close to expiry.
if (session.isexpired || session.isexpired(Date.now + 1) {
    try {
        // Attempt to refresh the existing session.
        session = await client.sessionRefresh(session);
    } catch (error) {
        // Couldn't refresh the session so reauthenticate.
        session = await client.authenticateDevice(deviceId);
        var refreshToken = session.refresh_token;
    }

    var authToken = session.token;
}
```


### Ending sessions

Logout and end the current session:

```js
await client.sessionLogout(session);
```


## User accounts

Nakama [User Accounts](../../concepts/user-accounts/) store user information defined by Nakama and custom developer metadata.

Sagi-shi allows players to edit their accounts and stores metadata for things like game progression and in-game items.

<figure>
  <img src="../images/profile.png" alt="Sagi-shi player profile screen">
  <figcaption>Player profile</figcaption>
</figure>


### Get the user account

Many of Nakama's features are accessible with an authenticated session, like [fetching a user account](../../concepts/user-accounts/#fetch-account).

Get a Sagi-shi player's full user account with their basic [user information](../../concepts/user-accounts/#fetch-account) and user id:

```js
const account = await client.getAccount(session);
const user = account.user;
var username = user.username;
var avatarUrl = user.avatarUrl;
var userId = user.id;
```


### Update the user account

Nakama provides easy methods to update server stored resources like user accounts.

Sagi-shi players need to be able to update their public profiles:

```js
var newUsername = "NotTheImp0ster";
var newDisplayName = "Innocent Dave";
var newAvatarUrl = "https://example.com/imposter.png";
var newLangTag = "en";
var newLocation = "Edinburgh";
var newTimezone = "BST";
await client.updateAccount(session, newUsername, newDisplayName, newAvatarUrl, newLangTag, newLocation, newTimezone);
```


### Getting users

In addition to getting the current authenticated player's user account, Nakama has a convenient way to get a list of other players' public profiles from their ids or usernames.

Sagi-shi uses this method to display player profiles when engaging with other Nakama features:

```js
var users = await client.getUsers(session, ["<AnotherUserId>"]);
```


### Storing metadata

Nakama [User Metadata](../../concepts/user-accounts/#user-metadata) allows developers to extend user accounts with public user fields.

User metadata can only be updated on the server. See the [updating user metadata](../server-framework/recipes/updating-user-metadata) recipe for an example.

Sagi-shi will use metadata to store what in-game items players have equipped:


### Reading metadata

Get the updated account object and parse the JSON metadata:

```js
// Get the updated account object.
var account = await client.getAccount(session);

// Parse the account user metadata.
var metadata = JSON.parse(account.user.metadata);

console.log("Title: %o", metadata.title);
console.log("Hat: %o", metadata.hat);
console.log("Skin: %o", metadata.skin);
```


### Wallets

Nakama [User Wallets](../../concepts/user-accounts/#virtual-wallet) can store multiple digital currencies as key/value pairs of strings/integers.

Players in Sagi-shi can unlock or purchase titles, skins and hats with a virtual in-game currency.

<!-- mockup Sagi-shi in-game store with multiple currencies
important for our game demographic, to visualize their game's store
-->


#### Accessing wallets

Parse the JSON wallet data from the user account:

```js
var account = await client.getAccount(session);
var wallet = JSON.parse(account.wallet);
var keys = wallet.keys;

keys.forEach(function(currency) {
    console.log("%o: %o", currency, wallet[currency].toString())
});
```


#### Updating wallets

Wallets can only be updated on the server. See the [user account virtual wallet](../../concepts/user-accounts/#virtual-wallet) documentation for an example.


#### Validating in-app purchases

Sagi-shi players can purchase the virtual in-game currency through in-app purchases that are authorized and validated to be legitimate on the server.

See the [In-app Purchase Validation](../../concepts/iap-validation/) documentation for examples.


## Storage Engine

The Nakama [Storage Engine](../../concepts/collections/) is a distributed and scalable document-based storage solution for your game.

The Storage Engine gives you more control over how data can be [accessed](../../concepts/access-controls/#object-permissions) and [structured](../../concepts/collections/#collections) in collections.

Collections are named, and store JSON data under a unique key and the user id.

By default, the player has full permission to create, read, update and delete their own storage objects.

Sagi-shi players can unlock or purchase many items, which are stored in the Storage Engine.

<figure>
  <img src="../images/player_items.png" alt="Sagi-shi player items screen">
  <figcaption>Player items</figcaption>
</figure>



### Reading storage objects

Create a new storage object id with the collection name, key and user id. Then read the storage objects and parse the JSON data:

```js
var readObjectId = new storageObjectId {
    collection = "Unlocks",
    key = "Hats",
    userId = session.user.id
};

var result = await client.readStorageObjects(session, readObjectId);

if (result.objects.any())
{
    var storageObject = result.objects.first();
    var unlockedHats = JSON.parse(storageObject.value);
    console.log("Unlocked hats: %o", string.join(",", unlockedHats.Hats));
}
```

!!! note "Note"
    To read other players' public storage object, use their `UserId` instead.
    Players can only read storage objects they own or that are public (`PermissionRead` value of `2`).


### Writing storage objects

Nakama allows developers to write to the Storage Engine from the client and server.

Consider what adverse effects a malicious user can have on your game and economy when deciding where to put your write logic, for example data that should only be written authoritatively (i.e. game unlocks or progress).

Sagi-shi allows players to favorite items for easier access in the UI and it is safe to write this data from the client.

Create a write storage object with the collection name, key and JSON encoded data. Finally, write the storage objects to the Storage Engine:

```js
var favoriteHats = new {
    hats = ["cowboy", "alien"]
};

var writeObject = new WriteStorageObject {
    collection = "favorites",
    ley = "Hats",
    value = JSON.stringify(favoriteHats),
    permissionRead = 1, // Only the server and owner can read
    permissionWrite = 1 // The server and owner can write
};

await client.writeStorageObjects(session, writeObject);
```

You can also pass multiple objects to the `WriteStorageObjectsAsync` method:

```js
var writeObjects = {
    new WriteStorageObject {
        //...
    },
    new WriteStorageObject
    {
        // ...
    }
};

await client.writeStorageObjects(session, writeObjects);
```


### Conditional writes

Storage Engine [Conditional Writes](../../concepts/collections/#conditional-writes) ensure that write operations only happen if the object hasn't changed since you accessed it.

This gives you protection from overwriting data, for example the Sagi-shi server could have updated an object since the player last accessed it.

To perform a conditional write, add a version to the write storage object with the most recent object version:

```js
// Assuming we already have a storage object (storageObject)
var writeObject = new WriteStorageObject {
    collection = storageObject.collection,
    key = storageObject.key,
    value = "<NewJSONValue>",
    permissionWrite = 0,
    permissionRead = 1,
    version = storageObject.version
};

try {
    await client.writeStorageObjects(session, writeObjects);
}
catch (error) {
    console.log(error.message);
}
```


### Listing storage objects

Instead of doing multiple read requests with separate keys you can list all the storage objects the player has access to in a collection.

Sagi-shi lists all the player's unlocked or purchased titles, hats and skins:

```js
var limit = 3;
var cursor = null;
var unlocksObjectList = await client.listStorageObjects(session, "Unlocks", limit, cursor);

unlocksObjectList.objects.forEach(function(unlockStorageObject) {
    switch(unlockStorageObject.key) {
        case "Titles":
            var unlockedTitles = JSON.parse<TitlesStorageObject>(unlockStorageObject.value);
            // Display the unlocked titles
            break;
        case "Hats":
            var unlockedHats = JSON.parse<HatsStorageObject>(unlockStorageObject.value);
            // Display the unlocked hats
            break;
        case "Skins":
            var unlockedSkins = JSON.parse<SkinsStorageObject>(unlockStorageObject.value);
            // Display the unlocked skins
            break;
    }
});
```


### Paginating results

Nakama methods that list results return a cursor which can be passed to subsequent calls to Nakama to indicate where to start retrieving objects from in the collection.

For example:
- If the cursor has a value of 5, you will get results from the fifth object.
- If the cursor is `null`, you will get results from the first object.

```js
objectList = await client.listStorageObjects(session, "<CollectionName>", limit, objectList.cursor);
```


### Protecting storage operations on the server

Nakama Storage Engine operations can be protected on the server to protect data the player shouldn't be able to modify (i.e.  game unlocks or progress). See the [writing to the Storage Engine authoritatively](../server-framework/recipes/writing-to-storage-authoritatively) recipe.


## Remote Procedure Calls

The Nakama [Server](..//server-framework/basics/) allows developers to write custom logic and expose it to the client as [RPCs](../server-framework/basics/#rpc-example).

Sagi-shi contains various logic that needs to be protected on the server, like checking if the player owns equipment before equipping it.

<!-- PRC flow diagram -->


### Creating server logic

See the [handling player equipment authoritatively](../server-framework/recipes/handling-player-equipment-authoritatively) recipe for an example of creating a remote procedure to check if the player owns equipment before equipping it.


### Client RPCs

Nakama Remote Procedures can be called from the client and take optional JSON payloads.

The Sagi-shi client makes an RPC to securely equip a hat:

```js
try {
    var payload = { "item": "cowboy"};
    var response = await client.rpc(session, "EquipHat", payload);
    console.log("New hat equipped successfully", response);
}
catch (error) {
    console.log("Error: %o", error.message);
}
```


### Socket RPCs

Nakama Remote Procedures can also be called from the socket when you need to interface with Nakama's real-time functionality. These real-time features require a live socket (and corresponding session identifier). RPCs can be made on the socket carrying this same identifier.

```js
var response = await socket.rpc("<rpcId>", "<payloadString>");
```


## Friends

Nakama [Friends](../../concepts/friends/) offers a complete social graph system to manage friendships amongst players.

Sagi-shi allows players to add friends, manage their relationships and play together.

<figure>
  <img src="../images/friends.png" alt="Sagi-shi Friends screen">
  <figcaption>Friends screen</figcaption>
</figure>


### Adding friends

Adding a friend in Nakama does not immediately add a mutual friend relationship. An outgoing friend request is created to each user, which they will need to accept.

Sagi-shi allows players to add friends by their usernames or user ids:


```js
// Add friends by Username.
var usernames = ["AlwaysTheImposter21", "SneakyBoi"];
await client.addFriends(session, usernames);

// Add friends by User ID.
var ids = ["<SomeUserId>", "<AnotherUserId>"];
await client.addFriends(session, ids);
```


### Friendship states

Nakama friendships are categorized with the following [states](../../concepts/friends/#friend-state):

| Value | State |
| ----- | ----- |
| 0 | Mutual friends |
| 1 | An outgoing friend request pending acceptance |
| 2 | An incoming friend request pending acceptance |
| 4 | Banned |


### Listing friends

Nakama allows developers to list the player's friends based on their [friendship state](../../concepts/friends/#friend-state).

Sagi-shi lists the 20 most recent mutual friends:

```js
var limit = 20; // Limit is capped at 1000
var friendshipState = 0;
var result = await client.listFriends(session, friendshipState, limit, cursor: null);

result.forEach((friend) => {
    console.log("ID: %o", friend.user.id);
});
```


### Accepting friend requests

When accepting a friend request in Nakama the player adds a [bi-directional friend relationship](../../concepts/friends-best-practices/#modeling-relationships).

Nakama takes care of changing the state from pending to mutual for both.

In a complete game you would allow players to accept individual requests.

Sagi-shi just fetches and accepts all the incoming friend requests:

```js
var limit = 1000;
var result = await client.listFriends(session, 2, limit, cursor: null);

result.forEach((friend) => {
    await client.addFriends(session, friend.user.id);
});
```


### Deleting friends

Sagi-shi players can remove friends by their username or user id:

```js
// Delete friends by User ID.
var ids = ["<SomeUserId>", "<AnotherUserId>"];
await client.deleteFriends(session, ids});

// Delete friends by Username.
var usernames = ["AlwaysTheImposter21", "SneakyBoi"];
await client.deleteFriends(session, null, usernames});
```


### Blocking users

Sagi-shi players can block others by their username or user id:

```js
// Block friends by User ID.
var ids = ["<SomeUserId>", "<AnotherUserId>"];
await client.blockFriends(session, ids);

// Block friends by Username.
var usernames = ["AlwaysTheImposter21", "SneakyBoi"];
await client.blockFriends(session, usernames);
```

Learn more about [blocking friends](../../concepts/friends/#block-a-friend) and the associated [relationship states](../../concepts/friends-best-practices/#relationship-state).

Blocked users can listed just like [listing friends](#listing-friends) but using the corresponding friendship state (`3`).


## Status & Presence

Nakama [Status & Presence](../../concepts/status/) has a real-time status and presence service that allows users to set their online presence, update their status message and follow other user's updates.

Players don't have to be friends with others they want to follow.

Sagi-shi uses status messages and online presences to notify players when their friends are online and share matches.

<figure>
  <img src="../images/status.png" alt="Sagi-shi status update screen">
  <figcaption>Updating player status</figcaption>
</figure>


### Follow users

The Nakama real-time APIs allow developers to subscribe to events on the socket, like a status presence change, and receive them in real-time.

The method to [follow users](../../concepts/status/#follow-users) also returns the current online users, known as presences, and their status.

Sagi-shi follows a player's friends and notifies them when they are online:

```js
// Subscribe to the Status event.
socket.receivedStatusPresence = (e) => {
    e.joins.forEach(function(presence){
        console.log("%o is online with status: %o", presence.username, presence.status);
    })
    e.leaves.forEach(function(presence){
        console.log("%o went offline", presence.username);
    })
};

// Follow mutual friends and get the initial Status of any that are currently online.
var friendsResult = await client.listFriends(session, 0);
var friendIds = [];
friendsResult.friends.forEach(function(friend) {
    friendIds.push(friend.user.id);
});
var result = await socket.followUsers(friendIds);

result.presences.forEach(function(presence){
    console.log("%o is online with status: %o", presence.username, presence.status);
});
```


### Unfollow users

Sagi-shi players can unfollow others:

```js
await socket.unfollowUsers(["<UserId>"]);
```


### Updating player status

Sagi-shi players can change and publish their status to their followers:

```js
await socket.updateStatus("Viewing the Main Menu");
```


## Groups

Nakama [Groups](../../concepts/groups-clans/) is a group or clan system with public/private visibility, user memberships and permissions, metadata and group chat.

Sagi-shi allows players to form and join groups to socialize and compete.

<figure>
  <img src="../images/groups_list.png" alt="Sagi-shi groups screen">
  <figcaption>Groups list screen</figcaption>
</figure>


### Creating groups

Groups have a public or private "open" visibility. Anyone can join public groups, but they must request to join and be accepted by a superadmin/admin of a private group.

Sagi-shi players can create groups around common interests:

```js
const groupName = "Imposters R Us";
const description = "A group for people who love playing the imposter.";

const group = await client.createGroup(session {
    name: groupName,
    description: description,
    open: true, // public group
    maxSize = 100
});
```

### Update group visibility

Nakama allows group superadmin or admin members to update some properties from the client, like the open visibility:

```js
const groupId = "<groupId>";
await client.updateGroup(session, groupId, {
    open: false
});
```


### Update group size

Other properties, like the group's maximum member size, can only be changed on the server.

See the [updating group size](../../concepts/groups-clans/#updating-group-size) recipe for an example, and the [Groups server function reference](../server-framework/function-reference/#groups) to learn more about updating groups on the server.

<figure>
  <img src="../images/group_edit.png" alt="Sagi-shi group edit screen">
  <figcaption>Sagi-shi group edit</figcaption>
</figure>


### Listing and filtering groups

Groups can be listed like other Nakama resources and also [filtered](../../concepts/groups-clans/#list-and-filter-groups) with a wildcard group name.

Sagi-shi players use group listing and filtering to search for existing groups to join:


```js
var limit = 20;
var result = await client.ListGroupsAsync(session, "imposter%", limit);

result.groups.forEach(function(group){
    console.log("%o group is %o", group.name, group.open);
});

// Get the next page of results.
var nextResults = await client.listGroups(session, name: "imposter%", limit, result.cursor);
```


### Deleting groups

Nakama allows group superadmins to delete groups.

Developers can disable this feature entirely, see the [Guarding APIs guide](../guides/guarding-apis/) for an example on how to protect various Nakama APIs.

Sagi-shi players can delete groups which they are superadmins for:

```js
const groupId = "<groupId>";
await client.deleteGroup(session, groupId);
```


### Group metadata

Like Users Accounts, Groups can have public metadata.

Sagi-shi uses group metadata to store the group's interests, active player times and languages spoken.

Group metadata can only be updated on the server. See the [updating group metadata](../server-framework/recipes/updating-group-metadata) recipe for an example.

The Sagi-shi client makes an RPC with the group metadata payload:

```js
const payload = new {
    groupId = "<GroupId>",
    interests = ["Deception", "Sabotage", "Cute Furry Bunnies"],
    activeTimes = ["9am-2pm Weekdays", "9am-10pm Weekends"],
    languages = ["English", "German"]
};

try {
    var result = await client.rpc(session, "UpdateGroupMetadata", JSON.stringify(payload));
    console.log("Successfully updated group metadata");
}
catch (error) {
    console.log("Error: %o", error.message);
}
```


### Group membership states

Nakama group memberships are categorized with the following [states](../../concepts/groups-clans/#groups-and-clans):

| Code | Purpose | |
| ---- | ------- | - |
|    0 | Superadmin | There must at least be 1 superadmin in any group. The superadmin has all the privileges of the admin and can additionally delete the group and promote admin members. |
|    1 | Admin | There can be one of more admins. Admins can update groups as well as accept, kick, promote, demote, ban or add members. |
|    2 | Member | Regular group member. They cannot accept join requests from new users. |
|    3 | Join request | A new join request from a new user. This does not count towards the maximum group member count. |

<!-- maybe another mockup screen for joining and accepting -->


### Joining a group

If a player joins a public group they immediately become a member, but if they try and join a private group they must be accepted by a group admin.

Sagi-shi players can join a group:

```js
const group_id = "<group id>";
await client.joinGroup(session, group_id);
```


### Listing the user's groups

Sagi-shi players can list groups they are a member of:

```js
const userId = "<user id>";
const groups = await client.listUserGroups(session, userId);
groups.user_groups.forEach(function(userGroup){
  console.log("Group: name '%o' State: '%o'.", userGroup.group.name, userGroup.state);
});
```

### Listing members

Sagi-shi players can list a group's members:

```js
const groupId = "<group id>";
const groups = await client.listUserGroups(session, groupId);
groups.group_users.forEach(function(groupUser){
  console.log("User: ID '%o' State: '%o'.", groupUser.user.id, groupUser.state);
});
```

### Accepting join requests

Private group admins or superadmins can accept join requests by re-adding the user to the group.

Sagi-shi first lists all the users with a join request state and then loops over and adds them to the group:

```js
const groupId = "<group id>";
const result = await client.listGroupUsers(session, groupId);
groups.group_users.forEach(function(groupUser){
    await client.addGroupUsers(session, groupId, [groupUser.user.id]);
});
```

### Promoting members

Nakama group members can be promoted to admin or superadmin roles to help manage a growing group or take over if members leave.

Admins can promote other members to admins, and superadmins can promote other members up to superadmins.

The members will be promoted up one level. For example:

- Promoting a member will make them an admin
- Promoting an admin will make them a superadmin

```js
const groupId = "<group id>";
const userId = "<user id>";
await client.promoteGroupUsers(session, groupId, [userId]);
```


### Demoting members

Sagi-shi group admins and superadmins can demote members:

```js
const groupId = "<group id>";
const userId = "<user id>";
await client.demoteGroupUsers(session, groupId, [userId]);
```


### Kicking members

Sagi-shi group admins and superadmins can remove group members:

```js
const groupId = "<group id>";
const userId = "<user id>";
await client.kickGroupUsers(session, groupId, [userId]);
```


### Banning members

Sagi-shi group admins and superadmins can ban a user when demoting or kicking is not severe enough:

```js
const groupId = "<group id>";
const userId = "<user id>";
await client.banGroupUsers(session, groupId, [userId]);
```

### Leaving groups

Sagi-shi players can leave a group:

```js
const groupId = "<group id>";
await client.leaveGroup(session, groupId);
```


## Chat

Nakama Chat is a real-time chat system for groups, private/direct messages and dynamic chat rooms.

Sagi-shi uses dynamic chat during matches, for players to mislead each other and discuss who the imposters are, group chat and private/direct messages.

<figure>
  <img src="../images/chat.png" alt="Sagi-shi chat screen">
  <figcaption>Sagi-shi Chat</figcaption>
</figure>


### Joining dynamic rooms

Sagi-shi matches have a non-persistent chat room for players to communicate in:

```js
const roomName = "<match id>";
const persistence = false;
const hidden = false;
// 1 = Room, 2 = Direct Message, 3 = Group
const channel = await socket.joinChat(1, roomName, persistence, hidden);

console.log("Connected to dynamic room channel: %o", channel.id);
```

### Joining group chat

Sagi-shi group members can have conversations that span play sessions in a persistent group chat channel:

```js
const groupId = "<group id>";
const persistence = true;
const hidden = false;
// 1 = Room, 2 = Direct Message, 3 = Group
const channel = await socket.joinChat(3, groupId, persistence, hidden);

console.log("Connected to group channel: %o", channel.id);
```

### Joining direct chat

Sagi-shi players can also chat privately one-to-one during or after matches and view past messages:

```js
const userId = "<user id>";
const persistence = true;
const hidden = false;
// 1 = Room, 2 = Direct Message, 3 = Group
const channel = await socket.joinChat(2, userId, persistence, hidden);

console.log("Connected to direct message channel: %o", channel.id);
```

### Sending messages

Sending messages is the same for every type of chat channel. Messages contain chat text and emotes and are sent as JSON serialized data:

```js
var channelId = "<channel id>";
var data = { "message": "I think Red is the imposter!" };
const messageAck = await socket.writeChatMessage(channelId, data);

var emoteData = {
    "emote": "point",
    "emoteTarget": "<redPlayerUserId>"
}
const emoteMessageAck = await socket.writeChatMessage(channelId, emoteData);
```

### Listing message history

Message listing takes a parameter which indicates if messages are received from oldest to newest (forward) or newest to oldest.

Sagi-shi players can list a group's message history:

```js
const groupId = "<group id>";
const limit = 100;
const forward = true;

const result = await client.listChannelMessages(session, groupId, limit, forward, cursor: null);
result.messages.forEach((message) => {
  console.log("%o: %o", message.username, message.data);
});
```

Chat also has cacheable cursors to fetch the most recent messages. Read more about cacheable cursors in the [listing notifications](../../concepts/in-app-notifications/#list-notifications) documentation.

```js
const cursor = result.cacheable_cursor;
const nextResults = await client.listChannelMessages(session, groupId, limit, forward, cursor);
```


### Updating messages

Nakama also supports updating messages. It is up to you whether you want to use this feature, but in a game of deception like Sagi-shi it can add an extra element of deception.

For example a player sends the following message:

```js
var channelId = "<ChannelId>";
var messageData = {"message": "I think Red is the imposter!" };
const messageSendAck = await socket.writeChatMessage(channelId, messageData);
```

They then quickly edit their message to confuse others:

```js
var newMessageData = {"message": "I think BLUE is the imposter!" };
const messageUpdateAck = await socket.updateChatMessage(channelId, messageSendAck.message.id, newMessageData));
```


## Matches

Nakama supports [Server Authoritative](../../concepts/server-authoritative-multiplayer/) and [Server Relayed](../../concepts/client-relayed-multiplayer/) multiplayer matches.

In server authoritative matches the server controls the gameplay loop and must keep all clients up to date with the current state of the game.

In server relayed matches the client is in control, with the server only relaying information to the other connected clients.

In a competitive game such as Sagi-shi, server authoritative matches would likely be used to prevent clients from interacting with your game in unauthorized ways.

For the simplicity of this guide, the server relayed model is used.

<!-- flow chat of server relayed matches, to be fleshed out -->


### Creating matches

Sagi-shi players can create their own matches and invite their online friends to join:

```js
var match = await socket.createMatch();
var friendsList = await client.listFriends(session);
var onlineFriends = [];
friendsList.friends.forEach((friend){
    if (friend.user.online){
        onlineFriends.push(friend.user);
    }
});

onlineFriends.friend.forEach(function(friend){
    var messageData = {"message": "Hey %o, join me for a match!", friends.username},
    var matchId = match.id,
    const channel = await socket.joinChat(2, friend.id),
    const messageAck = await socket.writeChatMessage(channel, messageData)
});
```

### Joining matches

Sagi-shi players can try to join existing matches if they know the id:

```js
var matchId = "<MatchId>";
var match = await socket.joinMatch(matchId);
```

Or set up a real-time matchmaker listener and add themselves to the matchmaker:

```js
socket.onmatchmakermatched = async (matchmakerMatched) => {
    var match = await socket.joinMatch(matchmakerMatched);
};

var minPlayers = 2;
var maxPlayers = 10;
var query = "";

var matchmakingTicket = await socket.addMatchmaker(query, minPlayers, maxPlayers);
```


**Joining matches from player status**

Sagi-shi players can update their status when they join a new match:

```js
var status = {
    "Status": "Playing a match",
    "MatchId": "<MatchId>"
};

await socket.updateStatus(JSON.stringify(status));
```

When their followers receive the real-time status event they can try and join the match:

```js
socket.receivedStatusPresence = async (e) => {
    // Join the first match found in a friend's status
    e.joins.forEach(function(presence){
        var status = JSON.parse(presence.status),
        if (status.hasOwnProperty("MatchId")) {
            await socket.joinMatch(status["MatchId"]);
            break;
        }
    });
```

### Listing matches

Match Listing takes a number of criteria to filter matches by including player count, a match [label](../../concepts/server-authoritative-multiplayer/#match-label) and an option to provide a more complex [search query](../../concepts/server-authoritative-multiplayer/#search-query).

<!-- todo: we don't have concept or client side docs for match listing labels or search queries -->

Sagi-shi matches start in a lobby state. The match exists on the server but the actual gameplay doesn't start until enough players have joined.

Sagi-shi can then list matches that are waiting for more players:

```js
var minPlayers = 2;
var maxPlayers = 10;
var limit = 10;
var authoritative = true;
var label = "";
var query = "";
const result = await client.listMatches(session, minPlayers, maxPlayers, limit, authoritative, label, query);

result.matches.forEach(function(match){
    console.log("%o: %o/10 players", match.id, match.size);
});
```

To find a match that has a label of `"AnExactMatchLabel"`:

```js
var label = "AnExactMatchLabel";
```

**Advanced:**

In order to use a more complex structured query, the match label must be in JSON format.

To find a match where it expects player skill level to be `>100` and optionally has a game mode of `"sabotage"`:

```js
var query = "+label.skill:>100 label.mode:sabotage";
```


### Spawning players

The match object has a list of current online users, known as presences.

Sagi-shi uses the match presences to spawn players on the client:

```js
var match = await socket.joinMatch(matchId);

var players = {};

match.presences.forEach(function(presence){
    var go = spawnPlayer(); // Instantiate player object
    players.push(presence.session.id, go);
});
```

Sagi-shi keeps the spawned players up-to-date as they leave and join the match using the match presence received event:

```js
socket.receivedMatchPresence = (matchPresenceEvent) => {
    // For each player that has joined in this event...
    matchPresenceEvent.joins.forEach(function(presence){
        // Spawn a player for this presence and store it in a dictionary by session id.
        var go = // Instantiate player object;
        players.push(presence.session.id, go);
    })

    // For each player that has left in this event...
    matchPresenceEvent.leaves.forEach(function(presence){
        // Remove the player from the game if they've been spawned
        if (players.hasOwnProperty("SessionId"){
            const index = players.session.id;
            if (index > -1) {
                players.splice(index, 1);
            }
        })
    })
};
```

### Sending match state

Nakama has real-time networking to [send](../../concepts/client-relayed-multiplayer/#send-data-messages) and [receive](../../concepts/client-relayed-multiplayer/#receive-data-messages) match state as players move and interact with the game world.

During the match, each Sagi-shi client sends match state to the server to be relayed to the other clients.

Match state contains an op code that lets the receiver know what data is being received so they can deserialize it and update their view of the game.

Example op codes used in Sagi-shi:
- 1: player position
- 2: player calling vote


**Sending player position**

Define a class to represent Sagi-shi player position states:

```js
class PositionState {
    static X;
    static Y;
    static Z;
}
```

Create an instance from the player's transform, set the op code and send the JSON encoded state:

```js
var state = new PositionState {
    x = transform.position.x,
    y = transform.position.y,
    z = transform.position.z
};

var opCode = 1;

await socket.sendMatchState(match.Id, opCode, JSON.stringify(state));
```


**Op Codes as a static class**

Sagi-shi has many networked game actions. Using a static class of constants for op codes will keep your code easier to follow and maintain:

```js
class OpCodes {
    static position = 1;
    static vote = 2;
}

await socket.sendMatchState(match.Id, OpCodes.position, JSON.stringify(state));
```


### Receiving match sate

Sagi-shi players can receive match data from the other connected clients by subscribing to the match state received event:

```js
socket.ReceivedMatchState = (matchState) => {
    switch (matchState.opCode) {
        case opCodes.position:
            // Get the updated position data
            var stateJson = matchState.state;
            var positionState = JSON.parse(stateJson);

            // Update the GameObject associated with that player
            if (players.hasOwnProperty(matchState.user_presence.session.id)) {
                // Here we would normally do something like smoothly interpolate to the new position, but for this example let's just set the position directly.
                players[matchState.user_presence.session.id].transform.position = new Vector3(positionState.s, positionState.y, positionState.z);
            }
            break;
        default:
            console.log("Unsupported op code");
            break;
    }
};
```


## Matchmaker

Developers can find matches for players using Match Listing or the Nakama [Matchmaker](../../concepts/matches/), which enables players join the real-time matchmaking pool and be notified when they are matched with other players that match their specified criteria.

Matchmaking helps players find each other, it does not create a match. This decoupling is by design, allowing you to use matchmaking for more than finding a game match. For example, if you were building a social experience you could use matchmaking to find others to chat with.

<!-- matchmker flow chart, to be fleshed out -->


### Add matchmaker

Matchmaking criteria can be simple, find 2 players, or more complex, find 2-10 players with a minimum skill level interested in a specific game mode.

Sagi-shi allows players to join the matchmaking pool and have the server match them with other players:

```js
var minPlayers = 2;
var maxPlayers = 10;
var query = "+skill:>100 mode:sabotage";
var stringProperties = { "mode": "sabotage"};
var numericProperties = { "skill": 125};
var matchmakerTicket = await socket.addMatchmaker(query, minPlayers, maxPlayers, stringProperties, numericProperties);
```

After being successfully matched according to the provided criteria, players can join the match:

```js
socket.onmatchmakermatched = (matched) => {
  const matchId = null;
  socket.joinMatch(matchId, matched.token);
};
```

<!-- todo: add a stand alone guide/recipe for widening the matchmaker criteria as it's a more complex example to include here -->


## Parties

Nakama [Parties](../../concepts/parties/) is a real-time system that allows players to form short lived parties that don't persist after all players have disconnected.

Sagi-shi allows friends to form a party and matchmake together.

<!-- parties flow chart or Sagi-shi mockup to be fleshed out -->


### Creating parties

The player who creates the party is the party's leader. Parties have maximum number of players and can be open to automatically accept players or closed so that the party leader can accept incoming join requests.

Sagi-shi uses closed parties with a maximum of 4 players:

```js
var open = false;
var maxPlayers = 4;
const party = await socket.createParty(open, maxPlayers);
```

Sagi-shi shares party ids with friends via private/direct messages:

```js
var friendsList = await client.listFriends(session);
var onlineFriends = [];
friendsList.friends.forEach((friend){
    if (friend.user.online){
        onlineFriends.push(friend.user);
    }
});

onlineFriends.friend.forEach(function(friend){
    var messageData = {"message": "Hey %o, wanna join the party?", friends.username};
    var partyId = party.id;
    const channel = await socket.joinChat(2, friend.id);
    const messageAck = await socket.writeChatMessage(channel, messageData);
});
```

### Joining parties

Sagi-shi players can join parties from chat messages by checking for the party id in the message:

```js
socket.receivedChannelMessage = async (m) => {
    var content = JSON.parse(m.content);
    if (content.hasOwnProperty("partyId")) {
        await socket.joinParty(content["partyId"]);
    }
};
```

### Promoting a member

Sagi-shi party members can be promoted to the party leader:

```js
var newLeader = "<user id>";
await socket.promotePartyMember(party.Id, newLeader);
```

### Leaving parties

Sagi-shi players can leave parties:

```js
await socket.leaveParty(party.Id);
```


### Matchmaking with parties

One of the main benefits of joining a party is that all the players can join the matchmaking pool together.

Sagi-shi players can listen to the the matchmaker matched event and join the match when one is found:

```js
socket.onmatchmakermatched = async (matchmakerMatched) => {
    await socket.joinMatch(matchmakerMatched.match.id);
};
```

The party leader will start the matchmaking for their party:

```js
var partyId = "<party id>";
var minPlayers = 2;
var maxPlayers = 10;
var query = "";
var matchmakerTicket = await socket.addMatchmakerParty(partyId, query, minPlayers, maxPlayers);
```


## Leaderboards

Nakama [Leaderboards](../../concepts/leaderboards/) introduce a competitive aspect to your game and increase player engagement and retention.

Sagi-shi has a leaderboard of weekly imposter wins, where player scores increase each time they win, and similarly a leaderboard for weekly crew member wins.

<figure>
  <img src="../images/leaderboard.png" alt="Sagi-shi leaderboard screen">
  <figcaption>Sagi-shi Leaderboard</figcaption>
</figure>


### Creating leaderboards

Leaderboards have to be created on the server, see the [leaderboard](../../concepts/leaderboards/#create-a-leaderboard) documentation for details on creating leaderboards.


### Submitting scores

When players submit scores, Nakama will increment the player's existing score by the submitted score value.

Along with the score value, Nakama also has a subscore, which can be used for ordering when the scores are the same.

Sagi-shi players can submit scores to the leaderboard with contextual metadata, like the map the score was achieved on:

```js
var score = 1;
var subscore = 0;
var metadata = { "map": "space_station"};
await client.writeLeaderboardRecord(session, "weekly_imposter_wins", score, subscore, JSON.stringify(metadata));
```

### Listing the top records

Sagi-shi players can list the top records of the leaderboard:

```js
var limit = 20;
var leaderboardName = "weekly_imposter_wins";
const result = await client.listLeaderboardRecords(session, leaderboardName, ownerIds: null, expiry: null, limit, cursor: null);

result.records.forEach(fuction(record){
    console.log("%o:%o", record.owner.id, record.score);
});
```


**Listing records around the user**

Nakama allows developers to list leaderboard records around a player.

Sagi-shi gives players a snapshot of how they are doing against players around them:

```js
var userId = session.user.id;
var limit = 20;
var leaderboardName = "weekly_imposter_wins";
var result = await client.listLeaderboardRecordsAroundOwner(session, leaderboardName, userId, expiry: null, limit);

result.records.forEach(fuction(record){
    console.log("%o:%o", record.owner.id, record.score);
});
```

**Listing records for a list of users**

Sagi-shi players can get their friends' scores by supplying their user ids to the owner id parameter:

```js
var friendsList = await client.ListFriendsAsync(session);
var userIds = [];
friendsList.friends.forEach(function(friend){
    userIds.push(friend.user.id);
});
var recordList = await client.listLeaderboardRecords(session, "weekly_imposter_wins", userIds, expiry: null, 100, cursor: null);

recordList.records.forEach(fuction(record){
    console.log("%o:%o", record.username, record.score);
});
```

The same approach can be used to get group member's scores by supplying their user ids to the owner id parameter:

```js
var groupId = "<group id>";
var groupUserList = await client.listGroupUsers(session, groupId);
var userIds = [];
groupUserList.forEach(function(group_user){
    if (group_user.state < 3){
        userIds.push(group_user.id);
    }
});

var recordList = await client.listLeaderboardRecords(session, "weekly_imposter_wins", userIds, expiry: null, 100, cursor: null);
recordList.records.forEach(fuction(record){
    console.log("%o:%o", record.username, record.score);
});
```

### Deleting records

Sagi-shi players can delete their own leaderboard records:

```js
var leaderboardId = "<leaderboard id>";
await client.deleteLeaderboardRecord(session, leaderboardId);
```


## Tournaments

Nakama [Tournaments](../../concepts/tournaments/) are short lived competitions where players compete for a prize.

Sagi-shi players can view, filter and join running tournaments.

<figure>
  <img src="../images/tournaments.png" alt="Sagi-shi tournaments screen">
  <figcaption>Sagi-shi Tournaments</figcaption>
</figure>


### Creating tournaments

Tournaments have to be created on the server, see the [tournament](../../concepts/tournaments/#create-tournament) documentation for details on how to create a tournament.

Sagis-shi has a weekly tournament which challenges players to get the most correct imposter votes. At the end of the week the top players receive a prize of in-game currency.


### Joining tournaments

By default in Nakama players don't have to join tournaments before they can submit a score, but Sagi-shi makes this mandatory:

```js
var id = "<tournament id>";
await await client.joinTournament(session, id);
```

### Listing tournaments

Sagi-shi players can list and filter tournaments with various criteria:

```js
var categoryStart = 1;
var categoryEnd = 2;
var startTime = 1538147711;
var endTime = null; // all tournaments from the start time
var limit = 100; // number to list per page
var cursor = null;
var result = await client.listTournaments(session, categoryStart, categoryEnd, startTime, endTime, limit, cursor);

result.tournaments.forEach(function(tournament) {
    console.log("%o:%o", tournament.id, tournament.title);
});
```

!!! note "Note"
    Categories are filtered using a range, not individual numbers, for performance reasons. Structure your categories to take advantage of this (e.g. all PvE tournaments in the 1XX range, all PvP tournaments in the 2XX range, etc.).


### Listing records

Sagi-shi players can list tournament records:

```js
var tournamentName = "weekly_top_detective";
var limit = 20;
var result = await client.listTournamentRecords(session, tournamentName, limit);
result.records.forEach(function(record) {
  console.log("%o:%o", record.owner.id, record.score);
});
```

**Listing records around a user**

Similarly to leaderboards, Sagi-shi players can get other player scores around them:

```js
var userId = "<user id>";
var limit = 20;
var tournamentName = "weekly_top_detective";
var result = await client.listTournamentRecordsAroundOwner(session, tournamentName, userId, limit);
result.records.forEach(function(record) {
  console.log("%o:%o", record.owner.id, record.score);
});
```

### Submitting scores

Sagi-shi players can submit scores, subscores and metadata to the tournament:

```js
var tournamentName = "weekly_top_detective";
var score = 1;
var subscore = 0;
var metadata = { "map": "space_station"};
await client.writeTournamentRecord(session, tournamentName, score, subscore, metadata);
```

## Notifications

Nakama [Notifications](../../concepts/in-app-notifications/) can be used for the game server to broadcast real-time messages to players.

Notifications can be either persistent (remaining until a player has viewed it) or transient (received only if the player is currently online).

Sagi-shi uses Notifications to notify tournament winners about their winnings.

<figure>
  <img src="../images/notifications.png" alt="Sagi-shi notification screen">
  <figcaption>Sagi-shi notifications</figcaption>
</figure>


### Receiving notifications

Notifications have to be sent from the server.

Nakama uses a code to differentiate notifications. Codes of `0` and below are [system reserved](../../concepts/in-app-notifications/#notification-codes) for Nakama internals.

Sagi-shi players can subscribe to the notification received event. Sagi-shi uses a code of `100` for tournament winnings:

```js
socket.onnotification = (notification) => {
    const rewardCode = 100;
    switch (notification.code) {
        case rewardCode:
            console.log("Congratulations, you won the tournament!\n%o\n%o", notification.subject, notification.content);
            break;
        default:
            console.log("Other notification: %o:%o\n%o", notification.code, notification.subject, notification.content);
            break;
    }
};
```


### Listing notifications

Sagi-shi players can list the notifications they received while offline:

```js
const result = await client.listNotifications(session, 10);
result.notifications.forEach(notification => {
  console.info("Notification code %o and subject %o.", notification.code, notification.subject);
});
console.info("Fetch more results with cursor:", result.cacheable_cursor);
```

```js
var limit = 100;
var cacheableCursor = null;
var result = await client.listNotifications(session, limit, cacheableCursor);
result.notification.forEach(function(notification) {
    console.log("Notification: %o:%o\n%o", notification.code, notification.subject, notification.content)
});
```


**Pagination and cacheable cursors**

Like other listing methods, notification results can be paginated using a cursor or cacheable cursor from the result.

```js
const cacheableCursor = result.cacheable_cursor;
```

The next time the player logs in the cacheable cursor can be used to list unread notifications.

```js
var nextResults = await client.listNotifications(session, limit, cacheableCursor);
```


### Deleting notifications

Sagi-shi players can delete notifications once they've read them:

```js
var notificationId = "<notification id>";
await client.deleteNotifications(session, [notificationId]);
```
