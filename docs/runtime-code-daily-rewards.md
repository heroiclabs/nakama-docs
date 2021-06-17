# Daily Rewards Tutorial

A common way of engaging and retaining players is to offer them a daily reward for logging into your game each day.

In this example, you'll learn how to implement a daily reward system using Server Runtime Code.

<iframe width="560" height="315" src="https://www.youtube.com/embed/Zw-KEs6eBoE" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Prerequisites

To easily follow with this tutorial, perform the following before proceeding:

* Download the [Nakama Project Template](https://github.com/heroiclabs/nakama-project-template) or
* Follow the [TypeScript Setup Guide](/runtime-code-typescript-setup)

## Registering the RPCs

The daily reward sample defines two RPCs ([Remote Procedure Calls](/runtime-code-basics/#rpc-example)) to check eligibility and issue the reward.

The RPCs are then registered to Nakama events in either `main.ts`, `main.go` or `main.lua`.

=== "TypeScript"
    ```typescript
    function InitModule(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
        initializer.registerRpc('canclaimdailyreward_js', rpcCanClaimDailyReward);
        initializer.registerRpc('claimdailyreward_js', rpcClaimDailyReward);
        logger.info('JavaScript logic loaded.');
    }

    ```
=== "Go"
    ```go
    package main

    import (
        "context"
        "database/sql"
        "time"

        "github.com/heroiclabs/nakama-common/runtime"
        "github.com/heroiclabs/unity-devrel-samples/modules"
    )

    const (
        rpcIdCanClaimDailyReward = "canclaimdailyreward_go"
        rpcIdClaimDailyReward    = "claimdailyreward_go"
    )

    //noinspection GoUnusedExportedFunction
    func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
        initStart := time.Now()

        if err := initializer.RegisterRpc(rpcIdCanClaimDailyReward, modules.RpcCanClaimDailyReward); err != nil {
            return err
        }

        if err := initializer.RegisterRpc(rpcIdClaimDailyReward, modules.RpcClaimDailyReward); err != nil {
            return err
        }

        logger.Info("Plugin loaded in '%d' msec.", time.Since(initStart).Milliseconds())
        return nil
    }
    ```
=== "Lua"
    ```lua
    local nk = require("nakama")
    local daily_reward = require("daily_reward")

    nk.register_rpc(daily_reward.rpc_can_claim_daily_reward, "canclaimdailyreward_lua")
    nk.register_rpc(daily_reward.rpc_claim_daily_reward, "claimdailyreward_lua")
    ```

To register an RPC with the server, you need to specify a string identifier as well as a function to run when the RPC is called by a client.

## Implementing the RPCs

The RPCs implement the following logic:

### `canClaimDailyReward`

* Get the latest daily reward object from the [Nakama Storage Engine](/storage-collections)
* Check to see if the last time the user claimed a reward was before 00:00
* Return a JSON response indicating if the user can claim a daily reward

### `claimDailyReward`

* Get the latest daily reward object from the [Nakama Storage Engine](/storage-collections)
* Check to see if the last time the user claimed a reward was before 00:00
* Update the user's [Wallet](/runtime-code-function-reference/#wallet)
* Send a [Notification](/social-in-app-notifications) to the user
* Update or create the daily reward object in the [Nakama Storage Engine](/storage-collections)
* Return a JSON response with the number of coins received

### Module code (Go/Lua)

This section is specifically for people using Go or Lua. There is some additional code you will need to include in your daily reward module scripts for each respective language.

=== "Go"
    ```go
    package modules

    import (
        "context"
        "database/sql"
        "encoding/json"
        "time"

        "github.com/heroiclabs/nakama-common/api"
        "github.com/heroiclabs/nakama-common/runtime"
    )

    var (
        errInternalError  = runtime.NewError("internal server error", 13) // INTERNAL
        errMarshal        = runtime.NewError("cannot marshal type", 13)   // INTERNAL
        errNoInputAllowed = runtime.NewError("no input allowed", 3)       // INVALID_ARGUMENT
        errNoUserIdFound  = runtime.NewError("no user ID in context", 3)  // INVALID_ARGUMENT
        errUnmarshal      = runtime.NewError("cannot unmarshal type", 13) // INTERNAL
    )
    ```

=== "Lua"
    ```lua
    local nk = require("nakama")

    local M = {
    }

    -- Module code goes here

    return M
    ```

### Getting the last daily reward object

Let's take a look at the code for retrieving the latest daily reward object from the Nakama Storage Engine.

=== "TypeScript"
    ```typescript
    function getLastDailyRewardObject(context: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string) : any {
        if (!context.userId) {
            throw Error('No user ID in context');
        }

        if (payload) {
            throw Error('No input allowed');
        }

        var objectId: nkruntime.StorageReadRequest = {
            collection: 'reward',
            key: 'daily',
            userId: context.userId,
        }

        var objects: nkruntime.StorageObject[];
        try {
            objects = nk.storageRead([ objectId ]);
        } catch (error) {
            logger.error('storageRead error: %s', error);
            throw error;
        }

        var dailyReward: any = {
            lastClaimUnix: 0,
        }

        objects.forEach(object => {
            if (object.key == 'daily') {
                dailyReward = object.value;
            }
        });

        return dailyReward;
    }
    ```

=== "Go"
    ```go
    type dailyReward struct {
        LastClaimUnix int64 `json:"last_claim_unix"` // The last time the user claimed the reward in UNIX time.
    }

    func getLastDailyRewardObject(ctx context.Context, logger runtime.Logger, nk runtime.NakamaModule, payload string) (dailyReward, *api.StorageObject, error) {
        var d dailyReward
        d.LastClaimUnix = 0

        userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
        if !ok {
            return d, nil, errNoUserIdFound
        }

        if len(payload) > 0 {
            return d, nil, errNoInputAllowed
        }

        objects, err := nk.StorageRead(ctx, []*runtime.StorageRead{{
            Collection: "reward",
            Key:        "daily",
            UserID:     userID,
        }})
        if err != nil {
            logger.Error("StorageRead error: %v", err)
            return d, nil, errInternalError
        }

        var o *api.StorageObject
        for _, object := range objects {
            switch object.GetKey() {
            case "daily":
                if err := json.Unmarshal([]byte(object.GetValue()), &d); err != nil {
                    logger.Error("Unmarshal error: %v", err)
                    return d, nil, errUnmarshal
                }
                return d, object, nil
            }
        }

        return d, o, nil
    }
    ```

=== "Lua"
    ```lua
    function get_last_daily_reward_object(context, payload)
        if (not context.user_id or #context.user_id < 1) then
            error({ "no user ID in context", 3 })
        end

        if (#payload > 0) then
            error({ "no input allowed", 3 })
        end

        local objectid = {
            collection = "reward",
            key = "daily",
            user_id = context.user_id
        }
        local success, objects = pcall(nk.storage_read, { objectid })
        if (not success) then
            nk.logger_error(string.format("storage_read error: %q", objects))
            error({ "internal server error", 13 })
        end

        local daily_reward = {
            ["last_claim_unix"] = 0
        }
        for _, object in ipairs(objects)
        do
            if (object.key == "daily") then
                daily_reward = object.value
                break
            end
        end

        return daily_reward
    end
    ```

Regardless of the language you use, the core logic remains the same.

* Check the context to ensure there is a valid user ID
* Check the user did NOT pass anything in the payload
* Query the storage engine for a `daily` object in the `reward` collection
* Return the daily reward object or a default one

### Checking if the user is eligible to receive a daily reward

=== "TypeScript"
    ```typescript
    function canUserClaimDailyReward(dailyReward: any) {
        if (!dailyReward.lastClaimUnix) {
            dailyReward.lastClaimUnix = 0;
        }

        var d = new Date();
        d.setHours(0, 0, 0, 0);

        return dailyReward.lastClaimUnix < msecToSec(d.getTime());
    }

    function msecToSec(n: number): number {
        return Math.floor(n / 1000);
    }
    ```

=== "Go"
    ```go
    func canUserClaimDailyReward(d dailyReward) bool {
        t := time.Now()
        midnight := time.Date(t.Year(), t.Month(), t.Day(), 0, 0, 0, 0, time.Local)
        return time.Unix(d.LastClaimUnix, 0).Before(midnight)
    }
    ```

=== "Lua"
    ```lua
    function can_user_claim_daily_reward(daily_reward)
        local dt = os.date("*t")
        local elapsed_sec_from_midnight = (dt.hour * 3600 + dt.min * 60 + dt.sec) % 86400
        return daily_reward.last_claim_unix < (os.time() - elapsed_sec_from_midnight)
    end
    ```

This function checks the last claim Unix timestamp value of the daily reward object. If it is less than the timestamp for midnight of the previous day, it returns true, otherwise it returns false.

### CanClaimDailyReward RPC

With the two helper functions complete it's time to implement the first of the RPCs. This RPC will return the value of the helper function that checks the user's eligibility as a JSON object.

=== "TypeScript"
    ```typescript
    function rpcCanClaimDailyReward(context: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
        var dailyReward = getLastDailyRewardObject(context, logger, nk, payload);
        var response = {
            canClaimDailyReward: canUserClaimDailyReward(dailyReward)
        }

        var result = JSON.stringify(response);
        logger.debug('rpcCanClaimDailyReward response: %q', result);

        return result;
    }
    ```

=== "Go"
    ```go
    func RpcCanClaimDailyReward(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
        var resp struct {
            CanClaimDailyReward bool `json:"canClaimDailyReward"`
        }

        dailyReward, _, err := getLastDailyRewardObject(ctx, logger, nk, payload)
        if err != nil {
            logger.Error("Error getting daily reward: %v", err)
            return "", errInternalError
        }

        resp.CanClaimDailyReward = canUserClaimDailyReward(dailyReward)

        out, err := json.Marshal(resp)
        if err != nil {
            logger.Error("Marshal error: %v", err)
            return "", errMarshal
        }

        logger.Debug("rpcCanClaimDailyReward resp: %v", string(out))
        return string(out), nil
    }
    ```

=== "Lua"
    ```lua
    function M.rpc_can_claim_daily_reward(context, payload)
        local daily_reward = get_last_daily_reward_object(context, payload)
        local resp = {
            ["canClaimDailyReward"] = can_user_claim_daily_reward(daily_reward)
        }

        local success, result = pcall(nk.json_encode, resp)
        if (not success) then
            nk.logger_error(string.format("json_encode error: %q", result))
            error({ "internal server error", 13 })
        end

        nk.logger_debug(string.format("rpc_can_claim_daily_reward resp: %q", result))
        return result
    end
    ```

### ClaimDailyReward RPC

This RPC will ensure the user is eligible to receive the daily reward, update the user's Wallet, send out a notification and then update the user's daily reward in the Storage Engine.

=== "TypeScript"
    ```typescript
    function rpcClaimDailyReward(context: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
        var response = { coinsReceived: 0 };

        var dailyReward = getLastDailyRewardObject(context, logger, nk, payload);
        if (canUserClaimDailyReward(dailyReward)) {
            response.coinsReceived = 500;

            var changeset = {
                coins: response.coinsReceived,
            }

            try {
                nk.walletUpdate(context.userId, changeset, {}, false);
            } catch (error) {
                logger.error('walletUpdate error: %q', error);
                throw error;
            }

            var notification: nkruntime.NotificationRequest = {
                code: 1001,
                content: changeset,
                persistent: true,
                subject: "You've received your daily reward!",
                userId: context.userId,
            }

            try {
                nk.notificationsSend([notification]);
            } catch (error) {
                logger.error('notificationsSend error: %q', error);
                throw error;
            }

            dailyReward.lastClaimUnix = msecToSec(Date.now());

            var write: nkruntime.StorageWriteRequest = {
                collection: 'reward',
                key: 'daily',
                permissionRead: 1,
                permissionWrite: 0,
                value: dailyReward,
                userId: context.userId,
            }

            if (dailyReward.version) {
                // Use OCC to prevent concurrent writes.
                write.version = dailyReward.version
            }

            // Update daily reward storage object for user.
            try {
                nk.storageWrite([ write ])
            } catch (error) {
                logger.error('storageWrite error: %q', error);
                throw error;
            }
        }

        var result = JSON.stringify(response);
        logger.debug('rpcClaimDailyReward response: %q', result)

        return result;
    }
    ```

=== "Go"
    ```go
    func RpcClaimDailyReward(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
        userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
        if !ok {
            return "", errNoUserIdFound
        }

        var resp struct {
            CoinsReceived int64 `json:"coinsReceived"`
        }
        resp.CoinsReceived = int64(0)

        dailyReward, dailyRewardObject, err := getLastDailyRewardObject(ctx, logger, nk, payload)
        if err != nil {
            logger.Error("Error getting daily reward: %v", err)
            return "", errInternalError
        }

        if canUserClaimDailyReward(dailyReward) {
            resp.CoinsReceived = 500

            // Update player wallet.
            changeset := map[string]int64{
                "coins": resp.CoinsReceived,
            }
            if _, _, err := nk.WalletUpdate(ctx, userID, changeset, map[string]interface{}{}, false); err != nil {
                logger.Error("WalletUpdate error: %v", err)
                return "", errInternalError
            }

            err := nk.NotificationsSend(ctx, []*runtime.NotificationSend{{
                Code: 1001,
                Content: map[string]interface{}{
                    "coins": changeset["coins"],
                },
                Persistent: true,
                Sender:     "", // Server sent.
                Subject:    "You've received your daily reward!",
                UserID:     userID,
            }})
            if err != nil {
                logger.Error("NotificationsSend error: %v", err)
                return "", errInternalError
            }

            dailyReward.LastClaimUnix = time.Now().Unix()

            object, err := json.Marshal(dailyReward)
            if err != nil {
                logger.Error("Marshal error: %v", err)
                return "", errInternalError
            }

            version := ""
            if dailyRewardObject != nil {
                // Use OCC to prevent concurrent writes.
                version = dailyRewardObject.GetVersion()
            }

            // Update daily reward storage object for user.
            _, err = nk.StorageWrite(ctx, []*runtime.StorageWrite{{
                Collection:      "reward",
                Key:             "daily",
                PermissionRead:  1,
                PermissionWrite: 0, // No client write.
                Value:           string(object),
                Version:         version,
                UserID:          userID,
            }})
            if err != nil {
                logger.Error("StorageWrite error: %v", err)
                return "", errInternalError
            }
        }

        out, err := json.Marshal(resp)
        if err != nil {
            logger.Error("Marshal error: %v", err)
            return "", errMarshal
        }

        logger.Debug("rpcClaimDailyReward resp: %v", string(out))
        return string(out), nil
    }
    ```

=== "Lua"
    ```lua
    function M.rpc_claim_daily_reward(context, payload)
        local resp = {
            ["coinsReceived"] = 0
        }

        local daily_reward = get_last_daily_reward_object(context, payload)

        -- If last claimed is before the new day grant a new reward!
        if (can_user_claim_daily_reward(daily_reward)) then
            resp.coinsReceived = 500

            -- Update player wallet.
            local changeset = {
                ["coins"] = resp.coinsReceived
            }
            local success, result = pcall(nk.wallet_update, context.user_id, changeset, {}, false)
            if (not success) then
                nk.logger_error(string.format("wallet_update error: %q", result))
                error({ "internal server error", 13 })
            end

            local notification = {
                code = 1001,
                content = changeset,
                persistent = true,
                sender = "",
                subject = "You've received your daily reward!",
                user_id = context.user_id
            }
            local success, result = pcall(nk.notifications_send, { notification })
            if (not success) then
                nk.logger_error(string.format("notifications_send error: %q", result))
                error({ "internal server error", 13 })
            end

            daily_reward.last_claim_unix = os.time()

            local version = nil
            if (daily_reward.version) then
                -- Use OCC to prevent concurrent writes.
                version = daily_reward.version
            end

            -- Update daily reward storage object for user.
            local write = {
                collection = "reward",
                key = "daily",
                permission_read = 1,
                permission_write = 0,
                value = daily_reward,
                version = version,
                user_id = context.user_id
            }
            local success, result = pcall(nk.storage_write, { write })
            if (not success) then
                nk.logger_error(string.format("storage_write error: %q", result))
                error({ "internal server error", 13 })
            end
        end

        local success, result = pcall(nk.json_encode, resp)
        if (not success) then
            nk.logger_error(string.format("json_encode error: %q", result))
            error({ "internal server error", 13 })
        end

        nk.logger_debug(string.format("rpc_claim_daily_reward resp: %q", result))
        return result
    end
    ```

## Exploring in the Nakama Console

Spin up your server ([using Docker](/install-docker-quickstart)) and test the RPCs using the [Nakama Console](/console-overview).

You can access the Nakama Console by opening a browser and going to [http://localhost:7351](http://localhost:7351).

Once there, you can try interacting with your RPCs via the [API Explorer](http://127.0.0.1:7351/#/apiexplorer) by selecting them from the dropdown and specifying a user ID as a context.

## Wrap Up

With those two RPCs implemented you now have a simple daily reward system, congratulations!

Feel free to experiment further by adding more complicated eligibility criteria or other such features.
