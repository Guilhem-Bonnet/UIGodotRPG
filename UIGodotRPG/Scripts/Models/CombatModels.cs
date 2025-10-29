using Godot;
using System.Collections.Generic;

namespace FrontBRRPG.Models
{
	/// <summary>
	/// Énumération des types de personnages
	/// </summary>
	public enum CharacterClass
	{
		Alchimiste,
		Assassin,
		Berserker,
		Guerrier,
		Illusioniste,
		Magicien,
		Necromancien,
		Paladin,
		Pretre,
		Robot,
		Vampire,
		Zombie
	}

	/// <summary>
	/// Énumération des types d'événements de combat
	/// </summary>
	public enum CombatEventType
	{
		Attack,
		Damage,
		Heal,
		Death,
		Resurrection,
		StatusEffect,
		Buff,
		Debuff,
		Spell,
		Critical,
		Miss,
		Victory,
		StartBattle,
		EndBattle
	}

	/// <summary>
	/// Représente un effet actif sur un personnage (buff/debuff)
	/// </summary>
	public class StatusEffect
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsPositive { get; set; } // true = buff, false = debuff
		public int Duration { get; set; } // -1 = permanent
		public string IconPath { get; set; }

		public StatusEffect(string name, string description, bool isPositive, int duration = -1, string iconPath = "")
		{
			Name = name;
			Description = description;
			IsPositive = isPositive;
			Duration = duration;
			IconPath = iconPath;
		}
	}

	/// <summary>
	/// Données complètes d'un personnage
	/// </summary>
	public class CharacterData
	{
		public string Name { get; set; }
		public CharacterClass Class { get; set; }
		public int CurrentHP { get; set; }
		public int MaxHP { get; set; }
		public bool IsDead { get; set; }
		public string FocusTarget { get; set; }
		public string LastAttacker { get; set; }
		public List<StatusEffect> ActiveEffects { get; set; }
		
		// Stats de combat
		public int TotalDamageDealt { get; set; }
		public int TotalDamageTaken { get; set; }
		public int TotalHealing { get; set; }
		public int KillCount { get; set; }
		public int DeathCount { get; set; }

		public CharacterData(string name, CharacterClass characterClass, int maxHP = 100)
		{
			Name = name;
			Class = characterClass;
			CurrentHP = maxHP;
			MaxHP = maxHP;
			IsDead = false;
			FocusTarget = "";
			LastAttacker = "";
			ActiveEffects = new List<StatusEffect>();
			TotalDamageDealt = 0;
			TotalDamageTaken = 0;
			TotalHealing = 0;
			KillCount = 0;
			DeathCount = 0;
		}

		public float HPPercentage => MaxHP > 0 ? (float)CurrentHP / MaxHP * 100f : 0f;
	}

	/// <summary>
	/// Entrée de log avec métadonnées
	/// </summary>
	public class CombatLogEntry
	{
		public CombatEventType EventType { get; set; }
		public string Message { get; set; }
		public Color TextColor { get; set; }
		public string Timestamp { get; set; }
		public string SourceCharacter { get; set; }
		public string TargetCharacter { get; set; }
		public int Value { get; set; } // Dégâts, soins, etc.

		public CombatLogEntry(CombatEventType eventType, string message, Color? textColor = null)
		{
			EventType = eventType;
			Message = message;
			TextColor = textColor ?? Colors.White;
			Timestamp = System.DateTime.Now.ToString("HH:mm:ss");
			SourceCharacter = "";
			TargetCharacter = "";
			Value = 0;
		}

		public string GetFormattedMessage()
		{
			return $"[{Timestamp}] {Message}";
		}
	}
}
