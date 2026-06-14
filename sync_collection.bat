@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "Wave 3: Office + Factory map themes (RoomTheme surfaces + item palettes), glass-shatter FX/sound on electronics (Fx.Glass + SfxManager.Glass), slow-mo clear finisher punch (Sting + shake)"
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
