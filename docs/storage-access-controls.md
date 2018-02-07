# Access controls

The storage engine has two features which control access to records. Record ownership and access permissions.

## Record Ownership

A storage record is created with an owner. The owner is either the user who created it, the system owner, or an owner assigned when the record is created with the code runtime.

A record which is system owned must have public read access permissions before it can be fetched by clients. Access permissions are covered below.

These code examples show how to retrieve a record owned by the system (marked with public read).

```csharp fct_label="Unity"
string userId = session.Id; // an INSession object.

var message = new NStorageFetchMessage.Builder()
    // "null" below means system owned record
    .Fetch("myapp", "configuration", "config", null)
    .Build();
client.Send(message, (INResultSet<INStorageData> list) => {
  foreach (var record in list.Results) {
    Debug.LogFormat("Record value '{0}'", record.Data);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
byte[] userId = session.getId(); // a Session object's Id.

CollatedMessage<ResultSet<StorageRecord>> message = StorageFetchMessage.Builder.newBuilder()
    // "null" below means system owned record
    .record("myapp", "configuration", "config", null)
    .build();
Deferred<ResultSet<StorageRecord>> deferred = client.send(message);
deferred.addCallback(new Callback<ResultSet<StorageRecord>, ResultSet<StorageRecord>>() {
  @Override
  public ResultSet<StorageRecord> call(ResultSet<StorageRecord> list) throws Exception {
    for (StorageRecord record : list) {
      String value = new String(record.getValue());
      System.out.format("Record value '%s'", value);
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
let userID = session.userID // a Session object's Id.

var message = StorageFetchMessage()
// "userID: nil" below means system owned record
message.fetch(bucket: "myapp", collection: "configuration", key: "config", userID: nil)
client.send(message: message).then { list in
  for record in list {
    NSLog("Record value '%@'", record.value)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var userId = session.id // a Session object's Id.

var message = new nakamajs.StorageFetchRequest();
// "null" below means system owned record
message.fetch("myapp", "configuration", "config", null);
client.send(message).then(function(records){
  records.data.forEach(function(record) {
    console.log("Record value '%o'", record.value);
  })
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

You can also use the code runtime to fetch a record. The code runtime is exempt from the standard rules around access permissions because it is run by the server as authoritative code.

```lua
local record_keys = {
  {Bucket = "myapp", Collection = "configuration", Record = "config", UserId = nil},
}
local records = nk.storage_fetch(record_keys)
for _, r in ipairs(records)
do
  local message = ("value: %q"):format(r.Value)
  print(message)
end
```

A user who writes a storage record from a client will be set as the owner by default while from the code runtime the owner is implied to be the system unless explicitly set.

## Record Permissions

A record has permissions which are enforced for the owner of that record when writing or updating the record:

- __ReadPermission__ can have "Public Read" (2), "Owner Read" (1), or "No Read" (0).
- __WritePermission__ can have "Owner Write" (1), or "No Write" (0).

These permissions are ignored when interacting with the storage engine via the code runtime as the server is authoritative and can always read/write records. This means that "No Read"/"No Write" means that no client can read/write the record.

Records with permission "Owner Read" and "Owner Write" may only be accessed or modified by the user who owns it. No other client may access the record.

"Public Read" means that any user can read that record. This is very useful for gameplay where users need to share their game state or parts of it with other users. For example you might have users with their own "Army" record who want to battle each other. Each user can write their own record with public read and it can be fetched by the other user so that it can be rendered on each others' devices.

When modifying records from the client, the default permission of a record is set to "Owner Read" and "Owner Write". When modifying records from the code runtime, the default permission of a record is set to "No Read" and "No Write".

!!! note "Listing records"
    When listing records you'll only get back records with appropriate permissions.

```csharp fct_label="Unity"
string armySetup = "{\"soldiers\": 50}";

var message = new NStorageWriteMessage.Builder()
    .Write("myapp", "battle", "army", armySetup, StoragePermissionRead.PublicRead, StoragePermissionWrite.OwnerWrite)
    .Build();
client.Send(message, (INResultSet<INStorageKey> list) => {
  foreach (var record in list.Results) {
    Debug.LogFormat("Stored record has version '{0}'", record.Version);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
String armySetup = "{\"soldiers\": 50}";

CollatedMessage<ResultSet<RecordId>> message = StorageWriteMessage.Builder.newBuilder()
    .record("myapp", "battle", "army", armySetup, StorageRecord.PermissionRead.PUBLIC_READ, StorageRecord.PermissionWrite.OWNER_WRITE)
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
let armySetup = "{\"soldiers\": 50}".data(using: .utf8)!

var message = StorageWriteMessage()
message.write(bucket: "myapp", collection: "battle", key: "army", value: armySetup, readPermission: PermissionRead.publicRead, writePermission: PermissionWrite.ownerWrite)
client.send(message: message).then { list in
  for recordId in list {
    NSLog("Stored record has version '%@'", recordId.version)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```js fct_label="Javascript"
var armySetup = {"soldiers": 50};

var message = new nakamajs.StorageWriteRequest();
// "2" refers to Public Read permission
// "1" refers to Owner Write permission
message.write("myapp", "battle", "army", armySetup, 2, 1);
client.send(message).then(function(result){
  result.keys.forEach(function(storageKey) {
    console.log("Stored record has version %o", storageKey.version);
  })
}).catch(function(error){
  console.log("An error occured: %o", error);
})
```

You can store a record with custom permissions from the code runtime.

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local new_records = {
  {Bucket = "myapp", Collection = "battle", Record = "army", UserId = user_id, Value = {}, PermissionRead = 2, PermissionWrite = 1}
}
nk.storage_write(new_records)
```
