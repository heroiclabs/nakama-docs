# Updating Group Metadata
The following example demonstrates how to update group metadata to store a group's interests, active times and languages spoken.

```go
type UpdateGroupMetadataPayload struct {
    GroupId     string   `json:"groupId"`
    Interests   []string `json:"interests"`
    ActiveTimes []string `json:"activeTimes"`
    Languages   []string `json:"languages"`
}

initializer.RegisterRpc("UpdateGroupMetadata", func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
    userID, _ := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)

    payloadData := &UpdateGroupMetadataPayload{}
    if err := json.Unmarshal([]byte(payload), payloadData); err != nil {
        return "", errors.New("error deserializing payload")
    }

    groups, err := nk.GroupsGetId(ctx, []string{payloadData.GroupId})
    if err != nil || len(groups) == 0 {
        return "", errors.New("error getting group")
    }

    group := groups[0]

    canUpdateMetadata := false
    userGroups, _, err := nk.UserGroupsList(ctx, userID, 100, nil, "")
    for _, userGroup := range userGroups {
        if userGroup.Group.Id == group.Id {
            if userGroup.State.Value < 2 {
                canUpdateMetadata = true
            }
        }
    }
    if err != nil {
        logger.Error(err.Error())
        return "", errors.New("error finding user groups")
    }

    if !canUpdateMetadata {
        return "", errors.New("user does not have permission to update metadata")
    }

    if err := nk.GroupUpdate(ctx,
        group.Id,
        group.Name,
        group.CreatorId,
        group.LangTag,
        group.Description,
        group.AvatarUrl,
        group.Open.Value,
        map[string]interface{}{
            "interests":   payloadData.Interests,
            "activeTimes": payloadData.ActiveTimes,
            "languages":   payloadData.Languages,
        },
        int(group.MaxCount)); err != nil {
        return "", errors.New("error updating group")
    }

    return "{}", nil
})
```
