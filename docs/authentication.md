# Authentication

The server has builtin authentication so clients can only send requests and connect if they have the [server key](install-configuration.md#socket). When authentication is successful a client can create a session as a [user](user-accounts.md).

!!! Warning "Important"
    The default server key is `defaultkey` but it is very important to set a [unique value](install-configuration.md#socket). This value should be embedded within client code.

```js tab="JavaScript"
var client = new nakamajs.Client("defaultkey", "127.0.0.1", 7350);
client.ssl = false;
```

```csharp tab=".NET"
// Use "https" scheme if you've setup SSL.
var client = new Client("http", "127.0.0.1", 7350, "defaultkey");
```

```csharp tab="Unity"
// Use "https" scheme if you've setup SSL.
var client = new Client("http", "127.0.0.1", 7350, "defaultkey");
```

```cpp tab="Cocos2d-x C++"
NClientParameters parameters;
parameters.serverKey = "defaultkey";
parameters.host = "127.0.0.1";
parameters.port = DEFAULT_PORT;
NClientPtr client = NCocosHelper::createDefaultClient(parameters);
```

```js tab="Cocos2d-x JS"
var serverkey = "defaultkey";
var host = "127.0.0.1";
var port = 7350;
var useSSL = false;
var timeout = 7000; // ms

var client = new nakamajs.Client(serverkey, host, port, useSSL, timeout);
```

```cpp tab="C++"
NClientParameters parameters;
parameters.serverKey = "defaultkey";
parameters.host = "127.0.0.1";
parameters.port = DEFAULT_PORT;
NClientPtr client = createDefaultClient(parameters);
```

```java tab="Java"
Client client = new DefaultClient("defaultkey", "127.0.0.1", 7349, false)
// or same as above.
Client client = DefaultClient.defaults("defaultkey");
```

```swift tab="Swift"
// Requires Nakama 1.x
let client : Client = Builder("defaultkey")
    .host("127.0.0.1")
    .port(7350)
    .ssl(false)
    .build()
// or same as above.
let client : Client = Builder.defaults(serverKey: "defaultkey")
```

```gdscript tab="Godot"
onready var client := Nakama.create_client("defaultkey", "127.0.0.1", 7350, "http")
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

A device identifier must contain alphanumeric characters with dashes and be between 10 and 128 bytes.

```sh tab="cURL"
curl "http://127.0.0.1:7350/v2/account/authenticate/device?create=true&username=mycustomusername" \
  --user 'defaultkey:' \
  --data '{"id":"uniqueidentifier"}'
```

```js tab="JavaScript"
// This import is only required with React Native
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

const session = await client.authenticateDevice({ id: deviceId, create: true, username: "mycustomusername" });
console.info("Successfully authenticated:", session);
```

```csharp tab=".NET"
// Should use a platform API to obtain a device identifier.
var deviceId = System.Guid.NewGuid().ToString();
var session = await client.AuthenticateDeviceAsync(deviceId);
System.Console.WriteLine("New user: {0}, {1}", session.Created, session);
```

```csharp tab="Unity"
var deviceId = PlayerPrefs.GetString("nakama.deviceid");
if (string.IsNullOrEmpty(deviceId)) {
    deviceId = SystemInfo.deviceUniqueIdentifier;
    PlayerPrefs.SetString("nakama.deviceid", deviceId); // cache device id.
}
var session = await client.AuthenticateDeviceAsync(deviceId);
Debug.LogFormat("New user: {0}, {1}", session.Created, session);
```

```cpp tab="Cocos2d-x C++"
auto loginFailedCallback = [](const NError& error)
{
};

auto loginSucceededCallback = [](NSessionPtr session)
{
  CCLOG("Successfully authenticated");
};

std::string deviceId = "unique device id";

client->authenticateDevice(
        deviceId,
        opt::nullopt,
        opt::nullopt,
        {},
        loginSucceededCallback,
        loginFailedCallback);
```

```js tab="Cocos2d-x JS"
var deviceId = "unique device id";
client.authenticateDevice({ id: deviceId, create: true, username: "mycustomusername" })
  .then(function(session) {
        cc.log("Authenticated successfully. User id:", session.user_id);
    },
    function(error) {
        cc.error("authenticate failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto loginFailedCallback = [](const NError& error)
{
};

auto loginSucceededCallback = [](NSessionPtr session)
{
  cout << "Successfully authenticated" << endl;
};

std::string deviceId = "unique device id";

client->authenticateDevice(
        deviceId,
        opt::nullopt,
        opt::nullopt,
        {},
        loginSucceededCallback,
        loginFailedCallback);
```

```java tab="Java"
String id = UUID.randomUUID().toString();
Session session = client.authenticateDevice(id).get();
System.out.format("Session: %s ", session.getAuthToken());
```

```swift tab="Swift"
// Requires Nakama 1.x
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

```gdscript tab="Godot"
# Unique ID is not supported by Godot in HTML5, use a different way to generate an id, or a different authentication option.
var deviceid = OS.get_unique_id()
var session : NakamaSession = yield(client.authenticate_device_async(deviceid), "completed")
if session.is_exception():
	print("An error occured: %s" % session)
	return
print("Successfully authenticated: %s" % session)
```

```tab="REST"
POST /v2/account/authenticate/device?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
    "id": "uniqueidentifier"
}
```

In games it is often a better option to use [Google](#google) or [Game Center](#game-center) to unobtrusively register the user.

### Email

Users can be registered with an email and password. The password is hashed before it's stored in the database server and cannot be read or "recovered" by administrators. This protects a user's privacy.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

An email address must be valid as defined by RFC-5322 and passwords must be at least 8 characters.

```sh tab="cURL"
curl "http://127.0.0.1:7350/v2/account/authenticate/email?create=true&username=mycustomusername" \
  --user 'defaultkey:' \
  --data '{"email":"email@example.com", "password": "3bc8f72e95a9"}'
```

```js tab="JavaScript"
const email = "email@example.com";
const password = "3bc8f72e95a9";
const session = await client.authenticateEmail({ email: email, password: password, create: true, username: "mycustomusername" })
console.info("Successfully authenticated:", session);
```

```csharp tab=".NET"
const string email = "email@example.com";
const string password = "3bc8f72e95a9";
var session = await client.AuthenticateEmailAsync(email, password);
System.Console.WriteLine("New user: {0}, {1}", session.Created, session);
```

```csharp tab="Unity"
const string email = "email@example.com";
const string password = "3bc8f72e95a9";
var session = await client.AuthenticateEmailAsync(email, password);
Debug.LogFormat("New user: {0}, {1}", session.Created, session);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = [](NSessionPtr session)
{
  CCLOG("Authenticated successfully. User ID: %s", session->getUserId().c_str());
};

auto errorCallback = [](const NError& error)
{
};

string email = "email@example.com";
string password = "3bc8f72e95a9";
string username = "mycustomusername";
bool create = true;
client->authenticateEmail(email, password, username, create, {}, successCallback, errorCallback);
```

```js tab="Cocos2d-x JS"
const email = "email@example.com";
const password = "3bc8f72e95a9";
client.authenticateEmail({ email: email, password: password, create: true, username: "mycustomusername" })
  .then(function(session) {
      cc.log("Authenticated successfully. User ID:", session.user_id);
    },
    function(error) {
      cc.error("authenticate failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = [](NSessionPtr session)
{
    std::cout << "Authenticated successfully. User ID: " << session->getUserId() << std::endl;
};

auto errorCallback = [](const NError& error)
{
};

string email = "email@example.com";
string password = "3bc8f72e95a9";
string username = "mycustomusername";
bool create = true;
client->authenticateEmail(email, password, username, create, {}, successCallback, errorCallback);
```

```java tab="Java"
String email = "email@example.com";
String password = "3bc8f72e95a9";
Session session = client.authenticateEmail(email, password).get();
System.out.format("Session: %s ", session.getAuthToken());
```

```swift tab="Swift"
// Requires Nakama 1.x
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

```gdscript tab="Godot"
var email = "email@example.com"
var password = "3bc8f72e95a9"
var session : NakamaSession = yield(client.authenticate_email_async(email, password, "mycustomusername", true), "completed")
if session.is_exception():
	print("An error occured: %s" % session)
	return
print("Successfully authenticated: %s" % session)
```

```tab="REST"
POST /v2/account/authenticate/email?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
    "email": "email@example.com",
    "password": "3bc8f72e95a9"
}
```

### Social providers

The server supports a lot of different social services with register and login. With each provider the user account will be fetched from the social service and used to setup the user. In some cases a user's [friends](social-friends.md) will also be fetched and added to their friends list.

To register or login as a user with any of the providers an OAuth or access token must be obtained from that social service.

#### Facebook

With Facebook you'll need to add the Facebook SDK to your project which can be <a href="https://developers.facebook.com/docs/" target="\_blank">downloaded online</a>. Follow their guides on how to integrate the code. With a mobile project you'll also need to complete instructions on how to configure iOS and Android.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

You can optionally import Facebook friends into Nakama's [friend graph](social-friends.md) when authenticating. To do this, set `import` to true.

```sh tab="cURL"
curl "http://127.0.0.1:7350/v2/account/authenticate/facebook?create=true&username=mycustomusername&import=true" \
  --user 'defaultkey:' \
  --data '{"token":"valid-oauth-token"}'
```

```js tab="JavaScript"
const oauthToken = "...";
const session = await client.authenticateFacebook({ token: oauthToken, create: true, username: "mycustomusername", import: true });
console.log("Successfully authenticated:", session);
```

```csharp tab=".NET"
const string oauthToken = "...";
var session = await client.AuthenticateFacebookAsync(oauthToken);
System.Console.WriteLine("New user: {0}, {1}", session.Created, session);
```

```csharp tab="Unity"
// using Facebook.Unity;
// https://developers.facebook.com/docs/unity/examples#init
var perms = new List<string>(){"public_profile", "email"};
FB.LogInWithReadPermissions(perms, async (ILoginResult result) => {
    if (FB.IsLoggedIn) {
        var accessToken = Facebook.Unity.AccessToken.CurrentAccessToken;
        var session = await client.LinkFacebookAsync(session, accessToken);
        Debug.LogFormat("New user: {0}, {1}", session.Created, session);
    }
});
```

```cpp tab="Cocos2d-x C++"
auto loginFailedCallback = [](const NError& error)
{
};

auto loginSucceededCallback = [](NSessionPtr session)
{
  CCLOG("Authenticated successfully. User ID: %s", session->getUserId().c_str());
};

std::string oauthToken = "...";
bool importFriends = true;

client->authenticateFacebook(
        oauthToken,
        "mycustomusername",
        true,
        importFriends,
        {},
        loginSucceededCallback,
        loginFailedCallback);
```

```js tab="Cocos2d-x JS"
const oauthToken = "...";
client.authenticateFacebook({ token: oauthToken, create: true, username: "mycustomusername", import: true })
  .then(function(session) {
      cc.log("Authenticated successfully. User ID:", session.user_id);
    },
    function(error) {
      cc.error("authenticate failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto loginFailedCallback = [](const NError& error)
{
};

auto loginSucceededCallback = [](NSessionPtr session)
{
  cout << "Authenticated successfully. User ID: " << session->getUserId() << endl;
};

std::string oauthToken = "...";
bool importFriends = true;

client->authenticateFacebook(
        oauthToken,
        "mycustomusername",
        true,
        importFriends,
        {},
        loginSucceededCallback,
        loginFailedCallback);
```

```java tab="Java"
String oauthToken = "...";
Session session = client.authenticateFacebook(oauthToken).get();
System.out.format("Session %s", session.getAuthToken());
```

```swift tab="Swift"
// Requires Nakama 1.x
let oauthToken = "..."
let message = AuthenticateMessage(facebook: oauthToken)
client.register(with: message).then { session in
  NSLog("Session: %@", session.token)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```gdscript tab="Godot"
var oauth_token = "..."
var session : NakamaSession = yield(client.authenticate_facebook_async(oauth_token), "completed")
if session.is_exception():
	print("An error occured: %s" % session)
	return
print("Successfully authenticated: %s" % session)
```

```tab="REST"
POST /v2/account/authenticate/facebook?create=true&username=mycustomusername&import=true
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "token": "...",
}
```

You can add a button to your UI to login with Facebook.

```csharp tab="Unity"
FB.Login("email", (ILoginResult result) => {
  if (FB.IsLoggedIn) {
    var oauthToken = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString;
    var session = await client.LinkFacebookAsync(session, accesstoken);
    Debug.LogFormat("Session: '{0}'.", session.AuthToken);
});
```

#### Google

Similar to Facebook for register and login you should use one of Google's client SDKs.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

```sh tab="cURL"
curl "http://127.0.0.1:7350/v2/account/authenticate/google?create=true&username=mycustomusername" \
  --user 'defaultkey:' \
  --data '{"token":"valid-oauth-token"}'
```

```js tab="JavaScript"
const playerIdToken = "...";
const session = await client.authenticateGoogle({ token: oauthToken, create: true, username: "mycustomusername" });
console.info("Successfully authenticated: %o", session);
```

```csharp tab=".NET"
const string playerIdToken = "...";
var session = await client.AuthenticateGoogleAsync(playerIdToken);
System.Console.WriteLine("New user: {0}, {1}", session.Created, session);
```

```csharp tab="Unity"
const string playerIdToken = "...";
var session = await client.AuthenticateGoogleAsync(playerIdToken);
Debug.LogFormat("New user: {0}, {1}", session.Created, session);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = [](NSessionPtr session)
{
  CCLOG("Authenticated successfully. User ID: %s", session->getUserId().c_str());
};

auto errorCallback = [](const NError& error)
{
};

string oauthToken = "...";
client->authenticateGoogle(oauthToken, "mycustomusername", true, {}, successCallback, errorCallback);
```

```js tab="Cocos2d-x JS"
const oauthToken = "...";
client.authenticateGoogle({ token: oauthToken, create: true, username: "mycustomusername" })
  .then(function(session) {
      cc.log("Authenticated successfully. User ID:", session.user_id);
    },
    function(error) {
      cc.error("authenticate failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = [](NSessionPtr session)
{
    std::cout << "Authenticated successfully. User ID: " << session->getUserId() << std::endl;
};

auto errorCallback = [](const NError& error)
{
};

string oauthToken = "...";
client->authenticateGoogle(oauthToken, "mycustomusername", true, {}, successCallback, errorCallback);
```

```java tab="Java"
String playerIdToken = "...";
Session session = client.authenticateGoogle(oauthToken).get();
System.out.format("Session %s", session.getAuthToken());
```

```swift tab="Swift"
// Requires Nakama 1.x
let playerIdToken = "..."
let message = AuthenticateMessage(google: oauthToken)
client.register(with: message).then { session in
  NSLog("Session: %@", session.token)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```gdscript tab="Godot"
var oauth_token = "..."
var session : NakamaSession = yield(client.authenticate_google_async(oauth_token), "completed")
if session.is_exception():
	print("An error occured: %s" % session)
	return
print("Successfully authenticated: %s" % session)
```

```tab="REST"
POST /v2/account/authenticate/google?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "token": "...",
}
```

#### Game Center

Apple devices have builtin authentication which can be done without user interaction through Game Center. The register or login process is a little complicated because of how Apple's services work.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

```sh tab="cURL"
curl "http://127.0.0.1:7350/v2/account/authenticate/gamecenter?create=true&username=mycustomusername" \
  --user 'defaultkey:' \
  --data '{"player_id":"...", "bundle_id":"...", "timestamp_seconds":0, "salt":"...", "public_key_url":"..."}'
```

```csharp tab=".NET"
var bundleId = "...";
var playerId = "...";
var publicKeyUrl = "...";
var salt = "...";
var signature = "...";
var timestamp = "...";
var session = await client.AuthenticateGameCenterAsync(bundleId, playerId,
    publicKeyUrl, salt, signature, timestamp);
System.Console.WriteLine("New user: {0}, {1}", session.Created, session);
```

```csharp tab="Unity"
// You'll need to use native code (Obj-C) with Unity.
// The "UnityEngine.SocialPlatforms.GameCenter" doesn't give enough information
// to enable authentication.

// We recommend you use a library which handles native Game Center auth like
// https://github.com/desertkun/GameCenterAuth

var bundleId = "...";
var playerId = "...";
var publicKeyUrl = "...";
var salt = "...";
var signature = "...";
var timestamp = "...";
var session = await client.AuthenticateGameCenterAsync(bundleId, playerId,
    publicKeyUrl, salt, signature, timestamp);
Debug.LogFormat("New user: {0}, {1}", session.Created, session);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = [](NSessionPtr session)
{
  CCLOG("Authenticated successfully. User ID: %s", session->getUserId().c_str());
};

auto errorCallback = [](const NError& error)
{
};

std::string playerId = "...";
std::string	bundleId = "...";
NTimestamp timestampSeconds = "...";
std::string salt = "...";
std::string signature = "...";
std::string publicKeyUrl = "...";

client->authenticateGameCenter(
  playerId,
  bundleId,
  timestampSeconds,
  salt,
  signature,
  publicKeyUrl,
  "mycustomusername",
  true,
  {},
  successCallback,
  errorCallback);
```

```js tab="Cocos2d-x JS"
const player_id = "...";
const bundle_id = "...";
const timestamp_seconds = "...";
const salt = "...";
const signature = "...";
const public_key_url = "...";
client.authenticateGameCenter({
  player_id: player_id,
  bundle_id: bundle_id,
  password: password,
  timestamp_seconds: timestamp_seconds,
  salt: salt,
  signature: signature,
  public_key_url: public_key_url,
  username: "mycustomusername",
  create: true
}).then(function(session) {
      cc.log("Authenticated successfully. User ID:", session.user_id);
    },
    function(error) {
      cc.error("authenticate failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = [](NSessionPtr session)
{
    std::cout << "Authenticated successfully. User ID: " << session->getUserId() << std::endl;
};

auto errorCallback = [](const NError& error)
{
};

std::string playerId = "...";
std::string	bundleId = "...";
NTimestamp timestampSeconds = "...";
std::string salt = "...";
std::string signature = "...";
std::string publicKeyUrl = "...";

client->authenticateGameCenter(
  playerId,
  bundleId,
  timestampSeconds,
  salt,
  signature,
  publicKeyUrl,
  "mycustomusername",
  true,
  {},
  successCallback,
  errorCallback);
```

```swift tab="Swift"
// Requires Nakama 1.x
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

```gdscript tab="Godot"
var bundle_id = "..."
var player_id = "..."
var public_key_url = "..."
var salt = "..."
var signature = "..."
var timestamp = "..."
var session : NakamaSession = yield(client.authenticate_game_center_async(bundle_id, player_id, public_key_url, salt, signature, timestamp), "completed")
if session.is_exception():
	print("An error occured: %s" % session)
	return
print("Successfully authenticated: %s" % session)
```

```tab="REST"
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
  "signature": "...",
  "public_key_url": "..."
}
```

#### Steam

Steam requires you to configure the server before you can register a user.

!!! Note "Server configuration"
    Have a look at the [configuration](install-configuration.md) section for what settings you need for the server.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

```sh tab="cURL"
curl "http://127.0.0.1:7350/v2/account/authenticate/steam?create=true&username=mycustomusername" \
  --user 'defaultkey' \
  --data '{"token":"valid-steam-token"}'
```

```csharp tab=".NET"
const string token = "...";
var session = await client.AuthenticateSteamAsync(token);
System.Console.WriteLine("New user: {0}, {1}", session.Created, session);
```

```csharp tab="Unity"
const string token = "...";
var session = await client.AuthenticateSteamAsync(token);
Debug.LogFormat("New user: {0}, {1}", session.Created, session);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = [](NSessionPtr session)
{
  CCLOG("Authenticated successfully. User ID: %s", session->getUserId().c_str());
};

auto errorCallback = [](const NError& error)
{
};

string token = "...";
string username = "mycustomusername";
bool create = true;
client->authenticateSteam(token, username, create, {}, successCallback, errorCallback);
```

```js tab="Cocos2d-x JS"
const token = "...";
client.authenticateSteam({ token: token, create: true, username: "mycustomusername" })
  .then(function(session) {
      cc.log("Authenticated successfully. User ID:", session.user_id);
    },
    function(error) {
      cc.error("authenticate failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = [](NSessionPtr session)
{
    std::cout << "Authenticated successfully. User ID: " << session->getUserId() << std::endl;
};

auto errorCallback = [](const NError& error)
{
};

string token = "...";
string username = "mycustomusername";
bool create = true;
client->authenticateSteam(token, username, create, {}, successCallback, errorCallback);
```

```java tab="Java"
String token = "...";
Session session = client.authenticateSteam(token).get();
System.out.format("Session %s", session.getAuthToken());
```

```swift tab="Swift"
// Requires Nakama 1.x
let sessionToken = "..."
let message = AuthenticateMessage(steam: sessionToken)
client.register(with: message).then { session in
  NSLog("Session: %@", session.token)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
// Use client.login(...) after register.
```

```gdscript tab="Godot"
var steam_token = "..."
var session : NakamaSession = yield(client.authenticate_steam_async(steam_token), "completed")
if session.is_exception():
	print("An error occured: %s" % session)
	return
print("Successfully authenticated: %s" % session)
```

```tab="REST"
POST /v2/account/authenticate/steam?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "token": "...",
}
```

### Custom

A custom identifier can be used in a similar way to a device identifier to login or register a user. This option should be used if you have an external or custom user identity service which you want to use. For example EA's Origin service handles accounts which have their own user IDs.

A custom identifier must contain alphanumeric characters with dashes and be between 6 and 128 bytes.

You can choose a custom username when creating the account. To do this, set `username` to a custom name. If you want to only authenticate without implicitly creating a user account, set `create` to false.

```sh tab="cURL"
curl "http://127.0.0.1:7350/v2/account/authenticate/custom?create=true&username=mycustomusername" \
  --user 'defaultkey:' \
  --data '{"id":"some-custom-id"}'
```

```js tab="JavaScript"
const customId = "some-custom-id";
const session = await client.authenticateCustom({ id: customId, create: true, username: "mycustomusername" });
console.info("Successfully authenticated:", session);
```

```csharp tab=".NET"
const string customId = "some-custom-id";
var session = await client.AuthenticateCustomAsync(customId);
System.Console.WriteLine("New user: {0}, {1}", session.Created, session);
```

```csharp tab="Unity"
const string customId = "some-custom-id";
var session = await client.AuthenticateCustomAsync(customId);
Debug.LogFormat("New user: {0}, {1}", session.Created, session);
```

```cpp tab="Cocos2d-x C++"
auto successCallback = [](NSessionPtr session)
{
  CCLOG("Authenticated successfully. User ID: %s", session->getUserId().c_str());
};

auto errorCallback = [](const NError& error)
{
};

string id = "some-custom-id";
string username = "mycustomusername";
bool create = true;
client->authenticateCustom(id, username, create, {}, successCallback, errorCallback);
```

```js tab="Cocos2d-x JS"
const customId = "some-custom-id";
client.authenticateCustom({ id: customId, create: true, username: "mycustomusername" })
  .then(function(session) {
      cc.log("Authenticated successfully. User ID:", session.user_id);
    },
    function(error) {
      cc.error("authenticate failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto successCallback = [](NSessionPtr session)
{
    std::cout << "Authenticated successfully. User ID: " << session->getUserId() << std::endl;
};

auto errorCallback = [](const NError& error)
{
};

string id = "some-custom-id";
string username = "mycustomusername";
bool create = true;
client->authenticateCustom(id, username, create, {}, successCallback, errorCallback);
```

```java tab="Java"
String customId = "some-custom-id";
Session session = client.authenticateCustom(customId).get();
System.out.format("Session %s", session.getAuthToken());
```

```swift tab="Swift"
// Requires Nakama 1.x
let customID = "some-custom-id"

let message = AuthenticateMessage(custom: customID)
client.register(with: message).then { session in
  NSLog("Session: %@", session.token)
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
// Use client.login(...) after register.
```

```gdscript tab="Godot"
var custom_id = "some-custom-id"
var session : NakamaSession = yield(client.authenticate_custom_async(custom_id), "completed")
if session.is_exception():
	print("An error occured: %s" % session)
	return
print("Successfully authenticated: %s" % session)
```

```tab="REST"
POST /v2/account/authenticate/custom?create=true&username=mycustomusername
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Basic base64(ServerKey:)

{
  "id": "some-custom-id",
}
```

## Session

When an authentication call succeeds, the server responds with a [session](/session) object. The session object contains at least the following:
- the user's ID
- the user's name
- a token used to authenticate subsequent user messages
- the session creation time
- the session expiration time

Once the client obtains the session object, you can utilize Nakama's realtime features such as [multiplayer](/gameplay-multiplayer-realtime), [notifications](/social-in-app-notifications) and [status updates](/social-status), [passing stream data](/advanced-streams) or [real-time chat](/social-realtime-chat).


```js tab="JavaScript"
var socket = client.createSocket();
session = await socket.connect(session);
console.info("Socket connected.");
```

```csharp tab=".NET"
var socket = Socket.From(client);
await socket.ConnectAsync(session);
System.Console.WriteLine("Socket connected.");
```

```csharp tab="Unity"
var socket = client.NewSocket();
await socket.ConnectAsync(session);
Debug.Log("Socket connected.");
```

```cpp tab="Cocos2d-x C++"
#include "NakamaCocos2d/NWebSocket.h"

int port = 7350; // different port to the main API port
bool createStatus = true; // if the server should show the user as online to others.
// define realtime client in your class as NRtClientPtr rtClient;
rtClient = client->createRtClient(port, NRtTransportPtr(new NWebSocket()));
// define listener in your class as NRtDefaultClientListener listener;
listener.setConnectCallback([]()
{
  CCLOG("Socket connected.");
});
rtClient->setListener(&listener);
rtClient->connect(session, createStatus);
```

```js tab="Cocos2d-x JS"
const socket = client.createSocket();
socket.connect(session)
  .then(
      function() {
        cc.log("Socket connected.");
      },
      function(error) {
        cc.error("connect failed:", JSON.stringify(error));
      }
    );
```

```cpp tab="C++"
int port = 7350; // different port to the main API port
bool createStatus = true; // if the server should show the user as online to others.
// define realtime client in your class as NRtClientPtr rtClient;
rtClient = client->createRtClient(port);
// define listener in your class as NRtDefaultClientListener listener;
listener.setConnectCallback([]()
{
  cout << "Socket connected." << endl;
});
rtClient->setListener(&listener);
rtClient->connect(session, createStatus);
```

```java tab="Java"
SocketClient socket = client.createSocket();
socket.connect(session, new AbstractSocketListener() {}).get();
```

```swift tab="Swift"
// Requires Nakama 1.x
let session : Session = someSession // obtained from register or login.
client.connect(with: session).then { _ in
  NSLog("Socket connected.")
});
```

```gdscript tab="Godot"
# Make this a node variable, or it will disconnect when the function that creates it returns.
onready var socket := Nakama.create_socket_from(client)

func _ready():
	var connected : NakamaAsyncResult = yield(socket.connect_async(session), "completed")
	if connected.is_exception():
		print("An error occured: %s" % connected)
		return
	print("Socket connected.")
```

## Link or unlink

You can link one or more other login option to the current user. This makes it easy to support multiple logins with each user and easily identify a user across devices.

You can only link device Ids, custom Ids, and social provider IDs which are not already in-use with another user account.

```sh tab="cURL"
curl "http://127.0.0.1:7350/v2/account/link/custom" \
  --header 'Authorization: Bearer $session' \
  --data '{"id":"some-custom-id"}'
```

```js tab="JavaScript"
const customId = "some-custom-id";
const success = await client.linkCustom(session, { id: customId });
console.log("Successfully linked custom ID to current user.");
```

```csharp tab=".NET"
const string customId = "some-custom-id";
await client.LinkCustomAsync(session, customId);
System.Console.WriteLine("Id '{0}' linked for user '{1}'", customId, session.UserId);
```

```csharp tab="Unity"
const string customid = "some-custom-id";
await client.LinkCustomAsync(session, customId);
Debug.LogFormat("Id '{0}' linked for user '{1}'", customId, session.UserId);
```

```cpp tab="Cocos2d-x C++"
auto linkFailedCallback = [](const NError& error)
{
};

auto linkSucceededCallback = []()
{
  CCLOG("Linked successfully");
};

std::string customid = "some-custom-id";

client->linkCustom(customid, linkSucceededCallback, linkFailedCallback);
```

```js tab="Cocos2d-x JS"
const customId = "some-custom-id";
client.linkCustom(session, { id: customId })
  .then(function() {
      cc.log("Linked successfully");
    },
    function(error) {
      cc.error("link failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto linkFailedCallback = [](const NError& error)
{
};

auto linkSucceededCallback = []()
{
  cout << "Linked successfully" << endl;
};

std::string customid = "some-custom-id";

client->linkCustom(customid, linkSucceededCallback, linkFailedCallback);
```

```java tab="Java"
String customId = "some-custom-id";
client.linkCustom(session, customId).get();
System.out.format("Id %s linked for user %s", customId, session.getUserId());
```

```swift tab="Swift"
// Requires Nakama 1.x
let id = "some-custom-id"
var message = SelfLinkMessage(device: id);
client.send(with: message).then {
  NSLog("Successfully linked device ID to current user.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```gdscript tab="Godot"
var custom_id = "some-custom-id"
var linked : NakamaAsyncResult = yield(client.link_custom_async(session, custom_id), "completed")
if linked.is_exception():
	print("An error occured: %s" % linked)
	return
print("Id '%s' linked for user '%s'" % [custom_id, session.user_id])
```

```tab="REST"
POST /v2/account/link/custom
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{
    "id":"some-custom-id"
}
```

You can unlink any linked login options for the current user.

```sh tab="cURL"
curl "http://127.0.0.1:7350/v2/account/unlink/custom" \
  --header 'Authorization: Bearer $session' \
  --data '{"id":"some-custom-id"}'
```

```js tab="JavaScript"
const customId = "some-custom-id";
const success = await client.unlinkCustom(session, { id: customId });
console.info("Successfully unlinked custom ID from the current user.");
```

```csharp tab=".NET"
const string customId = "some-custom-id";
await client.UnlinkCustomAsync(session, customId);
System.Console.WriteLine("Id '{0}' unlinked for user '{1}'", customId, session.UserId);
```

```csharp tab="Unity"
const string customId = "some-custom-id";
await client.UnlinkCustomAsync(session, customId);
Debug.LogFormat("Id '{0}' unlinked for user '{1}'", customId, session.UserId);
```


```cpp tab="Cocos2d-x C++"
auto unlinkFailedCallback = [](const NError& error)
{
};

auto unlinkSucceededCallback = []()
{
  CCLOG("Successfully unlinked custom ID from the current user.");
};

std::string customid = "some-custom-id";

client->unlinkCustom(customid, unlinkSucceededCallback, unlinkFailedCallback);
```

```js tab="Cocos2d-x JS"
const customId = "some-custom-id";
client.unlinkCustom(session, { id: customId })
  .then(function() {
      cc.log("Successfully unlinked custom ID from the current user.");
    },
    function(error) {
      cc.error("unlink failed:", JSON.stringify(error));
    });
```

```cpp tab="C++"
auto unlinkFailedCallback = [](const NError& error)
{
};

auto unlinkSucceededCallback = []()
{
  cout << "Successfully unlinked custom ID from the current user." << endl;
};

std::string customid = "some-custom-id";

client->unlinkCustom(customid, unlinkSucceededCallback, unlinkFailedCallback);
```

```java tab="Java"
String customId = "some-custom-id";
client.unlinkCustom(session, customId).get();
System.out.format("Id %s unlinked for user %s", customId, session.getUserId());
```

```swift tab="Swift"
// Requires Nakama 1.x
let id = "some-custom-id"
var message = SelfUnlinkMessage(device: id);
client.send(with: message).then {
  NSLog("Successfully unlinked device ID from current user.")
}.catch { err in
  NSLog("Error %@ : %@", err, (err as! NakamaError).message)
}
```

```gdscript tab="Godot"
var custom_id = "some-custom-id"
var unlinked : NakamaAsyncResult = yield(client.unlink_custom_async(session, custom_id), "completed")
if unlinked.is_exception():
	print("An error occured: %s" % unlinked)
	return
print("Id '%s' unlinked for user '%s'" % [custom_id, session.user_id])
```

```tab="REST"
POST /v2/account/unlink/custom
Host: 127.0.0.1:7350
Accept: application/json
Content-Type: application/json
Authorization: Bearer <session token>

{
  "id":"some-custom-id"
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
| Google | A Google account. |
| Steam | An account from the Steam network. |
