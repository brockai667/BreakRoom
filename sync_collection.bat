@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Fix pass: readable power-ups (shape per kind, magnet, fewer, capped), proper weapon models for chainsaw/katana/crowbar/bowling/grenade + animated chainsaw chain (ChainsawBlade), Garage/Kitchen item re-skin via RoomTheme, contained Achievements/Settings panels with row backgrounds and on-design buttons"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
