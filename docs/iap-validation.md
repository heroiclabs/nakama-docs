[apple_iap_1]: images/apple_iap_1.jpg "Apple App Store Connect"
[apple_iap_2]: images/apple_iap_2.jpg "Apple App Store Connect Shared Secret"
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

Nakama directly connects to Apple, Google and Huawei services to check the validity of all incoming purchase receipts. This verification is completely outside the client's code, and cannot be intercepted and tampered with.

Every purchase receipt is verified, every time, and invalid ones are rejected.

__Replay Attacks__

All transactions are logged, preventing multiple submissions of the same purchase token or receipt.

__Receipt Sharing__

Successful transactions are bound to the account that submits them. Different users cannot submit the same transaction, even a valid one, in an attempt to receive the associated reward.

__Product Mismatches__

Each validated purchase receipt exposes data (e.g. product ID) that can be used to tie a purchase to a product, preventing attacks that attempt to use a valid (cheap) purchase to unlock a different (expensive) reward.

__Single Source of Truth__

While Nakama maintains an internal record of all transactions, the remote payment provider is always used for validation.

## Apple

Nakama supports validating purchases made for products and subscription in iOS.

Apple purchase receipts are sent to Apple for validation. As suggested by Apple, both Production and Sandbox servers are used to validate receipts depending on the priority setup in the Nakama configuration.

### Setup

To validate receipts against the App Store, Nakama requires your app's shared secret. You can setup a shared secret in [App Store Connect](https://appstoreconnect.apple.com) under your app's In-App Purchases management section.

![Apple App Store Connect][apple_iap_1]

Make a record of your shared secret:

![Apple App Store Connect Shared Secret][apple_iap_2]

You'll need to set the value of Nakama's `iap.apple.shared_password` configuration flag to the value of the Shared Secret above. For more info, take a look at the [configuration](install-configuration.md#iap-in-app-purchase) page.

### Validate Purchase

Nakama only supports validating iOS 7+ receipts.

Apple receipts can contain multiple purchases, Nakama will validate all of them and store them as individual purchase records.

=== "cURL"
    ```sh
    curl "http://127.0.0.1:7350/v2/iap/purchase/apple \
      --user 'defaultkey:' \
      --data '{"receipt":"base64_encoded_receipt_data"}'
    ```

=== "Defold"
	```lua
    local function validate_receipt(receipt)
        local request = nakama.create_api_validate_purchase_apple_request(receipt)
        local result = nakama.validate_purchase_apple(client, request)
        if result.error then
            print(result.message)
            return
        end
        pprint(result)
    end

    -- Use https://defold.com/extension-iap/
    iap.set_listener(function(self, transaction, error)
        if not error then
            validate_receipt(transaction.receipt)
        end
    end)
    iap.buy("com.defold.nakama.goldbars-10")
	```

Refer to the function reference page for the provided runtime [purchase validation functions](runtime-code-function-reference.md#purchase).

## Google

Nakama supports validating purchases made for products and subscription on Android.

### Setup

To validate receipts against the Play Store, Nakama requires your Google Service Account `ClientEmail` and `PrivateKey`. The values must be set in Nakama's `iap.google.client_email` and `iap.google.private_key` configuration flags values, respectively.
For more info, take a look at the [configuration](install-configuration.md#iap-in-app-purchase) page.

To get these values, first you'll need to setup a Service Account in the [Google API Console](https://play.google.com/console/developer/). You can refer to the [Google Play documentation](https://developers.google.com/android-publisher/getting_started#using_a_service_account) to create it.

![Create Service Account][google_iap_1_create_service_account]

Once a service account is created, you'll need to create a key:

![Create Key][google_iap_2_create_key]

Download the key as a JSON file.

![Create Key][google_iap_3_create_key_2]

Open it, extract the values of `ClientEmail` and `PrivateKey` and set them as the respective Nakama configuration values for:

- `iap.google.client_email`
- `purchase.google.private_key`

For more info, take a look at the [configuration](install-configuration.md#iap-in-app-purchase) page.

Finally you will need to ensure you grant Nakama access to the purchase validation APIs.
Navigate back to [Google Play Developer Console](https://play.google.com/apps/publish) and navigate to __Settings__ > __API Access__.

The service account you created in the previous steps should be listed above. You'll need to grant access to the service account to access the API:

![Create API Access][google_iap_5_play_api]

Make sure that you give the service account access to __Visibility__, __View Financial Data__, and __Manage Orders__. These permissions are required for Nakama to validate receipts against Google Play.

![Grand Access][google_iap_6_grant_access]

Navigate to __Users & Permissions__ to check that the service account is setup correctly.

![List users with access][google_iap_7_play_users]

### Validate Purchase

=== "cURL"
    ```sh
    curl "http://127.0.0.1:7350/v2/iap/purchase/google \
      --user 'defaultkey:' \
      --data '{"purchase":"json_encoded_purchase_data"}'
    ```

=== "Defold"
	```lua
    local function validate_receipt(receipt)
        local request = nakama.create_api_validate_purchase_google_request(receipt)
        local result = nakama.validate_purchase_google(client, request)
        if result.error then
            print(result.message)
            return
        end
        pprint(result)
    end

    -- Use https://defold.com/extension-iap/
    iap.set_listener(function(self, transaction, error)
        if not error then
            validate_receipt(transaction.receipt)
        end
    end)
    iap.buy("com.defold.nakama.goldbars-10")
	```

Refer to the function reference page for the provided runtime [purchase validation functions](runtime-code-function-reference.md#purchase).

## Huawei

Nakama validates Huawei purchases against their IAP validation service. As suggested by Huawei, the validity of the purchase data is also checked against the provided signature before contacting the Huawei service. If the data is invalid for any reason, the purchase is rejected before validation with Huawei's validation service.

### Validate Purchase
=== "cURL"
    ```sh
    curl "http://127.0.0.1:7350/v2/iap/purchase/huawei \
      --user 'defaultkey:' \
      --data '{"purchase":"json_encoded_purchase_data","signature":"purchase_data_signature"}'
    ```

=== "Defold"
	```lua
    -- Huawei purchases are not yet supported by https://defold.com/extension-iap/
	```

Refer to the function reference page for the provided runtime [purchase validation functions](runtime-code-function-reference.md#purchase).


## Interpreting the validation result

A validation result contains a list of validated purchases.

Because Apple may contain multiple purchases in a single receipt, the resulting list will contain only the purchases which have been validated and are new to Nakama's purchase ledger. If a purchase has already been validated by Nakama previously, it won't be included in the list, allowing the developer to discriminate new purchases and protect against replay attacks.

For Google and Huawei, each validation corresponds to a single purchase, which is included in the validation response list if new, and omitted otherwise.

Each validated purchase also includes the payload of the provider validation response, should the developer need it for any reason.

Should the purchase/receipt be invalid, the validation fail for any reason or the provider be unreachable, an error will be returned.
