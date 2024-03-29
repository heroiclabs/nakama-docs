# Events

Events are a powerful way to send data to the game server and create data to be processed in the background on the server. You can create and receive data which can be forwarded to 3rd-party services like Analytics, Ads, In-app Purchases, and more.

This has a number of advantages for game studios and developers:

* It reduces the number of client-side SDKs which shrinks the game client; valuable to improve the FTUE and number of players who will download the game,
* Less network traffic is used by game clients to various 3rd party services, and
* It significantly reduces the integration time, and active maintenance cost spent by the development team.

A good use case is to implement game analytics or Liveops by using events emitted in the client or by server-side functions. These events will be processed in the background inside Nakama server. Players have minimal interruption to their gameplay experience and developers can optimize and improve games based on the feedback from the analytics.

Internally this feature is implemented with a high performance circular buffer to store events received and a worker pool of consumers (event handlers) to ensure that large numbers of events received cannot overload the server if the event handlers cannot keep up.

## Generate Events

Events can be sent to the server by game clients or created on the server. An event has a name and a map of properties which decorate the event with more information.

### Send Events

Use the event API to send to the server.

=== "Bash"
	```bash
	curl -vvv -H "Authorization: Bearer $SESSION"  http://127.0.0.1:7350/v2/event -d '{"name": "my_event", "properties": {"my_key": "my_value"}}'
	```

### Create Events

Use the server side module to generate events in your Go code.

=== "Go"
	```go
	// import "github.com/heroiclabs/nakama-common/api"
	// import "github.com/heroiclabs/nakama-common/runtime"

	// ctx context.Context, nk runtime.NakamaModule
	evt := &api.Event{
		Name:       "event_name"
		Properties: map[string]string{
			"my_key": "my_value",
		},
		External:   true,
	}
	if err := nk.Event(ctx, evt); err != nil {
		// Handle error.
	}
	```

=== "Lua"
    ```lua
    local nk = require("nakama")

    local properties = {
        my_key = "my_value"
    }
    local timestamp = nk.time
    local external = false

    nk.event("event_name", properties, timestamp, external)
    ```

=== "TypeScript"
    ```typescript
    let properties = {
        my_key: 'my_value'
    };

    nk.event('event_name', properties);
    ```

You can also take advantage of [after hooks](../server-framework/basics.md#after-hook) in the server runtime when you want to send events from other features in the server. For example when a user account is updated and you might want to send an event to be processed.

=== "Go"
	```go
	func afterUpdateAccount(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.UpdateAccountRequest) error {
		evt := &api.Event{
			Name:       "account_updated",
			Properties: map[string]string{},
			External:   false, // used to denote if the event was generated from the client
		}
		return nk.Event(context.Background(), evt)
	}

	func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
		if err := initializer.RegisterAfterUpdateAccount(afterUpdateAccount); err != nil {
			return err
		}
		return nil
	}
	```

=== "Lua"
	```lua
    local nk = require("nakama")

    local function after_update_account(ctx, payload)
        local properties = {
            my_key = "my_value"
        }
        local timestamp = nk.time
        local external = false  -- used to denote if the event was generated from the client
        return nk.event("event_name", properties, timestamp, external)
    end

    nk.register_req_after(after_update_account, "UpdateAccount")
	```


=== "TypeScript"
    ```typescript
    function InitModule(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
        let afterUpdateAccount: nkruntime.AfterHookFunction<void, nkruntime.UserUpdateAccount> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: void, payload: nkruntime.UserUpdateAccount) {
            let properties = {
                my_key: 'my_value'
            };

            nk.event('event_name', properties);
        }
        initializer.registerAfterUpdateAccount(afterUpdateAccount);
    }
    ```

### Builtin Events

The server will generate builtin events which can be processed specifically for the __SessionStart__ and __SessionEnd__ actions. These special events occur when the server has a new socket session start and when it ends. See below for how to process these events.

## Process Events

Events can be processed with a function registered to the runtime initializer at server startup. Events will have its external field marked as `true` if its been generated by clients.

!!! Note
    Processing events through Lua/TypeScript runtime functions is not yet supported.

=== "Go"
	```go
	func processEvent(ctx context.Context, logger runtime.Logger, evt *api.Event) {
		switch evt.GetName() {
		case "account_updated":
			logger.Debug("process evt: %+v", evt)
			// Send event to an analytics service.
		default:
			logger.Error("unrecognised evt: %+v", evt)
		}
	}

	// initializer runtime.Initializer
	if err := initializer.RegisterEvent(processEvent); err != nil {
		return err
	}
	```

### Builtin Events

Events which are internally generated have their own registration functions. The SessionStart is created when a new socket connection is opened on the server.

=== "Go"
	```go
	func eventSessionStart(ctx context.Context, logger runtime.Logger, evt *api.Event) {
		logger.Debug("process event session start: %+v", evt)
	}

	// initializer runtime.Initializer
	if err := initializer.RegisterEventSessionStart(eventSessionStart); err != nil {
		return err
	}
	```

The SessionEnd event is created when the socket connection for a session is closed. The socket could have closed for a number of reasons but can be observed to react on.

=== "Go"
	```go
	func eventSessionEnd(ctx context.Context, logger runtime.Logger, evt *api.Event) {
		logger.Debug("process event session end: %+v", evt)
	}

	// initializer runtime.Initializer
	if err := initializer.RegisterEventSessionEnd(eventSessionEnd); err != nil {
		return err
	}
	```

## Advanced Settings

To protect the performance of the server the event processing subsystem is designed to limit the resources allocated to process events. You can adjust the resources allocated to this subsystem with these configuration settings.

| Configuration Key | Value | Description |
| ----------------- | ----- | ----------- |
| runtime.event_queue_size | int | Size of the event queue buffer. Defaults to 65536. |
| runtime.event_queue_workers | int | Number of workers to use for concurrent processing of events. Defaults to 8. |

To review other configuration settings have a look at [these](../getting-started/configuration.md) docs.

## Example

This is a complete sample plugin in Go which processes SessionStart, SessionEnd, and events generated when a user account is updated.

=== "Go"
	```go
	package main

	import (
		"context"
		"database/sql"
		"github.com/heroiclabs/nakama-common/api"
		"github.com/heroiclabs/nakama-common/runtime"
	)

	func afterUpdateAccount(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.UpdateAccountRequest) error {
		evt := &api.Event{
			Name:       "account_updated",
			Properties: map[string]string{},
			External:   false,
		}
		return nk.Event(context.Background(), evt)
	}

	func processEvent(ctx context.Context, logger runtime.Logger, evt *api.Event) {
		switch evt.GetName() {
		case "account_updated":
			logger.Debug("process evt: %+v", evt)
			// Send event to an analytics service.
		default:
			logger.Error("unrecognised evt: %+v", evt)
		}
	}

	func eventSessionEnd(ctx context.Context, logger runtime.Logger, evt *api.Event) {
		logger.Debug("process event session end: %+v", evt)
	}

	func eventSessionStart(ctx context.Context, logger runtime.Logger, evt *api.Event) {
		logger.Debug("process event session start: %+v", evt)
	}

	//noinspection GoUnusedExportedFunction
	func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
		if err := initializer.RegisterAfterUpdateAccount(afterUpdateAccount); err != nil {
			return err
		}
		if err := initializer.RegisterEvent(processEvent); err != nil {
			return err
		}
		if err := initializer.RegisterEventSessionEnd(eventSessionEnd); err != nil {
			return err
		}
		if err := initializer.RegisterEventSessionStart(eventSessionStart); err != nil {
			return err
		}
		logger.Info("Server loaded.")
		return nil
	}
	```
