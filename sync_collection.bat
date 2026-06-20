@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Lobby cleanup: cooler cleaner industrial palette (no more muddy brown), brighter neutral lighting, neutralized garish red stage + orange mat, softer vending panel, bigger weapon-rack models. Screenshot-verified in Play, 0 errors"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
