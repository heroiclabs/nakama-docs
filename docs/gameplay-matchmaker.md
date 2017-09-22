# Matchmaker

In realtime and turn-based games it's important to be able to find active opponents to play against. A matchmaker system is designed to provide this kind of behavior.

In the server we've taken the design further and decoupled how you're matched from the realtime multiplayer engine. This makes it easy to use the matchmaker system to find opponents even if the gameplay isn't realtime. It could be a casual social game where you want to find random new users to become friends with or an asynchronous PvP game where gameplay happens in a simulated battle.

There's many kinds of gameplay experiences where you'd like to find other users outside of a realtime match.

## Request opponents

The distributed server maintains a pool of users who've requested to be matched together with a specific number of opponents. Each user can add themselves to the matchmaker pool and register an event handler to be notified when enough users meet their criteria to be matched.

You can send a message to add the user to the matchmaker pool.

```csharp fct_label="Unity"
INMatchmakeTicket matchmake = null;

// Look for a match for two participants. Yourself and one more.
var message = NMatchmakeAddMessage.Default(2);
client.Send(message, (INMatchmakeTicket result) => {
  Debug.Log("Added user to matchmaker pool.");

  var cancelTicket = Encoding.UTF8.GetString(result.Ticket);
  Debug.LogFormat("The cancellation code {0}", cancelTicket);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

The message returns a ticket which can be used to cancel the matchmake attempt. A user can remove themselves from the pool if wanted.

This is useful for some games where a user can cancel their action to matchmake at some later point and remove themselves being matched with other users.

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
};
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

The token makes it easy to join a match. The token enables the server to know that these users wanted to join a match and is able to create a match dynamically for them.

Tokens are short-lived and must be used to join a match as soon as possible. When a token expires it can no longer be used or refreshed.

The match token is also used to prevent unwanted users from attempting to join a match they were not matched into. The rest of the multiplayer match code in the same as in the [realtime multiplayer section](gameplay-multiplayer-realtime.md).
