param(
    [Parameter(Mandatory = $true)]
    [string]$MigrationName
)

$env:PersistenceSettings__Driver = "Sqlite"
dotnet ef migrations add $MigrationName `
    --project .\Hestia.Persistence\Hestia.Persistence.csproj `
    --context SqliteApplicationDbContext `
    --startup-project .\Hestia\Hestia.csproj `
    --output-dir .\Migrations\Sqlite
