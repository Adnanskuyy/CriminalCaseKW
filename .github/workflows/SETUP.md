# CI/CD Setup Guide

This project uses a **self-hosted GitHub Actions runner** with **GameCI Docker** builds, automatically deploying WebGL builds to **GitHub Pages**.

---

## Architecture

```
Manual Trigger (workflow_dispatch) from V2 branch
    |
    v
Self-Hosted Runner (Your Windows PC)
    |- Docker Desktop
    |- Unity 6000.3.8f1 (installed locally, but GameCI uses its own Docker image)
    |
    v
GameCI unity-builder (Docker)
    |- Pulls unityci/editor:6000.3.8f1-webgl-...
    |- Builds WebGL
    |
    v
Upload Artifact
    |
    v
GitHub-Hosted Deploy Job
    |- Downloads artifact
    |- Deploys to GitHub Pages
```

---

## Prerequisites Completed

- [x] Docker Desktop installed and running
- [x] Unity 6000.3.8f1 installed at `C:\Program Files\Unity\Hub\Editor\6000.3.8f1`
- [x] GitHub Actions runner downloaded to `C:\actions-runner`
- [x] Runner registered and connected to `Adnanskuyy/CriminalCaseKW`
- [x] Runner labels: `self-hosted`, `windows`, `unity`, `docker`

---

## Required GitHub Secrets

Go to **Repository Settings -> Secrets and variables -> Actions -> New repository secret**

| Secret | Description | How to get it |
|--------|-------------|---------------|
| `UNITY_EMAIL` | Your Unity account email | Your Unity ID login email |
| `UNITY_PASSWORD` | Your Unity account password | Your Unity ID login password |
| `UNITY_LICENSE` | (Optional) Your Unity .ulf license file content | See License Activation section below |

> For **Personal licenses**, `UNITY_EMAIL` and `UNITY_PASSWORD` are usually sufficient. If automatic activation fails, use the manual license file method.

---

## License Activation (Personal License)

### Option A: Automatic (Recommended)
Just provide `UNITY_EMAIL` and `UNITY_PASSWORD` as secrets. GameCI will attempt to activate automatically on each build.

### Option B: Manual License File
If automatic activation fails:

1. Go to **Actions -> Request Unity Activation File -> Run workflow**
2. Download the generated `.alf` file from the artifact
3. Go to [license.unity3d.com](https://license.unity3d.com/manual)
4. Upload the `.alf` file and select **Unity Personal Editor**
5. Download the generated `.ulf` file
6. Open the `.ulf` file in a text editor, copy ALL the content
7. Create a new secret named `UNITY_LICENSE` and paste the content

---

## Branch Strategy

- **Current development branch:** `V2`
- **Deployment target:** GitHub Pages (from `V2` branch for testing)
- **Production branch:** `main` (merge after successful testing)

The workflow is configured for **manual triggers only** to prevent accidental deployments during active development.

---

## Enable GitHub Pages

1. Go to **Repository Settings -> Pages**
2. Under **Build and deployment**, select **Source: GitHub Actions**
3. Save

---

## Install Runner as Windows Service (Persistent)

The runner is currently running as a background process. To make it start automatically on boot:

1. Open **PowerShell as Administrator**
2. Run:
   ```powershell
   cd C:\actions-runner
   .\config.cmd --runasservice
   ```
   Or if already configured:
   ```powershell
   sc.exe create "GitHubActionsRunner" binPath= "C:\actions-runner\run.cmd" start= auto
   ```

Alternatively, just keep the current terminal open, or run `run.cmd` on startup via Task Scheduler.

---

## Important Notes

### GameCI Docker Image Availability
Unity `6000.3.8f1` is very new. GameCI Docker images are published community-side and may not be available immediately for this exact version. If the build fails with "image not found", you have two options:

1. **Wait** for GameCI to publish the image (track at [game-ci/docker](https://github.com/game-ci/docker))
2. **Fallback to direct CLI build** (no Docker) — see `build-deploy-direct.yml` alternative

### WebGL Compression
GitHub Pages serves files with standard MIME types. If you use Brotli compression in Unity WebGL builds, the server won't serve the correct `Content-Encoding` header. Either:
- Set WebGL compression to **Gzip** in Unity: `Edit -> Project Settings -> Player -> WebGL -> Publishing Settings -> Compression Format: Gzip`
- Or use **Disabled** compression (larger files but guaranteed to work)

### Build Time
First GameCI build will take **20-40 minutes** because it needs to pull the Unity Docker image (~8 GB). Subsequent builds will be faster as Docker caches the image layers.

---

## Workflows

### `build-deploy.yml`
**Manual trigger only** (`workflow_dispatch`). Used for testing builds from the `V2` branch before merging to `main`. Builds WebGL via GameCI Docker and deploys to GitHub Pages.

### `activate.yml`
Manual workflow to request a Unity activation file (`.alf`) for manual license generation.

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Docker daemon not running" | Start Docker Desktop manually |
| "Cannot connect to Docker" | Ensure Docker Desktop is using WSL2 backend |
| "Unity license activation failed" | Use manual activation (Option B) |
| "Image not found" for Unity version | GameCI image not yet published; use older Unity version or direct CLI build |
| Runner shows offline | Check if `Runner.Listener` process is running on your PC |
