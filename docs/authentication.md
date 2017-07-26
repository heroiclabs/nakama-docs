# Authentication

The server has builtin authentication so clients can only send requests and connect if they have the [server key](install-configuration.md#socket). When authentication is successful a client can create a session as a [user](user-accounts.md).

!!! Warning "Important"
    The default server key is "defaultkey" but it is very important to set a [unique value](install-configuration.md#socket). This value should be embedded within client code.

```csharp fct_label="Unity"
INClient client = new NClient.Builder("defaultkey")
    .Host("127.0.0.1")
    .Port(7350)
    .SSL(false)
    .Build();
// or same as above.
INClient client = NClient.Default("defaultkey");
```

Every user account is created from one of the [options used to register](#register-or-login). We call each of these options a "link" because it's a way to access the user's account. You can add more than one link to each account which is useful to enable users to login in multiple ways across different devices.

## Register or login

Before you login a user they must first be registered. We recommend you setup your code to login the user and fallback to register the account if one does not exist. This pattern is shown in the [device](#device) section.

!!! Tip
    The authentication system is very flexible. You could register a user with an email address, [link](#link-or-unlink) their Facebook account, and use it to login from another device.

For a __full example__ on the best way to handle register and login in each of the clients have a look at their guides.

### Device

A device identifier can be used as a way to unobtrusively register a user with the server. This offers a frictionless user experience but can be unreliable because device identifiers can sometimes change in device updates.

A device identifier must contain alphanumeric characters with dashes and be between 10 and 60 bytes.

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

In games it is often a better option to use [Google](#google) or [Game Center](#game-center) to unobtrusively register the user.

### Email

Users can be registered with an email and password. The password is hashed before it's stored in the database server and cannot be read or "recovered" by administrators. This protects a user's privacy.

An email address must be valid as defined by RFC-5322 and passwords must be at least 8 characters.

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

### Social providers

The server supports a lot of different social services with register and login. With each provider the user account will be fetched from the social service and used to setup the user. In some cases a user's [friends](social-friends.md) will also be fetched and added to their friends list.

To register or login as a user with any of the providers an OAuth or access token must be obtained from that social service.

#### Facebook

With Facebook you'll need to add the Facebook SDK to your project which can be <a href="https://developers.facebook.com/docs/" target="\_blank">downloaded online</a>. Follow their guides on how to integrate the code. With a mobile project you'll also need to complete instructions on how to configure iOS and Android.

```csharp fct_label="Unity"
var sessionHandler = delegate(INSession session) {
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

You can add a button to your UI to login with Facebook.

```csharp fct_label="Unity"
var sessionHandler = delegate(INSession session) {
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

#### Game Center

Apple devices have builtin authentication which can be done without user interaction through Game Center. The register or login process is a little complicated because of how Apple's services work.

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

#### Steam

Steam requires you to configure the server before you can register a user.

!!! Note "Server configuration"
    Have a look at the [configuration](install-configuration.md) section for what settings you need for the server.

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

### Custom

A custom identifier can be used in a similar way to a device identifier to login or register a user. This option should be used if you have an external or custom user identity service which you want to use. For example EA's Origin service handles accounts which have their own user IDs.

A custom identifier must contain alphanumeric characters with dashes and be between 10 and 60 bytes.

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

## Sessions

The register and login messages return a session on success. The session contains the current user's ID and handle as well as information on when it was created and when it expires.

!!! Tip
    You can change how long a session token is valid before it expires in the [configuration](install-configuration.md) in the server. By default a session is only valid for 60 seconds.

```csharp fct_label="Unity"
string id = "3e70fd52-7192-11e7-9766-cb3ce5609916";
var message = NAuthenticateMessage.Device(id);
client.Login(message, (INSession session) => {
  var sessionId = Encoding.UTF8.GetString(session.Id);
  Debug.LogFormat("Session id '{0}' handle '{1}'.", sessionId, session.Handle);
  Debug.LogFormat("Session expired: {0}", session.HasExpired(DateTime.UtcNow));
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

### Connect

With a session you can connect with the server and send messages. Most of our clients do not auto-reconnect for you so you should handle it with your own code.

You can only send messages to the server once you've connected a client.

```csharp fct_label="Unity"
INSession session = session; // obtained from Register or Login.
client.Connect(session, (bool done) => {
  Debug.Log("Successfully connected.");
});
```

## Link or unlink

You can link one or more other login option to the current user. This makes it easy to support multiple logins with each user and easily identify a user across devices.

You can only link device Ids, custom Ids, and social provider IDs which are not already in-use with another user account.

```csharp fct_label="Unity"
string id = "062b0916-7196-11e7-8371-9fcee9f0b20c";

var message = SelfLinkMessage.Device(id);
client.Send(message, (bool done) => {
  Debug.Log("Successfully linked device ID to current user.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

You can unlink any linked login options for the current user.

```csharp fct_label="Unity"
string id = "062b0916-7196-11e7-8371-9fcee9f0b20c";

var message = SelfUnlinkMessage.Device(id);
client.Send(message, (bool done) => {
  Debug.Log("Successfully unlinked device ID from current user.");
}, (INError err) => {
  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
});
```

Like with register and login you can link or unlink many different account options.

| Link | Description |
| ---- | ----------- |
| Custom | A custom identifier from another identity service. |
| Device | A unique identifier for a device which belongs to the user. |
| Email | An email and password set by the user. |
| Facebook | A Facebook social account. |
| Game Center | An account from Apple's Game Center service. |
| Google | A Google Play social account. |
| Steam | An account from the Steam network. |
