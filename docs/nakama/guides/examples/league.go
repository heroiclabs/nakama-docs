package main

import (
	"context"
	"database/sql"
	"github.com/heroiclabs/nakama-common/runtime"
	"time"
)

func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
	initStart := time.Now()

	// Create the two tiers of leaderboards
	bottomTierId := "bottom-tier"
	topTierId := "top-tier"
	authoritative := true
	sortOrder := "desc"
	operator := "inc"
	resetSchedule := "0 0 * * 1"
	metadata := make(map[string]interface{})
	nk.LeaderboardCreate(ctx, bottomTierId, authoritative, sortOrder, operator, resetSchedule, metadata)
	nk.LeaderboardCreate(ctx, topTierId, authoritative, sortOrder, operator, resetSchedule, metadata)

	// Register leaderboard reset function to handle promotions and relegations
	initializer.RegisterLeaderboardReset(func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, leaderboard *api.Leaderboard, reset int64) error {
		// We're only interested in our top/bottom tier leaderboards so return if it isn't them
		if leaderboard.Id != topTierId && leaderboard.Id != bottomTierId {
			return nil
		}

		// Get all 100 records (assuming the tier has a max of 100 players) from the tier
		records, _, _, _, _ := nk.LeaderboardRecordsList(ctx, leaderboard.Id, []string{}, 100, "", reset)

		// If leaderboard is top tier and has 10 or more players, relegate bottom 3 players
		if leaderboard.Id == topTierId && len(records) >= 10 {
			for _, record := range records[len(records)-3:] {
				// Relegate record owner by copying their record into the bottom tier and deleting their current top tier record
				nk.LeaderboardRecordWrite(ctx, bottomTierId, record.OwnerId, record.Username.Value, record.Score, record.Subscore, nil, nil)
				nk.LeaderboardRecordDelete(ctx, topTierId, record.OwnerId)
			}
		}

		// If leaderboard is bottom tier and has 10 or more players, promote top 3 players
		if leaderboard.Id == bottomTierId && len(records) >= 10 {
			for _, record := range records[0:2] {
				// Promote record owner by copying their record into the top tier and deleting their current bottom tier record
				nk.LeaderboardRecordWrite(ctx, topTierId, record.OwnerId, record.Username.Value, record.Score, record.Subscore, nil, nil)
				nk.LeaderboardRecordDelete(ctx, bottomTierId, record.OwnerId)
			}
		}

		// Distribute rewards based on player's tier
		for _, record := range records {
			reward := int64(100)

			if leaderboard.Id == topTierId {
				reward = 500
			}

			changeset := map[string]int64{
				"coins": reward,
			}

			nk.WalletUpdate(ctx, record.OwnerId, changeset, nil, true)
		}

		return nil
	})

	logger.Info("Plugin loaded in '%d' msec.", time.Since(initStart).Milliseconds())
	return nil
}
