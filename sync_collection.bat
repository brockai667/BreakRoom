@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "RAMPAGE ultimate (rage meter HUD + press R, shockwaves chain-destroy, 3x dmg / 2x money, red screen tint), bowling ball now thrown and rolls along the ground (BowlingBall), power-up drops disabled per user request"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
