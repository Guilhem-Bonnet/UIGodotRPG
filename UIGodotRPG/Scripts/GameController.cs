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
        private PackedScene _battleViewerScene;

        public override void _Ready()
        {
            _wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");

            // Charger les scènes
            _menuScene = GD.Load<PackedScene>("res://Scenes/Menu.tscn");
            _characterSelectionScene = GD.Load<PackedScene>("res://Scenes/CharacterSelectionScreen.tscn");
            _battleViewerScene = GD.Load<PackedScene>("res://Scenes/BattleViewer.tscn");

            // Connexion WebSocket events
            _wsClient.ConnectionEstablished += OnWebSocketConnected;
            _wsClient.ConnectionClosed += OnWebSocketDisconnected;

            // Démarrer sur le menu
            ShowMenu();
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
            var screen = ChangeScreen(_menuScene);
            
            // Connecter les boutons du menu
            var playButton = screen.GetNode<Button>("MarginContainer/VBoxContainer/PlayButton");
            var quitButton = screen.GetNode<Button>("MarginContainer/VBoxContainer/QuitButton");
            
            playButton.Pressed += ShowCharacterSelection;
            quitButton.Pressed += () => GetTree().Quit();
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

        public void ShowBattleViewer(List<CharacterConfig> characters)
        {
            var screen = ChangeScreen(_battleViewerScene);
            if (screen is BattleViewer battleViewer)
            {
                battleViewer.StartNewBattle(characters);
            }
        }

        private void OnCharactersSelected(List<CharacterConfig> characters)
        {
            GD.Print($"[GameController] {characters.Count} personnages sélectionnés, démarrage du combat");

            // Passer à l'écran de combat
            ShowBattleViewer(characters);

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
