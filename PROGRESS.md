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

### 2. Zmazanie SampleScene — HOTOVÉ
- Overené: žiadny runtime kód nevolá `SceneManager.LoadScene("SampleScene")`.
- Scéna bola v `ProjectSettings/EditorBuildSettings.asset` (index 1, MainMenu je index 0 = štartovacia) — odstránená
  aj odtiaľ, inak by build obsahoval odkaz na neexistujúci súbor.
- `Assets/Editor/ButtonSetup.cs` mal v `toAdd[]` hardcoded reťazec `"Assets/Scenes/SampleScene.unity"` (guard
  `File.Exists` by to aj tak preskočil, ale radšej vyčistené, nech zoznam nezavádza).
- Zmazaný aj `Assets/Settings/SampleSceneProfile.asset` (+.meta) — URP Volume Profile, ktorý používala výhradne
  táto scéna a po jej zmazaní by ostal osirotený (nikde inde v Assets ani ProjectSettings sa naň neodkazuje).

### 3. SpecialObjects — duplicitné "lamp" — HOTOVÉ
- `Scripts/SpecialObjects.cs`: pole `ELECTRONIC` malo `"lamp"` dvakrát — odstránený duplikát.

### 4. Collection lifetime štatistiky — HOTOVÉ
- Zistil som, že `Stat_smashed` (celkovo rozbité), `Stat_bestCombo` a `Stat_cleared` **už existovali** —
  počíta a ukladá ich `Scripts/Achievements.cs` (`CommitRound`, volané pri prechode `GameSession.HasPendingResult`
  false→true) a zobrazuje ich `CollectionManager.cs` aj `MainMenuExtras.cs`. Chýbal iba "najlepší grade".
- Pridané: `Achievements.CommitRound` teraz berie aj `GameSession.PendingGrade`, porovná ho s uloženým
  `Stat_bestGrade` cez pomocník `GradeRank(string)` (S=5...D=1) a uloží lepší z oboch do PlayerPrefs
  (`PlayerPrefs.SetString("Stat_bestGrade", ...)`).
- Zobrazenie: `CollectionManager.cs` (riadok lifetime štatistík) a `MainMenuExtras.cs` (stĺpec štatistík na
  MainMenu) teraz obsahujú aj "Best grade: X".
- Rozhodnutie: logiku som pridal do existujúceho `Achievements.CommitRound`, nie do `GameManager`, lebo tam už
  bola jediná zbernica pre všetky lifetime štatistiky (jednotné miesto zápisu, žiadna duplicita).

### 5. XP bar — NÁJDENÝ A OPRAVENÝ SKUTOČNÝ BUG
- Tok: `Breakable`/`GameManager.AwardBreak` volajú `XPManager.Instance.AddXP()`, `XPManager.UpdateUI()` nastaví
  `xpBarFill.fillAmount`, text (level/XP/level-up) rieši `LegacyXPUI.Update()` cez polia `levelText/xpText/...`.
  Samotná logika (XP krivka, ukladanie do PlayerPrefs) bola v poriadku.
- Problém bol v **prepojení UI**: `XPManager` predtým nebol self-bootstrapping (na rozdiel od `RoundHUD`,
  `CollectionManager`, `Achievements`, `MainMenuExtras`) — čakal, že ho niekto ručne vloží do scény a prepojí
  `xpBarFill`/`LegacyXPUI` polia v Editore. Skontroloval som všetky uložené scény (`grep XPManager
  Assets/Scenes/*.unity`):
  - `Office.unity` malo GameObject `XPManager`, ale **`xpBarFill: {fileID: 0}`** (neprepojené) a **žiadny
    `LegacyXPUI`** — hoci `Assets/Editor/CreateOfficeScene.cs` (generátor) XP HUD aj `LegacyXPUI` correctly stavia,
    uložená scéna je zjavne staršia než tento generátor (nebol odvtedy znova spustený/scéna neuložená).
  - `Bathroom/Bedroom/Factory/Garage/Kitchen/Obyvacka/Warehouse.unity` mali len holý GameObject `XPManager` bez
    `xpBarFill` a bez akéhokoľvek textu/baru v scéne vôbec — `Assets/Editor/Create{Factory,LivingRoom}Scene.cs`
    XPManager len vytvoria (`AddComponent<XPManager>()`), ale UI vôbec nestavajú; ostatné generátory
    (Bathroom/Bedroom/Garage/Kitchen/Warehouse) XPManager nevytvárajú vôbec.
  - Výsledok: XP sa reálne pripočítavalo a ukladalo (`PlayerPrefs`), ale **XP bar/level text sa hráčovi nikdy
    nezobrazil v žiadnej scéne** (ani v Hube — tam `XPManager` nikdy nebol, len `XPManager.SavedLevel` na
    odomykanie).
- Oprava (`Scripts/XPManager.cs`): prerobené na presne ten istý self-bootstrapping vzor ako `RoundHUD.cs`
  — `[RuntimeInitializeOnLoadMethod]` vytvorí jedinú trvalú inštanciu (`DontDestroyOnLoad`), tá si cez
  `SceneManager.sceneLoaded` sama postaví XP HUD (level text, XP text, bar, level-up flash + `LegacyXPUI`)
  z kódu a zapína/vypína ho podľa scény (skrytý v `Hub/MainMenu/Shop/Collection`, rovnaký `SKIP` zoznam ako
  `RoundHUD`). Staré, holé `XPManager` GameObjecty uložené v scénach sa teraz jednoducho zničia ako duplicitná
  inštancia (singleton guard v `Awake()`) — neškodné, netreba scény ručne upravovať.
- `LegacyXPUI.cs` som nemenil — logika bola správna, len jej polia dovtedy nikdy neboli prepojené.
- Editor generátory (`CreateOfficeScene.cs`, `CreateFactoryScene.cs`, `CreateLivingRoomScene.cs`) som nechal tak,
  ako sú (ich manuálne vytváraný `XPManager` je teraz len neškodný duplikát, ktorý sa hneď zničí) — neprerábal
  som ich, keďže runtime oprava problém rieši univerzálne bez ohľadu na obsah scén.

## Na overenie v Unity
- XP bar TODO#6: otvoriť ľubovoľnú izbu (Bathroom/Bedroom/Factory/Garage/Kitchen/Obyvacka/Office/Warehouse),
  rozbiť niečo a overiť, že vpravo dole naskočí XP bar/level text a pri level-upe sa ukáže "LEVEL UP!" flash.
  Zároveň overiť v Hube/MainMenu/Shope/Collection, že sa XP HUD nezobrazuje (podľa dizajnu).
- Voliteľné (nie nutné): v Unity editore môžete cez staré scény prejsť a ručne zmazať osirotené GameObjecty
  "XPManager" (sú neškodné — zničia sa samé za behu), pre čistotu scén.
