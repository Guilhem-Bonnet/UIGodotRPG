using Godot;
using System;

public partial class Life : ProgressBar
{
    // Définir les variables de santé
    private int maxHealth = 100;
    private int currentHealth;

    // Référence à la barre de progression
    private ProgressBar healthBar;

    public override void _Ready()
    {
        // Initialiser la santé du joueur
        currentHealth = maxHealth;

        // Récupérer la barre de santé dans la scène
        healthBar = GetNode<ProgressBar>("HealthBar");

        // Définir les valeurs min et max de la barre de progression
        healthBar.MaxValue = maxHealth;
        healthBar.Value = currentHealth;
    }

    // Fonction pour prendre des dégâts
    public void TakeDamage(int damage)
    {
        currentHealth = Math.Max(0, currentHealth - damage);
        UpdateHealthBar();
    }

    // Fonction pour mettre à jour la barre de santé
    private void UpdateHealthBar()
    {
        healthBar.Value = currentHealth;
    }


}