@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Redesign Main Menu: 3D hangar (factory-kit) + rotating sledge hero on podium, AAA layout (title banner top, weapon left, PLAY/COLLECTION/QUIT right stack), cinematic camera pan, spotlight + accent lights; fix edit-mode Destroy in WeaponPreview; flatten BPS materials"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
