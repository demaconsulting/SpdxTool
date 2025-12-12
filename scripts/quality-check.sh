#!/bin/bash
# Quality check script for local development
# This script runs all quality checks before committing code

set -e

echo "=================================================="
echo "SpdxTool Quality Check"
echo "=================================================="
echo ""

# Configuration
# Target framework for self-validation (LTS version recommended)
TARGET_FRAMEWORK="${SPDX_TARGET_FRAMEWORK:-net8.0}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Create unique temporary directory for this run
TEMP_DIR=$(mktemp -d -t spdx-quality-XXXXXX)
trap "rm -rf $TEMP_DIR" EXIT

# Track if any checks fail
FAILED=0

# Function to print status
print_status() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓${NC} $2"
    else
        echo -e "${RED}✗${NC} $2"
        FAILED=1
    fi
}

echo "Step 1: Cleaning build artifacts..."
dotnet clean > /dev/null 2>&1
print_status $? "Clean completed"
echo ""

echo "Step 2: Restoring dependencies..."
dotnet restore > /dev/null 2>&1
print_status $? "Restore completed"
echo ""

echo "Step 3: Building solution..."
if dotnet build --no-restore 2>&1 | tee "$TEMP_DIR/build.log" | grep -q "Build FAILED"; then
    echo -e "${RED}Build failed. See output above for details.${NC}"
    FAILED=1
else
    print_status 0 "Build completed"
fi
echo ""

echo "Step 4: Running tests..."
dotnet test --no-build --verbosity quiet > "$TEMP_DIR/test.log" 2>&1
TEST_EXIT=$?
if [ $TEST_EXIT -eq 0 ]; then
    print_status 0 "All tests passed"
else
    echo -e "${RED}Tests failed. Exit code: $TEST_EXIT${NC}"
    cat "$TEMP_DIR/test.log"
    FAILED=1
fi
echo ""

echo "Step 5: Running code analysis..."
# Build already runs analysis, so we just check for warnings
if grep -q "warning" "$TEMP_DIR/build.log"; then
    echo -e "${YELLOW}⚠${NC} Warnings found in build output"
    grep "warning" "$TEMP_DIR/build.log" | head -5
else
    print_status 0 "No analysis warnings"
fi
echo ""

echo "Step 6: Running self-validation..."
# Note: Using configured framework (default: net8.0 LTS) for self-validation.
# Self-validation behavior is identical across frameworks.
# Override with: SPDX_TARGET_FRAMEWORK=net9.0 ./scripts/quality-check.sh
if dotnet run --project src/DemaConsulting.SpdxTool --no-build --framework "$TARGET_FRAMEWORK" -- --validate > "$TEMP_DIR/validate.log" 2>&1; then
    if grep -q "Validation Passed" "$TEMP_DIR/validate.log"; then
        print_status 0 "Self-validation passed (framework: $TARGET_FRAMEWORK)"
    else
        echo -e "${RED}Self-validation failed${NC}"
        cat "$TEMP_DIR/validate.log"
        FAILED=1
    fi
else
    echo -e "${RED}Self-validation failed${NC}"
    cat "$TEMP_DIR/validate.log"
    FAILED=1
fi
echo ""

echo "=================================================="
if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}All quality checks passed!${NC}"
    echo "You're ready to commit your changes."
    exit 0
else
    echo -e "${RED}Some quality checks failed.${NC}"
    echo "Please fix the issues before committing."
    exit 1
fi
