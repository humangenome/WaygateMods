# Waygate mod registry

Dimraeth has no mod loader, no Steam Workshop and no mod site. [WaygateServer](https://github.com/HumanGenome/WaygateServer) runs the game as a dedicated server on BepInEx, which gives it one. This repository is the list of mods for those servers, and the only place the [Waygate](https://github.com/HumanGenome/Waygate) app installs mods from.

- `registry.json` is the list. One entry per mod version: id, name, version, author, side (server, client or both), the game build it was tested on, the release zip's url and sha256, the plugin dll's sha256, license and source.
- `index.json` is `registry.json` signed with the same key that signs Waygate app updates. The app downloads `index.json`, checks the signature, and only then reads the list. A change to this repository that is not signed changes nothing on any player's PC.
- `templates/` holds a server mod and a client mod project to start from.
- `tools/validate.py` checks a registry file; `tools/sign-index.sh` signs it.

## How it reaches players

A server lists the client mods it runs. Before the game starts, the Waygate app asks the server for that list, looks each one up here, downloads the release named here, checks both hashes, installs it into a folder that belongs to that server only, and asks the player once. The server itself never sends mod files. A mod withdrawn here (`"revoked": true`) is removed from every player's PC on the app's next start.

Server-only mods are installed by whoever runs the server: the folder `BepInEx\plugins\mods\<id>\` under the game folder holds the zip's contents. Hosting panels that list this registry install them for you.

## Listing a mod

See [CONTRIBUTING.md](CONTRIBUTING.md).

## First entries

| Mod | Side | Author |
|---|---|---|
| Server Multipliers | server | HumanGenome |
| Message of the Day | server | HumanGenome |
| Client HUD | client | HumanGenome |

Source for all three: [WaygateMods-first](https://github.com/HumanGenome/WaygateMods-first).

## License

The tooling in this repository is MIT. Each listed mod carries its own license, named in its entry.
