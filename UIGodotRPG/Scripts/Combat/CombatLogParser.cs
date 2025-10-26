using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FrontBRRPG.Combat
{
    /// <summary>
    /// Parse les logs de combat textuels en événements structurés
    /// </summary>
    public partial class CombatLogParser : Node
    {
        // Utiliser un event C# au lieu d'un signal Godot
        public event Action<CombatEvent> EventParsed;

        private readonly Dictionary<string, CombatEventType> _emojiToEventType = new()
        {
            { "🟢", CombatEventType.BattleStart },
            { "🛑", CombatEventType.BattleEnd },
            { "🏆", CombatEventType.Winner },
            { "💀", CombatEventType.Death },
            { "☠️", CombatEventType.Death },
            { "🪓", CombatEventType.Attack },
            { "🛡️", CombatEventType.Attack },
            { "✨", CombatEventType.SpecialAbility },
            { "💨", CombatEventType.SpecialAbility },
            { "💥", CombatEventType.Damage },
            { "🧟", CombatEventType.SpecialAbility },
            { "🎲", CombatEventType.DiceRoll },
            { "❤️", CombatEventType.Heal },
            { "⚡", CombatEventType.SpecialAbility },
            { "🧪", CombatEventType.SpecialAbility },
            { "🩸", CombatEventType.Damage }
        };

        // Patterns regex pour extraire les informations
        private readonly Regex _attackPattern = new Regex(@"(\w+) (?:fonce sur|attaque|frappe|lance|utilise) (\w+)");
        private readonly Regex _damagePattern = new Regex(@"(\d+) (?:dégâts|PV|points de vie|HP)");
        private readonly Regex _dicePattern = new Regex(@"🎲 (\w+) lance les dés : (\d+)");
        private readonly Regex _deathPattern = new Regex(@"💀 (\w+) est (?:mort|tombé|éliminé)");
        private readonly Regex _winnerPattern = new Regex(@"🏆 (\w+) (?:est le dernier survivant|remporte le combat)");
        private readonly Regex _abilityPattern = new Regex(@"(\w+) (?:crée|invoque|transforme|lance) (.+?)(?: (?::|!))");

        public CombatEvent ParseMessage(string message)
        {
            var evt = new CombatEvent
            {
                RawMessage = message,
                Timestamp = DateTime.Now
            };

            // Détecter le type d'événement par l'emoji
            foreach (var kvp in _emojiToEventType)
            {
                if (message.StartsWith(kvp.Key))
                {
                    evt.Type = kvp.Value;
                    break;
                }
            }

            // Parser selon le type d'événement
            switch (evt.Type)
            {
                case CombatEventType.BattleStart:
                    GD.Print("[Parser] Combat démarré");
                    break;

                case CombatEventType.BattleEnd:
                    GD.Print("[Parser] Combat terminé");
                    break;

                case CombatEventType.Winner:
                    var winnerMatch = _winnerPattern.Match(message);
                    if (winnerMatch.Success)
                    {
                        evt.SourceCharacter = winnerMatch.Groups[1].Value;
                        GD.Print($"[Parser] Gagnant: {evt.SourceCharacter}");
                    }
                    break;

                case CombatEventType.Death:
                    var deathMatch = _deathPattern.Match(message);
                    if (deathMatch.Success)
                    {
                        evt.SourceCharacter = deathMatch.Groups[1].Value;
                        GD.Print($"[Parser] Mort de {evt.SourceCharacter}");
                    }
                    break;

                case CombatEventType.DiceRoll:
                    var diceMatch = _dicePattern.Match(message);
                    if (diceMatch.Success)
                    {
                        evt.SourceCharacter = diceMatch.Groups[1].Value;
                        evt.DiceRoll = int.Parse(diceMatch.Groups[2].Value);
                        GD.Print($"[Parser] {evt.SourceCharacter} lance le dé: {evt.DiceRoll}");
                    }
                    break;

                case CombatEventType.Attack:
                    var attackMatch = _attackPattern.Match(message);
                    if (attackMatch.Success)
                    {
                        evt.SourceCharacter = attackMatch.Groups[1].Value;
                        evt.TargetCharacter = attackMatch.Groups[2].Value;
                        GD.Print($"[Parser] {evt.SourceCharacter} attaque {evt.TargetCharacter}");
                    }
                    break;

                case CombatEventType.Damage:
                    var dmgMatch = _damagePattern.Match(message);
                    if (dmgMatch.Success)
                    {
                        evt.DamageAmount = int.Parse(dmgMatch.Groups[1].Value);
                        GD.Print($"[Parser] {evt.DamageAmount} dégâts");
                    }
                    break;

                case CombatEventType.SpecialAbility:
                    var abilityMatch = _abilityPattern.Match(message);
                    if (abilityMatch.Success)
                    {
                        evt.SourceCharacter = abilityMatch.Groups[1].Value;
                        evt.AbilityName = abilityMatch.Groups[2].Value;
                        GD.Print($"[Parser] {evt.SourceCharacter} utilise {evt.AbilityName}");
                    }
                    break;

                default:
                    evt.Type = CombatEventType.Info;
                    break;
            }

            EventParsed?.Invoke(evt);
            return evt;
        }

        /// <summary>
        /// Parse un batch de messages et retourne la liste des événements
        /// </summary>
        public List<CombatEvent> ParseMessages(string[] messages)
        {
            var events = new List<CombatEvent>();
            foreach (var message in messages)
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    events.Add(ParseMessage(message));
                }
            }
            return events;
        }

        /// <summary>
        /// Parse et agrège les événements pour reconstruire les dégâts
        /// Exemple: Attaque + Dé + Dégâts = une action complète
        /// </summary>
        public List<CombatEvent> ParseAndAggregate(string[] messages)
        {
            var events = ParseMessages(messages);
            var aggregated = new List<CombatEvent>();
            
            for (int i = 0; i < events.Count; i++)
            {
                var current = events[i];
                
                // Si c'est une attaque, chercher les infos suivantes
                if (current.Type == CombatEventType.Attack)
                {
                    // Chercher le lancer de dé qui suit (dans les 2 prochains messages)
                    for (int j = i + 1; j < Math.Min(i + 3, events.Count); j++)
                    {
                        if (events[j].Type == CombatEventType.DiceRoll && 
                            events[j].SourceCharacter == current.SourceCharacter)
                        {
                            current.DiceRoll = events[j].DiceRoll;
                        }
                        else if (events[j].Type == CombatEventType.Damage)
                        {
                            current.DamageAmount = events[j].DamageAmount;
                        }
                    }
                }
                
                aggregated.Add(current);
            }
            
            return aggregated;
        }
    }
}
