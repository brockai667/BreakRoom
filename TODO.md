# Break Room — TODO (hotové, pozri PROGRESS.md pre detaily)

## 1. Collection (zbierka) — ✅ HOTOVÉ
Vytvorený `Assets/Scripts/CollectionManager.cs` — postaví sa sám z kódu
(cez `RuntimeInitializeOnLoadMethod`, rovnako ako `SpecialObjects`).
- [x] Skript `CollectionManager.cs`
- [x] Obsah: progres (level, peniaze, odomknuté X/7) + mriežka všetkých
      zbraní so stavom (nasadené / vlastníš / zamknuté + cena), rarity farby a ikony.
- [x] Tlačidlo SPÄŤ -> MainMenu.
- [ ] OVERIŤ V UNITY: otvoriť projekt (vygeneruje sa .meta), spustiť hru,
      MainMenu -> Collection -> späť. (nedá sa otestovať bez editora)
- [x] Lifetime štatistiky (najlepší grade, celkovo rozbitých vecí) — hotové,
      pozri PROGRESS.md sekcia 4.

## 2. Vyčistiť legacy kód v GameManager — ✅ HOTOVÉ
`GameManager` mal staré End-Round UI polia označené "legacy - nepoužité"
(endPanel, timeText, destroyedText, moneyEarnedText, totalMoneyText) a metódu
`EndRound()`. Nový flow ide cez Hub.
- [x] Odstránené nepoužité polia a legacy metóda (overené cez graphify + grep,
      že ich už nič nevolá) aj naviazaný kód v `Assets/Editor/AddGameSystems.cs`.

## 3. Zmazať SampleScene — ✅ HOTOVÉ
`Assets/Scenes/SampleScene.unity` bola default Unity scéna, nikde sa nepoužívala.
- [x] Vymazaná scéna + .meta, odstránená z EditorBuildSettings a z
      `Assets/Editor/ButtonSetup.cs`, zmazaný aj osirotený SampleSceneProfile.

## 4. Drobnosti na overenie — ✅ HOTOVÉ
- [x] XP bar (LegacyXPUI / XPManager) — nájdený a opravený skutočný bug (bar
      sa nikdy nezobrazoval, pozri PROGRESS.md sekcia 5). Na overenie v Unity.
- [x] SpecialObjects: duplicitné "lamp" v ELECTRONIC odstránené.

## Ďalšie (vlastná iniciatíva, pozri PROGRESS.md)
- [x] XML/slovenské doc komentáre k verejnému API v Assets/Scripts.
- [x] Bug sweep (MainMenuExtras Best_<mapa> pokrytie, Objectives RNG skew).
- [x] Audit PlayerPrefs kľúčov a Save()/OnChanged v PlayerInventory.
