using Godot;
using System;
using FrontBRRPG.Network;

/// <summary>
/// Gestionnaire de navigation entre les différentes vues (UI monitoring et Arène)
/// </summary>
public partial class ViewManager : Control
{
	private Control _currentView;
	private WebSocketClient _wsClient;
	
	// Références aux scènes
	private PackedScene _testWebSocketScene;
	private PackedScene _areneScene;
	
	// Boutons de navigation
	private Button _switchToAreneButton;
	private Button _switchToMonitoringButton;
	
	public override void _Ready()
	{
		_wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
		
		// Charger les scènes
		_testWebSocketScene = GD.Load<PackedScene>("res://Scenes/TestWebSocket.tscn");
		_areneScene = GD.Load<PackedScene>("res://Arene/Arene.tscn");
		
		// Créer les boutons de navigation
		CreateNavigationButtons();
		
		// Démarrer avec la vue de monitoring
		ShowMonitoringView();
	}
	
	private void CreateNavigationButtons()
	{
		// Bouton pour aller à l'arène (en haut à droite)
		_switchToAreneButton = new Button();
		_switchToAreneButton.Text = "🏛️ Vue Arène";
		_switchToAreneButton.Position = new Vector2(1650, 10);
		_switchToAreneButton.Size = new Vector2(250, 50);
		_switchToAreneButton.AddThemeFontSizeOverride("font_size", 18);
		_switchToAreneButton.Pressed += ShowAreneView;
		AddChild(_switchToAreneButton);
		
		// Bouton pour retourner au monitoring (en haut à gauche)
		_switchToMonitoringButton = new Button();
		_switchToMonitoringButton.Text = "📊 Vue Monitoring";
		_switchToMonitoringButton.Position = new Vector2(10, 10);
		_switchToMonitoringButton.Size = new Vector2(250, 50);
		_switchToMonitoringButton.AddThemeFontSizeOverride("font_size", 18);
		_switchToMonitoringButton.Pressed += ShowMonitoringView;
		_switchToMonitoringButton.Visible = false;
		AddChild(_switchToMonitoringButton);
	}
	
	public void ShowMonitoringView()
	{
		SwitchView(_testWebSocketScene);
		_switchToAreneButton.Visible = true;
		_switchToMonitoringButton.Visible = false;
		GD.Print("[ViewManager] Vue Monitoring activée");
	}
	
	public void ShowAreneView()
	{
		SwitchView(_areneScene);
		_switchToAreneButton.Visible = false;
		_switchToMonitoringButton.Visible = true;
		GD.Print("[ViewManager] Vue Arène activée");
	}
	
	private void SwitchView(PackedScene scene)
	{
		// Supprimer la vue actuelle
		if (_currentView != null)
		{
			_currentView.QueueFree();
			_currentView = null;
		}
		
		// Instancier la nouvelle vue
		_currentView = scene.Instantiate<Control>();
		
		// L'ajouter en premier enfant (derrière les boutons de navigation)
		MoveChild(_currentView, 0);
		AddChild(_currentView);
		
		// S'assurer que les boutons restent au-dessus
		MoveChild(_switchToAreneButton, GetChildCount() - 1);
		MoveChild(_switchToMonitoringButton, GetChildCount() - 1);
	}
	
	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			_wsClient?.Disconnect();
			GetTree().Quit();
		}
	}
}
