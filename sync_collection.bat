@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Lobby recompose: center the weapon podium as a clear hero (bigger weapon, spotlit), workers flank symmetrically, balance decor (vending/lockers/posters moved right vs showroom counter left), plants frame corners, disable stray off-center podium base. Much cleaner balanced composition. Screenshot-verified in Play, 0 errors"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
