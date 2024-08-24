# MeteorMod ![alt](ThunderStore/MeteorMod_icon_small.png "test")

## Installation
### Thunderstore
- Install either [r2modman](https://github.com/ebkr/r2modmanPlus/releases/latest) (scroll down to Assets) or [Thunderstore (needs overwolf)](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager)
- Open the mod manager and select the game
- Search for [MeteorMod](https://thunderstore.io/c/goodbye-volcano-high/p/Comahar/MeteorMod/)
- Click Install

### Manual
- Download and install [BepinEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html).
    - You can also follow onyx-mp4's beatiful [BepinEx installation guide](https://onyx-mp4.github.io/?scene=bepinex-unzip).
- Download latest release from [Releases](https://github.com/Comahar/MeteorMod/releases/latest/download/MeteorMod.zip).
- Extract the zip file to your game's folder.

## Features
### Settings
![Mod Settings Image](https://github.com/Comahar/MeteorMod/assets/40366128/ab1e7210-eafe-45df-ba68-cbc9953877e1)


#### Subtitles

Disables subtitles in the game.

#### Skip Splash Screen

Skips the start splash screen when starting the game.

#### Debug Menu

Enables debug tools and the developer's debug menu. Access the menu with CTRL+SHIFT+D.

![Debug Menu Image](https://github.com/Comahar/MeteorMod/assets/40366128/64d26043-ecf8-43a8-952e-faa9038e6d2a)


#### Show Hidden Settings

Shows hidden settings created by the developer. As they are hidden, there is a good chance they may not work. They are labeled yellow.

![Hidden Settings Image](https://github.com/Comahar/MeteorMod/assets/40366128/8304de17-2b6d-4c8e-b444-8718c8a4d66b)


#### Beatmaps

Disables rhythm game segments' interactivity and UI. Toggle in a performance with CTRL+SHIFT+F.

### Fixes

- Fixes a visual bug where a toggle setting resets to the previous value when changing settings pages.

## MeteorCore

### Plugin Settings Framework

MeteorCore allows other plugins to create their own settings. It handles saving, loading, creating settings pages and UI items, also comes with basic UI items for common setting types.

### Plugin Localization Framework

MeteorCore creates a framework for other plugins to have their own localisation files.

## Development

### Additional Tools
- [ILSpy](https://github.com/icsharpcode/ILSpy) or [dnSpy](https://github.com/dnSpy/dnSpy)
- [AssetStudio](https://github.com/Perfare/AssetStudio)
- [AssetRipper](https://github.com/AssetRipper/AssetRipper)
- [UnityExplorer](https://github.com/sinai-dev/UnityExplorer)
- [Unity 2021.3.18f1](https://unity.com/releases/editor/whats-new/2021.3.18)

## Support

You can reach me on Discord @wh00rwhat. 

You can also crate an [issue](https://github.com/Comahar/MeteorMod/issues) or [discussion](https://github.com/Comahar/MeteorMod/discussions) on GitHub.

## Roadmap

Even though some of these may be impossible to implement due to the game's limitations or the amount of work needed, it will give something to look forward to.

- [x] Mod setting framework
- [x] Mod localization framework
- [x] Subtitle disable setting
- [x] Skip splashscreen setting
- [x] Enable debug menu setting
- [x] Show hidden settings
- [ ] Minigame disable setting (how would this work?)
- [x] Beatmap disable setting
	- [ ] Disable beatcatcher sounds
- [ ] Disable affinity remember icon (is this neeeded?)
- [ ] Affinity Editor (needs custom settings menu)
- [x] Chapter Select (check out [ChapterSelect](https://github.com/onyx-mp4/ChapterSelect))
- [ ] Post Episode Report (does it even work? how to hook?)
- [ ] Custom Beatmaps
- [ ] Custom Songs
- [ ] Custom Scenes/Animations/Voicelines
- [ ] Rewind system?

## Contributing

Feel free to contribute to this plugin by creating a pull request. Even if you don't know how to code, you can still contribute by creating an issue or a discussion. You can also suggest new features.


## Credits

- @sharky1996 for inspiring me to create this plugin.
- @hadradavus and @onyx.eyes for their help with this plugin.
- @synedrus for selecting the plugin's name.


## License

[LGPL-3.0](https://choosealicense.com/licenses/lgpl-3.0/)
