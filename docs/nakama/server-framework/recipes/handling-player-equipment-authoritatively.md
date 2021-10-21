# Handling Player Equipment Authoritatively
The following example demonstrates how you can use an RPC to allow players to equip items authoritatively, ensuring the player owns the items before allowing them to equip them.

```go
// Declare struct for the expected input payload
type EquipPayload struct {
    Item string `json:"item"`
}

// Declare struct for the response payload
type EquipResponsePayload struct {
    Success bool `json:"success"`
}

// Declare a struct for hat unlocks
type HatUnlocks struct {
	Hats []string `json:"hats"`
}

// ...

// In your InitModule function
initializer.RegisterRpc("EquipHat", func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
    userID, _ := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)

    // Deserialize the payload from the client
    equipPayload := &EquipPayload{}
    if err := json.Unmarshal([]byte(payload), equipPayload); err != nil {
        return "", errors.New("error deserializing client payload")
    }

    // Check if the player has unlocked the hat
    objects, err := nk.StorageRead(ctx, []*runtime.StorageRead{{
        Collection: "Unlocks",
        Key:        "Hats",
        UserID:     userID,
    }})
    if err != nil || len(objects) == 0 {
        return "", errors.New("error finding hat unlocks for user")
    }

    hatUnlocks := &HatUnlocks{}
    if err := json.Unmarshal([]byte(objects[0].Value), hatUnlocks); err != nil {
        return "", errors.New("error deserializing item unlocks")
    }

    hasHat := false
    for _, hat := range hatUnlocks.Hats {
        if hat == equipPayload.Item {
            hasHat = true
        }
    }

    if !hasHat {
        return "", errors.New("user has not unlocked the hat")
    }

    // Get the user's account details
    account, err := nk.AccountGetId(ctx, userID)
    if err != nil {
        return "", errors.New("error getting account data")
    }

    // Get the user's existing metadata
    metadata := make(map[string]interface{})
    if err := json.Unmarshal([]byte(account.User.Metadata), &metadata); err != nil {
        return "", errors.New("error deserializing metadata")
    }

    // Equip the hat and update the user's metadata
    metadata["hat"] = equipPayload.Item

    if err := nk.AccountUpdateId(ctx, userID, "", metadata, "", "", "", "", ""); err != nil {
        return "", errors.New("error updating account data")
    }

    // Return a response to the client
    bytes, err := json.Marshal(&EquipResponsePayload{Success: true})
    if err != nil {
        return "", errors.New("error serializing response")
    }

    return string(bytes), nil
})
```
