#!/bin/bash
# Lance le jeu principal (GameController) sans passer par l'éditeur

set -e

echo "🎮 Lancement du jeu..."

# Compiler si nécessaire
DLL_PATH=".godot/mono/temp/bin/Debug/Front-BR-RPG.dll"
if [ ! -f "$DLL_PATH" ]; then
    echo "⚠️  Compilation nécessaire..."
    dotnet build --nologo
else
    NEWER_FILES=$(find Scripts -name "*.cs" -newer "$DLL_PATH" 2>/dev/null | wc -l)
    if [ $NEWER_FILES -gt 0 ]; then
        echo "🔄 Recompilation..."
        dotnet build --nologo
    else
        echo "✅ Déjà compilé"
    fi
fi

# Lancer le jeu
GODOT="/home/guilhem/bin/Godot_v4.5.1-stable_mono_linux_x86_64/Godot_v4.5.1-stable_mono_linux.x86_64"
$GODOT --path "$(pwd)" res://Scenes/GameController.tscn
