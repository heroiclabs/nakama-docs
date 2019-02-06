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
  nk.leaderboard_create(id, "desc", "best", "0 0 * * 1", json, false)

  return nk.json_encode({["id"] = id})
  -- will return "{'id': 'some UUID'}" (JSON) as bytes
end

nk.register_rpc(some_example, "my_unique_id")
```

We import the `"nakama"` module which is embedded within the server and contains lots of server-side functions which are helpful as you build your code. You can see all available functions in the [module reference](runtime-code-function-reference.md).

## Register hooks

The code in a module will be evaluated immediately and can be used to register functions which can operate on messages from clients as well as execute logic on demand.

All registered functions receive a "context" table as the first argument. The "context" contains fields which depend on when the code is executed.

| Field | Purpose |
| ----- | ------- |
| `context.env` | A table of key/value pairs which are defined in the YAML [configuration](install-configuration.md) of the server. This is useful to store API keys and other secrets which may be different between servers run in production and in development. |
| `context.execution_mode` | The mode associated with the execution context. It's one of these values: "run_once", "rpc", "before", "after", "match", "matchmaker", "leaderboard_reset", "tournament_reset", "tournament_end". |
| `context.query_params` | Query params that was passed through from HTTP request. |
| `context.session_id` | The user session associated with the execution context. |
| `context.user_id` | The user ID associated with the execution context. |
| `context.username` | The username associated with the execution context. |
| `context.user_session_exp` | The user session expiry in seconds associated with the execution context. |
| `context.client_ip` | The IP address of the client making the request. |
| `context.client_port` | The port number of the client making the request. |
| `context.match_id` | The match ID that is currently being executed. Only applicable to server authoritative multiplayer. |
| `context.match_node` | The node ID that the match is being executed on. Only applicable to server authoritative multiplayer. |
| `context.match_label` | Labels associated with the match. Only applicable to server authoritative multiplayer. |
| `context.match_tick_rate` | Tick rate defined for this match. Only applicable to server authoritative multiplayer. |

There are multiple ways to register a function within the runtime each of which is used to handle specific behavior between client and server.

If you are sending requests to the server via the realtime connection, ensure that use this variant of the function:
```lua
nk.register_rt_before()
nk.register_rt_after()
```

Otherwise use this:
```lua
nk.register_req_after()
nk.register_req_before()
```

If you'd like to run server code when the matchmaker has matched players together, register your function using the following:
```lua
nk.register_matchmaker_matched()
```

If you'd like to run server code when the leaderboard/tournament resets register your function using the following:
```lua
nk.register_leaderboard_reset()
nk.register_tournament_reset()
```

Similary, you can run server code when the tournament ends:
```lua
nk.register_tournament_end()
```

Have a look at [this section](#message-names) to see the server message names.

### Before hook

Any function may be registered to intercept a message received from a client and operate on it (or reject it) based on custom logic. This is useful to enforce specific rules on top of the standard features in the server.

The second argument will be the "incoming payload" containing data received that will be processed by the server.

```lua hl_lines="9"
local nk = require("nakama")

local function limit_friends(context, payload)
  local user = nk.users_get_id({context.user_id})[1]
  -- lets assume we've stored a user's level in their metadata.
  if user.metadata.level <= 10 then
    error("Must reach level 10 before you can add friends.")
  end
  return payload -- important!
end
nk.register_req_before(limit_friends, "AddFriends")
```

The code above fetches the current user's profile and checks the metadata which is assumed to be JSON encoded with `"{level: 12}"` in it. If a user's level is too low an error is thrown to prevent the Friend Add message from being passed onwards in the server pipeline.

!!! Note
    You must remember to return the payload at the end of your function in the same structure as you received it. See `"return payload"` highlighted in the code above.

!!! Tip
    If you choose to `nil` instead of the `payload`, the server will halt further processing of that message. This can be a workaround to stop the server from accepting certain messages or blacklisting certain server features.

### After hook

Similar to [Before hook](#before-hook) you can attach a function to operate on a message. The registered function will be called after the message has been processed in the pipeline. The custom code will be executed asynchronously after the response message has been sent to a client.

The second argument is the "outgoing payload" containing the server's response to the request. The third argument contains the "incoming payload" containing the data originally passed to the server for this request.

```lua
local nk = require("nakama")

local function add_reward(context, outgoing_payload, incoming_payload)
  local value = {
    user_ids = {outgoing_payload.user_id}
  }
  local record = {
    collection = "rewards",
    key = "reward",
    user_id = context.user_id,
    value = value
  }
  nk.storage_write({ record })
end

nk.register_req_after(add_reward, "AddFriends")
```

The simple code above writes a record to a user's storage when they add a friend. Any data returned by the function will be discarded.

### RPC hook

Some logic between client and server is best handled as RPC functions which clients can execute.

```lua
local nk = require("nakama")

local function custom_rpc_func(context, payload)
  nk.logger_info(("Payload: %q"):format(payload))

  -- "payload" is bytes sent by the client we'll JSON decode it.
  local json = nk.json_decode(payload)

  return nk.json_encode(json)
end

nk.register_rpc(custom_rpc_func, "custom_rpc_func_id")
```

The code above registers a function with the identifier "custom_rpc_func_id". This ID can be used within client code to send an RPC message to execute the function and return the result. Results are always returned as a Lua string (or optionally `nil`).

### Server to server

Sometimes it's useful to create HTTP REST handlers which can be used by web services and ease integration into custom server environments. This can be achieved by using the [RPC hook](#rpc-hook), however this uses the [Runtime HTTP Key](install-configuration.md#runtime) to authenticate with the server.

```lua
local nk = require("nakama")

local function http_handler(context, payload)
  local message = nk.json_decode(payload)
  nk.logger_info(("Message: %q"):format(message))
  return nk.json_encode({["context"] = context})
end

nk.register_rpc(http_handler, "http_handler_path")
```

This function can be called with any HTTP client. For example with cURL you could execute the function with the server.

```shell
curl "http://127.0.0.1:7350/v2/rpc/http_handler_path?http_key=defaultkey" \
     -d '"{\"some\": \"data\"}"' \
     -H 'Content-Type: application/json' \
     -H 'Accept: application/json'
```

!!! Warning "HTTP key"
    You should change the default HTTP key before you deploy your code in production.

## Run once

The runtime environment allows you to run code that must only be executed only once. This is useful if you have custom SQL queries that you need to perform (like creating a new table) or to register with third party services.

```lua
nk.run_once(function(context)
  -- this is to create a system ID that cannot be used via a client.
  local system_id = context.env["SYSTEM_ID"]

  nk.sql_exec([[
INSERT INTO users (id, username)
VALUES ($1, $2)
ON CONFLICT (id) DO NOTHING
  ]], { system_id, "system_id" })
end)
```

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

local status, result = pcall(nk.users_get_username, {"22e9ed62"})
if (not status) then
  nk.logger_error(("Error occurred: %q"):format(result))
else
  for _, u in ipairs(result)
  do
    local message = ("id: %q, display name: %q"):format(u.id, u.display_name)
    print(message) -- will appear in console output.
  end
end
```

!!! Warning "Lua stacktraces"
    If the server logger level is set to `info` (default level) or below, the server will return Lua stacktraces to the client. This is useful for debugging but should be disabled for production.

## Restrictions

The Lua virtual machine embedded in the server uses a restricted set of Lua standard library modules. This ensures the code sandbox cannot tamper with operating system input/output or the filesystem.

The list of available modules are: base module, "math", "os", "string", and "table".

## An example module

As a fun example lets use the [Pokéapi](http://pokeapi.co/) and build a helpful module named "pokeapi.lua".

```lua
local nk = require("nakama")

local M = {}

local API_BASE_URL = "http://pokeapi.co/api/v2"

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
    return nk.json_encode(pokemon)
  end
end

nk.register_rpc(get_pokemon, "get_pokemon")
```

We can now make an RPC call for a pokemon from a client.

```sh fct_label="cURL"
curl http://127.0.0.1:7350/v2/rpc/get_pokemon \
  -H 'authorization: Bearer <session token>'
  -d '"{\"PokemonName\": \"dragonite\"}"'
```

```js fct_label="Javascript"
const payload = { "PokemonName": "dragonite"};
const rpcid = "get_pokemon";
const pokemonInfo = await client.rpc(session, rpcid, payload);
console.log("Retrieved pokemon info: %o", pokemonInfo);
```

```csharp fct_label=".Net"
var payload = "{\"PokemonName\": \"dragonite\"}";
var rpcid = "get_pokemon";
var pokemonInfo = await client.RpcAsync(session, rpcid, payload);
System.Console.WriteLine("Retrieved pokemon info: {0}", pokemonInfo);
```

```csharp fct_label="Unity"
var payload = "{\"PokemonName\": \"dragonite\"}";
var rpcid = "get_pokemon";
var pokemonInfo = await client.RpcAsync(session, rpcid, payload);
Debug.LogFormat("Retrieved pokemon info: {0}", pokemonInfo);
```

```java fct_label="Android/Java"
String payload = "{\"PokemonName\": \"dragonite\"}";
String rpcid = "get_pokemon";
Rpc pokemonInfo = client.rpc(session, rpcid, payload);
System.out.format("Retrieved pokemon info: %s", pokemonInfo.getPayload());
```

```swift fct_label="Swift"
// Requires Nakama 1.x
let payload = "{\"PokemonName\": \"dragonite\"}".data(using: .utf8)!

let message = RPCMessage(id: "client_rpc_echo")
message.payload = payload
client.send(message: message).then { result in
  NSLog("JSON response %@", result.payload)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```fct_label="REST"
POST /v2/rpc/get_pokemon
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{
  "PokemonName": "dragonite"
}
```

## Message names

You should use the following request names for `register_req_before` and `register_req_after` hooks:

| Request Name | Description
| ------------ | -----------
| AddFriends | Add friends by ID or username to a user's account.
| AddGroupUsers | Add users to a group.
| AuthenticateCustom | Authenticate a user with a custom id against the server.
| AuthenticateDevice | Authenticate a user with a device id against the server.
| AuthenticateEmail | Authenticate a user with an email+password against the server.
| AuthenticateFacebook | Authenticate a user with a Facebook OAuth token against the server.
| AuthenticateGameCenter | Authenticate a user with Apple's GameCenter against the server.
| AuthenticateGoogle | Authenticate a user with Google against the server.
| AuthenticateSteam | Authenticate a user with Steam against the server.
| BlockFriends | Block one or more users by ID or username.
| CreateGroup | Create a new group with the current user as the owner.
| DeleteFriends | Delete one or more users by ID or username.
| DeleteGroup | Delete one or more groups by ID.
| DeleteLeaderboardRecord | Delete a leaderboard record.
| DeleteNotifications | Delete one or more notifications for the current user.
| DeleteStorageObjects | Delete one or more objects by ID or username.
| GetAccount | Fetch the current user's account.
| GetUsers | Fetch zero or more users by ID and/or username.
| Healthcheck | A healthcheck which load balancers can use to check the service.
| ImportFacebookFriends | Import Facebook friends and add them to a user's account.
| JoinGroup | Immediately join an open group, or request to join a closed one.
| KickGroupUsers | Kick a set of users from a group.
| LeaveGroup | Leave a group the user is a member of.
| LinkCustom | Add a custom ID to the social profiles on the current user's account.
| LinkDevice | Add a device ID to the social profiles on the current user's account.
| LinkEmail | Add an email+password to the social profiles on the current user's account.
| LinkFacebook | Add Facebook to the social profiles on the current user's account.
| LinkGameCenter | Add Apple's GameCenter to the social profiles on the current user's account.
| LinkGoogle | Add Google to the social profiles on the current user's account.
| LinkSteam | Add Steam to the social profiles on the current user's account.
| ListChannelMessages | List a channel's message history.
| ListFriends | List all friends for the current user.
| ListGroups | List groups based on given filters.
| ListGroupUsers | List all users that are part of a group.
| ListLeaderboardRecords | List leaderboard records
| ListMatches | Fetch list of running matches.
| ListNotifications | Fetch list of notifications.
| ListStorageObjects | List publicly readable storage objects in a given collection.
| ListUserGroups | List groups the current user belongs to.
| PromoteGroupUsers | Promote a set of users in a group to the next role up.
| ReadStorageObjects | Get storage objects.
| UnlinkCustom | Remove the custom ID from the social profiles on the current user's account.
| UnlinkDevice | Remove the device ID from the social profiles on the current user's account.
| UnlinkEmail | Remove the email+password from the social profiles on the current user's account.
| UnlinkFacebook | Remove Facebook from the social profiles on the current user's account.
| UnlinkGameCenter | Remove Apple's GameCenter from the social profiles on the current user's account.
| UnlinkGoogle | Remove Google from the social profiles on the current user's account.
| UnlinkSteam | Remove Steam from the social profiles on the current user's account.
| UpdateAccount | Update fields in the current user's account.
| UpdateGroup | Update fields in a given group.
| WriteLeaderboardRecord | Write a record to a leaderboard.
| WriteStorageObjects | Write objects into the storage engine.

You should use the following message names for `register_rt_before` and `register_rt_after` hooks:

| Message Name | Description
| ------------ | -----------
| ChannelJoin | Join a realtime chat channel.
| ChannelLeave | Leave a realtime chat channel.
| ChannelMessageSend | Send a message to a realtime chat channel.
| ChannelMessageUpdate | Update a message previously sent to a realtime chat channel.
| ChannelMessageRemove | Remove a message previously sent to a realtime chat channel.
| MatchCreate | A client to server request to create a realtime match.
| MatchDataSend | A client to server request to send data to a realtime match.
| MatchJoin | A client to server request to join a realtime match.
| MatchLeave | A client to server request to leave a realtime match.
| MatchmakerAdd | Submit a new matchmaking process request.
| MatchmakerRemove | Cancel a matchmaking process using a ticket.
| StatusFollow | Start following some set of users to receive their status updates.
| StatusUnfollow | Stop following some set of users to no longer receive their status updates.
| StatusUpdate | Set the user's own status.

Names are case-insensitive. For more information, have a look at [`api.proto`](https://github.com/heroiclabs/nakama/blob/master/api/api.proto) and [`realtime.proto`](https://github.com/heroiclabs/nakama/blob/master/rtapi/realtime.proto).
