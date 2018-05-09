# Authentication

The server has builtin authentication so clients can only send requests and connect if they have the [server key](install-configuration.md#socket). When authentication is successful a client can create a session as a [user](user-accounts.md).

!!! Warning "Important"
    The default server key is `defaultkey` but it is very important to set a [unique value](install-configuration.md#socket). This value should be embedded within client code.

```js fct_label="Javascript"
var client = new nakamajs.Client("defaultkey", "127.0.0.1", 7350);
client.ssl = false;

// or same as above.
var client = new nakamajs.Client("defaultkey");
```

```csharp fct_label="Unity"
INClient client = new NClient.Builder("defaultkey")
    .Host("127.0.0.1")
    .Port(7350)
    .SSL(false)
    .Build();
// or same as above.
INClient client = NClient.Default("defaultkey");
```

```java fct_label="Android/Java"
Client client = DefaultClient.builder("defaultkey")
    .host("127.0.0.1")
    .port(7350)
    .ssl(false)
    .build();
// or same as above.
Client client = DefaultClient.defaults("defaultkey");
```

```swift fct_label="Swift"
let client : Client = Builder("defaultkey")
    .host("127.0.0.1")
    .port(7350)
    .ssl(false)
    .build()
// or same as above.
let client : Client = Builder.defaults(serverKey: "defaultkey")
```

Every user account is created from one of the [options used to authenticate](#authenticate). We call each of these options a "link" because it's a way to access the user's account. You can add more than one link to each account which is useful to enable users to login in multiple ways across different devices.

## Authenticate

Before you interact with the server, you must obtain a session token by authenticating with the system. The authentication system is very flexible. You could register a user with an email address, [link](#link-or-unlink) their Facebook account, and use it to login from another device.

!!! Note
    By default the system will create a user automatically if the identifier used to authenticate did not previously exist in the system. This pattern is shown in the [device](#device) section.

For a __full example__ on the best way to handle register and login in each of the clients have a look at their guides.

### Device

A device identifier can be used as a way to unobtrusively register a user with the server. This offers a frictionless user experience but can be unreliable because device identifiers can sometimes change in device updates.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

A device identifier must contain alphanumeric characters with dashes and be between 10 and 60 bytes.

```sh fct_label="cURL"
curl -X POST \
  --url http://127.0.0.1:7350/v2/account/authenticate/custom?create=true&username=mycustomusername \
  --header 'authorization: Basic ZGVmYXVsdGtleTo=' \
  --header 'content-type: application/json' \
  --data '{"id":"uniqueidentifier"}'
```

```js fct_label="Javascript"

// The following example is only applicable to React Native apps:
var deviceInfo = require('react-native-device-info');

var deviceId = null;
try {
  const value = await AsyncStorage.getItem('@MyApp:deviceKey');
  if (value !== null){
    deviceId = value
  } else {
    deviceId = deviceInfo.getUniqueID();
    AsyncStorage.setItem('@MyApp:deviceKey', deviceId).catch(function(error){
      console.log("An error occured: %o", error);
    });
  }
} catch (error) {
  console.log("An error occured: %o", error);
}

try {
  var session = await client.authenticateDevice({ id: deviceId, create: true, username: "mycustomusername" })
  console.log("Successfully authenticated, session: %o", session);
} catch(e) {
  console.log("Could not authenticate: %o", error);
}
```

```fct_label="REST"
POST /v2/account/authenticate/device?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "id": "uniqueidentifier"
}
```

```csharp fct_label="Unity"
var sessionHandler = delegate(INSession session) {
  Debug.LogFormat("Session: '{0}'.", session.Token);
};

var id = PlayerPrefs.GetString("nk.id");
if (string.IsNullOrEmpty(id)) {
  id = SystemInfo.deviceUniqueIdentifier;
  PlayerPrefs.SetString("nk.id", id); // cache device id.
}

var message = NAuthenticateMessage.Device(id);
client.Login(message, sessionHandler, (INError err) => {
  if (err.Code == ErrorCode.UserNotFound) {
    client.Register(message, sessionHandler, (INError err) => {
      Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
    });
  } else {
    Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
  }
});
```

```java fct_label="Android/Java"
String id = UUID.randomUUID().toString();
AuthenticateMessage message = AuthenticateMessage.Builder.device(id);
Deferred<Session> deferred = client.login(message);
deferred.addCallbackDeferring(new Callback<Deferred<Session>, Session>() {
  @Override
  public Deferred<Session> call(Session session) throws Exception {
    return client.connect(session);
  }
}).addErrback(new Callback<Deferred<Session>, Error>() {
  @Override
  public Deferred<Session> call(Error err) throws Exception {
    if (err.getCode() == Error.ErrorCode.USER_NOT_FOUND) {
      System.out.println("User not found, we'll register the user.");
      return client.register(message);
    }
    throw err;
  }
}).addCallbackDeferring(new Callback<Deferred<Session>, Session>() {
  @Override
  public Deferred<Session> call(Session session) throws Exception {
    return client.connect(session);
  }
}).addCallback(new Callback<Session, Session>() {
  @Override
  public Session call(Session session) throws Exception {
    System.out.format("Session connected: '%s'", session.getToken());
    return session;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
```

```swift fct_label="Swift"
let defaults = UserDefaults.standard
let deviceKey = "device_id"

var deviceId : String? = defaults.string(forKey: deviceKey)
if deviceId == nil {
  deviceId = UIDevice.current.identifierForVendor!.uuidString
  defaults.set(deviceId!, forKey: deviceKey)
}

let message = AuthenticateMessage(device: deviceId!)
client.login(with: message).then { session in
  print("Login successful")
}.catch{ err in
  if (err is NakamaError) {
    switch err as! NakamaError {
    case .userNotFound(_):
      let _ = self.client.register(with: message)
      return
    default:
      break
    }
  }
  print("Could not login: %@", err)
}
```

In games it is often a better option to use [Google](#google) or [Game Center](#game-center) to unobtrusively register the user.

### Email

Users can be registered with an email and password. The password is hashed before it's stored in the database server and cannot be read or "recovered" by administrators. This protects a user's privacy.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

An email address must be valid as defined by RFC-5322 and passwords must be at least 8 characters.

```sh fct_label="cURL"
curl -X POST \
  --url http://127.0.0.1:7350/v2/account/authenticate/email?create=true&username=mycustomusername \
  --header 'authorization: Basic ZGVmYXVsdGtleTo=' \
  --header 'content-type: application/json' \
  --data '{"email":"email@example.com", "password": "3bc8f72e95a9"}'
```

```js fct_label="Javascript"
var email = "email@example.com"
var password = "3bc8f72e95a9"

try {
  var session = await client.authenticateEmail({ email: email, password: password, create: true, username: "mycustomusername" })
  console.log("Successfully authenticated, session: %o", session);
} catch(e) {
  console.log("Could not authenticate: %o", error);
}
```

```fct_label="REST"
POST /v2/account/authenticate/email?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "email": "email@example.com",
  "password": "password"
}
```

```csharp fct_label="Unity"
string email = "email@example.com"
string password = "3bc8f72e95a9"

var message = NAuthenticateMessage.Email(email, password);
client.Register(message, (INSession session) => {
  Debug.LogFormat("Session: '{0}'.", session.Token);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
// Use client.Login(...) after register.
```

```java fct_label="Android/Java"
String email = "email@example.com"
String password = "3bc8f72e95a9"

AuthenticateMessage message = AuthenticateMessage.Builder.email(email, password);
Deferred<Session> deferred = client.register(message);
deferred.addCallback(new Callback<Session, Session>() {
  @Override
  public Session call(Session session) throws Exception {
    System.out.format("Session: '%s'", session.getToken());
    return session;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
// Use client.login(...) after register.
```

```swift fct_label="Swift"
let email = "email@example.com"
let password = "3bc8f72e95a9"

let message = AuthenticateMessage(email: email, password: password)
client.register(with: message).then { session in
  NSLog("Session: %@", session.token)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
// Use client.login(...) after register.
```

### Social providers

The server supports a lot of different social services with register and login. With each provider the user account will be fetched from the social service and used to setup the user. In some cases a user's [friends](social-friends.md) will also be fetched and added to their friends list.

To register or login as a user with any of the providers an OAuth or access token must be obtained from that social service.

#### Facebook

With Facebook you'll need to add the Facebook SDK to your project which can be <a href="https://developers.facebook.com/docs/" target="\_blank">downloaded online</a>. Follow their guides on how to integrate the code. With a mobile project you'll also need to complete instructions on how to configure iOS and Android.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

You can optionally import Facebook friends into Nakama's [friend graph](social-friends.md) when authenticating. To do this, set `import` to true.

```sh fct_label="cURL"
curl -X POST \
  --url http://127.0.0.1:7350/v2/account/authenticate/facebook?create=true&username=mycustomusername&import=true \
  --header 'authorization: Basic ZGVmYXVsdGtleTo=' \
  --header 'content-type: application/json' \
  --data '{"token":"valid-oauth-token"}'
```

```js fct_label="Javascript"
var oauthToken = "...";

try {
  var session = await client.authenticateFacebook({ token: oauthToken, create: true, username: "mycustomusername", import: true })
  console.log("Successfully authenticated, session: %o", session);
} catch(e) {
  console.log("Could not authenticate: %o", error);
}
```

```fct_label="REST"
POST /v2/account/authenticate/facebook?create=true&username=mycustomusername&import=true
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "token": "...",
}
```

```csharp fct_label="Unity"
Action<INSession> sessionHandler = delegate(INSession session) {
  Debug.LogFormat("Session: '{0}'.", session.Token);
  client.Connect(session);
};

var initCallback = delegate() {
  if (FB.IsInitialized) {
    FB.ActivateApp();
    // use a Facebook access token to create a user account.
    var oauthToken = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString;

    var message = NAuthenticateMessage.Facebook(oauthToken);
    client.Login(message, sessionHandler, (INError err) => {
      if (err.Code == ErrorCode.UserNotFound) {
        client.Register(message, sessionHandler, (INError err) => {
          Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
        });
      } else {
        Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
      }
    });
  }
};

// you must call FB.Init as early as possible at startup.
if (!FB.IsInitialized) {
  FB.Init(initCallback);
}
```

```swift fct_label="Swift"
let oauthToken = "..."

let message = AuthenticateMessage(facebook: oauthToken)
client.register(with: message).then { session in
  NSLog("Session: %@", session.token)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

You can add a button to your UI to login with Facebook.

```csharp fct_label="Unity"
Action<INSession> sessionHandler = delegate(INSession session) {
  Debug.LogFormat("Session: '{0}'.", session.Token);
  client.Connect(session);
};

FB.Login("email", (ILoginResult result) => {
  if (FB.IsLoggedIn) {
    var oauthToken = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString;

    var message = NAuthenticateMessage.Facebook(oauthToken);
    client.Login(message, sessionHandler, (INError err) => {
      if (err.Code == ErrorCode.UserNotFound) {
        client.Register(message, sessionHandler, (INError err) => {
          Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
        });
      } else {
        Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
      }
    });
  } else {
    Debug.LogErrorFormat("Facebook login failed got '{0}'.", result.Error);
  }
});
```

#### Google

Similar to Facebook for register and login you should use one of Google's client SDKs.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

```sh fct_label="cURL"
curl -X POST \
  --url http://127.0.0.1:7350/v2/account/authenticate/google?create=true&username=mycustomusername \
  --header 'authorization: Basic ZGVmYXVsdGtleTo=' \
  --header 'content-type: application/json' \
  --data '{"token":"valid-oauth-token"}'
```

```js fct_label="Javascript"
var oauthToken = "...";

try {
  var session = await client.authenticateGoogle({ token: oauthToken, create: true, username: "mycustomusername" })
  console.log("Successfully authenticated, session: %o", session);
} catch(e) {
  console.log("Could not authenticate: %o", error);
}
```

```fct_label="REST"
POST /v2/account/authenticate/google?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "token": "...",
}
```

```csharp fct_label="Unity"
string oauthToken = "...";

var message = NAuthenticateMessage.Google(oauthToken);
client.Register(message, (INSession session) => {
  Debug.LogFormat("Session: '{0}'.", session.Token);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
// Use client.Login(...) after register.
```

```java fct_label="Android/Java"
String oauthToken = "...";

AuthenticateMessage message = AuthenticateMessage.Builder.google(oauthToken);
Deferred<Session> deferred = client.register(message);
deferred.addCallback(new Callback<Session, Session>() {
  @Override
  public Session call(Session session) throws Exception {
    System.out.format("Session: '%s'", session.getToken());
    return session;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
// Use client.login(...) after register.
```

```swift fct_label="Swift"
let oauthToken = "..."

let message = AuthenticateMessage(google: oauthToken)
client.register(with: message).then { session in
  NSLog("Session: %@", session.token)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

#### Game Center

Apple devices have builtin authentication which can be done without user interaction through Game Center. The register or login process is a little complicated because of how Apple's services work.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

```sh fct_label="cURL"
curl -X POST \
  --url http://127.0.0.1:7350/v2/account/authenticate/gamecenter?create=true&username=mycustomusername \
  --header 'authorization: Basic ZGVmYXVsdGtleTo=' \
  --header 'content-type: application/json' \
  --data '{"player_id":"...", "bundle_id":"...", "timestamp_seconds":0, "salt":"...", "public_key_url":"..."}'
```

```fct_label="REST"
POST /v2/account/authenticate/gamecenter?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "player_id": "...",
  "bundle_id": "...",
  "timestamp_seconds": 0,
  "salt": "...",
  "public_key_url": "..."
}
```

```csharp fct_label="Unity"
// You'll need to use native code (Obj-C) with Unity.
// The "UnityEngine.SocialPlatforms.GameCenter" doesn't give enough information
// to enable authentication.

// We recommend you use a library which handles native Game Center auth like
// https://github.com/desertkun/GameCenterAuth

string playerId = "...";
string bundleId = "...";
string base64salt = "...";
string base64signature = "...";
string publicKeyUrl = "...";
long timestamp = 0L;

var message = NAuthenticateMessage.GameCenter(
    playerId, bundleId, timestamp, base64salt, base64signature, publicKeyUrl);
client.Register(message, (INSession session) => {
  Debug.LogFormat("Session: '{0}'.", session.Token);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
// Use client.Login(...) after register.
```

```swift fct_label="Swift"
let playerID : String = "..."
let bundleID : String = "..."
let base64salt : String = "..."
let base64signature : String = "..."
let publicKeyURL : String = "..."
let timestamp : Int = 0

let message = AuthenticateMessage(
    gamecenter: bundleID, playerID: playerID, publicKeyURL: publicKeyURL,
    salt: base64salt, timestamp: timestamp, signature: base64signature)
client.register(with: message).then { session in
  NSLog("Session: %@", session.token)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
// Use client.login(...) after register.
```

#### Steam

Steam requires you to configure the server before you can register a user.

!!! Note "Server configuration"
    Have a look at the [configuration](install-configuration.md) section for what settings you need for the server.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

```sh fct_label="cURL"
curl -X POST \
  --url http://127.0.0.1:7350/v2/account/authenticate/steam?create=true&username=mycustomusername \
  --header 'authorization: Basic ZGVmYXVsdGtleTo=' \
  --header 'content-type: application/json' \
  --data '{"token":"valid-steam-token"}'
```

```fct_label="REST"
POST /v2/account/authenticate/steam?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "token": "...",
}
```

```csharp fct_label="Unity"
string sessionToken = "...";

var message = NAuthenticateMessage.Steam(sessionToken);
client.Register(message, (INSession session) => {
  Debug.LogFormat("Session: '{0}'.", session.Token);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
// Use client.Login(...) after register.
```

```java fct_label="Android/Java"
String sessionToken = "...";

AuthenticateMessage message = AuthenticateMessage.Builder.steam(sessionToken);
Deferred<Session> deferred = client.register(message);
deferred.addCallback(new Callback<Session, Session>() {
  @Override
  public Session call(Session session) throws Exception {
    System.out.format("Session: '%s'", session.getToken());
    return session;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
// Use client.login(...) after register.
```

```swift fct_label="Swift"
let sessionToken = "..."

let message = AuthenticateMessage(steam: sessionToken)
client.register(with: message).then { session in
  NSLog("Session: %@", session.token)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
// Use client.login(...) after register.
```

### Custom

A custom identifier can be used in a similar way to a device identifier to login or register a user. This option should be used if you have an external or custom user identity service which you want to use. For example EA's Origin service handles accounts which have their own user IDs.

A custom identifier must contain alphanumeric characters with dashes and be between 10 and 60 bytes.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

```sh fct_label="cURL"
curl -X POST \
  --url http://127.0.0.1:7350/v2/account/authenticate/custom?create=true&username=mycustomusername \
  --header 'authorization: Basic ZGVmYXVsdGtleTo=' \
  --header 'content-type: application/json' \
  --data '{"id":"some-custom-id"}'
```

```js fct_label="Javascript"
var customId = "some-custom-id";

try {
  var session = await client.authenticateCustom({ id: customId, create: true, username: "mycustomusername" })
  console.log("Successfully authenticated, session: %o", session);
} catch(e) {
  console.log("Could not authenticate: %o", error);
}
```

```fct_label="REST"
POST /v2/account/authenticate/custom?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "id": "...",
}
```

```csharp fct_label="Unity"
// Some id from another service.
string customId = "a1fca336-7191-11e7-bdab-df34f6f90285";

var message = NAuthenticateMessage.Custom(customId);
client.Register(message, (INSession session) => {
  Debug.LogFormat("Session: '{0}'.", session.Token);
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
// Use client.Login(...) after register.
```

```java fct_label="Android/Java"
// Some id from another service.
String customId = "a1fca336-7191-11e7-bdab-df34f6f90285";

AuthenticateMessage message = AuthenticateMessage.Builder.google(customId);
Deferred<Session> deferred = client.register(message);
deferred.addCallback(new Callback<Session, Session>() {
  @Override
  public Session call(Session session) throws Exception {
    System.out.format("Session: '%s'", session.getToken());
    return session;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
// Use client.login(...) after register.
```

```swift fct_label="Swift"
// Some id from another service.
let customID = "a1fca336-7191-11e7-bdab-df34f6f90285"

let message = AuthenticateMessage(custom: customID)
client.register(with: message).then { session in
  NSLog("Session: %@", session.token)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
// Use client.login(...) after register.
```

## Sessions

The register and login messages return a session on success. The session contains the current user's ID and handle as well as information on when it was created and when it expires.

!!! Tip
    You can change how long a session token is valid before it expires in the [configuration](install-configuration.md) in the server. By default a session is only valid for 60 seconds.

```js fct_label="Javascript"
var id = "3e70fd52-7192-11e7-9766-cb3ce5609916";

try {
  var session = await client.authenticateDevice({ id: id })
  console.log("Session id '%o', handle '%o.", session.user_id, session.handle);
  console.log("Session expired: %o", session.isexpired(new Date()));
} catch(e) {
  console.log("Could not authenticate: %o", error);
}
```

```csharp fct_label="Unity"
string id = "3e70fd52-7192-11e7-9766-cb3ce5609916";
var message = NAuthenticateMessage.Device(id);
client.Login(message, (INSession session) => {
  var userId = session.Id;
  Debug.LogFormat("Session id '{0}' handle '{1}'.", userId, session.Handle);
  Debug.LogFormat("Session expired: {0}", session.HasExpired(DateTime.UtcNow));
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
String id = "3e70fd52-7192-11e7-9766-cb3ce5609916";
CollatedMessage<Session> message = AuthenticateMessage.Builder.device(id);
Deferred<Session> deferred = client.login(message);
deferred.addCallback(new Callback<Session, Session>() {
  @Override
  public Session call(Session session) throws Exception {
    String userId = new String(session.getId());
    System.out.format("Session id '%s' handle '%s'", userId, session.getHandle());
    long now = System.currentTimeMillis();
    System.out.format("Session expired: '%s'", session.IsExpired(now));
    return session;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
```

```swift fct_label="Swift"
let id = "3e70fd52-7192-11e7-9766-cb3ce5609916"
let message = AuthenticateMessage(device: id)
client.login(with: message).then { session in
  let expired = session.isExpired(currentTimeSince1970: Date().timeIntervalSince1970)
  NSLog("Session id '%@' handle '%@'.", session.userID, session.handle)
  NSLog("Session expired: '%@'", expired)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

### Connect

With a session you can connect with the server and exchange realtime messages. Most of our clients do not auto-reconnect for you so you should handle it with your own code.

You can only send messages to the server once you've connected a client.

```js fct_label="Javascript"
var session = ... // obtained by authentication
var socket = client.createSocket();
try {
  await socket.connect(session);
  console.log("Successfully connected.");
} catch(error) {
  console.log("An error occured: %o", error);
}
```

```csharp fct_label="Unity"
INSession session = someSession; // obtained from Register or Login.
client.Connect(session, (bool done) => {
  Debug.Log("Successfully connected.");
});
```

```java fct_label="Android/Java"
Session session = someSession; // obtained from register or login.
Deferred<Session> deferred = client.connect(session);
deferred.addCallback(new Callback<Session, Session>() {
  @Override
  public Session call(Session session) throws Exception {
    System.out.println("Successfully connected.");
    return session;
  }
});
```

```swift fct_label="Swift"
let session : Session = someSession // obtained from register or login.
client.connect(with: session).then { _ in
  NSLog("Successfully connected.")
});
```

## Link or unlink

You can link one or more other login option to the current user. This makes it easy to support multiple logins with each user and easily identify a user across devices.

You can only link device Ids, custom Ids, and social provider IDs which are not already in-use with another user account.

```sh fct_label="cURL"
curl -X POST \
  --url http://127.0.0.1:7350/v2/account/link/custom \
  --header 'Accept: application/json' \
  --header 'Authorization: Bearer $session' \
  --header 'Content-Type: application/json' \
  --data '{"id":"some-custom-id"}'
```

```js fct_label="Javascript"
var customId = "some-custom-id";

try {
  var success = await client.linkCustom(session, { id: customId });
  console.log("Successfully linked custom ID to current user.");
} catch(e) {
  console.log("An error occured: %o", error);
}
```

```fct_label="REST"
POST /v2/account/link/custom
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer SessionToken

{
  "id":"some-custom-id"
}
```

```csharp fct_label="Unity"
string id = "062b0916-7196-11e7-8371-9fcee9f0b20c";

var message = NSelfLinkMessage.Device(id);
client.Send(message, (bool done) => {
  Debug.Log("Successfully linked device ID to current user.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
String id = "062b0916-7196-11e7-8371-9fcee9f0b20c";

CollatedMessage<Session> message = SelfLinkMessage.Builder.device(id);
Deferred<Boolean> deferred = client.send(message);
deferred.addCallback(new Callback<Boolean, Boolean>() {
  @Override
  public Boolean call(Boolean done) throws Exception {
    System.out.println("Successfully linked device ID to current user.");
    return done;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
```

```swift fct_label="Swift"
let id = "062b0916-7196-11e7-8371-9fcee9f0b20c"

var message = SelfLinkMessage(device: id);
client.send(with: message).then {
  NSLog("Successfully linked device ID to current user.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

You can unlink any linked login options for the current user.

```sh fct_label="cURL"
curl -X POST \
  --url http://127.0.0.1:7350/v2/account/unlink/custom \
  --header 'Accept: application/json' \
  --header 'Authorization: Bearer $session' \
  --header 'Content-Type: application/json' \
  --data '{"id":"some-custom-id"}'
```

```js fct_label="Javascript"
var customId = "some-custom-id";

try {
  var success = await client.unlinkCustom(session, { id: customId });
  console.log("Successfully unlinked custom ID from the current user.");
} catch(e) {
  console.log("An error occured: %o", error);
}
```

```fct_label="REST"
POST /v2/account/unlink/custom
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer SessionToken

{
  "id":"some-custom-id"
}
```

```csharp fct_label="Unity"
string id = "062b0916-7196-11e7-8371-9fcee9f0b20c";

var message = NSelfUnlinkMessage.Device(id);
client.Send(message, (bool done) => {
  Debug.Log("Successfully unlinked device ID from current user.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

```java fct_label="Android/Java"
String id = "062b0916-7196-11e7-8371-9fcee9f0b20c";

CollatedMessage<Session> message = SelfUnlinkMessage.Builder.device(id);
Deferred<Boolean> deferred = client.send(message);
deferred.addCallback(new Callback<Boolean, Boolean>() {
  @Override
  public Boolean call(Boolean done) throws Exception {
    System.out.println("Successfully unlinked device ID from current user.");
    return done;
  }
}).addErrback(new Callback<Error, Error>() {
  @Override
  public Error call(Error err) throws Exception {
    System.err.format("Error('%s', '%s')", err.getCode(), err.getMessage());
    return err;
  }
});
```

```swift fct_label="Swift"
let id = "062b0916-7196-11e7-8371-9fcee9f0b20c"

var message = SelfUnlinkMessage(device: id);
client.send(with: message).then {
  NSLog("Successfully unlinked device ID from current user.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

You can link or unlink many different account options.

| Link | Description |
| ---- | ----------- |
| Custom | A custom identifier from another identity service. |
| Device | A unique identifier for a device which belongs to the user. |
| Email | An email and password set by the user. |
| Facebook | A Facebook social account. You can optionally import Facebook Friends upon linking. |
| Game Center | An account from Apple's Game Center service. |
| Google | A Google Play social account. |
| Steam | An account from the Steam network. |
