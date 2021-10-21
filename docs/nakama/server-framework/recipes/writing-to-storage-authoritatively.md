# Writing to Storage Engine Authoritatively
The following example shows how you would write to the Storage Engine authoritatively.

```go
userID, _ := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)

unlockedHats := map[string]interface{}{
    "hats": []string{"alien", "cowboy", "space-soldier"},
}

hatBytes, err := json.Marshal(unlockedHats)
if err != nil {
    return err
}

unlockedSkins := map[string]interface{}{
    "skins": []string{"ninja", "robot", "bear"},
}

skinBytes, err := json.Marshal(unlockedSkins)
if err != nil {
    return err
}

hatWrite := &runtime.StorageWrite{
    Collection:      "Unlocks",
    Key:             "Hats",
    UserID:          userID,
    Value:           string(hatBytes),
    PermissionRead:  1, // Only the server and owner can read
    PermissionWrite: 0, // Only the server can write
}

skinWrite := &runtime.StorageWrite{
    Collection:      "Unlocks",
    Key:             "Skins",
    UserID:          userID,
    Value:           string(skinBytes),
    PermissionRead:  1, // Only the server and owner can read
    PermissionWrite: 0, // Only the server can write
}

_, err = nk.StorageWrite(ctx, []*runtime.StorageWrite{hatWrite, skinWrite})
if err != nil {
    return err
}
```

!!! note "Note"
When writing multiple storage object at once using the `StorageWrite` function the objects are written in a single transaction, guaranteeing that all objects are successfully written or all fail.