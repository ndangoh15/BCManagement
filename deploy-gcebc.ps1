# ==========================
# CONFIG
# ==========================
$server   = "CS-PROG01"
$username = "CS-PROG01\admin"

# Chemins projets
$angularProjectPath = "D:\GCEB PROJECT\BirthCertificateScanner\BCManagement\front-end"
$dotnetProjectPath  = "D:\GCEB PROJECT\BirthCertificateScanner\BCManagement\back-end"

# Chemins de BUILD LOCAUX (hors IIS)
$localBuildRoot     = "D:\GCEBC_BUILD"
$angularBuildOutput = Join-Path $localBuildRoot "Front"
$dotnetBuildOutput  = Join-Path $localBuildRoot "API"

# Log
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logRoot   = "C:\DeployLogs\GCEBC"
$logPath   = Join-Path $logRoot "deploy_$timestamp.log"

# ==========================
# FONCTIONS
# ==========================
function Ensure-Dir {
    param([string]$p)
    if ($p -and -not (Test-Path $p)) {
        New-Item -Path $p -ItemType Directory -Force | Out-Null
    }
}

function Write-Log {
    param([string]$msg)
    Ensure-Dir $logRoot
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $msg"
    Write-Host $line
    Add-Content -Path $logPath -Value $line
}

# ==========================
# MAPPER Z: AVANT D'UTILISER LES CHEMINS DISTANTS
# ==========================
Write-Log "Préparation du lecteur Z: vers \\$server\C$ ..."
net use Z: /delete /y | Out-Null
Start-Sleep 1
Remove-PSDrive -Name Z -ErrorAction SilentlyContinue
New-PSDrive -Name Z -PSProvider FileSystem -Root "\\$server\C$" | Out-Null

# Maintenant on peut déclarer les chemins distants
# $remoteRootBase     = "Z:\inetpub\wwwroot\GCEBC"
# $angularRemotePath  = Join-Path $remoteRootBase "GCEBC_Front"
# $dotnetRemotePath   = Join-Path $remoteRootBase "GCEBC_API"

$remoteRootBase     = "Z:\"
$angularRemotePath  = Join-Path $remoteRootBase "GCEBC_Front"
$dotnetRemotePath   = Join-Path $remoteRootBase "GCEBC_API"

Ensure-Dir $remoteRootBase
Ensure-Dir $angularRemotePath
Ensure-Dir $dotnetRemotePath

# ==========================
# BUILD LOCAUX
# ==========================
Ensure-Dir $localBuildRoot
Ensure-Dir $angularBuildOutput
Ensure-Dir $dotnetBuildOutput

Write-Log "Build Angular..."
Set-Location $angularProjectPath

if (-not (Test-Path (Join-Path $angularProjectPath "node_modules"))) {
    Write-Log "node_modules absent → npm install"
    npm install
    if ($LASTEXITCODE -ne 0) { throw "npm install failed" }
}

ng build --configuration=production --output-path "$angularBuildOutput" --base-href /
if ($LASTEXITCODE -ne 0) { throw "ng build failed" }

Write-Log "Build API .NET..."
Set-Location $dotnetProjectPath

dotnet restore
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }

dotnet publish ".\WebAPI\WebAPI.csproj" -c Release -o "$dotnetBuildOutput" --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

# ==========================
# COPIE FRONT VERS IIS
# ==========================
Write-Log "Copie FRONT vers $angularRemotePath ..."
robocopy "$angularBuildOutput" "$angularRemotePath" /MIR /R:1 /W:1 | Out-Null

Write-Log "FRONT déployé."

# ==========================
# COPIE API VERS IIS
# ==========================
Write-Log "Copie API vers $dotnetRemotePath ..."
robocopy "$dotnetBuildOutput" "$dotnetRemotePath" /MIR /R:1 /W:1 | Out-Null

# Supprimer éventuel web.config Angular (avec AngularRoutes / mimeMap) dans l'API
$apiWebCfg = Join-Path $dotnetRemotePath "web.config"
if (Test-Path $apiWebCfg) {
    $content = Get-Content $apiWebCfg -Raw
    if ($content -like "*AngularRoutes*" -or $content -like "*mimeMap*") {
        Remove-Item $apiWebCfg -Force
        Write-Log "web.config Angular supprimé dans le dossier API."
    }
}

# Recréer un web.config propre pour .NET
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments=".\WebAPI.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\log" />
  </system.webServer>
</configuration>
"@ | Set-Content $apiWebCfg

Write-Log "API déployée."

# ==========================
# RESTART IIS
# ==========================
Write-Log "Redémarrage IIS..."
iisreset | Out-Null

Write-Log "✅ Déploiement GCEBC terminé."
