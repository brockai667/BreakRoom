@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Real Kenney models for maps: Garage = factory-kit workshop (machines, hoppers, conveyor, cogs, crates), Bedroom = furniture-kit (bed, dressers, wardrobe, desk, plants, boxes) - genuinely distinct, not copies; FBX instantiate + fitted box colliders"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
