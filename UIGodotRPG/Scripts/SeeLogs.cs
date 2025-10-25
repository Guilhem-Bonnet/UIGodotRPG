using Godot;

public partial class SeeLogs : Control
{
    private Node collapsibleContainer; // Le nœud CollapsibleContainer
    private CheckButton toggleButton; // Le CheckButton pour activer/désactiver

    [Export]
    public NodePath urnCollapsiblePath; // Pour exposer le chemin du CollapsibleContainer dans l'éditeur

    public override void _Ready()
    {
        // Récupérer le nœud CollapsibleContainer via le chemin exporté
        collapsibleContainer = GetNode(urnCollapsiblePath);
        
        // Récupérer le CheckButton
        toggleButton = GetNode<CheckButton>("CheckButton");

        // Connecter le signal "toggled" au bouton
        toggleButton.Toggled += OnToggled;
    }

    // Méthode appelée lorsque le bouton est togglé
    private void OnToggled(bool toggledOn)
    {
        if (toggledOn)
        {
            collapsibleContainer.Call("open_tween"); // Ouvre avec Tween
        }
        else
        {
            collapsibleContainer.Call("close"); // Ferme le conteneur
        }
    }
}
