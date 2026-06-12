@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  echo --- status ---
  git status --short
  echo --- add ---
  git add -A
  echo --- commit ---
  git commit -m "Gameplay update 2: per-weapon realistic swing animations (overhead/horizontal/diagonal/jab) + impact-timed hits, hit-stop + camera shake on impact, weapon upgrades for money, settings menu (sensitivity/volume/FOV/quality)"
  echo --- pull ---
  git pull --no-edit
  echo --- push ---
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
