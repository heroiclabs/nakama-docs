# Lua Setup

The game server allows you to load and run custom logic written in Lua as well as [TypeScript](runtime-code-typescript-setup.md) and [Go](runtime-code-lua-setup.md).

It's useful to implement game code you would not want to run on the client or trust the client to provide unchecked inputs on. You can think of this feature of Nakama as similar to what is sometimes called Lambda or Cloud Functions in other systems. A good use case is if you wanted to grant the user a [reward each day that they play the game](runtime-code-daily-rewards.md).

Unlike when writing your server logic in Go or TypeScript, there is no toolchain or other setup needed when writing your code in Lua. This is because Lua is a powerful embeddable scripting language and does not need to be compiled or transpiled. This makes it a good choice if you want to get up and running quickly and easily.

!!! Note
    You can learn more about how to write your Lua code in the <a href="https://www.lua.org/docs.html" target="_blank">official documentation</a>.

## Develop Code

You can find the full Lua [Nakama module function reference here](runtime-code-function-reference/#nakama-module).

Before you begin, create a new folder for your project and open it in an editor of your choice (e.g. VS Code).

Start by creating a new folder called `modules` and inside create a new file called `main.lua`. The code below is a simple Hello World example which uses the `"Logger"` to write a message.

```lua
local nk = require("nakama")
nk.logger_info("Hello World!")
```

## Run the Project

You can use Docker with a [compose file](install-docker-quickstart.md) for local development or setup a binary environment for:

* [Linux](install-binary-linux-quickstart.md)
* [Windows](install-binary-windows-quickstart.md)
* [macOS](install-binary-macos-quickstart.md)

When this is complete you can run the game server and have it load your code:

``` shell
nakama --logger.level DEBUG
```

The server logs will show this output or similar which shows that the code we wrote above was loaded and executed at startup.

``` json
{"level":"info","ts":"...","caller":"server/runtime_lua_nakama.go:1742","msg":"Hello World!","runtime":"lua"}
```

## Next Steps

Have a look at the [Nakama project template](https://github.com/heroiclabs/nakama-project-template) which covers the following Nakama features:

* [Authoritative multiplayer match handler](gameplay-multiplayer-server-multiplayer.md) for the TicTacToe game.
* [In-App Notifications](social-in-app-notifications.md),
* [Storage](storage-collections.md)
* [RPCs](runtime-code-basics.md#rpc-hook)
* [User Wallets](user-accounts.md#virtual-wallet).
