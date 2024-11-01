using Godot;
using System;

public partial class SpellButton : TextureButton
{
    private ProgressBar cooldownBar;
    private float cooldownDuration = 5.0f; // Durée de recharge en secondes
    private float cooldownTimer = 0.0f;
    private bool isOnCooldown = false;
    [Export] public string SpellInfo = "Description du sort ici.";
    private Tooltip tooltip;

    public override void _Ready()
    {
        // Initialiser la ProgressBar (qui doit être un enfant de SpellButton dans la scène)
        cooldownBar = GetNode<ProgressBar>("CooldownBar");
        cooldownBar.MaxValue = cooldownDuration;
        cooldownBar.Value = cooldownDuration; // Commence à 100% (plein)
        cooldownBar.Visible = false; // Cachée lorsqu'il n'est pas en cooldown

        // Charge la scène de l'info-bulle (assurez-vous de bien configurer le chemin)
        var tooltipScene = GD.Load<PackedScene>("res://Tooltip.tscn");
        tooltip = (Tooltip)tooltipScene.Instantiate();

        // Ajoutez l'info-bulle à la scène
        GetTree().Root.AddChild(tooltip);

        // Connectez les signaux de survol de la souris
        Connect("mouse_entered", Callable.From(OnMouseEntered));
        Connect("mouse_exited", Callable.From(OnMouseExited));
    }
        

    private void OnMouseEntered()
    {
        // Affiche l'info-bulle à côté du bouton
        Vector2 tooltipPosition = GetGlobalMousePosition() + new Vector2(10, 10); // Ajustez la position si nécessaire
        tooltip.ShowTooltip(SpellInfo, tooltipPosition);
    }

    private void OnMouseExited()
    {
        // Masque l'info-bulle
        tooltip.HideTooltip();
    }

    public override void _Process(double delta)
    {
        // Gérer le cooldown
        if (isOnCooldown)
        {
            cooldownTimer -= (float)delta;
            cooldownBar.Value = cooldownTimer;

            // Si le cooldown est terminé
            if (cooldownTimer <= 0.0f)
            {
                isOnCooldown = false;
                cooldownBar.Visible = false;
                cooldownTimer = 0.0f;
            }
        }
    }

    // Appelé lorsqu'on appuie sur le bouton de sort
    public void OnSpellPressed()
    {
        if (!isOnCooldown)
        {
            // Lancer le sort et déclencher le cooldown
            ActivateCooldown();
        }
    }

    // Fonction pour démarrer le cooldown
    private void ActivateCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownDuration;
        cooldownBar.Visible = true;
    }

    // Fonction pour définir l'icône du sort et la durée du cooldown depuis le HBoxContainer
    public void SetSpellIconAndCooldown(Texture2D icon, float cooldown)
    {
        TextureNormal = icon;
        cooldownDuration = cooldown;
        cooldownBar.MaxValue = cooldownDuration;
    }
}


