@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Visual upgrade: runtime VisualUpgrade (soft shadows, richer trilight ambient, glossier reflective materials) applied to all gameplay/hub scenes; smoothness on debris. Bigger lit look within existing low-poly art"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
