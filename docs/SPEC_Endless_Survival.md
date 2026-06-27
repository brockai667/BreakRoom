# Spec: Endless / Survival mód + lokálny rebríček

**Hra:** Break Room (FPS rozbíjačka, Unity 6000.3.16f1, URP)
**Stav:** Návrh (v1)
**Autor:** Claude • **Dátum:** 2026-06-27

---

## Problem Statement

Hra má momentálne len jeden druh kola: vyčisti miestnosť alebo vyprší pevný čas → koniec → Hub. Keď je miestnosť vyčistená, nie je dôvod hrať ďalej a každé kolo je rovnaké. Chýba režim s **dlhou hrateľnosťou** a **vlastným skóre na porovnávanie**, ktorý dáva dôvod vracať sa a „prebíjať si rekord".

## Goals

1. **Pridať druhý herný mód „Survival"** voliteľný z Hubu pre ktorúkoľvek mapu — bez zásahu do existujúceho Normal módu.
2. **Predĺžiť priemernú dĺžku session** — survival beh trvá, kým hráč „nevydrží", nie pevných 5 min.
3. **Zaviesť vlastné skóre + lokálny rebríček na mapu** (PlayerPrefs), aby mal hráč čo prekonávať.
4. **Recyklovať existujúce systémy** (GameManager combo/AwardBreak, Breakable, juice, HUD, Round Summary) — minimum nového kódu, žiadne nové assety.
5. **Posilniť ekonomiku** — survival stále zarába `$` cez `AwardBreak`, takže kŕmi Shop.

## Non-Goals (vedome mimo v1)

1. **Online/globálny rebríček** — len lokálne (PlayerPrefs). Sieť je samostatná iniciatíva.
2. **Nové mapy alebo assety** — survival beží na existujúcich miestnostiach.
3. **Denná výzva** — príbuzná, ale samostatná feature (ďalší spec).
4. **Skiny zbraní / odmeny viazané na survival** — neskôr; teraz len skóre + `$`.
5. **Vlny s unikátnymi „boss" objektmi** — eskalácia v v1 je cez tempo/HP, nie nový obsah.

## User Stories

- Ako **hráč** chcem si v Hube vybrať pri mape *Survival*, aby som mal okrem bežného kola aj nekonečný mód.
- Ako **hráč** chcem, aby sa miestnosť počas survivalu **dopĺňala**, takže je vždy čo rozbíjať a beh neskončí „prázdnotou".
- Ako **hráč** chcem **predlžovať si čas rozbíjaním**, aby ma odmeňovalo agresívne hranie a tlačilo to na tempo.
- Ako **hráč** chcem na konci vidieť **skóre, prežitý čas, najlepšie combo, vlnu a zarobené `$`**, aby som vedel, ako sa mi darilo.
- Ako **hráč** chcem v Hube vidieť **najlepšie survival skóre pre danú mapu** (a v Main Menu celkový rekord), aby som mal čo prekonávať.

## Requirements

### Must-Have (P0) — bez toho mód nedáva zmysel

**R1 — Výber módu.** `GameSession` dostane `enum GameMode { Normal, Survival }` + `SelectedScene`. Hub má pri spustení mapy prepínač/tlačidlo „Survival". Pri Normal sa správanie nemení.
- *AC:* Spustenie z Hubu cez Survival načíta zvolenú room scénu a v nej beží survival; spustenie cez Normal sa správa presne ako dnes.

**R2 — SurvivalManager (samostatný singleton).** Nový komponent riadi survival beh. `GameManager` sa pri `GameSession.Mode == Survival` zdrží pevného časovača a auto-konca cez objectives; deleguje na `SurvivalManager`. Vzor singletonu presne ako v projekte (`public static SurvivalManager Instance { get; private set; }`, guard v `Awake`).
- *AC:* V Normal móde `SurvivalManager` neexistuje/neaktívny; v Survival móde GameManager nevyhodnocuje „origRemaining<=0".

**R3 — Survival klok (čas sa míňa, rozbíjanie ho dopĺňa).** Beh štartuje s `startClock` (napr. 30 s), ktorý plynule klesá. Každé rozbitie pridá čas: `+timePerBreak` (základ ~1.0 s) × veľkostný bonus × combo bonus, do `maxClock` stropu (napr. 45 s). Klok 0 = koniec behu.
- *AC:* Bez rozbíjania klok klesne na 0 a beh skončí; sústavné rozbíjanie klok udrží/zdvihne až po strop.

**R4 — Respawn (miestnosť sa dopĺňa).** Pri štarte si `SurvivalManager` zaznamená transformy a typy všetkých `Breakable` v scéne ako **spawn pointy**. Po zničení objektu sa po `respawnDelay` na voľnom spawn pointe vytvorí nový breakable. Udržiava cieľový počet živých objektov (napr. 12–20).
- *AC:* Po rozbití sa miestnosť do pár sekúnd dopĺňa; nikdy nenastane stav „nič na rozbitie" počas behu.

**R5 — Skóre + ukončenie.** Survival `score` = Σ (hodnota breaku × `ComboMultiplier`). Na konci sa zobrazí end-screen (recyklovaný Round Summary) so: skóre, prežitý čas, najlepšie combo, dosiahnutá vlna, zarobené `$`. `$` aj XP idú normálne cez `AwardBreak`.
- *AC:* End-screen sa ukáže pri kloku 0; hodnoty sedia s priebehom; `$` sa pripísali do `PlayerInventory`.

**R6 — Lokálny rekord na mapu.** Po behu sa skóre porovná a uloží do PlayerPrefs: `Survival_Best_<scene>` (najvyššie skóre) a `Survival_BestTime_<scene>`. Aktualizuj `Survival_BestEver` (max cez mapy) pre Main Menu.
- *AC:* Vyššie skóre prepíše rekord; nižšie nie; hodnoty prežijú reštart hry.

### Nice-to-Have (P1) — výrazne zlepší, ale jadro funguje aj bez

**R7 — Vlny / eskalácia.** Každých `waveSeconds` (napr. 30 s) prežitia `wave++`: skráti `respawnDelay`, zvýši HP násobič nových breakable, mierne zníži `timePerBreak` (rastúci tlak). Announcer hláška „WAVE 2!".
**R8 — Rebríček v Hube.** Panel „Survival — Best" so skóre+časom pre každú mapu (Top 1, ideálne Top 3 cez `Survival_Best_<scene>_1..3`).
**R9 — Main Menu rekord.** Pod existujúcim „najlepšie kolo" ukázať „Best Survival: <score>".
**R10 — Juice na klok.** Pri kloku < 5 s červené pulzovanie HUD + zvuk; combo míľniky dávajú väčší časový bonus + hlášku.

### Future Considerations (P2) — navrhnúť tak, aby sa nezablokovali

- Online rebríček (oddeliť ukladanie skóre za rozhranie, nech sa dá neskôr vymeniť za server).
- Survival-špecifické modifikátory (napr. „len kladivo", „2× `$`").
- Boss-vlny s JACKPOT objektmi (už existuje JACKPOT objekt — dá sa vstreknúť do vlny).

## Success Metrics

**Leading (hneď po zostavení):**
- Priemerná dĺžka survival behu ≥ 90 s (cieľ), stretch ≥ 150 s.
- Survival beh vyvolá ≥ 1.5× viac rozbití než priemerné Normal kolo na tej istej mape.
- 0 errorov v konzole, stabilný respawn (žiadny stav „prázdna miestnosť").

**Lagging:** vyšší podiel session, kde sa hráč po behu vráti do Hubu a kúpi/upgradne zbraň (skóre tlačí na lepšie vybavenie). Merané manuálne v playteste (nie je analytika).

## Open Questions

- **(Engineering)** Respawn ako *nový* `Breakable` na spawn pointe vs. re-enable pôvodného? → návrh: instancovať primitívny/ľahký breakable, aby sa neviazalo na konkrétne modely mapy. *Blokujúce pred P0.*
- **(Design)** Štartovať Hub s prepínačom Normal/Survival, alebo dať Survival ako samostatné tlačidlo na karte mapy? → návrh: samostatné tlačidlo „SURVIVAL" vedľa „SMASH". *Neblokujúce.*
- **(Design)** Strop `maxClock` — koľko, aby sa nedal beh „naťahovať donekonečna" pri ľahkej mape? → vyladiť v playteste.

## Timeline / Phasing

**Fáza 1 (hrateľné jadro):** R1 + R2 + R3 + R4 + R5 s recyklovaným Round Summary. → playtest „dá sa survival hrať a skončí".
**Fáza 2 (skóre + progres):** R6 + R7 + skóre/štatistiky na end-screen. → playtest „rekord sa ukladá, tlak rastie".
**Fáza 3 (meta + leštenie):** R8 + R9 + R10. → playtest „rebríček v Hube/Menu, klok juice".

Po každej fáze: screenshot v Play (per stojace pravidlo), oprava, potom `/ship` (push cez `sync_collection.bat`).

## Acceptance Criteria (zhrnutie)

- [ ] Z Hubu sa dá spustiť ľubovoľná mapa v Survival móde; Normal mód nezmenený.
- [ ] Klok klesá; rozbíjanie ho dopĺňa po strop; pri 0 beh končí.
- [ ] Miestnosť sa počas behu sústavne dopĺňa (cieľový počet živých objektov).
- [ ] End-screen ukáže skóre, čas, best combo, vlnu, `$`; `$`/XP pripísané.
- [ ] `Survival_Best_<scene>` + `Survival_BestEver` sa ukladajú a prežijú reštart.
- [ ] (P1) Vlny zrýchľujú respawn a zvyšujú HP; Announcer hlási vlnu.
- [ ] (P1) Hub ukáže best skóre na mapu; Main Menu celkový rekord.
- [ ] 0 errorov; Normal mód po zmene plne funguje (regresný test).
