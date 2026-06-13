# Break Room — pracovný postup pre Claude

## Git workflow (DÔLEŽITÉ)
- **PRED každou prácou: `git pull`** (stiahni najnovší stav, nech nepracujem na starom).
- **Po dokončení úlohy (commit):** ak som ťa v zadaní vopred poprosil commitnúť/pushnúť (napr. „…a nakoniec to pushni"), urob `git commit` + `git push` rovno, **bez ďalšieho pýtania**. Inak sa na konci **opýtaj** „Commitnúť tieto zmeny?" a commitni/pushni až po mojom súhlase. Necommituj rozrobený/rozbitý stav. (GitHub `brockai667/BreakRoom`, vetva `main`.)
- Ak je `.git\index.lock`, najprv ho zmaž.
- Pomocný skript na rýchly commit+push: `sync_collection.bat` (alebo `git_sync.bat`).

## Pozn. k prostrediu
- Push z izolovaného sandboxu nejde (chýba GitHub prihlásenie) — push sa robí
  na používateľovom Windowse (dvojklik na .bat, alebo cez computer-use).
- Unity (6000.3.16f1) je GUI na Windowse; import/kompilácia prebehne pri otvorení projektu.

## Architektúra hry (BreakRoom)
Prvoosobová „rozbíjačka": ničíš nábytok zbraňami → zarábaš `$` a XP → v hube `Hub` nakupuješ/vylepšuješ zbrane. URP, Unity 6000.3.16f1. **Bez namespace-ov, bez DI — singletony + statické fasády.**

**Scény & tok:** room scéna (hra) → po vyčistení miestnosti / vypršaní času `GameManager.EndAndGoHub()` → scéna `Hub`. Statická `GameSession` prenáša výsledok kola medzi scénami (`SetResult`, `InitialHubTab`). Prepínanie: `SceneManager.LoadScene("Hub" | "MainMenu" | <room>)`.

**Kľúčové systémy (singletony cez `Instance`):**
- `GameManager` — beh kola: časovač, combo (`AwardBreak`, `ComboMultiplier`), auto-koniec keď `origRemaining<=0`, hodnotenie S/A/B/C/D, `roundMoney`.
- `PlayerInventory` (`DontDestroyOnLoad`) — peniaze, vlastníctvo/equip/upgrade zbraní, event `OnChanged`. **Persistencia = `PlayerPrefs`** (kľúče `Money`, `Equipped`, `Own_<id>`, `Up_<id>`).
- `XPManager` (XP/level), `Announcer` (hlášky na HUD), `WeaponPreview` (náhľad zbrane na pódiu).
- Statické fasády — volaj `Trieda.Metoda()`: `SfxManager` (Hit/Break/Coin/Boom/Zap), `Fx` (Dust/Sparks/Explosion), `CameraShaker.Shake`, `Objectives.NotifyBreak`.

**Zbrane = data-driven:** `WeaponData.Get(id)` / `WeaponData.All` (id napr. `fists, bat, gloves, hammer, axe, sledge, flamethrower`), polia `price/damage/splashRadius/displayName/…`. Nákup/equip/upgrade rieši výhradne `PlayerInventory`.

**Ničenie (`Breakable`):** HP/reward/XP škálujú podľa veľkosti objektu (`Configure`); veľké veci sa delia na chunky + fragmenty (runtime primitives s `Rigidbody`); varianty `golden/explosive/electronic`. Odmena ide **vždy** cez `GameManager.AwardBreak` (combo + XP + počítadlo).

**UI je v kóde, nie prefaby:** Shop/Collection budujú prvky programovo (`new GameObject` + `AddComponent<Image/Text>`, helpery `UITheme.Rounded/Hover`, legacy `UnityEngine.UI.Text` + `LegacyRuntime.ttf`; miestami TMP). Scény generujú editor skripty `Assets/Editor/Create*Scene.cs`.

## Konvencie kódu (dodržuj presne)
- **Bez namespace-ov.** PascalCase triedy/metódy, camelCase polia, `UPPER_CASE` konštanty, `[Header(...)]` nad inšpektorovými poľami. Komentáre po slovensky, stručné.
- **Singleton vzor presne takto:** `public static T Instance { get; private set; }` a v `Awake`: `if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this;` (+`DontDestroyOnLoad(gameObject)` ak má prežiť zmenu scény).
- **Persistencia = `PlayerPrefs`** s prefixmi (`Own_`, `Up_`); po zmene stavu volaj `Save()` + `OnChanged?.Invoke()`.
- **Unity 6 API:** `FindFirstObjectByType<T>()` (NIE zastarané `FindObjectOfType`). Materiály v URP: shader `Universal Render Pipeline/Lit` (fallback `Standard`), farba cez `_BaseColor`.
- Po každej úprave kódu spusti `graphify update .` (graf ostane aktuálny, 0 nákladov).
- **Pred väčšou zmenou sa zorientuj cez `graphify query "..."`** (nie slepý grep/čítanie) — viď sekcia graphify.

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
