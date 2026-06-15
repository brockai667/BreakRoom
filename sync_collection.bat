@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Fix map scale: calibrated to Obyvacka pattern (multiply native localScale, Bedroom S=0.4, Garage S=1.25) - furniture/machines now correct human/workshop scale, verified in play. Garage=factory workshop, Bedroom=real furniture"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
