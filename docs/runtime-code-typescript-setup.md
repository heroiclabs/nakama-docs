# TypeScript Setup

The game server embeds a JavaScript Virtual Machine (VM) which can be used to load and run custom logic which is specific to your game project. This is in addition to Lua and Go as supported programming languages to write your server code.

It's useful to implement game code you would not want to run on the client or trust the client to provide unchecked inputs on. You can think of this feature of Nakama as similar to what is sometimes called Lambda or Cloud Functions in other systems. A good use case is if you wanted to grant the user a reward each day that they play the game.

TypeScript is a fantastic superset of the JavaScript language. It allows you to write your code with types which helps to reduce bugs and unexpected runtime behaviour of code. Nakama's support for JavaScript has been built to directly consider the use of TypeScript for your code and is the recommended way to develop your JavaScript code.

!!! Note
    You can learn more about how to write your JavaScript code in TypeScript in the <a href="https://www.typescriptlang.org/docs/handbook/typescript-in-5-minutes.html" target="_blank">official documentation</a>.

## Prerequisites

You will need to have these tools installed to work with TypeScript for your project:

* Node v14 (active LTS) or greater.
* Basic UNIX tools or knowledge on the Windows equivalents.

The TypeScript compiler and other dependencies will be fetched with NPM.

## Initialize the Project

These steps will set up a workspace to write all your project code to be run by the game server.

1. Define the folder name that will be the workspace for the project. In this case we'll use "ts-project".

    ``` shell
    mkdir -p ts-project/{src,build}
    cd ts-project
    ```

2. Use NPM to set up the Node dependencies in the project. Install the TypeScript compiler.

    ``` shell
    npm init -y
    npm install --save-dev typescript
    ```

3. Use the TypeScript compiler installed to the project to set up the compiler options.

    ``` shell
    npx tsc --init
    ```

    You'll now have a "tsconfig.json" file which describes the available options that are run on the TypeScript compiler. When you've trimmed the commented out entries and updated it a minimal file will look something like:

    ``` json
    {
      "compilerOptions": {
        "target": "es5",
        "strict": true,
        "esModuleInterop": true,
        "skipLibCheck": true,
        "forceConsistentCasingInFileNames": true
      }
    }
    ```

    Add this configuration option to the `"compilerOptions"` block:

    ``` json
    "outFile": "./build/index.js",
    ```

4. Add the Nakama runtime types as a dependency to the project and configure the compiler to find the types.

    ``` shell
    npm i 'https://github.com/heroiclabs/nakama-common'
    ```

    Add this configuration option to the `"compilerOptions"` block of the "tsconfig.json" file:

    ``` json
    "typeRoots": [
      "./node_modules"
    ],
    ```

    This completes the setup and your project should look similar to this layout:

    ``` shell
    .
    ├── build
    ├── node_modules
    │   ├── nakama-runtime
    │   └── typescript
    ├── package-lock.json
    ├── package.json
    ├── src
    └── tsconfig.json
    ```

## Develop Code

We'll write some simple code and compile it to JavaScript so it can be run by the game server.

All code must start execution from a function that the game server looks for in the global scope at startup. This function must be called `"InitModule"` and is how you register RPCs, before/after hooks, and other event functions managed by the server.

The code below is a simple Hello World example which uses the `"Logger"` to write a message. Name the source file "main.ts" inside the "src" folder. You can write it in your favourite editor or IDE.

``` typescript
let InitModule: nkruntime.InitModule =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
    logger.info("Hello World!");
}
```

We can now add the file to the compiler options and run the TypeScript compiler.

``` json
{
  "files": [
    "./src/main.ts"
  ],
  "compilerOptions": {
    // ... etc
  }
}
```

To compile the codebase:

``` shell
npx tsc
```

## Run the Project

You can use Docker with a [compose file](install-docker-quickstart.md) for local development or [install the binary](install-binary.md) for the game server. You will also need to install the database if you run without Docker. Have a look at the installation section which cover these steps. When this is complete you can run the game server and have it load your code:

``` shell
nakama --logger.level DEBUG --runtime.js_entrypoint "build/index.js"
```

The server logs will show this output or similar which shows that the code we wrote above was loaded and executed at startup.

``` json
{"level":"info","ts":"...","msg":"Hello World!","caller":"server/runtime_javascript_logger.go:54"}
```

## Next Steps

Have a look at the [Nakama project template](https://github.com/heroiclabs/nakama-project-template) which shows a larger TypeScript example which includes how to write an [authoritative multiplayer match handler](gameplay-multiplayer-server-multiplayer.md) for the TicTacToe game. It shows off other concepts as well which includes [In-App Notifications](social-in-app-notifications.md), [Storage](storage-collections.md), [RPCs](runtime-code-basics.md#rpc-hook), and [User Wallets](user-accounts.md#virtual-wallet).

[https://github.com/heroiclabs/nakama-project-template](https://github.com/heroiclabs/nakama-project-template)
