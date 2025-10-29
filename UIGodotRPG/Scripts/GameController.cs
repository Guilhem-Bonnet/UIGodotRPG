using Godot;
using System;
using System.Collections.Generic;
using FrontBRRPG.Network;
using FrontBRRPG.UI;

namespace FrontBRRPG
{
    /// <summary>
    /// Contrôleur principal qui gère la navigation entre les écrans
    /// </summary>
    public partial class GameController : Node
    {
        private Control _currentScreen;
        private WebSocketClient _wsClient;

        // Références aux scènes
        private PackedScene _menuScene;
        private PackedScene _characterSelectionScene;
        private PackedScene _areneScene;
        private PackedScene _testEnvironmentScene;

        public override void _Ready()
        {
            GD.Print("🎮 [GameController] Initialisation...");
            
            _wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
            GD.Print("✅ [GameController] WebSocketClient récupéré");

            // Charger les scènes
            _menuScene = GD.Load<PackedScene>("res://Scenes/Menu.tscn");
            GD.Print($"✅ [GameController] Menu chargé: {_menuScene != null}");
            
            _characterSelectionScene = GD.Load<PackedScene>("res://Scenes/CharacterSelectionScreen.tscn");
            GD.Print($"✅ [GameController] CharacterSelection chargé: {_characterSelectionScene != null}");
            
            _areneScene = GD.Load<PackedScene>("res://Arene/Arene.tscn");
            GD.Print($"✅ [GameController] Arene chargé: {_areneScene != null}");
            
            _testEnvironmentScene = GD.Load<PackedScene>("res://Scenes/TestEnvironment.tscn");
            GD.Print($"✅ [GameController] TestEnvironment chargé: {_testEnvironmentScene != null}");

            // Connexion WebSocket events
            _wsClient.ConnectionEstablished += OnWebSocketConnected;
            _wsClient.ConnectionClosed += OnWebSocketDisconnected;

            // Démarrer sur le menu
            GD.Print("🚀 [GameController] Lancement du menu...");
            ShowMenu();
        }
        
        public override void _Input(InputEvent @event)
        {
            // Raccourci F5 pour lancer l'environnement de test
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F5)
            {
                ShowTestEnvironment();
            }
        }

        private void OnWebSocketConnected()
        {
            GD.Print("[GameController] WebSocket connecté");
        }

        private void OnWebSocketDisconnected(string reason)
        {
            GD.Print($"[GameController] WebSocket déconnecté: {reason}");
        }

        public void ShowMenu()
        {
            GD.Print("📋 [GameController] ShowMenu() appelé");
            var screen = ChangeScreen(_menuScene);
            GD.Print($"✅ [GameController] Écran menu instancié: {screen != null}");
            
            // Connecter les boutons du menu
            var playButton = screen.GetNode<Button>("MarginContainer/VBoxContainer/PlayButton");
            var testButton = screen.GetNode<Button>("MarginContainer/VBoxContainer/TestButton");
            var quitButton = screen.GetNode<Button>("MarginContainer/VBoxContainer/QuitButton");
            
            GD.Print($"✅ [GameController] Boutons trouvés - Play: {playButton != null}, Test: {testButton != null}, Quit: {quitButton != null}");
            
            playButton.Pressed += ShowCharacterSelection;
            testButton.Pressed += ShowTestEnvironment;
            quitButton.Pressed += () => GetTree().Quit();
            
            GD.Print("✅ [GameController] Menu affiché avec succès !");
        }

        public void ShowCharacterSelection()
        {
            var screen = ChangeScreen(_characterSelectionScene);
            if (screen is CharacterSelectionUI selectionUI)
            {
                selectionUI.CharactersSelected += OnCharactersSelected;
                
                // Connecter les boutons
                var backButton = screen.GetNode<Button>("MarginContainer/VBoxContainer/ButtonsContainer/BackButton");
                var resetButton = screen.GetNode<Button>("MarginContainer/VBoxContainer/ButtonsContainer/ResetButton");
                
                backButton.Pressed += ShowMenu;
                resetButton.Pressed += () => selectionUI.ResetSelection();
            }
        }

        public void ShowArene(List<CharacterConfig> characters)
        {
            var screen = ChangeScreen(_areneScene);
            
            // L'AreneController gère maintenant la configuration des personnages
            if (screen is AreneController areneController)
            {
                if (characters != null && characters.Count > 0)
                {
                    areneController.SetSelectedCharacters(characters);
                    GD.Print($"[GameController] Arène chargée avec {characters.Count} personnages");
                }
            }
        }

        private void OnCharactersSelected(List<CharacterConfig> characters)
        {
            GD.Print($"[GameController] {characters.Count} personnages sélectionnés, démarrage du combat");

            // Passer à l'arène
            ShowArene(characters);

            // Connecter au WebSocket si pas déjà connecté
            if (!_wsClient.IsConnected)
            {
                _wsClient.ConnectToServer();
                
                // Attendre la connexion puis lancer le combat
                _wsClient.ConnectionEstablished += () => 
                {
                    GD.Print("[GameController] Envoi de la configuration de combat au serveur");
                    _wsClient.StartBattle(characters);
                };
            }
            else
            {
                // Déjà connecté, lancer directement
                _wsClient.StartBattle(characters);
            }
        }

        public void ShowTestEnvironment()
        {
            GD.Print("🧪 [GameController] Lancement de l'environnement de test...");
            ChangeScreen(_testEnvironmentScene);
        }

        private Control ChangeScreen(PackedScene scene)
        {
            // Supprimer l'écran actuel
            if (_currentScreen != null)
            {
                _currentScreen.QueueFree();
                _currentScreen = null;
            }

            // Instancier le nouvel écran
            _currentScreen = scene.Instantiate<Control>();
            AddChild(_currentScreen);

            GD.Print($"[GameController] Écran changé: {scene.ResourcePath}");
            return _currentScreen;
        }

        public override void _Notification(int what)
        {
            if (what == NotificationWMCloseRequest)
            {
                // Déconnecter proprement
                _wsClient.Disconnect();
                GetTree().Quit();
            }
        }
    }
}
