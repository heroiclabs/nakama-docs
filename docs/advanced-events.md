# Events

Events API is available to use as an internal message queue, decoupling __event producers__ from __event handlers__. It is widely employed for making blocking server-side requests in such a way that the user does not notice an increase in latency.

One good example is implementing __analytics__ by having __events__ emitted by functions that execute game logic, and processing those __events__ in a separate thread by issuing network requests to a 3rd party server (such as Segment or Google Analytics). This way a user gets their response from the nakama-based game immediately, and a potentially slow external request is made in the background.

## Event Handlers

!!! Note
    Events API __requires__ that a server-based Event Handler is implemented in __Go__.

Event Handlers are supported for events that are issued by either a client (these are called __external__) or the server itself (__internal__). Register a generic handler as

```go tab="Go"
initializer.RegisterEvent(func(ctx context.Context, logger runtime.Logger, event *api.Event) {})
```

during module initialization.

Two additional handlers may be specified for processing __Session__ events

```go tab="Go"
initializer.RegisterEventSessionStart(func(ctx context.Context, logger runtime.Logger, event *api.Event) {})
initializer.RegisterEventSessionEnd(func(ctx context.Context, logger runtime.Logger, event *api.Event) {})
```

these will only work on two types of events: `session_start` and `session_end`, and are useful for tracking users connecting to and disconnecting from the game server.

## Event producers

!!! Note
    At the moment of writing, server-side events may only be produced in a __Go__ runtime.

Events can be created:

- in __go__ plugins using

  ```go tab="Go"
  err = nk.Event(ctx, &api.Event{
    Name:           "event_name",
    Timestamp:      timestamp,
    Properties:     map[string]string {
      "payload":  payload,
    },
    External:       false,
  })
  ```

- by issuing HTTP requests to the `/v2/event` API endpoint

  ```bash tab="curl"
  curl -vvv -H "Authorization: Bearer $TOKEN"  https://localhost:7350/v2/event -d '{"name": "my_event", "properties": {"my_key": "my_value"}}'
  ```

- and with various compatible nakama clients - please refer to clients documentation.

## Configuration

Internally, the __event queue__ is a circular buffer; if __event handlers__ cannot cope with the rate of incoming __events__ then older records will be overwritten to maintain quality of service.

The __event queue__ has two configurable options

- Size of the event queue buffer `-runtime.event_queue_size int`. Defaults to 65536.

- Number of workers to use for concurrent processing of events. `-runtime.event_queue_workers int`. Defaults to 8.


## Example

This is a complete sample plugin in __Go__ that registers an [RPC function](/runtime-code-basics) to use as a trigger for issuing an __event__ and defines a __handler__ for it, along with two (unused) handlers for following connection open and close events.

!!! Note
    All of the functions below return errors (handling omitted for brevity here), please make sure you process them in your code!

```go tab="Go"
package main

import (
	"context"
	"database/sql"
	"time"

	"github.com/golang/protobuf/ptypes"

	"github.com/heroiclabs/nakama-common/api"
	"github.com/heroiclabs/nakama-common/runtime"
)

func trigger(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	logger.Info("events: processing trigger")

	timestamp, _ := ptypes.TimestampProto(time.Now())

	_ = nk.Event(ctx, &api.Event{
		Name:		"RPC_triggered_event",
		Timestamp:	timestamp,
		Properties:	map[string]string {
			"payload":  payload,
			"property": "value",
		},
		External:	false,
	})

	return "triggered an event", nil
}

func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
	logger.Info("events: the plugin loaded")

	_ = initializer.RegisterRpc("trigger_event", trigger)

	_ = initializer.RegisterEvent(func(ctx context.Context, logger runtime.Logger, event *api.Event) {
		// Having a switch-case in the event handling function is a common pattern for processing events of multiple kinds in the same function.
		switch event.GetName() {
		case "RPC_triggered_event":
			event_type := "internal"
			if event.GetExternal() {
				event_type = "external"
			}

			for k, v := range event.GetProperties() {
				logger.Info("events: %s event %s has property %s = %s", event_type, event.GetName(), k, v)
			}
		default:
			logger.Warn("events: %s processing is not implemented", event.GetName())
		}
	})

	_ = initializer.RegisterEventSessionStart(func(ctx context.Context, logger runtime.Logger, event *api.Event) {
		logger.Info("event: received a session_start event %+v", event)
	})

	_ = initializer.RegisterEventSessionEnd(func(ctx context.Context, logger runtime.Logger, event *api.Event) {
		logger.Info("event: received a session_close event %+v", event)
	})

	return nil
}
```
