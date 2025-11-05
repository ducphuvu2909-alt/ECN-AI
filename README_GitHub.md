# ECN Manager (ASP.NET Core 8 + JWT) — IIS Ready

## Quick Start (Local IIS)
1. Install .NET 8 Hosting Bundle.
2. Publish:
   ```
   dotnet restore
   dotnet publish -c Release -o ./publish
   ```
3. In IIS, point the site/app to the **publish** folder.
4. Browse: `http://localhost/<Alias>/index.html`

## GitHub Actions
- Workflow `.github/workflows/dotnet-publish.yml` builds and uploads the **publish** artifact.
- On every push to **main**, go to Actions → download artifact `ecnmanager-publish` and deploy to IIS.

## Demo logins
- Admin: U004 / bao
- Editor: U001 / minh
- Approver: U002 / lan
- Viewer: U003 / quang

> Change `appsettings.json → Jwt:Key` to a long random secret before production.
