using Godot;
using System;
using System.Collections.Generic;

namespace FrontBRRPG.Combat
{
    /// <summary>
    /// Type d'événement de combat
    /// </summary>
    public enum CombatEventType
    {
        BattleStart,        // 🟢 Début du combat
        BattleEnd,          // 🛑 Fin du combat
        Attack,             // Attaque standard
        Damage,             // Dégâts infligés
        Heal,               // Soin
        Death,              // Mort d'un personnage
        StatusEffect,       // Effet de statut appliqué
        DiceRoll,           // Lancer de dé
        SpecialAbility,     // Capacité spéciale utilisée
        Info,               // Information générale
        Winner              // Annonce du gagnant
    }

    /// <summary>
    /// Événement de combat parsé depuis un log
    /// </summary>
    public class CombatEvent
    {
        public CombatEventType Type { get; set; }
        public string RawMessage { get; set; } = "";
        public string SourceCharacter { get; set; } = "";
        public string TargetCharacter { get; set; } = "";
        public int? DamageAmount { get; set; }
        public int? HealAmount { get; set; }
        public int? DiceRoll { get; set; }
        public string AbilityName { get; set; } = "";
        public string StatusEffect { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// État d'un personnage dans le combat
    /// </summary>
    public class CharacterState
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public int CurrentHP { get; set; } = 100;
        public int MaxHP { get; set; } = 100;
        public bool IsDead { get; set; } = false;
        public bool IsAttackable { get; set; } = true;
        public bool IsEatable { get; set; } = false;
        public List<string> StatusEffects { get; set; } = new List<string>();
        public Vector2 Position { get; set; } = Vector2.Zero;
        public string IconPath { get; set; } = "";

        public float HPPercentage => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;
    }

    /// <summary>
    /// État global du combat
    /// </summary>
    public class BattleState
    {
        public Dictionary<string, CharacterState> Characters { get; set; } = new Dictionary<string, CharacterState>();
        public List<CombatEvent> EventHistory { get; set; } = new List<CombatEvent>();
        public bool IsActive { get; set; } = false;
        public string Winner { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public void AddEvent(CombatEvent evt)
        {
            EventHistory.Add(evt);
            
            // Mettre à jour l'état des personnages selon l'événement
            if (evt.Type == CombatEventType.Damage && evt.TargetCharacter != "" && evt.DamageAmount.HasValue)
            {
                if (Characters.ContainsKey(evt.TargetCharacter))
                {
                    Characters[evt.TargetCharacter].CurrentHP -= evt.DamageAmount.Value;
                    if (Characters[evt.TargetCharacter].CurrentHP < 0)
                        Characters[evt.TargetCharacter].CurrentHP = 0;
                }
            }
            else if (evt.Type == CombatEventType.Heal && evt.TargetCharacter != "" && evt.HealAmount.HasValue)
            {
                if (Characters.ContainsKey(evt.TargetCharacter))
                {
                    Characters[evt.TargetCharacter].CurrentHP += evt.HealAmount.Value;
                    if (Characters[evt.TargetCharacter].CurrentHP > Characters[evt.TargetCharacter].MaxHP)
                        Characters[evt.TargetCharacter].CurrentHP = Characters[evt.TargetCharacter].MaxHP;
                }
            }
            else if (evt.Type == CombatEventType.Death && evt.SourceCharacter != "")
            {
                if (Characters.ContainsKey(evt.SourceCharacter))
                {
                    Characters[evt.SourceCharacter].IsDead = true;
                    Characters[evt.SourceCharacter].CurrentHP = 0;
                }
            }
        }

        public CharacterState GetOrCreateCharacter(string name, string type = "")
        {
            if (!Characters.ContainsKey(name))
            {
                Characters[name] = new CharacterState 
                { 
                    Name = name, 
                    Type = type,
                    CurrentHP = 100,
                    MaxHP = 100
                };
            }
            return Characters[name];
        }
    }
}
