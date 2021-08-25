# Migrating from GameSparks to Nakama

With the [announced](https://docs.gamesparks.com/transition-faq/) deprecation of GameSparks, users must now consider their available alternatives and the task of migration. This may include dramatic changes to existing gameplay design if the alternative providers do not have parity with the GameSparks feature set.

This is not a concern when choosing to migrate to Nakama as it provides a superset of all GameSparks features you may be using in your games and does so with much more flexibility, enabling you to customize your gameplay and social features in ways you could not with GameSparks.

Nakama is suitable for developers and studios of any size, offering [open-source Nakama](https://github.com/heroiclabs/nakama/releases), a managed offering in [Heroic Cloud](https://heroiclabs.com/heroic-cloud/), and the option of deploying [Nakama Enterprise](https://heroiclabs.com/nakama-enterprise/) in your private cloud.

With any of the available offerings you can maintain your existing game functionality as available in GameSparks, or enhance it with the dynamic social features provided by Nakama and the flexibility of its [server framework](../server-framework/basics.md).

## Cloud-code

| GameSparks | Nakama |
| :--------- | :----- |
| Integrated into the GameSparks cluster architecture and enables creation of custom events, synchronous database calls, bulk jobs, and custom endpoints and modules. | The Nakama [Server Framework](../server-framework/basics.md) provides built-in support for your custom cloud-code login written in [Go](../server-framework/go-setup.md), [Lua](../server-framework/lua-setup.md), or [JavaScript/TypeScript](../server-framework/typescript-setup.md).<br><br> While Lua and TypeScript runtime code is [sandboxed](../server-framework/basics.md#sandboxing), Go enables full access, the ability to bring in C libraries, and create custom endpoints via the HTTP server. You can also integrate with your desired business intelligence tools (e.g. Power BI), create [server to server calls](../server-framework/basics.md#server-to-server), and rewrite behavior using [before](../server-framework/basics.md#before-hook) and [after](../server-framework/basics.md#after-hook) hooks.<br><br>Additionally, Nakama provides a Server Framework Runtime that can used to compose any desired combination of server logic.

## RPCs

A staple of Nakama's server runtime framework is the ability to create RPC functions that can be called from the client. You can use RPCs to perform various client/server interactions, such as sending player positions or retrieving a status of something from the server.

Below is an example of how to create an RPC on the server.

=== "TypeScript"
    ```typescript
    let exampleRpc: nkruntime.RpcFunction = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
        logger.info('ExampleRpc called by user ID %q', ctx.userId);
        return JSON.stringify({ success: true });
    }

    initializer.registerRpc('example-rpc', exampleRpc)
    ```
=== "Go"
    ```go
    exampleRpc := func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
      userID, _ := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
      logger.Info("ExampleRpc called by user %q", userID)
      return "{ \"success\": true }", nil
    }

    initializer.RegisterRpc("example-rpc", exampleRpc)
    ```

Shown below is an example of how you would call your RPC function from the client (in this case we're using Unity).

=== "Unity"
    ```csharp
    public class ExampleRpcResult {
      bool Success;
    }

    // ...

    var response = await client.RpcAsync(session, "example-rpc", "");
    var result = JsonUtility.FromJson<ExampleRpcResult>(response.Payload);
    Debug.LogFormat("Result successful? {0}", result.Success);
    ```

## Authentication

<table>
<thead>
  <tr>
    <th>GameSparks</th>
    <th>Nakama</th>
  </tr>
</thead>
<tbody>
  <tr>
    <td>GameSparks enables Device Authentication and Registered Accounts, with integrations for the following third-party authentications:<br>
    <ul>
        <li>Facebook</li>
        <li>Game Center</li>
        <li>Google Play</li>
        <li>Switch</li>
        <li>PSN</li>
        <li>Steam</li>
        <li>Twitch</li>
        <li>Kongregate</li>
        <li>XBoxOne</li>
        <li>XBoxLive</li>
        <li>QQ</li>
        <li>WeChat</li>
        <li>Twitter</li>
    </ul><br>
    Additionally, conversion of anonymous device authenticated accounts to registered accounts is supported.</td>
    <td>Nakama provides full <a href="https://heroiclabs.com/docs/nakama/concepts/authentication/">Authentication</a> feature parity, and integrations for the following third-party authentications:<br>
    <ul>
        <li>Facebook</li>
        <li>Facebook Instant</li>
        <li>Game Center</li>
        <li>Google Play</li>
        <li>Steam</li>
        <li>PSN (Enterprise only)</li>
        <li>Switch (Enterprise only)</li>
    </ul><br>
    Nakama also supports <a href="https://heroiclabs.com/docs/nakama/concepts/authentication/#custom">custom authentication</a> for use with your external or custom identity manager.<br><br>
    Account "conversion" is accomplished via <a href="https://heroiclabs.com/docs/nakama/concepts/authentication/#link-or-unlink">linking</a> multiple authentication methods (e.g. Email, Facebook) to a user account. See the example below.</td>
  </tr>
</tbody>
</table>

#### Examples

The following example creates a new user account using device authentication and then links an email and password authentication option afterwards.

=== "Unity"
    ```csharp
    // Create and authenticate a new user with device authentication
    var session = await client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier, "MyAwesomeUsername", true);

    // Link an email and password to the new user
    await client.LinkEmailAsync(session, "example@heroiclabs.com", "MyAwesomePassword");

    // Authenticate with the email and password
    session = await client.AuthenticateEmailAsync("example@heroiclabs.com", "MyAwesomePassword");
    ```

Sometimes it can be useful to run code before or after a user has authenticated. For example, when using the Custom authentication flow it is often useful to take the user's authorization Id and use it to authenticate against a third party service. Another example would be to reward the player for logging in by giving them virtual currency.

To add before/after authentication hooks you can use the `BeforeAuthenticate[Method]` and `AfterAuthenticate[Method]` functions. For example, `BeforeAuthenticateEmail` and `AfterAuthenticateEmail` as shown below.

=== "TypeScript"
    ```typescript
    let beforeAuthenticateEmail: nkruntime.BeforeHookFunction<nkruntime.AuthenticateEmailRequest> = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.AuthenticateEmailRequest): void | nkruntime.AuthenticateEmailRequest {
        logger.info("Running BeforeAuthenticateEmail for user: %q", data.username);
        return data;
    }

    let afterAuthenticateEmail: nkruntime.AfterHookFunction<nkruntime.Session, nkruntime.AuthenticateEmailRequest> = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.Session, request: nkruntime.AuthenticateEmailRequest) {
        logger.info("Running AfterAuthenticateEmail for user: %q", request.username);
    }

    initializer.registerBeforeAuthenticateEmail(beforeAuthenticateEmail)
    initializer.registerAfterAuthenticateEmail(afterAuthenticateEmail)
    ```
=== "Go"
    ```go
    beforeAuthenticateEmail := func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.AuthenticateEmailRequest) (*api.AuthenticateEmailRequest, error) {
      logger.Info("Running BeforeAuthenticateEmail for user: %q", in.Username)
      return in, nil
    }

    afterAuthenticateEmail := func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, out *api.Session, in *api.AuthenticateEmailRequest) error {
      logger.Info("Running AfterAuthenticateEmail for user: %q", in.Username)
      return nil
    }

    initializer.RegisterBeforeAuthenticateEmail(beforeAuthenticateEmail)
    initializer.RegisterAfterAuthenticateEmail(afterAuthenticateEmail)
    ```

## Social

### Leaderboards

| GameSparks | Nakama |
| :--------- | :----- |
| Leaderboards available in GameSparks include basic non-resetting, real-time boards, periodically updating leaderboards, and partitioned leaderboards.<br><br> Leaderboard event notifications and limited API support are available. | Nakama provides a first-class leaderboards API, enabling the creation of unlimited numbers of leaderboards in any format desired. [Metadata](../concepts/leaderboards.md#custom-fields) can be used to include any additional attributes desired. These attributes, or any desired filter, can be used in creating custom ("Bucketed") leaderboards.<br><br>Custom [reset schedules](../concepts/leaderboards.md#reset-schedules) can be created, along with on-the-fly rank calculation. Using the Nakama Server Framework you can also [hook](../server-framework/basics.md#register-hooks) into any leaderboard events.<br><br>See the [Leaderboards](../concepts/leaderboards.md) documentation, [function reference](../server-framework/function-reference.md#leaderboards), and [Bucketed Leaderboards guide](bucketed-leaderboards/index.md) to learn more. |

#### Examples

The following example shows how to send new highscore notifications to users when they beat their current leaderboard score. This would be run from the server side.

=== "TypeScript"
    ```typescript
    // Create a new Leaderboard with the ID "AwesomeLeaderboard".
    // This is for demonstration purposes, you would normally have created this beforehand.
    nk.leaderboardCreate("AwesomeLeaderboard", false, nkruntime.SortOrder.DESCENDING, nkruntime.Operator.BEST, "0 0 * * 1");

    // Get the user's previous leaderboard score or default to 0.
    let previousScore = 0;
    let recordList = nk.leaderboardRecordsList("AwesomeLeaderboard", [ctx.userId], 1);
    if (recordList.records != null && recordList.records.length > 0) {
        previousScore = recordList.records[0].score;
    }

    // Write the user's new leaderboard score (here we simulate an ever increasing score using the current timestamp).
    let newScore = Date.now();
    nk.leaderboardRecordWrite("AwesomeLeaderboard", ctx.userId, ctx.username, newScore);

    // If the user's new score is higher than their previous score, send a "New Highscore" notification.
    if (newScore > previousScore) {
        let content = {
            score: newScore,
            previousScore
        }

        nk.notificationSend(ctx.userId, "New Highscore!", content, 101, null, true);

        logger.info("New Highscore notification sent to user: %q", ctx.userId);
    }
    ```
=== "Go"
    ```go
    // Create a new Leaderboard with the ID "AwesomeLeaderboard".
    // This is for demonstration purposes, you would normally have created this beforehand.
    if err := nk.LeaderboardCreate(ctx, "AwesomeLeaderboard", false, "desc", "best", "0 0 * * 1", nil); err != nil {
      return "", err
    }

    // Get the user's ID and account object.
    userID, _ := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
    account, err := nk.AccountGetId(ctx, userID)
    if err != nil {
      return "", err
    }

    // Get the user's previous leaderboard score or default to 0.
    previousScore := int64(0)
    records, _, _, _, _ := nk.LeaderboardRecordsList(ctx, "AwesomeLeaderboard", []string{userID}, 1, "", 0)
    if len(records) > 0 {
      previousScore = records[0].Score
    }

    // Write the user's new leaderboard score (here we simulate an ever increasing score using the current timestamp).
    newScore := time.Now().Unix()
    nk.LeaderboardRecordWrite(ctx, "AwesomeLeaderboard", userID, account.User.Username, newScore, 0, nil, nil)

    // If the user's new score is higher than their previous score, send a "New Highscore" notification.
    if newScore > previousScore {
      content := map[string]interface{}{
        "score":         newScore,
        "previousScore": previousScore,
      }

      if err := nk.NotificationSend(ctx, userID, "New Highscore!", content, 101, "", true); err != nil {
        return "", err
      }

      logger.Info("New Highscore notification sent to user: %q", userID)
    }
    ```

Sometimes it can be useful to perform certain actions on the server whenever a leaderboard resets. This is possible via the `LeaderboardReset` hook as shown below.

=== "TypeScript"
    ```typescript
    let onLeaderboardReset: nkruntime.LeaderboardResetFunction = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, leaderboard: nkruntime.Leaderboard, reset: number) {
        logger.info('Leaderboard ID %q has reset at %q', leaderboard.id, reset);
    };

    initializer.registerLeaderboardReset(onLeaderboardReset);
    ```
=== "Go"
    ```go
    onLeaderboardReset := func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, leaderboard runtime.Leaderboard, reset int64) error {
  		logger.Info("Leaderboard ID %q has reset at %q", leaderboard.GetId(), reset);
  		return nil
  	}

  	initializer.RegisterLeaderboardReset(onLeaderboardReset)
    ```

### Teams

| GameSparks | Nakama |
| :--------- | :----- |
| Teams in GameSparks are created based on the type of team the player is attempting to create or join. They can be configured with a total max membership (number of users), a max membership per user (e.g. can only belong to one "guild"), or max ownership per user.<br><br> Custom data can be configured for teams, and limited API access for creating and editing teams and team membership is available. | Nakama provides a first-class teams API for persisted [groups](../concepts/groups-clans.md) (and [parties](../concepts/parties.md) for ephemeral groups). Multi-tiered membership and option rules are available, and any desired custom logic can be implemented via custom hooks and RPCs. Custom data can be stored in the form of group or membership metadata or using the Nakama [Storage Engine](../concepts/collections.md).<br><br> See the [Groups or Clans](../concepts/groups-clans.md) documentation and [function reference](../server-framework/function-reference.md#groups) to learn more. |

### Chat

| GameSparks | Nakama |
| :--------- | :----- |
| While GameSparks provides a built-in chat system, this feature **only works through teams**. Developers are therefore required to develop their own chat service. | Nakama provides complete built-in support for [realtime chat](../concepts/realtime-chat.md), whether public channels, group channels, or direct messaging between users is desired.<br><br> Persistence is enabled on all channels by default, meaning message history is available for players to review at any time. This persistence can be disabled, with messages then sent only to online players.<br><br>[Hooks](../server-framework/basics.md#before-hook) can be used to filter and reject messages (e.g. for toxic content) or to remove profanity as in the example below.  |

#### Examples

The following example shows how to use hooks in your server runtime code to reject or sanitize message content.

=== "TypeScript"
    ```typescript
    let rtBeforeChannelMessageSend: nkruntime.RtBeforeHookFunction<nkruntime.Envelope> = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, envelope: nkruntime.Envelope) : nkruntime.Envelope {
        let e = envelope as nkruntime.EnvelopeChannelMessageSend;
        if (e == null)
        {
            return e;
        }

        if (e.channelMessageSend.content.indexOf('Bad Word') !== -1) {
            // Alternatively, to sanitize instead of reject:
            //e.channelMessageSend.content = e.channelMessageSend.content.replace('Bad Word', '****');

            // Reject the message send.
            throw new Error("Profanity detected");
        }

        return e;
    }

    initializer.registerRtBefore("ChannelMessageSend", rtBeforeChannelMessageSend);
    ```
=== "Go"
    ```go
    // Inside your Go module's `InitModule` function.
    if err := initializer.RegisterBeforeRt("ChannelMessageSend", func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, envelope *rtapi.Envelope) (*rtapi.Envelope, error) {
      message := envelope.GetChannelMessageSend()
      if strings.Contains(message.Content, "bad word") {
        // Alternatively, to sanitize instead of reject:
        // message.Content = strings.ReplaceAll(message.Content, "bad word", "****")

        // Reject the message send:
        return nil, runtime.NewError("profanity detected", 3)
      }
      return envelope, nil
    }); err != nil {
      return err
    }
    ```

### Status

| GameSparks | Nakama |
| :--------- | :----- |
| No Status feature is provided by GameSparks. Developers must implement a custom solution via the Game Data Service and Cloud Code if desired. | Nakama provides complete [Status](../concepts/status.md) support, enabling users to set simple online status indicators, follow users and receive status updates, and more advanced status messages (e.g. their match or party ID so others can join). |

### Matchmaking

| GameSparks | Nakama |
| :--------- | :----- |
| GameSparks Matchmaking features were deprecated as of February 2019 and not available to projects created after that time. | Nakama provides extensive [matchmaking](../concepts/matches.md) capabilities for both realtime multiplayer and asynchronous games. Check out the [client-relayed](../concepts/client-relayed-multiplayer.md) and [server-authoritative](../concepts/server-authoritative-multiplayer.md) documentation to learn more.<br><br>There is built-in support for match listing to power Lobby views, flexible matchmaking queries for finding opponents, along with tuning relaxing criteria as desired, and providing messaging to your players for results. Players can join matches individually or together as a [party](#realtime-parties).<br><br>Custom matches can be created via server hook, with deep integration between the matchmaker and realtime authoritative matches being built-in also. See the [function reference](../server-framework/function-reference.md#matches) to learn more.<br><br>Matchmaking, like other Nakama features, can be used with third-party engine networking. |

### Realtime parties

| GameSparks | Nakama |
| :--------- | :----- |
| Realtime and matchmaking features were deprecated by GameSparks as of February 2019 and are unavailable for projects created after that date. | The Nakama [realtime parties](../concepts/parties.md) feature enables the creation of ephemeral groups (not persisted once the players leave) so that players can communicate and matchmake together as a group. |

### Tournaments

| GameSparks | Nakama |
| :--------- | :----- |
| No express Tournaments feature is provided by GameSparks but rather must be created using a combination Events, Matches, Realtime servers, and Matchmaking. Both the Realtime servers and Matchmaking features were deprecated as of February 2019 and not available to projects created after that time. | Nakama provides a first-class tournaments API, enabling the creation of dynamic tournament, league, or other desired competitive gameplay into your project. A reset schedule can be provided for recurring tournaments, and metadata can be used to add any additional attributes desired, for example the weather conditions during the tournament.<br><br>[Hooks](../server-framework/basics.md#register-hooks) can be used to manage tournament reset and completion events. See the [Tournaments](../concepts/tournaments.md) documentation and [function reference](../server-framework/function-reference.md#tournaments) to learn more.

## Virtual currencies

| GameSparks | Nakama |
| :--------- | :----- |
| Currencies are supported as an integer value stored on the respective player account. Multiple currencies are supported. | A first-class API providing [wallet and ledger](../concepts/user-accounts.md#virtual-wallet) support is provided in Nakama. This enables wide-ranging game economies when combined with other Nakama features. For example adding currency gifting between players via [chat](../concepts/realtime-chat.md) or a player marketplace via the [Storage Engine](../concepts/collections.md).<br><br>Complete transaction history is provided in each user's ledger, accessible via the [script runtime](../server-framework/function-reference.md#wallets).<br><br>Additionally, the server framework can be used to implement any desired bonus structures, as in the below example. |

#### Examples

The following example shows how you can use hooks to reward a user with virtual currency after they have signed up with email authentication. You could register the same hook for multiple authentication methods or even reward users differently for registering/linking other authentication methods.

If you wish to extend this functionality and provide daily rewards to users who login, please read the [Daily Rewards](https://heroiclabs.com/docs/nakama/tutorials/server/daily-rewards/) guide.

Place the following in your `InitModule` function.

=== "TypeScript"
    ```typescript
    initializer.registerAfterAuthenticateEmail(afterAuthenticateEmail);
    ```
=== "Go"
    ```go
    initializer.RegisterAfterAuthenticateEmail(afterAuthenticateEmail)
    ```

Then add the hook handler function.

=== "TypeScript"
    ```typescript
    let afterAuthenticateEmail: nkruntime.AfterHookFunction<nkruntime.Session, nkruntime.AuthenticateEmailRequest> = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.Session, request: nkruntime.AuthenticateEmailRequest) {
        if (data.created) {
            let changeset = {
                coins: 1000
            };

            nk.walletUpdate(ctx.userId, changeset);
        }
    };

    initializer.registerAfterAuthenticateEmail(afterAuthenticateEmail);
    ```
=== "Go"
    ```go
    func afterAuthenticateEmail(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, out *api.Session, in *api.AuthenticateEmailRequest) error {
      // If the account was just created, reward the user with a sign up bonus of virtual currency.
      if out.Created {
        userID, _ := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
        changeset := map[string]int64{
          "coins": 1000,
        }

        if _, _, err := nk.WalletUpdate(ctx, userID, changeset, nil, false); err != nil {
          logger.Error("WalletUpdate error: %v", err)
          return err
        }
      }

      return nil
    }
    ```

To reward a user separately for linking another authentication method, such as Facebook, you would add a hook as shown below:

=== "TypeScript"
    ```typescript
    initializer.registerAfterLinkFacebook(afterLinkFacebook);
    ```
=== "Go"
    ```go
    initializer.RegisterAfterLinkFacebook(afterLinkFacebook)
    ```

## Virtual goods

<table>
<thead>
  <tr>
    <th>GameSparks</th>
    <th>Nakama</th>
  </tr>
</thead>
<tbody>
  <tr>
    <td>Virtual goods can be implemented in-game using defined virtual currency and via custom logic implementing the virtual goods API, or by integrating with third-party stores such as:<br>
    <ul>
        <li>Apple iTunes</li>
        <li>Google Play</li>
        <li>Sony PSN</li>
        <li>Steam</li>
        <li>XBoxOne</li>
    </ul></td>
    <td>Virtual goods can be implemented in any manner desired via the <a href="https://heroiclabs.com/docs/nakama/concepts/collections/">Storage API</a>. Nakama provides integrations for the following third-party stores:<br>
    <ul>
        <li>Apple iTunes</li>
        <li>Google Play</li>
        <li>Sony PSN (Enterprise Only)</li>
        <li>Steam (Enterprise Only)</li>
        <li>XBoxOne (Enterprise Only)</li>
        <li>Huawei</li>
    </ul><br>
    For Google Play and Apple iTunes integrations <a href="https://heroiclabs.com/docs/nakama/concepts/iap-validation/">in-app purchase validation</a> is built-in.</td>
  </tr>
</tbody>
</table>

## Achievements

| GameSparks | Nakama |
| :--------- | :----- |
| Achievements are an array of strings stored on a respective player account. Using triggers or custom code they can be implemented to deliver virtual currency or goods when awarded. | Using the Nakama [Storage Engine](../concepts/collections.md) and [Server Framework](../server-framework/basics.md) any manner of achievements, triggers, user messaging, and any custom reward system can be implemented to deliver engaging and rewarding gameplay. |

## Segments

| GameSparks | Nakama |
| :--------- | :----- |
| Used to provide a modified version of a particular game feature (e.g. virtual good) to the specified segments of players, for example localization. | Using the Nakama [Server Framework](../server-framework/basics.md), [Events](../concepts/events.md) can be routed and processed in any manner desired for your game and any targeted players. |

## Realtime servers

| GameSparks | Nakama |
| :--------- | :----- |
| GameSparks realtime servers were deprecated as of February 2019 along with the matchmaking features. For projects existing prior to that date, this service provided for server-authoritative multiplayer functionality. | Full built-in support for [client-relayed multiplayer](../concepts/client-relayed-multiplayer.md) and [server-authoritative multiplayer](../concepts/server-authoritative-multiplayer.md) is provided in Nakama. Integration with native [matchmaking](../concepts/matches.md) and support for leading client SDKs is also built-in. |

## Streams

| GameSparks | Nakama |
| :--------- | :----- |
| All GameSparks realtime features were deprecated in February 2019 and not available to projects created after that date. | [Streams](../server-framework/streams.md) are a powerful feature that give you control over the routing and delivery of all Nakama realtime features (e.g. chat, notifications). See the [built-in streams](../server-framework/streams.md#built-in-streams) for some examples. |

## File upload and download

| GameSparks | Nakama |
| :--------- | :----- |
| Support for uploading files and content (50MB/file limit) to S3 bucket, both by developer or client, which can then be made available for any other client for download. | Full support to read/write assets in any desired external object store is offered via the Nakama [Storage Engine](../concepts/collections.md) and [Server Framework](../server-framework/basics.md).<br><br>The full Go runtime and any third-party libraries are available via the Server Framework, enabling upload of files and content to any third-party asset store without size limitations. |

## Email

| GameSparks | Nakama |
| :--------- | :----- |
| Direct integration with SendGrid, enabling emails via cloud-code. | Integration via HTTP API for any provider (e.g. SendGrid, Mandrill), enabling emails via the [Server Framework](../server-framework/basics.md). |

## Administration

### Analytics

| GameSparks | Nakama |
| :--------- | :----- |
| Dashboard views for Instance and API Stream performance. Custom analytics can only be viewed in the Manage screens. | In addition to the [Nakama Console](../getting-started/console-overview.md) and [Heroic Cloud dashboards](../../heroic-cloud/projects.md#managing-projects), Nakama exposes Prometheus exports for Application Performance Monitoring (APM) metrics. Custom metrics can be implemented as desired.<br><br>Additionally, any desired third-party analytics can be added via [hooks](../server-framework/basics.md#register-hooks). |

### Data explorer

| GameSparks | Nakama |
| :--------- | :----- |
| Enables the querying and management of all collections. | [Nakama Console](../getting-started/console-overview.md) enables easy access to view, query, and manage your server instance and related data. |

### Groups permissions

| GameSparks | Nakama |
| :--------- | :----- |
| Allows multi-user collaboration and predefined permissions based upon assigned role. | [Nakama Console](../getting-started/console-overview.md) provides multi-user authentication and predefined roles, serving as groups with set Access Control List (ACL) permissions. |

### Manage screens

| GameSparks | Nakama |
| :--------- | :----- |
| Enables creation of custom management screens to, for example, manage players or transactions, view LiveOps data, update game settings and features. | [Nakama Console](../getting-started/console-overview.md) provides built-in access to view and manager game and player data. The Console can be further customized, if desired, either via the [Server Framework](../server-framework/basics.md) or custom dashboards (e.g. Retool) using the Console API. Access to the full Go runtime, provided via the Server Framework, can be used to create any HTTP endpoints for custom reports, screens, etc.<br><br>Additionally, since both Nakama and the Nakama Console are open source they can be easily extended with your desired custom functionality. |

### Regional deployments

| GameSparks | Nakama |
| :--------- | :----- |
| Enables deployment in any desired AWS region. | Heroic Cloud provides [multiple regions](https://heroiclabs.com/blog/tutorials/heroic-cloud-east-asia-region-announced/) for deployment. Nakama can be deployed to any public or private cloud, enabling you to select your desired provider and region. |

### Snapshots

| GameSparks | Nakama |
| :--------- | :----- |
| Enables creation of backups for the entire configuration. | Heroic Cloud enables reverting to previously deployed images, if needed. Continuous integration (CI) of game code, with rollback support, is provided.<br><br>Additionally, versioned data is supported by CockroachDB allowing rollback of any database-stored data to a previous version. |
