---
name: art-direction
description: "Art bible pre BreakRoom (smash hra, štýl One Armed Robber – low-poly flat-shaded). Použi pri každej požiadavke na grafiku, vizuál, dizajn miestnosti/scény, paletu, materiály, svetlo, post-FX, UI/HUD, modely, prefa­by alebo keď sa rieši, prečo niečo 'nevyzerá premyslene'. Dáva slovník, pravidlá štýlu, konkrétne URP nastavenia, hex paletu napojenú na UITheme/RoomTheme, sekciu o 'juice' deštrukcii a hotový prompt na generovanie konzistentnej grafiky. Triggers: dizajn, grafika, vizuál, art, štýl, look, scéna, room, miestnosť, paleta, farby, materiál, shader, svetlo, lighting, post-fx, UI, HUD, font, model, asset, prefab, 'ako to spraviť pekné', 'nevyzerá dobre'."
---

# BreakRoom — Art Direction Skill (One Armed Robber low-poly)

Toto je **art bible** pre hru BreakRoom: smash/destrukčná hra, kde si vyberáš zbrane a rozbíjaš veci v miestnostiach.
Cieľový štýl = **One Armed Robber** a podobné hry: **low-poly, flat-shaded, farebné, hravé, prehnané proporcie, šťavnatá deštrukcia.** NIE realistické, NIE futuristické, NIE PBR.

> **Ako tento skill používať.** Keď dostaneš úlohu na grafiku/dizajn (navrhni miestnosť, sprav paletu, oprav vzhľad, sploašti materiály, nastav svetlo, sprav HUD…), **najprv si prečítaj relevantnú časť tohto dokumentu a drž sa jej.** Vždy odpovedz konkrétne na tento projekt (jeho súbory, scény, paletu), nie všeobecne. Na konci je v **Časti F** hotový prompt, ktorý sa dá kopírovať aj do iného Clauda/image-gen nástroja.

---

## Zlaté pravidlo (keď si nie si istý)

> **Tvar a farba, nie textúra a lesk.** Čitateľná silueta + plochá sýta farba + jedno teplé svetlo + šťavnaté rozbitie. Detail rieš geometriou a farbou, nikdy normal/metallic/AO mapami.

Ak by čokoľvek nižšie bolo v rozpore s týmto, vyhráva toto pravidlo.

---

## Časť A — Slovník (aby si vedel povedať čo chceš)

| Pojem | Čo znamená v tejto hre |
|---|---|
| **Flat-shaded** | Materiál bez lesku a bez máp; povrch má jednu plochú farbu, tieni ho len svetlo. Smoothness/metallic = 0. |
| **Low-poly** | Málo polygónov, hranaté, „chunky" tvary. Radšej menej detailu, viac čitateľnosti. |
| **Silueta** | Obrys objektu. Ak spoznáš vec podľa čierneho obrysu, je dobre navrhnutá. |
| **Albedo / base color** | Základná farba materiálu (`_BaseColor` v URP, `_Color` v Standard). U nás jediný nositeľ farby. |
| **Tint / re-tint** | Prefarbenie objektu zmenou base color (to robí `RoomTheme.cs`). |
| **Colormap** | Jedna malá textúra s farebnými plôškami (Kenney štýl). OK. Fotorealistická textúra NIE OK. |
| **Juice / game feel** | Hit-stop, screen shake, úlomky, zvuk, vyskakujúce skóre — to, čo robí rozbíjanie uspokojivým. |
| **Hit-stop** | Krátke zamrznutie času pri údere (máme `Juice.HitStop`). |
| **Key / fill / ambient** | Hlavné (teplé) svetlo / dopĺňajúce (studené) svetlo / celkový základný jas scény. |
| **Accent** | Značková farba hry — oranžová **#FF7524** (`UITheme.Accent`). Combo, skóre, dôležité UI. |
| **Readability pass** | Kontrola, či hráč na prvý pohľad rozozná „čo môžem rozbiť" od pozadia. |

Týmito slovami so mnou hovor — „daj tomu výraznejšiu siluetu", „sploašti materiály", „pridaj juice", „zladi paletu s accentom".

---

## Časť B — Pravidlá štýlu (One Armed Robber look)

**1. Geometria**
- Low-poly, hranaté, mierne prehnané proporcie (väčšie, „bacuľatejšie" tvary pôsobia hravo).
- Priorita = **čitateľná silueta**. Keď to nespoznáš ako čiernu siluetu, zjednoduš/zväčš charakteristický prvok.
- Kenney packy (Furniture, Factory Kit) sú referenčný level detailu — drž sa ho.

**2. Materiály — najdôležitejšie pravidlo projektu**
- Všetko **flat**: smoothness 0, metallic 0, **žiadne** normal / metallic / occlusion / height (parallax) mapy.
- Farba ide cez `_BaseColor` (alebo vertex color). Nič iné nesmie niesť farbu.
- **Zákaz miešať štýly.** Realistické PBR assety (Brick Project Studio) musia byť pred použitím sploaštené (`BreakRoom → Art → Flatten Materials…`), inak bijú s Kenney. Keď to nejde sploašti pekne, radšej ten asset nepoužívaj.

**3. Farba**
- Obmedzená, sýta, ale nie neónová paleta. Teplé svetlo, doplnkové studené tiene.
- Každá miestnosť má vlastnú tému (pozri `RoomTheme.cs`) — paleta nesie identitu miestnosti.
- Veci na rozbitie majú byť **farebne odlíšené od pozadia** (steny/podlaha tlmené, predmety sýtejšie) → hráč hneď vidí cieľ.
- Accent **#FF7524** je posvätný: combo, skóre, jackpot, dôležité UI. Nepoužívaj ho na bežné steny/nábytok.

**4. Svetlo (rovnaké vo všetkých scénach)**
- 1× teplý directional „key" zhora-zboku (mäkký tieň) + jemný studený ambient „fill".
- Mäkké tiene, ľahké AO. Žiadne ostré realistické reflexie — tie zabíjajú flat look.
- Konzistencia naprieč scénami je dôležitejšia než „pekné svetlo" v jednej scéne.

**5. Kompozícia / readability**
- Jasný ohniskový bod miestnosti (kde sa najviac rozbíja).
- Pozadie tlmené, interaktívne veci výrazné. Keď je scéna „samá vec", oko sa stratí.

**6. UI / HUD**
- Font **Bangers** (komiksový, úderný) na skóre a combo → hravý feel.
- Panely cez `UITheme` (zaoblené rohy, mäkký tieň, accent). Drž jeden štýl, nemiešaj.

---

## Časť C — Unity / URP technické nastavenia (konkrétne)

**Render pipeline:** URP (už používaš).

**Materiály (každý herný materiál):**
- Shader: `Universal Render Pipeline/Lit` (alebo `Simple Lit` pre ešte plochejšie).
- `Smoothness = 0`, `Metallic = 0`.
- Žiadne textúry v: Normal Map, Metallic/Specular, Occlusion, Height. Len Base Map (colormap) alebo čistá Base Color.
- Keywords off: `_NORMALMAP`, `_METALLICSPECGLOSSMAP`, `_OCCLUSIONMAP`, `_PARALLAXMAP`; environment/specular reflections off.
- ➜ Na hromadné nasadenie použi **`Assets/Editor/FlattenMaterials.cs`** (`BreakRoom → Art → Flatten Materials…`). Najprv Dry run + Folder `Assets/Brick Project Studio`, režim `ShadingOnly`; pre maximálne flat `SolidFromAlbedo`.

**Svetlo (na scénu, nech sú rovnaké):**
- Directional „key": teplá farba (~#FFE3B0), intenzita ~1.0–1.2, mäkké tiene (Soft Shadows).
- Ambient: `Source = Color`, jemná studená (~#3A4150), nízka intenzita — alebo jemný gradient.
- ➜ Vynúť cez `ProLighting.cs`, nech to nemusíš klikať v každej scéne.

**Post-processing (URP Volume, jemne — nech to nie je „instagram filter"):**
- Bloom: mierny, threshold vysoko (nech žiaria len jasné veci / accent).
- Color Adjustments: mierne zvýšený kontrast + saturácia (hravosť).
- Vignette: veľmi jemná.
- (Voliteľne) Tonemapping: Neutral. Žiadne ťažké DOF/motion blur — kazí čitateľnosť smash akcie.

**Fonty:**
- Bangers (už naimportovaný) na skóre/combo/announcer. Bežný text nechaj na čitateľný UI font.

---

## Časť D — Paleta (hex, napojená na tvoj kód)

**Značkové / UI (z `UITheme.cs` — nemeniť svojvoľne):**
| Rola | Hex | Pozn. |
|---|---|---|
| Accent (oranžová) | `#FF7524` | combo, skóre, jackpot, CTA |
| Accent dim | `#8C3D12` | tlmená verzia accentu |
| Overlay | `#05050A` (82 %) | stmavenie pozadia za menu |
| Panel | `#1F2129` | UI panel |
| Panel light | `#2B2E3B` | svetlejší panel |
| Button normal / hover | `#333947` / `#4A5266` | tlačidlá |
| Good | `#389E4D` | potvrdenie/úspech |
| Danger | `#C7422E` | varovanie/zrušenie |
| Text / sub-text | `#FFFFFF` / `#B8C2D6` | |

**Per-room palety (z `RoomTheme.cs` — identita miestností):**
- **Garage** — tlmená priemyselná: steny `#52575F`, podlaha `#454547`; veci: hrdza, oceľ, olejová zelená, pneumatika, drevo, žltý nástroj.
- **Kitchen** — svetlá čistá: steny `#DBDBD1`, podlaha `#C7CCD1`; veci: biela, nerez, keramická modrá, krémová, červený spotrebič, mentol, chróm.
- **Office** — neutrálna: steny `#BDC2CC`, podlaha `#666B75`; veci: čierna technika, sivá skriňa, drevený stôl, modrá stolička, papier, béžová, zelená doska.
- **Factory** — betón/kov: steny `#73757A`, podlaha `#4D4D52`; veci: oceľ, hrdza, výstražná žltá, tmavý kov, drevená debna, olejová zelená, meď.
- **Bedroom** — teplá levanduľová: steny `#9E8FA8`, podlaha `#755740`.

**Pravidlo pre nové miestnosti:** vyber 1 dominantnú náladu (teplá/studená/tlmená), 5–7 farieb predmetov v rovnakej rodine, steny/podlahu tlmenejšie než predmety. Pridaj scénu do `RoomTheme.cs` rovnakým vzorom (`wall/floor/ceil` + pole `…Items`).

---

## Časť E — Juice / deštrukcia (80 % „wow" je tu, nie v modeloch)

Aj s úplne jednoduchými low-poly modelmi pôsobí hra skvele, ak je rozbíjanie šťavnaté. Toto máš čiastočne hotové — pri každej smash akcii skontroluj, či hrá:

1. **Hit-stop** — `Juice.HitStop(0.04–0.08f)` pri zásahu (máš `Juice.cs`).
2. **Screen shake** — `CameraShaker.cs`, krátky, úmerný sile úderu.
3. **Úlomky/fragmenty** — viacstupňové ničenie (`Breakable.cs`: golden/explosive/jackpot varianty).
4. **Zvuk** — `SfxManager.cs` / `Announcer.cs`; vrstvi (úder + prasknutie + dopad).
5. **Vyskakujúce skóre / combo** — v accent farbe `#FF7524`, Bangers font, krátka „pop" animácia.
6. **Čiastočky/prach + (voliteľne) jemný flash** — `Fx.cs`.

**Tuning pravidlo:** silnejšia zbraň/väčší objekt = dlhší hit-stop + väčší shake + viac úlomkov + hlasnejší zvuk. Slabý úder nech je naozaj slabý — kontrast robí pocit.

---

## Časť F — Hotový prompt (kopíruj do Clauda / image-gen)

> Použi tento blok, keď chceš odo mňa (alebo iného nástroja) konkrétny návrh. Vyplň `[…]`.

```
Si art director pre low-poly smash hru "BreakRoom" v štýle One Armed Robber.
ŠTÝL (povinný): low-poly, flat-shaded, sýte ploché farby, prehnané proporcie,
čitateľné siluety, teplé jednotné svetlo, šťavnatá deštrukcia. ZAKÁZANÉ:
realizmus, PBR, normal/metallic/AO/height mapy, lesk, futuristický look.

KONTEXT: Unity URP. Farba cez _BaseColor (RoomTheme prefarbuje miestnosti).
Accent #FF7524 len na combo/skóre/jackpot/dôležité UI. Font Bangers na skóre/combo.

ÚLOHA: [napr. navrhni break room "Kitchen" od nuly / urob audit tejto scény zo
screenshotu / navrhni paletu pre novú miestnosť X / navrhni vzhľad zbrane Y].

VÝSTUP chcem v tomto poradí:
1. Koncept (1–2 vety: nálada, ohniskový bod).
2. Paleta: 5–7 hex farieb (steny/podlaha tlmené, predmety sýtejšie) + kde použiť accent.
3. Zoznam objektov na rozbitie s farbami a prečo sú čitateľné voči pozadiu.
4. Svetlo: key (teplá, hex+intenzita) + ambient (studená, hex) + post-FX.
5. Juice: čo má hrať pri rozbití (hit-stop, shake, úlomky, zvuk, skóre pop).
6. Konkrétne kroky/kód pre tento projekt (RoomTheme záznam, materiály, prefab-y).

Buď konkrétny a konzistentný s vyššie uvedeným štýlom. Žiadne všeobecné reči.
```

---

## Časť G — Workflow (poradie podľa dopadu)

1. **Zjednoť štýl** — sploašti všetky ne-Kenney materiály (`Flatten Materials…`). Najväčší jediný skok.
2. **Jednotné svetlo + post-FX** vo všetkých scénach (`ProLighting.cs` + URP Volume).
3. **Ukotvi 1 referenčnú miestnosť** poriadne (paleta + svetlo + readability). Zvyšok je kópia toho istého jazyka.
4. **HUD do štýlu** — Bangers na skóre/combo, accent #FF7524.
5. **Vylaď juice** na hlavných objektoch (hit-stop/shake/úlomky/zvuk/skóre pop).
6. Až potom rieš nové modely/miestnosti — vždy podľa Časti F.

---

## Napojenie na existujúci kód (kde čo žije)

- `Assets/Editor/FlattenMaterials.cs` — sploaštenie materiálov na flat (menu `BreakRoom → Art`).
- `Assets/Scripts/RoomTheme.cs` — per-room paleta (stena/podlaha/strop + predmety), prefarbuje cez `_BaseColor`.
- `Assets/Scripts/UITheme.cs` — UI paleta + accent `#FF7524`, zaoblené panely, hover, tieň.
- `Assets/Scripts/Juice.cs` — hit-stop. `CameraShaker.cs` — shake. `Fx.cs` — efekty. `Breakable.cs` — viacstupňová deštrukcia (golden/explosive/jackpot).
- `Assets/Scripts/ProLighting.cs` — vynútenie jednotného svetla.
- `SfxManager.cs`, `Announcer.cs`, `MusicManager.cs` — zvuk.
- Scény s témou: **Garage, Kitchen, Office, Factory, Bedroom** (+ hub/lobby).
```
