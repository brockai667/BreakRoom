@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC COLLECTION %date% %time% =====
  echo --- status ---
  git status --short
  echo --- add ---
  git add -A
  echo --- commit ---
  git commit -m "Add Collection screen (CollectionManager): weapon collection grid + player progress, self-builds from code, wired from MainMenu"
  echo --- pull ---
  git pull --no-edit
  echo --- push ---
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
