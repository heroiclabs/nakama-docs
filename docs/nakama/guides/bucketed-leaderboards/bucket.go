package modules

import (
	"context"
	"database/sql"
	"encoding/json"
	"math/rand"

	"github.com/heroiclabs/nakama-common/api"
	"github.com/heroiclabs/nakama-common/runtime"
	"github.com/satori/go.uuid"
)

// Define the bucketed leaderboard storage object
type userBucketStorageObject struct {
	ResetTimeUnix uint32   `json:"resetTimeUnix"`
	UserIDs       []string `json:"userIds"`
}

// Get a user's bucket (records) and generate a new bucket if needed
func RpcGetBucketRecordsFn(ids []string, bucketSize int) func(context.Context, runtime.Logger, *sql.DB, runtime.NakamaModule, string) (string, error) {
	return func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
		if len(payload) > 0 {
			return "", ErrNoInputAllowed
		}

		userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
		if !ok {
			return "", ErrNoUserIdFound
		}

		collection := "buckets"
		key := "bucket"

		objects, err := nk.StorageRead(ctx, []*runtime.StorageRead{
			{
				Collection: collection,
				Key:        key,
				UserID:     userID,
			},
		})
		if err != nil {
			logger.Error("nk.StorageRead error: %v", err)
			return "", ErrInternalError
		}

		// Fetch any existing bucket or create one if none exist
		userBucket := &userBucketStorageObject{ResetTimeUnix: 0, UserIDs: []string{}}
		if len(objects) > 0 {
			if err := json.Unmarshal([]byte(objects[0].GetValue()), userBucket); err != nil {
				logger.Error("json.Unmarshal error: %v", err)
				return "", ErrUnmarshal
			}
		}

		// Fetch the tournament leaderboard
		leaderboards, err := nk.TournamentsGetId(ctx, ids)
		if err != nil {
			logger.Error("nk.TournamentsGetId error: %v", err)
			return "", ErrInternalError
		}

		// Leaderboard has reset or no current bucket exists for user
		if userBucket.ResetTimeUnix != leaderboards[0].GetEndActive() || len(userBucket.UserIDs) < 1 {
			logger.Debug("rpcGetBucketRecordsFn new bucket for %q", userID)

			pivotID := uuid.Must(uuid.NewV4(), nil).String()

			// Use a KEYSET clause to efficiently select users at a random pivot
			// Note: Increase bucketSize to overscan and filter in the application layer
			rows, err := db.QueryContext(ctx, `SELECT id FROM users WHERE id > $1 LIMIT $2`, pivotID, bucketSize)
			if err != nil {
				logger.Error("db.QueryContext error: %v", err)
				return "", ErrInternalError
			}
			//goland:noinspection GoUnhandledErrorResult
			defer rows.Close()

			for rows.Next() {
				var id string
				if err := rows.Scan(&id); err != nil {
					logger.Error("rows.Scan error: %v", err)
					return "", ErrInternalError
				}
				if id == userID || id == "00000000-0000-0000-0000-000000000000" {
					continue
				}
				userBucket.UserIDs = append(userBucket.UserIDs, id)
			}
			if err := rows.Err(); err != nil {
				logger.Error("rows.Err error: %v", err)
				return "", ErrInternalError
			}

			// Not enough users to fill bucket with random pivot only
			if len(userBucket.UserIDs) < bucketSize {
				rows2, err := db.QueryContext(ctx, `SELECT id FROM users LIMIT $1`, bucketSize-len(userBucket.UserIDs))
				if err != nil {
					logger.Error("db.QueryContext error: %v", err)
					return "", ErrInternalError
				}
				//goland:noinspection GoUnhandledErrorResult
				defer rows2.Close()

				for rows2.Next() {
					var id string
					if err := rows2.Scan(&id); err != nil {
						logger.Error("rows2.Scan error: %v", err)
						return "", ErrInternalError
					}
					if id == userID || id == "00000000-0000-0000-0000-000000000000" {
						continue
					}
					userBucket.UserIDs = append(userBucket.UserIDs, id)
				}
				if err := rows2.Err(); err != nil {
					logger.Error("rows.Err error: %v", err)
					return "", ErrInternalError
				}
			}

			// Set the Reset and Bucket end times to be in sync
			userBucket.ResetTimeUnix = leaderboards[0].GetEndActive()

			value, err := json.Marshal(userBucket)
			if err != nil {
				return "", ErrMarshal
			}

			// Store generated bucket for the user
			if _, err := nk.StorageWrite(ctx, []*runtime.StorageWrite{
				{
					Collection:      collection,
					Key:             key,
					PermissionRead:  0,
					PermissionWrite: 0,
					UserID:          userID,
					Value:           string(value),
				},
			}); err != nil {
				logger.Error("nk.StorageWrite error: %v", err)
				return "", ErrInternalError
			}
		}

		// Add self to the list of leaderboard records to fetch
		userBucket.UserIDs = append(userBucket.UserIDs, userID)

		// Generate some dummy leaderboard scores for demo purposes only - you would not have this in production
		accounts, _ := nk.AccountsGetId(ctx, userBucket.UserIDs)
		for _, account := range accounts {
			score := rand.Int63n(10000)
			nk.LeaderboardRecordWrite(ctx, ids[0], account.GetUser().GetId(), account.GetUser().GetUsername(), score, 0, nil, nil)
		}

		_, records, _, _, err := nk.LeaderboardRecordsList(ctx, ids[0], userBucket.UserIDs, bucketSize, "", 0)
		if err != nil {
			logger.Error("nk.LeaderboardRecordsList error: %v", err)
			return "", ErrInternalError
		}

		result := &api.LeaderboardRecordList{Records: records}
		encoded, err := json.Marshal(result)
		if err != nil {
			return "", ErrMarshal
		}

		logger.Debug("rpcGetBucketRecordsFn resp: %s", encoded)
		return string(encoded), nil
	}
}
