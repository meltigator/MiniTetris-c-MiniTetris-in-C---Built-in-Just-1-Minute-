#!/bin/bash
#
# written by Andrea Giani
#

echo "=== MSYS2 Demo: MiniTetris C# ==="

# Compile
echo "Compiling MiniTetris C#..."
cd CSharpTetris
"$PROGRAMFILES\dotnet\dotnet.exe" build
if [ $? -ne 0 ]; then
    echo "ERROR: Compile C# failed"
    exit 1
fi

# Change
cd ..

# Go
echo "Run..."

# start tetris C# on background
cd CSharpTetris
"$PROGRAMFILES\dotnet\dotnet.exe" run &
CSHARP_PID=$!

# Force run on Windows!
start "bin/Debug/net6.0/CSharpTetris.exe"

# Wait & Exit
echo "Play the game!."
wait $CSHARP_PID

echo "Demo finished!"
