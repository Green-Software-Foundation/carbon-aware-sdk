param (
    [Parameter(Mandatory=$false, Position=0)] [string] $DOTNET_SOLUTION,
    [Parameter(Mandatory=$false, Position=1)] [string] $DEST_PACKAGES
)

if ([string]::IsNullOrEmpty($DOTNET_SOLUTION) -or [string]::IsNullOrEmpty($DEST_PACKAGES))
{
    $Path = $MyInvocation.InvocationName
    Write-Host "Missing parameters. Usage: $Path DOTNET_SOLUTION DEST_PACKAGES"
    Write-Host "Example: $Path src/CarbonAwareSDK.sln /mypackages"
    Exit 1
}

$REVISION = $(git rev-parse HEAD)
$BRANCH = "dev"

New-Item -Path $DEST_PACKAGES -ItemType Directory

# Remove existing packages
Get-ChildItem -Path $path -Recurse -Include *.nupkg, *.snupkg | Remove-Item

# Setup package metadata
dotnet restore $DOTNET_SOLUTION
dotnet build $DOTNET_SOLUTION
dotnet pack $DOTNET_SOLUTION -o $DEST_PACKAGES -c Debug `
    -p:RepositoryBranch=$BRANCH `
    -p:SourceRevisionId=$REVISION `
    -p:IncludeSymbols="true" `
    -p:SymbolPackageFormat="snupkg"
