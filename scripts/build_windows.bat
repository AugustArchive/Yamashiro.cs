:: Script for building Yamashiro on Windows
:: Made by August (https://augu.dev)

:: Do initial build
set CURRENT=%cd%
echo Now building...

:: Check if dotnet is installed
dotnet --version
if errorlevel 1 goto notInstalled

:: dotnet is installed, now we do shit
if exist %CURRENT%\build\Yamashiro rmdir %CURRENT%\build\Yamashiro /q /s
echo Building...

dotnet publish Yamashiro/Yamashiro.csproj -c Release -r win-x64 -o build\Yamashiro
echo Built the project!

goto:eof

:notInstalled
echo Error^: dotnet is not installed on your machine.