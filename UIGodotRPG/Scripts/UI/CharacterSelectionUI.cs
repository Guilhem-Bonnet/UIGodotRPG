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
        private const int MAX_CHARACTERS = 10;

        public override void _Ready()
        {
            _characterGrid = GetNode<GridContainer>("MarginContainer/VBoxContainer/ScrollContainer/CharacterGrid");
            _startBattleButton = GetNode<Button>("MarginContainer/VBoxContainer/ButtonsContainer/StartBattleButton");
            _selectionCountLabel = GetNode<Label>("MarginContainer/VBoxContainer/HeaderContainer/SelectionCountLabel");
            _selectionInfoLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/SelectionInfoLabel");

            _startBattleButton.Pressed += OnStartBattlePressed;

            CreateCharacterButtons();
            UpdateSelectionUI();
        }

        private void CreateCharacterButtons()
        {
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
                if (_selectedCharacters.Count >= MAX_CHARACTERS)
                {
                    button.SetPressedNoSignal(false);
                    GD.Print($"[Selection] Maximum de {MAX_CHARACTERS} personnages atteint");
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
            _selectionCountLabel.Text = $"Personnages sélectionnés: {count}/{MAX_CHARACTERS}";

            var canStart = count >= MIN_CHARACTERS;
            _startBattleButton.Disabled = !canStart;

            // Afficher la liste des personnages sélectionnés
            var info = "[color=#FFFF00]Personnages sélectionnés:[/color]\n";
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
