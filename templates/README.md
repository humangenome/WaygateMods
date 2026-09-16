# Mod templates

Two starting points, one per side. Copy a folder, rename the project and the ids, and build against the interop assemblies of an unpacked WaygateServer package (set `WAYGATE_PACK` to its `BepInEx` folder; those assemblies are generated from the game and are never committed to a mod repository).

- `server-mod/`: runs on the server only. Players install nothing. Use it for rules the server computes: multipliers, chat notices, save schedules, world state.
- `client-mod/`: runs on the player's PC only, installed by the Waygate app when a server lists it. Use it for HUD, camera, keybinds and visuals. It must read replicated state and never write signed player state or change the network contract.

Both templates tick off `TransitionManager.Update`, the one Update that exists in every scene the game runs, and check which end they are on before doing anything.
