# Godot client guide

This client is built in [GDScript](https://github.com/heroiclabs/nakama-godot) and is compatible with Godot Engine versions `3.1` and newer. To work with our Godot client you'll need to install and setup [Godot Engine](https://godotengine.org/download).

The client is available on the <a href="https://godotengine.org/asset-library/asset" target="\_blank">Godot Asset Library</a> and also on <a href="https://github.com/heroiclabs/nakama-godot/releases/latest" target="\_blank">GitHub releases</a>. You can download "Nakama.zip" which contains all source code required in the client.

For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-godot/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Tip "Contribute"
    The Godot client is <a href="https://github.com/heroiclabs/nakama-godot" target="\_blank">open source</a> on GitHub. Report issues and contribute code to help us improve it.

## Setup

When you've <a href="https://github.com/heroiclabs/nakama-godot/releases/latest" target="\_blank">downloaded</a> the "Nakama.zip" file you should extract its content into your Godot project folder to install it. In the editor add the `Nakama.gd` singleton (in `addons/com.heroiclabs.nakama/`) as an [autoload](https://docs.godotengine.org/en/stable/getting_started/step_by_step/singletons_autoload.html) via `Project -> Project Settings -> AutoLoad`.

The client object is used to interact with the server. You can create a client object via the `Nakama` autoload like this:

```gdscript
extends Node

var client : NakamaClient

func _ready():
    client = Nakama.create_client("defaultkey", "127.0.0.1", 7350, "http")
```

Godot uses [singletons (AutoLoad)](https://docs.godotengine.org/en/stable/getting_started/step_by_step/singletons_autoload.html) to store informations shared across multiple scenes. You can create your own AutoLoad script to share the client across all your scripts.

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user](user-accounts.md) with one of the [authenticate options](authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```gdscript
onready var client = Nakama.create_client("defaultkey", "127.0.0.1", 7350, "http")
```

&nbsp;&nbsp; 2\. Authenticate a user. By default the server will create a user if it doesn't exist.

```gdscript
func _ready():
    var email = "hello@example.com"
    var password = "somesupersecretpassword"
    # Use yield(client.function(), "completed") to wait for the request to complete. Hopefully, Godot will implement the await keyword in future versions.
    var session = yield(client.authenticate_email_async(email, password), "completed")
    print(session) # Print the session or exception
```

In the code above we use `authenticate_email_async` but for other authentication options have a look at the [code examples](authentication.md#authenticate). This [full example](#full-example) covers all our recommended steps.

## Sessions

When authenticated the server responds with an auth token (JWT) which contains useful properties and gets deserialized into a `NakamaSession` object.

```gdscript
    print(session.token) # raw JWT token
    print(session.user_id)
    print(session.username)
    print("Session has expired: %s" % session.expired)
    print("Session expires at: %s" % session.expire_time)
```

It is recommended to store the auth token from the session and check at startup if it has expired. If the token has expired you must reauthenticate. The expiry time of the token can be changed as a [setting](install-configuration.md#common-properties) in the server.

```gdscript
    var invalid_authtoken = "restored from somewhere"
    var restored_session = NakamaClient.restore_session(invalid_authtoken)
    if restored_session.expired:
        print("Session has expired. Must reauthenticate!")
```

## Send requests

The client includes lots of builtin APIs for various features of the game server. These are accessed with async methods. It can also call custom logic through RPC functions on the server.

All requests are sent with a session object which authorizes the client.

```gdscript
    var account = yield(client.get_account_async(session), "completed")
    print("User id: %s" % account.user.id)
    print("User username: '%s'" % account.user.username)
    print("Account virtual wallet: %s" % str(account.wallet))
```

Methods which end with "async" can use the `yield` keyword to asynchronously wait for the response. For more advice on yield and coroutines features in GDScript have look at the <a href="https://docs.godotengine.org/en/stable/getting_started/scripting/gdscript/gdscript_basics.html#coroutines-with-yield" target="\_blank">official documentation</a>.

The other sections of the documentation include more code examples on the client.

### Exceptions

Since Godot Engine does not support exceptions, whenever you make an async request via the client or socket, you can check if an error occurred via the `is_exception()` method.

```gdscript
    var invalid_session = NakamaSession.new() # An empty session, which will cause and error when we use it.
    var invalid_account = yield(client.get_account_async(invalid_session), "completed")
    print(invalid_account) # This will print the exception
    if invalid_account.is_exception():
        print("We got an exception")
```

## Socket messages

The client can create one or more sockets with the server. Each socket can have it's own event listeners registered for responses received from the server.

```gdscript
onready var client := Nakama.create_client("defaultkey", "127.0.0.1", 7350, "http")
onready var socket := Nakama.create_socket_from(client)

func _ready():
    # Authenticate with the client and receive the session as shown above.
    # ...
    socket.connect("connected", self, "_on_socket_connected")
    socket.connect("closed", self, "_on_socket_closed")
    socket.connect("received_error", self, "_on_socket_error")
    yield(socket.connect_async(session), "completed")
    print("Done")

func _on_socket_connected():
    print("Socket connected.")

func _on_socket_closed():
    print("Socket closed.")

func _on_socket_error(err):
    printerr("Socket error %s" % err)
```

You can connect to the server over a realtime socket connection to send and receive [chat messages](social-realtime-chat.md), get [notifications](social-in-app-notifications.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

To join a chat channel and receive messages:

```gdscript
    var room_name = "Heroes"
    socket.connect("received_channel_message", self, "_on_channel_message_received")
    var channel = yield(socket.join_chat_async(room_name, NakamaClient.ChannelType.Room), "completed")
    var content = {"hello": "world"} # Content MUST be a dictionary.
    var send_ack = yield(socket.write_chat_message_async(channel.id, content), "completed")
    print(send_ack)

func _on_channel_message_received(message):
    print("Received: %s", message)
    print("Message has channel id: %s" % message.channel_id)
    print("Message content: %s" % message.content)
```

There are more examples for chat channels [here](social-realtime-chat.md).

## Handle events

A socket object has event handlers which are called on various messages received from the server.

```gdscript
    socket.connect("received_channel_presence", self, "_on_channel_presence_received")

func _on_channel_presence_received(presence):
    print("Received presence: %s" % presence)
    for left in presence.leaves:
        print("User %s left" % left.username)
    for joined in presence.joins:
        print("User %s joined." % joined.username)
```

Signal handlers only need to be implemented for the features you want to use.

| Signals | Description |
| --------- | ----------- |
| connected | Receive an event when the socket connects. |
| closed | Handles an event for when the client is disconnected from the server. |
| received\_error | Receives events about server errors. |
| received\_notification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| received\_channel\_message | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| received\_channel\_presence | It handles join and leave events within [chat](social-realtime-chat.md). |
| received\_match\_state | Receives [realtime multiplayer](gameplay-multiplayer-realtime.md) match data. |
| received\_match\_presence | It handles join and leave events within [realtime multiplayer](gameplay-multiplayer-realtime.md). |
| received\_matchmaker\_matched | Received when the [matchmaker](gameplay-matchmaker.md) has found a suitable match. |
| received\_status\_presence | It handles status updates when subscribed to a user [status feed](social-status.md). |
| received\_stream\_presence | Receives [stream](advanced-streams.md) join and leave event. |
| received\_stream\_state | Receives [stream](advanced-streams.md) data sent by the server. |

## Logs and errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code. To log all messages sent by the client you can assign your custom logger with log level `DEBUG` to the `Nakama` singleton before creating the client.

```gdscript
    Nakama.logger = NakamaLogger.new("MyLogger", NakamaLogger.LOG_LEVEL.DEBUG)
    # Our logger will be automatically assigned to newly created clients and sockets.
    var my_client = Nakama.create_client("defaultkey", "127.0.0.1", 7350, "http") # Client will log all messages.
    var my_socket = Nakama.create_socket_from(client) # Socket will log all messages.
```

## HTML5 exports.

HTML5 exports should work out of the box but please keep in mind the [limitations](http://docs.godotengine.org/en/stable/getting_started/workflow/export/exporting_for_web.html#unimplemented-functionality) of that platform when writing your scripts.

## Full example

A small example on how to manage a session object with Godot engine and the client.

```gdscript
# MyClient.gd
extends Node

const STORE_FILE = "user://store.ini"
const STORE_SECTION = "nakama"
const STORE_KEY = "session"

var session : NakamaSession = null

onready var client := Nakama.create_client("defaultkey", "127.0.0.1", 7350, "http")

func _ready():
    var cfg = ConfigFile.new()
    cfg.load(STORE_FILE)
    var token = cfg.get_value(STORE_SECTION, STORE_KEY, "")
    if token:
        var restored_session = NakamaClient.restore_session(token)
        if restored_session.valid and not restored_session.expired:
            session = restored_session
            return
    var deviceid = OS.get_unique_id() # This is not supported by Godot in HTML5, use a different way to generate an id, or a different authentication option.
    session = yield(client.authenticate_device_async(deviceid), "completed")
    if not session.is_exception():
        cfg.set_value(STORE_SECTION, STORE_KEY, session.token)
        cfg.save(STORE_FILE)
    print(session._to_string())
```

You can add this script as an autoload via `Project -> Project Settings -> AutoLoad` and access the client from other scripts via `MyClient.client`. A similar approach might be used for sockets.

A collection of other code examples is available <a href="https://github.com/heroiclabs/nakama-godot/tree/master/snippets" target="\_blank">here</a>.
