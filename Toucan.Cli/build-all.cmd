dotnet publish Toucan.Cli.net60.csproj -c Release -o build\linux-x64 --runtime linux-x64
dotnet publish Toucan.Cli.net60.csproj -c Release -o build\osx-x64 --runtime osx-x64
dotnet publish Toucan.Cli.csproj -c Release -o build\win-x64 --runtime win-x64