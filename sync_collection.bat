@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "CONTENT: 5 new weapons + 2 new maps. Weapons (data-driven, auto-listed in shop with 3D icons + level unlocks): Frying Pan, Pipe Wrench, Golf Club, Pickaxe, Spiked Mace - new BuildModel cases in WeaponPreview. Maps: Bathroom (tub/toilet/sink+vanity/shower/washer/shelves, all smashable) and Warehouse (racks of crates, pallets, drums, forklift) via new editor builders; added to HubManager.MAPS + build settings. Play-verified: all 5 weapons show in shop with icons/prices, both maps selectable (Bathroom, Warehouse) and generated, 0 compile errors."
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
