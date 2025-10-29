using Godot;
using System.Collections.Generic;
using FrontBRRPG.Models;

namespace FrontBRRPG.Utils
{
	/// <summary>
	/// Gestionnaire des ressources d'icônes et des couleurs
	/// </summary>
	public static class CharacterAssets
	{
		// Mapping des icônes par classe
		private static readonly Dictionary<CharacterClass, string> _classIcons = new()
		{
			{ CharacterClass.Alchimiste, "res://icons/DALL·E 2024-10-26 13.15.14 - An icon representing a dark fantasy alchemist. At the center, a mysterious figure holding a glowing vial filled with a swirling, magical liquid. The a.webp" },
			{ CharacterClass.Assassin, "res://icons/DALL·E 2024-10-26 13.17.41 - An icon representing a dark fantasy shadow assassin. At the center, a hooded figure with glowing red eyes, holding twin curved daggers dripping with s.webp" },
			{ CharacterClass.Berserker, "res://icons/DALL·E 2024-10-26 10.18.31 - An icon representing a dark fantasy berserker. At the center, a brutal, glowing war axe is raised, crackling with red energy and surrounded by a faint.webp" },
			{ CharacterClass.Guerrier, "res://icons/DALL·E 2024-10-26 10.24.45 - An icon representing a dark fantasy warrior. The central figure is a heavily armored warrior wielding a massive, jagged sword, covered in battle scars.webp" },
			{ CharacterClass.Illusioniste, "res://icons/DALL·E 2024-10-26 13.45.17 - An icon representing an Illusionist class in dark fantasy with even more details. At the center, a shadowy, barely visible figure cloaked in mist and .webp" },
			{ CharacterClass.Magicien, "res://icons/DALL·E 2024-10-26 13.42.06 - An icon representing a powerful grand mage in a detailed dark fantasy style. At the center, a robed figure stands tall, holding a glowing, mystical st.webp" },
			{ CharacterClass.Necromancien, "res://icons/DALL·E 2024-10-26 13.06.32 - A full-sized icon representing a dark fantasy zombie or ghoul. At the center, a grotesque decayed figure with skeletal features, glowing hollow eyes, .webp" },
			{ CharacterClass.Paladin, "res://icons/DALL·E 2024-10-26 10.23.31 - An icon representing a dark fantasy priest of light. At the center, a radiant figure holds a glowing staff topped with a brilliant crystal, casting bo.webp" },
			{ CharacterClass.Pretre, "res://icons/DALL·E 2024-10-26 10.21.51 - An icon representing a dark fantasy priest. At the center, a shadowy figure holding a twisted, glowing staff topped with a dark crystal. The figure we.webp" },
			{ CharacterClass.Robot, "res://icons/DALL·E 2024-10-26 10.20.42 - An icon representing a dark fantasy robot. At the center, a heavily armored, mechanical figure with glowing red eyes and intricate gears is shown, evo.webp" },
			{ CharacterClass.Vampire, "res://icons/DALL·E 2024-10-26 10.29.44 - A highly detailed dark fantasy vampire icon. At the center, a terrifying vampire with glowing red eyes and sharp fangs, dressed in an ornate, dark clo.webp" },
			{ CharacterClass.Zombie, "res://icons/DALL·E 2024-10-26 13.06.32 - A full-sized icon representing a dark fantasy zombie or ghoul. At the center, a grotesque decayed figure with skeletal features, glowing hollow eyes, .webp" }
		};

		// Mapping des icônes d'effets
		public static readonly Dictionary<string, string> EffectIcons = new()
		{
			{ "poison", "res://icons/DALL·E 2024-10-26 13.26.43 - An icon representing ongoing poison damage in a dark fantasy style. At the center, a shadowy figure with glowing green veins, symbolizing the poison c.webp" },
			{ "healing", "res://icons/DALL·E 2024-10-26 13.22.39 - An icon representing healing from a priest or paladin in a detailed dark fantasy style. At the center, a radiant holy symbol, such as a cross or shiel.webp" },
			{ "resurrection", "res://icons/DALL·E 2024-10-26 13.29.40 - An icon representing resurrection from a priest or paladin in a detailed dark fantasy style. At the center, a radiant, glowing figure rises from the g.webp" },
			{ "attack", "res://icons/DALL·E 2024-10-26 13.20.08 - An icon representing an attack in a detailed dark fantasy style. At the center, a sharp, glowing sword or blade is mid-swing, surrounded by dynamic mo.webp" },
			{ "defense", "res://icons/DALL·E 2024-10-26 13.20.30 - An icon representing defense in a detailed dark fantasy style. At the center, a large, weathered shield with intricate engravings and glowing runes is.webp" }
		};

		// Couleurs pour les types d'événements
		public static readonly Dictionary<CombatEventType, Color> EventColors = new()
		{
			{ CombatEventType.Attack, new Color(1.0f, 0.8f, 0.4f) }, // Orange clair
			{ CombatEventType.Damage, new Color(1.0f, 0.3f, 0.3f) }, // Rouge
			{ CombatEventType.Heal, new Color(0.3f, 1.0f, 0.3f) }, // Vert
			{ CombatEventType.Death, new Color(0.5f, 0.0f, 0.5f) }, // Violet foncé
			{ CombatEventType.Resurrection, new Color(1.0f, 1.0f, 0.3f) }, // Jaune
			{ CombatEventType.StatusEffect, new Color(0.7f, 0.5f, 1.0f) }, // Violet clair
			{ CombatEventType.Buff, new Color(0.3f, 0.8f, 1.0f) }, // Cyan
			{ CombatEventType.Debuff, new Color(1.0f, 0.5f, 0.0f) }, // Orange
			{ CombatEventType.Spell, new Color(0.8f, 0.3f, 1.0f) }, // Magenta
			{ CombatEventType.Critical, new Color(1.0f, 0.0f, 0.0f) }, // Rouge vif
			{ CombatEventType.Miss, new Color(0.6f, 0.6f, 0.6f) }, // Gris
			{ CombatEventType.Victory, new Color(1.0f, 0.8f, 0.0f) }, // Or
			{ CombatEventType.StartBattle, new Color(0.3f, 1.0f, 0.8f) }, // Vert cyan
			{ CombatEventType.EndBattle, new Color(0.8f, 0.8f, 0.8f) } // Blanc cassé
		};

		// Couleurs pour les barres de vie
		public static Color GetHPBarColor(float hpPercentage)
		{
			if (hpPercentage > 75f)
				return new Color(0.0f, 0.8f, 0.2f); // Vert
			else if (hpPercentage > 50f)
				return new Color(0.8f, 0.8f, 0.0f); // Jaune
			else if (hpPercentage > 25f)
				return new Color(1.0f, 0.5f, 0.0f); // Orange
			else
				return new Color(1.0f, 0.0f, 0.0f); // Rouge
		}

		/// <summary>
		/// Récupère le chemin de l'icône pour une classe donnée
		/// </summary>
		public static string GetClassIcon(CharacterClass characterClass)
		{
			return _classIcons.TryGetValue(characterClass, out var icon) ? icon : "";
		}

		/// <summary>
		/// Récupère le chemin de l'icône pour une classe donnée à partir de son nom
		/// </summary>
		public static string GetClassIconByName(string className)
		{
			if (System.Enum.TryParse<CharacterClass>(className, true, out var characterClass))
			{
				return GetClassIcon(characterClass);
			}
			return "";
		}

		/// <summary>
		/// Récupère l'icône d'un effet
		/// </summary>
		public static string GetEffectIcon(string effectName)
		{
			var lowerName = effectName.ToLower();
			foreach (var kvp in EffectIcons)
			{
				if (lowerName.Contains(kvp.Key))
					return kvp.Value;
			}
			return "";
		}

		/// <summary>
		/// Parse un nom de classe depuis une string
		/// </summary>
		public static CharacterClass ParseClass(string className)
		{
			return System.Enum.TryParse<CharacterClass>(className, true, out var result) 
				? result 
				: CharacterClass.Guerrier;
		}
	}
}
