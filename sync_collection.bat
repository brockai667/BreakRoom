@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Elevate scenes: new Kitchen builder (real Kenney kitchen models - counter run with stove/sink/fridge, upper cabinets, hood, island + bar stools, dining set); Factory atmosphere (emissive wall stripes + colored accent lights); Garage industrial accents; cozy Bedroom accent lighting (warm lamp + cool window). All verified in Play, 0 errors"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
