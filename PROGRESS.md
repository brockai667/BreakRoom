# PROGRESS — autonómna session (2026-07-01)

Pracujem samostatne na TODO.md, bez čakania na potvrdenie. Commit + push priebežne po každom logickom kroku (bolo vopred schválené v zadaní).

## Stav na začiatku
- `git pull` → up-to-date, žiadne nové commity zvonku.
- V pracovnom strome bola nekomitnutá zmena `Assets/TextMesh Pro/Resources/Fonts & Materials/Bangers-Regular SDF.asset` (Unity re-serializovalo font asset, -421/+9 riadkov). Nesúvisí s mojou úlohou, nechávam ju tak (nekomitujem ju samostatne, ak sa nepremieša s mojimi zmenami omylom).

## Plán (z TODO.md + vlastná iniciatíva)
1. [ ] GameManager legacy cleanup (EndRound, endPanel, timeText, destroyedText, moneyEarnedText, totalMoneyText)
2. [ ] Zmazať Assets/Scenes/SampleScene.unity + .meta
3. [ ] SpecialObjects — duplicitné "lamp" v ELECTRONIC
4. [ ] Collection lifetime štatistiky (najlepší grade, celkovo rozbitých vecí) → PlayerPrefs + zobrazenie v Collection UI
5. [ ] XP bar flow (LegacyXPUI/XPManager) — overiť a opraviť
6. [ ] XML docstringy + slovenské komentáre k verejnému API
7. [ ] Bugy / dead code / zastarané Unity API
8. [ ] Konzistencia PlayerPrefs kľúčov a Save()/OnChanged v PlayerInventory

## Rozhodnutia
- Používam `graphify query` pred väčšími zmenami podľa CLAUDE.md.
- Veci overiteľné len v Unity editore (napr. scéna sa naozaj vygeneruje/nič sa nerozbije) zapíšem nižšie ako "na overenie v Unity".

## Na overenie v Unity
(bude dopĺňané priebežne)

## Log krokov

### 1. GameManager legacy cleanup — HOTOVÉ
- `graphify query` + grep potvrdili: `.EndRound()` nemá žiadneho volajúceho okrem vlastnej definície;
  `endPanel/timeText/destroyedText/moneyEarnedText/totalMoneyText` boli čítané len v `GameManager.Start()`
  (null-check) a zapisované len z `Assets/Editor/AddGameSystems.cs` (jednorazový editor nástroj na scaffolding
  starého End-Round panelu v scéne Office).
- Odstránené z `Scripts/GameManager.cs`: polia `endPanel/timeText/destroyedText/moneyEarnedText/totalMoneyText`,
  metóda `EndRound()`, null-check `if (endPanel != null) endPanel.SetActive(false)`, nepoužívaný `using UnityEngine.UI;`.
- `Assets/Editor/AddGameSystems.cs`: odstránená celá "END ROUND PANEL" sekcia (staval GameObjecty EndPanel/TimeText/...
  a priraďoval ich do polí, ktoré už neexistujú) + nepoužívané helpery `MkT`/`MkBtn`. Ponechaná tvorba
  `PlayerInventory`/`GameManager`/`HandDisplay` a čistenie starých objektov v scéne (vrátane legacy "EndPanel" nálepky,
  ak by ešte v scéne ostala zo starej verzie).
- Dôvod ponechania zvyšku `AddGameSystems.cs`: stále vytvára platné HandDisplay UI, nie je súčasťou zadania na zmazanie.
- `graphify update .` spustené, graf aktuálny.
