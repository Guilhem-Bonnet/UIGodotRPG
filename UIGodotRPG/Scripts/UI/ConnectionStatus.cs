using Godot;
using System;
using FrontBRRPG.Network;

namespace FrontBRRPG.UI
{
	/// <summary>
	/// Indicateur visuel simple du statut de connexion WebSocket
	/// </summary>
	public partial class ConnectionStatus : PanelContainer
	{
		private WebSocketClient _wsClient;
		private Label _statusLabel;
		private ColorRect _indicator;
		private Godot.Timer _updateTimer;
		
		private Color _connectedColor = new Color(0, 0.8f, 0.2f); // Vert
		private Color _disconnectedColor = new Color(0.8f, 0.2f, 0); // Rouge
		private Color _connectingColor = new Color(0.8f, 0.6f, 0); // Orange
		
		public override void _Ready()
		{
			// Créer l'indicateur visuel
			_indicator = new ColorRect();
			_indicator.CustomMinimumSize = new Vector2(12, 12);
			_indicator.Color = _disconnectedColor;
			
			// Créer le label
			_statusLabel = new Label();
			_statusLabel.Text = "⚠️ Déconnecté";
			_statusLabel.AddThemeFontSizeOverride("font_size", 14);
			
			// Container horizontal
			var hbox = new HBoxContainer();
			hbox.AddThemeConstantOverride("separation", 8);
			hbox.AddChild(_indicator);
			hbox.AddChild(_statusLabel);
			AddChild(hbox);
			
			// Style du panel
			var stylebox = new StyleBoxFlat();
			stylebox.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
			stylebox.SetCornerRadiusAll(4);
			stylebox.ContentMarginLeft = 10;
			stylebox.ContentMarginRight = 10;
			stylebox.ContentMarginTop = 5;
			stylebox.ContentMarginBottom = 5;
			AddThemeStyleboxOverride("panel", stylebox);
			
			// Récupérer le WebSocketClient
			_wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
			
			// Connecter aux événements
			_wsClient.ConnectionEstablished += OnConnectionEstablished;
			_wsClient.ConnectionClosed += OnConnectionClosed;
			_wsClient.ConnectionError += OnConnectionError;
			
			// Timer pour mise à jour régulière
			_updateTimer = new Timer();
			_updateTimer.WaitTime = 0.5; // 2 fois par seconde
			_updateTimer.Autostart = true;
			_updateTimer.Timeout += UpdateStatus;
			AddChild(_updateTimer);
			
			UpdateStatus();
		}
		
		private void OnConnectionEstablished()
		{
			UpdateStatus();
		}
		
		private void OnConnectionClosed(string reason)
		{
			UpdateStatus();
		}
		
		private void OnConnectionError(string error)
		{
			_statusLabel.Text = $"❌ Erreur: {error}";
			_indicator.Color = _disconnectedColor;
		}
		
		private void UpdateStatus()
		{
			if (_wsClient == null) return;
			
			if (_wsClient.IsConnected)
			{
				_statusLabel.Text = $"✅ Connecté ({_wsClient.ServerUrl})";
				_indicator.Color = _connectedColor;
			}
			else
			{
				_statusLabel.Text = "⚠️ Déconnecté";
				_indicator.Color = _disconnectedColor;
			}
		}
		
		public override void _ExitTree()
		{
			if (_wsClient != null)
			{
				_wsClient.ConnectionEstablished -= OnConnectionEstablished;
				_wsClient.ConnectionClosed -= OnConnectionClosed;
				_wsClient.ConnectionError -= OnConnectionError;
			}
		}
	}
}
