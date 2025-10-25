using Godot;
using System.Collections.Generic;
using FrontBRRPG.Network;

namespace FrontBRRPG.UI
{
    /// <summary>
    /// Interface principale de l'application
    /// </summary>
    public partial class MainUI : Control
    {
        private WebSocketClient _wsClient;
        private RichTextLabel _logLabel;
        private Button _startButton;
        private Button _connectButton;

        public override void _Ready()
        {
            // Récupération du singleton AutoLoad
            _wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
            GD.Print("✅ WebSocketClient récupéré : " + (_wsClient != null));

            // Récupération des éléments UI (à adapter selon votre scène)
            _logLabel = GetNodeOrNull<RichTextLabel>("VBoxContainer/LogLabel");
            _startButton = GetNodeOrNull<Button>("VBoxContainer/StartButton");
            _connectButton = GetNodeOrNull<Button>("VBoxContainer/ConnectButton");

            // Connexion aux signaux WebSocket
            _wsClient.MessageReceived += OnMessageReceived;
            _wsClient.ConnectionEstablished += OnConnectionEstablished;
            _wsClient.ConnectionClosed += OnConnectionClosed;
            _wsClient.ConnectionError += OnConnectionError;

            // Connexion aux boutons si ils existent
            if (_startButton != null)
                _startButton.Pressed += OnStartButtonPressed;
                
            if (_connectButton != null)
                _connectButton.Pressed += OnConnectButtonPressed;

            // Afficher l'état initial
            UpdateConnectionStatus();
        }

        private void OnConnectionEstablished()
        {
            GD.Print("🎉 Connexion établie !");
            AddLog("[color=green]✅ Connecté au serveur RPG-Arena[/color]");
            UpdateConnectionStatus();
        }

        private void OnMessageReceived(string msg)
        {
            // Les messages du serveur sont des logs de combat
            AddLog(msg);
        }

        private void OnConnectionClosed(string reason)
        {
            AddLog($"[color=orange]⚠️  Connexion fermée : {reason}[/color]");
            UpdateConnectionStatus();
        }

        private void OnConnectionError(string error)
        {
            AddLog($"[color=red]❌ Erreur de connexion : {error}[/color]");
            UpdateConnectionStatus();
        }

        private void OnConnectButtonPressed()
        {
            if (_wsClient.IsConnected)
            {
                AddLog("[color=yellow]Déconnexion...[/color]");
                _wsClient.Disconnect();
            }
            else
            {
                AddLog("[color=cyan]Tentative de connexion...[/color]");
                _wsClient.ConnectToServer();
            }
        }

        private void OnStartButtonPressed()
        {
            if (!_wsClient.IsConnected)
            {
                AddLog("[color=red]❌ Connectez-vous d'abord au serveur[/color]");
                return;
            }

            // Configuration de test avec 2 personnages
            var characters = new List<CharacterConfig>
            {
                new CharacterConfig { Type = CharacterTypes.Guerrier, Name = "Conan" },
                new CharacterConfig { Type = CharacterTypes.Berserker, Name = "Ragnar" }
            };

            var names = string.Join(" vs ", characters.ConvertAll(c => $"{c.Name} ({c.Type})"));
            AddLog($"[color=cyan]🎮 Démarrage du combat : {names}[/color]");
            _wsClient.StartBattle(characters);
        }

        private void AddLog(string message)
        {
            if (_logLabel != null)
            {
                _logLabel.AppendText(message + "\n");
                // Auto-scroll vers le bas
                CallDeferred(MethodName.ScrollToBottom);
            }
            else
            {
                GD.Print(message);
            }
        }

        private void ScrollToBottom()
        {
            if (_logLabel != null)
            {
                var scrollContainer = _logLabel.GetParentOrNull<ScrollContainer>();
                if (scrollContainer != null)
                {
                    scrollContainer.ScrollVertical = (int)scrollContainer.GetVScrollBar().MaxValue;
                }
            }
        }

        private void UpdateConnectionStatus()
        {
            if (_connectButton != null)
            {
                _connectButton.Text = _wsClient.IsConnected ? "Déconnecter" : "Se connecter";
            }
            
            if (_startButton != null)
            {
                _startButton.Disabled = !_wsClient.IsConnected;
            }
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
