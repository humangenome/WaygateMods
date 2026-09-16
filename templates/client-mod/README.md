# Client Mod

What it does, in one paragraph. Client side: the Waygate app installs it on a player's PC when a server lists it, for that server only. Players do nothing by hand.

| Key | What it does | Default |
|---|---|---|
| `Enabled` | Turn the mod on or off | true |

## Rules for a client mod

- Read replicated state; never write XP, level, attributes or gold (the game signs them and kicks on a mismatch).
- Never register network prefabs, change RPC signatures or touch `NetworkConfig` (the server refuses the join).
- Never write the player's own saves or PlayerPrefs.

## Build

Set `WAYGATE_PACK` to the `BepInEx` folder of an unpacked WaygateServer package, then `dotnet build -c Release`. Fill `sha256` in `waygate-mod.json` with the dll's sha256 and zip the four files (manifest, dll, README, LICENSE) at the zip root as `YourName-ClientMod-1.0.0.zip`.

Tested on game build 25335390.
