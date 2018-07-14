# Server-authoritative Multiplayer

Nakama supports [client-authoritative](gameplay-multiplayer-realtime.md) (also known as relayed peer-to-peer) multiplayer as well as server-authoritative multiplayer.

In [client-authoritative multiplayer](gameplay-multiplayer-realtime.md) messages are relayed by the server without inspection. This relies on a client to act as the host to reconcile state changes between peers as well as perform arbitration on ambiguous or malicious messages sent from bad clients. This mode is useful for many game designs but not suitable for gameplay which depends on central state managed by the game server.

To support multiplayer game designs which require data messages to change central state maintained on the server, server needs to validate messages and broadcast changes to the connected peers. The server-authoritative multiplayer introduces a way to run custom match logic with a fixed tick rate on the server. This enables you to build:

1. **Asynchronous real-time authoritiative multiplayer**: This is similar to Epic's Fortnite Royale where there are many players in a match fighting to stay alive. Messages are sent to the server, server calculates changes to the environment and players and data is broadcasted to relevant peers. This typically requires a fairly high tick-rate.
2. **Active turn-based multiplayer**: This is similar to Supercell's Crash Royale _(or Stormbound if you know your mobile games)_ where two players are connected and are playing a short turn-based match. Players are expected to respond to turns immediately. The server receives input, validates them and broadcast to players. The expected tick-rate is quite low as rate of message receive is low.
3. **Passive turn-based multiplayer**: A great example is Words With Friends, where the gameplay can span several hours to weeks. The server receives input, validates them, stores them in the database and broadcast changes to any connected peers before shutting down the server loop until next gameplay.

To support this new functionality the Authoritative Multiplayer feature introduces several new concepts.

## Authoritative multiplayer concepts

### Tick rate

Alongside validating incoming input, authoritative control must also account for clients that misbehave by failing to send data when expected. Depending on game rules the server must be able to react to this scenario, perhaps by updating state or disconnecting the client.

To achieve this the server will periodically call the match loop function even when there is no input waiting to be processed. The Lua code then has the opportunity to advance the game state as needed. In turn-based games this may take the form of the current player passing their turn, for example.

This periodic call is known as Tick Rate and represents a desired fixed frequency at which the match should update. Tick Rate is configurable and typical frequencies range from once per second for turn-based games, to several times per second for fast-paced gameplay.

All incoming client data messages are queued until the next tick, when they are handed off to the match loop to be processed. Tick Rate is expressed as a number representing calls per second. For example a rate of “10” represents 10 calls to the match loop per second.

### Match state

The match state is a region of memory Nakama exposes to Authoritative Multiplayer matches to use for the duration of the match. The Lua module governing each match may use this state to store any data it requires, and is given the opportunity to update it during each tick in the Lua functions.

State can be thought of as the result of continuous transformations applied to an initial state based on the loop of user input, after proper validation.

### Host node

This host node is responsible for maintaining the in-memory Match State, and allocating resources to execute the Tick Rate. Incoming user input that is waiting for the next tick to be processed is also buffered in the host node to ensure it is immediately available on next match loop.

A single node is responsible for this to ensure the highest level of consistency accessing and updating the state, and to avoid potential delays reconciling distributed state.

Match presences will still be replicated so all nodes in a cluster will continue to have immediate access to both a list of matches and details about match participants.

### Match handler

The match handler is a Lua module which contains 5 functions **required** to execute logic for a match at a fixed tick rate on the server. The server can run many thousands of matches per node. The match handler has an API to broadcast messages out to the connected players.

The minimum structure of a match handler looks like:

```lua
local M = {}

function M.match_init(context, setupstate)
  local gamestate = {}
  local tickrate = 10
  local label = ""
  return gamestate, tickrate, label
end

function M.match_join_attempt(context, dispatcher, tick, state, presence)
  local acceptuser = true
  return state, acceptuser
end

function M.match_join(context, dispatcher, tick, state, presences)
  return state
end

function M.match_leave(context, dispatcher, tick, state, presences)
  return state
end

function M.match_loop(context, dispatcher, tick, state, messages)
  return state
end

return M
```

The match handler above does not do any work but demonstrates the various hooks into the authoritative realtime engine.

## Create an authoritative match

Authoritative matches can be created on the server in one of two ways.

### Explicitly create new match

You can use an RPC function which submits some user IDs to the server and will create a match which generates a match ID and then you could broadcast out to those users about the match ID with an in-app notification or push message (or both). This approach is great when you want to manually create a match and compete with specific users.

```lua
local nk = require("nakama")
local function create_match(context, payload)
  local modulename = "pingpong"
  local setupstate = { data = payload }
  local matchid = nk.match_create(modulename, setupstate)
  return matchid
end
nk.register_rpc(create_match, "create_match")
```

### Matchmaker

Use the matchmaker to find opponents and use the matchmaker matched callback on the server to create an authoritative match and return the match ID. This uses the standard matchmaker API on the client. The clients will receive the matchmaker callback as normal with a match ID.

```lua
local nk = require("nakama")

local function makematch(context, matched_users)
  -- print matched users
  for _, user in ipairs(matched_users) do
    local presence = user.presence
    print(("Matched user '%s' named '%s'"):format(presence.user_id, presence.username))
    for k, v in pairs(user.properties) do
      print(("Matched on '%s' value '%s'"):format(k, v))
    end
  end

  local modulename = "pingpong"
  local setupstate = { invited = matched_users }
  local matchid = nk.match_create(modulename, setupstate)
  return matchid
end
nk.register_matchmaker_matched(makematch)
```

Expected to return an authoritative match ID for a match ready to receive these users, or `nil` if the match should proceed through the peer-to-peer relayed mode.

The string passed into the `match_create` function is the Lua module name - which in Lua translates to the dot-separated path to the file. It must have the same file name as used with `match_create` so it can be fetched by the server. In this example it'd be called `pingpong.lua`.

## Join an authoritative match

Opponents are not in the match until they join even after matched by the matchmaker. This enables players to opt out of matches they decide not to play.

This can be done by clients in the same way as a relayed match. A full example of how to do this is covered [here](gameplay-multiplayer-realtime.md#join-a-match).

## Match listing

You can list matches that are currently active on the server. Moreover, you can also filter matches based on exact-match queries on the label field.

For instance if a match was created with a label field of `skill=100-150`, you can filter down to relevant matches like this:

```lua
--TBD
```

This is useful to present a lobby-like experience, or search for matches before creating a new match (similar to a Poker table):

```lua
--TBD
```

## Match handler API

Lua modules that govern Authoritative Multiplayer matches must implement all of the function callbacks below.

!!! Warning "Errors"
    Errors thrown in any of the callbacks result in a force disconnect of all clients to that match.

__match_init(context, params) -> state, tickrate, label__

This is invoked when a match is created as a result of `match_create()` and sets up the initial state of a match. This will be called once at match start.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| context | table | [Context object](runtime-code-basics.md#register-hooks) represents information about the match and server for information purposes. |
| params | table | Optional arbitrary second argument passed to `match_create()`, or `nil` if none was used. This can be matched users or any other data you'd like to pass into this function. |

_Returns_

You must return three values:

(table) - The initial in-memory state of the match. May be any non-nil Lua term, or nil to end the match.    
(number) - Tick rate representing the desired number of `match_loop()` calls per second. Must be between 1 and 30, inclusive.     
(string) - A string label that can be used to filter matches in listing operations. Must be between 0 and 256 characters long. This is used in [match listing](#match-listing) to filter matches.     

_Example_

```lua
function match_init(context, params)
  local state = {
    debug = (params and params.debug) or false
  }
  if state.debug then
    print("match init context:\n" .. du.print_r(context) .. "match init params:\n" .. du.print_r(params))
  end
  local tick_rate = 1
  local label = "skill=100-150"

  return state, tick_rate, label
end
```

---

__match_join_attempt(context, dispatcher, tick, state, presence) -> state, accepted__

Called when a user attempts to join the match using the client's match join operation. Match join attempt can be used to prevent more players from joining after a match has started or disallow the user for any other game specific reason.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| context | table | [Context object](runtime-code-basics.md#register-hooks) represents information about the match and server for information purposes. |
| dispatcher | table | [Dispatcher](#match-runtime-api) exposes useful functions to the match. |
| tick | number | Tick is the current match tick number, starts at 0 and increments after every `match_loop` call. Does not increment with calls to `match_join_attempt`, `match_join`, or `match_leave`. |
| state | table | The current in-memory match state, may be any Lua term except nil. |
| presence | table | Presence is the user attempting to join the match. |

_Returns_

You must return two values:

(table) - An (optionally) updated state. May be any non-nil Lua term, or nil to end the match.      
(boolean) - True if the join attempt should be allowed, false otherwise.

_Example_

```lua
local function match_join_attempt(context, dispatcher, tick, state, presence)

  -- Presence format:
  -- {
  --   user_id: "user unique ID",
  --   session_id: "session ID of the user's current connection",
  --   username: "user's unique username",
  --   node: "name of the Nakama node the user is connected to"
  -- }

  if state.debug then
    print("match join attempt:\n" .. du.print_r(presence))
  end
  return state, true
end
```

---

__match_join(context, dispatcher, tick, state, presences) -> state__

Called when one or more users have successfully completed the match join process after their `match_join_attempt()` returns
`true`. When their presences are sent to this function the users are ready to receive match data messages and can be
targets for the dispatcher's `broadcast_message()` function.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| context | table | [Context object](runtime-code-basics.md#register-hooks) represents information about the match and server for information purposes. |
| dispatcher | table | [Dispatcher](#match-runtime-api) exposes useful functions to the match. |
| tick | number | Tick is the current match tick number, starts at 0 and increments after every `match_loop` call. Does not increment with calls to `match_join_attempt`, `match_join`, or `match_leave`. |
| state | table | The current in-memory match state, may be any Lua term except nil. |
| presences | table | Presences is a list of users that have joined the match. |

_Returns_

You must return:

(table) - An (optionally) updated state. May be any non-nil Lua term, or nil to end the match.

_Example_

```lua
local function match_join(context, dispatcher, tick, state, presences)

  -- Presences format:
  -- {
  --   {
  --     user_id: "user unique ID",
  --     session_id: "session ID of the user's current connection",
  --     username: "user's unique username",
  --     node: "name of the Nakama node the user is connected to"
  --   },
  --  ...
  -- }

  if state.debug then
    print("match join:\n" .. du.print_r(presences))
  end
  return state
end
```

---

__match_leave(context, dispatcher, tick, state, presences) -> state__

Called when one or more users have left the match for any reason, including connection loss.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| context | table | [Context object](runtime-code-basics.md#register-hooks) represents information about the match and server for information purposes. |
| dispatcher | table | [Dispatcher](#match-runtime-api) exposes useful functions to the match. |
| tick | number | Tick is the current match tick number, starts at 0 and increments after every `match_loop` call. Does not increment with calls to `match_join_attempt`, `match_join`, or `match_leave`. |
| state | table | The current in-memory match state, may be any Lua term except nil. |
| presences | table | Presences is a list of users that have joined the match. |

_Returns_

You must return:

(table) - An (optionally) updated state. May be any non-nil Lua term, or nil to end the match.

_Example_

```lua
local function match_leave(context, dispatcher, tick, state, presences)
  if state.debug then
    print("match leave:\n" .. du.print_r(presences))
  end
  return state
end
```

---

__match_loop(context, dispatcher, tick, state, messages) -> state__

Called on an interval based on the tick rate returned by `match_init`. Each tick the match loop is fired which can process messages received from clients and apply changes to the match state before the next tick. It can also dispatch messages to one or more connected opponents.

To send messages back to the opponents in the match you can keep track of them in the game state and use the dispatcher object to send messages to subsets of the users or all of them.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| context | table | [Context object](runtime-code-basics.md#register-hooks) represents information about the match and server for information purposes. |
| dispatcher | table | [Dispatcher](#match-runtime-api) exposes useful functions to the match. |
| tick | number | Tick is the current match tick number, starts at 0 and increments after every `match_loop` call. Does not increment with calls to `match_join_attempt`, `match_join`, or `match_leave`. |
| state | table | The current in-memory match state, may be any Lua term except nil. |
| messages | table | Messages is a list of data messages received from users between the previous and current tick. |

_Returns_

You must return:

(table) - An (optionally) updated state. May be any non-nil Lua term, or nil to end the match.

_Example_

```lua
local function match_loop(context, dispatcher, tick, state, messages)

  -- Messages format:
  -- {
  --   {
  --     sender = {
  --       user_id: "user unique ID",
  --       session_id: "session ID of the user's current connection",
  --       username: "user's unique username",
  --       node: "name of the Nakama node the user is connected to"
  --     },
  --     op_code = 1, -- numeric op code set by the sender.
  --     data = "any string data set by the sender" -- may be nil.
  --   },
  --   ...
  -- }

  if state.debug then
    print("match " .. context.match_id .. " tick " .. tick)
    print("match " .. context.match_id .. " messages:\n" .. du.print_r(messages))
  end
  if tick < 10 then
    return state
  end
end
```

## Match runtime API

The `dispatcher` object passed into the handler functions expose the following functions:

__broadcast_message(op_code, data, presences, sender)__

Send a message to one or more presences.

This may be called at any point in the match loop to give match participants information about match state changes.

May also be useful inside the match join callback to send initial state to the user on successful join or inform them why they have been rejected.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| op_code | number | Numeric message op code. |
| data | string | Data payload string, or nil. |
| presences | table | List of presences (a subset of match participants) to use as message targets, or nil to send to the whole match. |
| sender | table | A presence to tag on the message as the 'sender', or nil. |

_Example_

```lua
local nk = require("nakama")
function match_loop(context, dispatcher, tick, state, messages)
  local opcode = 1234
  local message = { ["hello"] = "world" }
  local encoded = nk.json_encode(message)
  local presences = nil -- send to all.
  local sender = nil -- used if a message should come from a specific user.
  dispatcher.broadcast_message(opcode, encoded, presences, sender)
  return state
end
```

---

__match_kick(presences)__

Removes participants from the match.

Call at any point during the match loop to remove participants based on misbehaviour or other game-specific rules.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| presences | table | List of presences to remove from the match. |

---

__match_label_update(label)__

Sets a new label for the match.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| label | strig | New label to set for the match. |

## Full example

Below you can find an example ping-pong match handler:

```lua
local nk = require("nakama")

local M = {}

function M.match_init(context, setupstate)
  local gamestate = {
    presences = {}
  }
  local tickrate = 1 -- per sec
  local label = ""
  return gamestate, tickrate, label
end

function M.match_join_attempt(context, dispatcher, tick, state, presence)
  local acceptuser = true
  return state, acceptuser
end

function M.match_join(context, dispatcher, tick, state, presences)
  for _, presence in ipairs(presences) do
    state.presences[presence.session_id] = presence
  end
  return state
end

function M.match_leave(context, dispatcher, tick, state, presences)
  for _, presence in ipairs(presences) do
    state.presences[presence.session_id] = nil
  end
  return state
end

function M.match_loop(context, dispatcher, tick, state, messages)
  for _, presence in pairs(state.presences) do
    print(("Presence connected %s named %s"):format(presence.user_id, presence.username))
  end
  for _, message in ipairs(messages) do
    print(("Received %s from %s"):format(message.sender.username, message.data))
    local decoded = nk.json_decode(message.data)
    for k, v in pairs(decoded) do
      print(("Message contained %s value %s"):format(k, v))
    end
  end
  return state
end

return M
```
