# Setting Up Matchmaking

A core feature of many multiplayer games is the ability to find and play against random opponents.

Since Nakama is aware of other online players, it is the best place to arrange matches between players. The Nakama [Matchmaking](../../../concepts/matches.md) feature simplifies the creation and management of matches so that you don't have to build the infrastructure yourself.

## Server-side settings

Nakama handles matchmaking on the server side by polling for tickets submitted by players from the client. These tickets typically contain information about the player and the type of match they are looking for.

First, let's set up the matchmaking settings on the server so that we can accept incoming tickets. There are three options that can be changed regarding how the server handles match requests:

* `max_tickets`: Limits the total number of tickets a player (or group) can submit at one time
* `interval_sec`: Changes the time, in seconds, between each attempt at forming a new match. A lower interval means lower wait times, at the potential cost of needing more server resources to handle requests
* `max_intervals`: Sets the number of times to try at the max player limit before using the min player limit

These are all server settings that cannot be changed by players.
In order to configure these options, we create a [configuration file](../../../getting-started/configuration.md).

In Pirate Panic, our configuration file is `local.yml`, but in your game this can be whatever you want. Inside the configuration file, we create a section for the matchmaker:

=== "local.yml"
    ```yml
    matchmaker:
      max_tickets: 2
      interval_sec: 15
      max_intervals: 3
    ```

A full list of parameters and their defaults for other parts of the server configuration can be explored [in the documentation](../../../getting-started/configuration.md#matchmaker).

## Requesting a match

Now that we've set up the server to handle requests, we can let players create tickets by using `AddMatchmakerAsync`:

```csharp
_ticket = await _connection.Socket.AddMatchmakerAsync(
    query: "*",
    minCount: 2,
    maxCount: 2,
    stringProperties: null,
    numericProperties: null);
```

This ticket describes a player looking to find any match (`*` wildcard query) with exactly two total players.

!!! note "Note"
    You can also restrict queries to find specific players that have matching properties. For example, to only match with other players in Europe you could make a `query = "+region:europe"`. See the [matchmaker documentation](../../../concepts/matches.md#query) for more examples.

`stringProperties` and `numericProperties` can be used to store user data (e.g. name, region, or rank) for use with the matchmaker. Since we have no preference we can leave this `null` in this example.

To give players the option to cancel their matchmaking request if a match hasn't been found yet, use `RemoveMatchmakerAsync` on the ticket that was returned by `AddMatchmakerAsync`:

```csharp
await _connection.Socket.RemoveMatchmakerAsync(_ticket);
```

## Joining a match

Once a ticket is submitted, the server will handle matchmaking and assign all the matched players with a new match, if one can be found.

We can then register a callback function on the client side to run once this occurs which can be used to switch scenes or otherwise prepare the game for joining the match:

```csharp
_connection.Socket.ReceivedMatchmakerMatched += OnMatchmakerMatched;
...
private void OnMatchmakerMatched(IMatchmakerMatched matched)
{
    ...
    _connection.Socket.ReceivedMatchmakerMatched -= OnMatchmakerMatched; // Unregister callback function

    SceneManager.LoadScene(GameConfigurationManager.Instance.GameConfiguration.SceneNameBattle); // Switch scene to battle scene
}
```

Here, `ReceivedMatchmakerMatched` is a [register hook](../../../server-framework/function-reference.md#register-hooks) that automatically fires when the server finds an opponent.

## Further reading

Learn more about the topics and features, and view the complete source code, discussed above:

* [Leaderboards](../../../concepts/leaderboards.md)
* [Matchmaking](../../../concepts/matches.md)
* [Server-side function reference](../../../runtime-code-function-reference/#leaderboards)
* [BattleMenuUI.cs](https://github.com/heroiclabs/unity-sampleproject/blob/master/PiratePanic/Assets/PiratePanic/Scripts/Menus/BattleMenuUI.cs)
* [LeaderboardsMenuUI.cs](https://github.com/heroiclabs/unity-sampleproject/blob/master/PiratePanic/Assets/PiratePanic/Scripts/Menus/LeaderboardsMenuUI.cs)
* [Server main.ts](https://github.com/heroiclabs/unity-sampleproject/blob/master/ServerModules/src/main.ts)
* [Nakama Config local.yml](https://github.com/heroiclabs/unity-sampleproject/blob/master/ServerModules/local.yml)
* [dockerconfig](https://github.com/heroiclabs/unity-sampleproject/blob/master/ServerModules/docker-compose.yml)
