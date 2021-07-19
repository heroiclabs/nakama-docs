# Function Reference

The code runtime built into the server includes a module with functions to implement various logic and custom behavior. It is easy to define authoritative code and conditions on input received by clients.

## Nakama

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

=== "TypeScript"
    ```shell
    npm i 'https://github.com/heroiclabs/nakama-common'
    ```

!!! Note
    All Lua code examples assume the `"nakama"` module has been imported.

    All Go functions will have `nk runtime.NakamaModule` available as a parameter that may be used to access server runtime functions. A `context` will also be supplied in function input arguments.

    All JavaScript functions, similar to Go, have the `nk` (of TypeScript type `nkruntime.Nakama`) object available as a parameter to access the server runtime functions.

## Users

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Users Get by ID**: Fetch one or more users by ID. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `[]*api.Users`: A list of user record objects. |
    | | user_ids | `[]string` | An array of user IDs to fetch. |
    | **Users Get by Username**: Fetch one or more users by username. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `[]*api.Users`: A list of user record objects. |
    | | usernames | `[]string` | An array of usernames to fetch. |
    | **Users Ban by ID**: Ban one or more users by ID. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | user_ids | `[]string` | An array of user IDs to ban. |
    | **Users Unban by ID**: Unban one or more users by ID. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | user_ids | `[]string` | An array of user IDs to unban. |

    Examples:
    ```go
    // Users Get By ID
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

    // Users Get By Username
    users, err := nk.UsersGetUsername(ctx, []string{"b7865e7e", "c048ba7a"})
    if err != nil {
      logger.WithField("err", err).Error("Users get username error.")
    } else {
      for _, u := range users {
        logger.Info("id: %s, displayname: %s", u.Id, u.DisplayName)
      }
    }

    // Users Ban By ID
    userIDs := []string{
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936",
    }
    err := nk.UsersBanId(ctx, userIDs)
    if err != nil {
      logger.WithField("err", err).Error("Users ban ID error.")
    }

    // Users Unban by ID
    userIDs := []string{
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936",
    }
    err := nk.UsersUnbanId(ctx, userIDs)
    if err != nil {
      logger.WithField("err", err).Error("Users unban id error.")
    }
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Users Get by ID**: Fetch one or more users by ID. | user_ids | `table` | A table of user IDs to fetch. | `table`: A list of user record objects. |
    | **Users Get by Username**: Fetch one or more users by username. | usernames | `table` | A table of usernames to fetch. | `table`: A list of user record objects. |
    | **Users Ban by ID**: Ban one or more users by ID. | user_ids | `table` | A table of user IDs to Ban. |
    | **Users Unban by ID**: Unban one or more users by ID. | user_ids | `table` | A table of user IDs to unban. |

    Examples:
    ```lua
    -- Users Get By ID
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

    -- Users Get By Username
    local usernames = {"b7865e7e", "c048ba7a"}
    local users = nk.users_get_username(usernames)
    for _, u in ipairs(users)
    do
      local message = string.format("id: %q, displayname: %q", u.user_id, u.display_name)
      nk.logger_info(message)
    end

    -- Users Ban By ID
    local user_ids = {
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936"
    }
    nk.users_ban_id(user_ids)

    -- Users Unban by ID
    local user_ids = {
      "3ea5608a-43c3-11e7-90f9-7b9397165f34",
      "447524be-43c3-11e7-af09-3f7172f05936"
    }
    nk.users_unban_id(user_ids)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Users Get by ID**: Fetch one or more users by ID. | user_ids | `string[]` | An array of user IDs to fetch. | `nkruntime.User[]`: A list of user record objects. |
    | **Users Get by Username**: Fetch one or more users by username. | usernames | `string[]` | An array of username to fetch. | `nkruntime.User[]`: A list of user record objects. |
    | **Users Ban by ID**: Ban one or more users by ID. | user_ids | `string[]` | An array of user IDs to ban. |
    | **Users Unban by ID**: Unban one or more users by ID. | user_ids | `string[]` | An array of user IDs to unban. |

    Examples:
    ```ts
    // Users Get By ID
    let userIds = [
        '3ea5608a-43c3-11e7-90f9-7b9397165f34',
        '447524be-43c3-11e7-af09-3f7172f05936',
    ];

    let users: nkruntime.Users[] = [];
    try {
        users = nk.usersGetId(userIds);
    } catch (error) {
        // Handle error
    }

    // Users Get By Username
    let usernames = [
        'b7865e7e',
        'c048ba7a',
    ];

    let users: nkruntime.Users[] = [];
    try {
        users = nk.usersGetUsername(usernamees);
    } catch (error) {
        // Handle error
    }

    users.forEach(u => {
        logger.info('d: %q, displayname: %q', u.userId, u.displayName);
    })

    // Users Ban By ID
    let userIds = [
        '3ea5608a-43c3-11e7-90f9-7b9397165f34',
        '447524be-43c3-11e7-af09-3f7172f05936',
    ];

    try {
        users = nk.usersBanId(userIds);
    } catch (error) {
        // Handle error
    }

    // Users Unban by ID
    let userIds = [
        '3ea5608a-43c3-11e7-90f9-7b9397165f34',
        '447524be-43c3-11e7-af09-3f7172f05936',
    ];

    try {
        users = nk.usersUnbanId(userIds);
    } catch (error) {
        // Handle error
    }
    ```


### Accounts

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Get Account** | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `*api.Account`: All account information including wallet, device IDs and more. |
    |  | user_id | `string` | User ID to fetch information for. Must be valid UUID. |
    | **Get Accounts** | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `*api.Account`: An array of accounts. |
    |  | user_ids | `[]string` | An array of user IDs to fetch information for. Must be valid UUID. |
    | **Update Account** | ctx | context.Context | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | user_id | `string` | User ID for which the information is to be updated. Must be valid UUID. |
    | | metadata | `map[string]interface{}` | Metadata to update. |
    | | username | `string` | Username to be set. Must be unique. Use "" if it is not being updated. |
    | | display_name | `string` | Display name to be updated. Use "" if it is not being updated. |
    | | timezone | `string` | Timezone to be updated. Use "" if it is not being updated. |
    | | location | `string` | Location to be updated. Use "" if it is not being updated. |
    | | language | `string` | Lang tag to be updated. Use "" if it is not being updated. |
    | | avatar_url | `string` | User's avatar URL. Use "" if it is not being updated. |
    | **Delete Account** | ctx | context.Context | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | user_id | `string` | User ID for which the information is to be updated. Must be valid UUID. |
    | | recorded | `bool` | Whether to record this deletion in the database. By default this is set to false. |

    Examples:
    ```go
    // Get Account
    account, err := nk.AccountGetId(ctx, "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592")
    if err != nil {
        logger.WithField("err", err).Error("Get accounts error.")
        return
    }
    logger.Info("Wallet is: %v", account.Wallet)

    // Get Accounts
    userIDs := []string{"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}
    accounts, err := nk.AccountsGetId(ctx, userIDs)
    if err != nil {
        logger.WithField("err", err).Error("Get accounts error.")
        return
    }

    for _, account := range accounts {
        logger.Info("Wallet is: %v", account.Wallet)
    }

    // Update Account
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

    // Delete Account
    if err := nk.AccountDeleteId(ctx, "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592", false); err != nil {
        logger.WithField("err", err).Error("Delete account error.")
    }
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Get Account** | user_id | `string` | User ID to fetch information for. Must be valid UUID. | `table`: All account information including wallet, device IDs and more. |
    | **Get Accounts** | user_ids | `table` | An array of user IDs to fetch information for. Must be valid UUID. | `table`: An array of accounts. |
    | **Update Account** | user_id | `Opt. string` | User ID for which the information is to be updated. Must be valid UUID. |
    | | metadata | `Opt. table` | Metadata to update. Use `nil` if it is not being updated. |
    | | username | `Opt. string` | Username to be set. Must be unique. Use `nil` if it is not being updated. |
    | | display_name | `Opt. string` | Display name to be updated. Use `nil` if it is not being updated. |
    | | timezone | `Opt. string` | Timezone to be updated. Use `nil` if it is not being updated. |
    | | location | `Opt. string` | Location to be updated. Use `nil` if it is not being updated. |
    | | language | `Opt. string` | Lang tag to be updated. Use `nil` if it is not being updated. |
    | | avatar_url | `Opt. string` | User's avatar URL. Use `nil` if it is not being updated. |
    | **Delete Account** | user_id | `string` | User ID for which the information is to be updated. Must be valid UUID. |
    | | recorded | `bool` | Whether to record this deletion in the database. By default this is set to false. |

    Examples:
    ```lua
    -- Get Account
    local account = nk.account_get_id("8f4d52c7-bf28-4fcf-8af2-1d4fcf685592")
    local wallet = account.wallet
    nk.logger_info(string.format("Wallet is: %s", nk.json_encode(wallet)))

    -- Get Accounts
    local user_ids = {"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}
    local accounts = nk.accounts_get_id(user_ids)

    -- Update Account
    local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- Some user ID.
    local metadata = {}
    local username = ""
    local display_name = nil
    local timezone = nil
    local location = nil
    local language = nil
    local avatar_url = nil
    nk.account_update_id(user_id, metadata, username, display_name, timezone, location, language, avatar_url)

    -- Delete Account
    nk.account_delete_id("8f4d52c7-bf28-4fcf-8af2-1d4fcf685592", false)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Get Account** | user_id | `string` | User ID to fetch information for. Must be valid UUID. | `nkruntime.Account`: All account information including wallet, device IDs and more. |
    | **Get Accounts** | user_ids | `string[]` | An array of user IDs to fetch information for. Must be valid UUID. | `nkruntime.Account[]`: An array of accounts. |
    | **Update Account** | user_id | `Opt. string` | User ID for which the information is to be updated. Must be valid UUID. |
    | | metadata | `Opt. object` | Metadata to update. |
    | | username | `Opt. string` | Username to be set. Must be unique. Use `null` if it is not being updated. |
    | | display_name | `Opt. string` | Display name to be updated. Use `null` if it is not being updated. |
    | | timezone | `Opt. string` | Timezone to be updated. Use `null` if it is not being updated. |
    | | location | `Opt. string` | Location to be updated. Use `null` if it is not being updated. |
    | | language | `Opt. string` | Lang tag to be updated. Use `null` if it is not being updated. |
    | | avatar_url | `Opt. string` | User's avatar URL. Use `null` if it is not being updated. |
    | **Delete Account** | user_id | `string` | User ID for which the information is to be updated. Must be valid UUID. |
    | | recorded | `bool` | Whether to record this deletion in the database. By default this is set to false. |

    Examples:
    ```typescript
    // Get Account
    let account;
    try {
        account = nk.AccountGetId('8f4d52c7-bf28-4fcf-8af2-1d4fcf685592');
    } catch (error) {
        logger.error('An error occurred: %s', error);
        throw error;
    }
    logger.Info('Account: %s', JSON.stringify(account));

    // Get Accounts
    let accounts: nkruntime.Account[];
    try {
        accounts = nk.AccountsGetId(['8f4d52c7-bf28-4fcf-8af2-1d4fcf685592', 'a042c19c-2377-11eb-b7c1-cfafae11cfbc']);
    } catch (error) {
        logger.error('An error occurred: %s', error);
        throw error;
    }
    logger.Info('Accounts: %s', JSON.stringify(accounts));

    // Update Account
    let userID = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let username = null;
    let metadata = {pro: true};
    let displayName = 'new display name';
    let timezone = null
    let location = null
    let langTag = null
    let avatarUrl = null

    try {
        // Update metadata and display name only
        nk.accountUpdateId(userID, username, displayName, timezone, location, langTag, avatarUrl, metadata);
    } catch (error) {
        // handle error
    }

    // Delete Account
    let userID = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';

    try {
        nk.accountDeleteId(userID, false);
    } catch (error) {
        // handle error
    }
    ```

### Authenticate

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Authenticate Apple**: Authenticate user and create a session token using a Apple sign in token. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || token | `string` | Apple sign in token. |
    || username | `string` | If left empty, one is generated. |
    || create | `bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Custom**: Authenticate user and create a session token using a custom authentication managed by an external service or source not already supported by Nakama. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || id | `string` | Custom ID to use to authenticate the user. Must be between 6-128 characters. |
    || username | `string` | If left empty, one is generated. |
    || create | `bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Device**: Authenticate user and create a session token using a device identifier. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || id | `string` | Device ID to use to authenticate the user. Must be between 1 - 128 characters. |
    || username | `string` | If left empty, one is generated. |
    || create | `bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Email**: Authenticate user and create a session token using an email address and password.. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || email | `string` | Email address to use to authenticate the user. Must be between 10-255 characters. |
    || password | `string` | Password to set. Must be longer than 8 characters. |
    || username | `string` | If left empty, one is generated. |
    || create | `bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Facebook**: Authenticate user and create a session token using a Facebook account token. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || token | `string` | Facebook OAuth access token. |
    || import| `bool` | Whether to automatically import Facebook friends after authentication. This is `true` by default. |
    || username | `string` | If left empty, one is generated. |
    || create | `bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Facebook Instant Game**: Authenticate user and create a session token using a Facebook Instant Game. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || player info | `string` | Facebook Player info. |
    || username | `string` | If left empty, one is generated. |
    || create | `bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Game Center**: Authenticate user and create a session token using Apple Game Center credentials. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || player_id | `string` | PlayerId provided by GameCenter. |
    || bundle_id | `string` | BundleId of your app on iTunesConnect. |
    || timestamp | `int64` | Timestamp at which Game Center authenticated the client and issued a signature. |
    || salt | `string` | A random string returned by Game Center authentication on client. |
    || signature | `string` | A signature returned by Game Center authentication on client. |
    || public_key_url | `string` | A url to the public key returned by Game Center authentication on client. |
    || username | `string` | If left empty, one is generated. |
    || create | `bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Google**: Authenticate user and create a session token using a Google ID token. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || token | `string` | Google OAuth access token. |
    || username | `string` | If left empty, one is generated. |
    || create | `bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Steam**: Authenticate user and create a session token using a Steam account token. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || token | `string` | Steam token. |
    || username | `string` | If left empty, one is generated. |
    || create | `bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate token generator**: Generate a Nakama session token from a username. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || user_id | `string` | User ID to use to generate the token. |
    || username | `string` | If left empty, one is generated. |
    || expires_at | `number` | Optional. Number of seconds the token should be valid for. Default to [server configured expiry time](../getting-started/configuration.md#session). |

    Examples:
    ```go
    // Authenticate Apple
    userid, username, created, err := nk.AuthenticateApple(ctx, "some-oauth-access-token", "username", true)
    if err != nil {
    logger.WithField("err", err).Error("Authenticate custom error.")
    }

    // Authenticate Custom
    userid, username, created, err := nk.AuthenticateCustom(ctx, "48656C6C6F20776F726C64", "username", true)
    if err != nil {
        logger.WithField("err", err).Error("Authenticate custom error.")
    }

    // Authenticate Device
    userid, username, created, err := nk.AuthenticateDevice(ctx, "48656C6C6F20776F726C64", "username", true)
    if err != nil {
        logger.WithField("err", err).Error("Authenticate custom error.")
    }

    // Authenticate Email
    userid, username, created, err := nk.AuthenticateEmail(ctx, "email@example.com", "48656C6C6F20776F726C64", "username", true)
    if err != nil {
        logger.WithField("err", err).Error("Authenticate email error.")
    }

    // Authenticate Facebook
    userid, username, created, err := nk.AuthenticateFacebook(ctx, "some-oauth-access-token", true, "username", true)
    if err != nil {
        logger.WithField("err", err).Error("Authenticate facebook error.")
    }

    // Authenticate Facebook Instant Game
    userid, username, created, err := nk.AuthenticateFacebookInstantGame(ctx, "player-info", true, "username", true)
    if err != nil {
        logger.WithField("err", err).Error("Authenticate facebook error.")
    }

    // Authenticate Game Center
    userid, username, created, err := nk.AuthenticateGameCenter(ctx, playerID, bundleID, timestamp, salt, signature, publicKeyUrl, username, create)
    if err != nil {
        logger.WithField("err", err).Error("Authenticate game center error.")
    }

    // Authenticate Google
    userid, username, created, err := nk.AuthenticateGoogle(ctx, "some-id-token", "username", true)
    if err != nil {
        logger.WithField("err", err).Error("Authenticate google error.")
    }

    // Authenticate Steam
    userid, username, created, err := nk.AuthenticateSteam(ctx, "steam-token", "username", true)
    if err != nil {
        logger.WithField("err", err).Error("Authenticate steam error.")
    }

    // Authentication token generator
    token, validity, err := nk.AuthenticateTokenGenerate("user_id", "username", 0)
    if err != nil {
        logger.WithField("err", err).Error("Authenticate token generate error.")
        return
    }
    logger.Info("Session token: %q, valid for %v seconds", token, validity)
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Authenticate Apple**: Authenticate user and create a session token using a Apple sign in token. | token | `string` | Apple sign in token. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Custom**: Authenticate user and create a session token using a custom authentication managed by an external service or source not already supported by Nakama. | id | `string` | Custom ID to use to authenticate the user. Must be between 6-128 characters. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Device**: Authenticate user and create a session token using a device identifier. | id | `string` | Device ID to use to authenticate the user. Must be between 1 - 128 characters. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Email**: Authenticate user and create a session token using an email address and password. | email | `string` | Email address to use to authenticate the user. Must be between 10-255 characters. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || password | `string` | Password to set. Must be longer than 8 characters. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Facebook**: Authenticate user and create a session token using a Facebook account token. | token | `string` | Facebook OAuth access token. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || import| `bool` | Whether to automatically import Facebook friends after authentication. This is `true` by default. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Facebook Instant Game**: Authenticate user and create a session token using a Facebook Instant Game. | player info | `string` | Facebook Player info. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Game Center**: Authenticate user and create a session token using Apple Game Center credentials. | player_id | `string` | PlayerId provided by GameCenter. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || bundle_id | `string` | BundleId of your app on iTunesConnect. |
    || timestamp | `number` | Timestamp at which Game Center authenticated the client and issued a signature. |
    || salt | `string` | A random string returned by Game Center authentication on client. |
    || signature | `string` | A signature returned by Game Center authentication on client. |
    || public_key_url | `string` | A url to the public key returned by Game Center authentication on client. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Google**: Authenticate user and create a session token using a Google ID token. | token | `string` | Google OAuth access token. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Steam**: Authenticate user and create a session token using a Steam account token. | token | `string` | Steam token. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate token generator**: Generate a Nakama session token from a username. | user_id | `string` | User ID to use to generate the token. | The user's ID (`string`), username (`string`), and a boolean flag indicating if the account was just created (`true`) or already existed (`false`). |
    || username | `Opt. string` | If left empty, one is generated. |
    || expires_at | `Opt. number` | Optional. Number of seconds the token should be valid for. Default to [server configured expiry time](../getting-started/configuration.md#session). |

    Examples:
    ```lua
    -- Authenticate Apple
    local user_id, username, created = nk.authenticate_apple("some-oauth-access-token", "username", true)

    -- Authenticate Custom
    local user_id, username, created = nk.authenticate_custom("48656C6C6F20776F726C64", "username", true)

    -- Authenticate Device
    local user_id, username, created = nk.authenticate_device("48656C6C6F20776F726C64", "username", true)

    -- Authenticate Email
    local user_id, username, created = nk.authenticate_email("email@example.com", "48656C6C6F20776F726C64", "username", true)

    -- Authenticate Facebook
    local user_id, username, created = nk.authenticate_facebook("some-oauth-access-token", true, "username", true)

    -- Authenticate Facebook Instant Game
    local user_id, username, created = nk.authenticate_facebook_instant_game("player-info", true, "username", true)

    -- Authenticate Game Center
    local user_id, username, created = nk.authenticate_game_center(player_id, bundle_id, timestamp, salt, signature, public_key_url, username, create)

    -- Authenticate Google
    local user_id, username, created = nk.authenticate_google("some-id-token", "username", true)

    -- Authenticate Steam
    local user_id, username, created = nk.authenticate_steam("steam-token", "username", true)

    -- Authentication token generator
    local token, exp = nk.authenticate_token_generate("user_id", "username")
    nk.logger_info(("Access token: %q, valid for %q seconds"):format(token, exp))
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Authenticate Apple**: Authenticate user and create a session token using a Apple sign in token. | token | `string` | Apple sign in token. | An object of type `nkruntime.AuthResult`. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Custom**: Authenticate user and create a session token using a custom authentication managed by an external service or source not already supported by Nakama. | id | `string` | Custom ID to use to authenticate the user. Must be between 6-128 characters. | An object of type `nkruntime.AuthResult`. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Device**: Authenticate user and create a session token using a device identifier. | id | `string` | Device ID to use to authenticate the user. Must be between 1 - 128 characters. | An object of type `nkruntime.AuthResult`. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Email**: Authenticate user and create a session token using an email address and password. | email | `string` | Email address to use to authenticate the user. Must be between 10-255 characters. | An object of type `nkruntime.AuthResult`. |
    || password | `string` | Password to set. Must be longer than 8 characters. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Facebook**: Authenticate user and create a session token using a Facebook account token. | token | `string` | Facebook OAuth access token. | An object of type `nkruntime.AuthResult`. |
    || import| `bool` | Whether to automatically import Facebook friends after authentication. This is `true` by default. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Facebook Instant Game**: Authenticate user and create a session token using a Facebook Instant Game. | player info | `string` | Facebook Player info. | An object of type `nkruntime.AuthResult`. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Game Center**: Authenticate user and create a session token using Apple Game Center credentials. | player_id | `string` | PlayerId provided by GameCenter. | An object of type `nkruntime.AuthResult`. |
    || bundle_id | `string` | BundleId of your app on iTunesConnect. |
    || timestamp | `number` | Timestamp at which Game Center authenticated the client and issued a signature. |
    || salt | `string` | A random string returned by Game Center authentication on client. |
    || signature | `string` | A signature returned by Game Center authentication on client. |
    || public_key_url | `string` | A url to the public key returned by Game Center authentication on client. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Google**: Authenticate user and create a session token using a Google ID token. | token | `string` | Google OAuth access token. | An object of type `nkruntime.AuthResult`. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate Steam**: Authenticate user and create a session token using a Steam account token. | token | `string` | Steam token. | An object of type `nkruntime.AuthResult`. |
    || username | `Opt. string` | If left empty, one is generated. |
    || create | `Opt. bool` | Create user if one didn't exist previously. By default this is set to `true`. |
    | **Authenticate token generator**: Generate a Nakama session token from a username. | user_id | `string` | User ID to use to generate the token. | An object of type `nkruntime.AuthResult`. |
    || username | `Opt. string` | If left empty, one is generated. |
    || expires_at | `Opt. number` | Optional. Number of seconds the token should be valid for. Default to [server configured expiry time](../getting-started/configuration.md#session). |

    Examples:
    ```ts
    // Authenticate Apple
    let result = {} as nkruntime.AuthResult;
    try {
        result = nk.authenticateApple('some-oauth-access-token', 'username', true);
    } catch (error) {
        // Handle error
    }

    // Authenticate Custom
    let result = {} as nkruntime.AuthResult;
    try {
        result = nk.authenticateCustom('48656C6C6F20776F726C64', 'username', true);
    } catch (error) {
        // Handle error
    }

    // Authenticate Device
    let result = {} as nkruntime.AuthResult;
    try {
        result = nk.authenticateDevice('48656C6C6F20776F726C64', 'username', true);
    } catch (error) {
        // Handle error
    }

    // Authenticate Email
    let result = {} as nkruntime.AuthResult;
    try {
        result = nk.authenticateEmail('email@example.com', 'username', true);
    } catch (error) {
        // Handle error
    }

    // Authenticate Facebook
    let result = {} as nkruntime.AuthResult;
    try {
        result = nk.authenticateFacebook('some-oauth-access-token', 'username', true);
    } catch (error) {
        // Handle error
    }

    // Authenticate Facebook Instant Game
    let result = {} as nkruntime.AuthResult;
    try {
        result = nk.authenticateFacebookInstantGame('player-info', 'username', true);
    } catch (error) {
        // Handle error
    }

    // Authenticate Game Center
    let result = {} as nkruntime.AuthResult;
    try {
        // Example assumes arguments are defined.
        result = nk.authenticateGameCenter(playerId, bundleId, timestamp, salt, signature, publicKeyUrl, username, create);
    } catch (error) {
        // Handle error
    }

    // Authenticate Google
    let result = {} as nkruntime.AuthResult;
    try {
        result = nk.authenticateGoogle('some-id-token', 'username', true);
    } catch (error) {
        // Handle error
    }

    // Authenticate Steam
    let result = {} as nkruntime.AuthResult;
    try {
        result = nk.authenticateSteam('steam-token', 'username', true);
    } catch (error) {
        // Handle error
    }

    // Authentication token generator
    let result = {} as nkruntime.TokenGenerateResult;
    try {
        result = nk.authenticateTokenGenerate('steam-token', 'username', true);
    } catch (error) {
        // Handle error
    }
    ```

## Friends

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Friends List**: List all friends, invites, invited, and blocked which belong to a user. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `*api.FriendList`: The user information for users that are friends of the current user. |
    | | user_id | `string` | The ID of the user who's friends, invites, invited, and blocked you want to list. |
    | | limit | `int` | The number of friends to retrieve in this page of results. No more than 1000 limit allowed per result. |
    | | state | `int` | The state of the friendship with the user. If unspecified this returns friends in all states for the user. |
    | | cursor | `string` | The cursor returned from a previous listing request. Used to obtain the next page of results. |

    Example:
    ```go
    userID := "b1aafe16-7540-11e7-9738-13777fcc7cd8"
    limit := 100
    state := 0
    cursor := ""

    friends, err := nk.FriendsList(ctx, userID, limit, state, cursor)
    if err != nil {
      logger.WithField("err", err).Error("nk.FriendsList error.")
      return
    }

    for _, friend := range friends {
        // States are: friend(0), invite_sent(1), invite_received(2), blocked(3)
        logger.Info("Friend username %s has state %d", friend.GetUser().Username, friend.GetState())
    }
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Friends List**: List all friends, invites, invited, and blocked which belong to a user. | user_id | `string` | The ID of the user who's friends, invites, invited, and blocked you want to list. | `*table`: The user information for users that are friends of the current user. |
    | | limit | `Opt. number` | The number of friends to retrieve in this page of results. No more than 1000 limit allowed per result. |
    | | state | `Opt. number` | The state of the friendship with the user. If unspecified this returns friends in all states for the user. |
    | | cursor | `Opt. string` | The cursor returned from a previous listing request. Used to obtain the next page of results. |

    Example:
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

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Friends List**: List all friends, invites, invited, and blocked which belong to a user. | user_id | `string` | The ID of the user who's friends, invites, invited, and blocked you want to list. | `*nkruntime.FriendList`: The user information for users that are friends of the current user. |
    | | limit | `Opt. number` | The number of friends to retrieve in this page of results. No more than 1000 limit allowed per result. |
    | | state | `Opt. number` | The state of the friendship with the user. If unspecified this returns friends in all states for the user. |
    | | cursor | `Opt. string` | The cursor returned from a previous listing request. Used to obtain the next page of results. |

    Example:
    ```typescript
    let friends = {} as nkruntime.FriendList;
    try {
        let userId = 'b1aafe16-7540-11e7-9738-13777fcc7cd8';
        let limit = 100;
        let state = 0;
        friends = nk.friendsList(userId, limit, state);
    } catch (error) {
        // Handle error
    }
    friends.friends?.forEach((f) => {
        // States are: friend(0), invite_sent(1), invite_received(2), blocked(3)
        logger.info('Friend username: %s has state %d', f.user?.username, f.state);
    });
    ```

## Groups

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Group Create**: Setup a group with various configuration settings. The group will be created if they don't exist or fail if the group name is taken. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | (string) - The `group_id` of the newly created group. |
    | | user_id | `string` | Mandatory. The user ID to be associated as the group superadmin. |
    | | name | `string` | Mandatory. Group name, must be unique. |
    | | creator_id | `string` | The user ID to be associated as creator. If not set or nil/null system user will be set. |
    | | lang | `string` | Group language. If not set will default to 'en'. |
    | | description | `string` | Group description, can be left empty as nil/null. |
    | | avatar_url | `string` | URL to the group avatar, can be left empty as nil/null. |
    | | open | `bool` | Whether the group is for anyone to join, or members will need to send invitations to join. Defaults to false. |
    | | metadata | `map[string]interface{}` | Custom information to store for this group. Can be left empty as nil/null. |
    | | max_count | `int` | Maximum number of members to have in the group. Defaults to 100. |
    | **Group Delete**: Delete a group. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | group_id | `string` | The ID of the group to delete. |
    | **Group Update**: Update a group with various configuration settings. The group which is updated can change some or all of its fields. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | group_id | `string` | The ID of the group to update. |
    | | name | `string` | Group name, can be empty if not changed. |
    | | creator_id | `string` | The user ID to be associated as creator. Can be empty if not changed. |
    | | lang | `string` | Group language. Empty if not updated. |
    | | description | `string` | Group description, can be left empty if not updated. |
    | | avatar_url | `string` | URL to the group avatar, can be left empty if not updated. |
    | | open | `bool` | Whether the group is for anyone to join or not. Use `nil` if field is not being updated. |
    | | metadata | `map[string]interface{}` | Custom information to store for this group. Use `nil` if field is not being updated. |
    | | max_count | `int` | Maximum number of members to have in the group. Use `0`, nil/null if field is not being updated. |
    | **Group Users List**: List all members, admins and superadmins which belong to a group. This also list incoming join requests. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `[]*api.GroupUserList_GroupUser`:The user information for members, admins and superadmins for the group. Also users who sent a join request. |
    | | group_id | `string` | The ID of the group to list members for. |
    | **Group User Join**: Join a group for a particular user. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | group_id | `string` | The ID of the group to join. |
    | | user_id | `string` | The user ID to add to this group. |
    | | username | `string` | The username of the user to add to this group. |
    | **Group User Leave**: Leave a group for a particular user. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | group_id | `string` | The ID of the group to leave. |
    | | user_id | `string` | The user ID to remove from this group. |
    | | username | `string` | The username of the user leaving this group. |
    | **Group Users Add**: Add users to a group. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | group_id | `string` | The ID of the group to add users to. |
    | | user_ids | `[]string` | A table array of user ids to add. |
    | **Group Users Kick**: Kick users from a group. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | group_id | `string` | The ID of the group to kick users from. |
    | | user_ids | `[]string` | A table array of user ids to kick. |
    | **Group Users Promote**: Promote users in a group. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | group_id | `string` | The ID of the group who's members you want to promote. |
    | | user_ids | `[]string` | A table array of user ids to promote. |
    | **Group Users Demote**: Demote users in a group. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | group_id | `string` | The ID of the group who's members you want to demote. |
    | | user_ids | `[]string` | A table array of user ids to demote. |
    | **Groups Get by ID**: Fetch one or more groups by their ID. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | A table (array) of groups with their fields. |
    | | group_ids | `[]string` | A set of strings of the ID for the groups to get. |
    | **Groups List for a user**: List all groups which a user belongs to and whether they've been accepted or if it's an invite. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | A table (array) of groups with their fields. | `[]*api.UserGroupList_UserGroup`: A list of groups for the user. |
    | | user_id | `string` | The ID of the user who's groups you want to list. |

    Examples:
    ```go
    // Group Create
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

    group, err := nk.GroupCreate(ctx, userID, name, creatorID, langTag, description, avatarURL, open, metadata, maxCount)
    if err != nil {
        logger.WithField("err", err).Error("Group create error.")
    }

    // Group Delete
    groupID := "f00fa79a-750f-11e7-8626-0fb79f45ff97"
    if group, err := nk.GroupDelete(ctx, groupID); err != nil {
        logger.WithField("err", err).Error("Group delete error.")
    }

    // Group Update
    metadata := map[string]interface{}{ // Add whatever custom fields you want.
    "my_custom_field": "some value",
    }

    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    description := "An updated description."

    if err := nk.GroupUpdate(ctx, groupID, "", "", "", description, "", true, metadata, 0); err != nil {
        logger.WithField("err", err).Error("Group update error.")
    }

    // Group Users List
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"

    groupUserList, err := nk.GroupUsersList(ctx, groupID)
    if err != nil {
    logger.WithField("err", err).Error("Group users list error.")
    return
    }

    for _, member := range groupUserList {
        // States are => 0: Superadmin, 1: Admin, 2: Member, 3: Requested to join
        logger.Info("Member username %s has state %d", member.GetUser().Username, member.GetState())
    }

    // Group User Join
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userID := "9a51cf3a-2377-11eb-b713-e7d403afe081"
    username := "myusername"

    if err := nk.GroupUserJoin(ctx, groupID, userID, username); err != nil {
        logger.WithField("err", err).Error("Group user join error.")
    }

    // Group User Leave
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userID := "9a51cf3a-2377-11eb-b713-e7d403afe081"
    username := "myusername"

    if err := nk.GroupUserLeave(ctx, groupID, userID, username); err != nil {
        logger.WithField("err", err).Error("Group user leave error.")
    }

    // Group Users Add
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userIDs := []string{"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    if err := nk.GroupUsersAdd(ctx, groupID, userIDs); err != nil {
        logger.WithField("err", err).Error("Group users add error.")
    }

    // Group Users Kick
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userIds := []string{"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    if err := nk.GroupUsersKick(ctx, groupID, userIds); err != nil {
        logger.WithField("err", err).Error("Group users kick error.")
    }

    // Group Users Promote
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userIDs := []string{"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    if err := nk.GroupUsersPromote(ctx, groupID, userIDs); err != nil {
        logger.WithField("err", err).Error("Group users promote error.")
    }

    // Group Users Demote
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"
    userIds := []string{"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    if err := nk.GroupUsersDemote(ctx, groupID, userIDs); err != nil {
        logger.WithField("err", err).Error("Group users demote error.")
    }

    // Groups Get by ID
    groupID := "dcb891ea-a311-4681-9213-6741351c9994"

    groups, err := nk.GroupsGetId(ctx, []string{groupID})
    if err != nil {
        logger.WithField("err", err).Error("Groups get by ID error.")
        return
    }

    for _, group := range groups {
        logger.Info("Group name %s with id %s.", group.Name, group.Id)
    }

    // Groups List for a User
    userID := "dcb891ea-a311-4681-9213-6741351c9994"

    groups, err := nk.UserGroupsList(ctx, userID)
    if err != nil {
        logger.WithField("err", err).Error("User groups list error.")
        return
    }

    for _, group := range groups {
        logger.Printf("User has state %d in group %s.", group.GetState(), group.GetGroup().Name)
    }
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Group Create**: Setup a group with various configuration settings. The group will be created if they don't exist or fail if the group name is taken. | user_id | `string` | Mandatory. The user ID to be associated as the group superadmin. | (string) - The `group_id` of the newly created group. |
    | | name | `string` | Mandatory. Group name, must be unique. |
    | | creator_id | `Opt. string` | The user ID to be associated as creator. If not set or nil/null system user will be set. |
    | | lang | `Opt. string` | Group language. If not set will default to 'en'. |
    | | description | `Opt. string` | Group description, can be left empty as nil/null. |
    | | avatar_url | `Opt. string` | URL to the group avatar, can be left empty as nil/null. |
    | | open | `Opt. bool` | Whether the group is for anyone to join, or members will need to send invitations to join. Defaults to false. |
    | | metadata | `Opt. table` | Custom information to store for this group. Can be left empty as nil/null. |
    | | max_count | `number` | Maximum number of members to have in the group. Defaults to 100. |
    | **Group Delete**: Delete a group. | group_id | `string` | The ID of the group to delete. |
    | **Group Update**: Update a group with various configuration settings. The group which is updated can change some or all of its fields. | group_id | `string` | The ID of the group to update. |
    | | name | `string` | Group name, can be empty if not changed. |
    | | creator_id | `Opt. string` | The user ID to be associated as creator. Can be empty if not changed. |
    | | lang | `Opt. string` | Group language. Empty if not updated. |
    | | description | `Opt. string` | Group description, can be left empty if not updated. |
    | | avatar_url | `Opt. string` | URL to the group avatar, can be left empty if not updated. |
    | | open | `Opt. bool` | Whether the group is for anyone to join or not. Use `nil` if field is not being updated. |
    | | metadata | `Opt. table` | Custom information to store for this group. Use `nil` if field is not being updated. |
    | | max_count | `Opt. number` | Maximum number of members to have in the group. Use `0`, nil/null if field is not being updated. |
    | **Group Users List**: List all members, admins and superadmins which belong to a group. This also list incoming join requests. | group_id | `string` | The ID of the group to list members for. | `table`:The user information for members, admins and superadmins for the group. Also users who sent a join request. |
    | **Group User Join**: Join a group for a particular user. | group_id | `string` | The ID of the group to join. |
    | | user_id | `string` | The user ID to add to this group. |
    | | username | `string` | The username of the user to add to this group. |
    | **Group User Leave**: Leave a group for a particular user. | group_id | `string` | The ID of the group to leave. |
    | | user_id | `string` | The user ID to remove from this group. |
    | | username | `string` | The username of the user leaving this group. |
    | **Group Users Add**: Add users to a group. | group_id | `string` | The ID of the group to add users to. |
    | | user_ids | `table` | A table array of user ids to add. |
    | **Group Users Kick**: Kick users from a group. | group_id | `string` | The ID of the group to kick users from. |
    | | user_ids | `table` | A table array of user ids to kick. |
    | **Group Users Promote**: Promote users in a group. | group_id | `string` | The ID of the group who's members you want to promote. |
    | | user_ids | `table` | A table array of user ids to promote. |
    | **Group Users Demote**: Demote users in a group. | group_id | `string` | The ID of the group who's members you want to demote. |
    | | user_ids | `table` | A table array of user ids to demote. |
    | **Groups Get by ID**: Fetch one or more groups by their ID. | group_ids | `table` | A set of strings of the IDs for the groups to get. | A table (array) of groups with their fields.
    | **Groups List for a user**: List all groups which a user belongs to and whether they've been accepted or if it's an invite. | user_id | `string` | The ID of the user who's groups you want to list. | `table`: A list of groups for the user. |

    Examples:
    ```lua
    -- Group Create
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

    -- Group Delete
    local group_id = "f00fa79a-750f-11e7-8626-0fb79f45ff97"
    nk.group_delete(group_id)

    -- Group Update
    local metadata = {
    some_field = "some value"
    }
    group_id = "f00fa79a-750f-11e7-8626-0fb79f45ff97"
    description = "An updated description."

    nk.group_update(group_id, "", "", "", description, "", nil, metadata, 0)

    -- Group Users List
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local members = nk.group_users_list(group_id)
    for _, m in ipairs(members)
    do
    local msg = string.format("Member username %q has state %q", m.user.username, m.state)
    nk.logger_info(msg)
    end

    -- Group User Join
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_id = "9a51cf3a-2377-11eb-b713-e7d403afe081"
    local username = "myusername"

    nk.group_user_join(group_id, user_id, username)

    -- Group User Leave
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_id = "9a51cf3a-2377-11eb-b713-e7d403afe081"
    local username = "myusername"

    nk.group_user_leave(group_id, user_id, username)

    -- Group Users Add
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_ids = {"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    nk.group_users_add(group_id, user_ids)

    -- Group Users Kick
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_ids = {"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    nk.group_users_kick(group_id, userna)

    -- Group Users Promote
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_ids = {"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    nk.group_users_promote(group_id, user_ids)

    -- Group Users Demote
    local group_id = "a1aafe16-7540-11e7-9738-13777fcc7cd8"
    local user_ids = {"9a51cf3a-2377-11eb-b713-e7d403afe081", "a042c19c-2377-11eb-b7c1-cfafae11cfbc"}

    nk.group_users_demote(group_id, user_ids)
    -- Groups Get by ID
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
    -- Groups List for a User
    local user_id = "64ef6cb0-7512-11e7-9e52-d7789d80b70b"
    local groups = nk.user_groups_list(user_id)
    for _, g in ipairs(groups)
    do
    local msg = string.format("User has state %q in group %q", g.state, g.group.name)
    nk.logger_info(msg)
    end
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Group Create**: Setup a group with various configuration settings. The group will be created if they don't exist or fail if the group name is taken. | user_id | `string` | Mandatory. The user ID to be associated as the group superadmin. | (string) - The `group_id` of the newly created group. |
    | | name | `string` | Mandatory. Group name, must be unique. |
    | | creator_id | `Opt. string` | The user ID to be associated as creator. If not set or nil/null system user will be set. |
    | | lang | `Opt. string` | Group language. If not set will default to 'en'. |
    | | description | `Opt. string` | Group description, can be left empty as nil/null. |
    | | avatar_url | `Opt. string` | URL to the group avatar, can be left empty as nil/null. |
    | | open | `Opt. bool` | Whether the group is for anyone to join, or members will need to send invitations to join. Defaults to false. |
    | | metadata | `Opt. object` | Custom information to store for this group. Can be left empty as nil/null. |
    | | max_count | `number` | Maximum number of members to have in the group. Defaults to 100. |
    | **Group Delete**: Delete a group. | group_id | `string` | The ID of the group to delete. |
    | **Group Update**: Update a group with various configuration settings. The group which is updated can change some or all of its fields. | group_id | `string` | The ID of the group to update. |
    | | name | `string` | Group name, can be empty if not changed. |
    | | creator_id | `Opt. string` | The user ID to be associated as creator. Can be empty if not changed. |
    | | lang | `Opt. string` | Group language. Empty if not updated. |
    | | description | `Opt. string` | Group description, can be left empty if not updated. |
    | | avatar_url | `Opt. string` | URL to the group avatar, can be left empty if not updated. |
    | | open | `Opt. bool` | Whether the group is for anyone to join or not. Use `nil` if field is not being updated. |
    | | metadata | `Opt. object` | Custom information to store for this group. Use `nil` if field is not being updated. |
    | | max_count | `Opt. number` | Maximum number of members to have in the group. Use `0`, nil/null if field is not being updated. |
    | **Group Users List**: List all members, admins and superadmins which belong to a group. This also list incoming join requests. | group_id | `string` | The ID of the group to list members for. | `nkruntime.GroupUserList`:The user information for members, admins and superadmins for the group. Also users who sent a join request. |
    | **Group User Join**: Join a group for a particular user. | group_id | `string` | The ID of the group to join. |
    | | user_id | `string` | The user ID to add to this group. |
    | | username | `string` | The username of the user to add to this group. |
    | **Group User Leave**: Leave a group for a particular user. | group_id | `string` | The ID of the group to leave. |
    | | user_id | `string` | The user ID to remove from this group. |
    | | username | `string` | The username of the user leaving this group. |
    | **Group Users Add**: Add users to a group. | group_id | `string` | The ID of the group to add users to. |
    | | user_ids | `string[]` | A table array of user ids to add. |
    | **Group Users Kick**: Kick users from a group. | group_id | `string` | The ID of the group to kick users from. |
    | | user_ids | `string[]` | A table array of user ids to kick. |
    | **Group Users Promote**: Promote users in a group. | group_id | `string` | The ID of the group who's members you want to promote. |
    | | user_ids | `string[]` | A table array of user ids to promote. |
    | **Group Users Demote**: Demote users in a group. | group_id | `string` | The ID of the group who's members you want to demote. |
    | | user_ids | `string[` | A table array of user ids to demote. |
    | **Groups Get by ID**: Fetch one or more groups by their ID. | group_ids | `string[` | A set of strings of the IDs for the groups to get. | A table (array) of groups with their fields.
    | **Groups List for a user**: List all groups which a user belongs to and whether they've been accepted or if it's an invite. | user_id | `string` | The ID of the user who's groups you want to list. | `nkruntime.UserGroupList`: A list of groups for the user. |

    Examples:
    ```typescript
    // Group Create
    let userId = 'dcb891ea-a311-4681-9213-6741351c9994';
    let creatorId = 'dcb891ea-a311-4681-9213-6741351c9994';
    let name = 'Some unique group name';
    let description = 'My awesome group.';
    let lang = 'en';
    let open = true;
    let avatarURL = 'url://somelink';
    let metadata = { custom_field: 'some_value' };
    let maxCount = 100;

    let group = {} as nkruntime.Group;
    try {
        group = nk.groupCreate(userId, name, creatorId, lang, description, avatarURL, open, metadata, maxCount);
    } catch (error) {
        // Handle error
    }

    // Group Delete
    try {
        nk.groupdDelete('f00fa79a-750f-11e7-8626-0fb79f45ff97');
    } catch (error) {
        // Handle error
    }

    // Group Update
    let metadata = { someField: 'some value' };
    let groupId = 'f00fa79a-750f-11e7-8626-0fb79f45ff97';
    let description = 'An updated description';

    try {
        nk.groupUpdate(groupId, null, null, null, description, null, true, metadata);
    } catch (error) {
        // Handle error.
    }

    // Group Users List
    let groupId = 'dcb891ea-a311-4681-9213-6741351c9994';

    let groupUsers = {} as nkruntime.GroupUserList;
    try {
        groupUsers = nk.groupUsersList(groupId);
    } catch (error) {
        // Handle error
    }

    groupUsers.groupUsers?.forEach(gu => {
        // States are => 0: Superadmin, 1: Admin, 2: Member, 3: Requested to join
        logger.info('Member username: %s has state %d', gu.user.username, gu.state);
    });

    // Group User Join
    let groupId = 'dcb891ea-a311-4681-9213-6741351c9994';
    let userId = '9a51cf3a-2377-11eb-b713-e7d403afe081';
    let username = 'myusername';

    try {
        nk.groupUserJoin(groupId, userId, username);
    } catch (error) {
        // Handle error
    }

    // Group User Leave
    let groupId = 'dcb891ea-a311-4681-9213-6741351c9994';
    let userId = '9a51cf3a-2377-11eb-b713-e7d403afe081';
    let username = 'myusername';

    try {
        nk.groupUserLeave(groupId, userId, username);
    } catch (error) {
        // Handle error
    }

    // Group Users Add
    let groupId = 'dcb891ea-a311-4681-9213-6741351c9994';
    let userIds = ['9a51cf3a-2377-11eb-b713-e7d403afe081', 'a042c19c-2377-11eb-b7c1-cfafae11cfbc'];

    try {
        nk.groupUsersAdd(groupId, userIds);
    } catch (error) {
        // Handle error
    }

    // Group Users Kick
    let groupId = 'dcb891ea-a311-4681-9213-6741351c9994';
    let userIds = ['9a51cf3a-2377-11eb-b713-e7d403afe081', 'a042c19c-2377-11eb-b7c1-cfafae11cfbc'];

    try {
        nk.groupUsersKick(groupId, userIds);
    } catch (error) {
        // Handle error
    }

    // Group Users Promote
    let groupId = 'dcb891ea-a311-4681-9213-6741351c9994';
    let userIds = ['9a51cf3a-2377-11eb-b713-e7d403afe081', 'a042c19c-2377-11eb-b7c1-cfafae11cfbc'];

    try {
        nk.groupUsersPromote(groupId, userIds);
    } catch (error) {
        // Handle error
    }

    // Group Users Demote
    let groupId = 'dcb891ea-a311-4681-9213-6741351c9994';
    let userIds = ['9a51cf3a-2377-11eb-b713-e7d403afe081', 'a042c19c-2377-11eb-b7c1-cfafae11cfbc'];

    try {
        nk.groupUsersDemote(groupId, userIds);
    } catch (error) {
        // Handle error
    }

    // Groups Get by ID
    let groups: nkruntime.Group[];
    try {
        let groupIds = ['dcb891ea-a311-4681-9213-6741351c9994'];
        groups = nk.groupsGetId(groupIds);
    } catch (error) {
        // Handle error
    }

    // Groups List for a User
    let userId = '64ef6cb0-7512-11e7-9e52-d7789d80b70b';

    let groups = {} as nkruntime.UserGroupList;
    try {
        groups = nk.userGroupsList(userId);
    } catch (error) {
        // Handle error
    }
    ```

## Leaderboards

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Leaderboard Create**: Setup a new dynamic leaderboard with the specified ID and various configuration settings. The leaderboard will be created if it doesn't already exist, otherwise its configuration will *not* be updated. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | id | `string` | The unique identifier for the new leaderboard. This is used by clients to submit scores. |
    | | authoritative | `bool` | Mark the leaderboard as authoritative which ensures updates can only be made via the Lua runtime. No client can submit a score directly. Default false. |
    | | sort | `string` | The sort order for records in the leaderboard; possible values are "asc" or "desc". Default "desc". |
    | | operator | `string` | The operator that determines how scores behave when submitted; possible values are "best", "set", or "incr". Default "best". |
    | | reset | `string` | The cron format used to define the reset schedule for the leaderboard. This controls when a leaderboard is reset and can be used to power daily/weekly/monthly leaderboards. |
    | | metadata | `map[string]interface{}` | The metadata you want associated to the leaderboard. Some good examples are weather conditions for a racing game. |
    | **Leaderboard Delete**: Delete a leaderboard and all scores that belong to it. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | id | `string` | The unique identifier for the leaderboard to delete. |
    | **Leaderboard Record write**: Use the preconfigured operator for the given leaderboard to submit a score for a particular user. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | id | `string` | The unique identifier for the leaderboard to submit to. |
    | | owner | `string` | The owner of this score submission. Mandatory field. |
    | | username | `string` | The owner username of this score submission, if it's a user. |
    | | score | `int64` | The score to submit. Default 0. |
    | | subscore | `int64` | A secondary subscore parameter for the submission. Default 0. |
    | | metadata | `map[string]interface{}` | The metadata you want associated to this submission. Some good examples are weather conditions for a racing game. |
    | **Leaderboard Record delete**: Remove an owner's record from a leaderboard, if one exists. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | id | `string` | The unique identifier for the leaderboard to delete from. Mandatory field. |
    | | owner | `string` | The owner of the score to delete. Mandatory field. |
    | **Leaderboard Record list**: List records on the specified leaderboard, optionally filtering to only a subset of records by their owners. Records will be listed in the preconfigured leaderboard sort order. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | A page of leaderboard records, a list of owner leaderboard records (empty if the `owners` input parameter is not set), an optional next page cursor that can be used to retrieve the next page of records (if any), and an optional previous page cursor that can be used to retrieve the previous page of records (if any). |
    | | id | `string` | The unique identifier for the leaderboard to list. Mandatory field. |
    | | owners | `[]string` | Array of owners to filter to. |
    | | limit | `int` | The maximum number of records to return from 10 to 100. |
    | | cursor | `string` | A cursor used to fetch the next page when applicable. |

    Examples:
    ```go
    // Leaderboard Create
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

    // Leaderboard Delete
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    if err := nk.LeaderboardDelete(ctx, id); err != nil {
    logger.WithField("err", err).Error("Leaderboard delete error.")
    }

    // Leaderboard Record Write
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

    // Leaderboard Record Delete
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    ownerID := "4c2ae592-b2a7-445e-98ec-697694478b1c"

    if err := nk.LeaderboardRecordDelete(ctx, id, ownerID); err != nil {
    logger.WithField("err", err).Error("Leaderboard record delete error.")
    }

    // Leaderboard Record list
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

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Leaderboard Create**: Setup a new dynamic leaderboard with the specified ID and various configuration settings. The leaderboard will be created if it doesn't already exist, otherwise its configuration will *not* be updated. | id | `string` | The unique identifier for the new leaderboard. This is used by clients to submit scores. |
    | | authoritative | `bool` | Mark the leaderboard as authoritative which ensures updates can only be made via the Lua runtime. No client can submit a score directly. Default false. |
    | | sort | Opt. `string` | The sort order for records in the leaderboard; possible values are "asc" or "desc". Default "desc". |
    | | operator | Opt. `string` | The operator that determines how scores behave when submitted; possible values are "best", "set", or "incr". Default "best". |
    | | reset | Opt. `string` | The cron format used to define the reset schedule for the leaderboard. This controls when a leaderboard is reset and can be used to power daily/weekly/monthly leaderboards. |
    | | metadata | Opt. `table` | The metadata you want associated to the leaderboard. Some good examples are weather conditions for a racing game. |
    | **Leaderboard Delete**: Delete a leaderboard and all scores that belong to it. | id | `string` | The unique identifier of the leaderboard to delete. |
    | **Leaderboard Record write**: Use the preconfigured operator for the given leaderboard to submit a score for a particular user. | id | `string` | The unique identifier for the leaderboard to submit to. |
    | | owner | `string` | The owner of this score submission. Mandatory field. |
    | | username | Opt. `string` | The owner username of this score submission, if it's a user. |
    | | score | Opt. `number` | The score to submit. Default 0. |
    | | subscore | Opt. `number` | A secondary subscore parameter for the submission. Default 0. |
    | | metadata | Opt. `table` | The metadata you want associated to this submission. Some good examples are weather conditions for a racing game. |
    | **Leaderboard Record delete**: Remove an owner's record from a leaderboard, if one exists. | id | `string` | The unique identifier for the leaderboard to delete from. Mandatory field.
    | | owner | `string` | The owner of the score to delete. Mandatory field. |
    | **Leaderboard Record list**: List records on the specified leaderboard, optionally filtering to only a subset of records by their owners. Records will be listed in the preconfigured leaderboard sort order. | id | `string` | The unique identifier for the leaderboard to list. Mandatory field. | A page of leaderboard records, a list of owner leaderboard records (empty if the `owners` input parameter is not set), an optional next page cursor that can be used to retrieve the next page of records (if any), and an optional previous page cursor that can be used to retrieve the previous page of records (if any). |
    | | id | `string` | The unique identifier for the leaderboard to list. Mandatory field. |
    | | owners | Opt. `table` | Array of owners to filter to. |
    | | limit | Opt. `number` | The maximum number of records to return from 10 to 100. |
    | | cursor | Opt. `string` | A cursor used to fetch the next page when applicable. |

    Examples:
    ```lua
    -- Leaderboard Create
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local authoritative = false
    local sort = "desc"
    local operator = "best"
    local reset = "0 0 * * 1"
    local metadata = {
    weather_conditions = "rain"
    }
    nk.leaderboard_create(id, authoritative, sort, operator, reset, metadata)

    -- Leaderboard Delete
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    nk.leaderboard_delete(id)

    -- Leaderboard Records Write
    local metadata = {
    weather_conditions = "rain"
    }
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    local username = "02ebb2c8"
    local score = 10
    local subscore = 0
    nk.leaderboard_record_write(id, owner, username, score, subscore, metadata)

    -- Leaderboard Records Delete
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    nk.leaderboard_record_delete(id, owner)

    -- Leaderboard Records list
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owners = {}
    local limit = 10
    local records, owner_records, next_cursor, prev_cursor = nk.leaderboard_records_list(id, owners, limit)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Leaderboard Create**: Setup a new dynamic leaderboard with the specified ID and various configuration settings. The leaderboard will be created if it doesn't already exist, otherwise its configuration will *not* be updated. | id | `string` | The unique identifier for the new leaderboard. This is used by clients to submit scores. |
    | | authoritative | `bool` | Mark the leaderboard as authoritative which ensures updates can only be made via the Lua runtime. No client can submit a score directly. Default false. |
    | | sort | Opt. `string` | The sort order for records in the leaderboard; possible values are "asc" or "desc". Default "desc". |
    | | operator | Opt. `string` | The operator that determines how scores behave when submitted; possible values are "best", "set", or "incr". Default "best". |
    | | reset | Opt. `string` | The cron format used to define the reset schedule for the leaderboard. This controls when a leaderboard is reset and can be used to power daily/weekly/monthly leaderboards. |
    | | metadata | Opt. `Object` | The metadata you want associated to the leaderboard. Some good examples are weather conditions for a racing game. |
    | **Leaderboard Delete**: Delete a leaderboard and all scores that belong to it. | id | `string` | The unique identifier of the leaderboard to delete. |
    | **Leaderboard Record write**: Use the preconfigured operator for the given leaderboard to submit a score for a particular user. | id | `string` | The unique identifier for the leaderboard to submit to. |
    | | owner | `string` | The owner of this score submission. Mandatory field. |
    | | username | Opt. `string` | The owner username of this score submission, if it's a user. |
    | | score | Opt. `number` | The score to submit. Default 0. |
    | | subscore | Opt. `number` | A secondary subscore parameter for the submission. Default 0. |
    | | metadata | Opt. `Object` | The metadata you want associated to this submission. Some good examples are weather conditions for a racing game. |
    | **Leaderboard Record delete**: Remove an owner's record from a leaderboard, if one exists. | id | `string` | The unique identifier for the leaderboard to delete from. Mandatory field. |
    | | owner | `string` | The owner of the score to delete. Mandatory field. |
    | **Leaderboard Record list**: List records on the specified leaderboard, optionally filtering to only a subset of records by their owners. Records will be listed in the preconfigured leaderboard sort order. | id | `string` | The unique identifier for the leaderboard to list. Mandatory field. | A page of leaderboard records, a list of owner leaderboard records (empty if the `owners` input parameter is not set), an optional next page cursor that can be used to retrieve the next page of records (if any), and an optional previous page cursor that can be used to retrieve the previous page of records (if any). |
    | | id | `string` | The unique identifier for the leaderboard to list. Mandatory field. |
    | | owners | Opt. `string[]` | Array of owners to filter to. |
    | | limit | Opt. `number` | The maximum number of records to return from 10 to 100. |
    | | cursor | Opt. `string` | A cursor used to fetch the next page when applicable. |

    Examples:
    ```typescript
    // Leaderboard Create
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let authoritative = false;
    let sort = nkruntime.SortOrder.DESCENDING;
    let operator = nkruntime.Operator.BEST;
    let reset = '0 0 * * 1';
    let metadata = {
    weatherConditions: 'rain',
    };
    try {
        nk.leaderboardCreate(id, authoritative, sort, operator, reset, metadata);
    } catch(error) {
        // Handle error
    }

    // Leaderboard Delete
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    try {
        nk.leaderboardDelete(id);
    } catch(error) {
        // Handle error
    }

    // Leaderboard Record Write
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let ownerID = '4c2ae592-b2a7-445e-98ec-697694478b1c';
    let username = '02ebb2c8';
    let score = 10;
    let subscore = 0;
    let metadata = {
    weatherConditions: 'rain',
    };

    try {
        nk.leaderboardRecordWrite(id, ownerID, username, score, subscore, metadata);
    } catch(error) {
        // Handle error
    }

    // Leaderboard Record Delete
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let owner = '4c2ae592-b2a7-445e-98ec-697694478b1c';
    try {
        nk.leaderboardRecordWrite(id, owner);
    } catch(error) {
        // Handle error
    }

    // Leaderboard Record list
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let ownerIds: string[] = [];

    try {
        nk.leaderboardRecordsList(id, ownerIds);
    } catch(error) {
        // Handle error
    }
    ```

## Matches

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Match Create**: Create a new authoritative realtime multiplayer match running on the given runtime module name. The given `params` are passed to the match's init hook. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `string`: The match ID of the newly created match. Clients can immediately use this ID to join the match. |
    | | module | `string` | The name of an available runtime module that will be responsible for the match. This was registered in `InitModule`. |
    | | params | `map[string]interface{}` | Any value to pass to the match's init hook. |
    | **Match Get**: Get a running match information. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `*api.Match`: Running match info. |
    | | id | `string` | Match ID. |
    | **Match List**: List currently running realtime multiplayer matches and optionally filter them by authoritative mode, label, and current participant count. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `*api.Match`: A list of matches matching the parameters criteria. |
    | | limit | `int` | The maximum number of matches to list. Default 1. |
    | | authoritative | `bool` | Boolean `true` if listing should only return authoritative matches, `false` to only return relayed matches. Default `false`. |
    | | label | `string` | A label to filter authoritative matches by. Default `""` meaning any label matches. |
    | | min_size | `int` | Inclusive lower limit of current match participants. |
    | | max_size | `int` | Inclusive upper limit of current match participants. |
    | | query | `string` | Additional query parameters to shortlist matches. |

    Examples:
    ```go
    // Match Create
    // Assumes you've registered a match with initializer.RegisterMatch("some.folder.module", ...)
    modulename := "some.folder.module"
    params := map[string]interface{}{
    "some": "data",
    }

    matchID, err := nk.MatchCreate(ctx, modulename, params)
    if err != nil {
    return "", err
    }

    // Match Get
    matchId := "52f02f3e-5b48-11eb-b182-0f5058adfcc6"

    match, err := nk.MatchGet(ctx, matchId)
    if err != nil {
    return "", err
    }

    // Match List
    // List at most 10 matches, not authoritative, and that
    // have between 2 and 4 players currently participating.
    limit := 10
    isAuthoritative := false
    label := ""
    minSize := 2
    maxSize := 4

    matches, err := nk.MatchList(ctx, limit, isAuthoritative, label, minSize, maxSize, "")
    if err != nil {
    logger.WithField("err", err).Error("Match list error.")
    } else {
    for _, match := range matches {
        logger.Info("Found match with id: %s", match.GetMatchId())
    }
    }
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Match Create**: Create a new authoritative realtime multiplayer match running on the given runtime module name. The given `params` are passed to the match's init hook. | module | `string` | The name of an available runtime module that will be responsible for the match. This was provided as an independent match handler module. | `string`: The match ID of the newly created match. Clients can immediately use this ID to join the match. |
    | | params | Opt. any | Any value to pass to the match's init hook. |
    | **Match Get**: Get a running match information. | id | `string` | Match ID. | `table`: Running match info. |
    | **Match List**: List currently running realtime multiplayer matches and optionally filter them by authoritative mode, label, and current participant count. | limit | Opt. `number` | The maximum number of matches to list. Default 1. | `table`: A list of matches matching the parameters criteria. |
    | | authoritative | Opt. `bool` | Boolean `true` if listing should only return authoritative matches, `false` to only return relayed matches, `nil/null` to return both. Default `nil/null`. |
    | | label | Opt. `string` | A label to filter authoritative matches by. Default `nil/null` meaning any label matches. |
    | | min_size | Opt. `number` | Inclusive lower limit of current match participants. |
    | | max_size | Opt. `number` | Inclusive upper limit of current match participants. |
    | | query | Opt. `number` | Additional query parameters to shortlist matches |

    Examples:
    ```lua
    -- Match Create
    -- Assumes you've registered a runtime module with a path of "some/folder/module.lua".
    local module = "some.folder.module"
    local params = {
    some = "data"
    }
    local match_id = nk.match_create(module, params)

    -- Match Get
    local match_id = "52f02f3e-5b48-11eb-b182-0f5058adfcc6"

    local match_data = nk.match_get(match_id)

    -- Match List
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

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Match Create**: Create a new authoritative realtime multiplayer match running on the given runtime module name. The given `params` are passed to the match's init hook. | module | `string` | The name of an available runtime module that will be responsible for the match. This was provided as an independent match handler module. | `string`: The match ID of the newly created match. Clients can immediately use this ID to join the match. |
    | | params | Opt. `{[key: string]: any})` | Any value to pass to the match's init hook. |
    | **Match Get**: Get a running match information. | id | `string` | Match ID. | `nkruntime.Match`: Running match info. |
    | **Match List**: List currently running realtime multiplayer matches and optionally filter them by authoritative mode, label, and current participant count. | limit | Opt. `number` | The maximum number of matches to list. Default 1. | `nkruntime.Match[]`: A list of matches matching the parameters criteria. |
    | | authoritative | Opt. `bool` | Boolean `true` if listing should only return authoritative matches, `false` to only return relayed matches, `nil/null` to return both. Default `nil/null`. |
    | | label | Opt. `string` | A label to filter authoritative matches by. Default `nil/null` meaning any label matches. |
    | | min_size | Opt. `number` | Inclusive lower limit of current match participants. |
    | | max_size | Opt. `number` | Inclusive upper limit of current match participants. |
    | | query | Opt. `number` | Additional query parameters to shortlist matches |

    Examples:
    ```typescript
    // Match Create
    let module = 'some.folder.module';
    let params = {
        some: 'data',
    }

    try {
        nk.matchCreate(module, params);
    } catch(error) {
        // Handle error
    }

    // Match Get
    let matchId = '52f02f3e-5b48-11eb-b182-0f5058adfcc6';

    let match: nkruntime.Match;
    try {
        match = nk.matchGet(matchId);
    } catch(error) {
        // Handle error
    }

    // Match List
    // List at most 10 matches, not authoritative, and that
    // have between 2 and 4 players currently participating.
    let limit = 10;
    let isAuthoritative = false;
    let label = '';
    let minSize = 2;
    let maxSize = 4;

    let matches: nkruntime.Match[] = [];
    try {
        matches = nk.matchList(limit, isAuthoritative, label, minSize, maxSize);
    } catch(error) {
        // Handle error
    }
    ```
## Tournaments

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Tournament Create**: Setup a new dynamic tournament with the specified ID and various configuration settings. The underlying leaderboard will be created if it doesn't already exist, otherwise its configuration will *not* be updated. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | id | `string`| The unique identifier for the new tournament. This is used by clients to submit scores. |
    | | sort | `string` | The sort order for records in the tournament. Possible values are "asc" or "desc" (Default). |
    | | operator | `string` | The operator that determines how scores behave when submitted. The possible values are "best" (Default), "set", or "incr". |
    | | duration | `int` | The active duration for a tournament. This is the duration when clients are able to submit new records. The duration starts from either the reset period or tournament start time whichever is sooner. A game client can query the tournament for results between end of duration and next reset period. |
    | | reset | `string` | The cron format used to define the reset schedule for the tournament. This controls when the underlying leaderboard resets and the tournament is considered active again. Optional. |
    | | metadata | `map[string]interface{}` | The metadata you want associated to the tournament. Some good examples are weather conditions for a racing game. Optional. |
    | | title | `string` | The title of the tournament. Optional. |
    | | description | `string` | The description of the tournament. Optional. |
    | | category | `int` | A category associated with the tournament. This can be used to filter different types of tournaments. Between 0 and 127. Optional. |
    | | start_time | `int` | The start time of the tournament. Leave empty for immediately or a future time. |
    | | end_time | `int` | The end time of the tournament. When the end time is elapsed, the tournament will not reset and will cease to exist. Must be greater than `start_time` if set. Default value is __never__. |
    | | max_size | `int` | Maximum size of participants in a tournament. Optional. |
    | | max_num_score | `int` | Maximum submission attempts for a tournament record. |
    | | join_required | `bool` | Whether the tournament needs to be joint before a record write is allowed. Defaults to `false`. |
    | **Tournament Delete**: Delete a tournament and all records that belong to it. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | id | `string` | The unique identifier for the tournament to delete. |
    | **Tournament Add Attempt**: Add additional score attempts to the owner's tournament record. This overrides the max number of score attempts allowed in the tournament for this specific owner. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | id | `string` | The unique identifier for the tournament to update. |
    | | owner | `string` | The owner of the record to increment the count for. |
    | | count | `int` | The number of attempt counts to increment. Can be negative to decrease count. |
    | **Tournament Join**: A tournament may need to be joined before the owner can submit scores. This operation is idempotent and will always succeed for the owner even if they have already joined the tournament. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | id | `string` | The unique identifier for the tournament to update. |
    | | user_id | `string` | The owner of the record. |
    | | username | `string` | The username of the record owner. |
    | **Tournaments Get By ID**: Fetch one or more tournaments by ID. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | ids | `[]string` | The table array of tournament ids. |
    | **Tournament List**: Find tournaments which have been created on the server. Tournaments can be filtered with categories and via start and end times. This function can also be used to see the tournaments that an owner has joined. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `[]*api.TournamentList`: A list of tournament results and possibly a cursor. |
    | | category_start | `int` | Filter tournament with categories greater or equal than this value. |
    | | category_end | `int` | Filter tournament with categories equal or less than this value. |
    | | start_time | `int` | Filter tournament with that start after this time. |
    | | end_time | `int` | Filter tournament with that end before this time. |
    | | limit | `int` | Return only the required number of tournament denoted by this limit value. Defaults to 10. |
    | | cursor | `string` | Cursor to paginate to the next result set. If this is empty/null there is no further results. |
    | **Tournament Record Write**: Submit a score and optional subscore to a tournament leaderboard. If the tournament has been configured with join required this will fail unless the owner has already joined the tournament. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester.
    | | id | `string` | The unique identifier of the leaderboard to submit. |
    | | owner | `string` | The owner of this score submission. |
    | | username | `string` |  The owner username of this score submission, if it's a user. This is optional. |
    | | score | `int64` | The score to submit. This is optional. Default 0. |
    | | subscore | `int64` | A secondary subscore parameter for the submission. This is optional. Default 0. |
    | | metadata | `map[string]interface{}` | The metadata you want associated to this submission. Some good examples are weather conditions for a racing game. This is optional. |
    | **Tournament Records Haystack**: Fetch the list of tournament records around the owner. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `[]*api.Tournament`: A list of tournament record objects. |
    | | id | `string` | The unique identifier of the leaderboard to submit.
    | | owner | `string` | The owner of this score submission. |
    | | limit | `int` | Opt. number | Opt. `number` | Number of records to return. Default value is 1. |
    | | expiry | `int` | Opt. number | Opt. `number` | Tournament expiry unix epoch. |

    Examples:
    ```go
    // Tournament Create
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

    // Tournament Delete
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    err := nk.TournamentDelete(ctx, id)
    if err != nil {
    logger.WithField("err", err).Error("Tournament delete error.")
    }

    // Tournament Add Attempt
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    ownerID := "leaderboard-record-owner"
    count := -10
    err := nk.TournamentAddAttempt(ctx, id, ownerID, count)
    if err != nil {
    logger.WithField("err", err).Error("Tournament add attempt error.")
    }

    // Tournament Join
    id := "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    ownerID := "leaderboard-record-owner"
    userName := "myusername"
    err := nk.TournamentJoin(ctx, id, ownerID, userName)
    if err != nil {
    logger.WithField("err", err).Error("Tournament join error.")
    }

    // Tournament Get By ID
    tournamentIDs := []string{
    "3ea5608a-43c3-11e7-90f9-7b9397165f34",
    "447524be-43c3-11e7-af09-3f7172f05936",
    }
    tournaments, err := nk.TournamentsGetId(ctx, tournamentIDs)
    if err != nil {
    logger.WithField("err", err).Error("Tournaments get error.")
    }

    // Tournament List
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

    // Tournament Record Write
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

    // Tournament Records Haystack
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

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Tournament Create**: Setup a new dynamic tournament with the specified ID and various configuration settings. The underlying leaderboard will be created if it doesn't already exist, otherwise its configuration will *not* be updated. | id | `string`| The unique identifier for the new tournament. This is used by clients to submit scores. |
    | | sort | Opt. `string` | The sort order for records in the tournament. Possible values are "asc" or "desc" (Default). |
    | | operator | Opt. `string` | The operator that determines how scores behave when submitted. The possible values are "best" (Default), "set", or "incr". |
    | | duration | Opt. `number` | The active duration for a tournament. This is the duration when clients are able to submit new records. The duration starts from either the reset period or tournament start time whichever is sooner. A game client can query the tournament for results between end of duration and next reset period. |
    | | reset | Opt. `string` | The cron format used to define the reset schedule for the tournament. This controls when the underlying leaderboard resets and the tournament is considered active again. Optional. |
    | | metadata | Opt. `table` | The metadata you want associated to the tournament. Some good examples are weather conditions for a racing game. Optional. |
    | | title | Opt. `string` | The title of the tournament. Optional. |
    | | description | Opt. `string` | The description of the tournament. Optional. |
    | | category | Opt. `number` | A category associated with the tournament. This can be used to filter different types of tournaments. Between 0 and 127. Optional. |
    | | start_time | Opt. `number` | The start time of the tournament. Leave empty for immediately or a future time. |
    | | end_time | Opt. `number` | The end time of the tournament. When the end time is elapsed, the tournament will not reset and will cease to exist. Must be greater than `start_time` if set. Default value is __never__. |
    | | max_size | Opt. `number` | Maximum size of participants in a tournament. Optional. |
    | | max_num_score | Opt. `number` | Maximum submission attempts for a tournament record. |
    | | join_required | Opt. `boolean` | Whether the tournament needs to be joint before a record write is allowed. Defaults to `false`. |
    | **Tournament Delete**: Delete a tournament and all records that belong to it. | id | `string` | The unique identifier for the tournament to delete. |
    | **Tournament Add Attempt**: Add additional score attempts to the owner's tournament record. This overrides the max number of score attempts allowed in the tournament for this specific owner. | id | `string` | The unique identifier for the tournament to update. |
    | | owner | `string` | The owner of the record to increment the count for. |
    | | count | `number` | The number of attempt counts to increment. Can be negative to decrease count. |
    | **Tournament Join**: A tournament may need to be joined before the owner can submit scores. This operation is idempotent and will always succeed for the owner even if they have already joined the tournament. | id | `string` | The unique identifier for the tournament to update. |
    | | user_id | `string` | The owner of the record. |
    | | username | `string` | The username of the record owner. |
    | **Tournaments Get By ID**: Fetch one or more tournaments by ID. | ids | `table` | The table array of tournament ids. |
    | **Tournament List**: Find tournaments which have been created on the server. Tournaments can be filtered with categories and via start and end times. This function can also be used to see the tournaments that an owner has joined. | category_start | `number` | Filter tournament with categories greater or equal than this value. | `table`: A list of tournament results and possibly a cursor. |
    | | category_end | `number` | Filter tournament with categories equal or less than this value. |
    | | start_time | Opt. `number` | Filter tournament with that start after this time. |
    | | end_time | Opt. `number` | Filter tournament with that end before this time. |
    | | limit | Opt. `number` | Return only the required number of tournament denoted by this limit value. Defaults to 10. |
    | | cursor | Opt. `string` | Cursor to paginate to the next result set. If this is empty/null there is no further results. |
    | **Tournament Record Write**: Submit a score and optional subscore to a tournament leaderboard. If the tournament has been configured with join required this will fail unless the owner has already joined the tournament. | id | `string` | The unique identifier of the leaderboard to submit. |
    | | owner | `string` | The owner of this score submission. |
    | | username | Opt. `string` |  The owner username of this score submission, if it's a user. This is optional. |
    | | score | Opt. `number` | The score to submit. This is optional. Default 0. |
    | | subscore | Opt. `number` | A secondary subscore parameter for the submission. This is optional. Default 0. |
    | | metadata | Opt. `table` | The metadata you want associated to this submission. Some good examples are weather conditions for a racing game. This is optional. |
    | **Tournament Records Haystack**: Fetch the list of tournament records around the owner. | id | `string` | The unique identifier of the leaderboard to submit. | `table`: A list of tournament record objects. |
    | | owner | `string` | The owner of this score submission. |
    | | limit | Opt. `number` | Number of records to return. Default value is 1. |
    | | expiry | Opt. `number` | Tournament expiry unix epoch. |

    Examples:
    ```lua
    -- Tournament Create
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

    -- Tournament Delete
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    nk.tournament_delete(id)

    -- Tournament Add Attempt
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "leaderboard-record-owner"
    local count = -10
    nk.tournament_add_attempt(id, owner, count)

    -- Tournament Join
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "leaderboard-record-owner"
    local username = "myusername"
    nk.tournament_join(id, owner, username)

    -- Tournament Get By ID
    local tournament_ids = {
    "3ea5608a-43c3-11e7-90f9-7b9397165f34",
    "447524be-43c3-11e7-af09-3f7172f05936"
    }
    local tournaments = nk.tournaments_get_id(tournament_ids)

    -- Tournament List
    local category_start = 1
    local category_end = 2
    local start_time = 1538147711
    local end_time = 0 -- All tournaments from the start time.
    local limit = 100  -- Number to list per page.
    local tournaments = nk.tournament_list(category_start, category_end, start_time, end_time, limit)
    for i, tournament in ipairs(tournaments) do
    nk.logger_info(string.format("ID: %q - can enter? %q", tournament.id, tournament.can_enter))
    end

    -- Tournament Record Write
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    local username = "02ebb2c8"
    local score = 10
    local subscore = 0
    local metadata = {
    weather_conditions = "rain"
    }
    nk.tournament_record_write(id, owner, username, score, subscore, metadata)

    -- Tournament Records Haystack
    local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
    local owner = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    nk.tournament_records_haystack(id, owner, 10)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Tournament Create**: Setup a new dynamic tournament with the specified ID and various configuration settings. The underlying leaderboard will be created if it doesn't already exist, otherwise its configuration will *not* be updated. | id | `string`| The unique identifier for the new tournament. This is used by clients to submit scores. |
    | | sort | Opt. `string` | The sort order for records in the tournament. Possible values are "asc" or "desc" (Default). |
    | | operator | Opt. `string` | The operator that determines how scores behave when submitted. The possible values are "best" (Default), "set", or "incr". |
    | | duration | Opt. `number` | The active duration for a tournament. This is the duration when clients are able to submit new records. The duration starts from either the reset period or tournament start time whichever is sooner. A game client can query the tournament for results between end of duration and next reset period. |
    | | reset | Opt. `string` | The cron format used to define the reset schedule for the tournament. This controls when the underlying leaderboard resets and the tournament is considered active again. Optional. |
    | | metadata | Opt. `object` | The metadata you want associated to the tournament. Some good examples are weather conditions for a racing game. Optional. |
    | | title | Opt. `string` | The title of the tournament. Optional. |
    | | description | Opt. `string` | The description of the tournament. Optional. |
    | | category | Opt. `number` | A category associated with the tournament. This can be used to filter different types of tournaments. Between 0 and 127. Optional. |
    | | start_time | Opt. `number` | The start time of the tournament. Leave empty for immediately or a future time. |
    | | end_time | Opt. `number` | The end time of the tournament. When the end time is elapsed, the tournament will not reset and will cease to exist. Must be greater than `start_time` if set. Default value is __never__. |
    | | max_size | Opt. `number` | Maximum size of participants in a tournament. Optional. |
    | | max_num_score | Opt. `number` | Maximum submission attempts for a tournament record. |
    | | join_required | Opt. `boolean` | Whether the tournament needs to be joint before a record write is allowed. Defaults to `false`. |
    | **Tournament Delete**: Delete a tournament and all records that belong to it. | id | `string` | The unique identifier for the tournament to delete. |
    | **Tournament Add Attempt**: Add additional score attempts to the owner's tournament record. This overrides the max number of score attempts allowed in the tournament for this specific owner. | id | `string` | The unique identifier for the tournament to update. |
    | | owner | `string` | The owner of the record to increment the count for. |
    | | count | `string` | The number of attempt counts to increment. Can be negative to decrease count. |
    | **Tournament Join**: A tournament may need to be joined before the owner can submit scores. This operation is idempotent and will always succeed for the owner even if they have already joined the tournament. | id | `string` | The unique identifier for the tournament to update. |
    | | user_id | `string` | The owner of the record. |
    | | username | `string` | The username of the record owner. |
    | **Tournaments Get By ID**: Fetch one or more tournaments by ID. | ids | `string[]` | The table array of tournament ids. |
    | **Tournament List**: Find tournaments which have been created on the server. Tournaments can be filtered with categories and via start and end times. This function can also be used to see the tournaments that an owner has joined. | category_start | `number` | Filter tournament with categories greater or equal than this value. | `nkruntime.TournamentList`: A list of tournament results and possibly a cursor. |
    | | category_end | `number` | Filter tournament with categories equal or less than this value. |
    | | start_time | Opt. `number` | Filter tournament with that start after this time. |
    | | end_time | Opt. `number` | Filter tournament with that end before this time. |
    | | limit | Opt. `number` | Return only the required number of tournament denoted by this limit value. Defaults to 10. |
    | | cursor | Opt. `number` | Cursor to paginate to the next result set. If this is empty/null there is no further results. |
    | **Tournament Record Write**: Submit a score and optional subscore to a tournament leaderboard. If the tournament has been configured with join required this will fail unless the owner has already joined the tournament. | id | `string` | The unique identifier of the leaderboard to submit. |
    | | owner | `string` | The owner of this score submission. |
    | | username | Opt. `string` |  The owner username of this score submission, if it's a user. This is optional. |
    | | score | Opt. `number` | The score to submit. This is optional. Default 0. |
    | | subscore | Opt. `number` | A secondary subscore parameter for the submission. This is optional. Default 0. |
    | | metadata | Opt. `Object` | The metadata you want associated to this submission. Some good examples are weather conditions for a racing game. This is optional. |
    | **Tournament Records Haystack**: Fetch the list of tournament records around the owner. | id | `string` | The unique identifier of the leaderboard to submit. | `nkruntime.Tournament[]`: A list of tournament record objects. |
    | | owner | `string` | The owner of this score submission. |
    | | limit | Opt. `number` | Number of records to return. Default value is 1. |
    | | expiry | Opt. `number` | Tournament expiry unix epoch. |

    Examples:
    ```ts
    // Tournament Create
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let sortOrder = nkruntime.SortOrder.DESCENDING;
    let operator = nkruntime.Operator.BEST;
    let duration = 3600;     // In seconds.
    let resetSchedule = '0 12 * * *'; // Noon UTC each day.
    let metadata = {
        weatherConditions: 'rain',
    };
    let title = 'Daily Dash';
    let description = "Dash past your opponents for high scores and big rewards!";
    let category = 1;
    let startTime = 0;       // Start now.
    let endTime = 0;         // Never end, repeat the tournament each day forever.

    let maxSize = 10000;     // First 10,000 players who join.
    let maxNumScore = 3;     // Each player can have 3 attempts to score.
    let joinRequired = true; // Must join to compete.

    try {
        nk.tournamentCreate(
            id,
            sortOrder,
            operator,
            duration,
            resetSchedule,
            metadata,
            title,
            description,
            category,
            startTime,
            endTime,
            maxSize,
            maxNumScore,
            joinRequired
        );
    } catch (error) {
        // Handle error
    }

    // Tournament Delete
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    try {
        nk.tournamentDelete(id);
    } catch (error) {
        // Handle error
    }

    // Tournament Add Attempt
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let owner = 'leaderboard-record-owner';
    let count = -10;
    try {
        nk.tournamentAddAttempt(id, owner, count);
    } catch (error) {
        // Handle error
    }

    // Tournament Join
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let owner = 'leaderboard-record-owner';
    let username = 'myusername';
    try {
        nk.tournamentJoin(id, owner, username);
    } catch (error) {
        // Handle error
    }

    // Tournament Get By ID
    let tournamentIds = [
        '3ea5608a-43c3-11e7-90f9-7b9397165f34',
        '447524be-43c3-11e7-af09-3f7172f05936',
    ]
    let owner = 'leaderboard-record-owner';
    let username = 'myusername';

    let tournaments
    try {
        nk.tournamentsGetId(id, owner, username);
    } catch (error) {
        // Handle error
    }

    // Tournament List
    let categoryStart = 1;
    let categoryEnd = 2;
    let startTime = 1538147711M
    let endTime = 0 // All tournaments from the start time.
    let limit = 100  // Number to list per page.

    let results: nkruntime.TournamentList = {};
    try {
        results = nk.tournamentList(categoryStart, categoryEnd, startTime, endTime, limit);
    } catch (error) {
        // Handle error
    }

    // Tournament Record Write
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let owner = '4c2ae592-b2a7-445e-98ec-697694478b1c';
    let username = '02ebb2c8';
    let score = 10;
    let subscore = 0;
    let metadata = {
    weather_conditions = 'rain',
    }

    try {
        nk.tournamentRecordWrite(categoryStart, categoryEnd, startTime, endTime, limit);
    } catch (error) {
        // Handle error
    }

    // Tournament Records Haystack
    let id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let owner = '4c2ae592-b2a7-445e-98ec-697694478b1c';

    let results: nkruntime.Tournament[] = [];
    try {
        results = nk.tournamentRecordsHaystack(id, owner);
    } catch (error) {
        // Handle error
    }
    ```

## Notifications

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Notification Send**: Send one [in-app notification](../concepts/in-app-notifications.md) to a user. | ctx | `context.Context` | - | - | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | subject | `string` | Notification subject. Must be set. |
    | | content | `map[string]interface{}` | Notification content. Must be set but can be an struct. |
    | | code | `int` | Notification code to use. Must be equal or greater than 0. |
    | | sender_id | `string` | The sender of this notification. If left empty, it will be assumed that it is a system notification. |
    | | persistent | `bool` | Whether to record this in the database for later listing. Defaults to false. |
    | **Notifications Send**: Send one or more [in-app notifications](../concepts/in-app-notifications.md) to a user. | ctx | `context.Context` | - | - | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | notifications | `[]*runtime.NotificationSend` | A list of notifications to be sent together. |

    Examples:
    ```go
    // Notification Send
    subject := "You've unlocked level 100!"
    content := map[string]interface{}{
      "reward_coins": 1000,
    }
    receiverID := "4c2ae592-b2a7-445e-98ec-697694478b1c"
    senderID := "dcb891ea-a311-4681-9213-6741351c9994" // who the message if from
    code := 101
    persistent := true

    nk.NotificationSend(ctx, receiverID, subject, content, code, senderID, persistent)

    // Notifications Send
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

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Notification Send**: Send one [in-app notification](../concepts/in-app-notifications.md) to a user. | subject | `string` | Notification subject. Must be set. |
    | | content | `table` | Notification content. Must be set but can be a struct. |
    | | code `number` | Notification code to use. Must be equal or greater than 0. |
    | | sender_id | Opt. `string` | The sender of this notification. If left empty, it will be assumed that it is a system notification. |
    | | persistent | Opt. `bool` | Whether to record this in the database for later listing. Defaults to false. |
    | **Notifications Send**: Send one or more [in-app notifications](../concepts/in-app-notifications.md) to a user. | notifications | `table` | A list of notifications to be sent together. |

    Examples:
    ```lua
    -- Notification Send
    local subject = "You've unlocked level 100!"
    local content = {
      reward_coins = 1000
    }
    local receiver_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    local sender_id = "dcb891ea-a311-4681-9213-6741351c9994"
    local code = 101
    local persistent = true

    nk.notification_send(receiver_id, subject, content, code, sender_id, persistent)

    -- Notifications Send
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

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Notification Send**: Send one [in-app notification](../concepts/in-app-notifications.md) to a user. | subject | `string` | Notification subject. Must be set. |
    | | content | `table` | Notification content. Must be set but can be a struct. |
    | | code `number` | Notification code to use. Must be equal or greater than 0. |
    | | sender_id | Opt. `string` | The sender of this notification. If left empty, it will be assumed that it is a system notification. |
    | | persistent | Opt. `bool` | Whether to record this in the database for later listing. Defaults to false. |
    | **Notifications Send**: Send one or more [in-app notifications](../concepts/in-app-notifications.md) to a user. | notifications | `table` | A list of notifications to be sent together. |

    Examples:
    ```typescript
    // Notification Send
    let receiverId = '4c2ae592-b2a7-445e-98ec-697694478b1c';
    let subject = "You've unlocked level 100!";
    let content = {
      rewardCoins: 1000,
    }
    let code = 101;
    let senderId = 'dcb891ea-a311-4681-9213-6741351c9994' // who the message if from
    let persistent = true;

    nk.notificationSend(receiverId, subject, content, code, senderId, persistent);

    // Notifications Send
    let notifications: nkruntime.NotificationRequest[] = [
        {
            userId: '4c2ae592-b2a7-445e-98ec-697694478b1c',
            subject: "You've unlocked level 100!",
            content: {rewardCoins: 1000},
            code: 101,
            persistent: true,
        },
        {
            userId: '69769447-b2a7-445e-98ec-8b1c4c2ae592',
            subject: "You've unlocked level 100!",
            content: {rewardCoins: 1000},
            code: 101,
            persistent: true,
        },
    ]

    nk.notificationsSend(notifications);
    ```

## Storage Engine

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Storage Read**: Fetch one or more records by their bucket/collection/keyname and optional user. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `[]*api.StorageObject`: A list of matches matching the parameters criteria. |
    | | object_ids | `[]*runtime.StorageRead` | An array of object identifiers to be fetched. |
    | **Storage List**: List records in a collection and page through results. The records returned can be filtered to those owned by the user or "" for public records. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `[]*api.StorageObject`, `string`: A list of storage objects. |
    | | user_id | `string` | User ID or `""` (empty string) for public records. |
    | | collection | `string` | Collection to list data from. |
    | | limit | `int` | Limit number of records retrieved. Defaults to 100. |
    | | cursor | `string` | Pagination cursor from previous result. If none available set to `nil` or `""` (empty string). |
    | **Storage Delete**: Remove one or more objects by their collection/keyname and optional user. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. |
    | | object_ids | `[]*runtime.StorageDelete` | An array of object identifiers to be deleted. |
    | **Storage Write**: Write one or more objects by their collection/keyname and optional user. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `[]*api.StorageObjectAcks`: A list of acks with the version of the written objects. |
    | | object_ids | `[]*runtime.StorageWrite` | An array of object identifiers to be written. |

    Examples:
    ```go
    // Storage Read
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

    // Storage List
    userID := "4ec4f126-3f9d-11e7-84ef-b7c182b36521" // Some user ID.
    listRecords, nextCursor, err := nk.StorageList(ctx, userID, "collection", 10, "")
    if err != nil {
    logger.WithField("err", err).Error("Storage list error.")
    } else {
    for _, r := range listRecords {
        logger.Info("read: %d, write: %d, value: %s", r.PermissionRead, r.PermissionWrite, r.Value)
    }
    }

    // Storage Delete
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

    // Storage Write
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

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Storage Read**: Fetch one or more records by their bucket/collection/keyname and optional user. | object_ids | `table` | A table of object identifiers to be fetched. | `table`: A list of matches matching the parameters criteria. |
    | **Storage List**: List records in a collection and page through results. The records returned can be filtered to those owned by the user or "" for public records. | user_id | `string` | User ID or `""` (empty string) for public records. | `table`, `string`: A list of storage objects. |
    | | collection | `string` | Collection to list data from. |
    | | limit | `number` | Limit number of records retrieved. Defaults to 100. |
    | | cursor | Opt. `string` | Pagination cursor from previous result. If none available set to `nil` or `""` (empty string). |
    | **Storage Delete**: Remove one or more objects by their collection/keyname and optional user. | object_ids | `table` | A table of object identifiers to be deleted. |
    | **Storage Write**: Write one or more objects by their collection/keyname and optional user. | object_ids | `table` | A table of object identifiers to be written. | `table`: A list of acks with the version of the written objects. |

    Examples:
    ```lua
    -- Storage Read
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

    -- Storage List
    local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- Some user ID.
    local records = nk.storage_list(user_id, "collection", 10, "")
    for _, r in ipairs(records)
    do
    local m = string.format("read: %q, write: %q, value: %q", r.permission_read, r.permission_write, r.value)
    nk.logger_info(m)
    end

    -- Storage Delete
    local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- Some user ID.
    local friend_user_id = "8d98ee3f-8c9f-42c5-b6c9-c8f79ad1b820" -- Friend ID.
    local object_ids = {
    { collection = "save", key = "save1", user_id = user_id },
    { collection = "save", key = "save2", user_id = user_id },
    { collection = "public", key = "progress", user_id = friend_user_id }
    }
    nk.storage_delete(object_ids)

    -- Storage Write
    local user_id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- Some user ID.
    local new_objects = {
    { collection = "save", key = "save1", user_id = user_id, value = {} },
    { collection = "save", key = "save2", user_id = user_id, value = {} },
    { collection = "save", key = "save3", user_id = user_id, value = {}, permission_read = 2, permission_write = 1 },
    { collection = "save", key = "save3", user_id = user_id, value = {}, version="*", permission_read = 1, permission_write = 1 }
    }
    nk.storage_write(new_objects)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Storage Read**: Fetch one or more records by their bucket/collection/keyname and optional user. | object_ids | `nkruntime.StorageReadRequest[]` | A table of object identifiers to be fetched. | `nkruntime.StorageObject[]`: A list of matches matching the parameters criteria. |
    | **Storage List**: List records in a collection and page through results. The records returned can be filtered to those owned by the user or "" for public records. | user_id | `string` | User ID or `""` (empty string) for public records. | `nkruntime.StorageObjectList`: A list of storage objects. |
    | | collection | `string` | Collection to list data from. |
    | | limit | `number` | Limit number of records retrieved. Defaults to 100. |
    | | cursor | Opt. `string` | Pagination cursor from previous result. If none available set to `nil` or `""` (empty string). |
    | **Storage Delete**: Remove one or more objects by their collection/keyname and optional user. | object_ids | `nkruntime.StorageDeleteRequest[]` | A table of object identifiers to be deleted. |
    | **Storage Write**: Write one or more objects by their collection/keyname and optional user. | object_ids | `nkruntime.StorageWriteRequest[]` | A table of object identifiers to be written. | `nkruntime.StorageWriteAck`: A list of acks with the version of the written objects. |

    Examples:
    ```ts
    // Storage Read
    let userId = '4ec4f126-3f9d-11e7-84ef-b7c182b36521';
    let objectIds: nkruntime.StorageReadRequest[] = [
    {collection: 'save', key: 'save1', userId: userId},
    {collection: 'save', key: 'save2', userId},
    {collection: 'save', key: 'save3', userId},
    ]
    let results: nkruntime.StorageObject[] = [];
    try {
        results = nk.storageRead(objectIds);
    } catch (error) {
        // Handle error
    }

    results.forEach(o => {
        logger.info('Storage object: %s', JSON.stringify(o));
    });

    // Storage List
    let user_id = '4ec4f126-3f9d-11e7-84ef-b7c182b36521' // Some user ID.

    let result: nkruntime.StorageObjectList = {};
    try {
        let result = nk.storageList(user_id, "collection", 10);
    } catch (error) {
        // Handle error
    }

    result.objects?.forEach(r => {
        logger.info('Storage object: %s', JSON.stringify(r));
    });

    // Storage Delete
    let userId = '4ec4f126-3f9d-11e7-84ef-b7c182b36521' // Some user ID.
    let friendUserId = '8d98ee3f-8c9f-42c5-b6c9-c8f79ad1b820' // Friend ID.
    let objectIds: nkruntime.StorageDeleteRequest[] = [
    { collection: 'save', key: 'save1', userId },
    { collection: 'save', key: 'save2', userId },
    { collection: 'public', key: 'progress', userId: friendUserId },
    ]

    try {
        nk.storageDelete(objectIds);
    } catch (error) {
        // Handle error
    }

    // Storage Write
    let userId = '4ec4f126-3f9d-11e7-84ef-b7c182b36521' // Some user ID.
    let newObjects: nkruntime.StorageWriteRequest[] = [
    { collection: "save", key: "save1", userId, value: {} },
    { collection: "save", key: "save2", userId, value: {} },
    { collection: "save", key: "save3", userId, value: {}, permissionRead: 2, permissionWrite: 1 },
    { collection: "save", key: "save3", userId, value: {}, version: <some_version>, permissionRead: 1, permissionWrite: 1 }
    ];

    try {
        nk.storageWrite(newObjects);
    } catch (error) {
        // Handle error
    }
    ```

## Economy

### Purchases

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Purchase Get By Transaction Id**: Look up a purchase receipt by transaction ID. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `string, *api.ValidatedPurchase`: A validated purchase and its owner.|
    | | transactionID | `string` | Transaction ID of the purchase to look up. |
    | **Purchases List**: List stored validated purchase receipts. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `*api.PurchaseList`: A page of stored validated purchases.|
    | | userID | `string` | Filter by userID. Can be an empty string to list purchases for all users. |
    | | limit | `int` | Limit number of records retrieved. Defaults to 100. |
    | | cursor | `string` | Pagination cursor from previous result. If none available set to nil or `""` (empty string). |
    | **Purchase Validate Apple**: Validates and stores the purchases present in an Apple App Store Receipt. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `*api.ValidatePurchaseResponse`: The resulting successfully validated purchases.
    | | userID | `string` | The userID of the owner of the receipt. |
    | | receipt | `string` | Base-64 encoded receipt data returned by the purchase operation itself. |
    | | passwordOverride | `string` | Optional. Override the `iap.apple.shared_password` provided in your [configuration](../getting-started/configuration.md#iap-in-app-purchase). |
    | **Purchase Validate Google**: Validates and stores a purchase receipt from the Google Play Store. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `*api.ValidatePurchaseResponse`: The resulting successfully validated purchases.
    | | userID | `string` | The userID of the owner of the receipt. |
    | | receipt | `string` | The JSON encoded Google receipt. |
    | **Purchase Validate Huawei**: Validates and stores a purchase receipt from the Huawei App Gallery. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `*api.ValidatePurchaseResponse`: The resulting successfully validated purchases.
    | | userID | `string` | The userID of the owner of the receipt. |
    | | receipt | `string` | The Huawei receipt data. |
    | | signature | `string` | The receipt signature. |

    Examples:
    ```go
    // Purchase Get By Transaction Id
    transactionId := "4c2ae592-b2a7-445e-98ec-697694478b1c"
    userId, purchase, err := nk.PurchaseGetByTransactionId(ctx, transactionId)
    if err != nil {
    // Handle error
    }

    // Purchases List
    userId := "4c2ae592-b2a7-445e-98ec-697694478b1c"
    purchases, err := nk.PurchasesList(ctx, userId, 100, "")
    if err != nil {
    // Handle error
    }
    for _, p := range purchases.ValidatedPurchases {
        logger.Info("Purchase: %+v", v)
    }

    // Purchase Validate Apple
    userId := "4c2ae592-b2a7-445e-98ec-697694478b1c"
    receipt := "<base64-receipt-data>"
    validation, err := nk.PurchaseValidateApple(ctx, userId, receipt)
    if err != nil {
    // Handle error
    }
    for _, p := range validation.ValidatedPurchases {
        logger.Info("Validated purchase: %+v", v)
    }

    // Purchase Validate Google
    userId := "4c2ae592-b2a7-445e-98ec-697694478b1c"
    receipt := "{\"json\":{\"orderId \":\"..\",\"packageName \":\"..\",\"productId \":\"..\",\"purchaseTime\":1607721533824,\"purchaseState\":0,\"purchaseToken\":\"..\",\"acknowledged\":false},\"signature \":\"..\",\"skuDetails \":{\"productId\":\"..\",\"type\":\"inapp\",\"price\":\"u20ac82.67\",\"price_amount_micros\":82672732,\"price_currency_code\":\"EUR\",\"title\":\"..\",\"description\":\"..\",\"skuDetailsToken\":\"..\"}}"
    validation, err := nk.PurchaseValidateGoogle(ctx, userId, receipt)
    if err != nil {
    // Handle error
    }
    for _, p := range validation.ValidatedPurchases {
        logger.Info("Validated purchase: %+v", v)
    }
    // Purchase Validate Huawei
    userId := "4c2ae592-b2a7-445e-98ec-697694478b1c"
    signature := "<signature-data>"
    receipt := "<receipt-data>"

    validation, err := nk.PurchaseValidateHuawei(ctx, userId, signature, receipt)
    if err != nil {
    // Handle error
    }
    for _, p := range validation.ValidatedPurchases {
        logger.Info("Validated purchase: %+v", v)
    }
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Purchase Get By Transaction Id**: Look up a purchase receipt by transaction ID. | transactionID | `string` | Transaction ID of the purchase to look up. | `table`: A validated purchase and its owner.|
    | **Purchases List**: List stored validated purchase receipts. | userID | Opt. `string` | Filter by userID. Can be an empty string to list purchases for all users. | `table`: A page of stored validated purchases.|
    | | limit | `number` | Limit number of records retrieved. Defaults to 100. |
    | | cursor | Opt. `string` | Pagination cursor from previous result. If none available set to nil or `""` (empty string). |
    | **Purchase Validate Apple**: Validates and stores the purchases present in an Apple App Store Receipt. | userID | `string` | The userID of the owner of the receipt. | `table`: The resulting successfully validated purchases.
    | | receipt | `string` | Base-64 encoded receipt data returned by the purchase operation itself. |
    | | passwordOverride | `string` | Optional. Override the `iap.apple.shared_password` provided in your [configuration](../getting-started/configuration.md#iap-in-app-purchase). |
    | **Purchase Validate Google**: Validates and stores a purchase receipt from the Google Play Store. | userID | `string` | The userID of the owner of the receipt. | `table`: The resulting successfully validated purchases.
    | | receipt | `string` | The JSON encoded Google receipt. |
    | **Purchase Validate Huawei**: Validates and stores a purchase receipt from the Huawei App Gallery. | userID | `string` | The userID of the owner of the receipt. | `table`: The resulting successfully validated purchases.
    | | receipt | `string` | The Huawei receipt data. |
    | | signature | `string` | The receipt signature. |\

    Examples:
    ```lua
    -- Purchase Get By Transaction Id
    local transaction_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    local purchases = nk.purchase_get_by_transaction_id(transaction_id)

    -- Purchases List
    local user_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    local purchases = nk.purchases_list(user_id)

    -- Purchase Validate Apple
    local user_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    local receipt = "<base64-receipt-data>"

    local validation = nk.purchase_validate_apple(user_id, receipt)

    -- Purchase Validate Google
    local user_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    local receipt = "{\"json\":{\"orderId \":\"..\",\"packageName \":\"..\",\"productId \":\"..\",\"purchaseTime\":1607721533824,\"purchaseState\":0,\"purchaseToken\":\"..\",\"acknowledged\":false},\"signature \":\"..\",\"skuDetails \":{\"productId\":\"..\",\"type\":\"inapp\",\"price\":\"u20ac82.67\",\"price_amount_micros\":82672732,\"price_currency_code\":\"EUR\",\"title\":\"..\",\"description\":\"..\",\"skuDetailsToken\":\"..\"}}"

    local validation = nk.purchase_validate_google(user_id, receipt)

    -- Purchase Validate Huawei
    local user_id = "4c2ae592-b2a7-445e-98ec-697694478b1c"
    local receipt = "<receipt-data>"
    local signature = "<signature-data>"

    local validation = nk.purchase_validate_huawei(user_id, receipt, signature)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Purchase Get By Transaction Id**: Look up a purchase receipt by transaction ID. | transactionID | `string` | Transaction ID of the purchase to look up. | `nkruntime.ValidatedPurchaseAroundOwner`: A validated purchase and its owner.|
    | **Purchases List**: List stored validated purchase receipts. | userID | Opt. `string` | Filter by userID. Can be an empty string to list purchases for all users. | `nkruntime.ValidatedPurchaseList`: A page of stored validated purchases.|
    | | limit | Opt. `number` | Limit number of records retrieved. Defaults to 100. |
    | | cursor | Opt. `string` | Pagination cursor from previous result. If none available set to nil or `""` (empty string). |
    | **Purchase Validate Apple**: Validates and stores the purchases present in an Apple App Store Receipt. | userID | `string` | The userID of the owner of the receipt. | `nkruntime.ValidatePurchaseResponse`: The resulting successfully validated purchases.
    | | receipt | `string` | Base-64 encoded receipt data returned by the purchase operation itself. |
    | | passwordOverride | `string` | Optional. Override the `iap.apple.shared_password` provided in your [configuration](../getting-started/configuration.md#iap-in-app-purchase). |
    | **Purchase Validate Google**: Validates and stores a purchase receipt from the Google Play Store. | userID | `string` | The userID of the owner of the receipt. | `nkruntime.ValidatePurchaseResponse`: The resulting successfully validated purchases.
    | | receipt | `string` | The JSON encoded Google receipt. |
    | **Purchase Validate Huawei**: Validates and stores a purchase receipt from the Huawei App Gallery. | userID | `string` | The userID of the owner of the receipt. | `nkruntime.ValidatePurchaseResponse`: The resulting successfully validated purchases.
    | | receipt | `string` | The Huawei receipt data. |
    | | signature | `string` | The receipt signature. |

    Examples:
    ```ts
    // Purchase Get By Transaction Id
    let userId = '4c2ae592-b2a7-445e-98ec-697694478b1c';

    let validation: nkruntime.ValidatedPurchaseAroundOwner;
    try {
        validation = nk.purchasesList(userId);
    } catch(error) {
        // Handle error
    }

    // Purchases List
    let userId = '4c2ae592-b2a7-445e-98ec-697694478b1c';

    let validation: nkruntime.ValidatedPurchaseList;
    try {
        validation = nk.purchasesList(userId);
    } catch(error) {
        // Handle error
    }

    // Purchase Validate Apple
    let userId = '4c2ae592-b2a7-445e-98ec-697694478b1c';
    let receipt = '<base64-receipt-data>';

    let validation: nkruntime.ValidatePurchaseResponse;
    try {
        validation = nk.purchaseValidateApple(userId, receipt);
    } catch(error) {
        // Handle error
    }

    // Purchase Validate Google
    let userId = '4c2ae592-b2a7-445e-98ec-697694478b1c';
    let receipt = '{\"json\":{\"orderId \":\"..\",\"packageName \":\"..\",\"productId \":\"..\",\"purchaseTime\":1607721533824,\"purchaseState\":0,\"purchaseToken\":\"..\",\"acknowledged\":false},\"signature \":\"..\",\"skuDetails \":{\"productId\":\"..\",\"type\":\"inapp\",\"price\":\"u20ac82.67\",\"price_amount_micros\":82672732,\"price_currency_code\":\"EUR\",\"title\":\"..\",\"description\":\"..\",\"skuDetailsToken\":\"..\"}}';

    let validation: nkruntime.ValidatePurchaseResponse;
    try {
        validation = nk.purchaseValidateGoogle(userId, receipt);
    } catch(error) {
        // Handle error
    }

    // Purchase Validate Huawei
    let userId = '4c2ae592-b2a7-445e-98ec-697694478b1c';
    let receipt = '<receipt-data>';
    let signature = '<signature-data>';

    let validation: nkruntime.ValidatePurchaseResponse;
    try {
        validation = nk.purchaseValidateHuawei(userId, receipt, signature);
    } catch(error) {
        // Handle error
    }
    ```

### Wallets

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Wallet Update**: Update a user's wallet with the given changeset. | ctx | `context.Context` | - | - | The [context](basics.md#register-hooks) object represents information about the server and requester. | `map[string]int64`, `map[string]int64`: The changeset after the update and previously to the update, respectively. |
    | | user_id | `string` |  The ID of the user to update the wallet for. |
    | | changeset | `map[string]interface{}` | The set of wallet operations to apply. |
    | | metadata | `map[string]interface{}` | Additional metadata to tag the wallet update with. |
    | | update_ledger | `bool` | Whether to record this update in the ledger. Defaults to `true`. |
    | **Wallets Update**: Update one or more user wallets with individual changesets. This function will also insert a new wallet ledger item into each user's wallet history that tracks their update. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | `runtime.WalletUpdateResult`: A list of wallet updates results. |
    | | updates | `[]*runtime.WalletUpdate` | The set of user wallet update operations to apply. |
    | | update_ledger | `bool` | Whether to record this update in the ledger. Default `true`. |
    | **Wallet Ledger List**: List all wallet updates for a particular user from oldest to newest. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | A Go slice containing wallet entries with `Id`, `UserId`, `CreateTime`, `UpdateTime`, `Changeset`, `Metadata` parameters. |
    | | limit | `int` | Limit number of results. Defaults to 100. |
    | **Wallet Ledger Update**: Update the metadata for a particular wallet update in a users wallet ledger history. Useful when adding a note to a transaction for example. | ctx | `context.Context` | The [context](basics.md#register-hooks) object represents information about the server and requester. | The updated wallet ledger item. |
    | | id | `string` | The ID of the wallet ledger item to update. |
    | | metadata | `table` | The new metadata to set on the wallet ledger item. |

    Examples:
    ```go
    // Wallet Update
    userID := "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
    changeset := map[string]interface{}{
    "coins": 10, // Add 10 coins to the user's wallet.
    "gems":  -5, // Remove 5 gems from the user's wallet.
    }
    metadata := map[string]interface{}{
    "game_result": "won",
    }
    updated, previous, err := nk.WalletUpdate(ctx, userID, changeset, metadata, true)
    if err != nil {
    logger.WithField("err", err).Error("Wallet update error.")
    }

    // Wallets Update
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

    // Wallet Ledger List
    userID := "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
    items, err := nk.WalletLedgerList(ctx, userID)
    if err != nil {
    logger.WithField("err", err).Error("Wallet ledger list error.")
    } else {
    for _, item := range items {
        logger.Info("Found wallet update with id: %v", item.GetID())
    }
    }

    // Wallet Ledger Update
    itemID := "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
    metadata := map[string]interface{}{
    "game_result": "loss",
    }
    _, err := nk.WalletLedgerUpdate(ctx, itemID, metadata)
    if err != nil {
    logger.WithField("err", err).Error("Wallet ledger update error.")
    }
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Wallet Update**: Update a user's wallet with the given changeset. | user_id | `string` |  The ID of the user to update the wallet for. | `table`, `table`: The changeset after the update and previously to the update, respectively. |
    | | changeset | `table` | The set of wallet operations to apply. |
    | | metadata | Opt. `table` | Additional metadata to tag the wallet update with. |
    | | update_ledger | Opt. `bool` | Whether to record this update in the ledger. Defaults to `true`. |
    | **Wallets Update**: Update one or more user wallets with individual changesets. This function will also insert a new wallet ledger item into each user's wallet history that tracks their update. | updates | `table` | The set of user wallet update operations to apply. | `table`: A list of wallet updates results. |
    | | update_ledger | Opt. `bool` | Whether to record this update in the ledger. Default `true`. |
    | **Wallet Ledger List**: List all wallet updates for a particular user from oldest to newest. | user_id | `string` | The ID of the user to update the wallet. | A JS Object containing wallet entries with `id`, `user_id`, `create_time`, `update_time`, `changeset`, `metadata` parameters. |
    | | limit | Opt. `number` | Limit number of results. Defaults to 100. |
    | **Wallet Ledger Update**: Update the metadata for a particular wallet update in a users wallet ledger history. Useful when adding a note to a transaction for example. | id | `string` | The ID of the wallet ledger item to update. | The updated wallet ledger item. |
    | | metadata | `table` | The new metadata to set on the wallet ledger item. |

    Examples:
    ```lua
    -- Wallet Update
    local user_id = "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
    local changeset = {
    coins = 10, -- Add 10 coins to the user's wallet.
    gems = -5   -- Remove 5 gems from the user's wallet.
    }
    local metadata = {
    game_result = "won"
    }
    local updated, previous = nk.wallet_update(user_id, changeset, metadata, true)

    -- Wallets Update
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

    -- Wallet Ledger List
    local user_id = "8f4d52c7-bf28-4fcf-8af2-1d4fcf685592"
    local updates = nk.wallet_ledger_list(user_id)
    for _, u in ipairs(updates)
    do
    local message = string.format("Found wallet update with id: %q", u.id)
    nk.logger_info(message)
    end

    -- Wallet Ledger Update
    local id = "2745ba53-4b43-4f83-ab8f-93e9b677f33a"
    local metadata = {
    game_result = "loss"
    }
    local u = nk.wallet_ledger_update(id, metadata)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Wallet Update**: Update a user's wallet with the given changeset. | user_id | `string` |  The ID of the user to update the wallet for. | `nkruntime.WalletUpdateResult`: The changeset after the update and previously to the update, respectively. |
    | | changeset | `{[key: string]: number}` | The set of wallet operations to apply. |
    | | metadata | Opt. `Object` | Additional metadata to tag the wallet update with. |
    | | update_ledger | Opt. `bool` | Whether to record this update in the ledger. Defaults to `true`. |
    | **Wallets Update**: Update one or more user wallets with individual changesets. This function will also insert a new wallet ledger item into each user's wallet history that tracks their update. | updates | `nkruntime.WalletUpdate[]` | The set of user wallet update operations to apply. | `nkruntime.WalletUpdateResult`: A list of wallet updates results. |
    | | update_ledger | Opt. `bool` | Whether to record this update in the ledger. Default `true`. |
    | **Wallet Ledger List**: List all wallet updates for a particular user from oldest to newest. | user_id | `string` | The ID of the user to update the wallet. | A JS Object containing wallet entries with `Id`, `userId`, `createTime`, `updateTime`, `changeset`, `metadata` parameters. |
    | | limit | Opt. `number` | Limit number of results. Defaults to 100. |
    | **Wallet Ledger Update**: Update the metadata for a particular wallet update in a users wallet ledger history. Useful when adding a note to a transaction for example. | id | `string` | The ID of the wallet ledger item to update. | The updated wallet ledger item. |
    | | metadata | `table` | The new metadata to set on the wallet ledger item. |

    Examples:
    ```ts
    // Wallet Update
    let user_id = '8f4d52c7-bf28-4fcf-8af2-1d4fcf685592';
    let changeset = {
    coins: 10, // Add 10 coins to the user's wallet.
    gems: -5,   // Remove 5 gems from the user's wallet.
    }
    let metadata = {
    gameResult: 'won'
    }

    let result: nkruntime.WalletUpdateResult;
    try {
        result = nk.walletUpdate(user_id, changeset, metadata, true);
    } catch (error) {
        // Handle error
    }

    // Wallets Update
    let updates: nkruntime.WalletUpdate[] = [
    {
        userId: '8f4d52c7-bf28-4fcf-8af2-1d4fcf685592',
        changeset: {
        coins: 10, // Add 10 coins to the user's wallet.
        gems: -5,  // Remove 5 gems from the user's wallet.
        },
        metadata: {
        gameResult: 'won',
        }
    }
    ];

    let results: nkruntime.WalletUpdateResult[] = [];
    try {
        results = nk.walletsUpdate(updates);
    } catch (error) {
        // Handle error
    }

    // Wallet Ledger List
    let userId = '8f4d52c7-bf28-4fcf-8af2-1d4fcf685592';

    let results: nkruntime.WalletLedgerList[] = [];
    try {
        results = nk.walletLedgerList(userId);
    } catch (error) {
        // Handle error
    }

    // Wallet Ledger Update
    let id = '2745ba53-4b43-4f83-ab8f-93e9b677f33a';
    let metadata = {
    gameResult = 'loss';
    }

    let result: nkruntime.WalletLedgerResult;
    try {
        local result = nk.walletLedgerUpdate(id, metadata);
    } catch (error) {
        // Handle error
    }
    ```

## Hooks

### Register hooks

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Register Matchmaker Matched**: Registers a function that will be called when matchmaking finds opponents. | func | `function` | A function reference which will be executed on each matchmake completion. |
    | **Register Request After**: Register a function with the server which will be executed after every non-realtime message as specified while registering the function. | func | `function` | These are separate functions for each of those actions. |
    | **Register Request Before**: Register a function with the server which will be executed before any non-realtime message with the specified message name. | func | `function` | These are separate functions for each of those actions. |
    | **Register Realtime After**: Register a function with the server which will be executed after every realtime message with the specified message name. | func | `function` | A function reference which will be executed on each `msgname` message. |
    | | msgname | `string` | The specific [message name](basics.md#message-names) to execute the `func` function after. |
    | **Register Realtime Before**: Register a function with the server which will be executed before any realtime message with the specified message name. | func | `function` | A function reference which will be executed on each `msgname` message. The function should pass the `payload` input back as a return argument so the pipeline can continue to execute the standard logic. |
    | | msgname | `string` | The specific [message name](basics.md#message-names) to execute the `func` function after. |
    | **Register RPC**: Registers a function for use with client RPC to the server. | func | `function` | A function reference which will be executed on each RPC message. |
    | | id | `string` | The unique identifier used to register the `func` function for RPC. |
    | **Register Leaderboard Reset**: Registers a function to be run when a [leaderboard](#leaderboards) resets. | func | `function` | A function reference which will be executed on a leaderboard reset. |
    | **Register Tournament Reset**: Registers a function to be run when a [tournament](#tournaments) resets. | func | `function` | A function reference which will be executed on a tournament reset. |
    | **Register Tournament End**: Registers a function to be run when a [tournament](#tournaments) ends. | func | `function` | A function reference which will be executed on a tournament end. |

    Examples:
    ```go
    // Register Matchmaker Matched
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

    // Register Request After
    func AfterAddFriends(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.AddFriendsRequest) error {
    // Run some code.
    }

        // Register as an appropriate after hook, this call should be in InitModule.
    if err := initializer.RegisterAfterAddFriends(AfterAddFriends); err != nil {
    logger.WithField("err", err).Error("After add friends hook registration error.")
    return err
    }

    // Register Request Before
    func BeforeAddFriends(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.AddFriendsRequest) (*api.AddFriendsRequest, error) {
    // Run some code.
    return in, nil // Important!
    }

        // Register as an appropriate before hook, this call should be in InitModule.
    if err := initializer.RegisterBeforeAddFriends(BeforeAddFriends); err != nil {
    logger.WithField("err", err).Error("Before add friends hook registration error.")
    return err
    }

    // Register Realtime After
    func MyFunc(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime NakamaModule, envelope *rtapi.Envelope) error {
    // Run some code.
    }

        // Register as an appropriate after hook, this call should be in InitModule.
    if err := initializer.RegisterAfterRt("ChannelJoin", MyFunc); err != nil {
    logger.WithField("err", err).Error("After realtime hook registration error.")
    return err
    }
    // Register Realtime Before
    func MyFunc(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, envelope *rtapi.Envelope) (*rtapi.Envelope, error) {
    return envelope, nil // For code to keep processing the input message.
    }

        // Register as an appropriate before hook, this call should be in InitModule.
    if err := initializer.RegisterBeforeRt("ChannelJoin", MyFunc); err != nil {
    logger.WithField("err", err).Error("Before realtime hook registration error.")
    return err
    }
    // Register RPC
    func MyFunc(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
    logger.Info("Payload: %s", payload)
    return payload, nil
    }

        // Register as an RPC function, this call should be in InitModule.
    if err := initializer.RegisterRpc("my_func_id", MyFunc); err != nil {
    logger.WithField("err", err).Error("RPC registration error.")
    return err
    }
    // Register Leaderboard Reset
    func LeaderboardReset(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, lb runtime.Leaderboard, reset int64) error {
        // Custom logic runs on reset.
        return nil
    }

    if err := initializer.RegisterLeaderboardReset(LeaderboardReset); err != nil {
        logger.WithField("err", err).Error("Leaderboard reset registration error.")
        return err
    }

    // Register Tournament Reset
    func TournamentReset(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, lb runtime.Tournament, reset int64) error {
        // Custom logic runs on reset.
        return nil
    }

    if err := initializer.RegisterTournamentReset(TournamentReset); err != nil {
        logger.WithField("err", err).Error("Tournament reset registration error.")
        return err
    }

    // Register Tournament End
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
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Register Matchmaker Matched**: Registers a function that will be called when matchmaking finds opponents. | func | `function` | A function reference which will be executed on each matchmake completion. |
    | **Register Request After**: Register a function with the server which will be executed after every non-realtime message as specified while registering the function. | func | `function` | These are separate functions for each of those actions. |
    | | msgname | `string` | The specific [message name](basics.md#message-names) to execute the `func` function after. |
    | **Register Request Before**: Register a function with the server which will be executed before any non-realtime message with the specified message name. | func | `function` | These are separate functions for each of those actions. |
    | | msgname | `string` | The specific [message name](basics.md#message-names) to execute the `func` function after. |
    | **Register Realtime After**: Register a function with the server which will be executed after every realtime message with the specified message name. | func | `function` | A function reference which will be executed on each `msgname` message. |
    | | msgname | `string` | The specific [message name](basics.md#message-names) to execute the `func` function after. |
    | **Register Realtime Before**: Register a function with the server which will be executed before any realtime message with the specified message name. | func | `function` | A function reference which will be executed on each `msgname` message. The function should pass the `payload` input back as a return argument so the pipeline can continue to execute the standard logic. |
    | | msgname | `string` | The specific [message name](basics.md#message-names) to execute the `func` function after. |
    | **Register RPC**: Registers a function for use with client RPC to the server. | func | `function` | A function reference which will be executed on each RPC message. |
    | | id | `string` | The unique identifier used to register the `func` function for RPC. |
    | **Register Leaderboard Reset**: Registers a function to be run when a [leaderboard](#leaderboards) resets. | func | `function` | A function reference which will be executed on a leaderboard reset. |
    | **Register Tournament Reset**: Registers a function to be run when a [tournament](#tournaments) resets. | func | `function` | A function reference which will be executed on a tournament reset. |
    | **Register Tournament End**: Registers a function to be run when a [tournament](#tournaments) ends. | func | `function` | A function reference which will be executed on a tournament end. |

    Examples:
    ```lua
    -- Register Matchmaker Matched
        -- For example let's create a two player authoritative match.
    local function matchmaker_matched(context, matched_users)
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

    if #matched_users ~= 2 then
        return nil
    end

    if matched_users[1].properties["mode"] ~= "authoritative" then
        return nil
    end
    if matched_users[2].properties["mode"] ~= "authoritative" then
        return nil
    end

    return nk.match_create("match", { debug = true, expected_users = matched_users })
    end
    nk.register_matchmaker_matched(matchmaker_matched)

    -- Register Request After
    local function my_func(context, payload)
    -- Run some code.
    end
    nk.register_req_after(my_func, "FriendsAdd")

    -- Register Request Before
    local function my_func(context, payload)
    -- Run some code.
    return payload -- Important!
    end
    nk.register_req_before(my_func, "FriendsAdd")

    -- Register Realtime After
    local function my_func(context, payload)
    -- Run some code.
    end
    nk.register_rt_after(my_func, "ChannelJoin")

    -- Register Realtime Before
    local function my_func(context, payload)
    -- Run some code.
    return payload -- Important!
    end
    nk.register_rt_before(my_func, "ChannelJoin")

    -- Register RPC
    local function my_func(context, payload)
    -- Run some code.
    return payload
    end
    nk.register_rpc(my_func, "my_func_id")

    -- Register Leaderboard Reset
    local fn = function(ctx, leaderboard, reset) {
        -- Custom logic
    }
    nk.register_leaderboard_reset(fn)

    -- Register Tournament Reset
    local fn = function(ctx, tournament, t_end, reset) {
        -- Custom logic
    }
    nk.register_tournament_reset(fn)

    -- Register Tournament End
    local fn = function(ctx, tournament, t_end, reset) {
        -- Custom logic
    }
    nk.register_tournament_end(fn)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Register Matchmaker Matched**: Registers a function that will be called when matchmaking finds opponents. | func | `function` | A function reference which will be executed on each matchmake completion. |
    | **Register Request After**: Register a function with the server which will be executed after every non-realtime message as specified while registering the function. | func | `function` | These are separate functions for each of those actions. |
    | **Register Request Before**: Register a function with the server which will be executed before any non-realtime message with the specified message name. | func | `function` | These are separate functions for each of those actions. |
    | **Register Realtime After**: Register a function with the server which will be executed after every realtime message with the specified message name. | func | `function` | A function reference which will be executed on each `msgname` message. |
    | | msgname | `string` | The specific [message name](basics.md#message-names) to execute the `func` function after. |
    | **Register Realtime Before**: Register a function with the server which will be executed before any realtime message with the specified message name. | func | `function` | A function reference which will be executed on each `msgname` message. The function should pass the `payload` input back as a return argument so the pipeline can continue to execute the standard logic. |
    | | msgname | `string` | The specific [message name](basics.md#message-names) to execute the `func` function after. |
    | **Register RPC**: Registers a function for use with client RPC to the server. | func | `function` | A function reference which will be executed on each RPC message. |
    | | id | `string` | The unique identifier used to register the `func` function for RPC. |
    | **Register Leaderboard Reset**: Registers a function to be run when a [leaderboard](#leaderboards) resets. | func | `function` | A function reference which will be executed on a leaderboard reset. |
    | **Register Tournament Reset**: Registers a function to be run when a [tournament](#tournaments) resets. | func | `function` | A function reference which will be executed on a tournament reset. |
    | **Register Tournament End**: Registers a function to be run when a [tournament](#tournaments) ends. | func | `function` | A function reference which will be executed on a tournament end. |

    Examples:
    ```ts
    // Register Matchmaker Matched
        // For example let's create a two player authoritative match.
    function matchmakerMatched(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, matchedUsers: nkruntime.MatchmakerResult[]) {
        matchedUsers.forEach(u => {
            logger.info(u.presence.userId);
            logger.info(u.presence.sessionId);
            logger.info(u.presence.username);
            logger.info(u.presence.node);
            logger.info(JSON.stringify(u.properties));
        });

        if (matchedUsers.length !== 2) {
            return;
        }

        if (matchedUsers[0].properties.mode !== 'authoritative') {
            return;
        }
        if (matchedUsers[1].properties.mode !== 'authoritative') {
            return;
        }

        return nk.matchCreate('match', { debug: true, expectedUsers: matchedUsers });
    }

    initializer.registerMatchmakerMatched(matchmakerMatched);

    // Register Request After
    let afterAddFriendsFn: nkruntime.AfterHookFunction<void, nkruntime.AddFriendsRequest> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.AddFriendsRequest) {
        // Run some code.
    }
    initializer.registerAfterAddFriends(afterAddFriendsFn);

    // Register Request Before
    let beforeAddFriendsFn: nkruntime.BeforeHookFunction<nkruntime.AddFriendsRequest> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.AddFriendsRequest) {
        // Run some code.
        return data; // Important!
    }
    initializer.registerBeforeAddFriends(beforeAddFriendsFn);

    // Register Realtime After
    let rtAfterFn: nkruntime.RtBeforeHookFunction<nkruntime.EnvelopeChannelJoin> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, envelope: nkruntime.EnvelopeChannelJoin) {
        // Run some code.
    }
    initializer.registerRtAfter('ChannelJoin', rtAfterFn);

    // Register Realtime Before
    let rtBeforeFn: nkruntime.RtBeforeHookFunction<nkruntime.EnvelopeChannelJoin> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, envelope: nkruntime.EnvelopeChannelJoin): nkruntime.EnvelopeChannelJoin {
        // Run some code.
        return envelope;
    }
    initializer.registerRtBefore('ChannelJoin', rtBeforeFn);

    // Register RPC
    let rpcFn: nkruntime.RpcFunction = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
        // Run some code.
    }
    initializer.registerRpc('my_func_id', rpcFn);

    // Register Leaderboard Reset
    let leaderboardResetFn: nkruntime.LeaderboardResetFunction = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, leaderboard: nkruntime.Leaderboard, reset: number) {
        // Custom logic runs on reset.
    }
    initializer.registerLeaderboardReset(leaderboardResetFn);

    // Register Tournament Reset
    let tournamentResetFn: nkruntime.TournamentResetFunction = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, tournament: nkruntime.Tournament, reset: number) {
        // Custom logic runs on reset.
    }
    initializer.registerTournamentReset(tournamentResetFn);

    // Register Tournament End
    let tournamentEndFn: nkruntime.TournamentEndFunction = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, tournament: nkruntime.Tournament, end: number) {
        // Custom logic runs on end.
    }
    initializer.registerTournamentEnd(tournamentEndFn);
    ```

### Run once

The runtime environment allows you to run code that must only be executed only once. This is useful if you have custom SQL queries that you need to perform (like creating a new table) or to register with third party services.

=== "Go"
    Go runtime modules do not need a dedicated 'run once' function, use the `InitModule` function to achieve the same effect.

    Example:
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

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | run_once | func | `function` | A function reference which will be executed only once. |

    Example:
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

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | run_once | func | `function` | A function reference which will be executed only once. |

    Example:
    ```typescript
    let InitModule: nkruntime.InitModule =
            function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
        let systemId: string = ctx.env["SYSTEM_ID"]

        nk.sqlExec(`
    INSERT INTO users (id, username)
    VALUES ($1, $2)
    ON CONFLICT (id) DO NOTHING
        `, { systemId, "system_id" })

        logger.Info('system id: %s', systemId)
    }
    ```

## Utils

### AES128

!!! note "Note"
    If a non-CFB (Cipher FeedBack) operation is required use the Go runtime functions.

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Decrypt** | input | `string` | The string which has been aes128 encrypted. | `string`: Deciphered input. |
    | | key | `string` | The 16 Byte decryption key | |
    | **Encrypt** | input | `string` | The string which will be aes128 encrypted. | `string`: Ciphered input. |
    | | key | `string` | The 16 Byte encryption key | |

    Examples:
    ```lua
    -- Decrypt
    local plaintext = nk.aes128_decrypt("48656C6C6F20776F726C64", "goldenbridge_key")

    -- Encrypt
    local cyphertext = nk.aes128_encrypt("48656C6C6F20776F726C64", "goldenbridge_key")
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Decrypt** | input | `string` | The string which has been aes128 encrypted. | `string`: Deciphered input. |
    | | key | `string` | The 16 Byte decryption key | |
    | **Encrypt** | input | `string` | The string which will be aes128 encrypted. | `string`: Ciphered input. |
    | | key | `string` | The 16 Byte encryption key | |

    Examples:
    ```ts
    // Decrypt
    let ciphertext: string;
    try {
        ciphertext = nk.aes128Encrypt('48656C6C6F20776F726C64', 'goldenbridge_key');
    } catch (error) {
        // Handle error
    }

    // Encrypt
    let plaintext: string;
    try {
        plaintext = nk.aes128Decrypt('48656C6C6F20776F726C64', 'goldenbridge_key');
    } catch (error) {
        // Handle error
    }
    ```

=== "Go"
    Use the standard Go crypto package:

    ```go
    import "crypto/aes"
    ```

### AES256

!!! note "Note"
    If a non-CFB (Cipher FeedBack) operation is required use the Go runtime functions.

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Decrypt** | input | `string` | The string which has been aes256 encrypted. | `string`: Deciphered input. |
    | | key | `string` | The 32 Byte decryption key | |
    | **Encrypt** | input | `string` | The string which will be aes256 encrypted. | `string`: Ciphered input. |
    | | key | `string` | The 32 Byte encryption key | |

    Examples:
    ```lua
    -- Decrypt
    local plaintext = nk.aes256_decrypt("48656C6C6F20776F726C64", "goldenbridge_key")

    -- Encrypt
    local cyphertext = nk.aes256_encrypt("48656C6C6F20776F726C64", "goldenbridge_key")
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Decrypt** | input | `string` | The string which has been aes256 encrypted. | `string`: Deciphered input. |
    | | key | `string` | The 32 Byte decryption key | |
    | **Encrypt** | input | `string` | The string which will be aes256 encrypted. | `string`: Ciphered input. |
    | | key | `string` | The 32 Byte encryption key | |

    Examples:
    ```ts
    // Decrypt
    let ciphertext: string;
    try {
        ciphertext = nk.aes256Encrypt('48656C6C6F20776F726C64', 'goldenbridge_key');
    } catch (error) {
        // Handle error
    }

    // Encrypt
    let plaintext: string;
    try {
        plaintext = nk.aes256Decrypt('48656C6C6F20776F726C64', 'goldenbridge_key');
    } catch (error) {
        // Handle error
    }
    ```

=== "Go"
    Use the standard Go crypto package:

    ```go
    import "crypto/aes"
    ```

### Base16

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Decode** | input | `string` | The string which will be base16 decoded. | `[]byte`: Decoded string. |
    | **Encode** | input | `string` | The string which will be base16 encoded. | `[]byte`: Encoded string. |

    Examples:
    ```go
    // Use the standard Go encoding package.
    import "encoding/hex"
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Decode** | input | `string` | The string which will be base16 decoded. | `string`: Decoded string. |
    | **Encode** | input | `string` | The string which will be base16 encoded. | `string`: Encoded string. |

    Examples:
    ```lua
    -- Decode
    local decoded = nk.base16_decode("48656C6C6F20776F726C64")
    nk.logger_info(decoded) -- outputs "Hello world".

    -- Encode
    local encoded = nk.base16_encode("Hello world")
    nk.logger_info(encoded) -- outputs "48656C6C6F20776F726C64"
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Decode** | input | `string` | The string which will be base16 decoded. | `string`: Decoded string. |
    | **Encode** | input | `string` | The string which will be base16 encoded. | `string`: Encoded string. |

    Examples:
    ```ts
    // Decode
    let result: string;
    try {
        result = nk.base16Decode('48656C6C6F20776F726C64');
    } catch (error) {
        // Handle error
    }

    // Encode
    let result: string;
    try {
        result = nk.base16Encode('Hello World');
    } catch (error) {
        // Handle error
    }
    logger.info(encoded) // outputs '48656C6C6F20776F726C64'
    ```

### Base64

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Decode** | input | `string` | The string which will be base64 decoded. | `[]byte`: Decoded string. |
    | **Encode** | input | `string` | The string which will be base64 encoded. | `[]byte`: Encoded string. |
    | **URL Decode** | input | `string` | The string which will be base64 URL decoded. | `[]byte`: Decoded string. |
    | **URL Encode** | input | `string` | The string which will be base64 URL encoded. | `[]byte`: Encoded string. |

    Examples:
    ```go
    // Use the standard Go encoding package.
    import "encoding/base64"
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Decode** | input | `string` | The string which will be base64 decoded. | `string`: Decoded string. |
    | **Encode** | input | `string` | The string which will be base64 encoded. | `string`: Encoded string. |
    | **URL Decode** | input | `string` | The string which will be base64 URL decoded. | `string`: Decoded string. |
    | **URL Encode** | input | `string` | The string which will be base64 URL encoded. | `string`: Encoded string. |

    Examples:
    ```lua
    -- Decode
    local decoded = nk.base64_decode("SGVsbG8gd29ybGQ=")
    nk.logger_info(decoded) -- outputs "Hello world".

    -- Encode
    local encoded = nk.base64_encode("Hello world")
    nk.logger_info(encoded) -- outputs "SGVsbG8gd29ybGQ="

    -- URL Decode
    local decoded = nk.base64url_decode("SGVsbG8gd29ybGQ=")
    nk.logger_info(decoded) -- outputs "Hello world".

    -- URL Encode
    local encoded = nk.base64url_encode("Hello world")
    nk.logger_info(encoded) -- outputs "SGVsbG8gd29ybGQ="
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Decode** | input | `string` | The string which will be base16 decoded. | `string`: Decoded string. |
    | **Encode** | input | `string` | The string which will be base16 encoded. | `string`: Encoded string. |
    | **URL Decode** | input | `string` | The string which will be base64 URL decoded. | `string`: Decoded string. |
    | **URL Encode** | input | `string` | The string which will be base64 URL encoded. | `string`: Encoded string. |

    Examples:
    ```ts
    // Decode
    let result: string;
    try {
        result = nk.base64Decode('SGVsbG8gd29ybGQ=');
    } catch (error) {
        // Handle error
    }
    logger.info(encoded) // outputs 'Hello world'

    // Encode
    let result: string;
    try {
        result = nk.base64Encode('Hello World');
    } catch (error) {
        // Handle error
    }
    logger.info(encoded) // outputs 'SGVsbG8gd29ybGQ='

    // URL Decode
    let result: string;
    try {
        result = nk.base64UrlDecode('SGVsbG8gd29ybGQ=');
    } catch (error) {
        // Handle error
    }
    logger.info(encoded) // outputs 'Hello World'

    // URL Encode
    let result: string;
    try {
        result = nk.base64UrlEncode('Hello World');
    } catch (error) {
        // Handle error
    }
    logger.info(encoded) // outputs 'SGVsbG8gd29ybGQ='
    ```


### BCrypt

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **BCrypt Hash**: Generate one-way hashed string using bcrypt. | input | `string` | The string which will be bcrypted. | `[]byte`: Hashed string. |
    | **BCrypt Compare**: Compare hashed input against a plaintext input. | hash | `string` | The string that is already bcrypted. | True if they are the same, false otherwise. |

    Examples:
    ```go
    // Use the standard Go crypto package.
    import "golang.org/x/crypto/bcrypt"
    ```


=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **BCrypt Hash**: Generate one-way hashed string using bcrypt. | input | `string` | The string which will be bcrypted. | `string`: Hashed string. |
    | **BCrypt Compare**: Compare hashed input against a plaintext input. | hash | `string` | The string that is already bcrypted. | True if they are the same, false otherwise. |

    Examples:
    ```lua
    -- BCrypt Hash
    local hashed = nk.bcrypt_hash("Hello World")
    nk.logger_info(hashed)

    -- BCrypt Compare
    local is_same = nk.bcrypt_compare("$2a$04$bl3tac7Gwbjy04Q8H2QWLuUOEkpoNiAeTxazxi4fVQQRMGbMaUHQ2", "123456")
    nk.logger_info(is_same) -- outputs true.
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **BCrypt Hash**: Generate one-way hashed string using bcrypt. | input | `string` | The string which will be bcrypted. | `string`: Hashed string. |
    | **BCrypt Compare**: Compare hashed input against a plaintext input. | hash | `string` | The string that is already bcrypted. | True if they are the same, false otherwise. |

    Examples:
    ```typescript
    // BCrypt Hash
    let result: string;
    try {
        result = nk.bcryptHash('Hello World');
    } catch (error) {
        // Handle error
    }

    // BCrypt Compare
    let result: boolean;
    try {
        result = nk.bcryptCompare('$2a$04$bl3tac7Gwbjy04Q8H2QWLuUOEkpoNiAeTxazxi4fVQQRMGbMaUHQ2', '123456');
    } catch (error) {
        // Handle error
    }
    ```

### HMAC

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **HMAC SHA256 Hash**: Create a HMAC-SHA256 hash from input and key. | input | `string` | Plaintext input to hash. | Hashed input as a string using the key. |
    | | key | `string` | Hashing key. |

    Example:
    ```go
    // Use the standard Go crypto package.
    import "crypto/hmac"
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **HMAC SHA256 Hash**: Create a HMAC-SHA256 hash from input and key. | input | `string` | Plaintext input to hash. | Hashed input as a string using the key. |
    | | key | `string` | Hashing key. |

    Example:
    ```lua
    local hash = nk.hmac_sha256_hash("encryptthis", "somekey")
    print(hash)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **HMAC SHA256 Hash**: Create a HMAC-SHA256 hash from input and key. | input | `string` | Plaintext input to hash. | Hashed input as a string using the key. |
    | | key | `string` | Hashing key. |

    Example:
    ```typescript
    let hash: string;
    try {
        hash = nk.hmacSha256Hash('some input text to hash', 'some_key');
    } catch (error) {
        // Handle error
    }
    ```

### MD5

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **md5_hash**: Create an md5 hash from the input. | input | string | The input string to hash. | A string with the md5 hash of the input. |

    Example:
    ```go
    // Use the standard Go crypto package.
    import "crypto/md5"
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **md5_hash**: Create an md5 hash from the input. | input | string | The input string to hash. | A string with the md5 hash of the input. |

    Example:
    ```lua
    local input = "somestring"
    local hashed = nk.md5_hash(input)
    nk.logger_info(hashed)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **md5_hash**: Create an md5 hash from the input. | input | string | The input string to hash. | A string with the md5 hash of the input. |

    Example:
    ```typescript
    let input = 'somestring';
    let hashed = nk.md5Hash(input);
    logger.info(hashed);
    ```

### CRON

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Cron Next**: Parses a CRON expression and a timestamp in UTC seconds, and returns the next matching timestamp in UTC seconds. | expression | `string` | A valid CRON expression in standard format, for example "* * * * *". | The next UTC seconds timestamp (number) that matches the given CRON expression, and is immediately after the given timestamp. |
    | | timestamp  | `number` | A time value expressed as UTC seconds. |

    Example:
    ```go
    // Use a Go CRON package, for example:
    import "github.com/robfig/cron"
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Cron Next**: Parses a CRON expression and a timestamp in UTC seconds, and returns the next matching timestamp in UTC seconds. | expression | `string` | A valid CRON expression in standard format, for example "* * * * *". | The next UTC seconds timestamp (number) that matches the given CRON expression, and is immediately after the given timestamp. |
    | | timestamp  | `number` | A time value expressed as UTC seconds. |

    Example:
    ```lua
    -- Based on the current time, return the UTC seconds value representing the
    -- nearest upcoming Monday at 00:00 UTC (midnight.)
    local expr = "0 0 * * 1"
    local ts = os.time()
    local next = nk.cron_next(expr, ts)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Cron Next**: Parses a CRON expression and a timestamp in UTC seconds, and returns the next matching timestamp in UTC seconds. | expression | `string` | A valid CRON expression in standard format, for example "* * * * *". | The next UTC seconds timestamp (number) that matches the given CRON expression, and is immediately after the given timestamp. |
    | | timestamp  | `number` | A time value expressed as UTC seconds. |

    Example:
    ```typescript
    let result: number;
    try {
        let expr = '0 0 * * 1';
        let ts = Math.floor(Date.now() / 1000);
        result = nk.cronNext(expr, ts);
    } catch (error) {
        // Handle error
    }
    ```

### HTTP

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **http_request**: Send a HTTP request and receive the result as a Lua table. | url | string | The URL of the web resource to request. | `table`: `code, headers, body` - Multiple return values for the HTTP response. |
    | | method | `string` | The HTTP method verb used with the request. |
    | | headers | Opt. `table` | A table of headers used with the request. |
    | | content | Opt. `string` | The bytes to send with the request. |
    | | timeout | Opt. `number` | Timeout of the request in milliseconds. Optional, by default is 5000ms. |

    Example:
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

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **http_request**: Send a HTTP request and receive the result as a Lua table. | url | `string` | The URL of the web resource to request. | `nkruntime.httpResponse`: `code, headers, body` - Multiple return values for the HTTP response. |
    | | method | `string` | The HTTP method verb used with the request. |
    | | headers | Opt. `string` | A table of headers used with the request. |
    | | content | Opt. `string` | The bytes to send with the request. |
    | | timeout | Opt. `number` | Timeout of the request in milliseconds. Optional, by default is 5000ms. |

    Example:
    ```typescript
    let method: nkruntime.RequestMethod = 'get';
    let headers = {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
    };
    let body = JSON.stringify({});

    let res = {} as nkruntime.HttpResponse;
    try {
        res = nk.httpRequest('https://google.com', method, headers, body);
    } catch (error) {
        // Handle error
    }

    if (res.code >= 400) {
        logger.error('Failed %q', res);
    } else {
        logger.info('Success %q %q', res.code, res.body);
    }
    ```

=== "Go"
    ```go
    // Use the standard Go HTTP package.
    import "net/http"
    ```

### JSON

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **json_decode**: Decode the JSON input as a Lua table. | input | `string` | The JSON encoded input. | `table`: Decoded JSON input as a Lua table. |
    | **json_encode**: Encode the input as JSON. | input | `string` | The input to encode as JSON . | The encoded JSON string. |

    Example:
    ```lua
    -- json_decode
    local json = nk.json_decode('{"hello": "world"}')
    nk.logger_info(json.hello)

    -- json_encode
    local input = {["some"] = "json"}
    local json = nk.json_encode(input)
    nk.logger_info(json) -- Outputs '{"some": "json"}'.
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **json_decode**: Decode the JSON input as a Lua table. | input | `string` | The JSON encoded input. | `table`: Decoded JSON input as a Lua table. |
    | **json_encode**: Encode the input as JSON. | input | `string` | The input to encode as JSON . | The encoded JSON string. |

    Example:
    ```typescript
    // json_decode - Use the JS global JSON object.
    let obj = JSON.parse('{"hello": "world"}');
    logger.info('Hello %s', obj.hello); // Prints 'Hello world'

    // json_encode - Use the JS global JSON object.
    let encodedJson: string = JSON.stringify(obj);
    logger.info('Encoded json: %s', encodedJson);
    ```

=== "Go"
    ```go
    // Use the standard Go JSON package.
    import "encoding/json"
    ```

### Logger

=== "Go"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Logger Error**: Write an ERROR level message to the server logs. | message | `string` | The message to write to server logs with ERROR level severity. |
    | | vars | | Variables to replace placeholders in message. |
    | **Logger Info**: Write an INFO level message to the server logs. | message | `string` | The message to write to server logs with INFO level severity. |
    | | vars | | Variables to replace placeholders in message. |
    | **Logger Warn**: Write an WARN level message to the server logs. | message | `string` | The message to write to server logs with WARN level severity. |
    | | vars | | Variables to replace placeholders in message. |

    Examples:
    ```go
    // Logger Error
    logger.Error("%s - %s", "hello", "world")

    // Logger Info
    logger.Info("%s - %s", "hello", "world")

    // Logger Warn
    logger.Warn("%s - %s", "hello", "world")
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Logger Error**: Write an ERROR level message to the server logs. | message | `string` | The message to write to server logs with ERROR level severity. |
    | | vars | | Variables to replace placeholders in message. |
    | **Logger Info**: Write an INFO level message to the server logs. | message | `string` | The message to write to server logs with INFO level severity. |
    | | vars | | Variables to replace placeholders in message. |
    | **Logger Warn**: Write an WARN level message to the server logs. | message | `string` | The message to write to server logs with WARN level severity. |
    | | vars | | Variables to replace placeholders in message. |

    Examples:
    ```lua
    -- Logger Error
    local message = string.format("%q - %q", "hello", "world")
    nk.logger_error(message)

    -- Logger Info
    local message = string.format("%q - %q", "hello", "world")
    nk.logger_info(message)

    -- Logger Warn
    local message = string.format("%q - %q", "hello", "world")
    nk.logger_warn(message)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **Logger Error**: Write an ERROR level message to the server logs. | message | `string` | The message to write to server logs with ERROR level severity. |
    | | vars | | Variables to replace placeholders in message. |
    | **Logger Info**: Write an INFO level message to the server logs. | message | `string` | The message to write to server logs with INFO level severity. |
    | | vars | | Variables to replace placeholders in message. |
    | **Logger Warn**: Write an WARN level message to the server logs. | message | `string` | The message to write to server logs with WARN level severity. |
    | | vars | | Variables to replace placeholders in message. |

    Examples:
    ```typescript
    // Logger Error
    logger.error('%s - %s', 'hello', 'world');

    // Logger Info
    logger.info('%s - %s', 'hello', 'world');

    // Logger Warn
    logger.warn('%s - %s', 'hello', 'world');
    ```

### SQL

These functions allow your Lua/TS scripts to run arbitrary SQL statements beyond the ones built into Nakama itself. It is your responsibility to manage the performance of these queries.

=== "Go"
    Use the standard Go sql package.
    ```go
    import "database/sql"
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **sql_exec**: Execute an arbitrary SQL query and return the number of rows affected. Typically an `"INSERT"`, `"DELETE"`, or `"UPDATE"` statement with no return columns. | query | `string` | A SQL query to execute. | `number`: A list of matches matching the parameters criteria.|
    | | parameters | `table` | Arbitrary parameters to pass to placeholders in the query. |
    | **sql_query**: Execute an arbitrary SQL query that is expected to return row data. Typically a `"SELECT"` statement. | query | `string` | A SQL query to execute. | `table`: A table of rows and the respective columns and values. |
    | | parameters | `table` | Arbitrary parameters to pass to placeholders in the query. |

    Examples:
    ```lua
    -- sql_exec: This example query deletes all expired leaderboard records.
    local query = [[DELETE FROM leaderboard_record
                    WHERE expires_at > 0 AND expires_at <= $1]]
    local parameters = { os.time() * 1000 }
    local affected_rows_count = nk.sql_exec(query, parameters)

    -- sql_query: Example fetching a list of usernames for the 100 most recently signed up users.
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

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **sql_exec**: Execute an arbitrary SQL query and return the number of rows affected. Typically an `"INSERT"`, `"DELETE"`, or `"UPDATE"` statement with no return columns. | query | `string` | A SQL query to execute. | `{rowsAffected: number}`: A list of matches matching the parameters criteria.|
    | | parameters | `any[]` | Arbitrary parameters to pass to placeholders in the query. |
    | **sql_query**: Execute an arbitrary SQL query that is expected to return row data. Typically a `"SELECT"` statement. | query | `string` | A SQL query to execute. | `nkruntime.SqlQueryResult`: An array of rows and the respective columns and values. |
    | | parameters | `any[]` | Arbitrary parameters to pass to placeholders in the query. |

    Examples:
    ```typescript
    // sql_exec: This example query deletes all expired leaderboard records.
    let query = 'DELETE FROM leaderboard_record WHERE expires_at > 0 AND expires_at <= $1';
    let parameters = [ Math.floor(Date.now() / 1000) ];
    let result;
    try {
        result = nk.sqlExec(query, parameters);
    } catch (error) {
        // Handle error
    }

    logger.info('Affected %d rows', result?.rowsAffected);

    // sql_query
    let query = 'SELECT username, create_time FROM users ORDER BY create_time DESC LIMIT 100';
    let parameters: any[] = [];

    let rows: nkruntime.SqlQueryResult = [];
    try {
        rows = nk.sqlQuery(query, parameters);
    } catch (error) {
        // Handler error
    }

    rows.forEach(r => {
        logger.info('Username %q created at %q', row.username, row.create_time);
    });
    ```

### Time

Get the current UTC time in milliseconds using the system wall clock.

=== "Go"
    Use the standard Go time package.

    Example:
    ```go
    import "time"
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **time()** | | | | A number representing the current UTC time in milliseconds. |

    Example:
    ```lua
    local utc_msec = nk.time()
    ```

=== "TypeScript"
    Use the standard Date package.

    Example:
    ```typescript
    let utcMsec = Date.now();
    ```

### UUID

=== "Go"
    Use a separate Go package for UUIDs.
    ```go
    import "github.com/gofrs/uuid"
    ```

=== "Lua"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **uuid_v4()**: Generate a version 4 UUID in the standard 36-character string representation. | | | | The generated version 4 UUID identifier string. |
    | **uuid_bytes_to_string**: Convert the 16-byte raw representation of a UUID into the equivalent 36-character standard UUID string representation. Will raise an error if the input is not valid and cannot be converted. | uuid_bytes | `string` | The UUID bytes to convert. | A string containing the equivalent 36-character standard representation of the UUID. |
    | **uuid_string_to_bytes**: Convert the 36-character string representation of a UUID into the equivalent 16-byte raw UUID representation. Will raise an error if the input is not valid and cannot be converted. | uuid_string | `string` | The UUID string to convert. | A string containing the equivalent 16-byte representation of the UUID. |

    Example:
    ```lua
    -- uuid_v4
    local uuid = nk.uuid_v4()
    nk.logger_info(uuid)

    -- uuid_bytes_to_string
    local uuid_bytes = "896418357731323983933079013" -- some uuid bytes.
    local uuid_string = nk.uuid_bytes_to_string(uuid_bytes)
    nk.logger_info(uuid_string)

    -- uuid_string_to_bytes
    local uuid_string = "4ec4f126-3f9d-11e7-84ef-b7c182b36521" -- some uuid string.
    local uuid_bytes = nk.uuid_string_to_bytes(uuid_string)
    nk.logger_info(uuid_bytes)
    ```

=== "TypeScript"
    | Action | Parameter | Type | Description | Returns |
    |-|-|-|-|-|
    | **uuid_v4()**: Generate a version 4 UUID in the standard 36-character string representation. | | | | The generated version 4 UUID identifier string. |

    Example:
    ```typescript
    // uuid_v4
    let uuid = nk.uuidV4();
    ```
