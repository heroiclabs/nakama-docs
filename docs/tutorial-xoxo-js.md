# Tic-Tac-Toe PhaserJS Tutorial

<iframe width="560" height="315" src="https://www.youtube.com/embed/videoseries?list=PLOAExZcDNj9v8Ne6pXtOdhCycZIpubsZV" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

In this tutorial you will be creating XOXO, an online tic-tac-toe game, using [PhaserJS](https://phaser.io/) and your Nakama server.

This game and tutorial highlight the powerful [authentication](authentication.md), [matchmaking](gameplay-matchmaker.md), and [realtime multiplayer](gameplay-multiplayer-realtime.md) features of Nakama.

!!! note "Note"
    Check out the accompanying [video series](https://www.youtube.com/playlist?list=PLOAExZcDNj9v8Ne6pXtOdhCycZIpubsZV) to this tutorial for additional content, and access the [source code](https://github.com/heroiclabs/xoxo-phaserjs) for reference.

## Prerequisites

To easily follow along with this tutorial, perform the following before proceeding:

* [Install Node LTS](https://nodejs.org/en/download/)
* [Install TypeScript](https://www.typescriptlang.org/download)
* [Install Phaser](https://phaser.io/download)
* [Install Nakama](install-docker-quickstart.md)
* [Install the Nakama JavaScript Client](javascript-client-guide.md)
* [Install Svelte](https://svelte.dev/blog/the-easiest-way-to-get-started)
* [Clone the Nakama Project Template](https://github.com/heroiclabs/nakama-project-template)
* [Prepare PhaserJS Game Engine](#configuring-your-javascript-framework)
    
!!! note "Note"
    For this tutorial you will be using JavaScript/TypeScript exclusively, so you can safely delete all Go and Lua files from the cloned template project.

### Configuring your JavaScript framework

Here you will install the TypeScript dependencies required for this project, transpile your TypeScript code to JavaScript, and add PhaserJS to your Svelte JavaScript framework.

1. Install NPM to manage your dependencies. From your terminal window:

    ```sh
    npm install
    ```

2. Before starting your Nakama server, transpile the TypeScript code to JS:

    ```sh
    npx tsc
    ```

3. Add the PhaserJS script tag to your `index.html` file:

    === "index.html"
            ```html
            <!DOCTYPE html>
            <html lang="en">
            <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width,initial-scale=1'>
                    <title>Svelte app</title>
                    <link rel='icon' type='image/png' href='/favicon.png'>
                    <link rel='stylesheet' href='/global.css'>
                    <link rel='stylesheet' href='/build/bundle.css'>
                    // PhaserJS script tag
                    <script src="//cdn.jsdelivr.net/npm/phaser@3.54.0/dist/phaser.min.js"></script>
                    <script defer src='/build/bundle.js'></script>
            </head>
            <body>
            </body>
            </html>
            ```

4. You can now run your application locally by running:

    ```sh
    npm run dev
    ```

Your application will be available at `localhost:5000`.

## Building the UI

1. Start by adding the PhaserJS config to your `App.svelte` file (your game's main entry point, imported in your `main.js` file):

    === "App.svelte"
        ```js
        <script>
                import MainMenu from "./scenes/MainMenu";
                import Matchmaking from "./scenes/Matchmaking";
                import InGame from "./scenes/InGame";
                import CONFIG from "./config";
                const config = {
                        type: Phaser.AUTO,
                        width: CONFIG.WIDTH,
                        height: CONFIG.HEIGHT,
                        backgroundColor: "#FF4C4C",
                        scene: [MainMenu, Matchmaking, InGame],
                };
                new Phaser.Game(config);
        </script>
        ```
    
    === "main.js"
        ```js
        import App from './App.svelte';

        const app = new App({
            target: document.body,
            props: {}
        });

        export default app;
        ```

2. Next, create a `config.js` file for you game settings. In this case, the height and width of the game canvas:
    === "config.js"
        ```js
        const CONFIG = {
            WIDTH: 414,
            HEIGHT: 736
        }
        export default CONFIG
        ```

3. Now you can start creating the game's Phaser scenes to group related logic. For our game we have three, the main menu, matchmaking, and in game scenes:
    === "MainMenu.js"
        ```js

        // ...

            create() {
                Nakama.authenticate()

                // Create the Welcome banner
                this.add
                    .text(CONFIG.WIDTH / 2, 75, "Welcome to", {
                        fontFamily: "Arial",
                        fontSize: "24px",
                    })
                    .setOrigin(0.5);

                this.add
                    .text(CONFIG.WIDTH / 2, 123, "XOXO", {
                        fontFamily: "Arial",
                        fontSize: "72px",
                    })
                    .setOrigin(0.5);

                this.add.grid(
                    CONFIG.WIDTH / 2,
                    CONFIG.HEIGHT / 2,
                    300,
                    300,
                    100,
                    100,
                    0xffffff,
                    0,
                    0xffca27
                );

                // Create a button to start the game
                const playBtn = this.add
                    .rectangle(CONFIG.WIDTH / 2, 625, 225, 70, 0xffca27)
                    .setInteractive({ useHandCursor: true });

                const playBtnText = this.add
                    .text(CONFIG.WIDTH / 2, 625, "Begin", {
                        fontFamily: "Arial",
                        fontSize: "36px",
                    })
                    .setOrigin(0.5);

                playBtn.on("pointerdown", () => {
                    Nakama.findMatch()
                    this.scene.start("in-game");
                });

                // ...
            }
        }
        ```

    === "Matchmaking.js"
        ```js

        // ...

            create() {
                this.add
                    .text(CONFIG.WIDTH / 2, 125, "Searching for an opponent...", {
                        fontFamily: "Arial",
                        fontSize: "24px",
                    })
                    .setOrigin(0.5);

                this.anims.create({
                    key: "spinnerAnimation",
                    frames: this.anims.generateFrameNumbers("spinner"),
                    frameRate: 30,
                    repeat: Phaser.FOREVER,
                });

                this.add
                    .sprite(CONFIG.WIDTH / 2, CONFIG.HEIGHT / 2, "spinner")
                    .play("spinnerAnimation")
                    .setScale(0.5);
            }
        }
        ```

    === "InGame.js"
        ```js

        // ...

            updateBoard(board) {
                board.forEach((element, index) => {
                    let newImage = this.INDEX_TO_POS[index]

                    if (element === 1) {
                        this.phaser.add.image(newImage.x, newImage.y, "O");
                    } else if (element === 2) {
                        this.phaser.add.image(newImage.x, newImage.y, "X");
                    }
                })
            }

            updatePlayerTurn() {
                this.playerTurn = !this.playerTurn

                if (this.playerTurn) {
                    this.headerText.setText("Your turn!")
                } else {
                    this.headerText.setText("Opponents turn!")
                }
            }

            setPlayerTurn(data) {
                let userId = localStorage.getItem("user_id");
                if (data.marks[userId] === 1) {
                    this.playerTurn = true;
                    this.playerPos = 1;
                    this.headerText.setText("Your turn!")
                } else {
                    this.headerText.setText("Opponents turn!")
                }
            }

            endGame(data) {
                this.updateBoard(data.board)

                if (data.winner === this.playerPos) {
                    this.headerText.setText("Winner!")
                } else {
                    this.headerText.setText("You loose :(")
                }
            }

            nakamaListener() {
                Nakama.socket.onmatchdata = (result) => {
                    switch (result.op_code) {
                        case 1:
                            this.gameStarted = true;
                            this.setPlayerTurn(result.data)
                            break;
                        case 2:
                            console.log(result.data)
                            this.updateBoard(result.data.board)
                            this.updatePlayerTurn()
                            break;
                        case 3:
                            this.endGame(result.data)
                            break;
                    }
                };
            }

            // ...

                // Register the player move in the correct square
                this.nakamaListener()

                this.add
                    .rectangle(
                        gridCenterX - gridCellWidth,
                        topY,
                        gridCellWidth,
                        gridCellWidth
                    )
                    .setInteractive({ useHandCursor: true })
                    .on("pointerdown", async () => {
                        await Nakama.makeMove(0)
                    });

                this.add
                    .rectangle(gridCenterX, topY, gridCellWidth, gridCellWidth)
                    .setInteractive({ useHandCursor: true })
                    .on("pointerdown", () => {
                        Nakama.makeMove(1)
                    });

                // ...
            }
        }
        ```

In the Main Menu scene, you authenticate the user in Nakama (discussed below) and display a "Welcome to XOXO" banner and button that, on click, takes the user to the In Game scene.

The Matchmaking scene simply displays a waiting spinner while Nakama finds an opponent for the user.

For the In Game scene, you are creating the interactive board of nine individual squares for players to enter their X's and O's. You also define the gameplay functions to set and update players turns, and end the game. The `nakamaListener` defined here communicates these actions via websocket to the Nakama server.

## Connecting to Nakama

1. Next connect your client to Nakama server and configure it for [device authentication](authentication.md#device):
    === "nakama.js"
        ```js
        import { Client } from "@heroiclabs/nakama-js";
        import { v4 as uuidv4 } from "uuid";

        class Nakama {
            constructor() {
                this.client
                this.session
                this.socket 
                this.matchID 
            }

            async authenticate() {
                this.client = new Client("defaultkey", "localhost", "7350");
                this.client.ssl = false;

                let deviceId = localStorage.getItem("deviceId");
                if (!deviceId) {
                    deviceId = uuidv4();
                    localStorage.setItem("deviceId", deviceId);
                }

                this.session = await this.client.authenticateDevice(deviceId, true);
                localStorage.setItem("user_id", this.session.user_id);

                const trace = false;
                this.socket = this.client.createSocket(this.useSSL, trace);
                await this.socket.connect(this.session);

            }

            // ...
        ```

2. Here you'll also configure the [multiplayer](gameplay-realtime-multiplayer.md) functionality. Learn more about this in the [authoritative multiplayer](gameplay-multiplayer-server-multiplayer.md) and [matchmaker](gameplay-matchmaker.md) documentation.
    === "nakama.js"
        ```js
        import { Client } from "@heroiclabs/nakama-js";
        import { v4 as uuidv4 } from "uuid";

        class Nakama {
            constructor() {
                this.client
                this.session
                this.socket 
                this.matchID 
            }

            // ...

            async findMatch() { 
                const rpcid = "find_match";
                const matches = await this.client.rpc(this.session, rpcid, {});

                this.matchID = matches.payload.matchIds[0]
                await this.socket.joinMatch(this.matchID);
                console.log("Matched joined!")
            }

            async makeMove(index) { 
                var data = { "position": index };
                await this.socket.sendMatchState(this.matchID, 4, data);
                console.log("Match data sent")
            }
        }

        export default new Nakama()
        ```

And that's it, you're ready to play!

## Further reading

Learn more about the topics and features in this tutorial with the following:

* [JavaScript Client Guide](javascript-client-guide.md)
* [Authentication](authentication.md)
* [Authoritative Multiplayer](gameplay-multiplayer-server-multiplayer.md)
* [Matchmaker](gameplay-matchmaker.md)
