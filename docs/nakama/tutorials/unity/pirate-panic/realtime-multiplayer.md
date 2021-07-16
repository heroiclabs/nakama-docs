# Realtime Multiplayer

[Realtime multiplayer](../../../concepts/client-relayed-multiplayer.md), also known as client-relayed multiplayer, makes it easy for players in a match to quickly send and receive data between one another. When clients connect to a match, they can send messages to the server. Nakama's realtime multiplayer engine then automatically routes these messages to everyone else in the match. Once messages are received, each client can then process them and prompt actions, updates, or other changes to the game state.

Nakama also supports [server-authoritative multiplayer](../../../concepts/server-relayed-multiplayer.md), which puts the server at the center of each match. All clients send and receive data directly to the server, which then processes the data and routes it to other clients, if necessary. Server-authoritative multiplayer allows for more flexibility in how data is handled, but can be more complicated to develop and maintain.

For Pirate Panic we will be using realtime multiplayer.

## Joining a match

Once a ticket is submitted, the server will handle matchmaking and assign all the matched players with a new match, if one can be found.

We can then register a callback function on the client side to run once this occurs which can be used to switch scenes or otherwise prepare the game for joining the match.

For this, we use `ReceivedMatchmakerMatched`, a [register hook](../../../server-framework/function-reference.md#register-hooks) that automatically fires when the server finds an opponent.

```csharp
_connection.Socket.ReceivedMatchmakerMatched += OnMatchmakerMatched;
...
private void OnMatchmakerMatched(IMatchmakerMatched matched)
{
    ...
    _connection.Socket.ReceivedMatchmakerMatched -= OnMatchmakerMatched; // Unregister callback function

    _connection.BattleConnection = new BattleConnection(matched); // Save Matched object to BattleConnection for later use

    SceneManager.LoadScene(GameConfigurationManager.Instance.GameConfiguration.SceneNameBattle); // Switch scene to battle scene
}
```

We'll also need to save the `IMatchmakerMatched` object since it will be necessary to join the match once we've loaded in the battle scene. In PiratePanic, we've created the `BattleConnection` object to store this information, which allows us to access it from multiple classes.

=== "BattleConnection.cs"
    ```csharp
    public class BattleConnection
    {
        public string MatchId { get; set; }
        public string HostId { get; set; }
        public string OpponentId { get; set; }
        public IMatchmakerMatched Matched { get; set; }
        // More properties can be stored here!

        public BattleConnection(IMatchmakerMatched matched)
        {
            Matched = matched;
        }
    }
    ```

Once we loaded into the battle scene, we need to call `JoinMatchAsync` on start by passing in the matchmaker token, which is an identity and reservation token provided by the matchmaker.

This will allow the server to add this client to the correct match and ensure that all messages sent to and from other players in this match will also be sent to this client:

```csharp
protected async void Start() {
    IMatch match = await _connection.Socket.JoinMatchAsync(_connection.BattleConnection.Matched);
    _connection.BattleConnection.MatchId = match.Id;
    ...
}
```

Players can leave a match by calling `_connection.Socket.LeaveMatchAsync(matchId)`, where `matchId` is the ID that was returned by `JoinMatchAsync`.

## Sending realtime data

Now that a player has joined the match, let's send some information to other players. This is done with a `socket.SendMatchStateAsync()` call.

For example, when a player casts a spell during the game, the other players should receive information (e.g. type of spell, where it was cast, etc.) so that their game client can render it.

=== "MatchMessageSpellActivated,cs"
    ```csharp
    using Nakama.TinyJson;
    ...
    public class MatchMessageSpellActivated {
        public readonly string OwnerId;
        ... // More message data goes here

        public MatchMessageSpellActivated(string ownerId, ...) {
            OwnerId = ownerId;
            ...
        }
    }
    ```

Then, inside a function somewhere else:

```csharp
long opCode = 5; // custom opcode for spells
MatchMessageSpellActivated message = new MatchMessageSpellActivated(playerId, ...);
string json = JsonWriter.ToJson(message); // Converts C# object to JSON object
_connection.Socket.SendMatchStateAsync(_connection.BattleConnection.MatchId, opCode, json);
```

Here we created a new object called `MatchMessageSpellActivated` that holds the necessary information. We need to make sure to standardize the object so that it can be easily converted on the other end, JSON in this case. Creating a custom class and then converting it to JSON is a common pattern for sending any type of data. You can add different properties to the class to send whatever you want.

The `opCode` lets the recipient client handle messages differently based on their category without needing to investigate the structure of the payload itself. The opcode can be any positive integer, no inherent meaning is pre-defined by Nakama so you can create your own schemes.

## Receiving realtime data

Now that we sent a message to the match, the other players in the match need to be able to receive it. To do this, Nakama has the register hook `ReceivedMatchState`, which fires every time any message is received by the client.

For our cast spell example, we need to receive that message to show the spell on our client as well as process any damage that it dealt:

```csharp
void Start() {
    _connection.Socket.ReceivedMatchState += ReceiveMatchStateMessage; // Bind function to hook
}
...
private void ReceiveMatchStateMessage(IMatchState matchState) {
    string messageJson = System.Text.Encoding.UTF8.GetString(matchState.State);

    if (matchState.opCode == 5) {
        MatchMessageSpellActivated spell = JsonParser.FromJson<MatchMessageSpellActivated>(messageJson);
        // Do stuff with the spell (instantiate object, destroy towers, etc.)
    }
    ...
    // Handle more opcodes, or alternatively create a switch statement
}
```

Here, we're creating a custom function, `ReceiveMatchStateMessage`, and binding it to `ReceivedMatchState` so it gets called on every message received. The message is stored in `matchState.State` and, since we used the opcode `5` to specify the message is of type `MatchMessageSpellActivated`, we can convert the JSON object back to C# to read its properties.

The `ReceiveMatchStateMessage` function can be used to handle different message types specified by their respective `opcodes`.

## Match presence

Every player in a match has a `presence` in the match. Nakama internally tracks this presence information as a match roster. Whenever a player joins or leaves the match, a delta of updates is sent to the client so that the clients can update their own player list view or perform game-specific actions.

When a client initially joins a match, Nakama will automatically provide them with a list of current joined opponents. This list can be accessed via the `match.Presences` object. As a reminder, `match` is the `IMatch` object that was returned by `JoinMatchAsync`.

Additionally, we can use the `ReceivedMatchPresence` hook to run a function every time a player joins or leaves. The list of joins can be accessed as `match.Joins`, and leaves as `match.Leaves`.

For example, the code below prints a message if a player left the game and there is still someone remaining:

```csharp
void Start() {
    _connection.Socket.ReceivedMatchPresence += OnMatchPresence;
}
...
private void OnMatchPresence(IMatchPresenceEvent e) {
    if (e.Leaves.Count() > 0) {
        Debug.LogWarning($"OnMatchPresence() User(s) left the game");
    }
}
```

We can also use presence information to assign a host player responsible for handling events that should only be done one time. For example, in the beginning of the game we might want to distribute initial card hands to each player.

Assigning a host can be done by assigning as host the first player that joins the match. We can do this by checking if the number of presences in the match is `0` (presence count doesn't include the current client):

```csharp
if (match.Presences.Count() == 0) {
    _connection.BattleConnection.HostId = _connection.Session.UserId;
    ...
} else {
    string opponentId = match.Presences.First().UserId;
    _connection.BattleConnection.OpponentId = opponentId;
    _connection.BattleConnection.HostId = opponentId;
    ...
}
```

Then, if we want to perform a host-only action, we can check if the host ID is equal to the current user's ID:

```csharp
if (_connection.BattleConnection.HostId == _connection.Session.UserId) {
    // Handle host-only behaviors
}
```

## Using RPC

You may want to offload sensitive calculations to the server rather than trusting the host to perform them. For example, after the match ends we want to reward the winner with gems.

In  Pirate Panic, once a player's main fort is destroyed we want to send a message to the server to end the match:

```csharp
private async void OnAfterMainFortDestroyed() {
    ...
    response = await _connection.Client.RpcAsync(_connection.Session, "handle_match_end", matchEndRequest.ToJson());
}
```

Here, `matchEndRequest` is any object containing the information to send to the server. We need to convert it to JSON to send it via RPC so that it can then be decoded on the server side to Typescript:

```typescript
const rpcHandleMatchEnd: nkruntime.RpcFunction = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    let request : MatchEndRequest = JSON.parse(payload);
    ...
}
```

See an example of the behavior of such a RPC in the [Leaderboards](leaderboards.md) section.

## Further reading

Learn more about the topics and features, and view the complete source code, discussed above:

* [Authoritative Multiplayer](../../../concepts/server-relayed-multiplayer.md)
* [Realtime Multiplayer](../../../concepts/client-relayed-multiplayer.md)
* [BattleConnection.cs](https://github.com/heroiclabs/unity-sampleproject/blob/master/PiratePanic/Assets/PiratePanic/Scripts/Menus/Battle/Multiplayer/BattleConnection.cs)
* [Scene02BattleController.cs](https://github.com/heroiclabs/unity-sampleproject/blob/master/PiratePanic/Assets/PiratePanic/Scripts/Scene02BattleController.cs)
* [MatchMessage.cs](https://github.com/heroiclabs/unity-sampleproject/blob/master/PiratePanic/Assets/PiratePanic/Scripts/Menus/Battle/Multiplayer/MatchStates/MatchMessage.cs)
* [MatchMessageSpellActivated.cs](https://github.com/heroiclabs/unity-sampleproject/blob/master/PiratePanic/Assets/PiratePanic/Scripts/Menus/Battle/Multiplayer/MatchStates/Spells/MatchMessageSpellActivated.cs)
