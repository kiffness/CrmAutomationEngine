# CRM Automation Engine

A multi-tenant CRM automation platform that integrates with HubSpot to trigger email campaigns based on CRM events. Consists of a central ASP.NET Core server and a WPF desktop client.

```
HubSpot ──webhook──▶ Server (ASP.NET Core + Hangfire) ──▶ SendGrid
                              ▲
                              │ REST API
                              ▼
                       WPF Desktop App
```

## Features

- **Webhook ingestion** — receives HubSpot events (contact created, deal stage changed), validates HMAC signatures, and enrols contacts into automations
- **Email automation** — fires SendGrid emails immediately or with a configurable delay using Hangfire
- **Contact sync** — daily Hangfire job pulls all contacts from HubSpot and upserts them locally
- **Template manager** — create HTML email templates with `{{firstName}}`, `{{lastName}}`, `{{email}}`, `{{companyName}}` placeholders
- **Multi-tenancy** — pool model; single deployment serves multiple tenants, fully isolated via EF Core global query filters
- **Desktop client** — WPF app to manage templates, automations, contacts, and monitor the job queue

## Stack

| Layer | Technology |
|---|---|
| API / Server | ASP.NET Core 9 |
| Desktop Client | WPF (.NET 9) |
| Database | EF Core + PostgreSQL |
| Background Jobs | Hangfire + Hangfire.PostgreSql |
| Email | SendGrid |
| CRM | HubSpot API (private app token) |
| Auth | JWT |
| Reverse Proxy | Caddy (automatic HTTPS) |
| Containers | Docker + Docker Compose |
| Installer | WiX v4 (MSI) |

## Project Structure

```
CrmAutomationEngine/
├── src/
│   ├── CrmAutomationEngine.Core          # Domain entities, interfaces, enums
│   ├── CrmAutomationEngine.Infrastructure # EF Core, HubSpot client, SendGrid
│   ├── CrmAutomationEngine.Server        # ASP.NET Core API + Hangfire jobs
│   ├── CrmAutomationEngine.Desktop       # WPF desktop app
│   └── CrmAutomationEngine.Installer     # WiX v4 MSI installer
├── Dockerfile
├── docker-compose.yml
└── .github/workflows/
    ├── deploy.yml    # deploys server on push to release/live
    └── release.yml   # builds and publishes MSI on version tag
```

## Server Deployment

### Prerequisites

- VPS with Docker and a running Caddy container on a shared Docker network named `proxy`
- A self-hosted GitHub Actions runner registered on the VPS
- HubSpot developer account with a private app token per tenant

### First deploy

**1. Create the shared proxy network** (if it doesn't exist):
```bash
docker network create proxy
```

**2. Add the reverse proxy entry to your Caddyfile:**
```
relay.yourdomain.com {
    reverse_proxy crm-server:8080
}
```

**3. Create `/home/runner/crm/.env`** on the VPS:
```env
DB_PASSWORD=a-strong-password
JWT_KEY=a-long-random-secret-at-least-32-chars
SENDGRID_API_KEY=SG.xxxx
SENDGRID_FROM_EMAIL=noreply@yourdomain.com

# First-run tenant seeding — clear these after first deploy
SEED_TENANT_NAME=My Company
SEED_ADMIN_EMAIL=admin@example.com
SEED_ADMIN_PASSWORD=a-strong-password
SEED_HUBSPOT_TOKEN=
SEED_HUBSPOT_CLIENT_SECRET=
```

**4. Push to `release/live`** — the GitHub Actions workflow copies the repo and runs `docker compose up -d --build`. On first startup the server auto-runs EF Core migrations and seeds the first tenant.

### Subsequent deploys

```bash
git push origin release/live
```

### HubSpot webhook URL

Configure your HubSpot webhook subscription to send events to:
```
https://relay.yourdomain.com/api/webhook/{apiKey}
```

The `apiKey` is auto-generated for the tenant on first seed and stored in the `Tenants` table.

## Desktop App

### Installation

Download the latest `.msi` from [GitHub Releases](../../releases) and run the installer. It will prompt for the server URL during setup and write it to the app's config file.

### First login

After installing, edit `appsettings.json` in the install directory and paste the JWT token returned by:

```http
POST https://relay.yourdomain.com/api/auth/login
Content-Type: application/json

{ "email": "admin@example.com", "password": "your-password" }
```

## Releasing a new desktop version

Tag the commit and push — GitHub Actions builds the MSI and attaches it to a GitHub Release automatically:

```bash
git tag v1.0.0
git push origin v1.0.0
```

## Local Development

**Server:**
```bash
# requires a local Postgres instance
dotnet run --project src/CrmAutomationEngine.Server
```

**Desktop:**
```bash
dotnet run --project src/CrmAutomationEngine.Desktop
```

Update `src/CrmAutomationEngine.Desktop/appsettings.json` with your local server URL and a valid JWT token before running.
