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

```sh fct_label="cURL"
curl -X POST \
  'http://127.0.0.1:7350/v2/leaderboard/<leaderboardId>' \
  -H 'Authorization: Bearer <session token>'
  -d '{"record": {"score": 100}}'
```

```js fct_label="Javascript"
var leaderboardId = "level1";
var submission = {score: 100};
var record = await client.writeLeaderboardRecord(session, leaderboardId, submission);
console.log("New record username %o and score %o", record.username, record.score);
```

```csharp fct_label=".NET"
const string leaderboard = "level1";
const long score = 100L;
var r = await client.WriteLeaderboardRecordAsync(session, leaderboard, score);
System.Console.WriteLine("New record for '{0}' score '{1}'", r.Username, r.Score);
```

```csharp fct_label="Unity"
const string leaderboard = "level1";
const long score = 100L;
var r = await client.WriteLeaderboardRecordAsync(session, leaderboard, score);
Debug.LogFormat("New record for '{0}' score '{1}'", r.Username, r.Score);
```

```swift fct_label="Swift"
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

```fct_label="REST"
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

## Submit a score

A user can submit a score to a leaderboard and update it at any time. When a score is submitted the leaderboard's pre-configured sort order and operator determine what effect the operation will have.

The "set" operator will ensure the leaderboard record always keeps the latest value, even if it is worse than the previous one.

Submitting to a leaderboard with the "best" operator ensures the record tracks the best value it has seen for that record. For a descending leaderboard this means the highest value, for an ascending one the lowest. If there is no previous value for the record, this behaves like "set".

With the "incr" operator the new value is added to any existing score for that record. If there is no previous value for the record, this behaves like "set".

```sh fct_label="cURL"
curl -X POST \
  'http://127.0.0.1:7350/v2/leaderboard/<leaderboardId>' \
  -H 'Authorization: Bearer <session token>'
  -d '{"record": {"score": 100}}'
```

```js fct_label="JavaScript"
var leaderboardId = "level1";
var submission = {score: 100};
var record = await client.writeLeaderboardRecord(session, leaderboardId, submission);
console.log("New record username %o and score %o", record.username, record.score);
```

```csharp fct_label=".NET"
const string leaderboard = "level1";
const long score = 100L;
var r = await client.WriteLeaderboardRecordAsync(session, leaderboard, score);
System.Console.WriteLine("New record for '{0}' score '{1}'", r.Username, r.Score);
```

```csharp fct_label="Unity"
const string leaderboard = "level1";
const long score = 100L;
var r = await client.WriteLeaderboardRecordAsync(session, leaderboard, score);
Debug.LogFormat("New record for '{0}' score '{1}'", r.Username, r.Score);
```

```swift fct_label="Swift"
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

```fct_label="REST"
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

## List records

A user can list records from a leaderboard. This makes it easy to compare scores to other users and see their positions.

### List by score

The standard way to list records is ordered by score based on the sort order in the leaderboard.

```sh fct_label="cURL"
curl -X GET \
  'http://127.0.0.1:7350/v2/leaderboard/<leaderboardId>' \
  -H 'Authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
var leaderboardId = "level1";
var result = await client.listLeaderboardRecords(session, leaderboardId);
result.records.forEach(function(record) {
  console.log("Record username %o and score %o", record.username, record.score);
});
```

```csharp fct_label=".NET"
const string leaderboard = "level1";
var result = await client.ListLeaderboardRecordsAsync(session, leaderboard);
foreach (var r in result.Records)
{
  System.Console.WriteLine("Record for '{0}' score '{1}'", r.Username, r.Score);
}
```

```csharp fct_label="Unity"
const string leaderboard = "level1";
var result = await client.ListLeaderboardRecordsAsync(session, leaderboard);
foreach (var r in result.Records)
{
  Debug.LogFormat("Record for '{0}' score '{1}'", r.Username, r.Score);
}
```

```swift fct_label="Swift"
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

```fct_label="REST"
GET /v2/leaderboard/<leaderboardId>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

You can fetch the next set of results with a cursor.

```sh fct_label="cURL"
curl -X GET \
  'http://127.0.0.1:7350/v2/leaderboard/<leaderboardId>?cursor=<next_cursor>' \
  -H 'Authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
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

```csharp fct_label=".NET"
const string leaderboard = "level1";
var result = await client.ListLeaderboardRecordsAsync(session, leaderboard);
foreach (var r in result.Records)
{
  System.Console.WriteLine("Record for '{0}' score '{1}'", r.Username, r.Score);
}
// If there are more results get next page.
if (result.NextCursor != null)
{
  var c = result.NextCursor;
  result = await client.ListLeaderboardRecordsAsync(session, leaderboard, null, 100, c);
  foreach (var r in result.Records)
  {
    System.Console.WriteLine("Record for '{0}' score '{1}'", r.Username, r.Score);
  }
}
```

```csharp fct_label="Unity"
const string leaderboard = "level1";
var result = await client.ListLeaderboardRecordsAsync(session, leaderboard);
foreach (var r in result.Records)
{
  Debug.LogFormat("Record for '{0}' score '{1}'", r.Username, r.Score);
}
// If there are more results get next page.
if (result.NextCursor != null)
{
  var c = result.NextCursor;
  result = await client.ListLeaderboardRecordsAsync(session, leaderboard, null, 100, c);
  foreach (var r in result.Records)
  {
    Debug.LogFormat("Record for '{0}' score '{1}'", r.Username, r.Score);
  }
}
```

```swift fct_label="Swift"
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

```fct_label="REST"
GET /v2/leaderboard/<leaderboardId>?cursor=<next_cursor>
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```

### List by friends

You can use a bunch of owner IDs to filter the records to only ones owned by those users. This can be used to retrieve only scores belonging to the user's friends.

```sh fct_label="cURL"
curl -X GET \
  'http://127.0.0.1:7350/v2/leaderboard/<leaderboardId>?owner_ids=some&owner_ids=friends' \
  -H 'Authorization: Bearer <session token>'
```

```js fct_label="JavaScript"
var leaderboardId = "level1";
var ownerIds = ["some", "friends", "user ids"];
var result = await client.listLeaderboardRecords(session, leaderboardId, ownerIds);
result.records.forEach(function(record) {
  console.log("Record username %o and score %o", record.username, record.score);
});
```

```csharp fct_label=".NET"
const string leaderboard = "level1";
var ownerIds = new[] {"some", "friends", "user ids"};
var result = await client.ListLeaderboardRecordsAsync(session, leaderboard, ownerIds);
foreach (var r in result.OwnerRecords)
{
  System.Console.WriteLine("Record for '{0}' score '{1}'", r.Username, r.Score);
}
```

```csharp fct_label="Unity"
const string leaderboard = "level1";
var ownerIds = new[] {"some", "friends", "user ids"};
var result = await client.ListLeaderboardRecordsAsync(session, leaderboard, ownerIds);
foreach (var r in result.OwnerRecords)
{
  Debug.LogFormat("Record for '{0}' score '{1}'", r.Username, r.Score);
}
```

```swift fct_label="Swift"
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

```fct_label="REST"
GET /v2/leaderboard/<leaderboardId>?owner_ids=some&owner_ids=friends
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>
```
