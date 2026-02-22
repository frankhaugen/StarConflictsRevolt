# Tools

## Local setup

Run once from repo root:

**Windows (PowerShell):**
```powershell
./tools/setup-githooks.ps1
```

**Unix / Git Bash:**
```bash
./tools/setup-githooks.sh
```

Requires .NET SDK for the generator. To revert hooks: `git config --unset core.hooksPath`

---

## generate-slnx.cs

Keeps `.slnx` in sync with Deployment/, docs/, and root files. Run manually:

```bash
dotnet run --file tools/generate-slnx.cs
```

The pre-commit hook runs this when relevant paths are staged; use `--no-verify` to skip once.

**Expected output shape:**

```xml
<Solution>
  <Project Path="MyProject/MyProject.csproj" />
  <Folder Name="/Deployment/">
    <File Path="Deployment/azResource.bicep" />
  </Folder>
  <Folder Name="/Solution Items/">
    <File Path=".gitignore" />
    <File Path="readme.md" />
  </Folder>
  <Folder Name="/Docs/">
    <File Path="docs/README.md" />
  </Folder>
  <Folder Name="/Docs/api/">
    <File Path="docs/api/ENDPOINTS.md" />
  </Folder>
</Solution>
```
