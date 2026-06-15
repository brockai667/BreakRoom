@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Redesign Perks panel: row backgrounds, contained columns (fixed left overflow), per-perk level progress bar, accent underline - clean modern look matching Settings/Achievements, verified in Hub"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
