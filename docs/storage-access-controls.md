# Access controls

The storage engine built-in to Nakama has two main concepts around data access controls. They are record permissions and record ownership. We'll cover both in detail below.

## Record Ownership

A storage record in Nakama always have an owner: It's either has a explicit owner which is the user or an implicit owner which is the the system (if owner is not set when the record is written).

The following code sample shows how to fetch a record that is owned by the system from the client:

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

You can also use the script runtime to fetch a record owned by the system:

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

Owner of a record is automatically set to the currently logged in user when writing new records from the client. However, the owner of a record is set to System when writing records from the script runtime.

## Record Permissions

A record has permissions which are enforced for the owner of that record when writing or updating the record:

- `ReadPermission` can have `Public Read` (2), `Owner Read` (1), `No Read` (0).
- `WritePermission` is simpler with just `Owner Write` (1), and `No Write` (0).

These permissions are ignored when interacting with the storage engine via the script runtime as the server is authoritative and can always read/write records. This means that `No Read`/`No Write` actually means that no client can read/write the record.

Records with permission `Owner Read` and `Owner Write` dictate that they are readable/writable by the owner of the record. No other clients can access or update this record.

Finally `Public Read` means that any user can read that record. A great fit for using this permission is for scenarios where you have users with their own "Army" record and they want to battle each other. So each user will need to know the army setup of the opponent so that it can be rendered on each others' clients.

When storing/updating records from the client, the default permission of a record is set to `Owner Read` and `Owner Write`. However, when storing/updating records from the script runtime, the default permission of a record is set to `No Read` and `No Write`.

!!! note "Listing records"
    When listing records, you'll only get back records with appropriate permissions.

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

You can store a record via the script runtime with custom permissions like this:

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local new_records = {
  {Bucket = "myapp", Collection = "battle", Record = "army", UserId = user_id, Value = {}, PermissionRead = 2, PermissionWrite = 1},
}
nk.storage_write(new_records)
```
