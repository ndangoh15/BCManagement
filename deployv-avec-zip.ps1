<# 
    Deploy script for GCEBC (Frontend: Angular, Backend: .NET)
    Server : CS-PROG01
    User   : CS-PROG01\admin

    Fonctionnalités :
    - Build Angular + .NET
    - npm install / dotnet restore si nécessaire
    - Compression ZIP des builds (archives locales)
    - Logs détaillés dans C:\DeployLogs\GCEBC\
    - Rollback automatique (front & API) en cas d'échec
    - Déploiement sur IIS via partage admin \\CS-PROG01\C$
#>

# ==========================
#  🔧 VARIABLES GLOBALES
# ==========================
$server   = "CS-PROG01"
$username = "CS-PROG01\admin"

# --- Chemins locaux projets ---
$angularProjectPath = "D:\GCEB PROJECT\BirthCertificateScanner\BCManagement\front-end"
$dotnetProjectPath  = "D:\GCEB PROJECT\BirthCertificateScanner\BCManagement\back-end"


# On supprime Z: s'il existe déjà
if (Get-PSDrive -Name "Z" -ErrorAction SilentlyContinue) {
	Start-Sleep -Seconds 2
    Remove-PSDrive -Name "Z" -Force
}
New-PSDrive -Name "Z" -PSProvider FileSystem -Root "\\$server\C$" -Persist | Out-Null

# --- Chemins locaux de build (temp) ---
# Ce sont juste des dossiers de sortie de build sur ta machine locale
$localRoot = "C:\inetpub\wwwroot\GCEBC"
$angularBuildOutput = Join-Path $localRoot "GCEBC_Front_build"
$dotnetBuildOutput  = Join-Path $localRoot "GCEBC_API_build"

# --- Chemins distants (serveur IIS) via PSDrive Z: ---
$remoteRootBase = "Z:\inetpub\wwwroot\GCEBC"

$angularRemotePath    = Join-Path $remoteRootBase "GCEBC_Front"
$angularRemoteTemp    = Join-Path $remoteRootBase "GCEBC_Front_temp"
$angularRemoteBackup  = Join-Path $remoteRootBase "GCEBC_Front_old"

$dotnetRemotePath     = Join-Path $remoteRootBase "GCEBC_API"
$dotnetRemoteTemp     = Join-Path $remoteRootBase "GCEBC_API_temp"
$dotnetRemoteBackup   = Join-Path $remoteRootBase "GCEBC_API_old"

# --- Logs & archives ---
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logRoot   = "C:\DeployLogs\GCEBC"
$logPath   = Join-Path $logRoot "deploy_$timestamp.log"

$archiveRoot        = "C:\DeployArchives\GCEBC"
$frontArchivePath   = Join-Path $archiveRoot "GCEBC_Front_$timestamp.zip"
$apiArchivePath     = Join-Path $archiveRoot "GCEBC_API_$timestamp.zip"

# --- Outil externe ---
$psexecPath = ".\PsExec.exe"  # doit être dans le même dossier que ce script

# ==========================
#  🧩 FONCTIONS UTILITAIRES
# ==========================
function Ensure-Directory {
    param([string]$Path)
    if (-not (Test-Path $Path)) {
        New-Item -Path $Path -ItemType Directory -Force | Out-Null
    }
}

Ensure-Directory $logRoot
Ensure-Directory $archiveRoot
Ensure-Directory $localRoot

# On crée le fichier de log s'il n'existe pas
if (-not (Test-Path $logPath)) {
    New-Item -Path $logPath -ItemType File -Force | Out-Null
}

function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )
    $time = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "[$time] [$Level] $Message"
    Write-Host $line
    Add-Content -Path $logPath -Value $line
}

function Assert-CommandExists {
    param([string]$CommandName)
    if (-not (Get-Command $CommandName -ErrorAction SilentlyContinue)) {
        Write-Log "La commande '$CommandName' est introuvable. Ajoute-la au PATH ou installe-la." "ERROR"
        throw "Commande manquante : $CommandName"
    }
}

function Assert-FileExists {
    param([string]$Path, [string]$Description)
    if (-not (Test-Path $Path)) {
        Write-Log "$Description introuvable : $Path" "ERROR"
        throw "$Description introuvable : $Path"
    }
}

function Safe-Rename {
    param(
        [string]$Source,
        [string]$Destination
    )
    if (Test-Path $Destination) {
        Write-Log "Suppression de $Destination avant renommage..." "WARN"
        Remove-Item -Path $Destination -Recurse -Force
    }
    if (Test-Path $Source) {
        Write-Log "Renommage de '$Source' vers '$Destination'..." "INFO"
        Rename-Item -Path $Source -NewName (Split-Path $Destination -Leaf)
    } else {
        Write-Log "Source '$Source' introuvable pour renommage." "WARN"
    }
}

# ==========================
#  ✅ VÉRIFICATIONS DE BASE
# ==========================
Write-Log "=== DÉBUT DU DÉPLOIEMENT GCEBC ==="

Write-Log "Vérification des outils (ng, dotnet, robocopy, PsExec)..." "INFO"
Assert-CommandExists "ng"
Assert-CommandExists "dotnet"
Assert-CommandExists "robocopy"
Assert-FileExists $psexecPath "PsExec"

Write-Log "Création des dossiers locaux de build si nécessaire..." "INFO"
Ensure-Directory $angularBuildOutput
Ensure-Directory $dotnetBuildOutput

# ==========================
#  [1/7] BUILD ANGULAR
# ==========================
Write-Log "[1/7] Build Angular..." "INFO"
Set-Location $angularProjectPath

# npm install uniquement si node_modules absent
if (-not (Test-Path (Join-Path $angularProjectPath "node_modules"))) {
    Write-Log "node_modules introuvable. Exécution de 'npm install'..." "INFO"
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Log "npm install a échoué (code $LASTEXITCODE)." "ERROR"
        throw "Échec npm install"
    }
} else {
    Write-Log "node_modules trouvé. npm install ignoré." "INFO"
}

# Build Angular
ng build --configuration=production --output-path "$angularBuildOutput" --base-href /
if ($LASTEXITCODE -ne 0) {
    Write-Log "ng build a échoué (code $LASTEXITCODE)." "ERROR"
    throw "Échec build Angular"
}

# ==========================
#  [2/7] BUILD .NET
# ==========================
Write-Log "[2/7] Build .NET API..." "INFO"
Set-Location $dotnetProjectPath

Write-Log "dotnet restore..." "INFO"
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Log "dotnet restore a échoué (code $LASTEXITCODE)." "ERROR"
    throw "Échec dotnet restore"
}

Write-Log "dotnet publish (Release)..." "INFO"
dotnet publish "D:\GCEB PROJECT\BirthCertificateScanner\BCManagement\back-end\WebAPI\WebAPI.csproj" -c Release -o "$dotnetBuildOutput" --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Log "dotnet publish a échoué (code $LASTEXITCODE)." "ERROR"
    throw "Échec dotnet publish"
}

# ==========================
#  [3/7] COMPRESSION DES BUILDS
# ==========================
Write-Log "[3/7] Compression des builds en archives ZIP..." "INFO"

if (Test-Path $frontArchivePath) { Remove-Item $frontArchivePath -Force }
if (Test-Path $apiArchivePath) { Remove-Item $apiArchivePath -Force }

Compress-Archive -Path (Join-Path $angularBuildOutput '*') -DestinationPath $frontArchivePath
if ($LASTEXITCODE -ne 0) {
    Write-Log "Compression Angular a échoué (code $LASTEXITCODE)." "ERROR"
    throw "Échec compression Angular"
}

Compress-Archive -Path (Join-Path $dotnetBuildOutput '*') -DestinationPath $apiArchivePath
if ($LASTEXITCODE -ne 0) {
    Write-Log "Compression API a échoué (code $LASTEXITCODE)." "ERROR"
    throw "Échec compression API"
}

Write-Log "Archives créées :`n  Front : $frontArchivePath`n  API   : $apiArchivePath" "INFO"

# ==========================
#  [4/7] CONNEXION AU SERVEUR (PSDRIVE Z:)
# ==========================
Write-Log "[4/7] Connexion au serveur distant $server via C$..." "INFO"



# On s'assure que le répertoire distant racine existe
if (-not (Test-Path $remoteRootBase)) {
    Write-Log "Création du répertoire distant : $remoteRootBase" "INFO"
    New-Item -Path $remoteRootBase -ItemType Directory -Force | Out-Null
}

# ==========================
#  [5/7] DÉPLOIEMENT ANGULAR AVEC ROLLBACK
# ==========================
Write-Log "[5/7] Déploiement FRONT (Angular) avec rollback possible..." "INFO"

try {
    # 1. Copier le build local vers un dossier temporaire distant
    Write-Log "Copie du build Angular vers $angularRemoteTemp (robocopy /MIR)..." "INFO"
    robocopy "$angularBuildOutput" "$angularRemoteTemp" /MIR /Z /NP /R:1 /W:1 | Out-Null

    # 2. Préparer backup : Front_old
    if (Test-Path $angularRemoteBackup) {
        Write-Log "Suppression de l'ancien backup Angular : $angularRemoteBackup" "INFO"
        Remove-Item -Path $angularRemoteBackup -Recurse -Force
    }

    if (Test-Path $angularRemotePath) {
        Write-Log "Renommage de l'ancien front vers backup : $angularRemotePath -> $angularRemoteBackup" "INFO"
        Safe-Rename -Source $angularRemotePath -Destination $angularRemoteBackup
    }

    # 3. Activer le nouveau build
    Write-Log "Activation du nouveau front : $angularRemoteTemp -> $angularRemotePath" "INFO"
    Safe-Rename -Source $angularRemoteTemp -Destination $angularRemotePath

    Write-Log "Déploiement Angular terminé avec succès." "INFO"
}
catch {
    Write-Log "ERREUR lors du déploiement Angular : $($_.Exception.Message)" "ERROR"
    Write-Log "Tentative de rollback FRONT..." "WARN"

    # Rollback : si ancien backup existe et que le front actif est KO
    if ((Test-Path $angularRemoteBackup) -and (-not (Test-Path $angularRemotePath))) {
        Safe-Rename -Source $angularRemoteBackup -Destination $angularRemotePath
        Write-Log "Rollback FRONT effectué." "INFO"
    } else {
        Write-Log "Rollback FRONT impossible (pas de backup valide)." "ERROR"
    }

    throw "Échec déploiement Angular (rollback tenté)."
}

# ==========================
#  [6/7] DÉPLOIEMENT .NET AVEC ROLLBACK + IISRESET
# ==========================
Write-Log "[6/7] Déploiement BACKEND (API .NET) avec rollback automatique..." "INFO"

$rollbackNeeded = $false

try {
    # 1. Copier le build local vers dossier temporaire distant
    Write-Log "Copie du build API vers $dotnetRemoteTemp (robocopy /MIR)..." "INFO"
    robocopy "$dotnetBuildOutput" "$dotnetRemoteTemp" /MIR /Z /NP /R:1 /W:1 | Out-Null

    # 2. Arrêt de IIS sur le serveur distant
    Write-Log "Arrêt de IIS sur $server via PsExec..." "INFO"
    & $psexecPath "\\$server" -accepteula iisreset /stop
    if ($LASTEXITCODE -ne 0) {
        Write-Log "iisreset /stop a échoué (code $LASTEXITCODE)." "ERROR"
        throw "Échec arrêt IIS"
    }

    # 3. Préparation backup API
    if (Test-Path $dotnetRemoteBackup) {
        Write-Log "Suppression de l'ancien backup API : $dotnetRemoteBackup" "INFO"
        Remove-Item -Path $dotnetRemoteBackup -Recurse -Force
    }

    if (Test-Path $dotnetRemotePath) {
        Write-Log "Renommage de l'API actuelle vers backup : $dotnetRemotePath -> $dotnetRemoteBackup" "INFO"
        Safe-Rename -Source $dotnetRemotePath -Destination $dotnetRemoteBackup
        $rollbackNeeded = $true
    } else {
        Write-Log "Aucune API active trouvée (premier déploiement ?)." "INFO"
    }

    # 4. Activer le nouveau build
    Write-Log "Activation du nouveau build API : $dotnetRemoteTemp -> $dotnetRemotePath" "INFO"
    Safe-Rename -Source $dotnetRemoteTemp -Destination $dotnetRemotePath

    # 5. Redémarrage de IIS
    Write-Log "Redémarrage de IIS sur $server..." "INFO"
    & $psexecPath "\\$server" -accepteula iisreset /start
    if ($LASTEXITCODE -ne 0) {
        Write-Log "iisreset /start a échoué (code $LASTEXITCODE)." "ERROR"
        throw "Échec redémarrage IIS"
    }

    Write-Log "Déploiement API terminé avec succès." "INFO"
}
catch {
    Write-Log "ERREUR lors du déploiement API : $($_.Exception.Message)" "ERROR"

    # Tentative de rollback API
    Write-Log "Tentative de rollback API..." "WARN"
    try {
        if ($rollbackNeeded -and (Test-Path $dotnetRemoteBackup)) {
            # On essaie de remettre l'ancienne API
            if (Test-Path $dotnetRemotePath) {
                Write-Log "Suppression de l'API déployée défectueuse..." "WARN"
                Remove-Item -Path $dotnetRemotePath -Recurse -Force
            }

            Safe-Rename -Source $dotnetRemoteBackup -Destination $dotnetRemotePath
            Write-Log "Rollback API effectué." "INFO"

            # On tente de redémarrer IIS pour retrouver un état stable
            Write-Log "Redémarrage de IIS après rollback..." "INFO"
            & $psexecPath "\\$server" iisreset /start
        }
        else {
            Write-Log "Rollback API impossible : pas de backup valide." "ERROR"
        }
    }
    catch {
        Write-Log "ERREUR pendant le rollback API : $($_.Exception.Message)" "ERROR"
    }

    throw "Échec déploiement API (rollback tenté)."
}

# ==========================
#  [7/7] CLEAN
# ==========================
Write-Log "[7/7] Nettoyage : démontage du lecteur Z:..." "INFO"
if (Get-PSDrive -Name "Z" -ErrorAction SilentlyContinue) {
    Remove-PSDrive -Name "Z" -Force
}

Write-Log "✅ Déploiement GCEBC terminé avec succès." "INFO"
Write-Log "Journal détaillé dans : $logPath" "INFO"
