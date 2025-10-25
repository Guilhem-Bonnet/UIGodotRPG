using Godot;
using System;

public partial class ProfileGrid : GridContainer
{
    [Export] private PackedScene CharacterProfileScene; // Chargez votre scène de profil de personnage ici
    [Export] private int NumberOfProfiles = 9; // Définissez le nombre de profils à afficher

    public override void _Ready()
    {
        // Vérifiez que la scène est bien chargée
        if (CharacterProfileScene == null)
        {
            GD.PrintErr("CharacterProfileScene n'est pas assignée.");
            return;
        }

        // Instanciez et ajoutez les profils dans la grille
        for (int i = 0; i < NumberOfProfiles; i++)
        {
            Node profileInstance = CharacterProfileScene.Instantiate();
            AddChild(profileInstance); // Ajoute chaque profil à la grille
        }
    }
}
