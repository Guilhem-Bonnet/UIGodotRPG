using Godot;
using System;
using System.Collections.Generic;
using FrontBRRPG;

/// <summary>
/// Script de test pour démontrer les cartes personnages enrichies
/// </summary>
public partial class TestCharacterCards : Node
{
	[Export] private ProfileGrid _profileGrid;
	
	private List<PersonnageUIManager> _characters = new();
	private Random _random = new();
	
	public override void _Ready()
	{
		if (_profileGrid == null)
		{
			GD.PrintErr("[TestCharacterCards] ProfileGrid non assigné !");
			return;
		}
		
		// Attendre un peu pour que les personnages soient créés
		GetTree().CreateTimer(1.0).Timeout += StartDemo;
	}
	
	private void StartDemo()
	{
		_characters = _profileGrid.GetCharacterProfiles();
		
		if (_characters.Count == 0)
		{
			GD.PrintErr("[TestCharacterCards] Aucun personnage trouvé !");
			return;
		}
		
		GD.Print($"[TestCharacterCards] Démarrage de la démo avec {_characters.Count} personnages");
		
		// Lancer des événements de combat simulés
		SimulateCombat();
	}
	
	private async void SimulateCombat()
	{
		await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
		
		// Tour 1: Quelques attaques
		GD.Print("=== Tour 1: Attaques ===");
		if (_characters.Count >= 2)
		{
			var attacker = _characters[0];
			var target = _characters[1];
			int damage = _random.Next(10, 30);
			
			attacker.RegisterAttack(damage, target.CharacterData.Name);
			target.TakeDamage(damage, attacker.CharacterData.Name);
		}
		
		await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
		
		// Tour 2: Buffs et attaques
		GD.Print("=== Tour 2: Buffs et attaques ===");
		if (_characters.Count >= 3)
		{
			_characters[0].AddStatusEffect("Force", true, 3);
			_characters[2].AddStatusEffect("Poison", false, 3);
			
			int damage = _random.Next(15, 35);
			_characters[0].RegisterAttack(damage, _characters[2].CharacterData.Name);
			_characters[2].TakeDamage(damage, _characters[0].CharacterData.Name);
		}
		
		await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
		
		// Tour 3: Soins et mort
		GD.Print("=== Tour 3: Soins et combat intense ===");
		if (_characters.Count >= 4)
		{
			_characters[3].Heal(20, _characters[1].CharacterData.Name);
			
			// Attaque mortelle
			int damage = _random.Next(80, 120);
			_characters[1].RegisterAttack(damage, _characters[2].CharacterData.Name);
			_characters[2].TakeDamage(damage, _characters[1].CharacterData.Name);
		}
		
		await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
		
		// Tour 4: Résurrection
		GD.Print("=== Tour 4: Résurrection ===");
		if (_characters.Count >= 3)
		{
			_characters[2].Heal(50, "Prêtre");
		}
		
		await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
		
		// Tour 5: Combat final
		GD.Print("=== Tour 5: Combat final ===");
		foreach (var character in _characters)
		{
			if (!character.CharacterData.IsDead && _random.Next(0, 2) == 0)
			{
				var targets = _characters.FindAll(c => c != character && !c.CharacterData.IsDead);
				if (targets.Count > 0)
				{
					var target = targets[_random.Next(0, targets.Count)];
					int damage = _random.Next(5, 25);
					
					character.RegisterAttack(damage, target.CharacterData.Name);
					target.TakeDamage(damage, character.CharacterData.Name);
				}
			}
		}
		
		await ToSignal(GetTree().CreateTimer(3.0), SceneTreeTimer.SignalName.Timeout);
		
		// Afficher les stats finales
		GD.Print("\n=== Stats finales ===");
		foreach (var character in _characters)
		{
			GD.Print($"{character.CharacterData.Name}: {character.GetCombatStats()}");
		}
	}
}
