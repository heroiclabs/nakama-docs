[apple_iap_1]: images/apple_iap_1.jpg "Apple iTunes Connect"
[apple_iap_2]: images/apple_iap_2.jpg "Apple iTunes Connect Shared Secret"
[google_iap_1_create_service_account]: images/google_iap_1_create_service_account.jpg "Create Service Account"
[google_iap_2_create_key]: images/google_iap_2_create_key.jpg "Create Key"
[google_iap_3_create_key_2]: images/google_iap_3_create_key_2.jpg "Create Key"
[google_iap_4_create_key_3]: images/google_iap_4_create_key_3.jpg "Create Key"
[google_iap_5_play_api]: images/google_iap_5_play_api.jpg "Create API Access"
[google_iap_6_grant_access]: images/google_iap_6_grant_access.jpg "Grand Access"
[google_iap_7_play_users]: images/google_iap_7_play_users.jpg "List users with access"

# In-app Purchase Validation

The spectrum of monetisation models and tools is extremely varied. From ad-supported, microtransactions, freemium, one-off purchases, and everything in between. A key tool in many of these solutions is the In-App Purchase, which enables single purchases for unlocks, in-game consumables, subscriptions for premium access, and more.

There are a number of readily available attacks against the most common in-app purchase implementations.

These are usually focused around:

- Feeding the client fake purchase responses which indicate success.
- Replaying a valid purchase response multiple times.
- Sharing a purchase response with another client, so multiple players can receive the reward from a single purchase.
- ...and more, with new vulnerabilities emerging occasionally.

For in-app purchases, a trusted source of truth is required. Nakama checks and tracks purchases and purchase history, solving a significant set of possible vulnerabilities and pain points.

In-App Purchase Validation is available for Apple and Google purchases, regardless of platform. Both single product and subscription purchases are supported.

__Fake Purchases__

Nakama directly connects to Apple and Google services to check the validity of all incoming payments. This verification is completely outside the client's code, and cannot be intercepted and tampered with.

Every transaction is verified, every time, and invalid ones are rejected.

__Replay Attacks__

All transactions are logged, preventing multiple submissions of the same purchase token or receipt.

__Receipt Sharing__

Successful transactions are bound to the account that submits them. Different users cannot submit the same transaction, even a valid one, in an attempt to receive the associated reward.

__Product Mismatches__

The transaction is checked to ensure the correct reward is tied to each purchase. This prevents attacks that attempt to use a valid (cheap) purchase to unlock a different (expensive) reward.

__Subscription Expiry__

Nakama checks subscriptions to see if they've expired, and rejects the transaction as needed.

__Purchase Cancellation__

Valid receipts that link to cancelled purchases are flagged and rejected.

__Single Source of Truth__

While Nakama maintains an internal record of all transactions, the remote payment provider is always given priority. Valid purchases that have been checked, logged, then subsequently cancelled, will be rejected appropriately.

## Apple

Nakama supports validating purchases made for products and subscription in iOS.

Apple purchase receipts are sent to Apple for validation. As suggested by Apple, both Production and Sandbox servers are used to validate receipts depending on the priority setup in the Nakama configuration.

### Setup

To validate receipts against the App Store, Nakama requires your app's shared secret. You can setup a shared secret in [iTunes Connect](https://itunesconnect.apple.com).

![Apple iTunes Connect][apple_iap_1]

Make a record of your shared secret:

![Apple iTunes Connect Shared Secret][apple_iap_2]

You'll need to set the value of `purchase.apple.password` to the value of the Shared Secret above. For more info, take a look at the [configuration](install-configuration.md#purchase) page.

If your app is in production, you'll need to set the value of `purchase.apple.production` to true to give priority Apple's Production servers.

### Validate Purchase

Nakama only supports validating iOS 7+ receipts. In addition, Nakama only validates the first item in the receipt as Apple receipts can contain more than one in-app purchase item.

=== "Unity"
	```csharp
	var productId = "com.yourcompany.product";
	var receiptData = "...some-base64-encoded-data...";
	
	var message = NPurchaseValidateMessage.Apple(productId, receiptData);
	client.Send(message, (INPurchaseRecord record) =>
	{
	  if (!record.Success) {
	    Debug.Log("Purchase was not validation. Reason: {0}.", record.Message);
	  } else {
	    if (record.SeenBefore) {
	      // This is useful for recovering previous purchases
	      Debug.Log("This is a valid purchase but the purchase item was redeemed once before.");
	    } else {
	      Debug.Log("New purchase was validated");
	    }
	  }
	}, (INError e) => {
	  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
	});
	```

| Param | Type | Description |
| ----- | ---- | ----------- |
| receipt_data | string | Base-64 encoded receipt data returned by the purchase operation itself. |
| product_id | string | The product, item, or subscription package ID the purchase relates to. |

## Google

Nakama supports validating purchases made for products and subscription on Android.

### Setup

To validate receipts against the Play Store, Nakama requires your app's package name, as well as a service file. You can setup a service account and download the service file on [Google Play Developer Console](https://play.google.com/apps/publish).

Firstly, you'll need to setup a Service Account in the [Google API Console](https://console.developers.google.com/iam-admin/serviceaccounts/).

![Create Service Account][google_iap_1_create_service_account]

Once a service account is created, you'll need to create a key:

![Create Key][google_iap_2_create_key]

Download the key as a JSON file. You'll need to put this file somewhere that Nakama server can access.

![Create Key][google_iap_3_create_key_2]

Once the key is created, navigate back to [Google Play Developer Console](https://play.google.com/apps/publish) and navigate to __Settings__ > __API Access__.

The service account you created in the previous steps should be listed above. You'll need to grant access to the service account to access the API:

![Create API Access][google_iap_5_play_api]

Make sure that you give the service account access to __Visibility__, __View Financial Data__, and __Manage Orders__. These permissions are required for Nakama to validate receipts against Google Play.

![Grand Access][google_iap_6_grant_access]

Navigate to __Users & Permissions__ to check that the service account is setup correctly.

![List users with access][google_iap_7_play_users]

Lastly, you'll need to update Nakama's configuration with the following information:

- `purchase.google.package_name`: Package name for your Android app, as you've listed in Google Play.

- `purchase.google.service_key_file`: Path of the JSON file you download in previous steps. This file contains authentication information that allows Nakama to communicate with Google Play on your behalf. Make sure that the file is kept safe and is only accessible by Nakama and other authorized parties.

### Validate Purchase

=== "Unity"
	```csharp
	var productId = "com.yourcompany.product";
	var purchaseType = "product";
	var purchaseToken = "some-token-from-google";
	
	var message = NPurchaseValidateMessage.Google(productId, purchaseType, purchaseToken);
	client.Send(message, (INPurchaseRecord record) =>
	{
	  if (!record.Success) {
	    Debug.Log("Purchase was not validation. Reason: {0}.", record.Message);
	  } else {
	    if (record.SeenBefore) {
	      // This is useful for recovering previous purchases
	      Debug.Log("This is a valid purchase but the purchase item was redeemed once before.");
	    } else {
	      Debug.Log("New purchase was validated");
	    }
	  }
	}, (INError e) => {
	  Debug.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
	});
	```

| Param | Type | Description |
| ----- | ---- | ----------- |
| product_type | string | Whether the purchase is for a `product` or a `subscription` |
| purchase_token | string | The token returned in the purchase operation response, acts as a transaction identifier. |
| product_id | string | The identifier of the product or subscription being purchased. |

## Interpreting Responses

Responses contain the following information:

- `success` - Whether or not the transaction is valid and all the information matches.
- `seen_before` - If this is a new transaction or if Nakama has a log of it.
- `purchase_provider_reachable` - Indicates whether or not Nakama was able to reach the remote purchase service.
- `message` - A string indicating why the purchase verification failed, if appropriate.
- `data` - The complete response Nakama received from the remote service.

!!! note
    If `purchase_provider_reachable` is `false` it indicates that Nakama was unable to query the remote purchase service. In this situation the client should use its discretion to decide if the purchase should be accepted, and must queue up the verification request for a later retry.

Each response contains all the information needed to take the appropriate action. Below is a quick reference for interpreting the key fields:


` ` | `seen_before` = `true` | `seen_before` = `false` |
------ | ------- | ---------- |
`success` = `true`    | <mark>Valid, but Nakama has an existing record of it.</mark> | Valid and new.
`success` = `false`   | <mark style="background-color: pink"> Rejected, check `message` field for reason.</mark> | <mark style="background-color: pink">Rejected, check `message` field for reason.</mark>

## Recovering Purchases

When users change devices, it's common to offer an option (or fully automated process) to re-apply the benefits of any previous purchases to their new client installation.

Clients should always refer to the platform purchase provider for a list of purchases, then verify each one with Nakama. In this case clients should accommodate responses where the `seen_before` indicator is true and act accordingly.


