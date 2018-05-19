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

```js fct_label="JavaScript"
// Updated example TBD
```

```csharp fct_label=".Net"
// Updated example TBD
```

```csharp fct_label="Unity"
// Updated example TBD
```

The message returns a ticket which can be used to cancel the matchmake attempt. A user can remove themselves from the pool if wanted. This is useful for some games where a user can cancel their action to matchmake at some later point and remove themselves being matched with other users.

Have a look at [this section](#rule-based-matchmaking) for more advanced matchmaking examples.

### Receive matchmake results

You can register an event handler which is called when the server has found opponents for the user.

```js fct_label="JavaScript"
// Updated example TBD
```

```csharp fct_label=".Net"
// Updated example TBD
```

```csharp fct_label="Unity"
// Updated example TBD
```

## Rule-based matchmaking

In the example above, only the required user count was set. This is the most loose matchmaking request that will select users randomly. Below we'll look at more complex matchmaking requests and how they are represented in Nakama.

We'll use a popular car racing game as an example to demonstrate how matchmaking can be set up. In this game a player can play as a cop or a racer, and has two different ranks based on whichever character they play. The player can have a custom car with specific boosters which other players will need to know about.

### Using term filters

Term filters use one or more string terms as matching criteria. This is useful to match users who are all interested in certain game types, only want to play certain maps, or battle opponents with specific characteristics.

```js fct_label="JavaScript"
// Updated example TBD
```

```csharp fct_label=".Net"
// Updated example TBD
```

```csharp fct_label="Unity"
// Updated example TBD
```

### Using range filters

Range filters operate on an inclusive lowerbound and an inclusive upperbound integer. The property should be within the two numbers for the matchmaking candidate to be considered.

```js fct_label="JavaScript"
// Updated example TBD
```

```csharp fct_label=".Net"
// Updated example TBD
```

```csharp fct_label="Unity"
// Updated example TBD
```

### Multi-filter matchmaking

You can of course mix and match various filters to enhance the matchmaking search.

```js fct_label="JavaScript"
// Updated example TBD
```

```csharp fct_label=".Net"
// Updated example TBD
```

```csharp fct_label="Unity"
// Updated example TBD
```

## Cancel a request

After a user has sent a message to add themselves to the matchmaker pool you'll receive a ticket which can be used to cancel the action at some later point.

Users may cancel matchmake actions at any point after they've added themselves to the pool but before they've been matched. Once matching completes for the ticket it can no longer be used to cancel.

```js fct_label="JavaScript"
// Updated example TBD
```

```csharp fct_label=".Net"
// Updated example TBD
```

```csharp fct_label="Unity"
// Updated example TBD
```

The user is now removed from the matchmaker pool.

!!! Note "Removed on disconnect"
    A user will automatically be removed from the matchmaker pool by the server when they disconnect.

## Join a match

To join a match after the event handler has notified the user their criteria is met and they've been given opponents you can use the match token.

```js fct_label="JavaScript"
// Updated example TBD
```

```csharp fct_label=".Net"
// Updated example TBD
```

```csharp fct_label="Unity"
// Updated example TBD
```

The token makes it easy to join a match. The token enables the server to know that these users wanted to join a match and is able to create a match dynamically for them.

Tokens are short-lived and must be used to join a match as soon as possible. When a token expires it can no longer be used or refreshed.

The match token is also used to prevent unwanted users from attempting to join a match they were not matched into. The rest of the multiplayer match code in the same as in the [realtime multiplayer section](gameplay-multiplayer-realtime.md).
