# Basics

The server integrates the <a href="https://www.lua.org/manual/5.1/manual.html" target="\_blank">Lua programming language</a> as a fast embedded code runtime.

It is useful to run custom logic which isn’t running on the device or browser. The code you deploy with the server can be used immediately by clients so you can change behavior on the fly and add new features faster.

You should use server-side code when you want to set rules around various features like how many [friends](social-friends.md) a user may have or how many [groups](social-groups-clans.md) can be joined. It can be used to run authoritative logic or perform validation checks as well as integrate with other services over HTTPS.

## Load modules

You can create a Lua file wherever you like on the filesystem as long as the server knows where to scan for the folder which contains your code.

By default the server will scan all files within the "data/modules" folder relative to the server file or the folder specified in the YAML [configuration](install-configuration.md#runtime) at startup. You can also specify the modules folder via a command flag when you start the server.

```shell
nakama --runtime.path "$HOME/some/path/"
```

All files with the ".lua" extension will be loaded and evaluated as part of the boot up sequence. Each Lua file represents a module and all code in each module will be run and can be used to register functions which can operate on messages from clients as well as execute logic on demand.

## Simple example

Lets create a module called "example.lua". In it we'll register code to be run by a client as an [RPC call](#register_rpc).

```lua
local nk = require("nakama")

local function some_example(context, payload)
  -- we'll assume payload was sent as JSON and decode it.
  local json = nk.json_decode(payload)

  -- log data sent to RPC call.
  nk.logger_info(("Payload: %q"):format(json))

  local id = nk.uuid_v4()
  -- create a leaderboard with the json as metadata.
  nk.create_leaderboard(id, "desc", "0 0 * * 1", json, false)

  return nk.json_encode({["id"] = id})
  -- will return "{'id': 'some UUID'}" (JSON) as bytes
end

nk.register_rpc(some_example, "my_unique_id")
```

We import the `"nakama"` module which is embedded within the server and contains lots of server-side functions which are helpful as you build your code. You can see all available functions in the [module reference](runtime-code-function-reference.md).

## Register hooks

The code in a module will be evaluated immediately and can be used to register functions which can operate on messages from clients as well as execute logic on demand.

All registered functions receive a "context" table as the first argument and "payload" as the second. The "context" contains fields which depend on when the code is executed.

| Field | Purpose |
| ----- | ------- |
| `context.Env` | A table of key/value pairs which are defined in the YAML [configuration](install-configuration.md) of the server. This is useful to store API keys and other secrets which may be different between servers run in production and in development. |
| `context.ExecutionMode` | The mode associated with the execution context. It's one of these values: "after", "before", "http", or "rpc". |
| `context.UserHandle` | The user handle associated with the execution context. It will always be `nil` on `register_http`. |
| `context.UserId` | The user ID associated with the execution context. It will always be `nil` on `register_http`. |
| `context.UserSessionExp` | The user session expiry in milliseconds associated with the execution context. It will always be `nil` on `register_http`. |

There are four ways to register a function within the runtime each of which is used to handle specific behavior between client and server.

### register_before

Any function may be registered to intercept a message received from a client and operate on it (or reject it) based on custom logic. This is useful to enforce specific rules on top of the standard features in the server.

```lua hl_lines="9"
local nk = require("nakama")

local function limit_friends(context, payload)
  local user = nk.user_fetch_id({context.UserId})[1]
  -- lets assume we've stored a user's level in their metadata.
  if user.Metadata.level <= 10 then
    error("Must reach level 10 before you can add friends.")
  end
  return payload -- important!
end
nk.register_before(limit_friends, "tfriendsadd")
```

The code above fetches the current user's profile and checks the metadata which is assumed to be JSON encoded with `"{level: 12}"` in it. If a user's level is too low an error is thrown to prevent the Friend Add message from being passed onwards in the server pipeline.

!!! Note
    You must remember to return the payload at the end of your function in the same structure as you received it. See `"return payload"` highlighted in the code above.

### register_after

Similar to [`"register_before"`](#register_before) you can attach a function to operate on a message. The registered function will be called after the message has been processed in the pipeline. The custom code will be executed asynchronously after the response message has been sent to a client.

```lua
local nk = require("nakama")

local function add_reward(context, payload)
  local value = {
    user_ids = {payload.UserId}
  }
  local record = {
    Bucket = "mygame",
    Collection = "rewards",
    Record = "reward",
    UserId = context.UserId,
    Value = value
  }
  nk.storage_write({ record })
end

nk.register_after(add_reward, "tfriendsadd")
```

The simple code above writes a record to a user's storage when they add a friend. Any data returned by the function will be discarded.

### register_http

Sometimes it's useful to create HTTP REST handlers which can be used by web services and ease integration into custom server environments. If you want to send a message from a client you should probably use `"register_rpc"` instead.

```lua
local nk = require("nakama")

local function http_handler(context, payload)
  local message = nk.json_encode(payload)
  nk.logger_info(("Message: %q"):format(message))
  return {["context"] = context}
end

nk.register_http(http_handler, "http_handler_path")
```

This function can be called with any HTTP client. For example with cURL you could execute the function with the server.

```shell
curl -X POST "http://127.0.0.1:7350/runtime/http_handler_path?key=defaultkey" \
     -d "{'some': 'data'}" \
     -H 'Content-Type: application/json' \
     -H 'Accept: application/json'
```

!!! Warning "HTTP key"
    You should change the default HTTP key before you deploy your code in production.

### register_rpc

Some logic between client and server is best handled as RPC functions which clients can execute.

```lua
local nk = require("nakama")

local function custom_rpc_func(context, payload)
  nk.logger_info(("Payload: %q"):format(payload))

  -- "payload" is bytes sent by the client we'll JSON decode it.
  local json = nk.json_decode(payload)

  return nkx.json_encode(json)
end

nk.register_rpc(custom_rpc_func, "custom_rpc_func_id")
```

The code above registers a function with the identifier "custom_rpc_func_id". This ID can be used within client code to send an RPC message to execute the function and return the result. Results are always returned as a Lua string (or optionally `nil`).

## Errors and logs

You can handle errors like you would normally in Lua code. If you want to trap the error which occurs in the execution of a function you'll need to execute it via `pcall` as a "protected call".

```lua
local function will_error()
  error("This function will always throw an error!")
end

if pcall(will_error) then
  -- no errors with "will_error"
else
  -- handle errors
end
```

The function `will_error` uses the `error` function in Lua to throw an error with a reason message. The `pcall` will invoke the `will_error` function and trap any errors. We can then handle the success or error cases as needed.

We recommend you use this pattern with your Lua code.

```lua
local nk = require("nakama")

local status, result = pcall(nk.users_fetch_handle, {"22e9ed62"})
if (not status) then
  nk.logger_error(("Error occurred: %q"):format(result))
else
  for _, u in ipairs(result)
  do
    local message = ("id: %q, fullname: %q"):format(u.Id, u.Fullname)
    print(message) -- will appear in console output.
  end
end
```

## Restrictions

The Lua virtual machine embedded in the server uses a restricted set of Lua standard library modules. This ensures the code sandbox cannot tamper with operating system input/output or the filesystem.

The list of available modules are: base module, "math", "os", "string", and "table".

## An example module

As a fun example lets use the [Pokéapi](http://pokeapi.co/) and build a helpful module named "pokeapi.lua".

```lua
local nk = require("nakama")

local M = {}

local API_BASE_URL = "http://pokeapi.co/api/v2/"

function M.lookup_pokemon(name)
  local url = ("%s/pokemon/%s"):format(API_BASE_URL, name)
  local method = "GET"
  local headers = {
    ["Content-Type"] = "application/json",
    ["Accept"] = "application/json"
  }
  local success, code, _, body = pcall(nk.http_request, url, method, headers, nil)
  if (not success) then
    nk.logger_error(("Failed request %q"):format(code))
    error(code)
  elseif (code >= 400) then
    nk.logger_error(("Failed request %q %q"):format(code, body))
    error(body)
  else
    return nk.json_decode(body)
  end
end

return M
```

We can import it into another module we'll call "pokemon.lua" which will register an RPC call.

```lua
local nk = require("nakama")
local pokeapi = require("pokeapi")

local function get_pokemon(_, payload)
  -- we'll assume payload was sent as JSON and decode it.
  local json = nk.json_decode(payload)

  local success, result = pcall(pokeapi.lookup_pokemon, json.PokemonName)
  if (not success) then
    error("Unable to lookup pokemon.")
  else
    local pokemon = {
      name = result.name,
      height = result.height,
      weight = result.weight,
      image = result.sprites.front_default
    }
    return pokemon
  end
end

nk.register_rpc(get_pokemon, "get_pokemon")
```

We can make now make an RPC call for a pokemon from a client.

```csharp fct_label="Unity"
byte[] payload = Encoding.UTF8.GetBytes("{\"PokemonName\": \"Dragonite\"}");

var message = new NRuntimeRpcMessage
    .Builder("get_pokemon")
    .Payload(payload)
    .Build();
client.Send(message, (INRuntimeRpc rpc) => {
  var result = Encoding.UTF8.GetString(rpc.Payload);
  Debug.LogFormat("JSON response {0}", result);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
byte[] payload = "{\"PokemonName\": \"Dragonite\"}".getBytes();

CollatedMessage<RpcResult> message = RpcMessage.Builder.newBuilder("get_pokemon")
    .payload(payload)
    .build();
Deferred<RpcResult> deferred = client.send(message);
deferred.addCallback(new Callback<RpcResult, RpcResult>() {
  @Override
  public RpcResult call(RpcResult rpc) throws Exception {
    String result = new String(rpc.getPayload());
    System.out.format("JSON response %s", result);
    return rpc;
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
let payload = "{\"PokemonName\": \"Dragonite\"}"

let message = RPCMessage(id: "client_rpc_echo")
message.payload = payload
client.send(message: message).then { result in
  NSLog("JSON response %@", result.payload)
}.catch { err in
  NSLog("Error @% : @%", err, (err as! NakamaError).message)
}
```
