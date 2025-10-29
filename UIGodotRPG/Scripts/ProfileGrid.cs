using Godot;
using System;
using System.Collections.Generic;
using FrontBRRPG;
using FrontBRRPG.Network;

public partial class ProfileGrid : GridContainer
{
	[Export] private PackedScene CharacterProfileScene;
	[Export] private int NumberOfProfiles = 4; // Par défaut 4 profils (au lieu de 9)
	[Export] private bool AutoCreateProfiles = false; // Désactiver la création auto

	private List<PersonnageUIManager> _characterProfiles = new List<PersonnageUIManager>();
	private bool _dynamicInitialization = false;

	public override void _Ready()
	{
		// Vérifiez que la scène est bien chargée
		if (CharacterProfileScene == null)
		{
			GD.PrintErr("[ProfileGrid] CharacterProfileScene n'est pas assignée.");
			return;
		}

		// Ne créer les profils QUE si AutoCreateProfiles est activé
		if (AutoCreateProfiles && !_dynamicInitialization)
		{
			CreateProfiles(NumberOfProfiles);
		}
		else
		{
			GD.Print($"[ProfileGrid] Attente d'initialisation dynamique (AutoCreateProfiles = {AutoCreateProfiles})");
		}
	}
	
	/// <summary>
	/// Initialise la grille avec une liste spécifique de personnages
	/// Doit être appelé AVANT _Ready() ou via CallDeferred
	/// </summary>
	public void InitializeWithCharacters(List<CharacterConfig> characters)
	{
		_dynamicInitialization = true;
		
		// Nettoyer les profils existants
		ClearProfiles();
		
		// Créer les profils pour chaque personnage
		foreach (var character in characters)
		{
			var profileInstance = CharacterProfileScene.Instantiate<PersonnageUIManager>();
			AddChild(profileInstance);
			_characterProfiles.Add(profileInstance);
			
			// Initialiser le profil avec les données du personnage
			profileInstance.InitializeCharacter(character.Name, character.Type, 100);
			GD.Print($"[ProfileGrid] ✅ Carte initialisée: {character.Name} ({character.Type})");
		}
		
		GD.Print($"[ProfileGrid] {characters.Count} profils créés et initialisés dynamiquement");
	}
	
	/// <summary>
	/// Crée un nombre spécifique de profils vides
	/// </summary>
	private void CreateProfiles(int count)
	{
		for (int i = 0; i < count; i++)
		{
			var profileInstance = CharacterProfileScene.Instantiate<PersonnageUIManager>();
			AddChild(profileInstance);
			_characterProfiles.Add(profileInstance);
		}
		
		GD.Print($"[ProfileGrid] {count} profils créés");
	}
	
	/// <summary>
	/// Supprime tous les profils existants
	/// </summary>
	private void ClearProfiles()
	{
		foreach (var profile in _characterProfiles)
		{
			profile.QueueFree();
		}
		_characterProfiles.Clear();
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
