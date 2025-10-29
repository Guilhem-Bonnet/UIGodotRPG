using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FrontBRRPG;
using FrontBRRPG.Network;
using FrontBRRPG.Combat;
using FrontBRRPG.UI;

/// <summary>
/// Contrôleur de l'arène qui synchronise les personnages avec le WebSocket
/// </summary>
public partial class AreneController : Control
{
	private WebSocketClient _wsClient;
	private CombatLogParser _parser;
	private BattleState _battleState;
	private ProfileGrid _profileGrid;
	private ConnectionStatus _connectionStatus;
	
	private bool _combatStarted = false;
	private double _waitTimeForData = 0;
	private const double MAX_WAIT_TIME = 3.0; // 3 secondes d'attente
	
	// Dictionnaire pour associer les noms aux UI
	private Dictionary<string, PersonnageUIManager> _characterUIs = new Dictionary<string, PersonnageUIManager>();
	private List<CharacterConfig> _selectedCharacters;
	
	public override void _Ready()
	{
		// Récupérer le WebSocketClient AutoLoad
		_wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
		
		// Récupérer la grille de profils
		_profileGrid = GetNode<ProfileGrid>("ScrollContainer/GridContainer");
		
		// Créer et ajouter l'indicateur de connexion
		_connectionStatus = new ConnectionStatus();
		_connectionStatus.Position = new Vector2(10, 10);
		_connectionStatus.ZIndex = 100; // Au-dessus de tout
		AddChild(_connectionStatus);
		
		// Si des personnages ont été sélectionnés, initialiser avec eux
		if (_selectedCharacters != null && _selectedCharacters.Count > 0)
		{
			CallDeferred(nameof(InitializeProfilesWithCharacters));
			GD.Print($"[AreneController] Initialisation avec {_selectedCharacters.Count} personnages sélectionnés");
		}
		
		// Initialiser le parser
		_parser = new CombatLogParser();
		AddChild(_parser);
		_parser.EventParsed += OnEventParsed;
		
		_battleState = new BattleState();
		
		// NE PAS cacher les cartes - les laisser visibles avec données par défaut
		// Elles seront mises à jour quand on recevra les données du combat
		// HideAllCharacterCards(); // DÉSACTIVÉ
		
		// Connecter aux événements WebSocket
		_wsClient.MessageReceived += OnMessageReceived;
		_wsClient.ConnectionEstablished += OnConnectionEstablished;
		_wsClient.ConnectionClosed += OnConnectionClosed;
		
		// Connexion automatique au serveur
		if (!_wsClient.IsConnected)
		{
			GD.Print("[AreneController] Connexion au serveur WebSocket...");
			_wsClient.ConnectToServer();
		}
		
		GD.Print("[AreneController] Initialisé et connecté au WebSocket");
	}
	
	public override void _Process(double delta)
	{
		// Si le combat n'a pas démarré et qu'on attend des données
		if (!_combatStarted && _wsClient.IsConnected)
		{
			_waitTimeForData += delta;
			
			// Après 3 secondes d'attente, initialiser avec des cartes de test visibles
			if (_waitTimeForData > MAX_WAIT_TIME && _characterUIs.Count == 0)
			{
				GD.Print("⏰ [AreneController] Timeout - Initialisation des cartes en mode test");
				InitializeTestCards();
				_combatStarted = true; // Ne plus attendre
			}
		}
	}
	
	/// <summary>
	/// Initialise les profils avec les personnages sélectionnés (appelé en deferred)
	/// </summary>
	private void InitializeProfilesWithCharacters()
	{
		if (_profileGrid != null && _selectedCharacters != null)
		{
			_profileGrid.InitializeWithCharacters(_selectedCharacters);
			
			// Récupérer les cartes créées et les stocker dans le dictionnaire
			var profiles = _profileGrid.GetCharacterProfiles();
			for (int i = 0; i < _selectedCharacters.Count && i < profiles.Count; i++)
			{
				var characterName = _selectedCharacters[i].Name;
				_characterUIs[characterName] = profiles[i];
				GD.Print($"[AreneController] ✅ Carte enregistrée: {characterName}");
			}
			
			GD.Print($"[AreneController] {_characterUIs.Count} cartes prêtes à recevoir les données de combat");
		}
	}
	
	/// <summary>
	/// Permet de définir les personnages sélectionnés avant l'initialisation de la scène
	/// </summary>
	public void SetSelectedCharacters(List<CharacterConfig> characters)
	{
		_selectedCharacters = characters;
		GD.Print($"[AreneController] {characters.Count} personnages configurés");
	}
	
	private void OnConnectionEstablished()
	{
		GD.Print("[AreneController] WebSocket connecté ✅");
		
		// Si aucun personnage n'a été sélectionné, démarrer avec une configuration par défaut
		if (_selectedCharacters == null || _selectedCharacters.Count == 0)
		{
			StartDefaultBattle();
		}
	}
	
	/// <summary>
	/// Démarre un combat avec une configuration par défaut
	/// </summary>
	private void StartDefaultBattle()
	{
		GD.Print("[AreneController] Démarrage d'un combat par défaut avec 4 personnages");
		
		var defaultCharacters = new List<CharacterConfig>
		{
			new CharacterConfig { Name = "Ragnar", Type = "Berserker" },
			new CharacterConfig { Name = "Shadow", Type = "Assassin" },
			new CharacterConfig { Name = "Lumière", Type = "Pretre" },
			new CharacterConfig { Name = "Merlin", Type = "Magicien" }
		};
		
		_wsClient.StartBattle(defaultCharacters);
	}
	
	private void OnConnectionClosed(string reason)
	{
		GD.Print($"[AreneController] WebSocket fermé: {reason}");
	}
	
	private void OnMessageReceived(string message)
	{
		GD.Print($"📨 [AreneController] Message reçu: {message}");
		
		// Marquer que le combat a commencé (on reçoit des données)
		if (!_combatStarted)
		{
			_combatStarted = true;
			GD.Print("✅ [AreneController] Combat démarré - données reçues");
		}
		
		// Parser le message
		var evt = _parser.ParseMessage(message);
		_battleState.AddEvent(evt);
		
		GD.Print($"🎯 [AreneController] Event parsé - Type: {evt.Type}, Source: {evt.SourceCharacter}, Target: {evt.TargetCharacter}");
		
		// Gérer les événements spéciaux
		if (evt.Type == CombatEventType.BattleStart)
		{
			_battleState.IsActive = true;
			_battleState.StartTime = DateTime.Now;
			GD.Print("⚔️ [AreneController] Combat démarré !");
		}
		else if (evt.Type == CombatEventType.BattleEnd)
		{
			_battleState.IsActive = false;
			_battleState.EndTime = DateTime.Now;
			GD.Print("🏁 [AreneController] Combat terminé !");
		}
		
		// Mettre à jour les UIs des personnages
		UpdateCharacterUIs(evt);
	}
	
	private void OnEventParsed(CombatEvent evt)
	{
		// Créer les UIs pour les nouveaux personnages détectés
		if (evt.SourceCharacter != "" && !_characterUIs.ContainsKey(evt.SourceCharacter))
		{
			CreateOrUpdateCharacterUI(evt.SourceCharacter);
		}
		
		if (evt.TargetCharacter != "" && !_characterUIs.ContainsKey(evt.TargetCharacter))
		{
			CreateOrUpdateCharacterUI(evt.TargetCharacter);
		}
	}
	
	/// <summary>
	/// Initialise des cartes de test si aucune donnée n'arrive du backend
	/// </summary>
	private void InitializeTestCards()
	{
		// Utiliser les personnages sélectionnés ou des personnages par défaut
		var testCharacters = new List<(string name, string type)>();
		
		if (_selectedCharacters != null && _selectedCharacters.Count > 0)
		{
			// Utiliser les personnages sélectionnés
			foreach (var character in _selectedCharacters)
			{
				testCharacters.Add((character.Name, character.Type));
			}
			GD.Print($"🧪 [AreneController] Utilisation des {_selectedCharacters.Count} personnages sélectionnés");
		}
		else
		{
			// Personnages de test par défaut
			testCharacters = new List<(string name, string type)>
			{
				("Ragnar", "Berserker"),
				("Shadow", "Assassin"),
				("Lumière", "Pretre"),
				("Merlin", "Magicien")
			};
			GD.Print($"🧪 [AreneController] Utilisation des 4 personnages de test par défaut");
		}
		
		// Créer EXACTEMENT le nombre de cartes nécessaires
		var profileScene = GD.Load<PackedScene>("res://Components/Personnage.tscn");
		
		for (int i = 0; i < testCharacters.Count; i++)
		{
			var (name, type) = testCharacters[i];
			
			// Créer une nouvelle carte
			var ui = profileScene.Instantiate<PersonnageUIManager>();
			_profileGrid.AddChild(ui);
			
			// Initialiser avec les données
			ui.InitializeCharacter(name, type, 100);
			_characterUIs[name] = ui;
			
			GD.Print($"🧪 [AreneController] Carte créée: {name} ({type})");
		}
		
		GD.Print($"✅ [AreneController] {testCharacters.Count} cartes créées (pas de cartes vides)");
	}
	
	/// <summary>
	/// Cache toutes les cartes de personnages au démarrage
	/// </summary>
	private void HideAllCharacterCards()
	{
		var allUIs = _profileGrid.GetChildren().OfType<PersonnageUIManager>().ToList();
		foreach (var ui in allUIs)
		{
			ui.Visible = false;
		}
		GD.Print($"[AreneController] {allUIs.Count} cartes cachées en attente de données");
	}
	
	/// <summary>
	/// Crée ou met à jour l'UI d'un personnage
	/// </summary>
	private void CreateOrUpdateCharacterUI(string characterName)
	{
		if (_characterUIs.ContainsKey(characterName))
			return;
			
		// Récupérer le prochain UI disponible dans la grille
		var availableUI = FindAvailableCharacterUI();
		
		if (availableUI != null)
		{
			var character = _battleState.GetOrCreateCharacter(characterName);
			
			// Rendre la carte visible et l'initialiser
			availableUI.Visible = true;
			availableUI.InitializeCharacter(characterName, character.Type, character.MaxHP);
			_characterUIs[characterName] = availableUI;
			
			GD.Print($"[AreneController] ✅ UI créée pour {characterName} ({character.Type}) - HP: {character.MaxHP}");
		}
		else
		{
			GD.PrintErr($"[AreneController] ❌ Pas d'UI disponible pour {characterName}");
		}
	}
	
	/// <summary>
	/// Trouve un PersonnageUIManager disponible (pas encore assigné)
	/// </summary>
	private PersonnageUIManager FindAvailableCharacterUI()
	{
		var allUIs = _profileGrid.GetChildren()
			.OfType<PersonnageUIManager>()
			.ToList();
		
		foreach (var ui in allUIs)
		{
			if (!_characterUIs.ContainsValue(ui))
			{
				return ui;
			}
		}
		
		return null;
	}
	
	/// <summary>
	/// Met à jour les UIs selon l'événement de combat
	/// </summary>
	private void UpdateCharacterUIs(CombatEvent evt)
	{
		switch (evt.Type)
		{
			case CombatEventType.Attack:
				// L'attaquant cible quelqu'un
				if (_characterUIs.ContainsKey(evt.SourceCharacter))
				{
					_characterUIs[evt.SourceCharacter].SetFocus(evt.TargetCharacter);
					
					// Si on a les dégâts dans cet événement, enregistrer l'attaque
					if (evt.DamageAmount.HasValue && evt.DamageAmount.Value > 0)
					{
						_characterUIs[evt.SourceCharacter].RegisterAttack(evt.DamageAmount.Value, evt.TargetCharacter);
					}
					else
					{
						_characterUIs[evt.SourceCharacter].AddLog($"⚔️ Attaque {evt.TargetCharacter}");
					}
				}
				
				// La cible est attaquée
				if (_characterUIs.ContainsKey(evt.TargetCharacter))
				{
					_characterUIs[evt.TargetCharacter].SetAttacker(evt.SourceCharacter);
				}
				break;
				
			case CombatEventType.Damage:
				// Appliquer les dégâts à la cible
				if (evt.TargetCharacter != "" && evt.DamageAmount.HasValue)
				{
					if (_characterUIs.ContainsKey(evt.TargetCharacter))
					{
						_characterUIs[evt.TargetCharacter].TakeDamage(evt.DamageAmount.Value, evt.SourceCharacter);
					}
					
					// Enregistrer l'attaque pour l'attaquant s'il existe
					if (!string.IsNullOrEmpty(evt.SourceCharacter) && _characterUIs.ContainsKey(evt.SourceCharacter))
					{
						_characterUIs[evt.SourceCharacter].RegisterAttack(evt.DamageAmount.Value, evt.TargetCharacter);
					}
					
					// Mettre à jour depuis le BattleState
					if (_battleState.Characters.ContainsKey(evt.TargetCharacter))
					{
						var character = _battleState.Characters[evt.TargetCharacter];
						if (_characterUIs.ContainsKey(evt.TargetCharacter))
						{
							_characterUIs[evt.TargetCharacter].UpdateHP(character.CurrentHP);
						}
					}
				}
				break;
				
			case CombatEventType.Heal:
				if (evt.TargetCharacter != "" && evt.HealAmount.HasValue)
				{
					if (_characterUIs.ContainsKey(evt.TargetCharacter))
					{
						_characterUIs[evt.TargetCharacter].Heal(evt.HealAmount.Value, evt.SourceCharacter);
					}
				}
				break;
				
			case CombatEventType.Death:
				if (_characterUIs.ContainsKey(evt.SourceCharacter))
				{
					_characterUIs[evt.SourceCharacter].SetDead(true);
					_characterUIs[evt.SourceCharacter].AddLog("💀 EST MORT !");
				}
				break;
				
			case CombatEventType.SpecialAbility:
				if (_characterUIs.ContainsKey(evt.SourceCharacter))
				{
					_characterUIs[evt.SourceCharacter].AddLog($"✨ {evt.AbilityName}");
				}
				break;
			
			case CombatEventType.StatusEffect:
				// Déterminer si c'est un buff ou debuff basé sur le message ou l'effet
				if (_characterUIs.ContainsKey(evt.TargetCharacter) && !string.IsNullOrEmpty(evt.StatusEffect))
				{
					// Par défaut, considérer comme positif sauf si le message contient des mots-clés négatifs
					bool isPositive = !evt.StatusEffect.ToLower().Contains("poison") &&
					                  !evt.StatusEffect.ToLower().Contains("maudit") &&
					                  !evt.StatusEffect.ToLower().Contains("affaibli") &&
					                  !evt.StatusEffect.ToLower().Contains("stun");
					
					_characterUIs[evt.TargetCharacter].AddStatusEffect(evt.StatusEffect, isPositive);
				}
				else if (_characterUIs.ContainsKey(evt.SourceCharacter) && !string.IsNullOrEmpty(evt.AbilityName))
				{
					// Gérer comme une résurrection si le message le mentionne
					if (evt.RawMessage.ToLower().Contains("ressus") || evt.RawMessage.ToLower().Contains("revive"))
					{
						_characterUIs[evt.SourceCharacter].SetDead(false);
					}
					else
					{
						// Sinon, log simple
						_characterUIs[evt.SourceCharacter].AddLog(evt.RawMessage);
					}
				}
				break;
				
			case CombatEventType.DiceRoll:
				if (_characterUIs.ContainsKey(evt.SourceCharacter) && evt.DiceRoll.HasValue)
				{
					_characterUIs[evt.SourceCharacter].AddLog($"🎲 Dé: {evt.DiceRoll.Value}");
				}
				break;
				
			case CombatEventType.Winner:
				if (_characterUIs.ContainsKey(evt.SourceCharacter))
				{
					_characterUIs[evt.SourceCharacter].AddLog("🏆 VICTOIRE !");
				}
				break;
		}
	}
	
	/// <summary>
	/// Réinitialise l'arène pour un nouveau combat
	/// </summary>
	public void ResetArena()
	{
		foreach (var ui in _characterUIs.Values)
		{
			ui.SetDead(false);
			ui.UpdateHP(100);
			ui.SetFocus("");
			ui.SetAttacker("");
			ui.ClearEffects();
		}
		
		_characterUIs.Clear();
		_battleState = new BattleState();
		
		GD.Print("[AreneController] Arène réinitialisée");
	}
	
	public override void _ExitTree()
	{
		// Nettoyage
		if (_wsClient != null)
		{
			_wsClient.MessageReceived -= OnMessageReceived;
			_wsClient.ConnectionEstablished -= OnConnectionEstablished;
			_wsClient.ConnectionClosed -= OnConnectionClosed;
		}
		
		if (_parser != null)
		{
			_parser.EventParsed -= OnEventParsed;
		}
	}
}
