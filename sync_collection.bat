@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  echo --- status ---
  git status --short
  echo --- add ---
  git add -A
  echo --- commit ---
  git commit -m "Gameplay update 1: rebalance economy+weapons (less OP, realistic), combo+multiplier, round HUD, reliable auto-evaluate on room clear, per-map objectives, slow-mo + dust juice, fix deprecated FindObjectOfType warnings"
  echo --- pull ---
  git pull --no-edit
  echo --- push ---
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
