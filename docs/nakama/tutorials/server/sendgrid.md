# Sending email using SendGrid

A common requirement is to be able to send email notifications to players. This can be achieved by integrating with a third-party email provider such as SendGrid.

The example snippet demonstrates how to use the [`nk.httpRequest`](../../server-framework/function-reference.md#http) function to call SendGrid's HTTP API from from your game server. While this example focuses specifically on integrating with SendGrid, the same principle can be applied to any third-party service that provides an HTTP API.

!!! note "Note"
    For full SendGrid documentation on sending mail via the HTTP API see the [official documentation](https://docs.sendgrid.com/api-reference/mail-send/mail-send).

```typescript
var headers = {
  Authorization: "Bearer <YourSendgridApiKey>",
  "Content-Type": "application/json",
};

var body = {
  personalizations: [
    {
      to: [
        {
          email: "tom@example.com",
          name: "Tom",
        },
      ],
      substitutions: {
        "-name-": "Tom",
      },
    },
    {
      to: [
        {
          email: "sean@example.com",
          name: "Sean",
        },
      ],
      substitutions: {
        "-name-": "Sean",
      },
    },
  ],
  from: {
    email: "no-reply@awesomegame.com",
    name: "Awesome Game",
  },
  subject: "Login now to receive your Daily Login Reward!",
  content: [{
    type: "text/html",
    value:
      `<p>
        Hello, -name-!<br />
        Login to Awesome Game now to receive your Daily Login Reward of 1000 Awesome Coins!
      </p>`,
  }]
};

var response = nk.httpRequest("https://api.sendgrid.com/v3/mail/send", "post", headers, JSON.stringify(body));

if (response.code != 202) {
  logger.error(response.body);
} else {
  logger.info("Successfully sent email.");
}

```
