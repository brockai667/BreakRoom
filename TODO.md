# Break Room — TODO (nedokončené)

## 1. Collection (zbierka) — ✅ HOTOVÉ
Vytvorený `Assets/Scripts/CollectionManager.cs` — postaví sa sám z kódu
(cez `RuntimeInitializeOnLoadMethod`, rovnako ako `SpecialObjects`).
- [x] Skript `CollectionManager.cs`
- [x] Obsah: progres (level, peniaze, odomknuté X/7) + mriežka všetkých
      zbraní so stavom (nasadené / vlastníš / zamknuté + cena), rarity farby a ikony.
- [x] Tlačidlo SPÄŤ -> MainMenu.
- [ ] OVERIŤ V UNITY: otvoriť projekt (vygeneruje sa .meta), spustiť hru,
      MainMenu -> Collection -> späť. (nedá sa otestovať bez editora)
- [ ] Voliteľne neskôr: pridať lifetime štatistiky (najlepší grade, celkovo
      rozbitých vecí) — vyžaduje uložiť ich do PlayerPrefs na konci kola.

## 2. Vyčistiť legacy kód v GameManager
`GameManager` má staré End-Round UI polia označené "legacy - nepoužité"
(endPanel, timeText, destroyedText, moneyEarnedText, totalMoneyText) a metódu
`EndRound()`. Nový flow ide cez Hub.
- [ ] Odstrániť nepoužité polia a legacy metódy (po overení, že ich už nič nevolá).

## 3. Zmazať SampleScene
`Assets/Scenes/SampleScene.unity` je default Unity scéna, nikde sa nepoužíva.
- [ ] Vymazať scénu + .meta.

## 4. Drobnosti na overenie
- [ ] XP bar (LegacyXPUI / XPManager) — overiť, či sa v hube/kole reálne zobrazuje.
- [ ] SpecialObjects: zoznam ELECTRONIC má "lamp" dvakrát (kozmetické).
