@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Content update: 4 new weapons (crowbar/katana/chainsaw/bowling), achievements + lifetime stats, global perks shop (PerksMenu + Perks effects), per-map music + combo intensity, modern pause menu (resume/restart/quit to hub)"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
