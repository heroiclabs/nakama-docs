# Itch.io Custom Authentication
The following example handles authentication with the Itch.io platform via a `BeforeAuthenticateCustom` hook.

```go
func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
	// Register BeforeAuthenticateCustom Hook to authenticate against Itch.io.
	initializer.RegisterBeforeAuthenticateCustom(BeforeAuthenticateCustom)

	return nil
}

func BeforeAuthenticateCustom(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, in *api.AuthenticateCustomRequest) (*api.AuthenticateCustomRequest, error) {
	token := in.GetAccount().GetId()
	logger.Debug("Attempting to login with Itch.io using Token: %v", token)

	if token == "" {
		return in, runtime.NewError("Missing Itch.io token.", 3) // gRPC status code 3 - INVALID_ARGUMENT
	}

	client := &http.Client{}

	const itchURL = "https://itch.io/api/1/jwt/me"
	req, err := http.NewRequest("GET", itchURL, nil)
	if err != nil {
		return in, runtime.NewError(fmt.Sprintf("Error creating network request object: %v", err), 14) // gRPC status code 14 - UNAVAILABLE
	}

	req.Header.Add("Authorization", "Bearer "+token)

	resp, err := client.Do(req)
	if err != nil {
		return in, runtime.NewError(fmt.Sprintf("Network request failed: %v", err), 13) // gRPC status code 13 = INTERNAL
	}

	if resp.StatusCode >= 300 {
		return in, runtime.NewError(fmt.Sprintf("Itch.io authorization request error: %v", resp.Status), 16) // gRPC status code 16 = UNATHENTICATED
	}

	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return in, runtime.NewError(fmt.Sprintf("iotil.ReadAll failed: %v", err), 13) // gRPC status code 13 = INTERNAL
	}

	var response struct {
		User *struct {
			Username    string
			DisplayName string `json:"display_name"`
			ID          int
		}
		Errors []string
	}

	if err := json.Unmarshal(body, &response); err != nil {
		return in, runtime.NewError(fmt.Sprintf("Failed to unmarsdhal Itch.io response: %v", response.Errors), 13)
	}

	if len(response.Errors) > 0 {
		return in, runtime.NewError(fmt.Sprintf("Itch.io response has errors: %v", response.Errors), 13)
	}

	// Add a prefix to ensure ID is >= 6 characters as Itch.io IDs may be shorter.
	in.Account.Id = fmt.Sprintf("itch-%d", response.User.ID)

	// If no username was passed, take the username from Itch.io.
	if in.Username == "" {
		in.Username = response.User.Username
	}

	return in, nil
}
```

The following code shows how you would authenticate from the client side:

```csharp
public async void AuthenticationWithCustom()
{
    // Authenticate using Custom ID (using itch.io authentication).
    try
    {
        var itchioApiKey = Environment.GetEnvironmentVariable("ITCHIO_API_KEY");
        Session = await Client.AuthenticateCustomAsync(itchioApiKey);
        Debug.Log("Authenticated with Custom ID");
    }
    catch (ApiResponseException ex)
    {
        Debug.LogFormat("Failed authentication: {0}", ex.Message);
    }
}
```