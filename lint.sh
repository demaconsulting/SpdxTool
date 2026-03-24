#!/bin/bash

# Comprehensive Linting Script
# 
# PURPOSE:
# - Run ALL lint checks when executed (no options or modes)
# - Output lint failures directly for agent parsing
# - NO command-line arguments, pretty printing, or colorization
# - Agents execute this script to identify files needing fixes

LINT_ERROR=0

# Install npm dependencies
npm install

# Create Python virtual environment (for yamllint)
if [ ! -d ".venv" ]; then
  python -m venv .venv
fi
source .venv/bin/activate
pip install -r pip-requirements.txt

# Run spell check
npx cspell --no-progress --no-color "**/*.{md,yaml,yml,json,cs,cpp,hpp,h,txt}" || LINT_ERROR=1

# Run markdownlint check
npx markdownlint-cli2 "**/*.md" || LINT_ERROR=1

# Run yamllint check
yamllint . || LINT_ERROR=1

# Run .NET formatting check (verifies no changes are needed)
dotnet format --verify-no-changes || LINT_ERROR=1

exit $LINT_ERROR
