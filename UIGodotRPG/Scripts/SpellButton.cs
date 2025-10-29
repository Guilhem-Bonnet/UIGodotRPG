using Godot;
using System;

public partial class SpellButton : TextureButton
{
    private ProgressBar cooldownBar;
    private ColorRect cooldownOverlay;
    private Label cooldownLabel;
    private float cooldownDuration = 5.0f; // Durée de recharge en secondes
    private float cooldownTimer = 0.0f;
    private bool isOnCooldown = false;
    [Export] public string SpellInfo = "Description du sort ici.";
    [Export] public string SpellEmoji = "⚔️"; // Emoji par défaut
    private Tooltip tooltip;
    private Label iconLabel; // Pour afficher l'emoji à la place de la texture

    public override void _Ready()
    {
        // Initialiser les composants visuels
        cooldownBar = GetNode<ProgressBar>("CooldownBar");
        cooldownOverlay = GetNode<ColorRect>("CooldownOverlay");
        cooldownLabel = GetNode<Label>("CooldownLabel");
        
        cooldownBar.MaxValue = 100.0f;
        cooldownBar.Value = 0.0f;
        
        // Initialement, le spell est disponible (pas de cooldown)
        cooldownOverlay.Visible = false;
        cooldownLabel.Visible = false;
        cooldownBar.Visible = false;

        // Créer un label pour l'emoji (remplace la texture)
        iconLabel = new Label();
        iconLabel.Text = SpellEmoji;
        iconLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        iconLabel.AddThemeFontSizeOverride("font_size", 32);
        iconLabel.HorizontalAlignment = HorizontalAlignment.Center;
        iconLabel.VerticalAlignment = VerticalAlignment.Center;
        iconLabel.SetAnchorsPreset(LayoutPreset.FullRect);
        iconLabel.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(iconLabel);

        // Charge la scène de l'info-bulle
        var tooltipScene = GD.Load<PackedScene>("res://Components/Tooltip.tscn");
        tooltip = (Tooltip)tooltipScene.Instantiate();
        this.CallDeferred("add_child", tooltip);

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
            
            // Calculer le pourcentage (0 = juste activé, 100 = prêt)
            float chargePercent = 0;
            if (cooldownDuration > 0)
            {
                chargePercent = ((cooldownDuration - cooldownTimer) / cooldownDuration) * 100.0f;
            }
            
            // Mettre à jour la barre et le label avec les SECONDES restantes
            cooldownBar.Value = chargePercent;
            cooldownLabel.Text = $"{cooldownTimer:F1}s"; // Format: 3.2s, 1.5s, 0.8s

            // Si le cooldown est terminé (100%)
            if (cooldownTimer <= 0.0f)
            {
                SetCooldownState(100.0f); // Prêt !
                isOnCooldown = false;
                cooldownTimer = 0.0f;
            }
        }
    }

    /// <summary>
    /// Met à jour l'état du cooldown depuis l'extérieur (CombatSimulator)
    /// </summary>
    public void UpdateCooldown(float currentCooldown, float maxCooldown, string emoji = null)
    {
        // Vérifier que les composants sont initialisés
        if (cooldownOverlay == null || cooldownLabel == null || cooldownBar == null)
        {
            return; // Pas encore prêt, on ignore
        }
        
        cooldownDuration = maxCooldown;
        cooldownTimer = currentCooldown;
        isOnCooldown = currentCooldown > 0;
        
        // Mettre à jour l'emoji si fourni
        if (emoji != null && iconLabel != null)
        {
            SpellEmoji = emoji;
            iconLabel.Text = emoji;
        }
        
        // Calculer le pourcentage (0 = juste activé, 100 = prêt)
        float chargePercent = 0;
        if (maxCooldown > 0)
        {
            chargePercent = ((maxCooldown - currentCooldown) / maxCooldown) * 100.0f;
        }
        
        SetCooldownState(chargePercent);
    }
    
    /// <summary>
    /// Définit l'état visuel du cooldown en fonction du pourcentage
    /// </summary>
    private void SetCooldownState(float chargePercent)
    {
        // Vérifier que les composants sont initialisés
        if (cooldownOverlay == null || cooldownLabel == null || cooldownBar == null)
        {
            return; // Pas encore prêt
        }
        
        if (chargePercent >= 100.0f)
        {
            // 100% = Disponible
            cooldownOverlay.Visible = false;
            cooldownLabel.Visible = false;
            cooldownBar.Visible = false;
        }
        else
        {
            // 0-99% = En chargement - Afficher les secondes restantes
            cooldownOverlay.Visible = true;
            cooldownLabel.Visible = true;
            cooldownBar.Visible = true;
            cooldownBar.Value = chargePercent;
            cooldownLabel.Text = $"{cooldownTimer:F1}s"; // Secondes avec 1 décimale
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
        SetCooldownState(0); // 0% au début
    }

    // Fonction pour définir l'icône du sort (emoji) et la durée du cooldown
    public void SetSpellIconAndCooldown(string emoji, float cooldown)
    {
        SpellEmoji = emoji;
        if (iconLabel != null)
        {
            iconLabel.Text = emoji;
        }
        cooldownDuration = cooldown;
        SpellInfo = $"{emoji} {cooldown}s cooldown";
    }
}
