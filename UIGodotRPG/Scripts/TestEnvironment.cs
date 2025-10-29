using Godot;
using System;
using System.Collections.Generic;
using FrontBRRPG;
using FrontBRRPG.Network;
using FrontBRRPG.Combat;
using FrontBRRPG.UI;
using FrontBRRPG.Testing;

/// <summary>
/// Environnement de test pour tester les interactions avec le backend sans passer par le menu
/// NOUVEAU: Intégration du CombatSimulator pour tests UI en temps réel
/// </summary>
public partial class TestEnvironment : Control
{
	[Export] private int NumberOfTestCharacters = 4;
	[Export] private bool AutoConnect = false;
	[Export] private bool AutoStartBattle = false;
	[Export] private bool UseSimulator = true; // Nouveau: utiliser le simulateur au lieu du vrai backend
	
	private WebSocketClient _wsClient;
	private ProfileGrid _profileGrid;
	private CombatLogParser _parser;
	private BattleState _battleState;
	private Dictionary<string, PersonnageUIManager> _characterUIs = new Dictionary<string, PersonnageUIManager>();
	private ConnectionStatus _connectionStatus;
	private CombatSimulator _simulator; // Nouveau: simulateur de combat
	
	// Boutons existants
	private Label _statusLabel;
	private Button _connectButton;
	private Button _startBattleButton;
	private Button _sendTestEventButton;
	private RichTextLabel _logDisplay;
	
	// Nouveaux boutons de simulation
	private Button _simulatorToggleButton;
	private Button _testAttackButton;
	private Button _testHealButton;
	private Button _testDeathButton;
	private Button _testResurrectButton;
	private Button _testStatusButton;
	private HSlider _speedSlider;
	private Label _speedLabel;
	
	public override void _Ready()
	{
		GD.Print("🧪 [TestEnvironment] Initialisation de l'environnement de test");
		
		// Récupérer le WebSocketClient
		_wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
		
		// Récupérer la grille de profils
		_profileGrid = GetNode<ProfileGrid>("VBoxContainer/ScrollContainer/ProfileGrid");
		
		// Récupérer les contrôles UI existants
		_statusLabel = GetNode<Label>("VBoxContainer/HeaderContainer/StatusLabel");
		_connectButton = GetNode<Button>("VBoxContainer/ButtonsContainer/ConnectButton");
		_startBattleButton = GetNode<Button>("VBoxContainer/ButtonsContainer/StartBattleButton");
		_sendTestEventButton = GetNode<Button>("VBoxContainer/ButtonsContainer/SendTestEventButton");
		_logDisplay = GetNode<RichTextLabel>("VBoxContainer/LogDisplay");
		
		// NOUVEAU: Récupérer les contrôles du simulateur (optionnel, sera créé si absent)
		TryGetSimulatorControls();
		
		// Créer l'indicateur de connexion
		_connectionStatus = new ConnectionStatus();
		_connectionStatus.Position = new Vector2(10, 10);
		_connectionStatus.ZIndex = 100;
		AddChild(_connectionStatus);
		
		// Connecter les boutons existants
		_connectButton.Pressed += OnConnectButtonPressed;
		_startBattleButton.Pressed += OnStartBattleButtonPressed;
		_sendTestEventButton.Pressed += OnSendTestEventButtonPressed;
		
	// NOUVEAU: Initialiser le simulateur
	if (UseSimulator)
	{
		GD.Print("🎮 [TestEnvironment] Création du simulateur...");
		_simulator = new CombatSimulator();
		_simulator.ActionInterval = 1.5f; // Action toutes les 1.5s
		_simulator.AutoPlay = false; // Démarre en pause
		AddChild(_simulator);
		GD.Print("🎮 [TestEnvironment] Simulateur ajouté comme enfant");
		
		// Connecter les signaux du simulateur
		_simulator.CombatLog += OnSimulatorLog;
		GD.Print("🎮 [TestEnvironment] Signal CombatLog connecté");
		
		AddLog("🎮 Mode Simulateur activé - Pas de connexion backend nécessaire");
		AddLog("⌨️  Contrôles: 1=Attaque 2=Soin 3=Mort 4=Résurrection 5=Statut ESPACE=Play/Pause");
	}		// Initialiser le parser
		_parser = new CombatLogParser();
		AddChild(_parser);
		_parser.EventParsed += OnEventParsed;
		
		_battleState = new BattleState();
		
		// Connecter aux événements WebSocket
		_wsClient.MessageReceived += OnMessageReceived;
		_wsClient.ConnectionEstablished += OnConnectionEstablished;
		_wsClient.ConnectionClosed += OnConnectionClosed;
		
		// Créer les personnages de test
		CreateTestCharacters();
		
	// NOUVEAU: Initialiser le simulateur avec les personnages
	if (UseSimulator && _simulator != null)
	{
		GD.Print("🎮 [TestEnvironment] Initialisation du simulateur avec les personnages...");
		var profiles = _profileGrid.GetCharacterProfiles();
		GD.Print($"🎮 [TestEnvironment] Nombre de profils récupérés: {profiles.Count}");
		_simulator.Initialize(profiles);
		AddLog($"✅ Simulateur initialisé avec {profiles.Count} personnages");
		AddLog("⏸️  Simulateur en PAUSE - Appuyez sur ESPACE pour démarrer");
	}		// Connexion automatique si activée (et simulateur désactivé)
		if (!UseSimulator && AutoConnect && !_wsClient.IsConnected)
		{
			OnConnectButtonPressed();
		}
		
		UpdateUI();
	}
	
	private void CreateTestCharacters()
	{
		var testCharacters = new List<CharacterConfig>();
		
		var characterTypes = new[] { "Berserker", "Assassin", "Pretre", "Magicien", "Guerrier", "Paladin" };
		var characterNames = new[] { "Ragnar", "Shadow", "Lumière", "Merlin", "Conan", "Arthas" };
		
		for (int i = 0; i < Math.Min(NumberOfTestCharacters, characterTypes.Length); i++)
		{
			testCharacters.Add(new CharacterConfig
			{
				Name = characterNames[i],
				Type = characterTypes[i]
			});
		}
		
		// Initialiser les cartes
		_profileGrid.InitializeWithCharacters(testCharacters);
		
		// Enregistrer les cartes dans le dictionnaire
		var profiles = _profileGrid.GetCharacterProfiles();
		for (int i = 0; i < testCharacters.Count && i < profiles.Count; i++)
		{
			var characterName = testCharacters[i].Name;
			_characterUIs[characterName] = profiles[i];
			GD.Print($"🧪 [TestEnvironment] Carte créée: {characterName} ({testCharacters[i].Type})");
		}
		
		AddLog($"✅ {testCharacters.Count} personnages de test créés");
	}
	
	private void OnConnectButtonPressed()
	{
		if (_wsClient.IsConnected)
		{
			_wsClient.Disconnect();
			AddLog("❌ Déconnexion...");
		}
		else
		{
			_wsClient.ConnectToServer();
			AddLog("🔌 Connexion au serveur WebSocket...");
		}
	}
	
	private void OnStartBattleButtonPressed()
	{
		if (!_wsClient.IsConnected)
		{
			AddLog("⚠️ Impossible de démarrer le combat : non connecté");
			return;
		}
		
		var characters = new List<CharacterConfig>();
		foreach (var kvp in _characterUIs)
		{
			characters.Add(new CharacterConfig
			{
				Name = kvp.Key,
				Type = kvp.Value.CharacterData.Class.ToString()
			});
		}
		
		_wsClient.StartBattle(characters);
		AddLog($"⚔️ Démarrage du combat avec {characters.Count} personnages");
	}
	
	private void OnSendTestEventButtonPressed()
	{
		// Simuler un événement de test (dégâts aléatoires)
		if (_characterUIs.Count >= 2)
		{
			var characters = new List<string>(_characterUIs.Keys);
			var random = new Random();
			
			var attacker = characters[random.Next(characters.Count)];
			var target = characters[random.Next(characters.Count)];
			
			if (attacker != target && _characterUIs.ContainsKey(attacker) && _characterUIs.ContainsKey(target))
			{
				var damage = random.Next(10, 30);
				_characterUIs[target].TakeDamage(damage, attacker);
				_characterUIs[attacker].RegisterAttack(damage, target);
				
				AddLog($"🎲 Événement test: {attacker} attaque {target} pour {damage} dégâts");
			}
		}
	}
	
	private void OnConnectionEstablished()
	{
		AddLog("✅ Connecté au serveur WebSocket");
		UpdateUI();
		
		// Démarrer automatiquement le combat si activé
		if (AutoStartBattle)
		{
			CallDeferred(nameof(OnStartBattleButtonPressed));
		}
	}
	
	private void OnConnectionClosed(string reason)
	{
		AddLog($"❌ Connexion fermée: {reason}");
		UpdateUI();
	}
	
	private void OnMessageReceived(string message)
	{
		AddLog($"📨 Message reçu: {message.Substring(0, Math.Min(100, message.Length))}...");
		_parser.ParseMessage(message);
	}
	
	private void OnEventParsed(CombatEvent evt)
	{
		AddLog($"🎯 Événement parsé: {evt.Type} | Source: {evt.SourceCharacter} | Target: {evt.TargetCharacter}");
		
		// Créer les UIs pour les nouveaux personnages détectés
		if (evt.SourceCharacter != "" && !_characterUIs.ContainsKey(evt.SourceCharacter))
		{
			AddLog($"⚠️ Personnage inconnu détecté: {evt.SourceCharacter}");
		}
		
		if (evt.TargetCharacter != "" && !_characterUIs.ContainsKey(evt.TargetCharacter))
		{
			AddLog($"⚠️ Personnage inconnu détecté: {evt.TargetCharacter}");
		}
		
		// Mettre à jour les UIs selon l'événement
		UpdateCharacterUIs(evt);
	}
	
	private void UpdateCharacterUIs(CombatEvent evt)
	{
		switch (evt.Type)
		{
			case CombatEventType.Attack:
				if (_characterUIs.ContainsKey(evt.SourceCharacter))
				{
					_characterUIs[evt.SourceCharacter].SetFocus(evt.TargetCharacter);
					
					if (evt.DamageAmount.HasValue && evt.DamageAmount.Value > 0)
					{
						_characterUIs[evt.SourceCharacter].RegisterAttack(evt.DamageAmount.Value, evt.TargetCharacter);
					}
				}
				
				if (_characterUIs.ContainsKey(evt.TargetCharacter))
				{
					_characterUIs[evt.TargetCharacter].SetAttacker(evt.SourceCharacter);
				}
				break;
				
			case CombatEventType.Damage:
				if (evt.TargetCharacter != "" && evt.DamageAmount.HasValue)
				{
					if (_characterUIs.ContainsKey(evt.TargetCharacter))
					{
						_characterUIs[evt.TargetCharacter].TakeDamage(evt.DamageAmount.Value, evt.SourceCharacter);
					}
					
					if (!string.IsNullOrEmpty(evt.SourceCharacter) && _characterUIs.ContainsKey(evt.SourceCharacter))
					{
						_characterUIs[evt.SourceCharacter].RegisterAttack(evt.DamageAmount.Value, evt.TargetCharacter);
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
					AddLog($"💀 {evt.SourceCharacter} est mort !");
				}
				break;
				
			case CombatEventType.StatusEffect:
				if (_characterUIs.ContainsKey(evt.TargetCharacter) && !string.IsNullOrEmpty(evt.StatusEffect))
				{
					bool isPositive = evt.StatusEffect.Contains("buff") || evt.StatusEffect.Contains("heal");
					_characterUIs[evt.TargetCharacter].AddStatusEffect(evt.StatusEffect, isPositive);
					AddLog($"✨ {evt.TargetCharacter} a reçu l'effet: {evt.StatusEffect}");
				}
				break;
		}
	}
	
	private void UpdateUI()
	{
		bool connected = _wsClient.IsConnected;
		
		_connectButton.Text = connected ? "🔌 Déconnecter" : "🔌 Connecter";
		_startBattleButton.Disabled = !connected;
		_sendTestEventButton.Disabled = !connected;
		
		string status = connected ? "✅ Connecté" : "❌ Déconnecté";
		_statusLabel.Text = $"Status: {status} | Personnages: {_characterUIs.Count}";
	}
	
	private void AddLog(string message)
	{
		string timestamp = DateTime.Now.ToString("HH:mm:ss");
		_logDisplay.Text += $"[{timestamp}] {message}\n";
		
		// Limiter le nombre de lignes
		var lines = _logDisplay.Text.Split('\n');
		if (lines.Length > 50)
		{
			_logDisplay.Text = string.Join("\n", lines[^50..]);
		}
		
		// Auto-scroll
		CallDeferred("scroll_to_end");
	}
	
	private void scroll_to_end()
	{
		// Godot 4: utiliser GetVScrollBar().Value
		var vScrollBar = _logDisplay.GetVScrollBar();
		if (vScrollBar != null)
		{
			vScrollBar.Value = vScrollBar.MaxValue;
		}
	}
	
	public override void _ExitTree()
	{
		_wsClient.MessageReceived -= OnMessageReceived;
		_wsClient.ConnectionEstablished -= OnConnectionEstablished;
		_wsClient.ConnectionClosed -= OnConnectionClosed;
		
		if (_parser != null)
		{
			_parser.EventParsed -= OnEventParsed;
		}
		
		if (_simulator != null)
		{
			_simulator.CombatLog -= OnSimulatorLog;
		}
	}
	
	// ========== NOUVELLES MÉTHODES DU SIMULATEUR ==========
	
	/// <summary>
	/// Récupère les contrôles du simulateur s'ils existent dans la scène
	/// </summary>
	private void TryGetSimulatorControls()
	{
		// Ces contrôles seront ajoutés à la scène TestEnvironment.tscn
		// Pour l'instant on les ignore s'ils n'existent pas
		try
		{
			_simulatorToggleButton = GetNodeOrNull<Button>("VBoxContainer/SimulatorContainer/ToggleButton");
			_testAttackButton = GetNodeOrNull<Button>("VBoxContainer/SimulatorContainer/TestAttackButton");
			_testHealButton = GetNodeOrNull<Button>("VBoxContainer/SimulatorContainer/TestHealButton");
			_testDeathButton = GetNodeOrNull<Button>("VBoxContainer/SimulatorContainer/TestDeathButton");
			_testResurrectButton = GetNodeOrNull<Button>("VBoxContainer/SimulatorContainer/TestResurrectButton");
			_testStatusButton = GetNodeOrNull<Button>("VBoxContainer/SimulatorContainer/TestStatusButton");
			_speedSlider = GetNodeOrNull<HSlider>("VBoxContainer/SimulatorContainer/SpeedSlider");
			_speedLabel = GetNodeOrNull<Label>("VBoxContainer/SimulatorContainer/SpeedLabel");
			
			// Connecter les signaux si les boutons existent
			if (_simulatorToggleButton != null)
				_simulatorToggleButton.Pressed += OnSimulatorTogglePressed;
			if (_testAttackButton != null)
				_testAttackButton.Pressed += () => _simulator?.TestAttack();
			if (_testHealButton != null)
				_testHealButton.Pressed += () => _simulator?.TestHeal();
			if (_testDeathButton != null)
				_testDeathButton.Pressed += () => _simulator?.TestDeath();
			if (_testResurrectButton != null)
				_testResurrectButton.Pressed += () => _simulator?.TestResurrection();
			if (_testStatusButton != null)
				_testStatusButton.Pressed += () => _simulator?.TestStatusEffect();
			if (_speedSlider != null)
				_speedSlider.ValueChanged += OnSpeedChanged;
		}
		catch (Exception)
		{
			// Les contrôles n'existent pas encore, ce n'est pas grave
			GD.Print("[TestEnvironment] Contrôles simulateur non trouvés (utiliser touches clavier à la place)");
		}
	}
	
	/// <summary>
	/// Callback pour les logs du simulateur
	/// </summary>
	private void OnSimulatorLog(string message, string colorHex)
	{
		AddLog(message);
	}
	
	/// <summary>
	/// Toggle play/pause du simulateur
	/// </summary>
	private void OnSimulatorTogglePressed()
	{
		if (_simulator == null) return;
		
		// Implémentation simplifiée: on démarre/arrête via des boutons manuels pour l'instant
		AddLog("⏯️ Toggle simulateur (utilisez les boutons de test manuel)");
	}
	
	/// <summary>
	/// Change la vitesse de simulation
	/// </summary>
	private void OnSpeedChanged(double value)
	{
		if (_simulator == null) return;
		
		_simulator.SetSpeed((float)value);
		if (_speedLabel != null)
			_speedLabel.Text = $"Vitesse: {value:F1}x";
	}
	
	/// <summary>
	/// Gère les inputs clavier pour les tests rapides
	/// </summary>
	public override void _Input(InputEvent @event)
	{
		if (!UseSimulator || _simulator == null) return;
		
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			switch (keyEvent.Keycode)
			{
				case Key.Key1:
					_simulator.TestAttack();
					AddLog("🎮 Test: Attaque (touche 1)");
					break;
				case Key.Key2:
					_simulator.TestHeal();
					AddLog("🎮 Test: Soin (touche 2)");
					break;
				case Key.Key3:
					_simulator.TestDeath();
					AddLog("🎮 Test: Mort (touche 3)");
					break;
				case Key.Key4:
					_simulator.TestResurrection();
					AddLog("🎮 Test: Résurrection (touche 4)");
					break;
				case Key.Key5:
					_simulator.TestStatusEffect();
					AddLog("🎮 Test: Effet de statut (touche 5)");
					break;
				case Key.Space:
					if (_simulator.AutoPlay)
						_simulator.Pause();
					else
						_simulator.Start();
					AddLog($"🎮 Simulation: {(_simulator.AutoPlay ? "▶️ Démarrée" : "⏸️ Pausée")}");
					break;
			}
		}
	}
}
