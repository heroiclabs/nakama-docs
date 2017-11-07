# Leaderboards

Leaderboards are a great way to add a social and competitive element to any game. They're a fun way to drive competition among your players. The server supports an __unlimited__ number of individual leaderboards with each one as a scoreboard which tracks separate records.

The server has no special requirement on what the score value should represent from your game. A leaderboard is created with a sort order on values. If you're using lap time or currency in records you'll want to order the results in ASC or DESC mode as preferred.

!!! Tip
    You can use a leaderboard to track any score you like. Some good examples are: highest points, longest survival time, fastest lap time, quickest level completion, and anything else which can be competed over!

Leaderboards are dynamic in the server because they don't need to be preconfigured like would be needed if you've used Google Play Games or Apple Game Center in the past. A leaderboard can be created via server-side code.

## Leaderboard object

Each leaderboard is a collection of records where each record is a ranked score with metadata. A leaderboard is uniquely identified by an ID.

Leaderboard records are sorted based on their configured sort order: DESC (default) or ASC. The sort order is decided when a leaderboard is created and cannot be changed later. All leaderboard configuration is immutable once created. You should delete the leaderboard and create a new one if you need to change the sort order.

### Reset schedules

You can assign each leaderboard an optional reset schedule. Records contained in the leaderboard will expire based on this schedule and users will be able to submit new scores for each reset cycle.

Reset schedules are defined in <a href="https://en.wikipedia.org/wiki/Cron" target="\_blank">CRON format</a> when the leaderboard is created. If a leaderboards has no reset schedule set it will never expire.

### Leaderboard records

Each leaderboard contains a list of records ordered by their scores.

Records belong to an owner usually a user but other objects like a group ID or some other custom ID can be used. Each owner will only have one record per leaderboard. If a leaderboard expires each owner will be able to submit a new score which rolls over.

The score in each record can be updated as the owner progresses. Scores can be updated as often as wanted and can increase or decrease. A record has some builtin fields like lang, location, and timezone. These values are used to filter leaderboard results.

#### Custom fields

Each record can optionally include additional data about the score or the owner when submitted. The extra fields must be JSON encoded and submitted as the metadata. A good use case for metadata is info about race conditions in a driving game, such as weather, which can give extra UI hints when users list scores.

```csharp fct_label="Unity"
string id = leaderboard.Id; // an INLeaderboard Id.
var score = 1200;

// add custom fields with each record.
var metadata = "{\"race_conditions\": [\"sunny\", \"clear\"]}";

var message = new NLeaderboardRecordWriteMessage.Builder(id)
    .Set(score)
    .Metadata(metadata)
    .Build();
client.Send(message, (INResultSet<INLeaderboardRecord> results) => {
  foreach (var r in list.Results) {
    Debug.LogFormat("Record handle '{0}' score '{1}'.", r.Handle, r.Score);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
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

## Create a leaderboard

A leaderboard can be created via server-side code at startup or within a [registered function](runtime-code-function-reference.md#register-hooks). The ID given to the leaderboard is used to submit scores to it.

```lua
local metadata = {
  weather_conditions = "rain"
}
local id = "4ec4f126-3f9d-11e7-84ef-b7c182b36521"
nk.leaderboard_create(id, "desc", "0 0 * * 1", metadata, false)
```

## List leaderboards

All leaderboards can be listed by a user.

```csharp fct_label="Unity"
var message = new NLeaderboardsListMessage.Builder().Build();
client.Send(message, (INResultSet<INLeaderboard> list) => {
  Debug.LogFormat("Found '{0}' leaderboards.", list.Results.Count);

  foreach (var leaderboard in list.Results) {
    Debug.LogFormat("Leaderboard id '{0}' and sort '{1}'", leaderboard.Id, leaderboard.Sort);
    Debug.LogFormat("Leaderboard metadata '{0}'", leaderboard.Metadata);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
var message = LeaderboardsListMessage()
client.send(message: message).then { leaderboards in
  NSLog("Found %d leaderboards.", leaderboards.count)
  for leaderboard in leaderboards {
    NSLog("Leaderboard id %@ and sort %d.", leaderboard.id, leaderboard.sort)
    NSLog("Leaderboard metadata %@", leaderboard.metadata)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

When you have more than 100 leaderboards you can fetch the next set of results with a cursor.

```csharp fct_label="Unity"
var errorHandler = delegate(INError err) {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

var messageBuilder = new NLeaderboardsListMessage.Builder();
client.Send(messageBuilder.Build(), (INResultSet<INLeaderboard> list) => {
  // Lets get the next page of results.
  INCursor cursor = list.Cursor;
  if (cursor != null && list.Results.Count > 0) {
    var message = messageBuilder.Cursor(cursor).Build();

    client.Send(message, (INResultSet<INLeaderboard> nextList) => {
      foreach (var l in nextList.Results) {
        Debug.LogFormat("Leaderboard id '{0}' and sort '{1}'", l.Id, l.Sort);
        Debug.LogFormat("Leaderboard metadata '{0}'", l.Metadata);
      }
    }, errorHandler);
  }
}, errorHandler);
```

```swift fct_label="Swift"

var message = LeaderboardsListMessage()
client.send(message: message).then { leaderboards in
  // Lets get the next page of results.
  if let cursor = leaderboards.cursor && leaderboards.count > 0 {
    message.cursor = cursor
    client.send(message: message).then { ls in
      NSLog("Found %d leaderboards.", ls.count)
      for leaderboard in ls {
        NSLog("Leaderboard id %@ and sort %d.", leaderboard.id, leaderboard.sort)
        NSLog("Leaderboard metadata %@", leaderboard.metadata)
      }
    }.catch { err in
      NSLog("Error %@ : %@", err, (err as! NakamaError).message)
    }
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

## Submit a score

A user can submit a score to a leaderboard and update it at any time. When a score is submitted it's sent with an operator which indicates how the new value should change the current score stored. The operators are: "set", "best", "inc", "decr".

### Set operator

The set operator will submit the score and replace any current value for the owner.

```csharp fct_label="Unity"
string id = leaderboard.Id; // an INLeaderboard Id.
var score = 1200;

var message = new NLeaderboardRecordWriteMessage.Builder(id)
    .Set(score)
    .Build();
client.Send(message, (INResultSet<INLeaderboardRecord> list) => {
  foreach (var r in list.Results) {
    Debug.LogFormat("Record handle '{0}' score '{1}'.", r.Handle, r.Score);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
let id = leaderboard.id //a Leaderboard ID.
let score = 1200

var recordWrite : LeaderboardRecordWrite(leaderboardID: id)
recordWrite.set = score

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

### Best operator

The best operator will check the new score is better than the current score and keep which ever value is best based on the sort order of the leaderboard. If no score exists this operator works like "set".

```csharp fct_label="Unity"
string id = leaderboard.Id; // an INLeaderboard Id.
var score = 1200;

var message = new NLeaderboardRecordWriteMessage.Builder(id)
    .Best(score)
    .Build();
client.Send(message, (INResultSet<INLeaderboardRecord> list) => {
  foreach (var r in list.Results) {
    Debug.LogFormat("Record handle '{0}' score '{1}'.", r.Handle, r.Score);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
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

### Increment operator

The increment operator will add the score value to the current score. If no score exists the new score will be added to 0.

```csharp fct_label="Unity"
string id = leaderboard.Id; // an INLeaderboard Id.
var score = 1200;

var message = new NLeaderboardRecordWriteMessage.Builder(id)
    .Increment(score)
    .Build();
client.Send(message, (INResultSet<INLeaderboardRecord> list) => {
  foreach (var r in list.Results) {
    Debug.LogFormat("Record handle '{0}' score '{1}'.", r.Handle, r.Score);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
let id = leaderboard.id //a Leaderboard ID.
let score = 1200

var recordWrite : LeaderboardRecordWrite(leaderboardID: id)
recordWrite.increment = score

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

### Decrement operator

The decrement operator will subtract the score value from the current score. If no score exists the new score will be subtracted from 0.

```csharp fct_label="Unity"
string id = leaderboard.Id; // an INLeaderboard Id.
var score = 1200;

var message = new NLeaderboardRecordWriteMessage.Builder(id)
    .Decrement(score)
    .Build();
client.Send(message, (INResultSet<INLeaderboardRecord> list) => {
  foreach (var r in list.Results) {
    Debug.LogFormat("Record handle '{0}' score '{1}'.", r.Handle, r.Score);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
let id = leaderboard.id //a Leaderboard ID.
let score = 1200

var recordWrite : LeaderboardRecordWrite(leaderboardID: id)
recordWrite.decrement = score

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

## List records

A user can list records from a leaderboard. This makes it easy to compare scores to other users and see their positions.

### List by score

The standard way to list records is ordered by score based on the sort order in the leaderboard.

```csharp fct_label="Unity"
string id = leaderboard.Id; // an INLeaderboard Id.

var message = new NLeaderboardRecordsListMessage.Builder(id).Build();
client.Send(message, (INResultSet<INLeaderboardRecord> list) => {
  foreach (var r in list.Results) {
    Debug.LogFormat("Record handle '{0}' score '{1}'.", r.Handle, r.Score);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
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

You can fetch the next set of results with a cursor.

```csharp fct_label="Unity"
var errorHandler = delegate(INError err) {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
};

string id = leaderboard.Id; // an INLeaderboard Id.

var messageBuilder = new NLeaderboardRecordsListMessage.Builder(id);
client.Send(message.Build(), (INResultSet<INLeaderboardRecord> list) => {
  // Lets get the next page of results.
  INCursor cursor = list.Cursor;
  if (cursor != null && list.Results.Count > 0) {
    var message = messageBuilder.Cursor(cursor).Build();

    client.Send(message, (INResultSet<INLeaderboardRecord> nextList) => {
      foreach (var r in nextList.Results) {
        Debug.LogFormat("Record handle '{0}' score '{1}'.", r.Handle, r.Score);
      }
    }, errorHandler);
  }
}, errorHandler);
```

```swift fct_label="Swift"
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

### List by filter

You can add a filter on one of "lang", "location", or "timezone".

```csharp fct_label="Unity"
string id = leaderboard.Id; // an INLeaderboard Id.

var message = new NLeaderboardRecordsListMessage.Builder(id)
    .FilterByLocation("San Francisco")
    .Build();
client.Send(message, (INResultSet<INLeaderboardRecord> list) => {
  foreach (var r in list.Results) {
    Debug.LogFormat("Record handle '{0}' score '{1}'.", r.Handle, r.Score);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
let id = leaderboard.id // a Leaderboard ID.
var message = LeaderboardRecordsListMessage(leaderboardID: id)
message.filterByLocation = "San Francisco"
client.send(message: message).then { records in
  for record in records {
    NSLog("Record handle %@ and score %d.", record.handle, record.score)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

### List by friends

You can use a bunch of owner IDs to filter the records to only ones owned by those users. This can be used to retrieve only scores belonging to the user's friends.

```csharp fct_label="Unity"
IList<string> ownerIds = new List<string>();
ownerIds.Add(user.Id); // an INUser Id.
string id = leaderboard.Id; // an INLeaderboard Id.

var message = new NLeaderboardRecordsListMessage.Builder(id)
    .FilterByOwnerIds(ownerIds)
    .Build();
client.Send(message, (INResultSet<INLeaderboardRecord> list) => {
  foreach (var r in list.Results) {
    Debug.LogFormat("Record handle '{0}' score '{1}'.", r.Handle, r.Score);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
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

### Find current user

A leaderboard can be "scrolled" to the page which contains a record owner. This can be used to give users a view of their own position within a leaderboard.

```csharp fct_label="Unity"
string id = leaderboard.Id; // an INLeaderboard Id.
string ownerId = user.Id;   // an INUser Id.

var message = new NLeaderboardRecordsListMessage.Builder(id)
    .FilterByPagingToOwnerId(ownerId)
    .Build();
client.Send(message, (INResultSet<INLeaderboardRecord> list) => {
  foreach (var r in list.Results) {
    Debug.LogFormat("Record handle '{0}' score '{1}'.", r.Handle, r.Score);
  }
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```swift fct_label="Swift"
let id = leaderboard.id // a Leaderboard ID.
string ownerID = user.id // a user ID.

var message = LeaderboardRecordsListMessage(leaderboardID: id)
message.filterByPagingToOwnerID = ownerID
client.send(message: message).then { records in
  for record in records {
    NSLog("Record handle %@ and score %d.", record.handle, record.score)
  }
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```
