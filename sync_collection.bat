@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Progression + new maps: weapon level-gating, flamethrower as premium (real-money IAP placeholder), 2 new maps Garage+Kitchen (RoomTheme re-skin, level unlocks, per-map music), fix Frenzy power-up un-pausing the game"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
