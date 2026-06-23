@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Factory reborn as a connected assembly line (conveyor spine + products, machines flanking & facing the belt, input silo -> scanner gate -> robot arms -> output pallets, overhead catwalk + magnet crane, wall pipe run, floor flow arrows, structures backdrop) - researched real production-line flow. Lobby: move workers to the barrier so they no longer overlap the counter/crates. Screenshot-verified in Play, 0 errors"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
