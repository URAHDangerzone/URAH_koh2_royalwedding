# URAH Royal Wedding

A mod for **Knights of Honor II: Sovereign**: an unmarried king can hold a royal wedding feast to find
a bride among the noble ladies of his own court. The button sits in the empty spouse slot of the royal
family window. The feast costs gold (more for a bigger kingdom), takes 90 seconds and succeeds by
chance: base 20 %, plus the king's level, knights at court and the king effects of Courtesy
(+10/20/30 %) and Rhetoric (+5/10/15 %), minus old age and war. A failed feast keeps its gold and the
next one has to wait 5 minutes. Works with any map.

It is a data mod (`URAH_RoyalWedding/`, the action's defs and texts) plus a BepInEx 5 plugin
(`plugin/`) that adds the action. The plugin only patches the game while the mod is active.

## Install

1. BepInEx 5 in the game folder.
2. `URAH_RoyalWedding.dll` into `BepInEx/plugins/URAH_RoyalWedding/`.
3. The folder `URAH_RoyalWedding/` into the game's Mods folder
   (Windows: `%USERPROFILE%\AppData\LocalLow\BlackSeaGames\Sovereign\Mods\`), then activate
   **URAH Royal Wedding** in the game's Mods menu.

Multiplayer: host and guests need mod and plugin; the feast only shows for guests while the host runs
the plugin.

## Building

```sh
git clone --recursive <this repo>      # external/URAH_shared is a submodule
cd plugin && dotnet build -c Release -p:Deploy=true
```

Compiles against your own game install with BepInEx 5 (`GameDir`, default: Steam on Linux; pass
`-p:GameDir=...`). `Deploy=true` copies the DLL into `BepInEx/plugins/`.

## License

The code, defs and texts of this repository are under the MIT License (`LICENSE`). The preview image
(`URAH_RoyalWedding/mod_preview.png`) is art from Knights of Honor II: Sovereign, © Black Sea Games /
THQ Nordic, and is not covered by the MIT License.
