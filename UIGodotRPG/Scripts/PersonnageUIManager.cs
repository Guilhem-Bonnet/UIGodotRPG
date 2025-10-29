using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FrontBRRPG.Models;
using FrontBRRPG.Utils;

namespace FrontBRRPG
{
	public partial class PersonnageUIManager : Control
	{
		public CharacterData CharacterData { get; private set; }
		
		private List<CombatLogEntry> _logHistory = new();
		
		private TextEdit _nameEdit;
		private Life _healthBar;
		private Label _nameFocusLabel;
		private Label _nameAttaquantLabel;
		private Label _statsLabel;
		private Label _classEmojiLabel;
		private Sprite2D _deathStateSprite;
		private ItemList _bonusList;
		private ItemList _malusList;
		private LogContainer _logContainer;
		private ColorRect _overlayDeath;
		private ColorRect _cardBackground;
		
		public override void _Ready()
		{
			_nameEdit = GetNode<TextEdit>("TextEdit");
			_healthBar = GetNode<Life>("HealthBar");
			_nameFocusLabel = GetNode<Label>("Label_NameFocus");
			_nameAttaquantLabel = GetNode<Label>("Label_NameAttaquant");
			_statsLabel = GetNode<Label>("Label_Stats");
			_classEmojiLabel = GetNode<Label>("ClassEmojiLabel");
			_deathStateSprite = GetNode<Sprite2D>("Sprite2D_DeathState");
			_bonusList = GetNode<ItemList>("Bonus");
			_malusList = GetNode<ItemList>("Malus");
			_logContainer = GetNode<LogContainer>("Logs-CollapsibleContainer/ScrollContainer/VBoxContainer");
			_overlayDeath = GetNode<ColorRect>("ColorRect");
			_cardBackground = GetNode<ColorRect>("ColorRect2");
			
			_deathStateSprite.Visible = false;
			_overlayDeath.Visible = false;
			_nameFocusLabel.Text = "";
			_nameAttaquantLabel.Text = "";
			_statsLabel.Text = "";
		}
		
		public void InitializeCharacter(string name, string type, int maxHp = 100)
		{
			var characterClass = CharacterAssets.ParseClass(type);
			CharacterData = new CharacterData(name, characterClass, maxHp);
			
			_nameEdit.Text = name;
			_healthBar.MaxValue = maxHp;
			_healthBar.Value = maxHp;
			
			SetClassEmoji(characterClass);
			UpdateHealthBarColor();
			UpdateStatsLabel();
			
			GD.Print($"[PersonnageUI] {name} ({type}) initialisé");
		}
		
		public void UpdateHP(int newHP)
		{
			CharacterData.CurrentHP = Math.Max(0, Math.Min(newHP, CharacterData.MaxHP));
			_healthBar.Value = CharacterData.CurrentHP;
			
			UpdateHealthBarColor();
			
			if (CharacterData.CurrentHP <= 0 && !CharacterData.IsDead)
			{
				SetDead(true);
			}
			else if (CharacterData.CurrentHP > 0 && CharacterData.IsDead)
			{
				SetDead(false);
			}
		}
		
		public void TakeDamage(int damage, string attacker = "")
		{
			CharacterData.TotalDamageTaken += damage;
			CharacterData.LastAttacker = attacker;
			UpdateHP(CharacterData.CurrentHP - damage);
			
			var logEntry = new CombatLogEntry(
CombatEventType.Damage,
$"💥 Subit {damage} dégâts ! HP: {CharacterData.CurrentHP}/{CharacterData.MaxHP}",
CharacterAssets.EventColors[CombatEventType.Damage]
);
			logEntry.SourceCharacter = attacker;
			logEntry.TargetCharacter = CharacterData.Name;
			logEntry.Value = damage;
			
			AddLogEntry(logEntry);
			UpdateStatsLabel();
			
			if (!string.IsNullOrEmpty(attacker))
			{
				_nameAttaquantLabel.Text = $"Attaqué par: {attacker}";
			}
		}
		
		public void Heal(int amount, string healer = "")
		{
			int actualHealing = Math.Min(amount, CharacterData.MaxHP - CharacterData.CurrentHP);
			CharacterData.TotalHealing += actualHealing;
			UpdateHP(CharacterData.CurrentHP + amount);
			
			var logEntry = new CombatLogEntry(
CombatEventType.Heal,
$"❤️ Soigné de {actualHealing} HP ! HP: {CharacterData.CurrentHP}/{CharacterData.MaxHP}",
CharacterAssets.EventColors[CombatEventType.Heal]
);
			logEntry.SourceCharacter = healer;
			logEntry.TargetCharacter = CharacterData.Name;
			logEntry.Value = actualHealing;
			
			AddLogEntry(logEntry);
			UpdateStatsLabel();
		}
		
		/// <summary>
		/// Enregistre une attaque effectuée par ce personnage
		/// </summary>
		public void RegisterAttack(int damageDealt, string targetName)
		{
			CharacterData.TotalDamageDealt += damageDealt;
			UpdateStatsLabel();
			
			var logEntry = new CombatLogEntry(
				CombatEventType.Attack,
				$"⚔️ Attaque {targetName} pour {damageDealt} dégâts !",
				CharacterAssets.EventColors[CombatEventType.Attack]
			);
			logEntry.SourceCharacter = CharacterData.Name;
			logEntry.TargetCharacter = targetName;
			logEntry.Value = damageDealt;
			
			AddLogEntry(logEntry);
		}
		
		public void SetDead(bool dead)
		{
			CharacterData.IsDead = dead;
			_deathStateSprite.Visible = dead;
			_overlayDeath.Visible = dead;
			
			if (dead)
			{
				CharacterData.DeathCount++;
				UpdateStatsLabel();
				var logEntry = new CombatLogEntry(
CombatEventType.Death,
"💀 EST MORT !",
CharacterAssets.EventColors[CombatEventType.Death]
);
				AddLogEntry(logEntry);
				Modulate = new Color(0.5f, 0.5f, 0.5f);
			}
			else
			{
				var logEntry = new CombatLogEntry(
CombatEventType.Resurrection,
"✨ RESSUSCITÉ !",
CharacterAssets.EventColors[CombatEventType.Resurrection]
);
				AddLogEntry(logEntry);
				Modulate = Colors.White;
			}
		}
		
		public void SetFocus(string targetName)
		{
			CharacterData.FocusTarget = targetName;
			_nameFocusLabel.Text = targetName != "" ? $"🎯 Focus: {targetName}" : "";
		}
		
		public void SetAttacker(string attackerName)
		{
			CharacterData.LastAttacker = attackerName;
			_nameAttaquantLabel.Text = attackerName != "" ? $"⚔️ Attaqué par: {attackerName}" : "";
		}
		
		public void AddStatusEffect(string effectName, bool isPositive, int duration = -1)
		{
			var effect = new StatusEffect(effectName, "", isPositive, duration);
			CharacterData.ActiveEffects.Add(effect);
			
			var iconPath = CharacterAssets.GetEffectIcon(effectName);
			Texture2D icon = null;
			if (!string.IsNullOrEmpty(iconPath) && ResourceLoader.Exists(iconPath))
			{
				icon = ResourceLoader.Load<Texture2D>(iconPath);
			}
			
			if (isPositive)
			{
				_bonusList.AddItem(effectName, icon);
				var logEntry = new CombatLogEntry(
CombatEventType.Buff,
$"✨ Bonus: {effectName}",
CharacterAssets.EventColors[CombatEventType.Buff]
);
				AddLogEntry(logEntry);
			}
			else
			{
				_malusList.AddItem(effectName, icon);
				var logEntry = new CombatLogEntry(
CombatEventType.Debuff,
$"🩸 Malus: {effectName}",
CharacterAssets.EventColors[CombatEventType.Debuff]
);
				AddLogEntry(logEntry);
			}
		}
		
		public void ClearEffects()
		{
			_bonusList.Clear();
			_malusList.Clear();
			CharacterData.ActiveEffects.Clear();
		}
		
		public void AddLog(string message)
		{
			var logEntry = new CombatLogEntry(CombatEventType.StatusEffect, message);
			AddLogEntry(logEntry);
		}
		
		private void AddLogEntry(CombatLogEntry logEntry)
		{
			_logHistory.Add(logEntry);
			
			if (_logHistory.Count > 100)
			{
				_logHistory.RemoveAt(0);
			}
			
			_logContainer.AddLog(logEntry.GetFormattedMessage());
		}
		
		private void SetClassEmoji(CharacterClass characterClass)
		{
			_classEmojiLabel.Text = characterClass switch
			{
				// Classes backend confirmées - emojis détaillés
				CharacterClass.Guerrier => "⚔️",      // Warrior
				CharacterClass.Berserker => "🪓",     // Berserker (hache)
				CharacterClass.Assassin => "🥷",      // Assassin (ninja) - plus simple
				CharacterClass.Alchimiste => "⚗️",    // Alchemist (alambic)
				CharacterClass.Illusioniste => "🔮",  // Illusionist
				CharacterClass.Pretre => "✨",        // Priest
				CharacterClass.Paladin => "🛡️",      // Paladin
				CharacterClass.Zombie => "🧟",        // Zombie
				CharacterClass.Vampire => "🧛",       // Vampire
				CharacterClass.Robot => "🤖",         // Robot
				// Classes supplémentaires frontend
				CharacterClass.Magicien => "🧙",      // Wizard
				CharacterClass.Necromancien => "💀",  // Necromancer
				_ => "❓"
			};
		}
		
		private void UpdateHealthBarColor()
		{
			var hpPercentage = CharacterData.HPPercentage;
			var color = CharacterAssets.GetHPBarColor(hpPercentage);
			
			var stylebox = _healthBar.GetThemeStylebox("fill") as StyleBoxFlat;
			if (stylebox != null)
			{
				stylebox.BgColor = color;
			}
		}
		
		/// <summary>
		/// Met à jour le label des stats de combat
		/// </summary>
		private void UpdateStatsLabel()
		{
			_statsLabel.Text = $"💀 {CharacterData.DeathCount}  |  " +
			                  $"⚔ {CharacterData.TotalDamageDealt}  |  " +
			                  $"🛡 {CharacterData.TotalDamageTaken}  |  " +
			                  $"❤ {CharacterData.TotalHealing}";
		}
		
		public string GetCombatStats()
		{
			return $"HP: {CharacterData.CurrentHP}/{CharacterData.MaxHP} | " +
			       $"Dégâts infligés: {CharacterData.TotalDamageDealt} | " +
			       $"Dégâts subis: {CharacterData.TotalDamageTaken} | " +
			       $"Soins: {CharacterData.TotalHealing} | " +
			       $"Kills: {CharacterData.KillCount} | " +
			       $"Morts: {CharacterData.DeathCount}";
		}
	}
}
