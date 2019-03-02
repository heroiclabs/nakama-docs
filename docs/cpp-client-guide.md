# C++ client guide

The official C++ client handles all communication in realtime with the server. It implements all features in the server and is written with C++11.

## Download

The client can be downloaded from <a href="https://github.com/heroiclabs/nakama-cpp/releases/latest" target="\_blank">GitHub releases</a>. You can download "nakama-cpp-$version.zip".

For upgrades you can see changes and enhancements in the <a href="https://github.com/heroiclabs/nakama-cpp/blob/master/CHANGELOG.md" target="\_blank">CHANGELOG</a> before you update to newer versions.

!!! Bug "Help and contribute"
    The C++ client is <a href="https://github.com/heroiclabs/nakama-cpp" target="\_blank">open source on GitHub</a>. Please report issues and contribute code to help us improve it.

## Setup

When you've downloaded the Nakama C++ archive and extracted it to `NAKAMA_CPP_SDK` folder, you should include it in your project.

We don't recommend to copy Nakama C++ SDK to your project because it's quite big in size (~1 Gb).

### Setup for Mac and iOS projects

1. Add `NAKAMA_CPP_SDK/include` in `Build Settings > Header Search Paths`
2. Add libs folder in `Build Settings > Library Search Paths`:
    - `NAKAMA_CPP_SDK/libs/ios` - for iOS
    - `NAKAMA_CPP_SDK/libs/mac` - for Mac
3. Add all `.a` files located in libs folder and `libresolv.9.tbd` in `General > Linked Frameworks and Libraries`

### Setup for Android projects

If you use `CMake` then see [Setup for CMake projects](#Setup-for-CMake-projects) section.

If you use `ndk-build` then add following to your `Android.mk` file:

```makefile
# add this to your module
LOCAL_STATIC_LIBRARIES += nakama-cpp

# add this at bottom of Android.mk file
$(call import-add-path, NAKAMA_CPP_SDK)
$(call import-module, nakama-cpp-android)
```

Android uses a permissions system which determines which platform services the application will request to use and ask permission for from the user. The client uses the network to communicate with the server so you must add the "INTERNET" permission.

```xml
<uses-permission android:name="android.permission.INTERNET"/>
```

### Setup for CMake projects

Add following to your `CMakeLists.txt` file:

```cmake
add_subdirectory(NAKAMA_CPP_SDK ${CMAKE_CURRENT_BINARY_DIR}/nakama-cpp)
target_link_libraries(${APP_NAME} ext_nakama-cpp)
```

### Setup for Visual Studio projects

In `Project Settings` add following:

1. Add `NAKAMA_CPP_SDK/include` in `C/C++ > General > Additional Include Directories`
2. Add libs folder in `Linker > General > Additional Library Directories`:
    - `NAKAMA_CPP_SDK/libs/win32/vc140` - for VS 2015
    - `NAKAMA_CPP_SDK/libs/win32/vc141` - for VS 2017
3. Add all `.lib` files located in libs folder in `Linker > Input > Additional Dependencies`

### Custom setup

- add define:
  - `NLOGS_ENABLED` - define it if you want to use Nakama logger. See [Logging](#logging) section
- add include directory: `$(NAKAMA_CPP_SDK)/include`
- add link directory: `$(NAKAMA_CPP_SDK)/libs/{platform}/{ABI}`
- add link libraries:
  - `nakama-cpp`
  - `grpc++`
  - `libprotobuf`
  - `gpr`
  - `grpc`
  - `cares`
  - `crypto`
  - `ssl`
  - `address_sorting`

For Windows:

- Add extension `.lib` to libs names e.g. `nakama-cpp.lib`
- To debug you must add `d` suffix to libs names e.g. `nakama-cppd.lib`

For Mac, iOS, Android and Linux:

- Add prefix `lib` and extension `.a` to libs names e.g. `libnakama-cpp.a`

For Mac and iOS:

- Add `libresolv.9.tbd` system library

## Usage

Include nakama header.

```cpp
#include "nakama-cpp/Nakama.h"
```

Use nakama namespace.

```cpp
using namespace Nakama;
```

The client object is used to execute all logic against the server.

```cpp
DefaultClientParameters parameters;
parameters.serverKey = "defaultkey";
parameters.host = "127.0.0.1";
parameters.port = 7349;
NClientPtr client = createDefaultClient(parameters);
```

!!! Note
    By default the client uses connection settings "127.0.0.1" and 7349 port to connect to a local Nakama server.

```cpp
// Quickly setup a client for a local server.
NClientPtr client = createDefaultClient(DefaultClientParameters());
```

## Tick

The `tick` method pumps requests queue and executes callbacks in your thread. You must call it periodically (recommended every 50ms) in your thread.

```cpp
client->tick();
if (rtClient)
    rtClient->tick();
```

Without this the default client and realtime client will not work, and you will not receive responses from the server.

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user](user-accounts.md) with one of the [authenticate options](authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```cpp
NClientPtr client = createDefaultClient(DefaultClientParameters());
```

&nbsp;&nbsp; 2\. Authenticate a user. By default Nakama will try and create a user if it doesn't exist.

!!! Tip
    It's good practice to cache a device identifier on Android when it's used to authenticate because they can change with device OS updates.

```cpp
auto loginFailedCallback = [](const NError& error)
{
};

auto loginSucceededCallback = [](NSessionPtr session)
{
};

std::string deviceId = "unique device id";

client->authenticateDevice(
        deviceId,
        opt::nullopt,
        opt::nullopt,
        loginSucceededCallback,
        loginFailedCallback);
```

In the code above we use `authenticateDevice()` but for other authentication options have a look at the [code examples](authentication.md#register-or-login).

## Sessions

When authenticated the server responds with an auth token (JWT) which contains useful properties and gets deserialized into a `NSession` object.

```cpp
std::cout << session->getAuthToken() << std::endl; // raw JWT token
std::cout << session->getUserId() << std::endl;
std::cout << session->getUsername() << std::endl;
std::cout << "Session has expired: " << session->isExpired() << std::endl;
std::cout << "Session expires at: " << session->getExpireTime() << std::endl;
```

It is recommended to store the auth token from the session and check at startup if it has expired. If the token has expired you must reauthenticate. The expiry time of the token can be changed as a [setting](install-configuration.md#common-properties) in the server.

A __full example__ class with all code above is [here](#full-c-example).

## Send requests

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](social-friends.md), join [groups](social-groups-clans.md), or submit scores in [leaderboards](gameplay-leaderboards.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

All requests are sent with a session object which authorizes the client.

```cpp
auto successCallback = [](const NAccount& account)
{
    std::cout << "user id : " << account.user.id << std::endl;
    std::cout << "username: " << account.user.username << std::endl;
    std::cout << "wallet  : " << account.wallet << std::endl;
};

client->getAccount(session, successCallback, errorCallback);
```

Have a look at other sections of documentation for more code examples.

## Realtime client

The client can create one or more realtime clients. Each realtime client can have it's own event listener registered for responses received from the server.

!!! Note
    The socket is exposed on a different port on the server to the client. You'll need to specify a different port here to ensure that connection is established successfully.

```cpp
int port = 7350; // different port to the main API port
bool createStatus = true; // if the server should show the user as online to others.
// define realtime client in your class as NRtClientPtr rtClient;
rtClient = client->createRtClient(port);
// define listener in your class as NRtDefaultClientListener listener;
listener.setConnectCallback([]()
{
    std::cout << "Socket connected" << std::endl;
});
rtClient->setListener(&listener);
rtClient->connect(session, createStatus);
```

Don't forget to call `tick` method. See [Tick](#tick) section for details.

You can use realtime client to send and receive [chat messages](social-realtime-chat.md), get [notifications](social-in-app-notifications.md), and [matchmake](gameplay-matchmaker.md) into a [multiplayer match](gameplay-multiplayer-realtime.md). You can also execute remote code on the server via [RPC](runtime-code-basics.md).

To join a chat channel and receive messages:

```cpp
listener.setChannelMessageCallback([](const NChannelMessage& message)
{
    std::cout << "Received a message on channel " << message.channel_id << std::endl;
    std::cout << "Message content: " << message.content << std::endl;
});

std::string roomName = "Heroes";

auto successJoinCallback = [this](NChannelPtr channel)
{
    std::cout << "joined chat: " << channel->id << std::endl;

    // content must be JSON
    std::string content = "{\"message\":\"Hello world\"}";

    rtClient->writeChatMessage(channel->id, content);
};

rtClient->joinChat(
            roomName,
            NChannelType::ROOM,
            {},
            {},
            successJoinCallback,
            errorCallback);
```

There are more examples for chat channels [here](social-realtime-chat.md).

## Handle events

A realtime client has event handlers which are called on various messages received from the server.

```cpp
listener.setStatusPresenceCallback([](const NStatusPresenceEvent& event)
{
    for (auto& presence : event.joins)
    {
        std::cout << "Joined User ID: " << presence.user_id << " Username: " << presence.username << " Status: " << presence.status << std::endl;
    }

    for (auto& presence : event.leaves)
    {
        std::cout << "Left User ID: " << presence.user_id << " Username: " << presence.username << " Status: " << presence.status << std::endl;
    }
});
```

Event handlers only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| onDisconnect | Handles an event for when the client is disconnected from the server. |
| onNotification | Receives live [in-app notifications](social-in-app-notifications.md) sent from the server. |
| onChannelMessage | Receives [realtime chat](social-realtime-chat.md) messages sent by other users. |
| onChannelPresence | It handles join and leave events within [chat](social-realtime-chat.md). |
| onMatchState | Receives [realtime multiplayer](gameplay-multiplayer-realtime.md) match data. |
| onMatchPresence | It handles join and leave events within [realtime multiplayer](gameplay-multiplayer-realtime.md). |
| onMatchmakerMatched | Received when the [matchmaker](gameplay-matchmaker.md) has found a suitable match. |
| onStatusPresence | It handles status updates when subscribed to a user [status feed](social-status.md). |
| onStreamPresence | Receives [stream](advanced-streams.md) join and leave event. |
| onStreamState | Receives [stream](advanced-streams.md) data sent by the server. |

### Logging

#### Initializing Logger

Client logging is off by default.

To enable logs output to console with debug logging level:

```cpp
NLogger::initWithConsoleSink(NLogLevel::Debug);
```

To enable logs output to custom sink with debug logging level:

```cpp
NLogger::init(sink, NLogLevel::Debug);
```

#### Using Logger

To log string with debug logging level:

```
NLOG_DEBUG("debug log");
```

formatted log:

```
NLOG(NLogLevel::Info, "This is string: %s", "yup I'm string");
NLOG(NLogLevel::Info, "This is int: %d", 5);
```

Changing logging level boundary:

```
NLogger::setLevel(NLogLevel::Debug);
```

`NLogger` behaviour depending on logging level boundary:

- `Debug` writes all logs.

- `Info` writes logs with `Info`, `Warn`, `Error` and `Fatal` logging level.

- `Warn` writes logs with `Warn`, `Error` and `Fatal` logging level.

- `Error` writes logs with `Error` and `Fatal` logging level.

- `Fatal` writes only logs with `Fatal` logging level.

!!! Note
    To use logging macroses you have to define `NLOGS_ENABLED`.

## Errors

The [server](install-configuration.md#log) and the client can generate logs which are helpful to debug code.

To enable client logs see [Initializing Logger](#initializing-logger) section.

In every request in the client you can set error callback. It will be called when request fails. The callback has `NError` structure which contains details of the error:

```cpp
auto errorCallback = [](const NError& error)
{
    // convert error to readable string
    std::cout << toString(error) << std::endl;

    // check error code
    if (error.code == ErrorCode::ConnectionError)
    {
        std::cout << "The server is currently unavailable. Check internet connection." << std::endl;
    }
};

client->getAccount(session, successCallback, errorCallback);
```

The client writes all errors to logger so you don't need to do this.

## Full C++ example

An example class used to manage a session with the C++ client.

```cpp
class NakamaSessionManager
{
public:
    NakamaSessionManager()
    {
        DefaultClientParameters parameters;

        _client = createDefaultClient(parameters);
    }

    void start(const string& deviceId)
    {
        // to do: read session token from your storage
        string sessionToken;

        if (!sessionToken.empty())
        {
            // Lets check if we can restore a cached session.
            auto session = restoreSession(sessionToken);

            if (!session->isExpired())
            {
                // Session was valid and is restored now.
                _session = session;
                return;
            }
        }

        auto successCallback = [this](NSessionPtr session)
        {
            _session = session;

            // to do: save session token in your storage
            std::cout << "session token: " << session->getAuthToken() << std::endl;
        };

        auto errorCallback = [](const NError& error)
        {
        };

        _client->authenticateDevice(deviceId, opt::nullopt, opt::nullopt, successCallback, errorCallback);
    }

protected:
    NClientPtr _client;
    NSessionPtr _session;
};
```

## Client reference

You can find the C++ Client Reference [here](https://heroiclabs.github.io/nakama-cpp/).
