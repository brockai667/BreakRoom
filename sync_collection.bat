@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "PRO LOOK via Brick Project Studio (realistic pack already in project, not Roblox blocks): Kitchen rebuilt with real BPS fitted kitchen - base cabinets/sink/stove/fridge counter run, wall cabinets, range hood, coffee maker/blender, dining set (measured BPS sizes, real-world scale). Garage: real garage with a low-poly CAR (body/cabin/windows/wheels/lights), workbench+tools, tyres, oil drums, shelf, garage door. Play-verified, 0 errors. (BPS rollout to Living Room/Bedroom/Office next.)"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
