# Shared Beds: a RimWorld mod

> This is a mod for the video game [RimWorld](https://rimworldgame.com/) (version 1.6). It is a hobby project, not professional software.

![Shared Beds](About/Preview.png)

Beds sit empty about two thirds of the time. With **Shared Beds** you can mark any colonist bed as *shared*. A shared bed has no permanent owner: a colonist claims it when going to sleep and releases it on waking up. The same bed can serve two or three colonists on different sleep schedules.

It suits barracks, small colonies, colonies on 2 or 3 shifts, polyamorous colonists, and colonies running on Wake-up.

## Features

- **Shared bed button** on every colonist bed, next to *Medical* and *For prisoners*. Shared beds have pink sheets and show "Shared" when free.
- **How colonists pick a bed at bedtime:**
  1. Their own permanently assigned bed, as in vanilla.
  2. A lover's bed, if it has a free slot. This also works with several partners.
  3. A free shared bed. They prefer the one they used before, then the most impressive room. Double beds are kept for couples.
  4. Otherwise, vanilla claims an unowned normal bed for them. A setting controls whether shared beds come before this.
- **Room bonuses work while sleeping.** While asleep, a colonist really owns the shared bed, so bedroom and barracks thoughts, *Greedy* and *Jealous* behave as usual. A greedy colonist is content while sleeping in an impressive shared room.
- **Trade-off:** colonists get a small *No personal space* mood debuff (default -2, adjustable) for one day after sleeping in a shared bed without a bed of their own. Ascetics don't mind.
- **Safe for colonists with their own bed.** If one of them naps in a shared bed, they keep their own bed.
- **Bed alert:** the early-game "Need colonist beds" alert counts each shared sleeping slot as several colonists (default 3).

## Requirements

- RimWorld 1.6
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)

DLCs are not required. The mod works with Hospitality: guest beds can't be made shared.

## Installation

Until the Steam Workshop release, download or clone this repository into your `RimWorld/Mods/` folder. Then enable **Shared Beds** after Harmony in the mod list.

## Settings

Go to *Options → Mod settings → Shared Beds*:

| Setting | Default |
|---|---|
| Prefer shared beds over unowned normal beds | on |
| "No personal space" mood effect (0 disables) | -2 |
| Colonists per shared sleeping slot, for the beds alert | 3 |

## Translations

English, Spanish, Latin American Spanish, French, German, Italian, Brazilian Portuguese, Russian, Ukrainian, Polish, Simplified and Traditional Chinese, Japanese, Korean and Turkish.

Most of these were machine translated. Corrections are welcome as issues or pull requests: the files are in `Languages/<Language>/`.

## Building from source

The C# source is in `Source/`. You need the .NET SDK (6 or newer) and a local RimWorld install:

```sh
cd Source
dotnet build -c Release
```

This writes `1.6/Assemblies/SharedBeds.dll`. The project expects RimWorld and Harmony in the default Steam paths on Linux (`~/.steam/steam/steamapps/...`). On other systems, override `RimWorldManaged` and `HarmonyDir` with `-p:`.

## Removing the mod from a save

Removing the mod is safe. Shared beds turn back into normal beds, and anyone asleep in one at that moment keeps it as their own bed.
