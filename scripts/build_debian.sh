# Build script for Debian (my VPS is Debian)
if ![ -x "$(command -v dotnet)" ]; then
  echo "[Error] <=> Missing dotnet package!" >&2
  exit 1
fi

if [ -d "$PWD/build/Yamashiro" ]; then
  rm -fr "$PWD/build/Yamashiro"
fi

echo "[Script] <=> Now compiling project..."
cd Yamashiro
dotnet publish Yamashiro.csproj -c Release -r debian.10-x64 -o ../build/Yamashiro