@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Factory + Office rebuilt properly using measured model sizes (FactoryKitProbe -> kit_sizes.txt). Factory: pieces now actually CONNECT on the grid - belt rotated 90 and placed every 2u (continuous), machines flush to belt edge, connected pipe run, gantry crane over output, scanner gate, flow arrows. Office: real Kenney open-plan office (CreateOfficeKenney) - rows of desks with monitors+chairs, meeting table, bookcases, filing cabinets, plants; removed the giant pink Living-Room rug that bled through; player spawns at the entrance. Both Play-verified, 0 errors"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
