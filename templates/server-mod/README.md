# Server Mod

What it does, in one paragraph. Server side: players install nothing.

| Key | What it does | Default |
|---|---|---|
| `Enabled` | Turn the mod on or off | true |

## Install

Copy the folder from the release zip into `BepInEx\plugins\mods\YourName-ServerMod\` under the server's game folder. Start the server once to get the config file, edit it, restart.

## Build

Set `WAYGATE_PACK` to the `BepInEx` folder of an unpacked WaygateServer package, then `dotnet build -c Release`. Fill `sha256` in `waygate-mod.json` with the dll's sha256 and zip the four files (manifest, dll, README, LICENSE) at the zip root as `YourName-ServerMod-1.0.0.zip`.

Tested on game build 25335390.
