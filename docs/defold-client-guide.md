# Defold client guide

The client is built in [Lua 5.1](https://www.lua.org/manual/5.1/) on top of the [Defold game engine](https://defold.com/). It is available at [GitHub](https://github.com/heroiclabs/nakama-defold/releases). It can also be used by other engines running lua, see [how](#adapting-to-other-engines).

!!! Tip "Contribute"
    The Defold client is <a href="https://github.com/heroiclabs/nakama-defold" target="\_blank">open source</a> on GitHub. Report issues and contribute code to help us improve it.

## Setup

### Add the client to your project.

Add the URL of a [client release .zip](https://github.com/heroiclabs/nakama-defold/releases) as a library dependency to `game.project`. The client will now show up as a `nakama` folder in your project.

### Add Defold plugins.

Defold projects additionally require the following modules:

    https://github.com/britzl/defold-websocket/archive/1.6.0.zip
    https://github.com/britzl/defold-luasocket/archive/0.11.1.zip
    https://github.com/britzl/defold-luasec/archive/1.1.0.zip

## Authenticate

There are many options when it comes to [authentication](https://heroiclabs.com/docs/authentication). In this example we'll do it with email and password but it's just as easy to use a social profile from Google Play Games, Facebook, Game Center, etc.

```lua
function init(self)
    local defold = require "nakama.engine.defold"
    local nakama = require "nakama.nakama"
    local config = {
    host = "127.0.0.1",
    port = 7350,
    username = "defaultkey",
    password = "",
    engine = defold,
    }
    local client = nakama.create_client(config)

    local email = "batman@heroes.com"
    local password = "password"
    local body = nakama.create_api_account_email(email, password)

    nakama.sync(function()
    local session = nakama.authenticate_email(client, body)
    nakama.set_bearer_token(client, session.token)

    local account = nakama.get_account(client)
    print("user id is " .. account.user_id .. " and username is " .. account.username)
    end)
end
```

## Sessions

When client authenticates, the server responds with an auth token (JWT) which should be used when making API requests. The token contains useful properties and gets deserialized into a `session` table.

```lua
local client = nakama.create_client(config)

print(session.token) -- raw JWT token
print(session.user_id)
print(session.username)
print(session.expires)
print(session.created)

-- Configure client to use the token with API requests
nakama.set_bearer_token(client, session.token)
```

It is recommended to store the auth token from the session and check at startup if it has expired. If the token has expired you must reauthenticate. The expiry time of the token can be changed as a [setting](install-configuration.md#common-properties) in the server.

```lua
-- Assume we've stored the auth token
local nakama_session = require "nakama.session"
local token = sys.load(token_path)
local session = nakama_session.create(token)
if nakama_session.expired(session) then
    print("Session has expired. Must reauthenticate.")
else
    nakama.set_bearer_token(client, session.token)
end
```

## Send requests

The client includes many APIs for Nakama game server features. These can be accessed with the methods which either use a callback function to return a result or block until a response is available. You absolutely cannot use the blocking variety unless running in a [coroutine](https://www.lua.org/manual/5.1/manual.html#2.11).

```lua
-- using a callback
nakama.get_account(client, function(account)
    print(account.user.id);
    print(account.user.username);
    print(account.wallet);
end)

-- if run from within a coroutine
local account = nakama.get_account(client)
print(account.user.id);
print(account.user.username);
print(account.wallet);
```

The Nakama client provides a convenience function for creating and starting a coroutine to run multiple requests synchronously:

```lua
nakama.sync(function()
    local account = nakama.get_account(client)
    local result = nakama.update_account(client, request)
end)
```

## Socket messages

The client can create one or more sockets with the server. Each socket can have it's own event listeners registered for responses received from the server.

```lua
-- create socket
local socket = nakama.create_socket(client)

nakama.sync(function()
    -- connect
    local ok, err = nakama.socket_connect(socket)

    -- add socket listeners
    nakama.on_disconnect(socket, function(message)
        print("Disconnected!")
    end)
    nakama.on_channelpresence(socket, function(message)
        pprint(message)
    end)

    -- send channel join message
    local channel_id = "pineapple-pizza-lovers-room"
    local channel_join_message = {
        channel_join = {
            type = 1, -- 1 = room, 2 = Direct Message, 3 = Group
            target = channel_id,
            persistence = false,
            hidden = false,
        }
    }
    local result = nakama.socket_send(socket, channel_join_message)
end)
```

You can connect to the server over a realtime socket connection to send and receive [chat messages](social-realtime-chat.md), get [notifications](social-in-app-notifications.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

## Handle events

A socket object has event handlers which are called on various messages received from the server.

Event handlers only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| on_disconnect | Handles an event for when the client is disconnected from the server. |
| on_error | Receives events about server errors. |
| on_notification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| on_channelmessage | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| on_channelpresence | It handles join and leave events within [chat](social-realtime-chat.md). |
| on_matchdata | Receives [realtime multiplayer](gameplay-multiplayer-realtime.md) match data. |
| on_matchpresence | It handles join and leave events within [realtime multiplayer](gameplay-multiplayer-realtime.md). |
| on_matchmakermatched | Received when the [matchmaker](gameplay-matchmaker.md) has found a suitable match. |
| on_statuspresence | It handles status updates when subscribed to a user [status feed](social-status.md). |
| on_streampresence | Receives [stream](advanced-streams.md) join and leave event. |
| on_streamdata | Receives [stream](advanced-streams.md) data sent by the server. |

## Logs and errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful for debugging.

To enable verbose logging when you work on a client:
```lua
log = require "nakama.util.log"
log.print() -- enable trace logging in nakama client
```

For production use
```lua
log.silent() -- disable trace logging in nakama client
```

## Full example

A small example on how to manage a session object with Defold engine and the Lua client.

```lua
local nakama = require "nakama.nakama"
local log = require "nakama.util.log"
local defold = require "nakama.engine.defold"

local function email_login(client, email, password, username)
    local body = nakama.create_api_account_email(email, password)
    local result = nakama.authenticate_email(client, body, true, username)
    if result.token then
        nakama.set_bearer_token(client, result.token)
        return true
    end
    log("Unable to login")
    return false
end

function init(self)
    log.print()

    local config = {
        host = "127.0.0.1",
        port = 7350,
        username = "defaultkey",
        password = "",
        engine = defold,
    }
    local client = nakama.create_client(config)

    nakama.sync(function()
        local ok = email_login(client, "batman@heroes.com", "password", "batman")
        if not ok then
            return
        end
        local account = nakama.get_account(client)
        pprint(account)

        local socket = nakama.create_socket(client)
        nakama.on_channelmessage(socket, function(message)
            pprint(message)
        end)
        nakama.on_channelpresence(socket, function(message)
            pprint(message)
        end)
        local ok, err = nakama.socket_connect(socket)
        print("connect", ok, err)

        local channel_id = "pineapple-pizza-lovers-room"
        local channel_join_message = {
            channel_join = {
                type = 1, -- 1 = room, 2 = Direct Message, 3 = Group
                target = channel_id,
                persistence = false,
                hidden = false,
            }
        }
        local result = nakama.socket_send(socket, channel_join_message)
        pprint(result)
    end)
end
```

## Adapting to other engines

Adapting the Nakama Defold client to other Lua-based engines should be as easy as providing a different engine module when configuring the Nakama client:

```lua
local myengine = require "nakama.engine.myengine"
local nakama = require "nakama.nakama"
local config = {
    engine = myengine,
}
local client = nakama.create_client(config)
```

The engine module must provide the following functions:

* `http(config, url_path, query_params, method, post_data, callback)` - Make HTTP request.
    * `config` - Config table passed to `nakama.create()`
    * `url_path` - Path to append to the base uri
    * `query_params` - Key-value pairs to use as URL query parameters
    * `method` - "GET", "POST"
    * `post_data` - Data to post
    * `callback` - Function to call with result (response)

* `socket_create(config, on_message)` - Create socket. Must return socket instance (table with engine specific socket state).
    * `config` - Config table passed to `nakama.create()`
    * `on_message` - Function to call when a message is sent from the server

* `socket_connect(socket, callback)` - Connect socket.
    * `socket` - Socket instance returned from `socket_create()`
    * `callback` - Function to call with result (ok, err)

* `socket_send(socket, message, callback)` - Send message on socket.
    * `socket` - Socket instance returned from `socket_create()`
    * `message` - Message to send
    * `callback` - Function to call with message returned as a response (message)
