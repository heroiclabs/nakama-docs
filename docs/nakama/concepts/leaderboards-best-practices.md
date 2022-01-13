# Leaderboards best practices

Leaderboards are often part of a game’s competitive experience. Nakama provides a data model and API for creating, calculating, and managing leaderboards. In this article, you’ll learn about:

- The lifecycle of a leaderboard
- How Nakama models leaderboards and individual scores
- How to more out of leaderboards with best practices

## Leaderboards and why you should use them

In Nakama, a leaderboard is a real-time ranking of scores. If you can represent a score as a magnitude, then you can put it on a leaderboard. Leaderboards can track bigger-is-better results, such as points earned or gold and jewels collected, or smaller-is-better results, such as power-ups used or seconds elapsed.

Leaderboards provide a base on which to build your game’s specific flavor of leaderboard. They’re efficient to store, fast to recalculate—thanks to in-memory caching and ranking—and flexible enough for any sortable scoreboard.

## The lifecycle of a leaderboard

From a high level, Nakama handles leaderboards and scores through a three-part lifecycle:

1. **Start.** When you create a leaderboard, you choose some rules for it, such as whether higher scores are better or whether it resets on a schedule. These properties of the leaderboard are fixed at creation time and stay the same as long as the leaderboard exists.

2. **Score.** Once play begins, your game populates the leaderboard with scores. Each time a score is recorded, Nakama recalculates the ranking according to the rules set at the leaderboard’s creation.

   You application can use four methods to record scores:

   - `set` — insert a new score
   - `best` — insert a new score, if it’s better than that owner’s previous score
   - `incr` — add to the owner’s previous score
   - `decr` — subtract from the owner’s previous score

   If a leaderboard isn’t scheduled to reset, then it continues in this phase indefinitely.

3. **Expire.** When a leaderboard reaches its scheduled reset time, Nakama triggers callbacks and purges expired scores from the ranking.

   For any event handlers registered for the expiration of a leaderboard, the expiration triggers callbacks with the state of the leaderboard at the end of the iteration. For example, an event handler is triggered that rewards the top five finishers.

   At the same time, Nakama purges expired scores from the leaderboard ranking. Expired scores aren’t deleted, but they no longer count toward the leaderboard’s live ranking. In other words, the ranking starts again from the beginning of the score phase of the lifecycle.

## Modeling leaderboards

Next, let’s look at how Nakama actually represents leaderboards. In Nakama, leaderboards are recorded in two parts: a _leaderboard_ and many _leaderboard records_.

### Leaderboards

When you create a _leaderboard_, you set a few immutable properties that determine how Nakama recalculates rankings, whether to expire records, and the semantics of inserting new scores into the leaderboard.

The most important properties of a leaderboard include:

| Property                 | Description                                                                       | Example                                          |
| ------------------------ | --------------------------------------------------------------------------------- | ------------------------------------------------ |
| ID                       | A unique key for the leaderboard                                                  | `weekly_best_lap_time`                           |
| Authoritative status     | Whether clients are allowed to insert their own scores                            | false`                                           |
| Sort order               | An ascending (lower is better) or descending (higher is better) ranking of scores | `asc`                                            |
| Operator                 | The semantics of score insertion, such as latest, personal best, or additive      | `incr` to add to the previous score              |
| Maximum number of scores | The number of times a score owner can enter into the ranking                      | `3`                                              |
| Reset schedule           | A cron-like schedule for expiring old scores                                      | `59 23 * * 0` to reset at the end of each Sunday |
| JSONB metadata           | Metadata for the leaderboard                                                      | `{ "displayString": "Weekly Top Scores" }`       |


From a storage perspective, Nakama writes leaderboard details to the database as an authoritative, permanent record of the leaderboard’s configuration. Meanwhile, Nakama creates an in-memory cache of the leaderboard for live sorting.

### Leaderboard records

A _leaderboard record_ captures an individual score added to a leaderboard.

The most important properties of a leaderboard record include:

| Property       | Description                                                | Example                                |
| -------------- | ---------------------------------------------------------- | -------------------------------------- |
| Leaderboard ID | The leaderboard the score relates to                       | `weekly_best_lap_time`                 |
| Owner ID       | The owner of the score, such as a user or group            | `e4f8c28d-1680-4f35-8ccc-67910185d84e` |
| Username       | The display name for the owner of the record               | `anon1234`                             |
| Score          | The score as an integer                                    | `70` (seconds)                         |
| Subscore       | A secondary or fractional score to break ties              | `54` (hundredths of a second)          |
| Expiry time    | When the entry ages out of the ranking for the leaderboard | `2021-01-02T23:59:00`                  |
| JSONB metadata | Metadata for the score                                     | `{ "raining": false }`                 |


Like the leaderboard itself, leaderboard records reside in two places: the database, as a canonical record of the score, and in Nakama’s in-memory representation of the leaderboard, for live sorting.

Note that the ranking of scores themselves is not part of the storage model. Ranking is kept in memory for rapid recalculation and retrieval.

Taken together, leaderboards and leaderboard records provide fast lookups, quick updates to the ranking, and durability and resilience for the cluster as a whole.

## Best practices

Now that you have a foundation in how Nakama’s leaderboards are defined, consider these best practices for using leaderboards in your game.

### Use owner IDs for more than just players

Each leaderboard record has an owner. A common choice for the owner of a leaderboard record is a user ID, representing a single player. But that’s only the start of possibilities for leaderboards.

For instance, you can create multiple leaderboards and record the performance of a player in one leaderboard and the collective score of that user’s group in another, to provide real-time rankings for both individual and team efforts.

### Repeat leaderboards using reset schedules

Don’t overlook leaderboards’ expiration schedule. You might be tempted to create new leaderboards on a periodic basis. In most cases, Nakama can help.

If you reset a leaderboard instead of creating a new one, you can take advantage of simpler leaderboard accounting on the client side. For example, the client need only know the ID of the hourly leaderboard, rather than know the ID of the individual latest hourly leaderboard (and all of the time zone and synchronization details that go along with it). Additionally, the admin console tools are built with schedule awareness, to make it easier to manage the data used by your leaderboards.

### Split large cohorts with bucketed leaderboards

Over time, large leaderboards can become static and disengaging. As your users gain skill and experience with the game, the best scores are cemented and rarely change without heroic efforts. Segmenting leaderboards into smaller cohorts—even a random subset of users—can increase rankings turnover and engagement.

Making small slices of global leaderboards can work in some cases, such as filtering the global leaderboard by a users’ friends. But calculating rank for subsets of a leaderboard can become very costly, as the size of the subset increases.

Instead, slice up large cohorts into separate leaderboards, known as _bucketed leaderboards_. Creating multiple leaderboards typically has better performance characteristics than filtering larger leaderboards. To learn more about bucketed leaderboards, read [the guide on this topic](/docs/nakama/guides/bucketed-leaderboards/).

## Further reading

To see leaderboards in a real game, check out the [Leaderboards](https://heroiclabs.com/docs/nakama/tutorials/unity/pirate-panic/leaderboards/) section of the [Pirate Panic](/docs/nakama/tutorials/unity/pirate-panic/) tutorial.

Tournaments build on what you’ve learned about leaderboards. To build even more competitive action into your game, [read about tournaments](/docs/nakama/concepts/tournaments/).
