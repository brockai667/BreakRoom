@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Remove RAMPAGE entirely (RampageManager + GameManager/WeaponHit refs, RAGE bar gone); Shop: real 3D weapon model icons rendered to sprites (WeaponIcon, brighter isolated lighting, 2D fallback) + bigger card text; Lobby: premium rounded buttons + orange accent, warmer room + more decor (vending machine, lockers, posters, wall trims, ceiling lights, plants); Main Menu finished: unified premium PLAY/COLLECTION/QUIT buttons with accent. Verified in Play, 0 errors"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
