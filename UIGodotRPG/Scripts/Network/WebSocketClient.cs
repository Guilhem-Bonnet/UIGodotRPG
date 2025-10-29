using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace FrontBRRPG.Network
{
	/// <summary>
	/// Client WebSocket singleton pour la connexion au serveur RPG-Arena
	/// AutoLoad: /root/WebSocketClient
	/// </summary>
	public partial class WebSocketClient : Node
	{
		private WebSocketPeer _wsPeer;
		private string _serverUrl = "ws://localhost:5000/ws";
		private bool _isConnected = false;
		private bool _hasLoggedConnection = false;
		private double _reconnectTimer = 0;
		private const double RECONNECT_DELAY = 3.0; // secondes
		private bool _shouldReconnect = false;

		// État public
		public new bool IsConnected => _isConnected && _wsPeer?.GetReadyState() == WebSocketPeer.State.Open;
		public string ServerUrl 
		{ 
			get => _serverUrl;
			set => _serverUrl = value;
		}

		// Signaux Godot
		[Signal]
		public delegate void MessageReceivedEventHandler(string message);

		[Signal]
		public delegate void ConnectionEstablishedEventHandler();

		[Signal]
		public delegate void ConnectionClosedEventHandler(string reason);

		[Signal]
		public delegate void ConnectionErrorEventHandler(string error);

		public override void _Ready()
		{
			GD.Print("🌐 WebSocketClient initialisé (AutoLoad)");
			_wsPeer = new WebSocketPeer();
			SetProcess(true);
		}

		/// <summary>
		/// Démarre la connexion au serveur
		/// </summary>
		public void ConnectToServer()
		{
			if (IsConnected)
			{
				GD.Print("⚠️  Déjà connecté au serveur");
				return;
			}

			GD.Print($"🔄 Connexion à {_serverUrl}...");
			_wsPeer = new WebSocketPeer();
			_hasLoggedConnection = false;
			
			var err = _wsPeer.ConnectToUrl(_serverUrl);
			if (err != Error.Ok)
			{
				GD.PrintErr($"❌ Échec de connexion WebSocket : {err}");
				EmitSignal(SignalName.ConnectionError, $"Connection failed: {err}");
				_shouldReconnect = true;
			}
		}

		/// <summary>
		/// Envoie une configuration de bataille au serveur
		/// Protocole: envoie une liste JSON de configurations de personnages
		/// Format attendu par le serveur: array of {type: string, name: string}
		/// </summary>
		public void StartBattle(List<CharacterConfig> characters)
		{
			if (!IsConnected)
			{
				GD.PrintErr("❌ Impossible de démarrer la bataille : non connecté");
				EmitSignal(SignalName.ConnectionError, "Not connected to server");
				return;
			}

			if (characters == null || characters.Count < 2)
			{
				GD.PrintErr("❌ Il faut au moins 2 personnages pour démarrer une bataille");
				return;
			}

			try
			{
				// Créer les objets anonymes pour correspondre au format du serveur
				var characterData = new List<object>();
				foreach (var character in characters)
				{
					characterData.Add(new 
					{ 
						type = character.Type.ToLower(), 
						name = character.Name 
					});
				}

				var json = JsonSerializer.Serialize(characterData);
				var error = _wsPeer.SendText(json);
				
				if (error != Error.Ok)
				{
					GD.PrintErr($"❌ Erreur lors de l'envoi : {error}");
					return;
				}

				GD.Print($"✅ Configuration de bataille envoyée : {json}");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"❌ Erreur sérialisation : {ex.Message}");
			}
		}

		/// <summary>
		/// Ferme proprement la connexion
		/// </summary>
		public void Disconnect()
		{
			_shouldReconnect = false;
			if (_wsPeer != null && IsConnected)
			{
				GD.Print("🔌 Fermeture de la connexion WebSocket");
				_wsPeer.Close(1000, "Client disconnect");
			}
			_isConnected = false;
		}

		public override void _Process(double delta)
		{
			if (_wsPeer == null) return;

			// Gestion de la reconnexion
			if (_shouldReconnect)
			{
				_reconnectTimer += delta;
				if (_reconnectTimer >= RECONNECT_DELAY)
				{
					_reconnectTimer = 0;
					_shouldReconnect = false;
					ConnectToServer();
				}
				return;
			}

			_wsPeer.Poll();

			var state = _wsPeer.GetReadyState();

			// Gestion de l'état de connexion
			switch (state)
			{
				case WebSocketPeer.State.Open:
					if (!_isConnected)
					{
						_isConnected = true;
						GD.Print("✅ Connexion WebSocket établie !");
						EmitSignal(SignalName.ConnectionEstablished);
					}
					
					// Lecture des messages
					while (_wsPeer.GetAvailablePacketCount() > 0)
					{
						var packet = _wsPeer.GetPacket();
						var text = packet.GetStringFromUtf8();
						EmitSignal(SignalName.MessageReceived, text);
					}
					break;

				case WebSocketPeer.State.Closed:
					if (_isConnected || !_hasLoggedConnection)
					{
						var closeCode = _wsPeer.GetCloseCode();
						var closeReason = _wsPeer.GetCloseReason();
						GD.Print($"🚫 Connexion fermée (code: {closeCode}, raison: {closeReason})");
						EmitSignal(SignalName.ConnectionClosed, closeReason);
						_isConnected = false;
						_hasLoggedConnection = true;
						_shouldReconnect = true;
						_reconnectTimer = 0;
					}
					break;

				case WebSocketPeer.State.Connecting:
					if (!_hasLoggedConnection)
					{
						_hasLoggedConnection = true;
					}
					break;

				case WebSocketPeer.State.Closing:
					// Attente de fermeture
					break;
			}
		}

		public override void _ExitTree()
		{
			Disconnect();
			base._ExitTree();
		}
		
		/// <summary>
		/// Récupère la liste des classes de personnages disponibles depuis le backend
		/// </summary>
		public async System.Threading.Tasks.Task<List<string>> GetAvailableClassesAsync()
		{
			var httpClient = new System.Net.Http.HttpClient();
			try
			{
				var baseUrl = _serverUrl.Replace("ws://", "http://").Replace("/ws", "");
				var response = await httpClient.GetStringAsync($"{baseUrl}/api/personnages/classes");
				var classes = JsonSerializer.Deserialize<List<string>>(response);
				GD.Print($"[WebSocket] Classes disponibles récupérées: {classes?.Count ?? 0}");
				return classes ?? new List<string>();
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[WebSocket] Erreur lors de la récupération des classes: {ex.Message}");
				// Retourner les classes par défaut si le backend n'est pas disponible
				return new List<string>
				{
					"Guerrier", "Berserker", "Magicien", "Assassin",
					"Pretre", "Paladin", "Necromancien", "Alchimiste",
					"Illusioniste", "Vampire", "Zombie", "Robot"
				};
			}
			finally
			{
				httpClient.Dispose();
			}
		}
	}
}
