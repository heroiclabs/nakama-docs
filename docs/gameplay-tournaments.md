# Tournaments

Tournaments are competitions which span a short period where opponents compete over a prize.

This document outlines the design of tournaments for Nakama server. It covers the rules for how players find tournaments, which ones they can join, when they're allowed to submit scores, and how rewards are distributed when the tournament ends.

## Rules

A tournament is created with an optional reset schedule and a duration. These values allow for flexible control over how long a tournament can be played before it is reset for the next duration. For example a tournament could be created that starts at noon each day and be played for one hour. This would be expressed with a CRON expression ("0 12 * * \*") and a duration of 3600 seconds.

The tournament can restrict the number of opponents allowed (i.e. First come first serve) and enforce an optional join requirement. For example each opponent must join before they can submit scores with only the first 10,000 opponents are allowed to join.

Tournaments are created programmatically to start in the future or immediately upon creation and are all expressed as leaderboards with special configuration.

A tournament can also be played by opponents who are not users. For example a guild tournament can be implemented where score submissions are made by guild ID.

## List tournaments

Find tournaments which have been created on the server. Tournaments can be filtered with categories and via start and end times. This function can also be used to see the tournaments that an owner (usually a user) has joined.

=== "cURL"
	```sh
	curl -X GET "http://127.0.0.1:7350/v2/tournament?category_start=<category_start>&category_end=<category_end>&start_time=<start_time>&end_time=<end_time>&limit=<limit>&cursor=<cursor>" \
	  -H 'Authorization: Bearer <session token>'
	```

=== "JavaScript"
	```js
	var categoryStart = 1;
	var categoryEnd = 2;
	var startTime = 1538147711;
	var endTime = -1; // all tournaments from the start time
	var limit = 100; // number to list per page
	var cursor = null;
	var result = await client.listTournaments(session, categoryStart, categoryEnd, startTime, endTime, limit, cursor);
	```

=== ".NET"
	```csharp
	var categoryStart = 1;
	var categoryEnd = 2;
	var startTime = 1538147711;
	var endTime = -1L; // all tournaments from the start time
	var limit = 100; // number to list per page
	var cursor = null;
	var result = await client.ListTournamentsAsync(session, categoryStart, categoryEnd, startTime, endTime, limit, cursor);
	```

=== "Unity"
	```csharp
	var categoryStart = 1;
	var categoryEnd = 2;
	var startTime = 1538147711;
	var endTime = -1L; // all tournaments from the start time
	var limit = 100; // number to list per page
	var cursor = null;
	var result = await client.ListTournamentsAsync(session, categoryStart, categoryEnd, startTime, endTime, limit, cursor);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](NTournamentListPtr list)
	{
	  CCLOG("Tournament count %u", list->tournaments.size());

	  for (auto& tournament : list->tournaments)
	  {
	    CCLOG("Tournament ID %s, title %s", tournament.id.c_str(), tournament.title.c_str());
	  }
	};

	uint32_t categoryStart = 1;
	uint32_t categoryEnd = 2;
	uint32_t startTime = 1538147711;
	uint32_t endTime = -1; // all tournaments from the start time
	int32_t limit = 100; // number to list per page

	client->listTournaments(session,
	    categoryStart,
	    categoryEnd,
	    startTime,
	    endTime,
	    limit,
	    opt::nullopt,
	    successCallback
	    );
	```

=== "Cocos2d-x JS"
	```js
	var categoryStart = 1;
	var categoryEnd = 2;
	var startTime = 1538147711;
	var endTime = -1; // all tournaments from the start time
	var limit = 100; // number to list per page
	var cursor = null;
	client.listTournaments(session, categoryStart, categoryEnd, startTime, endTime, limit, cursor)
	  .then(function(result) {
	    },
	    function(error) {
	      cc.error("list tournaments failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](NTournamentListPtr list)
	{
	  std::cout << "Tournament count " << list->tournaments.size() << std::endl;

	  for (auto& tournament : list->tournaments)
	  {
	    std::cout << "Tournament ID " << tournament.id << ", title " << tournament.title << std::endl);
	  }
	};

	uint32_t categoryStart = 1;
	uint32_t categoryEnd = 2;
	uint32_t startTime = 1538147711;
	uint32_t endTime = -1; // all tournaments from the start time
	int32_t limit = 100; // number to list per page

	client->listTournaments(session,
	    categoryStart,
	    categoryEnd,
	    startTime,
	    endTime,
	    limit,
	    opt::nullopt,
	    successCallback
	    );
	```

=== "Java"
	```java
	int categoryStart = 1;
	int categoryEnd = 2;
	int startTime = 1538147711;
	int endTime = -1; // all tournaments from the start time
	int limit = 100; // number to list per page
	String cursor = null;
	TournamentList tournaments = client.listTournaments(session, categoryStart, categoryEnd, startTime, endTime, limit, cursor).get();
	```

=== "Swift"
	```swift
	// Will be made available soon.
	```

=== "Godot"
	```gdscript
	var category_start = 1
	var category_end = 2
	var start_time = 1538147711
	var end_time = -1 # all tournaments from the start time
	var limit = 100 # number to list per page
	var cursor = null
	var result : NakamaAPI.ApiTournamentList = yield(client.list_tournaments_async(session, category_start, category_end, start_time, end_time, limit, cursor), "completed")
	if result.is_exception():
		print("An error occured: %s" % result)
		return
	print("Tournaments: %s" % [result])
	```

=== "REST"
    ```
	GET /v2/tournament
	  ?category_start=<category_start>
	  &category_end=<category_end>
	  &start_time=<start_time>
	  &end_time=<end_time>
	  &limit=<limit>
	  &cursor=<cursor>
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>
	```

## Join tournament

A tournament may need to be joined before the owner can submit scores. This operation is idempotent and will always succeed for the owner even if they have already joined the tournament.

=== "cURL"
	```sh
	curl -X POST "http://127.0.0.1:7350/v2/tournament/<tournament_id>/join" \
	  -H 'Authorization: Bearer <session token>'
	```

=== "JavaScript"
	```js
	var id = "someid";
	var success = await client.joinTournament(session, id);
	```

=== ".NET"
	```csharp
	var id = "someid";
	var success = await client.JoinTournamentAsync(session, id);
	```

=== "Unity"
	```csharp
	var id = "someid";
	var success = await client.JoinTournamentAsync(session, id);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = []()
	{
	  CCLOG("Successfully joined tournament");
	};

	string id = "someid";
	client->joinTournament(session, id, successCallback);
	```

=== "Cocos2d-x JS"
	```js
	var id = "someid";
	client.joinTournament(session, id)
	  .then(function() {
	      cc.log("Successfully joined tournament");
	    },
	    function(error) {
	      cc.error("Join tournament failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = []()
	{
	  std::cout << "Successfully joined tournament" << std::cout;
	};

	string id = "someid";
	client->joinTournament(session, id, successCallback);
	```

=== "Java"
	```java
	String id = "someid";
	client.joinTournament(session, id).get();
	```

=== "Swift"
	```swift
	// Will be made available soon.
	```

=== "Godot"
	```gdscript
	var id = "someid"
	var success : NakamaAsyncResult = yield(client.join_tournament_async(session, id), "completed")
	if success.is_exception():
		print("An error occured: %s" % success)
		return
	print("Joined tournament")
	```

=== "REST"
    ```
	POST /v2/tournament/<tournament_id>/join
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>
	```

## List tournament records

Fetch a mixed list of tournament records as well as a batch of records which belong to specific owners. This can be useful to build up a leaderboard view which shows the top 100 players as well as the scores between the current user and their friends.

=== "cURL"
	```sh
	curl -X GET "http://127.0.0.1:7350/v2/tournament/<tournament_id>?owner_ids=<owner_ids>&limit=<limit>&cursor=<cursor>" \
	  -H 'Authorization: Bearer <session token>'
	```

=== "JavaScript"
	```js
	var id = "someid";
	var ownerIds = ["some", "friends", "user ids"];
	var result = await client.listTournamentRecords(session, id, owenrIds);
	result.records.forEach(function(record) {
	  console.log("Record username %o and score %o", record.username, record.score);
	});
	```

=== ".NET"
	```csharp
	var id = "someid";
	var limit = 100;
	var cursor = null;
	var result = await client.ListTournamentRecordsAsync(session, id, new []{ session.UserId }, limit, cursor);
	```

=== "Unity"
	```csharp
	var id = "someid";
	var limit = 100;
	var cursor = null;
	var result = await client.ListTournamentRecordsAsync(session, id, new []{ session.UserId }, limit, cursor);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](NTournamentRecordListPtr list)
	{
	  for (auto& record : list->records)
	  {
	    CCLOG("Record username %s and score %ld", record.username.c_str(), record.score);
	  }
	};

	string id = "someid";
	client->listTournamentRecords(session, id, opt::nullopt, opt::nullopt, {}, successCallback);
	```

=== "Cocos2d-x JS"
	```js
	var id = "someid";
	var ownerIds = ["some", "friends", "user ids"];
	client.listTournamentRecords(session, id, owenrIds)
	  .then(function(result) {
	      result.records.forEach(function(record) {
	        cc.log("Record username", record.username, "and score", record.score);
	      });
	    },
	    function(error) {
	      cc.error("list tournament records failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](NTournamentRecordListPtr list)
	{
	  for (auto& record : list->records)
	  {
	    std::cout << "Record username " << record.username << " and score " << record.score << std::endl;
	  }
	};

	string id = "someid";
	client->listTournamentRecords(session, id, opt::nullopt, opt::nullopt, {}, successCallback);
	```

=== "Java"
	```java
	String id = "someid";
	LeaderboardRecordList records = client.listLeaderboardRecords(session, id, session.getUserId()).get();
	```

=== "Swift"
	```swift
	// Will be made available soon.
	```

=== "Godot"
	```gdscript
	var id = "someid"
	var limit = 100
	var cursor = null
	var result : NakamaAPI.ApiTournamentRecordList = yield(client.list_tournament_records_async(session, id, [session.user_id], limit, cursor), "completed")
	if result.is_exception():
		print("An error occured: %s" % result)
		return
	print("Records: %s" % [result])
	```

=== "REST"
    ```
	GET /v2/tournament/<tournament_id>?owner_ids=<owner_ids>&limit=<limit>&cursor=<cursor>
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>
	```

## List tournament records around owner

Fetch the list of tournament records around the owner.

=== "cURL"
	```sh
	curl -X GET "http://127.0.0.1:7350/v2/tournament/<tournament_id>/owner/<owner_id>?limit=<limit>" \
	  -H 'Authorization: Bearer <session token>'
	```

=== "JavaScript"
	```js
	var id = "someid";
	var ownerId = "some user ID";
	var limit = 100;
	var result = await client.listTournamentRecordsAroundOwner(session, id, ownerId, limit);
	```

=== ".NET"
	```csharp
	var id = "someid";
	var ownerId = session.UserId;
	var limit = 100;
	var result = await client.ListTournamentRecordsAroundOwnerAsync(session, id, ownerId, limit);
	```

=== "Unity"
	```csharp
	var id = "someid";
	var ownerId = session.UserId;
	var limit = 100;
	var result = await client.ListTournamentRecordsAroundOwnerAsync(session, id, ownerId, limit);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](NTournamentRecordListPtr list)
	{
	  for (auto& record : list->records)
	  {
	    CCLOG("Record username %s and score %ld", record.username.c_str(), record.score);
	  }
	};

	string id = "someid";
	string ownerId = session->getUserId();
	int32_t limit = 100;
	client->listTournamentRecordsAroundOwner(session, id, ownerId, limit, successCallback);
	```

=== "Cocos2d-x JS"
	```js
	var id = "someid";
	var ownerIds = ["some", "friends", "user ids"];
	client.listTournamentRecords(session, id, owenrIds)
	  .then(function(result) {
	      result.records.forEach(function(record) {
	        cc.log("Record username", record.username, "and score", record.score);
	      });
	    },
	    function(error) {
	      cc.error("list tournament records failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](NTournamentRecordListPtr list)
	{
	  for (auto& record : list->records)
	  {
	    std::cout << "Record username " << record.username << " and score " << record.score << std::endl;
	  }
	};

	string id = "someid";
	string ownerId = session->getUserId();
	int32_t limit = 100;
	client->listTournamentRecordsAroundOwner(session, id, ownerId, limit, successCallback);
	```

=== "Java"
	```java
	String id = "someid";
	String ownerId = session.getUserId();
	int expiry = -1;
	int limit = 100;
	TournamentRecordList records = client.listTournamentRecordsAroundOwner(session, id, ownerId, expiry, limit).get();
	```

=== "Swift"
	```swift
	// Will be made available soon.
	```

=== "Godot"
	```gdscript
	var id = "someid"
	var owner_id = "some user ID"
	var limit = 100
	var result : NakamaAPI.ApiTournamentRecordList = yield(client.list_tournament_records_around_owner_async(session, id, owner_id, limit), "completed")
	if result.is_exception():
		print("An error occured: %s" % result)
		return
	print("Records: %s" % [result])
	```

=== "REST"
    ```
	GET /v2/tournament/v2/tournament/<tournament_id>/owner/<owner_id>?limit=<limit>
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>
	```

## Write tournament record

Submit a score and optional subscore to a tournament leaderboard. If the tournament has been configured with join required this will fail unless the owner has already joined the tournament.

=== "cURL"
	```sh
	curl -X GET "http://127.0.0.1:7350/v2/tournament/<tournament_id>" \
	  -H 'Authorization: Bearer <session token>'
	```

=== "JavaScript"
	```js
	var id = "someid";
	var score = 100;
	var subscore = 10;
	var metadata = {
	  "weather_conditions": "sunny",
	  "track_name": "Silverstone"
	}
	var newrecord = client.writeTournamentRecord(session, id, score, subscore, metadata);
	```

=== ".NET"
	```csharp
	var id = "someid";
	var score = 100L;
	var subscore = 10L;
	// using Nakama.TinyJson;
	var metadata = new Dictionary<string, string>()
	{
	  { "weather_conditions", "sunny" },
	  { "track_name", "Silverstone" }
	}.ToJson();
	var newRecord = await client.WriteTournamentRecordAsync(session, id, score, subscore, metadata);
	Console.WriteLine(newRecord);
	```

=== "Unity"
	```csharp
	var id = "someid";
	var score = 100L;
	var subscore = 10L;
	// using Nakama.TinyJson;
	var metadata = new Dictionary<string, string>()
	{
	  { "weather_conditions", "sunny" },
	  { "track_name", "Silverstone" }
	}.ToJson();
	var newRecord = await client.WriteTournamentRecordAsync(session, id, score, subscore, metadata);
	Debug.Log(newRecord);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [this](const NLeaderboardRecord& record)
	{
	  CCLOG("written tournament record");
	};

	string id = "someid";
	int64_t score = 100;
	int64_t subscore = 10;
	string metadata = "{\"weather_conditions\": \"sunny\", \"track_name\" : \"Silverstone\" }";
	client->writeTournamentRecord(session, id, score, subscore, metadata, successCallback);
	```

=== "Cocos2d-x JS"
	```js
	var id = "someid";
	var score = 100;
	var subscore = 10;
	var metadata = {
	  "weather_conditions": "sunny",
	  "track_name": "Silverstone"
	}
	client.writeTournamentRecord(session, id, score, subscore, metadata)
	  .then(function(newrecord) {
	      cc.log("written tournament record:", JSON.stringify(newrecord));
	    },
	    function(error) {
	      cc.error("write tournament record failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [this](const NLeaderboardRecord& record)
	{
	  std::cout << "written tournament record" << std::endl;
	};

	string id = "someid";
	int64_t score = 100;
	int64_t subscore = 10;
	string metadata = "{\"weather_conditions\": \"sunny\", \"track_name\" : \"Silverstone\" }";
	client->writeTournamentRecord(session, id, score, subscore, metadata, successCallback);
	```

=== "Java"
	```java
	string id = "someid";
	int score = 10;
	int subscore = 20;
	final String metadata = "{\"tarmac\": \"wet\"}";
	LeaderboardRecord record = client.writeTournamentRecord(session, id, score, subscore, metadata).get();
	```

=== "Swift"
	```swift
	// Will be made available soon.
	```

=== "Godot"
	```gdscript
	var id = "someid"
	var score = 100
	var subscore = 10
	var metadata = JSON.print({
		"weather_conditions": "sunny",
		"track_name": "Silverstone"
	})
	var new_record : NakamaAPI.ApiLeaderboardRecord = yield(client.write_tournament_record_async(session, id, score, subscore, metadata), "completed")
	if new_record.is_exception():
		print("An error occured: %s" % new_record)
		return
	print("Record: %s" % [new_record])
	```

=== "REST"
    ```
	GET /v2/tournament/v2/tournament/<tournament_id>
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>
	```

## Authoritative functions

The runtime functions can be accessed from Lua or Go code and enable custom logic to be used to apply additional rules to various aspects of a tournament. For example it may be required that an opponent is higher than a specific level before they're allowed to join the tournament.

All API design examples are written in Go. For brevity imports and some variables are assumed to exist.

### Create tournament

Create a tournament with all it's configuration options.

=== "Lua"
	```lua
	local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
	local authoritative = false
	local sort = "desc"     -- one of: "desc", "asc"
	local operator = "best" -- one of: "best", "set", "incr"
	local reset = "0 12 * * *" -- noon UTC each day
	local metadata = {
	  weather_conditions = "rain"
	}
	title = "Daily Dash"
	description = "Dash past your opponents for high scores and big rewards!"
	category = 1
	start_time = nk.time() / 1000 -- starts now in seconds
	end_time = 0                  -- never end, repeat the tournament each day forever
	duration = 3600               -- in seconds
	max_size = 10000              -- first 10,000 players who join
	max_num_score = 3             -- each player can have 3 attempts to score
	join_required = true          -- must join to compete
	nk.tournament_create(id, sort, operator, duration, reset, metadata, title, description, category,
	    start_time, endTime, max_size, max_num_score, join_required)
	```

=== "Go"
	```go
	// import "github.com/gofrs/uuid"
	id := uuid.Must(uuid.NewV4())
	sortOrder := "desc"  // one of: "desc", "asc"
	operator := "best"   // one of: "best", "set", "incr"
	resetSchedule := "0 12 * * *" // noon UTC each day
	metadata := map[string]interface{}{}
	title := "Daily Dash"
	description := "Dash past your opponents for high scores and big rewards!"
	category := 1
	startTime := time.Now().UTC().Unix() // start now
	endTime := 0                         // never end, repeat the tournament each day forever
	duration := 3600                     // in seconds
	maxSize := 10000                     // first 10,000 players who join
	maxNumScore := 3                     // each player can have 3 attempts to score
	joinRequired := true                 // must join to compete
	err := nk.TournamentCreate(id.String(), sortOrder, operator, resetSchedule, metadata, title,
	    description, category, startTime, endTime, duration, maxSize, maxNumScore, joinRequired)
	if err != nil {
	  logger.Printf("unable to create tournament: %q", err.Error())
	  return "", runtime.NewError("failed to create tournament", 3)
	}
	```

### Delete tournament

Delete a tournament by it's ID.

=== "Lua"
	```lua
	local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
	nk.tournament_delete(id)
	```

=== "Go"
	```go
	err := nk.TournamentDelete(id)
	if err != nil {
	  logger.Printf("unable to delete tournament: %q", err.Error())
	  return "", runtime.NewError("failed to delete tournament", 3)
	}
	```

### Add score attempts

Add additional score attempts to the owner's tournament record. This overrides the max number of score attempts allowed in the tournament for this specific owner.

=== "Lua"
	```lua
	local id = "someid"
	local owner = "someuserid"
	local attempts = 10
	nk.tournament_add_attempt(id, owner, attempts)
	```

=== "Go"
	```go
	id := "someid"
	userID := "someuserid"
	attempts := 10
	err := nk.TournamentAddAttempt(id, userID, attempts)
	if err != nil {
	  logger.Printf("unable to update user %v record attempts: %q", userID, err.Error())
	  return "", runtime.NewError("failed to add tournament attempts", 3)
	}
	```

## Reward distribution

When a tournament's active period ends a function registered on the server will be called to pass the expired records for use to calculate and distribute rewards to owners.

To register a reward distribution function in Go use the `initializer`.

=== "Lua"
	```lua
	local nk = require("nakama")
	local function distribute_rewards(_context, tournament, session_end, expiry)
	  // ...
	end
	nk.register_tournament_end(distribute_rewards)
	```

=== "Go"
	```go
	import (
	  "context"
	  "database/sql"
	  "log"

	  "github.com/heroiclabs/nakama/api"
	  "github.com/heroiclabs/nakama/runtime"
	)

	func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
	  if err := initializer.RegisterTournamentEnd(distributeRewards); err != nil {
	    return err
	  }
	}

	func distributeRewards(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, tournament *api.Tournament, end int64, reset int64) error {
	  // ...
	  return nil
	}
	```

A simple reward distribution function which sends a persistent notification to the top ten players to let them know they've won and adds coins to their virtual wallets would look like:

=== "Lua"
	```lua
	local nk = require("nakama")
	local function distribute_rewards(_context, tournament, session_end, expiry)
	  local notifications = {}
	  local wallet_updates = {}
	  local records, owner_records, nc, pc = nk.leaderboard_records_list(tournament.id, nil, 10, nil, expiry)
	  for i = 0, #records do
	    notifications[i] = {
	      code = 1,
	      content = { coins = 100 },
	      persistent = true,
	      subject = "Winner",
	      user_id = records[i].owner_id
	    }
	    wallet_updates[i] = {
	      user_id = records[i].owner_id,
	      changeset = { coins = 100 },
	      metadata = {}
	    }
	  end

	  nk.wallets_update(wallet_updates, true)
	  nk.notifications_send(notifications)
	end
	```

=== "Go"
	```go
	func distributeRewards(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, tournament *api.Tournament, end int64, reset int64) error {
	  wallets := make([]*runtime.WalletUpdate, 0, 10)
	  notifications := make([]*runtime.NotificationSend, 0, 10)
	  content := map[string]interface{}{}
	  changeset := map[string]interface{}{"coins": 100}
	  records, _, _, _, err := nk.LeaderboardRecordsList(tournament.Id, []string{}, 10, nil, reset)
	  for _, record := range records {
	    wallets = append(wallets, &runtime.WalletUpdate{record.OwnerId, changeset, content})
	    notifications = append(notifications, &runtime.NotificationSend{record.OwnerId, "Winner", content, 1, "", true})
	  }
	  err = nk.WalletsUpdate(wallets, false)
	  if err := nil {
	    logger.Error("failed to update winner wallets: %v", err)
	    return err
	  }
	  err = nk.NotificationsSend(notifications)
	  if err := nil {
	    logger.Error("failed to send winner notifications: %v", err)
	    return err
	  }
	  return nil
	}
	```

## Advanced

Tournaments can be used to implement a league system. The main difference between a league and a tournament is that leagues are usually seasonal and incorporate a ladder or tiered hierarchy that opponents can progress on.

A league can be structured as a collection of tournaments which share the same reset schedule and duration. The reward distribution function can be used to progress opponents between one tournament and the next in between each reset schedule.
