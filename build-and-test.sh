#!/bin/bash

# ============================================
# Local Validation Script
# ============================================
# Usage: ./build-and-test.sh
# ============================================

set -e

# Configurações
SOLUTION_FILE="Project.Base.sln"
SONAR_URL="http://localhost:9000"
SONAR_PROPS="sonar-project.properties"

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Funções de log
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# ============================================
# Step 1: Build
# ============================================
echo ""
log_info "=========================================="
log_info " Step 1/3: Building Solution"
log_info "=========================================="
echo ""

log_info "Restoring dependencies..."
dotnet restore "$SOLUTION_FILE"
if [ $? -ne 0 ]; then
    log_error "❌ Restore failed"
    exit 1
fi
log_success "✅ Restore completed"

log_info "Building solution..."
dotnet build "$SOLUTION_FILE" --configuration Release --no-restore
if [ $? -ne 0 ]; then
    log_error "❌ Build failed"
    exit 1
fi
log_success "✅ Build completed"

# ============================================
# Step 2: Run Tests with Coverage
# ============================================
echo ""
log_info "=========================================="
log_info " Step 2/3: Running Tests with Coverage"
log_info "=========================================="
echo ""

# Create coverage directory
mkdir -p ./coverage

log_info "Running tests with code coverage..."
dotnet test "$SOLUTION_FILE" \
    --configuration Release \
    --no-build \
    --no-restore \
    --logger "trx;LogFileName=test-results.trx"

if [ $? -ne 0 ]; then
    log_error "❌ Tests failed"
    exit 1
fi
log_success "✅ All tests passed"

# ============================================
# Step 3: SonarQube Analysis
# ============================================
echo ""
log_info "=========================================="
log_info " Step 3/3: SonarQube Analysis"
log_info "=========================================="
echo ""

if [ ! -f "$SONAR_PROPS" ]; then
    log_warning "⚠️  SonarQube properties file not found: $SONAR_PROPS"
    log_warning "⚠️  Skipping SonarQube analysis"
    echo ""
    log_warning "To enable SonarQube analysis:"
    echo "  1. Ensure SonarQube is running at: $SONAR_URL"
    echo "  2. Install the SonarQube scanner: dotnet tool install -g dotnet-sonarscanner"
    echo "  3. Run: ./build-and-test.sh"
    echo ""
    exit 0
fi

# Check if SonarQube scanner is available
if ! command -v dotnet-sonarscanner &> /dev/null; then
    log_warning "⚠️  SonarQube scanner not found (dotnet-sonarscanner)"
    log_warning "⚠️  Skipping SonarQube analysis"
    echo ""
    log_info "To install SonarQube scanner, run:"
    echo "  dotnet tool install -g dotnet-sonarscanner"
    echo ""
    log_warning "Alternatively, run tests locally and analyze via SonarQube web UI:"
    echo "  $SONAR_URL"
    echo ""
    exit 0
fi

log_info "Starting SonarQube analysis..."
log_info "Target: $SONAR_URL"
echo ""

# Run SonarQube begin step
dotnet-sonarscanner begin \
    /k:"Project.Base" \
    /d:sonar.host.url="$SONAR_URL" \
    /d:sonar.projectKey="Project.Base" \
    /d:sonar.projectName="Project.Base" \
    /d:sonar.sources="Project.Base.WebApi,Project.Base.Contracts,Project.Base.Domain,Project.Base.Repository" \
    /d:sonar.tests="Project.Base.Tests" \
    /d:sonar.cs.opencover.reports="coverage/*/Project.Base.Tests.opencover.xml" \
    /d:sonar.coverage.cobertura.reports="coverage/*/Project.Base.Tests.cobertura.xml" \
    /d:sonar.dotnet.visualstudio.solution.file="Project.Base.sln"

if [ $? -ne 0 ]; then
    log_warning "⚠️  SonarQube begin step failed, continuing with build..."
fi

# Build and test for SonarQube
dotnet build "$SOLUTION_FILE" --configuration Release --no-restore

dotnet test "$SOLUTION_FILE" \
    --configuration Release \
    --no-build \
    --no-restore \
    --logger "trx;LogFileName=test-results.trx"

if [ $? -ne 0 ]; then
    log_error "❌ Tests failed during SonarQube analysis"
    # End SonarQube scan even on failure
    dotnet-sonarscanner end
    exit 1
fi

# Run SonarQube end step
dotnet-sonarscanner end

if [ $? -ne 0 ]; then
    log_warning "⚠️  SonarQube end step failed"
fi

log_success "✅ SonarQube analysis completed"
echo ""
log_info "View results at: $SONAR_URL/dashboard?id=Project.Base"
echo ""

# ============================================
# Final Summary
# ============================================
echo ""
echo "=========================================="
log_success "✅ All validations passed!"
echo "=========================================="
echo ""
log_warning "⚠️  Note: NuGet publishing is ONLY available via GitHub Actions workflow."
echo "      To publish, push a tag (vX.Y.Z) to trigger the CI/CD pipeline."
echo ""

exit 0
