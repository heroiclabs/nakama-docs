local nk = require("nakama")

-- Create the two tiers of leaderboards
local bottom_tier_id = "bottom-tier"
local top_tier_id = "top-tier"
local authoritative = true
local sort = "desc"
local operator = "best"
local reset = "0 0 * * 1"
local metadata = {}
nk.leaderboard_create(bottom_tier_id, authoritative, sort, operator, reset, metadata)
nk.leaderboard_create(top_tier_id, authoritative, sort, operator, reset, metadata)

-- Register leaderboard reset function to handle promotions and relegations
local function leaderboardReset(ctx, leaderboard, reset)
    -- We're only interested in our top/bottom tier leaderboards so return if it isn't them
    if leaderboard.id ~= bottom_tier_id and leaderboard.id ~= top_tier_id then
        return
    end

    -- Get all leaderboard records (assuming the tier has no more than 10,000 players)
    local records, _, _, _ = nk.leaderboard_records_list(leaderboard.id, {}, 10000)

    -- If leaderboard is top tier and has 10 or more players, relegate bottom 3 players
    if leaderboard.id == top_tier_id and #records >= 10 then
        -- Relegate record owner by copying their record into the bottom tier and deleting their current top tier record
        for i=3, 1, -1 do
            local record = records[#records-i]
            nk.leaderboard_record_write(bottom_tier_id, record.owner, record.username, record.score, record.subscore, {})
            nk.leaderboard_record_delete(top_tier_id, record.owner)
        end
    end

    -- If leaderboard is bottom tier and has 10 or more players, promote top 3 players
    if leaderboard.id == bottom_tier_id and #records >= 10 then
        -- Promote record owner by copying their record into the top tier and deleting their current top tier record
        for i=1, 3 do
            local record = records[i]
            nk.leaderboard_record_write(top_tier_id, record.owner, record.username, record.score, record.subscore, {})
            nk.leaderboard_record_delete(bottom_tier_id, record.owner)
        end
    end

    -- Distribute rewards based on player's tier
    for i=1, #records do
        local record = records[i]
        local reward = 100

        -- Increase reward for top tier players
        if leaderboard.id == top_tier_id then
            reward = 500
        end

        local changeset = {
            coins = reward
        }

        nk.wallet_update(record.owner, changeset, {}, true)
    end
end
nk.register_leaderboard_reset(leaderboardReset)