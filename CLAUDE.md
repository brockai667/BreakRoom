# Break Room — pracovný postup pre Claude

## Git workflow (DÔLEŽITÉ)
- **PRED každou prácou: `git pull`** (stiahni najnovší stav, nech nepracujem na starom).
- **PO každej práci: `git commit` + `git push`** na GitHub (`brockai667/BreakRoom`, vetva `main`).
- Ak je `.git\index.lock`, najprv ho zmaž.
- Pomocný skript na rýchly commit+push: `sync_collection.bat` (alebo `git_sync.bat`).

## Pozn. k prostrediu
- Push z izolovaného sandboxu nejde (chýba GitHub prihlásenie) — push sa robí
  na používateľovom Windowse (dvojklik na .bat, alebo cez computer-use).
- Unity (6000.3.16f1) je GUI na Windowse; import/kompilácia prebehne pri otvorení projektu.
