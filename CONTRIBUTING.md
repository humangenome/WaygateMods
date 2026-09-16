# Contributing a mod to the Waygate registry

The registry is one file, `registry.json`. Every entry is a mod the Waygate app may install on a player's PC, or a hosting panel may install on a server. Listing is by pull request and every entry is checked by a person before it is merged.

## What a listed mod is

- A BepInEx 6 IL2CPP plugin for Dimraeth, built against the interop assemblies that ship in the WaygateServer package. Start from [templates/server-mod](templates/server-mod) or [templates/client-mod](templates/client-mod).
- Published as a GitHub release on the author's own repository. The release carries one zip named `<id>-<version>.zip` with, at its root: `waygate-mod.json`, the plugin dll, `README.md` and `LICENSE`. Nothing else, no folders.
- Open source under an OSI license, with the source repository named in the entry.
- Tested on the game build named in the entry.

## The manifest, `waygate-mod.json`

```json
{
  "id": "YourGitHubName-ModName",
  "name": "Mod Name",
  "version": "1.0.0",
  "author": "YourGitHubName",
  "side": "server",
  "dll": "ModName.dll",
  "sha256": "<sha256 of ModName.dll, lower-case hex>",
  "game_build": 25335390,
  "description": "One sentence: what it does and who installs it.",
  "dependencies": []
}
```

- `id` is `Author-Name`: your GitHub user or organisation name, a dash, the mod name. Letters, digits and underscores only, exactly one dash. It never changes between versions.
- `side` is `server` (runs on the server only, players install nothing), `client` (runs on the player's PC only) or `both` (runs on both and must match). The Waygate app installs `client` and `both` mods on a player's PC when a server lists them; a `server` mod is never sent to players.
- `sha256` is the hash of the dll inside the same zip. The app checks it, and so does the server.

## The entry, in `registry.json`

```json
{
  "id": "YourGitHubName-ModName",
  "name": "Mod Name",
  "version": "1.0.0",
  "author": "YourGitHubName",
  "side": "server",
  "game_build": 25335390,
  "dll": "ModName.dll",
  "dll_sha256": "<same as the manifest's sha256>",
  "sha256": "<sha256 of the release zip>",
  "size": 12345,
  "url": "https://github.com/YourGitHubName/ModName/releases/download/v1.0.0/YourGitHubName-ModName-1.0.0.zip",
  "source": "https://github.com/YourGitHubName/ModName",
  "license": "MIT",
  "description": "One sentence: what it does and who installs it.",
  "dependencies": [],
  "revoked": false
}
```

`tools/validate.py registry.json` checks the shape and downloads every url to confirm the hashes and sizes. Run it before opening the pull request; the same check runs on the pull request.

## Rules

1. The url is a GitHub release asset on a repository you control. Not a branch, not a website, not a file host.
2. The hashes match the bytes at the url. A changed release is a new version with a new entry, never a re-upload under the same name.
3. Nothing in the zip but the four files above. A second dll, a preloader patcher, game assets or any file from the game's own folders is refused.
4. A server mod must not raise the player cap, spoof the player count, or change what the server reports about itself.
5. No networking beyond the game's own connections, no process spawning, no writes outside BepInEx's own folders, no reads of another game instance's saves, no obfuscated builds. Native code needs a stated reason.
6. A `client` or `both` mod must not touch signed player state (XP, level, attributes, gold) and must not change the network contract (prefabs, RPC signatures, `NetworkConfig`); the game kicks for the first and refuses the join for the second.
7. The README says what the mod does, how to configure it and which game build it was tested on. No install steps for players: the Waygate app is the installer.

## Review

A maintainer reads the source, decompiles the release dll and compares it, checks the strings, confirms the hashes and runs it on a test server. Merge lists the mod; `tools/sign-index.sh` then signs `registry.json` into `index.json`, which is what the app reads. An entry the app has not seen in a signed index does not exist to it.

## Withdrawing a mod

Set `"revoked": true` and `"revoked_reason"` on the entry (never delete the entry). Every Waygate app removes that mod from every server profile on its next start, and a server still listing it is refused with the reason. A takedown request is an issue on this repository; the entry is revoked within one working day and the author can reply in the same issue.
