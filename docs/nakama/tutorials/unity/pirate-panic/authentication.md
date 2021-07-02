# Authentication

We will begin with one of the more important features of a multiplayer game: the ability for players to establish themselves as a user, and communicate with one another.

Nakama helps us do this by making it easy to create a **centralized server**, where data is stored and processed, and allowing our players (**clients**) to connect to the server.

## Establishing a connection

Here we connect the Unity client and the Nakama server.

The first step is to register a new Client object upon game start in Unity. This object will be the interface for any interactions between the client and the server.

Below is the snippet from the Main Menu script that initializes the client:

=== "Scene01MainMenuController.cs"
    ```csharp
    var client = new Client("http", "localhost", 7350, "defaultkey", UnityWebRequestAdapter.Instance);
    client.Timeout = 5;
    ```

Here, we're creating a connection to the server that you started locally (available at `http://localhost:7350`) by passing the following connection values:

* `Scheme`: The connection scheme, `http` here.
* `Host`: The server host, `localhost` for this example.
* `Port`: The server port, set to `7350` by default.
* `ServerKey`: By default `defaultkey` is used. Can be changed via the [server configuration](../../../../install-configuration.md#socket).

## Device authentication

After establishing the server connection, we authenticate the player so we can connect them to an identity.

When starting Pirate Panic, you may notice that you are automatically authenticated and assigned a username. This type of seamless authentication is device authentication, which creates accounts linked to the device that is running the game, such as a phone or computer.

In Unity, device authentication can be done by fetching the Device ID and passing it into `AuthenticateDeviceAsync`:

```csharp
string deviceId = SystemInfo.deviceUniqueIdentifier;
session = await client.AuthenticateDeviceAsync(deviceId);
```
!!! note "Note"
    If building for WebGL you may have to use `System.Guid.NewGuid().ToString()` instead of `deviceUniqueIdentifier`.

When authentication is complete, the player joins a ***session***, which represents the period of time a player is logged in. See [Sessions](../../../../session.md) to learn more.

The session object allows us to make requests and access user information as an authenticated client, primarily through the `Socket` and `Account` objects:

```csharp
var socket = client.NewSocket(useMainThread: true);
await socket.ConnectAsync(session);
account = await client.GetAccountAsync(session);
```

In Pirate Panic, the `client`, `socket`, `account`, and `session` properties are saved in the `GameConnection` object. So any access to `_connection` later on, remember that it is referring to these properties.

!!! note "Note"
    You can organize the server connection however you wish, just be sure it can be accessed by other classes.

## Facebook authentication

Device authentication makes it easy for players to jump right into the game, but linking accounts to devices means that if the device is ever lost or reset the account is lost. Connecting via an external social provider, such as Facebook, to user accounts enables you to mitigate this issue and fetch additional user information.

!!! note "Note"
    The Facebook SDK is included in the Pirate Panic code if you cloned the project repository. If you're starting your own project, you'll need to [get the SDK][https://developers.facebook.com/docs/unity].


Include the Facebook SDK in the scene script and initialize it as follows:

```csharp
using Facebook.Unity;
..
private void InitializeFacebook() {
    FB.Init(() =>
    {
        FB.ActivateApp();
    });
}
```

Then, the following snippet handles the log in:

=== "ProfileUpdatePanel.cs"
    ```csharp
    private void LinkFacebook()
    {
        List<string> permissions = new List<string>();
        permissions.Add("public_profile");

        FB.LogInWithReadPermissions(permissions, async result =>
        {
            string facebookToken = result.AccessToken.TokenString;
            await _connection.Client.AuthenticateFacebookAsync(facebookToken);
        });
    }
    ```

!!! note "Note"
    You may want to bind `LinkFacebook` to a **Connect Facebook** button so players can choose when to login rather than being forced to do so right away.

Here, we're giving the `public_profile` permission so that Nakama can access basic user information like name or profile picture. You can also add other permissions and features, which can be found in the [Facebook SDK documentation][https://developers.facebook.com/docs/unity].

Logging into Facebook will automatically add a player's Facebook friends into their in-game [friends](friends.md) list.

## Initializing a new player

After players have a way to log in, next is setting up the initial player information so they can start adding friends, collecting gems, completing quests, or anything else that requires storing user data over time.

This can be done on the server side using a [register hook](../../../../runtime-code-basics.md#register-hooks). There is a different hook for each authentication method. For example, since we set up device and Facebook authentication, we should use `registerAfterAuthenticateDevice` and `registerAfterAuthenticateFacebook`.

We bind a function to them that runs when the hook is triggered:

=== "main.ts"
    ```typescript
    initializer.registerAfterAuthenticateDevice(afterAuthenticateDeviceFn);

    const afterAuthenticateDeviceFn: nkruntime.AfterHookFunction<nkruntime.Session, nkruntime.AuthenticateDeviceRequest> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.Session, req: nkruntime.AuthenticateDeviceRequest) {
        afterAuthenticate(ctx, logger, nk, data);

        if (!data.created) {
            return; // Account already exists.
        }
        ...
        // Player initialization goes here. Write initial stats, add items to inventory, etc.
    }
    ```

## Session tokens

For security reasons, player sessions will automatically expire after the time period defined in your [Nakama configuration](../../../../install-configuration.md#common-properties). To avoid forcing players to constantly log back in, we can set up a method to automatically request a new session when the old one is about to expire.

This is done by saving **tokens** for authentication on the client, and periodically passing these back to the server to request new tokens.

There are two types of tokens:

* **Access tokens**: Tell the server about the identity of the client and allows the server to trust this client.
* **Refresh tokens**: Used to request new access tokens.

With Nakama, we can use the `client` object to request new tokens with `SessionRefreshAsync`:

=== "Scene01MainMenuController.cs"
    ```csharp
    string authToken = PlayerPrefs.GetString("nakama.authToken", null);
    string refreshToken = PlayerPrefs.GetString("nakama.refreshToken", null);

    session = Session.Restore(authToken, refreshToken);

    // Check whether a session is close to expiry.
    if (session.HasExpired(DateTime.UtcNow.AddDays(1))) {
        try {
            // get a new access token
            session = await client.SessionRefreshAsync(session);
        } catch (ApiResponseException) {
            // get a new refresh token
            session = await client.AuthenticateDeviceAsync(deviceId);
            PlayerPrefs.SetString("nakama.refreshToken", session.RefreshToken);
        }

        PlayerPrefs.SetString("nakama.authToken", session.AuthToken);
    }
    ```
Here, we use Unity's built in `PlayerPrefs` engine to store the access token and refresh token into `nakama.authToken` and `nakama.refreshToken` respectively.
If the session is less than a day away from expiring, the script will attempt to request a new access token using `SessionRefreshAsync`. If the refresh token is invalid, this will fail and we'll need to re-authenticate all over again.

## Further reading

Learn more about the topics and features, and view the complete source code, discussed above:

* [Authentication](../../../../authentication.md)
* [Session Management](../../../../expert-auth.md)
* [GameConnection.cs](https://github.com/heroiclabs/unity-sampleproject/blob/master/PiratePanic/Assets/PiratePanic/Scripts/GameConnection.cs)
* [Scene01MainMenuController.cs](https://github.com/heroiclabs/unity-sampleproject/blob/master/PiratePanic/Assets/PiratePanic/Scripts/Scene01MainMenuController.cs)
* [ProfileUpdatePanel.cs](https://github.com/heroiclabs/unity-sampleproject/blob/master/PiratePanic/Assets/PiratePanic/Scripts/Menus/Profile/ProfileUpdatePanel.cs)
* [Server main.ts](https://github.com/heroiclabs/unity-sampleproject/blob/master/ServerModules/src/main.ts)
