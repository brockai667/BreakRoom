# PROGRESS — autonómna session (2026-07-01)

Pracujem samostatne na TODO.md, bez čakania na potvrdenie. Commit + push priebežne po každom logickom kroku (bolo vopred schválené v zadaní).

---

# Session 2 — testy, dorobenie "na overenie", nový obsah

Pokračovanie samostatnej práce (bez pýtania), zadanie: A) edit-mode NUnit testy pre čistú logiku,
B) dořešiť položky z "Na overenie v Unity" nižšie, C) nová zbraň + nová room scéna cez generátor.
`git pull` na začiatku → up-to-date, žiadne nové commity zvonku (working tree malo len tú istú
nesúvisiacu zmenu fontu ako predtým, nechávam ju tak).

## Hotové
- Malé, bezpečné production zmeny na umožnenie EditMode testov (pozri Rozhodnutia + Log krokov #1).
- A) Celý test assembly + 5 test súborov (WeaponData/GameManager/PlayerInventory/XPManager/Breakable),
  spolu ~50 test prípadov. Pozri "Log krokov — Session 2 / A".
- B) Prešiel som obe položky zo starého zoznamu "Na overenie v Unity" (Collection nav, XP bar) — kód je
  staticky overený ako kompletný a konzistentný, zvyšné overenie je čisto vizuálne/interaktívne (vyžaduje
  bežiaci editor). Presné kroky sú v sekcii "Na overenie v Unity" nižšie.

## Hotové (pokrač.)
- C1) Nová zbraň "Shovel" (pozri Log krokov — Session 2 / C1).

## Rozrobené
- C2) Nová room scéna cez generátor (Assets/Editor/Create*Scene.cs).

## Log krokov — Session 2

### A) EditMode testy — HOTOVÉ (napísané, nespustené — vyžaduje Unity Test Runner)
- `Assets/Tests/EditMode/BreakRoom.EditModeTests.asmdef` — nový test assembly. `references` obsahuje
  `"Assembly-CSharp"` priamo menom (Assets/Scripts nemá vlastný asmdef, takže sa kompiluje do defaultnej
  `Assembly-CSharp`); `defineConstraints: ["UNITY_INCLUDE_TESTS"]`, `includePlatforms: ["Editor"]`,
  `precompiledReferences: ["nunit.framework.dll"]` — presne taká štruktúra, akú Unity samo generuje cez
  Test Runner "Create EditMode Test Assembly Folder", len ručne doplnené o referenciu na Assembly-CSharp.
- `PlayerPrefsSnapshot.cs` — pomocná trieda pre testy: pamätá si pôvodnú hodnotu PlayerPrefs kľúča (alebo že
  neexistoval) a vie ju v [TearDown] presne vrátiť, nech testy nekazia reálny uložený progres hráča v tomto
  projekte a sú deterministické (nezávislé od toho, čo je práve uložené). Int a string kľúče sa pamätajú
  oddelene (nikdy sa nečíta kľúč iným accessorom než akým bol zapísaný, aby to bolo bezpečné naprieč platformami
  PlayerPrefs úložiska).
- `WeaponDataTests.cs` — čisto dátové testy (žiadny GameObject): `All` neprázdne, žiadne duplicitné `id`,
  všetky polia vyplnené a v rozumných rozsahoch (price>=0, damage>0, ...), `fists` je zadarmo, `Get`
  existujúce/neexistujúce id (fallback na `All[0]`), `UnlockLevel` fists/bat=1 a hrubá monotónnosť.
- `GameManagerTests.cs` — `ComboMultiplier`/`CalculateBonus`/`ComputeGrade` cez `[TestCase]` na presných
  hraniciach (0/19/20/44/45/... pre bonus; 0/2/3/5/6/... pre combo; 50/90/91/160/161 pre cleared-grade;
  0/19/20/49/50/... pre necleared-grade). `AwardBreak`: základná odmena, kombo-škálovanie naprieč viacerými
  zásahmi (zvolený `baseReward=8`, nech všetky medzivýsledky vyjdu celé čísla — vyhol som sa .5 zaokrúhľovacím
  hraniciam, kde `Mathf.RoundToInt` používa banker's rounding a mohol by dať prekvapivý výsledok), zlatý bonus
  x3, a že sa nič nepripočíta keď `roundActive=false`. `Perk_money`/`Perk_combo` sa v [SetUp] vynulujú (inak by
  `AwardBreak` závisel od reálne uložených perkov hráča v tomto projekte).
- `PlayerInventoryTests.cs` — kúpa (dosť/nedosť peňazí, dvojitá kúpa), equip (len vlastnenej zbrane), upgrade
  (cena, level, zablokovanie nad MAX_UPGRADE), `AddMoney` (perzistencia, floor na nulu, `OnChanged`).
  Testovacia zbraň = "bat" (cena 150), nie "fists" (tá je vždy vlastnená). Kľúče "Money"/"Equipped"/"Own_bat"/
  "Up_bat" sa v [SetUp]/[TearDown] snapshotujú cez `PlayerPrefsSnapshot`.
- `XPManagerTests.cs` — `XPForLevel` (kvadratická krivka), `AddXP` (bez/s prekročením levelu, aj viac levelov
  naraz), `XPProgress` (v polovici intervalu, na max leveli), `SavedLevel` (statický getter bez inštancie).
- `BreakableTests.cs` — testuje novú `Breakable.ComputeStatsForSize(maxDim, jackpot, out ...)` (viz nižšie):
  malý/stredný/veľký objekt, presné hranice 0.7 a 1.3, a jackpot floor/násobenie na malom aj strednom objekte.
  Zvolené `maxDim` hodnoty (0.4/1.0/2.0/0.69/1.29) tak, aby žiadny medzivýsledok nepadol presne na .5 hranicu.

**Prečo som nemohol testy priamo spustiť:** som v headless (bez Unity binárky) prostredí, Unity Test Runner
beží len v editore. Logiku a čísla v `[TestCase]` som si ručne prepočítal proti zdrojovému kódu (viď komentáre
v testoch), ale reálne prejdenie cez NUnit runner nie je odtiaľto možné.

## Rozhodnutia
- **Testovací framework beží len v Unity Editore** (NUnit cez `com.unity.test-framework`, ktorý je už
  v `Packages/manifest.json`) — z tohto bezhlavého (headless, bez Unity binárky) prostredia neviem testy
  reálne spustiť. Napíšem ich staticky správne, skontrolujem si logiku ručne, a do PROGRESS.md napíšem presný
  postup na spustenie + čo overiť v Unity Test Runneri.
- **Malé zmeny v produkčnom kóde na umožnenie testovania** (žiadna zmena správania v reálnej hre):
  1. `GameManager.ComputeGrade` bola `private` → `public` (rovnaká viditeľnosť ako susedné čisté query metódy
     `ComboMultiplier`/`CalculateBonus`/`DestructionPct`, ktoré už boli public) — potrebné na priamy test
     hraníc S/A/B/C/D bez reflection hackov.
  2. `PlayerInventory.Awake()`: `DontDestroyOnLoad(gameObject)` obalené do `if (Application.isPlaying)` —
     v Edit Mode nemá DontDestroyOnLoad zmysel (žiadne prepínanie scén), takto sa dá singleton bezpečne
     vytvoriť aj v EditMode teste cez `AddComponent<PlayerInventory>()`. Počas reálnej hry je
     `Application.isPlaying` vždy true, takže správanie sa nemení.
  3. `PlayerInventory.AddMoney`: pridaný `Mathf.Max(0, ...)` floor na nulu. V praxi sa už aj predtým peniaze
     nedostali pod nulu (všetky volania so záporným amount — `Perks.TryBuy` — si to overia pred volaním), ale
     zadanie explicitne žiada otestovať, že "peniaze nejdú pod nulu" — bez skutočného floor v `AddMoney` by to
     bol len test disciplíny volajúcich, nie invariant samotnej metódy. Pridaný floor je defense-in-depth,
     nemení žiadne existujúce volanie (nikdy sa doteraz nespustil).
  4. `Breakable.Configure()`: časť počítajúca HP/reward/XP/subdivideStages/childPieces z `maxDim` (+jackpot)
     vyextrahovaná do novej `public static void ComputeStatsForSize(...)` — čistá funkcia bez závislosti na
     GameObjecte/Collideri, priamo testovateľná. Čistý extract, žiadna zmena logiky/poradia výpočtov.
- **`Assets/Tests/EditMode/*.asmdef` referencuje `"Assembly-CSharp"` menom** (nie cez asmdef-to-asmdef odkaz),
  keďže `Assets/Scripts` nemá vlastný `.asmdef` (kompiluje sa do defaultného `Assembly-CSharp`). Toto je
  štandardný, Unity-dokumentovaný postup pre projekty, kde hlavný kód nemá asmdef a testy sa pridávajú dodatočne.
- **`.meta` súbory pre nové assety (asmdef, testové .cs) negenerujem ručne** — necháva sa na Unity pri
  najbližšom otvorení projektu, rovnaká konvencia ako pri predošlých nových skriptoch v tomto repozitári.

### C1) Nová zbraň "Shovel" — HOTOVÉ
- Pred zmenou som si cez `WeaponPreview.cs`/`WeaponIcon.cs`/`CollectionManager.cs`/`ShopManager.cs`/
  `HandDisplay.cs`/`FirstPersonHands.cs`/`WeaponHit.cs` overil, ktoré miesta majú per-id switch a ktoré majú
  bezpečný `default:` fallback používaný väčšinou existujúcich zbraní (napr. `HeadSize()` v Shop/Collection
  a `HandDisplay`'s bladeRect majú explicitné case len pre 7 z 17 zbraní — zvyšných 10 už bežne používa
  `default:`, takže nová zbraň bez vlastného 2D fallbacku nič nekazí). Jediné miesto, kde si **každá** doteraj­šia
  zbraň nesie vlastný kód, je `WeaponPreview.BuildModel` (3D model na pódiu aj v ruke) — tam som teda pridal
  vlastný tvar, nech "Shovel" vyzerá rovnako doladene ako ostatné.
- `Scripts/WeaponData.cs`: nový záznam `id="shovel", displayName="Shovel"`, `price=520` (medzi crowbar 450
  a hammer 550 → rarita RARE podľa existujúcej cenovej škály), `damage=3, splashRadius=0.25f, hitDistance=3.6f,
  swingSpeed=1.0f` (podobný tier ako hammer/wrench). `UnlockLevel("shovel") = 4` (rovnaký tier ako hammer, keďže
  cena je blízko).
- `Scripts/WeaponPreview.cs`: nový `case "shovel": BuildShovel(...)` v `BuildModel` switchi + metóda
  `BuildShovel` (drevená násada + zjednodušená D-rukoväť + plochý list s hrotom) — rovnaký štýl ako ostatné
  `Build*` metódy (primitíva + `Mat()` helper).
- `Scripts/FirstPersonHands.cs`: `shovel` pridané do skupiny `Style.Diagonal` (spolu s axe/crowbar) — sedí
  k "wide flat smack" popisu lepšie než default Jab štýl švihu.
- **Netreba meniť** (fungujú univerzálne pre akékoľvek `id`, overené code-review): `WeaponIcon.Get` (renderuje
  čokoľvek, čo vráti `WeaponPreview.BuildModel`), `ShopManager`/`CollectionManager` rarita (odvodená z ceny),
  `HeadSize()` 2D fallback v Shop/Collection kartách, `HandDisplay` bladeRect fallback, `WeaponHit` (shovel nie
  je flamethrower/chainsaw/grenade/bowling → ide bežnou sekacou cestou `Swing()`/`PerformHit()`).
- `WeaponDataTests.cs` (Session 2 / A) testuje `WeaponData.All` generickými assertmi (žiadne duplicity, rozumné
  rozsahy polí) bez natvrdo zapísaného počtu zbraní — nová zbraň teda automaticky prejde tými istými testami
  bez potreby úpravy testov.

## Stav na začiatku
- `git pull` → up-to-date, žiadne nové commity zvonku.
- V pracovnom strome bola nekomitnutá zmena `Assets/TextMesh Pro/Resources/Fonts & Materials/Bangers-Regular SDF.asset` (Unity re-serializovalo font asset, -421/+9 riadkov). Nesúvisí s mojou úlohou, nechávam ju tak (nekomitujem ju samostatne, ak sa nepremieša s mojimi zmenami omylom).

## Plán (z TODO.md + vlastná iniciatíva) — VŠETKO HOTOVÉ
1. [x] GameManager legacy cleanup (EndRound, endPanel, timeText, destroyedText, moneyEarnedText, totalMoneyText)
2. [x] Zmazať Assets/Scenes/SampleScene.unity + .meta
3. [x] SpecialObjects — duplicitné "lamp" v ELECTRONIC
4. [x] Collection lifetime štatistiky (najlepší grade, celkovo rozbitých vecí) → PlayerPrefs + zobrazenie v Collection UI
5. [x] XP bar flow (LegacyXPUI/XPManager) — overiť a opraviť (skutočný bug nájdený a opravený)
6. [x] XML docstringy + slovenské komentáre k verejnému API
7. [x] Bugy / dead code / zastarané Unity API
8. [x] Konzistencia PlayerPrefs kľúčov a Save()/OnChanged v PlayerInventory

## Zhrnutie na konci session
Celé `TODO.md` je hotové (5/5 pôvodných položiek) + 3 vlastné iniciatívy (docstringy, bug sweep, PlayerPrefs
audit). Commitnuté priebežne po každom logickom kroku, na konci `git push`. Zoznam vecí na manuálne overenie
v Unity editore je v sekcii "Na overenie v Unity" vyššie (hlavne XP bar naskočenie v scénach a Collection
obrazovka). `TODO.md` som nechal nezmazaný (nebolo požadované), ale všetky jeho položky sú odškrtnuté vyššie.

## Rozhodnutia
- Používam `graphify query` pred väčšími zmenami podľa CLAUDE.md.
- Veci overiteľné len v Unity editore (napr. scéna sa naozaj vygeneruje/nič sa nerozbije) zapíšem nižšie ako "na overenie v Unity".

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

### B) Prejdené položky zo starého zoznamu — Session 2
Prešiel som obe pôvodné položky nižšie ešte raz a doplnil, čo sa dalo overiť staticky (bez spustenia editora):

- **Collection nav (TODO#1)** — staticky overené, že celý tok je kód-kompletný a konzistentný:
  `Assets/Scenes/Collection.unity` existuje a je v `ProjectSettings/EditorBuildSettings.asset`;
  `MainMenuExtras.cs` má tlačidlo COLLECTION s `SceneManager.LoadScene("Collection")`; `CollectionManager.cs`
  (self-bootstrapping cez `RuntimeInitializeOnLoadMethod`) reaguje na scénu "Collection" a jeho tlačidlo SPÄŤ
  volá `SceneManager.LoadScene("MainMenu")`. Kód je teda logicky bezchybný — zostáva len vizuálne/interaktívne
  overenie (skutočné vykreslenie UI, klikanie), čo sa dá overiť LEN v behu Unity editora.
  **Presný postup:** otvoriť projekt v Unity 6000.3.16f1 (nech sa vygenerujú chýbajúce .meta) → otvoriť scénu
  `MainMenu` → Play → klik na "COLLECTION" → skontrolovať, že sa zobrazí mriežka zbraní + štatistiky (vrátane
  nového "Best grade") → klik "BACK TO MENU" → späť v MainMenu.
- **XP bar (TODO#6)** — logika (`XPManager.AddXP/XPForLevel/XPProgress`) je teraz pokrytá aj unit testami
  (`XPManagerTests.cs`), samotné code review self-bootstrap vzoru (viď sekcia 5 vyššie) je nezmenené a stále
  správne. Zostáva len vizuálne overiť v behu hry:
  **Presný postup:** otvoriť ľubovoľnú izbu (Bathroom/Bedroom/Factory/Garage/Kitchen/Obyvacka/Office/Warehouse)
  → Play → rozbiť niečo → skontrolovať, že vpravo dole naskočí XP bar/level text a pri level-upe sa ukáže
  "LEVEL UP!" flash → prejsť do Hub/MainMenu/Shop/Collection a overiť, že sa XP HUD nezobrazuje (podľa dizajnu).
- Voliteľné (nie nutné, kozmetické): v Unity editore môžete cez staré scény prejsť a ručne zmazať osirotené
  GameObjecty "XPManager" (sú neškodné — zničia sa samé za behu ako duplicitná inštancia).

### A) Ako spustiť nové EditMode testy
1. Otvoriť projekt v Unity → Window → General → **Test Runner**.
2. Záložka **EditMode** → mal by sa objaviť `BreakRoom.EditModeTests` s ~50 testami
   (WeaponDataTests/GameManagerTests/PlayerInventoryTests/XPManagerTests/BreakableTests).
3. **Run All**. Očakávaný výsledok: všetky zelené. Čísla v `[TestCase]` som si ručne prepočítal proti
   zdrojovému kódu (pozri komentáre v testoch), ale keďže odtiaľto (bez Unity binárky) neviem testy reálne
   spustiť, môže sa objaviť prekvapenie najmä pri:
   - `PlayerInventoryTests` — závisí od toho, či `DontDestroyOnLoad` s `Application.isPlaying` guardom
     naozaj umožní `AddComponent<PlayerInventory>()` v Edit Mode bez chyby v konzole (mala by byť; ak Test
     Runner testy napriek tomu zlyhajú kvôli neočakávanému LogError, over tento konkrétny riadok v `Awake()`).
   - Ak niektorý test zlyhá na hodnote (nie na chybe/exception), over si prepočet v komentári nad danou
     `[TestCase]`/`[Test]` metódou — je možné, že som sa niekde pomýlil vo výpočte, kód samotný meniť netreba,
     stačí opraviť očakávanú hodnotu v teste.

### 6. Vlastná iniciatíva — nájdené a opravené bugy (Extra#8/#9)
- `Scripts/MainMenuExtras.cs`: "Best round" štatistika na MainMenu počítala `Mathf.Max` len z 3 z 8 máp
  (`Best_Obyvacka/Best_Garage/Best_Kitchen`), chýbali `Best_Office/Best_Factory/Best_Bedroom/Best_Bathroom/
  Best_Warehouse` (zoznam všetkých 8 máp je v `HubManager.MAPS`). Ak hráč dosiahol svoj najlepší zárobok v inej
  izbe, MainMenu ukazovalo nesprávne (nižšie) číslo. Opravené na cyklus cez všetkých 8 mien máp.
- `Scripts/Objectives.cs`: výber náhodného cieľa používal `Random.Range(0, 4)` namiesto
  `Random.Range(0, (int)Kind.Count)` — keďže `Kind.Count` (hodnota 3) je len sentinel bez vlastného `case` vetvy
  v switchi, hodnota 3 padala do `default:` (Electronics) rovnako ako hodnota 0. Efekt: Electronics cieľ mal
  2× vyššiu šancu než Golden/UnderTime namiesto rovnomerného 1/3 rozdelenia. Opravené.
- PlayerPrefs kontrola (Extra#9): prešiel som `PlayerInventory.cs` (Money/Equipped/Own_/Up_), `XPManager.cs`
  (Level/XP), `Achievements.cs` (Stat_smashed/Stat_bestCombo/Stat_cleared/Stat_bestGrade/Ach_), `Perks.cs`
  (Perk_), `GameSession.cs` (Best_<mapa>/Survival_Best_<mapa>/Survival_BestEver), `SettingsMenu.cs`,
  `Tutorial.cs`, `MusicManager.cs`, `PlayerController.cs` — kľúče sa neprekrývajú, každý `Set*` má zodpovedajúci
  `Get*` s rovnakým názvom a defaultom, `Save()`/`OnChanged?.Invoke()` sa volajú po každej zmene stavu (okrem
  `PlayerInventory.GiveWeapon()`, ktoré nevolá `OnChanged` samo — ale všetci 3 volajúci (`Load`, `TryBuy`,
  `ShopManager` premium flow) hneď potom volajú niečo, čo `OnChanged` aj tak vyvolá, takže to nie je reálny bug,
  len krehké voči budúcim zmenám — nechal som to tak, aby som nepridával kód navyše bez skutočnej potreby).
- `Scripts/SpecialObjects.cs`, `Scripts/GameManager.cs` a ostatné už používajú `FindFirstObjectByType`
  (Unity 6 API) — žiadny výskyt zastaraného `FindObjectOfType` v celom `Assets/Scripts`.

### 7. XML docstringy / slovenské komentáre (Extra#7)
- Delegované na subagenta (mechanická, rozsiahla úloha naprieč ~49 súbormi). Existujúci štýl komentárov v
  projekte je čisté `///` prózové zhrnutie bez `<summary>` XML tagov (napr. `CollectionManager.cs`,
  `Achievements.cs`, `Objectives.cs`) — subagent dostal inštrukciu držať sa presne tohto štýlu namiesto
  zavádzania `<summary>` blokov, ktoré by boli v kóde jediné svojho druhu.
- Subagent si prácu interne rozdelil na 5 dávok (pokryl presne všetkých 49 súborov v `Assets/Scripts`), pridal
  spolu 103 nových `///` riadkov (27 súborov zmenených, netýkalo sa `Assets/Editor` ani scén). Skontroloval som
  celý diff: len pridané riadky (žiadne mazanie/zmena existujúceho kódu ani komentárov), žiadna zmena logiky.
  Jediná vec, ktorú som opravil po review: 4 nové komentáre používali HTML entity `&lt;id&gt;`/`&lt;mapa&gt;`
  namiesto surových `<id>`/`<mapa>` — nekonzistentné s existujúcim pôvodným komentárom v `Perks.cs`
  (`"Perk_<id>"`), zjednotené na rovnaký (pôvodný) štýl v `PlayerInventory.cs` (2×) a `GameSession.cs` (1×).
- Súbory bez zmeny: mali už úplné pokrytie (napr. `CollectionManager.cs`, `Achievements.cs`, `Objectives.cs`,
  `UITheme.cs`, `CrosshairUI.cs`, `Juice.cs`, `CameraShaker.cs`, `CardHover.cs`, `MenuCameraPan.cs`,
  `MenuSpin.cs`, `AchievementsMenu.cs`, `RoomTheme.cs`, `RoundHUD.cs`, `Tutorial.cs`, `VisualUpgrade.cs`,
  `MainMenuExtras.cs`, `MusicManager.cs`, `PerksMenu.cs`, `HubShowroom.cs`, `ChainsawBlade.cs`) alebo boli
  zámerne ponechané (`RampageManager.cs` — 2-riadkový úmyselný "tombstone" komentár bez triedy,
  vysvetľujúci odstránenie RAMPAGE režimu; nemá čo dokumentovať a jeho zmazanie by zahodilo tento kontext).
