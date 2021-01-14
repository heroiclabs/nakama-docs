# Function Reference

The code runtime built into the server includes a module with functions to implement various logic and custom behavior. It is easy to define authoritative code and conditions on input received by clients.

## Nakama module

This module contains all the core gameplay APIs, all registration functions used at server startup, utilities for various codecs, and cryptographic primitives.

=== "Lua"
    ```lua
    local nk = require("nakama")
    ```

=== "Go"
    ```go
    import (
      "github.com/heroiclabs/nakama-common/runtime"
    )
    ```

!!! Note
    All Lua code examples assume the `"nakama"` module has been imported.

    All Go functions will have `nk runtime.NakamaModule` avaiable as a parameter that may be used to access server runtime functions. A `context` will also be supplied in function input arguments.

### account

__Get account__

Get all account information for a given user ID.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_id | `string` | string | User ID to fetch information for. Must be valid UUID. |

_Returns_

All account information including wallet, device IDs and more.

_Example_

=== "Lua"
    ```lua
    local account = nk.account_get_id("8f4d52c7-bf28-4fcf-8af2-1d4fcf685592")
    local wallet = account.wallet
    nk.logger_info(string.format("Wallet is: %s", nk.json_encode(wallet)))
    ```

=== "Go"
    ```go
    account, err := nk.AccountGetId(ctx, "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592")
    if err != nil {
      // Handle error.
    } else {
      logger.Info("Wallet is: %v", account.Wallet)
    }
    ```

---

__Get accounts__

Get all account information for all given user IDs.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_ids | `[]string` | table | A table array of user ids to fetch information for. Must be valid UUIDs. |

_Returns_

A table (array) of accounts.

_Example_

=== "Lua"
    ```lua
    local user_ids = {"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}
    local accounts = nk.accounts_get_id(user_ids)
    ```

=== "Go"
    ```go
    userIDs := []string{"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}
    accounts, err := nk.AccountsGetId(ctx, userIDs)
    if err != nil {
      logger.WithField("err", err).Error("Get accounts error.")
    } else {
      for _, account := range accounts {
        logger.Info("Wallet is: %v", account.Wallet)
      }
    }
    ```

---

__Update account__

Update a user account.

_Parameters_

!!! Note
    The order of parameters is different in Lua and Go. Check examples below

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_id | `string` | string |User ID for which the information is to be updated. Must be valid UUID. |
| metadata | `map[string]interface{}` | table | Metadata to update. Use `nil` if it is not being updated. |
| username | `string` | string | Username to be set. Must be unique. Use `""` (Go) or `nil` (Lua) if it is not being updated. |
| display_name | `string` | string | Display name to be updated. Use `""` (Go) or `nil` (Lua) if it is not being updated. |
| timezone | `string` | string | Timezone to be updated. Use `""` (Go) or `nil` (Lua) if it is not being updated. |
| location | `string` | string | Location to be updated. Use `""` (Go) or `nil` (Lua) if it is not being updated. |
| language | `string` | string | Lang tag to be updated. Use `""` (Go) or `nil` (Lua) if it is not being updated. |
| avatar_url | `string` | string | User's avatar URL. Use `""` (Go) or `nil` (Lua) if it is not being updated. |

_Example_

=== "Lua"
    ```lua
    local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- Some user ID.
    local metadata = {}
    local username = ""
    local display_name = nil
    local timezone = nil
    local location = nil
    local language = nil
    local avatar_url = nil
    nk.account_update_id(user_id, metadata, username, display_name, timezone, location, language, avatar_url)
    ```

=== "Go"
    ```go
    userID := "4ec4f126-3f9d-11e7-84ef-b7c182b36521" // Some user ID.
    username := ""
    metadata := make(map[string]interface{})
    displayName := ""
    timezone := ""
    location := ""
    langTag := ""
    avatarUrl := ""
    if err := nk.AccountUpdateId(ctx, userID, username, metadata, displayName, timezone, location, langTag, avatarUrl); err != nil {
      logger.WithField("err", err).Error("Account update error.")
    }
    ```

---

__Delete Account__

Delete an account.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_id | `string` | string | User ID to fetch information for. Must be valid UUID. |
| recorded | `bool` | bool | Whether to record this deletion in the database. By default this is set to false. |

_Example_

=== "Lua"
    ```lua
    nk.account_delete_id("8f4d52c7-bf28-4fcf-8af2-1d4fcf685592", false)
    ```

=== "Go"
    ```go
    if err := nk.AccountDeleteId(ctx, "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592", false); err != nil {
      logger.WithField("err", err).Error("Delete account error.")
    }
    ```

### aes128

__aes128_decrypt (input, key)__

AES-128 CFB decrypt input with the key. Key must be 16 bytes long. If a non-CFB mode of operation is required use the equivalent Go runtime functions instead.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which has been aes128 encrypted. |
| key | string | 16 bytes decryption key. |

_Returns_

The decrypted input.

_Example_

=== "Lua"
    ```lua
    local plaintext = nk.aes128_decrypt("48656C6C6F20776F726C64", "goldenbridge_key")
    ```

=== "Go"
    ```go
    // Use the standard Go crypto package.
    import "crypto/aes"
    ```

---

__aes128_encrypt (input, key)__

AES-128 CFB encrypt input with the key. Key must be 16 bytes long. If a non-CFB mode of operation is required use the equivalent Go runtime functions instead.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be aes128 encrypted. |
| key | string | 16 bytes encryption key. |

_Returns_

The encrypted input.

_Example_

=== "Lua"
    ```lua
    local cyphertext = nk.aes128_encrypt("48656C6C6F20776F726C64", "goldenbridge_key")
    ```

=== "Go"
    ```go
    // Use the standard Go crypto package.
    import "crypto/aes"
    ```

### authenticate

__Authenticate Custom__

Authenticate user and create a session token using a custom authentication managed by an external service or source not already supported by Nakama. Best suited for use with existing external identity services.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | Custom ID to use to authenticate the user. Must be between 6-128 characters. |
| username | `string` | string | Optional username. If left empty, one is generated. |
| create | `bool` | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

=== "Lua"
    ```lua
    local user_id, username, created = nk.authenticate_custom("48656C6C6F20776F726C64", "username", true)
    ```

=== "Go"
    ```go
    userid, username, created, err := nk.AuthenticateCustom(ctx, "48656C6C6F20776F726C64", "username", true)
    if err != nil {
      logger.WithField("err", err).Error("Authenticate custom error.")
    }
    ```

---

__Authenticate Device__

Authenticate user and create a session token using a device identifier.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | Device ID to use to authenticate the user. Must be between 1 - 128 characters. |
| username | `string` | string | Optional username. If left empty, one is generated. |
| create | `bool` | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

=== "Lua"
    ```lua
    local user_id, username, created = nk.authenticate_device("48656C6C6F20776F726C64", "username", true)
    ```

=== "Go"
    ```go
    userid, username, created, err := nk.AuthenticateDevice(ctx, "48656C6C6F20776F726C64", "username", true)
    if err != nil {
      logger.WithField("err", err).Error("Authenticate custom error.")
    }
    ```

---

__Authenticate Email__

Authenticate user and create a session token using an email address and password.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| email | `string` | string | Email address to use to authenticate the user. Must be between 10-255 characters. |
| password | `string` | string | Password to set - must be longer than 8 characters. |
| username | `string` | string | Optional username. If left empty, one is generated. |
| create | `bool` | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). Go function will also return error, if any, as fourth return value.

_Example_

=== "Lua"
    ```lua
    local user_id, username, created = nk.authenticate_email("email@example.com", "48656C6C6F20776F726C64", "username", true)
    ```

=== "Go"
    ```go
    userid, username, created, err := nk.AuthenticateEmail(ctx, "email@example.com", "48656C6C6F20776F726C64", "username", true)
    if err != nil {
      logger.WithField("err", err).Error("Authenticate email error.")
    }
    ```

---

__Authenticate Facebook__

Authenticate user and create a session token using a Facebook account token.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| token | `string` | string | Facebook OAuth access token. |
| import | `bool` | bool | Whether to import facebook friends after authenticated automatically. This is true by default. |
| username | `string` | string | Optional username. If left empty, one is generated. |
| create | `bool` | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

=== "Lua"
    ```lua
    local user_id, username, created = nk.authenticate_facebook("some-oauth-access-token", true, "username", true)
    ```

=== "Go"
    ```go
    userid, username, created, err := nk.AuthenticateFacebook(ctx, "some-oauth-access-token", true, "username", true)
    if err != nil {
      logger.WithField("err", err).Error("Authenticate facebook error.")
    }
    ```

---

__Authenticate Game Center__

Authenticate user and create a session token using Apple Game Center credentials.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| player_id | `string` | string | PlayerId provided by GameCenter. |
| bundle_id | `string` | string | BundleId of your app on iTunesConnect. |
| timestamp | `int64` | number | Timestamp at which Game Center authenticated the client and issued a signature. |
| salt | `string` | string | A random string returned by Game Center authentication on client. |
| signature | `string` | string | A signature returned by Game Center authentication on client. |
| public_key_url | `string` | string | A url to the publick key returned by Game Center authentication on client. |
| username | `string` | string | Optional username. If left empty, one is generated. |
| create | `bool` | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

=== "Lua"
    ```lua
    local user_id, username, created = nk.authenticate_gamecenter(player_id, bundle_id, timestamp, salt, signature, public_key_url, username, create)
    ```

=== "Go"
    ```go
    userid, username, created, err := nk.AuthenticateGameCenter(ctx, playerID, bundleID, timestamp, salt, signature, publicKeyUrl, username, create)
    if err != nil {
      logger.WithField("err", err).Error("Authenticate game center error.")
    }
    ```

---

__Authenticate Google__

Authenticate user and create a session token using a Google ID token.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| token | `string` | string | Google OAuth access token. |
| username | `string` | string | Optional username. If left empty, one is generated. |
| create | `bool` | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

=== "Lua"
    ```lua
    local user_id, username, created = nk.authenticate_google("some-id-token", "username", true)
    ```

=== "Go"
    ```go
    userid, username, created, err := nk.AuthenticateGoogle(ctx, "some-id-token", "username", true)
    if err != nil {
      logger.WithField("err", err).Error("Authenticate google error.")
    }
    ```

---

__Authenticate Steam__

Authenticate user and create a session token using a Steam account token.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| token | `string` | string | Steam token. |
| username | `string` | string | Optional username. If left empty, one is generated. |
| create | `bool` | bool | Create user if one didn't exist previously. By default this is set to true. |

_Returns_

The user's ID, username, and a boolean flag indicating if the account was just created (`true`) or already existed (`false`).

_Example_

=== "Lua"
    ```lua
    local user_id, username, created = nk.authenticate_steam("steam-token", "username", true)
    ```

=== "Go"
    ```go
    userid, username, created, err := nk.AuthenticateGoogle(ctx, "steam-token", "username", true)
    if err != nil {
      logger.WithField("err", err).Error("Authenticate steam error.")
    }
    ```

---

__Authentication token generator__

Generate a Nakama session token from a username. This is not the same as an authentication mechanism because a user does not get created and input is not checked against the database.

This is useful if you have an external source of truth where users are registered.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_id | `string` | string | User ID you'd like to use to generated the token. |
| username | `string` | string | Username information to embed in the token. This is mandatory. |
| expires_at | `number` | string | Number of seconds the token should be valid for. Optional, defaults to [server configured expiry time](install-configuration.md#session). |

_Returns_

The session token created for the given user details, an the expiry time of the token expressed as UTC seconds.

_Example_

=== "Lua"
    ```lua
    local token, exp = nk.authenticate_token_generate("user_id", "username")
    nk.logger_info(("Access token: %q, valid for %q seconds"):format(token, exp))
    ```

=== "Go"
    ```go
    token, validity, err := nk.AuthenticateTokenGenerate("user_id", "username", 0)
    if err != nil {
      logger.WithField("err", err).Error("Authenticate token generate error.")
    } else {
      logger.Info("Session token: %q, valid for %v seconds", token, validity)
    }
    ```

### base16

__Base 16 Decode__

Base 16 decode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base16 decoded. |

_Returns_

The base 16 decoded input.

_Example_

=== "Lua"
    ```lua
    local decoded = nk.base16_decode("48656C6C6F20776F726C64")
    nk.logger_info(decoded) -- outputs "Hello world".
    ```

=== "Go"
    ```go
    // Use the standard Go encoding package.
    import "encoding/hex"
    ```

---

__Base 16 encode__

Base 16 encode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base16 encoded. |

_Returns_

The base 16 encoded input.

_Example_

=== "Lua"
    ```lua
    local encoded = nk.base16_encode("Hello world")
    nk.logger_info(encoded) -- outputs "48656C6C6F20776F726C64".
    ```

=== "Go"
    ```go
    // Use the standard Go encoding package.
    import "encoding/hex"
    ```

### base64

__Base64 Decode__

Base 64 decode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base64 decoded. |

_Returns_

The base 64 decoded input.

_Example_

=== "Lua"
    ```lua
    local decoded = nk.base64_decode("SGVsbG8gd29ybGQ=")
    nk.logger_info(decoded) -- outputs "Hello world".
    ```

=== "Go"
    ```go
    // Use the standard Go encoding package.
    import "encoding/base64"
    ```

---

__Base64 Encode__

Base 64 encode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base64 encoded. |

_Returns_

The base 64 encoded input.

_Example_

=== "Lua"
    ```lua
    local encoded = nk.base64_encode("Hello world")
    nk.logger_info(encoded) -- outputs "SGVsbG8gd29ybGQ=".
    ```

=== "Go"
    ```go
    // Use the standard Go encoding package.
    import "encoding/base64"
    ```

---

__Base64 URL Decode__

Base 64 URL decode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base64 URL decoded. |

_Returns_

The base 64 URL decoded input.

_Example_

=== "Lua"
    ```lua
    local decoded = nk.base64url_decode("SGVsbG8gd29ybGQ=")
    nk.logger_info(decoded) -- outputs "Hello world".
    ```

=== "Go"
    ```go
    // Use the standard Go encoding package.
    import "encoding/base64"
    ```

---

__Base64 URL Encode__

Base 64 URL encode the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be base64 URL encoded. |

_Returns_

The base 64 URL encoded input.

_Example_

=== "Lua"
    ```lua
    local encoded = nk.base64url_encode("Hello world")
    nk.logger_info(encoded) -- outputs "SGVsbG8gd29ybGQ=".
    ```

=== "Go"
    ```go
    // Use the standard Go encoding package.
    import "encoding/base64"
    ```

### bcrypt

__BCrypt Hash__

Generate one-way hashed string using bcrypt.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The string which will be bcrypted. |

_Returns_

The hashed input.

_Example_

=== "Lua"
    ```lua
    local hashed = nk.bcrypt_hash("Hello World")
    nk.logger_info(hashed)
    ```

=== "Go"
    ```go
    // Use the standard Go crypto package.
    import "golang.org/x/crypto/bcrypt"
    ```

---

__BCrypt Compare__

Compare hashed input against a plaintext input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| hash | string | The string that is already bcrypted. |
| plaintext | string | The string that is to be compared. |

_Returns_

True if they are the same, false otherwise.

_Example_

=== "Lua"
    ```lua
    local is_same = nk.bcrypt_compare("$2a$04$bl3tac7Gwbjy04Q8H2QWLuUOEkpoNiAeTxazxi4fVQQRMGbMaUHQ2", "123456")
    nk.logger_info(is_same) -- outputs true.
    ```

=== "Go"
    ```go
    // Use the standard Go crypto package.
    import "golang.org/x/crypto/bcrypt"
    ```

### cron

__Cron Next__

Parses a CRON expression and a timestamp in UTC seconds, and returns the next matching timestamp in UTC seconds.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| expression | string | A valid CRON expression in standard format, for example "* * * * *". |
| timestamp  | number | A time value expressed as UTC seconds. |

_Returns_

The next UTC seconds timestamp that matches the given CRON expression, and is immediately after the given timestamp.

_Example_

=== "Lua"
    ```lua
    -- Based on the current time, return the UTC seconds value representing the
    -- nearest upcoming Monday at 00:00 UTC (midnight.)
    local expr = "0 0 * * 1"
    local ts = os.time()
    local next = nk.cron_next(expr, ts)
    ```

=== "Go"
    ```go
    // Use a Go CRON package, for example:
    import "github.com/robfig/cron"
    ```

### friends

__Friends List__

List all friends, invites, invited, and blocked which belong to a user.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_id | `string` | string | The ID of the user who's friends, invites, invited, and blocked you want to list. |
| limit | `int` | number | The number of friends to retrieve in this page of results. No more than 1000 limit allowed per result. |
| state | `int` | number | The state of the friendship with the user. If unspecified this returns friends in all states for the user. |
| cursor | `string` | string | The cursor returned from a previous listing request. Used to obtain the next page of results. |

_Returns_

The user information for users of the current user who're friends.

_Example_

=== "Lua"
    ```lua
    local user_id = "b1aafe16-7540-11e7-9738-13777fcc7cd8"
    local limit = 100
    local state = nil -- optional
    local cursor = nil -- optional

    local friends = nk.friends_list(user_id, limit, state, cursor)
    for _, m in ipairs(friends)
    do
      -- States are: friend(0), invite_sent(1), invite_received(2), blocked(3)
      local msg = string.format("Friend username %q has state %q", m.user.username, m.state)
      nk.logger_info(msg)
    end
    ```

=== "Go"
    ```go
    userID := "b1aafe16-7540-11e7-9738-13777fcc7cd8"
    limit := 100
    state := 0
    cursor := ""

    friends, err := nk.FriendsList(ctx, userID, limit, state, cursor)
    if err != nil {
      logger.WithField("err", err).Error("nk.FriendsList error.")
    } else {
      for _, friend := range friends {
        // States are: friend(0), invite_sent(1), invite_received(2), blocked(3)
        logger.Info("Friend username %s has state %d", friend.GetUser().Username, friend.GetState())
      }
    }
    ```

---

### groups

__Group Create__

Setup a group with various configuration settings. The group will be created if they don't exist or fail if the group name is taken.

A user ID must be given as they'll be made group superadmin.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_id | `string` | string | The user ID to be associcated as the group superadmin. Mandatory field. |
| name | `string` | string | Group name, must be set and unique. |
| creator_id | `string` | string | The user ID to be associcated as creator. If not set, system user will be set. |
| lang | `string` | string | Group language. Will default to 'en'. |
| description | `string` | string | Group description, can be left empty. |
| avatar_url | `string` | string | URL to the group avatar, can be left empty. |
| open | `bool` | bool | Whether the group is for anyone to join, or members will need to send invitations to join. Defaults to false. |
| metadata | `map[string]interface{}` | table | Custom information to store for this group. |
| max_count | `int` | number | Maximum number of members to have in the group. Defaults to 100. |

_Example_

=== "Lua"
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

=== "Go"
    ```go
    metadata := map[string]interface{}{ // Add whatever custom fields you want.
      "my_custom_field": "some value",
    }

    userID := "dcb891ea-a311-4681-9213-6741351c9994"
    creatorID := "dcb891ea-a311-4681-9213-6741351c9994"
    name := "Some unique group name"
    description := "My awesome group."
    langTag := "en"
    open := true
    avatarURL := "url://somelink"
    maxCount := 100

    if group, err := nk.GroupCreate(ctx, userID, name, creatorID, langTag, description, avatarURL, open, metadata, maxCount); err != nil {
      logger.WithField("err", err).Error("Group create error.")
    }
    ```

---

__Group Delete__

Delete a group.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| group_id | `string` | string | The group ID to delete. |

_Example_

=== "Lua"
    ```lua
    local group_id = "f00fa79a-750f-11e7-8626-0fb79f45ff97"
    nk.group_delete(group_id)
    ```

=== "Go"
    ```go
    groupID := "f00fa79a-750f-11e7-8626-0fb79f45ff97"
    if group, err := nk.GroupDelete(ctx, groupID); err != nil {
      logger.WithField("err", err).Error("Group delete error.")
    }
    ```

---

__Group Update__

Update a group with various configuration settings. The group which is updated can change some or all of its fields.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| group_id | `string` | string | The group ID to update. |
| name | `string` | string | Group name, can be empty if not changed. |
| creator_id | `string` | string | The user ID to be associcated as creator. Can be empty if not changed. |
| lang | `string` | string | Group language. Empty if not updated. |
| description | `string` | string | Group description, can be left empty if not updated. |
| avatar_url | `string` | string | URL to the group avatar, can be left empty if not updated. |
| open | `bool` | bool | Whether the group is for anyone to join or not. Use `nil` if field is not being updated. |
| metadata | `map[string]interface{}` | table | Custom information to store for this group. Use `nil` if field is not being updated. |
| max_count | `int` | number | Maximum number of members to have in the group. Use `0` if field is not being updated. |

_Example_

=== "Lua"
    ```lua
    local metadata = {
      some_field = "some value"
    }
    group_id = "f00fa79a-750f-11e7-8626-0fb79f45ff97"
    description = "An updated description."

    nk.group_update(group_id, "", "", "", description, "", nil, metadata, 0)
    ```

=== "Go"
    ```go
    metadata := map[string]interface{}{ // Add whatever custom fields you want.
      "my_custom_field": "some value",
    }

    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    description := "An updated description."

    if err := nk.GroupUpdate(ctx, groupID, "", "", "", description, "", true, metadata, 0); err != nil {
      logger.WithField("err", err).Error("Group update error.")
    }
    ```

---

__Group Users List__

List all members, admins and superadmins which belong to a group. This also list incoming join requests too.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| group_id | `string` | string | The Id of the group who's members, admins and superadmins you want to list. |

_Returns_

The user information for members, admins and superadmins for the group. Also users who sent a join request as well.

_Example_

=== "Lua"
    ```lua
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local members = nk.group_users_list(group_id)
    for _, m in ipairs(members)
    do
      local msg = string.format("Member username %q has state %q", m.user.username, m.state)
      nk.logger_info(msg)
    end
    ```

=== "Go"
    ```go
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"

    if groupUserList, err := nk.GroupUsersList(ctx, groupID); err != nil {
      logger.WithField("err", err).Error("Group users list error.")
    } else {
      for _, member := range groupUserList {
        // States are => 0: Superadmin, 1: Admin, 2: Member, 3: Requested to join
        logger.Info("Member username %s has state %d", member.GetUser().Username, member.GetState())
      }
    }
    ```

---

__Group User Join__

Join a group for a particular user.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| group_id | `string` | string | The Id of the group to join. |
| user_id | `string` | string | The user ID to add to this group. |
| username | `string` | string | The username of the user to add to this group. |

_Example_

=== "Lua"
    ```lua
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_id = "9a51cf3a-2377-11eb-b713-e7d403afe081"
    local username = "myusername"

    nk.group_user_join(group_id, user_id, username)
    ```

=== "Go"
    ```go
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userID := "9a51cf3a-2377-11eb-b713-e7d403afe081"
    username := "myusername"

    if err := nk.GroupUserJoin(ctx, groupID, userID, username); err != nil {
      logger.WithField("err", err).Error("Group user join error.")
    }
    ```

---

__Group User Leave__

Leave a group for a particular user.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| group_id | `string` | string | The Id of the group to leave. |
| user_id | `string` | string | The user ID to leave from this group. |
| username | `string` | string | The username of the user to leave from this group. |

_Example_

=== "Lua"
    ```lua
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_id = "9a51cf3a-2377-11eb-b713-e7d403afe081"
    local username = "myusername"

    nk.group_user_leave(group_id, user_id, username)
    ```

=== "Go"
    ```go
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userID := "9a51cf3a-2377-11eb-b713-e7d403afe081"
    username := "myusername"

    if err := nk.GroupUserLeave(ctx, groupID, userID, username); err != nil {
      logger.WithField("err", err).Error("Group user leave error.")
    }
    ```

---

__Group Users Add__

Add users to a group.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| group_id | `string` | string | The Id of the group that you want to add users into. |
| user_ids | `[]string` | table | A table array of user ids to add. |

_Example_

=== "Lua"
    ```lua
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_ids = {"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    nk.group_users_add(group_id, user_ids)
    ```

=== "Go"
    ```go
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userIDs := []string{"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    if err := nk.GroupUsersAdd(ctx, groupID, userIDs); err != nil {
      logger.WithField("err", err).Error("Group users add error.")
    }
    ```

---

__Group Users Kick__

Kick users from a group.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| group_id | `string` | string | The Id of the group who's members, admins and superadmins you want to list. |
| user_ids | `[]string` | table | A table array of user ids to kick. |

_Example_

=== "Lua"
    ```lua
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_ids = {"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    nk.group_users_kick(group_id, userna)
    ```

=== "Go"
    ```go
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userIds := []string{"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    if err := nk.GroupUsersKick(ctx, groupID, userIds); err != nil {
      logger.WithField("err", err).Error("Group users kick error.")
    }
    ```

---

__Group Users Promote__

Promote users in a group.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| group_id | `string` | string | The Id of the group who's members and admins you want to promote. |
| user_ids | `[]string` | table | A table array of user ids to promote. |

_Example_

=== "Lua"
    ```lua
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_ids = {"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    nk.group_users_promote(group_id, user_ids)
    ```

=== "Go"
    ```go
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userIDs := []string{"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    if err := nk.GroupUsersPromote(ctx, groupID, userIDs); err != nil {
      logger.WithField("err", err).Error("Group users promote error.")
    }
    ```

---

__Group Users Demote__

Demote users in a group.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| group_id | `string` | string | The Id of the group who's members, admins and superadmins you want to demote. |
| user_ids | `[]string` | table | A table array of user ids to demote. |

_Example_

=== "Lua"
    ```lua
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_ids = {"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    nk.group_users_demote(group_id, user_ids)
    ```

=== "Go"
    ```go
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userIds := []string{"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    if err := nk.GroupUsersDemote(ctx, groupID, userIDs); err != nil {
      logger.WithField("err", err).Error("Group users demote error.")
    }
    ```

---

__Groups Get by ID__

Fetch one or more groups by their ID.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| group_ids | `[]string` | table | A set of strings of the ID for the groups to get. |

_Returns_

A table (array) of groups with their fields.

_Example_

=== "Lua"
    ```lua
    local group_ids = {
      "0BF154F1-F7D1-4AAA-A060-5FFED3CDB982",
      "997C0D18-0B25-4AEC-8331-9255BD36665D"
    }

    local groups = nk.groups_get_id(group_ids)
    for _, g in ipairs(groups)
    do
      local msg = string.format("Group name %q with id %q", g.name, g.id)
      nk.logger_info(msg)
    end
    ```

=== "Go"
    ```go
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"

    if groups, err := nk.GroupsGetId(ctx, []string{groupID}); err != nil {
      logger.WithField("err", err).Error("Groups get by ID error.")
    } else {
      for _, group := range groups {
        logger.Info("Group name %s with id %s.", group.Name, group.Id)
      }
    }
    ```

---

__Groups List for a user__

List all groups which a user belongs to and whether they've been accepted into the group or if it's an invite.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_id | `string` | string | The Id of the user who's groups you want to list. |

_Returns_

A list of groups for the user.

_Example_

=== "Lua"
    ```lua
    local user_id = "64ef6cb0-7512-11e7-9e52-d7789d80b70b"
    local groups = nk.user_groups_list(user_id)
    for _, g in ipairs(groups)
    do
      local msg = string.format("User has state %q in group %q", g.state, g.group.name)
      nk.logger_info(msg)
    end
    ```

=== "Go"
    ```go
    userID := "dcb891ea-a311-4681-9213-6741351c9994"

    if groups, err := nk.UserGroupsList(ctx, userID); err != nil {
      logger.WithField("err", err).Error("User groups list error.")
    } else {
      for _, group := range groups {
        logger.Printf("User has state %d in group %s.", group.GetState(), group.GetGroup().Name)
      }
    }
    ```

### hmac

__HMAC SHA256 Hash__

Create a HMAC-SHA256 hash from input and key.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | Plaintext input to hash. |
| key | number | Hashing key. |

_Returns_

Hashed input using the key.

_Example_

=== "Lua"
    ```lua
    local hash = nk.hmac_sha256_hash("encryptthis", "somekey")
    print(hash)
    ```

=== "Go"
    ```go
    // Use the standard Go crypto package.
    import "crypto/hmac"
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
| timeout | number | Timeout of the request in milliseconds. Optional, by default is 5000ms. |

_Returns_

`code, headers, body` - Multiple return values for the HTTP response.

_Example_

=== "Lua"
    ```lua
    local url = "https://google.com/"
    local method = "HEAD"
    local headers = {
      ["Content-Type"] = "application/json",
      ["Accept"] = "application/json"
    }
    local content = nk.json_encode({}) -- encode table as JSON string
    local timeout = 5000 -- 5 seconds timeout
    local success, code, headers, body = pcall(nk.http_request, url, method, headers, content, timeout)
    if (not success) then
      nk.logger_error(string.format("Failed %q", code))
    elseif (code >= 400) then
      nk.logger_error(string.format("Failed %q %q", code, body))
    else
      nk.logger_info(string.format("Success %q %q", code, body))
    end
    ```

=== "Go"
    ```go
    // Use the standard Go HTTP package.
    import "net/http"
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

=== "Lua"
    ```lua
    local json = nk.json_decode('{"hello": "world"}')
    nk.logger_info(json.hello)
    ```

=== "Go"
    ```go
    // Use the standard Go JSON package.
    import "encoding/json"
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

=== "Lua"
    ```lua
    local input = {["some"] = "json"}
    local json = nk.json_encode(input)
    nk.logger_info(json) -- Outputs '{"some": "json"}'.
    ```

=== "Go"
    ```go
    // Use the standard Go JSON package.
    import "encoding/json"
    ```

### leaderboards

__Leaderboard Create__

Setup a new dynamic leaderboard with the specified ID and various configuration settings. The leaderboard will be created if it doesn't already exist, otherwise its configuration will *not* be updated.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier for the new leaderboard. This is used by clients to submit scores. |
| authoritative | `bool` | bool | Mark the leaderboard as authoritative which ensures updates can only be made via the Lua runtime. No client can submit a score directly. Optional in Lua. Default false. |
| sort | `string` | string | The sort order for records in the leaderboard; possible values are "asc" or "desc". Optional in Lua. Default "desc". |
| operator | `string` | string | The operator that determines how scores behave when submitted; possible values are "best", "set", or "incr". Optional in Lua. Default "best". |
| reset | `string` | string | The cron format used to define the reset schedule for the leaderboard. This controls when a leaderboard is reset and can be used to power daily/weekly/monthly leaderboards. Optional in Lua. |
| metadata | `map[string]interface{}` | table | The metadata you want associated to the leaderboard. Some good examples are weather conditions for a racing game. Optional in Lua. |

_Example_

=== "Lua"
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

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    authoritative := false
    sortOrder := "desc"
    operator := "best"
    resetSchedule := "0 0 * * 1"
    metadata := map[string]interface{}{
      "weather_conditions": "rain",
    }

    if err := nk.LeaderboardCreate(ctx, id, authoritative, sortOrder, operator, resetSchedule, metadata); err != nil {
      logger.WithField("err", err).Error("Leaderboard create error.")
    }
    ```

---

__Leaderboard Delete__

Delete a leaderboard and all scores that belong to it.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier for the leaderboard to delete. Mandatory field. |

_Example_

=== "Lua"
    ```lua
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    nk.leaderboard_delete(id)
    ```

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    if err := nk.LeaderboardDelete(ctx, id); err != nil {
      logger.WithField("err", err).Error("Leaderboard delete error.")
    }
    ```

---

__Leaderboard Record write__

Use the preconfigured operator for the given leaderboard to submit a score for a particular user.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier for the leaderboard to submit to. Mandatory field. |
| owner | `string` | string | The owner of this score submission. Mandatory field. |
| username | `string` | string | The owner username of this score submission, if it's a user. Optional in Lua. |
| score | `int64` | number | The score to submit. Optional in Lua. Default 0. |
| subscore | `int64` | number | A secondary subscore parameter for the submission. Optional in Lua. Default 0. |
| metadata | `map[string]interface{}` | table | The metadata you want associated to this submission. Some good examples are weather conditions for a racing game. Optional in Lua. |

_Example_

=== "Lua"
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

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    ownerID := "4c2ae592-b2a7-445e-98ec-697694478b1c"
    username := "02ebb2c8"
    score := int64(10)
    subscore := int64(0)
    metadata := map[string]interface{}{
      "weather_conditions": "rain",
    }

    if record, err := nk.LeaderboardRecordWrite(ctx, id, ownerID, username, score, subscore, metadata); err != nil {
      logger.WithField("err", err).Error("Leaderboard record write error.")
    }
    ```

---

__Leaderboard Record delete__

Remove an owner's record from a leaderboard, if one exists.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier for the leaderboard to delete from. Mandatory field. |
| owner | `string` | string | The owner of the score to delete. Mandatory field. |

_Example_

=== "Lua"
    ```lua
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    nk.leaderboard_record_delete(id, owner)
    ```

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    ownerID := "4c2ae592-b2a7-445e-98ec-697694478b1c"

    if err := nk.LeaderboardRecordDelete(ctx, id, ownerID); err != nil {
      logger.WithField("err", err).Error("Leaderboard record delete error.")
    }
    ```

---

__Leaderboard Record list__

List records on the specified leaderboard, optionally filtering to only a subset of records by their owners. Records will be listed in the preconfigured leaderboard sort order.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier of the leaderboard to list. |
| owners | `[]string` | table | Table array of owners to filter to. An optional input. |
| limit | `int` |  number | The maximum number of records to return from 10 to 100. An optional input. |
| cursor | `string` | string | A cursor used to fetch the next page when applicable. An optional input. |

_Returns_

A page of leaderboard records, a list of owner leaderboard records (empty if the `owners` input parameter is not set), an optional next page cursor that can be used to retrieve the next page of records (if any), and an optional previous page cursor that can be used to retrieve the previous page of records (if any).

_Example_

=== "Lua"
    ```lua
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owners = {}
    local limit = 10
    local records, owner_records, next_cursor, prev_cursor = nk.leaderboard_records_list(id, owners, limit)
    ```

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    ownerIDs := []string{}
    limit := 10
    cursor := ""
    expiry := int64(0)

    records, ownerRecords, prevCursor, nextCursor, err := nk.LeaderboardRecordsList(ctx, id, ownerIDs, limit, cursor, expiry)
    if err != nil {
      logger.WithField("err", err).Error("Leaderboard record list error.")
    }
    ```

### logger

__Logger Error__

Write an ERROR level message to the server logs.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| message | `string` | string | The message to write to server logs with ERROR level severity. |
| vars | - | - | Variables to replace placeholders in message. |

_Example_

=== "Lua"
    ```lua
    local message = string.format("%q - %q", "hello", "world")
    nk.logger_error(message)
    ```

=== "Go"
    ```go
    logger.Error("%s - %s", "hello", "world")
    ```

---

__Logger Info__

Write an INFO level message to the server logs.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| message | `string` | string | The message to write to server logs with INFO level severity. |
| vars | - | - | Variables to replace placeholders in message. |

_Example_

=== "Lua"
    ```lua
    local message = string.format("%q - %q", "hello", "world")
    nk.logger_info(message)
    ```

=== "Go"
    ```go
    logger.Info("%s - %s", "hello", "world")
    ```

---

__Logger Warning__

Write an WARN level message to the server logs.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| message | `string` | string | The message to write to server logs with WARN level severity. |
| vars | - | - | Variables to replace placeholders in message. |

_Example_

=== "Lua"
    ```lua
    local message = string.format("%q - %q", "hello", "world")
    nk.logger_warn(message)
    ```

=== "Go"
    ```go
    logger.Warn("%s - %s", "hello", "world")
    ```

### match

__Match Create__

Create a new authoritative realtime multiplayer match running on the given runtime module name. The given `params` are passed to the match's init hook.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| module | `string` | string | The name of an available runtime module that will be responsible for the match. In Go, this was registered in InitModule. In Lua, this was provided as an independent match handler module. |
| params | `map[string]interface{}` | any | Any value to pass to the match's init hook. Optional in Lua. |

_Returns_

(string) - The match ID of the newly created match. Clients can immediately use this ID to join the match.

_Example_

=== "Lua"
    ```lua
    -- Assumes you've registered a runtime module with a path of "some/folder/module.lua".
    local module = "some.folder.module"
    local params = {
      some = "data"
    }
    local match_id = nk.match_create(module, params)
    ```

=== "Go"
    ```go
    // Assumes you've registered a match with initializer.RegisterMatch("some.folder.module", ...)
    modulename := "some.folder.module"
    params := map[string]interface{}{
      "some": "data",
    }

    matchID, err := nk.MatchCreate(ctx, modulename, params)
    if err != nil {
      return "", err
    }
    ```

---

__Match List__

List currently running realtime multiplayer matches and optionally filter them by authoritative mode, label, and current participant count.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| limit | `int` | number | The maximum number of matches to list. Optional in Lua. Default 1. |
| authoritative | `bool` | bool | Boolean `true` if listing should only return authoritative matches, `false` to only return relayed matches, `nil` to return both. Optional in Lua. Default `false` (Go) or `nil` (Lua). |
| label | `string` | string | A label to filter authoritative matches by. Optional in Lua. Default "" (Go) or `nil` (Lua) meaning any label matches. |
| min_size | `int` | number | Inclusive lower limit of current match participants. Optional in Lua. |
| max_size | `int` | number | Inclusive upper limit of current match participants. Optional in Lua. |
| query | `string` | - | Additional query parameters to shortlist matches |

_Example_

=== "Lua"
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
      local message = string.format("Found match with id: %q", m.match_id)
      nk.logger_info(message)
    end
    ```

=== "Go"
    ```go
    // List at most 10 matches, not authoritative, and that
    // have between 2 and 4 players currently participating.
    limit := 10
    isauthoritative := false
    label := ""
    min_size := 2
    max_size := 4

    matches, err := nk.MatchList(ctx, limit, isauthoritative, label, min_size, max_size, "")
    if err != nil {
      logger.WithField("err", err).Error("Match list error.")
    } else {
      for _, match := range matches {
        logger.Info("Found match with id: %s", match.GetMatchId())
      }
    }
    ```

### md5

__md5_hash (input)__

Create an md5 hash from the input.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| input | string | The input string to hash. |

_Example_

=== "Lua"
    ```lua
    local input = "somestring"
    local hashed = nk.md5_hash(input)
    nk.logger_info(hashed)
    ```

=== "Go"
    ```go
    // Use the standard Go crypto package.
    import "crypto/md5"
    ```

### notifications

__Notification Send__

Send one in-app notification to a user. Have a look at the section on [in-app notifications](social-in-app-notifications.md).

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ---- | ----------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| subject | `string` | string | Notification subject. Must be set. |
| content | `map[string]interface{}` | table | Notification content. Must be set but can be an empty table. |
| code | `int` | number | Notification code to use. Must be equal or greater than 0. |
| sender_id | `string` | string | The sender of this notification. If left empty, it will be assumed that it is a system notification. |
| persistent | `bool` | bool | Whether to record this in the database for later listing. Defaults to false. |

_Example_

=== "Lua"
    ```lua
    local subject = "You've unlocked level 100!"
    local content = {
      reward_coins = 1000
    }
    local receiver_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    local sender_id = "dcb891ea-a311-4681-9213-6741351c9994"
    local code = 101
    local persistent = true

    nk.notification_send(receiver_id, subject, content, code, sender_id, persistent)
    ```

=== "Go"
    ```go
    subject := "You've unlocked level 100!"
    content := map[string]interface{}{
      "reward_coins": 1000,
    }
    receiverID := "4c2ae592-b2a7-445e-98ec-697694478b1c"
    senderID := "dcb891ea-a311-4681-9213-6741351c9994" // who the message if from
    code := 101
    persistent := true

    nk.NotificationSend(ctx, receiverID, subject, content, code, senderID, persistent)
    ```

---

__Notifications Send__

Send one or more in-app notifications to a user. Have a look at the section on [in-app notifications](social-in-app-notifications.md).

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| notifications | `[]*runtime.NotifictionSend` | table | A list of notifications to be sent together. |

_Example_

=== "Lua"
    ```lua
    local subject = "You've unlocked level 100!"
    local content = {
      reward_coins = 1000
    }
    local user_id = "4c2ae592-b2a7-445e-98ec-697694478b1c" -- who to send
    local sender_id = "dcb891ea-a311-4681-9213-6741351c9994" -- who the message if from
    local code = 101

    local new_notifications = {
      { subject = subject, content = content, sender_id = sender_id, user_id = user_id, code = code, persistent = true}
    }
    nk.notifications_send(new_notifications)
    ```

=== "Go"
    ```go
    notifications := []*runtime.NotificationSend{
      &runtime.NotificationSend{
        UserID:     "4c2ae592-b2a7-445e-98ec-697694478b1c",
        Subject:    "You've unlocked level 100!",
        Content:    map[string]interface{}{"reward_coins": 1000},
        Code:       101,
        Persistent: true,
      },
      &runtime.NotificationSend{
        UserID:     "69769447-b2a7-445e-98ec-8b1c4c2ae592",
        Subject:    "You've unlocked level 100!",
        Content:    map[string]interface{}{"reward_coins": 1000},
        Code:       101,
        Persistent: true,
      },
    }
    if err := nk.NotificationsSend(ctx, notifications); err != nil {
      logger.WithField("err", err).Error("Notifications send error.")
    }
    ```

### register hooks

__Register Matchmaker Matched__

Registers a function that will be called when matchmaking finds opponents.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each matchmake completion. |

_Example_

=== "Lua"
    ```lua
    -- For example let's create a two player authoritative match.
    local function matchmaker_matched(context, matchmaker_users)
      for _, m in ipairs(matched_users)
      do
        nk.logger_info(m.presence["user_id"])
        nk.logger_info(m.presence["session_id"])
        nk.logger_info(m.presence["username"])
        nk.logger_info(m.presence["node"])

        for _, p in ipairs(m.properties)
        do
          nk.logger_info(p)
        end
      end

      if #matchmaker_users ~= 2 then
        return nil
      end

      if matchmaker_users[1].properties["mode"] ~= "authoritative" then
        return nil
      end
      if matchmaker_users[2].properties["mode"] ~= "authoritative" then
        return nil
      end

      return nk.match_create("match", { debug = true, expected_users = matchmaker_users })
    end
    nk.register_matchmaker_matched(matchmaker_matched)
    ```

=== "Go"
    ```go
    func MakeMatch(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, entries []runtime.MatchmakerEntry) (string, error) {
      for _, e := range entries {
      	logger.Info("%+v", e.GetPresence())

        for k, v := range e.GetProperties() {
          logger.Info("%v: %v", k, v)
        }
      }

      params := map[string]interface{}{
        "debug":          true,
        "expected_users": entries,
      }

      matchID, err := nk.MatchCreate(ctx, "pingpong", params)
      if err != nil {
        return "", err
      }

      return matchID, nil
    }

    // Register as matchmaker matched hook, this call should be in InitModule.
    if err := initializer.RegisterMatchmakerMatched(MakeMatch); err != nil {
      logger.Error("Unable to register: %v", err)
      return err
    }
    ```

Expected to return an authoritative match ID for a match ready to receive these users, or `nil` if the match should proceed through the peer-to-peer relayed mode.

---

__Register Request After__

Register a function with the server which will be executed after every non-realtime message as specified while registering the function.

This can be used to apply custom logic to standard features in the server. It will not block the execution pipeline. The logic will be executed in parallel to any response message sent back to a client. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ---- | ----------- | ----------- |
| func | `function` | function | A function reference which will be executed on each `msgname` message (in Lua). In Go, there are separate functions for each of those actions. |
| msgname | - | string | The specific message name to execute the `func` function after. |

For a complete list of RegisterBefore functions, refer [this page](https://github.com/heroiclabs/nakama/blob/master/runtime/runtime.go). For message names (in Lua), have a look at [this section](runtime-code-basics.md#message-names).

_Example_

=== "Lua"
    ```lua
    local function my_func(context, payload)
      -- Run some code.
    end
    nk.register_req_after(my_func, "FriendsAdd")
    ```

=== "Go"
    ```go
    func AfterAddFriends(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.AddFriendsRequest) error {
      // Run some code.
    }

    // Register as an appropriate after hook, this call should be in InitModule.
    if err := initializer.RegisterAfterAddFriends(AfterAddFriends); err != nil {
      logger.WithField("err", err).Error("After add friends hook registration error.")
      return err
    }
    ```

---

__Register Request Before__

Register a function with the server which will be executed before any non-realtime message with the specified message name. This can be used to apply custom conditions to standard features in the server. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ---- | ----------- | ----------- |
| func | `function` | function | A function reference which will be executed on each `msgname` message (in Lua). In Go, there are separate functions for each of those actions. |
| msgname | - | string | The specific message name to execute the `func` function after. |

For a complete list of RegisterBefore functions, refer [this page](https://github.com/heroiclabs/nakama/blob/master/runtime/runtime.go). For message names in lua, have a look at [this section](runtime-code-basics.md#message-names).

!!! Note
    The function should pass the `payload` input back as a return argument so the pipeline can continue to execute the standard logic. If you return `nil`, the server will stop processing that message. Any other return argument will result in an error.

_Example_

=== "Lua"
    ```lua
    local function my_func(context, payload)
      -- Run some code.
      return payload -- Important!
    end
    nk.register_req_before(my_func, "FriendsAdd")
    ```

=== "Go"
    ```go
    func BeforeAddFriends(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.AddFriendsRequest) (*api.AddFriendsRequest, error) {
      // Run some code.
      return in, nil // Important!
    }

    // Register as an appropriate before hook, this call should be in InitModule.
    if err := initializer.RegisterBeforeAddFriends(BeforeAddFriends); err != nil {
      logger.WithField("err", err).Error("Before add friends hook registration error.")
      return err
    }
    ```

---

__Register Realtime After__

Register a function with the server which will be executed after every realtime message with the specified message name.

This can be used to apply custom logic to standard features in the server. It will not block the execution pipeline. The logic will be executed in parallel to any response message sent back to a client. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each `msgname` message. |
| msgname | string | The specific message name to execute the `func` function after. |

For message names, have a look at [this section](runtime-code-basics.md#message-names).

_Example_

=== "Lua"
    ```lua
    local function my_func(context, payload)
      -- Run some code.
    end
    nk.register_rt_after(my_func, "ChannelJoin")
    ```

=== "Go"
    ```go
    func MyFunc(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime NakamaModule, envelope *rtapi.Envelope) error {
      // Run some code.
    }

    // Register as an appropriate after hook, this call should be in InitModule.
    if err := initializer.RegisterAfterRt("ChannelJoin", MyFunc); err != nil {
      logger.WithField("err", err).Error("After realtime hook registration error.")
      return err
    }
    ```

---

__Register Realtime Before__

Register a function with the server which will be executed before any realtime message with the specified message name. This can be used to apply custom conditions to standard features in the server. Have a look at the section on [runtime code basics](runtime-code-basics.md).

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each `msgname` message. |
| msgname | string | The specific message name to execute the `func` function before. |

For message names, have a look at [this section](runtime-code-basics.md#message-names).

!!! Note
    The function should pass the `payload` input back as a return argument so the pipeline can continue to execute the standard logic. If you return `nil`, the server will stop processing that message. Any other return argument will result in an error.

_Example_

=== "Lua"
    ```lua
    local function my_func(context, payload)
      -- Run some code.
      return payload -- Important!
    end
    nk.register_rt_before(my_func, "ChannelJoin")
    ```

=== "Go"
    ```go
    func MyFunc(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, envelope *rtapi.Envelope) (*rtapi.Envelope, error) {
      return envelope, nil // For code to keep processing the input message.
    }

    // Register as an appropriate before hook, this call should be in InitModule.
    if err := initializer.RegisterBeforeRt("ChannelJoin", MyFunc); err != nil {
      logger.WithField("err", err).Error("Before realtime hook registration error.")
      return err
    }
    ```

---

__Register RPC__

Registers a function for use with client RPC to the server.

The ID can be any string identifier and is sent by the client. The ID is used to map the client RPC message to the specific function to execute. Have a look at the section on [runtime code basics](runtime-code-basics.md).

This function can also be used to register a HTTP endpoint within the server. Have a look at the [Server to server](runtime-code-basics.md#server-to-server) docs for more info.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on each RPC message. |
| id | string | The unique identifier used to register the `func` function for RPC. |

!!! Note
    The `func` can pass `nil`/`""` or a `string` back as a return argument which will returned as bytes in the RPC response.

_Example_

=== "Lua"
    ```lua
    local function my_func(context, payload)
      -- Run some code.
      return payload
    end
    nk.register_rpc(my_func, "my_func_id")
    ```

=== "Go"
    ```go
    func MyFunc(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
      logger.Info("Payload: %s", payload)
      return payload, nil
    }

    // Register as an RPC function, this call should be in InitModule.
    if err := initializer.RegisterRpc("my_func_id", MyFunc); err != nil {
      logger.WithField("err", err).Error("RPC registration error.")
      return err
    }
    ```

---

__Register Leaderboard Reset__

Registers a function to be run when a [leaderboard](runtime-code-function-reference.md#leaderboards) resets.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on a leaderboard reset. |

_Example_

=== "Lua"
    ```lua
    local fn = function(ctx, leaderboard, reset) {
        -- Custom logic
    }
    nk.register_leaderboard_reset(fn)
    ```

=== "Go"
    ```go
    func LeaderboardReset(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, lb runtime.Leaderboard, reset int64) error {
        // Custom logic runs on reset.
        return nil
    }

    if err := initializer.RegisterLeaderboardReset(LeaderboardReset); err != nil {
        logger.WithField("err", err).Error("Leaderboard reset registration error.")
        return err
    }
    ```

---

__Register Tournament Reset__

Registers a function to be run when a [tournament](runtime-code-function-reference.md#tournaments) resets.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on a tournament reset. |

_Example_

=== "Lua"
    ```lua
    local fn = function(ctx, tournament, t_end, reset) {
        -- Custom logic
    }
    nk.register_tournament_reset(fn)
    ```

=== "Go"
    ```go
    func TournamentReset(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, lb runtime.Tournament, reset int64) error {
        // Custom logic runs on reset.
        return nil
    }

    if err := initializer.RegisterTournamentReset(TournamentReset); err != nil {
        logger.WithField("err", err).Error("Tournament reset registration error.")
        return err
    }
    ```

---

__Register Tournament End__

Registers a function to be run when a [tournament](runtime-code-function-reference.md#tournaments) ends.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed on a tournament end. |

_Example_

=== "Go"
    ```go
    func TournamentEnd(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, lb runtime.Tournament, reset int64) error {
        // Custom logic
        return nil
    }

    if err := initializer.RegisterTournamentEnd(TournamentEnd); err != nil {
        logger.WithField("err", err).Error("Tournament end registration error.")
        return err
    }
    ```

=== "Lua"
    ```lua
    local fn = function(ctx, tournament, t_end, reset) {
        -- Custom logic
    }
    nk.register_tournament_end(fn)
    ```

---

### run once

__Run Once__

The runtime environment allows you to run code that must only be executed only once. This is useful if you have custom SQL queries that you need to perform (like creating a new table) or to register with third party services.

Go runtime modules do not need a dedicated 'run once' function, use the `InitModule` function to achieve the same effect.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| func | function | A function reference which will be executed only once. |

_Example_

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
    INSERT INTO users (id, username)
    VALUES ($1, $2)
    ON CONFLICT (id) DO NOTHING`, systemId, "system_id")
      if err != nil {
        logger.WithField("err", err).Error("DB exec error.")
        return err
      }

      return nil
    }
    ```

### storage

__Storage Read__

Fetch one or more records by their bucket/collection/keyname and optional user.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| object_ids | `[]*runtime.StorageRead` | table | A table / array of object identifiers to be fetched. |

_Returns_

A table array of object result set.

_Example_

=== "Lua"
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
      local message = string.format("read: %q, write: %q, value: %q", r.permission_read, r.permission_write, r.value)
      nk.logger_info(message)
    end
    ```

=== "Go"
    ```go
    userID := "4ec4f126-3f9d-11e7-84ef-b7c182b36521" // some user ID.
    objectIds := []*runtime.StorageRead{
      &runtime.StorageRead{
        Collection: "save",
        Key: "save1",
        UserID: userID,
      },
      &runtime.StorageRead{
        Collection: "save",
        Key: "save2",
        UserID: userID,
      },
      &runtime.StorageRead{
        Collection: "save",
        Key: "save3",
        UserID: userID,
      },
    }

    records, err := nk.StorageRead(ctx, objectIds)
    if err != nil {
      logger.WithField("err", err).Error("Storage read error.")
    } else {
      for _, record := range records {
        logger.Info("read: %d, write: %d, value: %s", record.PermissionRead, record.PermissionWrite, record.Value)
      }
    }
    ```

---

__Storage List__

You can list records in a collection and page through results. The records returned can be filtered to those owned by the user or "" for public records which aren't owned by a user.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_id | `string` | string | User ID or "" (empty string) for public records. |
| collection | `string` | string | Collection to list data from. |
| limit | `int` | number | Limit number of records retrieved. Min 10, Max 100. |
| cursor | `string` | string | Pagination cursor from previous result. If none available set to nil or "" (empty string). |

_Returns_

A table array of the records result set.

_Example_

=== "Lua"
    ```lua
    local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- Some user ID.
    local records = nk.storage_list(user_id "collection", 10, "")
    for _, r in ipairs(records)
    do
      local m = string.format("read: %q, write: %q, value: %q", r.permission_read, r.permission_write, r.value)
      nk.logger_info(m)
    end
    ```

=== "Go"
    ```go
    userID := "4ec4f126-3f9d-11e7-84ef-b7c182b36521" // Some user ID.
    listRecords, nextCursor, err := nk.StorageList(ctx, userID, "collection", 10, "")
    if err != nil {
      logger.WithField("err", err).Error("Storage list error.")
    } else {
      for _, r := range listRecords {
        logger.Info("read: %d, write: %d, value: %s", r.PermissionRead, r.PermissionWrite, r.Value)
      }
    }
    ```

---

__Storage Delete__

Remove one or more objects by their collection/keyname and optional user.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| object_ids | `[]*runtime.StorageDelete` | table | A table / array of object identifiers to be fetched. |

_Example_

=== "Lua"
    ```lua
    local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- Some user ID.
    local friend_user_id = "8d98ee3f-8c9f-42c5-b6c9-c8f79ad1b820" -- Friend ID.
    local object_ids = {
      { collection = "save", key = "save1", user_id = user_id },
      { collection = "save", key = "save2", user_id = user_id },
      { collection = "public", key = "progress", user_id = friend_user_id }
    }
    nk.storage_delete(object_ids)
    ```

=== "Go"
    ```go
    userID := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"       // Some user ID.
    friendUserID := "8d98ee3f-8c9f-42c5-b6c9-c8f79ad1b820" // Friend ID.
    objectIds := []*runtime.StorageDelete{
      &runtime.StorageDelete{
        Collection: "save",
        Key:        "save1",
        UserID:     userID,
      },
      &runtime.StorageDelete{
        Collection: "save",
        Key:        "save2",
        UserID:     userID,
      },
      &runtime.StorageDelete{
        Collection: "public",
        Key:        "progress",
        UserID:     friendUserID,
      },
    }

    err := nk.StorageDelete(ctx, objectIds)
    if err != nil {
      logger.WithField("err", err).Error("Storage delete error.")
    }
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

__Storage Write__

Write one or more objects by their collection/keyname and optional user.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| object_ids | `[]*runtime.StorageWrite` | table | A table / array of object identifiers to be fetched. |

_Example_

=== "Lua"
    ```lua
    local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- Some user ID.
    local new_objects = {
      { collection = "save", key = "save1", user_id = user_id, value = {} },
      { collection = "save", key = "save2", user_id = user_id, value = {} },
      { collection = "save", key = "save3", user_id = user_id, value = {}, permission_read = 2, permission_write = 1 },
      { collection = "save", key = "save3", user_id = user_id, value = {}, version="*", permission_read = 1, permission_write = 1 }
    }
    nk.storage_write(new_objects)
    ```

=== "Go"
    ```go
    userID := "4ec4f126-3f9d-11e7-84ef-b7c182b36521" // Some user ID.
    objectIDs := []*runtime.StorageWrite{
      &runtime.StorageWrite{
        Collection: "save",
        Key:        "save1",
        UserID:     userID,
        Value:      "{}", // Value must be a valid encoded JSON object.
      },
      &runtime.StorageWrite{
        Collection: "save",
        Key:        "save2",
        UserID:     userID,
        Value:      "{}", // Value must be a valid encoded JSON object.
      },
      &runtime.StorageWrite{
        Collection:      "public",
        Key:             "save3",
        UserID:          userID,
        Value:           "{}", // Value must be a valid encoded JSON object.
        PermissionRead:  2,
        PermissionWrite: 1,
      },
      &runtime.StorageWrite{
        Collection:      "public",
        Key:             "save4",
        UserID:          userID,
        Value:           "{}", // Value must be a valid encoded JSON object.
        Version:         "*",
        PermissionRead:  2,
        PermissionWrite: 1,
      },
    }

    _, err := nk.StorageWrite(ctx, objectIDs)
    if err != nil {
      logger.WithField("err", err).Error("Storage write error.")
    }
    ```

### sql

!!! Note
    These functions allow your Lua scripts to run arbitrary SQL staments beyond the ones built into Nakama itself. It is your responsibility to manage the performance of these queries.

__sql_exec (query, parameters)__

Execute an arbitrary SQL query and return the number of rows affected. Typically an `"INSERT"`, `"DELETE"`, or `"UPDATE"` statement with no return columns.

| Param | Type | Description |
| ----- | ---- | ----------- |
| query | string | A SQL query to execute. |
| parameters | table | Arbitrary parameters to pass to placeholders in the query. |

_Returns_

A single number indicating the number of rows affected by the query.

_Example_

=== "Lua"
    ```lua
    -- This example query deletes all expired leaderboard records.
    local query = [[DELETE FROM leaderboard_record
                    WHERE expires_at > 0 AND expires_at <= $1]]
    local parameters = { os.time() * 1000 }
    local affected_rows_count = nk.sql_exec(query, parameters)
    ```

=== "Go"
    ```go
    // Use the standard Go sql package.
    import "database/sql"
    ```

---

__sql_query (query, parameters)__

Execute an arbitrary SQL query that is expected to return row data. Typically a `"SELECT"` statement.

| Param | Type | Description |
| ----- | ---- | ----------- |
| query | string | A SQL query to execute. |
| parameters | table | Arbitrary parameters to pass to placeholders in the query. |

_Returns_

A Lua table containing the result rows in the format:

=== "Lua"
    ```lua
    {
      {column1 = "value1", column2 = "value2", ...}, -- Row 1.
      {column1 = "value1", column2 = "value2", ...}, -- Row 2.
      ...
    }
    ```

=== "Go"
    ```go
    // Use the standard Go sql package.
    import "database/sql"
    ```

_Example_

=== "Lua"
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
      nk.logger_info(string.format("Username %q created at %q", row.username, row.create_time))
    end
    ```

=== "Go"
    ```go
    // Use the standard Go sql package.
    import "database/sql"
    ```

!!! Note
    The fields available in each `row` depend on the columns selected in the query such as `row.username` and `row.create_time` above.

### time

__time ()__

Get the current UTC time in milliseconds using the system wall clock.

_Returns_

A number representing the current UTC time in milliseconds.

_Example_

=== "Lua"
    ```lua
    local utc_msec = nk.time()
    ```

=== "Go"
    ```go
    // Use the standard Go time package.
    import "time"
    ```

### tournaments

__Tournament Create__

Setup a new dynamic tournament with the specified ID and various configuration settings. The underlying leaderboard will be created if it doesn't already exist, otherwise its configuration will *not* be updated.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier for the new tournament. This is used by clients to submit scores. |
| sort | `string` | string | The sort order for records in the tournament; possible values are "asc" or "desc". Default "desc". |
| operator | `string` | string | The operator that determines how scores behave when submitted. The possible values are "best", "set", or "incr". Default "best". |
| duration | `int` | number | The active duration for a tournament. This is the duration when clients are able to submit new records. The duration starts from either the reset period or tournament start time whichever sooner. A game client can query the tournament for results between end of duration and next reset period. |
| reset | `string` | string | The cron format used to define the reset schedule for the tournament. This controls when the underlying leaderboard resets and the tournament is considered active again. This is optional. |
| metadata | `map[string]interface{}` | table | The metadata you want associated to the tournament. Some good examples are weather conditions for a racing game. This is optional. |
| title | `string` | string | The title of the tournament. This is optional. |
| description | `string` | string | The description of the tournament. This is optional. |
| category | `int` | number | A category associated with the tournament. This can be used to filter different types of tournaments. Between 0 and 127. This is optional. |
| start_time | `int` | number | The start time of the tournament. Leave empty for immediately, or a future time. |
| end_time | `int` | number | The end time of the tournament. When the end time is elapsed, the tournament will not reset and will cease to exist. Must be greater than start_time if set. Default value is __never__. |
| max_size | `int` | number | Maximum size of participants in a tournament. This is optional. |
| max_num_score | `int` | number | Maximum submission attempts for a tournament record. |
| join_required | `bool` | bool | Whether the tournament needs to be joint before a record write is allowed. |

_Example_

=== "Lua"
    ```lua
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local authoritative = false
    local sort = "desc"     -- One of: "desc", "asc".
    local operator = "best" -- One of: "best", "set", "incr".
    local reset = "0 12 * * *" -- Noon UTC each day.
    local metadata = {
      weather_conditions = "rain"
    }
    title = "Daily Dash"
    description = "Dash past your opponents for high scores and big rewards!"
    category = 1
    start_time = 0       -- Start now.
    end_time = 0         -- Never end, repeat the tournament each day forever.
    duration = 3600      -- In seconds.
    max_size = 10000     -- First 10,000 players who join.
    max_num_score = 3    -- Each player can have 3 attempts to score.
    join_required = true -- Must join to compete.
    nk.tournament_create(id, sort, operator, duration, reset, metadata, title, description,
        category, start_time, end_time, max_size, max_num_score, join_required)
    ```

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    sortOrder := "desc"           // One of: "desc", "asc".
    operator := "best"            // One of: "best", "set", "incr".
    resetSchedule := "0 12 * * *" // Noon UTC each day.
    metadata := map[string]interface{}{
      "weather_conditions": "rain",
    }
    title := "Daily Dash"
    description := "Dash past your opponents for high scores and big rewards!"
    category := 1
    startTime := 0       // Start now.
    endTime := 0         // Never end, repeat the tournament each day forever.
    duration := 3600     // In seconds.
    maxSize := 10000     // First 10,000 players who join.
    maxNumScore := 3     // Each player can have 3 attempts to score.
    joinRequired := true // Must join to compete.

    err := nk.TournamentCreate(ctx, id, sortOrder, operator, resetSchedule, metadata,
        title, description, category, startTime, endTime, duration, maxSize, maxNumScore, joinRequired)
    if err != nil {
      logger.WithField("err", err).Error("Tournament create error.")
    }
    ```

---

__Tournament Delete__

Delete a tournament and all records that belong to it.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier for the tournament to delete. |

_Example_

=== "Lua"
    ```lua
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    nk.tournament_delete(id)
    ```

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    err := nk.TournamentDelete(ctx, id)
    if err != nil {
      logger.WithField("err", err).Error("Tournament delete error.")
    }
    ```

---

__Tournament Add Attempt__

Add additional score attempts to the owner's tournament record. This overrides the max number of score attempts allowed in the tournament for this specific owner.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier for the tournament to update. |
| owner | `string` | string | The owner of the record to increment the count for. |
| count | `int` | number | The number of attempt counts to increment. Can be negative to decrease count. |

_Example_

=== "Lua"
    ```lua
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "leaderboard-record-owner"
    local count = -10
    nk.tournament_add_attempt(id, owner, count)
    ```

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    ownerID := "leaderboard-record-owner"
    count := -10
    err := nk.TournamentAddAttempt(ctx, id, ownerID, count)
    if err != nil {
      logger.WithField("err", err).Error("Tournament add attempt error.")
    }
    ```

---

__Tournament Join__

A tournament may need to be joined before the owner can submit scores. This operation is idempotent and will always succeed for the owner even if they have already joined the tournament.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier for the tournament to update. |
| user_id | `string` | string | The owner of the record. |
| username | `string` | string | The username of the record owner. |

_Example_

=== "Lua"
    ```lua
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "leaderboard-record-owner"
    local username = "myusername"
    nk.tournament_join(id, owner, username)
    ```

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    ownerID := "leaderboard-record-owner"
    userName := "myusername"
    err := nk.TournamentJoin(ctx, id, ownerID, userName)
    if err != nil {
      logger.WithField("err", err).Error("Tournament join error.")
    }
    ```

---

__Tournaments Get by ID__

Fetch one or more tournaments by ID.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| ids | `[]string` | table | The table array of tournament ids. |

_Example_

=== "Lua"
    ```lua
    local tournament_ids = {
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936"
    }
    local tournaments = nk.tournaments_get_id(tournament_ids)
    ```

=== "Go"
    ```go
    tournamentIDs := []string{
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936",
    }
    tournaments, err := nk.TournamentsGetId(ctx, tournamentIDs)
    if err != nil {
      logger.WithField("err", err).Error("Tournaments get error.")
    }
    ```

---

__Tournament List__

Find tournaments which have been created on the server. Tournaments can be filtered with categories and via start and end times. This function can also be used to see the tournaments that an owner (usually a user) has joined.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| category_start | `int` | number | Filter tournament with categories greater or equal than this value. |
| category_end | `int` | number | Filter tournament with categories equal or less than this value. |
| start_time | `int` | number | Filter tournament with that start after this time. |
| end_time | `int` | number | Filter tournament with that end before this time. |
| limit | `int` | number | Return only the required number of tournament denoted by this limit value. |
| cursor | `string` | string | Cursor to paginate to the next result set. If this is empty/null there is no further results. |

_Returns_

A table of tournament objects.

_Example_

=== "Lua"
    ```lua
    local category_start = 1
    local category_end = 2
    local start_time = 1538147711
    local end_time = 0 -- All tournaments from the start time.
    local limit = 100  -- Number to list per page.
    local tournaments = nk.tournament_list(category_start, category_end, start_time, end_time, limit)
    for i, tournament in ipairs(tournaments) do
      nk.logger_info(string.format("ID: %q - can enter? %q", tournament.id, tournament.can_enter))
    end
    ```

=== "Go"
    ```go
    categoryStart := 1
    categoryEnd := 2
    startTime := int(time.Now().Unix())
    endTime := 0 // All tournaments from the start time.
    limit := 100 // Number to list per page.
    cursor := ""
    list, err := nk.TournamentList(ctx, categoryStart, categoryEnd, startTime, endTime, limit, cursor)
    if err != nil {
      logger.WithField("err", err).Error("Tournament list error.")
    } else {
      for _, t := range list.Tournaments {
        logger.Info("ID %s - can enter? %b", t.Id, t.CanEnter)
      }
    }
    ```

---

__Tournament Record Write__

Submit a score and optional subscore to a tournament leaderboard. If the tournament has been configured with join required this will fail unless the owner has already joined the tournament.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier of the leaderboard to submit. |
| owner | `string` | string | The owner of this score submission. |
| username | `string` | string | The owner username of this score submission, if it's a user. This is optional. |
| score | `int64` | number | The score to submit. This is optional. Default 0. |
| subscore | `int64` | number | A secondary subscore parameter for the submission. This is optional. Default 0. |
| metadata | `map[string]interface{}` | table | The metadata you want associated to this submission. Some good examples are weather conditions for a racing game. This is optional. |

_Returns_

A table of tournament record objects.

_Example_

=== "Lua"
    ```lua
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    local username = "02ebb2c8"
    local score = 10
    local subscore = 0
    local metadata = {
      weather_conditions = "rain"
    }
    nk.tournament_record_write(id, owner, username, score, subscore, metadata)
    ```

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    ownerID := "4c2ae592-b2a7-445e-98ec-697694478b1c"
    username := "02ebb2c8"
    score := int64(10)
    subscore := int64(0)
    metadata := map[string]interface{}{
      "weather_conditions": "rain",
    }
    _, err := nk.TournamentRecordWrite(ctx, id, ownerID, username, score, subscore, metadata)
    if err != nil {
      logger.WithField("err", err).Error("Tournament record write error.")
    }
    ```

---

__Tournament Records Haystack__

Fetch the list of tournament records around the owner.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| id | `string` | string | The unique identifier of the leaderboard to submit. |
| owner | `string` | string | The owner of this score submission. |
| limit | `int` | number | Number of records to return. Default value is 1. |

_Returns_

A table of tournament record objects.

_Example_

=== "Lua"
    ```lua
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    nk.tournament_records_haystack(id, owner, 10)
    ```

=== "Go"
    ```go
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    ownerID := "4c2ae592-b2a7-445e-98ec-697694478b1c"
    limit := 10
    records, err := nk.TournamentRecordsHaystack(ctx, id, ownerID, limit)
    if err != nil {
      logger.WithField("err", err).Error("Tournament record haystack error.")
    } else {
      for _, r := range records {
        logger.Info("Leaderboard: %s, score: %d, subscore: %d", r.GetLeaderboardId(), r.Score, r.Subscore)
      }
    }
    ```

---

### users

__Users Get by ID__

Fetch one or more users by ID.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_ids | `[]string` | table | A table / array of user IDs to fetch. |

_Returns_

A table array of the user result set.

_Example_

=== "Lua"
    ```lua
    local user_ids = {
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936"
    }
    local users = nk.users_get_id(user_ids)
    for _, u in ipairs(users)
    do
      local message = string.format("username: %q, displayname: %q", u.username, u.display_name)
      nk.logger_info(message)
    end
    ```

=== "Go"
    ```go
    userIDs := []string{
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936",
    }
    users, err := nk.UsersGetId(ctx, userIDs)
    if err != nil {
      logger.WithField("err", err).Error("Users get ID error.")
    } else {
      for _, u := range users {
        logger.Info("username: %s, displayname: %s", u.Username, u.DisplayName)
      }
    }
    ```

---

__Users Get by Username__

Fetch a set of users by their usernames.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| usernames | `[]string` | table | A table array of usernames to fetch. |

_Returns_

A table array of the user result set.

_Example_

=== "Lua"
    ```lua
    local usernames = {"b7865e7e", "c048ba7a"}
    local users = nk.users_get_username(usernames)
    for _, u in ipairs(users)
    do
      local message = string.format("id: %q, displayname: %q", u.user_id, u.display_name)
      nk.logger_info(message)
    end
    ```

=== "Go"
    ```go
    users, err := nk.UsersGetUsername(ctx, []string{"b7865e7e", "c048ba7a"})
    if err != nil {
      logger.WithField("err", err).Error("Users get username error.")
    } else {
      for _, u := range users {
        logger.Info("id: %s, displayname: %s", u.Id, u.DisplayName)
      }
    }
    ```

---

__Users Ban by ID__

Ban one or more users by ID. These users will no longer be allowed to authenticate with the server until unbanned.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_ids | `[]string` | table | A table / array of user IDs to ban. |

_Example_

=== "Lua"
    ```lua
    local user_ids = {
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936"
    }
    nk.users_ban_id(user_ids)
    ```

=== "Go"
    ```go
    userIDs := []string{
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936",
    }
    err := nk.UsersBanId(ctx, userIDs)
    if err != nil {
      logger.WithField("err", err).Error("Users ban ID error.")
    }
    ```

---

__Users Unban by ID__

Unban one or more users by ID. These users will again be allowed to authenticate with the server.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_ids | `[]string` | table | A table / array of user IDs to unban. |

_Example_

=== "Lua"
    ```lua
    local user_ids = {
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936"
    }
    nk.users_unban_id(user_ids)
    ```

=== "Go"
    ```go
    userIDs := []string{
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936",
    }
    err := nk.UsersUnbanId(ctx, userIDs)
    if err != nil {
      logger.WithField("err", err).Error("Users unban id error.")
    }
    ```

### uuid

__uuid_v4 ()__

Generate a version 4 UUID in the standard 36-character string representation.

_Returns_

The generated version 4 UUID identifier.

_Example_

=== "Lua"
    ```lua
    local uuid = nk.uuid_v4()
    nk.logger_info(uuid)
    ```

=== "Go"
    ```go
    // Use a separate Go package for UUIDs.
    import "github.com/gofrs/uuid"
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

=== "Lua"
    ```lua
    local uuid_bytes = "896418357731323983933079013" -- some uuid bytes.
    local uuid_string = nk.uuid_bytes_to_string(uuid_bytes)
    nk.logger_info(uuid_string)
    ```

=== "Go"
    ```go
    // Use a separate Go package for UUIDs.
    import "github.com/gofrs/uuid"
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

=== "Lua"
    ```lua
    local uuid_string = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some uuid string.
    local uuid_bytes = nk.uuid_string_to_bytes(uuid_string)
    nk.logger_info(uuid_bytes)
    ```

=== "Go"
    ```go
    // Use a Go CRON package, for example:
    import "github.com/gofrs/uuid"
    ```

### wallet

__Wallet Update__

Update a user's wallet with the given changeset.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_id | `string` | string | The ID of the user to update the wallet for. |
| changeset | `map[string]interface{}` | table | The set of wallet operations to apply. |
| metadata | `map[string]interface{}` | table | Additional metadata to tag the wallet update with. Optional in Lua. |
| update_ledger | `bool` | bool | Whether to record this update in the ledger. Default true. Optional in Lua. |

_Example_

=== "Lua"
    ```lua
    local user_id = "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
    local changeset = {
      coins = 10, -- Add 10 coins to the user's wallet.
      gems = -5   -- Remove 5 gems from the user's wallet.
    }
    local metadata = {
      game_result = "won"
    }
    nk.wallet_update(user_id, changeset, metadata, true)
    ```

=== "Go"
    ```go
    userID := "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
    changeset := map[string]interface{}{
      "coins": 10, // Add 10 coins to the user's wallet.
      "gems":  -5, // Remove 5 gems from the user's wallet.
    }
    metadata := map[string]interface{}{
      "game_result": "won",
    }
    err := nk.WalletUpdate(ctx, userID, changeset, metadata, true)
    if err != nil {
      logger.WithField("err", err).Error("Wallet update error.")
    }
    ```

---

__Wallets Update__

Update one or more user wallets with individual changesets. This function will also insert a new wallet ledger item into each user's wallet history that tracks their update.

All updates will be performed atomically.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| updates | `[]*runtime.WalletUpdate` | table | The set of user wallet update operations to apply. |
| update_ledger | `bool` | bool | Whether to record this update in the ledger. Default true. This is optional. |

_Example_

=== "Lua"
    ```lua
    local updates = {
      {
        user_id = "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592",
        changeset = {
          coins = 10, -- Add 10 coins to the user's wallet.
          gems = -5   -- Remove 5 gems from the user's wallet.
        },
        metadata = {
          game_result = "won"
        }
      }
    }
    nk.wallets_update(updates, true)
    ```

=== "Go"
    ```go
    updates := []*runtime.WalletUpdate{
      &runtime.WalletUpdate{
        UserID: "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592",
        Changeset: map[string]interface{}{
          "coins": 10, // Add 10 coins to the user's wallet.
          "gems":  -5, // Remove 5 gems from the user's wallet.
        },
        Metadata: map[string]interface{}{
          "game_result": "won",
        },
      },
    }
    err := nk.WalletsUpdate(ctx, updates, true)
    if err != nil {
      logger.WithField("err", err).Error("Wallets update error.")
    }
    ```

---

__Wallet Ledger List__

List all wallet updates for a particular user from oldest to newest.

_Parameters_

| Param | Go type | Lua type | Description |
| ----- | ------- | -------- | ----------- |
| ctx | `context.Context` | - | The [context](runtime-code-basics.md#register-hooks) object represents information about the server and requester. |
| user_id | `string` | string | The ID of the user to update the wallet. |

_Returns_

A Lua table / Go slice containing wallet entries, with the following parameters:

=== "Lua"
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

=== "Go"
    ```go
    {
      {
        Id: "...",
        UserID: "...",
        CreateTime: 123,
        UpdateTime: 123,
        ChangeSet: {}, // map[string]interface{}
        Metadata: {},  // map[string]interface{}
      },
    }
    ```

_Example_

=== "Lua"
    ```lua
    local user_id = "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
    local updates = nk.wallet_ledger_list(user_id)
    for _, u in ipairs(updates)
    do
      local message = string.format("Found wallet update with id: %q", u.id)
      nk.logger_info(message)
    end
    ```

=== "Go"
    ```go
    userID := "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
    items, err := nk.WalletLedgerList(ctx, userID)
    if err != nil {
      logger.WithField("err", err).Error("Wallet ledger list error.")
    } else {
      for _, item := range items {
        logger.Info("Found wallet update with id: %v", item.GetID())
      }
    }
    ```

---

__Wallet Ledger Update__

Update the metadata for a particular wallet update in a users wallet ledger history. Useful when adding a note to a transaction for example.

_Parameters_

| Param | Type | Description |
| ----- | ---- | ----------- |
| id | string | The ID of the wallet ledger item to update. |
| metadata | table | The new metadata to set on the wallet ledger item. |

_Returns_

The updated wallet ledger item as a Lua table with the following format:

=== "Lua"
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

=== "Go"
    ```go
    {
      {
        Id: "...",
        UserID: "...",
        CreateTime: 123,
        UpdateTime: 123,
        ChangeSet: {}, // map[string]interface{}
        Metadata: {},  // map[string]interface{}
      },
    }
    ```

_Example_

=== "Lua"
    ```lua
    local id = "2745ba53-4b43-4f83-ab8f-93e9b677f33a"
    local metadata = {
      game_result = "loss"
    }
    local u = nk.wallet_ledger_update(id, metadata)
    ```

=== "Go"
    ```go
    itemID := "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
    metadata := map[string]interface{}{
      "game_result": "loss",
    }
    _, err := nk.WalletLedgerUpdate(ctx, itemID, metadata)
    if err != nil {
      logger.WithField("err", err).Error("Wallet ledger update error.")
    }
    ```
