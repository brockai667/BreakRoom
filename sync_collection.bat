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
  git commit -m "Fixes: translate whole game to English (UI, weapons, HUD, objectives, shop, settings + scene texts), rework Settings panel (visible CLOSE button, centered box, click-outside-to-close), move Settings button below money"
  echo --- pull ---
  git pull --no-edit
  echo --- push ---
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
