param (
    [Parameter(Mandatory=$false, Position=0)] [string] $DOTNET_PROJECT,
    [Parameter(Mandatory=$false, Position=1)] [string] $PACKAGE_SRC
)

if ([string]::IsNullOrEmpty($DOTNET_PROJECT) -or [string]::IsNullOrEmpty($PACKAGE_SRC))
{
    $Path = $MyInvocation.InvocationName
    Write-Host "Missing parameters. Usage: $Path DOTNET_PROJECT PACKAGE_SRC"
    Write-Host "Example: $Path myapp.csprj /mypackages"
    Exit 1
}

$packages = @(
    "GSF.CarbonAware"
)

# Remove packages from project
foreach ($pname in $packages) {
    dotnet remove $DOTNET_PROJECT package $pname
}

Remove-Item -Recurse -Force ~/.nuget/packages/gsf.carbonaware*

# Add packages to project
foreach ($pname in $packages) {
    dotnet add $DOTNET_PROJECT package $pname -s $PACKAGE_SRC --prerelease
}
