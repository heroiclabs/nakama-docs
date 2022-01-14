# Android/Java Client Guide

This client library guide will show you how to use the core Nakama features in **Java** by showing you how to develop the Nakama specific parts (without full game logic or UI) of an [Among Us (external)](https://www.innersloth.com/games/among-us/) inspired game called Sagi-shi (Japanese for "Imposter").

<figure>
  <img src="../images/gameplay.png" alt="Sagi-shi gameplay screen">
  <figcaption>Sagi-shi gameplay</figcaption>
</figure>

## Prerequisites

Before proceeding ensure that you have:

- [Installed Nakama server](../getting-started/docker-quickstart.md)
- Downloaded the [Nakama Java SDK](https://github.com/heroiclabs/nakama-java/releases/latest). You can download "nakama-java-$version.jar" or "nakama-java-$version-all.jar" which includes a shadowed copy of all dependencies.

To work with the Java client you'll need a build tool like [Gradle](https://gradle.org/) and an editor/IDE like [IntelliJ](https://www.jetbrains.com/idea/), [Eclipse](https://eclipse.org/ide/) or [Visual Studio Code](https://code.visualstudio.com/).


## Setup

When you've downloaded the jar package you should include it in your project or if you use Gradle add the client as a dependency to your `build.gradle`.

```groovy
dependencies {
    compile(group: 'com.heroiclabs.nakama', name: 'client', version: '+')
    compile('org.slf4j:slf4j-api:1.7.25') {
    force = true // don't upgrade to "1.8.0-alpha2"
    }
}
```

The client object is used to execute all logic against the server.

```java
DefaultClient client = new DefaultClient("defaultkey", "127.0.0.1", 7349);
```

We use the builder pattern with many classes in the Java client. Most classes have a shorthand ".defaults()" method to construct an object with default values.

!!! Note
    By default the client uses connection settings "127.0.0.1" and 7349 to connect to a local Nakama server.

```java
// Quickly setup a client for a local server.
DefaultClient client = new DefaultClient("defaultkey");
```


### Logging

The Nakama Java SDK uses the SLF4J logging API. You can find more information on how to use this API and how to use different logging bindings by reading the [SLF4J User Manual](https://www.slf4j.org/manual.html).

All examples in this guide use an SLF4J `Logger` which can be created as follows:

```java
// Where App is the name of your class
Logger logger = LoggerFactory.getLogger(App.class);
```

### For Android

Android uses a permissions system which determines which platform services the application will request to use and ask permission for from the user. The client uses the network to communicate with the server so you must add the "INTERNET" permission.

```xml
<uses-permission android:name="android.permission.INTERNET"/>
```

### Asynchronous programming
Many of the Nakama APIs are asynchronous and non-blocking and are available in the Java SDK using [[`ListenableFuture`](https://guava.dev/releases/snapshot/api/docs/com/google/common/util/concurrent/ListenableFuture.html) objects which are part of the Google Guava library.

Sagi-shi calls these async methods using callbacks to not block the calling thread so that the game is responsive and efficient.

```java
// Create a single thread executor
ExecutorService executor = Executors.newSingleThreadExecutor();

// Get a ListenableFuture with a Session result
ListenableFuture<Session> authFuture = client.authenticateDevice(deviceId);

// Setup the success and failure callbacks, specifying which executor to use
Futures.addCallback(authFuture, new FutureCallback<Session>() {
    @Override
    public void onSuccess(@NullableDecl Session session) {
        logger.debug("Authenticated user id: " + session.getUserId());
        executor.shutdown();
    }

    @Override
    public void onFailure(Throwable throwable) {
        logger.error(throwable.getMessage());
        executor.shutdown();
    }
}, executor);

// Wait for the executor to finish all tasks
try {
    executor.awaitTermination(5, TimeUnit.SECONDS);
}
catch (InterruptedException e) {
    logger.error(e.getMessage());
}
```

If you wish to chain asynchronous calls together, you can do so using `AsyncFunction<>` objects and the `Futures.transformAsync` function.

```java
// Get a ListenableFuture with a Session result
ListenableFuture<Session> authFuture = client.authenticateDevice(deviceId);

// Create an AsyncFunction to get the Account of a user using a Session object
AsyncFunction<Session, Account> accountFunction = session -> client.getAccount(session);

// Get a ListenableFuture from Futures.transformAsync by first passing the original Future, followed by the extended Future and finally an exector
ListenableFuture<Account> accountFuture = Futures.transformAsync(authFuture, accountFunction, executor);

// Setup the success and failture callbacks as usual
Futures.addCallback(accountFuture, new FutureCallback<>() {
    @Override
    public void onSuccess(@NullableDecl Account account) {
        logger.debug("Got account for user id: " + account.getUser().getId());
        executor.shutdown();
    }

    @Override
    public void onFailure(Throwable throwable) {
        logger.error(throwable.getMessage());
        executor.shutdown();
    }
}, executor);
```

!!!note Note
    For brevity, the code samples in this guide will use the simpler but thread blocking `.get()` function instead.

    ```java
    Account account = client.getAccount(session).get();
    ```


### Handling exceptions

Network programming requires additional safeguarding against connection and payload issues.

As shown above, API calls in Sagi-Shi use a callback pattern with both a success and a failure callback being provided. If an API call throws an exception it is handled in the `onFailure` callback and the exception details can be accessed in the `throwable` object.

<!--
### Handling retries
The Java SDK doesn't provide RetryConfiguration options
 -->


## Getting started

Learn how to get started using the Nakama Client and Socket objects to start building Sagi-shi and your own game.

<!-- simple system diagram of Java Nakama client, Nakama client socket, connecting to Nakama server -->


### Nakama Client

The Nakama Client connects to a Nakama Server and is the entry point to access Nakama features. It is recommended to have one client per server per game.

To create a client for Sagi-shi pass in your server connection details:

```java
// explictly passing the defaults
Client client = new DefaultClient("defaultkey", "127.0.0.1", 7349, false)
// or same as above
Client client = new DefaultClient("defaultkey");
```

!!!note Note
    The Nakama Java SDK communicates with the Nakama server directly via gRPC so you will want to use the gRPC port number that you have configured for your Nakama server; by default this is `7349`.


### Nakama Socket

The Nakama Socket is used for gameplay and real-time latency-sensitive features such as chat, parties, matches and RPCs.

The socket is exposed on a different port on the server to the client. You'll need to specify a different port here to ensure that connection is established successfully.

The client can create one or more sockets with the server. Each socket can have it's own event listeners registered for responses received from the server.

From the client create a socket:

```java
SocketClient socket = client.createWebSocket();

SocketListener listener = new AbstractSocketListener() {
    @Override
    public void onDisconnect(final Throwable t) {
        logger.info("Socket disconnected.");
    }
};

socket.connect(session, listener).get();
logger.info("Socket connected.");
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

```java
String deviceId = UUID.randomUUID().toString();
Session session = client.authenticateDevice(deviceId).get();
logger.info("Session authToken: {}", session.getAuthToken());
```

### Facebook authentication

Nakama [Facebook Authentication](../concepts/authentication/#facebook) is an easy to use authentication method which lets you optionally import the player's Facebook friends and add them to their Nakama Friends list.

```java
String oauthToken = "...";
Session session = client.authenticateFacebook(oauthToken).get();
logger.info("Session authToken: {}", session.getAuthToken());
```


### Custom authentication

Nakama supports [Custom Authentication](../concepts/authentication/#custom) methods to integrate with additional identity services.

See the [Itch.io custom authentication](../server-framework/recipes/itch-authentication) recipe for an example.


### Linking authentication

Nakama allows players to [Link Authentication](../concepts/authentication/#link-or-unlink) methods to their account once they have authenticated.


**Linking Device ID authentication**

```java
String deviceId = UUID.randomUUID().toString();
client.linkDevice(session, deviceId);
logger.info("Linked device id {} for user {}", deviceId, session.getUserId());
```


**Linking Facebook authentication**

```csharp
String facebookAccessToken = "...";
boolean importFriends = true;
client.linkFacebook(session, facebookAccessToken, importFriends);
logger.info("Linked facebook authentication for user {}", deviceId, session.getUserId());
```


### Session variables

TODO: test, missing from concept docs

Nakama [Session Variables](../concepts/session/#session-variables) can be stored when authenticating and will be available on the client and server as long as the session is active.

Sagi-shi uses session variables to implement analytics, referral and rewards programs and more.

Store session variables by passing them as an argument when authenticating:

```java
Map<String, String> vars = new HashMap<>();
vars.put("DeviceOS", System.getProperty("os.name"));
vars.put("GameVersion", "VersionNumber");
vars.put("InviterUserId", "<SomeUserId>");

Session session = client.authenticateDevice(deviceId, vars).get();
```
To access session variables on the Client use the `Vars` property on the `Session` object:

```java
Map<String, String> vars = session.getVars()
```


### Session lifecycle

Nakama [Sessions](../concepts/session/) expire after a time set in your server [configuration](../getting-started/configuration.md#session). Expiring inactive sessions is a good security practice.

It is recommended to store the auth token from the session and check at startup if it has expired. If the token has expired you must reauthenticate. The expiry time of the token can be changed as a [setting](../getting-started/configuration.md#common-properties) in the server.

```java
logger.info("Session connected: {}", session.getAuthToken());

// Android
SharedPreferences pref = getActivity().getPreferences(Context.MODE_PRIVATE);
SharedPreferences.Editor editor = pref.edit();
editor.putString("nk.session", session.getAuthToken());
editor.commit();
```

Nakama provides ways to restore sessions without the need to re-authenticate, for example when Sagi-shi players re-launch the game.

```java
session = DefaultSession.restore(authToken);
```

<!-- No Refresh API -->

<!-- No Logout API -->


## User accounts

Nakama [User Accounts](../concepts/user-accounts.md) store user information defined by Nakama and custom developer metadata.

Sagi-shi allows players to edit their accounts and stores metadata for things like game progression and in-game items.

<figure>
  <img src="../images/profile.png" alt="Sagi-shi player profile screen">
  <figcaption>Player profile</figcaption>
</figure>


### Get the user account

TODO: no API docs for Account, User
TODO: some account stuff seems to be duplicated in sessions and account, eg. getUsername

Many of Nakama's features are accessible with an authenticated session, like [fetching a user account](../concepts/user-accounts/#fetch-account).

Get a Sagi-shi player's full user account with their basic [user information](../concepts/user-accounts/#fetch-account) and user id:

```java
Account account = client.getAccount(session).get();
User user = account.getUser();
String username = user.getUsername();
String avatarUrl = user.getAvatarUrl();
String userId = user.getId();
```


### Update the user account

Nakama provides easy methods to update server stored resources like user accounts.

Sagi-shi players need to be able to update their public profiles:

```java
String newUsername = "NotTheImp0ster";
String newDisplayName = "Innocent Dave";
String newAvatarUrl = "https://example.com/imposter.png";
String newLangTag = "en";
String newLocation = "Edinburgh";
String newTimezone = "BST";
client.updateAccount(session, newUsername, newDisplayName, newAvatarUrl, newLangTag, newLocation, newTimezone).get();
```


### Getting users

In addition to getting the current authenticated player's user account, Nakama has a convenient way to get a list of other players' public profiles from their ids, usernames or Facebook ids.

Sagi-shi uses this method to display player profiles when engaging with other Nakama features:

```java
List<String> ids = Arrays.asList("userid1", "userid2");
List<String> usernames = Arrays.asList("username1", "username1");
String[] facebookIds = new String[] {"facebookid1"};

Users users = client.getUsers(session, ids, usernames, facebookIds).get();

for (User user : users.getUsersList()) {
  logger.info("User id {} username {}", user.getId(), user.getUsername());
}
```


### Storing metadata

TODO: missing Account, User API docs. Aren't defined as Java classes.
TODO: no metadata concept data.

Nakama [User Metadata](../concepts/user-accounts/#user-metadata) allows developers to extend user accounts with public user fields.

User metadata can only be updated on the server. See the [updating user metadata](../server-framework/recipes/updating-user-metadata) recipe for an example.

Sagi-shi will use metadata to store what in-game items players have equipped:


### Reading metadata

Define a class that describes the metadata and parse the JSON metadata:

```java
public class Metadata
{
    public String Title;
    public String Hat;
    public String Skin;
}
```

```java
// Get the updated account object.
Account account = client.getAccount(session).get();

// Parse the account user metadata and log the result.
Gson gson = new Gson();
Metadata metadata = gson.fromJson(account.getUser().getMetadata(), Metadata.class);
logger.info("Title: {}, Hat: {}, Skin: {}", metadata.Title, metadata.Hat, metadata.Skin);
```

!!!note Note
    The above code uses the `com.google.gson` serialization/deserialization library.

### Wallets

Nakama [User Wallets](../concepts/user-accounts/#virtual-wallet) can store multiple digital currencies as key/value pairs of strings/integers.

Players in Sagi-shi can unlock or purchase titles, skins and hats with a virtual in-game currency.

#### Accessing wallets

TODO: missing Account, User API docs.
TODO: no virtual wallets concept data.

Parse the JSON wallet data from the user account:

```java
Gson gson = new Gson();
Map<String, Double> wallet = new HashMap<>();
wallet = gson.fromJson(account.getWallet(), wallet.getClass());

logger.info("Wallet:");
wallet.forEach((k, v) -> logger.info("{}: {}", k, v));
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

Define a class that describes the storage object (or optionally use a `HashMap<String, Object>`) and create a new storage object id with the collection name, key and user id. Finally, read the storage objects and parse the JSON data:

```java
StorageObjectId objectId = new StorageObjectId("favorites");
objectId.setKey("Hats");
objectId.setUserId(session.getUserId());

StorageObjects objects = client.readStorageObjects(session, objectId).get();

objects.getObjectsList().forEach((object) -> {
    Map<String, Object> parsedObj = new Gson().fromJson(object.getValue(), new TypeToken<Map<String, Object>>(){}.getType());
    logger.info("{}:", object.getKey());
    parsedObj.forEach((k, v) -> logger.info("{}: {}", k, v));
});
```

!!! note "Note"
    To read other players' public storage object, use their `UserId` instead.
    Players can only read storage objects they own or that are public (`PermissionRead` value of `2`).


### Writing storage objects

Nakama allows developers to write to the Storage Engine from the client and server.

Consider what adverse effects a malicious user can have on your game and economy when deciding where to put your write logic, for example data that should only be written authoritatively (i.e. game unlocks or progress).

Sagi-shi allows players to favorite items for easier access in the UI and it is safe to write this data from the client.

Create a write storage object with the collection name, key and JSON encoded data. Finally, write the storage objects to the Storage Engine:

```java
// Serialize your object as JSON
Map<String, List<String>> favoriteHats = new HashMap<>();
favoriteHats.put("hats", Arrays.asList("cowboy", "alien"));
String favoriteHatsJson = new Gson().toJson(favoriteHats);

StorageObjectWrite writeObject = new StorageObjectWrite("favorites", "Hats", favoriteHatsJson, PermissionRead.OWNER_READ, PermissionWrite.OWNER_WRITE);
StorageObjectAcks acks = client.writeStorageObjects(session, writeObject).get();
logger.info("Stored objects {}", acks.getAcksList());
```

You can also pass multiple objects to the `client.writeStorageObjects` method:

```java
String favoriteHatsJson = "...";
String myStatsJson = "...";

StorageObjectWrite writeObject = new StorageObjectWrite("favorites", "Hats", favoriteHats, PermissionRead.OWNER_READ, PermissionWrite.OWNER_WRITE);
StorageObjectWrite statsObject = new StorageObjectWrite("stats", "player", myStats, PermissionRead.OWNER_READ, PermissionWrite.OWNER_WRITE);

StorageObjectAcks acks = client.writeStorageObjects(session, saveGameObject, statsObject).get();
System.out.format("Stored objects %s", acks.getAcksList());
```

### Conditional writes

Storage Engine [Conditional Writes](../concepts/collections/#conditional-writes) ensure that write operations only happen if the object hasn't changed since you accessed it.

This gives you protection from overwriting data, for example the Sagi-shi server could have updated an object since the player last accessed it.

To perform a conditional write, add a version to the write storage object with the most recent object version:

```java
// Assuming we already got an object (`obj`)
// Create a new object with the same Collection and Key and use the previous object's Version
StorageObjectWrite writeObject = new StorageObjectWrite(obj.getCollection(), obj.getKey(), newJsonValue, PermissionRead.OWNER_READ, PermissionWrite.OWNER_WRITE);
writeObject.setVersion(obj.getVersion())

// ... then write it to the Storage Engine as shown above
```


### Listing storage objects

Instead of doing multiple read requests with separate keys you can list all the storage objects the player has access to in a collection.

Sagi-shi lists all the player's unlocked or purchased titles, hats and skins:

```csharp
int limit = 3;
String cursor = null;

StorageObjectList objects = client.listUsersStorageObjects(session, "Unlocks", limit, cursor);
logger.info("Object count: {}", objects.getObjectsCount());
objects.getObjectsList().forEach(object -> logger.info("Key: {}", object.getKey()));
```


### Paginating results

Nakama methods that list results return a cursor which can be passed to subsequent calls to Nakama to indicate where to start retrieving objects from in the collection.

For example:
- If the cursor has a value of 5, you will get results from the fifth object.
- If the cursor is `null`, you will get results from the first object.

```java
StorageObjectList objectsPage2 = lient.listUsersStorageObjects(session, "Unlocks", limit, objectsPage1.cursor);
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

```java
String payload = "{ \"item\", \"cowboy\" }";

Rpc result = client.rpc(session, "EquipHat", payload).get();
logger.info("New hat equipped successfully {}", result.getPayload());
```


### Socket RPCs

Nakama Remote Procedures can also be called form the socket when you need to interface with Nakama's real-time functionality.

<!-- todo: the explanation of how this interfaces with real-time functionality is not clear -->

```java
Rpc result = socket.rpc("EquipHat", payload).get();
logger.info("New hat equipped successfully {}", result.getPayload());
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


```java
// Add friends by User ID.
List<String> ids = Arrays.asList("AlwaysTheImposter21", "SneakyBoi");

// Add friends by Username.
String[] usernames = new String[] { "<SomeUserId>", "<AnotherUserId>" };

client.addFriends(session, ids, usernames).get();
```


### Friendship states

Nakama friendships are categorized with the following [states](../concepts/friends.md#friend-state):

| Value | State                                         |
|-------|-----------------------------------------------|
| 0     | Mutual friends                                |
| 1     | An outgoing friend request pending acceptance |
| 2     | An incoming friend request pending acceptance |
| 4     | Banned                                        |


### Listing friends

Nakama allows developers to list the player's friends based on their friendship state.

Sagi-shi lists the 20 most recent mutual friends:

```java
int friendshipState = 0;
int limit = 20; // Limit is capped at 1000
String cursor = null;

FriendList friends = client.listFriends(session, friendshipState, limit, cursor).get();
friends.getFriendsList().forEach(friend -> logger.info("Friend Id: {}", friend.getUser().getId()));
```


### Accepting friend requests

When accepting a friend request in Nakama the player adds a [bi-directional friend relationship](../concepts/friends-best-practices/#modeling-relationships).

Nakama takes care of changing the state from pending to mutual for both.

In a complete game you would allow players to accept individual requests.

Sagi-shi just fetches and accepts all the incoming friend requests:

```java
int friendshipState = 2;
FriendList friends = client.listFriends(session, friendshipState).get();

for (Friend friend : friends.getFriendsList()) {
    List<String> ids = Arrays.asList(friend.getUser().getId());
    client.addFriends(session, ids, null).get();
}
```


### Deleting friends

Sagi-shi players can remove friends by their username or user id:

```java
// Delete friends by User ID.
List<String> ids = Arrays.asList("<SomeUserId>", "<AnotherUserId>");

// Delete friends by Username.
String[] usernames = new String[] { "<SomeUsername>", "<AnotherUsername>" };

client.deleteFriends(session, ids, usernames).get();
```


### Blocking users

Sagi-shi players can block others by their username or user id:

```java
// Block friends by User ID.
List<String> ids = Arrays.asList("<SomeUserId>", "<AnotherUserId>");

// Block friends by Username.
String[] usernames = new String[] { "<SomeUsername>", "<AnotherUsername>" };

client.blockFriends(session, ids, usernames).get();
```

!!!note Note
    Blocked friends are represented by a friendship state of `3`.


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

```java
// If one doesn't already exist, create a socket listener and handle status presence events
SocketListener socketListener = new SocketListener() {
    @Override
    public void onStatusPresence(StatusPresenceEvent e) {
        if (e.getJoins() != null) {
            e.getJoins().forEach(presence -> logger.info("{} is online with status {}", presence.getUsername(), presence.getStatus()));
        }

        if (e.getLeaves() != null) {
            e.getLeaves().forEach(presence -> logger.info("{} went offline", presence.getUsername()));
        }
    }

    // ... other required overrides (e.g. onChannelMessage etc)
}

// Then create and connect a socket connection
SocketClient socket = client.createSocket();
socket.connect(session, socketListener);

// Follow mutual friends and get the initial Status of any that are currently online
int friendshipState = 0; // Mutual friends
int limit = 20; // Limit is capped at 1000

FriendList friends = client.listFriends(session, friendshipState, limit, null).get();
List<String> friendIds = new ArrayList<>();
for (Friend friend : friends.getFriendsList()) {
    friendIds.add(friend.getUser().getId());
}

Status status = socket.followUsers(friendIds).get();
if (status.getPresences() != null) {
    status.getPresences().forEach(presence -> logger.info("{} is online with status {}", presence.getUsername(), presence.getStatus()));
}
```


### Unfollow users

Sagi-shi players can unfollow others:

```java
socket.unfollowUsers("<UserId>", "<AnotherUserId>").get();
```


### Updating player status

Sagi-shi players can change and publish their status to their followers:

```java
socket.updateStatus("Viewing the Main Menu").get();
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

```java
String name = "Imposters R Us";
String description = "A group for people who love playing the imposter.";
String avatarUrl = "";
String langTag = "";
Boolean open = true; // Public group
int maxSize = 100;
Group group = client.createGroup(session, name, description, avatarUrl, langTag, open, maxSize).get();
```


### Update group visibility

Nakama allows group superadmin or admin members to update some properties from the client, like the open visibility:

```java
String groupId = "<GroupId>";
boolean open = false;
client.updateGroup(session, groupId, name, description, avatarUrl, langTag, open);
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


```java
int limit = 20;
GroupList groupList = client.listGroups(session, "imposter%", limit).get();
groupList.getGroupsList().forEach(group -> logger.info("{} [{}]", group.getName(), group.hasOpen() && group.getOpen().getValue() ? "Public" : "Private"));

// Get the next page of results.
GroupList nextGroupListResults = client.listGroups(session, "imposter%", limit, groupList.getCursor()).get();
```


### Deleting groups

Nakama allows group superadmins to delete groups.

Developers can disable this feature entirely, see the [Guarding APIs guide](../guides/guarding-apis/) for an example on how to protect various Nakama APIs.

Sagi-shi players can delete groups which they are superadmins for:

```java
client.deleteGroup(session, "<GroupId>").get();
```


### Group metadata

Like Users Accounts, Groups can have public metadata.

Sagi-shi uses group metadata to store the group's interests, active player times and languages spoken.

Group metadata can only be updated on the server. See the [updating group metadata](../server-framework/recipes/updating-group-metadata) recipe for an example.

The Sagi-shi client makes an RPC with the group metadata payload:

```java
Map<String, Object> payload = new HashMap<>();
payload.put("GroupId", "<GroupId>");
payload.put("Interests", Arrays.asList("Deception", "Sabotage", "Cute Furry Bunnies"));
payload.put("ActiveTimes", Arrays.asList("9am-2pm Weekdays", "9am-10am Weekends"));
payload.put("Languages", Arrays.asList("English", "German"));

try {
    Rpc rpcResult = client.rpc(session, "UpdateGroupMetadata", new Gson().toJson(payload, payload.getClass())).get();
    logger.info("Successfully updated group metadata");
}
catch (ExecutionException ex) {
    logger.error(ex.getMessage());
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

```java
client.joinGroup(session, "<GroupId>").get();
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

```java
GroupUserList groupUserList = client.listGroupUsers(session, "<GroupId>").get();
groupUserList.getGroupUsersList().forEach(groupUser -> logger.info("{}: {}", groupUser.getUser().getId(), groupUser.getState().getValue()));
```


### Accepting join requests

Private group admins or superadmins can accept join requests by re-adding the user to the group.

Sagi-shi first lists all the users with a join request state and then loops over and adds them to the group:


```java
int state = 3;
int limit = 100;
String cursor = "";
GroupUserList groupUserList = client.listGroupUsers(session, groupId, state, limit, cursor).get();
groupUserList.getGroupUsersList().forEach(groupUser -> {
    client.addGroupUsers(session, groupId, groupUser.getUser().getId());
});
```


### Promoting members

Nakama group members can be promoted to admin or superadmin roles to help manage a growing group or take over if members leave.

Admins can promote other members to admins, and superadmins can promote other members up to superadmins.

The members will be promoted up one level. For example:

- Promoting a member will make them an admin
- Promoting an admin will make them a superadmin

```java
client.promoteGroupUsers(session, "<GroupId>", "UserId", "AnotherUserId").get();
```


### Demoting members

Sagi-shi group admins and superadmins can demote members:

```csharp
await client.DemoteGroupUsersAsync(session, "<GroupId>", new[] { "UserId" });
```


### Kicking members

Sagi-shi group admins and superadmins can remove group members:

```java
client.demoteGroupUsers(session, "<GroupId>", "UserId", "AnotherUserId").get();
```


### Banning members

Sagi-shi group admins and superadmins can ban a user when demoting or kicking is not severe enough:

```java
client.banGroupUsers(session, "<GroupId>", "UserId", "AnotherUserId").get();
```

### Leaving groups

Sagi-shi players can leave a group:

```java
client.leaveGroup(session, "<GroupId>").get();
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

```java
String roomName = "<MatchId>";
boolean persistence = false;
boolean hidden = false;
Channel channel = socket.joinChat(roomName, ChannelType.ROOM, persistence, hidden).get();
logger.info("Connected to dynamic room channel: {}", channel.getId());
```


### Joining group chat

Sagi-shi group members can have conversations that span play sessions in a persistent group chat channel:

```java
String groupId = "<GroupId>";
boolean persistence = true;
boolean hidden = false;
Channel channel = socket.joinChat(groupId, ChannelType.GROUP, persistence, hidden).get();
logger.info("Connected to group channel: {}", channel.getId());
```


### Joining direct chat

Sagi-shi players can also chat privately one-to-one during or after matches and view past messages:

```java
String userId = "<UserId>";
boolean persistence = true;
boolean hidden = false;
Channel channel = socket.joinChat(userId, ChannelType.DIRECT_MESSAGE, persistence, hidden).get();
logger.info("Connected to direct message channel: {}", channel.getId());
```


### Sending messages

Sending messages is the same for every type of chat channel. Messages contain chat text and emotes and are sent as JSON serialized data:

```java
String channelId = "<ChannelId>";

Map<String, String> messageContent = new HashMap<>();
messageContent.put("message", "I think Red is the imposter!");
ChannelMessageAck messageSendAck = socket.writeChatMessage(channelId, new Gson().toJson(messageContent, messageContent.getClass())).get();

Map<String, String> emoteContent = new HashMap<>();
emoteContent.put("emote", "point");
emoteContent.put("emoteTarget", "<RedPlayerUserId>");
ChannelMessageAck emoteSendAck = socket.writeChatMessage(channelId, new Gson().toJson(emoteContent, emoteContent.getClass())).get();
```


### Listing message history

Message listing takes a parameter which indicates if messages are received from oldest to newest (forward) or newest to oldest.

Sagi-shi players can list a group's message history:

```java
int limit = 100;
boolean forward = true;
String groupId = "<GroupId>";
ChannelMessageList channelMessageList = client.listChannelMessages(session, channelId, limit, null, forward).get();
channelMessageList.getMessagesList().forEach(message -> logger.info("{}:{}", message.getUsername(), message.getContent()));
```

Chat also has cacheable cursors to fetch the most recent messages, which you can store in whichever way you prefer.

```java
// Store this in whichever way suits your application
String cacheableCursor = channelMessageList.getCacheableCursor();

// ...then use the cacheable cursor later to retrieve the next results
ChannelMessageList nextResults = client.listChannelMessages(session, groupId, limit, cacheableCursor, forward);
```


### Updating messages

Nakama also supports updating messages. It is up to you whether you want to use this feature, but in a game of deception like Sagi-shi it can add an extra element of deception.

For example a player sends the following message:

```java
Map<String, String> messageContent = new HashMap<>();
messageContent.put("message", "I think Red is the imposter!");
ChannelMessageAck ack = socket.writeChatMessage(channelId, new Gson().toJson(messageContent, messageContent.getClass())).get();

Map<String, String> updatedMessageContent = new HashMap<>();
updatedMessageContent.put("message", "I think BLUE is the imposter!");
ChannelMessageAck updateAck = socket.updateChatMessage(channelId, ack.getMessageId(), new Gson().toJson(updatedMessageContent, updatedMessageContent.getClass())).get();
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

```java
Match match = socket.createMatch().get();
FriendList friendList = client.listFriends(session, 0, 100, "").get();
for (Friend friend : friendList.getFriendsList()) {
    if (friend.getUser().getOnline()) {
        Map<String, String> messageContent = new HashMap<>();
        messageContent.put("message", String.format("Hey %s, join me for a match!", friend.getUser().getUsername()));
        messageContent.put("matchId", match.getMatchId());

        Channel channel = socket.joinChat(friend.getUser().getId(), ChannelType.DIRECT_MESSAGE).get();
        ChannelMessageAck ack = socket.writeChatMessage(channel.getId(), new Gson().toJson(messageContent, messageContent.getClass())).get();
    }
}
```

### Joining matches

Sagi-shi players can try to join existing matches if they know the id:
```java
String matchId = "<MatchId>";
Match match = socket.joinMatch(matchId).get();
```

Or set up a real-time matchmaker listener and add themselves to the matchmaker:

```java
// In the SocketListener, override the onMatchmakerMatched function
@Override
public void onMatchmakerMatched(MatchmakerMatched matchmakerMatched) {
    Match match = socket.joinMatch(matchmakerMatched.getMatchId()).get();
}

// ...then, elsewhere
int minPlayers = 2;
int maxPlayers = 10;
String query = "";
MatchmakerTicket matchmakingTicket = socket.addMatchmaker(minPlayers, maxPlayers, query).get();
```


**Joining matches from player status**

Sagi-shi players can update their status when they join a new match:

```java
Map<String, String> status = new HashMap<>();
status.put("Status", "Playing a match");
status.put("MatchId", "<MatchId>");

socket.updateStatus(new Gson().toJson(status, status.getClass())).get();
```

When their followers receive the real-time status event they can try and join the match:

```java
@Override
public void onStatusPresence(StatusPresenceEvent e) {
    if (e.getJoins() != null) {
        e.getJoins().forEach(presence -> {
            Map<String, String> status = new Gson().fromJson(presence.getStatus(), new TypeToken<Map<String, String>>(){}.getType());
            if (status.containsKey("MatchId")) {
                socket.joinMatch(status.get("MatchId")).get();
            }
        });
    }
}
```

### Listing matches

Match Listing takes a number of criteria to filter matches by including player count, a match [label](../concepts/server-authoritative-multiplayer/#match-label) and an option to provide a more complex [search query](../concepts/server-authoritative-multiplayer.md#search-query).

<!-- todo: we don't have concept or client side docs for match listing labels or search queries -->

Sagi-shi matches start in a lobby state. The match exists on the server but the actual gameplay doesn't start until enough players have joined.

Sagi-shi can then list matches that are waiting for more players:

```java
int minPlayers = 2;
int maxPlayers = 10;
int limit = 10;
boolean authoritative = true;
String label = "";
MatchList matchList = client.listMatches(session, minPlayers, maxPlayers, limit, label, authoritative).get();
matchList.getMatchesList().forEach(match -> logger.info("{}: {}/{}", match.getMatchId(), match.getSize(), limit));
```

To find a match that has a label of `"AnExactMatchLabel"`:

```java
String label = "AnExactMatchLabel";
```

<!--
There doesn't appear to be a way to specify a query param in the java sdk for client.listMatches.

**Advanced:**

In order to use a more complex structured query, the match label must be in JSON format.

To find a match where it expects player skill level to be `>100` and optionally has a game mode of `"sabotage"`:

```csharp
var query = "+label.skill:>100 label.mode:sabotage"
``` -->


### Spawning players

The match object has a list of current online users, known as presences.

Sagi-shi uses the match presences to spawn players on the client (where a player is represented by a `Player` object):

```java
Match match = socket.joinMatch(matchId).get();
Map<String, Player> players = new HashMap<>();

match.getPresences().forEach(presence -> {
    Player player = new Player();
    player.Spawn();
    players.put(presence.getSessionId(), player);
});
```

Sagi-shi keeps the spawned players up-to-date as they leave and join the match using the match presence received event as a Socket Listener override:

```java
@Override
public void onMatchPresence(MatchPresenceEvent e) {
    if (e.getJoins() != null) {
        e.getJoins().forEach(presence -> {
            Player player = new Player();
            player.Spawn();
            players.put(presence.getSessionId(), player);
        });
    }

    if (e.getLeaves() != null) {
        e.getLeaves().forEach(presence -> {
            if (players.containsKey(presence.getSessionId())) {
                Player player = players.get(presence.getSessionId());
                player.Despawn();
                players.remove(presence.getSessionId());
            }
        });
    }
}
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

```java
public class PositionState {
    public float X;
    public float Y;
    public float Z;
}
```

Create an instance from the player's transform, set the op code and send the JSON encoded state:

```java
PositionState state = new PositionState();
state.X = 10;
state.Y = 5;
state.Z = 1;

int opCode = 1;
String json = new Gson().toJson(state, state.getClass());
socket.sendMatchData(matchId, opCode, json.getBytes());
```


**Op Codes as static properties**

Sagi-shi has many networked game actions. Using a static class of constants for op codes will keep your code easier to follow and maintain:

```java
public class OpCodes {
    public static final long Position = 1;
    public static final long Vote = 2;
}
```


### Receiving match sate

Sagi-shi players can receive match data from the other connected clients by subscribing to the match state received event:

```java
@Override
public void onMatchData(MatchData matchData) {
    if (matchData.getOpCode() == OpCodes.Position) {
        // Get the updated position data
        String json = new String(matchData.getData());
        PositionState position = new Gson().fromJson(json, new TypeToken<PositionState>(){}.getType());

        // Update the GameObject associated with that player
        if (players.containsKey(matchData.getPresence().getSessionId())) {
            Player player = players.get(matchData.getPresence().getSessionId());

            // Here we would normally do something like smoothly interpolate to the new position, but for this example let's just set the position directly.
            player.position = position;
        }
    }
}
```


## Matchmaker

Developers can find matches for players using Match Listing or the Nakama [Matchmaker](../concepts/matches/), which enables players join the real-time matchmaking pool and be notified when they are matched with other players that match their specified criteria.

Matchmaking helps players find each other, it does not create a match. This decoupling is by design, allowing you to use matchmaking for more than finding a game match. For example, if you were building a social experience you could use matchmaking to find others to chat with.

<!-- matchmaker flow chart, to be fleshed out -->


### Add matchmaker

Matchmaking criteria can be simple, find 2 players, or more complex, find 2-10 players with a minimum skill level interested in a specific game mode.

Sagi-shi allows players to join the matchmaking pool and have the server match them with other players:

```java
int minPlayers = 2;
int maxPlayers = 10;
String query = "";
Map<String, String> stringProperties = new HashMap<>();
stringProperties.put("mode", "sabotage");
Map<String, Double> numericProperties = new HashMap<>();
numericProperties.put("skill", 125d);
MatchmakerTicket matchmakerTicket = socket.addMatchmaker(minPlayers, maxPlayers, query, stringProperties, numericProperties).get();
```

Once a match has been found, it can be joined directly from the `onMatchmakerMatched` socket handler.

```java
@Override
public void onMatchmakerMatched(MatchmakerMatched matchmakerMatched) {
    Match match = socket.joinMatch(matchmakerMatched.getMatchId()).get();
}
```

<!-- todo: add a stand alone guide/recipe for widening the matchmaker criteria as it's a more complex example to include here -->


<!--
TODO: There is no party API in the Java SDK

## Parties

Nakama [Parties](../concepts/parties/) is a real-time system that allows players to form short lived parties that don't persist after all players have disconnected.

Sagi-shi allows friends to form a party and matchmake together.


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
-->


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
int score = 1;
int subscore = 0;
Map<String, String> metadata = new HashMap<>();
metadata.put("map", "space_station");

LeaderboardRecord record = client.writeLeaderboardRecord(session, "weekly_imposter_wins", score, subscore, new Gson().toJson(metadata, metadata.getClass())).get();
```

### Listing the top records

Sagi-shi players can list the top records of the leaderboard:

```java
int limit = 20;
Iterable<String> ownerIds = null;
int expiry = 0;
String cursor = null;
String leaderboardName = "weekly_imposter_wins";

LeaderboardRecordList list = client.listLeaderboardRecords(session, leaderboardName, ownerIds, expiry, limit, cursor).get();
list.getRecordsList().forEach(record -> logger.info("{}:{}", record.getOwnerId(), record.getScore()));
```


**Listing records around the user**

Nakama allows developers to list leaderboard records around a player.

Sagi-shi gives players a snapshot of how they are doing against players around them:

```java
int limit = 20;
int expiry = 0;
String cursor = null;
String leaderboardName = "weekly_imposter_wins";

LeaderboardRecordList list = client.listLeaderboardRecordsAroundOwner(session, leaderboardName, session.getUserId(), expiry, limit).get();
list.getRecordsList().forEach(record -> logger.info("{}:{}", record.getOwnerId(), record.getScore()));
```


**Listing records for a list of users**

Sagi-shi players can get their friends' scores by supplying their user ids to the owner id parameter:

```java
int limit = 20;
int expiry = 0;
String cursor = null;
String leaderboardName = "weekly_imposter_wins";

LeaderboardRecordList list = client.listLeaderboardRecords(session, leaderboardName, friendUserIds, expiry, limit, cursor).get();
list.getRecordsList().forEach(record -> logger.info("{}:{}", record.getOwnerId(), record.getScore()));
```

The same approach can be used to get group member's scores by supplying their user ids to the owner id parameter:

```java
GroupUserList groupUserList = client.listGroupUsers(session, "<GroupId>", -1, 100, null).get();
List<String> groupUserIds = new ArrayList<>();
groupUserList.getGroupUsersList().forEach(user -> groupUserIds.add(user.getUser().getId()));

int limit = 20;
int expiry = 0;
String cursor = null;
String leaderboardName = "weekly_imposter_wins";

LeaderboardRecordList list = client.listLeaderboardRecords(session, leaderboardName, groupUserIds, expiry, limit, cursor).get();
list.getRecordsList().forEach(record -> logger.info("{}:{}", record.getOwnerId(), record.getScore()));
```


### Deleting records

Sagi-shi players can delete their own leaderboard records:

```java
client.deleteLeaderboardRecord(session, "<LeaderboardId>").get();
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

```java
client.joinTournament(session, "<TournamentId>").get();
```

### Listing tournaments

Sagi-shi players can list and filter tournaments with various criteria:

```java
int categoryStart = 1;
int categoryEnd = 2;
long startTime = -1;
long endTime = -1;
int limit = 100;
TournamentList tournamentList = client.listTournaments(session, categoryStart, categoryEnd, startTime, endTime, limit, null).get();
tournamentList.getTournamentsList().forEach(tournament -> logger.info("{}:{}", tournament.getId(), tournament.getTitle()));
```

!!! note "Note"
    Categories are filtered using a range, not individual numbers, for performance reasons. Structure your categories to take advantage of this (e.g. all PvE tournaments in the 1XX range, all PvP tournaments in the 2XX range, etc.).


### Listing records

Sagi-shi players can list tournament records:

```java
int limit = 20;
String tournamentName = "weekly_top_detective";
int expiry = -1;
String cursor = null;
TournamentRecordList recordList = client.listTournamentRecords(session, tournamentName, expiry, limit, cursor).get();
recordList.getRecordsList().forEach(record -> logger.info("{}:{}", record.getOwnerId(), record.getScore()));
```


**Listing records around a user**

Similarly to leaderboards, Sagi-shi players can get other player scores around them:

```java
int limit = 20;
String tournamentName = "weekly_top_detective";
int expiry = -1;
TournamentRecordList recordList = client.listTournamentRecordsAroundOwner(session, tournamentName, session.getUserId(), expiry, limit).get();
recordList.getRecordsList().forEach(record -> logger.info("{}:{}", record.getOwnerId(), record.getScore()));
```


### Submitting scores

Sagi-shi players can submit scores, subscores and metadata to the tournament:

```java
int score = 1;
int subscore = 0;
Map<String, String> metadata = new HashMap<>();
metadata.put("map", "space_station");

client.writeLeaderboardRecord(session, "weekly_top_detective", score, subscore, new Gson().toJson(metadata, metadata.getClass())).get();
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

Sagi-shi players can subscribe to the notification received event using the Socket Listener. Sagi-shi uses a code of `100` for tournament winnings:

```java
@Override
public void onNotifications(NotificationList notificationList) {
    final int rewardCode = 100;
    for (Notification notification : notificationList.getNotificationsList()) {
        if (notification.getCode() == rewardCode) {
            logger.info("Congratulations, you won the tournament!\n{}\n{}", notification.getSubject(), notification.getContent());
        } else {
            logger.info("Other notification: {}:{}\n{}", notification.getCode(), notification.getSubject(), notification.getContent());
        }
    }
}
```


### Listing notifications

Sagi-shi players can list the notifications they received while offline:

```java
int limit = 100;
NotificationList notificationList = client.listNotifications(session, limit).get();
notificationList.getNotificationsList().forEach(notification -> logger.info("Notification: {}:{}\n{}", notification.getCode(), notification.getSubject(), notification.getContent()));
```


**Pagination and cacheable cursors**

Like other listing methods, notification results can be paginated using a cursor or cacheable cursor from the result.

Assuming the cacheable has been saved, the next time the player logs in the cacheable cursor can be used to list unread notifications.

```java
// Assuming this has been saved and loaded
String cacheableCursor = "";
NotificationList nextResults = client.listNotifications(session, limit, cacheableCursor);
```


### Deleting notifications

Sagi-shi players can delete notifications once they've read them:

```java
client.deleteNotifications(session, "<NotificationId>", "<AnotherNotificationId>");
```
