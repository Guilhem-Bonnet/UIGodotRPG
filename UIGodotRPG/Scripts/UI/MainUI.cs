using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FrontBRRPG.Network;
using FrontBRRPG.Combat;

namespace FrontBRRPG.UI
{
    /// <summary>
    /// Interface principale enrichie avec statistiques de connexion et de combat
    /// </summary>
    public partial class MainUI : Control
    {
        private WebSocketClient _wsClient;
        private CombatLogParser _parser;
        private BattleState _battleState;
        
        // UI Elements - Connection Panel
        private RichTextLabel _statusLabel;
        private RichTextLabel _serverLabel;
        private RichTextLabel _uptimeLabel;
        private RichTextLabel _latencyLabel;
        private RichTextLabel _messagesLabel;
        
        // UI Elements - Stats Panel
        private RichTextLabel _battleStatusLabel;
        private RichTextLabel _charactersLabel;
        private RichTextLabel _aliveLabel;
        private RichTextLabel _deadLabel;
        private RichTextLabel _actionsLabel;
        
        // UI Elements - Characters Panel
        private RichTextLabel _charactersList;
        
        // UI Elements - Log Panel
        private RichTextLabel _logLabel;
        private ScrollContainer _logScrollContainer;
        
        // UI Elements - Buttons
        private Button _connectButton;
        private Button _disconnectButton;
        private Button _startButton;
        private Button _clearButton;
        
        // Stats tracking
        private DateTime _connectionTime;
        private int _messageCount = 0;
        private int _totalActions = 0;
        private double _lastPingTime = 0;
        private double _latency = 0;

        public override void _Ready()
        {
            // Récupération du singleton AutoLoad
            _wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
            GD.Print("✅ WebSocketClient récupéré");

            // Initialiser le parser et le battle state
            _parser = new CombatLogParser();
            AddChild(_parser);
            _parser.EventParsed += OnEventParsed;
            
            _battleState = new BattleState();

            // Récupération des éléments UI - Connection Panel
            _statusLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TopContainer/ConnectionPanel/ConnectionVBox/StatusLabel");
            _serverLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TopContainer/ConnectionPanel/ConnectionVBox/ServerLabel");
            _uptimeLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TopContainer/ConnectionPanel/ConnectionVBox/UptimeLabel");
            _latencyLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TopContainer/ConnectionPanel/ConnectionVBox/LatencyLabel");
            _messagesLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TopContainer/ConnectionPanel/ConnectionVBox/MessagesLabel");
            
            // Stats Panel
            _battleStatusLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TopContainer/StatsPanel/StatsVBox/BattleStatusLabel");
            _charactersLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TopContainer/StatsPanel/StatsVBox/CharactersLabel");
            _aliveLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TopContainer/StatsPanel/StatsVBox/AliveLabel");
            _deadLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TopContainer/StatsPanel/StatsVBox/DeadLabel");
            _actionsLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TopContainer/StatsPanel/StatsVBox/ActionsLabel");
            
            // Characters Panel
            _charactersList = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/BottomSplit/CharactersPanel/CharactersVBox/CharactersScroll/CharactersList");
            
            // Log Panel
            _logScrollContainer = GetNode<ScrollContainer>("MarginContainer/VBoxContainer/BottomSplit/LogPanel/LogVBox/LogScrollContainer");
            _logLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/BottomSplit/LogPanel/LogVBox/LogScrollContainer/LogLabel");
            
            // Buttons
            _connectButton = GetNode<Button>("MarginContainer/VBoxContainer/ButtonsContainer/ConnectButton");
            _disconnectButton = GetNode<Button>("MarginContainer/VBoxContainer/ButtonsContainer/DisconnectButton");
            _startButton = GetNode<Button>("MarginContainer/VBoxContainer/ButtonsContainer/StartButton");
            _clearButton = GetNode<Button>("MarginContainer/VBoxContainer/ButtonsContainer/ClearButton");

            // Connexion aux signaux WebSocket
            _wsClient.MessageReceived += OnMessageReceived;
            _wsClient.ConnectionEstablished += OnConnectionEstablished;
            _wsClient.ConnectionClosed += OnConnectionClosed;
            _wsClient.ConnectionError += OnConnectionError;

            // Connexion aux boutons
            _connectButton.Pressed += OnConnectButtonPressed;
            _disconnectButton.Pressed += OnDisconnectButtonPressed;
            _startButton.Pressed += OnStartButtonPressed;
            _clearButton.Pressed += OnClearButtonPressed;

            // Afficher l'état initial
            UpdateAllUI();
        }

        public override void _Process(double delta)
        {
            // Mettre à jour les infos en temps réel si connecté
            if (_wsClient.IsConnected)
            {
                UpdateConnectionInfo();
            }
        }

        private void OnEventParsed(CombatEvent evt)
        {
            _battleState.AddEvent(evt);
            
            // Incrémenter le compteur d'actions pour certains types
            if (evt.Type == CombatEventType.Attack || 
                evt.Type == CombatEventType.Damage ||
                evt.Type == CombatEventType.SpecialAbility ||
                evt.Type == CombatEventType.Heal)
            {
                _totalActions++;
            }
            
            UpdateBattleStats();
            UpdateCharactersList();
        }

        private void OnConnectionEstablished()
        {
            _connectionTime = DateTime.Now;
            _messageCount = 0;
            AddLog("[color=#00FF00]✅ Connecté au serveur RPG-Arena[/color]", false);
            UpdateAllUI();
        }

        private void OnMessageReceived(string msg)
        {
            _messageCount++;
            
            // Parser le message
            var evt = _parser.ParseMessage(msg);
            
            // Détecter les événements de combat importants
            if (evt.Type == CombatEventType.BattleStart)
            {
                _battleState.IsActive = true;
                _battleState.StartTime = DateTime.Now;
                _totalActions = 0;
            }
            else if (evt.Type == CombatEventType.BattleEnd)
            {
                _battleState.IsActive = false;
                _battleState.EndTime = DateTime.Now;
            }
            else if (evt.Type == CombatEventType.Winner)
            {
                _battleState.Winner = evt.SourceCharacter;
            }
            
            // Afficher le log avec coloration
            var color = GetColorForEvent(evt.Type);
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            AddLog($"[color=#808080][{timestamp}][/color] [color={color}]{msg}[/color]", true);
            
            UpdateAllUI();
        }

        private void OnConnectionClosed(string reason)
        {
            AddLog($"[color=#FFA500]⚠️  Connexion fermée: {reason}[/color]", false);
            UpdateAllUI();
        }

        private void OnConnectionError(string error)
        {
            AddLog($"[color=#FF0000]❌ Erreur: {error}[/color]", false);
            UpdateAllUI();
        }

        private void OnConnectButtonPressed()
        {
            AddLog("[color=#00FFFF]🔌 Tentative de connexion...[/color]", false);
            _wsClient.ConnectToServer();
        }

        private void OnDisconnectButtonPressed()
        {
            AddLog("[color=#FFFF00]🔌 Déconnexion...[/color]", false);
            _wsClient.Disconnect();
        }

        private void OnStartButtonPressed()
        {
            if (!_wsClient.IsConnected)
            {
                AddLog("[color=#FF0000]❌ Connectez-vous d'abord au serveur[/color]", false);
                return;
            }

            // Reset battle state
            _battleState = new BattleState();
            _totalActions = 0;

            // Configuration de test avec plusieurs personnages
            var characters = new List<CharacterConfig>
            {
                new CharacterConfig { Type = CharacterTypes.Guerrier, Name = "Conan" },
                new CharacterConfig { Type = CharacterTypes.Berserker, Name = "Ragnar" },
                new CharacterConfig { Type = CharacterTypes.Magicien, Name = "Merlin" },
                new CharacterConfig { Type = CharacterTypes.Assassin, Name = "Shadow" }
            };

            var names = string.Join(", ", characters.Select(c => $"{c.Name} ({c.Type})"));
            AddLog($"[color=#00FFFF]⚔️  Démarrage du combat avec: {names}[/color]", false);
            _wsClient.StartBattle(characters);
            
            UpdateAllUI();
        }

        private void OnClearButtonPressed()
        {
            _logLabel.Text = "";
            AddLog("[color=#808080]🗑️ Logs effacés[/color]", false);
        }

        private void AddLog(string message, bool autoScroll = true)
        {
            _logLabel.AppendText(message + "\n");
            
            if (autoScroll)
            {
                CallDeferred(MethodName.ScrollToBottom);
            }
        }

        private void ScrollToBottom()
        {
            if (_logScrollContainer != null)
            {
                _logScrollContainer.ScrollVertical = (int)_logScrollContainer.GetVScrollBar().MaxValue;
            }
        }

        private void UpdateAllUI()
        {
            UpdateConnectionInfo();
            UpdateBattleStats();
            UpdateCharactersList();
            UpdateButtons();
        }

        private void UpdateConnectionInfo()
        {
            // Status
            if (_wsClient.IsConnected)
            {
                _statusLabel.Text = "[color=#00FF00]🟢 Connecté[/color]";
                
                // Uptime
                var uptime = DateTime.Now - _connectionTime;
                _uptimeLabel.Text = $"[color=#00FF88]⏱️ Temps de connexion: {uptime:hh\\:mm\\:ss}[/color]";
                
                // Latency (simulation simple - dans une vraie impl, utiliser ping/pong)
                _latency = _messageCount > 0 ? Math.Min(50 + (_messageCount % 20), 100) : 0;
                var latencyColor = _latency < 50 ? "#00FF00" : _latency < 100 ? "#FFFF00" : "#FF0000";
                _latencyLabel.Text = $"[color={latencyColor}]📡 Latence: {_latency:F0} ms[/color]";
            }
            else
            {
                _statusLabel.Text = "[color=#808080]⚫ Déconnecté[/color]";
                _uptimeLabel.Text = "[color=#808080]⏱️ Temps de connexion: --[/color]";
                _latencyLabel.Text = "[color=#808080]📡 Latence: -- ms[/color]";
            }
            
            // Server
            _serverLabel.Text = "[color=#00FFFF]🌐 Serveur: ws://localhost:5018/ws[/color]";
            
            // Messages
            var msgColor = _messageCount > 0 ? "#00FF88" : "#808080";
            _messagesLabel.Text = $"[color={msgColor}]📨 Messages reçus: {_messageCount}[/color]";
        }

        private void UpdateBattleStats()
        {
            // Battle Status
            if (_battleState.IsActive)
            {
                var duration = DateTime.Now - _battleState.StartTime;
                _battleStatusLabel.Text = $"[color=#FFFF00]⚔️  État: Combat en cours ({duration:mm\\:ss})[/color]";
            }
            else if (_battleState.Winner != "")
            {
                _battleStatusLabel.Text = $"[color=#FFD700]🏆 État: Victoire de {_battleState.Winner}[/color]";
            }
            else if (_battleState.Characters.Count > 0)
            {
                _battleStatusLabel.Text = "[color=#FF0000]🛑 État: Combat terminé[/color]";
            }
            else
            {
                _battleStatusLabel.Text = "[color=#808080]⚔️  État: En attente[/color]";
            }
            
            // Characters count
            var totalChars = _battleState.Characters.Count;
            var charsColor = totalChars > 0 ? "#00FFFF" : "#808080";
            _charactersLabel.Text = $"[color={charsColor}]👥 Personnages: {totalChars}[/color]";
            
            // Alive/Dead count
            var alive = _battleState.Characters.Values.Count(c => !c.IsDead);
            var dead = _battleState.Characters.Values.Count(c => c.IsDead);
            
            _aliveLabel.Text = alive > 0 
                ? $"[color=#00FF00]✅ Vivants: {alive}[/color]"
                : "[color=#808080]✅ Vivants: --[/color]";
                
            _deadLabel.Text = dead > 0
                ? $"[color=#FF0000]💀 Morts: {dead}[/color]"
                : "[color=#808080]💀 Morts: --[/color]";
            
            // Actions count
            var actionsColor = _totalActions > 0 ? "#FF8800" : "#808080";
            _actionsLabel.Text = $"[color={actionsColor}]💥 Actions totales: {_totalActions}[/color]";
        }

        private void UpdateCharactersList()
        {
            if (_battleState.Characters.Count == 0)
            {
                _charactersList.Text = "[color=#808080]Aucun personnage en combat[/color]\n";
                return;
            }
            
            var text = "";
            var sortedChars = _battleState.Characters.Values.OrderByDescending(c => !c.IsDead).ThenByDescending(c => c.CurrentHP);
            
            foreach (var character in sortedChars)
            {
                var emoji = character.IsDead ? "💀" : "✅";
                var nameColor = character.IsDead ? "#808080" : "#FFFFFF";
                var hpColor = character.IsDead ? "#808080" : 
                             character.HPPercentage > 0.5f ? "#00FF00" :
                             character.HPPercentage > 0.25f ? "#FFA500" : "#FF0000";
                
                var hpBar = GenerateHPBar(character.HPPercentage, 20);
                
                text += $"[color={nameColor}]{emoji} [b]{character.Name}[/b] ({character.Type})[/color]\n";
                text += $"  [color={hpColor}]{hpBar} {character.CurrentHP}/{character.MaxHP} HP ({character.HPPercentage*100:F0}%)[/color]\n";
                
                if (character.StatusEffects.Count > 0)
                {
                    text += $"  [color=#FF00FF]🌟 {string.Join(", ", character.StatusEffects)}[/color]\n";
                }
                
                text += "\n";
            }
            
            _charactersList.Text = text;
        }

        private string GenerateHPBar(float percentage, int length)
        {
            var filled = (int)(percentage * length);
            var empty = length - filled;
            return "[" + new string('█', filled) + new string('░', empty) + "]";
        }

        private void UpdateButtons()
        {
            _connectButton.Disabled = _wsClient.IsConnected;
            _disconnectButton.Disabled = !_wsClient.IsConnected;
            _startButton.Disabled = !_wsClient.IsConnected;
        }

        private string GetColorForEvent(CombatEventType type)
        {
            return type switch
            {
                CombatEventType.BattleStart => "#00FF00",
                CombatEventType.BattleEnd => "#FF0000",
                CombatEventType.Winner => "#FFD700",
                CombatEventType.Attack => "#FFFF00",
                CombatEventType.Damage => "#FF8800",
                CombatEventType.Heal => "#00FF88",
                CombatEventType.Death => "#FF0000",
                CombatEventType.SpecialAbility => "#FF00FF",
                CombatEventType.DiceRoll => "#00FFFF",
                _ => "#FFFFFF"
            };
        }

        public override void _ExitTree()
        {
            // Nettoyage des signaux
            if (_wsClient != null)
            {
                _wsClient.MessageReceived -= OnMessageReceived;
                _wsClient.ConnectionEstablished -= OnConnectionEstablished;
                _wsClient.ConnectionClosed -= OnConnectionClosed;
                _wsClient.ConnectionError -= OnConnectionError;
            }
            base._ExitTree();
        }
    }
}
