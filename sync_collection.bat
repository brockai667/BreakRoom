@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Wave 1 polish: hit-marker on hit/break (CrosshairUI ping), JACKPOT mini-boss object per round (SpecialObjects + Breakable jackpot multiplier), Best-round earnings stat on MainMenu"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
