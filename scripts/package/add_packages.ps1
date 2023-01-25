param (
    [Parameter(Mandatory=$true, Position=0)] [string] $DOTNET_PROJECT,
    [Parameter(Mandatory=$true, Position=1)] [string] $PACKAGE_SRC
)

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
