using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FrontBRRPG;

namespace FrontBRRPG.Testing
{
	/// <summary>
	/// Simulateur de combat pour tester l'UI en temps réel
	/// Génère des événements réalistes basés sur la structure backend
	/// </summary>
	public partial class CombatSimulator : Node
	{
		// Événements (signaux Godot - uniquement types primitifs)
		[Signal] public delegate void ActionPerformedEventHandler(string actionType, string actorName, string targetName);
		[Signal] public delegate void CharacterUpdatedEventHandler(string characterName);
		[Signal] public delegate void CombatLogEventHandler(string message, string colorHex);
		
		// Données de simulation
		private List<SimulatedCharacter> _characters = new();
		private Random _random = new();
		private double _timeSinceLastAction = 0;
		private bool _isRunning = false;
		private float _simulationSpeed = 1.0f; // 1.0 = vitesse normale
		
		// Configuration
		public float ActionInterval { get; set; } = 2.0f; // Action toutes les 2s par défaut
		public bool AutoPlay { get; set; } = false;
		
		public override void _Ready()
		{
			GD.Print("[CombatSimulator] Simulateur initialisé");
		}
		
		public override void _Process(double delta)
		{
			if (!_isRunning || !AutoPlay) return;
			
			// Mettre à jour les cooldowns
			UpdateCooldowns((float)delta * _simulationSpeed);
			
			// Exécuter une action si l'intervalle est écoulé
			_timeSinceLastAction += delta * _simulationSpeed;
			if (_timeSinceLastAction >= ActionInterval)
			{
				_timeSinceLastAction = 0;
				ExecuteRandomAction();
			}
		}
		
		/// <summary>
		/// Initialise le simulateur avec une liste de personnages
		/// </summary>
		public void Initialize(List<PersonnageUIManager> characterCards)
		{
			_characters.Clear();
			
			foreach (var card in characterCards)
			{
				var simChar = new SimulatedCharacter
				{
					Name = card.CharacterData.Name,
					Type = card.CharacterData.Class.ToString(),
					CurrentHP = card.CharacterData.CurrentHP,
					MaxHP = card.CharacterData.MaxHP,
					Attack = 25 + _random.Next(-5, 10),
					Defense = 15 + _random.Next(-3, 8),
					Card = card
				};
				
				// Créer des skills de test
				simChar.Skills = CreateTestSkills(simChar.Type);
				
				_characters.Add(simChar);
			}
			
			GD.Print($"[CombatSimulator] {_characters.Count} personnages initialisés");
		}
		
		/// <summary>
		/// Démarre la simulation automatique
		/// </summary>
		public void Start()
		{
			_isRunning = true;
			_timeSinceLastAction = 0;
			EmitCombatLog("🟢 Simulation de combat démarrée !", "#00FF00");
		}
		
		/// <summary>
		/// Met en pause la simulation
		/// </summary>
		public void Pause()
		{
			_isRunning = false;
			EmitCombatLog("⏸️ Simulation en pause", "#FFAA00");
		}
		
		/// <summary>
		/// Définit la vitesse de simulation (0.5x - 5x)
		/// </summary>
		public void SetSpeed(float speed)
		{
			_simulationSpeed = Mathf.Clamp(speed, 0.5f, 5.0f);
			EmitCombatLog($"⏱️ Vitesse: {_simulationSpeed:F1}x", "#00AAFF");
		}
		
		/// <summary>
		/// Exécute une action aléatoire
		/// </summary>
		public void ExecuteRandomAction()
		{
			if (_characters.Count == 0) return;
			
			var aliveCharacters = _characters.Where(c => !c.IsDead).ToList();
			if (aliveCharacters.Count < 2)
			{
				Pause();
				EmitCombatLog("💀 Combat terminé - pas assez de survivants", "#FF0000");
				return;
			}
			
			// Choisir un attaquant
			var attacker = aliveCharacters[_random.Next(aliveCharacters.Count)];
			
			// Choisir un défenseur (différent de l'attaquant)
			var possibleTargets = aliveCharacters.Where(c => c != attacker).ToList();
			if (possibleTargets.Count == 0) return;
			
			var target = possibleTargets[_random.Next(possibleTargets.Count)];
			
			// Choisir une action (70% attaque normale, 30% skill)
			if (_random.NextDouble() < 0.7)
			{
				ExecuteBasicAttack(attacker, target);
			}
			else
			{
				var readySkills = attacker.Skills.Where(s => s.IsReady).ToList();
				if (readySkills.Count > 0)
				{
					var skill = readySkills[_random.Next(readySkills.Count)];
					ExecuteSkill(attacker, target, skill);
				}
				else
				{
					ExecuteBasicAttack(attacker, target);
				}
			}
		}
		
		/// <summary>
		/// Simule une attaque basique
		/// </summary>
		private void ExecuteBasicAttack(SimulatedCharacter attacker, SimulatedCharacter target)
		{
			// Calcul des dégâts (avec variance)
			int baseDamage = attacker.Attack;
			int defense = target.Defense;
			int variance = _random.Next(-5, 6);
			int damage = Mathf.Max(1, baseDamage - defense / 2 + variance);
			
			// Chance de critique (15%)
			bool isCritical = _random.NextDouble() < 0.15;
			if (isCritical)
			{
				damage = (int)(damage * 1.5f);
			}
			
			// Appliquer les dégâts
			target.CurrentHP = Mathf.Max(0, target.CurrentHP - damage);
			target.Card.UpdateHP(target.CurrentHP);
			
			// Log
			string critText = isCritical ? " 💥 CRITIQUE !" : "";
			EmitCombatLog($"⚔️ {attacker.Name} attaque {target.Name} pour {damage} dégâts{critText}", 
				isCritical ? "#FF0000" : "#FFAA00");
			
		// Vérifier la mort
		if (target.CurrentHP <= 0 && !target.IsDead)
		{
			target.IsDead = true;
			target.Card.SetDead(true);
			EmitCombatLog($"💀 {target.Name} est mort !", "#FF0000");
		}
		
		// Émettre l'action
		EmitSignal(SignalName.ActionPerformed, "attack", attacker.Name, target.Name);
	}		/// <summary>
		/// Simule l'utilisation d'un skill
		/// </summary>
		private void ExecuteSkill(SimulatedCharacter caster, SimulatedCharacter target, SimulatedSkill skill)
		{
			// Déclencher le cooldown
			skill.CurrentCooldown = skill.BaseCooldown;
			
			switch (skill.Name)
			{
				case "Soin":
					int healing = 30 + _random.Next(-5, 11);
					caster.CurrentHP = Mathf.Min(caster.MaxHP, caster.CurrentHP + healing);
					caster.Card.Heal(healing, caster.Name);
					EmitCombatLog($"✨ {caster.Name} se soigne de {healing} HP", "#00FF88");
					break;
					
				case "Rage":
					int rageDamage = caster.Attack * 2;
					target.CurrentHP = Mathf.Max(0, target.CurrentHP - rageDamage);
					target.Card.TakeDamage(rageDamage, caster.Name);
					EmitCombatLog($"🔥 {caster.Name} utilise Rage sur {target.Name} pour {rageDamage} dégâts !", "#FF4400");
					
					if (target.CurrentHP <= 0 && !target.IsDead)
					{
						target.IsDead = true;
						target.Card.SetDead(true);
						EmitCombatLog($"💀 {target.Name} est mort !", "#FF0000");
					}
					break;
					
				case "Bouclier":
					// Simuler un buff de bouclier (à implémenter dans l'UI)
					caster.Card.AddStatusEffect("Bouclier", true, 3);
					EmitCombatLog($"🛡️ {caster.Name} active Bouclier (3 tours)", "#00AAFF");
					break;
					
				case "Poison":
					// Simuler un debuff de poison
					target.Card.AddStatusEffect("Empoisonné", false, 5);
				EmitCombatLog($"🧪 {caster.Name} empoisonne {target.Name} (5 tours)", "#88FF00");
				break;
		}
		
		EmitSignal(SignalName.ActionPerformed, "skill", caster.Name, target.Name);
	}		/// <summary>
		/// Met à jour les cooldowns de tous les skills
		/// </summary>
		private void UpdateCooldowns(float delta)
		{
			foreach (var character in _characters)
			{
				foreach (var skill in character.Skills)
				{
					if (skill.CurrentCooldown > 0)
					{
						skill.CurrentCooldown = Mathf.Max(0, skill.CurrentCooldown - delta);
					}
				}
			}
		}
		
		/// <summary>
		/// Crée des skills de test basés sur le type de personnage
		/// </summary>
		private List<SimulatedSkill> CreateTestSkills(string characterType)
		{
			var skills = new List<SimulatedSkill>
			{
				new SimulatedSkill { Name = "Attaque Base", BaseCooldown = 1.0f, CurrentCooldown = 0 }
			};
			
			// Skills spécifiques selon la classe
			switch (characterType)
			{
				case "Guerrier":
				case "Berserker":
					skills.Add(new SimulatedSkill { Name = "Rage", BaseCooldown = 5.0f, CurrentCooldown = 0 });
					skills.Add(new SimulatedSkill { Name = "Bouclier", BaseCooldown = 8.0f, CurrentCooldown = 0 });
					break;
					
				case "Pretre":
				case "Paladin":
					skills.Add(new SimulatedSkill { Name = "Soin", BaseCooldown = 4.0f, CurrentCooldown = 0 });
					skills.Add(new SimulatedSkill { Name = "Bouclier", BaseCooldown = 6.0f, CurrentCooldown = 0 });
					break;
					
				case "Assassin":
				case "Illusioniste":
					skills.Add(new SimulatedSkill { Name = "Poison", BaseCooldown = 6.0f, CurrentCooldown = 0 });
					skills.Add(new SimulatedSkill { Name = "Rage", BaseCooldown = 4.0f, CurrentCooldown = 0 });
					break;
					
				default:
					skills.Add(new SimulatedSkill { Name = "Rage", BaseCooldown = 5.0f, CurrentCooldown = 0 });
					break;
			}
			
			return skills;
		}
		
		/// <summary>
		/// Émet un log de combat
		/// </summary>
		private void EmitCombatLog(string message, string colorHex)
		{
			EmitSignal(SignalName.CombatLog, message, colorHex);
		}
		
		// Actions manuelles pour les tests
		
		public void TestAttack()
		{
			ExecuteRandomAction();
		}
		
		public void TestHeal()
		{
			var aliveCharacters = _characters.Where(c => !c.IsDead && c.CurrentHP < c.MaxHP).ToList();
			if (aliveCharacters.Count == 0) return;
			
			var character = aliveCharacters[_random.Next(aliveCharacters.Count)];
			int healing = 40 + _random.Next(-10, 21);
			character.CurrentHP = Mathf.Min(character.MaxHP, character.CurrentHP + healing);
			character.Card.Heal(healing, "Test");
			EmitCombatLog($"💊 Test: {character.Name} soigné de {healing} HP", "#00FF88");
		}
		
		public void TestDeath()
		{
			var aliveCharacters = _characters.Where(c => !c.IsDead).ToList();
			if (aliveCharacters.Count == 0) return;
			
			var character = aliveCharacters[_random.Next(aliveCharacters.Count)];
			character.IsDead = true;
			character.CurrentHP = 0;
			character.Card.UpdateHP(0);
			character.Card.SetDead(true);
			EmitCombatLog($"💀 Test: {character.Name} est mort !", "#FF0000");
		}
		
		public void TestResurrection()
		{
			var deadCharacters = _characters.Where(c => c.IsDead).ToList();
			if (deadCharacters.Count == 0) return;
			
			var character = deadCharacters[_random.Next(deadCharacters.Count)];
			character.IsDead = false;
			character.CurrentHP = character.MaxHP / 2;
			character.Card.UpdateHP(character.CurrentHP);
			character.Card.SetDead(false);
			EmitCombatLog($"✨ Test: {character.Name} ressuscite avec {character.CurrentHP} HP !", "#FFD700");
		}
		
		public void TestStatusEffect()
		{
			var aliveCharacters = _characters.Where(c => !c.IsDead).ToList();
			if (aliveCharacters.Count == 0) return;
			
			var character = aliveCharacters[_random.Next(aliveCharacters.Count)];
			var effects = new[] { "Empoisonné", "Rage", "Bouclier", "Faiblesse", "Force" };
			var effect = effects[_random.Next(effects.Length)];
			bool isPositive = effect == "Rage" || effect == "Bouclier" || effect == "Force";
			
			character.Card.AddStatusEffect(effect, isPositive, 3 + _random.Next(3));
			EmitCombatLog($"🎭 Test: {character.Name} reçoit {effect}", isPositive ? "#00FF00" : "#FF8800");
		}
	}
	
	// Classes internes pour la simulation
	
	public class SimulatedCharacter
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public int CurrentHP { get; set; }
		public int MaxHP { get; set; }
		public int Attack { get; set; }
		public int Defense { get; set; }
		public bool IsDead { get; set; }
		public List<SimulatedSkill> Skills { get; set; } = new();
		public PersonnageUIManager Card { get; set; }
	}
	
	public class SimulatedSkill
	{
		public string Name { get; set; }
		public float BaseCooldown { get; set; }
		public float CurrentCooldown { get; set; }
		public bool IsReady => CurrentCooldown <= 0;
		public int ChargePercent => BaseCooldown > 0 ? 
			(int)((BaseCooldown - CurrentCooldown) / BaseCooldown * 100) : 100;
	}
	
	public class SimulatedAction
	{
		public string Type { get; set; }
		public string Attacker { get; set; }
		public string Target { get; set; }
		public int Damage { get; set; }
		public bool IsCritical { get; set; }
		public string SkillName { get; set; }
	}
}
