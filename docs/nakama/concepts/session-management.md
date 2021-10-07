[nakama-sessions]: images/nakama-sessions.png "The overlap between player behaviours and session types"

# Session Management

Authentication, coupled with user accounts and session management seems complex. There are lots of moving parts that come together, such as your:

* Player’s behavior

* Player’s device

* Social provider accounts

* Nakama user accounts

It’s tempting to start coding right away and hope to glue together an unrelated collection of concepts. That can work, but you may frustrate yourself (and your players) with trial-and-error missteps.

Instead, we can approach these concepts as a whole, layering different kinds of sessions—play, social, and authentication—in your code and your player’s experience.

## What is a session, anyway?

The term *session *is a bit overloaded. It’s used to mean lots of different things in many different contexts, from abstract ideas of play* *to the low-level mechanics of game development. For the purposes of authentication, we care about three types of sessions:

### Play session

A time that the player plays the game before stopping to do something else. A play session and its length varies by game type, platform, and player. A play session might last the length of a turn, round, or match (or many turns, rounds, or matches) and it might be influenced by your player’s attention span and motivation.

Play sessions are how your player experiences the game.

### Sign-in session

A time your player is signed into their account. This is often through a provider like Google Play Games, Apple Game Center, or your own account infrastructure.

Sign-in sessions are how your game identifies a distinct player. Sign-in sessions are a way of tracking a particular player.

### Authentication session

A time a client and the server are in trusted communication with each other. You can also think of this as a *Nakama session*.

Other session types exist and have a role in developing authentication for your game (such as a network TCP session or a cryptographic TLS session), but they’re not essential to this article. We’ll assume these underlying protocols just work and focus more on player interactions.

## Sessions overlap

Players don’t generally think of being signed-in or about server connections. They think about playing and not playing. They don’t expect to enter their password or face down a "Connecting" screen every time they take a turn.

As a consequence, session types overlap and depend on each other. This is how sign-ins and authentication support your game: they run longer or shorter than a play session, in ways that are (hopefully) transparent to the player. You can imagine your game being in many sessions at once: the player signed in, the player spending time playing the game, and the client in connection with Nakama.

![The overlap between player behaviors and session types][nakama-sessions]

But session types aren’t tightly connected to each other. A player may only play for a few minutes each day, but stay logged into their Apple account for days or weeks. A server connection might be established briefly and intermittently, or persist at length, depending on the game. Sessions need to begin and end when in relation with player and game activity.

## Starting a session with Nakama

Before a client can do anything for the player on the server, such as matchmaking or updating a leaderboard, the client must authenticate with the server. Authentication marks the start of a session with Nakama. Typically, you authenticate each time the game starts (or reuse a cached session—more on this later).

Here’s the outline for a session in Nakama:

1. Start a session. Identify the player or device with the authenticate API; Nakama responds with a session object.

2. Make requests using your session object.

3. When the session expires, start again.

That first step is the most complex. To start the session, your client makes a request to authenticate with an identifier. Where does this ID come from? There are lots of sources: social providers’s OAuth tokens, device IDs, usernames and passwords, or a custom service of your own. Wherever your credentials come from, they all say the same thing: this player or device is playing and not any other.

## Authentication maps social profiles to Nakama

This is where the authentication session and sign-in session layers come together. Authentication maps your player’s sign-in session to Nakama session.

Typically, the identifier used with the authentication API is a token from a social provider, such as Apple Game Center, Google Play Games, or Facebook. If your player is not yet signed in, this is when your game triggers the sign-in flow and gets a new identifier. If your player is signed in, then you pass in the identifier already on-hand.

**When you authenticate with a player’s sign-in ID, you map your player’s sign-in ID with Nakama’s user account. **This is how you begin to knit together play session, sign-in session, and authentication session (when you want to get user data later, you can look it up by Nakama user ID or social provider ID).

## Papers, please: presenting the session object

When you have successfully authenticated, Nakama replies with a special session object: a JSON Web Token (JWT). The session object is like a building access badge with an expiration date. You need it to open doors: making further requests to the Nakama server.

JWTs are cryptographically-signed JSON objects ([learn more about the standard](https://jwt.io/)). A Nakama session object JWT contains some details—including an expiration date and time—that’s been signed with the server’s private key. As long as the JWT has not expired and has not been tampered with, the server can securely validate the JWT. At the same time, the JWT is *not* encrypted, so your client code can inspect its contents too.

Here’s an example JWT, in it’s encoded form:

```bash
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiJmNDA4MmFhMC1hYWQwLTQ1MjYtODkwZC1iYTUwYjI0NmJlMTkiLCJ1c24iOiJhS25pWU5pZ1FiIiwiZXhwIjoxNTk3NjY3MjIwfQ.1fdAmq3nrDcPy0k6BwPCcULmhLiB54Z_feEuDaINNsA
```

Decoded, it becomes this JSON object:

```json
{
    "uid": "f4082aa0-aad0-4526-890d-ba50b246be19",
    "usn": "aKniYNigQb",
    "exp": 1597667220
}
```

The JWT contains three key keys:

* `uid` - an ID for the authenticated Nakama user (the Nakama user records may have one or more social identifiers, but this is Nakama’s unique reference to the user)

* `usn` - a username

* `exp` - An expiration date and time, in Unix timestamp format

Armed with this JWT, your code presents the JWT (in encoded form) with every new request to Nakama (the client SDKs can help make this happen seamlessly). By using the JWT, requests to Nakama are lightweight and the server can validate the request fast. Thanks to the cryptographic signature, Nakama can validate the request without the delay of a database lookup.

In other words, cache that JWT! As long as you have a valid session object, you can support the play session by setting up matches, updating leaderboards, adding a player to a chat group, and more.

## How a session ends (and begins again)

Eventually (after some time set in your Nakama configuration), the session object expires and the session ends. If you try to make a request with an expired session object, the request is denied with a `401 Unauthorized` error.

**You can’t renew, extend, or refresh a session object.** When it expires, you can’t make new requests to Nakama (though active socket connections continue until closed, even if the original token has expired). If you want to continue making requests, then you have to go back to the beginning and reauthenticate.

But that doesn’t necessarily mean your sign-in session or play session has ended at the same time. For example, if you still have a valid sign-in session with Game Center and the player is still in an active play session, then you can reauthenticate without missing a beat. You don’t have to ask the player to sign-in again or force them out of the game before you reauthenticate.

Likewise, just because a session has ended doesn’t mean that you must reauthenticate immediately. You might wait for the player’s next play session or wait for a specific game action that requires Nakama.

## Building session management into your game

Layering different kinds of sessions into your game might feel complicated. But Nakama can help you with low-level details.

On the server, the Nakama API provides a general `[authenticate](https://heroiclabs.com/docs/authentication/#authenticate)` endpoint, plus [endpoints specific to Apple, Facebook, Google, and Steam](https://heroiclabs.com/docs/authentication/#social-providers). On the client, you can use the Nakama client for your language or engine to authenticate. Check out [the authentication examples](https://heroiclabs.com/docs/authentication/#authenticate) to learn more.
