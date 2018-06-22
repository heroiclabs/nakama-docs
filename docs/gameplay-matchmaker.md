# Matchmaker

Nakama's matchmaker allows users to find opponents and teammates for matches, groups, and other activities. The matchmaker maintains a pool of users that are currently looking for opponents and places them together whenever a good match is possible.

In the server we've decoupled how users are matched from the [realtime multiplayer engine](gameplay-multiplayer-realtime.md). This makes it easy to use the matchmaker system to find users even if the gameplay isn't realtime. It could be a casual social game where you want to find random new users to become friends with and chat together, or an asynchronous PvP game where gameplay happens in a simulated battle.

The matchmaker receives and tracks matchmaking requests, then groups users together based on the criteria they've expressed in their properties and query.

To ensure relevant results the matchmaker only searches through users that are both online and currently matchmaking themselves.

!!! Tip
    Users must connect and remain online until the matchmaking process completes. If they disconnect they will be removed from the matchmaker until they try again.

## Add a user to the matchmaker

To begin matchmaking users add themselves to the matchmaking pool. They remain in the pool until the matchmaker finds them matching opponents, the user cancels the process, or the user disconnects.

Each matchmaker entry is composed of optional **Properties**, a **Query**, and a **Minimum and maximum count**.

!!! Tip
    Users can submit themselves to the matchmaking pool multiple times, for example to search through multiple game modes with different rules.

### Properties

Properties are key-value pairs that describe the user. Rank information, connecting region information, or selected match types are good examples of data that should be stored in properties. These properties are submitted by the user when they begin the matchmaking process, and may be different each time the user matchmakes.

When matchmaking completes these properties are visible to all matched users. You can store extra information without affecting the matchmaking process itself if it's useful to clients - just submit properties that aren't queried for as part of the matchmaking process.

```js fct_label="JavaScript"
const message = { matchmaker_add: {
  min_count: 2,
  max_count: 4,
  query: "*",
  string_properties: {
    region: "europe"
  },
  numeric_properties: {
    rank: 8
  }
}};
var ticket = await socket.send(message);
```

```csharp fct_label=".NET"
var query = "*"
var minCount = 2;
var maxCount = 4;
var stringProperties = new Dictionary<string, string>(){
  {"region", "europe"}
};
var numericProperties = new Dictionary<string, int>(){
  {"rank", 8}
};

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount, stringProperties, numericProperties);
```

```csharp fct_label="Unity"
var query = "*"
var minCount = 2;
var maxCount = 4;
var stringProperties = new Dictionary<string, string>(){
  {"region", "europe"}
};
var numericProperties = new Dictionary<string, int>(){
  {"rank", 8}
};

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount, stringProperties, numericProperties);
```

### Query

The query defines how the user wants to find their opponents. Queries inspect the properties set by matchmaker users to find users eligible to be matched, or can ignore them to find any available users using the wildcard `*` query. A typical matchmaker query may look for opponents between given ranks, or in a particular region.

Nakama uses the [Bleve](http://www.blevesearch.com/) search and indexing engine internally to find opponents in the matchmaker pool. All of the standard [Bleve query string syntax](http://www.blevesearch.com/docs/Query-String-Query/) is accepted, see the [full documentation](http://www.blevesearch.com/docs/Query-String-Query/) for the complete query options available.

!!! Tip
    Each user's matchmaker properties are available in queries under the `properties` prefix.

Queries are composed of one or more query terms, usually defined as `field:value`. By default the field **should have** the value, but this is not strictly required - this results in good matches if possible but will still accept any opponents that are available otherwise. To strictly match values define terms as `+field:value`, meaning the field **must have** the value for the opponent to match.

Multiple terms in a query are separated by spaces `field1:value1 field2:value2` - `field1` **should have** `value1` **and** `field2` **should have** `value2`, but this is not strictly required. You can also mark each one as required `+field1:value1 +field2:value2` - `field1` **must have** `value1` **and** `field2` **must have** `value2` for the opponent to match.

You can use the same syntax to match any value type like strings (`field:foo` - no need for quotes) and numbers (`field:5`, or `field:>=5` for ranges).

See the [Bleve query string syntax](http://www.blevesearch.com/docs/Query-String-Query/) for the complete set of syntax and options available.

!!! Tip
    The simplest practical query is `*` - the matchmaker will allow any opponents to match together.

You can find opponents based on a mix of property filters with exact matches or ranges of values. This example searches for opponents that **must** be in `europe` and **must** have a `rank` between 5 and 10, inclusive:

```js fct_label="JavaScript"
const message = { matchmaker_add: {
  min_count: 2,
  max_count: 4,
  query: "+properties.region:europe +properties.rank:>=5 +properties.rank:<=10",
  string_properties: {
    region: "europe"
  },
  numeric_properties: {
    rank: 8
  }
}};
var ticket = await socket.send(message);
```

```csharp fct_label=".NET"
var query = "+properties.region:europe +properties.rank:>=5 +properties.rank:<=10"
var minCount = 2;
var maxCount = 4;
var stringProperties = new Dictionary<string, string>(){
  {"region", "europe"}
};
var numericProperties = new Dictionary<string, int>(){
  {"rank", 8}
};

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount, stringProperties, numericProperties);
```

```csharp fct_label="Unity"
var query = "+properties.region:europe +properties.rank:>=5 +properties.rank:<=10"
var minCount = 2;
var maxCount = 4;
var stringProperties = new Dictionary<string, string>(){
  {"region", "europe"}
};
var numericProperties = new Dictionary<string, int>(){
  {"rank", 8}
};

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount, stringProperties, numericProperties);
```

Or use the wildcard query `"*"` to ignore opponents properties and match with anyone:

```js fct_label="JavaScript"
const message = { matchmaker_add: {
  min_count: 2,
  max_count: 4,
  query: "*",
  string_properties: {
    region: "europe"
  },
  numeric_properties: {
    rank: 8
  }
}};
var ticket = await socket.send(message);
```

```csharp fct_label=".NET"
var query = "*"
var minCount = 2;
var maxCount = 4;
var stringProperties = new Dictionary<string, string>(){
  {"region", "europe"}
};
var numericProperties = new Dictionary<string, int>(){
  {"rank", 8}
};

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount, stringProperties, numericProperties);
```

```csharp fct_label="Unity"
var query = "*"
var minCount = 2;
var maxCount = 4;
var stringProperties = new Dictionary<string, string>(){
  {"region", "europe"}
};
var numericProperties = new Dictionary<string, int>(){
  {"rank", 8}
};

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount, stringProperties, numericProperties);
```

### Minimum and maximum count

Users wishing to matchmake must specify a minimum and maximum number of opponents the matchmaker must find to succeed. If there aren't enough users that match the query to satisfy the minimum count, the user remains in the pool.

The minimum and maximum count includes the user searching for opponents, so to find 3 other opponents the user submits a count of 4. Minimum count must be 2 or higher and the maximum count must be equal to or greater than the minimum count (`max_count >= min_count >= 2`).

If the counts define a range, the matchmaker will try to return the max opponents possible but will never return less than the minimum count:

```js fct_label="JavaScript"
const message = { matchmaker_add: {
  min_count: 2,
  max_count: 4,
  query: "*"
}};
var ticket = await socket.send(message);
```

```csharp fct_label=".NET"
var query = "*"
var minCount = 2;
var maxCount = 4;

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
```

```csharp fct_label="Unity"
var query = "*"
var minCount = 2;
var maxCount = 4;

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
```

To search for an exact number of opponents submit the same minimum and maximum count:

```js fct_label="JavaScript"
const message = { matchmaker_add: {
  min_count: 4,
  max_count: 4,
  query: "*"
}};
var ticket = await socket.send(message);
```

```csharp fct_label=".NET"
var query = "*"
var minCount = 4;
var maxCount = 4;

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
```

```csharp fct_label="Unity"
var query = "*"
var minCount = 4;
var maxCount = 4;

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
```

## Matchmaker tickets

Each time a user is added to the matchmaker pool they receive a ticket, a unique identifier for their entry into the pool.

```js fct_label="JavaScript"
const message = { matchmaker_add: {
  min_count: 2,
  max_count: 4,
  query: "*"
}};
var ticket = await socket.send(message);
```

```csharp fct_label=".NET"
var query = "*"
var minCount = 2;
var maxCount = 4;

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
```

```csharp fct_label="Unity"
var query = "*"
var minCount = 2;
var maxCount = 4;

var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
```

This ticket is used when the server notifies the client on matching success. It distinguishes between multiple possible matchmaker operations for the same user. The user may also cancel the matchmaking process using the ticket at any time, but only before the ticket has been fulfilled.

## Remove a user from the matchmaker

If a user decides they no longer wish to matchmake without disconnecting they can gracefully cancel the matchmaker process by removing themselves from the pool.

```js fct_label="JavaScript"
// `ticket` is returned by matchmaker add operations.
const message = {
  matchmaker_remove: {
    ticket: ticket
  }
}
socket.send(message);
```

```csharp fct_label=".NET"
// `matchmakerTicket` is returned by matchmaker add operations.
socket.RemoveMatchmakerAsync(matchmakerTicket);
```

```csharp fct_label="Unity"
// `matchmakerTicket` is returned by matchmaker add operations.
socket.RemoveMatchmakerAsync(matchmakerTicket);
```

If the user has multiple entries in the matchmaker only the one identified by the ticket will be removed.

## Receive matchmaker results

Matchmaking is not always an instant process. Depending on the currently connected users the matchmaker may take time to complete and will return the resulting list of opponents asynchronously.

Clients should register an event handler that triggers when the server sends them a matchmaker result.

```js fct_label="JavaScript"
socket.onmatchmakematched = (matched) => {
  console.info("Matchmaking complete for ticket: ", matched.ticket);
  console.info("Full list of opponents: ", matched.users);
};
```

```csharp fct_label=".NET"
socket.OnMatchmakerMatched += (_, matched) =>
{
  Debug.LogFormat("Matchmaking complete for ticket: {0}", matched.Ticket);
  Debug.LogFormat("Full list of opponents: {0}", matched.Users);
};
```

```csharp fct_label="Unity"
socket.OnMatchmakerMatched += (_, matched) =>
{
  Debug.LogFormat("Matchmaking complete for ticket: {0}", matched.Ticket);
  Debug.LogFormat("Full list of opponents: {0}", matched.Users);
};
```

## Join a match

It's common to use the matchmaker result event as a way to join a [realtime match](gameplay-multiplayer-realtime.md) with the matched opponents.

Each matchmaker result event carries a token that can be used to join a match together with the matched opponents. The token enables the server to know that these users wanted to play together and will create a match dynamically for them.

Tokens are short-lived and must be used to join a match as soon as possible. When a token expires it can no longer be used or refreshed.

The match token is also used to prevent unwanted users from attempting to join a match they were not matched into. The rest of the multiplayer match code is the same as in the [realtime multiplayer section](gameplay-multiplayer-realtime.md).

```js fct_label="JavaScript"
socket.onmatchmakematched = (matched) => {
  console.info("Matchmaking complete for ticket: ", matched.ticket);

  const message = {
    match_join: {
      token: matched.token
    }
  };
  socket.send(message);
};
```

```csharp fct_label=".NET"
socket.OnMatchmakerMatched += (_, matched) =>
{
  Debug.LogFormat("Matchmaking complete for ticket: {0}", matched.Ticket);
  
  await socket.JoinMatchAsync(matched);
};
```

```csharp fct_label="Unity"
socket.OnMatchmakerMatched += (_, matched) =>
{
  Debug.LogFormat("Matchmaking complete for ticket: {0}", matched.Ticket);
  
  await socket.JoinMatchAsync(matched);
};
```
