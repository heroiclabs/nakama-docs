# Basics

The server exposes a fast embedded code runtime where you can integrate custom logic written as either <a href="https://golang.org/pkg/plugin/" target="_blank">Go plugins</a>, <a href="https://www.lua.org/manual/5.1/manual.html" target="_blank">Lua modules</a> or a <a href="https://developer.mozilla.org/en-US/docs/Web/javascript" target="_blank">JavaScript</a> bundle.

!!! Note
    Go plugins must be compiled before they can be loaded by Nakama. The build process is different for binary and Docker-based Nakama installations, make sure you follow [the instructions](https://github.com/heroiclabs/nakama/tree/master/sample_go_module).

It is useful to run custom logic which isn’t running on the device or browser. The code you deploy with the server can be used immediately by clients so you can change behavior on the fly and add new features faster.

You should use server-side code when you want to set rules around various features like how many [friends](social-friends.md) a user may have or how many [groups](social-groups-clans.md) can be joined. It can be used to run authoritative logic or perform validation checks as well as integrate with other services over HTTPS.

## Load modules

By default, the server will scan all files within the "data/modules" folder relative to the server file or the folder specified in the YAML [configuration](install-configuration.md#runtime) at startup. You can also specify the modules folder via a command flag when you start the server.

```shell
nakama --runtime.path "$HOME/some/path/"
```

All files with the ".lua", ".so" and ".js" extensions found in the runtime path will be loaded and evaluated as part of the boot up sequence. Each of the runtimes has access to the <a href="../runtime-code-function-reference/#function-reference">Nakama API</a> to operate on messages from clients as well as execute logic on demand.

The runtimes are loaded with a precedence order: `Go > Lua > JavaScript` meaning that if match handlers or RPC functions/hooks exist in multiple runtimes they will all be loaded, but if there is an identifier collision in any of the runtimes, only the one with the most precedence will take effect. This approach gives the developer the flexibility to leverage the different runtimes as best suited and have them work seamlessly together. As an example, one could define an RPC function in the JavaScript runtime to create a match with a set of Go runtime defined match handlers.

### Go
The Go runtime looks for a Go plugin `.so` shared object file, either in the default location, or in the set runtime path. To learn how you can generate the `.so` file with your custom Go runtime code you can follow <a href="https://github.com/heroiclabs/nakama/tree/master/sample_go_module#build-process">this guide</a>.

### Lua
The Lua runtime will interpret and load any `.lua` files present either in the default location, or in the set runtime path. Each Lua file represents a module and all code in each module will be run and can be used to register custom functions.

### JavaScript
The JavaScript runtime expects a bundled `index.js` file present either in the default location, or in the set runtime path. If you wish to override this behaviour you can by setting the entrypoint flag.

```shell
nakama --runtime.js_entrypoint "some/path/foo.js"
```

!!! Note
    This path must be relative to the default or set runtime path.

We provide <a href="../runtime-code-typescript-setup/#typescript-setup">a guide on how to get started</a> with the JavaScript runtime by writing your code in TypeScript and using its compiler to generate a `.js` bundle that can be interpreted by Nakama.

## Examples

In this section we'll provide a few code examples that can be interpreted by the supported runtimes in their respective languages.

### RPC Example

The following example will show you how to create and register code to be run by a client as an [RPC call](#register_rpc).

In the Lua example, we will create a module called "example.lua". We will import the `"nakama"` module which is embedded within the server and contains lots of server-side functions which are helpful as you build your code. You can see all available functions in the [Lua module reference](runtime-code-function-reference.md).

In the Go example, we will import the runtime package and use the `NakamaModule` which has all the same functions as referenced above.

And finally the JavaScript example will follow a similar pattern to the Go runtime; all custom code must be within the scope of a globally defined `InitModule` function, which exposes a `logger`, `ctx`, `nk` and `initializer` - the logger, context, nakama module that exposes the Nakama APIs and the initializer used to register the rpc function, respectively.

=== "Lua"
	```lua
	local nk = require("nakama")

	local function some_example(context, payload)
		-- we'll assume payload was sent as JSON and decode it.
		local json = nk.json_decode(payload)

		-- log data sent to RPC call.
		nk.logger_info(string.format("Payload: %q", json))

		local id = nk.uuid_v4()
		-- create a leaderboard with the json as metadata.
		nk.leaderboard_create(id, "desc", "best", "0 0 * * 1", json, false)

		return nk.json_encode({["id"] = id})
		-- will return "{'id': 'some UUID'}" (JSON) as bytes
	end

	nk.register_rpc(some_example, "my_unique_id")
	```

=== "Go"
	```go
	import (
		"context"
		"database/sql"
		"encoding/json"
		"github.com/heroiclabs/nakama-common/runtime"
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

=== "JavaScript"
	```javascript
	function InitModule(ctx, logger, nk, initializer) {
		function createLeaderboardFn(ctx2, logger2, nk2, payload) {
			var json = JSON.parse(payload);

			logger2.debug('user_id: %s, payload: %q', ctx2.userId, json);

			var id = 'level1';
			var authoritative = false;
			var sort = SortOrder.ASCENDING;
			var operator = Operator.BEST;
			var reset = '0 0 * * 1';

			try {
				nk2.leaderboardCreate(id, authoritative, sort, operator, reset, json);
			} catch(error) {
				logger2.error('Failed to create leaderboard: %s', error.message);
				return JSON.stringify(error);
			}

			logger2.info('Leaderboard with id: %s created', id);

			return JSON.stringify({id: id});
		});

		initializer.registerRpc('my_unique_id', createLeaderboardFn);
	}
	```

## Register hooks

For all runtimes, the code will be evaluated at server startup and can be used to register functions which can operate on messages from clients as well as execute custom logic on demand. These functions are called hooks.

### Context

All registered functions across all runtimes receive a "context" as the first argument. This contains fields which depend on when and how the code is executed. You can extract information about the request or the user making it from the context.

=== "Lua"
	```lua
	local user_id = context.user_id
	```

=== "Go"
	```go
	userId, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok {
	  // User ID not found in the context.
	}
	```

=== "JavaScript"
	```javascript
	var userId = ctx.userId;
	```

If you are writing your runtime code in Lua, the "context" will be a table from which you can access the fields directly. The Go runtime context is a standard `context.Context` type and its fields can be accessed as shown above. In JavaScript, context is a plain object with properties.

| Go context key                 | Go type             | Lua context key    | JavaScript context property | Purpose                                                                                                                                                                                                                                           |
| ------------------------------ | ------------------- | ------------------ | ----------------------------| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `RUNTIME_CTX_ENV`              | `map[string]string` | `env`              | `env`                       | A table of key/value pairs which are defined in the YAML [configuration](install-configuration.md) of the server. This is useful to store API keys and other secrets which may be different between servers run in production and in development. |
| `RUNTIME_CTX_MODE`             | `string`            | `execution_mode`   | `executionMode`             | The mode associated with the execution context. It's one of these values: "run_once", "rpc", "before", "after", "match", "matchmaker", "leaderboard_reset", "tournament_reset", "tournament_end".                                                 |
| `RUNTIME_CTX_QUERY_PARAMS`     | `map[string]string` | `query_params`     | `queryParams`               | Query params that was passed through from HTTP request.                                                                                                                                                                                           |
| `RUNTIME_CTX_SESSION_ID`       | `string`            | `session_id`       | `sessionJd`                 | The user session associated with the execution context.                                                                                                                                                                                           |
| `RUNTIME_CTX_USER_ID`          | `string`            | `user_id`          | `userId`                    | The user ID associated with the execution context.                                                                                                                                                                                                |
| `RUNTIME_CTX_USERNAME`         | `string`            | `username`         | `username`                  | The username associated with the execution context.                                                                                                                                                                                               |
| `RUNTIME_CTX_USER_SESSION_EXP` | `int64`             | `user_session_exp` | `userSessionExp`            | The user session expiry in seconds associated with the execution context.                                                                                                                                                                         |
| `RUNTIME_CTX_CLIENT_IP`        | `string`            | `client_ip`        | `clientIp`                  | The IP address of the client making the request.                                                                                                                                                                                                  |
| `RUNTIME_CTX_CLIENT_PORT`      | `string`            | `client_port`      | `clientPort`                | The port number of the client making the request.                                                                                                                                                                                                 |
| `RUNTIME_CTX_MATCH_ID`         | `string`            | `match_id`         | `matchId`                   | The match ID that is currently being executed. Only applicable to server authoritative multiplayer.                                                                                                                                               |
| `RUNTIME_CTX_MATCH_NODE`       | `string`            | `match_node`       | `matchNode`                 | The node ID that the match is being executed on. Only applicable to server authoritative multiplayer.                                                                                                                                             |
| `RUNTIME_CTX_MATCH_LABEL`      | `string`            | `match_label`      | `matchLabel`                | Labels associated with the match. Only applicable to server authoritative multiplayer.                                                                                                                                                            |
| `RUNTIME_CTX_MATCH_TICK_RATE`  | `int`               | `match_tick_rate`  | `matchTickRate`             | Tick rate defined for this match. Only applicable to server authoritative multiplayer.                                                                                                                                                            |

There are multiple ways to register a function within the runtime each of which is used to handle specific behavior between client and server.

=== "Lua"
	```lua
	-- If you are sending requests to the server via the realtime connection, ensure that you use this variant of the function.
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

	-- Similarly, you can run server code when the tournament ends.
	nk.register_tournament_end()
	```

=== "Go"
	```go
	// NOTE: All Go runtime registrations must be made in the module's InitModule function.

	// If you are sending requests to the server via the realtime connection, ensure that you use this variant of the function.
	initializer.RegisterBeforeRt()
	initializer.RegisterAfterRt()

	// Otherwise use the relevant before / after hook, e.g.
	initializer.RegisterBeforeAddFriends()
	initializer.RegisterAfterAddFriends()
	// (...)

	// If you'd like to run server code when the matchmaker has matched players together, register your function using the following.
	initializer.RegisterMatchmakerMatched()

	// If you'd like to run server code when the leaderboard/tournament resets register your function using the following.
	initializer.RegisterLeaderboardReset()
	initializer.RegisterTournamentReset()

	// Similarly, you can run server code when the tournament ends.
	initializer.RegisterTournamentEnd()
	```

=== "JavaScript"
	```javascript
	// NOTE: All JavaScript runtime registrations must be made in the bundle's InitModule function.

	// If you are sending requests to the server via the realtime connection, ensure that you use this variant of the function.
	initializer.registerRtBefore()
	initializer.registerRtAfter()

	// Otherwise use the relevant before / after hook, e.g.
	initializer.registerAfterAddFriends()
	initializer.registerAfterAddFriends()
	// (...)

	// If you'd like to run server code when the matchmaker has matched players together, register your function using the following.
	initializer.registerMatchmakerMatched()

	// If you'd like to run server code when the leaderboard/tournament resets register your function using the following.
	initializer.registerLeaderboardReset()
	initializer.registerTournamentReset()

	// Similarly, you can run server code when the tournament ends.
	initializer.registerTournamentEnd()
	```

Have a look at [this section](#message-names) for a complete list of the server message names.

!!! Tip
    Only one hook may be registered for each type. If you register more than one hook, then only the last registration is used. RPC functions are unique per registered ID, and you can register the same function under multiple IDs.

### Before hook

Any function may be registered to intercept a message received from a client and operate on it (or reject it) based on custom logic. This is useful to enforce specific rules on top of the standard features in the server.

In Go, each hook will receive the request input as a struct containing the data that will be processed by the server for that request, if that feature is expected to receive an input. In Lua, the second argument will be the "incoming payload" containing data received that will be processed by the server. In JavaScript the payload is the fourth argument as seen in the example below.

=== "Lua"
	```lua
	local nk = require("nakama")

	local function limit_friends(context, payload)
		local user = nk.users_get_id({context.user_id})[1]
		-- Let's assume we've stored a user's level in their metadata.
		if user.metadata.level < 10 then
			error("Must reach level 10 before you can add friends.")
		end
		return payload -- important!
	end
	nk.register_req_before(limit_friends, "AddFriends")
	```

=== "Go"
	```go
	func BeforeAddFriends(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.AddFriendsRequest) (*api.AddFriendsRequest, error) {
			userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
			if !ok {
					return nil, errors.New("Missing user ID.")
			}

			account, err := nk.UsersGetId(ctx, []string{userID})
			if err != nil {
					return nil, err
			}

			var metadata map[string]interface{}
			if err := json.Unmarshal([]byte(account.GetUser().GetMetadata()), &metadata); err != nil {
					return nil, errors.New("Corrupted user metadata.")
			}

			// Let's assume we've stored a user's level in their metadata.
			if level, ok := metadata["level"].(int); !ok || level < 10 {
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
=== "JavaScript"
	```javascript
	function userAddFriendLevelCheck(ctx, logger, nk, payload) {
		var userId = ctx.userId;

		var user;
		try {
			user = nk.usersGetId([userId]);
		} catch(error) {
			logger.error('Failed to get user: %s', error.message);
			throw error;
		}

		// Let's assume we've stored a user's level in their metadata.
		if (user.metadata.level < 10) {
			throw Error('Must reach level 10 before you can add friends.');
		}

		// important!
		return payload;
	});

	// Register as an after hook for the appropriate feature, this call should be in InitModule.
	initializer.registerBeforeAddFriends(userAddFriendLevelCheck);
	```

The code above fetches the current user's profile and checks the metadata which is assumed to be JSON encoded with `"{level: 12}"` in it. If a user's level is too low, an error is thrown to prevent the Friend Add message from being passed onwards in the server pipeline.

!!! Note
    You must remember to return the payload at the end of your function in the same structure as you received it. See the lines highlighted in the code above.

!!! Tip
    If you choose to return `nil` (Lua) or `null|undefined` (JavaScript) instead of the `payload` (or a non-nil `error` in Go) the server will halt further processing of that message. This can be used to stop the server from accepting certain messages or disabling/blacklisting certain server features.

### After hook

Similar to [Before hook](#before-hook) you can attach a function to operate on a message. The registered function will be called after the message has been processed in the pipeline. The custom code will be executed asynchronously after the response message has been sent to a client.

The second argument is the "outgoing payload" containing the server's response to the request. The third argument contains the "incoming payload" containing the data originally passed to the server for this request.

=== "Lua"
	```lua
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

=== "Go"
	```go
	func AfterAddFriends(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.AddFriendsRequest) error {
	    userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
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
	            UserID:     userID,
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

=== "JavaScript"
	```javascript
	function afterAddFriends(ctx, logger, nk, outgoingPayload, incomingPayload) {
		var userId = ctx.userId;
		if (!userId) {
			throw Error('Missing user ID.');
		}

		var userIds = incomingPayload.userIds;
		var storageObj = {
			collection: 'rewards',
			key: 'reward',
			userId: userId,
			value: {userIds},
		};

		try {
			nk.storageWrite(storageObj);
		} catch(error) {
			logger.error('Error writing storage object: %s', error.message);
			throw error;
		};

		return null; // Can be omitted, will return `undefined` implicitly
	});

	// Register as an after hook for the appropriate feature, this call should be in InitModule.
	initializer.registerAfterAddFriends(afterAddFriends);
	```

The simple code above writes a record to a user's storage when they add a friend. Any data returned by the function will be discarded.

!!! Tip
    After hooks cannot change the response payload being sent back to the client, and errors do not prevent the response from being sent.

### RPC hook

Some logic between client and server is best handled as RPC functions which clients can execute. For this purpose Nakama supports the registration of custom RPC hooks.

=== "Lua"
	```lua
	local nk = require("nakama")

	local function custom_rpc_func(context, payload)
	  nk.logger_info(string.format("Payload: %q", payload))

	  -- "payload" is bytes sent by the client we'll JSON decode it.
	  local json = nk.json_decode(payload)

	  return nk.json_encode(json)
	end

	nk.register_rpc(custom_rpc_func, "custom_rpc_func_id")
	```

=== "Go"
	```go
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

=== "JavaScript"
	```javascript
	function customRpcFunc(ctx, logger, nk, payload) {
		logger.info('payload: %q', payload);

		// "payload" is bytes sent by the client we'll JSON decode it.
		var json = JSON.parse(payload);

		return JSON.stringify(json);
	}

	// Register as an after hook for the appropriate feature, this call should be in InitModule.
	initializer.registerRpc("custom_rpc_func_id", customRpcFunc);
	```

The code above registers a function with the identifier "custom_rpc_func_id". This ID can be used within client code to send an RPC message to execute the function on the server and return the result.

From Go runtime code, the result is returned as `(string, error)`. From Lua runtime code, results are always returned as a Lua string (or optionally `nil`). From the JavaScript runtime code, results should always be a string, null or omitted (undefined);

#### Server to server

Sometimes it's useful to create HTTP REST handlers which can be used by web services and ease integration into custom server environments. This can be achieved by using the [RPC hook](#rpc-hook). However, this uses the [Runtime HTTP Key](install-configuration.md#runtime) to authenticate with the server.

=== "Lua"
	```lua
	local nk = require("nakama")

	local function http_handler(context, payload)
	  local message = nk.json_decode(payload)
	  nk.logger_info(string.format("Message: %q", message))
	  return nk.json_encode({["context"] = context})
	end

	nk.register_rpc(http_handler, "http_handler_path")
	```

=== "Go"
	```go
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

=== "JavaScript"
	```javascript
		function customRpcFunc(ctx, logger, nk, payload) {
		logger.info('payload: %q', payload);

		if (ctx.userId) {
			// Reject non server-to-server call
			throw Error('Cannot invoke this function from user session');
		}

		var message = JSON.parse(payload);
		logger.info('Message: %q', message);

		return JSON.stringify({context: ctx});
	}

	// Register as an after hook for the appropriate feature, this call should be in InitModule.
	initializer.registerRpc("custom_rpc_func_id", customRpcFunc);
	```

!!! Tip
    RPC functions can be called both from clients and through server to server calls. You can tell them apart by [checking if the context has a user ID](#register-hooks) - server to server calls will never have a user ID. If you want to scope functions to never be accessible from the client just return an error if you find a user ID in the context.

The registered RPC Functions can be invoked with any HTTP client of your choice. For example, with cURL you could execute the function with the server as follows.

=== "Bash"
	```shell
	curl "http://127.0.0.1:7350/v2/rpc/http_handler_path?http_key=defaulthttpkey" \
		-d '"{\"some\": \"data\"}"' \
		-H 'Content-Type: application/json' \
		-H 'Accept: application/json'
	```

Notice that the JSON payload is escaped and wrapped inside a string. This is by design due to gRPC not having a type that would map between a Protobuf type and a JSON object at the time the RPC API was designed. Support for JSON has since been added to gRPC but we have kept it this way to not break the API contract and ensure compatibility.

Since Nakama v.2.7.0 an `unwrap` query parameter is supported which allows to invoke RPC functions with raw JSON data in the payload. An example is provided below.

=== "Bash"
	```shell
	curl "http://127.0.0.1:7350/v2/rpc/http_handler_path?http_key=defaulthttpkey&unwrap" \
		-d '{"some": "data"}' \
		-H 'Content-Type: application/json' \
		-H 'Accept: application/json'
	```

!!! Warning "HTTP key"
	You should change the default HTTP key before you deploy your code in production.

## Run once

The runtime environment allows you to run code that must only be executed only once. This is useful if you have custom SQL queries that you need to perform (like creating a new table) or to register with third party services.

=== "Lua"
	```lua
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

=== "Go"
	```go
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

!!! Warning "Unsupported in JavaScript"
	This functionality is currently not supported in the JavaScript runtime.

## Errors and logs

Error handling in Go follows the standard pattern of returning an `error` value as the last argument of a function call. If the `error` is `nil` then the call was successful.

Lua error handling uses raised errors rather than error return values. If you want to trap the error which occurs in the execution of a function you'll need to execute it via `pcall` as a "protected call".

JavaScript uses exceptions to handle errors. When an error occurs, an exception is thrown. To handle an exception thrown by a custom function or one provided by the runtime, you must wrap the code in a `try catch` block.

=== "Lua"
	```lua
	local function will_error()
	  error("This function will always throw an error!")
	end

	if pcall(will_error) then
	  -- No errors with "will_error".
	else
	  -- Handle errors.
	end
	```

=== "Go"
	```go
	func willError() error {
	  return errors.New("This function will always throw an error!")
	}

	if err := willError(); err != nil {
	  // Handle errors.
	} else {
	  // No errors with "willError".
	}
	```

=== "JavaScript"
	```javascript
	function throws() {
		throw Error("I'm an exception");
	}

	try {
		throws();
	} catch(error) {
		// Handle error.
		logger.error('Caught exception: %s', error.message);
	}
	```

The function `will_error` uses the `error` function in Lua to throw an error with a reason message. The `pcall` will invoke the `will_error` function and trap any errors. We can then handle the success or error cases as needed. We recommend you use this pattern with your Lua code.

Unhandled exceptions in JavaScript are caught and logged by the runtime, except if they are not handled during initialization (when the runtime invokes the InitModule function at startup) - these will halt the server. We recommend you use this pattern and wrap all runtime API calls for error handling and inspection.

=== "Lua"
	```lua
	local nk = require("nakama")

	local status, result = pcall(nk.users_get_username, {"22e9ed62"})
	if (not status) then
	  nk.logger_error(string.format("Error occurred: %q", result))
	else
	  for _, u in ipairs(result)
	  do
	    local message = string.format("id: %q, display name: %q", u.id, u.display_name)
	    nk.logger_info(message) -- Will appear in logging output.
	  end
	end
	```

=== "Go"
	```go
	users, err := nk.UsersGetUsername([]string{"22e9ed62"})
	if err != nil {
	  logger.Error("Error occurred: %v", err.Error())
	} else {
	  for _, u := range users {
	    logger.Info("id: %v, display name: %v", u.Id, u.DisplayName) // Will appear in logging output.
	  }
	}
	```

=== "JavaScript"
	```javascript
	try {
		// Will throw an exception because this function expects an array, not an object.
		nk.usersGetUsername({ids: []});
	} catch(error) {
		logger.error('An error has occurred: %s', error.message);
	}
	```

The JavaScript logger is just a stub to the Go logger, hence why in the examples you've seen formatting 'verbs' (e.g.: '%s') in the output strings, followed by the arguments that will replace them, in order. If you wish to better log and inspect the underlying Go structs used by the JavaScript VM you can use verbs such as '%#v'. The full reference can be found [here](https://golang.org/pkg/fmt/).

!!! Warning "Lua stacktraces"
    If the server logger level is set to `info` (default level) or below, the server will return Lua stacktraces to the client. This is useful for debugging but should be disabled for production.

## Restrictions

### Compatibility

#### Lua

The Lua runtime is a Lua 5.1-compatible implementation with a small set of additional packages backported from newer versions - see [available functions](#available-functions). For best results ensure your Lua modules and any 3rd party libraries are compatible with Lua 5.1.

#### Go

Go runtime available functionality depends on the version of Go each Nakama release is compiled with. This is usually the latest stable version at the time of release. Check server startup logging for the exact Go version used by your Nakama installation.

!!! Note
    Lua runtime code cannot use the Lua C API or extensions. Make sure your code and any 3rd party libraries are pure Lua 5.1.

#### JavaScript

The JavaScript runtime is powered by the [goja library](https://github.com/dop251/goja) which currently only supports the JavaScript ES5 spec.

### Available functions

The Lua virtual machine embedded in the server uses a restricted set of Lua standard library modules. This ensures the code sandbox cannot tamper with operating system input/output or the filesystem.

The list of available Lua modules are: base module, `math`, `string`, `table`, `bit32`, and a subset of `os` (only `clock`, `difftime`, `date`, and `time` functions).

Go runtime code can make use of the full range of standard library functions and packages.

The JavaScript runtime has access to the standard library functions included in the ES5 spec.

!!! Tip
    You cannot call Lua functions from the Go runtime, or Go functions from the Lua runtime.

### Global state

The Lua and JavaScript runtime code is executed in instanced contexts. You cannot use global variables as a way to store state in memory or communicate with other Lua/JS processes or function calls.

The Go runtime does not have this restriction and can store and share data as needed, but concurrency and access controls are the responsibility of the developer.

### Sandboxing

The Lua and JavaScript runtime code is fully sandboxed and cannot access the filesystem, input/output devices, or spawn OS threads or processes. This allows the server to guarantee that Lua/JS modules cannot cause fatal errors - the runtime code cannot trigger unexpected client disconnects or affect the main server process.

Go runtime code has full low-level access to the server and its environment. This allows full flexibility and control to include powerful features and offer high performance, but cannot guarantee error safety - the server does not guard against fatal errors in Go runtime code, such as segmentation faults or pointer dereference failures.

## An example module

As a fun example, let's use the [Pokéapi](http://pokeapi.co/) and build a helpful module named "pokeapi.lua".

=== "Lua"
	```lua
	local nk = require("nakama")

	local M = {}

	local API_BASE_URL = "https://pokeapi.co/api/v2"

	function M.lookup_pokemon(name)
	  local url = string.format("%s/pokemon/%s", API_BASE_URL, name)
	  local method = "GET"
	  local headers = {
	    ["Content-Type"] = "application/json",
	    ["Accept"] = "application/json"
	  }
	  local success, code, _, body = pcall(nk.http_request, url, method, headers, nil)
	  if (not success) then
	    nk.logger_error(string.format("Failed request %q", code))
	    error(code)
	  elseif (code >= 400) then
	    nk.logger_error(string.format("Failed request %q %q", code, body))
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

=== "Go"
	```go
	import (
	  "context"
	  "database/sql"
	  "encoding/json"
	  "errors"
	  "io/ioutil"
	  "net/http"

	  "github.com/heroiclabs/nakama-common/runtime"
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
	    return nil, err
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

=== "JavaScript"
	```javascript
	var apiBaseUrl = 'https://pokeapi.co/api/v2';

	function InitModule(ctx, logger, nk, initializer) {
		initializer.registerRpc('get_pokemon', getPokemon);
	}

	function lookupPokemon(nk, name) {
		var url = apiBaseUrl + '/pokemon/' + name;
		var headers = { 'Accept': 'application/json' };

		var response = nk.httpRequest(url, 'get', headers);

		return JSON.parse(response.body);
	}

	function getPokemon(ctx, logger, nk, payload) {
		// We'll assume payload was sent as JSON and decode it.
		var json = JSON.parse(payload);

		var pokemon;
		try {
			pokemon = lookupPokemon(nk, json['PokemonName']);
		} catch(error) {
			logger.error('An error occurred looking up pokemon: %s', error.message);
			throw error;
		}

		var result = {
			name: pokemon.name,
			height: pokemon.height,
			weight: pokemon.weight,
			image: pokemon.sprites.front_default,
		}

		return JSON.stringify(result);
	}
	```

!!! Tip
    To use the Go runtime don't forget to compile your code following [these instructions](https://github.com/heroiclabs/nakama/tree/master/sample_go_module) carefully.

We can now make an RPC call for a pokemon from a client.

=== "cURL"
	```sh
	curl "http://127.0.0.1:7350/v2/rpc/get_pokemon" \
	  -H 'authorization: Bearer <session token>'
	  -d '"{\"PokemonName\": \"dragonite\"}"'
	```

=== "Javascript"
	```js
	const payload = { "PokemonName": "dragonite"};
	const rpcid = "get_pokemon";
	const pokemonInfo = await client.rpc(session, rpcid, payload);
	console.log("Retrieved pokemon info: %o", pokemonInfo);
	```

=== ".NET"
	```csharp
	var payload = "{\"PokemonName\": \"dragonite\"}";
	var rpcid = "get_pokemon";
	var pokemonInfo = await client.RpcAsync(session, rpcid, payload);
	System.Console.WriteLine("Retrieved pokemon info: {0}", pokemonInfo);
	```

=== "Unity"
	```csharp
	var payload = "{\"PokemonName\": \"dragonite\"}";
	var rpcid = "get_pokemon";
	var pokemonInfo = await client.RpcAsync(session, rpcid, payload);
	Debug.LogFormat("Retrieved pokemon info: {0}", pokemonInfo);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](const NRpc& rpc)
	{
	  CCLOG("Retrieved pokemon info: %s", rpc.payload.c_str());
	};

	string payload = "{ \"PokemonName\": \"dragonite\" }";
	string rpcid = "get_pokemon";
	client->rpc(session, rpcid, payload, successCallback);
	```

=== "Cocos2d-x JS"
	```js
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

=== "C++"
	```cpp
	auto successCallback = [](const NRpc& rpc)
	{
	  std::cout << "Retrieved pokemon info: " << rpc.payload << std::endl;
	};

	string payload = "{ \"PokemonName\": \"dragonite\" }";
	string rpcid = "get_pokemon";
	client->rpc(session, rpcid, payload, successCallback);
	```

=== "Android/Java"
	```java
	String payload = "{\"PokemonName\": \"dragonite\"}";
	String rpcid = "get_pokemon";
	Rpc pokemonInfo = client.rpc(session, rpcid, payload);
	System.out.format("Retrieved pokemon info: %s", pokemonInfo.getPayload());
	```

=== "Swift"
	```swift
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

=== "Godot"
	```gdscript
	var payload = {"PokemonName": "dragonite"}
	var rpc_id = "get_pokemon"
	var pokemon_info : NakamaAPI.ApiRpc = yield(client.rpc_async(session, rpc_id, JSON.print(payload)), "completed")
	if pokemon_info.is_exception():
		print("An error occured: %s" % pokemon_info)
		return
	print("Retrieved pokemon info: %s" % [parse_json(pokemon_info.payload)])
	```

=== "REST"
    ```
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

If your runtime code is in Go, refer to [the interface definition](https://github.com/heroiclabs/nakama/blob/master/server/runtime.go) for a full list of hooks that are available in the runtime package.

In Lua, you should use the following request names for `register_req_before` and `register_req_after` hooks:

| Request Name            | Description                                                                       |
| ----------------------- | --------------------------------------------------------------------------------- |
| AddFriends              | Add friends by ID or username to a user's account.                                |
| AddGroupUsers           | Add users to a group.                                                             |
| AuthenticateCustom      | Authenticate a user with a custom id against the server.                          |
| AuthenticateDevice      | Authenticate a user with a device id against the server.                          |
| AuthenticateEmail       | Authenticate a user with an email+password against the server.                    |
| AuthenticateFacebook    | Authenticate a user with a Facebook OAuth token against the server.               |
| AuthenticateGameCenter  | Authenticate a user with Apple's GameCenter against the server.                   |
| AuthenticateGoogle      | Authenticate a user with Google against the server.                               |
| AuthenticateSteam       | Authenticate a user with Steam against the server.                                |
| BlockFriends            | Block one or more users by ID or username.                                        |
| CreateGroup             | Create a new group with the current user as the owner.                            |
| DeleteFriends           | Delete one or more users by ID or username.                                       |
| DeleteGroup             | Delete one or more groups by ID.                                                  |
| DeleteLeaderboardRecord | Delete a leaderboard record.                                                      |
| DeleteNotifications     | Delete one or more notifications for the current user.                            |
| DeleteStorageObjects    | Delete one or more objects by ID or username.                                     |
| GetAccount              | Fetch the current user's account.                                                 |
| GetUsers                | Fetch zero or more users by ID and/or username.                                   |
| Healthcheck             | A healthcheck which load balancers can use to check the service.                  |
| ImportFacebookFriends   | Import Facebook friends and add them to a user's account.                         |
| JoinGroup               | Immediately join an open group, or request to join a closed one.                  |
| KickGroupUsers          | Kick a set of users from a group.                                                 |
| LeaveGroup              | Leave a group the user is a member of.                                            |
| LinkCustom              | Add a custom ID to the social profiles on the current user's account.             |
| LinkDevice              | Add a device ID to the social profiles on the current user's account.             |
| LinkEmail               | Add an email+password to the social profiles on the current user's account.       |
| LinkFacebook            | Add Facebook to the social profiles on the current user's account.                |
| LinkGameCenter          | Add Apple's GameCenter to the social profiles on the current user's account.      |
| LinkGoogle              | Add Google to the social profiles on the current user's account.                  |
| LinkSteam               | Add Steam to the social profiles on the current user's account.                   |
| ListChannelMessages     | List a channel's message history.                                                 |
| ListFriends             | List all friends for the current user.                                            |
| ListGroups              | List groups based on given filters.                                               |
| ListGroupUsers          | List all users that are part of a group.                                          |
| ListLeaderboardRecords  | List leaderboard records                                                          |
| ListMatches             | Fetch a list of running matches.                                                  |
| ListNotifications       | Fetch a list of notifications.                                                    |
| ListStorageObjects      | List publicly readable storage objects in a given collection.                     |
| ListUserGroups          | List groups the current user belongs to.                                          |
| PromoteGroupUsers       | Promote a set of users in a group to the next role up.                            |
| DemoteGroupUsers        | Demote a set of users in a group to a lower role.                                 |
| ReadStorageObjects      | Get storage objects.                                                              |
| UnlinkCustom            | Remove the custom ID from the social profiles on the current user's account.      |
| UnlinkDevice            | Remove the device ID from the social profiles on the current user's account.      |
| UnlinkEmail             | Remove the email+password from the social profiles on the current user's account. |
| UnlinkFacebook          | Remove Facebook from the social profiles on the current user's account.           |
| UnlinkGameCenter        | Remove Apple's GameCenter from the social profiles on the current user's account. |
| UnlinkGoogle            | Remove Google from the social profiles on the current user's account.             |
| UnlinkSteam             | Remove Steam from the social profiles on the current user's account.              |
| UpdateAccount           | Update fields in the current user's account.                                      |
| UpdateGroup             | Update fields in a given group.                                                   |
| WriteLeaderboardRecord  | Write a record to a leaderboard.                                                  |
| WriteStorageObjects     | Write objects into the storage engine.                                            |

Names are case-insensitive. For more information, have a look at ["apigrpc.proto"](https://github.com/heroiclabs/nakama/blob/master/apigrpc/apigrpc.proto).

You should use the following message names for `register_rt_before` and `register_rt_after` hooks:

| Message Name         | Description                                                                 |
| -------------------- | --------------------------------------------------------------------------- |
| ChannelJoin          | Join a realtime chat channel.                                               |
| ChannelLeave         | Leave a realtime chat channel.                                              |
| ChannelMessageSend   | Send a message to a realtime chat channel.                                  |
| ChannelMessageUpdate | Update a message previously sent to a realtime chat channel.                |
| ChannelMessageRemove | Remove a message previously sent to a realtime chat channel.                |
| MatchCreate          | A client to server request to create a realtime match.                      |
| MatchDataSend        | A client to server request to send data to a realtime match.                |
| MatchJoin            | A client to server request to join a realtime match.                        |
| MatchLeave           | A client to server request to leave a realtime match.                       |
| MatchmakerAdd        | Submit a new matchmaking process request.                                   |
| MatchmakerRemove     | Cancel a matchmaking process using a ticket.                                |
| StatusFollow         | Start following some set of users to receive their status updates.          |
| StatusUnfollow       | Stop following some set of users to no longer receive their status updates. |
| StatusUpdate         | Set the user's own status.                                                  |

Names are case-insensitive. For more information, have a look at ["realtime.proto"](https://github.com/heroiclabs/nakama-common/blob/master/rtapi/realtime.proto).
