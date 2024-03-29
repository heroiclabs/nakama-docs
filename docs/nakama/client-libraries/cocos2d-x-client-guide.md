# Cocos2d-x C++ Client Guide

The official C++ client handles all communication in realtime with the server. It implements all features in the server and is written with C++11.

## Download

The client SDK can be downloaded from [GitHub releases](https://github.com/heroiclabs/nakama-cocos2d-x/releases/latest). You can download "nakama-cocos2d-x-sdk_$version.zip".

For upgrades you can see changes and enhancements in the [CHANGELOG](https://github.com/heroiclabs/nakama-cocos2d-x/blob/master/CHANGELOG.md) before you update to newer versions.

!!! Bug "Help and contribute"
    The Cocos2d-x C++ client is [open source on GitHub](https://github.com/heroiclabs/nakama-cocos2d-x. Please report issues and contribute code to help us improve it.

## Setup

When you've downloaded the Nakama Cocos2d archive and extracted it to `NAKAMA_COCOS2D_SDK` folder, you should include it in your project.

We don't recommend to copy Nakama Cocos2d SDK to your project because it's quite big in size (~600 Mb).

### Copy NakamaCocos2d folder

Copy `NakamaCocos2d` folder from `NAKAMA_COCOS2D_SDK` to your `Classes` folder.

### Setup for Mac and iOS projects

1. Add `NAKAMA_COCOS2D_SDK/include` in `Build Settings > Header Search Paths`
2. Add libs folder in `Build Settings > Library Search Paths`:
    - `NAKAMA_COCOS2D_SDK/libs/ios` - for iOS
    - `NAKAMA_COCOS2D_SDK/libs/mac` - for Mac
3. Add all `.a` files located in libs folder to `General > Frameworks, Libraries, and Embedded Content`
4. Add `NAKAMA_COCOS2D_SDK/NakamaCocos2d` folder to your XCode project as group (not a reference).

### Setup for Android projects

If you use `CMake` then see [Setup for CMake projects](#setup-for-cmake-projects) section.

If you use `ndk-build` then add following to your `Android.mk` file:

```makefile
# add this to your module
LOCAL_STATIC_LIBRARIES += nakama-cpp

# add this after $(call import-add-path, $(LOCAL_PATH)/../../../cocos2d)
$(call import-add-path, NAKAMA_COCOS2D_SDK)

# add this after $(call import-module, cocos)
$(call import-module, nakama-cpp-android)
```

Android uses a permissions system which determines which platform services the application will request to use and ask permission for from the user. The client uses the network to communicate with the server so you must add the "INTERNET" permission.

```xml
<uses-permission android:name="android.permission.INTERNET"/>
```

### Setup for CMake projects

Open for edit your `CMakeLists.txt` file and find following existing code:
```cmake
# mark app complie info and libs info
set(all_code_files
        ${GAME_HEADER}
        ${GAME_SOURCE}
    )
```
add next code before:
```cmake
# Cocos2d Nakama sources
list(APPEND GAME_SOURCE
        Classes/NakamaCocos2d/NCocosWebSocket.cpp
        Classes/NakamaCocos2d/NCocosHTTP.cpp
        Classes/NakamaCocos2d/NCocosHelper.cpp
    )
# Cocos2d Nakama headers
list(APPEND GAME_HEADER
        Classes/NakamaCocos2d/NCocosWebSocket.h
        Classes/NakamaCocos2d/NCocosHTTP.h
        Classes/NakamaCocos2d/NCocosLogSink.h
        Classes/NakamaCocos2d/NCocosHelper.h
    )
```

At bottom of your `CMakeLists.txt` file add following:

```cmake
add_subdirectory(NAKAMA_COCOS2D_SDK ${CMAKE_CURRENT_BINARY_DIR}/nakama-cpp)
target_link_libraries(${APP_NAME} ext_nakama-cpp)
```

### Setup for Visual Studio projects

In `Project Settings` add following:

1. Add `NAKAMA_COCOS2D_SDK/include` to `C/C++ > General > Additional Include Directories`
2. Add folder to `Linker > General > Additional Library Directories`:
    - `NAKAMA_COCOS2D_SDK/libs/win32/v140` - for VS 2015 x86
    - `NAKAMA_COCOS2D_SDK/libs/win64/v140` - for VS 2015 x64
    - `NAKAMA_COCOS2D_SDK/libs/win32/v141` - for VS 2017 x86
    - `NAKAMA_COCOS2D_SDK/libs/win64/v141` - for VS 2017 x64
    - `NAKAMA_COCOS2D_SDK/libs/win32/v142` - for VS 2019 x86
    - `NAKAMA_COCOS2D_SDK/libs/win64/v142` - for VS 2019 x64
3. Add all `.lib` files located in above folder to `Linker > Input > Additional Dependencies`
4. Add sources from `Classes/NakamaCocos2d` to your Visual Studio project.

## Usage

Include nakama helper header.

```cpp
#include "NakamaCocos2d/NCocosHelper.h"
```

Initialize logger with debug logging level.

```cpp
using namespace Nakama;

NCocosHelper::init(NLogLevel::Debug);
```

The client object is used to execute all logic against the server.

```cpp
NClientParameters parameters;
parameters.serverKey = "defaultkey";
parameters.host = "127.0.0.1";
parameters.port = DEFAULT_PORT;
NClientPtr client = NCocosHelper::createDefaultClient(parameters);
```

!!! Note
    By default the client uses connection settings "127.0.0.1" and 7350 port to connect to a local Nakama server.

```cpp
// Quickly setup a client for a local server.
NClientPtr client = NCocosHelper::createDefaultClient(NClientParameters());
```

## Tick

The `tick` method pumps requests queue and executes callbacks in your thread. You must call it periodically (recommended every 50ms) in your thread.

```cpp
auto tickCallback = [this](float dt)
{
    client->tick();
    if (rtClient)
        rtClient->tick();
};

auto scheduler = Director::getInstance()->getScheduler();
scheduler->schedule(tickCallback, this, 0.05f /*sec*/, CC_REPEAT_FOREVER, 0, false, "nakama-tick");
```

Without this the default client and realtime client will not work, and you will not receive responses from the server.

## Authenticate

With a client object you can authenticate against the server. You can register or login a [user]((../concepts/user-accounts.md) with one of the [authenticate options](../concepts/authentication.md).

To authenticate you should follow our recommended pattern in your client code:

&nbsp;&nbsp; 1\. Build an instance of the client.

```cpp
NClientPtr client = NCocosHelper::createDefaultClient(NClientParameters());
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

In the code above we use `authenticateDevice()` but for other authentication options have a look at the [code examples](../concepts/authentication.md).

## Sessions

When authenticated the server responds with an auth token (JWT) which contains useful properties and gets deserialized into a `NSession` object.

```cpp
CCLOG("%s", session->getAuthToken().c_str()); // raw JWT token
CCLOG("%s", session->getUserId().c_str());
CCLOG("%s", session->getUsername().c_str());
CCLOG("Session has expired: %s", session->isExpired() ? "yes" : "no");
CCLOG("Session expires at: %llu", session->getExpireTime());
CCLOG("Session created at: %llu", session->getCreateTime());
```

It is recommended to store the auth token from the session and check at startup if it has expired. If the token has expired you must reauthenticate. The expiry time of the token can be changed as a [setting](../getting-started/configuration.md#common-properties) in the server.

A __full example__ class with all code above is [here](#full-cocos2d-x-c-example).

## Send requests

When a user has been authenticated a session is used to connect with the server. You can then send messages for all the different features in the server.

This could be to [add friends](../concepts/friends.md), join [groups](../concepts/groups-clans.md), or submit scores in [leaderboards](../concepts/leaderboards.md). You can also execute remote code on the server via [RPC](../server-framework/basics.md).

All requests are sent with a session object which authorizes the client.

```cpp
auto successCallback = [](const NAccount& account)
{
    CCLOG("user id : %s", account.user.id.c_str());
    CCLOG("username: %s", account.user.username.c_str());
    CCLOG("wallet  : %s", account.wallet.c_str());
};

client->getAccount(session, successCallback, errorCallback);
```

Have a look at other sections of documentation for more code examples.

## Realtime client

The client can create one or more realtime clients. Each realtime client can have it's own event listener registered for responses received from the server.

!!! Note
    The socket is exposed on a different port on the server to the client. You'll need to specify a different port here to ensure that connection is established successfully.

```cpp
bool createStatus = true; // if the server should show the user as online to others.
// define realtime client in your class as NRtClientPtr rtClient;
rtClient = NCocosHelper::createRtClient(client, DEFAULT_PORT);
// define listener in your class as NRtDefaultClientListener listener;
listener.setConnectCallback([]()
{
    CCLOG("Socket connected");
});
rtClient->setListener(&listener);
rtClient->connect(session, createStatus);
```

Don't forget to call `tick` method. See [Tick](#tick) section for details.

You can use realtime client to send and receive [chat messages](../concepts/realtime-chat.md), get [notifications](../concepts/in-app-notifications.md), and [matchmake](../concepts/matches.md) into a [multiplayer match](../concepts/client-relayed-multiplayer.md). You can also execute remote code on the server via [RPC](../server-framework/basics.md).

To join a chat channel and receive messages:

```cpp
listener.setChannelMessageCallback([](const NChannelMessage& message)
{
    CCLOG("Received a message on channel %s", message.channel_id.c_str());
    CCLOG("Message content: %s", message.content.c_str());
});

std::string roomName = "Heroes";

auto successJoinCallback = [this](NChannelPtr channel)
{
    CCLOG("joined chat: %s", channel->id.c_str());

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

There are more examples for chat channels [here](../concepts/realtime-chat.md).

## Handle events

A realtime client has event handlers which are called on various messages received from the server.

```cpp
listener.setStatusPresenceCallback([](const NStatusPresenceEvent& event)
{
    for (auto& presence : event.joins)
    {
        CCLOG("Joined User ID: %s Username: %s Status: %s", presence.user_id.c_str(), presence.username.c_str(), presence.status.c_str());
    }

    for (auto& presence : event.leaves)
    {
        CCLOG("Left User ID: %s Username: %s Status: %s", presence.user_id.c_str(), presence.username.c_str(), presence.status.c_str());
    }
});
```

Event handlers only need to be implemented for the features you want to use.

| Callbacks | Description |
| --------- | ----------- |
| onDisconnect | Received when the client is disconnected from the server. |
| onNotification | Receives live [in-app notifications](../concepts/in-app-notifications.md) sent from the server. |
| onChannelMessage | Receives [realtime chat](../concepts/realtime-chat.md) messages sent by other users. |
| onChannelPresence | Receives join and leave events within [chat](../concepts/realtime-chat.md). |
| onMatchState | Receives [realtime multiplayer](../concepts/client-relayed-multiplayer.md) match data. |
| onMatchPresence | Receives join and leave events within [realtime multiplayer](../concepts/client-relayed-multiplayer.md). |
| onMatchmakerMatched | Received when the [matchmaker](../concepts/matches.md) has found a suitable match. |
| onStatusPresence | Receives status updates when subscribed to a user [status feed](../concepts/status.md). |
| onStreamPresence | Receives [stream](../server-framework/streams.md) join and leave event. |
| onStreamState | Receives [stream](../server-framework/streams.md) data sent by the server. |

## Logging

Client logging is off by default.

To enable logs output to console with debug logging level:

```cpp
NCocosHelper::init(NLogLevel::Debug);
```

## Errors

The [server](../getting-started/configuration.md#log) and the client can generate logs which are helpful to debug code.

To enable client logs see [Logging](#logging) section.

In every request in the client you can set error callback. It will be called when request fails. The callback has `NError` structure which contains details of the error:

```cpp
auto errorCallback = [](const NError& error)
{
    // convert error to readable string
    CCLOGERROR("%s", toString(error).c_str());

    // check error code
    if (error.code == ErrorCode::ConnectionError)
    {
        CCLOG("The server is currently unavailable. Check internet connection.");
    }
};

client->getAccount(session, successCallback, errorCallback);
```

The client writes all errors to logger so you don't need to do this.

## Full Cocos2d-x C++ example

You can find the Cocos2d-x C++ example [here](https://github.com/heroiclabs/nakama-cocos2d-x/tree/master/example).

## Client reference

You can find the C++ Client Reference [here](https://heroiclabs.github.io/nakama-cpp/).
