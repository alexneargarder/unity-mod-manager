# Unity Mod Manager (Broforce fork)

Fork of [Unity Mod Manager](https://github.com/newman55/unity-mod-manager) reduced to the loader. [RocketLib](https://github.com/Gorzontrok/RocketLib), [Bro-Maker](https://github.com/Gorzontrok/Bro-Maker) and the mods compile against `UnityModManager.dll`, so removing or changing a public member breaks mods already built.

## Build

- `dotnet build UnityModManager.sln`, after putting the gitignored `lib/UnityEngine.dll` and `lib/UnityEngine.UI.dll` in place.
- Windows only: the csproj's PreBuild and PostBuild targets are Windows commands that close Broforce, copy the DLL into a fixed r2modman profile's `UMM/Core/` and launch the game through Steam; elsewhere PreBuild fails (MSB3073).

## Package and paths

- `Releases/Package/` is the Thunderstore package's tree; UnityDoorstop loads its `UMM/Core/UnityModManager.dll`. Its `UMM/Core/Config.xml` is Broforce's game config, the repo's only copy.
- Paths resolve from the folder holding `UnityModManager.dll`: `Config.xml` and `Log.txt` beside it, mods in `../Mods`, saved state in `../Config/` (`UMM/Params.xml`, and `<Id>/Settings.xml` per mod). `Params.xml` holds each mod's enabled state, which r2modman never sees: a mod unticked in UMM's window logs "To skip (disabled)." whatever r2modman shows.
- Keep saved state out of mod folders: r2modman replaces a package's folder on update.
- `UnityModManager.version` is the csproj's `AssemblyVersion` (upstream UMM's version), not the package's `version_number`; a mod whose `ManagerVersion` exceeds it fails to load.
- A mod whose `ManagerVersion` is below 0.13, or unset with no `MinimalManagerVersion` in Config.xml (as in most Broforce mods), loads through the legacy path: its `UnityModManager` references are rebound and it can never be reloaded.

## Startup flow

1. UnityDoorstop calls `Doorstop.Entrypoint.Start` → `UnityModManager.Main`, which waits for `Assembly-CSharp` to load; then `Injector.Run` reads Config.xml and Params.xml, applies `Fixes`, and Harmony-patches the Config.xml points.
2. `StartingPoint` (after `Startup.Start`) runs `UnityModManager.Start`, which discovers, sorts and loads mods.
3. `UIStartingPoint` (after `MainMenu.Start`) spawns the `UI` MonoBehaviour.

- On a runtime with `Environment.Version` below 4.0, `Fixes` makes `Assembly.GetTypes()` return an empty array for any assembly named `UnityModManager*`.

## Mods

- Each folder directly under `Mods` is one mod. `Info.json` is read from its root, else the first one found below it (r2modman nests package files); that file's folder becomes the mod's `Path`.
- Mods load in dependency order over `Requirements` and `LoadAfter`. Loading a mod enables and activates each required mod, even one the user disabled; a missing or too-old one fails the load. A `Requirements` entry is `Id`, or `Id-X.Y.Z` (three parts) for a minimum version; anything else is read as the whole Id.
- The DLL loads from a `.cache` copy in the mod folder: the original stays unlocked while the game runs, and a rebuilt DLL takes effect at next launch.
- Setting `ModEntry.Active` on an unloaded mod loads it. A DLL mod without `OnToggle` can't be deactivated at runtime: unticking it only clears `Enabled`, effective next launch.
- The `UI` MonoBehaviour calls `OnUpdate`, `OnGUI` and the other per-frame callbacks on active mods, so none fires before `UIStartingPoint`; each call's exceptions are caught and logged under the mod.
- `OnSaveGUI` runs only from the window's Save button and on exit (`SaveSettingsAndParams`, which also writes Params.xml; active mods only), and before a reload.

## Logging

- `UnityEngine.Debug.Log` output never reaches `Log.txt`, only the Unity player log, the sole trace when startup aborts and the UI never spawns.
- Loader code logs through the static `UnityModManager.Logger` (`[Manager]` prefix), per-mod messages through `ModEntry.Logger` (`[<Id>]` prefix).
- Every logged line is mirrored to `Console.WriteLine`; `NativeLog` writes only there.
- Lines are buffered and written to `Log.txt` only from the `UI` object's `LateUpdate` and on exit: nothing is written before the UI spawns, and lines logged just before a hard crash can be lost, so read the Unity player log then; UMM deletes the old `Log.txt` when it initializes, so a `Log.txt` dated before the launch means UMM never loaded that run.

## Logs

- **UMM log** (UMM's and every mod logger's output): `UMM/Core/Log.txt` in the r2modman profile the game ran with, under `%AppData%\r2modmanPlus-local\Broforce\profiles\<profile>\` on Windows and `~/.config/r2modmanPlus-local/Broforce/profiles/<profile>/` on Linux.
- **Unity player log** (the engine's log from process start, rotated each launch): read it when the UMM log is missing, for engine errors and for native crashes. Windows `%USERPROFILE%\AppData\LocalLow\Free Lives\Broforce\output_log.txt`; native Linux `~/.config/unity3d/Free Lives/Broforce/Player.log`; Proton `<Steam library>/steamapps/compatdata/274190/pfx/drive_c/users/steamuser/AppData/LocalLow/Free Lives/Broforce/output_log.txt`.

## Code style

- Comment only a genuinely complex algorithm or an engine or runtime gotcha the code can't show: at most two lines, at the point of use, never wrapped to a column width. No labels, rationale, ordering or coupling notes, contracts, restatements, deletions or history; keep existing label comments.
- No doc blocks on files, classes, methods or fields, except a one-sentence XML `///` comment on a public API another project calls.

## Changelogs

- `Releases/Package/CHANGELOG.md`, with `Releases/Package/manifest.json`'s `version_number` as the released version. Headers are `## vX.Y.Z (vA.B.C of base UMM repo)`, newest first.
- Entries are `-` bullets, each a change a player would notice in one short, non-technical clause of at most 20 words, never a list of symptoms, names or implementation detail. A branch adds at most one bug-fix line: one clause saying what its fixes change for a player.
- Write a branch's lines once its changes are complete, covering the whole branch.
- Versioning: `v1.0.X` for minor features, non-breaking changes and small fixes; `v1.X.0` for breaking changes and significant features; `vX.0.0` for major rewrites or major new features.

## Docs

- Repo docs (`AGENTS.md` files, READMEs, tool descriptions, changelogs) state the current contract only: no other project's defects, transitional caveats or fixes in progress.
