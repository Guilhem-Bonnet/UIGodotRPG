using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire d'interface pour un personnage dans l'arène
/// Synchronise l'affichage avec les événements de combat du serveur
/// </summary>
public partial class PersonnageUIManager : Control
{
	// Propriétés du personnage
	public string CharacterName { get; set; } = "";
	public string CharacterType { get; set; } = "";
	public int CurrentHP { get; set; } = 100;
	public int MaxHP { get; set; } = 100;
	public bool IsDead { get; set; } = false;
	public string FocusTarget { get; set; } = "";
	public string Attacker { get; set; } = "";
	
	// Références UI
	private TextEdit _nameEdit;
	private Life _healthBar;
	private Label _nameFocusLabel;
	private Label _nameAttaquantLabel;
	private Sprite2D _classeSprite;
	private Sprite2D _deathStateSprite;
	private ItemList _bonusList;
	private ItemList _malusList;
	private LogContainer _logContainer;
	private ColorRect _overlayDeath;
	
	public override void _Ready()
	{
		// Récupérer les références UI
		_nameEdit = GetNode<TextEdit>("TextEdit");
		_healthBar = GetNode<Life>("HealthBar");
		_nameFocusLabel = GetNode<Label>("Label_NameFocus");
		_nameAttaquantLabel = GetNode<Label>("Label_NameAttaquant");
		_classeSprite = GetNode<Sprite2D>("Sprite2DClasse");
		_deathStateSprite = GetNode<Sprite2D>("Sprite2D_DeathState");
		_bonusList = GetNode<ItemList>("Bonus");
		_malusList = GetNode<ItemList>("Malus");
		_logContainer = GetNode<LogContainer>("CollapsibleContainer/ScrollContainer/VBoxContainer");
		_overlayDeath = GetNode<ColorRect>("ColorRect");
		
		// Initialiser l'affichage
		_deathStateSprite.Visible = false;
		_overlayDeath.Visible = false;
		_nameFocusLabel.Text = "";
		_nameAttaquantLabel.Text = "";
	}
	
	/// <summary>
	/// Initialise le personnage avec ses données
	/// </summary>
	public void InitializeCharacter(string name, string type, int maxHp = 100)
	{
		CharacterName = name;
		CharacterType = type;
		MaxHP = maxHp;
		CurrentHP = maxHp;
		
		_nameEdit.Text = name;
		_healthBar.MaxValue = maxHp;
		_healthBar.Value = maxHp;
		
		// Charger l'icône appropriée selon le type
		LoadClassIcon(type);
		
		GD.Print($"[PersonnageUI] {name} ({type}) initialisé avec {maxHp} HP");
	}
	
	/// <summary>
	/// Met à jour les HP du personnage
	/// </summary>
	public void UpdateHP(int newHP)
	{
		CurrentHP = Math.Max(0, Math.Min(newHP, MaxHP));
		_healthBar.Value = CurrentHP;
		
		// Si mort, afficher l'overlay
		if (CurrentHP <= 0 && !IsDead)
		{
			SetDead(true);
		}
		
		GD.Print($"[PersonnageUI] {CharacterName} HP: {CurrentHP}/{MaxHP}");
	}
	
	/// <summary>
	/// Inflige des dégâts au personnage
	/// </summary>
	public void TakeDamage(int damage)
	{
		UpdateHP(CurrentHP - damage);
		AddLog($"💥 Subit {damage} dégâts ! HP: {CurrentHP}/{MaxHP}");
	}
	
	/// <summary>
	/// Soigne le personnage
	/// </summary>
	public void Heal(int amount)
	{
		UpdateHP(CurrentHP + amount);
		AddLog($"❤️ Soigné de {amount} HP ! HP: {CurrentHP}/{MaxHP}");
	}
	
	/// <summary>
	/// Définit l'état de mort du personnage
	/// </summary>
	public void SetDead(bool dead)
	{
		IsDead = dead;
		_deathStateSprite.Visible = dead;
		_overlayDeath.Visible = dead;
		
		if (dead)
		{
			AddLog("💀 EST MORT !");
			Modulate = new Color(0.5f, 0.5f, 0.5f); // Grisé
		}
		else
		{
			Modulate = Colors.White;
		}
	}
	
	/// <summary>
	/// Met à jour la cible focalisée
	/// </summary>
	public void SetFocus(string targetName)
	{
		FocusTarget = targetName;
		_nameFocusLabel.Text = targetName != "" ? $"🎯 Focus: {targetName}" : "";
	}
	
	/// <summary>
	/// Met à jour l'attaquant actuel
	/// </summary>
	public void SetAttacker(string attackerName)
	{
		Attacker = attackerName;
		_nameAttaquantLabel.Text = attackerName != "" ? $"⚔️ Attaqué par: {attackerName}" : "";
	}
	
	/// <summary>
	/// Ajoute un bonus (buff)
	/// </summary>
	public void AddBonus(string bonusName, Texture2D icon = null)
	{
		_bonusList.AddItem(bonusName, icon);
		AddLog($"✨ Bonus: {bonusName}");
	}
	
	/// <summary>
	/// Ajoute un malus (debuff)
	/// </summary>
	public void AddMalus(string malusName, Texture2D icon = null)
	{
		_malusList.AddItem(malusName, icon);
		AddLog($"🩸 Malus: {malusName}");
	}
	
	/// <summary>
	/// Supprime tous les bonus/malus
	/// </summary>
	public void ClearEffects()
	{
		_bonusList.Clear();
		_malusList.Clear();
	}
	
	/// <summary>
	/// Ajoute une entrée au journal de logs
	/// </summary>
	public void AddLog(string message)
	{
		var timestamp = DateTime.Now.ToString("HH:mm:ss");
		var logMessage = $"[{timestamp}] {message}";
		_logContainer.AddLog(logMessage);
	}
	
	/// <summary>
	/// Charge l'icône de classe appropriée
	/// </summary>
	private void LoadClassIcon(string type)
	{
		var iconPath = type.ToLower() switch
		{
			"guerrier" => "res://icons/rework/64x64/Guerrierx64.png",
			"berserker" => "res://icons/rework/64x64/Berserkx64.png",
			"magicien" => "res://icons/rework/64x64/Magicienx64.png",
			"assassin" => "res://icons/rework/64x64/Assassinx64.png",
			"pretre" => "res://icons/rework/64x64/Pretrex64.png",
			"paladin" => "res://icons/rework/64x64/Paladinx64.png",
			"necromancien" => "res://icons/rework/64x64/Necromancienx64.png",
			"alchimiste" => "res://icons/rework/64x64/Alchimistex64.png",
			"illusioniste" => "res://icons/rework/64x64/Illusionistex64.png",
			"vampire" => "res://icons/rework/64x64/Vampirex64.png",
			"zombie" => "res://icons/rework/64x64/Zombiex64.png",
			"robot" => "res://icons/rework/64x64/Robotx64.png",
			_ => "res://icons/rework/64x64/Berserkx64.png" // Default
		};
		
		if (ResourceLoader.Exists(iconPath))
		{
			_classeSprite.Texture = GD.Load<Texture2D>(iconPath);
		}
	}
}
