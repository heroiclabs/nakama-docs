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

=== "JavaScript"
	```js
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

=== ".NET"
	```csharp
	var query = "*";
	var minCount = 2;
	var maxCount = 4;
	var stringProperties = new Dictionary<string, string>() {
	  {"region", "europe"}
	};
	var numericProperties = new Dictionary<string, int>() {
	  {"rank", 8}
	};
	var matchmakerTicket = await socket.AddMatchmakerAsync(
	    query, minCount, maxCount, stringProperties, numericProperties);
	```

=== "Unity"
	```csharp
	var query = "*";
	var minCount = 2;
	var maxCount = 4;
	var stringProperties = new Dictionary<string, string>() {
	  {"region", "europe"}
	};
	var numericProperties = new Dictionary<string, int>() {
	  {"rank", 8}
	};
	var matchmakerTicket = await socket.AddMatchmakerAsync(
	    query, minCount, maxCount, stringProperties, numericProperties);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](const NMatchmakerTicket& ticket)
	{
	  CCLOG("Matchmaker ticket: %s", ticket.ticket.c_str());
	};
	
	int32_t minCount = 2;
	int32_t maxCount = 4;
	string query = "*";
	NStringMap stringProperties;
	NStringDoubleMap numericProperties;
	
	stringProperties.emplace("region", "europe");
	numericProperties.emplace("rank", 8.0);
	
	rtClient->addMatchmaker(
	    minCount,
	    maxCount,
	    query,
	    stringProperties,
	    numericProperties,
	    successCallback);
	```

=== "Cocos2d-x JS"
	```js
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
	socket.send(message)
	  .then(function(ticket) {
	      cc.log("matchmaker ticket:", JSON.stringify(ticket));
	    },
	    function(error) {
	      cc.error("matchmaker add failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](const NMatchmakerTicket& ticket)
	{
	  std::cout << "Matchmaker ticket: " << ticket.ticket << std::endl;
	};
	
	int32_t minCount = 2;
	int32_t maxCount = 4;
	string query = "*";
	NStringMap stringProperties;
	NStringDoubleMap numericProperties;
	
	stringProperties.emplace("region", "europe");
	numericProperties.emplace("rank", 8.0);
	
	rtClient->addMatchmaker(
	    minCount,
	    maxCount,
	    query,
	    stringProperties,
	    numericProperties,
	    successCallback);
	```

=== "Java"
	```java
	String query = "*";
	int minCount = 2;
	int maxCount = 4;
	Map<String, String> stringProperties = new HashMap<String, String>() {{
	  put("region", "europe");
	}};
	Map<String, Double> numericProperties = new HashMap<String, Double>() {{
	  put("rank", 8.0);
	}};
	
	MatchmakerTicket matchmakerTicket = socket.addMatchmaker(
	    query, minCount, maxCount, stringProperties, numericProperties).get();
	```

=== "Godot"
	```gdscript
	var query = "*"
	var min_count = 2
	var max_count = 4
	var string_properties = { "region": "europe" }
	var numeric_properties = { "rank": 8 }
	var matchmaker_ticket : NakamaRTAPI.MatchmakerTicket = yield(
		socket.add_matchmaker_async(query, min_count, max_count, string_properties, numeric_properties),
		"completed"
	)
	if matchmaker_ticket.is_exception():
		print("An error occured: %s" % matchmaker_ticket)
		return
	print("Got ticket: %s" % [matchmaker_ticket])
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

=== "JavaScript"
	```js
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

=== ".NET"
	```csharp
	var query = "+properties.region:europe +properties.rank:>=5 +properties.rank:<=10";
	var stringProperties = new Dictionary<string, string>() {
	  {"region", "europe"}
	};
	var numericProperties = new Dictionary<string, int>() {
	  {"rank", 8}
	};
	var matchmakerTicket = await socket.AddMatchmakerAsync(
	    query, 2, 4, stringProperties, numericProperties);
	```

=== "Unity"
	```csharp
	var query = "+properties.region:europe +properties.rank:>=5 +properties.rank:<=10";
	var stringProperties = new Dictionary<string, string>() {
	  {"region", "europe"}
	};
	var numericProperties = new Dictionary<string, int>() {
	  {"rank", 8}
	};
	var matchmakerTicket = await socket.AddMatchmakerAsync(
	    query, 2, 4, stringProperties, numericProperties);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](const NMatchmakerTicket& ticket)
	{
	  CCLOG("Matchmaker ticket: %s", ticket.ticket.c_str());
	};
	
	int32_t minCount = 2;
	int32_t maxCount = 4;
	string query = "+properties.region:europe +properties.rank:>=5 +properties.rank:<=10";
	NStringMap stringProperties;
	NStringDoubleMap numericProperties;
	
	stringProperties.emplace("region", "europe");
	numericProperties.emplace("rank", 8.0);
	
	rtClient->addMatchmaker(
	    minCount,
	    maxCount,
	    query,
	    stringProperties,
	    numericProperties,
	    successCallback);
	```

=== "Cocos2d-x JS"
	```js
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
	socket.send(message)
	  .then(function(ticket) {
	      cc.log("matchmaker ticket:", JSON.stringify(ticket));
	    },
	    function(error) {
	      cc.error("matchmaker add failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](const NMatchmakerTicket& ticket)
	{
	  std::cout << "Matchmaker ticket: " << ticket.ticket << std::endl;
	};
	
	int32_t minCount = 2;
	int32_t maxCount = 4;
	string query = "+properties.region:europe +properties.rank:>=5 +properties.rank:<=10";
	NStringMap stringProperties;
	NStringDoubleMap numericProperties;
	
	stringProperties.emplace("region", "europe");
	numericProperties.emplace("rank", 8.0);
	
	rtClient->addMatchmaker(
	    minCount,
	    maxCount,
	    query,
	    stringProperties,
	    numericProperties,
	    successCallback);
	```

=== "Java"
	```java
	String query = "+properties.region:europe +properties.rank:>=5 +properties.rank:<=10";
	int minCount = 2;
	int maxCount = 4;
	Map<String, String> stringProperties = new HashMap<String, String>() {{
	  put("region", "europe");
	}};
	Map<String, Double> numericProperties = new HashMap<String, Double>() {{
	  put("rank", 8.0);
	}};
	
	MatchmakerTicket matchmakerTicket = socket.addMatchmaker(
	    query, minCount, maxCount, stringProperties, numericProperties).get();
	```

=== "Godot"
	```gdscript
	var query = "+properties.region:europe +properties.rank:>=5 +properties.rank:<=10"
	var string_properties = { "region": "europe"}
	var numeric_properties = { "rank": 8 }
	var matchmaker_ticket : NakamaRTAPI.MatchmakerTicket = yield(
		socket.add_matchmaker_async(query, 2, 4, string_properties, numeric_properties),
		"completed"
	)
	if matchmaker_ticket.is_exception():
		print("An error occured: %s" % matchmaker_ticket)
		return
	print("Got ticket: %s" % [matchmaker_ticket])
	```

Or use the wildcard query `"*"` to ignore opponents properties and match with anyone:

=== "JavaScript"
	```js
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

=== ".NET"
	```csharp
	var query = "*";
	var minCount = 2;
	var maxCount = 4;
	var stringProperties = new Dictionary<string, string>() {
	  {"region", "europe"}
	};
	var numericProperties = new Dictionary<string, int>() {
	  {"rank", 8}
	};
	var matchmakerTicket = await socket.AddMatchmakerAsync(
	    query, minCount, maxCount, stringProperties, numericProperties);
	```

=== "Unity"
	```csharp
	var query = "*";
	var minCount = 2;
	var maxCount = 4;
	var stringProperties = new Dictionary<string, string>() {
	  {"region", "europe"}
	};
	var numericProperties = new Dictionary<string, int>() {
	  {"rank", 8}
	};
	var matchmakerTicket = await socket.AddMatchmakerAsync(
	    query, minCount, maxCount, stringProperties, numericProperties);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](const NMatchmakerTicket& ticket)
	{
	  CCLOG("Matchmaker ticket: %s", ticket.ticket.c_str());
	};
	
	int32_t minCount = 2;
	int32_t maxCount = 4;
	string query = "*";
	NStringMap stringProperties;
	NStringDoubleMap numericProperties;
	
	stringProperties.emplace("region", "europe");
	numericProperties.emplace("rank", 8.0);
	
	rtClient->addMatchmaker(
	    minCount,
	    maxCount,
	    query,
	    stringProperties,
	    numericProperties,
	    successCallback);
	```

=== "Cocos2d-x JS"
	```js
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
	socket.send(message)
	  .then(function(ticket) {
	      cc.log("matchmaker ticket:", JSON.stringify(ticket));
	    },
	    function(error) {
	      cc.error("matchmaker add failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](const NMatchmakerTicket& ticket)
	{
	  std::cout << "Matchmaker ticket: " << ticket.ticket << std::endl;
	};
	
	int32_t minCount = 2;
	int32_t maxCount = 4;
	string query = "*";
	NStringMap stringProperties;
	NStringDoubleMap numericProperties;
	
	stringProperties.emplace("region", "europe");
	numericProperties.emplace("rank", 8.0);
	
	rtClient->addMatchmaker(
	    minCount,
	    maxCount,
	    query,
	    stringProperties,
	    numericProperties,
	    successCallback);
	```

=== "Java"
	```java
	String query = "*";
	int minCount = 2;
	int maxCount = 4;
	Map<String, String> stringProperties = new HashMap<String, String>() {{
	  put("region", "europe");
	}};
	Map<String, Double> numericProperties = new HashMap<String, Double>() {{
	  put("rank", 8.0);
	}};
	
	MatchmakerTicket matchmakerTicket = socket.addMatchmaker(
	    query, minCount, maxCount, stringProperties, numericProperties).get();
	```

=== "Godot"
	```gdscript
	var query = "*"
	var min_count = 2
	var max_count = 4
	var string_properties = { "region": "europe" }
	var numeric_properties = { "rank": 8 }
	var matchmaker_ticket : NakamaRTAPI.MatchmakerTicket = yield(
		socket.add_matchmaker_async(query, min_count, max_count, string_properties, numeric_properties),
		"completed"
	)
	if matchmaker_ticket.is_exception():
		print("An error occured: %s" % matchmaker_ticket)
		return
	print("Got ticket: %s" % [matchmaker_ticket])
	```

### Minimum and maximum count

Users wishing to matchmake must specify a minimum and maximum number of opponents the matchmaker must find to succeed. If there aren't enough users that match the query to satisfy the minimum count, the user remains in the pool.

The minimum and maximum count includes the user searching for opponents, so to find 3 other opponents the user submits a count of 4. Minimum count must be 2 or higher and the maximum count must be equal to or greater than the minimum count (`max_count >= min_count >= 2`).

If the counts define a range, the matchmaker will try to return the max opponents possible but will never return less than the minimum count:

=== "JavaScript"
	```js
	const message = { matchmaker_add: {
	  min_count: 2,
	  max_count: 4,
	  query: "*"
	}};
	var ticket = await socket.send(message);
	```

=== ".NET"
	```csharp
	var query = "*";
	var minCount = 2;
	var maxCount = 4;
	var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
	```

=== "Unity"
	```csharp
	var query = "*";
	var minCount = 2;
	var maxCount = 4;
	var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
	```

=== "Java"
	```java
	String query = "*";
	int minCount = 2;
	int maxCount = 4;
	MatchmakerTicket matchmakerTicket = socket.addMatchmaker(query, minCount, maxCount).get();
	```

To search for an exact number of opponents submit the same minimum and maximum count:

=== "JavaScript"
	```js
	const message = { matchmaker_add: {
	  min_count: 4,
	  max_count: 4,
	  query: "*"
	}};
	var ticket = await socket.send(message);
	```

=== ".NET"
	```csharp
	var query = "*";
	var minCount = 4;
	var maxCount = 4;
	var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
	```

=== "Unity"
	```csharp
	var query = "*";
	var minCount = 4;
	var maxCount = 4;
	var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](const NMatchmakerTicket& ticket)
	{
	  CCLOG("Matchmaker ticket: %s", ticket.ticket.c_str());
	};
	
	int32_t minCount = 2;
	int32_t maxCount = 4;
	string query = "*";
	
	rtClient->addMatchmaker(
	    minCount,
	    maxCount,
	    query,
	    {},
	    {},
	    successCallback);
	```

=== "Cocos2d-x JS"
	```js
	const message = { matchmaker_add: {
	  min_count: 2,
	  max_count: 4,
	  query: "*"
	}};
	socket.send(message)
	  .then(function(ticket) {
	      cc.log("matchmaker ticket:", JSON.stringify(ticket));
	    },
	    function(error) {
	      cc.error("matchmaker add failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](const NMatchmakerTicket& ticket)
	{
	  std::cout << "Matchmaker ticket: " << ticket.ticket << std::endl;
	};
	
	int32_t minCount = 2;
	int32_t maxCount = 4;
	string query = "*";
	
	rtClient->addMatchmaker(
	    minCount,
	    maxCount,
	    query,
	    {},
	    {},
	    successCallback);
	```

=== "Java"
	```java
	String query = "*";
	int minCount = 4;
	int maxCount = 4;
	
	MatchmakerTicket matchmakerTicket = socket.addMatchmaker(query, minCount, maxCount).get();
	```

=== "Godot"
	```gdscript
	var query = "*"
	var min_count = 4
	var max_count = 4
	var matchmaker_ticket : NakamaRTAPI.MatchmakerTicket = yield(
		socket.add_matchmaker_async(query, min_count, max_count),
		"completed"
	)
	if matchmaker_ticket.is_exception():
		print("An error occured: %s" % matchmaker_ticket)
		return
	print("Got ticket: %s" % [matchmaker_ticket])
	```

## Matchmaker tickets

Each time a user is added to the matchmaker pool they receive a ticket, a unique identifier for their entry into the pool.

=== "JavaScript"
	```js
	const message = { matchmaker_add: {
	  min_count: 2,
	  max_count: 4,
	  query: "*"
	}};
	var ticket = await socket.send(message);
	```

=== ".NET"
	```csharp
	var query = "*";
	var minCount = 2;
	var maxCount = 4;
	var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
	```

=== "Unity"
	```csharp
	var query = "*";
	var minCount = 2;
	var maxCount = 4;
	var matchmakerTicket = await socket.AddMatchmakerAsync(query, minCount, maxCount);
	```

=== "Cocos2d-x C++"
	```cpp
	auto successCallback = [](const NMatchmakerTicket& ticket)
	{
	  CCLOG("Matchmaker ticket: %s", ticket.ticket.c_str());
	};
	
	int32_t minCount = 2;
	int32_t maxCount = 4;
	string query = "*";
	
	rtClient->addMatchmaker(
	    minCount,
	    maxCount,
	    query,
	    {},
	    {},
	    successCallback);
	```

=== "Cocos2d-x JS"
	```js
	const message = { matchmaker_add: {
	  min_count: 2,
	  max_count: 4,
	  query: "*"
	}};
	socket.send(message)
	  .then(function(ticket) {
	      cc.log("matchmaker ticket:", JSON.stringify(ticket));
	    },
	    function(error) {
	      cc.error("matchmaker add failed:", JSON.stringify(error));
	    });
	```

=== "C++"
	```cpp
	auto successCallback = [](const NMatchmakerTicket& ticket)
	{
	  std::cout << "Matchmaker ticket: " << ticket.ticket << std::endl;
	};
	
	int32_t minCount = 2;
	int32_t maxCount = 4;
	string query = "*";
	
	rtClient->addMatchmaker(
	    minCount,
	    maxCount,
	    query,
	    {},
	    {},
	    successCallback);
	```

=== "Java"
	```java
	String query = "*";
	int minCount = 2;
	int maxCount = 4;
	
	MatchmakerTicket matchmakerTicket = socket.addMatchmaker(query, minCount, maxCount).get();
	```

=== "Godot"
	```gdscript
	var query = "*"
	var min_count = 2
	var max_count = 4
	var matchmaker_ticket : NakamaRTAPI.MatchmakerTicket = yield(
		socket.add_matchmaker_async(query, min_count, max_count),
		"completed"
	)
	if matchmaker_ticket.is_exception():
		print("An error occured: %s" % matchmaker_ticket)
		return
	print("Got ticket: %s" % [matchmaker_ticket])
	```

This ticket is used when the server notifies the client on matching success. It distinguishes between multiple possible matchmaker operations for the same user. The user may also cancel the matchmaking process using the ticket at any time, but only before the ticket has been fulfilled.

## Remove a user from the matchmaker

If a user decides they no longer wish to matchmake without disconnecting they can gracefully cancel the matchmaker process by removing themselves from the pool.

=== "JavaScript"
	```js
	// "ticket" is returned by the matchmaker.
	const message = {
	  matchmaker_remove: {
	    ticket: ticket
	  }
	}
	socket.send(message);
	```

=== ".NET"
	```csharp
	// "matchmakerTicket" is returned by the matchmaker.
	await socket.RemoveMatchmakerAsync(matchmakerTicket);
	```

=== "Unity"
	```csharp
	// "matchmakerTicket" is returned by the matchmaker.
	await socket.RemoveMatchmakerAsync(matchmakerTicket);
	```

=== "Cocos2d-x C++"
	```cpp
	// "ticket" is returned by the matchmaker.
	rtClient->removeMatchmaker(ticket, []()
	  {
	    CCLOG("removed from Matchmaker");
	  });
	```

=== "Cocos2d-x JS"
	```js
	// "ticket" is returned by the matchmaker.
	const message = {
	  matchmaker_remove: {
	    ticket: ticket
	  }
	}
	socket.send(message);
	```

=== "C++"
	```cpp
	// "ticket" is returned by the matchmaker.
	rtClient->removeMatchmaker(ticket, []()
	  {
	    std::cout << "removed from Matchmaker" << std::endl;
	  });
	```

=== "Java"
	```java
	// "matchmakerTicket" is returned by the matchmaker.
	socket.removeMatchmaker(matchmakerTicket.getTicket()).get();
	```

=== "Godot"
	```gdscript
	var removed : NakamaAsyncResult = yield(socket.remove_matchmaker_async(matchmaker_ticket.ticket), "completed")
	if removed.is_exception():
		print("An error occured: %s" % removed)
		return
	print("Removed from matchmaking %s" % [matchmaker_ticket.ticket])
	```

If the user has multiple entries in the matchmaker only the one identified by the ticket will be removed.

## Receive matchmaker results

Matchmaking is not always an instant process. Depending on the currently connected users the matchmaker may take time to complete and will return the resulting list of opponents asynchronously.

Clients should register an event handler that triggers when the server sends them a matchmaker result.

=== "JavaScript"
	```js
	socket.onmatchmakermatched = (matched) => {
	  console.info("Received MatchmakerMatched message: ", matched);
	  console.info("Matched opponents: ", matched.users);
	};
	```

=== ".NET"
	```csharp
	socket.ReceivedMatchmakerMatched += matched =>
	{
	    Console.WriteLine("Received: {0}", matched);
	    var opponents = string.Join(",\n  ", matched.Users); // printable list.
	    Console.WriteLine("Matched opponents: [{0}]", opponents);
	};
	```

=== "Unity"
	```csharp
	socket.ReceivedMatchmakerMatched += matched =>
	{
	    Debug.LogFormat("Received: {0}", matched);
	    var opponents = string.Join(",\n  ", matched.Users); // printable list.
	    Debug.LogFormat("Matched opponents: [{0}]", opponents);
	};
	```

=== "Cocos2d-x C++"
	```cpp
	rtListener->setMatchmakerMatchedCallback([](NMatchmakerMatchedPtr matched)
	  {
	    CCLOG("Matched! matchId: %s", matched->matchId.c_str());
	  });
	```

=== "Cocos2d-x JS"
	```js
	this.socket.onmatchmakermatched = (matched) => {
	  cc.log("Received MatchmakerMatched message:", JSON.stringify(matched));
	  cc.log("Matched opponents:", matched.users.toString());
	};
	```

=== "C++"
	```cpp
	rtListener->setMatchmakerMatchedCallback([](NMatchmakerMatchedPtr matched)
	  {
	    std::cout << "Matched! matchId: " << matched->matchId << std::endl;
	  });
	```

=== "Java"
	```java
	SocketListener listener = new AbstractSocketListener() {
	  @Override
	  public void onMatchmakerMatched(final MatchmakerMatched matched) {
	    System.out.format("Received MatchmakerMatched message: %s", matched.toString());
	    System.out.format("Matched opponents: %s", opponents.toString());
	  }
	};
	```

=== "Godot"
	```gdscript
	func _ready():
		# First, setup the socket as explained in the authentication section.
		socket.connect("received_matchmaker_matched", self, "_on_matchmaker_matched")
	
	func _on_matchmaker_matched(p_matched : NakamaRTAPI.MatchmakerMatched):
		print("Received MatchmakerMatched message: %s" % [p_matched])
		print("Matched opponents: %s" % [p_matched.users])
	```

## Join a match

It's common to use the matchmaker result event as a way to join a [realtime match](gameplay-multiplayer-realtime.md) with the matched opponents.

Each matchmaker result event carries a token that can be used to join a match together with the matched opponents. The token enables the server to know that these users wanted to play together and will create a match dynamically for them.

Tokens are short-lived and must be used to join a match as soon as possible. When a token expires it can no longer be used or refreshed.

The match token is also used to prevent unwanted users from attempting to join a match they were not matched into. The rest of the multiplayer match code is the same as in the [realtime multiplayer section](gameplay-multiplayer-realtime.md).

=== "JavaScript"
	```js
	socket.onmatchmakermatched = (matched) => {
	  console.info("Received MatchmakerMatched message: ", matched);
	  const message = {
	    match_join: {
	      token: matched.token
	    }
	  };
	  socket.send(message);
	};
	```

=== ".NET"
	```csharp
	socket.ReceivedMatchmakerMatched += async matched =>
	{
	    Console.WriteLine("Received: {0}", matched);
	    await socket.JoinMatchAsync(matched);
	};
	```

=== "Unity"
	```csharp
	socket.ReceivedMatchmakerMatched += async matched =>
	{
	    Debug.LogFormat("Received: {0}", matched);
	    await socket.JoinMatchAsync(matched);
	};
	```

=== "Cocos2d-x C++"
	```cpp
	rtListener->setMatchmakerMatchedCallback([this](NMatchmakerMatchedPtr matched)
	  {
	    CCLOG("Matched! token: %s", matched->token.c_str());
	
	    rtClient->joinMatchByToken(matched->token, [](const NMatch& match)
	      {
	        CCLOG("Joined Match!");
	      });
	  });
	```

=== "Cocos2d-x JS"
	```js
	this.socket.onmatchmakermatched = (matched) => {
	  cc.log("Received MatchmakerMatched message:", JSON.stringify(matched));
	  cc.log("Matched opponents:", matched.users.toString());
	  const message = {
	    match_join: {
	      token: matched.token
	    }
	  };
	  socket.send(message);
	};
	```

=== "C++"
	```cpp
	rtListener->setMatchmakerMatchedCallback([this](NMatchmakerMatchedPtr matched)
	  {
	    std::cout << "Matched! token: " << matched->token << std::endl;
	
	    rtClient->joinMatchByToken(matched->token, [](const NMatch& match)
	      {
	        std::cout << "Joined Match!" << std::endl;
	      });
	  });
	```

=== "Java"
	```java
	SocketListener listener = new AbstractSocketListener() {
	  @Override
	  public void onMatchmakerMatched(final MatchmakerMatched matched) {
	    socket.joinMatchToken(matched.getToken()).get();
	  }
	};
	```

=== "Godot"
	```gdscript
	func _on_matchmaker_matched(p_matched : NakamaRTAPI.MatchmakerMatched):
		print("Received MatchmakerMatched message: %s" % [p_matched])
		var joined_match : NakamaRTAPI.Match = yield(socket.join_matched_async(p_matched), "completed")
		if joined_match.is_exception():
			print("An error occured: %s" % joined_match)
			return
		print("Joined match: %s" % [joined_match])
	```
