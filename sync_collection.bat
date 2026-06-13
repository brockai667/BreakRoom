@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Add power-ups (Rage/Frenzy/Cash/Quake drops), per-weapon swing styles for new weapons, lifetime stats + achievements shown in Collection (smaller weapon grid for 11 weapons)"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
