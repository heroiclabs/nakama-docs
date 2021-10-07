function InitModule(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
    // Create the two tiers of leaderboards
    let bottomTierId = "bottom-tier";
    let topTierId = "top-tier";
    let authoritative = true;
    let sortOrder = nkruntime.SortOrder.DESCENDING;
    let operator = nkruntime.Operator.INCREMENTAL;
    let resetSchedule = "0 0 * * 1";
    let metadata = {};
    nk.leaderboardCreate(bottomTierId, authoritative, sortOrder, operator, resetSchedule, metadata);
    nk.leaderboardCreate(topTierId, authoritative, sortOrder, operator, resetSchedule, metadata);

    // Register leaderboard reset function to handle promotions and relegations
    let leaderboardReset: nkruntime.LeaderboardResetFunction = (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, leaderboard: nkruntime.Leaderboard, reset: number) => {
        // We're only interested in our top/bottom tier leaderboards so return if it isn't them
        if (leaderboard.id !== topTierId && leaderboard.id !== bottomTierId) {
            return;
        }

        let result = nk.leaderboardRecordsList(leaderboard.id, null, 10000, null, reset);

        // Get all leaderboard records (assuming the tier has no more than 10,000 players)
        if (leaderboard.id === topTierId && result.records.length >= 10) {
            // Relegate record owner by copying their record into the bottom tier and deleting their current top tier record
            result.records.slice(result.records.length - 3).forEach(r => {
                nk.leaderboardRecordWrite(bottomTierId, r.ownerId, r.username, r.score, r.subscore, null, null);
                nk.leaderboardRecordDelete(topTierId, r.ownerId);
            });
        }

        // If leaderboard is bottom tier and has 10 or more players, promote top 3 players
        if (leaderboard.id === topTierId && result.records.length >= 10) {
            // Promote record owner by copying their record into the top tier and deleting their current bottom tier record
            result.records.slice(0, 3).forEach(r => {
                nk.leaderboardRecordWrite(topTierId, r.ownerId, r.username, r.score, r.subscore, null, null);
                nk.leaderboardRecordDelete(bottomTierId, r.ownerId);
            });
        }

        // Distribute rewards based on player's tier
        result.records.forEach(r => {
            let reward = 100;

            // Increase reward for top tier players
            if (leaderboard.id === topTierId) {
                reward = 500;
            }

            let changeset = {
                coins: reward
            };

            nk.walletUpdate(r.ownerId, changeset, null, true);
        });
    };
    initializer.registerLeaderboardReset(leaderboardReset);
}

// Reference InitModule to avoid it getting removed on build
!InitModule && InitModule.bind(null);