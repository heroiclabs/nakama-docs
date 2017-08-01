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
    Debug.LogFormat("Successfully stored record has version '{0}'", version);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
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
    Debug.LogFormat("Successfully stored record has version '{0}'", version);
  }
}, (INError error) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
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
