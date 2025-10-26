using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FrontBRRPG.Network;
using FrontBRRPG.Combat;

namespace FrontBRRPG.UI
{
    /// <summary>
    /// Affiche l'état du combat en temps réel avec les HP, actions, et statuts
    /// </summary>
    public partial class BattleViewer : Control
    {
        private VBoxContainer _charactersContainer;
        private RichTextLabel _combatLogLabel;
        private ScrollContainer _combatLogScroll;
        private Label _battleStatusLabel;

        private BattleState _battleState = new BattleState();
        private CombatLogParser _logParser;
        private Dictionary<string, CharacterDisplay> _characterDisplays = new Dictionary<string, CharacterDisplay>();

        public override void _Ready()
        {
            _charactersContainer = GetNode<VBoxContainer>("MarginContainer/HSplitContainer/LeftPanel/ScrollContainer/CharactersContainer");
            _combatLogScroll = GetNode<ScrollContainer>("MarginContainer/HSplitContainer/RightPanel/VBoxContainer/CombatLogScroll");
            _combatLogLabel = GetNode<RichTextLabel>("MarginContainer/HSplitContainer/RightPanel/VBoxContainer/CombatLogScroll/CombatLogLabel");
            _battleStatusLabel = GetNode<Label>("MarginContainer/HSplitContainer/RightPanel/VBoxContainer/BattleStatusLabel");

            // Créer le parser
            _logParser = new CombatLogParser();
            AddChild(_logParser);
            _logParser.EventParsed += OnEventParsed;

            // Connexion au WebSocket
            var wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
            wsClient.MessageReceived += OnMessageReceived;
            wsClient.ConnectionEstablished += OnConnectionEstablished;
            wsClient.ConnectionClosed += OnConnectionClosed;

            UpdateBattleStatus("En attente de connexion...");
        }

        private void OnConnectionEstablished()
        {
            UpdateBattleStatus("Connecté - En attente du combat");
        }

        private void OnConnectionClosed(string reason)
        {
            UpdateBattleStatus($"Déconnecté: {reason}");
        }

        private void OnMessageReceived(string message)
        {
            // Parser le message
            var evt = _logParser.ParseMessage(message);
            
            // Mettre à jour l'état du combat
            _battleState.AddEvent(evt);

            // Afficher dans le log
            AddCombatLog(message, evt);

            // Mettre à jour l'affichage des personnages
            UpdateCharacterDisplays();

            // Mettre à jour le statut
            if (evt.Type == CombatEventType.BattleStart)
            {
                _battleState.IsActive = true;
                _battleState.StartTime = DateTime.Now;
                UpdateBattleStatus("🟢 Combat en cours...");
            }
            else if (evt.Type == CombatEventType.BattleEnd)
            {
                _battleState.IsActive = false;
                _battleState.EndTime = DateTime.Now;
                UpdateBattleStatus("🛑 Combat terminé");
            }
            else if (evt.Type == CombatEventType.Winner)
            {
                _battleState.Winner = evt.SourceCharacter;
                UpdateBattleStatus($"🏆 Vainqueur: {evt.SourceCharacter}");
            }
        }

        private void OnEventParsed(CombatEvent evt)
        {
            // Créer les affichages de personnages si nécessaire
            if (evt.SourceCharacter != "" && !_characterDisplays.ContainsKey(evt.SourceCharacter))
            {
                CreateCharacterDisplay(evt.SourceCharacter);
            }
            if (evt.TargetCharacter != "" && !_characterDisplays.ContainsKey(evt.TargetCharacter))
            {
                CreateCharacterDisplay(evt.TargetCharacter);
            }
        }

        private void CreateCharacterDisplay(string characterName)
        {
            var character = _battleState.GetOrCreateCharacter(characterName);
            var display = new CharacterDisplay(character);
            _charactersContainer.AddChild(display);
            _characterDisplays[characterName] = display;
            
            GD.Print($"[BattleViewer] Affichage créé pour {characterName}");
        }

        private void UpdateCharacterDisplays()
        {
            foreach (var kvp in _battleState.Characters)
            {
                if (_characterDisplays.ContainsKey(kvp.Key))
                {
                    _characterDisplays[kvp.Key].UpdateDisplay(kvp.Value);
                }
            }
        }

        private void AddCombatLog(string rawMessage, CombatEvent evt)
        {
            var color = GetColorForEventType(evt.Type);
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[color=#808080]{timestamp}[/color] [color={color}]{rawMessage}[/color]\n";
            
            _combatLogLabel.Text += logEntry;
            
            // Auto-scroll en bas
            CallDeferred(nameof(ScrollToBottom));
        }

        private void ScrollToBottom()
        {
            _combatLogScroll.ScrollVertical = (int)_combatLogScroll.GetVScrollBar().MaxValue;
        }

        private string GetColorForEventType(CombatEventType type)
        {
            return type switch
            {
                CombatEventType.BattleStart => "#00FF00",
                CombatEventType.BattleEnd => "#FF0000",
                CombatEventType.Attack => "#FFFF00",
                CombatEventType.Damage => "#FF8800",
                CombatEventType.Heal => "#00FF88",
                CombatEventType.Death => "#FF0000",
                CombatEventType.Winner => "#FFD700",
                CombatEventType.SpecialAbility => "#FF00FF",
                CombatEventType.DiceRoll => "#00FFFF",
                _ => "#FFFFFF"
            };
        }

        private void UpdateBattleStatus(string status)
        {
            _battleStatusLabel.Text = status;
        }

        public void StartNewBattle(List<CharacterConfig> characters)
        {
            // Réinitialiser
            _battleState = new BattleState();
            _combatLogLabel.Text = "";
            
            // Supprimer les anciens affichages
            foreach (var display in _characterDisplays.Values)
            {
                display.QueueFree();
            }
            _characterDisplays.Clear();

            // Créer les affichages pour chaque personnage
            foreach (var character in characters)
            {
                var state = _battleState.GetOrCreateCharacter(character.Name, character.Type);
                CreateCharacterDisplay(character.Name);
            }

            UpdateBattleStatus("En attente du début du combat...");
            GD.Print($"[BattleViewer] Nouvelle bataille initialisée avec {characters.Count} personnages");
        }
    }

    /// <summary>
    /// Affichage d'un personnage avec HP bar et statut
    /// </summary>
    public partial class CharacterDisplay : PanelContainer
    {
        private Label _nameLabel;
        private ProgressBar _hpBar;
        private Label _hpLabel;
        private Label _statusLabel;

        public CharacterDisplay(CharacterState character)
        {
            CustomMinimumSize = new Vector2(0, 80);
            
            var vbox = new VBoxContainer();
            AddChild(vbox);

            // Nom du personnage
            _nameLabel = new Label();
            _nameLabel.Text = $"{character.Name} ({character.Type})";
            _nameLabel.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(_nameLabel);

            // HP Bar
            _hpBar = new ProgressBar();
            _hpBar.MinValue = 0;
            _hpBar.MaxValue = 100;
            _hpBar.Value = character.CurrentHP;
            _hpBar.ShowPercentage = false;
            _hpBar.CustomMinimumSize = new Vector2(0, 30);
            vbox.AddChild(_hpBar);

            // HP Label
            _hpLabel = new Label();
            _hpLabel.Text = $"HP: {character.CurrentHP}/{character.MaxHP}";
            _hpLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(_hpLabel);

            // Statut
            _statusLabel = new Label();
            _statusLabel.Text = "✅ Vivant";
            _statusLabel.AddThemeFontSizeOverride("font_size", 14);
            vbox.AddChild(_statusLabel);

            UpdateDisplay(character);
        }

        public void UpdateDisplay(CharacterState character)
        {
            _hpBar.Value = character.CurrentHP;
            _hpLabel.Text = $"HP: {character.CurrentHP}/{character.MaxHP}";

            // Couleur de la HP bar selon le pourcentage
            var hpPercent = character.HPPercentage;
            var color = hpPercent > 0.5f ? Colors.Green :
                       hpPercent > 0.25f ? Colors.Orange : Colors.Red;
            
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = color;
            _hpBar.AddThemeStyleboxOverride("fill", styleBox);

            // Statut
            if (character.IsDead)
            {
                _statusLabel.Text = "💀 Mort";
                _nameLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                _statusLabel.Text = "✅ Vivant";
                _nameLabel.Modulate = Colors.White;
            }

            // Effets de statut
            if (character.StatusEffects.Count > 0)
            {
                _statusLabel.Text += " | " + string.Join(", ", character.StatusEffects);
            }
        }
    }
}
