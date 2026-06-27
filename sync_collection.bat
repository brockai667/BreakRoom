@echo off
cd /d "C:\Users\damia\BreakRoom"
if exist ".git\index.lock" del /f /q ".git\index.lock"
(
  echo ===== SYNC %date% %time% =====
  git status --short
  git add -A
  git commit -m "NEW MODE: Endless/Survival + local leaderboard. SurvivalManager (singleton): depleting clock you refill by smashing (combo = bigger time bonus), room continuously respawns crates on recorded furniture spawn points, waves every 30s tighten respawn + announce. Score = sum(pay x combo); ends at clock 0 -> saves Survival_Best_<map> + Survival_BestEver. Hub: purple SURVIVAL button above SMASH, 'Survival best: N' in map label; survival end-screen; Main Menu shows best. GameManager defers to SurvivalManager in survival mode (Normal mode untouched). GameSession.Mode plumbing. Spec in docs/SPEC_Endless_Survival.md. Play-verified in Living Room: score+money+time on smash, waves 1->12, pause/quit saves record (best 12), 0 errors."
  git pull --no-edit
  git push
  echo ---- EXIT CODE: %errorlevel% ----
) > sync_collection.log 2>&1
