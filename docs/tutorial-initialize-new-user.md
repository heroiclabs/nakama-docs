# Initialize a new user

It's often useful when a new user registers to have a bunch of records setup for them. In games this could be needed for a user's virtual wallet, initial inventory items, etc. In this tutorial we'll cover a few different ways to handle this use case.

<!--

!!! Summary
    While there are various ways to solve this use case we highly recommend you [initialize the records on usage](#initialize-record-when-used).

-->

## After register callback

The simplest approach is to write records in the success callback for the register function in a client.

This code demonstrates how to do it with a condensed example. In real application code you'll break up the [authentication](authentication.md) and connect logic from the storage writes based on how you manage connect and reconnect.

=== "Unity"
	```csharp
	var deviceId = SystemInfo.deviceUniqueIdentifier;
	var session = await client.AuthenticateDeviceAsync(deviceId);
	
	var json = "{\"coins\": 100, \"gems\": 10, \"artifacts\": 0}";
	var object = new WriteStorageObject = {
	  "collection" = "wallets",
	  "key" = "mywallet",
	  "value" = json
	};
	const storageWriteAck = await client.WriteStorageObjectsAsync(session, objects);
	Debug.Log("Successfully setup new user's records.");
	```

This code has tradeoffs which should be noted. A disconnect can happen before the records are written to storage. This may leave the setup of the user incomplete and the application in a bad state.

This option is only worth choosing when you want to avoid writing server-side code or have built retry logic on top of a client.

## Server-side hook

Another way to write records for the new user is to run server-side code after registration has completed. This can be done with a [register hook](runtime-code-basics.md#register-hooks).

The ["register_after"](runtime-code-function-reference.md#register-hooks) hook can be used with one of the `"authenticaterequest_*"` message types to tell the server to run a function after that message has been processed. It's important to note that the server does not distinguish between register and login messages so we use a [conditional write](storage-collections.md#conditional-writes) to store the records.

=== "Lua"
	```lua
	local function initialize_user(context, payload)
	  if payload.created then
	    -- Only run this logic if the account that has authenticated is new.
	    local changeset = {
	      coins = 10,   -- Add 10 coins to the user's wallet.
	      gems = 5      -- Add 5 gems from the user's wallet.
	      artifacts = 0 -- No artifacts to start with.
	    }
	    local metadata = {}
	    nk.wallet_update(context.user_id, changeset, metadata, true)
	  end
	end
	
	-- change to whatever message name matches your authentication type.
	nk.register_req_after(initialize_user, "AuthenticateDevice")
	```

=== "Go"
	```go
	func InitializeUser(ctx context.Context, logger Logger, db *sql.DB, nk NakamaModule, out *api.Session, in *api.AuthenticateDeviceRequest) error {
	  if out.Created {
	    // Only run this logic if the account that has authenticated is new.
	    userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	    if !ok {
	      return "", errors.New("Invalid context")
	    }
	    changeset := map[string]interface{}{
	      "coins": 10,    // Add 10 coins to the user's wallet.
	      "gems":  5,     // Add 5 gems from the user's wallet.
	      "artifacts": 0, // No artifacts to start with.
	    }
	    var metadata map[string]interface{}
	    if err := nk.WalletUpdate(ctx, userID, changeset, metadata, true); err != nil {
	      // Handle error.
	    }
	  }
	}
	
	// Register as after hook, this call should be in InitModule.
	if err := initializer.RegisterAfterAuthenticateDevice(InitializeUser); err != nil {
	  logger.Error("Unable to register: %v", err)
	  return err
	}
	```

This approach avoids the tradeoff with client disconnects but requires a database write to happen after every login or register message. This could be acceptable depending on how frequently you write data to the storage engine and can be minimized if you [cache a user's session](authentication.md#sessions) for quick reconnects.

<!--

## Initialize record when used

The last way to write initial records for the user is to `"init"` the record with a storage update wherever it's written to in application code. With this approach you never use storage writes and always perform all write operations as updates.

In our example it means wherever you will update the "mywallet" record you ensure it's been initialized first.

=== "Unity"
	```csharp
	var json = "{\"coins\": 100, \"gems\": 10, \"artifacts\": 0}";
	
	var message = new NStorageUpdateMessage.Builder()
	    .Update("mygame", "wallets", "mywallet", new StorageUpdateBuilder()
	        // make sure record is setup as
	        //  {"coins": 100, "gems": 10, "artifacts": 0}
	        .Init("/coins", 100)
	        .Init("/gems", 10)
	        .Init("/artifacts", 0)
	        .Incr("/coins", -10) // perform other updates to the record.
	        .Build())
	    .Build();
	client.Send(message, (INResultSet<INStorageKey> list) => {
	  Debug.Log("Updated user's storage records.");
	}, (INError err) {
	  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
	});
	```

You can also perform the "initialize before update" with server-side code.

	```lua
	local nk = require("nakama")
	
	local value = {
	  coins = 100,
	  gems = 10,
	  artifacts = 0
	}
	local update_ops = {
	  -- make sure record is setup as
	  --  {"coins": 100, "gems": 10, "artifacts": 0}
	  { Op = "init", Path = "/coins", Value = 100 },
	  { Op = "init", Path = "/gems", Value = 10 },
	  { Op = "init", Path = "/artifacts", Value = 0 },
	
	  -- perform other updates to the record.
	  { Op = "incr", Path = "/coins", Value = -10 }
	}
	local record = {
	  Bucket = "mygame",
	  Collection = "wallets",
	  Record = "mywallet",
	  UserId = context.UserId,
	  Update = update_ops
	}
	nk.storage_update({ record })
	```

This is our recommended approach. It has no tradeoffs compared with the other approaches except that you must remember to add `"init"` logic wherever the record would be updated.

-->
