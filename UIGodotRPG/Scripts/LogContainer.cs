using Godot;
using System;

/// <summary>
/// Conteneur de logs dynamiques avec support des couleurs et auto-scroll
/// </summary>
public partial class LogContainer : VBoxContainer
{
    [Export] public int MaxLogs { get; set; } = 50;
    [Export] public bool AutoScroll { get; set; } = false; // DÉSACTIVÉ PAR DÉFAUT pour pouvoir lire les logs
    
    private PackedScene _logComponentScene;
    private ScrollContainer _scrollContainer;

    public override void _Ready()
    {
        _logComponentScene = GD.Load<PackedScene>("res://Components/LogComponent.tscn");
        
        // Trouver le ScrollContainer parent pour l'auto-scroll
        _scrollContainer = GetParent() as ScrollContainer;
        
        // Supprimer les logs de démo
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
    }

    /// <summary>
    /// Ajoute un log simple (texte uniquement)
    /// </summary>
    public void AddLog(string logMessage)
    {
        AddLog(logMessage, Colors.White);
    }
    
    /// <summary>
    /// Ajoute un log avec couleur personnalisée
    /// </summary>
    public void AddLog(string logMessage, Color color)
    {
        // Instancier le LogComponent
        var logInstance = _logComponentScene.Instantiate();
        
        // Récupérer et configurer le Label
        var label = logInstance.GetNode<Label>("Label");
        label.Text = logMessage;
        label.AddThemeColorOverride("font_color", color);
        
        // Ajouter au container
        AddChild(logInstance);
        
        // Limiter le nombre de logs
        while (GetChildCount() > MaxLogs)
        {
            var oldestLog = GetChild(0);
            RemoveChild(oldestLog);
            oldestLog.QueueFree();
        }
        
        // Auto-scroll vers le bas
        if (AutoScroll && _scrollContainer != null)
        {
            CallDeferred(MethodName.ScrollToBottom);
        }
    }
    
    /// <summary>
    /// Scroll automatique vers le dernier log
    /// </summary>
    private void ScrollToBottom()
    {
        if (_scrollContainer != null)
        {
            _scrollContainer.ScrollVertical = (int)_scrollContainer.GetVScrollBar().MaxValue;
        }
    }
    
    /// <summary>
    /// Efface tous les logs
    /// </summary>
    public void ClearLogs()
    {
        foreach (Node child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }
    }
    
    /// <summary>
    /// Retourne le nombre de logs actuels
    /// </summary>
    public int GetLogCount()
    {
        return GetChildCount();
    }
}
