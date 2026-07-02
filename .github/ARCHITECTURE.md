# GitHub Actions CI/CD Architecture

This document describes the new multi-environment CI/CD architecture for Client Extras.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                   GitHub Repository                          │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Branch Triggers                                       │  │
│  │  • dev / develop / feature/**  → DEV Pipeline         │  │
│  │  • main                        → PROD Pipeline        │  │
│  └───────────────────────────────────────────────────────┘  │
│                          ↓                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │         Caller Workflows (Entry Points)              │   │
│  │  • dev-ci-new.yml (DEV)                             │   │
│  │  • prod-ci-new.yml (PROD)                           │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ↓                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │    Reusable Workflow (reusable-ci.yml)              │   │
│  │  • Parameterized build and deploy logic             │   │
│  │  • Accepts: environment, branch, S3 folder          │   │
│  └────────────────────────────────��─────────────────────┘   │
│                          ↓                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │    Build Steps (Shared)                              │   │
│  │  • Checkout → Build → Publish → ZIP → Upload to S3  │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ↓                                    │
│           ┌─────────────────────────┐                        │
│           │  AWS S3 Deployment      │                        │
│           │  • s3://bucket/dev/     │                        │
│           │  • s3://bucket/staging/ │                        │
│           │  • s3://bucket/prod/    │                        │
│           └─────────────────────────┘                        │
└─────────────────────────────────────────────────────────────┘
```

## Workflow Files

### 1. **reusable-ci.yml** (Core Workflow)
- **Type**: Reusable workflow (can be called by other workflows)
- **Purpose**: Single source of truth for build logic
- **Inputs**:
  - `environment_name`: Environment identifier (dev, staging, prod)
  - `branch_ref`: Git branch to checkout
  - `s3_folder`: S3 destination folder
- **Benefits**:
  - DRY (Don't Repeat Yourself) - no code duplication
  - Easy to maintain and update build logic in one place
  - Consistent build process across all environments

### 2. **dev-ci-new.yml** (Development Entry Point)
- **Triggers**: 
  - Push to `dev`, `develop`, or `feature/**` branches
  - Pull requests to `dev` or `develop`
  - Manual workflow dispatch with optional branch input
- **Calls**: `reusable-ci.yml` with DEV parameters
- **Environment**: dev
- **S3 Destination**: `s3://mybucketp03/ClientExtras/dev/`

### 3. **prod-ci-new.yml** (Production Entry Point)
- **Triggers**: 
  - Push to `main` branch (typically after PR merge)
  - Manual workflow dispatch
- **Calls**: `reusable-ci.yml` with PROD parameters
- **Environment**: prod (requires approval if set in GitHub settings)
- **S3 Destination**: `s3://mybucketp03/ClientExtras/prod/`

## Usage Scenarios

### Scenario 1: Develop Feature
```bash
# Push to feature branch
git push origin feature/my-feature

# ✅ dev-ci-new.yml automatically triggers
# ✅ Builds and deploys to s3://bucket/ClientExtras/dev/
```

### Scenario 2: Merge to Main for Production
```bash
# Create PR from develop to main
# After review and approval, merge

# ✅ prod-ci-new.yml automatically triggers
# ✅ Builds from main and deploys to s3://bucket/ClientExtras/prod/
```

### Scenario 3: Manual Deployment (DEV)
```bash
# Go to GitHub → Actions → DEV CI Build - Dynamic Branch Support
# Click "Run workflow"
# Optionally specify a branch
# ✅ Deploys specified branch to DEV environment
```

### Scenario 4: Manual Deployment (PROD)
```bash
# Go to GitHub → Actions → PROD CI Build - Main Branch Deploy
# Click "Run workflow"
# ✅ Deploys main branch to PROD environment
```

## Environment Configuration

### DEV Environment
- **Branch**: dev, develop, feature/*
- **Approval**: None required (optional)
- **Secrets**: Uses repository secrets
- **S3 Path**: `ClientExtras/dev/`

### PROD Environment
- **Branch**: main only
- **Approval**: **RECOMMENDED** - Set in GitHub repository settings
- **Secrets**: Can use environment-specific secrets
- **S3 Path**: `ClientExtras/prod/`

## Setting Up Environment Protection (Recommended for PROD)

1. Go to **Settings** → **Environments** → **prod**
2. Enable **Required reviewers**
3. Add team members who must approve production deployments
4. Optionally set **Deployment branches** to `main` only

## Build Output Structure

Each build creates a timestamped ZIP file:
```
ClientExtras_{ENVIRONMENT}_{TIMESTAMP}.zip
Example: ClientExtras_dev_20260702_091125.zip
Example: ClientExtras_prod_20260702_091125.zip
```

## Timestamp Format (IST - Indian Standard Time)
```
yyyyMMdd_HHmmss
Example: 20260702_091125 (July 2, 2026 at 09:11:25 AM IST)
```

## Environment Variables

### Available in All Workflows
- `CONFIGURATION`: Release
- `PROJECT_NAME`: ClientExtras
- `S3_BUCKET`: mybucketp03
- `BUILD_VERSION`: Auto-generated timestamp (IST)

### Input Parameters (Set by Caller Workflow)
- `environment_name`: DEV or PROD
- `branch_ref`: Branch to checkout
- `s3_folder`: S3 destination

## Migration from Old Workflows

### Old Setup (Before)
- `dev-ci.yml` - hardcoded to dev branch
- `prod-ci.yml` - hardcoded to main branch
- Duplicated build logic

### New Setup (After)
- `dev-ci-new.yml` - supports multiple branches (dev, develop, feature/*)
- `prod-ci-new.yml` - triggers on main branch merge
- `reusable-ci.yml` - single source of truth for build logic
- ✅ Flexible branch patterns
- ✅ No code duplication
- ✅ Easy to add new environments (staging, qa, etc.)

## Adding a New Environment (e.g., Staging)

1. Create `.github/workflows/staging-ci-new.yml`:
```yaml
name: STAGING CI Build

on:
  push:
    branches:
      - staging
  workflow_dispatch:

jobs:
  staging-build:
    name: Build and Deploy to STAGING
    uses: ./.github/workflows/reusable-ci.yml
    with:
      environment_name: 'staging'
      branch_ref: 'staging'
      s3_folder: 'staging'
    secrets:
      AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
      AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
      AWS_REGION: ${{ secrets.AWS_REGION }}
```

2. Set up environment in GitHub settings (if needed)
3. Done! ✅

## Troubleshooting

### Workflow doesn't trigger
- Check branch name matches trigger pattern
- Verify `.github/workflows/` files are committed to the branch
- Check repository secrets are set

### Build fails
- Check .NET version compatibility
- Verify S3 credentials are valid
- Check AWS region setting

### Environment approval not working
- Go to Settings → Environments → prod
- Ensure "Required reviewers" is enabled
- Check team members have proper permissions

## Next Steps

1. ✅ Commit these new workflow files
2. ✅ Keep old workflows (dev-ci.yml, prod-ci.yml) for now - they'll coexist
3. ✅ Test the new workflows with a feature branch
4. ✅ After validation, optionally delete old workflows
5. ✅ Configure environment protection for PROD (Settings → Environments)
