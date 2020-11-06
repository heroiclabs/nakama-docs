# Choosing TCP or UDP: a guide for game developers

As a game developer, you’ll hear lots of myths about network code, especially when it comes to choosing UDP or TCP. You’ll be told that:

* *You can’t build anything fast with TCP*, dismissing counterexamples.

* *UDP is always faster than TCP*, ignoring that UDP is deprioritized by traffic-shaping networks.

* *Your game must have the fastest net code*, pretending that a game’s design or player behavior makes no difference.

If you build your game around these myths, then your development progress will slow as you focus on increasingly complex and fragile netcode. Players will become frustrated when the game’s netcode doesn’t mesh with their gameplay expectations. And you’ll be disappointed when your investment in myths doesn’t pay off with your players’ enjoyment.

You need a way to reason about netcode, instead of blindly believing in myths.

## Network dreams

In an ideal world, you wouldn’t have to think about TCP or UDP at all. On a perfect network, every packet sent would be received. There would be no reason to ever confirm receipt of a message. You could *trust *that the network delivered your message anywhere on Earth, in order and in less than about 70ms. Lag and packet loss? Unheard of.

The bad news is that we don’t live in a perfect world with perfect networks. Network conditions vary and protocols serve different needs. Should you choose TCP or UDP? It’s impossible to say. There’s no optimal choice when it comes to your transport layer.

But there’s good news: there’s no optimal choice when it comes to your transport layer. TCP and UDP can satisfy different needs, often within the same game. We don’t have to choose one true protocol. Instead, we can write netcode in concert with game design, minimizing the significance of protocols entirely.

## Netcode and game design are intertwined

When you think about the relationship between your game’s design and netcode, you’re often thinking in terms of interactivity. What happens when a player makes a decision and acts on that decision? Does the game respond to the player and how does it feel?

Netcode and the network itself are part of that interaction loop. In a first-person shooter, slow messages might give a sluggish or jerky feel to movement. But in a deck-building game, slow messages might heighten the drama and anticipation of the player’s next move.

Of course, few game designs fit neatly into "fast" or “slow” interactivity. Most games have a mix of more or less interactive gameplay elements, each with different networking requirements. A deck-builder might have real-time voice chat in addition to turn-based mechanics. A first-person shooter might have discrete, punctuating events, such as matchmaking or consuming an inventory item, alongside a fast, continuous simulation.

## Some messages are mandatory (and some aren’t)

If your game has more than one mode of interaction, then it doesn’t make sense that your game would have just one way of sending messages over the network. The content of the messages—the type of gameplay that the messages support—can give us a way to reason about how to use the transport layer.

Any one message, depending on its contents, might have more or less impact on your player. If a single frame of voice chat audio is dropped, then it’s unlikely that your player will notice the loss. Some messages, if lost, may be highly aggravating: a lost store purchase confirmation may lead a player to an unwanted repeat purchase. Other messages may fall in-between these extremes: the movement of a projectile might be highly consequential or nearly irrelevant, depending on whether it’s traveling near the player.

![image alt text](image_0.png)

If some messages must* *be received while others are merely nice to have, then the game’s networking code must support a gradient of reliability.

## Reliability is a gradient

Transport layer protocols partly map to the gradient by setting distinct expectations for message delivery.

UDP offers few reliability guarantees. It’s a more trusting protocol that expects that a message will be delivered. If it’s not delivered, the protocol won’t hear or do anything about it. It also doesn’t make any assumptions about when the message is delivered. UDP makes no effort to ensure the order of delivery. The first message you sent may not be the first received. When it comes to reliability, UDP is relaxed.

TCP offers robust reliability guarantees. It goes to some effort to confirm that a connection between the sender and recipient is established, that the recipient is prepared to receive messages, and, after the messages are sent, that they are delivered completely and in order. When it comes to reliability, TCP is a strict micromanager.

![image alt text](image_1.png)

### A Tale of Four Messages

Under good network conditions, it’s actually quite hard to tell UDP and TCP apart in terms of reliability. But if conditions are poor, it’s easier to see how reliability tradeoffs emerge. To illustrate, suppose your game needs to send four messages sequentially (but let’s ignore their contents and importance, for now).

In good conditions, messages arrive intact, promptly, in-order, and just once. In poorer conditions, none of these assumptions hold. How do the different protocols cope?

With UDP, send the four messages, M1, M2, M3, and M4, each one after the other. What happens to each?

![image alt text](image_2.png)

* M1 gets lost along the way. It never arrives.

* M2 eventually arrives, but the routing is so convoluted, it’s last to arrive.

* M3 arrives first!

* M4 arrives, but a router along the way spontaneously duplicated the message in transit, so it arrives *twice*, once in second place and once in third.

As far as UDP is concerned, this is how it’s supposed to happen. It’s "best effort" but at least it’s low overhead. The protocol doesn’t require the receiver to notify the sender of any problems. If anything is to be done about this network hiccup—to make sense of out-of-order delivery, repeated delivery, or the lost message the receiver didn’t even know about—it has to be done at the application layer, in your game.

Let’s rewind things and play this scenario out again over TCP. TCP is a more complex protocol and we’ll have to tell a more complex story. But let’s send those messages again, assuming that we’ve already completed the three-way handshake and we’re in the ESTABLISHED state. What happens to each?

![image alt text](image_3.png)

* M1 gets lost along the way. Eventually resent (more on this later), it arrives last. 

* M2 eventually arrives, but the routing is so convoluted, it arrives after M3 and M4.

* M3 arrives first!

* M4 arrives, but a router along the way spontaneously duplicated the message in transit, so it arrives *twice*, in second *and *third place. TCP is clever though and knows to discard the duplicate M4.

TCP wants to protect you from this wrong and incomplete order, however. As M2, M3, and M4 arrive, TCP holds those messages until M1 arrives (this is known as *head of line blocking*). While M1 is lost, TCP waits for a while to acknowledge what it has received (delayed ACK) and then waits some more while M1 is resent. Eventually, all four messages are received and TCP hands off the data to your application.

As far as TCP is concerned, this is how it’s supposed to happen. It’s more reliable, but has noticeably more overhead in the form of round-trip exchanges. There are adjustments that can be made to minimize some of the more problematic features (such as turning off delayed ACK), but to get the guarantees of TCP, your application must necessarily submit to these constraints.

Unfortunately, the protocols themselves only represent two points on the reliability gradient. TCP doesn’t have a reduced guarantees mode and UDP doesn’t have an opt-in for more reliability. If our only choices involved choosing TCP or UDP, we’d face a hard decision between the risks of UDP’s best effort and the restrictions of TCP. Luckily, your game has a say in the reliability story, too.

## Optionally reliable

Your game, as a networked application, can choose how to use the network.

In general, your game is not fixed to a single protocol: you can use UDP when you don’t require the reliability guarantees of TCP and you can switch to TCP when you do. Under good network conditions (low latency and packet loss), this won’t matter much. But if things get shaky, then you’ll avoid some misadventures associated with lost messages.

For points on the gradient between optional and required, there’s a strategy called *optionally reliable UDP*. This is when you build additional guarantees in your application, rather than relying on the protocol. In outline, this is sending UDP packets that require some response from the recipient; the message is re-sent until the expected response is received.

But whether you choose UDP or TCP or optionally reliable UDP comes down to the *content* of your messages and what they mean to your game and players.

## Start with your payloads 

The content of your messages matters more than the vehicle they travel in. Start by focusing on your payloads, then figure out how to get them there.

Before you start sending messages, make sure you**_ _****know what you need to send over the wire** (and what you don’t). For example, in an action game, your player’s movements will probably be sent to the server many times per second, while your player’s inventory status only needs to be sent when it changes. Smaller data structures are less likely to pay fragmentation penalties. The fastest packet is the one you never send.

**Know your player’s context**. Your netcode should reflect when and how your player is interacting with the game. This starts with the player’s physical environment, such as playing on a desktop computer with a high-quality wired connection versus playing from a mobile device on a spotty mobile network. It also includes the game context; the game state may make certain kinds of messages more likely than others.

**Know what messages, if lost, would annoy the player** (or worse), what they’d never notice, and what’s in between. Each message’s place on the reliability gradient is key. If you know that prediction, interpolation, or other strategies can gracefully hide the occasional lost message, then that gives you more options for sending that message.

## Stand on shoulders of giants

If you’re thinking that this only begins to dig into thinking about net code, then you’re right. As you build out your games netcode, you’ll need to learn more about different aspects of prediction, interpolation, serialization, and beyond.

Read Ruoyu Sun’s *[Game Networking Demystifie*d](https://ruoyusun.com/2019/03/28/game-networking-1.html), which takes a closer look at game states and client inputs and how they relate to cheating, server authority, and game genre.

Another great read is *[Peeking into VALORANT's Netcod*e](https://technology.riotgames.com/news/peeking-valorants-netcode) from Riot Games, which goes in-depth on how product design goals can lead to specific decisions in netcode design.

And it always pays to read others’ code. Check out [Josh Bothun’s open-source implementation of a Quake-style FPS with contemporary netcode techniques](https://github.com/minism/fps-netcode) to see some actual code.

