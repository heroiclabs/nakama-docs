# Collections

Every app or game has data which is specific to the project.

This information must be stored for users, updated, retrieved, and displayed within various parts of a UI. For this purpose the server incorporates a storage engine with a design optimized for [object ownership](storage-access-controls.md), access permissions, and batch operations.

Data is stored in collections with one or more objects which contain a unique key with JSON content. A collection is created without any configuration required and are grouped into buckets. This creates a simple nested namespace which represents the location of a object.

```
Collection
+---------------------------------------------------------------------+
|  Object                                                             |
|  +----------+------------+-------------+-----+-------------------+  |
|  | ?UserId? | Identifier | Permissions | ... |       Value       |  |
|  +---------------------------------------------------------------+  |
+---------------------------------------------------------------------+
```

This design gives great flexibility for developers to group sets of information which belong together within a game or app.

## Write objects

A user can write one or more objects which will be stored in the database server. These objects will be written in a single transaction which guarantees the writes succeed together.

!!! Tip
    In most cases you won't need to group data under multiple collections so it can be easiest to just name it after your project. For example "Super Mario Run" could use a collection "smr" or similar.

```sh fct_label="cURL"
curl -X PUT \
  http://127.0.0.1:7350/v2/storage \
  -H 'Authorization: Bearer <session token>' \
  -d '{"objects":
    [
      {
        "collection": "saves",
        "key": "savegame",
        "value": "{\"progress\": \"50\"}"
      },
      {
        "collection": "stats",
        "key": "skill",
        "value": "{\"progress\": \"24\"}"
      }
    ]
  }'
```

```js fct_label="Javascript"
var save_game = {"progress": 50};
var my_stats = {"skill": 24};

const object_ids = await client.writeStorageObjects(session,[
  {
    "collection": "saves",
    "key": "savegame",
    "value": save_game
  },
  {
    "collection": "stats",
    "key": "skills",
    "value": my_stats
  }
]);

console.info("Successfully stored objects:", object_ids);
```

```csharp fct_label=".Net"
var saveGame = "{\"progress\": 50}";
var myStats = "{\"skill\": 24}";

var result = await client.WriteStorageObjectsAsync(session, new WriteStorageObject
{
  Collection = "saves",
  Key = "savegame",
  Value = saveGame
},
{
  Collection = "stats",
  Key = "skills",
  Value = myStats
});

System.Console.WriteLine("Successfully stored objects {0}", object_ids);
```

```csharp fct_label="Unity"
var saveGame = "{\"progress\": 50}";
var myStats = "{\"skill\": 24}";

var result = await client.WriteStorageObjectsAsync(session, new WriteStorageObject
{
  Collection = "saves",
  Key = "savegame",
  Value = saveGame
},
{
  Collection = "stats",
  Key = "skills",
  Value = myStats
});

Debug.LogFormat("Successfully stored objects {0}", object_ids);
```

```java fct_label="Android/Java"
// Requires Nakama 1.x

String saveGame = "{\"progress\": 50}";
String myStats = "{\"skill\": 24}";

String bucket = "myapp";
CollatedMessage<ResultSet<RecordId>> message = StorageWriteMessage.Builder.newBuilder()
    .record(bucket, "saves", "savegame", saveGame)
    .record(bucket, "stats", "mystats", myStats)
    .build();
Deferred<ResultSet<RecordId>> deferred = client.send(message);
deferred.addCallback(new Callback<ResultSet<RecordId>, ResultSet<RecordId>>() {
  @Override
  public ResultSet<RecordId> call(ResultSet<RecordId> list) throws Exception {
    for (RecordId recordId : list) {
      String version = new String(recordId.getVersion());
      System.out.format("Stored record has version '%s'", version);
    }
    return list;
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
// Requires Nakama 1.x

let saveGame = "{\"progress\": 50}".data(using: .utf8)!
let myStats = "{\"skill\": 24}".data(using: .utf8)!

let bucket = "myapp"
var message = StorageWriteMessage()
message.write(bucket: bucket, collection: "saves", key: "savegame", value: saveGame)
message.write(bucket: bucket, collection: "saves", key: "mystats", value: myStats)
client.send(message: message).then { list in
  for recordId in list {
    NSLog("Stored record has version '%@'", recordId.version)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
PUT /v2/storage
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{"objects":
  [
    {
      "collection": "saves",
      "key": "key",
      "value": "{\"hello\": \"world\"}"
    },
    {
      "collection": "stats",
      "key": "skill",
      "value": "{\"progress\": \"24\"}"
    }
  ]
}
```

!!! Hint
    In Swift, make your objects conform to the `Codable` interface to allow for easy interoperability with Nakama's storage operations. For more info, please follow this [guide](https://developer.apple.com/documentation/foundation/archives_and_serialization/encoding_and_decoding_custom_types#overview).

### Conditional writes

When objects are successfully stored a version is returned which can be used with further updates to perform concurrent modification checks with the next write. This is often known as a conditional write.

A conditional write ensures a client can only update the object if they've seen the previous version of the object already. The goal is to prevent a change to the object if another client has changed the value between when the first client's read and it's next write.

```sh fct_label="cURL"
curl -X PUT \
  http://127.0.0.1:7350/v2/storage \
  -H 'Authorization: Bearer <session token>' \
  -d '{"objects":
    [
      {
        "collection": "saves",
        "key": "savegame",
        "value": "{\"progress\": \"50\"}",
        "version": "some-previous-version"
      }
    ]
  }'
```

```js fct_label="Javascript"
var save_game = {"progress": 50};

const object_ids = await client.writeStorageObjects(session,[
  {
    "collection": "saves",
    "key": "savegame",
    "value": save_game,
    "version": "some-previous-version"
  }
]);

console.info("Successfully stored objects:", object_ids);
```

```csharp fct_label=".Net"
var saveGame = "{\"progress\": 50}";

var result = await client.WriteStorageObjectsAsync(session, new WriteStorageObject
{
  Collection = "saves",
  Key = "savegame",
  Value = saveGame,
  Version = "some-previous-version"
});

System.Console.WriteLine("Successfully stored objects {0}", object_ids);
```

```csharp fct_label="Unity"
var saveGame = "{\"progress\": 50}";

var result = await client.WriteStorageObjectsAsync(session, new WriteStorageObject
{
  Collection = "saves",
  Key = "savegame",
  Value = saveGame,
  Version = "some-previous-version"
});

Debug.LogFormat("Successfully stored objects {0}", object_ids);
```

```java fct_label="Android/Java"
// Requires Nakama 1.x

String saveGame = "{\"progress\": 54}";
String version = record.getVersion(); // a RecordId object's version.

CollatedMessage<ResultSet<RecordId>> message = StorageWriteMessage.Builder.newBuilder()
    .record("myapp", "saves", "savegame", saveGame, version)
    .build();
Deferred<ResultSet<RecordId>> deferred = client.send(message);
deferred.addCallback(new Callback<ResultSet<RecordId>, ResultSet<RecordId>>() {
  @Override
  public ResultSet<RecordId> call(ResultSet<RecordId> list) throws Exception {
    // Cache updated version for next write.
    version = list.getResults().get(0).getVersion();
    return list;
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
// Requires Nakama 1.x

let saveGame = "{\"progress\": 54}".data(using: .utf8)!
var version = record.version // a StorageRecordId object's version.

var message = StorageWriteMessage()
message.write(bucket: "myapp", collection: "saves", key: "savegame", value: saveGame, version: version)
client.send(message: message).then { list in
  version = list[0].version // Cache updated version for next write.
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
PUT /v2/storage
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{"objects":
  [
    {
      "collection": "saves",
      "key": "key",
      "value": "{\"hello\": \"world\"}",
      "version": "some-previous-version"
    }
  ]
}
```

We support another kind of conditional write which is used to write an object only if none already exists for that object's key name.

```sh fct_label="cURL"
curl -X PUT \
  http://127.0.0.1:7350/v2/storage \
  -H 'Authorization: Bearer <session token>' \
  -d '{"objects":
    [
      {
        "collection": "saves",
        "key": "savegame",
        "value": "{\"progress\": \"50\"}",
        "version": "*"
      }
    ]
  }'
```

```js fct_label="Javascript"
var save_game = {"progress": 50};

const object_ids = await client.writeStorageObjects(session,[
  {
    "collection": "saves",
    "key": "savegame",
    "value": save_game,
    "version": "*"
  }
]);

console.info("Successfully stored objects:", object_ids);
```

```csharp fct_label=".Net"
var saveGame = "{\"progress\": 50}";

var result = await client.WriteStorageObjectsAsync(session, new WriteStorageObject
{
  Collection = "saves",
  Key = "savegame",
  Value = saveGame,
  Version = "*"
});

System.Console.WriteLine("Successfully stored objects {0}", object_ids);
```

```csharp fct_label="Unity"
var saveGame = "{\"progress\": 50}";

var result = await client.WriteStorageObjectsAsync(session, new WriteStorageObject
{
  Collection = "saves",
  Key = "savegame",
  Value = saveGame,
  Version = "*"
});

Debug.LogFormat("Successfully stored objects {0}", object_ids);
```

```java fct_label="Android/Java"
// Requires Nakama 1.x

String saveGame = "{\"progress\": 1}";
String version = "*"; // represents "no version".

CollatedMessage<ResultSet<RecordId>> message = StorageWriteMessage.Builder.newBuilder()
    .record("myapp", "saves", "savegame", saveGame, version)
    .build();
Deferred<ResultSet<RecordId>> deferred = client.send(message);
deferred.addCallback(new Callback<ResultSet<RecordId>, ResultSet<RecordId>>() {
  @Override
  public ResultSet<RecordId> call(ResultSet<RecordId> list) throws Exception {
    // Cache updated version for next write.
    version = list.getResults().get(0).getVersion();
    return list;
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
// Requires Nakama 1.x

let saveGame = "{\"progress\": 1}".data(using: .utf8)!
var version = "*" // represents "no version".

var message = StorageWriteMessage()
message.write(bucket: "myapp", collection: "saves", key: "savegame", value: saveGame, version: version)
client.send(message: message).then { list in
  version = list[0].version // Cache updated version for next write.
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
PUT /v2/storage
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{"objects":
  [
    {
      "collection": "saves",
      "key": "key",
      "value": "{\"hello\": \"world\"}",
      "version": "*"
    }
  ]
}
```

## Read objects

Just like with [writing objects](#write-objects) you can read one or more objects from the database server.

Each object has an owner and permissions. An object can only be read if the permissions allow it. An object which has no owner can be fetched with `null` and is useful for global objects which all users should be able to read.

```sh fct_label="cURL"
curl -X POST \
  http://127.0.0.1:7350/v2/storage \
  -H 'Authorization: Bearer <session token>' \
  -d '{"object_ids":
    [
      {
        "collection": "saves",
        "key": "savegame",
        "user_id": "some-user-id"
      }
    ]
  }'
```

```js fct_label="Javascript"
const objects = await client.readStorageObjects(session, {
  "object_ids": [{
    "collection": "saves",
    "key": "savegame",
    "user_id": session.user_id
  }]
});

console.info("Successfully read objects:", objects);
```

```csharp fct_label=".Net"
var result = await client.ReadStorageObjectsAsync(session, new StorageObjectId {
  Collection = "saves",
  Key = "savegame",
  UserId = session.UserId
});

System.Console.WriteLine("Successfully read objects {0}", result.Objects);
```

```csharp fct_label="Unity"
var result = await client.ReadStorageObjectsAsync(session, new StorageObjectId {
  Collection = "saves",
  Key = "savegame",
  UserId = session.UserId
});

Debug.LogFormat("Successfully read objects {0}", result.Objects);
```

```java fct_label="Android/Java"
// Requires Nakama 1.x

byte[] userId = session.getId(); // a Session object's Id.

CollatedMessage<ResultSet<StorageRecord>> message = StorageFetchMessage.Builder.newBuilder()
    .record("myapp", "saves", "savegame", userId)
    .record("myapp", "configuration", "config", null)
    .build();
Deferred<ResultSet<StorageRecord>> deferred = client.send(message);
deferred.addCallback(new Callback<ResultSet<StorageRecord>, ResultSet<StorageRecord>>() {
  @Override
  public ResultSet<StorageRecord> call(ResultSet<StorageRecord> list) throws Exception {
    for (StorageRecord record : list) {
      String value = new String(record.getValue());
      System.out.format("Record value '%s'", value);
      System.out.format("Record permissions read '%s' write '%s'",
          record.getPermissionRead(), record.getPermissionWrite());
    }
    return list;
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
// Requires Nakama 1.x

let userID = session.userID // a Session object's Id.

var message = StorageFetchMessage()
message.fetch(bucket: "myapp", collection: "saves", key: "savegame", userID: userID)
message.fetch(bucket: "myapp", collection: "configuration", key: "config", userID: nil)
client.send(message: message).then { list in
  for record in list {
    NSLog("Record value '%@'", record.value)
    NSLog("Record permissions read '%@' write '%@'",
        record.permissionRead, record.permissionWrite)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
POST /v2/storage
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{"object_ids":
  [
    {
      "collection": "saves",
      "key": "savegame",
      "user_id": "some-user-id"
    }
  ]
}
```

## List objects

You can list objects in a collection and page through results. The objects returned can be filter to those owned by the user or `"null"` for public records which aren't owned by a user.

```sh fct_label="cURL"
curl -X GET \
  'http://127.0.0.1:7350/v2/storage/saves?user_id=some-user-id&limit=10' \
  -H 'Authorization: Bearer <session token>'
```

```js fct_label="Javascript"
const objects = await client.listStorageObjects(session, "saves", session.user_id);
console.info("Successfully list objects:", objects);
```

```csharp fct_label=".Net"
// Updated example TBD
```

```csharp fct_label="Unity"
// Updated example TBD
```

```java fct_label="Android/Java"
// Requires Nakama 1.x

byte[] userId = session.getId(); // a Session object's Id.

CollatedMessage<ResultSet<StorageRecord>> message = StorageListMessage.Builder.newBuilder(userId)
    .bucket("myapp")
    .collection("saves")
    .build();
Deferred<ResultSet<StorageRecord>> deferred = client.send(message);
deferred.addCallback(new Callback<ResultSet<StorageRecord>, ResultSet<StorageRecord>>() {
  @Override
  public ResultSet<StorageRecord> call(ResultSet<StorageRecord> list) throws Exception {
    for (StorageRecord record : list) {
      String value = new String(record.getValue());
      System.out.format("Record value '%s'", value);
      System.out.format("Record permissions read '%s' write '%s'",
          record.getPermissionRead(), record.getPermissionWrite());
    }
    return list;
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
// Requires Nakama 1.x

let userId = session.userID // a Session object's Id.

var message = StorageListMessage(bucket: "myapp")
message.collection = "saves"
message.userID = userId
client.send(message: message).then { list in
  for record in list {
    NSLog("Record value '%@'", record.value)
    NSLog("Record permissions read '%@' write '%@'",
        record.permissionRead, record.permissionWrite)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
GET /v2/storage/{{collection}}?user_id={{user_id}}&limit={{limit}};cursor={{cursor}}
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

<!--
## Update records

Every record's value is required to be a JSON object. To update the value you can fetch the object modify it and write it back; but a more efficient option is to update the value on the server based on a set of "operations".

### Operations

You can create a sequence of operations which are applied to the JSON object. If any of the operations fail, then the update is aborted, and none of the changes are applied. Each operation has a code and makes a specific change to the object.

| Operation | Description |
| --------- | ----------- |
| add | Add field or value to array at the path. |
| append | Append a value or array of values at the path. |
| copy | Copy value at the path to another path. |
| incr | Add a positive/negative value to the value at the path. |
| init | Initialize the value at the path only if it's not already present. |
| merge | Perform a merge of the object at the path. |
| move | Move a value from one path to another and remove from the original path. |
| remove | Remove the value or array at the path. |
| replace | Replaces an existing value at the specified path. |
| test | Tests equality of the value at the path. The entire patch set fails if the test fails. |
| compare | Performs a comparator which returns -1, 0, or 1 depending on whether the value is less than, the same, or greater than the value in the path. |

### Paths

Paths are used to refer to certain parts of a JSON object. They're separated by forward slashes `"/name/first"` and will be interpreted as a <a href="http://tools.ietf.org/html/rfc6901" target="\_blank">JSON Pointer</a>.

Array positions in a path are indicated using a zero-indexed integer `"stash.4.item_name"`. You can append to the end of the array with the 'append' operation. If the value specified in the append is an array all of the entries in the value will be appended to the array in the path.

You can update one or more records with different update operations.

```csharp fct_label="Unity"
var json = "{\"coins\": 100, \"gems\": 10, \"artifacts\": 0}";

var message = new NStorageUpdateMessage.Builder()
    .Update("myapp", "wallets", "wallet", new NStorageUpdateMessage.StorageUpdateBuilder()
        .Init("/foo", json)
        .Incr("/foo/coins", 10)
        .Incr("/foo/gems", 50)
        .Build())
    .Build();
client.Send(message, (INResultSet<INStorageKey> list) => {
  foreach (var record in list.Results) {
    var version = record.Version;
    Debug.LogFormat("Stored record has version '{0}'", version);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
String json = "{\"coins\": 100, \"gems\": 10, \"artifacts\": 0}";

CollatedMessage<ResultSet<RecordId>> message = StorageUpdateMessage.Builder.newBuilder()
    .record("myapp", "wallets", "wallet", StorageUpdateMessage.OpBuilder.newBuilder()
        .init("/foo", json)
        .incr("/foo/coins", 10)
        .incr("/foo/gems", 50)
        .build())
    .build();
Deferred<ResultSet<RecordId>> deferred = client.send(message);
deferred.addCallback(new Callback<ResultSet<RecordId>, ResultSet<RecordId>>() {
  @Override
  public ResultSet<RecordId> call(ResultSet<RecordId> list) throws Exception {
    for (RecordId recordId : list) {
      String version = new String(recordId.getVersion());
      System.out.format("Stored record has version '%s'", version);
    }
    return list;
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
let json = "{\"coins\": 100, \"gems\": 10, \"artifacts\": 0}"
let value = json.data(using: .utf8)!

let ops = [
  StorageOp(init_: "/foo", value: value),
  StorageOp(incr: "/foo/coins", value: 10),
  StorageOp(incr: "/foo/gems", value: 50)
]

var message = StorageUpdateMessage()
message.update(bucket: "myapp", collection: "wallets", key: "wallet", ops: ops)
client.send(message: message).then { list in
  for record in list {
    NSLog("Stored record has version '%@'", record.version)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
let value = {"coins": 100, "gems": 10, "artifacts": 0};

var ops = [
  nakamajs.StorageUpdateRequest.init_("/foo", value),
  nakamajs.StorageUpdateRequest.incr("/foo/coins", 10),
  nakamajs.StorageUpdateRequest.incr("/foo/gems", 50)
];

var message = new nakamajs.StorageUpdateRequest();
message.update("myapp", "wallet", "wallet", ops);
client.send(message).then(function(results){
  results.keys.forEach(function(storageKey) {
    console.log("Stored record has version %o", storageKey.version);
  })
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```
-->

## Remove objects

A user can remove an object if it has the correct permissions and they own it.

```sh fct_label="cURL"
curl -X PUT \
  http://127.0.0.1:7350/v2/storage/delete \
  -H 'Authorization: Bearer <session token>' \
  -d '{"object_ids":
    [
      {
        "collection": "saves",
        "key": "savegame"
      }
    ]
  }'
```

```js fct_label="Javascript"
await client.deleteStorageObjects(session, {
  "object_ids": [{
    "collection": "saves",
    "key": "savegame"
  }]
});

console.info("Successfully deleted objects.");
```

```csharp fct_label=".Net"
var result = await client.DeleteStorageObjectsAsync(session, new StorageObjectId {
  Collection = "saves",
  Key = "savegame",
  UserId = session.UserId
});

System.Console.WriteLine("Successfully deleted objects.");
```

```csharp fct_label="Unity"
var result = await client.DeleteStorageObjectsAsync(session, new StorageObjectId {
  Collection = "saves",
  Key = "savegame",
  UserId = session.UserId
});

Debug.Log("Successfully deleted objects.");
```

```java fct_label="Android/Java"
// Requires Nakama 1.x

CollatedMessage<Boolean> message = StorageRemoveMessage.Builder.newBuilder()
    .record("myapp", "saves", "savegame")
    .build();
Deferred<Boolean> deferred = client.send(message);
deferred.addCallback(new Callback<Boolean, Boolean>() {
  @Override
  public Boolean call(Boolean done) throws Exception {
    System.out.println("Removed user's record(s).");
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
// Requires Nakama 1.x

var message = StorageRemoveMessage()
message.remove(bucket: "myapp", collection: "saves", key: "savegame")
client.send(message: message).then {
  NSLog("Removed user's record(s).")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
PUT /v2/storage/delete
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{"object_ids":
  [
    {
      "collection": "saves",
      "key": "savegame"
    }
  ]
}
```

You can also conditionally remove an object if the object version matches the version sent by the client.

```sh fct_label="cURL"
curl -X PUT \
  http://127.0.0.1:7350/v2/storage/delete \
  -H 'Authorization: Bearer <session token>' \
  -d '{"object_ids":
    [
      {
        "collection": "saves",
        "key": "savegame",
        "version": "some-object-version"
      }
    ]
  }'
```

```js fct_label="Javascript"
await client.deleteStorageObjects(session, {
  "object_ids": [{
    "collection": "saves",
    "key": "savegame",
    "version": "some-object-version"
  }]
});

console.info("Successfully deleted objects.");
```

```csharp fct_label=".Net"
var result = await client.DeleteStorageObjectsAsync(session, new StorageObjectId {
  Collection = "saves",
  Key = "savegame",
  UserId = session.UserId,
  Version = "some-object-version"
});

System.Console.WriteLine("Successfully deleted objects.");
```

```csharp fct_label="Unity"
var result = await client.DeleteStorageObjectsAsync(session, new StorageObjectId {
  Collection = "saves",
  Key = "savegame",
  UserId = session.UserId,
  Version = "some-object-version"
});

Debug.Log("Successfully deleted objects.");
```

```java fct_label="Android/Java"
// Requires Nakama 1.x

byte[] version = record.getVersion(); // a RecordId object's version.

CollatedMessage<Boolean> message = StorageRemoveMessage.Builder.newBuilder()
    .record("myapp", "saves", "savegame", version)
    .build();
Deferred<Boolean> deferred = client.send(message);
deferred.addCallback(new Callback<Boolean, Boolean>() {
  @Override
  public Boolean call(Boolean done) throws Exception {
    System.out.println("Removed user's record(s).");
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
// Requires Nakama 1.x

let version = record.version // a StorageRecordId object's version.

var message = StorageRemoveMessage()
message.remove(bucket: "myapp", collection: "saves", key: "savegame", version: version)
client.send(message: message).then {
  NSLog("Removed user's record(s).")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
PUT /v2/storage/delete
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{"object_ids":
  [
    {
      "collection": "saves",
      "key": "savegame",
      "version": "some-object-version"
    }
  ]
}
```

