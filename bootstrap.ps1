$ErrorActionPreference = "Stop"

Write-Host "========================================"
Write-Host "   ORDER FLOW - PROJECT BOOTSTRAP"
Write-Host "========================================"

# ========================================
# 1. Create directories
# ========================================

Write-Host "`n[1/8] Creating directories..."

$directories = @(
    "src\Orders",
    "src\Inventory",
    "src\Payments",
    "src\Contracts",
    "src\Client",
    "tests",
    "deploy\postgres",
    "docs\architecture"
)

foreach ($dir in $directories) {
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
}

# ========================================
# 2. Create solution
# ========================================

Write-Host "`n[2/8] Creating solution..."

dotnet new sln -n OrderFlow --format slnx

# ========================================
# 3. Orders
# ========================================

Write-Host "`n[3/8] Creating Orders service..."

dotnet new classlib -n Orders.Domain `
    -o src\Orders\Orders.Domain

dotnet new classlib -n Orders.Application `
    -o src\Orders\Orders.Application

dotnet new classlib -n Orders.Infrastructure `
    -o src\Orders\Orders.Infrastructure

dotnet new webapi -n Orders.Api `
    -o src\Orders\Orders.Api `
    --use-controllers

# ========================================
# 4. Inventory
# ========================================

Write-Host "`n[4/8] Creating Inventory service..."

dotnet new classlib -n Inventory.Domain `
    -o src\Inventory\Inventory.Domain

dotnet new classlib -n Inventory.Application `
    -o src\Inventory\Inventory.Application

dotnet new classlib -n Inventory.Infrastructure `
    -o src\Inventory\Inventory.Infrastructure

dotnet new webapi -n Inventory.Api `
    -o src\Inventory\Inventory.Api `
    --use-controllers

# ========================================
# 5. Payments
# ========================================

Write-Host "`n[5/8] Creating Payments service..."

dotnet new classlib -n Payments.Domain `
    -o src\Payments\Payments.Domain

dotnet new classlib -n Payments.Application `
    -o src\Payments\Payments.Application

dotnet new classlib -n Payments.Infrastructure `
    -o src\Payments\Payments.Infrastructure

dotnet new webapi -n Payments.Api `
    -o src\Payments\Payments.Api `
    --use-controllers

# ========================================
# 6. Contracts / Client / Tests
# ========================================

Write-Host "`n[6/8] Creating Contracts, Client and Tests..."

dotnet new classlib `
    -n OrderFlow.Contracts `
    -o src\Contracts\OrderFlow.Contracts

dotnet new blazorwasm `
    -n OrderFlow.Client `
    -o src\Client\OrderFlow.Client

dotnet new xunit `
    -n Orders.UnitTests `
    -o tests\Orders.UnitTests

dotnet new xunit `
    -n Inventory.UnitTests `
    -o tests\Inventory.UnitTests

dotnet new xunit `
    -n Payments.UnitTests `
    -o tests\Payments.UnitTests

dotnet new xunit `
    -n Architecture.Tests `
    -o tests\Architecture.Tests

# ========================================
# 7. Add projects to solution
# ========================================

Write-Host "`n[7/8] Adding projects to solution..."

dotnet sln add `
    src\Orders\Orders.Domain\Orders.Domain.csproj `
    src\Orders\Orders.Application\Orders.Application.csproj `
    src\Orders\Orders.Infrastructure\Orders.Infrastructure.csproj `
    src\Orders\Orders.Api\Orders.Api.csproj `
    src\Inventory\Inventory.Domain\Inventory.Domain.csproj `
    src\Inventory\Inventory.Application\Inventory.Application.csproj `
    src\Inventory\Inventory.Infrastructure\Inventory.Infrastructure.csproj `
    src\Inventory\Inventory.Api\Inventory.Api.csproj `
    src\Payments\Payments.Domain\Payments.Domain.csproj `
    src\Payments\Payments.Application\Payments.Application.csproj `
    src\Payments\Payments.Infrastructure\Payments.Infrastructure.csproj `
    src\Payments\Payments.Api\Payments.Api.csproj `
    src\Contracts\OrderFlow.Contracts\OrderFlow.Contracts.csproj `
    src\Client\OrderFlow.Client\OrderFlow.Client.csproj `
    tests\Orders.UnitTests\Orders.UnitTests.csproj `
    tests\Inventory.UnitTests\Inventory.UnitTests.csproj `
    tests\Payments.UnitTests\Payments.UnitTests.csproj `
    tests\Architecture.Tests\Architecture.Tests.csproj

# ========================================
# 8. Project references
# ========================================

Write-Host "`n[8/8] Creating project references..."

# ---------- Orders ----------

dotnet add src\Orders\Orders.Application\Orders.Application.csproj `
    reference src\Orders\Orders.Domain\Orders.Domain.csproj

dotnet add src\Orders\Orders.Application\Orders.Application.csproj `
    reference src\Contracts\OrderFlow.Contracts\OrderFlow.Contracts.csproj

dotnet add src\Orders\Orders.Infrastructure\Orders.Infrastructure.csproj `
    reference `
    src\Orders\Orders.Application\Orders.Application.csproj `
    src\Orders\Orders.Domain\Orders.Domain.csproj `
    src\Contracts\OrderFlow.Contracts\OrderFlow.Contracts.csproj

dotnet add src\Orders\Orders.Api\Orders.Api.csproj `
    reference `
    src\Orders\Orders.Application\Orders.Application.csproj `
    src\Orders\Orders.Infrastructure\Orders.Infrastructure.csproj

# ---------- Inventory ----------

dotnet add src\Inventory\Inventory.Application\Inventory.Application.csproj `
    reference src\Inventory\Inventory.Domain\Inventory.Domain.csproj

dotnet add src\Inventory\Inventory.Application\Inventory.Application.csproj `
    reference src\Contracts\OrderFlow.Contracts\OrderFlow.Contracts.csproj

dotnet add src\Inventory\Inventory.Infrastructure\Inventory.Infrastructure.csproj `
    reference `
    src\Inventory\Inventory.Application\Inventory.Application.csproj `
    src\Inventory\Inventory.Domain\Inventory.Domain.csproj `
    src\Contracts\OrderFlow.Contracts\OrderFlow.Contracts.csproj

dotnet add src\Inventory\Inventory.Api\Inventory.Api.csproj `
    reference `
    src\Inventory\Inventory.Application\Inventory.Application.csproj `
    src\Inventory\Inventory.Infrastructure\Inventory.Infrastructure.csproj

# ---------- Payments ----------

dotnet add src\Payments\Payments.Application\Payments.Application.csproj `
    reference src\Payments\Payments.Domain\Payments.Domain.csproj

dotnet add src\Payments\Payments.Application\Payments.Application.csproj `
    reference src\Contracts\OrderFlow.Contracts\OrderFlow.Contracts.csproj

dotnet add src\Payments\Payments.Infrastructure\Payments.Infrastructure.csproj `
    reference `
    src\Payments\Payments.Application\Payments.Application.csproj `
    src\Payments\Payments.Domain\Payments.Domain.csproj `
    src\Contracts\OrderFlow.Contracts\OrderFlow.Contracts.csproj

dotnet add src\Payments\Payments.Api\Payments.Api.csproj `
    reference `
    src\Payments\Payments.Application\Payments.Application.csproj `
    src\Payments\Payments.Infrastructure\Payments.Infrastructure.csproj

# ---------- Tests ----------

dotnet add tests\Orders.UnitTests\Orders.UnitTests.csproj `
    reference `
    src\Orders\Orders.Domain\Orders.Domain.csproj `
    src\Orders\Orders.Application\Orders.Application.csproj

dotnet add tests\Inventory.UnitTests\Inventory.UnitTests.csproj `
    reference `
    src\Inventory\Inventory.Domain\Inventory.Domain.csproj `
    src\Inventory\Inventory.Application\Inventory.Application.csproj

dotnet add tests\Payments.UnitTests\Payments.UnitTests.csproj `
    reference `
    src\Payments\Payments.Domain\Payments.Domain.csproj `
    src\Payments\Payments.Application\Payments.Application.csproj

# ========================================
# Clean Architecture folders
# ========================================

Write-Host "`nCreating Clean Architecture folders..."

$folders = @(
    # Orders
    "src\Orders\Orders.Domain\Entities",
    "src\Orders\Orders.Domain\Enums",
    "src\Orders\Orders.Domain\Exceptions",

    "src\Orders\Orders.Application\Abstractions",
    "src\Orders\Orders.Application\Commands",
    "src\Orders\Orders.Application\Queries",
    "src\Orders\Orders.Application\DTOs",
    "src\Orders\Orders.Application\Handlers",

    "src\Orders\Orders.Infrastructure\Persistence",
    "src\Orders\Orders.Infrastructure\Persistence\Configurations",
    "src\Orders\Orders.Infrastructure\Repositories",
    "src\Orders\Orders.Infrastructure\Messaging",
    "src\Orders\Orders.Infrastructure\Outbox",
    "src\Orders\Orders.Infrastructure\Inbox",
    "src\Orders\Orders.Infrastructure\Health",

    "src\Orders\Orders.Api\Controllers",
    "src\Orders\Orders.Api\Middleware",
    "src\Orders\Orders.Api\Extensions",

    # Inventory
    "src\Inventory\Inventory.Domain\Entities",
    "src\Inventory\Inventory.Domain\Enums",
    "src\Inventory\Inventory.Domain\Exceptions",

    "src\Inventory\Inventory.Application\Abstractions",
    "src\Inventory\Inventory.Application\Commands",
    "src\Inventory\Inventory.Application\Queries",
    "src\Inventory\Inventory.Application\DTOs",
    "src\Inventory\Inventory.Application\Handlers",

    "src\Inventory\Inventory.Infrastructure\Persistence",
    "src\Inventory\Inventory.Infrastructure\Persistence\Configurations",
    "src\Inventory\Inventory.Infrastructure\Repositories",
    "src\Inventory\Inventory.Infrastructure\Messaging",
    "src\Inventory\Inventory.Infrastructure\Outbox",
    "src\Inventory\Inventory.Infrastructure\Inbox",
    "src\Inventory\Inventory.Infrastructure\Health",

    "src\Inventory\Inventory.Api\Controllers",
    "src\Inventory\Inventory.Api\Middleware",
    "src\Inventory\Inventory.Api\Extensions",

    # Payments
    "src\Payments\Payments.Domain\Entities",
    "src\Payments\Payments.Domain\Enums",
    "src\Payments\Payments.Domain\Exceptions",

    "src\Payments\Payments.Application\Abstractions",
    "src\Payments\Payments.Application\Commands",
    "src\Payments\Payments.Application\Queries",
    "src\Payments\Payments.Application\DTOs",
    "src\Payments\Payments.Application\Handlers",

    "src\Payments\Payments.Infrastructure\Persistence",
    "src\Payments\Payments.Infrastructure\Persistence\Configurations",
    "src\Payments\Payments.Infrastructure\Repositories",
    "src\Payments\Payments.Infrastructure\Messaging",
    "src\Payments\Payments.Infrastructure\Outbox",
    "src\Payments\Payments.Infrastructure\Inbox",
    "src\Payments\Payments.Infrastructure\Health",

    "src\Payments\Payments.Api\Controllers",
    "src\Payments\Payments.Api\Middleware",
    "src\Payments\Payments.Api\Extensions",

    # Contracts
    "src\Contracts\OrderFlow.Contracts\Events",

    # Client
    "src\Client\OrderFlow.Client\Pages",
    "src\Client\OrderFlow.Client\Services",
    "src\Client\OrderFlow.Client\Models",

    # Tests
    "tests\Orders.UnitTests\Domain",
    "tests\Orders.UnitTests\Application",
    "tests\Inventory.UnitTests\Domain",
    "tests\Inventory.UnitTests\Application",
    "tests\Payments.UnitTests\Domain",
    "tests\Payments.UnitTests\Application",
    "tests\Architecture.Tests"
)

foreach ($folder in $folders) {
    New-Item -ItemType Directory -Force -Path $folder | Out-Null
}

# ========================================
# Placeholder files
# ========================================

Write-Host "`nCreating placeholder files..."

$gitkeepFolders = @(
    $folders
)

foreach ($folder in $gitkeepFolders) {
    New-Item -ItemType File -Force -Path "$folder\.gitkeep" | Out-Null
}

# ========================================
# Infrastructure / docs files
# ========================================

New-Item -ItemType File -Force `
    -Path "deploy\docker-compose.yml" | Out-Null

New-Item -ItemType File -Force `
    -Path "docs\architecture\saga-flow.md" | Out-Null

# ========================================
# README
# ========================================

@"
# OrderFlow

Order fulfilment microservices coding challenge.

## Services

- Orders
- Inventory
- Payments
- Blazor WebAssembly Client

## Architecture

Each backend service follows Clean Architecture:

- Domain
- Application
- Infrastructure
- API

## Messaging

Apache Pulsar is used for communication between services.

The Saga is choreographed:

Orders
-> Inventory
-> Payments
-> Orders

## Patterns

- Choreographed Saga
- Transactional Outbox
- Inbox / Deduplication
- CQRS
- Repository / Unit of Work
- Dead Letter Queue

## Database

Each service owns its own database.

- orderflow_orders
- orderflow_inventory
- orderflow_payments

## Stock consumption

After successful payment, Inventory permanently consumes the reserved stock.

## Ports

- Client: 5000
- Orders: 5001
- Inventory: 5002
- Payments: 5003
"@ | Set-Content README.md

# ========================================
# Directory.Build.props
# ========================================

@"
<Project>

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
"@ | Set-Content Directory.Build.props

# ========================================
# Gitignore
# ========================================

@"
bin/
obj/
.vs/
.vscode/
.idea/

*.user
*.suo

TestResults/
coverage/

appsettings.Development.json

.env
.env.*

"@ | Set-Content .gitignore

# ========================================
# Git
# ========================================

Write-Host "`nInitializing Git repository..."

if (-not (Test-Path ".git")) {
    git init
}

git add .

git commit -m "chore: bootstrap OrderFlow"

# ========================================
# Build
# ========================================

Write-Host "`nBuilding solution..."

dotnet build OrderFlow.slnx

Write-Host ""
Write-Host "========================================"
Write-Host "   ORDER FLOW BOOTSTRAP COMPLETED"
Write-Host "========================================"

Write-Host ""
Write-Host "Run:"
Write-Host "  cd OrderFlow"
Write-Host "  dotnet build"

Write-Host ""
Write-Host "Git:"
Write-Host "  git status"
Write-Host "  git log --oneline"