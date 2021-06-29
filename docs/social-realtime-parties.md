# Realtime Parties

!!! Note
    Realtime Parties is new in the 3.0 release of Nakama server. Read more about it in the [release announcement](https://heroiclabs.com/blog/announcements/nakama-3-0/).

Realtime parties is a great way to add team play to a game, enabling users to form a party and communicate with party members.

A party is a group of users who’ve gathered together to participate in some kind of gameplay or social gathering. Any user can create a party and will become the initial party leader. Users can invite other players to join or can mark the party so other players must request to join. A leader is always required and selected from players who are currently connected when the current leader leaves. When all players leave the party it’s gone.

A very common Nakama-supported use case for parties is to matchmake together in groups. This suits gameplay where users collaborate in squads, or where players compete in team battles. The [matchmaker](gameplay-matchmaker.md) can be passed a party ID which instructs the matching logic to ensure that enough capacity is reserved to join the match altogether.

Parties differ from the [groups](social-groups-clans.md) feature because they’re not designed to exist between play sessions when the user goes offline. A party only exists as long as at least one user is in it. You should use groups if you want to create guild systems or other gameplay which should exist as a persistent entity in your game or apps.

## Create party

A party can be created by any user. You can limit the max number of users allowed in the party (up to 256) and whether a party member must wait for approval from the leader before they can join. A party is also created with state which can be modified by the party leader.

=== ".NET"
    ```csharp
    // Create an open party (i.e. no approval needed to join) with 10 max users
    // This maximum does not include the party leader
    var party = await socket.CreatePartyAsync(open: true, maxSize: 10);
    System.Console.WriteLine("New Party: {0}", party);

    // Create a closed party (i.e. approval needed to join) with 5 max users
    // This maximum does not include the party leader
    var party = await socket.CreatePartyAsync(open: false, maxSize: 5);
    System.Console.WriteLine("New Party: {0}", party);
    ```

=== "Unity"
    ```csharp
    // Create an open party (i.e. no approval needed to join) with 10 max users
    // This maximum does not include the party leader
    var party = await socket.CreatePartyAsync(open: true, maxSize: 10);
    Debug.Log("New Party: " + party.ToString());

    // Create a closed party (i.e. approval needed to join) with 5 max users
    // This maximum does not include the party leader
    var party = await socket.CreatePartyAsync(open: true, maxSize: 5);
    Debug.Log("New Party: " + party.ToString());
    ```

You can create rules on which users can create parties in your game with [before hooks](runtime-code-basics.md#before-hook) using runtime code.

## Find party

After creating a new party, there’s many ways for the party ID to be shared with other users to enable them to find and join:

* [Status events](social-status.md) to post the ID as an update, so a player's subscribers can see them active in a party.
* [In-app notification](social-in-app-notifications.md) to the user’s [friends](social-friends.md) list.
* [Guild](social-groups-clans.md), sharing the party ID with all members.

The way you combine these features of the server to power your game design can create wonderful social mechanics to bring users together and create a deep and meaningful player community.

=== ".NET"
    ```csharp
    string partyId = "<partyid>";

    // Set a status update with your party ID
    await socket.UpdateStatusAsync("Join my party: ", partyId);
    ```

=== "Unity"
    ```csharp
    string partyId = "<partyid>";

    // Set a status update with your party ID
    await socket.UpdateStatusAsync("Join my party: ", partyId);
    ```

With the party ID, users can then [join the party](#join-party).

## Join party

A user must join a party before they can send messages, see the member list, or invite other users.

=== ".NET"
    ```csharp
    string partyId = "<partyid>";
    await socket.JoinPartyAsync(partyId);

    socket.ReceivedParty += party =>
    {
        System.Console.WriteLine("Joined party: " + party);
    };
    ```

=== "Unity"
    ```csharp
    string partyId = "<partyid>";
    await socket.JoinPartyAsync(partyId);

    socket.ReceivedParty += party =>
    {
        Debug.Log("Joined party: " + party.ToString());
    };
    ```

If the party is private, users can request to join and the party leader can accept or reject.

=== ".NET"
    ```csharp
    string partyId = "<partyid>";
    // List all existing join requests
    var requests = await socket.ListPartyJoinRequestsAsync(partyId);

    // Accept a join request
    await socket.AcceptPartyMemberAsync(partyId, <userPresence>);

    // Reject a join request
    await socket.RemovePartyMemberAsync(partyId, <userPresence>);
    ```

=== "Unity"
    ```csharp
    string partyId = "<partyid>";
    // List all existing join requests
    var requests = await socket.ListPartyJoinRequestsAsync(partyId);

    // Accept a join request
    await socket.AcceptPartyMemberAsync(partyId, <userPresence>);

    // Reject a join request
    await socket.RemovePartyMemberAsync(partyId, <userPresence>);
    ```

## Leave party

A user can leave a party at any time. The user will also be automatically removed from the party if they disconnect and do not reconnect and rejoin the party within the allowed transient disconnect timeout.

!!! Note
    The transient disconnect timeout period can be [configured](install-configuration.md) as a startup option of the server.

=== ".NET"
    ```csharp
    string partyId = "<partyid>";
    var party = await socket.LeavePartyAsync(partyId);
    System.Console.WriteLine("Left party: " + party);
    ```

=== "Unity"
    ```csharp
    string partyId = "<partyid>";
    var party = await socket.LeavePartyAsync(partyId);
    Debug.Log("Left party: " + party.ToString());
    ```

### Party leader rotation

When a user leaves the server will check whether they are the current party leader and, if they were, send a message to indicate the leader has left and which member was promoted as the new party leader.

When an automated promotion happens the leader chosen will always be the user who has been part of the party for the longest since it was created. This is a deterministic process and cannot be changed. See [manual leader promotion](#manual-leader-promotion) if your game needs different criteria to promote a new leader.

## Send messages

Any user who is a party member can send messages to the party containing: text, emotes, or game specific actions.

=== ".NET"
    ```csharp
    // Sending a message
    socket.SendPartyDataAsync(partyId: "<partyid>", opCode: 1, data: System.Text.Encoding.UTF8.GetBytes("{<message>}"));

    // Receiving the message
    socket.ReceivedPartyData += data =>
    {
        System.Console.WriteLine("Received: " + System.Text.Encoding.UTF8.GetString(data));
    };
    ```

=== "Unity"
    ```csharp
    // Sending a message
    socket.SendPartyDataAsync(partyId: "<partyid>", opCode: 1, data: System.Text.Encoding.UTF8.GetBytes("{<message>"));

    // Receiving the message
    socket.ReceivedPartyData += data =>
    {
        Debug.Log("Received: " + data.Data);
    };
    ```

### Manual leader promotion

As well as regular messages the party leader can also send a special promotion message which will declare that they’ve stepped down as the current leader and who they have promoted to the new leader.

=== ".NET"
    ```csharp
    socket.ReceivedPartyLeader += newLeader =>
    {
        System.Console.WriteLine("new party leader " + newLeader);
    };

    await socket.PromotePartyMemberAsync("<partyid>", partyMember);
    ```

=== "Unity"
    ```csharp
    socket.ReceivedPartyLeader += newLeader =>
    {
        System.Console.WriteLine("new party leader " + newLeader);
    };

    await socket.PromotePartyMemberAsync("<partyid>", partyMember);
    ```

## Close party

A party cannot be closed by any member other than the party leader. The party leader can also eject all party members, which will close the party and clear its state on the server. In most cases its more useful from a game design perspective to allow the leader to leave.

=== ".NET"
    ```csharp
    string partyId = "<partyid>";
    await socket.ClosePartyAsync(partyId);
    ```

=== "Unity"
    ```csharp
    string partyId = "<partyid>";
    await socket.ClosePartyAsync(partyId);
    ```

## Best practices

We recommend you don’t use parties to implement multiplayer game logic or use it for scenarios where the messages sent to the party members is intended to drive gameplay progression. In these cases it’s better to use the [authoritative multiplayer engine](gameplay-multiplayer-server-multiplayer.md) in the game server. This gives you complete control over your multiplayer net code and the logic associated with actions taken by users.