# Authoritative Multiplayer

Nakama supports relayed multiplayer (also known as [client-authoritative](gameplay-multiplayer-realtime.md)) multiplayer as well as server-authoritative multiplayer.

In relayed multiplayer messages destined for other clients are sent onwards by the server without inspection. This approach relies on one client in each match to act as the host to reconcile state changes between peers and perform arbitration on ambiguous or malicious messages sent from bad clients.

Relayed multiplayer is very useful for many types of gameplay but may not suitable for gameplay which depends on central state managed by the game server.

Technically all multiplayer games can be developed as relayed if player counts are small per match but to choose between which approach to use you must decide how important it is for authoritative control to be handled on the server. With Nakama you have the freedom and flexibility to decide without limitations.

To support multiplayer game designs which require data messages to change state maintained on the server the authoritative multiplayer engine introduces a way to run custom match logic with a fixed tick rate. Messages can be validated and state changes broadcast to connected peers. This enables you to build:

1. **Asynchronous real-time authoritiative multiplayer**: Fast paced realtime multiplayer. Messages are sent to the server, server calculates changes to the environment and players and data is broadcasted to relevant peers. This typically requires a high tick-rate for the gameplay to feel responsive.
2. **Active turn-based multiplayer**: Like with Stormbound or Clash Royale mobile games where two or more players are connected and are playing a quick turn-based match. Players are expected to respond to turns immediately. The server receives input, validates them and broadcast to players. The expected tick-rate is quite low as rate of message sent and received is low.
3. **Passive turn-based multiplayer**: A great example is Words With Friends on mobile where the gameplay can span several hours to weeks. The server receives input, validates them, stores them in the database and broadcast changes to any connected peers before shutting down the server loop until next gameplay sequence.

To support this functionality the Authoritative Multiplayer feature introduces several concepts.

## Concepts

### Match handler

A match handler represents all server-side functions grouped together to handle game inputs and operate on them. 5 functions are required to process logic for a match at a fixed rate on the server. The server can run many thousands of matches per machine depending on the player counts and hardware. The match handler has an API to broadcast messages out to connected players.

### Tick rate

The server will periodically call the match loop function even when there is no input waiting to be processed. The logic is able to advance the game state as needed. It can also validate incoming input and kick players who've been inactive for some time.

This periodic call is known as Tick Rate and represents a desired fixed frequency at which the match should update. Tick Rate is configurable and typical frequencies range from once per second for turn-based games to dozens of times per second for fast-paced gameplay.

All incoming client data messages are queued until each tick when they are handed off to the match loop to be processed. Tick Rate is expressed as a number representing desired executions per second. For example a rate of "10" represents 10 ticks to the match loop per second (on average once every 100ms).

### Match state

The match state is a region of memory Nakama exposes to Authoritative Multiplayer matches to use for the duration of the match. The match handler governing each match may use this state to store any data it requires and is given the opportunity to update it during each tick.

State can be thought of as the result of continuous transformations applied to an initial state based on the loop of user input after validation.

### Host node

This host node is responsible for maintaining the in-memory match state and allocating CPU resource to execute the loop at the tick rate. Incoming user input messages that are waiting for the next tick to be processed are buffered in the host node to ensure it is immediately available on next match loop.

A single node is responsible for this to ensure the highest level of consistency accessing and updating the state and to avoid potential delays reconciling distributed state.

Match presences will still be replicated so all nodes in a cluster to have immediate access to both a list of matches and details about match participants.

The minimum structure of a match handler looks like:

```lua fct_label="Lua"
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

This match handler above does not do any work but demonstrates the various hooks into the authoritative realtime engine. If `nil` is returned the match is stopped.

## Create Authoritatve Matches

Authoritative matches can be created on the server in one of two ways.

### Manually

You can use an RPC function which submits some user IDs to the server and will create a match.

A match ID will be created which could be sent out to the players with an in-app notification or push message (or both). This approach is great when you want to manually create a match and compete with specific users.

```lua fct_label="Lua"
local nk = require("nakama")
local function create_match(context, payload)
  local modulename = "pingpong"
  local setupstate = { initialstate = payload }
  local matchid = nk.match_create(modulename, setupstate)

  -- Send notification of some kind
  return matchid
end
nk.register_rpc(create_match, "create_match_rpc")
```

### Matchmaker

Use the [matchmaker](gameplay-matchmaker.md) to find opponents and use the matchmaker matched callback on the server to create an authoritative match and return a match ID. This uses the standard matchmaker API on the client.

The clients will receive the matchmaker callback as normal with a match ID.

```lua fct_label="Lua"
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

The matchmaker matched hook must return a match ID or `nil` if the match should proceed as relayed multiplayer.

The string passed into the match create function is a Lua module name. In this example it'd be a file named `pingpong.lua`.

## Join a Match

Players are not in the match until they join even after matched by the matchmaker. This enables players to opt out of matches they decide not to play.

This can be done by clients in the same way as with relayed multiplayer. A full example of how to do this is covered [here](gameplay-multiplayer-realtime.md#join-a-match).

## Match Listings

You can list matches that are currently active on the server. You can also filter matches based on exact-match queries on the label field.

For instance if a match was created with a label field of "skill=100-150" you can filter down to relevant matches.

```lua fct_label="Lua"
local nk = require("nakama")

local limit = 10
local isauthoritative = true
local label = "skill=100-150"
local min_size = 0
local max_size = 4
local matches = nk.match_list(limit, isauthoritative, label, min_size, max_size)
for _, match in ipairs(matches) do
  print(("Match id %s"):format(match.match_id))
end
```

This is useful to present a lobby-like experience or search for matches before creating a new match.

```lua fct_label="Lua"
local nk = require("nakama")
local function findorcreatematch(limit, label, min_size, max_size)
  local matches = nk.match_list(limit, true, label, min_size, max_size)
  if (#matches > 0) then
    table.sort(matches, function(a, b)
      return a.size > b.size;
    end)
    return matches[1].match_id
  end
  local modulename = "supermatch"
  local initialstate = {}
  local match_id = nk.match_create(modulename, initialstate)
  return match_id
end
```

## Match Handler API

The match handler that govern Authoritative Multiplayer matches must implement all of the function callbacks below.

!!! Note "Handler Errors"
    Errors generated in any of the callbacks result in a force disconnect of all clients to that match.

__match_init(context, params) -> state, tickrate, label__

This is invoked when a match is created as a result of the match create function and sets up the initial state of a match. This will be called once at match start.

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

```lua fct_label="Lua"
function match_init(context, params)
  local state = {}
  local tick_rate = 1
  local label = "skill=100-150"

  return state, tick_rate, label
end
```

---

__match_join_attempt(context, dispatcher, tick, state, presence) -> state, accepted__

Executed when a user attempts to join the match using the client's match join operation. Match join attempt can be used to prevent more players from joining after a match has started or disallow the user for any other game specific reason.

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

```lua fct_label="Lua"
local function match_join_attempt(context, dispatcher, tick, state, presence)
  -- Presence format:
  -- {
  --   user_id = "user unique ID",
  --   session_id = "session ID of the user's current connection",
  --   username = "user's unique username",
  --   node = "name of the Nakama node the user is connected to"
  -- }
  return state, true
end
```

---

__match_join(context, dispatcher, tick, state, presences) -> state__

Executed when one or more users have successfully completed the match join process after their `match_join_attempt()` returns
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

```lua fct_label="Lua"
local function match_join(context, dispatcher, tick, state, presences)
  -- Presences format:
  -- {
  --   {
  --     user_id = "user unique ID",
  --     session_id = "session ID of the user's current connection",
  --     username = "user's unique username",
  --     node = "name of the Nakama node the user is connected to"
  --   },
  --  ...
  -- }
  return state
end
```

---

__match_leave(context, dispatcher, tick, state, presences) -> state__

Executed when one or more users have left the match for any reason including connection loss.

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

(table) - An (optionally) updated state. May be any non-nil Lua term or nil to end the match.

_Example_

```lua fct_label="Lua"
local function match_leave(context, dispatcher, tick, state, presences)
  return state
end
```

---

__match_loop(context, dispatcher, tick, state, messages) -> state__

Executed on an interval based on the tick rate returned by `match_init`. Each tick the match loop is run which can process messages received from clients and apply changes to the match state before the next tick. It can also dispatch messages to one or more connected opponents.

To send messages back to the opponents in the match you can keep track of them in the game state and use the dispatcher object to send messages to subsets of the users or all of them.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| context | table | [Context object](runtime-code-basics.md#register-hooks) represents information about the match and server for information purposes. |
| dispatcher | table | [Dispatcher](#match-runtime-api) exposes useful functions to the match. |
| tick | number | Tick is the current match tick number starts at 0 and increments after every `match_loop` call. Does not increment with calls to `match_join_attempt`, `match_join`, or `match_leave`. |
| state | table | The current in-memory match state, may be any Lua term except nil. |
| messages | table | Messages is a list of data messages received from users between the previous and current tick. |

_Returns_

You must return:

(table) - An (optionally) updated state. May be any non-nil Lua term, or nil to end the match.

_Example_

```lua fct_label="Lua"
local function match_loop(context, dispatcher, tick, state, messages)
  -- Messages format:
  -- {
  --   {
  --     sender = {
  --       user_id = "user unique ID",
  --       session_id = "session ID of the user's current connection",
  --       username = "user's unique username",
  --       node = "name of the Nakama node the user is connected to"
  --     },
  --     op_code = 1, -- numeric op code set by the sender.
  --     data = "any string data set by the sender" -- may be nil.
  --   },
  --   ...
  -- }
  return state
end
```

## Match runtime API

The dispatcher type passed into the handler functions expose the following functions:

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

```lua fct_label="Lua"
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

_Example_

```lua fct_label="Lua"
local nk = require("nakama")
function match_loop(context, dispatcher, tick, state, messages)
  -- Assume we store presences in state
  for i, presence in ipairs(state.presences) do
    dispatcher.match_kick(presence)
  end
  return state
end
```

---

__match_label_update(label)__

Sets a new label for the match.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| label | strig | New label to set for the match. |

_Example_

```lua
local nk = require("nakama")
function match_loop(context, dispatcher, tick, state, messages)
  dispatcher.match_label_update("updatedlabel")
  return state
end
```

## Full example

This is an example of a Ping-Pong match handler. Messages received by the server are broadcast back to the peer who sent them.

```lua fct_label="Lua"
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
    print(("Presence %s named %s"):format(presence.user_id, presence.username))
  end
  for _, message in ipairs(messages) do
    print(("Received %s from %s"):format(message.sender.username, message.data))
    local decoded = nk.json_decode(message.data)
    for k, v in pairs(decoded) do
      print(("Message key %s contains value %s"):format(k, v))
    end
    -- PONG message back to sender
    dispatcher.broadcast_message(1, message, message.sender.session_id, nil)
  end
  return state
end

return M
```
