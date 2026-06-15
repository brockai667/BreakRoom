@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Lobby fixes: weapon always shown on podium (no empty center), backdrop moved back (less overlap), dimmer pedestal ring, weapon shelf board, counter/crates/plants to fill space; smooth Filled RAGE bar + faster gain"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
