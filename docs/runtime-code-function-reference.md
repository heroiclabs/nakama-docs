# Function Reference

The code runtime built into the server includes a module with functions to implement various logic and custom behavior. It is easy to define authoritative code and conditions on input received by clients.

## Nakama module

This module contains all the core gameplay APIs, all registration functions used at server startup, utilities for various codecs, and cryptographic primitives.

```lua
local nk = require("nakama")
```

!!! Note
    All code examples assume the "nakama" module has been imported.

### account

__account_get_id (user_id)__

Get all account information for a given user ID.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_id | string | User ID to fetch information for. Must be valid UUID. |

_Returns_

All account information including wallet, device IDs and more.

_Example_

```lua
local account = nk.account_get_id("8f4d52c7-bf28-4fcf-8af2-1d4fcf685592")
print(nk.json_encode(account.wallet))
```

---

__account_update_id (user_id, metadata, username, display_name, timezone, location, language, avatar_url)__

Update one or more users.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_id | string | User ID to be updated. Must be valid UUID. |
| metadata | table | Metadata to update. Use `nil` if it is not being updated. |
| username | string | Username to be set. Must be unique. Use empty string if not being updated. |
| display_name | string | Display name to be updated. Use `nil` if it is not being updated. |
| timezone | string | Timezone to be updated. Use `nil` if it is not being updated. |
| location | string | Location to be updated. Use `nil` if it is not being updated. |
| language | string | Lang tag to be updated. Use `nil` if it is not being updated. |
| avatar_url | string | User's avatar URL. Use `nil` if it is not being updated. |

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local metadata = {}
local status, err = pcall(nk.account_update_id, user_id, metadata)
if (not status) then
  print(("Account update error: %q"):format(err))
end
```

### aes128

__aes128_decrypt (input, key)__

AES decrypt input with the key. Key must be 16 bytes long.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which has been aes128 encrypted. |
| key | string | 16 bytes decryption key. |

_Returns_

The decrypted input.

_Example_

```lua
local plaintext = nk.aes128_decrypt("48656C6C6F20776F726C64", "goldenbridge_key")
```

---

__aes128_encrypt (input, key)__

AES encrypt input with the key. Key must be 16 bytes long.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be aes128 encrypted. |
| key | string | 16 bytes encryption key. |

_Returns_

The encrypted input.

_Example_

```lua
local cyphertext = nk.aes128_encrypt("48656C6C6F20776F726C64", "goldenbridge_key")
```

### authenticate

__authenticate_custom (id, username, create)__

Authenticate user and create a session token.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | Custom ID to use to authenticate the user. Must be between 6-128 characters. |
| username | string | Optional username. If left empty, one is generated. |
| create | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

```lua
local user_id, username, created = nk.authenticate_custom("48656C6C6F20776F726C64", "username", true)
```

---

__authenticate_device (id, username, create)__

Authenticate user and create a session token.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | Device ID to use to authenticate the user. Must be between 1 - 128 characters. |
| username | string | Optional username. If left empty, one is generated. |
| create | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

```lua
local user_id, username, created = nk.authenticate_device("48656C6C6F20776F726C64", "username", true)
```

---

__authenticate_email (email, password, username, create)__

Authenticate user and create a session token.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| email | string | Email address to use to authenticate the user. Must be between 10-255 characters. |
| password | string | Password to set - must be longer than 8 characters. |
| username | string | Optional username. If left empty, one is generated. |
| create | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

```lua
local user_id, username, created = nk.authenticate_email("email@example.com", "48656C6C6F20776F726C64", "username", true)
```

---

__authenticate_facebook (token, import, username, create)__

Authenticate user and create a session token.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| token | string | Facebook OAuth access token. |
| import | bool | Whether to import facebook friends after authenticated automatically. This is true by default. |
| username | string | Optional username. If left empty, one is generated. |
| create | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

```lua
local user_id, username, created = nk.authenticate_facebook("some-oauth-access-token", true, "username", true)
```

---

__authenticate_gamecenter (player_id, bundle_id, timestamp, salt, signature, public_key_url, username, create)__

Authenticate user and create a session token.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| player_id | string | |
| bundle_id | string | |
| timestamp | number | |
| salt | string | |
| signature | string | |
| public_key_url | string | |
| username | string | Optional username. If left empty, one is generated. |
| create | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

---

__authenticate_google (token, username, create)__

Authenticate user and create a session token.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| token | string | Google OAuth access token. |
| username | string | Optional username. If left empty, one is generated. |
| create | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

```lua
local user_id, username, created = nk.authenticate_google("some-oauth-access-token", "username", true)
```

---

__authenticate_steam (token, username, create)__

Authenticate user and create a session token.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| token | string | Steam token. |
| username | string | Optional username. If left empty, one is generated. |
| create | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

```lua
local user_id, username, created = nk.authenticate_steam("steam-token", "username", true)
```

---

__authenticate_token_generate (user_id, username)__

Generate a Nakama session token from a username. This is not the same as an authentication mechanism because a user does not get created and input is not checked against the database.

This is useful if you have an external source of truth where users are registered.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_id | string | User ID you'd like to use to generated the token. |
| username | string | Username information to embed in the token. This is mandatory. |
| expires_at | number | Number of seconds the token should be valid for. Optional, defaults to [server configured expiry time](install-configuration.md#session).

_Returns_

The session token created for the given user details.

_Example_

```lua
local token = nk.authenticate_token_generate("user_id", "username")
print(token)
```

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

---

__base64url_decode (input)__

Base 64 URL decode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base64 URL decoded. |

_Returns_

The base 64 URL decoded input.

_Example_

```lua
local decoded = nk.base64url_decode("SGVsbG8gd29ybGQ=")
print(decoded) -- outputs "Hello world"
```

---

__base64url_encode (input)__

Base 64 URL encode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base64 URL encoded. |

_Returns_

The base 64 URL encoded input.

_Example_

```lua
local encoded = nk.base64url_encode("Hello world")
print(encoded) -- outputs "SGVsbG8gd29ybGQ="
```

### bcrypt

__bcrypt_hash (input)__

Generate one-way hashed string using bcrypt.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be bcrypted. |

_Returns_

The hashed input.

_Example_

```lua
local hashed = nk.bcrypt_hash("Hello World")
print(hashed)
```

---

__bcrypt_compare (hash, plaintext)__

Compare hashed input against a plaintext input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| hash | string | The string that is already bcrypted. |
| plaintext | string | The string that is to be compared. |

_Returns_

True if they are the same, false otherwise.

_Example_

```lua
local is_same = nk.bcrypt_compare("$2a$04$bl3tac7Gwbjy04Q8H2QWLuUOEkpoNiAeTxazxi4fVQQRMGbMaUHQ2", "123456")
print(is_same) -- outputs true
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

__group_create (user_id, name, creator_id, lang, description, avatar_url, open, metadata, max_count)__

Setup a group with various configuration settings. The group will be created if they don't exist or fail if the group name is taken.

A user ID must be given as they'll be made group superadmin.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_id | string | The user ID to be associcated as the group superadmin. Mandatory field. |
| name | string | Group name, must be set and unique. |
| creator_id | string | The user ID to be associcated as creator. If not set, system user will be set. |
| lang | string | Group language. Will default to 'en'. |
| description | string | Group description, can be left empty. |
| avatar_url | string | URL to the group avatar, can be left empty. |
| open | bool | Whether the group is for anyone to join, or members will need to send invitations to join. Defaults to false. |
| metadata | table | Custom information to store for this group. |
| max_count | number | Maximum number of members to have in the group. Defaults to 100. |

_Example_

```lua
local metadata = { -- Add whatever custom fields you want.
  my_custom_field = "some value"
}

local user_id = "dcb891ea-a311-4681-9213-6741351c9994"
local creator_id = "dcb891ea-a311-4681-9213-6741351c9994"
local name = "Some unique group name"
local description = "My awesome group."
local lang = "en"
local open = true
local creator_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
local avatar_url = "url://somelink"
local maxMemberCount = 100

nk.group_create(user_id, name, creator_id, lang, description, avatar_url, open, metadata, maxMemberCount)
```

---

__group_update (group_id, name, creator_id, lang, description, avatar_url, open, metadata, max_count)__

Update a group with various configuration settings. The group which is updated can change some or all of its fields.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| group_id | string | The group ID to update. |
| name | string | Group name, can be empty if not changed. |
| creator_id | string | The user ID to be associcated as creator. Can be empty if not changed. |
| lang | string | Group language. Empty if not updated. |
| description | string | Group description, can be left empty if not updated. |
| avatar_url | string | URL to the group avatar, can be left empty if not updated. |
| open | bool | Whether the group is for anyone to join or not. Use `nil` if field is not being updated. |
| metadata | table | Custom information to store for this group. Use `nil` if field is not being updated. |
| max_count | number | Maximum number of members to have in the group. Use `0` if field is not being updated. |

_Example_

```lua
local metadata = {
  some_field = "some value"
}
group_id = "f00fa79a-750f-11e7-8626-0fb79f45ff97"
description = "An updated description."

nk.group_update(group_id, "", "", "", description, "", nil, metadata, 0)
```

---

__group_delete (group_id)__

Delete a group.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| group_id | string | The group ID to delete. |

_Example_

```lua
group_id = "f00fa79a-750f-11e7-8626-0fb79f45ff97"
nk.group_delete(group_id)
```

---

__user_groups_list (user_id)__

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
local groups = nk.user_groups_list(user_id)
for _, g in ipairs(groups)
do
  local msg = ("Group name %q with id %q"):format(g.name, g.id)
  print(msg)
end
```

---

__group_users_list (group_id)__

List all members, admins and superadmins which belong to a group. This also list incoming join requests too.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| group_id | string | The Id of the group who's members, admins and superadmins you want to list. |

_Returns_

The user information for members, admins and superadmins for the group. Also users who sent a join request as well.

_Example_

```lua
local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
local members = nk.group_users_list(group_id)
for _, m in ipairs(members)
do
  local msg = ("Member username %q has status %q"):format(m.username, m.state)
  print(msg)
end
```

### hmac

__hmac_sha256_hash (input, key)__

Create a 256 hash from input and key.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | Plaintext input to hash. |
| key | number | Hashing key. |

_Returns_

Hashed input using the key.

_Example_

```lua
local hash = nk.hmac_sha256_hash("encryptthis", "somekey")
print(hash)
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

### leaderboards

__leaderboard_create (id, authoritative, sort, operator, reset, metadata)__

Setup a new dynamic leaderboard with the specified ID and various configuration settings. The leaderboard will be created if it doesn't already exist, otherwise its configuration will *not* be updated.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The unique identifier for the new leaderboard. This is used by clients to submit scores. |
| authoritative | bool | Mark the leaderboard as authoritative which ensures updates can only be made via the Lua runtime. No client can submit a score directly. Optional. Default false. |
| sort | string | The sort order for records in the leaderboard; possible values are "asc" or "desc". Optional. Default "desc". |
| operator | string | The operator that determines how scores behave when submitted; possible values are "best", "set", or "incr". Optional. Default "best". |
| reset | string | The cron format used to define the reset schedule for the leaderboard. This controls when a leaderboard is reset and can be used to power daily/weekly/monthly leaderboards. Optional. |
| metadata | table | The metadata you want associated to the leaderboard. Some good examples are weather conditions for a racing game. Optional. |

_Example_

```lua
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
local authoritative = false
local sort = "desc"
local operator = "best"
local reset = "0 0 * * 1"
local metadata = {
  weather_conditions = "rain"
}
nk.leaderboard_create(id, authoritative, sort, operator, reset, metadata)
```

---

__leaderboard_delete (id)__

Delete a leaderboard and all scores that belong to it.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The unique identifier for the leaderboard to delete. Mandatory field. |

_Example_

```lua
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
nk.__leaderboard_delete(id)
```

---

__leaderboard_record_write (id, owner, username, score, subscore, metadata)__

Use the preconfigured operator for the given leaderboard to submit a score for a particular user.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The unique identifier for the leaderboard to submit to. Mandatory field. |
| owner | string | The owner of this score submission. Mandatory field. |
| username | string | The owner username of this score submission, if it's a user. Optional. |
| score | number | The score to submit. Optional. Default 0. |
| subscore | number | A secondary subscore parameter for the submission. Optional. Default 0. |
| metadata | table | The metadata you want associated to this submission. Some good examples are weather conditions for a racing game. Optional. |

_Example_

```lua
local metadata = {
  weather_conditions = "rain"
}
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
local owner = "4c2ae592-b2a7-445e-98ec-697694478b1c"
local username = "02ebb2c8"
local score = 10
local subscore = 0
nk.leaderboard_record_write(id, owner, username, score, subscore, metadata)
```

---

__leaderboard_record_delete (id, owner)__

Remove an owner's record from a leaderboard, if one exists.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The unique identifier for the leaderboard to delete from. Mandatory field. |
| owner | string | The owner of the score to delete. Mandatory field. |

_Example_

```lua
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
local owner = "4c2ae592-b2a7-445e-98ec-697694478b1c"
nk.__leaderboard_record_delete(id, owner)
```

---

__leaderboard_records_list (id, owners, limit, cursor)__

List records on the specified leaderboard, optionally filtering to only a subset of records by their owners. Records will be listed in the preconfigured leaderboard sort order.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The unique identifier of the leaderboard to list from. Mandatory field. |
| owners | table | Table array of owners to filter to. Optional. |
| limit | number | The maximum number of records to return, from 10 to 100. Optional. |
| cursor | string | A cursor used to fetch the next page when applicable. Optional. |

_Returns_

A page of leaderboard records, a list of owner leaderboard records (empty if the `owners` input parameter is not set), an optional next page cursor that can be used to retrieve the next page of records (if any), and an optional previous page cursor that can be used to retrieve the previous page of records (if any).

_Example_

```lua
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
local owners = {}
local limit = 10
local records, owner_records, next_cursor, prev_cursor = nk.__leaderboard_records_list(id, owners, limit)
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

### match

__match_create (module, params)__

Create a new authoritative realtime multiplayer match running on the given runtime module name. The given `params` are passed to the match's init hook.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| module | string | The name of an available runtime module that will be responsible for the match. |
| params | any | Any Lua value to pass to the match's init hook. Optional. |

_Returns_

(string) - The match ID of the newly created match. Clients can immediately use this ID to join the match.

_Example_

```lua
-- Assumes you've registered a runtime module with a path of "my/match/module.lua".
local module = "my.match.module"
local params = { some = "data" }
local match_id = nk.match_create(module, params)
```

---

__match_list (limit, authoritative, label, min_size, max_size)__

List currently running realtime multiplayer matches and optionally filter them by authoritative mode, label, and current participant count.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| limit | number | The maximum number of matches to list. Optional. Default 1. |
| authoritative | boolean | Boolean `true` if listing should only return authoritative matches, `false` to only return relayed matches, `nil` to return both. Optional. Default `nil`. |
| label | string | A label to filter authoritative matches by. Optional. Default `nil` meaning any label matches. |
| min_size | number | Inclusive lower limit of current match participants. Optional. |
| max_size | number | Inclusive upper limit of current match participants. Optional. |

_Example_

```lua
-- List at most 10 matches, not authoritative, and that
-- have between 2 and 4 players currently participating.
local limit = 10
local authoritative = false
local label = nil
local min_size = 2
local max_size = 4
local matches = nk.match_list(limit, authoritative, label, min_size, max_size)
for _, m in ipairs(matches)
do
  local message = ("found match with id: %q"):format(m.match_id)
  print(message)
end
```

### notifications

__notification_send (user_id, subject, content, code, sender_id, persistent)__

Send one in-app notification to a user. Have a look at the section on [in-app notifications](social-in-app-notifications.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_id | string | Notification recipient. Must be a valid UUID. |
| subject | string | Notification subject. Must be set. |
| content | table | Notification content. Must be set but can be an empty table. |
| code | number | Notification code to use. Must be equal or greater than 0. |
| sender_id | string | The sender of this notification. If left empty, it will be assumed that it is a system notification. |
| persistent | bool | Whether to record this in the database for later listing. Defaults to false. |

_Example_

```lua
local subject = "You've unlocked level 100!"
local content = nk.json_encode({
  reward_coins = 1000
})
local user_id = "4c2ae592-b2a7-445e-98ec-697694478b1c" -- who to send
local sender_id = "dcb891ea-a311-4681-9213-6741351c9994" -- who the message if from
local code = 101
local persistent = true

nk.notification_send(user_id, subject, content, code, sender_id, persistent)
```

---

__notifications_send (new_notifications)__

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
local sender_id = "dcb891ea-a311-4681-9213-6741351c9994" -- who the message if from
local code = 101

local new_notifications = {
  { subject = subject, content = content, sender_id = sender_id, user_id = user_id, code = code, persistent = true}
}
nk.notifications_send(new_notifications)
```

### register hooks

__register_matchmaker_matched (func)__

Registers a function that will be called when matchmaking finds opponents.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each matchmake completion. |

_Example_

```lua
local function my_func(context, matched_users)
  -- run some code

  for _, m in ipairs(matched_users)
  do
    print(m.presence["user_id"])
    print(m.presence["session_id"])
    print(m.presence["username"])
    print(m.presence["node"])

    for _, p in ipairs(m.properties)
    do
      print(p)
    end
  end
nk.register_matchmaker_matched(my_func)
```

For example a two persons's authoritative match can be created like this:

```lua
local function matchmaker_matched(context, matchmaker_users)
  if #matchmaker_users ~= 2 then
    return nil
  end

  if matchmaker_users[1].properties["mode"] ~= "authoritative" then
    return nil
  end
  if matchmaker_users[2].properties["mode"] ~= "authoritative" then
    return nil
  end

  return nk.match_create("match", {debug = true, expected_users = matchmaker_users})
end
nk.register_matchmaker_matched(matchmaker_matched)
```

Expected to return an authoritative match ID for a match ready to receive these users, or `nil` if the match should proceed through the peer-to-peer relayed mode.

---

__register_req_after (func, msgname)__

Register a function with the server which will be executed after every non-realtime message with the specified message name.

This can be used to apply custom logic to standard features in the server. Similar to the `register_before` function but it will not block the execution pipeline. The logic will be executed in parallel to any response message sent back to a client. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each `msgname` message. |
| msgname | string | The specific message name to execute the `func` function after. |

For message names, have a look at [this section](runtime-code-basics.md#message-names).

_Example_

```lua
local function my_func(context, payload)
  -- run some code
end
nk.register_req_after(my_func, "FriendsAdd")
```

---

__register_req_before (func, msgname)__

Register a function with the server which will be executed before every non-realtime message with the specified message name.

For example `register_req_before(somefunc, "FriendsAdd")` will execute the function before the Friend Add message is executed by the server's message pipeline. This can be used to apply custom conditions to standard features in the server. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each `msgname` message. |
| msgname | string | The specific message name to execute the `func` function before. |

For message names, have a look at [this section](runtime-code-basics.md#message-names).

!!! Note
    The `func` should pass the `payload` back as a return argument so the pipeline can continue to execute the standard logic. If you return `nil`, the server will stop processing that message. Any other return argument will result in an error.

_Example_

```lua
local function my_func(context, payload)
  -- run some code
  return payload -- important!
end
nk.register_req_before(my_func, "FriendsAdd")
```

---

__register_rt_after (func, msgname)__

Register a function with the server which will be executed after every realtime message with the specified message name.

This can be used to apply custom logic to standard features in the server. Similar to the `register_before` function but it will not block the execution pipeline. The logic will be executed in parallel to any response message sent back to a client. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each `msgname` message. |
| msgname | string | The specific message name to execute the `func` function after. |

For message names, have a look at [this section](runtime-code-basics.md#message-names).

_Example_

```lua
local function my_func(context, payload)
  -- run some code
end
nk.register_rt_after(my_func, "ChannelJoin")
```

---

__register_rt_before (func, msgname)__

Register a function with the server which will be executed before every realtime message with the specified message name.

For example `register_rt_before(somefunc, "ChannelJoin")` will execute the function before the Channel Join message is executed by the server's message pipeline. This can be used to apply custom conditions to standard features in the server. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each `msgname` message. |
| msgname | string | The specific message name to execute the `func` function before. |

For message names, have a look at [this section](runtime-code-basics.md#message-names).

!!! Note
    The `func` should pass the `payload` back as a return argument so the pipeline can continue to execute the standard logic. If you return `nil`, the server will stop processing that message. Any other return argument will result in an error.

_Example_

```lua
local function my_func(context, payload)
  -- run some code
  return payload -- important!
end
nk.register_rt_before(my_func, "ChannelJoin")
```

---

__register_rpc (func, id)__

Registers a function for use with client RPC to the server.

The ID can be any string identifier and is sent by the client. The ID is used to map the client RPC message to the specific function to execute. Have a look at the section on [runtime code basics](runtime-code-basics.md).

This function can also be used to register a HTTP endpoint within the server. Have a look at the [Server to server](runtime-code-basics.md#server-to-server) docs for more info.

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

### run once

__run_once (func)__

The runtime environment allows you to run code that must only be executed only once. This is useful if you have custom SQL queries that you need to perform (like creating a new table) or to register with third party services.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed only once. |

_Example_

```lua
nk.run_once(function(context)
  -- this is to create a system ID that cannot be used via a client.
  local system_id = context.env["SYSTEM_ID"]

  nk.sql_exec([[
INSERT INTO users (id, username)
VALUES ($1, $2)
ON CONFLICT (id) DO NOTHING
  ]], { system_id, "system_id" })
end)
```

### storage

__storage_read (object_ids)__

Fetch one or more records by their bucket/collection/keyname and optional user.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| object_ids | table | A table array of object identifiers to be fetched. |

_Returns_

A table array of object result set.

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local object_ids = {
  {collection = "save", key = "save1", user_id = user_id},
  {collection = "save", key = "save2", user_id = user_id},
  {collection = "save", key = "save3", user_id = user_id}
}
local objects = nk.storage_read(object_ids)
for _, r in ipairs(objects)
do
  local message = ("read: %q, write: %q, value: %q"):format(r.permission_read, r.permission_write, r.value)
  print(message)
end
```

---

__storage_list (user_id, collection, limit, cursor)__

You can list records in a collection and page through results. The records returned can be filtered to those owned by the user or "" for public records which aren't owned by a user.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_id | string | User ID or "" (empty string) for public records. |
| collection | string | Collection to list data from. |
| limit | number | Limit number of records retrieved. Min 10, Max 100. |
| cursor | string | Pagination cursor from previous result. If none available set to nil or "" (empty string). |

_Returns_

A table array of the records result set.

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local records = nk.storage_list(user_id "collection", 10, "")
for _, r in ipairs(records)
do
  local message = ("read: %q, write: %q, value: %q"):format(r.permission_read, r.permission_write, r.value)
  print(message)
end
```

---

__storage_delete (object_ids)__

Remove one or more objects by their collection/keyname and optional user.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| object_ids | table | A table array of object identifiers to be removed. |

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local friend_user_id = "8d98ee3f-8c9f-42c5-b6c9-c8f79ad1b820" -- friend ID.
local object_ids = {
  {collection = "save", key = "save1", user_id = user_id},
  {collection = "save", key = "save2", user_id = user_id},
  {collection = "public", key = "progress", user_id = friend_user_id}
}
nk.storage_delete(object_ids)
```

---

<!--
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
  {Bucket = "b", Collection = "c", Record = "r", UserId = user_id, Update = update_ops},
  {Bucket = "b", Collection = "c", Record = "r2", UserId = user_id, Update = update_ops, PermissionRead = 2, PermissionWrite = 1}
  {Bucket = "b", Collection = "c", Record = "r3", UserId = user_id, Update = update_ops, Version="*", PermissionRead = 1, PermissionWrite = 1}
}
nk.storage_update(record_keys)
```

---
-->

__storage_write (new_objects)__

Write one or more objects by their collection/keyname and optional user.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| new_objects | table | A table array of new objects to write. |

_Example_

```lua
local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some user ID.
local new_objects = {
  {collection = "save", key = "save1", user_id = user_id, value = {}},
  {collection = "save", key = "save2", user_id = user_id, value = {}},
  {collection = "save", key = "save3", user_id = user_id, value = {}, permission_read = 2, permission_write = 1},
  {collection = "save", key = "save3", user_id = user_id, value = {}, version="*", permission_read = 1, permission_write = 1}
}
nk.storage_write(new_objects)
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
local query = [[DELETE FROM leaderboard_record
                WHERE expires_at > 0 AND expires_at <= $1]]
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
-- Example fetching a list of usernames for the 100 most recetly signed up users.
local query = [[SELECT username, create_time
                FROM users
                ORDER BY create_time DESC
                LIMIT 100]]
local parameters = {}
local rows = nk.sql_query(query, parameters)

-- Example of processing the rows.
nk.logger_info("Selected " .. #rows .. " rows.")
for i, row in ipairs(rows) do
  nk.logger_info("Username " .. row.username .. " created at " .. row.create_time)
end
```

!!! Note
    The fields available in each `row` depend on the columns selected in the query such as `row.username` and `row.create_time` above.

### time

__time ()__

Get the current UTC time in milliseconds using the system wall clock.

_Returns_

A number representing the current UTC time in milliseconds.

_Example_

```lua
local utc_msec = nk.time()
```

### users

__users_get_id (user_ids)__

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
local users = nk.users_get_id(user_ids)
for _, u in ipairs(users)
do
  local message = ("username: %q, displayname: %q"):format(u.username, u.display_name)
  print(message)
end
```

---

__users_get_username (usernames)__

Fetch a set of users by their usernames.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| usernames | table | A table array of usernames to fetch. |

_Returns_

A table array of the user result set.

_Example_

```lua
local usernames = {"b7865e7e", "c048ba7a"}
local users = nk.users_get_username(usernames)
for _, u in ipairs(users)
do
  local message = ("id: %q, displayname: %q"):format(u.id, u.display_name)
  print(message)
end
```

---

__users_ban_id (user_ids)__

Ban one or more users by ID. These users will no longer be allowed to authenticate with the server until unbanned.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_ids | table | A table array of user IDs to ban. |

_Example_

```lua
local user_ids = {
  "3ea5608a-43c3-11e7-90f9-7b9397165f34",
  "447524be-43c3-11e7-af09-3f7172f05936"
}
nk.users_ban_id(user_ids)
```

---

__users_unban_id (user_ids)__

Unban one or more users by ID. These users will again be allowed to authenticate with the server.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_ids | table | A table array of user IDs to unban. |

_Example_

```lua
local user_ids = {
  "3ea5608a-43c3-11e7-90f9-7b9397165f34",
  "447524be-43c3-11e7-af09-3f7172f05936"
}
nk.users_unban_id(user_ids)
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

A string containing the equivalent 16-byte representation of the UUID. This function will also insert a new wallet ledger item into the user's wallet history that trackes this update.

_Example_

```lua
local uuid_string = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some uuid string.
local uuid_bytes = nk.uuid_string_to_bytes(uuid_string)
print(uuid_bytes)
```

### wallet

__wallet_update (user_id, changeset, metadata)__

Update a user's wallet with the given changeset.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_id | string | The ID of the user to update the wallet for. |
| changeset | table | The set of wallet operations to apply. |
| metadata | table | Additional metadata to tag the wallet update with. Optional. |

_Example_

```lua
local user_id = "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
local changeset = {
  coins = 10, -- Add 10 coins to the user's wallet.
  gems = -5   -- Remove 5 gems from the user's wallet.
}
local metadata = {}
nk.wallet_update(user_id, changeset, metadata)
```

---

__wallets_update (updates)__

Update one or more user wallets with individual changesets. This function will also insert a new wallet ledger item into each user's wallet history that trackes their update.

All updates will be performed atomically.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| updates | table | The set of user wallet update operations to apply. |

_Example_

```lua
local updates = {
  {
    user_id = "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592",
    changeset = {
      coins = 10, -- Add 10 coins to the user's wallet.
      gems = -5   -- Remove 5 gems from the user's wallet.
    },
    metadata = {}
  }
}
nk.wallets_update(updates)
```

---

__wallet_ledger_list (user_id)__

List all wallet updates for a particular user, from oldest to newest.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| user_id | string | The ID of the user to update the wallet for. |

_Returns_

A Lua table containing update operations with the following format:

```lua
{
  {
    id = "...",
    user_id = "...",
    create_time = 123,
    update_time = 123,
    changeset = {},
    metadata = {}
  }
}
```

_Example_

```lua
local user_id = "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
local updates = nk.wallet_ledger_list(user_id)
for _, u in ipairs(updates)
do
  local message = ("found wallet update with id: %q"):format(u.id)
  print(message)
end
```

---

__wallet_ledger_update (id, metadata)__

Update the metadata for a particular wallet update in a users wallet ledger history. Useful when adding a note to a transaction for example.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The ID of the wallet ledger item to update. |
| metadata | table | The new metadata to set on the wallet ledger item. |

_Returns_

The updated wallet ledger item as a Lua table with the following format:

```lua
{
  id = "...",
  user_id = "...",
  create_time = 123,
  update_time = 456,
  changeset = {},
  metadata = {}
}
```

_Example_

```lua
local id = "2745ba53-4b43-4f83-ab8f-93e9b677f33a"
local metadata = {
  updated = "metadata"
}
local u = nk.wallet_ledger_update(id, metadata
```
