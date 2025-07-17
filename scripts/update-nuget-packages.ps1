# PowerShell script to update all NuGet packages in all projects
# This script finds all .csproj files and updates their NuGet packages to the latest versions

Write-Host "Starting NuGet package update for all projects..." -ForegroundColor Green

# Get all .csproj files in the solution (go up one level since we're in scripts folder)
$projectFiles = Get-ChildItem -Path .. -Recurse -Filter "*.csproj"

if ($projectFiles.Count -eq 0) {
    Write-Host "No .csproj files found in the parent directory and subdirectories." -ForegroundColor Red
    exit 1
}

Write-Host "Found $($projectFiles.Count) project(s):" -ForegroundColor Yellow
foreach ($project in $projectFiles) {
    Write-Host "  - $($project.FullName)" -ForegroundColor Cyan
}

# Print an empty line for better readability in the console output
Write-Host ""

# Function to update packages for a single project
function Update-ProjectPackages {
    param(
        [string]$ProjectPath
    )

    Write-Host "Updating packages for: $ProjectPath" -ForegroundColor Yellow

    try {
        # Get list of packages in the project
        # 2>&1 redirects error messages to the same output stream as regular output
        $packagesOutput = dotnet list $ProjectPath package --outdated 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-Host "  Warning: Could not check outdated packages for $ProjectPath" -ForegroundColor Yellow
            Write-Host "  $packagesOutput" -ForegroundColor Gray
        }

        # Update all packages to latest versions
        $updateOutput = dotnet add $ProjectPath package --help 2>&1

        # Get all package references from the project file
        $projectContent = Get-Content $ProjectPath
        $packageReferences = $projectContent | Select-String '<PackageReference Include="([^"]+)"' | ForEach-Object {
            $_.Matches[0].Groups[1].Value
        }

        if ($packageReferences.Count -eq 0) {
            Write-Host "  No NuGet packages found in this project." -ForegroundColor Gray
            return
        }

        Write-Host "  Found $($packageReferences.Count) package(s) to update:" -ForegroundColor Cyan

        foreach ($package in $packageReferences) {
            Write-Host "    Updating $package..." -ForegroundColor Gray

            # Remove the package and add it back to get the latest version
            $removeResult = dotnet remove $ProjectPath package $package 2>&1
            if ($LASTEXITCODE -eq 0) {
                $addResult = dotnet add $ProjectPath package $package 2>&1
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "    ✓ Successfully updated $package" -ForegroundColor Green
                } else {
                    Write-Host "    ✗ Failed to add $package back: $addResult" -ForegroundColor Red
                }
            } else {
                Write-Host "    ✗ Failed to remove $package: $removeResult" -ForegroundColor Red
            }
        }

    } catch {
        Write-Host "  Error updating packages for $ProjectPath`: $($_.Exception.Message)" -ForegroundColor Red
    }

    Write-Host ""
}

# Update packages for each project
foreach ($project in $projectFiles) {
    Update-ProjectPackages -ProjectPath $project.FullName
}

Write-Host "Package update process completed!" -ForegroundColor Green
Write-Host ""
Write-Host "To verify the updates, you can run:" -ForegroundColor Yellow
Write-Host "  dotnet list package --outdated" -ForegroundColor Cyan
Write-Host ""
Write-Host "To restore and build all projects:" -ForegroundColor Yellow
Write-Host "  dotnet restore" -ForegroundColor Cyan
Write-Host "  dotnet build" -ForegroundColor Cyan
