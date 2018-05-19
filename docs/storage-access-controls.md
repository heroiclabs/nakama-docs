# Access controls

The storage engine has two features which control access to objects. Object ownership and access permissions.

## Object ownership

A storage object is created with an owner. The owner is either the user who created it, the system owner, or an owner assigned when the object is created with the code runtime.

An object which is system owned must have public read access permissions before it can be fetched by clients. Access permissions are covered below.

These code examples show how to retrieve an object owned by the system (marked with public read).

```sh fct_label="cURL"
curl -X POST \
  http://127.0.0.1:7350/v2/storage \
  -H 'Authorization: Bearer <session token>' \
  -d '{"object_ids":
    [
      {
        "collection": "configuration",
        "key": "config",
      }
    ]
  }'
```

```js fct_label="Javascript"
const objects = await client.readStorageObjects(session, {
  "object_ids": [{
    "collection": "configurations",
    "key": "config"
  }]
});

console.info("Successfully read objects:", objects);
```

```csharp fct_label=".Net"
var result = await client.ReadStorageObjectsAsync(session, new StorageObjectId {
  Collection = "configuration",
  Key = "config"
});

System.Console.WriteLine("Successfully read objects {0}", result.Objects);
```

```csharp fct_label="Unity"
var result = await client.ReadStorageObjectsAsync(session, new StorageObjectId {
  Collection = "configuration",
  Key = "config"
});

Debug.LogFormat("Successfully read objects {0}", result.Objects);
```

```java fct_label="Android/Java"
// Requires Nakama 1.x

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
// Requires Nakama 1.x

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

```fct_label="REST"
POST /v2/storage
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{"object_ids":
  [
    {
      "collection": "configuration",
      "key": "config"
    }
  ]
}
```

You can also use the code runtime to fetch an object. The code runtime is exempt from the standard rules around access permissions because it is run by the server as authoritative code.

```lua
local object_ids = {
  {collection = "configuration", key = "config", user_id = nil},
}
local objects = nk.storage_read(object_ids)
for _, r in ipairs(objects)
do
  local message = ("value: %q"):format(r.Value)
  print(message)
end
```

A user who writes a storage object from a client will be set as the owner by default while from the code runtime the owner is implied to be the system unless explicitly set.

## Object permissions

An object has permissions which are enforced for the owner of that object when writing or updating the object:

- __ReadPermission__ can have "Public Read" (2), "Owner Read" (1), or "No Read" (0).
- __WritePermission__ can have "Owner Write" (1), or "No Write" (0).

These permissions are ignored when interacting with the storage engine via the code runtime as the server is authoritative and can always read/write objects. This means that "No Read"/"No Write" means that no client can read/write the object.

Objects with permission "Owner Read" and "Owner Write" may only be accessed or modified by the user who owns it. No other client may access the object.

"Public Read" means that any user can read that object. This is very useful for gameplay where users need to share their game state or parts of it with other users. For example you might have users with their own "Army" object who want to battle each other. Each user can write their own object with public read and it can be read by the other user so that it can be rendered on each others' devices.

When modifying objects from the client, the default permission of a object is set to "Owner Read" and "Owner Write". When modifying objects from the code runtime, the default permission of an object is set to "No Read" and "No Write".

!!! note "Listing objects"
    When listing objects you'll only get back objects with appropriate permissions.

```sh fct_label="cURL"
# "2" refers to Public Read permission
# "1" refers to Owner Write permission

curl -X PUT \
  http://127.0.0.1:7350/v2/storage \
  -H 'Authorization: Bearer <session token>' \
  -d '{"objects":
    [
      {
        "collection": "battle",
        "key": "army",
        "value": "{\"soldiers\": 50}",
        "permission_read": 2,
        "permission_write": 1
      }
    ]
  }'
```

```js fct_label="Javascript"

var army_setup = {"soldiers": 50};
// "2" refers to Public Read permission
// "1" refers to Owner Write permission

const object_ids = await client.writeStorageObjects(session,[
  {
    "collection": "saves",
    "key": "savegame",
    "value": army_setup,
    "permission_read": 2,
    "permission_write": 1
  }
]);

console.info("Successfully stored objects:", object_ids);
```

```csharp fct_label=".Net"
var army_setup = "{\"soldiers\": 50}";
// "2" refers to Public Read permission
// "1" refers to Owner Write permission

var result = await client.WriteStorageObjectsAsync(session, new WriteStorageObject
{
  Collection = "saves",
  Key = "savegame",
  Value = army_setup,
  PermissionRead = 2,
  PermissionWrite = 1
});

System.Console.WriteLine("Successfully stored objects {0}", object_ids);
```

```csharp fct_label="Unity"
var army_setup = "{\"soldiers\": 50}";
// "2" refers to Public Read permission
// "1" refers to Owner Write permission

var result = await client.WriteStorageObjectsAsync(session, new WriteStorageObject
{
  Collection = "saves",
  Key = "savegame",
  Value = army_setup,
  PermissionRead = 2,
  PermissionWrite = 1
});

Debug.LogFormat("Successfully stored objects {0}", object_ids);
```

```java fct_label="Android/Java"
// Requires Nakama 1.x

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
// Requires Nakama 1.x

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

```fct_label="REST"
PUT /v2/storage
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{"objects":
  [
    {
      "collection": "battle",
      "key": "army",
      "value": "{\"soldiers\": 50}",
      "permission_read": 2,
      "permission_write": 1
    }
  ]
}
```

You can store an object with custom permissions from the code runtime.

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local new_records = {
  {collection = "battle", key = "army", user_id = user_id, value = {}, permission_read = 2, permission_write = 1}
}
nk.storage_write(new_records)
```
