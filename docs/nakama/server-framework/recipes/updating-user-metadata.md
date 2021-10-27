# Updating User Metadata
The following example shows how you can use an RPC to update a player's metadata authoritatively.

```go
initializer.RegisterRpc("UpdateMetadata", func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
    userId, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
    if !ok {
        return "", errors.New("could not get user ID from context")
    }

    if err := nk.AccountUpdateId(ctx, userId, "", map[string]interface{}{
        "title": "Definitely Not The Imposter",
        "hat":  "space_helmet"
        "skin": "alien"},
    }, "", "", "", "", ""); err != nil {
        return "", errors.New("could not update account")
    }

    return "{}", nil
})
```