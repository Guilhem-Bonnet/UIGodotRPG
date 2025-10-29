#!/bin/bash
# Ouvre le projet dans l'éditeur Godot 4.5.1

GODOT_EDITOR="/home/guilhem/bin/Godot_v4.5.1-stable_mono_linux_x86_64/Godot_v4.5.1-stable_mono_linux.x86_64"

echo "🚀 Lancement de l'éditeur Godot 4.5.1..."
$GODOT_EDITOR --path "$(pwd)" --editor
