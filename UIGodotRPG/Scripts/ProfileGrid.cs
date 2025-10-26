using Godot;
using System;
using System.Collections.Generic;

public partial class ProfileGrid : GridContainer
{
    [Export] private PackedScene CharacterProfileScene; // Chargez votre scène de profil de personnage ici
    [Export] private int NumberOfProfiles = 9; // Définissez le nombre de profils à afficher

    private List<PersonnageUIManager> _characterProfiles = new List<PersonnageUIManager>();

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
            var profileInstance = CharacterProfileScene.Instantiate<PersonnageUIManager>();
            AddChild(profileInstance);
            _characterProfiles.Add(profileInstance);
        }
        
        GD.Print($"[ProfileGrid] {NumberOfProfiles} profils créés");
    }
    
    /// <summary>
    /// Récupère tous les profils de personnages
    /// </summary>
    public List<PersonnageUIManager> GetCharacterProfiles()
    {
        return _characterProfiles;
    }
    
    /// <summary>
    /// Récupère un profil de personnage par index
    /// </summary>
    public PersonnageUIManager GetProfile(int index)
    {
        if (index >= 0 && index < _characterProfiles.Count)
        {
            return _characterProfiles[index];
        }
        return null;
    }
}
