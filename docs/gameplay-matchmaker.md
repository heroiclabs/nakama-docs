# Matchmaker

In realtime and turn-based games it's important to be able to find active opponents to play against. A matchmaker system is designed to provide this kind of behavior.

In the server we've taken the design further and decoupled how you're matched from the realtime multiplayer engine. This makes it easy to use the matchmaker system to find opponents even if the gameplay isn't realtime. It could be a casual social game where you want to find random new users to become friends with or an asynchronous PvP game where gameplay happens in a simulated battle.

The matchmaker receives and tracks matchmaking requests, then groups users together based on the criteria they've expressed in their properties and filters.

## Properties and filters

The matchmaking system has the concept of **Properties** and **Filters**, which clients send to the server to decide how their teammates or opponents are selected.

**Properties** are key-value pairs that describe the user's state in the game. Rank information, connecting region information, or selected match types are good examples of data that should be stored in Properties. When matchmaking completes these properties are visible to the matched users, so if needed you can store extra information without affecting the matchmaking process itself if it's useful to clients.

These properties define information about the user. Good examples of properties are "skill rating = 150" or "player level = 20".

**Filters** are criteria definitions the server will check against the properties of other users in the matchmaking pool. These determine if the user would accept being matched with others.

Filters define information about how the user wants their matched users to be selected. Good examples of filters are "skill rating 100 to 200" or "player level 15 to 25" indicating the user is looking for others that have properties in the given ranges.

The matchmaker can filter users using 4 different filtering criteria:

- User Count: The exact number of total users required to form a match. This is a required parameter that determines when the matchmaking algorithm has found enough users to satisfy the matchmaking request.
- Term Filter: String terms that must match. You can set this filter to match any of the supplied terms, or all the terms.
- Numeric Range Filter: An integer filter with lowerbound and upperbound requirements. If the property value is not within the specified range, the user is filtered out.
- Boolean check filters: A simple boolean check.

A matchmaking request can have complex criteria by using a combination of these filters. Once the matchmaker is successful, the properties and filters of each matched user will be redistributed to every matched user.

!!! Tip
    The [Rule-based matchmaking](#rule-based-matchmaking) section has details on how each filter type works and how to compose filters to achieve complex matchmaking rules.

## Request opponents

The server maintains a pool of users who've requested to be matched together with a specific number of opponents and each user's properties and filters if specified. Each user can add themselves to the matchmaker pool and register an event handler to be notified when enough users meet their criteria to be matched.

You can send a message to add the user to the matchmaker pool.

```csharp fct_label="Unity"
INMatchmakeTicket matchmake = null;

// Look for a match for two participants. Yourself and one more.
var message = NMatchmakeAddMessage.Default(2);
client.Send(message, (INMatchmakeTicket result) => {
  Debug.Log("Added user to matchmaker pool.");

  var cancelTicket = result.Ticket;
  Debug.LogFormat("The cancellation code {0}", cancelTicket);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```js fct_label="Javascript"
var matchmakeTicket = null;

// Look for a match for two participants. Yourself and one more.
var message = new nakamajs.MatchmakeAddRequest(2);
client.send(message).then(function(ticket) {
  console.log("Added user to matchmaker pool.");
  matchmakeTicket = ticket.ticket;
  console.log("The cancellation ticket is %o", matchmakeTicket);
}).catch(function(error){
  console.log("An error occured: %o", error);
});
```

The message returns a ticket which can be used to cancel the matchmake attempt. A user can remove themselves from the pool if wanted. This is useful for some games where a user can cancel their action to matchmake at some later point and remove themselves being matched with other users.

Have a look at [this section](#rule-based-matchmaking) for more advanced matchmaking examples.

### Receive matchmake results

You can register an event handler which is called when the server has found opponents for the user.

```csharp fct_label="Unity"
client.OnMatchmakeMatched = (INMatchmakeMatched matched) => {
  // a match token is used to join the match.
  Debug.LogFormat("Match token: '{0}'", matched.Token);

  // a list of users who've been matched as opponents.
  foreach (var presence in matched.Presence) {
    Debug.LogFormat("User id: '{0}'.", presence.UserId);
    Debug.LogFormat("User handle: '{0}'.", presence.Handle);
  }

  // list of all match properties
  foreach (var userProperty in matched.UserProperties) {
    foreach(KeyValuePair<string, string> entry in userProperty.Properties) {
      Debug.LogFormat("Property '{0}' for user '{1}' has value '{2}'.", entry.Key, userProperty.Id, entry.Value);
    }

    foreach(KeyValuePair<string, INMatchmakeFilter> entry in userProperty.Filters) {
      Debug.LogFormat("Filter '{0}' for user '{1}' has value '{2}'.", entry.Key, userProperty.Id, entry.Value.ToString());
    }
  }
};
```

```js fct_label="Javascript"
client.onmatchmakematched = function(matched) {
  // a match token is used to join the match.
  console.log("Match token: %o", matched.token);

  // a list of users who've been matched as opponents.
  matched.presences.forEach(function(presence) {
    console.log("User id: %o.", presence.userId);
    console.log("User handle: %o.", presence.handle);
  });

  // list of all match properties
  matched.properties.forEach(function(property) {
    console.log("User %o has properties: %o", property.userId, property.properties);
    console.log("User %o has filter: %o", property.userId, property.filters);
  });
};
```

## Rule-based matchmaking

In the example above, only the required user count was set. This is the most loose matchmaking request that will select users randomly. Below we'll look at more complex matchmaking requests and how they are represented in Nakama.

We'll use a popular car racing game as an example to demonstrate how matchmaking can be set up. In this game a player can play as a cop or a racer, and has two different ranks based on whichever character they play. The player can have a custom car with specific boosters which other players will need to know about.

### Using term filters

Term filters use one or more string terms as matching criteria. This is useful to match users who are all interested in certain game types, only want to play certain maps, or battle opponents with specific characteristics.

```csharp fct_label="Unity"
// Look for a match for 8 participants. Yourself and 7 more.
var message = new NMatchmakeAddMessage.Builder(8);

// Set player's properties.
// Whitelist of maps that the player is willing to play.
message.AddProperty("interested-maps", new HashSet<string> {"vegas", "downtown-la"});

// This player is playing as a "cop".
message.AddProperty("player-type", new HashSet<string> {"cop"});

// This player's car boosters - this is only used as extra metadata information.
message.AddProperty("car-boosters", new HashSet<string> {"bullets", "spikes", "emp-shockwave"});

// Let's add the matchmaking criteria.
// Whitelist of maps that the opponents must be willing to play.
message.AddTermFilter("interested-maps", new HashSet<string> {"vegas", "downtown-la", "santa-cruz"}, false);

// The player wants to only play with racers.
message.AddTermFilter("player-type", new HashSet<string> {"racer"}, true);

client.Send(message.Build(), (INMatchmakeTicket ticket) => {
  //... save ticket for later cancellation, if needed.
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```js fct_label="Javascript"
// Look for a match for 8 participants. Yourself and 7 more.
var message = new nakamajs.MatchmakeAddRequest(8);

// Set player's properties.
// Whitelist of maps that the player is willing to play.
message.addStringSetProperty("interested-maps", ["vegas", "downtown-la"]);

// This player is playing as a "cop".
message.addStringSetProperty("player-type", "cop");

// This player's car boosters - this is only used as extra metadata information.
message.addStringSetProperty("car-boosters", ["bullets", "spikes", "emp-shockwave"]);

// Let's add the matchmaking criteria.
// Whitelist of maps that the opponents must be willing to play.
message.addTermFilter("interested-maps", ["vegas", "downtown-la", "santa-cruz"], false);

// The player wants to only play with racers.
message.addTermFilter("player-type", ["racer"], true);

client.send(message).then(function(ticket) {
  //... save ticket for later cancellation, if needed.
}).catch(function(error){
  console.log("An error occured: %o", error);
});
```

### Using range filters

Range filters operate on an inclusive lowerbound and an inclusive upperbound integer. The property should be within the two numbers for the matchmaking candidate to be considered.

```csharp fct_label="Unity"
// Look for a match for 2 participants. Yourself and someone else.
var message = new NMatchmakeAddMessage.Builder(2);

// Set the player's cop rank.
message.AddProperty("cop-rank", 12);
message.AddProperty("racer-rank", 41);

// Look for players with between given ranks, to ensure the match is fair.
message.AddRangeFilter("cop-rank", 8, 12);
message.AddRangeFilter("racer-rank", 30, 50);

client.Send(message.Build(), (INMatchmakeTicket ticket) => {
  // ... save ticket for later cancellation, if needed.
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```js fct_label="Javascript"
// Look for a match for 2 participants. Yourself and someone else.
var message = new nakamajs.MatchmakeAddRequest(2);

// Set the player's cop rank.
message.addIntegerProperty("cop-rank", 12);
message.addIntegerProperty("racer-rank", 41);

// Look for players with between given ranks, to ensure the match is fair.
message.addRangeFilter("cop-rank", 8, 12);
message.addRangeFilter("racer-rank", 30, 50);

client.send(message).then(function(ticket) {
  //... save ticket for later cancellation, if needed.
}).catch(function(error){
  console.log("An error occured: %o", error);
});
```

### Multi-filter matchmaking

You can of course mix and match various filters to enhance the matchmaking search.

```csharp fct_label="Unity"
// Look for a match for 4 participants. Yourself and 3 other users.
var message = new NMatchmakeAddMessage.Builder(4);

// This player is playing as a "cop".
message.AddProperty("player-type", new HashSet<string> {"cop"});

// Set the player's ranks.
message.AddProperty("cop-rank", 12);
message.AddProperty("racer-rank", 41);

// Set a boolean property on whether the player has prestiged or not.
message.AddProperty("has-prestiged", true);

// The player wants to only play with either racers or cops.
message.AddTermFilter("player-type", new HashSet<string> {"racer", "cop"}, false);

// Look for players with between two ranks, to ensure the match is fair.
message.AddRangeFilter("cop-rank", 8, 12);
message.AddRangeFilter("racer-rank", 40, 45);

// Look only for players that have prestiged.
message.AddCheckFilter("has-prestiged", true)

client.Send(message.Build(), (INMatchmakeTicket ticket) => {
  //... save ticket for later cancellation, if needed.
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```js fct_label="Javascript"
// Look for a match for 3 participants. Yourself and 3 more.
var message = new nakamajs.MatchmakeAddRequest(4);

// This player is playing as a "cop".
message.addStringSetProperty("player-type", "cop");

// Set the player's ranks.
message.addIntegerProperty("cop-rank", 12);
message.addIntegerProperty("racer-rank", 41);

// Set a boolean property on whether the player has prestiged or not.
message.addBooleanProperty("has-prestiged", true);

// The player wants to only play with either racers or cops.
message.addTermFilter("player-type", ["racer", "cop"], false);

// Look for players with between two ranks, to ensure the match is fair.
message.addRangeFilter("cop-rank", 8, 12);
message.addRangeFilter("racer-rank", 40, 45);

// Look only for players that have prestiged.
message.addCheckFilter("has-prestiged", true)

client.send(message).then(function(ticket) {
  //... save ticket for later cancellation, if needed.
}).catch(function(error){
  console.log("An error occured: %o", error);
});
```

## Cancel a request

After a user has sent a message to add themselves to the matchmaker pool you'll receive a ticket which can be used to cancel the action at some later point.

Users may cancel matchmake actions at any point after they've added themselves to the pool but before they've been matched. Once matching completes for the ticket it can no longer be used to cancel.

```csharp fct_label="Unity"
INMatchmakeTicket cancelTicket = result; // See above.

var message = NMatchmakeRemoveMessage.Default(cancelTicket);
client.Send(message, (bool done) => {
  Debug.Log("User removed from matchmaker pool.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```js fct_label="Javascript"
var matchmakeTicket = result; // See above.

var message = new nakamajs.MatchmakeRemoveRequest(result.ticket);
client.send(message).then(function() {
  console.log("User removed from matchmaker pool.");
}).catch(function(error){
  console.log("An error occured: %o", error);
});
```

The user is now removed from the matchmaker pool.

!!! Note "Removed on disconnect"
    A user will automatically be removed from the matchmaker pool by the server when they disconnect.

## Join a match

To join a match after the event handler has notified the user their criteria is met and they've been given opponents you can use the match token.

```csharp fct_label="Unity"
client.OnMatchmakeMatched = (INMatchmakeMatched matched) => {
  // The match token is used to join a multiplayer match.
  var message = NMatchJoinMessage.Default(matched.Token);
  client.Send(message, (INResultSet<INMatch> matches) => {
    Debug.Log("Successfully joined match.");
  }, (INError error) => {
    Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
  });
};
```

```js fct_label="Javascript"
client.onmatchmakematched = function(matched) {
  // The match token is used to join a multiplayer match.
  var message = new nakamajs.MatchesJoinRequest();
  message.tokens.push(matched.token);
  client.send(message).then(function(matches) {
    console.log("Successfully joined match.");
  }).catch(function(error){
    console.log("An error occured: %o", error);
  });
};
```

The token makes it easy to join a match. The token enables the server to know that these users wanted to join a match and is able to create a match dynamically for them.

Tokens are short-lived and must be used to join a match as soon as possible. When a token expires it can no longer be used or refreshed.

The match token is also used to prevent unwanted users from attempting to join a match they were not matched into. The rest of the multiplayer match code in the same as in the [realtime multiplayer section](gameplay-multiplayer-realtime.md).
