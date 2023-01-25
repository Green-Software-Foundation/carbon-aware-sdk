param (
    [Parameter(Mandatory=$true, Position=0)] [string] $DOTNET_SOLUTION,
    [Parameter(Mandatory=$true, Position=1)] [string] $DEST_PACKAGES
)

$REVISION = $(git rev-parse HEAD)
$BRANCH = "dev"

New-Item -Path $DEST_PACKAGES -ItemType Directory

# Remove existing packages
Get-ChildItem -Path $path -Recurse -Include *.nupkg, *.snupkg | Remove-Item

# Setup package metadata
dotnet build $DOTNET_SOLUTION
dotnet pack $DOTNET_SOLUTION -o $DEST_PACKAGES -c Debug `
    -p:RepositoryBranch=$BRANCH `
    -p:SourceRevisionId=$REVISION `
    -p:IncludeSymbols="true" `
    -p:SymbolPackageFormat="snupkg"

