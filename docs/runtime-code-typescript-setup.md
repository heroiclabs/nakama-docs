# TypeScript Setup

The game server embeds a JavaScript Virtual Machine (VM) which can be used to load and run custom logic which is specific to your game project. This is in addition to Lua and Go as supported programming languages to write your server code.

It's useful to implement game code you would not want to run on the client or trust the client to provide unchecked inputs on. You can think of this feature of Nakama as similar to what is sometimes called Lambda or Cloud Functions in other systems. A good use case is if you wanted to grant the user a reward each day that they play the game.

TypeScript is a fantastic superset of the JavaScript language. It allows you to write your code with types which helps to reduce bugs and unexpected runtime behaviour of code. Nakama's support for JavaScript has been built to directly consider the use of TypeScript for your code and is the recommended way to develop your JavaScript code.

!!! Note
    You can learn more about how to write your JavaScript code in TypeScript in the <a href="https://www.typescriptlang.org/docs/handbook/typescript-in-5-minutes.html" target="_blank">official documentation</a>.

<iframe width="560" height="315" src="https://www.youtube.com/embed/FXguREV6Zf8" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Prerequisites

You will need to have these tools installed to work with TypeScript for your project:

* Node v14 (active LTS) or greater.
* Basic UNIX tools or knowledge on the Windows equivalents.

The TypeScript compiler and other dependencies will be fetched with NPM.

## Initialize the project

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

!!! note "Note"
    See [TypeScript Bundling with Rollup](#bundling-with-rollup) for an example not relying on the TypeScript complier, enabling to bundle other node modules with your TypeScript code for Nakama.

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

## Develop code

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

## Running the project

### With Docker

The easiest way to run your server locally is with Docker.

To do this, create a file called `Dockerfile`.

```dockerfile
FROM node:alpine AS node-builder

WORKDIR /backend

COPY package*.json .
RUN npm install

COPY tsconfig.json .
COPY main.ts .
RUN npx tsc

FROM heroiclabs/nakama:3.3.0

COPY --from=node-builder /backend/build/*.js /nakama/data/modules/build/
COPY local.yml .
```

Next create a `docker-compose.yml` file. For more information see the [Install Nakama with Docker Compose](/install-docker-quickstart/) documentation.

```yml
version: '3'
services:
  postgres:
    command: postgres -c shared_preload_libraries=pg_stat_statements -c pg_stat_statements.track=all
    environment:
      - POSTGRES_DB=nakama
      - POSTGRES_PASSWORD=localdb
    expose:
      - "8080"
      - "5432"
    image: postgres:12.2-alpine
    ports:
      - "5432:5432"
      - "8080:8080"
    volumes:
      - data:/var/lib/postgresql/data

  nakama:
    build: .
    depends_on:
      - postgres
    entrypoint:
      - "/bin/sh"
      - "-ecx"
      - >
        /nakama/nakama migrate up --database.address postgres:localdb@postgres:5432/nakama &&
        exec /nakama/nakama --config /nakama/data/local.yml --database.address postgres:localdb@postgres:5432/nakama
    expose:
      - "7349"
      - "7350"
      - "7351"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:7350/"]
      interval: 10s
      timeout: 5s
      retries: 5
    links:
      - "postgres:db"
    ports:
      - "7349:7349"
      - "7350:7350"
      - "7351:7351"
    restart: unless-stopped

volumes:
  data:
```

Now run the server with the command:

```
docker compose up
```

### Without Docker

Install a Nakama binary stack for [Linux](install-binary-linux-quickstart.md), [Windows](install-binary-windows-quickstart.md), or [macOS](install-binary-macos-quickstart.md). When this is complete you can run the game server and have it load your code:

``` shell
nakama --logger.level DEBUG --runtime.js_entrypoint "build/index.js"
```

!!! note "Note"
    You'll need to have built the `build/index.js` file by running `npx tsc` from the Terminal before you can execute the above command.

### Confirming the server is running

The server logs will show this output or similar which shows that the code we wrote above was loaded and executed at startup.

``` json
{"level":"info","ts":"...","msg":"Hello World!","caller":"server/runtime_javascript_logger.go:54"}
```

## Bundling with Rollup

The setup above relies solely on the TypeScript compiler. This helps to keep the toolchain and workflow simple, but limits your ability to bundle your TypeScript code with additional node modules.

[Rollup](https://rollupjs.org/guide/en/) is one of the options available to bundle node modules that don't depend on the Node.js runtime to run within Nakama.

### Prerequisites

* Node v14+ LTS
* Rollup v1.20.0+
* TypeScript v3.7+
* [Rollup TypeScript plugin](https://www.npmjs.com/package/@rollup/plugin-typescript)

### Example files

Below are template files for a Nakama, TypeScript, and Rollup project, to be customized based on your specific needs:

=== "main.ts"
    ```ts
    import { upperCase } from 'upper-case';

    function InitModule(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
        initializer.registerRpc('test_rpc', handleTestRpc);
        logger.info('TypeScript module loaded.');
    }

    function handleTestRpc(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama) {
        const userId = upperCase(ctx.userId);
        logger.info('test_rpc userid:', userId);
        return userId;
    }

    // Reference InitModule to avoid it getting removed on build
    !InitModule && InitModule.bind(null);
    ```

=== "package.json"
    ```json
    {
        "name": "nakama-rollup-ts",
        "version": "0.0.1",
        "description": "Nakama + Typescript + Rollup Template.",
        "main": "build/index.js",
        "license": "Apache-2.0",
        "scripts": {
            "type-check": "tsc --noEmit",
            "build": "rollup -c"
        },
        "dependencies": {
            "nakama-runtime": "git+https://github.com/heroiclabs/nakama-common.git",
            "upper-case": "^2.0.2"
        },
        "devDependencies": {
            "@babel/core": "^7.13.8",
            "@babel/plugin-external-helpers": "^7.12.13",
            "@babel/preset-env": "^7.13.9",
            "@rollup/plugin-babel": "^5.3.0",
            "@rollup/plugin-commonjs": "17.1.0",
            "@rollup/plugin-json": "^4.1.0",
            "@rollup/plugin-node-resolve": "11.1.1",
            "@rollup/plugin-typescript": "^8.2.0",
            "rollup": "^2.40.0",
            "tslib": "^2.1.0",
            "typescript": "^4.2.3"
        },
        "keywords": []
    }
    ```

=== "tsconfig.json"
    ```json
    {
        "compilerOptions": {
            "noImplicitReturns": true,
            "moduleResolution": "node",
            "esModuleInterop": true,
            "noUnusedLocals": true,
            "removeComments": true,
            "target": "es6",
            "module": "ESNext",
            "strict": false,
        },
        "files": [
            "./node_modules/nakama-runtime/index.d.ts",
        ],
        "include": [
            "src/**/*",
        ],
        "exclude": [
            "node_modules",
            "build"
        ]
    }
    ```

=== "rollup.config.js"
    ```js
    import resolve from '@rollup/plugin-node-resolve';
    import commonJS from '@rollup/plugin-commonjs';
    import json from '@rollup/plugin-json';
    import babel from '@rollup/plugin-babel';
    import typescript from '@rollup/plugin-typescript';
    import pkg from './package.json';

    const extensions = ['.mjs', '.js', '.ts', '.json'];

    export default {
        input: './src/main.ts',
        external: ['nakama-runtime'],
        plugins: [
            // Allows node_modules resolution
            resolve({ extensions }),

            // Compile TypeScript
            typescript(),

            json(),

            // Resolve CommonJS modules
            commonJS({ extensions }),

            // Transpile to ES5
            babel({
                extensions,
                babelHelpers: 'bundled',
            }),
        ],
        output: {
            file: pkg.main,
        },
    };
    ```

=== "babel.config.json"
    ```json
    {
        "presets": [
            "@babel/env"
        ],
        "plugins": []
    }
    ```

### Usage

1. Ensure you have all dependencies installed:
    ```sh
    npm i
    ```

2. Next perform a Type check:
    ```sh
    npm run type-check
    ```

3. Then, under `build/index.js`, build your project:
    ```sh
    npm run build
    ```

## Next steps

Have a look at the [Nakama project template](https://github.com/heroiclabs/nakama-project-template) which shows a larger TypeScript example which includes how to write an [authoritative multiplayer match handler](gameplay-multiplayer-server-multiplayer.md) for the Tic-Tac-Toe game.
It shows off other concepts as well which includes [In-App Notifications](social-in-app-notifications.md), [Storage](storage-collections.md), [RPCs](runtime-code-basics.md#rpc-hook), and [User Wallets](user-accounts.md#virtual-wallet).
