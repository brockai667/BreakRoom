# SessionStart hook: pripomenie ak Unity MCP server (port 8080) nebezi. Vzdy exit 0 (nikdy neblokuje).
$ErrorActionPreference = 'SilentlyContinue'
try {
  $up = Get-NetTCPConnection -LocalPort 8080 -State Listen -ErrorAction SilentlyContinue
  if (-not $up) {
    $msg = "Pozn.: Unity MCP server (port 8080) prave NEBEZI. Ak budem robit s Unity editorom (sceny, konzola, GameObjecty), najprv otvor Unity (BreakRoom) -> Window -> MCP For Unity -> Start Server. Bez toho ide praca s kodom (Graphify) normalne, ale nie ovladanie editora."
    $payload = @{ hookSpecificOutput = @{ hookEventName = 'SessionStart'; additionalContext = $msg } }
    Write-Output ($payload | ConvertTo-Json -Compress -Depth 5)
  }
} catch { }
exit 0