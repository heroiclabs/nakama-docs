# Leaderboards

Leaderboards are a great way to add a social and competitive element to any game. They're a fun way to drive competition among your players. The server supports an __unlimited__ number of individual leaderboards with each one as a scoreboard which tracks separate records.

The server has no special requirement on what the score value should represent from your game. A leaderboard is created with a sort order on values. If you're using lap time or currency in records you'll want to order the results in ASC or DESC mode as preferred. At creation time you must also specify the operator which controls how scores are submitted to the leaderboard: "best", "set", or "incr".

!!! Tip
    You can use a leaderboard to track any score you like. Some good examples are: highest points, longest survival time, fastest lap time, quickest level completion, and anything else which can be competed over!

Leaderboards are dynamic in the server because they don't need to be preconfigured like would be needed if you've used Google Play Games or Apple Game Center in the past. A leaderboard can be created via server-side code.

## Leaderboard object

Each leaderboard is a collection of records where each record is a ranked score with metadata. A leaderboard is uniquely identified by an ID.

Leaderboard records are sorted based on their configured sort order: DESC (default) or ASC. The sort order is decided when a leaderboard is created and cannot be changed later.

The leaderboard operator controls how scores are submitted to the leaderboard: "best" always keeps the best score for each user, "set" uses the latest submitted score, and "incr" will add the new value to any existing score. The operator cannot be changed after the leaderboard is created.

All leaderboard configuration is immutable once created. You should delete the leaderboard and create a new one if you need to change the sort order or operator.

### Reset schedules

You can assign each leaderboard an optional reset schedule. Records contained in the leaderboard will expire based on this schedule and users will be able to submit new scores for each reset cycle.

Reset schedules are defined in <a href="https://en.wikipedia.org/wiki/Cron" target="\_blank">CRON format</a> when the leaderboard is created. If a leaderboard has no reset schedule set its records will never expire.

### Leaderboard records

Each leaderboard contains a list of records ordered by their scores.

All records belong to an owner. This is usually a user but other objects like a group ID or some other custom ID can be used. Each owner will only have one record per leaderboard. If a leaderboard expires each owner will be able to submit a new score which rolls over.

The score in each record can be updated as the owner progresses. Scores can be updated as often as wanted and can increase or decrease depending on the combination of leaderboard sort order and operator.

#### Custom fields

Each record can optionally include additional data about the score or the owner when submitted. The extra fields must be JSON encoded and submitted as the metadata. A good use case for metadata is info about race conditions in a driving game, such as weather, which can give extra UI hints when users list scores.

=== "cURL"
	```sh
	curl -X POST "http://127.0.0.1:7350/v2/leaderboard/<leaderboardId>" \
	  -H 'Authorization: Bearer <session token>'
	  -d '{"record": {"score": 100}}'
	```

=== "Javascript"
	```js
	var leaderboardId = "level1";
	var submission = {score: 100};
	var record = await client.writeLeaderboardRecord(session, leaderboardId, submission);
	console.log("New record username %o and score %o", record.username, record.score);
	```

=== ".NET"
	```csharp
	const string leaderboardId = "level1";
	const long score = 100L;
	var r = await client.WriteLeaderboardRecordAsync(session, leaderboardId, score);
	System.Console.WriteLine("New record for '{0}' score '{1}'", r.Username, r.Score);
	```

=== "Unity"
	```csharp
	const string leaderboardId = "level1";
	const long score = 100L;
	var r = await client.WriteLeaderboardRecordAsync(session, leaderboardId, score);
	Debug.LogFormat("New record for '{0}' score '{1}'", r.Username, r.Score);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](const NLeaderboardRecord& record)
	{
	  CCLOG("New record with score %ld", record.score);
	};

	string leaderboardId = "level1";
	int64_t score = 100;

	client->writeLeaderboardRecord(session,
	  leaderboardId,
	  score,
	  opt::nullopt,
	  opt::nullopt,
	  successCallback
	);
	```

=== "Cocos2d-x JS"
	```js
	var leaderboardId = "level1";
	var submission = {score: 100};
	client.writeLeaderboardRecord(session, leaderboardId, submission)
	  .then(function(record) {
	      cc.log("New record with score", record.score);
	    },
	    function(error) {
	      cc.error("write leaderboard record failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](const NLeaderboardRecord& record)
	{
	  std::cout << "New record with score " << record.score << std::endl;
	};

	string leaderboardId = "level1";
	int64_t score = 100;

	client->writeLeaderboardRecord(session,
	  leaderboardId,
	  score,
	  opt::nullopt,
	  opt::nullopt,
	  successCallback
	);
	```

=== "Java"
	```java
	final String leaderboard = "level1";
	long score = 100L;
	LeaderboardRecord r = client.writeLeaderboardRecord(session, leaderboard, score);
	System.out.format("New record for %s score %s", r.getUsername(), r.getScore());
	```

=== "Swift"
	```swift
	// Requires Nakama 1.x
	let id = leaderboard.id //a Leaderboard ID.
	let score = 1200
	let metadata = "{\"race_conditions\": [\"sunny\", \"clear\"]}".data(using: .utf8)!

	var recordWrite : LeaderboardRecordWrite(leaderboardID: id)
	recordWrite.set = score
	recordWrite.metadata = metadata

	var message = LeaderboardRecordWriteMessage()
	message.leaderboardRecords.append(recordWrite)
	client.send(message: message).then { records in
	  for record in records {
	    NSLog("Record handle %@ and score %d.", record.handle, record.score)
	  }
	}.catch { err in
	  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
	}
	```

=== "Godot"
	```gdscript
	var leaderboard_id = "level1"
	var score = 100
	var record : NakamaAPI.ApiLeaderboardRecord = yield(client.write_leaderboard_record_async(session, leaderboard_id, score), "completed")
	if record.is_exception():
		print("An error occured: %s" % record)
		return
	print("New record username %s and score %s" % [record.username, record.score])
	```

=== "REST"
    ```
	POST /v2/leaderboard/<leaderboardId>
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>

	{
	  "record": {
	    "score": 100
	  }
	}
	```

## Create a leaderboard

A leaderboard can be created via server-side code at startup or within a [registered function](runtime-code-function-reference.md#register-hooks). The ID given to the leaderboard is used to submit scores to it.

=== "Lua"
	```lua
	local id = "level1"
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
	id := "level1"
	authoritative := false
	sort := "desc"
	operator := "best"
	reset := "0 0 * * 1"
	metadata := map[string]interface{}{"weather_conditions": "rain"}

	if err := nk.LeaderboardCreate(ctx, id, authoritative, sort, operator, reset, metadata); err != nil {
	  // Handle error.
	}
	```

## Submit a score

A user can submit a score to a leaderboard and update it at any time. When a score is submitted the leaderboard's pre-configured sort order and operator determine what effect the operation will have.

The "set" operator will ensure the leaderboard record always keeps the latest value, even if it is worse than the previous one.

Submitting to a leaderboard with the "best" operator ensures the record tracks the best value it has seen for that record. For a descending leaderboard this means the highest value, for an ascending one the lowest. If there is no previous value for the record, this behaves like "set".

With the "incr" operator the new value is added to any existing score for that record. If there is no previous value for the record, this behaves like "set".

=== "cURL"
	```sh
	curl -X POST "http://127.0.0.1:7350/v2/leaderboard/<leaderboardId>" \
	  -H 'Authorization: Bearer <session token>'
	  -d '{"score": 100}'
	```

=== "Javascript"
	```js
	var leaderboardId = "level1";
	var submission = {score: 100};
	var record = await client.writeLeaderboardRecord(session, leaderboardId, submission);
	console.log("New record username %o and score %o", record.username, record.score);
	```

=== ".NET"
	```csharp
	const string leaderboard = "level1";
	const long score = 100L;
	var r = await client.WriteLeaderboardRecordAsync(session, leaderboard, score);
	System.Console.WriteLine("New record for '{0}' score '{1}'", r.Username, r.Score);
	```

=== "Unity"
	```csharp
	const string leaderboard = "level1";
	const long score = 100L;
	var r = await client.WriteLeaderboardRecordAsync(session, leaderboard, score);
	Debug.LogFormat("New record for '{0}' score '{1}'", r.Username, r.Score);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](const NLeaderboardRecord& record)
	{
	  CCLOG("New record with score %ld", record.score);
	};

	string leaderboardId = "level1";
	int64_t score = 100;

	client->writeLeaderboardRecord(session,
	  leaderboardId,
	  score,
	  opt::nullopt,
	  opt::nullopt,
	  successCallback
	);
	```

=== "Cocos2d-x JS"
	```js
	var leaderboardId = "level1";
	var submission = {score: 100};
	client.writeLeaderboardRecord(session, leaderboardId, submission)
	  .then(function(record) {
	      cc.log("New record with score", record.score);
	    },
	    function(error) {
	      cc.error("write leaderboard record failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](const NLeaderboardRecord& record)
	{
	  std::cout << "New record with score " << record.score << std::endl;
	};

	string leaderboardId = "level1";
	int64_t score = 100;

	client->writeLeaderboardRecord(session,
	  leaderboardId,
	  score,
	  opt::nullopt,
	  opt::nullopt,
	  successCallback
	);
	```

=== "Java"
	```java
	final String leaderboard = "level1";
	long score = 100L;
	LeaderboardRecord r = client.writeLeaderboardRecord(session, leaderboard, score);
	System.out.format("New record for %s score %d", r.getUsername(), r.getScore());
	```

=== "Swift"
	```swift
	// Requires Nakama 1.x
	let id = leaderboard.id //a Leaderboard ID.
	let score = 1200

	var recordWrite : LeaderboardRecordWrite(leaderboardID: id)
	recordWrite.best = score

	var message = LeaderboardRecordWriteMessage()
	message.leaderboardRecords.append(recordWrite)
	client.send(message: message).then { records in
	  for record in records {
	    NSLog("Record handle %@ and score %d.", record.handle, record.score)
	  }
	}.catch { err in
	  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
	}
	```

=== "Godot"
	```gdscript
	var leaderboard_id = "level1"
	var score = 100
	var record : NakamaAPI.ApiLeaderboardRecord = yield(client.write_leaderboard_record_async(session, leaderboard_id, score), "completed")
	if record.is_exception():
		print("An error occured: %s" % record)
		return
	print("New record username %s and score %s" % [record.username, record.score])
	```

=== "REST"
    ```
	POST /v2/leaderboard/<leaderboardId>
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>

	{
	  "score": 100
	}
	```

## List records

A user can list records from a leaderboard. This makes it easy to compare scores to other users and see their positions.

### List by score

The standard way to list records is ordered by score based on the sort order in the leaderboard.

=== "cURL"
	```sh
	curl -X GET "http://127.0.0.1:7350/v2/leaderboard/<leaderboardId>" \
	  -H 'Authorization: Bearer <session token>'
	```

=== "Javascript"
	```js
	var leaderboardId = "level1";
	var result = await client.listLeaderboardRecords(session, leaderboardId);
	result.records.forEach(function(record) {
	  console.log("Record username %o and score %o", record.username, record.score);
	});
	```

=== ".NET"
	```csharp
	const string leaderboardId = "level1";
	var result = await client.ListLeaderboardRecordsAsync(session, leaderboardId);
	foreach (var r in result.Records)
	{
	    System.Console.WriteLine("Record for '{0}' score '{1}'", r.Username, r.Score);
	}
	```

=== "Unity"
	```csharp
	const string leaderboardId = "level1";
	var result = await client.ListLeaderboardRecordsAsync(session, leaderboardId);
	foreach (var r in result.Records)
	{
	    Debug.LogFormat("Record for '{0}' score '{1}'", r.Username, r.Score);
	}
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](NLeaderboardRecordListPtr recordsList)
	{
	  for (auto& record : recordsList->records)
	  {
	    CCLOG("Record username %s and score %ld", record.username.c_str(), record.score);
	  }
	};

	string leaderboardId = "level1";

	client->listLeaderboardRecords(session,
	  leaderboardId,
	  {},
	  opt::nullopt,
	  opt::nullopt,
	  successCallback
	);
	```

=== "Cocos2d-x JS"
	```js
	var leaderboardId = "level1";
	client.listLeaderboardRecords(session, leaderboardId)
	  .then(function(result) {
	      result.records.forEach(function(record) {
	        cc.log("Record username", record.username, "and score", record.score);
	      });
	    },
	    function(error) {
	      cc.error("list leaderboard records failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](NLeaderboardRecordListPtr recordsList)
	{
	  for (auto& record : recordsList->records)
	  {
	    std::cout << "Record username " << record.username << " and score " << record.score << std::endl;
	  }
	};

	string leaderboardId = "level1";

	client->listLeaderboardRecords(session,
	  leaderboardId,
	  {},
	  opt::nullopt,
	  opt::nullopt,
	  successCallback
	);
	```

=== "Java"
	```java
	final String leaderboard = "level1";
	LeaderboardRecordList records = client.listLeaderboardRecords(session, leaderboard);
	for (LeaderboardRecord record : records.getRecordsList()) {
	  System.out.format("Record for %s score %d", record.getUsername(), record.getScore());
	}
	```

=== "Swift"
	```swift
	// Requires Nakama 1.x
	let id = leaderboard.id //a Leaderboard ID.
	var message = LeaderboardRecordsListMessage(leaderboardID: id)
	client.send(message: message).then { records in
	  for record in records {
	    NSLog("Record handle %@ and score %d.", record.handle, record.score)
	  }
	}.catch { err in
	  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
	}
	```

=== "Godot"
	```gdscript
	var leaderboard_id = "level1"
	var result : NakamaAPI.ApiLeaderboardRecordList = yield(client.list_leaderboard_records_async(session, leaderboard_id), "completed")
	if result.is_exception():
		print("An error occured: %s" % result)
		return
	for r in result.records:
		var record : NakamaAPI.ApiLeaderboardRecord = r
		print("Record username %s and score %s" % [record.username, record.score])
	```


=== "REST"
    ```
	GET /v2/leaderboard/<leaderboardId>
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>
	```

You can fetch the next set of results with a cursor.

=== "cURL"
	```sh
	curl -X GET "http://127.0.0.1:7350/v2/leaderboard/<leaderboardId>?cursor=<next_cursor>" \
	  -H 'Authorization: Bearer <session token>'
	```

=== "Javascript"
	```js
	var leaderboardId = "level1";

	var result = await client.listLeaderboardRecords(session, leaderboardId);
	result.records.forEach(function(record) {
	  console.log("Record username %o and score %o", record.username, record.score);
	});

	// If there are more results get next page.
	if (result.next_cursor) {
	  result = await client.listLeaderboardRecords(session, leaderboardId, null, null, result.next_cursor);
	  result.records.forEach(function(record) {
	    console.log("Record username %o and score %o", record.username, record.score);
	  });
	}
	```

=== ".NET"
	```csharp
	const string leaderboardId = "level1";
	var result = await client.ListLeaderboardRecordsAsync(session, leaderboardId);
	foreach (var r in result.Records)
	{
	    System.Console.WriteLine("Record for '{0}' score '{1}'", r.Username, r.Score);
	}
	// If there are more results get next page.
	if (result.NextCursor != null)
	{
	    var c = result.NextCursor;
	    result = await client.ListLeaderboardRecordsAsync(session, leaderboardId, null, 100, c);
	    foreach (var r in result.Records)
	    {
	        System.Console.WriteLine("Record for '{0}' score '{1}'", r.Username, r.Score);
	    }
	}
	```

=== "Unity"
	```csharp
	const string leaderboardId = "level1";
	var result = await client.ListLeaderboardRecordsAsync(session, leaderboardId);
	foreach (var r in result.Records)
	{
	    Debug.LogFormat("Record for '{0}' score '{1}'", r.Username, r.Score);
	}
	// If there are more results get next page.
	if (result.NextCursor != null)
	{
	    var c = result.NextCursor;
	    result = await client.ListLeaderboardRecordsAsync(session, leaderboardId, null, 100, c);
	    foreach (var r in result.Records)
	    {
	        Debug.LogFormat("Record for '{0}' score '{1}'", r.Username, r.Score);
	    }
	}
	```


=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](NLeaderboardRecordListPtr recordsList)
	{
	  for (auto& record : recordsList->records)
	  {
	    CCLOG("Record username %s and score %ld", record.username.c_str(), record.score);
	  }

	  if (!recordsList->nextCursor.empty())
	  {
	    auto successCallback = [this](NLeaderboardRecordListPtr recordsList)
	    {
	      for (auto& record : recordsList->records)
	      {
	        CCLOG("Record username %s and score %ld", record.username.c_str(), record.score);
	      }
	    };

	    string leaderboardId = "level1";

	    client->listLeaderboardRecords(session,
	        leaderboardId,
	        {},
	        opt::nullopt,
	        recordsList->nextCursor,
	        successCallback
	    );
	  }
	};

	string leaderboardId = "level1";

	client->listLeaderboardRecords(session,
	  leaderboardId,
	  {},
	  opt::nullopt,
	  opt::nullopt,
	  successCallback
	);
	```

=== "Cocos2d-x JS"
	```js
	var leaderboardId = "level1";
	client.listLeaderboardRecords(session, leaderboardId)
	  .then(function(result) {
	      result.records.forEach(function(record) {
	        cc.log("Record username", record.username, "and score", record.score);
	      });

	      // If there are more results get next page.
	      if (result.next_cursor) {
	        client.listLeaderboardRecords(session, leaderboardId, null, null, result.next_cursor)
	          .then(function(result) {
	              result.records.forEach(function(record) {
	                cc.log("Record username", record.username, "and score", record.score);
	              }
	          });
	      }
	    },
	    function(error) {
	      cc.error("list leaderboard records failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [this](NLeaderboardRecordListPtr recordsList)
	{
	  for (auto& record : recordsList->records)
	  {
	    std::cout << "Record username " << record.username << " and score " << record.score << std::endl;
	  }

	  if (!recordsList->nextCursor.empty())
	  {
	    auto successCallback = [this](NLeaderboardRecordListPtr recordsList)
	    {
	      for (auto& record : recordsList->records)
	      {
	        std::cout << "Record username " << record.username << " and score " << record.score << std::endl;
	      }
	    };

	    string leaderboardId = "level1";

	    client->listLeaderboardRecords(session,
	        leaderboardId,
	        {},
	        opt::nullopt,
	        recordsList->nextCursor,
	        successCallback
	    );
	  }
	};

	string leaderboardId = "level1";

	client->listLeaderboardRecords(session,
	  leaderboardId,
	  {},
	  opt::nullopt,
	  opt::nullopt,
	  successCallback
	);
	```

=== "Java"
	```java
	final String leaderboard = "level1";
	LeaderboardRecordList records = client.listLeaderboardRecords(session, leaderboard);
	for (LeaderboardRecord record : records.getRecordsList()) {
	  System.out.format("Record for %s score %d", record.getUsername(), record.getScore());
	}

	// If there are more results get next page.
	if (records.getCursor() != null) {
	  var c = result.NextCursor;
	  records = client.listLeaderboardRecords(session, leaderboard, null, 100, records.getNextCursor());
	  for (LeaderboardRecord record : records.getRecordsList()) {
	    System.out.format("Record for %s score %d", record.getUsername(), record.getScore());
	  }
	}
	```

=== "Swift"
	```swift
	// Requires Nakama 1.x
	let id = leaderboard.id //a Leaderboard ID.
	var message = LeaderboardRecordsListMessage(leaderboardID: id)

	client.send(message: message).then { records in
	  if let cursor = records.cursor && records.count > 0 {
	    message.cursor = cursor
	    client.send(message: message).then { r in
	      for record in r {
	        NSLog("Record handle %@ and score %d.", record.handle, record.score)
	      }
	    }.catch { err in
	      NSLog("Error %@ : %@", err, (err as! NakamaError).message)
	    }
	  }
	}.catch { err in
	  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
	}
	```

=== "Godot"
	```gdscript
	var leaderboard_id = "level1"
	var result : NakamaAPI.ApiLeaderboardRecordList = yield(client.list_leaderboard_records_async(session, leaderboard_id, null, null, 100), "completed")
	if result.is_exception():
		print("An error occured: %s" % result)
		return
	for r in result.records:
		var record : NakamaAPI.ApiLeaderboardRecord = r
		print("Record username %s and score %s" % [record.username, record.score])

	if result.next_cursor:
		result = yield(client.list_leaderboard_records_async(session, leaderboard_id, null, null, 100, result.next_cursor), "completed")
		if result.is_exception():
			print("An error occured: %s" % result)
			return
		for r in result.records:
			var record : NakamaAPI.ApiLeaderboardRecord = r
			print("Record username %s and score %s" % [record.username, record.score])
	```

=== "REST"
    ```
	GET /v2/leaderboard/<leaderboardId>?cursor=<next_cursor>
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>
	```

### List by friends

You can use a bunch of owner IDs to filter the records to only ones owned by those users. This can be used to retrieve only scores belonging to the user's friends.

=== "cURL"
	```sh
	curl -X GET "http://127.0.0.1:7350/v2/leaderboard/<leaderboardId>?owner_ids=some&owner_ids=friends" \
	  -H 'Authorization: Bearer <session token>'
	```

=== "Javascript"
	```js
	var leaderboardId = "level1";
	var ownerIds = ["some", "friends", "user ids"];
	var result = await client.listLeaderboardRecords(session, leaderboardId, ownerIds);
	result.records.forEach(function(record) {
	  console.log("Record username %o and score %o", record.username, record.score);
	});
	```

=== ".NET"
	```csharp
	const string leaderboardId = "level1";
	var ownerIds = new[] {"some", "friends", "user ids"};
	var result = await client.ListLeaderboardRecordsAsync(session, leaderboardId, ownerIds);
	foreach (var r in result.OwnerRecords)
	{
	    System.Console.WriteLine("Record for '{0}' score '{1}'", r.Username, r.Score);
	}
	```

=== "Unity"
	```csharp
	const string leaderboardId = "level1";
	var ownerIds = new[] {"some", "friends", "user ids"};
	var result = await client.ListLeaderboardRecordsAsync(session, leaderboardId, ownerIds);
	foreach (var r in result.OwnerRecords)
	{
	    Debug.LogFormat("Record for '{0}' score '{1}'", r.Username, r.Score);
	}
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](NLeaderboardRecordListPtr recordsList)
	{
	  for (auto& record : recordsList->records)
	  {
	    CCLOG("Record username %s and score %ld", record.username.c_str(), record.score);
	  }
	};

	vector<string> ownerIds = { "some", "friends", "user ids" };

	string leaderboardId = "level1";

	client->listLeaderboardRecords(session,
	    leaderboardId,
	    ownerIds,
	    opt::nullopt,
	    opt::nullopt,
	    successCallback
	);
	```

=== "Cocos2d-x JS"
	```js
	var leaderboardId = "level1";
	var ownerIds = ["some", "friends", "user ids"];
	client.listLeaderboardRecords(session, leaderboardId, ownerIds)
	  .then(function(result) {
	      result.records.forEach(function(record) {
	        cc.log("Record username", record.username, "and score", record.score);
	      });
	    },
	    function(error) {
	      cc.error("list leaderboard records failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](NLeaderboardRecordListPtr recordsList)
	{
	  for (auto& record : recordsList->records)
	  {
	    std::cout << "Record username " << record.username << " and score " << record.score << std::endl;
	  }
	};

	vector<string> ownerIds = { "some", "friends", "user ids" };

	string leaderboardId = "level1";

	client->listLeaderboardRecords(session,
	    leaderboardId,
	    ownerIds,
	    opt::nullopt,
	    opt::nullopt,
	    successCallback
	);
	```

=== "Java"
	```java
	String leaderboard = "level1";
	String[] ownerIds = new String[] {"some", "friends", "user ids"};
	LeaderboardRecordList records = await client.ListLeaderboardRecordsAsync(session, leaderboard, ownerIds);
	for (LeaderboardRecord record : records.getRecordsList()) {
	  System.out.format("Record for %s score %d", record.getUsername(), record.getScore());
	}
	```

=== "Swift"
	```swift
	// Requires Nakama 1.x
	let id = leaderboard.id // a Leaderboard ID.
	let ownerIds : [String] = []
	ownersIds.append(user.id) // a user ID

	var message = LeaderboardRecordsListMessage(leaderboardID: id)
	message.filterByOwnerIds = ownerIds
	client.send(message: message).then { records in
	  for record in records {
	    NSLog("Record handle %@ and score %d.", record.handle, record.score)
	  }
	}.catch { err in
	  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
	}
	```

=== "Godot"
	```gdscript
	var leaderboard_id = "level1"
	var owner_ids = ["some", "friend", "user id"]
	var result : NakamaAPI.ApiLeaderboardRecordList = yield(client.list_leaderboard_records_async(session, leaderboard_id, owner_ids), "completed")
	if result.is_exception():
		print("An error occured: %s" % result)
		return
	for r in result.records:
		var record : NakamaAPI.ApiLeaderboardRecord = r
		print("Record username %s and score %s" % [record.username, record.score])
	```

=== "REST"
    ```
	GET /v2/leaderboard/<leaderboardId>?owner_ids=some&owner_ids=friends
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>
	```

## List leaderboard records around owner

Fetch the list of leaderboard records around the owner.

=== "cURL"
	```sh
	curl -X GET "http://127.0.0.1:7350/v2/leaderboard/<leaderboard_id>/owner/<owner_id>?limit=<limit>"
	  -H 'Authorization: Bearer <session token>'
	```

=== "Javascript"
	```js
	var id = "someid";
	var ownerId = "some user ID";
	var limit = 100;
	var result = await client.listLeaderboardRecordsAroundOwner(session, id, ownerId, limit);
	```

=== ".NET"
	```csharp
	var leaderboardId = "someid";
	var ownerId = session.UserId;
	var limit = 100;
	var result = await client.ListLeaderboardRecordsAroundOwnerAsync(session, leaderboardId, ownerId, limit);
	```

=== "Unity"
	```csharp
	var leaderboardId = "someid";
	var ownerId = session.UserId;
	var limit = 100;
	var result = await client.ListLeaderboardRecordsAroundOwnerAsync(session, leaderboardId, ownerId, limit);
	```

=== "Cocos2d-x C++"
	```cpp
	string leaderboardId = "level1";
	string ownerId = "some user ID";
	int32_t limit = 100;
	client->listLeaderboardRecordsAroundOwner(session,
	      leaderboardId,
	      ownerId,
	      limit,
	      successCallback
	  );
	```

=== "Cocos2d-x JS"
	```js
	var id = "someid";
	var ownerId = "some user ID";
	var limit = 100;
	client.listLeaderboardRecordsAroundOwner(session, id, ownerId, limit)
	  .then(...);
	```

=== "C++"
	```cpp
	string leaderboardId = "level1";
	string ownerId = "some user ID";
	int32_t limit = 100;
	client->listLeaderboardRecordsAroundOwner(session,
	      leaderboardId,
	      ownerId,
	      limit,
	      successCallback
	  );
	```

=== "Java"
	```java
	String id = "someid";
	String ownerId = session.getUserId();
	int limit = 100;
	LeaderboardRecordList records = client.listLeaderboardRecordsAroundOwner(session, id, ownerId, limit).get();
	```

=== "Swift"
	```swift
	// Will be made available soon.
	```

=== "Godot"
	```gdscript
	var leaderboard_id = "level1"
	var owner_id = "user id"
	var result : NakamaAPI.ApiLeaderboardRecordList = yield(client.list_leaderboard_records_around_owner_async(session, leaderboard_id, owner_id), "completed")
	if result.is_exception():
		print("An error occured: %s" % result)
		return
	for r in result.records:
		var record : NakamaAPI.ApiLeaderboardRecord = r
		print("Record username %s and score %s" % [record.username, record.score])
	```

=== "REST"
    ```
	GET /v2/leaderboard/<leaderboard_id>/owner/<owner_id>?limit=<limit>
	Host: 127.0.0.1:7350
	Accept: application/json
	Content-Type: application/json
	Authorization: Bearer <session token>
	```
