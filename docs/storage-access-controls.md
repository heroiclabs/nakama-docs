# Access controls

The storage engine has two features which control access to objects. Object ownership and access permissions.

## Object ownership

A storage object is created with an owner. The owner is either the user who created it, the system owner, or an owner assigned when the object is created with the code runtime.

An object which is system owned must have public read access permissions before it can be fetched by clients. Access permissions are covered below.

These code examples show how to retrieve an object owned by the system (marked with public read).

=== "cURL"
	```sh
	curl -X POST "http://127.0.0.1:7350/v2/storage" \
	  -H 'Authorization: Bearer <session token>' \
	  -d '{
	    "object_ids": [
	      {
	        "collection": "configuration",
	        "key": "config",
	      }
	    ]
	  }'
	```

=== "JavaScript"
	```js
	const objects = await client.readStorageObjects(session, {
	  "object_ids": [{
	    "collection": "configurations",
	    "key": "config"
	  }]
	});
	console.info("Read objects: %o", objects);
	```

=== ".NET"
	```csharp
	var result = await client.ReadStorageObjectsAsync(session, new StorageObjectId {
	  Collection = "configuration",
	  Key = "config"
	});
	Console.WriteLine("Read objects: [{0}]", string.Join(",\n  ", result.Objects));
	```

=== "Unity"
	```csharp
	var result = await client.ReadStorageObjectsAsync(session, new StorageObjectId {
	  Collection = "configuration",
	  Key = "config"
	});
	Debug.LogFormat("Read objects: [{0}]", string.Join(",\n  ", result.Objects));
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](const NStorageObjects& objects)
	{
	  for (auto& object : objects)
	  {
	    CCLOG("Object key: %s, value: %s", object.key.c_str(), object.value.c_str());
	  }
	};

	std::vector<NReadStorageObjectId> objectIds;
	NReadStorageObjectId objectId;
	objectId.collection = "configurations";
	objectId.key = "config";
	objectIds.push_back(objectId);
	client->readStorageObjects(session, objectIds, successCallback);
	```

=== "Cocos2d-x JS"
	```js
	client.readStorageObjects(session, {
	  "object_ids": [{
	    "collection": "configurations",
	    "key": "config"
	  }]
	}).then(function(objects) {
	      cc.log("Read objects:", JSON.stringify(objects));
	    },
	    function(error) {
	      cc.error("read storage objects failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](const NStorageObjects& objects)
	{
	  for (auto& object : objects)
	  {
	    std::cout << "Object key: " << object.key << ", value: " << object.value << std::endl;
	  }
	};

	std::vector<NReadStorageObjectId> objectIds;
	NReadStorageObjectId objectId;
	objectId.collection = "configurations";
	objectId.key = "config";
	objectIds.push_back(objectId);
	client->readStorageObjects(session, objectIds, successCallback);
	```

=== "Java"
	```java
	StorageObjectId objectId = new StorageObjectId("configuration");
	objectId.setKey("config");
	StorageObjects objects = client.readStorageObjects(session, objectId).get();
	System.out.format("Read objects %s", objects.getObjectsList().toString());
	```

=== "Swift"
	```swift
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

=== "Godot"
	```gdscript
	var result : NakamaAPI.ApiStorageObjects = yield(client.read_storage_objects_async(session, [
		NakamaStorageObjectId.new("configuration", "config")
	]), "completed")
	if result.is_exception():
		print("An error occured: %s" % result)
		return
	print("Read objects:")
	for o in result.objects:
		print("%s" % o)
	```

=== "REST"
    ```
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

=== "Lua"
	```lua
	local object_ids = {
	  { collection = "configuration", key = "config", user_id = nil },
	}
	local objects = nk.storage_read(object_ids)
	for _, o in ipairs(objects) do
	  local message = ("value: %q"):format(o.value)
	  nk.logger_info(message)
	end
	```

=== "Go"
	```go
	objectIds := []*runtime.StorageRead{
		&runtime.StorageRead{
			Collection: "configuration",
			Key: "config",
		},
	}

	objects, err := nk.StorageRead(ctx, objectIds)
	if err != nil {
		// Handle error.
	} else {
		for _, object := range objects {
			logger.Info("value: %s", object.Value)
		}
	}
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

=== "cURL"
	```sh
	# "2" refers to Public Read permission
	# "1" refers to Owner Write permission
	curl -X PUT "http://127.0.0.1:7350/v2/storage" \
	  -H 'Authorization: Bearer <session token>' \
	  -d '{
	    "objects": [
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

=== "JavaScript"
	```js
	var army_setup = { "soldiers": 50 };
	// "2" refers to Public Read permission
	// "1" refers to Owner Write permission
	const object_ids = await client.writeStorageObjects(session, [
	  {
	    "collection": "saves",
	    "key": "savegame",
	    "value": army_setup,
	    "permission_read": 2,
	    "permission_write": 1
	  }
	]);
	console.info("Stored objects: %o", object_ids);
	```

=== ".NET"
	```csharp
	var armySetup = "{ \"soldiers\": 50 }";
	// "2" refers to Public Read permission
	// "1" refers to Owner Write permission
	var result = await client.WriteStorageObjectsAsync(session, new WriteStorageObject
	{
	  Collection = "saves",
	  Key = "savegame",
	  Value = armySetup,
	  PermissionRead = 2,
	  PermissionWrite = 1
	});
	Console.WriteLine("Stored objects: [{0}]", string.Join(",\n  ", result.Objects));
	```

=== "Unity"
	```csharp
	var armySetup = "{ \"soldiers\": 50 }";
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
	Debug.LogFormat("Stored objects: [{0}]", string.Join(",\n  ", result.Objects));
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](const NStorageObjectAcks& acks)
	{
	};

	std::vector<NStorageObjectWrite> objects;
	NStorageObjectWrite object;
	object.collection = "saves";
	object.key = "savegame";
	object.value = "{ \"soldiers\": 50 }";
	object.permissionRead = NStoragePermissionRead::PUBLIC_READ;   // Public Read permission
	object.permissionWrite = NStoragePermissionWrite::OWNER_WRITE; // Owner Write permission
	objects.push_back(object);
	client->writeStorageObjects(session, objects, successCallback);
	```

=== "Cocos2d-x JS"
	```js
	var army_setup = { "soldiers": 50 };
	// "2" refers to Public Read permission
	// "1" refers to Owner Write permission
	client.writeStorageObjects(session, [
	  {
	    "collection": "saves",
	    "key": "savegame",
	    "value": army_setup,
	    "permission_read": 2,
	    "permission_write": 1
	  }
	]).then(function(object_ids) {
	      cc.log("Stored objects:", JSON.stringify(object_ids));
	    },
	    function(error) {
	      cc.error("write storage objects failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](const NStorageObjectAcks& acks)
	{
	};

	std::vector<NStorageObjectWrite> objects;
	NStorageObjectWrite object;
	object.collection = "saves";
	object.key = "savegame";
	object.value = "{ \"soldiers\": 50 }";
	object.permissionRead = NStoragePermissionRead::PUBLIC_READ;   // Public Read permission
	object.permissionWrite = NStoragePermissionWrite::OWNER_WRITE; // Owner Write permission
	objects.push_back(object);
	client->writeStorageObjects(session, objects, successCallback);
	```

=== "Java"
	```java
	String armySetup = "{ \"soldiers\": 50 }";
	StorageObjectWrite object = new StorageObjectWrite("saves", "savegame", armySetup, PermissionRead.PUBLIC_READ, PermissionWrite.OWNER_WRITE);
	StorageObjectAcks acks = client.writeStorageObjects(session, object).get();
	System.out.format("Stored objects %s", acks.getAcksList());
	```

=== "Swift"
	```swift
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

=== "Godot"
	```gdscript
	var army_setup = "{ \"soldiers\": 50 }";
	# "2" refers to Public Read permission
	# "1" refers to Owner Write permission
	var acks : NakamaAPI.ApiStorageObjectAcks = yield(client.write_storage_objects_async(session, [
		NakamaWriteStorageObject.new("saves", "savegame", 2, 1, army_setup, "")
	]), "completed")
	if acks.is_exception():
		print("An error occured: %s" % acks)
		return
	print("Stored objects: %s" % [acks.acks])
	```

=== "REST"
    ```
	PUT /v2/storage
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>

	{
	  "objects": [
	    {
	      "collection": "battle",
	      "key": "army",
	      "value": "{ \"soldiers\": 50 }",
	      "permission_read": 2,
	      "permission_write": 1
	    }
	  ]
	}
	```

You can store an object with custom permissions from the code runtime.

=== "Lua"
	```lua
	local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- Some user ID.
	local new_objects = {
	  { collection = "battle", key = "army", user_id = user_id, value = {}, permission_read = 2, permission_write = 1 }
	}
	nk.storage_write(new_objects)
	```

=== "Go"
	```go
	userID := "4ec4f126-3f9d-11e7-84ef-b7c182b36521" // Some user ID.
	objects := []*runtime.StorageWrite{
		&runtime.StorageWrite{
			Collection:      "battle",
			Key:             "army",
			UserID:          userID,
			Value:           "{}",
			PermissionRead:  2,
			PermissionWrite: 1,
		},
	}

	if _, err := nk.StorageWrite(ctx, objects); err != nil {
		// Handle error.
	}
	```
