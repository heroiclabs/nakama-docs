# Basics

The server exposes a fast embedded code runtime where you can integrate custom logic written as either <a href="https://golang.org/pkg/plugin/" target="\_blank">Go plugins</a> or <a href="https://www.lua.org/manual/5.1/manual.html" target="\_blank">Lua modules</a>.

!!! Note
    Go plugins must be compiled before they can be loaded by Nakama. The build process is different for binary and Docker-based Nakama installations, make sure you follow [the instructions](https://github.com/heroiclabs/nakama/tree/master/sample_go_module).

It is useful to run custom logic which isn’t running on the device or browser. The code you deploy with the server can be used immediately by clients so you can change behavior on the fly and add new features faster.

You should use server-side code when you want to set rules around various features like how many [friends](social-friends.md) a user may have or how many [groups](social-groups-clans.md) can be joined. It can be used to run authoritative logic or perform validation checks as well as integrate with other services over HTTPS.

## Load modules

By default the server will scan all files within the "data/modules" folder relative to the server file or the folder specified in the YAML [configuration](install-configuration.md#runtime) at startup. You can also specify the modules folder via a command flag when you start the server.

```shell
nakama --runtime.path "$HOME/some/path/"
```

All files with the ".lua" or ".so" extensions found in the runtime path will be loaded and evaluated as part of the boot up sequence. Each Lua file represents a module and all code in each module will be run and can be used to register functions which can operate on messages from clients as well as execute logic on demand. Shared Object files are the equivalent for Go plugins.

## Simple example

The following example will show you how to create and register code to be run by a client as an [RPC call](#register_rpc).

In the Lua example we will create a module called "example.lua". We will import the `"nakama"` module which is embedded within the server and contains lots of server-side functions which are helpful as you build your code. You can see all available functions in the [Lua module reference](runtime-code-function-reference.md).

In the Go example, we will import the runtime package and use the `NakamaModule` which has all the same functions as referenced above.

```lua fct_label="Lua"
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

```go fct_label="Go"
import (
  "context"
  "database/sql"
  "encoding/json"
  "github.com/heroiclabs/nakama/runtime"
)

// All Go modules must have a InitModule function with this exact signature.
func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
  // Register the RPC function.
  if err := initializer.RegisterRpc("my_unique_id", SomeExample); err != nil {
    logger.Error("Unable to register: %v", err)
    return err
  }
  return nil
}

func SomeExample(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	meta := make(map[string]interface{})
	// Note below, json.Unmarshal can only take a pointer as second argument
	if err := json.Unmarshal([]byte(payload), &meta); err != nil {
		// Handle error
		return "", err
	}

	id := "SomeId"
	authoritative := false
	sort := "desc"
	operator := "best"
	reset := "0 0 * * 1"

	if err := nk.LeaderboardCreate(ctx, id, authoritative, sort, operator, reset, meta); err != nil {
		// Handle error
		return "", err
	}

	return "Success", nil
}
```

## Register hooks

The code in a module will be evaluated immediately and can be used to register functions which can operate on messages from clients as well as execute logic on demand.

All registered functions receive a "context" as the first argument. This contains fields which depend on when and how the code is executed. You can extract information about the request or the user making it from the context.

```lua fct_label="Lua"
local user_id = context.user_id
```

```go fct_label="Go"
userId, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
if !ok {
  // User ID not found in the context.
}
```

If you are writing your runtime code in Lua, the "context" will be a table from which you can access the fields directly. The Go runtime context is a standard `context.Context` type and its fields can be accessed as shown above.

| Go context key | Go type | Lua context key | Purpose |
| -------------- | ------- | --------------- | ------- |
| `RUNTIME_CTX_ENV` | `map[string]string` | `env` | A table of key/value pairs which are defined in the YAML [configuration](install-configuration.md) of the server. This is useful to store API keys and other secrets which may be different between servers run in production and in development. |
| `RUNTIME_CTX_MODE` | `string` | `execution_mode` | The mode associated with the execution context. It's one of these values: "run_once", "rpc", "before", "after", "match", "matchmaker", "leaderboard_reset", "tournament_reset", "tournament_end". |
| `RUNTIME_CTX_QUERY_PARAMS` | `map[string]string` | `query_params` | Query params that was passed through from HTTP request. |
| `RUNTIME_CTX_SESSION_ID` | `string` | `session_id` | The user session associated with the execution context. |
| `RUNTIME_CTX_USER_ID` | `string` | `user_id` | The user ID associated with the execution context. |
| `RUNTIME_CTX_USERNAME` | `string` | `username` | The username associated with the execution context. |
| `RUNTIME_CTX_USER_SESSION_EXP` | `int64` | `user_session_exp` | The user session expiry in seconds associated with the execution context. |
| `RUNTIME_CTX_CLIENT_IP` | `string` | `client_ip` | The IP address of the client making the request. |
| `RUNTIME_CTX_CLIENT_PORT` | `string` | `client_port` | The port number of the client making the request. |
| `RUNTIME_CTX_MATCH_ID` | `string` | `match_id` | The match ID that is currently being executed. Only applicable to server authoritative multiplayer. |
| `RUNTIME_CTX_MATCH_NODE` | `string` | `match_node` | The node ID that the match is being executed on. Only applicable to server authoritative multiplayer. |
| `RUNTIME_CTX_MATCH_LABEL` | `string` | `match_label` | Labels associated with the match. Only applicable to server authoritative multiplayer. |
| `RUNTIME_CTX_MATCH_TICK_RATE` | `int` | `match_tick_rate` | Tick rate defined for this match. Only applicable to server authoritative multiplayer. |

There are multiple ways to register a function within the runtime each of which is used to handle specific behavior between client and server.

```lua fct_label="Lua"
-- If you are sending requests to the server via the realtime connection, ensure that use this variant of the function.
nk.register_rt_before()
nk.register_rt_after()

-- Otherwise use this.
nk.register_req_after()
nk.register_req_before()

-- If you'd like to run server code when the matchmaker has matched players together, register your function using the following.
nk.register_matchmaker_matched()

-- If you'd like to run server code when the leaderboard/tournament resets register your function using the following.
nk.register_leaderboard_reset()
nk.register_tournament_reset()

-- Similary, you can run server code when the tournament ends.
nk.register_tournament_end()
```

```go fct_label="Go"
// NOTE: All Go runtime registrations must be made in the module's InitModule function.

// If you are sending requests to the server via the realtime connection, ensure that use this variant of the function.
initializer.RegisterBeforeRt()
initializer.RegisterAfterRt()

// Otherwise use the relevant before / after hook, e.g.
initializer.RegisterBeforeAddFriends()
initializer.RegisterAfterAddFriends()

// If you'd like to run server code when the matchmaker has matched players together, register your function using the following.
initializer.RegisterMatchmakerMatched()

// If you'd like to run server code when the leaderboard/tournament resets register your function using the following.
initializer.RegisterLeaderboardReset()
initializer.RegisterTournamentReset()

// Similary, you can run server code when the tournament ends.
initializer.RegisterTournamentEnd()
```

Have a look at [this section](#message-names) for a complete list of the server message names.

!!! Tip
    Only one hook may be registered for each type. If you register more than one then only the last registration is used. RPC functions are unique per registered ID, and you can register the same function under multiple IDs.

### Before hook

Any function may be registered to intercept a message received from a client and operate on it (or reject it) based on custom logic. This is useful to enforce specific rules on top of the standard features in the server.

In Go each hook will receive as input a variable containing the data that will be processed by ther server for that request, if that feature is expected to receive any input. In Lua the second argument will be the "incoming payload" containing data received that will be processed by the server.

```lua hl_lines="9" fct_label="Lua"
local nk = require("nakama")

local function limit_friends(context, payload)
  local user = nk.users_get_id({context.user_id})[1]
  -- Let's assume we've stored a user's level in their metadata.
  if user.metadata.level <= 10 then
    error("Must reach level 10 before you can add friends.")
  end
  return payload -- important!
end
nk.register_req_before(limit_friends, "AddFriends")
```

```go fct_label="Go"
func BeforeAddFriends(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.AddFriendsRequest) (*api.AddFriendsRequest, error) {
	userId, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok {
		return nil, errors.New("Missing user ID.")
	}

	account, err := nk.UsersGetId(ctx, []string{userId})
	if err != nil {
		return nil, err
	}

	var metadata map[string]interface{}
	if err := json.Unmarshal([]byte(account.GetUser().GetMetadata()), &metadata); err != nil {
		return nil, errors.New("Corrupted user metadata.")
	}

	// Let's assume we've stored a user's level in their metadata.
	if level, ok := metadata["level"].(int); !ok || level <= 10 {
		return nil, errors.New("Must reach level 10 before you can add friends.")
	}

	return in, nil
}

// Register as a before hook for the appropriate feature, this call should be in InitModule.
if err := initializer.RegisterBeforeAddFriends(BeforeAddFriends); err != nil {
  logger.Error("Unable to register: %v", err)
  return err
}
```

The code above fetches the current user's profile and checks the metadata which is assumed to be JSON encoded with `"{level: 12}"` in it. If a user's level is too low an error is thrown to prevent the Friend Add message from being passed onwards in the server pipeline.

!!! Note
    You must remember to return the payload at the end of your function in the same structure as you received it. See the lines highlighted in the code above.

!!! Tip
    If you choose to return `nil` instead of the `payload` (or a non-nil `error` in Go) the server will halt further processing of that message. This can be a used to stop the server from accepting certain messages or disabling/blacklisting certain server features.

### After hook

Similar to [Before hook](#before-hook) you can attach a function to operate on a message. The registered function will be called after the message has been processed in the pipeline. The custom code will be executed asynchronously after the response message has been sent to a client.

The second argument is the "outgoing payload" containing the server's response to the request. The third argument contains the "incoming payload" containing the data originally passed to the server for this request.

```lua fct_label="Lua"
local nk = require("nakama")

local function add_reward(context, outgoing_payload, incoming_payload)
  local value = {
    user_ids = {incoming_payload.user_id}
  }
  local object = {
    collection = "rewards",
    key = "reward",
    user_id = context.user_id,
    value = value
  }
  nk.storage_write({ object })
end

nk.register_req_after(add_reward, "AddFriends")
```

```go fct_label="Go"
func AfterAddFriends(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.AddFriendsRequest) error {
	userId, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok {
		return errors.New("Missing user ID.")
	}

	value, err := json.Marshal(map[string]interface{}{"user_ids": in.GetIds()})
	if err != nil {
		return err
	}

	if _, err := nk.StorageWrite(ctx, []*runtime.StorageWrite{
		&runtime.StorageWrite{
			Collection: "rewards",
			Key:        "reward",
			UserID:     userId,
			Value:      string(value),
		},
	}); err != nil {
		return err
	}

	return nil
}

// Register as an after hook for the appropriate feature, this call should be in InitModule.
if err := initializer.RegisterAfterAddFriends(AfterAddFriends); err != nil {
  logger.Error("Unable to register: %v", err)
  return err
}
```

The simple code above writes a record to a user's storage when they add a friend. Any data returned by the function will be discarded.

!!! Tip
    After hooks cannot change the response payload being sent back to the client, and errors do not prevent the response from being sent.

### RPC hook

Some logic between client and server is best handled as RPC functions which clients can execute.

```lua fct_label="Lua"
local nk = require("nakama")

local function custom_rpc_func(context, payload)
  nk.logger_info(("Payload: %q"):format(payload))

  -- "payload" is bytes sent by the client we'll JSON decode it.
  local json = nk.json_decode(payload)

  return nk.json_encode(json)
end

nk.register_rpc(custom_rpc_func, "custom_rpc_func_id")
```

```go fct_label="Go"
func CustomRpcFunc(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
  logger.Info("Payload: %s", payload)

  // "payload" is bytes sent by the client we'll JSON decode it.
  var value interface{}
  if err := json.Unmarshal([]byte(payload), &value); err != nil {
    return "", err
  }

  response, err := json.Marshal(value)
  if err != nil {
    return "", err
  }

	return string(response), nil
}

// Register as an RPC function, this call should be in InitModule.
if err := initializer.RegisterRpc("custom_rpc_func_id", CustomRpcFunc); err != nil {
  logger.Error("Unable to register: %v", err)
  return err
}
```

The code above registers a function with the identifier "custom_rpc_func_id". This ID can be used within client code to send an RPC message to execute the function on the server and return the result.

From Go runtime code, the result is returned as `(string, error)`. From Lua runtime code, results are always returned as a Lua string (or optionally `nil`).

### Server to server

Sometimes it's useful to create HTTP REST handlers which can be used by web services and ease integration into custom server environments. This can be achieved by using the [RPC hook](#rpc-hook), however this uses the [Runtime HTTP Key](install-configuration.md#runtime) to authenticate with the server.

```lua fct_label="Lua"
local nk = require("nakama")

local function http_handler(context, payload)
  local message = nk.json_decode(payload)
  nk.logger_info(("Message: %q"):format(message))
  return nk.json_encode({["context"] = context})
end

nk.register_rpc(http_handler, "http_handler_path")
```

```go fct_label="Go"
func HttpHandler(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
  var message interface{}
  if err := json.Unmarshal([]byte(payload), &message); err != nil {
    return "", err
  }

  logger.Info("Message: %v", message)

  response, err := json.Marshal(map[string]interface{}{"context": ctx})
  if err != nil {
    return "", err
  }

	return string(response), nil
}

// Register as an RPC function, this call should be in InitModule.
if err := initializer.RegisterRpc("http_handler_path", HttpHandler); err != nil {
  logger.Error("Unable to register: %v", err)
  return err
}
```

This function can be called with any HTTP client. For example with cURL you could execute the function with the server.

!!! Tip
    RPC functions can be called both from clients and through server to server calls. You can tell them apart by [checking if the context has a user ID](#register-hooks) - server to server calls will never have a user ID. If you want to scope functions to never be accessible from the client just return an error if you find a user ID in the context.

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

```lua fct_label="Lua"
nk.run_once(function(context)
  -- This is to create a system ID that cannot be used via a client.
  local system_id = context.env["SYSTEM_ID"]

  nk.sql_exec([[
INSERT INTO users (id, username)
VALUES ($1, $2)
ON CONFLICT (id) DO NOTHING
  ]], { system_id, "system_id" })
end)
```

```go fct_label="Go"
func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
  // This is to create a system ID that cannot be used via a client.
  var systemId string
  if env, ok := ctx.Value(runtime.RUNTIME_CTX_ENV).(map[string]string); ok {
    systemId = env["SYSTEM_ID"]
  }

  _, err := db.ExecContext(ctx, `
INSERT INFO users (id, username)
VALUES ($1, $2)
ON CONFLICT (id) DO NOTHING
  `, systemId, "sysmtem_id")
  if err != nil {
    logger.Error("Error: %s", err.Error())
  }

  return nil
}
```

## Errors and logs

Error handling in Go follows the standard pattern of returning an `error` value as the last argument of a function call. If the `error` is `nil` then the call was successful.

Lua error handling uses raised errors rather than error return values. If you want to trap the error which occurs in the execution of a function you'll need to execute it via `pcall` as a "protected call".

```lua fct_label="Lua"
local function will_error()
  error("This function will always throw an error!")
end

if pcall(will_error) then
  -- No errors with "will_error".
else
  -- Handle errors.
end
```

```go fct_label="Go"
func willError() error {
  return errors.New("This function will always throw an error!")
}

if err := willError(); err != nil {
  // Handle errors.
} else {
  // No errors with "willError".
}
```

The function `will_error` uses the `error` function in Lua to throw an error with a reason message. The `pcall` will invoke the `will_error` function and trap any errors. We can then handle the success or error cases as needed.

We recommend you use this pattern with your Lua code.

```lua fct_label="Lua"
local nk = require("nakama")

local status, result = pcall(nk.users_get_username, {"22e9ed62"})
if (not status) then
  nk.logger_error(("Error occurred: %q"):format(result))
else
  for _, u in ipairs(result)
  do
    local message = ("id: %q, display name: %q"):format(u.id, u.display_name)
    nk.logger_info(message) -- Will appear in logging output.
  end
end
```

```go fct_label="Go"
users, err := nk.UsersGetUsername([]string{"22e9ed62"})
if err != nil {
  logger.Error("Error occurred: %v", err.Error())
} else {
  for _, u := range users {
    logger.Info("id: %v, display name: %v", u.Id, u.DisplayName) // Will appear in logging output.
  }
}
```

!!! Warning "Lua stacktraces"
    If the server logger level is set to `info` (default level) or below, the server will return Lua stacktraces to the client. This is useful for debugging but should be disabled for production.

## Restrictions

### Compatibility

The Lua runtime is a Lua 5.1-compatible implementation with a small set of additional packages backported from newer versions - see [available functions](#available-functions). For best results ensure your Lua modules and any 3rd party libraries are compatible with Lua 5.1.

Go runtime available functionality depends on the version of Go each Nakama release is compiled with. This is usually the latest stable version at the time of release. Check server startup logging for the exact Go version used by your Nakama installation.

!!! Note
    Lua runtime code cannot use the Lua C API or extensions. Make sure your code and any 3rd party libraries are pure Lua 5.1.

### Available functions

The Lua virtual machine embedded in the server uses a restricted set of Lua standard library modules. This ensures the code sandbox cannot tamper with operating system input/output or the filesystem.

The list of available Lua modules are: base module, `math`, `string`, `table`, `bit32`, and a subset of `os` (only `clock`, `difftime`, `date`, and `time` functions).

Go runtime code can make use of the full range of standard library functions and packages.

!!! Tip
    You cannot call Lua functions from the Go runtime, or Go functions from the Lua runtime.

### Global state

Lua runtime code is executed in instanced contexts. You cannot use global variables as a way to store state in memory, or communicate with other Lua processes or function calls.

The Go runtime does not have this restriction and can store and share data as needed, but concurrency and access controls are the responsibility of the developer.

### Sandboxing

Lua runtime code is fully sandboxed and cannot access the filesystem, input/output devices, or spawn OS threads or processes. This allows the server to guarantee that Lua modules cannot cause fatal errors - Lua code cannot trigger unexpected client disconnects or affect the main server process.

Go runtime code has full low level access to the server and its environment. This allows full flexibility and control to include powerful features and offer high performance, but cannot guarantee error safety - the server does not guard against fatal errors in Go runtime code, such as segmentation faults or pointer dereference failures.

## An example module

As a fun example lets use the [Pokéapi](http://pokeapi.co/) and build a helpful module named "pokeapi.lua".

```lua fct_label="Lua"
local nk = require("nakama")

local M = {}

local API_BASE_URL = "https://pokeapi.co/api/v2"

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
-- We can import the code up to this point into another module we'll call "pokemon.lua" which will register an RPC call.

local nk = require("nakama")
local pokeapi = require("pokeapi")

local function get_pokemon(_, payload)
  -- We'll assume payload was sent as JSON and decode it.
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

```go fct_label="Go"
import (
  "context"
  "database/sql"
  "encoding/json"
  "errors"
  "io/ioutil"
  "net/http"

  "github.com/heroiclabs/nakama/runtime"
)

const apiBaseUrl = "https://pokeapi.co/api/v2"

// All Go modules must have a InitModule function with this exact signature.
func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
  // Register the RPC function.
  if err := initializer.RegisterRpc("get_pokemon", GetPokemon); err != nil {
    logger.Error("Unable to register: %v", err)
    return err
  }
  return nil
}

func LookupPokemon(logger runtime.Logger, name string) (map[string]interface{}, error) {
  resp, err := http.Get(apiBaseUrl + "/pokemon/" + name)
  if err != nil {
    logger.Error("Failed request %v", err.Error())
    return nil, err
  }
  defer resp.Body.Close()
  body, err := ioutil.ReadAll(resp.Body)
  if err != nil {
    logger.Error("Failed to read body %v", err.Error())
    return nil, error
  }
  if resp.StatusCode >= 400 {
    logger.Error("Failed request %v %v", resp.StatusCode, body)
    return nil, errors.New(string(body))
  }

  var result map[string]interface{}
  err = json.Unmarshal(body, &result)

  return result, err
}

func GetPokemon(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
  // We'll assume payload was sent as JSON and decode it.
  var input map[string]string
  err := json.Unmarshal([]byte(payload), &input)
  if err != nil {
    return "", err
  }

  result, err := LookupPokemon(logger, input["PokemonName"])
  if err != nil {
    return "", err
  }

  response, err := json.Marshal(result)
  if err != nil {
    return "", err
  }
  return string(response), nil
}
```

!!! Tip
    To use the Go runtime don't forget to compile your code following [these instructions](https://github.com/heroiclabs/nakama/tree/master/sample_go_module) carefully.

We can now make an RPC call for a pokemon from a client.

```sh fct_label="cURL"
curl "http://127.0.0.1:7350/v2/rpc/get_pokemon" \
  -H 'authorization: Bearer <session token>'
  -d '"{\"PokemonName\": \"dragonite\"}"'
```

```js fct_label="Javascript"
const payload = { "PokemonName": "dragonite"};
const rpcid = "get_pokemon";
const pokemonInfo = await client.rpc(session, rpcid, payload);
console.log("Retrieved pokemon info: %o", pokemonInfo);
```

```csharp fct_label=".NET"
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

```cpp fct_label="Cocos2d-x C++"
auto successCallback = [](const NRpc& rpc)
{
  CCLOG("Retrieved pokemon info: %s", rpc.payload.c_str());
};

string payload = "{ \"PokemonName\": \"dragonite\" }";
string rpcid = "get_pokemon";
client->rpc(session, rpcid, payload, successCallback);
```

```js fct_label="Cocos2d-x JS"
const payload = { "PokemonName": "dragonite"};
const rpcid = "get_pokemon";
client.rpc(session, rpcid, payload)
  .then(function(pokemonInfo) {
      cc.log("Retrieved pokemon info:", JSON.stringify(pokemonInfo));
    },
    function(error) {
      cc.error("rpc call failed:", JSON.stringify(error));
    });
```

```cpp fct_label="C++"
auto successCallback = [](const NRpc& rpc)
{
  std::cout << "Retrieved pokemon info: " << rpc.payload << std::endl;
};

string payload = "{ \"PokemonName\": \"dragonite\" }";
string rpcid = "get_pokemon";
client->rpc(session, rpcid, payload, successCallback);
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

If your runtime code is in Go, refer to [the interface definition](https://github.com/heroiclabs/nakama/blob/master/runtime/runtime.go) for a full list of hooks that are available in the runtime package.

In Lua, you should use the following request names for `register_req_before` and `register_req_after` hooks:

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

Names are case-insensitive. For more information, have a look at ["api.proto"](https://github.com/heroiclabs/nakama/blob/master/api/api.proto) and ["realtime.proto"](https://github.com/heroiclabs/nakama/blob/master/rtapi/realtime.proto).
