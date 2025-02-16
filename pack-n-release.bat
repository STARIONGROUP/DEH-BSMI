dotnet publish DEH-BSMI.Tools/DEH-BSMI.Tools.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ReleasePublication-Win64
dotnet publish DEH-BSMI.Tools/DEH-BSMI.Tools.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ReleasePublication-Linux64
dotnet publish DEH-BSMI.Tools/DEH-BSMI.Tools.csproj -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o ReleasePublication-OSX64
pause