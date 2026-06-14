@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "New Bedroom map (own furniture via CreateBedroomScene, added to build), pro lobby showroom (podium + spotlight + stage backdrop, stanchions+rope, weapon display wall, track lights), HubManager+RoomTheme wiring"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
