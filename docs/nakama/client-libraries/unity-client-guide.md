# Nakama Unity Client Guide

This client library guide will show you how to use the core Nakama features in **Unity** by showing you how to develop the Nakama specific parts (without full game logic or UI) of an [Among Us (external)](https://www.innersloth.com/games/among-us/) inspired game called Sagi-shi (Japanese for "Imposter").

<figure>
  <img src="../images/gameplay.png" alt="Sagi-shi gameplay screen">
  <figcaption>Sagi-shi gameplay</figcaption>
</figure>

## Prerequisites

Before proceeding ensure that you have:

* [Installed Nakama server](../getting-started/docker-quickstart.md)
* [Downloaded and installed Unity](https://unity.com/download)
* Installed the [Nakama Unity SDK](#installation)

### Installation

The client is available from the:

* [Unity Asset Store](https://assetstore.unity.com/packages/tools/network/nakama-81338)
* [Heroic Labs GitHub releases page](https://github.com/heroiclabs/nakama-unity/releases/latest)

The `Nakama.unitypackage` contains all source code and DLL dependencies required in the client code.

After downloading the file:

* Drag or import it into your Unity project
* Set the editor scripting runtime version to .NET 4.6 (from the **Edit** -> **Project Settings** -> **Player** -> **Configuration** menu).
* From the **Assets** menu create a new C# script and a [client object](#nakama-client)

#### Updates

New versions of the Nakama Unity Client and the corresponding improvements are documented in the [Changelog](https://github.com/heroiclabs/nakama-unity/blob/master/Packages/Nakama/CHANGELOG.md).


### Asynchronous programming

Many of the Nakama APIs are asynchronous and non-blocking and are available in the Unity SDK as async methods.

Sagi-shi calls async methods using the `await` operator to not block the calling thread so that the game is responsive and efficient.

```csharp
await client.AuthenticateDeviceAsync("<DeviceId>");
```

!!! note "Note"
    Read about the [`async` keyword](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/async) and [`await` operator](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/await) in the official .NET C# reference.

Alternatively you can use callbacks with the task's `ContinueWith` method:

```csharp
var task = Nakama.Client.GetAccountAsync(null);

task.ContinueWith(t =>
{
    Debug.LogFormat("Found account for user: {0}", t.Result.User.Username);
}, TaskContinuationOptions.OnlyOnRanToCompletion);

task.ContinueWith(t => {
    Debug.LogFormat("Error: {0}", t.Exception);
}, TaskContinuationOptions.NotOnRanToCompletion);
```

!!! note "Note"
    When using `ContinueWith`, you need to run a second `ContinueWith` with the `TaskContinuationOptions.NotOnRanToCompletion` argument to catch exceptions.


### Handling exceptions

Network programming requires additional safeguarding against connection and payload issues.

API calls in Sagi-shi are surrounded with a try block and a catch clause with a `Nakama.ApiResponseException` object to gracefully handle errors:

```csharp
try
{
    await client.AuthenticateDeviceAsync("<DeviceId>");
}
catch (Nakama.ApiResponseException ex)
{
    Debug.LogFormat("Error authenticating device: {0}:{1}", ex.StatusCode, ex.Message);
}
```


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


## Getting started

Learn how to get started using the Nakama Client and Socket objects to start building Sagi-shi and your own game.

<!-- simple system diagram of Unity Nakama client, Nakama client socket, connecting to Nakama server -->


### Nakama Client

The Nakama Client connects to a Nakama Server and is the entry point to access Nakama features. It is recommended to have one client per server per game.

To create a client for Sagi-shi pass in your server connection details:

```csharp
var client = new Nakama.Client("http", "127.0.0.1", 7350, "defaultKey");
```


### Nakama Socket

The Nakama Socket is used for gameplay and real-time latency-sensitive features such as chat, parties, matches and RPCs.

From the client create a socket:

```csharp
var socket = client.NewSocket();

bool appearOnline = true;
int connectionTimeout = 30;
await socket.ConnectAsync(Session, appearOnline, connectionTimeout);
```


## Authentication

Nakama has many [authentication methods](../concepts/authentication.md) and supports creating [custom authentication](../concepts/authentication.md#custom) on the server.

Sagi-shi will use device and Facebook authentication, linked to the same user account so that players can play from multiple devices.

<figure>
  <img src="../images/login.png" alt="Sagi-shi login screen">
  <figcaption>Login screen and Authentication options</figcaption>
</figure>


### Device authentication

Nakama [Device Authentication](../concepts/authentication/#device) uses the physical device's unique identifier to easily authenticate a user and create an account if one does not exist.

When using only device authentication, you don't need a login UI as the player can automatically authenticate when the game launches.

Authentication is an example of a Nakama feature accessed from a Nakama Client instance.

```csharp
public async void AuthenticateWithDevice()
{
    // If the user's device ID is already stored, grab that - alternatively get the System's unique device identifier.
    var deviceId = PlayerPrefs.GetString("deviceId", SystemInfo.deviceUniqueIdentifier);

    // If the device identifier is invalid then let's generate a unique one.
    if (deviceId == SystemInfo.unsupportedIdentifier)
    {
        deviceId = System.Guid.NewGuid().ToString();
    }

    // Save the user's device ID to PlayerPrefs so it can be retrieved during a later play session for re-authenticating.
    PlayerPrefs.SetString("deviceId", deviceId);

    // Authenticate with the Nakama server using Device Authentication.
    try
    {
        Session = await client.AuthenticateDeviceAsync(deviceId);
        Debug.Log("Authenticated with Device ID");
    }
    catch(ApiResponseException ex)
    {
        Debug.LogFormat("Error authenticating with Device ID: {0}", ex.Message);
    }
}
```


### Facebook authentication

Nakama [Facebook Authentication](../concepts/authentication/#facebook) is an easy to use authentication method which lets you optionally import the player's Facebook friends and add them to their Nakama Friends list.

!!! note "Note"
    Install the [Facebook SDK for Unity (external)](https://developers.facebook.com/docs/unity/) to use Nakama Facebook Authentication.

```csharp
public void AuthenticateWithFacebook()
{
    FB.LogInWithReadPermissions(new[] { "public_profile", "email" }, async result =>
    {
        if (FB.IsLoggedIn)
        {
            try
            {
                var importFriends = true;
                Session = await client.AuthenticateFacebookAsync(AccessToken.CurrentAccessToken.TokenString, importFriends);
                Debug.Log("Authenticated with Facebook");
            }
            catch(ApiResponseException ex)
            {
                Debug.LogFormat("Error authenticating with Facebook: {0}", ex.Message);
            }
        }
    });
}
```


### Custom authentication

Nakama supports [Custom Authentication](../concepts/authentication/#custom) methods to integrate with additional identity services.

See the [Itch.io custom authentication](../server-framework/recipes/itch-authentication) recipe for an example.


### Linking authentication

Nakama allows players to [Link Authentication](../concepts/authentication/#link-or-unlink) methods to their account once they have authenticated.


**Linking Device ID authentication**

```csharp
public async void LinkDeviceAuthentication()
{
    // Acquiring the unique device ID has been shortened for brevity, see previous example.
    var deviceId = "<UniqueDeviceId>";

    // Link Device Authentication to existing player account.
    try
    {
        await client.LinkDeviceAsync(Session, deviceId);
        Debug.Log("Successfully linked Device ID authentication to existing player account");
    }
    catch(ApiResponseException ex)
    {
        Debug.LogFormat("Error linking Device ID: {0}", ex.Message);
    }
}
```


**Linking Facebook authentication**

```csharp
public void LinkFacebookAuthentication(bool importFriends = true)
{
    FB.LogInWithReadPermissions(new[] { "public_profile", "email" }, async result =>
    {
        if (FB.IsLoggedIn)
        {
            try
            {
                var importFriends = true;
                await client.LinkFacebookAsync(Session, AccessToken.CurrentAccessToken.TokenString, importFriends);
                Debug.Log("Successfully linked Facebook authentication to existing player account");
            }
            catch(ApiResponseException ex)
            {
                Debug.LogFormat("Error authenticating with Facebook: {0}", ex.Message);
            }
        }
    });
}
```


### Session variables

Nakama [Session Variables](../concepts/session/#session-variables) can be stored when authenticating and will be available on the client and server as long as the session is active.

Sagi-shi uses session variables to implement analytics, referral and rewards programs and more.

Store session variables by passing them as an argument when authenticating:

```csharp
var vars = new Dictionary<string, string>();
vars["DeviceOS"] = SystemInfo.operatingSystem;
vars["DeviceModel"] = SystemInfo.deviceModel;
vars["GameVersion"] = Application.version;
vars["InviterUserId"] = "<SomeUserId>";

/// ...

var session = await client.AuthenticateDeviceAsync("<DeviceId>", null, true, vars);

```
To access session variables on the Client use the `Vars` property on the `Session` object:

```csharp
var deviceOs = session.Vars["DeviceOS"];
```


### Session lifecycle

Nakama [Sessions](../concepts/session/) expire after a time set in your server [configuration](../getting-started/configuration.md#session). Expiring inactive sessions is a good security practice.

Nakama provides ways to restore sessions, for example when Sagi-shi players re-launch the game, or refresh tokens to keep the session active while the game is being played.

Use the auth and refresh tokens on the session object to restore or refresh sessions.

Sagi-shi stores these tokens in Unity's player preferences:

```csharp
PlayerPrefs.SetString("nakama.authToken", session.AuthToken);
PlayerPrefs.SetString("nakama.refreshToken", session.RefreshToken);
```

Restore a session without having to re-authenticate:

```csharp
var authToken = PlayerPrefs.GetString("nakama.authToken", null);
var refreshToken = PlayerPrefs.GetString("nakama.refreshToken", null);
session = Session.Restore(authToken, refreshToken);
```

Check if a session has expired or is close to expiring and refresh it to keep it alive:

```csharp
// Check whether a session has expired or is close to expiry.
if (session.IsExpired || session.HasExpired(DateTime.UtcNow.AddDays(1))) {
    try {
        // Attempt to refresh the existing session.
        session = await client.SessionRefreshAsync(session);
    } catch (ApiResponseException) {
        // Couldn't refresh the session so reauthenticate.
        session = await client.AuthenticateDeviceAsync(deviceId);
        PlayerPrefs.SetString("nakama.refreshToken", session.RefreshToken);
    }

    PlayerPrefs.SetString("nakama.authToken", session.AuthToken);
}
```


### Ending sessions

Logout and end the current session:

```csharp
await client.SessionLogoutAsync(session);
```


## User accounts

Nakama [User Accounts](../concepts/user-accounts.md) store user information defined by Nakama and custom developer metadata.

Sagi-shi allows players to edit their accounts and stores metadata for things like game progression and in-game items.

<figure>
  <img src="../images/profile.png" alt="Sagi-shi player profile screen">
  <figcaption>Player profile</figcaption>
</figure>


### Get the user account

Many of Nakama's features are accessible with an authenticated session, like [fetching a user account](../concepts/user-accounts/#fetch-account).

Get a Sagi-shi player's full user account with their basic [user information](../concepts/user-accounts/#fetch-account) and user id:

```csharp
var account = await client.GetAccountAsync(session);
var username = account.User.Username;
var avatarUrl = account.User.AvatarUrl;
var userId = account.User.Id;
```


### Update the user account

Nakama provides easy methods to update server stored resources like user accounts.

Sagi-shi players need to be able to update their public profiles:

```csharp
var newUsername = "NotTheImp0ster";
var newDisplayName = "Innocent Dave";
var newAvatarUrl = "https://example.com/imposter.png";
var newLangTag = "en";
var newLocation = "Edinburgh";
var newTimezone = "BST";
await client.UpdateAccountAsync(session, newUsername, newDisplayName, newAvatarUrl, newLangTag, newLocation, newTimezone);
```


### Getting users

In addition to getting the current authenticated player's user account, Nakama has a convenient way to get a list of other players' public profiles from their ids or usernames.

Sagi-shi uses this method to display player profiles when engaging with other Nakama features:

```csharp
var users = await client.GetUsersAsync(session, new string[] { "<AnotherUserId>" });
```


### Storing metadata

Nakama [User Metadata](../concepts/user-accounts/#user-metadata) allows developers to extend user accounts with public user fields.

User metadata can only be updated on the server. See the [updating user metadata](../server-framework/recipes/updating-user-metadata) recipe for an example.

Sagi-shi will use metadata to store what in-game items players have equipped:


### Reading metadata

Define a class that describes the metadata and parse the JSON metadata:

```csharp
public class Metadata
{
    public string Title;
    public string Hat;
    public string Skin;
}

// Get the updated account object.
var account = await client.GetAccountAsync(session);

// Parse the account user metadata.
var metadata = Nakama.TinyJson.JsonParser.FromJson<Metadata>(account.User.Metadata);

Debug.LogFormat("Title: {0}", metadata.Title);
Debug.LogFormat("Hat: {0}", metadata.Hat);
Debug.LogFormat("Skin: {0}", metadata.Skin);
```


### Wallets

Nakama [User Wallets](../concepts/user-accounts/#virtual-wallet) can store multiple digital currencies as key/value pairs of strings/integers.

Players in Sagi-shi can unlock or purchase titles, skins and hats with a virtual in-game currency.


#### Accessing wallets

Parse the JSON wallet data from the user account:

```csharp
var account = await client.GetAccountAsync(session);
var wallet = JsonParser.FromJson<Dictionary<string, int>>(account.Wallet);

foreach (var currency in wallet.Keys)
{
    Debug.LogFormat("{0}: {1}", currency, wallet[currency].ToString());
}
```


#### Updating wallets

Wallets can only be updated on the server. See the [user account virtual wallet](../concepts/user-accounts.md#virtual-wallet) documentation for an example.


#### Validating in-app purchases

Sagi-shi players can purchase the virtual in-game currency through in-app purchases that are authorized and validated to be legitimate on the server.

See the [In-app Purchase Validation](../concepts/iap-validation/) documentation for examples.


## Storage Engine

The Nakama [Storage Engine](../concepts/collections.md) is a distributed and scalable document-based storage solution for your game.

The Storage Engine gives you more control over how data can be [accessed](../concepts/access-controls.md#object-permissions) and [structured](../concepts/collections/#collections) in collections.

Collections are named, and store JSON data under a unique key and the user id.

By default, the player has full permission to create, read, update and delete their own storage objects.

Sagi-shi players can unlock or purchase many items, which are stored in the Storage Engine.

<figure>
  <img src="../images/player_items.png" alt="Sagi-shi player items screen">
  <figcaption>Player items</figcaption>
</figure>


### Reading storage objects

Define a class that describes the storage object and create a new storage object id with the collection name, key and user id. Finally, read the storage objects and parse the JSON data:

```csharp
public class HatsStorageObject
{
    public string[] Hats;
}

var readObjectId = new StorageObjectId
{
    Collection = "Unlocks",
    Key = "Hats",
    UserId = session.UserId
};

var result = await client.ReadStorageObjectsAsync(session, readObjectId);

if (result.Objects.Any())
{
    var storageObject = result.Objects.First();
    var unlockedHats = JsonParser.FromJson<HatsStorageObject>(storageObject.Value);
    Debug.LogFormat("Unlocked hats: {0}", string.Join(",", unlockedHats.Hats));
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

```csharp
var favoriteHats = new HatsStorageObject
{
    Hats = new string[] { "cowboy", "alien"}
};

var writeObject = new WriteStorageObject
{
    Collection = "favorites",
    Key = "Hats",
    Value = JsonWriter.ToJson(favoriteHats),
    PermissionRead = 1, // Only the server and owner can read
    PermissionWrite = 1, // The server and owner can write
};

await client.WriteStorageObjectsAsync(session, writeObject);
```

You can also pass multiple objects to the `WriteStorageObjectsAsync` method:

```csharp
var writeObjects = new[] {
    new WriteStorageObject {
        //...
    },
    new WriteStorageObject
    {
        // ...
    }
};

await client.WriteStorageObjectsAsync(session, writeObjects);
```


### Conditional writes

Storage Engine [Conditional Writes](../concepts/collections/#conditional-writes) ensure that write operations only happen if the object hasn't changed since you accessed it.

This gives you protection from overwriting data, for example the Sagi-shi server could have updated an object since the player last accessed it.

To perform a conditional write, add a version to the write storage object with the most recent object version:

```csharp
// Assuming we already have a storage object (storageObject)
var writeObject = new WriteStorageObject
{
    Collection = storageObject.Collection,
    Key = storageObject.Key,
    Value = "<NewJSONValue>",
    PermissionWrite = 0,
    PermissionRead = 1,
    Version = storageObject.Version
};

try
{
    await client.WriteStorageObjectsAsync(session, writeObjects);
}
catch (ApiResponseException ex)
{
    Debug.Log(ex.Message);
}
```


### Listing storage objects

Instead of doing multiple read requests with separate keys you can list all the storage objects the player has access to in a collection.

Sagi-shi lists all the player's unlocked or purchased titles, hats and skins:

```csharp
var limit = 3;
var unlocksObjectList = await client.ListStorageObjectsAsync(session, "Unlocks", limit, cursor: null);

foreach (var unlockStorageObject in unlocksObjectList.Objects)
{
    switch(unlockStorageObject.Key)
    {
        case "Titles":
            var unlockedTitles = JsonParser.FromJson<TitlesStorageObject>(unlockStorageObject.Value);
            // Display the unlocked titles
            break;
        case "Hats":
            var unlockedHats = JsonParser.FromJson<HatsStorageObject>(unlockStorageObject.Value);
            // Display the unlocked hats
            break;
        case "Skins":
            var unlockedSkins = JsonParser.FromJson<SkinsStorageObject>(unlockStorageObject.Value);
            // Display the unlocked skins
            break;
    }
}
```


### Paginating results

Nakama methods that list results return a cursor which can be passed to subsequent calls to Nakama to indicate where to start retrieving objects from in the collection.

For example:
- If the cursor has a value of 5, you will get results from the fifth object.
- If the cursor is `null`, you will get results from the first object.

```csharp
objectList = await client.ListStorageObjectsAsync(session, "<CollectionName>", limit, objectList.Cursor);
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

```csharp
try
{
    var payload = new Dictionary<string, string> {{ "item", "cowboy" }};
    var response = await client.RpcAsync(session, "EquipHat", payload.ToJson());
    Debug.Log("New hat equipped successfully", response);
}
catch (ApiResponseException ex)
{
    Debug.LogFormat("Error: {0}", ex.Message);
}
```


### Socket RPCs

Nakama Remote Procedures can also be called form the socket when you need to interface with Nakama's real-time functionality.

<!-- todo: the explanation of how this interfaces with real-time functionality is not clear -->

```csharp
var response = await socket.RpcAsync("<RpcId>", "<PayloadString>");
```


## Friends

Nakama [Friends](../concepts/friends/) offers a complete social graph system to manage friendships amongst players.

Sagi-shi allows players to add friends, manage their relationships and play together.

<figure>
  <img src="../images/friends.png" alt="Sagi-shi Friends screen">
  <figcaption>Friends screen</figcaption>
</figure>


### Adding friends

Adding a friend in Nakama does not immediately add a mutual friend relationship. An outgoing friend request is created to each user, which they will need to accept.

Sagi-shi allows players to add friends by their usernames or user ids:


```csharp
// Add friends by Username.
await client.AddFriendsAsync(session, null, new[] { "AlwaysTheImposter21", "SneakyBoi" });

// Add friends by User ID.
await client.AddFriendsAsync(session, new[] { "<SomeUserId>", "<AnotherUserId>" });
```


### Friendship states

Nakama friendships are categorized with the following [states](../concepts/friends.md#friend-state):

| Value | State |
| ----- | ----- |
| 0 | Mutual friends |
| 1 | An outgoing friend request pending acceptance |
| 2 | An incoming friend request pending acceptance |
| 4 | Banned |


### Listing friends

Nakama allows developers to list the player's friends based on their friendship state.

Sagi-shi lists the 20 most recent mutual friends:

```csharp
var limit = 20; // Limit is capped at 1000
var frienshipState = 0;
var result = await client.ListFriendsAsync(session, frienshipState, limit, cursor: null);

foreach (var friend in result.Friends)
{
    Debug.LogFormat("ID: {0}", friend.User.Id);
}
```


### Accepting friend requests

When accepting a friend request in Nakama the player adds a [bi-directional friend relationship](../concepts/friends-best-practices/#modeling-relationships).

Nakama takes care of changing the state from pending to mutual for both.

In a complete game you would allow players to accept individual requests.

Sagi-shi just fetches and accepts all the incoming friend requests:

```csharp
var limit = 1000;
var result = await client.ListFriendsAsync(session, 2, limit, cursor: null);

foreach (var friend in result.Friends)
{
    await client.AddFriendsAsync(session, new[] { friend.User.Id });
}
```


### Deleting friends

Sagi-shi players can remove friends by their username or user id:

```csharp
// Delete friends by User ID.
await client.DeleteFriendsAsync(session, new[] { "<SomeUserId>", "<AnotherUserId>" });

// Delete friends by Username.
await client.DeleteFriendsAsync(session, null, new[] { "<SomeUsername>", "<AnotherUsername>" });
```


### Blocking users

Sagi-shi players can block others by their username or user id:

<!-- todo: how are blocked friends represented? What are their friendship states or structure. -->


```csharp
// Block friends by User ID.
await client.BlockFriendsAsync(session, new[] { "<SomeUserId>", "<AnotherUserId>" });

// Block friends by Username.
await client.BlockFriendsAsync(session, null, new[] { "<SomeUsername>", "<AnotherUsername>" });
```

<!-- todo: how to list blocked users -->


## Status & Presence

Nakama [Status & Presence](../concepts/status/) is has a real-time status and presence service that allows users to set their online presence, update their status message and follow other user's updates.

Players don't have to be friends with others they want to follow.

Sagi-shi uses status messages and online presences to notify players when their friends are online and share matches.

<figure>
  <img src="../images/status.png" alt="Sagi-shi status update screen">
  <figcaption>Updating player status</figcaption>
</figure>

### Follow users

The Nakama real-time APIs allow developers to subscribe to events on the socket, like a status presence change, and receive them in real-time.

The method to follow users also returns the current online users, known as presences, and their status.

<!-- todo: follow users query needs explaining -->

Sagi-shi follows a player's friends and notifies them when they are online:

```csharp
// Subscribe to the Status event.
socket.ReceivedStatusPresence += e =>
{
    foreach (var presence in e.Joins)
    {
        Debug.LogFormat("{0} is online with status: '{1}'", presence.Username, presence.Status);
    }

    foreach (var presence in e.Leaves)
    {
        Debug.LogFormat("{0} went offline", presence.Username);
    }
};

// Follow mutual friends and get the initial Status of any that are currently online.
var friendsResult = await client.ListFriendsAsync(session, 0);
var friendIds = friendsResult.Friends.Select(f => f.User.Id);
var result = await socket.FollowUsersAsync(friendIds);

foreach (var presence in result.Presences)
{
    Debug.LogFormat("{0} is online with status: {1}", presence.Username, presence.Status);
}
```


### Unfollow users

Sagi-shi players can unfollow others:

```csharp
await socket.UnfollowUsersAsync(new[] { "<UserId>" });
```


### Updating player status

Sagi-shi players can change and publish their status to their followers:

```csharp
await socket.UpdateStatusAsync("Viewing the Main Menu");
```


## Groups

Nakama [Groups](../concepts/groups-clans/) is a group or clan system with public/private visibility, user memberships and permissions, metadata and group chat.

Sagi-shi allows players to form and join groups to socialize and compete.

<figure>
  <img src="../images/groups_list.png" alt="Sagi-shi groups screen">
  <figcaption>Groups list screen</figcaption>
</figure>


### Creating groups

Groups have a public or private "open" visibility. Anyone can join public groups, but they must request to join and be accepted by a superadmin/admin of a private group.

Sagi-shi players can create groups around common interests:

```csharp
var name = "Imposters R Us";
var description = "A group for people who love playing the imposter.";
var open = true; // public group
var maxSize = 100;

var group = await client.CreateGroupAsync(session, name, description, avatarUrl: null, langTag: null, open, maxSize);
```


### Update group visibility

Nakama allows group superadmin or admin members to update some properties from the client, like the open visibility:

```csharp
var open = false;
await client.UpdateGroupAsync(session, "<GroupId>", name: null, open);
```


### Update group size

Other properties, like the group's maximum member size, can only be changed on the server.

See the [updating group size](../concepts/groups-clans.md#updating-group-size) recipe for an example, and the [Groups server function reference](../server-framework/function-reference.md#groups) to learn more about updating groups on the server.

<figure>
  <img src="../images/group_edit.png" alt="Sagi-shi group edit screen">
  <figcaption>Sagi-shi group edit</figcaption>
</figure>


### Listing and filtering groups

Groups can be listed like other Nakama resources and also [filtered](../concepts/groups-clans/#list-and-filter-groups) with a wildcard group name.

Sagi-shi players use group listing and filtering to search for existing groups to join:


```csharp
var limit = 20;
var result = await client.ListGroupsAsync(session, "imposter%", limit);

foreach (var group in result.Groups)
{
    Debug.LogFormat("{0} [{1}]", group.Name, group.Open ? "Public" : "Private")
}

// Get the next page of results.
var nextResults = await client.ListGroupsAsync(session, name: "imposter%", limit, result.Cursor);
```


### Deleting groups

Nakama allows group superadmins to delete groups.

Developers can disable this feature entirely, see the [Guarding APIs guide](../guides/guarding-apis/) for an example on how to protect various Nakama APIs.

Sagi-shi players can delete groups which they are superadmins for:

```csharp
await client.DeleteGroupAsync(session, "<GroupId>");
```


### Group metadata

Like Users Accounts, Groups can have public metadata.

Sagi-shi uses group metadata to store the group's interests, active player times and languages spoken.

Group metadata can only be updated on the server. See the [updating group metadata](../server-framework/recipes/updating-group-metadata) recipe for an example.

The Sagi-shi client makes an RPC with the group metadata payload:

```csharp
var payload = new UpdateGroupMetadataPayload
{
    GroupId = "<GroupId>",
    Interests = new[] { "Deception", "Sabotage", "Cute Furry Bunnies" },
    ActiveTimes = new[] { "9am-2pm Weekdays", "9am-10pm Weekends" },
    Languages = new[] { "English", "German" }
};

try
{
    var result = await client.RpcAsync(session, "UpdateGroupMetadata", JsonWriter.ToJson(payload));
    Debug.Log("Successfully updated group metadata");
}
catch (ApiResponseException ex)
{
    Debug.LogFormat("Error: {0}", ex.Message);
}
```


### Group membership states

Nakama group memberships are categorized with the following [states](../concepts/groups-clans/#groups-and-clans):

| Code | Purpose | |
| ---- | ------- | - |
|    0 | Superadmin | There must at least be 1 superadmin in any group. The superadmin has all the privileges of the admin and can additionally delete the group and promote admin members. |
|    1 | Admin | There can be one of more admins. Admins can update groups as well as accept, kick, promote, demote, ban or add members. |
|    2 | Member | Regular group member. They cannot accept join requests from new users. |
|    3 | Join request | A new join request from a new user. This does not count towards the maximum group member count. |


### Joining a group

If a player joins a public group they immediately become a member, but if they try and join a private group they must be accepted by a group admin.

Sagi-shi players can join a group:

```csharp
await client.JoinGroupAsync(session, "<GroupId>");
```


### Listing the user's groups

Sagi-shi players can list groups they are a member of:

```csharp
var results = await client.ListUserGroupsAsync(session);

foreach (var userGroup in results.UserGroups)
{
    Debug.LogFormat("{0}: {1}", userGroup.Group.Name, userGroup.State);
}
```


### Listing members

Sagi-shi players can list a group's members:

```csharp
var result = await client.ListGroupUsersAsync(session, "<GroupId>");

foreach (var groupUser in result.GroupUsers)
{
    Debug.LogFormat("{0}: {1}", groupUser.User.Id, groupUser.State);
}
```


### Accepting join requests

Private group admins or superadmins can accept join requests by re-adding the user to the group.

Sagi-shi first lists all the users with a join request state and then loops over and adds them to the group:


```csharp
var result = await client.ListGroupUsersAsync(session, "<GroupId>", 3);

foreach (var groupUser in result.GroupUsers)
{
    await client.AddGroupUsersAsync(session, "<GroupId>", new[] { groupUser.User.Id });
}
```


### Promoting members

Nakama group members can be promoted to admin or superadmin roles to help manage a growing group or take over if members leave.

Admins can promote other members to admins, and superadmins can promote other members up to superadmins.

The members will be promoted up one level. For example:

- Promoting a member will make them an admin
- Promoting an admin will make them a superadmin

```csharp
await client.PromoteGroupUsersAsync(session, "<GroupId>", new[] { "UserId" });
```


### Demoting members

Sagi-shi group admins and superadmins can demote members:

```csharp
await client.DemoteGroupUsersAsync(session, "<GroupId>", new[] { "UserId" });
```


### Kicking members

Sagi-shi group admins and superadmins can remove group members:

```csharp
await client.KickGroupUsersAsync(session, "<GroupId>", new[] { "UserId" });
```


### Banning members

Sagi-shi group admins and superadmins can ban a user when demoting or kicking is not severe enough:

```csharp
await client.BanGroupUsersAsync(session, "<GroupId>", new[] { "UserId" });
```

### Leaving groups

Sagi-shi players can leave a group:

```csharp
await client.LeaveGroupAsync(session, "<GroupId>");
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

```csharp
var roomName = "<MatchId>";
var persistence = false;
var hidden = false;
var channel = await socket.JoinChatAsync(roomName, ChannelType.Room, persistence, hidden);

Debug.LogFormat("Connected to dynamic room channel: {0}", channel.Id);
```


### Joining group chat

Sagi-shi group members can have conversations that span play sessions in a persistent group chat channel:

```csharp
var groupId = "<GroupId>";
var persistence = true;
var hidden = false;
var channel = await socket.JoinChatAsync(groupId, ChannelType.Group, persistence, hidden);

Debug.LogFormat("Connected to group channel: {0}", channel.Id);
```


### Joining direct chat

Sagi-shi players can also chat privately one-to-one during or after matches and view past messages:

```csharp
var userId = "<UserId>";
var persistence = true;
var hidden = false;
var channel = await socket.JoinChatAsync(userId, ChannelType.DirectMessage, persistence, hidden);

Debug.LogFormat("Connected to direct message channel: {0}", channel.Id);
```


### Sending messages

Sending messages is the same for every type of chat channel. Messages contain chat text and emotes and are sent as JSON serialized data:

```csharp
var channelId = "<ChannelId>"

var messageContent = new Dictionary<string, string> {
    { "message", "I think Red is the imposter!" }
};

var messageSendAck = await socket.WriteChatMessageAsync(channelId, JsonWriter.ToJson(messageContent));

var emoteContent = new Dictionary<string, string> {
    { "emote", "point" },
    { "emoteTarget", "<RedPlayerUserId>" }
};

var emoteSendAck = await socket.WriteChatMessageAsync(channelId, JsonWriter.ToJson(emoteContent));
```


### Listing message history

Message listing takes a parameter which indicates if messages are received from oldest to newest (forward) or newest to oldest.

Sagi-shi players can list a group's message history:

```csharp
var limit = 100;
var forward = true;
var groupId = "<GroupId>";
var result = await client.ListChannelMessagesAsync(session, groupId, limit, forward, cursor: null);

foreach (var message in result.Messages)
{
    Debug.LogFormat("{0}:{1}", message.Username, message.Content);
}
```

Chat also has cacheable cursors to fetch the most recent messages, which you can store in `PlayerPrefs`.

<!-- todo: is this the first time we cover a feature that has a cacheable cursor? -->

```csharp
PlayerPrefs.SetString(string.Format("nakama.groupMessagesCacheableCursor_{0}", groupId), result.CacheableCursor);

var cacheableCursor = PlayerPrefs.GetString(string.Format("nakama.groupMessagesCacheableCursor_{0}", groupId), null);
var nextResults = await client.ListChannelMessagesAsync(session, groupId, limit, forward, cacheableCursor);
```


### Updating messages

Nakama also supports updating messages. It is up to you whether you want to use this feature, but in a game of deception like Sagi-shi it can add an extra element of deception.

For example a player sends the following message:

```csharp
var channelId = "<ChannelId>"
var messageContent = new Dictionary<string, string> {
    { "message", "I think Red is the imposter!" }
};
var messageSendAck = await socket.WriteChatMessageAsync(channelId, JsonWriter.ToJson(messageContent));
```

They then quickly edit their message to confuse others:

```csharp
var newMessageContent = new Dictionary<string, string> {
    { "message", "I think BLUE is the imposter!" }
};
var messageUpdateAck = await socket.UpdateChatMessageAsync(channelId, messageSendAck.MessageId, JsonWriter.ToJson(newMessageContent));
```


## Matches

Nakama supports [Server Authoritative](../concepts/server-authoritative-multiplayer.md) and [Server Relayed](../concepts/client-relayed-multiplayer.md) multiplayer matches.

In server authoritative matches the server controls the gameplay loop and must keep all clients up to date with the current state of the game.

In server relayed matches the client is in control, with the server only relaying information to the other connected clients.

In a competitive game such as Sagi-shi, server authoritative matches would likely be used to prevent clients from interacting with your game in unauthorized ways.

For the simplicity of this guide, the server relayed model is used.

<!-- flow chat of server relayed matches, to be fleshed out -->


### Creating matches

Sagi-shi players can create their own matches and invite their online friends to join:

```csharp
var match = await socket.CreateMatchAsync();
var friendsList = await client.ListFriendsAsync(session, 0, 100);
var onlineFriends = friendsList.Friends.Where(f => f.User.Online).Select(f => f.User);

foreach (var friend in onlineFriends)
{
    var content = new
    {
        message = string.Format("Hey {0}, join me for a match!", friend.Username),
        matchId = match.Id
    };

    var channel = await socket.JoinChatAsync(friend.Id, ChannelType.DirectMessage);
    var messageAck = await socket.WriteChatMessageAsync(channel, JsonWriter.ToJson(content));
}
```

### Joining matches

Sagi-shi players can try to join existing matches if they know the id:
```csharp
var matchId = "<MatchId>";
var match = await socket.JoinMatchAsync(matchId);
```

Or set up a real-time matchmaker listener and add themselves to the matchmaker:

```csharp
socket.ReceivedMatchmakerMatched += async matchmakerMatched => {
    var match = await socket.JoinMatchAsync(matchmakerMatched);
};

var minPlayers = 2;
var maxPlayers = 10;
var query = "";

var matchmakingTicket = await socket.AddMatchmakerAsync(query, minPlayers, maxPlayers);
```


**Joining matches from player status**

Sagi-shi players can update their status when they join a new match:

```csharp
var status = new Dictionary<string, string>
{
    { "Status", "Playing a match" },
    { "MatchId", "<MatchId>" }
};

await socket.UpdateStatusAsync(JsonWriter.ToJson(status));
```

When their followers receive the real-time status event they can try and join the match:

```csharp
socket.ReceivedStatusPresence += async e =>
{
    // Join the first match found in a friend's status
    foreach (var presence in e.Joins)
    {
        var status = JsonParser.FromJson<Dictionary<string, string>>(presence.Status);
        if (status.ContainsKey("MatchId"))
        {
            await socket.JoinMatchAsync(status["MatchId"]);
            break;
        }
    }
};
```

### Listing matches

Match Listing takes a number of criteria to filter matches by including player count, a match [label](../concepts/server-authoritative-multiplayer/#match-label) and an option to provide a more complex [search query](../concepts/server-authoritative-multiplayer.md#search-query).

<!-- todo: we don't have concept or client side docs for match listing labels or search queries -->

Sagi-shi matches start in a lobby state. The match exists on the server but the actual gameplay doesn't start until enough players have joined.

Sagi-shi can then list matches that are waiting for more players:

```csharp
var minPlayers = 2;
var maxPlayers = 10;
var limit = 10;
var authoritative = true;
var label = "";
var query = "";
var result = await client.ListMatchesAsync(session, minPlayers, maxPlayers, limit, authoritative, label, query);

foreach (var match in result.Matches)
{
    Debug.LogFormat("{0}: {1}/10 players", match.MatchId, match.Size);
}
```

To find a match that has a label of `"AnExactMatchLabel"`:

```csharp
var label = "AnExactMatchLabel";
```

**Advanced:**

In order to use a more complex structured query, the match label must be in JSON format.

To find a match where it expects player skill level to be `>100` and optionally has a game mode of `"sabotage"`:

```csharp
var query = "+label.skill:>100 label.mode:sabotage"
```


### Spawning players

The match object has a list of current online users, known as presences.

Sagi-shi uses the match presences to spawn players on the client:

```csharp
var match = await socket.JoinMatchAsync(matchId);

var players = new Dictionary<string, GameObject>();

foreach (var presence in match.Presences)
{
    // Spawn a player for this presence and store it in a dictionary by session id.
    var go = Instantiate(playerPrefab);
    players.Add(presence.SessionId, go);
}
```

Sagi-shi keeps the spawned players up-to-date as they leave and join the match using the match presence received event:

```csharp
socket.ReceivedMatchPresence += matchPresenceEvent =>
{
    // For each player that has joined in this event...
    foreach (var presence in matchPresenceEvent.Joins)
    {
        // Spawn a player for this presence and store it in a dictionary by session id.
        var go = Instantiate(playerPrefab);
        players.Add(presence.SessionId, go);
    }

    // For each player that has left in this event...
    foreach (var presence in matchPresenceEvent.Leaves)
    {
        // Remove the player from the game if they've been spawned
        if (players.ContainsKey(presence.SessionId))
        {
            Destroy(players[presence.SessionId]);
            players.Remove(presence.SessionId);
        }

    }
};
```


### Sending match state

Nakama has real-time networking to [send](../concepts/client-relayed-multiplayer/#send-data-messages) and [receive](../concepts/client-relayed-multiplayer/#receive-data-messages) match state as players move and interact with the game world.

During the match, each Sagi-shi client sends match state to the server to be relayed to the other clients.

Match state contains an op code that lets the receiver know what data is being received so they can deserialize it and update their view of the game.

Example op codes used in Sagi-shi:
- 1: player position
- 2: player calling vote


**Sending player position**

Define a class to represent Sagi-shi player position states:

```csharp
[Serializable]
public class PositionState
{
    public float X;
    public float Y;
    public float Z;
}
```

Create an instance from the player's transform, set the op code and send the JSON encoded state:

```csharp
var state = new PositionState
{
    X = transform.position.x,
    Y = transform.position.y,
    Z = transform.position.z
};

var opCode = 1;

await socket.SendMatchStateAsync(match.Id, opCode, JsonWriter.ToJson(state));
```


**Op Codes as a static class**

Sagi-shi has many networked game actions. Using a static class of constants for op codes will keep your code easier to follow and maintain:

```csharp
public static class OpCodes
{
    public const long Position = 1;
    public const long Vote = 2;
}

await socket.SendMatchStateAsync(match.Id, OpCodes.Position, JsonWriter.ToJson(state));
```


### Receiving match sate

Sagi-shi players can receive match data from the other connected clients by subscribing to the match state received event:

```csharp
socket.ReceivedMatchState += matchState =>
{
    switch (matchState.OpCode)
    {
        case OpCodes.Position:
            // Get the updated position data
            var stateJson = Encoding.UTF8.GetString(matchState.State);
            var positionState = JsonParser.FromJson<PositionState>(stateJson);

            // Update the GameObject associated with that player
            if (players.ContainsKey(matchState.UserPresence.SessionId))
            {
                // Here we would normally do something like smoothly interpolate to the new position, but for this example let's just set the position directly.
                players[matchState.UserPresence.SessionId].transform.position = new Vector3(positionState.X, positionState.Y, positionState.Z);
            }
            break;
        default:
            Debug.Log("Unsupported op code");
            break;
    }
};
```


## Matchmaker

Developers can find matches for players using Match Listing or the Nakama [Matchmaker](../concepts/matches/), which enables players join the real-time matchmaking pool and be notified when they are matched with other players that match their specified criteria.

Matchmaking helps players find each other, it does not create a match. This decoupling is by design, allowing you to use matchmaking for more than finding a game match. For example, if you were building a social experience you could use matchmaking to find others to chat with.

<!-- matchmker flow chart, to be fleshed out -->


### Add matchmaker

Matchmaking criteria can be simple, find 2 players, or more complex, find 2-10 players with a minimum skill level interested in a specific game mode.

Sagi-shi allows players to join the matchmaking pool and have the server match them with other players:

```csharp
var minPlayers = 2;
var maxPlayers = 10;
var query = "+skill:>100 mode:sabotage";
var stringProperties = new Dictionary<string, string> { { "mode", "sabotage" }};
var numericProperties = new Dictionary<string, double> { { "skill", 125 }};
var matchmakerTicket = await socket.AddMatchmakerAsync(query, minPlayers, maxPlayers, stringProperties, numericProperties);
```

<!-- todo: joining a match from a matchmaker ticket, we have an example for parties but could do with a stand alone one here to complete the example  -->

<!-- todo: add a stand alone guide/recipe for widening the matchmaker criteria as it's a more complex example to include here -->


## Parties

Nakama [Parties](../concepts/parties/) is a real-time system that allows players to form short lived parties that don't persist after all players have disconnected.

Sagi-shi allows friends to form a party and matchmake together.

<!-- parties flow chart or Sagi-shi mockup to be fleshed out -->


### Creating parties

The player who creates the party is the party's leader. Parties have maximum number of players and can be open to automatically accept players or closed so that the party leader can accept incoming join requests.

Sagi-shi uses closed parties with a maximum of 4 players:

```csharp
var open = false;
var maxPlayers = 4;
var party = await socket.CreatePartyAsync(open, maxPlayers);
```

Sagi-shi shares party ids with friends via private/direct messages:

```csharp
var friendsList = await client.ListFriendsAsync(session, 0, 100);
var onlineFriends = friendsList.Friends.Where(f => f.User.Online).Select(f => f.User);

foreach (var friend in onlineFriends)
{
    var content = new
    {
        message = string.Format("Hey {0}, wanna join the party?!", friend.Username),
        partyId = party.Id
    };

    var channel = await socket.JoinChatAsync(friend.Id, ChannelType.DirectMessage);
    var messageAck = await socket.WriteChatMessageAsync(channel, JsonWriter.ToJson(content));
}
```


### Joining parties

Safi-shi players can join parties from chat messages by checking for the party id in the message:

```csharp
socket.ReceivedChannelMessage += async m =>
{
    var content = JsonParser.FromJson<Dictionary<string, string>>(m.Content);
    if (content.ContainsKey("partyId"))
    {
        await socket.JoinPartyAsync(content["partyId"]);
    }
};
```


### Promoting a member

Sagi-shi party members can be promoted to the party leader:

```csharp
var newLeader = party.Presences.Where(p => p.SessionId != party.Leader.SessionId).First();
await socket.PromotePartyMemberAsync(party.Id, newLeader);
```

### Leaving parties

Sagi-shi players can leave parties:

```csharp
await socket.LeavePartyAsync(party.Id);
```


### Matchmaking with parties

One of the main benefits of joining a party is that all the players can join the matchmaking pool together.

Sagi-shi players can listen to the the matchmaker matched event and join the match when one is found:

```csharp
socket.ReceivedMatchmakerMatched += async matchmakerMatched =>
{
    await socket.JoinMatchAsync(matchmakerMatched.MatchId);
};
```

The party leader will start the matchmaking for their party:

```csharp
var minPlayers = 2;
var maxPlayers = 10;
var query = "";
var matchmakerTicket = await socket.AddMatchmakerPartyAsync("<PartyId>", query, minPlayers, maxPlayers);
```


## Leaderboards

Nakama [Leaderboards](../concepts/leaderboards/) introduce a competitive aspect to your game and increase player engagement and retention.

Sagi-shi has a leaderboard of weekly imposter wins, where player scores increase each time they win, and similarly a leaderboard for weekly crew member wins.

<figure>
  <img src="../images/leaderboard.png" alt="Sagi-shi leaderboard screen">
  <figcaption>Sagi-shi Leaderboard</figcaption>
</figure>


### Creating leaderboards

Leaderboards have to be created on the server, see the [leaderboard](../concepts/leaderboards.md#create-a-leaderboard) documentation for details on creating leaderboards.


### Submitting scores

When players submit scores, Nakama will increment the player's existing score by the submitted score value.

Along with the score value, Nakama also has a subscore, which can be used for ordering when the scores are the same.

Sagi-shi players can submit scores to the leaderboard with contextual metadata, like the map the score was achieved on:

```csharp
var score = 1;
var subscore = 0;
var metadata = new Dictionary<string, string> {{ "map", "space_station" }};
await client.WriteLeaderboardRecordAsync(session, "weekly_imposter_wins", score, subscore, JsonWriter.ToJson(metadata));
```

### Listing the top records

Sagi-shi players can list the top records of the leaderboard:

```csharp
var limit = 20;
var leaderboardName = "weekly_imposter_wins";
var result = await client.ListLeaderboardRecordsAsync(session, leaderboardName, ownerIds: null, expiry: null, limit, cursor: null);

foreach (var record in result.Records)
{
    Debug.LogFormat("{0}:{1}", record.OwnerId, record.Score);
}
```


**Listing records around the user**

Nakama allows developers to list leaderboard records around a player.

Sagi-shi gives players a snapshot of how they are doing against players around them:

```csharp
var limit = 20;
var leaderboardName = "weekly_imposter_wins";
var result = await client.ListLeaderboardRecordsAroundOwnerAsync(session, leaderboardName, session.UserId, expiry: null, limit);

foreach (var record in result.Records)
{
    Debug.LogFormat("{0}:{1}", record.OwnerId, record.Score);
}
```


**Listing records for a list of users**

Sagi-shi players can get their friends' scores by supplying their user ids to the owner id parameter:

```csharp
var friendsList = await client.ListFriendsAsync(session, 0, 100, cursor: null);
var userIds = friendsList.Friends.Select(f => f.User.Id);
var recordList = await client.ListLeaderboardRecordsAsync(session, "weekly_imposter_wins", userIds, expiry: null, 100, cursor: null);

foreach (var record in recordList.Records)
{
    Debug.LogFormat("{0} scored {1}", record.Username, record.Score);
}
```

The same approach can be used to get group member's scores by supplying their user ids to the owner id parameter:

```csharp
var groupUserList = await client.ListGroupUsersAsync(session, "<GroupId>", state: null, 100, cursor: null);
var userIds = groupUserList.GroupUsers.Where(x => x.State < 3).Select(g => g.User.Id);
var recordList = await client.ListLeaderboardRecordsAsync(session, "weekly_imposter_wins", userIds, expiry: null, 100, cursor: null);
foreach (var record in recordList.Records)
{
    Debug.LogFormat("{0} scored {1}", record.Username, record.Score);
}
```


### Deleting records

Sagi-shi players can delete their own leaderboard records:

```csharp
await client.DeleteLeaderboardRecordAsync(session, "<LeaderboardId>");
```


## Tournaments

Nakama [Tournaments](../concepts/tournaments/) are short lived competitions where players compete for a prize.

Sagi-shi players can view, filter and join running tournaments.

<figure>
  <img src="../images/tournaments.png" alt="Sagi-shi tournaments screen">
  <figcaption>Sagi-shi Tournaments</figcaption>
</figure>


### Creating tournaments

Tournaments have to be created on the server, see the [tournament](../concepts/tournaments.md#create-tournament) documentation for details on how to create a tournament.

Sagi-shi has a weekly tournament which challenges players to get the most correct imposter votes. At the end of the week the top players receive a prize of in-game currency.


### Joining tournaments

By default in Nakama players don't have to join tournaments before they can submit a score, but Sagi-shi makes this mandatory:

```csharp
await client.JoinTournamentAsync(session, "<TournamentId>");
```

### Listing tournaments

Sagi-shi players can list and filter tournaments with various criteria:

```csharp
var categoryStart = 1;
var categoryEnd = 2;
int? startTime = null;
int? endTime = null;
var limit = 100;
var result = await client.ListTournamentsAsync(session, categoryStart, categoryEnd, startTime, endTime, limit, cursor: null);

foreach (var tournament in result.Tournaments)
{
    Debug.LogFormat("{0}:{1}", tournament.Id, tournament.Title);
}
```

!!! note "Note"
    Categories are filtered using a range, not individual numbers, for performance reasons. Structure your categories to take advantage of this (e.g. all PvE tournaments in the 1XX range, all PvP tournaments in the 2XX range, etc.).


### Listing records

Sagi-shi players can list tournament records:

```csharp
var limit = 20;
var tournamentName = "weekly_top_detective";
var result = await client.ListTournamentRecordsAsync(session, tournamentName, ownerIds: null, expiry: null, limit, cursor: null);

foreach (var record in result.Records)
{
    Debug.LogFormat("{0}:{1}", record.OwnerId, record.Score);
}
```


**Listing records around a user**

Similarly to leaderboards, Sagi-shi players can get other player scores around them:

```csharp
var limit = 20;
var tournamentName = "weekly_top_detective";
var result = await client.ListTournamentRecordsAroundOwnerAsync(session, tournamentName, session.UserId, expiry: null, limit);

foreach (var record in result.Records)
{
    Debug.LogFormat("{0}:{1}", record.OwnerId, record.Score);
}
```


### Submitting scores

Sagi-shi players can submit scores, subscores and metadata to the tournament:

```csharp
var score = 1;t
var subscore = 0;
var metadata = new Dictionary<string, string> {{ "map", "space_station" }};
await client.WriteTournamentRecordAsync(session, "weekly_top_detective", score, subscore, JsonWriter.ToJson(metadata));
```


## Notifications

Nakama [Notifications](../concepts/in-app-notifications/) can be used for the game server to broadcast real-time messages to players.

Notifications can be either persistent (remaining until a player has viewed it) or transient (received only if the player is currently online).

Sagi-shi uses Notifications to notify tournament winners about their winnings.

<figure>
  <img src="../images/notifications.png" alt="Sagi-shi notification screen">
  <figcaption>Sagi-shi notifications</figcaption>
</figure>


### Receiving notifications

Notifications have to be sent from the server.

Nakama uses a code to differentiate notifications. Codes of `0` and below are [system reserved](../concepts/in-app-notifications.md#notification-codes) for Nakama internals.

Sagi-shi players can subscribe to the notification received event. Sagi-shi uses a code of `100` for tournament winnings:

```csharp
socket.ReceivedNotification += notification => {
    const int rewardCode = 100;
    switch (notification.Code)
    {
        case rewardCode:
            Debug.LogFormat("Congratulations, you won the tournament!\n{0}\n{1}", notification.Subject, notification.Content);
            break;
        default:
            Debug.LogFormat("Other notification: {0}:{1}\n{2}", notification.Code, notification.Subject, notification.Content);
            break;
    }
};
```


### Listing notifications

Sagi-shi players can list the notifications they received while offline:

```csharp
var limit = 100;
var result = await client.ListNotificationsAsync(session, limit, cacheableCursor: null);
foreach (var notification in result.Notifications)
{
    Debug.LogFormat("Notification: {0}:{1}\n{2}", notification.Code, notification.Subject, notification.Content);
}
```


**Pagination and cacheable cursors**

Like other listing methods, notification results can be paginated using a cursor or cacheable cursor from the result.

```csharp
PlayerPrefs.SetString("nakama.notificationsCacheableCursor", result.CacheableCursor);
```

The next time the player logs in the cacheable cursor can be used to list unread notifications.

```csharp
var cacheableCursor = PlayerPrefs.GetString("nakama.notificationsCacheableCursor", null);
var nextResults = await client.ListNotificationsAsync(session, limit, cacheableCursor);
```


### Deleting notifications

Sagi-shi players can delete notifications once they've read them:

```csharp
await client.DeleteNotificationsAsync(session, new[] { "<NotificationId>" });
```
