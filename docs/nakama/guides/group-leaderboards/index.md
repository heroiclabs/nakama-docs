# Group and Friend Leaderboards

Nakama [Leaderboards](../../concepts/leaderboards.md) give players a competitive activity to engage around. As a game grows, massive leaderboards can feel static and more challenging for players to climb the ranks. You can address this with:

- [Bucketed leaderboards](../bucketed-leaderboards/index.md)
- Group or friend Leaderboards

Using the leaderboard list API you can pass in a list of user IDs to create a custom leaderboard view.

The following code samples show you how to get leaderboard records for a group's members or user's friends.

## Creating the leaderboard

Create a leaderboard on the server that resets every Monday at 00:00.

```go
func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
    // Create a weekly leaderboard
    id := "weekly_leaderboard"
    authoritative := false
    sortOrder := "desc"
    operator := "incr"
    resetSchedule := "0 0 * * 1"
    metadata := make(map[string]interface{})

    if err := nk.LeaderboardCreate(ctx, id, authoritative, sortOrder, operator, resetSchedule, metadata); err != nil {
        logger.Error("error creating leaderboard")
        return err
    }
}
```

## Getting a custom view of the leaderboard

Create a payload struct for the RPC:

```go
type leaderboardRecord struct {
    Username string `json:"username"`
    UserId   string `json:"userId"`
    Score    int    `json:"score"`
    Rank     int    `json:"rank"`
}
```

Create a helper function that will take an array of user IDs and return an array of those records along with the relative Rank value based on user scores.

```go
func getLeaderboardForUsers(leaderboardId string, userIds []string, ctx context.Context, logger runtime.Logger, nk runtime.NakamaModule) ([]leaderboardRecord, error) {
    // Get all leaderboard records for user Ids
    _, records, _, _, err := nk.LeaderboardRecordsList(ctx, leaderboardId, userIds, 0, "", 0)
    if err != nil {
        return nil, err
    }

    // Create result slice and add a rank value
    results := []leaderboardRecord{}
    for i, record := range records {
        r := leaderboardRecord{
            Username: record.Username.Value,
            UserId:   record.OwnerId,
            Score:    int(record.Score),
            Rank:     len(records) - i,
        }
        results = append(results, r)
    }

    return results, nil
}
```

## Getting the group leaderboard view

Now that you have a general function to get a leaderboard for a list of users, create a function to get a leaderboard for a group.

Create a payload struct for the remote procedure:

```go
type groupLeaderboardRecordsPayload struct {
    GroupId       string `json:"groupId"`
    LeaderboardId string `json:"leaderboardId"`
}
```

Create a function that will get a slice of members from a group and then get the leaderboard for them:

```go
func getGroupLeaderboardRecords(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
    // Unmarshal the payload
    data := &groupLeaderboardRecordsPayload{}
    if err := json.Unmarshal([]byte(payload), data); err != nil {
        logger.Error("error unmarshaling payload")
        return "", err
    }

    // Get leaderboard
    leaderboards, err := nk.LeaderboardsGetId(ctx, []string{data.LeaderboardId})
    if err != nil {
        logger.Error("error getting leaderboards")
        return "", err
    }

    if len(leaderboards) == 0 {
        errorMessage := fmt.Sprintf("error finding leaderboard: %s", data.LeaderboardId)
        logger.Error(errorMessage)
        return "", errors.New(errorMessage)
    }

    // Get group members
    members, _, err := nk.GroupUsersList(ctx, data.GroupId, 100, nil, "")
    if err != nil {
        logger.Error("error getting group members")
        return "", err
    }

    // Get a slice of memberIds
    memberIds := []string{}
    for _, member := range members {
        memberIds = append(memberIds, member.User.Id)
    }

    // Get all leaderboard records for users
    results, err := getLeaderboardForUsers(leaderboards[0].Id, memberIds, ctx, logger, nk)
    if err != nil {
        logger.Error("error getting leaderboard records")
        return "", err
    }

    // Return the leaderboard records to the user
    bytes, err := json.Marshal(results)
    if err != nil {
        logger.Error("error marshaling result")
        return "", err
    }

    return string(bytes), nil
}
```

Register the function and expose it as a remote procedure that can be called from the client:

```go
func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
    // ...

    // Register RPC to retrieve a custom view over a leaderboard based on group members
    if err := initializer.RegisterRpc("getGroupLeaderboardRecords", getGroupLeaderboardRecords); err != nil {
        logger.Error(`error registering "getGroupLeaderboardRecords" rpc`)
        return err
    }

    return nil
}
```

## Getting the friend leaderboard view

Similarly, create a function to get a leaderboard for a user's friends.

Create a payload struct for the remote procedure:

```go
type friendsLeaderboardRecordsPayload struct {
    LeaderboardId string `json:"leaderboardId"`
}
```

Create a function that will get a slice of a user's friends and then get the leaderboard for them:

```go
func getFriendLeaderboardRecords(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
    // Unmarshal the payload
    data := &friendsLeaderboardRecordsPayload{}
    if err := json.Unmarshal([]byte(payload), data); err != nil {
        logger.Error("error unmarshaling payload")
        return "", err
    }

    // Get leaderboard
    leaderboards, err := nk.LeaderboardsGetId(ctx, []string{data.LeaderboardId})
    if err != nil {
        logger.Error("error getting leaderboards")
        return "", err
    }

    if len(leaderboards) == 0 {
        errorMessage := fmt.Sprintf("error finding leaderboard: %s", data.LeaderboardId)
        logger.Error(errorMessage)
        return "", errors.New(errorMessage)
    }

    // Get user id from context
    userId, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
    if !ok {
        errorMessage := fmt.Sprintf("error getting user id from context")
        logger.Error(errorMessage)
        return "", errors.New(errorMessage)
    }

    // Get friends (where state is 0 - mutual friends)
    state := 0
    friends, _, err := nk.FriendsList(ctx, userId, 100, &state, "")
    if err != nil {
        logger.Error("error getting friends")
        return "", err
    }

    // Get a slice of memberIds
    friendIds := []string{}
    for _, member := range friends {
        friendIds = append(friendIds, member.User.Id)
    }

    // Get all leaderboard records for users
    results, err := getLeaderboardForUsers(leaderboards[0].Id, friendIds, ctx, logger, nk)
    if err != nil {
        logger.Error("error getting leaderboard records")
        return "", err
    }

    // Return the leaderboard records to the user
    bytes, err := json.Marshal(results)
    if err != nil {
        logger.Error("error marshaling result")
        return "", err
    }

    return string(bytes), nil
}
```

Register the function and expose it as a remote procedure that can be called from the client:

```go
func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
    // ...

    // Register RPC to retrieve a custom view over a leaderboard based on friends
    if err := initializer.RegisterRpc("getFriendLeaderboardRecords", getFriendLeaderboardRecords); err != nil {
        logger.Error(`error registering "getFriendLeaderboardRecords" rpc`)
        return err
    }

    return nil
}
```
