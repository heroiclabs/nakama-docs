# Collections

Every app or game has data which is specific to the project.

This information must be stored for users, updated, retrieved, and displayed within various parts of a UI. For this purpose the server incorporates a storage engine with a design optimized for record ownership, access permissions, and batch operations.

Data is stored in collections with one or more records which contain a unique key with JSON content. A collection is created without any configuration required and are grouped into buckets. This creates a simple nested namespace which represents the location of a record.

```
Bucket
+---------------------------------------------------------------------------+
|  Collection                                                               |
|  +---------------------------------------------------------------------+  |
|  |  Record                                                             |  |
|  |  +----------+------------+-------------+-----+-------------------+  |  |
|  |  | ?UserId? | Identifier | Permissions | ... |       Value       |  |  |
|  |  +---------------------------------------------------------------+  |  |
|  +---------------------------------------------------------------------+  |
+---------------------------------------------------------------------------+
```

This design gives great flexibility for developers to group sets of information which belong together within a game or app.

## Write records

A user can write one or more records which will be stored in the database server. These records will be written in a single transaction which guarantees the writes succeed together.

!!! Tip
    In most cases you won't need to group data under multiple buckets so it can be easiest to just name it after your project. For example "Super Mario Run" could use a bucket name "smr" or similar.

```csharp fct_label="Unity"
byte[] saveGame = Encoding.UTF8.GetBytes("{\"progress\": 50}");
byte[] myStats = Encoding.UTF8.GetBytes("{\"skill\": 24}");

var bucket = "myapp";

// Write multiple different records across collections.
var message = new NStorageWriteMessage.Builder()
    .Write(bucket, "saves", "savegame", saveGame)
    .Write(bucket, "stats", "mystats", myStats)
    .Build();
client.Send(message, (INResultSet<INStorageKey> list) => {
  foreach (var record in list.Results) {
    var version = Encoding.UTF8.GetString(record.Version);
    Debug.LogFormat("Stored record has version '{0}'", version);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
byte[] saveGame = "{\"progress\": 50}".getBytes();
byte[] myStats = "{\"skill\": 24}".getBytes();

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
let saveGame = "{\"progress\": 50}".data(using: .utf8)!
let myStats = "{\"skill\": 24}".data(using: .utf8)!

let bucket = "myapp"
var message = StorageWriteMessage()
message.write(bucket: bucket, collection: "saves", key: "savegame", value: saveGame)
message.write(bucket: bucket, collection: "saves", key: "mystats", value: myStats)
client.send(message: message).then { list in
  for recordId in list {
    NSLog("Stored record has version '@%'", recordId.version)
  }
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

### Conditional writes

When records are successfully stored a version is returned which can be used with further updates to perform concurrent modification checks with the next write. This is often known as a conditional write.

A conditional write ensures a client can only update the record if they've seen the previous version of the record already. The goal is to prevent a change to the record if another client has changed the value between when the first client's read and it's next write.

```csharp fct_label="Unity"
byte[] saveGame = Encoding.UTF8.GetBytes("{\"progress\": 54}");
byte[] version = record.Version; // an INStorageKey object.

var message = new NStorageWriteMessage.Builder()
    .Write("myapp", "saves", "savegame", saveGame, version)
    .Build();
client.Send(message, (INResultSet<INStorageKey> list) => {
  version = list.Results[0].Version; // cache updated version for next write.
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
byte[] saveGame = "{\"progress\": 54}".getBytes();
byte[] version = record.getVersion(); // a RecordId object's version.

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
let saveGame = "{\"progress\": 54}".data(using: .utf8)!
var version = record.version // a StorageRecordId object's version.

var message = StorageWriteMessage()
message.write(bucket: "myapp", collection: "saves", key: "savegame", value: saveGame, version: version)
client.send(message: message).then { list in
  version = list[0].version // Cache updated version for next write.
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

We support another kind of conditional write which is used to write a record only if none already exists for that record's key name.

```csharp fct_label="Unity"
byte[] saveGame = Encoding.UTF8.GetBytes("{\"progress\": 1}");
byte[] version = Encoding.UTF8.GetBytes("*"); // represents "no version".

var message = new NStorageWriteMessage.Builder()
    .Write("myapp", "saves", "savegame", saveGame, version)
    .Build();
client.Send(message, (INResultSet<INStorageKey> list) => {
  version = list.Results[0].Version; // cache updated version for next write.
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
byte[] saveGame = "{\"progress\": 1}".getBytes();
byte[] version = "*".getBytes(); // represents "no version".

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
let saveGame = "{\"progress\": 1}".data(using: .utf8)!
var version = "*".data(using: .utf8)! // represents "no version".

var message = StorageWriteMessage()
message.write(bucket: "myapp", collection: "saves", key: "savegame", value: saveGame, version: version)
client.send(message: message).then { list in
  version = list[0].version // Cache updated version for next write.
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

## Fetch records

Just like with [writing records](#write-records) you can fetch one or more records from the database server.

Each record has an owner and permissions. A record can only be fetched if the permissions allow it. A record which has no owner can be fetched with `null` and is useful for global records which all users should be able to read.

```csharp fct_label="Unity"
byte[] userId = session.Id; // an INSession object.

var message = new NStorageFetchMessage.Builder()
    .Fetch("myapp", "saves", "savegame", userId)
    .Fetch("myapp", "configuration", "config", null)
    .Build();
client.Send(message, (INResultSet<INStorageData> list) => {
  foreach (var record in list.Results) {
    var data = Encoding.UTF8.GetString(record.Data);
    Debug.LogFormat("Record value '{0}'", data);
    Debug.LogFormat("Record permissions read '{0}' write '{1}'",
        record.PermissionRead, record.PermissionWrite);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
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
let userId = session.userID // a Session object's Id.

var message = StorageFetchMessage()
message.fetch(bucket: "myapp", collection: "saves", key: "savegame", userID: userId)
message.fetch(bucket: "myapp", collection: "configuration", key: "config", userID: nil)
client.send(message: message).then { list in
  for record in list {
    NSLog("Record value '@%'", record.value)
    NSLog("Record permissions read '@%' write '@%'",
        record.permissionRead, record.permissionWrite)
  }
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

## List records

You can list records in a collection and page through results. The records returned can be filter to those owned by the user or `"null"` for public records which aren't owned by a user.

```csharp fct_label="Unity"
byte[] userId = session.Id; // an INSession object.

var message = new NStorageListMessage.Builder()
    .Bucket("myapp")
    .Collection("saves")
    .UserId(userId)
    .Build();
client.Send(message, (INResultSet<INStorageData> list) => {
  foreach (var record in list.Results) {
    var data = Encoding.UTF8.GetString(record.Data);
    Debug.LogFormat("Record value '{0}'", data);
    Debug.LogFormat("Record permissions read '{0}' write '{1}'",
        record.PermissionRead, record.PermissionWrite);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
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
let userId = session.userID // a Session object's Id.

var message = StorageListMessage(bucket: "myapp")
message.collection = "saves"
message.userID = userId
client.send(message: message).then { list in
  for record in list {
    NSLog("Record value '@%'", record.value)
    NSLog("Record permissions read '@%' write '@%'",
        record.permissionRead, record.permissionWrite)
  }
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

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
var jsonString = "{\"coins\": 100, \"gems\": 10, \"artifacts\": 0}";
byte[] json = Encoding.UTF8.GetBytes(jsonString);

var message = new NStorageUpdateMessage.Builder()
    .Update("myapp", "wallets", "wallet", new NStorageUpdateMessage.StorageUpdateBuilder()
        .Init("", json)
        .Incr("/coins", 10)
        .Incr("/gems", 50)
        .Build())
    .Build();
client.Send(message, (INResultSet<INStorageKey> list) => {
  foreach (var record in list.Results) {
    var version = Encoding.UTF8.GetString(record.Version);
    Debug.LogFormat("Stored record has version '{0}'", version);
  }
}, (INError error) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
byte[] json = "{\"coins\": 100, \"gems\": 10, \"artifacts\": 0}".getBytes();

CollatedMessage<ResultSet<RecordId>> message = StorageUpdateMessage.Builder.newBuilder()
    .record("myapp", "wallets", "wallet", new StorageUpdateMessage.OpBuilder.newBuilder()
        .init("", json)
        .incr("/coins", 10)
        .incr("/gems", 50)
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
  StorageOp(incr: "/foo/coins", value: 10)
  StorageOp(incr: "/foo/gems", value: 50)
]

var message = StorageUpdateMessage()
message.update(bucket: "myapp", collection: "wallets", key: "wallet", ops: ops)
client.send(message: message).then { list in
  for record in list {
    NSLog("Stored record has version '@%'", record.version)
  }
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

## Remove records

A user can remove a record if it has the correct permissions and they own it.

!!! Note "Soft delete"
    A record is not permanently deleted from the server when removed. It remains until overwritten but cannot be read by the client. This is useful in case you need to restore a user's records later.

```csharp fct_label="Unity"
var message = new NStorageRemoveMessage.Builder()
    .Remove("myapp", "saves", "savegame")
    .Build();
client.Send(message, (bool done) => {
  Debug.Log("Removed user's record(s).");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
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
var message = StorageRemoveMessage()
message.remove(bucket: "myapp", collection: "saves", key: "savegame")
client.send(message: message).then {
  NSLog("Removed user's record(s).")
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```

You can also conditionally remove an object if the object version matches the version sent by the client.

```csharp fct_label="Unity"
byte[] version = record.Version; // an INStorageKey object.

var message = new NStorageRemoveMessage.Builder()
    .Remove("myapp", "saves", "savegame", version)
    .Build();
client.Send(message, (bool done) => {
  Debug.Log("Removed user's record(s).");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
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
let version = record.version // a StorageRecordId object's version.

var message = StorageRemoveMessage()
message.remove(bucket: "myapp", collection: "saves", key: "savegame", version: version)
client.send(message: message).then {
  NSLog("Removed user's record(s).")
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```
