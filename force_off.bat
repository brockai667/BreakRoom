@echo off
cd /d "C:\Users\damia\BreakRoom"
shutdown /a >nul 2>&1
shutdown /s /f /t 0 > shutdown_out.txt 2>&1
