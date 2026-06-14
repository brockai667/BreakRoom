@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Polish pass: animated round-summary breakdown (base+bonus+NEW BEST), MainMenu Collection button + lifetime stats, chainsaw continuous weapon, throwable grenade, achievements viewer in Hub, split SFX/Music volume, first-launch tutorial hints"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
