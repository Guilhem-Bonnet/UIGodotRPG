using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FrontBRRPG.Network;

namespace FrontBRRPG.UI
{
    public partial class CharacterSelectionUI : Control
    {
        // Utiliser un event C# au lieu d'un signal Godot
        public event Action<List<CharacterConfig>> CharactersSelected;

        private GridContainer _characterGrid;
        private Button _startBattleButton;
        private Label _selectionCountLabel;
        private RichTextLabel _selectionInfoLabel;

        private List<CharacterButton> _characterButtons = new List<CharacterButton>();
        private List<CharacterConfig> _selectedCharacters = new List<CharacterConfig>();

    private const int MIN_CHARACTERS = 2;
    private int _maxCharacters = 4;

        public override void _Ready()
        {
            _characterGrid = GetNode<GridContainer>("MarginContainer/VBoxContainer/ScrollContainer/CharacterGrid");
            _startBattleButton = GetNode<Button>("MarginContainer/VBoxContainer/ButtonsContainer/StartBattleButton");
            _selectionCountLabel = GetNode<Label>("MarginContainer/VBoxContainer/HeaderContainer/SelectionCountLabel");
            _selectionInfoLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/SelectionInfoLabel");

            var maxCharactersSpinBox = GetNode<SpinBox>("MarginContainer/VBoxContainer/HeaderContainer/MaxCharactersSpinBox");
            maxCharactersSpinBox.Value = _maxCharacters;
            maxCharactersSpinBox.ValueChanged += OnMaxCharactersChanged;

            _startBattleButton.Pressed += OnStartBattlePressed;

            LoadAvailableClassesAsync();
            UpdateSelectionUI();
        }
        
        private async void LoadAvailableClassesAsync()
        {
            var wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
            var availableClasses = await wsClient.GetAvailableClassesAsync();
            
            GD.Print($"[CharacterSelection] Chargement de {availableClasses.Count} classes disponibles");
            
            foreach (var className in availableClasses)
            {
                var button = new CharacterButton(className);
                button.Toggled += (bool pressed) => OnCharacterToggled(button, pressed);
                _characterGrid.AddChild(button);
                _characterButtons.Add(button);
            }
            
            UpdateSelectionUI();
        }

        private void CreateCharacterButtons()
        {
            // Méthode dépréciée - utiliser LoadAvailableClassesAsync() à la place
            var characterTypes = CharacterTypes.All;
            
            foreach (var type in characterTypes)
            {
                var button = new CharacterButton(type);
                button.Toggled += (bool pressed) => OnCharacterToggled(button, pressed);
                _characterGrid.AddChild(button);
                _characterButtons.Add(button);
            }
        }

        private void OnCharacterToggled(CharacterButton button, bool selected)
        {
            if (selected)
            {
                if (_selectedCharacters.Count >= _maxCharacters)
                {
                    button.SetPressedNoSignal(false);
                    GD.Print($"[Selection] Maximum de {_maxCharacters} personnages atteint");
                    return;
                }

                var config = new CharacterConfig
                {
                    Type = button.CharacterType,
                    Name = GetDefaultName(button.CharacterType)
                };
                _selectedCharacters.Add(config);
                GD.Print($"[Selection] Ajouté: {config.Name} ({config.Type})");
            }
            else
            {
                var toRemove = _selectedCharacters.FirstOrDefault(c => c.Type == button.CharacterType);
                if (toRemove != null)
                {
                    _selectedCharacters.Remove(toRemove);
                    GD.Print($"[Selection] Retiré: {toRemove.Name}");
                }
            }

            UpdateSelectionUI();
        }

        private void UpdateSelectionUI()
        {
            var count = _selectedCharacters.Count;
            _selectionCountLabel.Text = $"Personnages sélectionnés: {count}/{_maxCharacters}";

            var canStart = count >= MIN_CHARACTERS;
            _startBattleButton.Disabled = !canStart;

            // Afficher la liste des personnages sélectionnés
            var info = $"[color=#FFFF00]Personnages sélectionnés (max: {_maxCharacters}):[/color]\n";
            if (count == 0)
            {
                info += "[color=#808080]Aucun personnage sélectionné[/color]";
            }
            else
            {
                foreach (var character in _selectedCharacters)
                {
                    info += $"[color=#00FF00]• {character.Name}[/color] ({character.Type})\n";
                }
            }

            if (count < MIN_CHARACTERS)
            {
                info += $"\n[color=#FF0000]Minimum {MIN_CHARACTERS} personnages requis pour démarrer[/color]";
            }

            _selectionInfoLabel.Text = info;

            // Désactiver les boutons si le max est atteint
            foreach (var button in _characterButtons)
            {
                button.Disabled = (_selectedCharacters.Count >= _maxCharacters && !button.ButtonPressed);
            }
        }
        private void OnMaxCharactersChanged(double value)
        {
            _maxCharacters = (int)value;
            // Désélectionner les personnages en trop
            while (_selectedCharacters.Count > _maxCharacters)
            {
                var last = _selectedCharacters.Last();
                var btn = _characterButtons.FirstOrDefault(b => b.CharacterType == last.Type);
                if (btn != null)
                    btn.SetPressedNoSignal(false);
                _selectedCharacters.Remove(last);
            }
            UpdateSelectionUI();
        }

        private void OnStartBattlePressed()
        {
            if (_selectedCharacters.Count < MIN_CHARACTERS)
            {
                GD.Print($"[Selection] Impossible de démarrer: minimum {MIN_CHARACTERS} personnages requis");
                return;
            }

            GD.Print($"[Selection] Démarrage du combat avec {_selectedCharacters.Count} personnages");
            CharactersSelected?.Invoke(_selectedCharacters);
        }

        private string GetDefaultName(string type)
        {
            var names = new Dictionary<string, string[]>
            {
                { CharacterTypes.Alchimiste, new[] { "Paracelse", "Flamel", "Trismegiste" } },
                { CharacterTypes.Assassin, new[] { "Shadow", "Viper", "Phantom" } },
                { CharacterTypes.Berserker, new[] { "Ragnar", "Bjorn", "Ulfric" } },
                { CharacterTypes.Guerrier, new[] { "Conan", "Achille", "Leonidas" } },
                { CharacterTypes.Illusioniste, new[] { "Mystique", "Illusion", "Mirage" } },
                { CharacterTypes.Magicien, new[] { "Merlin", "Gandalf", "Morgana" } },
                { CharacterTypes.Necromancien, new[] { "Thanatos", "Mortis", "Nécros" } },
                { CharacterTypes.Paladin, new[] { "Arthas", "Uther", "Tirion" } },
                { CharacterTypes.Pretre, new[] { "Benedictus", "Velen", "Anduin" } },
                { CharacterTypes.Robot, new[] { "Alpha", "Beta", "Gamma" } },
                { CharacterTypes.Vampire, new[] { "Dracula", "Lestat", "Nosferatu" } },
                { CharacterTypes.Zombie, new[] { "Walker", "Shambler", "Undead" } }
            };

            if (names.ContainsKey(type))
            {
                var random = new Random();
                return names[type][random.Next(names[type].Length)];
            }

            return type;
        }

        public void ResetSelection()
        {
            _selectedCharacters.Clear();
            foreach (var button in _characterButtons)
            {
                button.SetPressedNoSignal(false);
            }
            UpdateSelectionUI();
        }
    }

    /// <summary>
    /// Bouton personnalisé pour un type de personnage
    /// </summary>
    public partial class CharacterButton : Button
    {
        public string CharacterType { get; private set; }

        public CharacterButton(string type)
        {
            CharacterType = type;
            ToggleMode = true;
            CustomMinimumSize = new Vector2(150, 150);
            
            // Créer le layout du bouton
            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            vbox.SizeFlagsVertical = SizeFlags.ExpandFill;
            AddChild(vbox);

            // Icône (placeholder pour l'instant)
            var iconLabel = new Label();
            iconLabel.Text = GetEmoji(type);
            iconLabel.HorizontalAlignment = HorizontalAlignment.Center;
            iconLabel.CustomMinimumSize = new Vector2(0, 80);
            iconLabel.AddThemeFontSizeOverride("font_size", 48);
            vbox.AddChild(iconLabel);

            // Nom du type
            var nameLabel = new Label();
            nameLabel.Text = type.Capitalize();
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            vbox.AddChild(nameLabel);
        }

        private string GetEmoji(string type)
        {
            var emojis = new Dictionary<string, string>
            {
                { CharacterTypes.Alchimiste, "🧪" },
                { CharacterTypes.Assassin, "🗡️" },
                { CharacterTypes.Berserker, "🪓" },
                { CharacterTypes.Guerrier, "🛡️" },
                { CharacterTypes.Illusioniste, "✨" },
                { CharacterTypes.Magicien, "🔮" },
                { CharacterTypes.Necromancien, "💀" },
                { CharacterTypes.Paladin, "⚔️" },
                { CharacterTypes.Pretre, "📿" },
                { CharacterTypes.Robot, "🤖" },
                { CharacterTypes.Vampire, "🧛" },
                { CharacterTypes.Zombie, "🧟" }
            };

            return emojis.ContainsKey(type) ? emojis[type] : "❓";
        }
    }
}
