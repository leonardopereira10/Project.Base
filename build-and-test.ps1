<#
.SYNOPSIS
   Local validation script for Project.Base (build, test, SonarQube analysis).
.DESCRIPTION
   Validates build, runs tests with coverage, and sends results to SonarQube.
   NuGet publishing is ONLY available via GitHub Actions workflow.
.USAGE
   .\build-and-test.ps1
   powershell -ExecutionPolicy Bypass -File build-and-test.ps1
#>

param(
    [string]$SonarQubeUrl = "http://localhost:9000",
    [switch]$Help
)

# ============================================
# Help
# ============================================
if ($Help) {
    Get-Help -Name $MyInvocation.MyCommand.Source -Full
    exit 0
}

# ============================================
# Configuration
# ============================================
$ErrorActionPreference = "Stop"

$solutionFile = "Project.Base.sln"
$coverageDir = "./coverage"
$sonarPropsFile = "sonar-project.properties"

# ============================================
# Helper Functions
# ============================================
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# ============================================
# Step 1/3: Build Solution
# ============================================
Write-Host ""
Write-Info ("=" * 60)
Write-Info " Step 1/3: Building Solution"
Write-Info ("=" * 60)
Write-Host ""

Write-Info "Restoring dependencies..."
dotnet restore $solutionFile
if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Restore failed"
    exit 1
}
Write-Success "Restore completed"

Write-Info "Building solution..."
dotnet build $solutionFile --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Build failed"
    exit 1
}
Write-Success "Build completed"

# ============================================
# Step 2/3: SonarQube Analysis (begin → test → end)
# ============================================
Write-Host ""
Write-Info ("=" * 60)
Write-Info " Step 2/3: SonarQube Analysis"
Write-Info ("=" * 60)
Write-Host ""

# Create coverage directory
if (-not (Test-Path $coverageDir)) {
    New-Item -ItemType Directory -Path $coverageDir | Out-Null
}

# Check if SonarQube scanner is available
$scannerAvailable = $false
try {
    $scannerVersion = dotnet-sonarscanner --version 2>&1
    if ($scannerVersion) {
        $scannerAvailable = $true
    }
} catch {
    $scannerAvailable = $false
}

Write-Info "Starting SonarQube analysis..."
Write-Info "Target: $SonarQubeUrl"
Write-Host ""

# Step 2a: SonarQube begin (inicia a análise)
Write-Info "Step 2a: SonarQube begin..."
if ($env:SONAR_TOKEN -eq "" -or $null -eq $env:SONAR_TOKEN) {
    Write-Warning-Custom "No token provided, skipping SonarQube analysis"
    exit 0
}

dotnet-sonarscanner begin `
    /k:"Project.Base" `
    /d:sonar.host.url="$SonarQubeUrl" `
    /d:sonar.cs.cobertura.reportsPaths="coverage/Project.Base.Tests.cobertura.xml" `
    /d:sonar.token="$env:SONAR_TOKEN"

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "SonarQube begin step failed"
    exit 1
}

# Step 2b: Build (necessário para o scanner)
Write-Info "Step 2b: Building solution for SonarQube..."
dotnet build $solutionFile --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Build failed during SonarQube analysis"
    dotnet-sonarscanner end
    exit 1
}

# Step 2c: Run tests with coverage (gera o relatório de cobertura)
Write-Info "Step 2c: Running tests with coverage..."
dotnet test $solutionFile `
    --configuration Release `
    --no-build `
    --no-restore `
    --collect:"XPlat Code Coverage" `
    --results-directory $coverageDir `
    --logger "trx;LogFileName=test-results.trx"

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Tests failed during SonarQube analysis"
    dotnet-sonarscanner end
    exit 1
}
Write-Success "All tests passed"

# Step 2d: SonarQube end (envia dados para o SonarQube)
Write-Info "Step 2d: SonarQube end (uploading analysis)..."
dotnet-sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "SonarQube end step failed"
    exit 1
}

Write-Success "SonarQube analysis completed"
Write-Host ""
Write-Info "View results at: $SonarQubeUrl/dashboard?id=Project.Base"
Write-Host ""

# ============================================
# Step 3/3: Local Test Summary
# ============================================
Write-Host ""
Write-Info ("=" * 60)
Write-Info " Step 3/3: Test Summary"
Write-Info ("=" * 60)
Write-Host ""

Write-Info "Running tests locally for detailed summary..."
dotnet test $solutionFile `
    --configuration Release `
    --no-build `
    --no-restore `
    --logger "trx;LogFileName=test-results.trx"

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Tests failed in local summary"
    exit 1
}
Write-Success "All tests passed"
Write-Host ""

# ============================================
# Final Summary
# ============================================
Write-Host ""
Write-Host ("=" * 60)
Write-Success "✅ All validations passed!"
Write-Host ("=" * 60)
Write-Host ""
Write-Warning-Custom "Note: NuGet publishing is ONLY available via GitHub Actions workflow."
Write-Host "      To publish, push a tag (vX.Y.Z) to trigger the CI/CD pipeline."
Write-Host ""

exit 0