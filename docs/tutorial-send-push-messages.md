# Send push messages

Push messages are a great complement to [in-app notifications](social-in-app-notifications.md) in Nakama. You can use push messages to re-engage users who have either closed or backgrounded your app.

There are many different push providers you can integrate into your client code and Nakama, but for this tutorial we'll use the One Signal service. They provide an HTTP API to register devices and send push messages to segments of users.

!!! Tip
    This tutorial covers an example One Signal integration using Nakama's Lua runtime. For Go consider using an [existing One Signal library](https://godoc.org/github.com/tbalthazar/onesignal-go).

## Setup

We can easily write a Lua module which provides the One Signal HTTP API as a set of functions which can be imported into other code but instead we'll import one written by the Heroic Labs team and maintained with the community. You can find the code in the <a href="https://github.com/heroiclabs/nakama-modules" target="\_blank">official repository</a> of modules.

[https://github.com/heroiclabs/nakama-modules/blob/master/modules/onesignal.lua](https://github.com/heroiclabs/nakama-modules/blob/master/modules/onesignal.lua)

Download and add the "onesignal.lua" file to the location you use for your Lua modules. You can put the files into whatever folder you like and specify the location when you run the server.

```
nakama --runtime.path "/some/path/dir/"
```

When your server is started you'll see the One Signal module loaded and recorded in the startup logs. In development it can be helpful to run the server with "--log.stdout" for log output in the console.

### Keys and credentials

With the module added we can import it into our own Lua module. Let's create a module called "pmessage.lua" which will handle all our usage with One Signal in this tutorial project.

The One Signal API requires both an API Key and an App Id be used to authenticate and communicate with their servers. You can find these credentials in their dashboard. Replace "someapikey" and "someappid" with your strings.

```lua
local ONESIGNAL_API_KEY = "someapikey"
local ONESIGNAL_APP_ID = "someappid"

local onesignal = require("onesignal").new(ONESIGNAL_API_KEY, ONESIGNAL_APP_ID)
```

## Register a user's device

Before push messages can be sent to devices they must be registered with the One Signal service.

A device identifier which must be obtained through Android/iOS/etc APIs on the handset provide a unique string used to identify the device when push messages are sent out. This identifier is used to create the device in One Signal.

!!! Note
    The REST API for One Signal does not recommend you <a href="https://documentation.onesignal.com/v3.0/reference#add-a-device" target="\_blank">register a device</a> on the server. The Lua module implements all functions available in the API for completeness but when registering devices it may be best to use their SDK.

```lua
--[[
  "device_type" can be one of:
  0 - ios
  1 - android
  2 - amazon
  3 - windowsphone
  ... etc.
]]--
local device_type = 0
-- "identifier" is the push identifier which must be sent from the client
local identifier = "platformspecificdeviceidentifier"
local language = "en"
local tags = {
  a = 1,
  foo = "bar"
}
onesignal:add_device(device_type, identifier, language, tags)
```

We'll register an RPC hook which can be called via clients and receives the device identifier as a JSON payload.

```lua
local nk = require("nakama")

--[[
  The payload input is expected to be structured as JSON:
  { "DeviceType": 1, "Identifier": "somevalue" }
]]--
local function register_push(context, payload)
  local json = nk.json_decode(payload)

  local dt = json.DeviceType
  local id = json.Identifier
  local success, result = pcall(onesignal.add_device, dt, id, "en", {})
  if (success) then
    -- store the push "player id" from One Signal in the current user metadata
    local metadata = { os_player_id = result.id }
    pcall(nk.account_update_id, context.user_id, metadata) -- ignore errors
  end
end

nk.register_rpc(register_push, "register_push")
```

In your project you can now send a JSON encoded RPC message from a client with a device identifier and trigger the RPC function which will register the device with the service. Have a look at the [example](runtime-code-basics.md#an-example-module) of a client RPC message which sends JSON.

## Send a push message

There's multiple ways to send push messages via One Signal. Messages can be queued and sent in the dashboard but can also be sent programmatically via the REST API.

### Send to Segments

A segment is a group of devices which are grouped together within the One Signal service. Segments have names which are used to determine which devices receive or are excluded from receiving a push message.

It'll be most common to send push messages to segments via the One Signal dashboard but it can also be done via Lua.

```lua
local contents = {
  en = "English message"
}
local headings = {
  en = "English title"
}
local included_segments = { "All" }
local filters = nil
local player_ids = nil
local params = {
  excluded_segments = { "Banned" }
}
onesignal:create_notification(
    contents, headings, included_segments, filters, player_ids, params)
```

### Send via Filters

Filters are used to specify included or excluded devices based on information within the service about a user or tags attached to their device. Filters can be used to send <a href="https://documentation.onesignal.com/v3.0/reference#section-send-to-users-based-on-filters" target="\_blank">highly targeted</a> push messages to devices.

```lua
local contents = {
  en = "English message"
}
local headings = {
  en = "English title"
}
local included_segments = nil
local filters = {
  { field = "tag", key = "level", relation = ">", value = "10" },
  { field = "amount_spent", relation = ">", value = "0" }
}
local player_ids = nil
local params = {}
onesignal:create_notification(
    contents, headings, included_segments, filters, player_ids, params)
```

### Send to Devices

Push messages can be sent to targeted devices with the identifier "player_id" which is returned when a device is added to the One Signal service. This is the most common use case of the One Signal module in a project.

As an example we'll retrieve the One Signal push identifier "os_player_id" which was stored for a user in their metadata with the [RPC code](#register-a-users-device) and create a notification which will be sent to those users.

```lua
local player_ids = {}
local user_ids = {
  "3ea5608a-43c3-11e7-90f9-7b9397165f34",
  "447524be-43c3-11e7-af09-3f7172f05936"
}
local users = nk.users_get_id(user_ids)
for _, u in ipairs(users)
do
  -- get the onesignal id for each user
  table.insert(player_ids, u.metadata.os_player_id)
end

local contents = {
  en = "English message"
}
local headings = {
  en = "English title"
}
local included_segments = nil
local filters = nil
local params = {}
onesignal:create_notification(
    contents, headings, included_segments, filters, player_ids, params)
```

## Notes

The official One Signal module may be updated occasionally to reflect changes in the REST API. You can subscribe to watch the [nakama-modules](https://github.com/heroiclabs/nakama-modules) code for updates.
