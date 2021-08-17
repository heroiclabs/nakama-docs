# Architecture Overview

Nakama is a monolithic stateful server that exposes realtime and non-realtime APIs from multiple subsystems. Nakama’s subsystems cover a variety of tasks, with the major subsystems discussed below.

## Authorization

Nakama’s authorization system handles [user authentication](../concepts/authentication.md), [session state](../concepts/session.md), and [authorization](../concepts/access-controls.md). This system is responsible for securely linking users to relevant data. The system:

* Establishes trusted client connections with signed JSON Web Tokens (JWTs)
* Links users accounts to [social sign-ins](../concepts/authentication.md#social-providers)
* Embeds custom data properties for edge caching

## Cluster Management

!!! note "Note"
    Cluster management is a feature of [Nakama Enterprise](https://heroiclabs.com/nakama-enterprise/).

By relying on conflict-free replicated data types and gossip-based peer-to-peer connections, Nakama’s cluster management system provides built-in service discovery. 

With this system, a Nakama cluster can gracefully respond to the loss of individual nodes or distributing load to new nodes brought online to handle surges in traffic. The system reacts to changes in the cluster’s topology and records changes in client connections to each node, to support flexible and efficient scaling.

## Console and metrics

Nakama’s built-in [console](console-overview.md) and metrics system provides essential tools for DevOps professionals. The console provides a standalone interface to inspect a node’s status and data, while metrics exports data via Prometheus to your team’s preferred external monitoring and analytics tools.

## Database

Nakama’s database system manages long-term persistence. While Nakama’s in-memory system provides fast read and write access to a variety of data, Nakama’s database component is a methodical bookkeeper, ensuring that long-lived data is stored efficiently and reliably.

When it comes to persistence, Nakama is ready for many deployment scenarios, supporting any PostgreSQL wire-compatible database. In a canonical configuration, Nakama runs alongside [CockroachDB](https://github.com/cockroachdb/cockroach) for scalable, geographically distributed, and durable data storage.

## External interfaces

Nakama’s external interface system exposes socket and request interfaces. Games built with Nakama can use both interfaces, but may only need one or the other, depending on the game design and other details specific to your game’s experience.

The socket interface is the primary entrypoint for realtime activity such as [chat](../concepts/realtime-chat.md) and [realtime multiplayer](../concepts/server-authoritative-multiplayer.md). The socket interface runs on WebSockets and rUDP, with a choice of binary (protocol buffers) or text (JSON) payloads.

The request interface, which runs on gRPC and HTTP, is the primary entrypoint for non-realtime activity, such as user account management.

## In-memory data

Nakama’s in-memory data system takes the place of an external in-memory data store, such as Redis. Under the hood, Nakama uses [Bleve](https://blevesearch.com/) to unlock full-text search on arbitrary JSON fields, providing a range of lightning-fast searches. 

The Nakama in-memory data system can be used for sophisticated and efficient [matchmaking](../concepts/matches.md) searches for matches with suitable labels (e.g. open-to-join) or players with common attributes (e.g. magic skill level X).

## Management

Nakama’s management system handles match lifecycle activities, scheduling for [leaderboards](../concepts/leaderboards.md) and [tournaments](../concepts/tournaments.md), [matchmaking](../concepts/matches.md), and hosting [server-authoritative multiplayer](../concepts/server-authoritative-multiplayer.md) resources. 

Most critically, this system manages the resources consumed by client activities and your game’s [custom server-side logic](../server-framework/basics.md).

## Message routing

Nakama’s message routing system makes sure that realtime client messages reach the correct nodes across the cluster, transparently. The message routing system tracks the whole cluster’s set of socket connections to clients and routes incoming messages to the right nodes, regardless of the cluster’s topology. The message routing system supports all of Nakama’s realtime features, such as [chat](../concepts/realtime-chat.md) and [status](../concepts/status.md).

## Presence

The presence tracking system builds on [authorization](#authorization) to represent a player’s live activity in the game. The player’s presence is recorded uniquely as a combination of the user, the session, and the node to which they’re connected. 

By way of presence, developers can help players set generic [statuses](../concepts/status.md) (such as available or busy), free-form status messages to friends (e.g. “Looking for a party to join!”), or compose more complex interactions (e.g. inviting friends to spectate the player’s current match).

## Streams

Nakama uses the [streams](../server-framework/streams.md) system to efficiently share data between clients. Streams are Nakama’s core representation of any real-time activity, such as [chat](../concepts/realtime-chat.md), [notifications](../concepts/in-app-notifications.md), and [matches](../concepts/matches.md). 

If data needs to be distributed to clients live, streams are the way it gets there. Individual player sessions join and leave streams, like subscribing and unsubscribing to a continuous flow of messages.
