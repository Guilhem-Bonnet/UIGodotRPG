using Godot;
using System;

public partial class LogContainer : VBoxContainer
{
    // Référence vers la scène du LogComponent
    private PackedScene logComponentScene;

    public override void _Ready()
    {
        // Charger la scène du LogComponent
        logComponentScene = GD.Load<PackedScene>("res://Components/LogComponent.tscn");
    }

    // Fonction pour ajouter dynamiquement un log
    public void AddLog(string logMessage)
    {
        // Instancier le LogComponent
        Node logComponentInstance = logComponentScene.Instantiate();

        // Récupérer le Label dans le LogComponent instancié et lui attribuer le message
        Label logLabel = logComponentInstance.GetNode<Label>("Label");
        logLabel.Text = logMessage;

        // Ajouter le LogComponent instancié comme enfant du LogContainer
        AddChild(logComponentInstance);
    }
}
