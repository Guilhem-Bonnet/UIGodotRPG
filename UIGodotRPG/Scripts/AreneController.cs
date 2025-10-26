using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FrontBRRPG.Network;
using FrontBRRPG.Combat;

/// <summary>
/// Contrôleur de l'arène qui synchronise les personnages avec le WebSocket
/// </summary>
public partial class AreneController : Control
{
	private WebSocketClient _wsClient;
	private CombatLogParser _parser;
	private BattleState _battleState;
	private ProfileGrid _profileGrid;
	
	// Dictionnaire pour associer les noms aux UI
	private Dictionary<string, PersonnageUIManager> _characterUIs = new Dictionary<string, PersonnageUIManager>();
	
	public override void _Ready()
	{
		// Récupérer le WebSocketClient AutoLoad
		_wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
		
		// Récupérer la grille de profils
		_profileGrid = GetNode<ProfileGrid>("ScrollContainer/GridContainer");
		
		// Initialiser le parser
		_parser = new CombatLogParser();
		AddChild(_parser);
		_parser.EventParsed += OnEventParsed;
		
		_battleState = new BattleState();
		
		// Connecter aux événements WebSocket
		_wsClient.MessageReceived += OnMessageReceived;
		_wsClient.ConnectionEstablished += OnConnectionEstablished;
		_wsClient.ConnectionClosed += OnConnectionClosed;
		
		GD.Print("[AreneController] Initialisé et connecté au WebSocket");
	}
	
	private void OnConnectionEstablished()
	{
		GD.Print("[AreneController] WebSocket connecté");
	}
	
	private void OnConnectionClosed(string reason)
	{
		GD.Print($"[AreneController] WebSocket fermé: {reason}");
	}
	
	private void OnMessageReceived(string message)
	{
		// Parser le message
		var evt = _parser.ParseMessage(message);
		_battleState.AddEvent(evt);
		
		// Gérer les événements spéciaux
		if (evt.Type == CombatEventType.BattleStart)
		{
			_battleState.IsActive = true;
			_battleState.StartTime = DateTime.Now;
			GD.Print("[AreneController] Combat démarré");
		}
		else if (evt.Type == CombatEventType.BattleEnd)
		{
			_battleState.IsActive = false;
			_battleState.EndTime = DateTime.Now;
			GD.Print("[AreneController] Combat terminé");
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
			availableUI.InitializeCharacter(characterName, character.Type, character.MaxHP);
			_characterUIs[characterName] = availableUI;
			
			GD.Print($"[AreneController] UI créée pour {characterName}");
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
					_characterUIs[evt.SourceCharacter].AddLog($"⚔️ Attaque {evt.TargetCharacter}");
				}
				
				// La cible est attaquée
				if (_characterUIs.ContainsKey(evt.TargetCharacter))
				{
					_characterUIs[evt.TargetCharacter].SetAttacker(evt.SourceCharacter);
				}
				break;
				
			case CombatEventType.Damage:
				// Appliquer les dégâts
				if (evt.TargetCharacter != "" && evt.DamageAmount.HasValue)
				{
					if (_characterUIs.ContainsKey(evt.TargetCharacter))
					{
						_characterUIs[evt.TargetCharacter].TakeDamage(evt.DamageAmount.Value);
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
						_characterUIs[evt.TargetCharacter].Heal(evt.HealAmount.Value);
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
