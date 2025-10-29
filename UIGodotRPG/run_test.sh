#!/bin/bash
# Lance directement le TestEnvironment avec le DLL déjà compilé
# Contournement du bug MSBuild de l'éditeur Godot

set -e

echo "🎮 Lancement du TestEnvironment..."

# Vérifier que le DLL existe et compiler si nécessaire
DLL_PATH=".godot/mono/temp/bin/Debug/Front-BR-RPG.dll"
if [ ! -f "$DLL_PATH" ]; then
    echo "⚠️  DLL manquant, compilation..."
    dotnet build --nologo
else
    # Vérifier si des fichiers .cs sont plus récents que le DLL
    NEWER_FILES=$(find Scripts -name "*.cs" -newer "$DLL_PATH" 2>/dev/null | wc -l)
    if [ $NEWER_FILES -gt 0 ]; then
        echo "🔄 Code modifié, recompilation..."
        dotnet build --nologo
    else
        echo "✅ DLL à jour"
    fi
fi

#!/bin/bash

# Chemin vers le binaire Godot
GODOT_PATH="/home/guilhem/bin/Godot_v4.5.1-stable_mono_linux_x86_64/Godot_v4.5.1-stable_mono_linux.x86_64"

if [ ! -f "$GODOT_PATH" ]; then
    echo "❌ Godot non trouvé: $GODOT_PATH"
    exit 1
fi

# Lancer directement la scène TestEnvironment
echo "▶️  Démarrage de TestEnvironment.tscn..."
echo ""
echo "📋 Contrôles du simulateur:"
echo "   ESPACE = Play/Pause auto-combat"
echo "   1 = Test attaque"
echo "   2 = Test soin"
echo "   3 = Test mort"
echo "   4 = Test résurrection"
echo "   5 = Test effet de statut"
echo ""

$GODOT_PATH --path "$(pwd)" res://Scenes/TestEnvironment.tscn
