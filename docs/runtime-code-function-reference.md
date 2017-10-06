# Function Reference

The code runtime built into the server includes a module with functions to implement various logic and custom behavior. It is easy to define authoritative code and conditions on input received by clients.

## Nakama module

This module contains all the core gameplay APIs, all registration functions used at server startup, utilities for various codecs, and cryptographic primitives.

```lua
local nk = require("nakama")
```

!!! Note
    All code examples assume the "nakama" module has been imported.

### base16

__base16_decode (input)__

Base 16 decode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base16 decoded. |

_Returns_

The base 16 decoded input.

_Example_

```lua
local decoded = nk.base16_decode("48656C6C6F20776F726C64")
print(decoded) -- outputs "Hello world"
```

---

__base16_encode (input)__

Base 16 encode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base16 encoded. |

_Returns_

The base 16 encoded input.

_Example_

```lua
local encoded = nk.base16_encode("Hello world")
print(encoded) -- outputs "48656C6C6F20776F726C64"
```

### base64

__base64_decode (input)__

Base 64 decode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base64 decoded. |

_Returns_

The base 64 decoded input.

_Example_

```lua
local decoded = nk.base64_decode("SGVsbG8gd29ybGQ=")
print(decoded) -- outputs "Hello world"
```

---

__base64_encode (input)__

Base 64 encode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base64 encoded. |

_Returns_

The base 64 encoded input.

_Example_

```lua
local encoded = nk.base64_encode("Hello world")
print(encoded) -- outputs "SGVsbG8gd29ybGQ="
```

### cron

__cron_next (expression, timestamp)__

Parses a CRON expression and a timestamp in UTC seconds, and returns the next matching timestamp in UTC seconds.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| expression | string | A valid CRON expression in standard format, for example "* * * * *". |
| timestamp  | number | A time value expressed as UTC seconds. |

_Returns_

The next UTC seconds timestamp that matches the given CRON expression, and is immediately after the given timestamp.

_Example_

```lua
-- Based on the current time, return the UTC seconds value representing the
-- nearest upcoming Monday at 00:00 UTC (midnight.)
local expr = "0 0 * * 1"
local ts = os.time()
local next = nk.cron_next(expr, ts)
```

### groups

__groups_create (new_groups)__

Setup one or more groups with various configuration settings. The groups will be created if they don't exist or fail if the group names are taken.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| new_groups | table | The Lua table array of new groups to create. |

_Example_

```lua
local metadata = { -- Add whatever custom fields you want.
  my_custom_field = "some value"
}
local group = {
  Name = "Some unique group name",
  Description = "My awesome group.",
  Lang = "en",
  Private = true,
  CreatorId = "4c2ae592-b2a7-445e-98ec-697694478b1c",
  AvatarUrl = "url://somelink",
  Metadata = metadata
}
local new_groups = { group }
nk.groups_create(new_groups)
```

__groups_update (update_groups)__

Update one or more groups with various configuration settings. The groups which are updated can change some or all of their fields.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| update_groups | table | A Lua table of groups to be updated. |

_Example_

```lua
local metadata = {
  some_field = "some value"
}
local group = {
  GroupId = "f00fa79a-750f-11e7-8626-0fb79f45ff97",
  Description = "An updated description.",
  Metadata = metadata
}
local update_groups = { group }
nk.groups_update(update_groups)
```

__groups_user_list (user_id)__

List all groups which a user belongs to and whether they've been accepted into the group or if it's an invite.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_id | string | The Id of the user who's groups you want to list. |

_Returns_

A list of groups for the user.

_Example_

```lua
local user_id = "64ef6cb0-7512-11e7-9e52-d7789d80b70b"
local groups = nk.groups_user_list(user_id)
for _, g in ipairs(groups)
do
  local msg = ("Group name %q with id %q"):format(g.Name, g.Id)
  print(msg)
end
```

__group_users_list (group_id)__

List all members and admins which belong to a group.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| group_id | string | The Id of the group who's members and admins you want to list. |

_Returns_

The members and admins for the group.

_Example_

```lua
local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
local members = nk.group_users_list(group_id)
for _, m in ipairs(members)
do
  local msg = ("Member handle %q has status %q"):format(m.Handle, m.Type)
  print(msg)
end
```

### http

__http_request (url, method, headers, content)__

Send a HTTP request and receive the result as a Lua table.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| url | string | The URL of the web resource to request. |
| method | string | The HTTP method verb used with the request. |
| headers | table | A table of headers used with the request. |
| content | string | The bytes to send with the request. |

_Returns_

`code, headers, body` - Multiple return values for the HTTP response.

_Example_

```lua
local url = "https://google.com/"
local method = "HEAD"
local headers = {
  ["Content-Type"] = "application/json",
  ["Accept"] = "application/json"
}
local content = nk.json_encode({}) -- encode table as JSON string
local success, code, headers, body = pcall(nk.http_request, url, method, headers, content)
if (not success) then
  nk.logger_error(("Failed %q"):format(code))
elseif (code >= 400) then
  nk.logger_error(("Failed %q %q"):format(code, body))
else
  nk.logger_info(("Success %q %q"):format(code, body))
end
```

### json

__json_decode (input)__

Decode the JSON input as a Lua table.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The JSON encoded input. |

_Returns_

A Lua table with the decoded JSON.

_Example_

```lua
local json = nk.json_decode('{"hello": "world"}')
print(json.hello)
```

---

__json_encode (input)__

Encode the input as JSON.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The input to encode as JSON. |

_Returns_

The encoded JSON string.

_Example_

```lua
local input = {["some"] = "json"}
local json = nk.json_encode(input)
print(json) -- outputs '{"some": "json"}'
```

### leaderboard

__leaderboard_create (id, sort, reset, metadata, authoritative)__

Setup a new dynamic leaderboard with the specified ID and various configuration settings. The leaderboard will be created if it doesn't already exist.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The unique identifier for the new leaderboard. This is used by clients to submit scores. |
| sort | string | The sort order for records in the leaderboard; possible values are "asc" or "desc". |
| reset | string | The cron format used to define the reset schedule for the leaderboard. This controls when a leaderboard is reset and can be used to power daily/weekly/monthly leaderboards. |
| metadata | table | The metadata you want associated to the leaderboard. Some good examples are weather conditions for a racing game. |
| authoritative | bool | Mark the leaderboard as authoritative which ensures updates can only be made via the Lua runtime. No client can submit a score directly. |

_Example_

```lua
local metadata = {
  weather_conditions = "rain"
}
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
nk.leaderboard_create(id, "desc", "0 0 * * 1", metadata, false)
```

---

__leaderboard_submit_set (id, value, owner, handle, lang, location, timezone, metadata)__

The set operator will submit the value and replace any current value for the owner.

__leaderboard_submit_best (id, value, owner, handle, lang, location, timezone, metadata)__

The best operator will check the new score is better than the current score and keep whichever value is best based on the sort order of the leaderboard. If no score exists this operator works like "set".

__leaderboard_submit_incr (id, value, owner, handle, lang, location, timezone, metadata)__

The increment operator will add the score value to the current score. If no score exists the new score will be added to 0.

__leaderboard_submit_decr (id, value, owner, handle, lang, location, timezone, metadata)__

The decrement operator will subtract the score value from the current score. If no score exists the new score will be subtracted from 0.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The unique identifier for the leaderboard to submit to. Mandatory field. |
| value | number | The value to update the leaderboard with. Mandatory field. |
| owner | string | The owner of this submission. Mandatory field. |
| handle | string | The owner handle of this submission. Optional. |
| lang | string | The language you want to associate with this submission. Optional. |
| location | string | The location you want to associate with this submission. Optional. |
| timezone | string | The timezone you want to associate with this submission. Optional. |
| metadata | table | The metadata you want associated to this submission. Some good examples are weather conditions for a racing game. |

_Example_

```lua
local metadata = {
  weather_conditions = "rain"
}
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
local owner_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
local handle = "02ebb2c8"
nk.leaderboard_submit_set(id, 10, owner_id, handle, "", "", "", metadata)
```

---

__leaderboard_records_list_user (id, owner, limit)__

List records on the specified leaderboard, automatically paginating down to where the given user is on this leaderboard. If the user has no record on the given leaderboard will return no records.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The unique identifier of the leaderboard to list from. Mandatory field. |
| owner | string | The record owner to find in the leaderboard. Mandatory field. |
| limit | number | The maximum number of records to return, from 10 to 100. Mandatory field. |

_Returns_

A page of leaderboard records and optionally a cursor that can be used to retrieve the next page, if any.

_Example_

```lua
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
local owner_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
local limit = 10
local records, cursor = nk.leaderboard_records_list_user(id, owner_id, limit)
```

---

__leaderboard_records_list_users (id, owners, limit)__

List records on the specified leaderboard, filtering to only the records owned by the given list of users. If one or more of the given users have no record on the given leaderboard they will be omitted from the results.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The unique identifier of the leaderboard to list from. Mandatory field. |
| owners | table | A set of owner IDs to retrieve records for. Mandatory field. |
| limit | number | The maximum number of records to return, from 10 to 100. Mandatory field. |
| cursor | string | A previous cursor value if paginating to the next page of results. Optional. |

_Returns_

A page of leaderboard records and optionally a cursor that can be used to retrieve the next page, if any.

_Example_

```lua
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
local owners = {
  "4c2ae592-b2a7-445e-98ec-697694478b1c",
  "f15526e8-6cf8-49a8-b4c8-0b1e4c32a1d7"
}
local limit = 10
local previous_cursor = nil
local records, cursor = nk.leaderboard_records_list_users(id, owner_ids, limit, previous_cursor)
```

### logger

__logger_error (message)__

Write an ERROR level message to the server logs.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| message | string | The message to write to server logs with ERROR level severity. |

_Returns_

(string) - The message which was written to the logs.

_Example_

```lua
local message = ("%q - %q"):format("hello", "world")
nk.logger_error(message)
```

---

__logger_info (message)__

Write an INFO level message to the server logs.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| message | string | The message to write to server logs with INFO level severity. |

_Returns_

(string) - The message which was written to the logs.

_Example_

```lua
local message = ("%q - %q"):format("hello", "world")
nk.logger_info(message)
```

---

__logger_warn (message)__

Write an WARN level message to the server logs.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| message | string | The message to write to server logs with WARN level severity. |

_Returns_

(string) - The message which was written to the logs.

_Example_

```lua
local message = ("%q - %q"):format("hello", "world")
nk.logger_warn(message)
```

### notifications

__notifications_send_id (new_notifications)__

Send one or more in-app notifications to a user. Have a look at the section on [in-app notifications](social-in-app-notifications.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| new_notifications | table | The Lua table array of notifications to send. |

_Example_

```lua
local subject = "You've unlocked level 100!"
local content = nk.json_encode({
  reward_coins = 1000
})
local user_id = "4c2ae592-b2a7-445e-98ec-697694478b1c" -- who to send
local code = 101

local new_notifications = {
  { Subject = subject, Content = content, UserId = user_id, Code = code, Persistent = true}
}
nk.notifications_send_id(new_notifications)
```



### register hooks

__register_after (func, msgname)__

Register a function with the server which will be executed after every message with the specified message name.

This can be used to apply custom logic to standard features in the server. Similar to the `register_before` function but it will not block the execution pipeline. The logic will be executed in parallel to any response message sent back to a client. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each `msgname` message. |
| msgname | string | The specific message name to execute the `func` function after. |

_Example_

```lua
local function my_func(context, payload)
  -- run some code
end
nk.register_after(my_func, "tfriendsadd")
```

---

__register_before (func, msgname)__

Register a function with the server which will be executed before every message with the specified message name.

For example `register_before(somefunc, "tfriendsadd")` will execute the function before the Friend Add message is executed by the server's message pipeline. This can be used to apply custom conditions to standard features in the server. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each `msgname` message. |
| msgname | string | The specific message name to execute the `func` function before. |

!!! Note
    The `func` should pass the `payload` back as a return argument so the pipeline can continue to execute the standard logic.

_Example_

```lua
local function my_func(context, payload)
  -- run some code
  return payload -- important!
end
nk.register_before(my_func, "tfriendsadd")
```

---

__register_http (func, path)__

Registers a HTTP endpoint within the server.

!!! Warning
    This should not be used to implement custom client functions instead have a look at `register_rpc`.

This can be useful to define web callbacks to handle various Ad networks. It can also be used to enable server to server communication to ease the integration of Nakama server into various server stacks. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each HTTP call. |
| path | string | The path that should be registered as a HTTP endpoint. |

!!! Note
    The `func` can pass `nil` or `table` back as a return argument which will determine the HTTP response code returned.

_Example_

```lua
local function my_func(context, payload)
  -- let's return the "context" as JSON back in the HTTP response body
  return context
end
nk.register_http(my_func, "/my_endpoint")
-- "my_func" will be registered at 'POST /runtime/my_endpoint'
```

You can send a request to the HTTP endpoint with JSON and responses will be returned in JSON.

```shell
curl -X POST http://127.0.0.1:7350/runtime/my_endpoint?key=defaultkey \
     -d '{"some": "data"}' \
     -H 'Content-Type: application/json' \
     -H 'Accept: application/json'
```

---

__register_rpc (func, id)__

Registers a function for use with client RPC to the server.

The ID can be any string identifier and is sent by the client. The ID is used to map the client RPC message to the specific function to execute. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each RPC message. |
| id | string | The unique identifier used to register the `func` function for RPC. |

!!! Note
    The `func` can pass `nil` or `string` back as a return argument which will returned as bytes in the RPC response.

_Example_

```lua
local function my_func(context, payload)
  -- run some code
end
nk.register_rpc(my_func, "my_func_id")
```

### storage

__storage_fetch (record_keys)__

Fetch one or more records by their bucket/collection/keyname and optional user.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| record_keys | table | A table array of record identifiers to be fetched. |

_Returns_

A table array of the records result set.

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local record_keys = {
  {Bucket = "mygame", Collection = "save", Record = "save1", UserId = user_id},
  {Bucket = "mygame", Collection = "save", Record = "save2", UserId = user_id},
  {Bucket = "mygame", Collection = "save", Record = "save3", UserId = user_id}
}
local records = nk.storage_fetch(record_keys)
for _, r in ipairs(records)
do
  local message = ("read: %q, write: %q, value: %q"):format(r.PermissionRead, r.PermissionWrite, r.Value)
  print(message)
end
```

---

__storage_list (user_id, bucket, collection, limit, cursor)__

You can list records in a collection and page through results. The records returned can be filtered to those owned by the user or "" for public records which aren't owned by a user.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_id | string | User ID or "" (empty string) for public records. |
| bucket | string | Bucket to list data from. |
| collection | string | Collection to list data from. |
| limit | number | Limit number of records retrieved. Min 10, Max 100. |
| cursor | string | Pagination cursor from previous result. If none available set to nil or "" (empty string). |

_Returns_

A table array of the records result set.

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local records = nk.storage_list(user_id, "bucket", "collection", 10, "")
for _, r in ipairs(records)
do
  local message = ("read: %q, write: %q, value: %q"):format(r.PermissionRead, r.PermissionWrite, r.Value)
  print(message)
end
```

---

__storage_remove (record_keys)__

Remove one or more records by their bucket/collection/keyname and optional user.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| record_keys | table | A table array of record identifiers to be removed. |

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local friend_user_id = "8d98ee3f-8c9f-42c5-b6c9-c8f79ad1b820" -- friend ID.
local record_keys = {
  {Bucket = "mygame", Collection = "save", Record = "save1", UserId = user_id},
  {Bucket = "mygame", Collection = "save", Record = "save2", UserId = user_id},
  {Bucket = "mygame", Collection = "public", Record = "progress", UserId = friend_user_id}
}
nk.storage_remove(record_keys)
```

---

__storage_update (record_keys)__

Update one or more records by their bucket/collection/keyname and optional user. Have a look at the section on [storage collections](storage-collections.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| record_keys | table | A table array of records to update. |

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local now = os.time() * 1000 -- current time converted for msec
local update_ops = {
  {Op = "init", Path = "/", Value = { progress = 1 }},
  {Op = "incr", Path = "/progress", Value = 1},
  {Op = "replace", Path = "/updated_at", Value = now}
}
local record_keys = {
  {Bucket = "b", Collection = "c", Record = "r", UserId = user_id, Update = update_ops}
}
nk.storage_update(record_keys)
```

---

__storage_write (new_records)__

Write one or more records by their bucket/collection/keyname and optional user.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| new_records | table | A table array of new records to write. |

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local new_records = {
  {Bucket = "mygame", Collection = "save", Record = "save1", UserId = user_id, Value = {}},
  {Bucket = "mygame", Collection = "save", Record = "save2", UserId = user_id, Value = {}},
  {Bucket = "mygame", Collection = "save", Record = "save3", UserId = user_id, Value = {}}
}
nk.storage_write(new_records)
```

### users

__users_ban (user_ids)__

Ban one or more users from the server.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_ids | table | A table array of user IDs to be banned. |

_Example_

```lua
local user_ids = {"4c2ae592-b2a7-445e-98ec-697694478b1c"}
local status, result = pcall(nk.users_ban, user_ids)
if (not status) then
  print(result)
end
```

---

__users_fetch_handle (user_handles)__

Fetch a set of users by handle.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_handles | table | A table array of user handles to fetch. |

_Returns_

A table array of the user result set.

_Example_

```lua
local user_handles = {"b7865e7e", "c048ba7a"}
local users = nk.users_fetch_handle(user_handles)
for _, u in ipairs(users)
do
  local message = ("id: %q, fullname: %q"):format(u.Id, u.Fullname)
  print(message)
end
```

---

__users_fetch_id (user_ids)__

Fetch one or more users by ID.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_ids | table | A table array of user IDs to fetch. |

_Returns_

A table array of the user result set.

_Example_

```lua
local user_ids = {
  "3ea5608a-43c3-11e7-90f9-7b9397165f34",
  "447524be-43c3-11e7-af09-3f7172f05936"
}
local users = nk.users_fetch_id(user_ids)
for _, u in ipairs(users)
do
  local message = ("handle: %q, fullname: %q"):format(u.Handle, u.Fullname)
  print(message)
end
```

---

__users_update (user_updates)__

Update one or more users.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_updates | table | The table array of users to update. |

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local user_updates = {
  { UserId = user_id, Metadata = {} }
}
local status, err = pcall(nk.users_update, user_updates)
if (not status) then
  print(("User update error: %q"):format(err))
end
```

### uuid

__uuid_v4 ()__

Generate a version 4 UUID in the standard 36-character string representation.

_Returns_

The generated version 4 UUID identifier.

_Example_

```lua
local uuid = nk.uuid_v4()
print(uuid)
```

---

__uuid_bytes_to_string (uuid_bytes)__

Convert the 16-byte raw representation of a UUID into the equivalent 36-character standard UUID string representation. Will raise an error if the input is not valid and cannot be converted.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| uuid_bytes | string | The UUID bytes to convert. |

_Returns_

A string containing the equivalent 36-character standard representation of the UUID.

_Example_

```lua
local uuid_bytes = "\78\196\241\38\63\157\17\231\132\239\183\193\130\179\101\33" -- some uuid bytes.
local uuid_string = nk.uuid_bytes_to_string(uuid_bytes)
print(uuid_string)
```

---

__uuid_string_to_bytes (uuid_string)__

Convert the 36-character string representation of a UUID into the equivalent 16-byte raw UUID representation. Will raise an error if the input is not valid and cannot be converted.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| uuid_string | string | The UUID string to convert. |

_Returns_

A string containing the equivalent 16-byte representation of the UUID.

_Example_

```lua
local uuid_string = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some uuid string.
local uuid_bytes = nk.uuid_string_to_bytes(uuid_string)
print(uuid_bytes)
```

### sql

!!! Note
    These functions allow your Lua scripts to run arbitrary SQL staments beyond the ones built into Nakama itself. It is your responsibility to manage the performance of these queries.

__sql_exec (query, parameters)__

Execute an arbitrary SQL query and return the number of rows affected. Typically an `INSERT`, `DELETE`, or `UPDATE` statement with no return columns.

| Param | Type | Description |
| ----- | ---- | ----------- |
| query | string | A SQL query to execute. |
| parameters | table | Arbitrary parameters to pass to placeholders in the query. |

_Returns_

A single number indicating the number of rows affected by the query.

_Example_

```lua
-- This example query deletes all expired leaderboard records.
local query = "DELETE FROM leaderboard_record WHERE expires_at > 0 AND expires_at <= $1"
local parameters = {os.time() * 1000}
local affected_rows_count = nk.sql_exec(query, parameters)
```

---

__sql_query (query, parameters)__

Execute an arbitrary SQL query that is expected to return row data. Typically a `SELECT` statement.

| Param | Type | Description |
| ----- | ---- | ----------- |
| query | string | A SQL query to execute. |
| parameters | table | Arbitrary parameters to pass to placeholders in the query. |

_Returns_

A Lua table containing the result rows in the format:
```lua
{
  {column1 = "value1", column2 = "value2", ...}, -- Row 1.
  {column1 = "value1", column2 = "value2", ...}, -- Row 2.
  ...
}
```

_Example_

```lua
-- This example fetches a list of handles for the 100 most recetly signed up users.
local query = "SELECT handle, updated_at FROM users ORDER BY created_at DESC LIMIT 100"
local parameters = {}
local rows = nk.sql_query(query, parameters)

-- Example of processing the rows.
nk.logger_info("Selected " .. #rows .. " rows.")
for i, row in ipairs(rows) do
  nk.logger_info("User handle " .. row.handle .. " created at " .. row.created_at)
end
```
