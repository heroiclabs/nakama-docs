# Tournaments

Tournaments are competitions which span a short period where opponents compete over a prize.

This document outlines the design of tournaments for Nakama server. It covers the rules for how players find tournaments, which ones they can join, when they're allowed to submit scores, and how rewards are distributed when the tournament ends.

## Rules

A tournament is created with an optional reset schedule and a duration. These values allow for flexible control over how long a tournament can be played before it is reset for the next duration. For example a tournament could be created that starts at noon each day and be played for one hour. This would be expressed with a CRON expression ("0 12 * * \*") and a duration of 3600 seconds.

The tournament can restrict the number of opponents allowed (i.e. First come first serve) and enforce an optional join requirement. For example each opponent must join before they can submit scores with only the first 10,000 opponents are allowed to join.

Tournaments are created programmatically to start in the future or immediately upon creation and are all expressed as leaderboards with special configuration.

A tournament can also be played by opponents who are not users. For example a guild tournament can be implemented where score submissions are made by guild ID.

## List Tournaments

Find tournaments which have been created on the server. Tournaments can be filtered with categories and via start and end times. This function can also be used to see the tournaments that an owner (usually a user) has joined.

```sh fct_label="cURL"
```

```js fct_label="JavaScript"
```

```csharp fct_label=".NET"
var categoryStart = 1;
var categoryEnd = 2;
var startTime = 1538147711;
var endTime = 0L; // all tournaments from the start time
var limit = 100; // number to list per page
var cursor = null;
var result = await client.ListTournamentsAsync(session, categoryStart, categoryEnd, startTime, endTime, limit, cursor);
```

```csharp fct_label="Unity"
var categoryStart = 1;
var categoryEnd = 2;
var startTime = 1538147711;
var endTime = 0L; // all tournaments from the start time
var limit = 100; // number to list per page
var cursor = null;
var result = await client.ListTournamentsAsync(session, categoryStart, categoryEnd, startTime, endTime, limit, cursor);
```

```java fct_label="Java"
```

```fct_label="REST"
```

## Join Tournament

A tournament may need to be joined before the owner can submit scores. This operation is idempotent and will always succeed for the owner even if they have already joined the tournament.

```sh fct_label="cURL"
```

```js fct_label="JavaScript"
```

```csharp fct_label=".NET"
var id = "someid";
var success = await client.JoinTournamentAsync(session, id);
```

```csharp fct_label="Unity"
var id = "someid";
var success = await client.JoinTournamentAsync(session, id);
```

```java fct_label="Java"
```

```fct_label="REST"
```

## List Tournament Records

Fetch a mixed list of tournament records as well as a batch of records which belong to specific owners. This can be useful to build up a leaderboard view which shows the top 100 players as well as the scores between the current user and their friends.

```sh fct_label="cURL"
```

```js fct_label="JavaScript"
```

```csharp fct_label=".NET"
var id = "someid";
var limit = 100;
var cursor = null;
var result = await client.ListTournamentRecordsAsync(session, id, new []{ session.UserId }, limit, cursor);
```

```csharp fct_label="Unity"
var id = "someid";
var limit = 100;
var cursor = null;
var result = await client.ListTournamentRecordsAsync(session, id, new []{ session.UserId }, limit, cursor);
```

```java fct_label="Java"
```

```fct_label="REST"
```

## List Tournament Records Around Owner

Fetch the list of tournament records around the owner.

```sh fct_label="cURL"
```

```js fct_label="JavaScript"
```

```csharp fct_label=".NET"
var id = "someid";
var ownerId = session.UserId;
var limit = 100;
var result = await client.ListTournamentRecordsAroundOwnerAsync(session, id, ownerId, limit);
```

```csharp fct_label="Unity"
var id = "someid";
var ownerId = session.UserId;
var limit = 100;
var result = await client.ListTournamentRecordsAroundOwnerAsync(session, id, ownerId, limit);
```

```java fct_label="Java"
```

```fct_label="REST"
```

## Write Tournament Record

Submit a score and optional subscore to a tournament leaderboard. If the tournament has been configured with join required this will fail unless the owner has already joined the tournament.

```sh fct_label="cURL"
```

```js fct_label="JavaScript"
```

```csharp fct_label=".NET"
var id = "someid";
var score = 100L;
var subscore = 10L;
// using Nakama.TinyJson
var metadata = new Dictionary<string, string>()
{
  { "weather_conditions", "sunny" },
  { "track_name", "Silverstone" }
}.ToJson();
var newrecord = client.WriteTournamentRecordAsync(session, id, score, subscore, metadata);
```

```csharp fct_label="Unity"
var id = "someid";
var score = 100L;
var subscore = 10L;
// using Nakama.TinyJson
var metadata = new Dictionary<string, string>()
{
  { "weather_conditions", "sunny" },
  { "track_name", "Silverstone" }
}.ToJson();
var newrecord = client.WriteTournamentRecordAsync(session, id, score, subscore, metadata);
```

```java fct_label="Java"
```

```fct_label="REST"
```

## Authoritative Functions

The runtime functions can be accessed from Lua or Go code and enable custom logic to be used to apply additional rules to various aspects of a tournament. For example it may be required that an opponent is higher than a specific level before they're allowed to join the tournament.

All API design examples are written in Go. For brevity imports and some variables are assumed to exist.

### Create Tournament

Create a tournament with all it's configuration options.

```lua fct_label="Lua"
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
local authoritative = false
local sort = "desc" -- one of: "desc", "asc"
local operator = "best" -- one of: "best", "set", "incr"
local reset = "0 12 * * *" -- noon UTC each day
local metadata = {
  weather_conditions = "rain"
}
title = "Daily Dash"
description = "Dash past your opponents for high scores and big rewards!"
category = 1
start_time = 0 -- start now
end_time = 0 -- never end, repeat the tournament each day forever
duration = 3600 -- in seconds
max_size = 10000 -- first 10,000 players who join
max_num_score = 3 -- each player can have 3 attempts to score
join_required = true -- must join to compete
nk.tournament_create(sortOrder, operator, reset, metadata, title, description, category,
  start_time, endTime, duration, max_size, max_num_score, join_required)
```

```go fct_label="Go"
// import "github.com/gofrs/uuid"
id := uuid.Must(uuid.NewV4())
sortOrder := "desc" // one of: "desc", "asc"
operator := "best"  // one of: "best", "set", "incr"
resetSchedule := "0 12 * * *" // noon UTC each day
metadata := map[string]interface{}{}
title := "Daily Dash"
description := "Dash past your opponents for high scores and big rewards!"
category := 1
startTime := nil // start now
endTime := nil // never end, repeat the tournament each day forever
duration := 3600 // in seconds
maxSize := 10000 // first 10,000 players who join
maxNumScore := 3 // each player can have 3 attempts to score
joinRequired := true // must join to compete
err := nk.TournamentCreate(id.String(), sortOrder, operator, resetSchedule, metadata, title, description, category, startTime, endTime, duration, maxSize, maxNumScore, joinRequired)
if err != nil {
  logger.Printf("unable to create tournament: %q", err.Error())
  return "", errors.New("failed to create tournament"), 3
}
```

### Delete Tournament

Delete a tournament by it's ID.

```lua fct_label="Lua"
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
nk.tournament_delete(id)
```

```go fct_label="Go"
err := nk.TournamentDelete(id)
if err != nil {
  logger.Printf("unable to delete tournament: %q", err.Error())
  return "", errors.New("failed to delete tournament"), 3
}
```

### Add Score Attempts

Add additional score attempts to the owner's tournament record. This overrides the max number of score attempts allowed in the tournament for this specific owner.

```lua fct_label="Lua"
local id = "someid"
local owner = "someuserid"
local attempts = 10
nk.tournament_add_attempt(id, owner, attempts)
```

```go fct_label="Go"
id := "someid"
userID := "someuserid"
attempts := 10
err := nk.TournamentAddAttempt(id, userID, attempts)
if err != nil {
  logger.Printf("unable to update user %v record attempts: %q", userID, err.Error())
  return "", errors.New("failed to add tournament attempts"), 3
}
```

## Reward Distribution

When a tournament's active period ends a function registered on the server will be called to pass the expired records for use to calculate and distribute rewards to owners.

To register a reward distribution function in Go use the `initializer`.

```lua fct_label="Lua"
```

```go fct_label="Go"
import (
  "context"
  "database/sql"
  "log"

  "github.com/heroiclabs/nakama/api"
  "github.com/heroiclabs/nakama/runtime"
)

func InitModule(ctx context.Context, logger *log.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) {
  initializer.RegisterTournamentEnd(distributeRewards)
}

func distributeRewards(ctx context.Context, logger *log.Logger, db *sql.DB, nk runtime.NakamaModule, tournament *api.Tournament, end int64, reset int64) {
  // ...
  return nil
}
```

A simple reward distribution function which sends a persistent notification to the top ten players to let them know they've won and adds coins to their virtual wallets would look like:

```lua fct_label="Lua"
```

```go fct_label="Go"
func distributeRewards(ctx context.Context, logger *log.Logger, _, nk runtime.NakamaModule, tournament *api.Tournament, end int64, reset int64) {
  notifications := make([]*runtime.NotificationSend, 0, 10)
  var content = map[string]interface{}{}
  changeset := map[string]interface{}{"coins": 100}
  records, _, _, _, err := nk.LeaderboardRecordsList(tournament.Id, []string{}, 10, nil, reset)
  for _, record := range records {
    wallets = append(wallets, &runtime.WalletUpdate{record.OwnerId, changeset, content})
    notifications = append(notifications, &runtime.NotificationSend{record.OwnerId, "Winner", content, 1, "", true})
  }
  err = nk.WalletsUpdate(wallets)
  if err := nil {
    logger.Printf("failed to update winner wallets: %v", err)
    return err
  }
  err = nk.NotificationsSend(notifications)
  if err := nil {
    logger.Printf("failed to send winner notifications: %v", err)
    return err
  }
  return nil
}
```

## Advanced

Tournaments can be used to implement a league system. The main difference between a league and a tournament is that leagues are usually seasonal and incorporate a ladder or tiered hierarchy that opponents can progress on.

A league can be structured as a collection of tournaments which share the same reset schedule and duration. The reward distribution function can be used to progress opponents between one tournament and the next in between each reset schedule.
