@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Modernize UI: shared UITheme (rounded corners, modern palette, button hover, soft shadows); redesign Settings panel (rounded box, accent header, hover, full dim overlay); rounded cards in Shop/Collection, rounded loadout rows, rounded HUD backdrop"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
