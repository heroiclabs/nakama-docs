# Initialize a new user

It's often useful when a new user registers to have a bunch of records setup for them. In games this could be needed for a user's virtual wallet, initial inventory items, etc. In this tutorial we'll cover a few different ways to handle this use case.

!!! Summary
    While there are various ways to solve this use case we highly recommend you [initialize the records on usage](#initialize-record-when-used).

## After register callback

The simplest approach is to write records in the success callback for the register function in a client.

This code demonstrates how to do it with a condensed example. In real application code you'll break up the [authentication](authentication.md) and connect logic from the storage writes based on how you manage connect and reconnect.

```csharp fct_label="Unity"
var errorHandler = delegate(INError err) {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

var id = SystemInfo.deviceUniqueIdentifier;
// Use one of the user register messages.
var authMessage = NAuthenticateMessage.Device(id);
client.Register(authMessage, (INSession session) => {
  client.Connect(session, (bool done) => {
    var jsonString = "{\"coins\": 100, \"gems\": 10, \"artifacts\": 0}";
    byte[] json = Encoding.UTF8.GetBytes(jsonString);

    var message = new NStorageWriteMessage.Builder()
        .Write("mygame", "wallets", "mywallet", json)
        .Build();
    client.Send(message, (INResultSet<INStorageKey> list) => {
      Debug.Log("Successfully setup new user's records.");
    }, errorHandler);
  });
}, errorHandler);
```

This code has tradeoffs which should be noted. A disconnect can happen before the records are written to storage. This may leave the setup of the user incomplete and the application in a bad state.

This option is only worth choosing when you want to avoid writing server-side code or have built retry logic on top of a client.

## Server-side hook

Another way to write records for the new user is to run server-side code after registration has completed. This can be done with a [register hook](runtime-code-basics.md#register-hooks).

The ["register_after"](runtime-code-function-reference.md#register-hooks) hook can be used with one of the `"authenticaterequest_*"` message types to tell the server to run a function after that message has been processed. It's important to note that the server does not distinguish between register and login messages so we use a [conditional write](storage-collections.md#conditional-writes) to store the records.

```lua
local nk = require("nakama")

local function initialize_user(context, _payload)
  local value = {
    coins = 100,
    gems = 10,
    artifacts = 0
  }
  local record = {
    Bucket = "mygame",
    Collection = "wallets",
    Record = "mywallet",
    UserId = context.UserId,
    Value = value,
    Version = "*"   -- only write record if one doesn't already exist.
  }
  pcall(nk.storage_write, { record }) -- write record, ignore errors.
end

-- change to whatever message name matches your authentication type.
nk.register_after(initialize_user, "authenticaterequest_device")
```

This approach avoids the tradeoff with client disconnects but requires a database write to happen after every login or register message. This could be acceptable depending on how frequently you write data to the storage engine and can be minimized if you [cache a user's session](authentication.md#sessions) for quick reconnects.

## Initialize record when used

The last way to write initial records for the user is to `"init"` the record with a storage update wherever it's written to in application code. With this approach you never use storage writes and always perform all write operations as updates.

In our example it means wherever you will update the "mywallet" record you ensure it's been initialized first.

```csharp fct_label="Unity"
var jsonString = "{\"coins\": 100, \"gems\": 10, \"artifacts\": 0}";
byte[] json = Encoding.UTF8.GetBytes(jsonString);

var message = new NStorageUpdateMessage.Builder()
    .Update("mygame", "wallets", "mywallet", new StorageUpdateBuilder()
        .Init("", json)     // make sure record is setup.
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
  { Op = "init", Path = "", Value = value }, -- make sure record is setup.

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
