# TypeScript Setup

The game server embeds a JavaScript Virtual Machine (VM) which can be used to load and run custom logic which is specific to your game project. This is in addition to Lua and Go as supported programming languages to write your server code.

It's useful to implement game code you would not want to run on the client or trust the client to provide unchecked inputs on. You can think of this feature of Nakama as similar to what is sometimes called Lambda or Cloud Functions in other systems. A good use case is if you wanted to grant the user a reward each day that they play the game.

TypeScript is a fantastic superset of the JavaScript language. It allows you to write your code with types which helps to reduce bugs and unexpected runtime behavior of code. Nakama's support for JavaScript has been built to directly consider the use of TypeScript for your code and is the recommended way to develop your JavaScript code.

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

FROM heroiclabs/nakama:3.4.0

COPY --from=node-builder /backend/build/*.js /nakama/data/modules/build/
COPY local.yml .
```

Next create a `docker-compose.yml` file. For more information see the [Install Nakama with Docker Compose](../getting-started/docker-quickstart.md) documentation.

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

Install a Nakama binary stack for [Linux](../getting-started/binary-linux-quickstart.md), [Windows](../getting-started/binary-windows-quickstart.md), or [macOS](../getting-started/binary-macos-quickstart.md). When this is complete you can run the game server and have it load your code:

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

### Configuring Rollup

When configuring your TypeScript project to use Rollup there are a few additional steps and alterations you will need to make to your project if you have followed the steps above.

The first thing you will need to do is install some additional dependencies that will allow you to run Rollup to build your server runtime code. These include [Babel](https://babeljs.io/), [Rollup](https://rollupjs.org/), several of their respective plugins/presets and `tslib`.

To do this, run the following command in the Terminal, which will install the dependencies and add them to your `package.json` file as development dependencies:

```
npm i -D @babel/core @babel/plugin-external-helpers @babel/preset-env @rollup/plugin-babel @rollup/plugin-commonjs @rollup/plugin-json @rollup/plugin-node-resolve @rollup/plugin-typescript rollup tslib
```

With Rollup installed as a dev dependency of your project, you now need to modify the `build` script in `package.json` to run the `rollup -c` command instead of the `tsc` command. You should also add a `type-check` script that will allow you to verify your TypeScript compiles without actually emitting a build file.

=== "package.json"
  ```json
  {
    ...
    "scripts": {
      "build": "rollup -c",
      "type-check": "tsc --noEmit"
    },
    ...
  }
  ```

Next, you must add the following `rollup.config.js` file to your project.

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

Followed by adding a `babel.config.json` file to your project.

=== "babel.config.json"
  ```json
  {
    "presets": [
      "@babel/env"
    ],
    "plugins": []
  }
  ```

There are also changes to the `tsconfig.json` file that must be made. Using Rollup simplifies the build process and means you no longer have to manually update the `tsconfig.json` file every time you add a new `*.ts` file to your project. Replace the contents of your existing `tsconfig.json` file with the example below.

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

Next, you need to include a line at the bottom of your `main.ts` file that references the `InitModule` function. This is to ensure that Rollup does not omit it from the build.

=== "main.ts"
  ```ts
  function InitModule(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
    logger.info('TypeScript module loaded.');
  }

  // Reference InitModule to avoid it getting removed on build
  !InitModule && InitModule.bind(null);
  ```

Finally, you need to make a slight alteration to your `Dockerfile` to ensure you copy across the `rollup.config.js` and `babel.config.json` files. You must also change the `RUN` command to run your updated build command rather than using the TypeScript compiler directly. Replace the contents of your `Dockerfile` with the example below.

=== "Dockerfile"
  ```docker
  FROM node:alpine AS node-builder

  WORKDIR /backend

  COPY package*.json .
  RUN npm install

  COPY . .
  RUN npm run build

  FROM heroiclabs/nakama:3.4.0

  COPY --from=node-builder /backend/build/*.js /nakama/data/modules/build/
  COPY local.yml .
  ```

### Building your module locally

1. Ensure you have all dependencies installed:
    ```sh
    npm i
    ```

2. Perform a type check to ensure your TypeScript will compile successfully:
    ```sh
    npm run type-check
    ```

3. Build your project:
    ```sh
    npm run build
    ```

### Running your module with Docker

To run Nakama with your custom server runtime code, run:

```
docker compose up
```

If you have made changes to your module and want to re-run it, you can run:

```
docker compose up --build nakama
```

This will ensure the image is rebuilt with your latest changes.

## Next steps

Have a look at the [Nakama project template](https://github.com/heroiclabs/nakama-project-template) which shows a larger TypeScript example which includes how to write an [authoritative multiplayer match handler](../concepts/server-relayed-multiplayer.md) for the Tic-Tac-Toe game.
It shows off other concepts as well which includes [In-App Notifications](../concepts/in-app-notifications.md), [Storage](../concepts/collections.md), [RPCs](basics.md#rpc-hook), and [User Wallets](../concepts/user-accounts.md#virtual-wallet).
